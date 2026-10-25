using System.Collections.Generic;

namespace FHSolver.Model
{
    /// <summary>
    /// 战局快照。纯 POCO，不引用任何游戏类型 —— Solver 只吃这一层。
    /// </summary>
    public sealed class BattleSnapshot
    {
        public bool Valid;
        public string ScenarioId;
        public int RoundNumber;

        public List<UnitSnapshot> Players = new List<UnitSnapshot>();

        /// <summary>只含存活怪物（AllAliveMonsters）。</summary>
        public List<UnitSnapshot> Monsters = new List<UnitSnapshot>();

        /// <summary>已死亡但仍留在场景里的怪物数（AllMonsters - AllAliveMonsters）。</summary>
        public int DeadMonsterCount;

        /// <summary>每个玩家一副修正牌堆，按 Players 顺序对应。</summary>
        public List<ModifierDeckSnapshot> PlayerDecks = new List<ModifierDeckSnapshot>();

        /// <summary>怪物共用牌堆（敌方 / 友方 / 敌方2 / 中立）。</summary>
        public List<ModifierDeckSnapshot> MonsterDecks = new List<ModifierDeckSnapshot>();

        /// <summary>
        /// 各玩家的手牌详情，按 Players 顺序对应。
        /// P1 枚举「出哪两张、哪张做 top」的输入。
        /// </summary>
        public List<List<AbilityCardSnapshot>> PlayerHands = new List<List<AbilityCardSnapshot>>();

        /// <summary>求解器最关心的东西：当前行动者下一次攻击会翻出哪张牌。</summary>
        public List<ModifierCardSnapshot> NextCards(int count)
        {
            var result = new List<ModifierCardSnapshot>();
            foreach (ModifierDeckSnapshot d in PlayerDecks)
            {
                result.AddRange(d.Peek(count));
            }
            return result;
        }
    }
}
