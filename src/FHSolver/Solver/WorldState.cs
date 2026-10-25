using System;
using System.Collections.Generic;
using FHSolver.Model;

namespace FHSolver.Solver
{
    /// <summary>
    /// 多回合推演用的**可变世界状态**（P5）。
    ///
    /// P1 是在一个**只读快照**上穷举单回合；P5 需要把方案"执行"掉，
    /// 得到下一回合的世界状态 —— 所以这里需要一个可变的副本。
    ///
    /// ⚠ 只推演**玩家自己**的回合。敌人/队友回合用**简化模型**推进
    ///   （按各自意图估算），不是精确模拟 —— 这一点在窗口「多回合」页会明说。
    /// </summary>
    public sealed class WorldState
    {
        // ---- 我 ----
        public int MyX;
        public int MyY;
        public int MyHealth = 1;
        public int MyMaxHealth = 1;
        public int MyShield;

        // ---- 场上单位（敌人会掉血、会被移除）----
        public List<UnitSnapshot> Monsters = new List<UnitSnapshot>();
        public List<UnitSnapshot> Others = new List<UnitSnapshot>();

        // ---- 我的牌 ----
        public List<AbilityCardSnapshot> Hand = new List<AbilityCardSnapshot>();
        public ModifierDeckSnapshot Deck = new ModifierDeckSnapshot();

        /// <summary>
        /// 怪物的四副共享修正牌堆（P5 敌人真实抽牌）。
        /// 下标与 <see cref="UnitSnapshot.DeckIndex"/> 对应：0=敌 1=友 2=敌2 3=中立。
        /// ⚠ 必须**每帧 Clone 深拷贝**：同阵营敌人共用一副，
        ///   束搜索的不同分支各自推进，不能互相污染。
        /// </summary>
        public List<ModifierDeckSnapshot> EnemyDecks = new List<ModifierDeckSnapshot>();

        /// <summary>敌人本回合是否因牌堆见底而退化成估算（洗牌后顺序不可知）。</summary>
        public bool EnemyDeckExhausted;

        // ---- 地图查询（纯函数委托，从 P1Context 带进来）----
        // P5 敌人**寻路**要用：可达格 / 格间距离 / 视线。
        // 之所以走委托而不是直接引用游戏类型，是为了让 StateAdvancer 保持纯计算
        // （测试台直接编译 Solver 层，不碰 ScenarioRuleLibrary）。
        public Func<int, int, int, List<(int X, int Y, int Hazard)>> Reachable;
        public Func<int, int, int, int, int> RangeDistance;
        public Func<int, int, int, int, bool> HasLOS;

        // ---- 地上可捡 ----
        public List<LootSpot> LootSpots = new List<LootSpot>();

        // ---- 回合与累计 ----
        public int Round = 1;
        public int TotalDamage;      // 累计有效伤害
        public int TotalKills;
        public int TotalXp;
        public int GoalValue;        // 累计战役目标分
        public int HazardTaken;      // 累计吃到的危险地形伤害

        public bool Dead;
        public string KilledBy = "";

