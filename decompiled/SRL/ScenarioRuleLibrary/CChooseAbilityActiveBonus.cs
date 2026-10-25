using System;

namespace ScenarioRuleLibrary;

public class CChooseAbilityActiveBonus : CActiveBonus
{
	public CAbilityChoose ChooseAbility;

	public CAbility chosenAbility;

	public CChooseAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		if (ability is CAbilityChoose cAbilityChoose && actor is CPlayerActor)
		{
			ChooseAbility = cAbilityChoose;
			{
				foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
				{
					if (behaviour == EActiveBonusBehaviourType.EndActionAbility)
					{
						AddBespokeBehaviour(new CChooseAbilityActiveBonus_EndOfActionAbilityChoice(actor, cAbilityChoose, this));
					}
				}
				return;
			}
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Forgo Actions For Companion active bonus");
	}

	public void AbilityChosen(int abilityIndex)
	{
		if (abilityIndex < ChooseAbility.ChooseAbilities.Count)
		{
			chosenAbility = ChooseAbility.ChooseAbilities[abilityIndex];
		}
	}

	public void ResetChosenAbility()
	{
		chosenAbility = null;
	}
}
