using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH126 : ScenarioScript
{
	private const string resourceID = "FH_Scenario_Campaign_126_SCENARIO_OBJECT_1";

	private const string helperID = "Helper126ID";

	private CObjective_Scripted GiveXCratesToHelpers;

	private List<Point> crateHighlights = new List<Point>();

	protected override bool Initialize()
	{
		GiveXCratesToHelpers = GetObjective<CObjective_Scripted>("GiveXCratesToHelpers");
		GiveXCratesToHelpers.Total = ScenarioScript.PartySize + 1;
		GiveXCratesToHelpers.CustomLocKeyArgs = new List<object> { GiveXCratesToHelpers.Total };
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateCrateHighlights();
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor is CObjectActor { Tile: var tile })
		{
			CharacterResourceData characterResourceData = ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == "FH_Scenario_Campaign_126_SCENARIO_OBJECT_1");
			CObjectResource prop = new CObjectResource(characterResourceData, characterResourceData.ResourceModel, ScenarioManager.ObjectImportType.Resource, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
			tile.SpawnProp(prop);
			UpdateCrateHighlights();
		}
	}

	public override bool CanActorLoot(CObjectProp prop, CActor actor)
	{
		if (prop is CObjectResource && actor.GetCharacterResourceAmount("FH_Scenario_Campaign_126_SCENARIO_OBJECT_1") > 0)
		{
			return false;
		}
		return base.CanActorLoot(prop, actor);
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndTurn)
		{
			return;
		}
		CActor tickActor = _tickActor;
		CPlayerActor player = tickActor as CPlayerActor;
		if (player == null || !player.CharacterHasResource("FH_Scenario_Campaign_126_SCENARIO_OBJECT_1", 1))
		{
			return;
		}
		IEnumerable<CEnemyActor> source = ScenarioManager.Scenario.AllyMonsters.Where((CEnemyActor x) => x.Class.ID == "Helper126ID" && x.GetDistanceTo(player) <= 1);
		if (source.Any())
		{
			List<CTile> validTiles = source.Select((CEnemyActor x) => x.Tile).ToList();
			ScenarioScript.PickHexes("FH/FH_Scenario_Campaign_126_Give_Helper_Crate_HelpText", player, validTiles, OnGiveResourceToHelper, 1, animateActor: true, null, canSkip: true, canUndo: false);
		}
	}

	private void OnGiveResourceToHelper(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		CActor livingActor = selectedTiles[0].LivingActor;
		EscapeMap(livingActor);
		ability.TargetingActor.RemoveCharacterResource("FH_Scenario_Campaign_126_SCENARIO_OBJECT_1", 1);
		GiveXCratesToHelpers.Current++;
		if (GiveXCratesToHelpers.Current >= GiveXCratesToHelpers.Total)
		{
			GiveXCratesToHelpers.ScriptedIsComplete = true;
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void UpdateCrateHighlights()
	{
		ScenarioManager.ToggleTileHighlights(crateHighlights, enabled: false, new CVector4(1f, 1f, 0f, 1f));
		crateHighlights.Clear();
		crateHighlights.AddRange(from x in ScenarioManager.Scenario.Objects
			where !x.IsDead
			select x.ArrayIndex);
		ScenarioManager.ToggleTileHighlights(crateHighlights, enabled: true, new CVector4(1f, 1f, 0f, 1f));
	}
}
