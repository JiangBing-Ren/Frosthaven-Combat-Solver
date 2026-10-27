using System.Collections.Generic;

namespace FHEngine.Engine
{
    // ============================================================
    // 阶段 2 的**空壳类型定义**。
    // 按约定：本阶段只写类型声明 + 注释，方法体一律 throw new NotImplementedException()。
    // 实现留给阶段 3。
    // ============================================================

    /// <summary>六边形坐标。采用与本体一致的轴向/偏移表示（阶段 3 落地时确定，此处只占位）。</summary>
    public struct HexCoord
    {
        public int X;
        public int Y;

        public HexCoord(int x, int y) { X = x; Y = y; }

        public override string ToString() => $"({X},{Y})";
    }

    /// <summary>回合状态机的阶段。</summary>
    public enum RoundPhase
    {
        /// <summary>回合开始：结算回合起始效果（如元素可消耗性刷新）。</summary>
        ROUND_START,

        /// <summary>选牌：每个玩家角色选两张牌的上/下半。</summary>
        CARD_SELECTION,

        /// <summary>先攻排序：所有单位按先攻升序确定行动顺序（含怪物翻行动卡）。</summary>
        INITIATIVE_SORT,

        /// <summary>回合循环：按先攻顺序逐个执行单位的行动。</summary>
        TURN_LOOP,

        /// <summary>回合结束：元素灌注在此发生。</summary>
        ROUND_END,
    }

    /// <summary>一个参战单位（玩家角色 / 怪物 / 召唤物 / 盟友）的运行时状态。</summary>
    public sealed class ActorState
    {
        /// <summary>单位标识。按项目约束**不提取本地化文本**，统一用 Id / LocKey。</summary>
        public string Id;

        /// <summary>本地化键（仅作标识，不解析为文本）。</summary>
        public string LocKey;

        public int Hp;
        public int MaxHp;
        public int Shield;

        /// <summary>站位。</summary>
        public HexCoord Pos;

        /// <summary>本回合先攻；选牌后确定。</summary>
        public int Initiative;

        /// <summary>已选的两张牌（角色）或本回合行动卡（怪物）。</summary>
        public List<string> SelectedCardIds = new List<string>();

        /// <summary>本回合已打出的半边（Top / Bottom）。</summary>
        public List<string> PlayedHalves = new List<string>();

        /// <summary>施加在身上的状态（Impair / Poison / Wound …）。</summary>
        public List<string> Conditions = new List<string>();

        public bool IsDead;
    }

    /// <summary>怪物的额外状态（在 ActorState 基础上叠加怪物专有字段）。</summary>
    public sealed class MonsterState
    {
        /// <summary>怪物类 Id，如 "AlgoxScoutID"。</summary>
        public string MonsterClassId;

        /// <summary>场景等级 Level0–7。</summary>
        public int Level;

        /// <summary>当前基础属性（已按等级表解析）。</summary>
        public int Move;
        public int Attack;
        public int Range;
        public int Target;

        /// <summary>本回合翻出的行动卡 Id。</summary>
        public int CurrentCardId;

        /// <summary>该怪物的行动卡池（剩余，按顺序）。</summary>
        public List<int> AbilityCardDeck = new List<int>();

        /// <summary>是否已行动过（本回合）。</summary>
        public bool HasActed;
    }

    /// <summary>
    /// 地图状态。
    ///
    /// ⚠ 阶段 1–3 **不碰 .lvldat**（.NET BinaryFormatter，需 C# + 游戏程序集）。
    /// 这里使用**手工构造的平坦地图**类型，便于 AI 纯函数的单元测试。
    /// </summary>
    public sealed class MapState
    {
        public int Width;
        public int Height;

        /// <summary>地块类型：Normal / Difficult / Hazardous / Wall / Blocked。</summary>
        public string[] Tiles;

        /// <summary>被单位占据的坐标（用于寻路的阻挡判定）。</summary>
        public List<HexCoord> Occupied = new List<HexCoord>();
    }

