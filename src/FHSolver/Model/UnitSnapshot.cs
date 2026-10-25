using System.Collections.Generic;

namespace FHSolver.Model
{
    /// <summary>阵营。与 ScenarioRuleLibrary.CActor.EType 一一对应。</summary>
    public enum FactionKind
    {
        Player,
        Enemy,
        Enemy2,
        Ally,
        Neutral,
        HeroSummon,
        Unknown,
    }

    /// <summary>先攻从哪来的 —— 决定输出里要不要标"预测"。</summary>
    public enum InitiativeSource
    {
        /// <summary>游戏已经定好了。</summary>
        Game,

        /// <summary>怪物本回合行动牌还没选出，用 NextAbilityCard 顶上。</summary>
        Predicted,

        /// <summary>来自求解器给该队友的推荐方案（先打出的那张牌）。</summary>
        Plan,
    }

    /// <summary>
    /// 场上一个单位的只读快照。玩家、敌人、盟友、召唤物共用。
    /// </summary>
    public sealed class UnitSnapshot
    {
        public string Guid;
        public string Name;

        /// <summary>
        /// 本地化显示名（简体中文）。求解器的推荐文本一律用这个，不要用 Name。
        /// Name 保留内部标识（ClassId#StandeeID），只在调试行里出现。
        /// </summary>
        public string DisplayName;
        public string ClassId;

        /// <summary>阵营。敌人/敌人2 才是可攻击目标，盟友/中立/召唤物**绝不是**。</summary>
        public FactionKind Faction;

        /// <summary>中文阵营名，用于输出。</summary>
        public string FactionName;

        /// <summary>
        /// 是否对玩家敌对。判定走游戏自己的 CActor.AreActorsAllied(Player, 该单位类型)，
        /// 不自己猜 —— 实测 AllAliveMonsters 把 AllyMonsters / NeutralMonsters 也混进来了，
        /// 直接拿它当敌人列表会把盟友当成攻击目标。
        /// </summary>
        public bool IsHostileToPlayer;

        public bool IsPlayer;
        public int Health;
        public int MaxHealth;
        public int X;
        public int Y;
        public int Initiative;

        /// <summary>
        /// 预测先攻：怪物在本回合行动牌还没选出来时 Initiative() 返回 0，
        /// 这时用 NextAbilityCard.Initiative 顶上。
        /// </summary>
        public int InitiativePredicted;

        /// <summary>InitiativePredicted 是否是预测值（而非游戏已定值）。</summary>
        public bool InitiativeIsPredicted;

        /// <summary>先攻从哪来。输出时据此标注"预测"/"推荐方案"。</summary>
        public InitiativeSource InitiativeFrom = InitiativeSource.Game;

        /// <summary>当前护盾。</summary>
        public int Shield;

        /// <summary>
        /// 该单位当前带有的**负面条件**（Poison / Disarm / Immobilize / Stun …）。
        /// 与 `CCondition.ENegativeCondition` 一一对应。
        ///
        /// 为什么必须建模：敌人（如精英冰角祭司）先于我们出手时会上缴械 / 静足，
        /// 之后我们的方案若还推荐"攻击 / 移动"就是**废方案**。
        /// </summary>
        public List<string> NegativeConditions = new List<string>();

        /// <summary>该单位当前带有的**正面条件**（Strengthen / Ward / Regenerate …）。</summary>
        public List<string> PositiveConditions = new List<string>();

        /// <summary>是否处于某个负面条件下。</summary>
        public bool HasNegative(string cond)
        {
            return NegativeConditions != null && NegativeConditions.Contains(cond);
        }

        /// <summary>是否处于某个正面条件下。</summary>
        public bool HasPositive(string cond)
        {
            return PositiveConditions != null && PositiveConditions.Contains(cond);
        }

        /// <summary>
        /// 该单位用的修正牌堆在 <see cref="P1Context.MonsterDecks"/> 里的下标。
        /// 玩家单位恒为 -1（玩家每人**各自一副**牌堆）；怪物按阵营共用一副：
        ///   0=Enemy 1=Ally 2=Enemy2 3=Neutral。
        /// P5 敌人真实抽牌需要它 —— 同阵营敌人共用一副，必须按顺序依次消耗。
        /// </summary>
        public int DeckIndex = -1;

        /// <summary>
        /// 怪物本回合的行动意图（RoundAbilityCard 已选出的；否则取 NextAbilityCard 预测）。
        /// 玩家单位恒为 null。用来提示"它会先动、会先把距离改掉"。
        /// </summary>
        public ActionSnapshot Intent;

        /// <summary>
        /// 先攻更低（更先动）的单位预计已经对它造成的伤害。
        /// 快照是在出牌阶段拍的，先攻低的队友/敌人动手之后血量才会变 ——
        /// 不补这个就会拿"满血 10"去算一个其实已经被队友打过的目标。
        /// </summary>
        public int PredictedDamage;

        /// <summary>扣掉先动单位预计伤害后的有效血量。</summary>
        public int EffectiveHealth
        {
            get { int h = Health - PredictedDamage; return h < 0 ? 0 : h; }
        }

        // 仅玩家有值
        public int HandCardCount;
        public int SelectedCardCount;
        public List<string> HandCardIds = new List<string>();

        /// <summary>
        /// 阵营 → 怪物共享牌堆下标。与 DeckReader.ReadAllMonsterDecks() 的返回顺序一致。
        /// 玩家自己的召唤物（HeroSummon）不算怪物牌堆 → 返回 -1。
        /// </summary>
        public static int DeckIndexForFaction(FactionKind f)
        {
            switch (f)
            {
                case FactionKind.Enemy:   return 0;
                case FactionKind.Ally:    return 1;
                case FactionKind.Enemy2:  return 2;
                case FactionKind.Neutral: return 3;
                default:                  return -1;
            }
        }

        public override string ToString()
        {
            // 带短 Guid：StandeeID 不唯一（同一怪物类的多个 standee 会重号），
            // 唯一标识是 ActorGuid，M3 选目标必须以它为准。
            string g = string.IsNullOrEmpty(Guid) ? "?" : Guid.Substring(0, 8);
            string shown = string.IsNullOrEmpty(DisplayName) ? Name : DisplayName;
            return $"{shown}[{FactionName}] {Health}/{MaxHealth}"
                 + (PredictedDamage > 0 ? $"(-{PredictedDamage}→{EffectiveHealth})" : "")
                 + $" 先攻{Initiative} #{g}";
        }
    }
}
