using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class CAbilityRevive : CAbility
{
	public enum EReviveState
	{
		SelectGraves,
		Revive,
		ReviveDone
	}

	private EReviveState m_State;

	private bool m_Summoned;

	public CActor SummonedActor { get; set; }

	[JsonProperty]
	public List<string> SummonIDs { get; set; }

	public string SelectedSummonID
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState == null)
			{
				return SummonIDs[0];
			}
			return SummonIDs[ScenarioManager.CurrentScenarioState.Players.Count - 1];
		}
	}

	public MonsterYMLData SelectedMonsterYMLData => ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == SelectedSummonID);

	public string SelectedLocKey => SelectedMonsterYMLData.LocKey;

	public CAbilityRevive(List<string> summons)
	{
		if (summons.Count != 4)
		{
			throw new Exception("Invalid size for summons list.  Must be exactly 4.  Current size: " + summons.Count);
		}
		SummonIDs = summons;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EReviveState.SelectGraves;
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_Summoned = false;
		if (SelectedSummonID == "Empty")
		{
			SetCancelAbilityFlag(cancel: true);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0089;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0089;
				}
			}
		}
		switch (m_State)
		{
		case EReviveState.SelectGraves:
			if (AreaEffect == null)
			{
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: true);
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
				if (!TilesInRange.Contains(adjacentTile))
				{
					TilesInRange.Add(adjacentTile);
				}
				List<CTile> list3 = new List<CTile>();
				foreach (CTile item in TilesInRange)
				{
					using List<CObjectProp>.Enumerator enumerator3 = item.FindProps(ScenarioManager.ObjectImportType.MonsterGrave).GetEnumerator();
					if (enumerator3.MoveNext())
					{
						_ = enumerator3.Current;
						list3.Add(item);
					}
				}
				TilesInRange = list3.ToList();
			}
			if (TargetingActor.Type != CActor.EType.Player)
			{
				if (TilesInRange.Count > 0)
				{
					if (TargetingActor.AIMoveFocusTiles.Count != 0)
					{
						List<CAIMoveNew.CActorPath> list4 = new List<CAIMoveNew.CActorPath>();
						foreach (Point aIMoveFocusTile in TargetingActor.AIMoveFocusTiles)
						{
							CAIMoveNew.CActorPath cActorPath2 = new CAIMoveNew.CActorPath();
							CTile cTile2 = ScenarioManager.Tiles[aIMoveFocusTile.X, aIMoveFocusTile.Y];
							if ((CObjectMonsterGrave)cTile2.FindProp(ScenarioManager.ObjectImportType.MonsterGrave) != null)
							{
								cActorPath2.m_Tile = cTile2;
								cActorPath2.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(TargetingActor, TargetingActor.ArrayIndex, cTile2, out cActorPath2.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestination: true, ignoreMoveCost: true, out var foundPath2, shouldPathThroughDoors: false);
								if (foundPath2)
								{
									list4.Add(cActorPath2);
								}
							}
						}
						if (list4.Count == 0)
						{
							foreach (CTile item2 in TilesInRange)
							{
								CAIMoveNew.CActorPath cActorPath3 = new CAIMoveNew.CActorPath();
								cActorPath3.m_Tile = item2;
								CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
								bool foundPath3;
								if (adjacentTile2 == item2)
								{
									foundPath3 = true;
									cActorPath3.m_ArrayIndices = new List<Point> { adjacentTile2.m_ArrayIndex };
								}
								else
								{
									cActorPath3.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(TargetingActor, item2.m_ArrayIndex, ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y], out cActorPath3.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestination: true, ignoreMoveCost: true, out foundPath3, shouldPathThroughDoors: false);
								}
								if (foundPath3)
								{
									list4.Add(cActorPath3);
								}
							}
						}
						list4.Sort((CAIMoveNew.CActorPath x, CAIMoveNew.CActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
						if (m_NumberTargetsRemaining == -1)
						{
							m_NumberTargetsRemaining = TilesInRange.Count;
						}
						if (AllTargets)
						{
							m_TilesSelected.AddRange(list4.Select((CAIMoveNew.CActorPath x) => x.m_Tile));
						}
						else
						{
							for (int num2 = 0; num2 < m_NumberTargetsRemaining; num2++)
							{
								if (list4.Count > 0 && list4.Count > num2)
								{
									m_TilesSelected.Add(list4[num2].m_Tile);
								}
								else if (TilesInRange.Count > num2)
								{
									m_TilesSelected.Add(TilesInRange[num2]);
									TilesInRange.RemoveAt(num2);
								}
							}
						}
					}
					else if (AllTargets)
					{
						m_TilesSelected.AddRange(TilesInRange);
					}
					else
					{
						for (int num3 = 0; num3 < m_NumberTargetsRemaining; num3++)
						{
							if (TilesInRange.Count > num3)
							{
								m_TilesSelected.Add(TilesInRange[num3]);
							}
						}
					}
					PhaseManager.StepComplete();
				}
				else
				{
					PhaseManager.NextStep();
				}
			}
			else
			{
				Log.ErrorWithReport("Revive ability not supported for players");
			}
			break;
		case EReviveState.Revive:
		{
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count > 0)
			{
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				AbilityHasHappened = true;
				CRevive_MessageData message = new CRevive_MessageData(AnimOverload, TargetingActor)
				{
					m_ActorReviving = TargetingActor,
					m_ReviveAbility = this,
					m_ReviveTiles = m_TilesSelected.ToList()
				};
				ScenarioRuleClient.MessageHandler(message);
				m_Summoned = true;
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
				PhaseManager.StepComplete();
			}
			CMonsterClass cMonsterClass = null;
			foreach (CTile item3 in m_TilesSelected)
			{
				CObjectMonsterGrave cObjectMonsterGrave = (CObjectMonsterGrave)item3.FindProp(ScenarioManager.ObjectImportType.MonsterGrave);
				cMonsterClass = MonsterClassManager.Find(SelectedMonsterYMLData.ID);
				if (!cObjectMonsterGrave.IsEliteGrave && cMonsterClass != null)
				{
					cMonsterClass = cMonsterClass.NonEliteVariant;
				}
				if (cMonsterClass == null)
				{
					continue;
				}
				CTile cTile = null;
				if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, null, null, VarsContext))
				{
					cTile = item3;
				}
				else
				{
					for (int i = 0; i < 5; i++)
					{
						List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(item3, i + 1);
						allUnblockedTilesFromOrigin.Remove(item3);
						List<CTile> list = new List<CTile>();
						foreach (CTile item4 in allUnblockedTilesFromOrigin)
						{
							if (CAbilityFilter.IsValidTile(item4, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, null, null, VarsContext))
							{
								list.Add(item4);
							}
						}
						if (list.Count <= 0)
						{
							continue;
						}
						if (TargetingActor.AIMoveFocusActors.Count > 0)
						{
							List<CAIMoveNew.CActorPath> list2 = new List<CAIMoveNew.CActorPath>();
							foreach (CTile item5 in list)
							{
								CAIMoveNew.CActorPath cActorPath = new CAIMoveNew.CActorPath();
								cActorPath.m_Tile = item5;
								bool foundPath;
								if (TargetingActor.AIMoveFocusActors[0] != null)
								{
									cActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(TargetingActor, item5.m_ArrayIndex, ScenarioManager.Tiles[TargetingActor.AIMoveFocusActors[0].ArrayIndex.X, TargetingActor.AIMoveFocusActors[0].ArrayIndex.Y], out cActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestination: true, ignoreMoveCost: true, out foundPath, shouldPathThroughDoors: false);
								}
								else
								{
									cActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(TargetingActor, item5.m_ArrayIndex, ScenarioManager.Tiles[TargetingActor.AIMoveFocusTiles[0].X, TargetingActor.AIMoveFocusTiles[0].Y], out cActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestination: true, ignoreMoveCost: true, out foundPath, shouldPathThroughDoors: false);
								}
								if (foundPath)
								{
									list2.Add(cActorPath);
								}
							}
							list2.Sort((CAIMoveNew.CActorPath x, CAIMoveNew.CActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
							cTile = list2[0].m_Tile;
						}
						else
						{
							cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
						}
						break;
					}
				}
				if (cTile == null)
				{
					continue;
				}
				int num = cMonsterClass.Health();
				int maxHealth = num;
				if (StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == EAbilityStatType.Strength))
				{
					int strength = Strength;
					SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
					num = Math.Min(Strength, num);
					Strength = strength;
				}
				CEnemyActor cEnemyActor = null;
				if (cMonsterClass is CObjectClass)
				{
					int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
					ObjectState objectState = new ObjectState(cMonsterClass.ID, chosenModelIndex, null, cTile.m_HexMap.MapGuid, new TileIndex(cTile.m_ArrayIndex), num, maxHealth, TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, TargetingActor.ActorGuid, 1, TargetingActor.OriginalType);
					cEnemyActor = ScenarioManager.Scenario.AddObject(objectState, initial: false);
					if (cEnemyActor != null)
					{
						ScenarioManager.CurrentScenarioState.Objects.Add(objectState);
					}
					else
					{
						m_Summoned = false;
					}
				}
				else
				{
					int chosenModelIndex2 = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
					EnemyState enemyState = new EnemyState(cMonsterClass.ID, chosenModelIndex2, null, cTile.m_HexMap.MapGuid, new TileIndex(cTile.m_ArrayIndex), num, maxHealth, TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, TargetingActor.ActorGuid, 1, TargetingActor.OriginalType);
					switch (TargetingActor.OriginalType)
					{
					case CActor.EType.Enemy:
						cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
						}
						else
						{
							m_Summoned = false;
						}
						break;
					case CActor.EType.Ally:
						cEnemyActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
						}
						else
						{
							m_Summoned = false;
						}
						break;
					case CActor.EType.Enemy2:
						cEnemyActor = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
						}
						else
						{
							m_Summoned = false;
						}
						break;
					case CActor.EType.Neutral:
						cEnemyActor = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
						}
						else
						{
							m_Summoned = false;
						}
						break;
					default:
						Log.ErrorWithReport("Non monster tried to summon a monster actor");
						return false;
					}
				}
				if (cEnemyActor != null)
				{
					SummonedActor = cEnemyActor;
					cEnemyActor.MarkWontPlayThisRound(wontPlay: true);
					GameState.SortIntoInitiativeAndIDOrder();
					ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				}
				using List<CObjectProp>.Enumerator enumerator3 = item3.FindProps(ScenarioManager.ObjectImportType.MonsterGrave).GetEnumerator();
				if (enumerator3.MoveNext())
				{
					CObjectProp current4 = enumerator3.Current;
					current4.Activate(TargetingActor, TargetingActor);
					item3.DelProp(current4);
				}
			}
			break;
		}
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override bool CanClearTargets()
	{
		return m_State == EReviveState.SelectGraves;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EReviveState.SelectGraves;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State >= EReviveState.ReviveDone;
	}

	public override string GetDescription()
	{
		return "Revive";
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool Validate()
	{
		foreach (string summonID in SummonIDs)
		{
			if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == summonID) == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(GetAbilityBaseCard().Name, "Unable to find Revive " + summonID + ".  GetAbilityBaseCard() Name: " + GetAbilityBaseCard().Name);
				return false;
			}
		}
		return true;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == TargetingActor.ActorGuid)?.IsSummon;
		}
		_ = m_State;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}
}
