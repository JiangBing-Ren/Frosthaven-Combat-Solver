using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class CChallengeSleepThrashing : CChallenge
{
	public override uint ID => 1489u;

	public override string Name => "sleep_thrashing";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnRest;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		CActor actor = triggerParams.actor;
		CPlayerActor playerActor = actor as CPlayerActor;
		if (playerActor == null)
		{
			return;
		}
		foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x != playerActor && (x.Type == CActor.EType.Player || x.Type == CActor.EType.Ally || x.Type == CActor.EType.HeroSummon) && Point.HexDist(x.ArrayIndex, playerActor.ArrayIndex) <= 1))
		{
			CAbility cAbility = CAbilityDamage.CreateDefaultDamage(ScenarioManager.HazardousTerrainDamage, 0, 1, item.IsMonsterType, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			GameState.PendingOnRestAbilities.Add((cAbility, item));
		}
	}
}
