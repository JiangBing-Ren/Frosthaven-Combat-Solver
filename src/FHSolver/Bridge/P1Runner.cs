using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AStar;
using BepInEx.Logging;
using FHSolver.Model;
using FHSolver.Solver;
using ScenarioRuleLibrary;

namespace FHSolver.Bridge;

/// <summary>
/// P1 单回合推荐（团队联合版）+ 多回合推演 + 报告生成。
///
/// ⚠ 本文件 2026-10-25 曾被一次脚本失误整个清空（io.open(...,'w') 先截断、
///   后续语句又抛异常），已用 ilspycmd 从部署的 DLL 完整反编译恢复。
///   恢复后**注释与原始变量名丢失**，逻辑与功能经 237 项断言验证无损。
///   现在项目已有 git（基线 fd9bd4e / 恢复 d984e33），改前请先 git status。
/// </summary>
internal static class P1Runner
{

	private sealed class UnitRef
	{
		public CActor Actor;

		public UnitSnapshot Snap;

		public int X;

		public int Y;
	}

	private sealed class PlayerRun
	{
		public int Index;

		public string Name;

		public int Initiative;

		public P1Context Ctx;

		public List<TurnPlan> Plans;

		public double Ms;

		public bool Ok;

		public string SkipReason;

		public Dictionary<string, int> Adjust;
	}

	private static readonly Dictionary<(int, int, int), List<(int X, int Y, int Hazard)>> DestCache = new Dictionary<(int, int, int), List<(int, int, int)>>();

	private static readonly Dictionary<(int, int, int, int), int> DistCache = new Dictionary<(int, int, int, int), int>();

	private static readonly Dictionary<(int, int, int, int), bool> LosCache = new Dictionary<(int, int, int, int), bool>();

	private static readonly Dictionary<(int, int), bool> ShadowCache = new Dictionary<(int, int), bool>();

	private static CActor _actor;

	private static readonly string[] UnmodeledMechanisms = new string[20]
	{
		"死行：阴影**已建模** —— 造/移/除/从阴影发动四类能力都能识别，上限 5，", "            「深渊力量」攻击按 1+阴影数 求值，造阴影会计入评分（权重可配）。", "            但**本回合新造的阴影不会立刻影响本回合**的其他半边。", "死行：「深渊力量」下半的持续加成需要**3 格内有阴影**才生效，这个前提仍未建模。", "            纯「造/移阴影」且不带移动的半边枚举不到（不产生位移也不攻击），带移动的正常。", "陷阱（Trap）伤害：**已建模** —— 进入伤害型陷阱会扣血并计入评分（方案自动绕开）；", "            但**不建模把敌人推/拉进陷阱**的收益（那需要建模推/拉的落点）。", "冰面滑动：**已被游戏寻路覆盖** —— FindCharacterPath 已算冰面 cost=0（滑行免费）。", "            且只有「最后一格不滑、下格有单位或转弯不滑」，全在寻路里，无需额外建模。", "开门是**独立动作**（消耗一次行动），本版本把开门当作移动的一部分。",
		"压力板 / 刷怪点 / 可摧毁物件的触发未建模。", "召唤物「主动召唤」未建模（召唤物作为占位单位与敌人目标已纳入）。", "范围攻击：**真 AOE 已建模**（多目标 + 每个目标独立抽修正牌，按射程近似覆盖范围）；", "            但**阵型单体**（有形状只打 1 个）仍未建模 —— 形状是站位要求不是伤害范围。", "先攻编织（用低先攻还是高先攻）目前只做枚举，不做**安全性**权衡。", "★ 生存维度只算**当前回合**会挨打；跨回合的挨打与总战损在「多回合」页里有。", "敌人**给我方上状态**（缴械/中毒/静足/眩晕）：**已建模** ——", "多回合推演：贴近目标取\"离焦点最近的可达格\"（真实是\"离最佳攻击位最近\"）；", "            不模拟敌人对你方的优/劣势；队友回合不推进。", "            但敌人**寻路与修正牌都是真实模拟的**（走哪、翻什么牌、打多少点都准）。"
	};

	public static void Run(int preferredPlayerIndex = -1)
	{
		// ★ 性能埋点（学 StS2 的 frame_percentiles / allocation_telemetry）：
		//   先量清楚哪一段慢，再决定优化什么 —— 别凭感觉优化。
		//   已知：P1 单角色枚举只要 2-3 ms，所以瓶颈不在枚举，
		//   更可能在「敌人预测」或「多回合推演」。
		PerfTimers perf = new PerfTimers();
		System.Diagnostics.Stopwatch swAll = System.Diagnostics.Stopwatch.StartNew();

		try
		{

			if (!ScenarioManager.IsInTactical)
			{
				ManualLogSource log = Plugin.Log;
				if (log != null)
				{
					log.LogInfo((object)"[FHSolver] P1：不在战斗中，跳过");
				}
				return;
			}
			CScenario scenario = ScenarioManager.Scenario;
			if (scenario == null)
			{
				ManualLogSource log2 = Plugin.Log;
				if (log2 != null)
				{
					log2.LogInfo((object)"[FHSolver] P1：无场景，跳过");
				}
				return;
			}
			List<CPlayerActor> list = new List<CPlayerActor>();
			foreach (CPlayerActor playerActor in scenario.PlayerActors)
			{
				if (playerActor != null)
				{
					list.Add(playerActor);
				}
			}
			if (list.Count == 0)
			{
				ManualLogSource log3 = Plugin.Log;
				if (log3 != null)
				{
					log3.LogInfo((object)"[FHSolver] P1：场上没有玩家单位");
				}
				return;
			}
			List<int> list2 = new List<int>();
			if (Plugin.StaticPlanAllPlayers)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list2.Add(i);
				}
			}
			else
			{
				int num = preferredPlayerIndex;
				if (num < 0 || num >= list.Count)
				{
					num = FindCurrentActorIndex(list);
				}
				list2.Add(num);
			}
			DestCache.Clear();
			DistCache.Clear();
			LosCache.Clear();
			ShadowCache.Clear();
			GoalSnapshot goals = GoalReader.Read();
			perf.Mark("读战役目标");

			List<ModifierDeckSnapshot> monsterDecks = DeckReader.ReadAllMonsterDecks();
			perf.Mark("读怪物牌堆");
			int shadowCount = Geometry.ShadowCount();
			List<string> list3 = new List<string>();
			try
			{
				if (list.Count > 0)
				{
					UnitSnapshot unitSnapshot = StateReader.ReadUnit((CActor)(object)list[0], isPlayer: true);
					if (unitSnapshot != null)
					{
						list3.AddRange(unitSnapshot.NegativeConditions);
						list3.AddRange(unitSnapshot.PositiveConditions);
					}
				}
			}
			catch
			{
			}
			ManualLogSource log4 = Plugin.Log;
			if (log4 != null)
			{
				log4.LogInfo((object)"[FHSolver] ===== P1 单回合穷举（团队联合版）=====");
			}
			List<EnemyPredictor.Prediction> list4 = EnemyPredictor.Predict(scenario);
			perf.Mark("敌人预测");

			// ★ 对账（学 StS2 的 deployment_drift=replan）：
			//   把上回合的"预期"和这一回合的"实际"比一遍 ——
			//   不一致的地方，就是模型最需要改进的地方。
			List<string> reconcile = new List<string>();
			BattleSnapshot reconcileSnap = null;
			int reconcileMyHp = 0;
			List<AbilityCardSnapshot> reconcileHand = null;
			TurnLedger.Fingerprint reconcileFp = default(TurnLedger.Fingerprint);
			try
			{

				BattleSnapshot snap = StateReader.Read();
				reconcileSnap = snap;

				List<UnitSnapshot> snapMonsters = snap?.Monsters ?? new List<UnitSnapshot>();
				List<UnitSnapshot> snapOthers = new List<UnitSnapshot>();
				if (snap != null && snap.Players != null) snapOthers.AddRange(snap.Players);

				int myHp = 0;
				List<AbilityCardSnapshot> myHand = null;
				ModifierDeckSnapshot myDeck = null;

				if (list.Count > 0)
				{
					UnitSnapshot me = null;
					foreach (UnitSnapshot u in snapOthers) { if (u != null) { me = u; break; } }
					if (me != null) myHp = me.Health;
					myHand = HandReader.ReadHand(list[0].CharacterClass);
					myDeck = DeckReader.ReadPlayerDeck(list[0].CharacterClass, "我");
					reconcileMyHp = myHp;
					reconcileHand = myHand;
				}


				TurnLedger.Fingerprint fp = TurnLedger.ComputeFingerprint(
					snapMonsters, snapOthers, myHand, myDeck, shadowCount);
				reconcileFp = fp;
				reconcile = TurnLedger.Reconcile(snapMonsters, myHp, fp);


				// 记录本回合的预期 —— 放到第二遍算完之后做（见下方 RecordExpectation 调用）
			}

			catch (Exception e)
			{
				Plugin.Log?.LogWarning($"[FHSolver] 对账失败: {e.Message}");
			}

