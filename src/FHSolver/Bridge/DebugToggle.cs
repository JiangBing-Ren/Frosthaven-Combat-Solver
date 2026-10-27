using System;
using System.Reflection;
using UnityEngine.InputSystem;
namespace FHSolver.Bridge
{
    /// <summary>
    /// 原版自带的「地块坐标」调试显示，改成「按住才显示」。
    ///
    /// 这是原版就有的功能，只是没暴露给玩家：
    ///   DebugMenu.ConsoleVar_ShowTileIndex（public static bool，默认 false）
    ///     └─ WorldspaceTileBehaviourUI.Update() 里
    ///        ArrayIndexText.enabled = DebugMenu.ConsoleVar_ShowTileIndex;
    ///
    /// 标签本身在 ClientScenarioManager 建图时就为每一块地创建好了
    /// （TileBehaviour.CreateWorldspaceTileBehaviourUI → TextMeshProUGUI），
    /// 内容形如 "12:7(W)"，即 X:Y + 通行标记。
    /// ⇒ 不用自己造 UI，置这个开关即可，字体/缩放/世界锚定全是游戏自己的。
    ///
    /// 「按住显示」跟着**控制**走，不跟着按键走：
    /// 从 FHControls 的输入绑定 JSON 里查实，Tab 绑的是
    /// ScenarioWorldspace / HighlightInteractablesOverlay。
    /// 玩家若在设置里改了键，这里自动跟着变 —— 因为读的是 InputAction.IsPressed()，
    /// 不是 Input.GetKey(KeyCode.Tab)。
    ///
    /// 同一个 DebugMenu 下还有几个可选项（需要就自己开）：
    ///   show_axial_tile_index / show_negative_hexes / show_ice_hexes
    ///   show_deactivated_actors / show_loot_deck
    /// </summary>
    internal static class DebugToggle
    {
        private const string ActionPath = "ScenarioWorldspace/HighlightInteractablesOverlay";

        private static FieldInfo _field;
        private static bool _fieldTried;

        private static InputAction _holdAction;
        private static bool _holdTried;

        private static bool _lastApplied = true;   // 首帧强制写一次

        // 每帧都会读一次控制状态；读失败只报一次，避免 60 条/秒刷日志
        private static bool _readErrorLogged;

        /// <summary>当前是否已打开。</summary>
        public static bool IsOn
        {
            get
            {
                try { return _field != null && (bool)_field.GetValue(null); }
                catch { return false; }
            }
        }

        /// <summary>
        /// 每帧调用。按配置决定「常亮」还是「按住才亮」。
        /// </summary>
        public static void Tick()
        {
            try
            {
                if (!Plugin.StaticShowTileIndex)
                {
                    Apply(false);
                    return;
                }

                Apply(Plugin.StaticHoldToShow ? IsHeld() : true);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 地块坐标开关刷新失败: {e.Message}");
            }
        }

        private static void Apply(bool on)
        {
            if (_field == null) ResolveField();
            if (_field == null) return;

            if (on == _lastApplied) return;
            _lastApplied = on;
            _field.SetValue(null, on);
        }

        /// <summary>
        /// 读游戏绑定的那个控制当前是否被按住。
        ///
        /// 为什么不直接调 IsPressed()：这个版本的 Unity.InputSystem 里
        /// `InputAction.IsPressed()` 已经不存在了（API 改过几轮，
        /// 有的是 IsInProgress()，有的只有 triggered / ReadValue）。
        /// 所以按候选逐个反射试，命中哪个用哪个，并记下来。
        /// 解析失败返回 false（= 不显示），只记一次日志。
        /// </summary>
        private static bool IsHeld()
        {
            if (_holdAction == null)
            {
                if (_holdTried) return false;
                _holdTried = true;

                try
                {
                    Type mgr = Type.GetType("InputFHManager, Assembly-CSharp");
                    PropertyInfo controlsProp = mgr?.GetProperty("Controls",
                        BindingFlags.Public | BindingFlags.Static);
                    object controls = controlsProp?.GetValue(null);
                    if (controls == null)
                    {
                        Plugin.Log?.LogWarning("[FHSolver] 取不到 InputFHManager.Controls");
                        return false;
                    }

                    // FHControls : IInputActionCollection2 —— 用接口拿，别按具体类型。
                    // FindAction 在这个版本挂在 IInputActionCollection2 上（不是 IInputActionCollection）。
                    var coll = controls as IInputActionCollection2;
                    _holdAction = coll?.FindAction(ActionPath, throwIfNotFound: false);

                    if (_holdAction == null)
                    {
                        Plugin.Log?.LogWarning($"[FHSolver] 找不到输入动作 {ActionPath}，「按住显示坐标」不可用");
                        return false;
                    }

                    // 先探测这个版本到底提供了哪种「是否按住」的读法，全部打进日志。
                    // 万一只剩边沿触发的 triggered，从日志就能看出来，不会静默失效。
                    string found = ProbeReaders(_holdAction);
                    Plugin.Log?.LogInfo($"[FHSolver] 已绑定「按住显示坐标」控制: {ActionPath}"
                        + $" | 可用读法: {found} | 采用: {_readerName}");
                }
                catch (Exception e)
                {
                    Plugin.Log?.LogWarning($"[FHSolver] 读取输入动作失败: {e.Message}");
                    _holdAction = null;
                    return false;
                }
            }

            return ReadPressed(_holdAction);
        }

