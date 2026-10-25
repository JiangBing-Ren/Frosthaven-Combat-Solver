namespace FHSolver.Model
{
    /// <summary>
    /// 敌人回合对我造成的一次打击（P5 真实抽牌后的结果）。
    ///
    /// 与上一版"按意图估算"的关键差别就在这个结构里：
    /// <see cref="Damage"/> 是**代入真实修正牌之后**的数值，
    /// <see cref="CardsText"/> 会把他翻出的牌原样写出来（"+1"、"×0"…），
    /// 玩家可以直接拿去跟游戏里实际翻出的牌对照验证。
    /// </summary>
    public sealed class EnemyStrike
    {
        public string AttackerName;

        /// <summary>它的先攻（用来判断它在你之前还是之后动手）。</summary>
        public int Initiative;

        /// <summary>实际扣掉的血（已扣护盾、已截断到 ≥0）。</summary>
        public int Damage;

        /// <summary>它这次翻出的修正牌，如 "+1" / "×0" / "+0 +1"（Rolling 会连翻多张）。</summary>
        public string CardsText;

        /// <summary>它这一回合是不是**移动过**才打到你（寻路结果）。</summary>
        public bool Moved;

        /// <summary>
        /// 只是**逼近**、这回合打不到你（真实 AI：够不着也会尽量靠近焦点）。
        /// 为 true 时 <see cref="Damage"/> 恒为 0 —— 它没出手，也没抽修正牌。
        /// </summary>
        public bool ApproachOnly;

        /// <summary>它出手时站的位置（移动后的落点）。</summary>
        public int FromX, FromY;

        /// <summary>
        /// 是否为退化估算。牌堆见底会触发洗牌（顺序不可知），
        /// 或压根没读到牌堆数据时为 true —— 这时 <see cref="Damage"/> 只是意图上的原始攻击力。
        /// </summary>
        public bool Approx;

        public override string ToString()
        {
            string mv = Moved ? $"（走到({FromX},{FromY})后）" : "（原地）";
            if (ApproachOnly)
            {
                return $"「{AttackerName}」先攻{Initiative} 够不着你 → 逼近到({FromX},{FromY})，"
                     + "这回合不会出手（下回合小心）";
            }
            string flag = Approx ? " [估算]" : "";
            return $"「{AttackerName}」先攻{Initiative}{mv} 翻 {CardsText}"
                 + $" → 对你 {Damage} 点{flag}";
        }
    }
}
