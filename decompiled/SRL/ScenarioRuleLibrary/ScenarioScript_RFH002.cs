namespace ScenarioRuleLibrary;

public class ScenarioScript_RFH002 : ScenarioScript
{
	public CObjective_Scripted MoveObjective;

	public int _tutStep;

	protected override string SerializeInternal()
	{
		return _tutStep.ToString();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			_tutStep = int.Parse(storage);
		}
	}

	protected override bool Initialize()
	{
		MoveObjective = GetObjective<CObjective_Scripted>("MoveObjective");
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
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound && ScenarioManager.CurrentScenarioState.RoundNumber == 2)
		{
			ScenarioManager.Scenario.PlayerActors[0].RemoveNegativeConditionToken(CCondition.ENegativeCondition.Immobilize);
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		if (_tickActor is CPlayerActor && MoveObjective.IsActive)
		{
			MoveObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	public void EndRoundEarly()
	{
		ScenarioRuleClient.MessageHandler(new CEndRound_MessageData(GameState.InternalCurrentActor));
	}
}