        private static string _readerName;

        /// <summary>
        /// 探测这个版本的 InputAction 提供了哪些「是否按住」的读法，选出最优的那个。
        /// 优先级：IsPressed() > IsInProgress() > ReadValue&lt;float&gt;() > triggered(属性，边沿触发，最差)。
        /// 返回人类可读的探测结果，直接进日志。
        /// </summary>
        private static string ProbeReaders(object action)
        {
            if (action == null) { _readerName = "(无)"; return "(无)"; }
            Type t = action.GetType();

            var found = new System.Collections.Generic.List<string>();

            foreach (string name in new[] { "IsPressed", "IsInProgress" })
            {
                MethodInfo m = t.GetMethod(name, BindingFlags.Public | BindingFlags.Instance,
                    null, Type.EmptyTypes, null);
                if (m != null && m.ReturnType == typeof(bool)) found.Add(name + "()");
            }

            foreach (string name in new[] { "triggered", "isPressed" })
            {
                PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(bool)) found.Add(name + "(属性)");
            }

            // ReadValue<float>() 也能用：Button 类动作按住时返回 1
            try
            {
                MethodInfo generic = t.GetMethod("ReadValue", BindingFlags.Public | BindingFlags.Instance);
                if (generic != null)
                {
                    MethodInfo closed = generic.MakeGenericMethod(typeof(float));
                    object v = closed.Invoke(action, null);
                    if (v is float) found.Add("ReadValue<float>()");
                }
            }
            catch { /* 这个版本没有就用别的 */ }

            // 选最优
            _readerName = "(无可用读取方式)";
            foreach (string cand in new[] { "IsPressed()", "IsInProgress()" })
            {
                if (found.Contains(cand)) { _readerName = cand; break; }
            }
            if (_readerName == "(无可用读取方式)" && found.Contains("ReadValue<float>()"))
            {
                _readerName = "ReadValue<float>()";
            }
            if (_readerName == "(无可用读取方式)" && found.Count > 0)
            {
                _readerName = found[0];
            }

            return found.Count == 0 ? "(一个都没有)" : string.Join(", ", found);
        }

        /// <summary>反射读取 InputAction 的「是否按住」。</summary>
        private static bool ReadPressed(object action)
        {
            if (action == null) return false;
            Type t = action.GetType();
            if (string.IsNullOrEmpty(_readerName)) ProbeReaders(action);

            try
            {
                if (_readerName == "ReadValue<float>()")
                {
                    MethodInfo generic = t.GetMethod("ReadValue", BindingFlags.Public | BindingFlags.Instance);
                    if (generic != null)
                    {
                        object v = generic.MakeGenericMethod(typeof(float)).Invoke(action, null);
                        if (v is float f) return f > 0.5f;
                    }
                    return false;
                }

                string bare = _readerName.Replace("()", "").Replace("(属性)", "");

                MethodInfo m = t.GetMethod(bare, BindingFlags.Public | BindingFlags.Instance,
                    null, Type.EmptyTypes, null);
                if (m != null && m.ReturnType == typeof(bool))
                {
                    return (bool)m.Invoke(action, null);
                }

                PropertyInfo p = t.GetProperty(bare, BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(bool))
                {
                    return (bool)p.GetValue(action, null);
                }
            }
            catch (Exception e)
            {
                if (!_readErrorLogged)
                {
                    _readErrorLogged = true;
                    Plugin.Log?.LogWarning($"[FHSolver] 读取控制状态失败({_readerName}): {e.Message}（后续同类错误不再刷屏）");
                }
            }
            return false;
        }

        private static void ResolveField()
        {
            if (_fieldTried) return;
            _fieldTried = true;
            try
            {
                Type t = Type.GetType("DebugMenu, Assembly-CSharp");
                _field = t?.GetField("ConsoleVar_ShowTileIndex",
                    BindingFlags.Public | BindingFlags.Static);
                Plugin.Log?.LogInfo(_field != null
                    ? "[FHSolver] 已找到 DebugMenu.ConsoleVar_ShowTileIndex（原版地块坐标开关）"
                    : "[FHSolver] 未找到 DebugMenu.ConsoleVar_ShowTileIndex");
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 解析 DebugMenu 失败: {e.Message}");
            }
        }

        public static string Summary => IsOn ? "地块坐标=开" : "地块坐标=关";
    }
}
