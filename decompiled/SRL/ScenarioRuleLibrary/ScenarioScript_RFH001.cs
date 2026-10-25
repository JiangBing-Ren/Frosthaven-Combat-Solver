using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_RFH001 : ScenarioScript
{
	private CMap Room2;

	private CObjective_Scripted ReachDoorObjective;

	private CObjective_Scripted AttackMentorObjective;

	protected override bool Initialize()
	{
		Room2 = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_2");
		ReachDoorObjective = ScenarioManager.CurrentScenarioState.AllObjectives.SingleOrDefault((CObjective x) => x.EventIdentifier == "ReachDoorObjective") as CObjective_Scripted;
		AttackMentorObjective = ScenarioManager.CurrentScenarioState.AllObjectives.SingleOrDefault((CObjective x) => x.EventIdentifier == "AttackMentor") as CObjective_Scripted;
		if (ScenarioManager.CurrentScenarioState.Monsters.Count > 0)
		{
			EnemyState enemyState = ScenarioManager.CurrentScenarioState.Monsters[0];
			enemyState.CustomLocKey = "FH/FH_Tutorial_Narrator";
			if (enemyState.Actor != null)
			{
				enemyState.Actor.CustomLocKey = enemyState.CustomLocKey;
			}
		}
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnRoomRevealed && _tickRoomRevealed == Room2 && ReachDoorObjective != null)
		{
			ReachDoorObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Attack)
		{
			AttackMentorObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			ScenarioRuleClient.MessageHandler(new CEndRound_MessageData(GameState.InternalCurrentActor));
		}
	}
}
