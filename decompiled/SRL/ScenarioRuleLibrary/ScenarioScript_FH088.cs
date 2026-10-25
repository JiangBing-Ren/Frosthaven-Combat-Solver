using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH088 : ScenarioScript
{
	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnRoomRevealed)
		{
			if (ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.PropTile.m_ArrayIndex == new Point(7, 8)) is CObjectDoor { DoorIsClosed: false } && OncePerRound())
			{
				ScenarioManager.CurrentScenarioState.Spawners[0].SetActive(active: true);
				ScenarioManager.CurrentScenarioState.Spawners[0].OnStartRound(ScenarioScript.PartySize, ScenarioScript.RoundNumber);
				ScenarioManager.CurrentScenarioState.Spawners[0].UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			}
		}
		else
		{
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 23);
		}
	}
}
