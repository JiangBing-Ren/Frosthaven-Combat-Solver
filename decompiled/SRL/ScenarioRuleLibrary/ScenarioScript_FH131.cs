using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH131 : ScenarioScript
{
	[Serializable]
	public class Storage131
	{
		public List<(string, Point)> PhaseOutPositions = new List<(string, Point)>();
	}

	public const ESpecificPropType CorpseSpecificPropType = ESpecificPropType.LootableCorpse;

	private int startRoundLeft;

	private int startRoundRight;

	public int endRoundLeft;

	public int endRoundRight;

	public CObjective_Scripted tiltObjectiveLeft;

	public CObjective_Scripted tiltObjectiveRight;

	public CObjective_Scripted playerBodyFallOff;

	public CObjective_Scripted icebergInWater;

	public List<CTile> EdgeTiles;

	[StateDump(Skip = true)]
	private int lastEndRound;

	private Storage131 serializedProperties;

	private bool _inEndRoundMove;

	private List<CActor> _actorsMoved = new List<CActor>();

	private List<CActor> _actorsDied = new List<CActor>();

	private List<Point> _highlightedTiles = new List<Point>();

	private const string conductorID = "LurkerConductorID";

	private const string wavethrowerID = "LurkerWavethrower131ID";

	public override bool DisableCameraRotation => true;

	private IEnumerable<CObjectActor> CorpseActors => ScenarioManager.Scenario.AllObjects.Where((CObjectActor x) => x.AttachedProp != null && x.AttachedProp.SpecificPropType == ESpecificPropType.LootableCorpse);

	private List<(string, Point)> PhaseOutPositions
	{
		get
		{
			return serializedProperties.PhaseOutPositions;
		}
		set
		{
			serializedProperties.PhaseOutPositions = value;
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage131();
		tiltObjectiveLeft = GetObjective<CObjective_Scripted>("TiltObjectiveLeft");
		tiltObjectiveRight = GetObjective<CObjective_Scripted>("TiltObjectiveRight");
		playerBodyFallOff = GetObjective<CObjective_Scripted>("PlayerOrBodyFallOff");
		icebergInWater = GetObjective<CObjective_Scripted>("IcebergInWater");
		icebergInWater.Total = 1;
		playerBodyFallOff.Total = 1;
		tiltObjectiveLeft.Total = 4;
		tiltObjectiveRight.Total = 4;
		EdgeTiles = new List<CTile>
		{
			ScenarioManager.GetTile(3, 2),
			ScenarioManager.GetTile(4, 2),
			ScenarioManager.GetTile(5, 2),
			ScenarioManager.GetTile(6, 2),
			ScenarioManager.GetTile(7, 2),
			ScenarioManager.GetTile(7, 3),
			ScenarioManager.GetTile(8, 4),
			ScenarioManager.GetTile(8, 5),
			ScenarioManager.GetTile(8, 6),
			ScenarioManager.GetTile(8, 7),
			ScenarioManager.GetTile(8, 8),
			ScenarioManager.GetTile(7, 9),
			ScenarioManager.GetTile(7, 10),
			ScenarioManager.GetTile(6, 10),
			ScenarioManager.GetTile(5, 10),
			ScenarioManager.GetTile(4, 10),
			ScenarioManager.GetTile(3, 10),
			ScenarioManager.GetTile(2, 9),
			ScenarioManager.GetTile(2, 8),
			ScenarioManager.GetTile(1, 7),
			ScenarioManager.GetTile(2, 6),
			ScenarioManager.GetTile(1, 5),
			ScenarioManager.GetTile(2, 4),
			ScenarioManager.GetTile(2, 3)
		};
		ToggleEdgePathfinding(enabled: false);
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (string.IsNullOrEmpty(storage))
		{
			tiltObjectiveLeft.Current = 2;
			tiltObjectiveRight.Current = 2;
		}
		startRoundLeft = (endRoundLeft = tiltObjectiveLeft.Current);
		startRoundRight = (endRoundRight = tiltObjectiveRight.Current);
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			enemy.NoGoldDrop = true;
		}
		foreach (CObjectActor corpseActor in CorpseActors)
		{
			corpseActor.ForcedUntargetable = true;
			corpseActor.IsImmuneToDamage = true;
			corpseActor.IsTransientBlocker = false;
			CActor.EType type = (corpseActor.OriginalType = CActor.EType.Neutral);
			corpseActor.Type = type;
			if (!corpseActor.IsDead)
			{
				CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(corpseActor, active: false);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		RefreshHighlights();
		CalcTilt();
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (Storage131)JsonConvert.DeserializeObject(storage, typeof(Storage131));
		}
	}

	private float CalcWeight(CActor actor)
	{
		float result = 0f;
		if (actor is CObjectActor { AttachedProp: var attachedProp } cObjectActor && attachedProp?.Owner != null)
		{
			return CalcWeight(cObjectActor.AttachedProp.Owner);
		}
		if (actor is CPlayerActor || actor is CEnemyActor)
		{
			result = 1f;
			if (actor.Class.ID == "LurkerConductorID")
			{
				result = 2f;
			}
		}
		else if (actor is CHeroSummonActor)
		{
			result = 0.5f;
		}
		return result;
	}

	public void CalcTilt()
	{
		float[] array = new float[2];
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (!allAliveActor.IsDead && (allAliveActor.ArrayIndex.X != 5 || (allAliveActor.ArrayIndex.Y != 2 && allAliveActor.ArrayIndex.Y != 4 && allAliveActor.ArrayIndex.Y != 6 && allAliveActor.ArrayIndex.Y != 8 && allAliveActor.ArrayIndex.Y != 10)))
			{
				int num = 0;
				if (allAliveActor.ArrayIndex.X >= 5)
				{
					num = 1;
				}
				float num2 = CalcWeight(allAliveActor);
				array[num] += num2;
			}
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if ((prop.ArrayIndex.X != 5 || (prop.ArrayIndex.Y != 2 && prop.ArrayIndex.Y != 4 && prop.ArrayIndex.Y != 6 && prop.ArrayIndex.Y != 8 && prop.ArrayIndex.Y != 10)) && prop is CObjectObstacle)
			{
				int num3 = 0;
				if (prop.ArrayIndex.X >= 5)
				{
					num3 = 1;
				}
				array[num3]++;
			}
		}
		if (array[0] > array[1])
		{
			endRoundLeft = Math.Min(startRoundLeft + 1, tiltObjectiveLeft.Total);
			endRoundRight = Math.Max(startRoundRight - 1, 0);
		}
		else if (array[1] > array[0])
		{
			endRoundLeft = Math.Max(startRoundLeft - 1, 0);
			endRoundRight = Math.Min(startRoundRight + 1, tiltObjectiveRight.Total);
		}
		else
		{
			endRoundLeft = startRoundLeft;
			endRoundRight = startRoundRight;
		}
		if (tiltObjectiveLeft.ScriptedIsComplete)
		{
			endRoundLeft = 5;
		}
		else if (tiltObjectiveRight.ScriptedIsComplete)
		{
			endRoundRight = 5;
		}
		icebergInWater.Current = ((tiltObjectiveLeft.ScriptedIsComplete || tiltObjectiveRight.IsComplete) ? 1 : 0);
		icebergInWater.ScriptedIsComplete = icebergInWater.Current >= icebergInWater.Total;
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void RefreshHighlights()
	{
		IEnumerable<Point> enumerable = from x in GetTeleportTiles()
			select x.m_ArrayIndex;
		ScenarioManager.ToggleTileHighlights(_highlightedTiles, enabled: false, new CVector4(1f, 1f, 0f, 1f));
		_highlightedTiles.Clear();
		if (ScenarioManager.Scenario?.Enemies?.Any((CEnemyActor x) => x.MonsterClass.RoundAbilityCard != null && (x.MonsterClass.RoundAbilityCard.ID == 9999701 || x.MonsterClass.RoundAbilityCard.ID == 9999702)) == true)
		{
			ScenarioManager.ToggleTileHighlights(enumerable, enabled: true, new CVector4(1f, 1f, 0f, 1f));
			_highlightedTiles.AddRange(enumerable);
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			if (_tickActor is CEnemyActor cEnemyActor)
			{
				cEnemyActor.NoGoldDrop = true;
			}
			CalcTilt();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		{
			if (!OncePerRound())
			{
				break;
			}
			startRoundLeft = tiltObjectiveLeft.Current;
			startRoundRight = tiltObjectiveRight.Current;
			List<CActor> list = ScenarioManager.Scenario.AllActors.Where((CActor x) => x.PhasedOut).ToList();
			ToggleEdgePathfinding(enabled: false);
			foreach (CActor actor in list)
			{
				CTile tile = actor.Tile;
				(string, Point) tuple = PhaseOutPositions.SingleOrDefault(((string, Point) x) => x.Item1 == actor.ActorGuid);
				(string, Point) tuple2 = tuple;
				if (tuple2.Item1 != null || tuple2.Item2 != default(Point))
				{
					tile = ScenarioManager.GetTile(tuple.Item2);
				}
				CTile cTile = GameState.FindClosestEmptyTile(tile);
				if (cTile != null)
				{
					actor.ArrayIndex = cTile.m_ArrayIndex;
					GameState.PhaseInActor(actor);
				}
			}
			CalcTilt();
			RefreshHighlights();
			if (ScenarioScript.RoundNumber == 3)
			{
				GameState.TriggerCampaignSection("FS_131_The_Dancing_Iceberg_Start_Round_3");
			}
			else if (ScenarioScript.RoundNumber == 5)
			{
				GameState.TriggerCampaignSection("FS_131_The_Dancing_Iceberg_Start_Round_5");
			}
			else if (ScenarioScript.RoundNumber == 7)
			{
				GameState.TriggerCampaignSection("FS_131_The_Dancing_Iceberg_Start_Round_7");
			}
			else if (ScenarioScript.RoundNumber == 8)
			{
				GameState.TriggerCampaignSection("FS_131_The_Dancing_Iceberg_Start_Round_8");
			}
			break;
		}
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			RefreshHighlights();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			if (ScenarioScript.RoundNumber != lastEndRound)
			{
				lastEndRound = ScenarioScript.RoundNumber;
				CalcTilt();
				tiltObjectiveLeft.Current = endRoundLeft;
				tiltObjectiveRight.Current = endRoundRight;
				if (tiltObjectiveLeft.Current == tiltObjectiveLeft.Total)
				{
					tiltObjectiveLeft.ScriptedIsComplete = true;
				}
				if (tiltObjectiveRight.Current == tiltObjectiveRight.Total)
				{
					tiltObjectiveRight.ScriptedIsComplete = true;
				}
				if (endRoundRight > 2)
				{
					MoveEveryoneInDirection(ScenarioManager.EAdjacentPosition.ERight);
				}
				else if (endRoundLeft > 2)
				{
					MoveEveryoneInDirection(ScenarioManager.EAdjacentPosition.ELeft);
				}
				ToggleEdgePathfinding(enabled: false);
			}
			break;
		}
	}

	private void MoveEveryoneInDirection(ScenarioManager.EAdjacentPosition dir)
	{
		_inEndRoundMove = true;
		ToggleEdgePathfinding(enabled: true);
		List<Action> list = new List<Action>();
		List<Action> list2 = new List<Action>();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props.OrderBy((CObjectProp x) => x.PropGuid).ToList())
		{
			if (prop.RuntimeAttachedActor != null)
			{
				continue;
			}
			CTile oldPos = prop.PropTile;
			CVector3 oldRot = prop.Rotation;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(prop.ArrayIndex.X, prop.ArrayIndex.Y, dir);
			CObjectProp newProp = null;
			bool flag = true;
			TileIndex newPos = null;
			string guid = prop.PropGuid;
			if (adjacentTile != null && !EdgeTiles.Contains(adjacentTile))
			{
				newPos = new TileIndex(adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y);
				SerializationInfo info = new SerializationInfo(prop.GetType(), new FormatterConverter());
				prop.GetObjectData(info, default);
				if (prop is CObjectObstacle cObjectObstacle)
				{
					newProp = prop;
					flag = false;
					cObjectObstacle.SetIsBlocker(isBlocker: false);
				}
				else if (prop is CObjectTrap)
				{
					newProp = prop;
					flag = false;
				}
				else if (prop is CObjectHazardousTerrain)
				{
					newProp = new CObjectHazardousTerrain(info, default);
				}
				else
				{
					if (prop is CObjectShadow)
					{
						continue;
					}
					if (prop is CObjectTerrainVisual)
					{
						newProp = new CObjectTerrainVisual(info, default);
					}
					else if (prop is CObjectDifficultTerrain)
					{
						newProp = new CObjectDifficultTerrain(info, default);
					}
					else if (prop is CObjectIceTerrain)
					{
						newProp = new CObjectIceTerrain(info, default);
					}
				}
			}
			if (flag)
			{
				list.Add(() =>
				{
					prop.DestroyProp();
					ScenarioManager.CurrentScenarioState.Props.Remove(prop);
				});
			}
			else
			{
				list.Add(() =>
				{
					oldPos.DelProp(prop, recursiveTiles: true);
					ScenarioManager.CurrentScenarioState.Props.Remove(prop);
				});
			}
			if (newProp == null)
			{
				continue;
			}
			list2.Add(() =>
			{
				if (newProp != prop)
				{
					if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.GuidRNG != null)
					{
						guid = ScenarioManager.CurrentScenarioState.GetGUIDBasedOnGuidRNGState().ToString();
					}
					else
					{
						guid = Guid.NewGuid().ToString();
					}
					newProp.SetGUID(guid);
				}
				newProp.SetLocation(newPos, null, prop.Rotation);
				ScenarioManager.CurrentScenarioState.Props.Add(newProp);
				if (newProp != prop)
				{
					adjacentTile.SpawnProp(newProp);
				}
				else
				{
					adjacentTile.AddProp(newProp, recursiveTiles: true);
				}
				if (newProp is CObjectObstacle cObjectObstacle2)
				{
					cObjectObstacle2.SetIsBlocker(isBlocker: true);
				}
				if (newProp is CObjectDifficultTerrain cObjectDifficultTerrain)
				{
					cObjectDifficultTerrain.InitDifficultTerrain();
				}
				CMoveProp_MessageData message = new CMoveProp_MessageData
				{
					m_MoveProp = newProp,
					m_MoveToTile = adjacentTile,
					m_MoveSpeed = 8f,
					m_MoveFromTile = oldPos,
					m_MoveFromRotation = oldRot,
					m_MoveToRotation = oldRot
				};
				ScenarioRuleClient.MessageHandler(message);
			});
		}
		foreach (Action item in list)
		{
			item();
		}
		foreach (Action item2 in list2)
		{
			item2();
		}
		_actorsMoved.Clear();
		_actorsDied.Clear();
		CustomAbility.MoveActorsInDirection(null, dir, 1, 1, ignoreObjectActor: true);
		if (0 != 0)
		{
			GameState.ThreadIsSleeping = true;
			Thread.Sleep(1000);
			GameState.ThreadIsSleeping = false;
		}
		_inEndRoundMove = false;
		foreach (CActor item3 in _actorsMoved)
		{
			RecalcAfterMove(item3);
			if (item3.PhasedOut)
			{
				_actorsDied.Add(item3);
			}
		}
		_actorsMoved.Clear();
		if (_actorsDied.Count > 0)
		{
			GameState.ThreadIsSleeping = true;
			Thread.Sleep(2000);
			GameState.ThreadIsSleeping = false;
			_actorsDied.Clear();
		}
		ToggleEdgePathfinding(enabled: false);
	}

	private void RecalcAfterMove(CActor actor)
	{
		CalcTilt();
		if (EdgeTiles.Contains(actor.Tile))
		{
			if (actor.FlyingAfterConditions || actor is CEnemyActor)
			{
				DealScenarioDamage(actor, 1);
				if (!actor.IsDead)
				{
					CAbilityRemoveConditions.RemoveConditions(actor, actor, CCondition.NegativeConditions.ToList(), CCondition.PositiveConditions.ToList(), null);
					GameState.PhaseOutActor(actor, instant: false);
					PhaseOutPositions.RemoveAll(((string, Point) x) => x.Item1 == actor.ActorGuid);
					PhaseOutPositions.Add((actor.ActorGuid, actor.PrevArrayIndex));
				}
			}
			else
			{
				GameState.KillActor(actor, actor, CActor.ECauseOfDeath.Suicide, out var _);
				if (actor is CPlayerActor || (actor is CObjectActor cObjectActor && cObjectActor.AttachedProp.Owner is CPlayerActor))
				{
					playerBodyFallOff.Current = 1;
					playerBodyFallOff.ScriptedIsComplete = playerBodyFallOff.Current >= playerBodyFallOff.Total;
					ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				}
			}
		}
		RefreshHighlights();
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		CalcTilt();
		CActor tickActor = _tickActor;
		if (tickActor is CObjectActor cObjectActor)
		{
			ScenarioManager.GetTile(cObjectActor.PrevArrayIndex).DelProp(cObjectActor.AttachedProp);
			cObjectActor.AttachedProp.PropTile.AddProp(cObjectActor.AttachedProp);
		}
		if (_inEndRoundMove)
		{
			_actorsMoved.Add(tickActor);
		}
		else
		{
			RecalcAfterMove(tickActor);
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor is CObjectActor cObjectActor && cObjectActor.AttachedProp.SpecificPropType == ESpecificPropType.LootableCorpse)
		{
			cObjectActor.AttachedProp.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
		}
		if ((_tickActor is CPlayerActor || _tickActor is CEnemyActor) && !EdgeTiles.Contains(_tickActor.Tile))
		{
			CTile cTile = _tickActor.Tile;
			if (HasCorpseOnTile(cTile))
			{
				IOrderedEnumerable<CTile> source = from x in GameState.GetNearestEmptyTiles(cTile)
					where !HasCorpseOnTile(x)
					orderby _tickActor.GetDistanceTo(x)
					select x;
				if (!source.Any())
				{
					return;
				}
				cTile = source.First();
			}
			SpawnCorpsesProp(cTile, _tickActor);
		}
		CalcTilt();
		RefreshHighlights();
	}

	private CObjectProp SpawnCorpsesProp(CTile tile, CActor owner)
	{
		TileIndex tileIndex = new TileIndex(tile.m_ArrayIndex);
		CObjectTerrainVisual cObjectTerrainVisual = new CObjectTerrainVisual(ESpecificPropType.LootableCorpse.ToString(), ScenarioManager.ObjectImportType.TerrainVisualEffect, tileIndex, null, null, owner, tile.m_HexMap.MapGuid);
		tile.SpawnProp(cObjectTerrainVisual);
		SpawnDummyActorOnProp(tileIndex.X, tileIndex.Y, cObjectTerrainVisual, 1, active: true);
		CObjectActor runtimeAttachedActor = cObjectTerrainVisual.RuntimeAttachedActor;
		runtimeAttachedActor.IsTransientBlocker = false;
		runtimeAttachedActor.ForcedUntargetable = true;
		CActor.EType type = (runtimeAttachedActor.OriginalType = CActor.EType.Neutral);
		runtimeAttachedActor.Type = type;
		runtimeAttachedActor.IsImmuneToDamage = true;
		runtimeAttachedActor.TreatAsObstacle = true;
		cObjectTerrainVisual.PropHealthDetails.DeathAction = PropHealthDetails.EDeathAction.None;
		cObjectTerrainVisual.PropCustomLocKey = "FH_131_Dead_Body_T";
		cObjectTerrainVisual.PropHealthDetails.CustomLocKey = "FH_131_Dead_Body_T";
		cObjectTerrainVisual.PropCustomDescriptionLocKey = "FH_131_Dead_Body_D";
		CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
		{
			m_SpawnTile = tile,
			m_SpawnEnemy = runtimeAttachedActor,
			m_ActorSpawningMessage = runtimeAttachedActor
		};
		ScenarioRuleClient.MessageHandler(message);
		CSetActiveActorHealthBar_MessageData message2 = new CSetActiveActorHealthBar_MessageData(runtimeAttachedActor, active: false);
		ScenarioRuleClient.MessageHandler(message2);
		return cObjectTerrainVisual;
	}

	public bool HasCorpseOnTile(CTile tile)
	{
		return tile.m_Props.Any((CObjectProp x) => x.SpecificPropType == ESpecificPropType.LootableCorpse);
	}

	public override bool ShouldIgnoreImmunity(CActor actor, CAbility ability)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Push && ability.Flags.HasFlag(CAbility.EAbilityFlag.Special1))
		{
			return true;
		}
		return base.ShouldIgnoreImmunity(actor, ability);
	}

	private void ToggleEdgePathfinding(bool enabled)
	{
		foreach (CTile edgeTile in EdgeTiles)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(edgeTile.m_ArrayIndex);
			node.Walkable = enabled;
			node.Blocked = !enabled;
			node.IsNegativeHex = true;
		}
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		TryToggleEdgePathfindingOnForcedMove();
	}

	protected override void TickAbilityPerformStartedInternal()
	{
		base.TickAbilityPerformStartedInternal();
		TryToggleEdgePathfindingOnForcedMove();
	}

	private void TryToggleEdgePathfindingOnForcedMove()
	{
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
		bool flag = cAbility is CAbilityPush || cAbility is CAbilityPull || (cAbility?.ControllingActor != null && CustomAbility.IsEnemy(cAbility.ControllingActor, cAbility.TargetingActor, checkOriginalType: true));
		ToggleEdgePathfinding(flag);
		ToggleCorpseTargetable(flag);
	}

	private void ToggleCorpseTargetable(bool targetable)
	{
		foreach (CObjectActor corpseActor in CorpseActors)
		{
			corpseActor.ForcedUntargetable = !targetable;
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (_tickAbility.Flags.HasFlag(CAbility.EAbilityFlag.Special1))
		{
			CActiveBonus cActiveBonus = _tickAbility.GetVar("CacheActiveBonus").GetObject<CActiveBonus>();
			if (cActiveBonus != null)
			{
				if (cActiveBonus.ToggledBonus)
				{
					cActiveBonus.ToggleActiveBonusFromVars(_tickAbility.TargetingActor, markCostAbilityAsExecuted: true);
				}
				if (_tickAbility.AbilityHasHappened)
				{
					CustomAbility.PayMovementPoints(0, _tickAbility.TargetingActor, cActiveBonus);
				}
				else
				{
					cActiveBonus.CostAbilityExecuted = false;
					if (PhaseManager.CurrentPhaseAction != null && PhaseManager.CurrentPhaseAction.CurrentTriggeredActiveBonus == cActiveBonus)
					{
						PhaseManager.CurrentPhaseAction.CurrentTriggeredActiveBonus = null;
					}
				}
			}
		}
		ToggleEdgePathfinding(enabled: false);
		ToggleCorpseTargetable(targetable: false);
		RefreshHighlights();
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		ToggleEdgePathfinding(enabled: false);
		ToggleCorpseTargetable(targetable: false);
		RefreshHighlights();
	}

	public List<CTile> LeftMostEmptyHexes()
	{
		List<CTile> list = new List<CTile>();
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CTile tile = ScenarioManager.GetTile(j, i);
				CNode node = ScenarioManager.PathFinder.GetNode(j, i);
				if (node != null && !node.Blocked && node.Walkable && tile != null && (tile.IsEmpty || (tile.LivingActor != null && tile.LivingActor.Class.ID.Contains("LurkerWavethrower131ID"))))
				{
					list.Add(tile);
					break;
				}
			}
		}
		return list;
	}

	public List<CTile> RightMostEmptyHexes()
	{
		List<CTile> list = new List<CTile>();
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int num = ScenarioManager.Width - 1; num >= 0; num--)
			{
				CTile tile = ScenarioManager.GetTile(num, i);
				CNode node = ScenarioManager.PathFinder.GetNode(num, i);
				if (node != null && !node.Blocked && node.Walkable && tile != null && (tile.IsEmpty || (tile.LivingActor != null && tile.LivingActor.Class.ID.Contains("LurkerWavethrower131ID"))))
				{
					list.Add(tile);
					break;
				}
			}
		}
		return list;
	}

	public List<CTile> GetTeleportTiles()
	{
		List<CTile> list = ((ScenarioScript.RoundNumber % 2 == 0) ? RightMostEmptyHexes() : LeftMostEmptyHexes());
		list.RemoveAll((CTile x) => EdgeTiles.Contains(x));
		return list;
	}

	public CTile GetTeleportTile(CActor actor)
	{
		List<CTile> teleportTiles = GetTeleportTiles();
		teleportTiles.RemoveAll((CTile x) => x.LivingActor != null && x.LivingActor != actor);
		return teleportTiles.OrderByDescending((CTile x) => Math.Abs(x.m_ArrayIndex.X - actor.Tile.m_ArrayIndex.X)).ThenByDescending((CTile tile) =>
		{
			int num = int.MaxValue;
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				int distanceTo = playerActor.GetDistanceTo(tile);
				if (distanceTo < num)
				{
					num = distanceTo;
				}
			}
			return num;
		}).FirstOrDefault();
	}
}
