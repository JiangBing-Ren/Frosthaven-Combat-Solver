namespace ScenarioRuleLibrary;

public class ScenarioScript_FH018 : ScenarioScript
{
	private CObjective _objGuard;

	protected override bool Initialize()
	{
		if (!GetObjective("Guard", out _objGuard))
		{
			return false;
		}
		return true;
	}

	protected override void DeserializeInternal(string s)
	{
		CheckGuardObjective();
	}

	protected override void TickPhaseInternal()
	{
		CheckGuardObjective();
	}

	private void CheckGuardObjective()
	{
		if (ScenarioManager.Scenario.AllyMonsters.Find((CEnemyActor a) => a.Class.ID.Contains("AlgoxGuard")) != null && !_objGuard.IsActive)
		{
			_objGuard.SetActivation(active: true);
		}
	}
}
