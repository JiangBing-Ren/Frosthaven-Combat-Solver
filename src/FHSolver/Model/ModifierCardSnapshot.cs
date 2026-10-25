namespace FHSolver.Model
{
    /// <summary>
    /// 一张攻击修正牌的只读快照。
    /// MathModifier 形如 "+1" / "-2" / "*2" / "*0" / "+0"，直接对应牌面。
    /// </summary>
    public sealed class ModifierCardSnapshot
    {
        public string Name;
        public string MathModifier;
        public bool Shuffle;
        public bool Rolling;
        public bool IsCurse;
        public bool IsBless;

        public override string ToString()
        {
            string s = MathModifier;
            if (Shuffle) s += "[洗]";
            if (Rolling) s += "[滚]";
            if (IsCurse) s += "[诅]";
            if (IsBless) s += "[祝]";
            return s;
        }
    }
}
