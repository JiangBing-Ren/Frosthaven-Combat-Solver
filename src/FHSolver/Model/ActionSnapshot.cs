using System.Collections.Generic;

namespace FHSolver.Model
{
    /// <summary>
    /// 卡某一半（Top / Bottom）到底是"本回合能做的一个动作"，还是"一张挂上去的常驻加成"。
    ///
    /// 这是 P1 能不能给出可执行方案的前提。修这个之前，求解器把死行「深渊力量」的下半
    /// （ActiveBonus: BuffMove + BuffSingleAttack，Duration: Persistent）当成了
    /// 「移动 2 / 攻击 2」，于是推荐出「用下半移动 2 去站位」这种游戏里根本不存在的操作。
    /// 常驻加成是打出这张牌时自动挂上的增益，**不能当成动作去执行**；
    /// 真想用这张牌做事，只能走基础攻击 2 / 基础移动 2。
    /// </summary>
    public enum HalfKind
    {
        /// <summary>空半边。</summary>
        Empty,

        /// <summary>本回合执行的一次性动作（移动 / 攻击 / 治疗 / 护盾 …）。</summary>
        Action,

        /// <summary>常驻加成（ActiveBonus，Duration != NA）。不是动作。</summary>
        PersistentBonus,
    }

    /// <summary>
    /// 一张行动卡某个半边（Top / Bottom）的只读摘要。
    /// 只保留求解器枚举动作所需的数值，不持有任何游戏类型。
    /// </summary>
    public sealed class ActionSnapshot
    {
        /// <summary>"上" / "下"（中文侧直接用；英文常量见 SideTop / SideBottom）。</summary>
        public string Side;

        /// <summary>这一半的语义。决定它能不能作为本回合的动作被枚举。</summary>
        public HalfKind Kind = HalfKind.Empty;

        /// <summary>是否是可以打出来的动作（不是常驻加成、不是空）。</summary>
        public bool IsAction => Kind == HalfKind.Action;

        /// <summary>是否是常驻加成（不能当动作执行）。</summary>
        public bool IsPersistentBonus => Kind == HalfKind.PersistentBonus;

        // ---- 一次性动作的内容 ----

        /// <summary>是否含移动能力。</summary>
        public bool HasMove;
        public int MoveStrength;
        // ---------- 阴影机制（死行 / Deathwalker）----------
        // ⚠ 阴影**不是**光照阴影，是死行职业机制叠在地块上的特殊道具 CObjectShadow。
        //   日志实测：死行手牌里 MoveShadow / CreateShadow / PerformFromShadow 出现 560+ 次
        //   全部被当成"未建模" → 死行方案几乎不可用。所以这四类必须单独识别。

        /// <summary>造阴影（CAbilityCreateShadow）。Strength = 造几个。</summary>
        public bool HasCreateShadow;
        public int CreateShadowCount;

        /// <summary>移动阴影（CAbilityMoveShadow）：挪动已有阴影，自己不动。</summary>
        public bool HasMoveShadow;
        public int MoveShadowCount;

        /// <summary>移除阴影（CAbilityRemoveShadow）。</summary>
        public bool HasRemoveShadow;
        public int RemoveShadowCount;

        /// <summary>
        /// 从阴影中发动（CAbilityPerformFromShadow）。
        /// 前提：所在格或相邻格要有阴影（m_ShadowsToUse 个）。
        /// </summary>
        public bool HasPerformFromShadow;
        public int ShadowsToUse;

        /// <summary>从阴影发动后是否消耗掉阴影（m_RemoveWhenDone）。</summary>
        public bool ShadowRemoveWhenDone;

        /// <summary>这一半涉及阴影操作（上面任意一个为 true）。</summary>
        public bool IsShadowAction => HasCreateShadow || HasMoveShadow
                                   || HasRemoveShadow || HasPerformFromShadow;


        /// <summary>
        /// 移动落脚限制（`CAbilityMove.MoveRestrictionType` 的名字）。
        ///
        /// 常见值：None / StraightLineOnly / MustEndNextToObstacle /
        ///        MustEndOnShadow / MustEndOnSpecificTiles / MustEndNextToEnemy
        ///
        /// ⚠ 死行「暗影步法」上半就是 `MustEndOnShadow`：
        ///   它可以"先移动 3 再攻击 3"，但**移动必须停在阴影格上**。
        ///   不认这条就会推荐出根本站不住的落点。
        /// </summary>
        public string MoveRestriction = "None";

        /// <summary>
        /// 这一击是否**忽略视线**（`MiscAbilityData.IgnoreLOS`）。
        ///
        /// 反编译实证（SharedAbilityTargeting.GetValidActorsInRange）：
        /// ```csharp
        /// GameState.GetTilesInRange(..., ignoreLOS: ability.MiscAbilityData?.IgnoreLOS ?? false, ...)
        /// ```
        /// 即**默认要视线**，只有个别能力显式关掉。
        /// 社区攻略说的"非目标型攻击不用视线"指的就是这类卡
        /// （如 Infuser 的 Power Spike："not targeted ... not in line of sight"）。
        /// 不建模就会漏掉墙后的合法目标 —— 低估这类卡。
        /// </summary>
        public bool IgnoreLOS;

