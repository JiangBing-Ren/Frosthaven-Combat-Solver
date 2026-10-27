using System.Collections.Generic;

namespace FHEngine.Engine
{
    /// <summary>
    /// 决策挂起点种类。
    ///
    /// 设计原则（硬性）：**引擎绝不自动判定**这些节点 —— 一律挂起，
    /// 等待外部注入决策。这是可回放与可测试的前提（决策序列是显式的输入）。
    /// </summary>
    public enum DecisionKind
    {
        /// <summary>优势 / 劣势：抽修正牌前选择抽 1 张（无）、2 张取好（优势）、2 张取坏（劣势）。</summary>
        AdvantageDisadvantage,

        /// <summary>燃烧卡牌：用打出一张手牌来抵消一次伤害。需玩家选择烧哪张。</summary>
        BurnCard,

        /// <summary>目标选择：多目标 / AOE 时选择打哪几个目标（或焦点格）。</summary>
        TargetSelection,

        /// <summary>元素消耗确认：能力可消耗元素时，是否消耗（以及消耗哪个）。</summary>
        ElementConsumption,

        /// <summary>长休卡选择：长休时选择恢复哪张已失去的牌。</summary>
        LongRest,

        /// <summary>多目标伤害分配：伤害可分配时，如何分配到各目标。</summary>
        DamageAssignment,
    }

    /// <summary>一个可选项的表示。</summary>
    public sealed class DecisionOption
    {
        /// <summary>选项标识（稳定、可比对）。</summary>
        public string Id;

        /// <summary>展示用描述（引擎内部用 Id；文本不属于本项目资产范围）。</summary>
        public string Label;

        /// <summary>该选项的附加数据（如目标 Id 列表、卡牌 Id）。</summary>
        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    /// <summary>
    /// 引擎发出的决策请求。引擎在发出后**暂停**，直到收到对应的 DecisionResponse。
    /// </summary>
    public sealed class DecisionRequest
    {
        /// <summary>与引发它的事件 seq 对齐（便于阶段 4 把决策插入事件流）。</summary>
        public long Seq;

        public DecisionKind Kind;

        /// <summary>需要做决定的单位 Id；"*" 表示由玩家侧统一决定。</summary>
        public string ActorId;

        /// <summary>触发时机描述，如 "before_draw_modifier" / "on_damage_taken"。</summary>
        public string Trigger;

        /// <summary>可选动作集合（**可为空**表示自由输入，如坐标选择）。</summary>
        public List<DecisionOption> Options = new List<DecisionOption>();
    }

    /// <summary>外部注入的决策结果。</summary>
    public sealed class DecisionResponse
    {
        /// <summary>对应 DecisionRequest.Seq。</summary>
        public long Seq;

        /// <summary>所选选项的 Id（Options 非空时）。</summary>
        public string ChosenOptionId;

        /// <summary>自由输入（如坐标）时使用的载荷。</summary>
        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    /// <summary>
    /// 决策提供者接口 —— 测试时喂固定序列，实战时由玩家 UI 或 AI 实现。
    /// </summary>
    public interface IDecisionProvider
    {
        /// <summary>请求一个决策。实现可阻塞（UI）或直接返回（脚本化测试）。</summary>
        DecisionResponse Request(DecisionRequest request);
    }

    /// <summary>脚本化决策提供者（回放/测试用）：按预先录制的序列顺序返回。</summary>
    public sealed class ScriptedDecisionProvider : IDecisionProvider
    {
        private readonly List<DecisionResponse> _script;
        private int _cursor;

        public ScriptedDecisionProvider(List<DecisionResponse> script)
        {
            _script = script ?? new List<DecisionResponse>();
        }

        public DecisionResponse Request(DecisionRequest request)
        {
            if (_cursor >= _script.Count)
            {
                throw new System.InvalidOperationException(
                    "决策脚本已耗尽，但引擎仍在请求决策（seq=" + request.Seq + "）。"
                    + "这通常意味着规则实现与录制的对局不一致。");
            }
            DecisionResponse r = _script[_cursor++];
            if (r.Seq != request.Seq)
            {
                throw new System.InvalidOperationException(
                    $"决策序列错位：脚本 seq={r.Seq}，引擎请求 seq={request.Seq}。");
            }
            return r;
        }
    }
}
