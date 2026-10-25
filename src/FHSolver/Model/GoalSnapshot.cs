using System.Collections.Generic;

namespace FHSolver.Model
{
    /// <summary>
    /// 地上可捡拾的东西。对应 `ScenarioManager.ObjectImportType`。
    /// 价值按用户定的优先级链换算：
    ///   使命目标 > 捡拾装备/宝箱 > 专长任务 > 巅峰战绩 > 经验值 > 战利品袋(材料)
    /// </summary>
    public enum LootSpotKind
    {
        /// <summary>宝箱（Chest / GoalChest）→ 优先级 2</summary>
        Chest,

        /// <summary>金币堆 / 战利品袋（MoneyToken）→ 优先级 6；但若下一张是装备则升到第 2 档</summary>
        MoneyToken,

        /// <summary>资源（Resource）→ 优先级 6</summary>
        Resource,

        /// <summary>任务物品（CarryableQuestItem）</summary>
        QuestItem,

        Unknown,
    }

    /// <summary>地上一处可以捡的东西。</summary>
    public sealed class LootSpot
    {
        public LootSpotKind Kind = LootSpotKind.Unknown;
        public string KindName = "";
        public int X;
        public int Y;

        /// <summary>
        /// 捡它的价值（已按优先级链换算成评分单位）。
        /// 战利品袋的价值取决于 `LootDeck` 下一张是不是装备 —— 见 MISSION_AND_LOOT.md §4.3。
        /// </summary>
        public int Value;

        public override string ToString()
        {
            return $"{KindName}@({X},{Y}) 价值{Value}";
        }
    }

    /// <summary>一个使命目标（或失败条件）的只读快照。</summary>
    public sealed class ObjectiveSnapshot
    {
        /// <summary>目标类型名（KillAllEnemies / LootX / ActorReachPosition … 共 22 种）。</summary>
        public string TypeName = "";

        /// <summary>true = 胜利目标；false = 失败条件。</summary>
        public bool IsWin;

        public bool IsComplete;
        public bool UnableToComplete;
        public bool IsActive;

        /// <summary>可选目标 —— 不完成也能通关。</summary>
        public bool IsOptional;

        /// <summary>单独完成即算通关（多个目标之间"任一即可"）。</summary>
        public bool EnoughToWinAlone;

        /// <summary>力竭仍算赢 → 可以放心耗尽手牌。</summary>
        public bool WinsDespiteExhaustion;

        public bool IsHidden;

        /// <summary>展示文本（本地化后的目标描述）。</summary>
        public string Text = "";

        public override string ToString()
        {
            var tags = new List<string>();
            if (IsWin) tags.Add("胜利");
            else tags.Add("失败条件");
            if (IsComplete) tags.Add("已完成");
            if (UnableToComplete) tags.Add("已无法完成");
            if (!IsActive) tags.Add("未生效");
            if (IsOptional) tags.Add("可选");
            if (EnoughToWinAlone) tags.Add("单独即可通关");
            if (WinsDespiteExhaustion) tags.Add("力竭仍赢");
            return $"[{string.Join("/", tags)}] {TypeName}"
                 + (string.IsNullOrEmpty(Text) ? "" : " — " + Text);
        }
    }

    /// <summary>
    /// 战役目标快照（P2）。
    ///
    /// 全部来自 `ScenarioState`，只读：
    ///   · `WinObjectives` / `LoseObjectives`
    ///   · `ChestProps` / `LootProps` / `ResourceProps`
    ///   · `LootDeck`（`List<byte>`，`deck[0]` = 下一次捡拾会抽到的牌）
    /// </summary>
    public sealed class GoalSnapshot
    {
        public List<ObjectiveSnapshot> Objectives = new List<ObjectiveSnapshot>();
        public List<LootSpot> LootSpots = new List<LootSpot>();

        /// <summary>下一次捡拾会拿到什么（已本地化）。</summary>
        public string NextLootName = "?";

        /// <summary>下一张是不是随机装备 —— 是的话战利品袋要按"装备"优先级（第 2 档）算。</summary>
        public bool NextLootIsItem;

        public bool HasLootDeck;

        /// <summary>有没有"力竭仍算赢"的目标（有就可以放心打光手牌）。</summary>
        public bool AnyWinsDespiteExhaustion;
    }
}
