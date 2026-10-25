using System;
using System.Collections.Generic;
using FHSolver.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 求解器窗口（M4）—— **独立可拖拽窗口**，Unity IMGUI 实现。
    ///
    /// ═══════════════════════════════════════════════════════════════
    /// 为什么是 IMGUI：
    ///   · `GUI.DragWindow` 一行实现拖拽，Canvas 要自己写事件转发
    ///   · 求解器是信息面板，不需要动效/自适应布局
    ///
    /// ⚠ 插件自己的 MonoBehaviour 会被 Unity 销毁（见 Plugin.cs 长注释），
    ///   所以**自建 GameObject + DontDestroyOnLoad**，并由 TickPump
    ///   （挂在游戏自身 Update 上）每帧 `Ensure()`，fake-null 时重建。双保险。
    ///
    /// 中文字体：IMGUI 默认字体不含中文 → `Font.CreateDynamicFontFromOSFont`
    ///   取系统字体（雅黑/黑体），保证卡名、目标名正常显示。
    ///
    /// **输入不透传**（用户反馈：鼠标移入/左键/滚轮还会影响游戏）：
    ///   鼠标位于窗口内时 `InputSystem.DisableDevice(Mouse.current)`。
    ///   这是**只禁鼠标设备**，键盘不受影响 —— 所以 Insert 仍能开关窗口。
    ///   IMGUI 的 Event 走 legacy 通道，独立于 Input System，窗口自己照常收事件。
    /// ═══════════════════════════════════════════════════════════════
    /// </summary>
    internal class SolverUI : MonoBehaviour
    {
        private const int WindowId = 0x5F41;

        // 布局：标题栏加高（用户要求），其下依次是按钮栏、分类页签、内容
        private const float TitleH = 26f;
        private const float ButtonBarH = 26f;
        private const float TabBarH = 26f;

        private static SolverUI _instance;
        private static GameObject _root;

        private static Rect _rect = new Rect(40, 40, 640, 660);

        // ⚠ 展开高度要单独记：GUI.Window 会把返回值赋回 _rect，
        //   收起时高度被写成 TitleH+ButtonBarH(52)，_rect.height 就永久丢了，
        //   再点"+"只会得到 52 高的空壳 —— 表现就是"收起后无法展开"。
        private static float _expandedHeight = 660f;
        private static Vector2 _scroll = Vector2.zero;

        // 按分类页组织内容（用户要求：角色 / 敌人 / 战利品 / 日志 / 报错）
        private static List<UiSection> _sections = new List<UiSection>();
        private static int _tab;
        private static bool _collapsed;
        private static bool _visible;
        private static string _title = "FHSolver";

        private static Font _font;
        private static bool _fontTried;

        private static bool _mouseBlocked;

        // 最近一次看到的鼠标位置（GUI 坐标系）。由 OnGUI 从 Event.current 更新。
        private static Vector2 _lastMouse = new Vector2(-1f, -1f);

        // 看门狗：拦截期间鼠标位置连续多少帧**完全没动**。
        // 正常情况（玩家只是把鼠标停在窗口上看方案）也会累加，所以到点强制放行 1 帧。
        // 作用：万一 Event 通道也拿不到新位置（设备被禁用的极端情况），
        //       不会永久锁死鼠标 —— 每 2 秒给玩家一次重新定位的机会。
        private static int _staleFrames;
        private const int StaleLimit = 120;      // ≈2 秒 @60fps

        // ---- 生命周期 ----

        /// <summary>由 TickPump 每帧调用；对象被销毁（fake null）时重建。</summary>
        public static void Ensure()
        {
            if (_root == null || _instance == null)
            {
                Create();
            }
        }

        private static void Create()
        {
            try
            {
                _root = new GameObject("FHSolver.UI");
                DontDestroyOnLoad(_root);
                _root.hideFlags = HideFlags.HideAndDontSave;
                _instance = _root.AddComponent<SolverUI>();
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 创建 UI 失败: {e.Message}");
            }
        }

        private static void EnsureFont()
        {
            if (_font != null || _fontTried) return;
            _fontTried = true;
            try
            {
                _font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei", "微软雅黑", "SimHei", "黑体", "Arial" }, 14);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 加载中文字体失败，回退默认: {e.Message}");
                _font = null;
            }
        }

        // ---- 对外接口 ----

        /// <summary>设置窗口内容（按分类页）。</summary>
        public static void SetSections(List<UiSection> sections, string title = null)
        {
            _sections = sections ?? new List<UiSection>();
            if (_tab >= _sections.Count) _tab = 0;
            if (!string.IsNullOrEmpty(title)) _title = title;
        }

        public static void Show()
        {
            _visible = true;
            Ensure();
        }

        public static void Hide()
        {
            _visible = false;
            SetMouseBlocked(false);
        }

        public static bool Visible => _visible;

        /// <summary>窗口上的「重新计算」按钮。</summary>
        public static void Recompute()
        {
            P1Runner.Run();
        }

        // ---- 鼠标拦截 ----

        private static void SetMouseBlocked(bool blocked)
        {
            if (blocked == _mouseBlocked) return;
            _mouseBlocked = blocked;
            try
            {
                Mouse m = Mouse.current;
                if (m == null) return;
                if (blocked) InputSystem.DisableDevice(m);
                else InputSystem.EnableDevice(m);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 鼠标拦截失败: {e.Message}");
                _mouseBlocked = false;
            }
        }

        /// <summary>鼠标是否在窗口内（收起时只算标题栏+按钮栏）。</summary>
        private static bool IsMouseInside()
        {
            var r = new Rect(_rect.x, _rect.y, _rect.width,
                _collapsed ? TitleH + ButtonBarH : _rect.height);
            return r.Contains(_lastMouse);
        }

        // ---- 绘制 ----

        private void Update()
        {
            // 只有"窗口不可见"这一件事放这里兜底。
            // ⚠ 绝不能在 Update 里判断"鼠标是否在窗口内"并据此禁用 ——
            //   那正是上一版的死锁来源（见 SetMouseBlocked 的注释）。
            if (!_visible) SetMouseBlocked(false);
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                SetMouseBlocked(false);
                return;
            }

            // ★ 鼠标位置必须从 IMGUI 的 Event 读（legacy 通道）。
            //   Mouse.current.position 在 DisableDevice 之后就不再更新，
            //   会冻结在"进入窗口那一刻"的坐标 → 判成永远在窗口内 → 死锁。
            Event e = Event.current;
            Vector2 prev = _lastMouse;
            if (e != null) _lastMouse = e.mousePosition;

            // ★ 只拦"鼠标真的在窗口上"的时候，移出去立刻放行 ——
            //   目的是让玩家能一边看方案一边操作游戏，不是接管鼠标。
            bool inside = IsMouseInside();

            if (inside && _lastMouse == prev)
            {
                _staleFrames++;
                if (_staleFrames > StaleLimit)
                {
                    inside = false;          // 放行一帧，打破潜在的位置冻结
                    _staleFrames = 0;
                }
            }
            else
            {
                _staleFrames = 0;
            }

            SetMouseBlocked(inside);

            EnsureFont();
            if (_font != null) GUI.skin.font = _font;

            float h = _collapsed ? TitleH + ButtonBarH : _expandedHeight;

            var win = GUI.Window(WindowId,
                new Rect(_rect.x, _rect.y, _rect.width, h), DrawWindow, _title);

            // ★ 只回填 x/y/width。**绝不能**把 win.height 直接写回 _rect.height：
            //   收起时 win.height 会变成 52，写回去就把展开高度永久覆盖了，
            //   再点"+"也只剩个空壳（这就是"收起后无法展开"的根因）。
            _rect.x = win.x;
            _rect.y = win.y;
            _rect.width = win.width;

            // 展开状态下才去记用户拖动调整后的高度（右下角可拖）
            if (!_collapsed && win.height > TitleH + ButtonBarH)
            {
                _expandedHeight = win.height;
            }

            _rect.x = Mathf.Clamp(_rect.x, -_rect.width + 80, Screen.width - 80);
            _rect.y = Mathf.Clamp(_rect.y, 0, Screen.height - 60);
        }

        private void DrawWindow(int id)
        {
            float w = _rect.width;

            // 铺一层不透明深色底 —— 默认 skin 在部分主题下偏透明，
            //   会透出后面的游戏画面，文字看着很吃力。
            var prevColor = GUI.color;
            GUI.color = new Color(0.07f, 0.07f, 0.09f, 0.97f);
            GUI.DrawTexture(new Rect(0, 0, w, _rect.height), Texture2D.whiteTexture);
            GUI.color = prevColor;

            // ---- 标题栏（加高，整条可拖）----
            GUI.DragWindow(new Rect(0, 0, w - 60, TitleH));

            // 右上角：收起/展开 + 关闭
            if (GUI.Button(new Rect(w - 56, 3, 24, TitleH - 6), _collapsed ? "+" : "-"))
            {
                _collapsed = !_collapsed;
            }
            if (GUI.Button(new Rect(w - 28, 3, 24, TitleH - 6), "x"))
            {
                Hide();
            }

            // ---- 按钮栏：重新计算 ----
            if (GUI.Button(new Rect(6, TitleH + 2, 92, ButtonBarH - 6), "重新计算"))
            {
                Recompute();
            }
            GUI.Label(new Rect(104, TitleH + 2, w - 112, ButtonBarH - 6),
                "Insert = 开关窗口", StyleDim());

            if (_collapsed) return;

            // ---- 分类页签 ----
            float x = 4;
            for (int i = 0; i < _sections.Count; i++)
            {
                string name = _sections[i].Name;
                float bw = Mathf.Max(54, name.Length * 14f + 12);
                if (x + bw > w - 4) break;              // 放不下就不画，避免挤爆
                if (GUI.Button(new Rect(x, TitleH + ButtonBarH, bw, TabBarH - 4), name))
                {
                    _tab = i;
                    _scroll = Vector2.zero;
                }
                x += bw + 2;
            }

            // ---- 内容 ----
            float top = TitleH + ButtonBarH + TabBarH;
            GUILayout.BeginArea(new Rect(0, top, w, _rect.height - top));
            _scroll = GUILayout.BeginScrollView(_scroll);

            if (_tab >= 0 && _tab < _sections.Count)
            {
                foreach (UiLine line in _sections[_tab].Lines)
                {
                    if (line != null) Render(line);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void Render(UiLine line)
        {
            string text = line.Text ?? "";
            switch (line.Style)
            {
                case UiStyle.Head:
                    GUILayout.Space(6);
                    GUILayout.Label(text, StyleHead());
                    break;
                case UiStyle.Sub:
                    GUILayout.Space(2);
                    GUILayout.Label(text, StyleSub());
                    break;
                case UiStyle.Good:
                    GUILayout.Label(text, StyleGood());
                    break;
                case UiStyle.Warn:
                    GUILayout.Label(text, StyleWarn());
                    break;
                case UiStyle.Dim:
                    GUILayout.Label(text, StyleDim());
                    break;
                default:
                    GUILayout.Label(text, StyleNormal());
                    break;
            }
        }

        // ---- 样式 ----

        private static GUIStyle _head, _sub, _normal, _good, _warn, _dim;

        private static GUIStyle Make(ref GUIStyle field, int size, Color color, FontStyle fs)
        {
            if (field == null)
            {
                field = new GUIStyle(GUI.skin.label)
                {
                    fontSize = size,
                    fontStyle = fs,
                    wordWrap = true,
                };
                field.normal.textColor = color;
            }
            return field;
        }

        private static GUIStyle StyleHead()
        {
            return Make(ref _head, 15, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
        }

        private static GUIStyle StyleSub()
        {
            return Make(ref _sub, 13, new Color(0.55f, 0.85f, 1f), FontStyle.Bold);
        }

        private static GUIStyle StyleNormal()
        {
            return Make(ref _normal, 12, new Color(0.92f, 0.92f, 0.92f), FontStyle.Normal);
        }

        private static GUIStyle StyleGood()
        {
            return Make(ref _good, 13, new Color(0.45f, 1f, 0.55f), FontStyle.Bold);
        }

        private static GUIStyle StyleWarn()
        {
            return Make(ref _warn, 12, new Color(1f, 0.6f, 0.35f), FontStyle.Normal);
        }

        private static GUIStyle StyleDim()
        {
            return Make(ref _dim, 11, new Color(0.62f, 0.62f, 0.62f), FontStyle.Normal);
        }
    }
}
