using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 性能分段计时（学 StS2 的 `frame_percentiles` / `allocation_telemetry`）。
    ///
    /// 为什么需要：之前只知道"枚举 2-3 ms"，但**没量过其它阶段**
    /// （读状态 / 敌人预测 / 多回合推演 / 生成报告）。
    /// 凭感觉优化容易优化错地方 —— 先量清楚，再动手。
    ///
    /// 用法：
    /// ```csharp
    /// var perf = new PerfTimers();
    /// perf.Mark("阶段A");   // 记录到上一个 Mark 的间隔
    /// ...
    /// perf.Summary();       // "读状态 3ms | 敌人预测 12ms | 总计 20ms"
    /// ```
    /// </summary>
    public sealed class PerfTimers
    {
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly Dictionary<string, double> _ms = new Dictionary<string, double>();
        private long _last;

        /// <summary>总计（毫秒）。</summary>
        public double TotalMs { get; set; }

        /// <summary>
        /// 记录"到上一个 Mark 为止"这一段的耗时。
        ///
        /// ⚠ **标签要描述"刚做完的那件事"**，不是"接下来要做的事" ——
        ///   否则每个标签都会错位一位（我第一版就踩了：标"读战役目标"，
        ///   实际记的是"启动到读目标"；标"P1枚举前"实际记的是"第二遍枚举"）。
        ///   正确用法：做完一件事立刻 Mark 它的名字。
        /// </summary>
        public void Mark(string label)
        {
            long now = _sw.ElapsedMilliseconds;
            double delta = now - _last;
            _last = now;
            Add(label, delta);
        }

        /// <summary>累加一段耗时（同名的会相加）。</summary>
        public void Add(string label, double ms)
        {
            double cur;
            _ms.TryGetValue(label, out cur);
            _ms[label] = cur + ms;
        }

        /// <summary>拼成一行可读摘要（按耗时降序）。</summary>

        public string Summary()
        {
            var sb = new StringBuilder();
            sb.Append("性能 ");
            var items = new List<KeyValuePair<string, double>>(_ms);
            items.Sort((a, b) => b.Value.CompareTo(a.Value));
            bool first = true;
            foreach (var kv in items)
            {
                if (!first) sb.Append(" | ");
                sb.Append(kv.Key).Append(' ').Append(kv.Value.ToString("F0")).Append("ms");
                first = false;
            }
            sb.Append(" | 总 ").Append(TotalMs.ToString("F0")).Append("ms");
            return sb.ToString();
        }
    }
}
