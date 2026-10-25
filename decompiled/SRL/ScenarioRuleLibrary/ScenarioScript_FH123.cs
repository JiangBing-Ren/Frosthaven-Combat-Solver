using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH123 : ScenarioScript
{
	[ConsoleVariable(Alias = "ss123_debug_positions")]
	public static int DebugPositions_cvar = 0;

	private static readonly List<ScenarioManager.EAdjacentPosition> MovingDirections = new List<ScenarioManager.EAdjacentPosition>
	{
		ScenarioManager.EAdjacentPosition.ETopLeft,
		ScenarioManager.EAdjacentPosition.ETopRight
	};

	private CObjective_Scripted _activatePlatesObjective;

	private CMap _secondRoom;

	private List<CActor> _actorsToFall;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private bool AreEnemies(CActor a, CActor b)
	{
		return CActor.AreActorsEnemies(a.Type, b.Type, a, b, skipCheckingScenario: true);
	}

	private bool IsAncientArtillery(CActor actor)
	{
		return actor.Class.ID.StartsWith("AncientArtillery");
	}

	private bool ShouldFall(CActor actor)
	{
		if (!IsStablePosition(actor.Tile.m_ArrayIndex) && !IsAncientArtillery(actor) && !actor.FlyingAfterConditions)
		{
			return GetValidWalkableTiles(actor.ArrayIndex).Any((CTile t) => CanFallThroughTile(t, actor));
		}
		return false;
	}

	protected override bool Initialize()
	{
		_activatePlatesObjective = (CObjective_Scripted)GetObjective("ActivatePlates");
		_activatePlatesObjective.Total = ScenarioScript._css.Props.OfType<CObjectPressurePlate>().Count();
		_secondRoom = ScenarioScript.FindRoom("Room_2");
		CScenarioModifier scenarioModifier = GetScenarioModifier("specialRule2");
		CScenarioModifier scenarioModifier2 = GetScenarioModifier("specialRule3");
		List<object> list = new List<object> { ScenarioScript._css.Level };
		List<object> customLocKeyArgs = list;
		scenarioModifier2.CustomLocKeyArgs = list;
		scenarioModifier.CustomLocKeyArgs = customLocKeyArgs;
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 67);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == _secondRoom)
		{
			_activatePlatesObjective.SetActivation(active: true);
		}
	}

	private void TickEndTurn()
	{
		UpdatePlatesObjective();
	}

	private void PlayFallVFX()
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = "TitanBeam123",
			m_ArrayIndex = new Point(13, 20)
		});
	}

	private void TickEndRound()
	{
		if (OncePerRound())
		{
			PlayFallVFX();
			List<CActor> allAliveActors = ScenarioManager.Scenario.AllAliveActors;
			_actorsToFall = allAliveActors.OrderBy((CActor a) => a.Initiative()).ToList();
			ResumeFallingNextActor();
		}
	}

	private void StopFallingAndCheckNextActor(CActor actor)
	{
		_actorsToFall.Remove(actor);
		ResumeFallingNextActor();
	}

	private void ResumeFallingNextActor()
	{
		if (_actorsToFall.Count != 0)
		{
			CActor cActor = _actorsToFall[0];
			if (ShouldFall(cActor))
			{
				TryFallOneHex(cActor, cActor.Tile.m_ArrayIndex, cActor.Tile);
			}
			else
			{
				StopFallingAndCheckNextActor(cActor);
			}
		}
	}

	private void UpdatePlatesObjective()
	{
		if (!_activatePlatesObjective.IsComplete)
		{
			int num = ScenarioScript._css.Props.OfType<CObjectPressurePlate>().Count((CObjectPressurePlate pp) => pp.Activated);
			_activatePlatesObjective.Current = num;
			if (_activatePlatesObjective.Current == _activatePlatesObjective.Total)
			{
				_activatePlatesObjective.ScriptedIsComplete = true;
				(GetProp(9, 7, "ThickDoor") as CObjectDoor).UnlockLockedDoorWithoutOpening();
			}
			_activatePlatesObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	private void TryFallOneHex(CActor actor, Point position, CTile fromHex)
	{
		List<CTile> list = GetValidWalkableTiles(position).ToList();
		if (list.Count == 0)
		{
			Error("No valid hexes for TryPickFallingHex!", null, 139);
			return;
		}
		List<CTile> list2 = list.FindAll((CTile t) => CanFallThroughTile(t, actor));
		if (list2.Count == 0)
		{
			Trace("No valid hexes to fall to pick for TryPickFallingHex!", null, 146);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			AnimateFall(actor, fromHex, actor.Tile);
			DealScenarioDamage(actor, ScenarioScript._css.Level);
			foreach (CTile item in list)
			{
				foreach (CActor livingActor in item.LivingActors)
				{
					DealScenarioDamage(livingActor, ScenarioScript._css.Level);
				}
			}
			StopFallingAndCheckNextActor(actor);
		}
		else if ((actor is CPlayerActor || actor is CHeroSummonActor) && list2.Count > 1)
		{
			AnimateFall(actor, fromHex, actor.Tile);
			ScenarioScript.PickHexes("FH_Scenario_Campaign_123_PickFallingHex_HelpText", actor, list2, TryFallOneHexPickHexesCallback, 1, animateActor: false, null, canSkip: false, canUndo: false, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: false, ignoreDoorLocks: false, stackAsSeparateAbilities: true, autoTrigger: false, null, interruptCurrentAbility: false, tryInject: false, new List<(string, object)>
			{
				("ForcePreviewTilesPossibleTarget", list2),
				("ForcePreviewTilesReach", CExpression.Parse("ScenarioScript_FH123.GetPreviewTilesForFall()"))
			});
		}
		else
		{
			CTile cTile = list2.FirstOrDefault((CTile t) => IsStablePosition(t.m_ArrayIndex));
			MoveFallActorToHex(actor, cTile ?? list2[0], fromHex);
		}
	}

	public static List<CTile> GetPreviewTilesForFall()
	{
		if (!(PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityNullHex cAbilityNullHex))
		{
			return new List<CTile>();
		}
		ScenarioScript_FH123 ss = ScenarioScript.current as ScenarioScript_FH123;
		CActor actor = cAbilityNullHex.TargetingActor;
		Point arrayIndex = actor.ArrayIndex;
		if (cAbilityNullHex.TilesSelected.Count > 0)
		{
			arrayIndex = cAbilityNullHex.TilesSelected[0].m_ArrayIndex;
			return ss.GetValidWalkableTilesRecursive(arrayIndex).ToList();
		}
		List<CTile> second = ss.GetValidWalkableTiles(arrayIndex).ToList().FindAll((CTile t) => ss.CanFallThroughTile(t, actor));
		return ss.GetValidWalkableTilesRecursive(arrayIndex).Except(second).ToList();
	}

	private void AnimateFall(CActor actor, CTile originalPosition, CTile finalPosition)
	{
		if (originalPosition != finalPosition)
		{
			CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(actor);
			cActorHasPushed_MessageData.m_PushAbility = null;
			cActorHasPushed_MessageData.m_ActorBeingPushed = actor;
			cActorHasPushed_MessageData.m_ChoreographerWait = false;
			ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
		}
	}

	private void TryFallOneHexPickHexesCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		CTile tile = ability.TargetingActor.Tile;
		MoveFallActorToHex(ability.TargetingActor, selectedTiles[0], tile);
	}

	private void MoveFallActorToHex(CActor actor, CTile tile, CTile fromHex)
	{
		GameState.MoveActorStraightLine(actor, tile.m_ArrayIndex, shouldWait: false, slideWithoutAnim: true, 2f, notifyChoreographer: false);
		if (IsSafePosition(tile.m_ArrayIndex))
		{
			Trace("No valid hexes to fall to pick for TryPickFallingHex!", null, 218);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			AnimateFall(actor, fromHex, tile);
			DealScenarioDamage(actor, ScenarioScript._css.Level);
			StopFallingAndCheckNextActor(actor);
		}
		else
		{
			TryFallOneHex(actor, tile.m_ArrayIndex, fromHex);
		}
	}

	private List<CTile> GetValidWalkableTilesRecursive(Point position, List<CTile> res = null)
	{
		if (res == null)
		{
			res = new List<CTile>();
		}
		foreach (CTile validWalkableTile in GetValidWalkableTiles(position))
		{
			res.Add(validWalkableTile);
			GetValidWalkableTilesRecursive(validWalkableTile.m_ArrayIndex, res);
		}
		return res;
	}

	private IEnumerable<CTile> GetValidWalkableTiles(Point position)
	{
		foreach (ScenarioManager.EAdjacentPosition movingDirection in MovingDirections)
		{
			CTile adjacentWalkableTile = GetAdjacentWalkableTile(position, movingDirection);
			if (adjacentWalkableTile != null)
			{
				yield return adjacentWalkableTile;
			}
		}
	}

	private CTile GetAdjacentWalkableTile(Point position, ScenarioManager.EAdjacentPosition movingDirection)
	{
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(position.X, position.Y, movingDirection);
		if (adjacentTile == null || !IsTileWalkable(adjacentTile))
		{
			return null;
		}
		return adjacentTile;
	}

	private bool CanFallThroughTile(CTile tile, CActor actor)
	{
		CObjectDoor cObjectDoor = tile.FindDoor();
		if (cObjectDoor != null && cObjectDoor.DoorIsClosed)
		{
			return false;
		}
		List<CActor> list = tile.LivingActors.ToList();
		if (list.Count == 0)
		{
			return true;
		}
		if (list.Exists((CActor ta) => AreEnemies(actor, ta)))
		{
			return false;
		}
		if (list.Exists((CActor ta) => !AreEnemies(actor, ta)))
		{
			foreach (CTile validWalkableTile in GetValidWalkableTiles(tile.m_ArrayIndex))
			{
				if (CanFallThroughTile(validWalkableTile, actor))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsTileWalkable(CTile tile)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
		if (cNode.Walkable)
		{
			return !cNode.Blocked;
		}
		return false;
	}

	public static bool IsStablePosition(Point position)
	{
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(position.X, position.Y, MovingDirections[0]);
		CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(position.X, position.Y, MovingDirections[1]);
		if (adjacentTile != null && !((adjacentTile2 == null) | !IsTileWalkable(adjacentTile)))
		{
			return !IsTileWalkable(adjacentTile2);
		}
		return true;
	}

	public static bool IsSafePosition(Point position)
	{
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(position.X, position.Y, MovingDirections[0]);
		CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(position.X, position.Y, MovingDirections[1]);
		if (adjacentTile == null || !IsTileWalkable(adjacentTile))
		{
			if (adjacentTile2 != null)
			{
				return !IsTileWalkable(adjacentTile2);
			}
			return true;
		}
		return false;
	}

	public static bool IsWalkable(CTile tile)
	{
		return ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Walkable;
	}

	public static bool IsBlocked(CTile tile)
	{
		return ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Blocked;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("fall_damage", ScenarioScript._css.Level.ToString());
	}
}