    /// <summary>元素档位。衰减方向：Strong → Waning → Inert（**在 ROUND_END 衰减**）。</summary>
    public enum ElementLevel
    {
        Inert = 0,
        Waning = 1,
        Strong = 2,
    }

    /// <summary>
    /// 元素盘（六种元素）。
    ///
    /// ★ 时序（2026-10-28 修正，此前写成"灌注在 ROUND_END"，是**错的**）：
    ///   · **灌注**发生在 **TURN_ENDED**（单位回合结束），不是轮末。
    ///     怪物组的特例：组内**最后一个**怪物的 TURN_ENDED 才灌注。
    ///     若放在 ROUND_END，A 单位回合末灌注的元素，同轮后续的 B 单位本可消耗却消耗不到，
    ///     与本体行为不一致。
    ///   · **衰减**发生在 **ROUND_END**：Strong → Waning → Inert。
    ///   · 能力结算中途**不得**修改元素表。
    /// </summary>
    public sealed class ElementBoard
    {
        /// <summary>顺序固定：Fire, Ice, Air, Earth, Light, Dark。</summary>
        public ElementLevel[] Levels = new ElementLevel[6];

        /// <summary>
        /// 某个单位本回合是否已执行灌注指令（决定其 TURN_ENDED 时是否发 ELEMENT_INFUSED）。
        /// 由 EngineEvent 驱动，不在这里隐式判定。
        /// </summary>
        public Dictionary<string, bool> PendingInfusion = new Dictionary<string, bool>();
    }

    /// <summary>
    /// 行动单元 —— TurnOrder 的**元素类型**。
    ///
    /// ⚠ 关键：怪物组的多只怪物**共享一个先攻值、作为一个整体行动**。
    /// 因此 TurnOrder 的元素不是"单个怪物"，而是"行动单元"（1 个玩家角色 / 1 个怪物组）。
    /// 不这样定义，阶段 3 排序会出歧义，阶段 4 的事件序列也对不齐。
    /// </summary>
    public sealed class TurnUnit
    {
        public int Initiative;

        /// <summary>玩家角色为 1 个 Id；怪物组为组内所有怪物 Id。</summary>
        public List<string> ActorIds = new List<string>();

        /// <summary>是否为怪物组。</summary>
        public bool IsMonsterGroup;

        /// <summary>组内是否已全部行动完（用于判断"最后一个怪物"→ 触发灌注）。</summary>
        public bool AllActed;
    }

    /// <summary>一副攻击修正牌组的运行时状态。</summary>
    public sealed class ModifierDeckState
    {
        public string DeckName;

        /// <summary>剩余牌（按抽取顺序）。</summary>
        public List<string> Remaining = new List<string>();

        public List<string> Discarded = new List<string>();

        /// <summary>抽到 Shuffle 牌后置位，回合末/立即洗牌（时机待 hook 验证）。</summary>
        public bool ShufflePending;
    }

    /// <summary>一场战斗的完整状态。引擎不直接修改它，只能通过 apply(state, event) 折叠。</summary>
    public sealed class BattleState
    {
        public int Round;

        /// <summary>当前阶段。</summary>
        public RoundPhase Phase;

        /// <summary>当前行动单位在其序列中的下标（TURN_LOOP 用）。</summary>
        public int TurnIndex;

        /// <summary>本回合的行动顺序（先攻升序）。元素是**行动单元**，不是单个单位。</summary>
        public List<TurnUnit> TurnOrder = new List<TurnUnit>();

        public List<ActorState> Actors = new List<ActorState>();
        public List<MonsterState> Monsters = new List<MonsterState>();

        public MapState Map;
        public ElementBoard Elements = new ElementBoard();

        /// <summary>各副修正牌堆：key = 拥有者 Id 或 "Monster"。</summary>
        public Dictionary<string, ModifierDeckState> ModifierDecks =
            new Dictionary<string, ModifierDeckState>();

        /// <summary>快照（用于回放与对拍）。返回深拷贝。</summary>
        public BattleState Clone()
        {
            throw new System.NotImplementedException("阶段 3 实现");
        }
    }
}
