using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 每帧轮询入口。
    ///
    /// 为什么不直接用 MonoBehaviour.Update：
    /// 实测（2026-10-22）无论是 BepInEx_Manager 上的插件组件、还是自建
    /// DontDestroyOnLoad 的 GameObject，都只跑得了 Awake 与 OnEnable，
    /// Start / Update / FixedUpdate / LateUpdate 全部不触发。
    /// 与其猜 Unity 消息分发为何不认运行时加载的程序集里的 MonoBehaviour，
    /// 不如直接挂到游戏自己必定被调用的方法上 —— 这是唯一确定能跑在主线程的入口。
    ///
    /// v0.3.1 的教训（重要）：插件 MonoBehaviour 会在启动后不久**被 Unity 销毁**。
    /// 由于 Plugin 继承 MonoBehaviour，UnityEngine.Object 重载了 `==`，
    /// 已销毁对象会被判成 null —— 于是 Plugin.Instance 与 _owner 表面上全变 null，
    /// 快照逻辑一次都进不去，而日志照常打得出来（ManualLogSource 不是 UnityEngine.Object）。
    /// 这同时也解释了为什么 MonoBehaviour.Update 从不触发。
    /// ⇒ 轮询所需的一切（enabled / 快捷键 / 预读张数）必须在 Awake 里快照成静态值，
    ///   状态放本类 static 字段，绝不依赖任何 UnityEngine.Object。
    /// </summary>
    internal static class TickPump
    {
        /// <summary>仅用于诊断，不参与门控。</summary>
        private static Plugin _owner;

        private static int _lastFrame = -1;
        private static float _lastHeartbeat;
        private static float _lastDumpTime;
        private static float _lastSolveTime;
        private static float _lastAutoDump;
        private static bool _errorReported;

        // 状态自持：不依赖插件实例
        private static bool _wasInTactical;
        private static int _lastRound = -1;

        /// <summary>候选挂载点（Assembly-CSharp，全局命名空间）。</summary>
        internal static readonly string[] CandidateTypeNames =
        {
            "Choreographer",           // 战斗表现层主控，战斗中必定存在
            "ClientScenarioManager",   // 客户端场景管理
            "CharacterManager",
            "InteractabilityManager",
        };

        internal static int PatchedCount { get; private set; }
        internal static int TotalTicks { get; private set; }

        private static readonly Dictionary<string, int> HitCounts = new Dictionary<string, int>();

        public static void Init(Plugin owner)
        {
            _owner = owner;
            try
            {
                var harmony = new Harmony(Plugin.GUID + ".tick");
                var postfix = new HarmonyMethod(
                    typeof(TickPatch).GetMethod(nameof(TickPatch.Postfix),
                        BindingFlags.Static | BindingFlags.Public));

                foreach (MethodBase target in TickPatch.CollectTargets())
                {
                    harmony.Patch(target, postfix: postfix);
                    PatchedCount++;
                    Plugin.Log?.LogInfo($"[FHSolver] 已挂载轮询入口: {target.DeclaringType.Name}.{target.Name}");
                }

                if (PatchedCount == 0)
                {
                    Plugin.Log?.LogError("[FHSolver] 没有任何轮询入口挂载成功，M1 无法工作！");
                }
                else
                {
                    Plugin.Log?.LogInfo($"[FHSolver] 轮询入口挂载完成: {PatchedCount} 个");
                }

                // M1.5：抽牌预测断言
                ModifierPredictPatch.Install(harmony);

                // P1：伤害对拍（预测 vs 游戏实际结果）
                DamageVerifyPatch.Install(harmony);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"[FHSolver] 挂载轮询入口失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>由 TickPatch.Postfix 调用。按帧去重后执行真正的轮询逻辑。</summary>
        internal static void OnGameTick(string source)
        {
            try
            {
                if (!HitCounts.ContainsKey(source))
                {
                    HitCounts[source] = 0;
                    Plugin.Log?.LogInfo($"[FHSolver] 首次收到游戏心跳，来源={source} 帧={Time.frameCount}");
                }
                HitCounts[source]++;

                int f = Time.frameCount;
                if (f == _lastFrame) return; // 同一帧内多个入口只处理一次
                _lastFrame = f;
                TotalTicks++;

                float now = Time.realtimeSinceStartup;

                bool inTactical = false;
                try { inTactical = ScenarioRuleLibrary.ScenarioManager.IsInTactical; }
                catch { /* 忽略，下一帧再试 */ }

                // 心跳是诊断信号，不受功能开关门控 ——
                // 否则一旦开关为 false 就彻底失去可观测性（v0.3.0 被这个挡过一次）
                if (now - _lastHeartbeat > 5f)
                {
                    _lastHeartbeat = now;
                    Plugin.Log?.LogInfo($"[FHSolver] 心跳#{TotalTicks}: "
                        + $"IsInTactical={inTactical}"
                        + $" 帧={f} 入口=[{HitSummary()}]"
                        + $" Enabled={Plugin.StaticEnabled}"
                        + $" latch={_wasInTactical} 上次回合={_lastRound}"
                        + $" | 预测 {ModifierPredictPatch.Summary}"
                        + $" | 对拍 {DamageVerifyPatch.Summary}"
                        + $" | {DebugToggle.Summary}");
                }

                // 一次性诊断：确认插件组件是否已被 Unity 销毁（fake null）。
                // Plugin 继承 MonoBehaviour，组件销毁后 `==` 会把它判成 null，
                // 但真实引用还在 —— 两个结果不一致即为已销毁。
                if (TotalTicks == 1)
                {
                    object raw = Plugin.Instance;
                    Plugin.Log?.LogInfo($"[FHSolver] 实例诊断: UnityNull={Plugin.Instance == null}"
                        + $" 真实引用Null={raw == null}"
                        + $" （UnityNull=True 且 真实引用Null=False ⇒ 组件已被 Unity 销毁）");
                }

                if (!Plugin.StaticEnabled) return;

                // 触发点 0：启动后第一次 tick 无条件打一次快照。
                // 用来区分「分支没进」与「DumpSnapshot 自己没输出」——
                // 无论是否在战斗，日志里都必定留下一行。
                if (TotalTicks == 1)
                {
                    Plugin.DumpSnapshot("启动自检");
                }

                // 触发点 1：F1 手动快照
                if (Plugin.StaticDumpKey.IsDown() && now - _lastDumpTime > 0.3f)
                {
                    _lastDumpTime = now;
                    Plugin.DumpSnapshot("手动");
                    return;
                }

                // 触发点 1.5：Insert = **开关**窗口。
                //   窗口开着 → 关掉（并把鼠标还回游戏）
                //   窗口关着 → 跑 P1 单回合穷举，结果推进窗口并打开
                // ⚠ 用户要求"Insert 不仅要能开窗口还得能关窗口"，所以不能只调 Run()。
                if (Plugin.StaticSolveKey.IsDown() && now - _lastSolveTime > 0.5f)
                {
                    _lastSolveTime = now;
                    if (SolverUI.Visible)
                    {
                        SolverUI.Hide();
                    }
                    else
                    {
                        P1Runner.Run();
                    }
                    return;
                }

                // M4：每帧保活窗口对象（插件组件会被 Unity 销毁，见 SolverUI 注释）
                try
                {
                    SolverUI.Ensure();

                }
                catch (Exception e)
                {
                    if (!_errorReported)
                    {
                        _errorReported = true;
                        Plugin.Log?.LogError($"[FHSolver] SolverUI 保活异常: {e.Message}");
                    }
                }

                // 地块坐标显示：每帧跟着「游戏绑定的那个控制」走（默认按住 Tab 才亮）。
                // 放在这里而不是 Init 里，因为按住状态必须逐帧判断。
                try
                {
                    DebugToggle.Tick();
                }
                catch (Exception e)
                {
                    if (!_errorReported)
                    {
                        _errorReported = true;
                        Plugin.Log?.LogWarning($"[FHSolver] 地块坐标开关异常: {e.Message}");
                    }
                }

                // 触发点 2：进入 / 离开战斗
                if (inTactical && !_wasInTactical)
                {
                    _wasInTactical = true;
                    _lastRound = -1;
                    _lastAutoDump = now;
                    Plugin.DumpSnapshot("进入战斗");
                }
                else if (!inTactical && _wasInTactical)
                {
                    _wasInTactical = false;
                    _lastRound = -1;
                    Plugin.Log?.LogInfo($"[FHSolver] 已离开战斗");
                }

                // 触发点 3：回合推进 —— 每完成一个回合周期自动再抓一次快照，
                // 用于比对牌堆消耗是否与预期一致（M1.5 牌堆预测正确性验证的核心依据）
                if (inTactical && _wasInTactical)
                {
                    int round = -1;
                    try
                    {
                        ScenarioRuleLibrary.ScenarioState st = ScenarioRuleLibrary.ScenarioManager.CurrentScenarioState;
                        if (st != null) round = st.RoundNumber;
                    }
                    catch { /* 忽略，下一帧再试 */ }

                    if (round > 0)
                    {
                        if (_lastRound > 0 && round != _lastRound)
                        {
                            _lastAutoDump = now;
                            Plugin.DumpSnapshot($"回合 {_lastRound} → {round}");
                        }
                        _lastRound = round;
                    }
                }

                // 触发点 4（保险）：战斗中每 15 秒强制快照。
                // 万一上面三个触发点全部失效，这一条仍能保证拿到数据。
                if (inTactical && now - _lastAutoDump > 15f)
                {
                    _lastAutoDump = now;
                    Plugin.DumpSnapshot("自动(15s)");
                }
            }
            catch (Exception e)
            {
                if (!_errorReported)
                {
                    _errorReported = true;
                    Plugin.Log?.LogError($"[FHSolver] TickPump 异常（仅报一次）: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal static string HitSummary()
        {
            var parts = new List<string>();
            foreach (var kv in HitCounts) parts.Add($"{kv.Key}={kv.Value}");
            return parts.Count == 0 ? "(无)" : string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Harmony patch 载体。目标方法在运行时反射解析（不在编译期引用 Assembly-CSharp，
    /// 避免把依赖图极深的 Assembly-CSharp 拉进本工程）。
    /// </summary>
    internal static class TickPatch
    {
        internal static IEnumerable<MethodBase> CollectTargets()
        {
            foreach (string typeName in TickPump.CandidateTypeNames)
            {
                Type t = Type.GetType(typeName + ", Assembly-CSharp");
                if (t == null)
                {
                    Plugin.Log?.LogWarning($"[FHSolver] 挂载点类型不存在: {typeName}");
                    continue;
                }

                MethodInfo m = t.GetMethod("Update",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);

                if (m == null)
                {
                    Plugin.Log?.LogWarning($"[FHSolver] {typeName} 无无参 Update，跳过");
                    continue;
                }

                yield return m;
            }
        }

        public static void Postfix(MethodBase __originalMethod)
        {
            TickPump.OnGameTick(__originalMethod?.DeclaringType?.Name ?? "?");
        }
    }
}