        /// <summary>
        /// 这一击是否能打**不可见**目标（`MiscAbilityData.CanTargetInvisible`）。
        /// 反编译实证（CAbilityFilter.IsValidTarget）：
        ///   target.Tokens.HasKey(Invisible) &amp;&amp; flag != true → 不是合法目标
        /// 即**默认不能打隐形的**，只有个别能力显式允许。
        /// </summary>
        public bool CanTargetInvisible;

        /// <summary>这一半的移动是否带落脚限制。</summary>
        public bool IsMoveRestricted => HasMove && MoveRestriction != "None";

        /// <summary>
        /// 这个落脚限制我们能不能建模。
        /// 只有 `None` 和 `MustEndOnShadow`（停在阴影格）能算 ——
        /// 阴影格是看得见的，其余（只能直线走 / 必须停在障碍旁 / 必须停在特定格）
        /// 都建模不了，一律退化成"原地不动"，宁可保守也不给错的落点。
        /// </summary>
        public bool IsMoveRestrictionModeled =>
            MoveRestriction == "None" || MoveRestriction == "MustEndOnShadow";

        // ---------- 治疗（Heal）----------
        // 日志实测：Heal 是最高频未建模项（401 次），漂泊者/双子/塑骨师等
        // 支援职业的核心。不建模等于这些职业的方案收益全算错。

        // ---------- 附加效果（识别 + 提示，不精确计算）----------
        // 这些不改变本回合的位移/伤害模型，但会影响后续回合，
        // 所以必须\"说出来\"，不能让玩家以为推荐就是全部效果。

        /// <summary>给目标上中毒（CAbilityPoison：回合末持续掉血）。</summary>
        public bool HasPoison;
        public int PoisonStrength;

        /// <summary>拉拽敌人（CAbilityPull）。</summary>
        public bool HasPull;
        public int PullStrength;

        /// <summary>控制/指挥单位（ControlActor）。</summary>
        public bool HasControlActor;

        /// <summary>传送（CAbilityTeleport2）。</summary>
        public bool HasTeleport;

        /// <summary>
        public bool HasExtraEffect => HasPoison || HasPull || HasControlActor || HasTeleport;

        // ---------- 这一击会**施加**给目标的条件 ----------
        // 反编译实证：CAbility.NegativeConditions / PositiveConditions 是
        //   Dictionary<CCondition.ENegativeCondition, CAbility> ——
        //   即"这个能力施加的条件"，不是它自己身上的条件。
        //
        // 为什么重要：敌人（精英冰角祭司）先手给玩家上**缴械**，
        // 玩家这一回合就不能攻击了 —— 方案必须看到这一点，
        // 否则会推荐出"其实执行不了"的废招。

        /// <summary>这一击会施加的负面条件（Disarm / Poison / Immobilize / Stun …）。</summary>
        public List<string> AppliesNegative = new List<string>();

        /// <summary>这一击会施加的正向条件（Strengthen / Bless / Ward …）。</summary>
        public List<string> AppliesPositive = new List<string>();

        /// <summary>
        /// 这一半需要/消耗的**元素**（如 \"Fire\" / \"Fire+Water\"）。空 = 不需要。
        /// 反编译：CAbilityInfuse.ElementsToInfuse（基类 CAbility 上没有这个属性）。
        /// 元素是跨回合资源（注入/消耗/旺盛衰弱），本版本只提示、不推演。
        /// </summary>
        public string ElementNeed = "";

        /// <summary>是否含治疗能力。</summary>
        public bool HasHeal;
        public int HealStrength;
        public int HealRange;

        /// <summary>一次治疗几个目标（0 = 由形状/范围决定）。</summary>
        public int HealTargets;

        /// <summary>是否含攻击能力。</summary>
        public bool HasAttack;
        public int AttackStrength;
        public int AttackRange;
        public int AttackTargets;

        /// <summary>
        /// 强度/距离会随场上条件变化（YAML 的 `StatIsBasedOnX`）。
        /// 典型：死行「深渊力量」上半 = `Attack: 1 + 阴影数量`，
        /// 满 5 个阴影时实际是**攻击 6**，但卡面只写 1。
        /// </summary>
        public bool StrengthIsVariable;

        /// <summary>卡面印的原始强度（未代入场上条件）。</summary>
        public int AttackStrengthBase;

        /// <summary>
        /// 代入场上条件后的**实际**强度。
        /// 死行「深渊力量」= 1 + 阴影数（上限 5 → 最高 6）。
        /// 求不出来时退回卡面值。
        /// </summary>
        public int AttackStrengthActual;

