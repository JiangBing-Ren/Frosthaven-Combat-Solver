using System;
using System.Collections.Generic;
using FHSolver.Model;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 读取攻击修正牌牌堆。
    /// 牌堆顺序 = Remaining[0..n]，翻牌时游戏直接取 [0]，因此这里读到的就是确定的翻牌序列。
    /// </summary>
    internal static class DeckReader
    {
        private static ModifierCardSnapshot ToSnap(AttackModifierYMLData c)
        {
            if (c == null) return null;
            try
            {
                return new ModifierCardSnapshot
                {
                    Name = c.Name,
                    MathModifier = c.MathModifier,
                    Shuffle = c.Shuffle,
                    Rolling = c.Rolling,
                    IsCurse = c.IsCurse,
                    IsBless = c.IsBless
                };
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 修正牌读取失败: {e.Message}");
                return null;
            }
        }

        private static void CopyCards(List<AttackModifierYMLData> from, List<ModifierCardSnapshot> to)
        {
            if (from == null) return;
            foreach (AttackModifierYMLData c in from)
            {
                ModifierCardSnapshot s = ToSnap(c);
                if (s != null) to.Add(s);
            }
        }

        /// <summary>读玩家（或英雄召唤物）的角色修正牌堆。</summary>
        public static ModifierDeckSnapshot ReadPlayerDeck(CCharacterClass cls, string label)
        {
            var snap = new ModifierDeckSnapshot { OwnerLabel = label };
            if (cls == null) return snap;
            try
            {
                CopyCards(cls.AttackModifierCards, snap.Remaining);
                CopyCards(cls.DiscardedAttackModifierCards, snap.Discarded);
                snap.ShufflePending = cls.m_ShuffleAttackModifierCards;
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"[FHSolver] 读取玩家牌堆[{label}] 失败: {e.Message}");
            }
            return snap;
        }

        /// <summary>读怪物共用牌堆。</summary>
        public static ModifierDeckSnapshot ReadMonsterDeck(CMonsterAttackModifierDeck deck, string label)
        {
            var snap = new ModifierDeckSnapshot { OwnerLabel = label };
            if (deck == null) return snap;
            try
            {
                CopyCards(deck.AttackModifierCards, snap.Remaining);
                CopyCards(deck.DiscardedAttackModifierCards, snap.Discarded);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"[FHSolver] 读取怪物牌堆[{label}] 失败: {e.Message}");
            }
            return snap;
        }

        /// <summary>读四个怪物牌堆（敌方 / 友方 / 敌方2 / 中立）。</summary>
        public static List<ModifierDeckSnapshot> ReadAllMonsterDecks()
        {
            var list = new List<ModifierDeckSnapshot>();
            try
            {
                list.Add(ReadMonsterDeck(MonsterClassManager.EnemyMonsterAttackModifierDeck, "怪物-敌"));
                list.Add(ReadMonsterDeck(MonsterClassManager.AlliedMonsterAttackModifierDeck, "怪物-友"));
                list.Add(ReadMonsterDeck(MonsterClassManager.Enemy2MonsterAttackModifierDeck, "怪物-敌2"));
                list.Add(ReadMonsterDeck(MonsterClassManager.NeutralMonsterAttackModifierDeck, "怪物-中立"));
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"[FHSolver] 读取怪物牌堆集合失败: {e.Message}");
            }
            return list;
        }
    }
}
