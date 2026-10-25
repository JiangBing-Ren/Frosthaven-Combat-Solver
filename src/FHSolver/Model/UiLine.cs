namespace FHSolver.Model
{
    /// <summary>UI 行的样式（决定颜色/字号）。</summary>
    public enum UiStyle
    {
        /// <summary>大标题（分区名）。</summary>
        Head,

        /// <summary>小标题（角色名 / 段落名）。</summary>
        Sub,

        /// <summary>普通正文。</summary>
        Normal,

        /// <summary>强调（Top-1 方案 / 击杀）。</summary>
        Good,

        /// <summary>警告（Lost / 未建模 / 会先动）。</summary>
        Warn,

        /// <summary>次要信息（调试 / 说明）。</summary>
        Dim,
    }

    /// <summary>
    /// 窗口里的一行。求解器把报告**结构化**产出，
    /// UI 只负责按样式渲染 —— 这样 Solver 层不用管显示细节。
    /// </summary>
    public sealed class UiLine
    {
        public string Text = "";
        public UiStyle Style = UiStyle.Normal;

        public UiLine() { }

        public UiLine(string text, UiStyle style = UiStyle.Normal)
        {
            Text = text;
            Style = style;
        }

        public override string ToString() => Text;
    }
}
