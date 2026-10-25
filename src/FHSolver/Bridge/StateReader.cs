using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FHSolver.Model;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge
{
    /// <summary>
    /// 只读读取战局状态 → POCO 快照。
    /// 这是全工程唯一接触游戏类型的地方，Solver 层不引用任何游戏类型。
    /// </summary>
    internal static class StateReader
    {
        public static BattleSnapshot Read()
        {
            var snap = new BattleSnapshot();
            try
            {
                if (!ScenarioManager.IsInTactical)
                {
                    return snap; // Valid=false，不在战斗中
                }

                ScenarioState state = ScenarioManager.CurrentScenarioState;
                CScenario scenario = ScenarioManager.Scenario;
                if (state == null || scenario == null)
                {
                    return snap;
                }

                snap.Valid = true;
                snap.ScenarioId = state.ID;
                snap.RoundNumber = state.RoundNumber;

                // 先 ToList() 拷一份再遍历，避免遍历途中被游戏改动导致异常。
                // 怪物必须取 AllAliveMonsters：AllMonsters 含 DeadEnemies/DeadAllyMonsters 等，
                // 会把已死单位（负血）一起带进来，污染目标选择。
                List<CPlayerActor> players = scenario.PlayerActors?.ToList() ?? new List<CPlayerActor>();
                List<CEnemyActor> monsters = scenario.AllAliveMonsters?.ToList() ?? new List<CEnemyActor>();
                int all = scenario.AllMonsters?.Count ?? monsters.Count;
                snap.DeadMonsterCount = all - monsters.Count;

                foreach (CPlayerActor p in players)
                {
                    if (p == null) continue;
                    UnitSnapshot u = ReadUnit(p, isPlayer: true);
                    CCharacterClass cls = null;
                    try { cls = p.CharacterClass; } catch { /* 忽略 */ }

                    if (cls != null)
                    {
                        try
                        {
                            u.HandCardCount = cls.HandAbilityCards?.Count ?? 0;
                            u.SelectedCardCount = cls.RoundAbilityCardsSelectedInCardSelection?.Count ?? 0;
                            var hand = cls.HandAbilityCards?.ToList();
                            if (hand != null)
                            {
                                foreach (CAbilityCard c in hand)
                                {
                                    // 注意：CBaseCard.ID 是 int，Name 才是字符串
                                    if (c != null) u.HandCardIds.Add(c.Name ?? c.ID.ToString());
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Plugin.Log?.LogWarning($"[FHSolver] 读取手牌失败: {e.Message}");
                        }

                        snap.PlayerDecks.Add(DeckReader.ReadPlayerDeck(cls, u.Name));
                    snap.PlayerHands.Add(HandReader.ReadHand(cls));

                    // （v0.5.0 曾在这里按最大移动力试算一次可达格数做通路自检；
                    //  P1 现在按具体出牌组合逐个算，这个自检已无意义，删。）
                    }
                    else
                    {
                        snap.PlayerDecks.Add(new ModifierDeckSnapshot { OwnerLabel = u.Name });
                    }

                    snap.Players.Add(u);
                }

                foreach (CEnemyActor m in monsters)
                {
                    if (m == null) continue;
                    snap.Monsters.Add(ReadUnit(m, isPlayer: false));
                }

                snap.MonsterDecks = DeckReader.ReadAllMonsterDecks();
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"[FHSolver] 读取战局快照失败: {e.Message}\n{e.StackTrace}");
                snap.Valid = false;
            }
            return snap;
        }

        internal static UnitSnapshot ReadUnit(CActor a, bool isPlayer)
        {
            var u = new UnitSnapshot { IsPlayer = isPlayer };
            try
            {
                u.Guid = a.ActorGuid;
                u.Health = a.Health;
                u.MaxHealth = a.MaxHealth;

                string classId = null;
                try { classId = a.Class?.ID; } catch { /* 忽略 */ }
                u.ClassId = classId ?? "?";

                // 坐标：AStar.Point 的 X/Y 字段
                try
                {
                    AStar.Point pos = a.ArrayIndex;
                    if (pos != null)
                    {
                        u.X = pos.X;
                        u.Y = pos.Y;
                    }
                }
                catch { /* 忽略 */ }

                // ---- 阵营：绝不混为一谈 ----
                // 曾用 AllAliveMonsters 当敌人列表，而它 = Enemies + AllyMonsters + Enemy2Monsters
                // + NeutralMonsters，盟友和中立全混在里面 → 会把盟友当成攻击目标。
                // 现在按 CActor.Type 分类，并且敌对性走游戏自己的 AreActorsAllied。
                try
                {
                    CActor.EType t = a.Type;
                    u.Faction = ToFaction(t);
                    u.FactionName = FactionLabel(u.Faction);
                    u.IsHostileToPlayer = !CActor.AreActorsAllied(CActor.EType.Player, t, null, a);
                }
                catch
                {
                    u.Faction = FactionKind.Unknown;
                    u.FactionName = "未知";
                    u.IsHostileToPlayer = false;   // 拿不准就不当敌人，绝不乱打
                }

                // ---- 先攻 ----
                // 先攻是方法，不是属性
                try { u.Initiative = a.Initiative(); } catch { /* 忽略 */ }
                u.InitiativePredicted = u.Initiative;
                u.InitiativeIsPredicted = false;

                // 怪物在行动牌还没选出来时 Initiative() 返回 0；用下一张行动牌的先攻补上
                if (u.Initiative == 0)
                {
                    try
                    {
                        if (a is CEnemyActor enemy && enemy.MonsterClass != null)
                        {
                            CMonsterAbilityCard next = enemy.MonsterClass.NextAbilityCard;
                            if (next != null && next.Initiative > 0)
                            {
                                u.InitiativePredicted = next.Initiative;
                                u.InitiativeIsPredicted = true;
                            }
                        }
                    }
                    catch { /* 忽略 */ }
                }

                if (isPlayer)
                {
                    var p = a as CPlayerActor;
                    u.Name = p?.CharacterName ?? u.ClassId;
                    try { u.DisplayName = Loc.T(a.ActorLocKey()); } catch { u.DisplayName = u.Name; }
                }
                else
                {
                    var m = a as CEnemyActor;
                    u.Name = u.ClassId + (m != null ? "#" + m.StandeeID : "");
                    try { u.DisplayName = Loc.T(a.ActorLocKey()); } catch { u.DisplayName = u.ClassId; }
                    if (m != null && !string.IsNullOrEmpty(u.DisplayName) && u.DisplayName != u.ClassId)
                    {
                        u.DisplayName = u.DisplayName + "#" + m.StandeeID;   // 同名 standee 要能区分
                    }
                }

                // ---- 怪物意图（M3 入口：CMonsterClass.RoundAbilityCard / NextAbilityCard）----
                // 玩家单位没有行动卡，跳过。怪物在本回合行动牌还没选出时用 NextAbilityCard 顶上，
                // 和上面 InitiativePredicted 的口径保持一致。
                if (!isPlayer)
                {
                    try
                    {
                        if (a is CEnemyActor enemy && enemy.MonsterClass != null)
                        {
                            CMonsterAbilityCard card = enemy.MonsterClass.RoundAbilityCard
                                                    ?? enemy.MonsterClass.NextAbilityCard;
                            if (card != null && card.Action != null)
                            {
                                u.Intent = HandReader.ReadAction(card.Action, "意图");

                                // ★ 未激活的卡（还在抽牌堆里）运行时强度可能是 0
                                //   （RoundAbilityCard 为空 → 回退 NextAbilityCard = 抽牌堆顶那张）
                                //   → 回退读 **YML 原始卡**（强度是静态写死的数值）
                                if (u.Intent != null && u.Intent.AttackStrength <= 0)
                                {
                                    try
                                    {
                                        CAction ymlAction = card.GetMonsterCardYML?.CardAction;
                                        if (ymlAction != null)
                                        {
                                            ActionSnapshot ymlIntent = HandReader.ReadAction(ymlAction, "意图");
                                            if (ymlIntent != null && ymlIntent.AttackStrength > 0)
                                            {
                                                u.Intent = ymlIntent;
                                                Plugin.Log?.LogInfo(
                                                    $"[FHSolver] 意图强度回退到 YML 卡：{enemy.ActorLocKey()} "

                                                    + $"攻{ymlIntent.AttackStrength}(射程{ymlIntent.AttackRange},目标{ymlIntent.AttackTargets})"
                                                    + $" | 原卡：{HandReader.DescribeAbilities(card.Action)}");
                                            }
                                        }
                                    }
                                    catch { /* 读不到就维持原样 */ }
                                }
                            }

                        }
                    }
                    catch { /* 读不到意图就留空 */ }
                }

                // 护盾：cache=false 避免写回游戏侧的 CachedShieldValue（只读插件不许留下副作用）
                try
                {
                    u.Shield = a.CalculateShield(
                        abilityToShieldFrom: null,
                        ignoreNeutralizeShield: false,
                        includeItemShieldValues: false,
                        cache: false,
                        forceActiveItems: false);
                }
                catch
                {
                    try { u.Shield = a.CachedShieldValue; } catch { /* 忽略 */ }
                }

                // ---- 条件（负面 / 正面）----
                // 缴械 → 不能攻击；静足 → 不能移动；眩晕 → 都不能。
                // 敌人（如精英冰角祭司）先于我们出手时会上这些，
                // 不建模就会给出"其实执行不了"的废方案。
                FillConditions(a, u);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取单位失败: {e.Message}");
            }
            return u;
        }

        /// <summary>
        /// 读单位当前的条件（负面 + 正面）。
        ///
        /// 走 <see cref="CTokens"/>：CActor.m_Tokens 是私有字段，用反射取，
        /// 然后逐个调 HasKey(token) 查 —— 比 GetAllNegativeConditions 稳
        /// （后者在不同版本里签名有出入）。
        ///
        /// 对建模最关键的三个：
        ///   Disarm   缴械 → 攻击类半边用不了
        ///   Immobilize 静足 → 移动类半边用不了
        ///   Stun     眩晕 → 整个半边都用不了
        /// </summary>
        private static void FillConditions(CActor a, UnitSnapshot u)
        {
            try
            {
                if (a == null || u == null) return;

                FieldInfo fi = typeof(CActor).GetField("m_Tokens",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi == null) return;

                object tokens = fi.GetValue(a);
                if (tokens == null) return;
                System.Type tt = tokens.GetType();

                foreach (string name in Enum.GetNames(typeof(CCondition.ENegativeCondition)))
                {
                    if (name == "NA") continue;
                    object token = Enum.Parse(typeof(CCondition.ENegativeCondition), name);
                    MethodInfo hasKey = tt.GetMethod("HasKey",
                        new[] { typeof(CCondition.ENegativeCondition), typeof(bool) });
                    bool has = false;
                    try
                    {
                        if (hasKey != null)
                        {
                            has = (bool)hasKey.Invoke(tokens, new object[] { token, false });
                        }
                    }
                    catch { /* 该重载不存在就跳过 */ }

                    if (has) u.NegativeConditions.Add(name);
                }

                foreach (string name in Enum.GetNames(typeof(CCondition.EPositiveCondition)))
                {
                    if (name == "NA") continue;
                    object token = Enum.Parse(typeof(CCondition.EPositiveCondition), name);
                    MethodInfo hasKey = tt.GetMethod("HasKey",
                        new[] { typeof(CCondition.EPositiveCondition) });
                    bool has = false;
                    try
                    {
                        if (hasKey != null)
                        {
                            has = (bool)hasKey.Invoke(tokens, new object[] { token });
                        }
                    }
                    catch { /* 该重载不存在就跳过 */ }

                    if (has) u.PositiveConditions.Add(name);
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"[FHSolver] 读取条件失败: {e.Message}");
            }
        }

        private static FactionKind ToFaction(CActor.EType t)
        {
            switch (t)
            {
                case CActor.EType.Player: return FactionKind.Player;
                case CActor.EType.Enemy: return FactionKind.Enemy;
                case CActor.EType.Enemy2: return FactionKind.Enemy2;
                case CActor.EType.Ally: return FactionKind.Ally;
                case CActor.EType.Neutral: return FactionKind.Neutral;
                case CActor.EType.HeroSummon: return FactionKind.HeroSummon;
                default: return FactionKind.Unknown;
            }
        }

        /// <summary>中文阵营名。</summary>
        public static string FactionLabel(FactionKind f)
        {
            switch (f)
            {
                case FactionKind.Player: return "玩家";
                case FactionKind.Enemy: return "敌人";
                case FactionKind.Enemy2: return "敌人";
                case FactionKind.Ally: return "盟友";
                case FactionKind.Neutral: return "中立";
                case FactionKind.HeroSummon: return "召唤物";
                default: return "未知";
            }
        }
    }
}
