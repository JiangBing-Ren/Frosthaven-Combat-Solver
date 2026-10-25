using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH108 : ScenarioScript
{
	[StateDump(Skip = true)]
	private CObjectProp nextTileTrap;

	protected override bool Initialize()
	{
		RefreshIce();
		CObjectDoor cObjectDoor = ScenarioScript.FindDoor(11, 13);
		CObjectDoor cObjectDoor2 = ScenarioScript.FindDoor(12, 13);
		cObjectDoor.TwinDoors = new CObjectDoor[1] { cObjectDoor2 };
		cObjectDoor2.TwinDoors = new CObjectDoor[1] { cObjectDoor };
		return base.Initialize();
	}

	private void RefreshIce()
	{
		foreach (CMap map in ScenarioManager.Scenario.Maps)
		{
			map.TreatAsIce = true;
			foreach (CMapTile mapTile in map.MapTiles)
			{
				TileIndex arrayIndex = mapTile.ArrayIndex;
				CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
				if (CAbilityFilter.IsTileFeatureless(cTile))
				{
					ScenarioManager.PathFinder.Nodes[arrayIndex.X, arrayIndex.Y].IsIceTerrain = true;
					continue;
				}
				ScenarioManager.PathFinder.Nodes[arrayIndex.X, arrayIndex.Y].IsIceTerrain = cTile.m_Props.Find((CObjectProp p) => p.PropType == EPropType.TerrainIce) != null;
			}
		}
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		RefreshIce();
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartScenario && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartTurn && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndTurn && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndRound)
		{
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 69);
		}
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Move && _tickAbility is CAbilityMove { Fly: false } cAbilityMove)
		{
			cAbilityMove.Jump = false;
			cAbilityMove.SetMiscVar("MaxStrength", 2);
		}
	}

	protected override void TickAbilityOverridesChangedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Move && _tickAbility is CAbilityMove { Fly: false } cAbilityMove && _tickAbility.HasStarted)
		{
			cAbilityMove.Jump = false;
		}
	}

	protected override void TickAbilityPerformStartedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Move && _tickAbility is CAbilityMove { Fly: false } cAbilityMove && _tickAbility.HasStarted)
		{
			cAbilityMove.Jump = false;
		}
	}

	public override void OnActorMoving(CActor movingActor, Point currentPosition, Point nextPosition)
	{
		CTile tile = ScenarioManager.GetTile(nextPosition);
		nextTileTrap = tile.m_Props.Find((CObjectProp p) => p.PropType == EPropType.Trap);
	}

	protected override void TickRepositionInternal()
	{
		if (nextTileTrap != null && _tickAbility.AbilityType == CAbility.EAbilityType.Move && _tickAbility is CAbilityMove { RemainingMovesAfterLimit: >0 } cAbilityMove)
		{
			cAbilityMove.ForceInterrupt();
		}
		nextTileTrap = null;
	}

	protected override void TickAbilityEndedInternal()
	{
		RefreshIce();
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		RefreshIce();
	}
}
