using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_RFH006 : ScenarioScript
{
	private List<CSpawner> spawners = new List<CSpawner>();

	protected override List<object> GetConclusionParametersInternal()
	{
		int roundNumber = ScenarioManager.CurrentScenarioState.RoundNumber;
		return new List<object> { roundNumber };
	}

	protected override bool Initialize()
	{
		spawners = ScenarioManager.CurrentScenarioState.Spawners.ToList();
		return base.Initialize();
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (!(_tickActor is CObjectActor))
		{
			return;
		}
		foreach (CSpawner spawner in spawners)
		{
			spawner.SetActive(active: false);
		}
	}
}
