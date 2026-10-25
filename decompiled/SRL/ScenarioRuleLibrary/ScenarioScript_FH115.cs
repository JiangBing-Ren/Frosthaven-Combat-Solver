using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH115 : ScenarioScript
{
	private IEnumerable<Point> PropPositions()
	{
		yield return new Point(3, 8);
		yield return new Point(5, 10);
		yield return new Point(8, 10);
		yield return new Point(7, 7);
		yield return new Point(10, 8);
		yield return new Point(12, 6);
		yield return new Point(9, 5);
	}

	private IEnumerable<CObjectActor> Props()
	{
		return ScenarioManager.Scenario.AllObjects.Where((CObjectActor x) => PropPositions().Contains(x.ArrayIndex));
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = Props().ToList().Count((CObjectActor x) => x.IsDead);
		return new List<object> { num };
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		CActor tickActor = _tickActor;
		CObjectActor pillarActor = tickActor as CObjectActor;
		if (pillarActor == null || !PropPositions().Any((Point x) => x == pillarActor.ArrayIndex))
		{
			return;
		}
		CAbilityCreateTerrain.SpawnTerrain(ScenarioManager.ObjectImportType.TerrainRubble, pillarActor.Tile);
		foreach (CActor item in GameState.GetActorsInRange(pillarActor.ArrayIndex, 1))
		{
			if (!item.IsDead)
			{
				DealScenarioDamage(item, ScenarioManager.TrapDamage);
			}
		}
	}
}
