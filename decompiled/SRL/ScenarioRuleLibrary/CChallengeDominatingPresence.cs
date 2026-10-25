using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CChallengeDominatingPresence : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1515u;

	public override string Name => "dominating_presence";

	public override void Reset()
	{
		base.Reset();
		actorsAppliedTo.Clear();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			actorsAppliedTo = JsonConvert.DeserializeObject<List<string>>(storage);
		}
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(actorsAppliedTo);
	}

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnSpawn;
	}

	public static bool ValidateEliteness(CActor actor)
	{
		if (!(actor is CEnemyActor cEnemyActor))
		{
			return false;
		}
		if (cEnemyActor.IsElite())
		{
			return false;
		}
		return ScenarioManager.Scenario.AllAliveMonsters.Any((CEnemyActor x) => x.Type == actor.Type && x.IsElite() && x.MonsterClass.NonEliteVariant == actor.Class);
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.trigger == Trigger.OnSpawn && triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
		{
			actorsAppliedTo.Add(cEnemyActor.ActorGuid);
			CAbilityShield obj = CAbilityShield.CreateDefaultShield(1, 1, 1, isMonster: true, null, "DominatingPresenceChallengeShield") as CAbilityShield;
			obj.ActiveBonusData.Hidden = true;
			obj.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			obj.ActiveBonusData.IsAura = true;
			obj.ActiveBonusData.AuraFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
			obj.ActiveBonusData.Requirements = new CAbilityRequirements
			{
				Vars = new List<(string, TValue)> { ("ValidateEliteness", new TValue(CExpression.Parse("CChallengeDominatingPresence.ValidateEliteness(Actor)"))) }
			};
			obj.Start(cEnemyActor, cEnemyActor);
			obj.ApplyToActor(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
		}
	}
}
