using System;
using System.Collections.Generic;
using System.Reflection;
using FHSolver.Model;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 战役目标读取（P2）—— **只读**。
    ///
    /// 数据来源全部是 `ScenarioState` 上的公开属性，不需要反射：
    ///   · `WinObjectives` / `LoseObjectives` → 使命目标与失败条件
    ///   · `ChestProps` → 宝箱（Chest / GoalChest）
    ///   · `LootProps`   → 金币堆 / 战利品袋（MoneyToken）
    ///   · `ResourceProps` → 资源
    ///   · `LootDeck`（`List<byte>`）→ 下一次捡拾会拿到什么
    ///
    /// 战利品可预测性的关键（MISSION_AND_LOOT.md §4.1）：
    ///   `CLootDeck.DrawNext` 直接取 `deck[0]` 作为 `AllCards` 下标，
    ///   **抽完放回末尾**（循环使用）。所以求解器在捡拾前就知道会拿到什么。
    /// </summary>
    internal static class GoalReader
    {
        // ---- 战利品价值（按优先级链换算的评分单位）----
        // 使命目标 > 捡拾装备/宝箱 > 专长 > 战绩 > 经验 > 战利品袋(材料)
        private const int V_CHEST = 120;       // 宝箱 / 装备 → 第 2 档
        private const int V_LOOT_ITEM = 120;   // 袋子开出装备 → 也升到第 2 档
        private const int V_LOOT_MAT = 15;     // 袋子开出材料 → 第 6 档
        private const int V_RESOURCE = 15;     // 资源 → 第 6 档
        private const int V_QUEST = 100;       // 任务物品

        /// <summary>读取战役目标。</summary>
        public static GoalSnapshot Read()
        {
            var snap = new GoalSnapshot();
            try
            {
                ScenarioState state = ScenarioManager.CurrentScenarioState;
                if (state == null) return snap;

                ReadObjectives(state, snap);
                ReadLootSpots(state, snap);
                ReadNextLoot(state, snap);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取战役目标失败: {e.Message}");
            }
            return snap;
        }

        // ---- ① 使命目标 ----

        private static void ReadObjectives(ScenarioState state, GoalSnapshot snap)
        {
            try
            {
                foreach (CObjective o in state.WinObjectives ?? new List<CObjective>())
                {
                    if (o == null) continue;
                    ObjectiveSnapshot s = ToSnapshot(o, isWin: true);
                    snap.Objectives.Add(s);
                    if (s.WinsDespiteExhaustion) snap.AnyWinsDespiteExhaustion = true;
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取胜利目标失败: {e.Message}");
            }

            try
            {
                foreach (CObjective o in state.LoseObjectives ?? new List<CObjective>())
                {
                    if (o == null) continue;
                    snap.Objectives.Add(ToSnapshot(o, isWin: false));
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取失败条件失败: {e.Message}");
            }
        }

        private static ObjectiveSnapshot ToSnapshot(CObjective o, bool isWin)
        {
            var s = new ObjectiveSnapshot
            {
                IsWin = isWin,
                TypeName = o.ObjectiveType.ToString(),
                IsComplete = o.IsComplete,
                UnableToComplete = o.UnableToComplete,
                IsActive = o.IsActive,
                IsOptional = o.IsOptional,
                EnoughToWinAlone = o.EnoughToWinAlone,
                WinsDespiteExhaustion = o.WinsDespiteExhaustion,
                IsHidden = o.IsHidden,
            };

            // 目标描述：CustomLocKey 是本地化键
            try
            {
                if (!string.IsNullOrEmpty(o.CustomLocKey))
                {
                    s.Text = Loc.T(o.CustomLocKey);
                }
            }
            catch { /* 取不到就留空 */ }

            return s;
        }

        // ---- ② 地上的可捡拾物 ----

        private static void ReadLootSpots(ScenarioState state, GoalSnapshot snap)
        {
            // 宝箱（Chest / GoalChest）→ 第 2 档
            AddSpots(snap, SafeProps(state, "ChestProps"),
                ScenarioManager.ObjectImportType.Chest, V_CHEST, "宝箱");

            // 金币堆 / 战利品袋 → 价值取决于下一张是不是装备
            int lootValue = snap.NextLootIsItem ? V_LOOT_ITEM : V_LOOT_MAT;
            AddSpots(snap, SafeProps(state, "LootProps"),
                ScenarioManager.ObjectImportType.MoneyToken, lootValue, "战利品袋");

            // 资源
            AddSpots(snap, SafeProps(state, "ResourceProps"),
                ScenarioManager.ObjectImportType.Resource, V_RESOURCE, "资源");
        }

        private static void AddSpots(GoalSnapshot snap, List<CObjectProp> props,
            ScenarioManager.ObjectImportType type, int value, string name)
        {
            if (props == null) return;
            foreach (CObjectProp p in props)
            {
                if (p == null) continue;
                int x, y;
                if (!TryTile(p, out x, out y)) continue;

                LootSpotKind kind = KindOf(p.ObjectType);
                snap.LootSpots.Add(new LootSpot
                {
                    Kind = kind,
                    KindName = KindName(kind, name),
                    X = x,
                    Y = y,
                    Value = value,
                });
            }
        }

        private static LootSpotKind KindOf(ScenarioManager.ObjectImportType t)
        {
            switch (t)
            {
                case ScenarioManager.ObjectImportType.Chest:
                case ScenarioManager.ObjectImportType.GoalChest:
                    return LootSpotKind.Chest;
                case ScenarioManager.ObjectImportType.MoneyToken:
                    return LootSpotKind.MoneyToken;
                case ScenarioManager.ObjectImportType.Resource:
                    return LootSpotKind.Resource;
                default:
                    return LootSpotKind.Unknown;
            }
        }

        private static string KindName(LootSpotKind k, string fallback)
        {
            switch (k)
            {
                case LootSpotKind.Chest: return "宝箱";
                case LootSpotKind.MoneyToken: return "战利品袋";
                case LootSpotKind.Resource: return "资源";
                case LootSpotKind.QuestItem: return "任务物品";
                default: return string.IsNullOrEmpty(fallback) ? "物件" : fallback;
            }
        }

        private static bool TryTile(CObjectProp p, out int x, out int y)
        {
            x = -1;
            y = -1;
            try
            {
                CTile t = p.PropTile;
                if (t == null || t.m_ArrayIndex == null) return false;
                x = t.m_ArrayIndex.X;
                y = t.m_ArrayIndex.Y;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static List<CObjectProp> SafeProps(ScenarioState state, string propName)
        {
            try
            {
                PropertyInfo pi = typeof(ScenarioState).GetProperty(
                    propName, BindingFlags.Public | BindingFlags.Instance);
                if (pi == null) return null;
                return pi.GetValue(state, null) as List<CObjectProp>;
            }
            catch
            {
                return null;
            }
        }

        // ---- ③ 下一张战利品 ----

        private static void ReadNextLoot(ScenarioState state, GoalSnapshot snap)
        {
            try
            {
                List<byte> deck = state.LootDeck;
                if (deck == null || deck.Count == 0) return;

                snap.HasLootDeck = true;

                // deck[0] 是下一次捡拾会抽到的牌（下标到 CLootDeck.AllCards）；0 是分隔符
                byte idx = deck[0];
                if (idx == 0) return;

                LootCard? lc = PeekLootCard(idx);
                if (!lc.HasValue) return;

                snap.NextLootIsItem = lc.Value.Type == LootCard.EType.RandomItem;
                snap.NextLootName = snap.NextLootIsItem ? "随机装备" : lc.Value.Type.ToString();

                // 若下一张是装备，已加过的战利品袋要按装备的价值重算
                if (snap.NextLootIsItem)
                {
                    foreach (LootSpot s in snap.LootSpots)
                    {
                        if (s.Kind == LootSpotKind.MoneyToken) s.Value = V_LOOT_ITEM;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取战利品牌堆失败: {e.Message}");
            }
        }

        /// <summary>按下标取一张战利品卡（不改动牌堆）。</summary>
        private static LootCard? PeekLootCard(byte index)
        {
            try
            {
                // CLootDeck.AllCards 是静态**数组**（LootCard[]，见反编译 CLootDeck.cs:375）。
                // ⚠ v1.36 修正：旧代码 `as List<LootCard>` 强转恒为 null →
                //   PeekLootCard 永远 null →「下一袋是装备 = 120 分」整档从未生效。
                FieldInfo fi = typeof(CLootDeck).GetField("AllCards",
                    BindingFlags.Public | BindingFlags.Static);
                if (fi == null) return null;

                object raw = fi.GetValue(null);
                if (raw == null) return null;

                LootCard[] arr = raw as LootCard[];
                if (arr != null)
                {
                    if (index >= arr.Length) return null;
                    return arr[index];
                }

                // 兜底：万一以后改成 List/IList
                System.Collections.IList list = raw as System.Collections.IList;
                if (list != null && index < list.Count)
                {
                    return (LootCard)list[index];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
