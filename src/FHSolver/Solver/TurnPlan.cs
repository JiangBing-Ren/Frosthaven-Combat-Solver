using System.Collections.Generic;
using System.Linq;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>
    /// 这一步用的是「卡的哪一半」还是「通用基础动作」。
    /// 之前求解器只说"打上半/打下半"，玩家照着做时会跟卡面能力对不上 ——
    /// 尤其是半边是常驻加成时，根本不存在"执行这一半"这回事。
    /// </summary>
    public enum HalfSource
    {
        /// <summary>卡的上半能力。</summary>
        Top,

        /// <summary>卡的下半能力。</summary>
        Bottom,

        /// <summary>通用「基础攻击 2」。</summary>
        DefaultAttack,

        /// <summary>通用「基础移动 2」。</summary>
        DefaultMove,
    }

    /// <summary>一次攻击的具体选择。</summary>
    public sealed class AttackChoice
    {
        /// <summary>"上" / "下" / "基础攻击"（对应 HalfSource）。</summary>
        public string Side;

        /// <summary>这一步用的是卡的哪一半 / 还是通用基础动作。</summary>
        public HalfSource Source = HalfSource.Top;

        /// <summary>出的是手牌里的第几张（对应快照 PlayerHands 的下标）。</summary>
        public int CardIndex;

        /// <summary>牌的本地化显示名。</summary>
        public string CardName;

        /// <summary>牌的内部名（调试用）。</summary>
        public string CardInternalName;

        public int BaseStrength;
        public int Range;

        /// <summary>攻击发起位置（已经算上半边内的移动）。</summary>
        public int FromX;
        public int FromY;

        /// <summary>目标唯一标识（ActorGuid）—— 执行时以它为准，不要用 StandeeID。</summary>
        public string TargetGuid;

        /// <summary>目标的本地化显示名。</summary>
        public string TargetName;

        /// <summary>目标的内部名（调试用）。</summary>
        public string TargetInternalName;
        public int TargetX;
        public int TargetY;
        public int TargetHealth;

        /// <summary>目标的最大血量 —— 用于显示「当前/最大」。</summary>
        public int TargetMaxHealth;

        public int TargetShield;

        /// <summary>目标的快照原始血量（未扣先动单位预计伤害），用于展示。</summary>
        public int TargetRawHealth;

        /// <summary>本次攻击会翻出的牌（确定量，不是期望）。</summary>
        public List<string> Mods = new List<string>();

        /// <summary>预测伤害（已减护盾、已截断到 0）。</summary>
        public int Damage;

        /// <summary>是否命中致命一击。</summary>
        public bool Kills;

        /// <summary>优/劣势下二选一是近似的（游戏 tie-break 还看负面/附加效果）。</summary>
        public bool AdvantageApprox;

        /// <summary>还原出的表达式，与 Player.log 里 "Attack modifier Expression String" 同形，可直接对照。</summary>
        public string Expr;

        /// <summary>本次攻击会从牌堆顶拿走几张（含滚动态）。</summary>
        public int CardsConsumed;

        public string ToShortString()
        {
            string kill = Kills ? " [击杀]" : "";
            string approx = AdvantageApprox ? "(近似)" : "";
            return $"{Side}半 攻{BaseStrength} → {TargetName}"
                 + $" 伤{Damage}{kill} 翻牌[{string.Join(" ", Mods)}]{approx}";
        }
    }

    /// <summary>
    /// 方案里的「一步」：用哪张牌的哪一半（或基础动作），做完之后站哪。
    /// </summary>
    public sealed class PlanHalf
    {
        /// <summary>出了手牌里的第几张（基础动作也记，表示弃的是哪张）。</summary>
        public int CardIndex = -1;

        /// <summary>牌的本地化显示名。</summary>
        public string CardName = "?";

        /// <summary>牌的内部名（调试用）。</summary>
        public string CardInternalName = "?";

        /// <summary>用的是上半 / 下半 / 基础攻击 / 基础移动。</summary>
        public HalfSource Source = HalfSource.Top;

        /// <summary>归一化后的可执行动作内容。</summary>
        public ActionSnapshot Half;

        /// <summary>用这一半会不会让这张牌永久失去。</summary>
        public bool IsLost;

        /// <summary>这是通用基础动作（Attack2 / Move2），不是卡面能力。</summary>
        public bool IsDefaultAction => Source == HalfSource.DefaultAttack
                                    || Source == HalfSource.DefaultMove;

        /// <summary>这一半做完后站的位置。</summary>
        public int ToX = -1;
        public int ToY = -1;

        /// <summary>中文标签："上" / "下" / "基础攻击" / "基础移动"。</summary>
        public string SideLabel
        {
            get
            {
                switch (Source)
                {
                    case HalfSource.Top: return "上";
                    case HalfSource.Bottom: return "下";
                    case HalfSource.DefaultAttack: return "基础攻击";
                    case HalfSource.DefaultMove: return "基础移动";
                    default: return "?";
                }
            }
        }

        /// <summary>这一步在游戏里要怎么点。</summary>
        public string HowToPlay
        {
            get
            {
                switch (Source)
                {
                    case HalfSource.Top: return $"打出「{CardName}」的【上半】";
                    case HalfSource.Bottom: return $"打出「{CardName}」的【下半】";
                    case HalfSource.DefaultAttack:
                        return $"把「{CardName}」当【基础攻击 2】打";
                    case HalfSource.DefaultMove:
                        return $"把「{CardName}」当【基础移动 2】走";
                    default: return "?";
                }
            }
        }
    }

    /// <summary>一套完整的单回合出牌方案。</summary>
    public sealed class TurnPlan
    {
        /// <summary>先攻取 Top 那张牌的先攻值（GH/FH 规则：先攻看上面那半边）。</summary>
        public int Initiative;

        /// <summary>先执行的那一步。</summary>
        public PlanHalf First;

        /// <summary>后执行的那一步。</summary>
        public PlanHalf Second;

        /// <summary>牌的本地化显示名（保留，供日志/去重使用）。</summary>
        public string TopCardName;
        public string BottomCardName;

        /// <summary>牌的内部名（调试用）。</summary>
        public string TopCardInternalName;
        public string BottomCardInternalName;

        /// <summary>出的是手牌里的第几张（对应快照 PlayerHands 的下标）。</summary>
        public int TopCardIndex = -1;
        public int BottomCardIndex = -1;

        /// <summary>半边执行顺序：true = 先上后下。</summary>
        public bool TopFirst;

        /// <summary>建议最终站位（-1 表示不需要移动 / 无移动）。</summary>
        public int FinalX = -1;
        public int FinalY = -1;

        /// <summary>中途站位（先执行的那个半边移动之后的位置）。</summary>
        public int MidX = -1;
        public int MidY = -1;

        public List<AttackChoice> Attacks = new List<AttackChoice>();

        // ---- 评分各项（都记下来，便于人工判断评分是否合理）----
        public int TotalDamage;
        public int EffectiveDamage;   // 去掉溢出后的有效伤害
        public int KillCount;
        public int TotalXP;
        public int LostCost;          // 用掉的 Lost 半边数

        /// <summary>
        /// P2 战役目标价值：这一回合的落点顺手捡到了什么（宝箱 / 战利品袋 / 资源）。
        /// 0 = 没捡到东西。使命目标不自动计分，只做提示。
        /// </summary>
        /// <summary>
        /// 两张牌各自的先攻值。
        /// 游戏选牌界面**按先攻升序**排（越上面先攻越低），玩家靠点数快速定位卡牌，
        /// 所以展示方案时也要按先攻升序写这两张牌，并各自标出点数。
        /// </summary>
        public int TopCardInitiative;
        public int BottomCardInitiative;

        /// <summary>
        /// 取**某张牌**的先攻 —— 按 CardIndex 匹配。
        ///
        /// ⚠ 绝不要按执行顺序（First / Second）去取这两个字段！
        ///   它们是按**上下半位**存的：TopCardInitiative = 上半位那张牌，
        ///   BottomCardInitiative = 下半位那张牌。
        ///   而执行顺序与上下半位是**两个已解耦的维度**（v1.8），
        ///   用 First/Second 去索引就会把两张牌的先攻**对调** ——
        ///   实战里出现过：先执行下半位那张时，两张牌的先攻数字被互换。
        /// </summary>
        public int InitiativeOf(int cardIndex)
        {
            if (cardIndex == TopCardIndex) return TopCardInitiative;
            if (cardIndex == BottomCardIndex) return BottomCardInitiative;
            return Initiative;      // 兜底（不该发生）
        }

        public int GoalValue;

        /// <summary>
        /// 本方案会新造出几个阴影（死行）。已达上限 5 时为 0。
        /// </summary>
        // ---------- 多样性签名（学 StS2 的 SearchRouteTraits 泳道）----------
        // 只按"排序最前的 K 个"取 Top-N，会让前几名全是同一路子的变体
        // （同样的牌、同样的落点、同样打谁）—— 看起来就"不聪明"。
        // 用这几个签名做分桶，保证 Top-N 里覆盖不同的**打法**。

        /// <summary>进攻签名：这一轮打的是哪些目标。</summary>
        public string OffenseSignature = "";

        /// <summary>站位签名：最终站在哪。</summary>
        public string PositionSignature = "";

        /// <summary>效用签名：治疗/阴影/拾取这类非攻击收益。</summary>
        public string UtilitySignature = "";

        /// <summary>这个方案的全部签名（分桶用）。</summary>
        public string[] Signatures => new[] { OffenseSignature, PositionSignature, UtilitySignature };

        /// <summary>
        /// ★ 这回合打完我会不会**被打死**（生存维度的最高优先级）。
        ///
        /// 参考 StS2 Combat Solver 的词典序评估：第一项就是"能不能活"，
        /// 线性加权做不到这件事（"打 10 点但挨 8 点"未必比"打 3 点不挨打"差）。
        /// </summary>
        public bool WouldDie;

        /// <summary>
        /// 本方案这回合会被敌人上的**负面条件**（缴械/静足/眩晕…）。
        /// 空 = 没有。
        /// </summary>
        public List<string> IncomingConditions;

        /// <summary>
        /// 本方案这回合结束时会**挨打**的伤害（生存维度）。
        /// </summary>
        public int IncomingDamage;

        /// <summary>
        /// 本方案的治疗收益（实际奶回的血量，过量不计）。
        /// </summary>
        public int HealValue;

        public int ShadowGain;

        /// <summary>
        /// 本方案移动过程中会吃到的**危险地形伤害**总和。
        /// 危险地形是"进入时"扣血（站在上面不动不重复扣），所以按路径累加。
        /// </summary>
        public int SelfHazardDamage;

        /// <summary>
        /// 被"当基础动作用掉"的牌的浪费价值（0 = 没烧牌）。
        /// 只在同分时用于排序：优先烧掉最没用的那张牌，而不是把好牌当 Attack2 打。
        /// </summary>
        public int WastedValue;

        public double Score;

        /// <summary>为什么这个方案被扣分／不可信，写在这里而不是让人猜。</summary>
        public string Note = "";

        public string ToOneLine()
        {
            // 按先攻升序写两张牌（选牌界面就是越上面先攻越低，玩家靠点数定位）
            var cards = new List<(string label, int ini)>
            {
                (First != null ? $"{First.SideLabel}「{First.CardName}」" : "?", First.CardIndex >= 0 ? InitiativeOf(First.CardIndex) : TopCardInitiative),
                (Second != null ? $"{Second.SideLabel}「{Second.CardName}」" : "?", Second.CardIndex >= 0 ? InitiativeOf(Second.CardIndex) : BottomCardInitiative),
            }.OrderBy(c => c.ini).ToList();

            string a = cards[0].label;
            string b = cards[1].label;
            string goal = GoalValue > 0 ? $" 目标+{GoalValue}" : "";
            return $"{a} + {b} 先攻{Initiative}"
                 + $" | 伤{TotalDamage}(有效{EffectiveDamage}) 杀{KillCount}"
                 + $" XP{TotalXP} 失{LostCost}{goal} | 评分{Score:F1}";
        }
    }
}
