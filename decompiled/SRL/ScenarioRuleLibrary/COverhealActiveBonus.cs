using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class COverhealActiveBonus : CActiveBonus
{
	public COverhealActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax:
			case EActiveBonusBehaviourType.AddAccumulativeOverheal:
			case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHeal:
				AddBespokeBehaviour(new COverhealActiveBonus_AddAccumulativeOverheal(actor, ability, this, behaviour));
				break;
			case EActiveBonusBehaviourType.SetMinimumOverheal:
				AddBespokeBehaviour(new COverhealActiveBonus_SetMinimumOverheal(actor, ability, this));
				break;
			}
		}
	}

	public void CheckForOverhealStrengthUpdate()
	{
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			switch (bespokeBehaviour.BehaviourType)
			{
			case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax:
			case EActiveBonusBehaviourType.AddAccumulativeOverheal:
			case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHeal:
			{
				COverhealActiveBonus_AddAccumulativeOverheal cOverhealActiveBonus_AddAccumulativeOverheal = bespokeBehaviour as COverhealActiveBonus_AddAccumulativeOverheal;
				cOverhealActiveBonus_AddAccumulativeOverheal?.UpdateCurrentOverhealStrength(Actor);
				foreach (CActor item in ValidActorsInRangeOfAura)
				{
					cOverhealActiveBonus_AddAccumulativeOverheal?.UpdateCurrentOverhealStrength(item);
				}
				break;
			}
			case EActiveBonusBehaviourType.SetMinimumOverheal:
				(bespokeBehaviour as COverhealActiveBonus_SetMinimumOverheal)?.UpdateCurrentOverhealStrength();
				break;
			}
		}
	}

	public override void Finish()
	{
		List<CActor> list = new List<CActor>(ValidActorsInRangeOfAura);
		base.Finish();
		using (new ActiveBonusCalculationScope())
		{
			foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
			{
				switch (bespokeBehaviour.BehaviourType)
				{
				case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax:
				case EActiveBonusBehaviourType.AddAccumulativeOverheal:
				case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHeal:
				{
					COverhealActiveBonus_AddAccumulativeOverheal cOverhealActiveBonus_AddAccumulativeOverheal = bespokeBehaviour as COverhealActiveBonus_AddAccumulativeOverheal;
					cOverhealActiveBonus_AddAccumulativeOverheal?.GetAccumulativeOverhealAndUpdate(Actor, excludeSelf: true);
					foreach (CActor item in list)
					{
						cOverhealActiveBonus_AddAccumulativeOverheal?.GetAccumulativeOverhealAndUpdate(item, excludeSelf: true);
					}
					break;
				}
				case EActiveBonusBehaviourType.SetMinimumOverheal:
					(bespokeBehaviour as COverhealActiveBonus_SetMinimumOverheal)?.CheckForAllMinimumOverhealsAndUpdate(excludeSelf: true);
					break;
				}
			}
		}
	}
}
