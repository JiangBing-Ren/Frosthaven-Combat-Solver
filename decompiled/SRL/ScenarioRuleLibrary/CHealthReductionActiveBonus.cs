using System;
using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CHealthReductionActiveBonus : CActiveBonus
{
	public CHealthReductionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (IsAura)
		{
			foreach (CActor item in ValidActorsInRangeOfAura)
			{
				CheckForHealthReductionUpdates(item);
			}
			return;
		}
		CheckForHealthReductionUpdates(Actor);
	}

	public override void Finish()
	{
		List<CActor> list = (IsAura ? ValidActorsInRangeOfAura.ToList() : new List<CActor> { Actor });
		base.Finish();
		foreach (CActor item in list)
		{
			CheckForHealthReductionUpdates(item);
		}
	}

	public override void FinishRecalcValidActors(List<CActor> oldActors)
	{
		using (new ActiveBonusCalculationScope())
		{
			base.FinishRecalcValidActors(oldActors);
			foreach (CActor item in ValidActorsInRangeOfAura.Concat(oldActors).Distinct().ToList())
			{
				CheckForHealthReductionUpdates(item);
			}
		}
	}

	private void CheckForHealthReductionUpdates(CActor actorToCheck)
	{
		int num = int.MaxValue;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actorToCheck, CAbility.EAbilityType.HealthReduction);
		list.RemoveAll((CActiveBonus b) => b.Finishing() || b.Finished());
		if (list.Count > 0)
		{
			foreach (CActiveBonus item in list)
			{
				int num2 = item.ReferenceStrength(item.Ability, actorToCheck);
				if (num2 < num)
				{
					num = num2;
				}
			}
			if (actorToCheck.HealthReduction != num)
			{
				actorToCheck.HealthReduction = num;
				actorToCheck.Health = Math.Min(actorToCheck.Health, actorToCheck.MaxHealth);
				CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(actorToCheck, actorToCheck, actorToCheck.Health);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		else
		{
			actorToCheck.HealthReduction = null;
			actorToCheck.Health = actorToCheck.MaxHealth;
			CRefreshActorHealth_MessageData message2 = new CRefreshActorHealth_MessageData(actorToCheck, actorToCheck, actorToCheck.Health);
			ScenarioRuleClient.MessageHandler(message2);
		}
	}
}
