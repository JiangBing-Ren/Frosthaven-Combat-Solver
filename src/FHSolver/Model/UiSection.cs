using System.Collections.Generic;
using System.Linq;

namespace FHSolver.Model
{
    /// <summary>
    /// 窗口里的一个分类页（M4 v2）。
    ///
    /// 用户要求按**角色**分开，再单列敌人、战利品、日志、报错：
    ///   |漂泊者| |旗矛手| |死亡行者| |敌人| |战利品| |日志| |报错|
    ///
    /// 每个页签只装自己的行，点哪个看哪个 —— 避免一坨长文本滚半天。
    /// </summary>
    public sealed class UiSection
    {
        public string Name = "";
        public List<UiLine> Lines = new List<UiLine>();

        public UiSection() { }

        public UiSection(string name, List<UiLine> lines = null)
        {
            Name = name;
            Lines = lines ?? new List<UiLine>();
        }

        public void Add(string text, UiStyle style = UiStyle.Normal)
        {
            Lines.Add(new UiLine(text, style));
        }

        /// <summary>
        /// 给一个新页签起一个**不与现有页签重名**的名字。
        ///
        /// ⚠ 之前就是漏了这步：每个块默认都叫「角色」，于是窗口上冒出十几个
        ///   一模一样的「角色」页签（只有第一个碰巧被改成了角色名）。
        ///   抽成纯函数是为了能在离线测试台里直接回归，Bridge 层测不到。
        /// </summary>
        /// <param name="existing">已经存在的页签名。</param>
        /// <param name="preferred">期望的名字（例如从报告标题里抽出的角色名）。</param>
        /// <param name="fallback">抽不到时的兜底名。</param>
        public static string UniqueName(IEnumerable<string> existing, string preferred,
            string fallback = "角色")
        {
            string name = string.IsNullOrEmpty(preferred) ? fallback : preferred;
            var set = new HashSet<string>(existing ?? Enumerable.Empty<string>());

            if (!set.Contains(name)) return name;

            int n = 1;
            while (set.Contains(name + n)) n++;
            return name + n;
        }

        public override string ToString() => $"[{Name}] {Lines.Count} 行";
    }
}