			List<PlayerRun> list5 = new List<PlayerRun>();
			foreach (int item in list2)
			{
				list5.Add(PlanOne(list, scenario, item, null, null, 0, goals, monsterDecks, shadowCount, list4, out var _));
			}
			foreach (PlayerRun item2 in list5)
			{
				if (item2.Ok && item2.Plans.Count > 0 && item2.Plans[0].Initiative > 0)
				{
					item2.Initiative = item2.Plans[0].Initiative;
				}
			}
			Dictionary<string, (int, int)> dictionary = new Dictionary<string, (int, int)>();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			if (Plugin.StaticPredictAllies && list5.Count > 1)
			{
				List<PlayerRun> list6 = (from r in list5
					where r.Ok
					orderby r.Initiative, r.Index
					select r).ToList();
				foreach (PlayerRun item3 in list6)
				{
					Dictionary<string, int> adjust = ((dictionary2.Count > 0) ? new Dictionary<string, int>(dictionary2) : null);
					Dictionary<string, (int, int)> dictionary3 = ((dictionary.Count > 0) ? new Dictionary<string, (int, int)>(dictionary) : null);
					foreach (EnemyPredictor.Prediction item4 in list4)
					{
						if (item4.WillMove && item4.Initiative < item3.Initiative)
						{
							if (dictionary3 == null)
							{
								dictionary3 = new Dictionary<string, (int, int)>();
							}
							dictionary3[item4.Guid] = (item4.ToX, item4.ToY);
						}
					}
					PlayerRun playerRun = PlanOne(list, scenario, item3.Index, adjust, dictionary3, item3.Initiative, goals, monsterDecks, shadowCount, list4, out var applied2);
					playerRun.Initiative = item3.Initiative;
					playerRun.Adjust = applied2;
					for (int num2 = 0; num2 < list5.Count; num2++)
					{
						if (list5[num2].Index == item3.Index)
						{
							list5[num2] = playerRun;
						}
					}
					if (!playerRun.Ok || playerRun.Plans.Count <= 0)
					{
						continue;
					}
					TurnPlan turnPlan = playerRun.Plans[0];
					CPlayerActor val = list[item3.Index];
					string text = ((val != null) ? ((CActor)val).ActorGuid : null);
					if (text != null && turnPlan.FinalX >= 0)
					{
						dictionary[text] = (turnPlan.FinalX, turnPlan.FinalY);
					}
					foreach (AttackChoice attack in turnPlan.Attacks)
					{
						if (!string.IsNullOrEmpty(attack.TargetGuid))
						{
							if (!dictionary2.ContainsKey(attack.TargetGuid))
							{
								dictionary2[attack.TargetGuid] = 0;
							}
							dictionary2[attack.TargetGuid] += attack.Damage;
						}
					}
				}
			}
			Dictionary<string, int> dictionary4 = new Dictionary<string, int>();
			foreach (PlayerRun item5 in list5)
			{
				if (item5.Ok && item5.Plans.Count != 0)
				{
					CPlayerActor val2 = list[item5.Index];
					string text2 = ((val2 != null) ? ((CActor)val2).ActorGuid : null);
					if (text2 != null && item5.Plans[0].Initiative > 0)
					{
						dictionary4[text2] = item5.Plans[0].Initiative;
					}
				}
			}
			List<UnitSnapshot> order = BuildInitiativeOrder(scenario, dictionary4);
			List<List<string>> list7 = new List<List<string>>();
			Dictionary<string, MultiTurnResult> dictionary5 = new Dictionary<string, MultiTurnResult>();
			int num3 = 0;
			foreach (PlayerRun item6 in list5)
			{
				if (!item6.Ok)
				{
					list7.Add(new List<string> { $"[{item6.Index + 1}] {item6.Name}：{item6.SkipReason}" });
					continue;
				}
				perf.Mark("P1枚举");
				List<string> list8 = Report(item6.Ctx, item6.Plans, item6.Ms, item6.Index, order, item6, list4);
				perf.Mark("生成角色文本");
				MultiTurnResult multiTurnResult = ComputeMultiTurn(item6);
				perf.Mark("多回合推演");

				if (multiTurnResult != null)
				{
					dictionary5[item6.Name] = multiTurnResult;
					list8.AddRange(MultiTurnFollowUpLines(multiTurnResult));
				}
				list7.Add(list8);
				num3++;
			}
			perf.Mark("生成页签");
			// ★ 记录本回合的预期（对账用）—— 必须在第二遍算完之后
			try { RecordExpectation(reconcileSnap, reconcileMyHp, reconcileHand, reconcileFp); }
			catch (Exception e) { Plugin.Log?.LogWarning($"[FHSolver] 记录预期失败: {e.Message}"); }

			Flush(list.Count, num3, list7, order, list5, list4, goals, shadowCount, list3, dictionary5, reconcile);

			swAll.Stop();
			perf.TotalMs = swAll.Elapsed.TotalMilliseconds;

