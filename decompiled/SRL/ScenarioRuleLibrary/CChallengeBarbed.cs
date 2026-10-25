using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CChallengeBarbed : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1492u;

	public override string Name => "barbed";

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
		yield return Trigger.OnEnemySufferDamagePostRetaliate;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		switch (triggerParams.trigger)
		{
		case Trigger.OnSpawn:
			if (triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
			{
				actorsAppliedTo.Add(cEnemyActor.ActorGuid);
				CAbility cAbility = CAbilityRetaliate.CreateDefaultRetaliate(2, 1, 1, isMonster: true);
				cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				cAbility.Flags = CAbility.EAbilityFlag.AddedFromChallenge;
				cAbility.Start(cEnemyActor, cEnemyActor);
				cAbility.ApplyToActor(cEnemyActor);
			}
			break;
		case Trigger.OnEnemySufferDamagePostRetaliate:
			if (!(triggerParams.actor is CEnemyActor actor))
			{
				break;
			}
			{
				foreach (CActiveBonus item in from x in CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.Retaliate)
					where (x.Ability.Flags & CAbility.EAbilityFlag.AddedFromChallenge) != 0
					select x)
				{
					item.Finish();
				}
				break;
			}
		}
	}
}