        /// <summary>深拷贝（推演分支时不能共享可变对象）。</summary>
        public WorldState Clone()
        {
            var c = new WorldState
            {
                MyX = MyX, MyY = MyY,
                MyHealth = MyHealth, MyMaxHealth = MyMaxHealth, MyShield = MyShield,
                Round = Round,
                TotalDamage = TotalDamage,
                TotalKills = TotalKills,
                TotalXp = TotalXp,
                GoalValue = GoalValue,
                HazardTaken = HazardTaken,
                Dead = Dead,
                KilledBy = KilledBy,
                EnemyDeckExhausted = EnemyDeckExhausted,
            };

            // 怪物牌堆深拷贝：同阵营敌人共用一副，分支间必须互不污染
            foreach (ModifierDeckSnapshot d in EnemyDecks)
            {
                c.EnemyDecks.Add(d == null ? null : new ModifierDeckSnapshot
                {
                    OwnerLabel = d.OwnerLabel,
                    ShufflePending = d.ShufflePending,
                    Remaining = new List<ModifierCardSnapshot>(d.Remaining),
                    Discarded = new List<ModifierCardSnapshot>(d.Discarded),
                });
            }

            foreach (UnitSnapshot u in Monsters) c.Monsters.Add(CloneUnit(u));
            foreach (UnitSnapshot u in Others) c.Others.Add(CloneUnit(u));

            // 手牌：AbilityCardSnapshot 本身只读，共享即可（只增不删）
            c.Hand = new List<AbilityCardSnapshot>(Hand);
            c.LootSpots = new List<LootSpot>(LootSpots);

            // 委托是不可变引用，直接共享即可
            c.Reachable = Reachable;
            c.RangeDistance = RangeDistance;
            c.HasLOS = HasLOS;

            c.Deck = new ModifierDeckSnapshot
            {
                OwnerLabel = Deck.OwnerLabel,
                ShufflePending = Deck.ShufflePending,
                Remaining = new List<ModifierCardSnapshot>(Deck.Remaining),
                Discarded = new List<ModifierCardSnapshot>(Deck.Discarded),
            };
            return c;
        }

        private static UnitSnapshot CloneUnit(UnitSnapshot u)
        {
            return new UnitSnapshot
            {
                Guid = u.Guid, Name = u.Name, DisplayName = u.DisplayName,
                ClassId = u.ClassId, Faction = u.Faction, FactionName = u.FactionName,
                IsHostileToPlayer = u.IsHostileToPlayer, IsPlayer = u.IsPlayer,
                Health = u.Health, MaxHealth = u.MaxHealth,
                X = u.X, Y = u.Y,
                Initiative = u.Initiative, InitiativePredicted = u.InitiativePredicted,
                InitiativeIsPredicted = u.InitiativeIsPredicted, InitiativeFrom = u.InitiativeFrom,
                Shield = u.Shield, Intent = u.Intent,
                DeckIndex = u.DeckIndex,
                PredictedDamage = u.PredictedDamage,
                HandCardCount = u.HandCardCount, SelectedCardCount = u.SelectedCardCount,
                HandCardIds = new List<string>(u.HandCardIds),
            };
        }

        /// <summary>按当前状态重建一个 P1 上下文（供下一层枚举用）。</summary>
        public P1Context ToContext(P1Context template, ScoringWeights weights)
        {
            var ctx = new P1Context
            {
                ActorName = template.ActorName,
                StartX = MyX,
                StartY = MyY,
                StartHealth = MyHealth,
                StartMaxHealth = MyMaxHealth,
                Hand = new List<AbilityCardSnapshot>(Hand),
                Deck = Deck,
                MonsterDecks = new List<ModifierDeckSnapshot>(EnemyDecks),
                LootSpots = new List<LootSpot>(LootSpots),
                Monsters = new List<UnitSnapshot>(),
                Others = new List<UnitSnapshot>(),
                Occupied = new HashSet<(int, int)>(),
                Adv = template.Adv,
                Reachable = Reachable ?? template.Reachable,
                RangeDistance = RangeDistance ?? template.RangeDistance,
                HasLOS = HasLOS ?? template.HasLOS,
                IsShadowTile = template.IsShadowTile,
                W = weights ?? template.W,
            };

            foreach (UnitSnapshot u in Monsters) ctx.Monsters.Add(u);
            foreach (UnitSnapshot u in Others) ctx.Others.Add(u);

            // 占用：我自己 + 所有单位 + 不可达格
            ctx.Occupied.Add((MyX, MyY));
            foreach (UnitSnapshot u in ctx.Monsters) ctx.Occupied.Add((u.X, u.Y));
            foreach (UnitSnapshot u in ctx.Others) ctx.Occupied.Add((u.X, u.Y));

            return ctx;
        }
    }
}