			// ★ 性能日志：一眼看出哪一段慢
			ManualLogSource perfLog = Plugin.Log;
			if (perfLog != null)
			{
				perfLog.LogInfo((object)"[FHSolver] " + perf.Summary());
			}

		}
		catch (Exception ex)
		{
			ManualLogSource log5 = Plugin.Log;
			if (log5 != null)
			{
				log5.LogError((object)("[FHSolver] P1 运行异常: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
	}

	/// <summary>
		/// 记录本回合的预期（下一回合用来对账）。
		/// </summary>
		private static void RecordExpectation(
			BattleSnapshot snap, int myHp, List<AbilityCardSnapshot> hand,
			TurnLedger.Fingerprint fp)
		{
			if (snap == null) return;
			List<UnitSnapshot> monsters = snap.Monsters ?? new List<UnitSnapshot>();
			TurnLedger.RecordExpectation(monsters, myHp, 0, hand,
				TurnLedger.LastPlanSummary, fp, snap.RoundNumber);
		}


		private static PlayerRun PlanOne(List<CPlayerActor> players, CScenario scenario, int idx, Dictionary<string, int> adjust, Dictionary<string, (int X, int Y)> cover, int myInitiative, GoalSnapshot goals, List<ModifierDeckSnapshot> monsterDecks, int shadowCount, List<EnemyPredictor.Prediction> enemyPreds, out Dictionary<string, int> applied)

	{
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		applied = null;
		PlayerRun playerRun = new PlayerRun
		{
			Index = idx
		};
		CPlayerActor val = players[idx];
		if (val == null)
		{
			playerRun.SkipReason = "单位为空";
			return playerRun;
		}
		CCharacterClass characterClass = val.CharacterClass;
		if (characterClass == null)
		{
			playerRun.SkipReason = "没有 CharacterClass";
			CClass val2 = ((CActor)val).Class;
			playerRun.Name = ((val2 != null) ? val2.ID : null) ?? "?";
			return playerRun;
		}
		playerRun.Name = val.CharacterName ?? ((CClass)characterClass).ID;
		playerRun.Initiative = SafeInitiative((CActor)(object)val);
		_actor = (CActor)(object)val;
		P1Context p1Context = new P1Context();
		p1Context.ActorName = playerRun.Name;
		p1Context.Hand = HandReader.ReadHand(characterClass);
		p1Context.Deck = DeckReader.ReadPlayerDeck(characterClass, playerRun.Name);
		p1Context.MonsterDecks = monsterDecks ?? new List<ModifierDeckSnapshot>();
		p1Context.ShadowCount = shadowCount;
		p1Context.Adv = ReadAdvantage((CActor)(object)val);
		p1Context.LootSpots = goals?.LootSpots ?? new List<LootSpot>();
		p1Context.Reachable = ReachableCached;
		p1Context.RangeDistance = RangeDistCached;
		p1Context.HasLOS = LosCached;
		p1Context.IsShadowTile = ShadowCached;
		P1Context p1Context2 = p1Context;
		Point arrayIndex = ((CActor)val).ArrayIndex;
		bool flag = true;
		p1Context2.StartX = arrayIndex.X;
		p1Context2.StartY = arrayIndex.Y;
		p1Context2.ThreatByInitiative = BuildThreatByInitiative(enemyPreds, MyInitiativeOptions(p1Context2, myInitiative));
		p1Context2.W.DiversityPerBucket = Plugin.StaticDiversityPerBucket;
		try
		{
			UnitSnapshot unitSnapshot = StateReader.ReadUnit((CActor)(object)val, isPlayer: true);
			if (unitSnapshot != null)
			{
				p1Context2.MyNegativeConditions = unitSnapshot.NegativeConditions ?? new List<string>();
				p1Context2.MyPositiveConditions = unitSnapshot.PositiveConditions ?? new List<string>();
			}
		}
		catch
		{
		}
		try
		{
			p1Context2.StartHealth = ((CActor)val).Health;
			p1Context2.StartMaxHealth = ((CActor)val).MaxHealth;
		}
		catch
		{
			p1Context2.StartHealth = 0;
			p1Context2.StartMaxHealth = 0;
		}
		List<UnitRef> list = new List<UnitRef>();
		try
		{
			foreach (CActor allActor in scenario.AllActors)
			{
				if (allActor == null || allActor.ActorGuid == ((CActor)val).ActorGuid)
				{
					continue;
				}
				UnitSnapshot unitSnapshot2 = StateReader.ReadUnit(allActor, allActor is CPlayerActor);
				if (unitSnapshot2.Initiative == 0 && unitSnapshot2.InitiativePredicted > 0)
				{
					unitSnapshot2.Initiative = unitSnapshot2.InitiativePredicted;
					unitSnapshot2.InitiativeFrom = InitiativeSource.Predicted;
				}
				list.Add(new UnitRef
				{
					Actor = allActor,
					Snap = unitSnapshot2,
					X = unitSnapshot2.X,
					Y = unitSnapshot2.Y
				});
				if (unitSnapshot2.IsHostileToPlayer)
				{
					if (!allActor.IsDead)
					{
						p1Context2.Monsters.Add(unitSnapshot2);
					}
				}
				else
				{
					p1Context2.Others.Add(unitSnapshot2);
				}
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = Plugin.Log;
			if (log != null)
			{
				log.LogWarning((object)("[FHSolver] 读取单位列表失败: " + ex.Message));
			}
		}
		if (adjust != null && adjust.Count > 0)
		{
			applied = new Dictionary<string, int>();
			foreach (UnitSnapshot monster in p1Context2.Monsters)
			{
				if (adjust.TryGetValue(monster.Guid, out var value) && value > 0)
				{
					monster.PredictedDamage = value;
					applied[monster.DisplayName] = value;
				}
			}
		}
		try
		{
			foreach (UnitRef item in list)
			{
				if (cover != null && cover.TryGetValue(item.Actor.ActorGuid, out (int, int) value2))
				{
					p1Context2.Occupied.Add(value2);
				}
				else if ((!item.Snap.IsHostileToPlayer || !item.Actor.IsDead) && !EnemyLeavesTileBefore(item, myInitiative))
				{
					p1Context2.Occupied.Add((item.X, item.Y));
				}
			}
		}
		catch (Exception ex2)
		{
			ManualLogSource log2 = Plugin.Log;
			if (log2 != null)
			{
				log2.LogWarning((object)("[FHSolver] 读取占用格失败: " + ex2.Message));
			}
		}
		if (p1Context2.Hand.Count < 2)
		{
			playerRun.SkipReason = $"手牌不足 2 张（{p1Context2.Hand.Count}），本回合无法出牌";
			return playerRun;
		}
		DateTime now = DateTime.Now;
		playerRun.Plans = P1Planner.Enumerate(p1Context2, Plugin.StaticTopN);
		playerRun.Ms = (DateTime.Now - now).TotalMilliseconds;
		playerRun.Ctx = p1Context2;
		playerRun.Ok = true;
		return playerRun;
	}

	private static int SafeInitiative(CActor a)
	{
		try
		{
			return a.Initiative();
		}
		catch
		{
			return 0;
		}
	}

	private static bool EnemyLeavesTileBefore(UnitRef ur, int myInitiative)
	{
		try
		{
			if (!ur.Snap.IsHostileToPlayer)
			{
				return false;
			}
			int num = ur.Snap.Initiative;
			if (num == 0)
			{
				num = ur.Snap.InitiativePredicted;
			}
			if (num <= 0 || myInitiative <= 0)
			{
				return false;
			}
			if (num >= myInitiative)
			{
				return false;
			}
			ActionSnapshot intent = ur.Snap.Intent;
			if (intent == null)
			{
				return false;
			}
			return intent.HasMove && intent.MoveStrength > 0;
		}
		catch
		{
			return false;
		}
	}

	private static List<UnitSnapshot> BuildInitiativeOrder(CScenario scenario, Dictionary<string, int> planInit)
	{
		List<UnitSnapshot> list = new List<UnitSnapshot>();
		try
		{
			foreach (CActor allActor in scenario.AllActors)
			{
				if (allActor != null)
				{
					UnitSnapshot unitSnapshot = StateReader.ReadUnit(allActor, allActor is CPlayerActor);
					if (planInit != null && planInit.TryGetValue(unitSnapshot.Guid, out var value) && value > 0)
					{
						unitSnapshot.Initiative = value;
						unitSnapshot.InitiativeFrom = InitiativeSource.Plan;
					}
					else if (unitSnapshot.Initiative == 0 && unitSnapshot.InitiativePredicted > 0)
					{
						unitSnapshot.Initiative = unitSnapshot.InitiativePredicted;
						unitSnapshot.InitiativeFrom = InitiativeSource.Predicted;
					}
					list.Add(unitSnapshot);
				}
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = Plugin.Log;
			if (log != null)
			{
				log.LogWarning((object)("[FHSolver] 读取先攻顺序失败: " + ex.Message));
			}
		}
		list.Sort((UnitSnapshot x, UnitSnapshot y) =>
		{
			int num = x.Initiative.CompareTo(y.Initiative);
			return (num != 0) ? num : string.CompareOrdinal(x.Name, y.Name);
		});
		return list;
	}

	private static int FindCurrentActorIndex(List<CPlayerActor> players)
	{
		try
		{
			CActor currentActor = GameState.CurrentActor;
			if (currentActor == null)
			{
				return 0;
			}
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i] != null && ((CActor)players[i]).ActorGuid == currentActor.ActorGuid)
				{
					return i;
				}
			}
		}
		catch
		{
		}
		return 0;
	}

	private static AdvStatus ReadAdvantage(CActor actor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		try
		{
			EAdvantageStatuses advantageStatus = actor.GetAdvantageStatus(false, false, 0);
			EAdvantageStatuses val = advantageStatus;
			EAdvantageStatuses val2 = val;
			if ((int)val2 != 1)
			{
				if ((int)val2 == 2)
				{
					return AdvStatus.Disadvantage;
				}
				return AdvStatus.None;
			}
			return AdvStatus.Advantage;
		}
		catch
		{
			return AdvStatus.None;
		}
	}

	private static List<(int X, int Y, int Hazard)> ReachableCached(int x, int y, int range)
	{
		(int, int, int) key = (x, y, range);
		if (DestCache.TryGetValue(key, out List<(int, int, int)> value))
		{
			return value;
		}
		List<(int, int, int)> list = Geometry.Reachable(_actor, x, y, range);
		DestCache[key] = list;
		return list;
	}

	private static int RangeDistCached(int x1, int y1, int x2, int y2)
	{
		(int, int, int, int) key = (x1, y1, x2, y2);
		if (DistCache.TryGetValue(key, out var value))
		{
			return value;
		}
		int num = Geometry.RangeDistance(x1, y1, x2, y2);
		DistCache[key] = num;
		return num;
	}

	private static bool LosCached(int x1, int y1, int x2, int y2)
	{
		(int, int, int, int) key = (x1, y1, x2, y2);
		if (LosCache.TryGetValue(key, out var value))
		{
			return value;
		}
		bool flag = Geometry.HasLOS(x1, y1, x2, y2);
		LosCache[key] = flag;
		return flag;
	}

	private static bool ShadowCached(int x, int y)
	{
		(int, int) key = (x, y);
		if (ShadowCache.TryGetValue(key, out var value))
		{
			return value;
		}
		bool flag = Geometry.IsShadowTile(x, y);
		ShadowCache[key] = flag;
		return flag;
	}

	private static List<string> Report(P1Context ctx, List<TurnPlan> plans, double ms, int playerIndex, List<UnitSnapshot> order, PlayerRun run, List<EnemyPredictor.Prediction> enemyPreds)
	{
		List<string> list = new List<string>();
		list.Add($"===== [{playerIndex + 1}] {ctx.ActorName} | 先攻 {run.Initiative} =====");
		list.Add($"手牌 {ctx.Hand.Count} 张，敌人 {ctx.Monsters.Count} 个" + ((ctx.Others.Count > 0) ? $"，其他单位 {ctx.Others.Count} 个" : "") + $"，优劣势={ctx.Adv}，修正牌堆剩 {(ctx.Deck?.Remaining?.Count).GetValueOrDefault()} 张" + $" | 枚举耗时 {ms:F0} ms");
		List<string> list2 = new List<string>();
		foreach (UnitSnapshot item in order)
		{
			if (!item.IsHostileToPlayer || item.Initiative >= run.Initiative)
			{
				continue;
			}
			string text = item.DisplayName;
			if (item.Intent != null)
			{
				List<string> list3 = new List<string>();
				if (item.Intent.HasMove)
				{
					list3.Add("移动" + item.Intent.MoveStrength);
				}
				if (item.Intent.HasAttack)
				{
					list3.Add("攻击" + item.Intent.AttackStrength + "射程" + item.Intent.AttackRange + ((item.Intent.AttackTargets != 1) ? ("目标" + item.Intent.AttackTargets) : ""));
				}
				if (list3.Count > 0)
				{
					text = text + "(" + string.Join("、", list3) + ")";
				}
			}
			list2.Add(text);
		}
		if (list2.Count > 0)
		{
			list.Add("⚠ " + string.Join("、", list2) + " 先攻比你低、会先动 ——轮到你时它们多半已经执行完自己的移动，所以上面的格子会被让出来（占用已按此调整）；但它们**停在哪**取决于你当时的站位，本版本不预测落点，请自己留意。");
		}
		if (run.Adjust != null && run.Adjust.Count > 0)
		{
			List<string> list4 = new List<string>();
			foreach (KeyValuePair<string, int> item2 in run.Adjust)
			{
				list4.Add($"{item2.Key} -{item2.Value}");
			}
			list.Add("✔ 已按先攻更低（更先动）的队友的预测伤害扣过血量：" + string.Join("，", list4));
		}
		else if (order.Count > 1)
		{
			list.Add("（没有先攻更低的队友，血量就是快照值）");
		}
		list.Add("");
		list.Add("【场上单位】");
		foreach (UnitSnapshot monster in ctx.Monsters)
		{
			string text2 = ((monster.Intent != null) ? ("  意图: " + monster.Intent) : "");
			list.Add($"  「{monster.DisplayName}」@({monster.X},{monster.Y}) 血 {monster.Health}/{monster.MaxHealth}" + ((monster.PredictedDamage > 0) ? $"（先动队友预计打 {monster.PredictedDamage} → 到你时约 {monster.EffectiveHealth}）" : "") + ((monster.Shield > 0) ? $" 护盾{monster.Shield}" : "") + $" 先攻{monster.Initiative}" + text2);
		}
		foreach (UnitSnapshot other in ctx.Others)
		{
			list.Add($"  「{other.DisplayName}」[{other.FactionName}] @({other.X},{other.Y}) 血 {other.Health}/{other.MaxHealth}" + "  ← 友方，不能打");
		}
		List<EnemyPredictor.Prediction> list5 = ((enemyPreds != null) ? (from p in enemyPreds
			where p.WillMove
			orderby p.Initiative
			select p).ToList() : new List<EnemyPredictor.Prediction>());
		if (list5.Count > 0)
		{
			list.Add("");
			list.Add("【敌人动向】先攻比你低的敌人会先动，它们原来占的格子会空出来（上面的占用已按此调整）");
			foreach (EnemyPredictor.Prediction item3 in list5)
			{
				list.Add($"  「{item3.DisplayName}」先攻{item3.Initiative} 移动{item3.MovePoints}" + $"  ({item3.FromX},{item3.FromY})→约({item3.ToX},{item3.ToY})" + (string.IsNullOrEmpty(item3.FocusName) ? "" : ("  追「" + item3.FocusName + "」")));
			}
		}
		// ⚠ 这里原来又遍历了一次 ctx.Others，导致"友方单位"被列两遍
		//   （上面 707 行那处已经列过了）—— 已删。
		list.Add("");

		list.Add("【卡面半边诊断】（从当前位置评估；常驻加成与范围攻击已单独标出）");
		foreach (string item4 in P1Planner.Diagnose(ctx))
		{
			list.Add(item4);
		}
		if (plans.Count == 0)
		{
			list.Add("");
			list.Add("没有可执行的出牌方案");
		}
		else
		{
			list.Add("");
			list.Add($"【推荐方案 Top {plans.Count}】按评分从高到低，第 1 个就是最推荐的");
			for (int num = 0; num < plans.Count; num++)
			{
				list.Add("");
				list.Add($"----- #{num + 1} -----");
				list.AddRange(DescribePlan(ctx, plans[num]));
			}
		}
		list.Add("");
		list.Add("===== 调试信息（玩家可忽略） =====");
		list.Add($"起点 ({ctx.StartX},{ctx.StartY})");
		foreach (UnitSnapshot monster2 in ctx.Monsters)
		{
			list.Add($"  敌人 {monster2.Name} @({monster2.X},{monster2.Y}) 护盾{monster2.Shield} Guid#{Short(monster2.Guid)}");
		}
		for (int num2 = 0; num2 < plans.Count && num2 < 3; num2++)
		{
			TurnPlan turnPlan = plans[num2];
			list.Add($"  #{num2 + 1} 内部名: 先={turnPlan.First?.CardInternalName}" + $"/{turnPlan.First?.Source} 后={turnPlan.Second?.CardInternalName}/{turnPlan.Second?.Source}" + $" 中途({turnPlan.MidX},{turnPlan.MidY}) 终点({turnPlan.FinalX},{turnPlan.FinalY})");
			foreach (AttackChoice attack in turnPlan.Attacks)
			{
				list.Add($"      {attack.Side}半 {attack.CardInternalName} 攻{attack.BaseStrength}" + $" 从({attack.FromX},{attack.FromY}) → {attack.TargetInternalName}@{attack.TargetX},{attack.TargetY}" + string.Format(" 伤{0}{1} 表达式{2} 耗{3}张", attack.Damage, attack.Kills ? "[杀]" : "", attack.Expr, attack.CardsConsumed));
			}
		}
		return list;
	}

	private static void Flush(int playerCount, int done, List<List<string>> blocks, List<UnitSnapshot> order, List<PlayerRun> runs, List<EnemyPredictor.Prediction> enemyPreds, GoalSnapshot goals, int shadowCount = 0, List<string> stateConditions = null, Dictionary<string, MultiTurnResult> mtResults = null, List<string> reconcileLines = null)
	{
		List<string> list = new List<string>();
		list.Add($"===== P1 单回合推荐 | 队伍 {playerCount} 人，已出 {done} 份 =====");
		list.Add($"全场 {order.Count} 个单位（敌人/盟友/召唤物/玩家已分开，友方绝不是攻击目标）");
		list.Add("");
		list.Add("【先攻顺序】（数值小的先动）");
		foreach (UnitSnapshot item in order)
		{
			string text = "";
			if (item.InitiativeFrom == InitiativeSource.Predicted)
			{
				text = "（预测，取自下一张行动牌）";
			}
			else if (item.InitiativeFrom == InitiativeSource.Plan)
			{
				text = "（取自该角色的推荐方案）";
			}
			string text2 = (item.IsHostileToPlayer ? "" : "  ← 友方，不是攻击目标");
			string text3 = "";
			if (item.Intent != null)
			{
				List<string> list2 = new List<string>();
				if (item.Intent.HasMove)
				{
					list2.Add($"移动{item.Intent.MoveStrength}");
				}
				if (item.Intent.HasAttack)
				{
					list2.Add($"攻击{item.Intent.AttackStrength}(射程{item.Intent.AttackRange})");
				}
				if (list2.Count > 0)
				{
					text3 = "  意图: " + string.Join("、", list2);
				}
			}
			list.Add($"     先攻{item.Initiative,4}  {item.DisplayName}[{item.FactionName}]" + text2 + text + text3);
		}
		if (goals != null && goals.Objectives.Count > 0)
		{
			list.Add("");
			list.Add("【使命目标】");
			foreach (ObjectiveSnapshot objective in goals.Objectives)
			{
				if (!objective.IsHidden)
				{
					list.Add("  " + objective);
				}
			}
			if (goals.AnyWinsDespiteExhaustion)
			{
				list.Add("  ★ 存在「力竭仍算赢」的目标 —— 可以放心打光手牌");
			}
		}
		if (stateConditions.Count > 0)
		{
			list.Add("");
			list.Add("【我方状态】" + string.Join("、", stateConditions));
			foreach (string item2 in StateWarnings(stateConditions))
			{
				list.Add("  " + item2);
			}
		}
		Dictionary<(int, int), int> dictionary = Geometry.TrapMap();
		if (dictionary.Count > 0)
		{
			list.Add("");
			list.Add($"【陷阱】场上 {dictionary.Count} 个伤害型陷阱（走进去掉血，已计入评分）");
			int num = 0;
			foreach (KeyValuePair<(int, int), int> item3 in dictionary)
			{
				if (num++ >= 8)
				{
					break;
				}
				list.Add($"  ({item3.Key.Item1},{item3.Key.Item2}) 伤害 {item3.Value}");
			}
			list.Add("  ⚠ 移动方案会自动绕开；若你有推/拉能力，也可把敌人弄进去");
		}
		// ---- 对账（上一回合的预期 vs 这一回合的实际）----
		//   学 StS2 的 deployment_drift=replan：模型不准的地方要能指出来，
		//   否则玩家只会觉得"这插件不靠谱"，而不知道该改哪一块。
		if (reconcileLines != null && reconcileLines.Count > 0)
		{
			list.Add("");
			list.Add("【对账】与上一回合的预测比对");
			foreach (string r in reconcileLines) list.Add("  " + r);
		}

		// ---- 原生驱动探测（只读，不改游戏状态）----
		if (Plugin.StaticDriverProbe)
		{
			var names = new List<string>();
			foreach (PlayerRun rr in runs)
			{
				if (rr == null || rr.Plans == null || rr.Plans.Count == 0) continue;
				TurnPlan bp = rr.Plans[0];
				if (bp?.First != null && bp.First.CardName != null) names.Add(bp.First.CardName);
				if (bp?.Second != null && bp.Second.CardName != null) names.Add(bp.Second.CardName);
			}
			List<string> probe = NativeDriver.Probe(names);
			if (probe != null && probe.Count > 0)
			{
				list.Add("");
				list.Add("【原生驱动探测】只读诊断，未改动任何游戏状态");
				foreach (string p in probe) list.Add("  " + p);
			}
		}

		if (Plugin.StaticShadowDiagnostics)

		{
			list.Add("");

			list.Add($"【阴影】场上当前 {shadowCount}/5 个");
			if (shadowCount == 0)
			{
				list.Add("  ⚠ 一个阴影都没有 → 「暗影步法」这类**必须停在阴影**的牌本回合用不了，");
				list.Add("     「从阴影中发动」类也用不了。先用能造阴影的牌铺开。");
			}
			else
			{
				List<(int, int)> list3 = Geometry.ShadowTiles();
				if (list3.Count > 0)
				{
					list.Add("  位置：" + string.Join(" ", list3.Take(8).Select(((int X, int Y) t) => $"({t.X},{t.Y})")));
				}
				list.Add($"  「深渊力量」类攻击按 1+阴影数 = {1 + shadowCount} 算");
			}
		}
		if (goals != null && goals.LootSpots.Count > 0)
		{
			list.Add("");
			list.Add("【可捡拾】方案终点踩上去即可捡到，已计入评分");
			foreach (LootSpot lootSpot in goals.LootSpots)
			{
				list.Add($"  {lootSpot}");
			}
			if (goals.HasLootDeck)
			{
				list.Add("  下一张战利品：" + goals.NextLootName + (goals.NextLootIsItem ? "（= 装备，按第 2 档优先）" : "（材料，第 6 档）"));
			}
		}
		List<PlayerRun> list4 = (from x in runs
			where x.Ok && x.Plans.Count > 0
			orderby x.Initiative, x.Index
			select x).ToList();
		if (list4.Count > 1)
		{
			list.Add("");
			list.Add("【团队总览】（按先攻先后；站位是各人跑完自己方案后的终点）");
			foreach (PlayerRun item4 in list4)
			{
				TurnPlan turnPlan = item4.Plans[0];
				list.Add($"  先攻{item4.Initiative,4}  {item4.Name}" + " → 先" + turnPlan.First.SideLabel + "「" + turnPlan.First.CardName + "」 再" + turnPlan.Second.SideLabel + "「" + turnPlan.Second.CardName + "」" + $" | 站位({turnPlan.FinalX},{turnPlan.FinalY}) 伤{turnPlan.TotalDamage} 杀{turnPlan.KillCount}");
			}
		}
		foreach (List<string> block in blocks)
		{
			list.Add("");
			foreach (string item5 in block)
			{
				list.Add(item5);
			}
		}
		foreach (string item6 in list)
		{
			ManualLogSource log = Plugin.Log;
			if (log != null)
			{
				log.LogInfo((object)("[FHSolver] " + item6));
			}
		}
		List<UiSection> list5 = ToUiSections(blocks, order, goals, enemyPreds, runs, mtResults);

		// ★ 一回合一个页签（用户要求：翻页功能 + 多回合详细方案）
		List<UiSection> roundSecs = BuildRoundSections(runs, mtResults);
		foreach (UiSection rs in roundSecs)
		{
			if (rs == null) continue;
			rs.Name = UiSection.UniqueName(list5.Select(x => x.Name), rs.Name);
			list5.Add(rs);
		}

		foreach (UiSection item7 in list5)
		{
			if (item7 == null || item7.Name != "多回合")
			{
				continue;
			}
			list.Add("");
			list.Add("===== 多回合推演 =====");
			foreach (UiLine line in item7.Lines)
			{
				list.Add(line.Text);
			}
		}
		WriteReportFile(list);
		SolverUI.SetSections(list5, "FHSolver");
		if (Plugin.StaticShowUiOnSolve)
		{
			SolverUI.Show();
		}
	}

	private static string Short(string g)
	{
		return string.IsNullOrEmpty(g) ? "?" : g.Substring(0, 8);
	}

	private static List<string> DescribePlan(P1Context ctx, TurnPlan p)
	{
		List<string> list = new List<string>();
		PlanHalf h1 = p.First;
		PlanHalf h2 = p.Second;
		if (h1 == null || h2 == null)
		{
			return list;
		}
		List<(int, string, string, int)> source = new List<(int, string, string, int)>
		{
			(h1.CardIndex + 1, h1.CardName, h1.SideLabel, p.InitiativeOf(h1.CardIndex)),
			(h2.CardIndex + 1, h2.CardName, h2.SideLabel, p.InitiativeOf(h2.CardIndex))
		}.OrderBy(((int idx, string name, string side, int ini) c) => c.ini).ThenBy(((int idx, string name, string side, int ini) c) => c.idx).ToList();
		list.Add("出牌：" + string.Join(" + ", source.Select(((int idx, string name, string side, int ini) c) => $"第{c.idx}张「{c.name}」(先攻{c.ini})的【{c.side}】")));
		list.Add($"次序：先做第{h1.CardIndex + 1}张，再做第{h2.CardIndex + 1}张" + $"（本方案先攻 {p.Initiative}）");
		// ★ 战斗效率提示：让玩家直接看到"这个方案要打几个回合才清场"
		//   （用户反馈"在通关前提下几乎是最差解" —— 回合数是最直观的指标）
		int remainingHp = 0;
		if (ctx != null && ctx.Monsters != null)
		{
			foreach (UnitSnapshot m in ctx.Monsters)
			{
				if (m != null && m.EffectiveHealth > 0) remainingHp += m.EffectiveHealth;
			}
		}
		string effNote = "";
		if (remainingHp > 0 && p.EffectiveDamage > 0)
		{
			int est = (int)Math.Ceiling(remainingHp / (double)p.EffectiveDamage);
			effNote = $"，★ 清场预计还要 {est} 回合（剩余敌人 {remainingHp} 血）";
		}

		list.Add($"预期：造成 {p.TotalDamage} 点伤害（有效 {p.EffectiveDamage}）" + ((p.KillCount > 0) ? $"，击杀 {p.KillCount} 个" : "") + $"，得经验 {p.TotalXP}" + ((p.LostCost > 0) ? $"，⚠ 会永久失去 {p.LostCost} 个半边" : "") + ((p.GoalValue > 0) ? $"，★ 顺手捡到（战役目标 +{p.GoalValue}）" : "") + ((p.SelfHazardDamage > 0) ? $"，⚠ 移动会吃 {p.SelfHazardDamage} 点危险地形伤害（已计入评分）" : "") + ((p.IncomingDamage > 0) ? $"，⚠ **这回合会挨打 {p.IncomingDamage} 点**（先攻比你低的敌人能打到你，已计入评分）" : "") + ((p.IncomingConditions != null && p.IncomingConditions.Count > 0) ? ("，⚠ 敌人会给你上 **" + string.Join("、", p.IncomingConditions) + "**（缴械=不能攻击 / 静足=不能移动）") : "") + (p.WouldDie ? "，☠ **这回合打完你会死**" : "") + ((p.ShadowGain > 0) ? $"，★ 造出 {p.ShadowGain} 个阴影（抬高后续「深渊力量」，位置见下方说明）" : "") + effNote);
		list.Add($"评分 {p.Score:F1}");
		DescribeHalf(list, ctx, h1, 1, ctx.StartX, ctx.StartY, p.Attacks.FirstOrDefault((AttackChoice a) => a.CardIndex == h1.CardIndex && a.Source == h1.Source));
		DescribeHalf(list, ctx, h2, 2, h1.ToX, h1.ToY, p.Attacks.FirstOrDefault((AttackChoice a) => a.CardIndex == h2.CardIndex && a.Source == h2.Source));
		if (!string.IsNullOrEmpty(p.Note))
		{
			list.Add("⚠ " + p.Note);
		}
		return list;
	}

	private static void DescribeHalf(List<string> outLines, P1Context ctx, PlanHalf step, int index, int fromX, int fromY, AttackChoice atk)
	{
		ActionSnapshot half = step.Half;
		outLines.Add("");
		outLines.Add($"第{index}步 —— {step.HowToPlay}{LostTag(step)}");
		if (half == null)
		{
			outLines.Add("  （无内容）");
			return;
		}
		if (half.IsPersistentBonus)
		{
			string text = ((half.BonusTexts.Count > 0) ? string.Join("，", half.BonusTexts) : string.Join("/", half.PersistentBehaviours));
			outLines.Add("  这是**常驻加成**，不是本回合能执行的动作：" + text);
			outLines.Add("  （它会在你打出这张牌时自动生效，不能拿来当移动或攻击去执行）");
			return;
		}
		if (half.UnmodeledTypes != null && half.UnmodeledTypes.Count > 0)
		{
			outLines.Add("  ⚠ 本步还有**未建模**的效果：" + string.Join("、", half.UnmodeledTypes) + " —— 实际打出来会比上面写的多，评分也没把它们算进去");
		}
		if (half.HasCreateShadow)
		{
			int createShadowCount = half.CreateShadowCount;
			int num = 5 - ctx.ShadowCount;
			if (num <= 0)
			{
				outLines.Add($"  ⚠ 想造 {createShadowCount} 个阴影，但已达上限 5 —— 造不了（先移除旧的）");
			}
			else
			{
				int num2 = ((createShadowCount > num) ? num : createShadowCount);
				outLines.Add($"  ★ 本步会造 {num2} 个阴影 —— **位置由你自己选**，" + $"当前场上 {ctx.ShadowCount}/5。");
				outLines.Add("     选址建议：放在你**下一步要站的格子**或**下一步要打到的敌人旁**（「暗影步法」这类牌必须先踩在阴影上才能用）。");
			}
		}
		if (half.HasMove && half.MoveStrength > 0)
		{
			int num3 = HexDist(fromX, fromY, step.ToX, step.ToY);
			if (num3 <= 0)
			{
				outLines.Add($"  移动：站在原地不动（{fromX},{fromY}），本回合不需要换位置");
			}
			else
			{
				string text2 = "";
				if (atk != null)
				{
					int num4 = HexDist(step.ToX, step.ToY, atk.TargetX, atk.TargetY);
					text2 = $"，站到能打到「{atk.TargetName}」的位置（届时与它相隔 {num4} 格）";
				}
				outLines.Add($"  移动：走到 ({step.ToX},{step.ToY})，共 {num3} 格{text2}");
			}
		}
		else
		{
			outLines.Add($"  移动：本步没有移动，位置不变（仍在 {fromX},{fromY}）");
		}
		if (!half.HasAttack)
		{
			outLines.Add("  攻击：本步没有攻击");
		}
		else if (half.IsUnsupportedAttack)
		{
			string text3 = (string.IsNullOrEmpty(half.AreaEffectShape) ? "" : ("（形状 " + half.AreaEffectShape + "）"));
			outLines.Add("  攻击：⚠ 这是**范围/多目标攻击**" + text3 + " —— 要先选焦点格、按形状摆站位，一次能打好几个，单体伤害模型算不准，当前版本未建模，请自行判断");
		}
		else if (atk != null)
		{
			string text4 = ((atk.TargetMaxHealth > 0) ? $"{atk.TargetRawHealth}/{atk.TargetMaxHealth}" : atk.TargetRawHealth.ToString());
			outLines.Add($"  攻击：「{atk.TargetName}」@({atk.TargetX},{atk.TargetY})" + "（血量 " + text4 + ((atk.TargetRawHealth != atk.TargetHealth) ? $"，先动单位预计打 {atk.TargetRawHealth - atk.TargetHealth} → 到你时约 {atk.TargetHealth}" : "") + ((atk.TargetShield > 0) ? $"，护盾{atk.TargetShield}" : "") + "）" + string.Format(" → 预计 {0} 点伤害{1}", atk.Damage, atk.Kills ? "【击杀】" : "") + (atk.AdvantageApprox ? "（优/劣势下为近似值）" : ""));
			outLines.Add("        会翻出：" + string.Join(" 、 ", atk.Mods) + $"（从牌堆顶拿走 {atk.CardsConsumed} 张）");
		}
		else
		{
			outLines.Add($"  攻击：本步有攻击，但站在 ({step.ToX},{step.ToY}) 谁也打不到");
		}
	}

	private static string LostTag(PlanHalf step)
	{
		return step.IsLost ? "  ⚠这一半打完会永久失去" : "";
	}

	private static int HexDist(int x1, int y1, int x2, int y2)
	{
		int num = x1 - (y1 - (y1 & 1)) / 2;
		int num2 = x2 - (y2 - (y2 & 1)) / 2;
		return (Math.Abs(num - num2) + Math.Abs(num + y1 - num2 - y2) + Math.Abs(y1 - y2)) / 2;
	}

	private static List<UiSection> ToUiSections(List<List<string>> blocks, List<UnitSnapshot> order, GoalSnapshot goals, List<EnemyPredictor.Prediction> enemyPreds, List<PlayerRun> runs, Dictionary<string, MultiTurnResult> mtResults = null)
	{
		List<UiSection> list = new List<UiSection>();
		foreach (List<string> block in blocks)
		{
			UiSection uiSection = new UiSection("角色");
			bool flag = true;
			int num = 0;
			foreach (string item in block)
			{
				string text = item.TrimEnd();
				if (text.Length == 0)
				{
					continue;
				}
				if (flag)
				{
					Match match = Regex.Match(text, "=====\\s*\\[\\d+\\]\\s*([^|]+?)\\s*\\|");
					if (match.Success)
					{
						uiSection.Name = match.Groups[1].Value.Trim();
						num++;
					}
					flag = false;
				}
				uiSection.Lines.Add(Classify(text));
			}
			if (uiSection.Lines.Count == 0)
			{
				continue;
			}
			if (num == 0)
			{
				string text2 = uiSection.Lines[0].Text ?? "";
				text2 = text2.Replace("=====", "").Trim();
				ref string name = ref uiSection.Name;
				string text3;
				if (text2.Length > 8)
				{
					text3 = text2.Substring(0, 8);
				}
				else
				{
					text3 = ((text2.Length > 0) ? text2 : "角色");
				}
				name = text3;
				if (uiSection.Name.Length == 0)
				{
					uiSection.Name = "角色";
				}
			}
			uiSection.Name = UiSection.UniqueName(list.Select((UiSection x) => x.Name), uiSection.Name);
			list.Add(uiSection);
		}
		UiSection uiSection2 = new UiSection("敌人");
		uiSection2.Add("先攻顺序（小的先动）", UiStyle.Head);
		foreach (UnitSnapshot item2 in order)
		{
			if (!item2.IsHostileToPlayer)
			{
				continue;
			}
			string text4 = "";
			if (item2.Intent != null)
			{
				List<string> list2 = new List<string>();
				if (item2.Intent.HasMove)
				{
					list2.Add("移动" + item2.Intent.MoveStrength);
				}
				if (item2.Intent.HasAttack)
				{
					list2.Add("攻" + item2.Intent.AttackStrength + "射程" + item2.Intent.AttackRange);
				}
				if (list2.Count > 0)
				{
					text4 = "  意图 " + string.Join("、", list2);
				}
			}
			uiSection2.Add($"  先攻{item2.Initiative}  {item2.DisplayName} @({item2.X},{item2.Y})" + $" 血 {item2.Health}/{item2.MaxHealth}" + text4);
		}
		if (enemyPreds != null && enemyPreds.Count > 0)
		{
			uiSection2.Add("会先动、会让出原格的敌人", UiStyle.Head);
			foreach (EnemyPredictor.Prediction enemyPred in enemyPreds)
			{
				uiSection2.Add($"  {enemyPred.DisplayName} 先攻{enemyPred.Initiative}" + $" ({enemyPred.FromX},{enemyPred.FromY}) → 约 ({enemyPred.ToX},{enemyPred.ToY})", UiStyle.Warn);
			}
		}
		if (uiSection2.Lines.Count > 1)
		{
			list.Add(uiSection2);
		}
		if (goals != null)
		{
			UiSection uiSection3 = new UiSection("战利品");
			if (goals.Objectives.Count > 0)
			{
				uiSection3.Add("使命目标", UiStyle.Head);
				foreach (ObjectiveSnapshot objective in goals.Objectives)
				{
					if (!objective.IsHidden)
					{
						uiSection3.Add("  " + objective, objective.IsComplete ? UiStyle.Dim : UiStyle.Warn);
					}
				}
				if (goals.AnyWinsDespiteExhaustion)
				{
					uiSection3.Add("  ★ 力竭仍算赢 —— 可以放心打光手牌", UiStyle.Good);
				}
			}
			if (goals.LootSpots.Count > 0)
			{
				uiSection3.Add("地上可捡拾（终点踩上去即捡到）", UiStyle.Head);
				foreach (LootSpot lootSpot in goals.LootSpots)
				{
					uiSection3.Add($"  {lootSpot}", UiStyle.Good);
				}
				if (goals.HasLootDeck)
				{
					uiSection3.Add("  下一张战利品：" + goals.NextLootName + (goals.NextLootIsItem ? "（= 装备，优先）" : "（材料）"));
				}
			}
			if (uiSection3.Lines.Count > 0)
			{
				list.Add(uiSection3);
			}
		}
		UiSection uiSection4 = new UiSection("日志");
		uiSection4.Add("报告原文（与 p1_recommend.txt 一致）", UiStyle.Head);
		foreach (List<string> block2 in blocks)
		{
			foreach (string item3 in block2)
			{
				string text5 = item3.TrimEnd();
				if (text5.Length != 0)
				{
					uiSection4.Add(text5, UiStyle.Dim);
				}
			}
		}
		list.Add(uiSection4);
		UiSection uiSection5 = BuildMultiTurnSection(runs, mtResults);
		if (uiSection5 != null && uiSection5.Lines.Count > 0)
		{
			list.Add(uiSection5);
		}
		UiSection uiSection6 = new UiSection("报错");
		uiSection6.Add("本次求解的告警 / 未建模项汇总", UiStyle.Head);
		int num2 = 0;
		foreach (List<string> block3 in blocks)
		{
			foreach (string item4 in block3)
			{
				string text6 = item4.TrimEnd();
				if (text6.Contains("未建模") || text6.StartsWith("⚠"))
				{
					uiSection6.Add("  " + text6, UiStyle.Warn);
					num2++;
				}
			}
		}
		if (num2 == 0)
		{
			uiSection6.Add("  （本次求解无告警）", UiStyle.Dim);
		}
		uiSection6.Add("【本版本未建模的机制】（会低估或高估收益，请自行判断）", UiStyle.Head);
		string[] unmodeledMechanisms = UnmodeledMechanisms;
		foreach (string text7 in unmodeledMechanisms)
		{
			uiSection6.Add("  · " + text7, UiStyle.Dim);
		}
		list.Add(uiSection6);
		return list;
	}

	private static List<int> MyInitiativeOptions(P1Context ctx, int fallback)
	{
		List<int> list = new List<int>();
		try
		{
			if (ctx != null && ctx.Hand != null)
			{
				foreach (AbilityCardSnapshot item in ctx.Hand)
				{
					if (item != null && item.Initiative > 0 && !list.Contains(item.Initiative))
					{
						list.Add(item.Initiative);
					}
				}
			}
			if (fallback > 0 && !list.Contains(fallback))
			{
				list.Add(fallback);
			}
		}
		catch
		{
		}
		return list;
	}

	private static Dictionary<int, P1Context.ThreatInfo> BuildThreatByInitiative(List<EnemyPredictor.Prediction> enemyPreds, List<int> myInitiatives)
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, P1Context.ThreatInfo> dictionary = new Dictionary<int, P1Context.ThreatInfo>();
		if (enemyPreds == null || myInitiatives == null)
		{
			return dictionary;
		}
		try
		{
			foreach (int myInitiative in myInitiatives)
			{
				if (myInitiative <= 0 || dictionary.ContainsKey(myInitiative))
				{
					continue;
				}
				P1Context.ThreatInfo threatInfo = new P1Context.ThreatInfo();
				foreach (EnemyPredictor.Prediction enemyPred in enemyPreds)
				{
					if (enemyPred == null || !enemyPred.HasAttack || enemyPred.AttackStrength <= 0 || (enemyPred.Initiative > 0 && enemyPred.Initiative >= myInitiative))
					{
						continue;
					}
					int num = ((enemyPred.AttackRange <= 0) ? 1 : enemyPred.AttackRange);
					for (int i = -num; i <= num; i++)
					{
						for (int j = -num - 1; j <= num + 1; j++)
						{
							int num2 = enemyPred.ToX + j;
							int num3 = enemyPred.ToY + i;
							if (num2 < 0 || num3 < 0 || Point.HexDist(new Point(enemyPred.ToX, enemyPred.ToY), new Point(num2, num3)) > num)
							{
								continue;
							}
							try
							{
								if (!Geometry.HasLOS(enemyPred.ToX, enemyPred.ToY, num2, num3))
								{
									continue;
								}
							}
							catch
							{
							}
							(int, int) key = (num2, num3);
							threatInfo.DamagedCells.TryGetValue(key, out var value);
							threatInfo.DamagedCells[key] = value + enemyPred.AttackStrength;
						}
					}
					if (enemyPred.AppliesNegative == null)
					{
						continue;
					}
					foreach (string item in enemyPred.AppliesNegative)
					{
						if (!threatInfo.AppliedConditions.Contains(item))
						{
							threatInfo.AppliedConditions.Add(item);
						}
					}
				}
				dictionary[myInitiative] = threatInfo;
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = Plugin.Log;
			if (log != null)
			{
				log.LogWarning((object)("[FHSolver] 威胁计算失败: " + ex.Message));
			}
		}
		return dictionary;
	}

	/// <summary>
	/// 跑一次多回合推演（供角色页与多回合页共用，避免算两遍）。
	///
	/// 用**束搜索**：第 1 回合完整枚举 → 执行 → 第 2 回合再枚举 ……
	/// 敌人的寻路与修正牌抽牌都是**真实模拟**的。
	/// </summary>
	private static MultiTurnResult ComputeMultiTurn(PlayerRun r)

	{
		if (r == null || !r.Ok || r.Ctx == null)
		{
			return null;
		}
		if (r.Plans == null || r.Plans.Count == 0)
		{
			return null;
		}
		if (!Plugin.StaticMultiTurnEnabled)
		{
			return null;
		}
		try
		{
			int num = Plugin.StaticMultiTurnDepth;
			int num2 = Plugin.StaticMultiTurnBeam;
			if (num < 1)
			{
				num = 1;
			}
			if (num2 < 1)
			{
				num2 = 1;
			}
			return MultiTurnSearch.Search(r.Ctx, r.Plans, num, num2, r.Initiative, new MultiTurnSearch.EvalWeights());
		}
		catch (Exception ex)
		{
			ManualLogSource log = Plugin.Log;
			if (log != null)
			{
				log.LogWarning((object)("[FHSolver] 多回合推演失败（" + r?.Name + "）: " + ex.Message));
			}
			return null;
		}
	}

	/// <summary>
	/// 把**第 2 回合及以后**的推演写成文本，**跟在角色方案后面**。
	///
	/// 参考 StS2 的 Combat Solver：路线按回合连续展示，
	/// 玩家看当前回合时就能看到\"接下来会怎样\"，而不是翻到别的页签。
	/// </summary>
	/// <summary>
	/// **某一回合的详细方案**（用户要求：多回合也要有详细方案，不是一句话）。
	///
	/// 展开：出哪两张牌的哪半 → 落点/血量 → 每次攻击的目标与伤害与翻的牌
	///      → 治疗 → 挨打 → 累计 → 敌人这一回合的��实行动。
	/// </summary>
	private static List<string> MultiTurnDetailedLines(RouteStep st)
	{
		List<string> lines = new List<string>();
		if (st == null)
		{
			return lines;
		}

		TurnPlan p = st.Plan;
		if (p != null)
		{
			lines.Add($"出牌：{p.First?.CardName ?? "?"}【{p.First?.SideLabel ?? "?"}】"
					  + $" + {p.Second?.CardName ?? "?"}【{p.Second?.SideLabel ?? "?"}】");
			lines.Add($"落点：({st.MyX},{st.MyY})    血量：{st.MyHealth}");

			if (p.Attacks != null && p.Attacks.Count > 0)
			{
				lines.Add("攻击：");
				foreach (AttackChoice a in p.Attacks)
				{
					if (a == null) continue;
					lines.Add($"    → {a.TargetName}  伤 {a.Damage}"
							  + (a.Kills ? "  ★击杀" : "")
							  + (a.Mods != null && a.Mods.Count > 0 ? $"  翻牌[{string.Join(" ", a.Mods)}]" : "")
							  + (string.IsNullOrEmpty(a.Expr) ? "" : $"  {a.Expr}"));

				}
			}
			if (p.HealValue > 0)
			{
				lines.Add($"治疗：+{p.HealValue}");
			}
			if (p.SelfHazardDamage > 0)
			{
				lines.Add($"⚠ 移动吃了 {p.SelfHazardDamage} 点危险地形伤害");
			}
		}

		if (st.IncomingDamage > 0)
		{
			lines.Add($"⚠ 这回合挨打 {st.IncomingDamage} 点（剩 {st.MyHealth} 血）");
		}
		if (st.Died)
		{
			lines.Add("☠ 阵亡");
		}
		lines.Add($"累计：伤 {st.CumulativeDamage}  杀 {st.CumulativeKills}");

		if (st.EnemyStrikes != null && st.EnemyStrikes.Count > 0)
		{
			lines.Add("敌人这一回合的行动：");
			foreach (EnemyStrike es in st.EnemyStrikes)
			{
				if (es == null) continue;
				lines.Add($"    {es.AttackerName}  伤 {es.Damage}"
						  + (string.IsNullOrEmpty(es.CardsText) ? "" : $"  翻牌[{es.CardsText}]")
						  + (es.ApproachOnly ? "（只是逼近，没出手）" : "")
						  + (es.Approx ? "  ⚠估算" : ""));
			}
		}
		return lines;
	}

	/// <summary>
	/// **一回合一个页签**（用户要求：翻页功能）。
	///
	/// 每个角色 × 每个后续回合 = 一页，页签名形如「第2回合方案·旗矛」。
	/// 页名冲突由 UiSection.UniqueName 处理。
	/// </summary>
	private static List<UiSection> BuildRoundSections(
		List<PlayerRun> runs, Dictionary<string, MultiTurnResult> mtResults)
	{
		List<UiSection> sections = new List<UiSection>();
		if (mtResults == null || runs == null) return sections;

		foreach (PlayerRun r in runs)
		{
			if (r == null || r.Ctx == null) continue;
			MultiTurnResult res;
			if (!mtResults.TryGetValue(r.Name, out res)) continue;
			if (res.Routes == null || res.Routes.Count == 0) continue;
			List<RouteStep> route = res.Routes[0];
			if (route == null) continue;

			foreach (RouteStep st in route)
			{
				if (st == null || st.Round < 2) continue;

				UiSection sec = new UiSection($"第{st.Round}回合方案·{r.Name}");
				sec.Add($"—— 第 {st.Round} 回合（沿第 1 回合最佳方案继续）——", UiStyle.Head);
				sec.Add("先攻 " + r.Initiative, UiStyle.Sub);
				foreach (string line in MultiTurnDetailedLines(st)) sec.Add(line);
				sections.Add(sec);
			}
		}
		return sections;
	}

	private static List<string> MultiTurnFollowUpLines(MultiTurnResult res)

	{
		List<string> list = new List<string>();
		if (res == null || res.Routes == null || res.Routes.Count == 0)
		{
			return list;
		}
		List<RouteStep> list2 = res.Routes[0];
		if (list2 == null)
		{
			return list;
		}
		List<RouteStep> list3 = new List<RouteStep>();
		foreach (RouteStep item in list2)
		{
			if (item != null && item.Round >= 2)
			{
				list3.Add(item);
			}
		}
		if (list3.Count == 0)
		{
			return list;
		}
		list.Add("—— 后续回合推演 —— 每回合的**详细方案**见独立页签「第N回合方案·角色名」——");

		foreach (RouteStep item2 in list3)
		{
			TurnPlan plan = item2.Plan;
			string arg = ((plan != null) ? ((plan.First?.CardName ?? "?") + "【" + (plan.First?.SideLabel ?? "?") + "】+" + (plan.Second?.CardName ?? "?") + "【" + (plan.Second?.SideLabel ?? "?") + "】") : "(无)");
			string text = $"第{item2.Round}回合：{arg}" + $" → 站({item2.MyX},{item2.MyY}) 血{item2.MyHealth}" + $" 累计伤{item2.CumulativeDamage} 杀{item2.CumulativeKills}";
			if (item2.IncomingDamage > 0)
			{
				text += $" ⚠会挨打-{item2.IncomingDamage}";
			}
			if (item2.Died)
			{
				text += " ☠ 阵亡";
			}
			list.Add(text);
			if (item2.EnemyStrikes == null)
			{
				continue;
			}
			foreach (EnemyStrike enemyStrike in item2.EnemyStrikes)
			{
				list.Add($"    敌人：{enemyStrike.AttackerName} 伤{enemyStrike.Damage}" + (string.IsNullOrEmpty(enemyStrike.CardsText) ? "" : (" 翻牌[" + enemyStrike.CardsText + "]")) + (enemyStrike.ApproachOnly ? "（只是逼近，没出手）" : "") + (enemyStrike.Approx ? " ⚠估算" : ""));
			}
		}
		return list;
	}

	private static UiSection BuildMultiTurnSection(List<PlayerRun> runs, Dictionary<string, MultiTurnResult> mtResults = null)
	{
		UiSection uiSection = new UiSection("多回合");
		if (runs == null || runs.Count == 0)
		{
			return uiSection;
		}
		if (!Plugin.StaticMultiTurnEnabled)
		{
			return uiSection;
		}
		int num = Plugin.StaticMultiTurnDepth;
		int num2 = Plugin.StaticMultiTurnBeam;
		if (num < 1)
		{
			num = 1;
		}
		if (num2 < 1)
		{
			num2 = 1;
		}
		uiSection.Add($"束搜索：深度 {num} 回合，每层保留 {num2} 个分支", UiStyle.Head);
		uiSection.Add("✓ 敌人回合已接**真实修正牌抽牌**：伤害 = 意图攻击力代入它确切会翻出的那张牌\n      （修正牌堆是确定序列，实测 354/354 全中，不是期望值）。", UiStyle.Good);
		uiSection.Add("✓ 敌人**寻路与抽牌都是真实模拟的**：会真的去找一个能打到你的落点，\n      考虑地形（困难地形多耗移动力）、占用（别的单位挡路）、视线（墙）。\n      **够不着时它照样会逼近**（真实 AI 的 fallback），但那回合不出手、不抽牌。", UiStyle.Good);
		uiSection.Add("⚠ 仍保留的简化：贴近目标取\"离焦点最近的可达格\"（真实是\"离最佳攻击位最近\"，绝大多数场景等价）；\n      队友回合不推进；不模拟敌人对我方的优劣势。", UiStyle.Warn);
		bool flag = false;
		foreach (PlayerRun run in runs)
		{
			if (run == null || !run.Ok || run.Ctx == null || run.Plans == null || run.Plans.Count == 0)
			{
				continue;
			}
			MultiTurnResult value = null;
			mtResults?.TryGetValue(run.Name, out value);
			if (value == null)
			{
				try
				{
					value = MultiTurnSearch.Search(run.Ctx, run.Plans, num, num2, run.Initiative, new MultiTurnSearch.EvalWeights());
				}
				catch (Exception ex)
				{
					uiSection.Add(run.Name + "：推演失败（" + ex.Message + "）", UiStyle.Warn);
					continue;
				}
			}
			if (value.Routes.Count == 0)
			{
				continue;
			}
			flag = true;
			uiSection.Add($"{run.Name}（先攻 {run.Initiative}）—— " + $"最优路线价值 {value.BestValue:F0}，共 {value.Routes.Count} 条候选", UiStyle.Sub);
			int num3 = Math.Min(value.Routes.Count, 3);
			for (int i = 0; i < num3; i++)
			{
				List<RouteStep> list = value.Routes[i];
				uiSection.Add($"  路线 {i + 1}", UiStyle.Good);
				foreach (RouteStep item in list)
				{
					TurnPlan plan = item.Plan;
					string arg = ((plan != null) ? ((plan.First?.CardName ?? "?") + "【" + (plan.First?.SideLabel ?? "?") + "】+" + (plan.Second?.CardName ?? "?") + "【" + (plan.Second?.SideLabel ?? "?") + "】") : "(无)");
					uiSection.Add($"    第{item.Round}回合：{arg}" + $" → 站({item.MyX},{item.MyY}) 血{item.MyHealth}" + $" 累计伤{item.CumulativeDamage} 杀{item.CumulativeKills}" + ((item.IncomingDamage > 0) ? $" 挨打-{item.IncomingDamage}" : "") + (item.Died ? "  ☠ 阵亡" : ""), item.Died ? UiStyle.Warn : UiStyle.Normal);
					if (item.EnemyStrikes == null)
					{
						continue;
					}
					foreach (EnemyStrike enemyStrike in item.EnemyStrikes)
					{
						if (enemyStrike != null)
						{
							uiSection.Add(enemyStrike.ApproachOnly ? $"       {enemyStrike}" : $"       挨打：{enemyStrike}", (enemyStrike.ApproachOnly || enemyStrike.Approx) ? UiStyle.Warn : UiStyle.Dim);
						}
					}
				}
			}
		}
		if (!flag)
		{
			uiSection.Add("（本轮没有可推演的角色）", UiStyle.Dim);
		}
		return uiSection;
	}

	private static List<string> StateWarnings(List<string> conds)
	{
		List<string> list = new List<string>();
		if (conds == null)
		{
			return list;
		}
		foreach (string cond in conds)
		{
			switch (cond)
			{
			case "Disarm":
				list.Add("⚠ **缴械** —— 攻击类动作用不了；改用移动/治疗/辅助，或直接打基础移动 2");
				break;
			case "Immobilize":
				list.Add("⚠ **静足** —— 不能移动，只能用原地就能做的动作");
				break;
			case "Stun":
				list.Add("⚠ **眩晕** —— 这一回合基本什么都做不了");
				break;
			case "Disadvantage":
				list.Add("⚠ **劣势** —— 抽 2 张取坏，伤害会低于期望");
				break;
			case "Muddle":
				list.Add("⚠ **混乱** —— 攻击被削弱");
				break;
			case "Poison":
				list.Add("⚠ **中毒** —— 回合末掉血，优先处理");
				break;
			case "Wound":
				list.Add("⚠ **流血** —— 回合末掉血");
				break;
			case "Strengthen":
				list.Add("★ **强化** —— 攻击 +1，已算进评分");
				break;
			case "Advantage":
				list.Add("★ **优势** —— 抽 2 张取好");
				break;
			case "Ward":
				list.Add("★ **守护** —— 免疫下一次负面效果");
				break;
			case "Regenerate":
				list.Add("★ **再生** —— 回合末回血");
				break;
			}
		}
		return list;
	}

	private static UiLine Classify(string line)
	{
		if (line.StartsWith("=====") || line.StartsWith("-----"))
		{
			return new UiLine(line, UiStyle.Sub);
		}
		if (line.Contains("【") && line.Contains("】"))
		{
			return new UiLine(line, UiStyle.Head);
		}
		if (line.StartsWith("出牌：") || line.Contains("预期：") || line.StartsWith("次序："))
		{
			return new UiLine(line, UiStyle.Good);
		}
		if (line.StartsWith("⚠") || line.Contains("未建模") || line.Contains("会先动") || line.Contains("永久失去") || line.Contains("无法"))
		{
			return new UiLine(line, UiStyle.Warn);
		}
		if (line.Length > 2 && line[0] == '[' && line[1] >= '0' && line[1] <= '9')
		{
			return new UiLine(line, UiStyle.Sub);
		}
		if (line.Contains("评分") || line.Contains("枚举耗时") || line.Contains("调试信息"))
		{
			return new UiLine(line, UiStyle.Dim);
		}
		return new UiLine(line);
	}

	private static void WriteReportFile(List<string> lines)
	{
		try
		{
			string staticReportPath = Plugin.StaticReportPath;
			if (string.IsNullOrEmpty(staticReportPath))
			{
				return;
			}
			string directoryName = Path.GetDirectoryName(staticReportPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"# FHSolver P1 推荐  {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			stringBuilder.AppendLine();
			foreach (string line in lines)
			{
				stringBuilder.AppendLine(line);
			}
			File.WriteAllText(staticReportPath, stringBuilder.ToString(), Encoding.UTF8);
			ManualLogSource log = Plugin.Log;
			if (log != null)
			{
				log.LogInfo((object)("[FHSolver] 推荐已写入: " + staticReportPath));
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log2 = Plugin.Log;
			if (log2 != null)
			{
				log2.LogWarning((object)("[FHSolver] 写推荐文件失败: " + ex.Message));
			}
		}
	}
}
