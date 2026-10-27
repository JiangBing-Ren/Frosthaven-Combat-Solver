using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FHEngine.Model
{
    /// <summary>
    /// 阶段 1 的规则数据模型 —— 与 Data/*.json **逐字段对应**。
    /// 由 tools/extract_rules.py 从明文 yml 提取生成，字段含义见 Data/manifest.json。
    ///
    /// 约定：
    ///   · 所有实体带 Source，可追溯到 (文件, 行号)。
    ///   · **不使用本地化文本**：统一用 Id / LocKey 作标识（项目约束：不复制文本资产）。
    ///   · netstandard2.0 纯 POCO，无 IO、无第三方依赖。
    /// </summary>

    /// <summary>来源追溯（文件相对路径 + 关键字段行号）。</summary>
    [DataContract]
    public sealed class SourceRef
    {
        [DataMember]
        public string file;
        [DataMember]
        public int line;
        [DataMember]
        public int line_initiative;
        [DataMember]
        public int line_math;
        [DataMember]
        public int line_abilitycards;
    }

    // ---------- 角色能力卡 ----------

    /// <summary>
    /// 单个能力节点（Attack / Move / Heal / Summon / …）。
    ///
    /// ⚠ **字符串字段的编码约定**（务必读）：
    ///   本体 yml 里 Summon / Conditions / AreaEffect / Filter / Targeting 等字段是
    ///   **异构类型**——有时标量、有时数组、有时嵌套 dict。
    ///   提取层（tools/extract_rules.py::flat()）统一编码为字符串：
    ///       标量 → str
    ///       数组 → 各元素用 "|" 连接      ← 例：Filter "Ally|Self"
    ///       dict → JSON 文本
    ///       None → ""
    ///   反向解析用 value.Split('|')，但**字符串本身含 "|" 时无法还原**，
    ///   所以**结构化查询请走 Abilities 递归树**（SubAbilities / Nested），
    ///   不要依赖对字符串字段的反向解析。
    ///   详见 Data/manifest.json 的 flat_convention。
    /// </summary>
    [DataContract]
    public sealed class Ability
    {
        /// <summary>该能力涉及的动作类型名（Move/Attack/Heal/Summon/…），从 yml 键直接取。</summary>
        public List<string> Types = new List<string>();

        [DataMember]
        public int? AttackAmount;    // Attack 可能是 {Amount:N} 或标量 → 统一到这里
        [DataMember]
        public int? Move;
        [DataMember]
        public int? Heal;
        [DataMember]
        public int? Damage;
        [DataMember]
        public int? Range;
        [DataMember]
        public int? Target;
        [DataMember]
        public int? Shield;
        [DataMember]
        public int? Retaliate;
        [DataMember]
        public string Summon;
        [DataMember]
        public string Conditions;
        [DataMember]
        public string AreaEffect;
        [DataMember]
        public string AreaEffectLayoutOverride;
        [DataMember]
        public string Filter;
        [DataMember]
        public string Targeting;
        [DataMember]
        public string MultiPassAttack;
        [DataMember]
        public string AnimOverload;

        /// <summary>子能力（SubAbilities）递归。</summary>
        [DataMember]
        public Dictionary<string, Ability> SubAbilities;

        /// <summary>
        /// 嵌套能力组 —— 例如 <c>ControlActor: { Abilities: {...} }</c>。
        /// 这类嵌套不挂在 SubAbilities 键下，而是藏在功能键内部，
        /// 阶段 3 需要它来判断"被控单位会做什么"。
        /// </summary>
        [DataMember]
        public Dictionary<string, Dictionary<string, Ability>> Nested;
    }

    /// <summary>一张卡的一个半边（Top / Bottom）。</summary>
    [DataContract]
    public sealed class CardHalf
    {
        public Dictionary<string, Ability> Abilities = new Dictionary<string, Ability>();
        [DataMember]
        public int XP;
        [DataMember]
        public string ActiveBonus;
    }

    /// <summary>角色能力卡。</summary>
    [DataContract]
    public sealed class AbilityCard
    {
        [DataMember]
        public SourceRef _source;
        [DataMember]
        public int Id;
        [DataMember]
        public string LocKey;
        [DataMember]
        public string Character;
        [DataMember]
        public int Level;
        [DataMember]
        public int Initiative;

        /// <summary>[上半去向, 下半去向] —— Discard（弃）或 Lost（永久失去）。</summary>
        public List<string> Discard = new List<string>();

        [DataMember]
        public CardHalf Top;
        [DataMember]
        public CardHalf Bottom;

        public override string ToString() => $"Card#{Id} {LocKey} ini={Initiative}";
    }

    // ---------- 怪物 ----------

    /// <summary>怪物某一等级的基础属性。</summary>
    [DataContract]
    public sealed class MonsterLevelStats
    {
        [DataMember]
        public int Health;
        [DataMember]
        public int Move;
        [DataMember]
        public int Attack;
        [DataMember]
        public int Range;
        [DataMember]
        public int Target;
        public List<string> Conditions = new List<string>();
    }

    /// <summary>怪物（普通 / 精英）。</summary>
    [DataContract]
    public sealed class Monster
    {
        [DataMember]
        public SourceRef _source;
        [DataMember]
        public string Id;
        [DataMember]
        public string LocKey;
        [DataMember]
        public string MonsterType;
        [DataMember]
        public int StandeeLimit;
        [DataMember]
        public bool PredominantlyMelee;
        [DataMember]
        public string NonEliteVariant;

        /// <summary>引用的行动卡 ID（卡是共享的，见 MonsterCard.UsedBy）。</summary>
        public List<int> AbilityCards = new List<int>();

        /// <summary>level(0-7) → 属性表。</summary>
        public Dictionary<string, MonsterLevelStats> Levels = new Dictionary<string, MonsterLevelStats>();

        public override string ToString() => $"Monster {Id}";
    }

    // ---------- 怪物行动卡 ----------

    /// <summary>哨兵值条目：yml 原值（可能是 -1/-2/99）与其路径。</summary>
    [DataContract]
    public sealed class RawValueEntry
    {
        [DataMember]
        public string path;
        [DataMember]
        public string field;
        [DataMember]
        public int? raw;
    }

    /// <summary>怪物行动卡。</summary>
    [DataContract]
    public sealed class MonsterCard
    {
        [DataMember]
        public SourceRef _source;
        [DataMember]
        public int Id;
        [DataMember]
        public string LocKey;
        [DataMember]
        public int Initiative;
        [DataMember]
        public bool Shuffle;

        /// <summary>子集内引用了这张卡的怪物 Id。</summary>
        public List<string> UsedBy = new List<string>();

        /// <summary>溯源用：yml 原值（含哨兵）。下游**不要**直接用。</summary>
        public List<RawValueEntry> RawValues = new List<RawValueEntry>();

        /// <summary>
        /// 下游**只用这个**：{ monsterId → { level → { 字段路径 → 最终值 } } }。
        /// 已代入怪物属性（哨兵 -1/-2 解析完成）。
        /// </summary>
        [DataMember]
        public Dictionary<string, Dictionary<string, Dictionary<string, int>>> ResolvedValues =
            new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

        /// <summary>
        /// 能力树 —— 与 <see cref="CardHalf.Abilities"/> **同构**的递归结构
        /// { 能力名 → Ability 节点 }。
        ///
        /// ⚠ 阶段 3 的怪物 AI 纯函数依赖这里的**结构信息**（有没有 Move 节点、
        /// 有没有 Attack 节点、ControlActor 嵌套了什么），因此绝不能扁平化。
        /// 数值的取值请用 ResolvedValues；这里只用于判断"卡面写了什么"。
        /// </summary>
        [DataMember]
        public Dictionary<string, Ability> Abilities;

        public override string ToString() => $"MonsterCard#{Id} {LocKey} ini={Initiative}";
    }

    // ---------- 攻击修正牌 ----------

    /// <summary>攻击修正牌定义（AMC_*）。</summary>
    [DataContract]
    public sealed class AttackModifier
    {
        [DataMember]
        public SourceRef _source;
        [DataMember]
        public string Name;
        /// <summary>数学修正，如 "+1" / "-2" / "*0" / "*2"。</summary>
        [DataMember]
        public string MathModifier;
        [DataMember]
        public bool Rolling;
        [DataMember]
        public bool Shuffle;
        [DataMember]
        public bool IsCurse;
        [DataMember]
        public bool IsBless;

        public override string ToString() => $"{Name} {MathModifier}";
    }

    /// <summary>牌组里的一条目：牌名 + 张数。</summary>
    [DataContract]
    public sealed class DeckEntry
    {
        [DataMember]
        public string Name;
        [DataMember]
        public int Count;
    }

    /// <summary>一副修正牌组。</summary>
    [DataContract]
    public sealed class ModifierDeck
    {
        [DataMember]
        public SourceRef _source;
        [DataMember]
        public string Name;
        [DataMember]
        public bool IsMonsterDeck;
        public List<DeckEntry> Cards = new List<DeckEntry>();
        [DataMember]
        public int TotalCards;

        public override string ToString() => $"Deck {Name} ({TotalCards})";
    }

    /// <summary>牌组归属映射：牌组名 → 引用的 AMC 列表。</summary>
    [DataContract]
    public sealed class DeckMembership
    {
        [DataMember]
        public bool IsMonsterDeck;
        public List<DeckEntry> Cards = new List<DeckEntry>();
        [DataMember]
        public int TotalCards;
        [DataMember]
        public string _source;
    }
}
