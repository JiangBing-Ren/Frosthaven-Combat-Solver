using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.Utils;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH073 : ScenarioScript
{
	private readonly Point _highlightedHexLocation = new Point(21, 6);

	private readonly Point _fishKingSpawnLocation = new Point(12, 5);

	private readonly Point _abaelHerderSpawnLocation = new Point(13, 6);

	private readonly Point _piranhaPigSpawnLocation = new Point(13, 4);

	private CObjective_Scripted _reachBoatObjective;

	private CObjective_Scripted _endTurnOnHighlightedHexObjective;

	private CObjective_Scripted _defeatTheFishKingObjective;

	private CMap _roomWithBoat;

	private CSpawner _fishKingSpawner;

	private CSpawner _abaelHerderSpawner;

	private CSpawner _piranhaPigSpawner;

	private CObjectDoor _door63;

	private CObjectDoor _door65;

	private CObjectDoor _door67;

	private CObjectDoor _door143;

	private CObjectDoor _door145;

	private CObjectDoor _door147;

	private bool _fishKingIsBelowHalfHealth;

	private bool _finalNarrativeProcessed;

	private int chosenConclusion;

	protected override bool Initialize()
	{
		_reachBoatObjective = (CObjective_Scripted)GetObjective("ReachTheBoat");
		_endTurnOnHighlightedHexObjective = (CObjective_Scripted)GetObjective("EndTurnOnTheHighlightedHex");
		_defeatTheFishKingObjective = (CObjective_Scripted)GetObjective("DefaultTheFishKing");
		_roomWithBoat = ScenarioScript.FindRoom("Room_3");
		_fishKingSpawner = ScenarioManager.CurrentScenarioState.Spawners.First((CSpawner s) => s.ArrayIndex.ToPoint() == _fishKingSpawnLocation);
		_abaelHerderSpawner = ScenarioManager.CurrentScenarioState.Spawners.First((CSpawner s) => s.ArrayIndex.ToPoint() == _abaelHerderSpawnLocation);
		_piranhaPigSpawner = ScenarioManager.CurrentScenarioState.Spawners.First((CSpawner s) => s.ArrayIndex.ToPoint() == _piranhaPigSpawnLocation);
		_door63 = ScenarioScript.FindDoor(6, 3);
		_door65 = ScenarioScript.FindDoor(6, 5);
		_door67 = ScenarioScript.FindDoor(6, 7);
		_door143 = ScenarioScript.FindDoor(14, 3);
		_door145 = ScenarioScript.FindDoor(14, 5);
		_door147 = ScenarioScript.FindDoor(14, 7);
		_door63.TwinDoors = new CObjectDoor[2] { _door65, _door67 };
		_door65.TwinDoors = new CObjectDoor[2] { _door63, _door67 };
		_door67.TwinDoors = new CObjectDoor[2] { _door63, _door65 };
		_door143.TwinDoors = new CObjectDoor[2] { _door145, _door147 };
		_door145.TwinDoors = new CObjectDoor[2] { _door143, _door147 };
		_door147.TwinDoors = new CObjectDoor[2] { _door143, _door145 };
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateBoatObjective();
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.SpecificPropType == ESpecificPropType.FlotsamTwoHex))
		{
			foreach (Point tile2 in item.GetTiles())
			{
				CTile tile = ScenarioManager.GetTile(tile2);
				if (!tile.m_Props.Contains(item))
				{
					tile.AddProp(item);
				}
			}
		}
		UpdatePathFinderNodes(null);
	}

	private void UpdateBoatObjective()
	{
		ScenarioManager.ToggleTileHighlights(_highlightedHexLocation.AsEnumerable(), _endTurnOnHighlightedHexObjective.IsActive && !_defeatTheFishKingObjective.IsActive && _roomWithBoat.Revealed);
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 98);
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	private void TickStartRound()
	{
	}

	private void TickEndTurn()
	{
		if (_tickActor.Type == CActor.EType.Player && _tickActor.ArrayIndex == _highlightedHexLocation && !_endTurnOnHighlightedHexObjective.ScriptedIsComplete)
		{
			_endTurnOnHighlightedHexObjective.ScriptedIsComplete = true;
			_defeatTheFishKingObjective.SetActivation(active: true);
			ScenarioManager.ToggleTileHighlights(_highlightedHexLocation.AsEnumerable(), enabled: false);
			_fishKingSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			_abaelHerderSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			_piranhaPigSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			CheckObjectivesComplete();
			UpdateBoatObjective();
			GameState.WaitForConfirmation("WaitForFishKingNarrative");
		}
		else
		{
			CheckObjectivesComplete();
		}
	}

	public override void OnLevelMessageWasDismissed(CLevelMessage message)
	{
		base.OnLevelMessageWasDismissed(message);
		if (GameState.ThreadIsSleeping && GameState.ThreadSleepReason == "WaitForFishKingNarrative" && (message.MessageName == "ReachedHex" || message.MessageName == "FishKingDefeated"))
		{
			GameState.SelectedContinueAbility();
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == _roomWithBoat && !_reachBoatObjective.ScriptedIsComplete)
		{
			_reachBoatObjective.ScriptedIsComplete = true;
			_endTurnOnHighlightedHexObjective.SetActivation(active: true);
			ScenarioManager.ToggleTileHighlights(_highlightedHexLocation.AsEnumerable(), enabled: true);
		}
		CheckObjectivesComplete();
		UpdatePathFinderNodes(null);
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor.Class.ID == "FishKingID" && _tickActor.Health <= _tickActor.MaxHealth / 2 && !_fishKingIsBelowHalfHealth)
		{
			_fishKingIsBelowHalfHealth = true;
			_defeatTheFishKingObjective.ScriptedIsComplete = true;
			CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = _tickActor.ArrayIndex,
				m_Zoom = 0.1f
			};
			ScenarioRuleClient.MessageHandler(message);
			CheckObjectivesComplete();
			GameState.WaitForConfirmation("WaitForFishKingNarrative");
		}
		else
		{
			CheckObjectivesComplete();
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		CheckObjectivesComplete();
	}

	private void UpdatePathFinderNodes(CAbility ability)
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && cTile.m_HexMap.Revealed)
			{
				Point arrayIndex = cTile.m_ArrayIndex;
				CNode node = ScenarioManager.PathFinder.GetNode(arrayIndex);
				bool flag = GetFlotsam(cTile) != null;
				cTile.TreatAsWater = !flag;
				node.IsWaterTerrain = cTile.IsWater;
				node.IsDifficultTerrain = node.IsWaterTerrain || cTile.FindPropOfTypes(EPropType.TerrainRubble) != null;
			}
		}
	}

	public override void ApplyReward(bool won)
	{
		base.ApplyReward(won);
		if (won)
		{
			if (GameState.ShowGenericConfirmButton(GameState.HostCharacter, "Scenario_73_Reward_Choice", "Scenario_73_Reward_Choice_A", null, "Scenario_73_Reward_Choice_B", showHeader: false))
			{
				chosenConclusion = 0;
			}
			else
			{
				chosenConclusion = 1;
			}
		}
	}

	protected override int GetConclusionIndexInternal()
	{
		return chosenConclusion;
	}

	public static CObjectProp GetFlotsam(CTile tile)
	{
		return tile.m_Props.FirstOrDefault((CObjectProp x) => x.SpecificPropType == ESpecificPropType.FlotsamOneHex || x.SpecificPropType == ESpecificPropType.FlotsamTwoHex);
	}

	public static void MoveFlotsamOnPath(CAbility ability)
	{
		if (!(ability is CAbilityNullHex cAbilityNullHex))
		{
			return;
		}
		if (!cAbilityNullHex.AbilityHasHappened)
		{
			return;
		}
		CDuringTurnAbilityActiveBonus originalAddBonus = PhaseManager.CurrentPhaseAction?.CurrentTriggeredActiveBonus as CDuringTurnAbilityActiveBonus;
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		ScenarioScript_FH073 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH073;
		CObjectProp flotsam = GetFlotsam(ability.TargetingActor.Tile);
		List<CTile> tilesSelected = cAbilityNullHex.TilesSelected;
		for (int i = 0; i < tilesSelected.Count; i++)
		{
			scenarioScript_FH.MovePlatformCallback(flotsam, ability.TargetingActor, tilesSelected[i]);
		}
		CAbilityMove cAbilityMove = ability.VarsContext.GetVar("TriggerAbility").GetObject<CAbilityMove>();
		if (cAbilityMove != null)
		{
			if (cAbilityMove.AbilityInterruptCopy != null)
			{
				cAbilityMove = cAbilityMove.AbilityInterruptCopy as CAbilityMove;
			}
			CAbilityMove cAbilityMove2 = cAbilityMove;
			int strength = (cAbilityMove.RemainingMoves = cAbilityNullHex.NumberTargetsRemaining);
			cAbilityMove2.Strength = strength;
			if (cAbilityMove.Strength == 0)
			{
				cAbilityMove.SetCancelAbilityFlag(cancel: true);
			}
		}
		if (ability.NumberTargetsRemaining <= 0)
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
		cAbility.StatIsBasedOnXEntries.Clear();
		cAbility.NumberTargets = ability.NumberTargetsRemaining;
		PhaseManager.StartAbilities(cAbility, ability.TargetingActor, stackInsteadOfInject: true, null, null, dontCopy: true);
		if (originalAddBonus != null)
		{
			CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
			currentPhaseAbility.m_PostEndAction = (Action)Delegate.Combine(currentPhaseAbility.m_PostEndAction, (Action)(() =>
			{
				originalAddBonus.ToggledBonus = true;
				PhaseManager.CurrentPhaseAction.CurrentTriggeredActiveBonus = originalAddBonus;
			}));
		}
	}

	public static bool CanMoveFlotsamOnTile(CTile tile)
	{
		CActor cActor = (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability)?.TargetingActor;
		CTile tile2 = cActor.Tile;
		if (tile.LivingActor != null && GetFlotsam(tile) != GetFlotsam(tile2) && !CActor.AreActorsAllied(cActor.Type, tile.LivingActor.Type, cActor, tile.LivingActor))
		{
			return false;
		}
		return CanMoveFlotsamOnTile(tile2, tile);
	}

	public static bool CanMoveFlotsamOnTile(CTile from, CTile tile)
	{
		if (tile.IsBlocked)
		{
			return false;
		}
		if (!tile.IsWalkable)
		{
			return false;
		}
		CActor cActor = (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability)?.TargetingActor;
		if (cActor == null)
		{
			return false;
		}
		CObjectProp flotsam = GetFlotsam(cActor.Tile);
		CObjectProp flotsam2 = GetFlotsam(tile);
		if (flotsam != flotsam2 && flotsam2 != null)
		{
			return false;
		}
		return true;
	}

	public static bool CanConfirmFlotsamMovement(CAbility ability)
	{
		if (ability.TilesSelected.Count == 0)
		{
			return true;
		}
		foreach (CTile item in ability.TilesSelected.TakeLast(2))
		{
			CActor livingActor = item.LivingActor;
			_ = ScenarioScript.current;
			if (livingActor != null && livingActor != ability.TargetingActor && GetFlotsam(livingActor.Tile) != GetFlotsam(ability.TargetingActor.Tile))
			{
				return false;
			}
		}
		return true;
	}

	protected override void MovePlatformCallback(CObjectProp boat, CTile tileToMoveFrom, CTile tileToMoveTo, CActor movingActor)
	{
		base.MovePlatformCallback(boat, tileToMoveFrom, tileToMoveTo, movingActor);
		UpdatePathFinderNodes(null);
	}
}
