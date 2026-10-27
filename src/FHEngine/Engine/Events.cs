using System.Collections.Generic;

namespace FHEngine.Engine
{
    /// <summary>
    /// 事件类型 —— 事件溯源的核心。
    ///
    /// 约束（阶段 4 对拍直接依赖）：
    ///   · 状态**只能**通过 apply(state, event) 折叠产生；
    ///   · 事件日志按 seq 单调递增，可完整重放；
    ///   · payload 为**弱类型**字典（键见各事件的字段定义），保证新增字段不破坏对拍工具。
    /// </summary>
    public enum EventType
    {
        /// <summary>玩家/AI 选定了本回合的两张牌与其半边。</summary>
        CARD_SELECTED,

        /// <summary>先攻公开（含怪物翻出的行动卡先攻）。</summary>
        INITIATIVE_REVEALED,

        /// <summary>某个单位开始其行动。</summary>
        TURN_STARTED,

        /// <summary>一个能力（半边动作）结算完成。</summary>
        ABILITY_RESOLVED,

        /// <summary>
        /// 一次攻击的攻击力结算（基础攻击 + 修正 = 最终值）。
        ///
        /// 为什么要单独一个事件：ATTACK_MODIFIER_DRAWN 只记录"抽到什么牌"，
        /// 中间"基础攻击 → 应用修正 → 最终攻击值"这一步若没有落点，
        /// 对拍粒度会变粗，也无法溯源伤害是怎么算出来的。
        /// </summary>
        ATTACK_RESOLVED,

        /// <summary>一次攻击抽到了修正牌。</summary>
        ATTACK_MODIFIER_DRAWN,

        /// <summary>伤害落地（含护盾/穿透结算后的最终值）。</summary>
        DAMAGE_APPLIED,

        /// <summary>元素灌注（**只在回合结束**）。</summary>
        ELEMENT_INFUSED,

        /// <summary>元素被消耗。</summary>
        ELEMENT_CONSUMED,

        /// <summary>
        /// 元素衰减（ROUND_END 内，Strong → Waning → Inert）。
        ///
        /// 为什么必须有事件承载：元素表是战斗状态的一部分，衰减是对状态的修改。
        /// 按事件溯源原则，任何状态修改都要有事件 —— 否则**回放时元素表无法从事件日志重建**。
        /// </summary>
        ELEMENT_DECAYED,

        /// <summary>某个单位结束其行动。</summary>
        TURN_ENDED,

        /// <summary>回合结束。</summary>
        ROUND_ENDED,
    }

    /// <summary>
    /// 引擎事件。日志格式（JSON Lines，一行一个事件）：
    /// <code>{ "seq", "round", "turn", "actorId", "type", "payload" }</code>
    /// </summary>
    public sealed class EngineEvent
    {
        /// <summary>
        /// 全局单调递增序号，从 0 开始，**不可重复、不可回退**。
        /// 阶段 4 的 diff 工具按 seq 对齐两侧事件流。
        /// </summary>
        public long Seq;

        /// <summary>回合号，从 1 开始。</summary>
        public int Round;

        /// <summary>本回合内的行动序号（TurnIndex）；非 TURN_* 事件沿用当前值。</summary>
        public int Turn;

        /// <summary>发起/关联单位的 Id；全局事件（如 ROUND_ENDED）用 "*"。</summary>
        public string ActorId;

        public EventType Type;

        /// <summary>
        /// 事件负载。各 Type 的键约定（阶段 2 文档 §1 有完整表）：
        ///   CARD_SELECTED        : cardIds(string[]), halves(string[])
        ///   INITIATIVE_REVEALED  : initiative(int), source("card"|"default")
        ///   TURN_STARTED         : kind("player"|"monster")
        ///   ABILITY_RESOLVED     : abilityName, half("Top"|"Bottom"), cardId
        ///   ATTACK_RESOLVED       : baseAttack(int), modifierMath(string), finalAttack(int), targetId
        ///   ATTACK_MODIFIER_DRAWN : modifierName, mathModifier, isRolling, deckOwner, drawnCount
        ///   DAMAGE_APPLIED       : targetId, amount, shieldAbsorbed, pierce, killed
        ///   ELEMENT_INFUSED      : elements(string[]), byActorId
        ///   ELEMENT_CONSUMED     : elements(string[]), byActorId, forAbility
        ///   ELEMENT_DECAYED      : before(object), after(object) —— **全量**快照，
        ///                          6 个元素各记一档（Fire/Ice/Air/Earth/Light/Dark）。
        ///                          即使本轮只有 Fire 变化，其余 5 个的当前档位也要记进 after。
        ///                          全量让 apply 成为纯函数：state.Elements = after，零推导；
        ///                          增量会让 apply 需要向前推导未变化元素，推导链长且易错。
        ///   TURN_ENDED           : actorId
        ///   ROUND_ENDED          : round
        /// </summary>
        public Dictionary<string, object> Payload = new Dictionary<string, object>();

        /// <summary>按 seq/round/turn/actorId/type/payload 的顺序序列化为一行 JSON。</summary>
        public string ToJsonLine()
        {
            throw new System.NotImplementedException("阶段 3 实现");
        }
    }

    /// <summary>
    /// 事件应用器 —— **唯一**允许修改 BattleState 的地方。
    /// 纯函数语义：apply(s, e) 返回一个新状态，不修改入参（便于回放与对拍）。
    /// </summary>
    public static class EventApplier
    {
        public static BattleState Apply(BattleState state, EngineEvent e)
        {
            throw new System.NotImplementedException("阶段 3 实现");
        }
    }
}