        /// <summary>
        /// 这一半的攻击是**范围攻击**（带 AreaEffect）。
        /// 范围攻击要先选焦点格、按固定形状铺开，站位受形状约束；
        /// 也因为一次能打好几个，单体伤害模型算不准 —— P1 明确不建模，只提示。
        /// </summary>
        public bool HasAreaEffect;

        /// <summary>
        /// 范围形状的原样字符串（如 "0,0,G|1,1,R|2,0,R|3,1,B"），
        /// 形状字母（Frosthaven 官方，旗矛「阵型」卡的关键）：
        ///   **G = 灰格，你自己必须站这儿**
        ///   **B = 蓝格，每个蓝格都必须有你的盟友站着，否则这个能力根本发动不了**
        ///   **R = 红格，敌人所在格；不必填满，只要至少有 1 个有效目标即可**
        /// 原样给玩家看，让他自己摆站位。注意 G/B/R 是"占位要求"，不是伤害数字。
        /// </summary>
        public string AreaEffectShape = "";

        // ---- 常驻加成的内容（仅展示，不进评分）----

        /// <summary>常驻加成的 Behaviour 名（BuffMove / BuffSingleAttack / BuffAttack / …）。</summary>
        public List<string> PersistentBehaviours = new List<string>();

        /// <summary>常驻加成的人话说明，如「移动 +2」「攻击 +2」。</summary>
        public List<string> BonusTexts = new List<string>();

        // ---- 基础动作 ----

        /// <summary>这不是卡面能力，而是通用的「基础攻击 2」/「基础移动 2」。</summary>
        public bool IsDefaultAction;

        /// <summary>"基础攻击" / "基础移动"，空串表示这是卡面能力。</summary>
        public string DefaultLabel = "";

        // ---- 其余 ----

        /// <summary>该半边带的所有能力（类型名），用于识别治疗/护盾/元素注入等。</summary>
        public List<string> AbilityTypes = new List<string>();

        /// <summary>
        /// 求解器**没有建模**的能力类型名（治疗/护盾/元素注入/影子/传送/推拉/召唤…）。
        /// 不为空时必须告知玩家：这半边的实际效果比推荐里写的多，评分也没算它们。
        /// </summary>
        public List<string> UnmodeledTypes = new List<string>();

        /// <summary>该半边的经验值收益。</summary>
        public int XP;

        /// <summary>附带效果简述（元素注入、状态等），仅用于日志。</summary>
        public List<string> Extras = new List<string>();

        /// <summary>这一半在求解器眼里有没有"我们算得出来的内容"（移动或攻击）。</summary>
        public bool HasModeledContent => HasMove || HasAttack;

        /// <summary>这一半是不是单体攻击（可套伤害模型）。</summary>
        public bool IsSingleTargetAttack => HasAttack && !HasAreaEffect && AttackTargets == 1;

        /// <summary>
        /// 这一半我们没算准：是多目标或范围攻击，伤害没进评分。
        /// 必须显式提示，否则玩家会以为推荐里写的就是全部效果。
        /// </summary>
        /// <summary>
        /// 这一击是不是"单体伤害模型算不准"的。
        ///
        /// v1.22 起：**真 AOE 不再算不支持** —— 按射程取多个目标、
        /// 每个目标独立抽修正牌（`ModifierDraw.Simulate` 的 offset 递进），
        /// 伤害可以算准。
        ///
        /// 只剩**阵型单体**（有形状但只打 1 个）算不支持 ——
        /// 它的形状是**站位要求**不是伤害范围，按射程取目标会取错。
        /// </summary>
        public bool IsUnsupportedAttack => HasAttack && HasAreaEffect && AttackTargets == 1;

        public override string ToString()
        {
            var parts = new List<string>();

            if (IsDefaultAction)
            {
                parts.Add(DefaultLabel + (HasAttack ? AttackStrength : MoveStrength));
            }
            else
            {
                if (HasMove) parts.Add($"移动{MoveStrength}");
                if (HasAttack)
                {
                    parts.Add($"攻击{AttackStrength}(射程{AttackRange},目标{AttackTargets})");
                }
            }

            if (HasAreaEffect)
            {
                parts.Add("范围攻击" + (string.IsNullOrEmpty(AreaEffectShape)
                    ? "" : $"[{AreaEffectShape}]"));
            }

            foreach (string t in PersistentBehaviours) parts.Add("常驻:" + t);
            foreach (string t in BonusTexts) parts.Add(t);

            foreach (string t in AbilityTypes)
            {
                if (t != "Move" && t != "Attack") parts.Add(t);
            }

            if (parts.Count == 0) return "(无)";

            string s = string.Join(" ", parts);
            if (UnmodeledTypes.Count > 0)
            {
                s += $"  ※未建模:{string.Join("/", UnmodeledTypes)}";
            }
            return s;
        }
    }
}
