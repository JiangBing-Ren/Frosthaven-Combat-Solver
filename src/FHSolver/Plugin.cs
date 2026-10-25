using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FHSolver.Model;
using UnityEngine;

namespace FHSolver
{
    /// <summary>
    /// 战斗求解器 —— 只读战斗辅助插件。
    /// M0：BepInEx 链路自检。
    /// M1：只读读取战局快照 + 攻击修正牌牌堆顺序。
    ///
    /// 注意：轮询逻辑不写在 Plugin 里，见 Runner.cs 的说明。
    /// </summary>
    [BepInPlugin(GUID, PluginName, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "hanf2.Frosthaven.FHSolver";
        public const string PluginName = "FHSolver";
        public const string Version = "1.28.0";




        internal static ManualLogSource Log { get; private set; }

        /// <summary>
        /// 保留仅用于诊断。注意：Plugin 继承 MonoBehaviour，
        /// 组件被 Unity 销毁后 UnityEngine.Object 的 == 重载会让本属性"表现得像 null"。
        /// 轮询逻辑一律不许依赖它，改用下面的静态快照值。
        /// </summary>
        internal static Plugin Instance { get; private set; }

        // ---- 以下三项在 Awake 里从 config 快照出来，之后不再依赖插件实例 ----
        // 实测（2026-10-22 v0.3.1）：插件 MonoBehaviour 在启动后不久被 Unity 销毁，
        // 导致 Plugin.Instance 与 TickPump._owner 全部变成 "Unity fake null"，
        // 快照逻辑因此一次都没触发。日志能打出来只是因为 ManualLogSource 不是 UnityEngine.Object。
        internal static bool StaticEnabled { get; private set; }
        internal static KeyboardShortcut StaticDumpKey { get; private set; }
        internal static int StaticPeek { get; private set; }

        /// <summary>F2：跑 P1 单回合穷举。</summary>
        internal static KeyboardShortcut StaticSolveKey { get; private set; }

        /// <summary>P1 输出几个候选方案。</summary>
        internal static int StaticTopN { get; private set; }

        /// <summary>求解完成后自动弹出窗口（M4）。关掉就只写文件，不打扰。</summary>
        internal static bool StaticShowUiOnSolve { get; private set; }

        /// <summary>是否做多回合推演（P5）。</summary>
        internal static bool StaticMultiTurnEnabled { get; private set; }

        /// <summary>多回合推演深度（P5）。</summary>
        internal static int StaticMultiTurnDepth { get; private set; }

        /// <summary>多回合推演每层保留的分支数（P5）。</summary>
        internal static int StaticMultiTurnBeam { get; private set; }

        /// <summary>多样性分桶容量（同一打法最多保留几个方案）。</summary>
        internal static int StaticDiversityPerBucket { get; private set; }

        /// <summary>是否在报告里显示「原生驱动探测」段（只读诊断，不改游戏状态）。</summary>
        internal static bool StaticDriverProbe { get; private set; }


        /// <summary>报告里是否显示阴影段（死行）。关掉可省几行。</summary>
        internal static bool StaticShadowDiagnostics { get; private set; }

        /// <summary>P1 推荐落地文件路径（静态快照，组件销毁后仍可用）。</summary>
        internal static string StaticReportPath { get; private set; }

        /// <summary>是否打开原版自带的「地块坐标」调试显示。</summary>
        internal static bool StaticShowTileIndex { get; private set; }

        /// <summary>true = 按住绑定的控制才显示；false = 常亮。</summary>
        internal static bool StaticHoldToShow { get; private set; }

        /// <summary>true = 给全队每个角色各出一份方案；false = 只出当前行动者。</summary>
        internal static bool StaticPlanAllPlayers { get; private set; }

        /// <summary>
        /// true = 把"先攻更低（更先动）的队友"预测造成的伤害扣掉再算。
        /// 快照是在出牌阶段拍的，先攻低的队友动手后血量才会变 ——
        /// 不补这个就会拿"满血 10"去算一个其实已经被队友打过的目标。
        /// </summary>
        internal static bool StaticPredictAllies { get; private set; }

        private ConfigEntry<bool> _enabled;
        private ConfigEntry<KeyboardShortcut> _dumpKey;
        private ConfigEntry<int> _peekCount;
        private ConfigEntry<KeyboardShortcut> _solveKey;
        private ConfigEntry<int> _topN;
        private ConfigEntry<bool> _showUiOnSolve;
        private ConfigEntry<bool> _multiTurnEnabled;
        private ConfigEntry<int> _multiTurnDepth;
        private ConfigEntry<int> _multiTurnBeam;
        private ConfigEntry<bool> _shadowDiagnostics;
        private ConfigEntry<int> _diversityPerBucket;
        private ConfigEntry<bool> _driverProbe;

        private ConfigEntry<string> _reportPath;
        private ConfigEntry<bool> _showTileIndex;
        private ConfigEntry<bool> _holdToShow;
        private ConfigEntry<bool> _planAllPlayers;
        private ConfigEntry<bool> _predictAllies;

        internal bool Enabled => _enabled?.Value ?? false;

        private void Awake()
        {
            Log = Logger;
            Instance = this;

            _enabled = Config.Bind("General", "Enabled", true, "是否启用求解器");
            _dumpKey = Config.Bind("General", "DumpKey",
                new KeyboardShortcut(KeyCode.F1), "输出战局快照的快捷键（F1，游戏输入绑定里空闲）");
            _peekCount = Config.Bind("Solver", "PeekCount", 5,
                "输出牌堆时预读的张数（AoE / 多目标推演用）");
            // 默认 Insert —— 已从游戏输入绑定实测确认空闲。
            // 游戏实际只占 39 个键：1-4 a alt b c comma ctrl d 箭头 enter escape f5 f9 l [ ] 
            // numpad* pageup/down period q r s space t tab v w x end。
            // Insert / F10 / F11 / F12 / Home / ~ 都空闲（End 被 ToggleSpeedUp 占用，不要用）。
            _solveKey = Config.Bind("Solver", "SolveKey",
                new KeyboardShortcut(KeyCode.Insert),
                "打开/刷新求解器推荐（默认 Insert：已核实游戏输入绑定里空闲）；"
                + "候选备选 F10 / F11 / F12 / Home / ~ 同样空闲");
            _topN = Config.Bind("Solver", "TopN", 5, "P1 输出几个候选方案");
            _showUiOnSolve = Config.Bind("Hud", "ShowUiOnSolve", true,
                "求解完成后自动弹出可拖拽窗口（关掉则只写文件，不打扰）");
            _multiTurnEnabled = Config.Bind("Solver", "MultiTurnEnabled", true,
                "是否做多回合推演（P5，束搜索）");
            _multiTurnDepth = Config.Bind("Solver", "MultiTurnDepth", 2,
                "多回合推演深度（P5）。越大越慢，每多一层约多一次全枚举");
            _multiTurnBeam = Config.Bind("Solver", "MultiTurnBeam", 3,
                "多回合推演每层保留的分支数（P5）。越大搜索越宽");
            _shadowDiagnostics = Config.Bind("Hud", "ShadowDiagnostics", true,
                "报告里显示【阴影】段（死行：当前有几个、在哪，0 个时会提示哪些牌用不了）");
            _diversityPerBucket = Config.Bind("Solver", "DiversityPerBucket", 2,
                "多样性分桶：同一打法（打谁/站哪）最多保留几个方案。0=关闭。调大更集中，调小更多样");
            _driverProbe = Config.Bind("Solver", "DriverProbe", true,
                "报告里显示「原生驱动探测」段（只读诊断，不改游戏状态）");

            _reportPath = Config.Bind("Solver", "ReportPath",
                @"D:\1Mods\FH\p1_recommend.txt",
                "P1 推荐的落地文件（每次按求解键覆盖写）—— 在游戏外开个小窗就能看");
            _showTileIndex = Config.Bind("Hud", "ShowTileIndex", true,
                "打开原版自带的「地块坐标」调试显示（DebugMenu.ConsoleVar_ShowTileIndex）");
            _holdToShow = Config.Bind("Hud", "HoldToShow", true,
                "按住才显示地块坐标。跟随游戏里 Tab 绑定的那个控制（ScenarioWorldspace/HighlightInteractablesOverlay），"
                + "改键后自动跟着变；关掉则常亮");
            _planAllPlayers = Config.Bind("Solver", "PlanAllPlayers", true,
                "给全队每个角色各出一份方案。这游戏强制两人开局，单人档也是多角色");
            _predictAllies = Config.Bind("Solver", "PredictEarlierAllies", true,
                "按先攻顺序，把先攻更低（更先动）的队友预测造成的伤害扣掉再算。"
                + "快照拍于出牌阶段，不补这个就会拿满血去算已经被队友打过的目标");

            // 立刻快照成静态值：此后即使组件被销毁也不影响
            StaticEnabled = _enabled?.Value ?? false;
            StaticDumpKey = _dumpKey != null ? _dumpKey.Value : new KeyboardShortcut(KeyCode.F1);
            StaticPeek = _peekCount?.Value ?? 5;
            // 兜底值必须跟 Config.Bind 的默认值一致（Insert）。
            // ⚠ 这里曾经是 Ctrl+F2 —— 改了 Bind 默认值却没改这行，
            //   导致 config 读不到时会静默回退到旧键，属于典型的遗留 bug。
            StaticSolveKey = _solveKey != null ? _solveKey.Value : new KeyboardShortcut(KeyCode.Insert);
            StaticTopN = _topN?.Value ?? 5;
            StaticShowUiOnSolve = _showUiOnSolve?.Value ?? true;
            StaticMultiTurnEnabled = _multiTurnEnabled?.Value ?? true;
            StaticMultiTurnDepth = _multiTurnDepth?.Value ?? 2;
            StaticMultiTurnBeam = _multiTurnBeam?.Value ?? 3;
            StaticShadowDiagnostics = _shadowDiagnostics?.Value ?? true;
            StaticDiversityPerBucket = _diversityPerBucket?.Value ?? 2;
            StaticDriverProbe = _driverProbe?.Value ?? true;

            StaticReportPath = _reportPath?.Value ?? @"D:\1Mods\FH\p1_recommend.txt";
            StaticShowTileIndex = _showTileIndex?.Value ?? true;
            StaticHoldToShow = _holdToShow?.Value ?? true;
            StaticPlanAllPlayers = _planAllPlayers?.Value ?? true;
            StaticPredictAllies = _predictAllies?.Value ?? true;

            Log.LogInfo($"[{PluginName}] ===== 插件已加载 v{Version} (GUID {GUID}) =====");

            // 运行环境取证
            Log.LogInfo($"[{PluginName}] BepInEx   : {typeof(BepInPlugin).Assembly.GetName().Version}");
            Log.LogInfo($"[{PluginName}] Harmony   : {typeof(HarmonyLib.Harmony).Assembly.GetName().Version}");
            Log.LogInfo($"[{PluginName}] Unity     : {Application.unityVersion}");
            Log.LogInfo($"[{PluginName}] 产品名     : {Application.productName} v{Application.version}");
            Log.LogInfo($"[{PluginName}] 快照快捷键 : {_dumpKey.Value}");
            Log.LogInfo($"[{PluginName}] 求解快捷键 : {_solveKey.Value}   TopN={_topN.Value}");
            Log.LogInfo($"[{PluginName}] 推荐落地文件: {_reportPath.Value}");
            Log.LogInfo($"[{PluginName}] 地块坐标显示: {_showTileIndex.Value}"
                + $"（按住才显示={_holdToShow.Value}）  全队出方案={_planAllPlayers.Value}");
            Log.LogInfo($"[{PluginName}] 团队联合规划  : {_predictAllies.Value}"
                + "（按先攻串行重算：先动队友按终点占位 + 扣掉其预计伤害）");

            ProbeGameTypes();

            // 轮询入口：挂到游戏自身的方法上，不用 MonoBehaviour.Update
            // （后者实测不触发，详见 Bridge/TickPump.cs 的说明）
            Bridge.TickPump.Init(this);

            Log.LogInfo($"[{PluginName}] ===== 自检完成，等待进入战斗 =====");
        }

        internal static void DumpSnapshot(string reason)
        {
            Model.BattleSnapshot snap = Bridge.StateReader.Read();

            if (!snap.Valid)
            {
                Log?.LogInfo($"[{PluginName}] ({reason}) 当前不在战斗中，无快照");
                return;
            }

            int peek = StaticPeek;
            Log.LogInfo($"[{PluginName}] ===== 战局快照 ({reason}) =====");
            Log.LogInfo($"[{PluginName}] 场景 {snap.ScenarioId} | 回合 {snap.RoundNumber}");

            Log.LogInfo($"[{PluginName}] -- 玩家 {snap.Players.Count} 名 --");
            for (int i = 0; i < snap.Players.Count; i++)
            {
                Model.UnitSnapshot u = snap.Players[i];
                Log.LogInfo($"[{PluginName}]   {u} 手牌{u.HandCardCount} 已选{u.SelectedCardCount}");

                // 手牌详情（P1 枚举输入）
                if (i < snap.PlayerHands.Count)
                {
                    foreach (AbilityCardSnapshot c in snap.PlayerHands[i])
                    {
                        Log.LogInfo($"[{PluginName}]     手牌| {c}");
                    }
                }

                if (i < snap.PlayerDecks.Count)
                {
                    Model.ModifierDeckSnapshot d = snap.PlayerDecks[i];
                    Log.LogInfo($"[{PluginName}]     {d.ToSummaryString()}");
                    Log.LogInfo($"[{PluginName}]     牌序(前{peek}): {d.ToDeckString(peek)}");
                }
            }

            Log.LogInfo($"[{PluginName}] -- 怪物 {snap.Monsters.Count} 只（另有已死 {snap.DeadMonsterCount} 只）--");
            foreach (Model.UnitSnapshot m in snap.Monsters)
            {
                Log.LogInfo($"[{PluginName}]   {m}");
            }

            Log.LogInfo($"[{PluginName}] -- 怪物牌堆 --");
            foreach (Model.ModifierDeckSnapshot d in snap.MonsterDecks)
            {
                if (d.Remaining.Count == 0 && d.Discarded.Count == 0) continue;
                Log.LogInfo($"[{PluginName}]   {d.ToSummaryString()}");
                Log.LogInfo($"[{PluginName}]     牌序(前{peek}): {d.ToDeckString(peek)}");
            }

            Log.LogInfo($"[{PluginName}] ===== 快照结束 =====");
        }

        private static readonly string[] TypeProbes =
        {
            // 战斗状态
            "ScenarioRuleLibrary.CActor, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.ScenarioManager, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.ScenarioState, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.GameState, ScenarioRuleLibrary",
            // 修正牌（MODIFIER.md 逆向成果的全部关键类型）
            "ScenarioRuleLibrary.CCharacterClass, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.CMonsterAttackModifierDeck, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.AttackModifierDeckState, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.YML.AttackModifierYMLData, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.Extensions+RandomState, ScenarioRuleLibrary",
            "SharedLibrary.Random, SharedLibrary",
            // 玩家
            "ScenarioRuleLibrary.CPlayerActor, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.CEnemyActor, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.CScenario, ScenarioRuleLibrary",
            // 伤害
            "ScenarioRuleLibrary.CAttackSummary, ScenarioRuleLibrary",
            "ScenarioRuleLibrary.CAbilityAttack, ScenarioRuleLibrary",
            // 表现层
            "Choreographer, Assembly-CSharp",
            // 经济/城建（注意：命名空间是 FHRL，不是 ForsthavenRuleLibrary.FHRL）
            "FHRL.DefRepository, ForsthavenRuleLibrary",
            "FHRL.ScenarioController, ForsthavenRuleLibrary",
        };

        private void ProbeGameTypes()
        {
            Log.LogInfo($"[{PluginName}] ---- 游戏类型探测 ----");
            int ok = 0;
            foreach (string probe in TypeProbes)
            {
                string shortName = probe.Split(',')[0];
                try
                {
                    Type t = Type.GetType(probe);
                    if (t != null)
                    {
                        ok++;
                        Log.LogInfo($"[{PluginName}]   [OK  ] {shortName}");
                    }
                    else
                    {
                        Log.LogWarning($"[{PluginName}]   [缺失] {shortName}");
                    }
                }
                catch (Exception e)
                {
                    Log.LogWarning($"[{PluginName}]   [异常] {shortName} : {e.Message}");
                }
            }
            Log.LogInfo($"[{PluginName}] 类型探测结果: {ok}/{TypeProbes.Length} 命中");
        }
    }
}
