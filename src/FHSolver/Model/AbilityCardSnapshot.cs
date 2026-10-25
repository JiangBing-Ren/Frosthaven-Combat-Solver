namespace FHSolver.Model
{
    /// <summary>
    /// 一张手牌的只读快照。求解器枚举「出哪两张、哪张做 top」时消费这一层。
    /// </summary>
    public sealed class AbilityCardSnapshot
    {
        public int Id;
        public string Name;

        /// <summary>
        /// 本地化显示名（简体中文）。键就是 Name（见 AbilityCardUI.MakeAbilityCard:
        /// CardName = cardData.Name）。取不到本地化时退化为 Name。
        /// </summary>
        public string DisplayName;

        /// <summary>先攻值——玩家先攻同样写在牌上（CBaseAbilityCard.Initiative）。</summary>
        public int Initiative;

        /// <summary>Top 半边打出后的去向：Discard（弃掉）或 Lost（永久失去）。</summary>
        public string TopDiscard;

        /// <summary>Bottom 半边打出后的去向。</summary>
        public string BottomDiscard;

        /// <summary>Top 半边是否永久失去。</summary>
        public bool TopLost => TopDiscard == "Lost";

        /// <summary>Bottom 半边是否永久失去。</summary>
        public bool BottomLost => BottomDiscard == "Lost";

        public ActionSnapshot Top;
        public ActionSnapshot Bottom;

        /// <summary>
        /// 是否有"基础攻击 2"可用。任何一张牌都可以不当卡面能力用、改当通用基础攻击，
        /// 这是 GH/FH 的兜底动作 —— 尤其在一张牌的另一半是**常驻加成**（不能用）时，
        /// 这张牌唯一能做的事就只剩基础攻击 / 基础移动了。
        /// </summary>
        public bool HasDefaultAttack;

        /// <summary>是否有"基础移动 2"可用。</summary>
        public bool HasDefaultMove;

        /// <summary>
        /// 基础攻击的强度与射程。**不写死 2/1**，从 DefaultAttackAction 里读 ——
        /// 道具 / 增益（比如 +1 攻击的光环）会改这个数值，写死就会算错。
        /// </summary>
        public int DefaultAttackStrength = 2;
        public int DefaultAttackRange = 1;

        /// <summary>基础移动的格数，同样从 DefaultMoveAction 读。</summary>
        public int DefaultMoveStrength = 2;

        /// <summary>基础攻击的经验收益（正常是 0，仍然读出来以免有改动）。</summary>
        public int DefaultAttackXP;
        public int DefaultMoveXP;

        /// <summary>
        /// 通用「基础攻击 2」的动作快照（Bridge 层在读卡时填好）。
        /// 求解器枚举时把它当作这张卡的第三个候选动作。
        /// </summary>
        public ActionSnapshot DefaultAttack;

        /// <summary>通用「基础移动 2」的动作快照。</summary>
        public ActionSnapshot DefaultMove;

        /// <summary>Lost 是本回合最优最容易滥用的资源：永久失去该半边。</summary>
        public bool HasLost => TopLost || BottomLost;

        public override string ToString()
        {
            // 逐边标 Lost：整卡一个 [失] 看不出是哪一半，玩家没法判断代价
            string t = TopLost ? "上[失]" : "上";
            string b = BottomLost ? "下[失]" : "下";
            return $"{t}:{Top} {b}:{Bottom}";
        }
    }
}
