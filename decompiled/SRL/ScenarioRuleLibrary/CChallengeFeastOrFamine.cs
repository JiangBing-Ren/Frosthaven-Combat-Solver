using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CChallengeFeastOrFamine : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1496u;

	public override string Name => "feast_or_famine";

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
		yield return Trigger.OnStartScenario;
		yield return Trigger.OnSpawn;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams != null)
		{
			if (triggerParams.trigger == Trigger.OnStartScenario)
			{
				MonsterClassManager.RemoveAllModifierCards(MonsterClassManager.Enemy2MonsterAttackModifierDeck, new List<string> { "AMC_Basic_P0" });
				MonsterClassManager.RemoveAllModifierCards(MonsterClassManager.EnemyMonsterAttackModifierDeck, new List<string> { "AMC_Basic_P0" });
			}
			else if (triggerParams.trigger == Trigger.OnSpawn && triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
			{
				actorsAppliedTo.Add(cEnemyActor.ActorGuid);
				CAbility cAbility = CAbilityImmunityTo.CreateDefaultImmunityTo(new List<CAbility.EAbilityType> { CAbility.EAbilityType.Curse }, new List<CAbility.EAttackType>(), immuneToSelfDamage: false);
				cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				cAbility.ActiveBonusData.Hidden = true;
				cAbility.Start(cEnemyActor, cEnemyActor);
				cAbility.ApplyToActor(cEnemyActor);
			}
		}
	}
}
