using System;

namespace ScenarioRuleLibrary;

public class CForgoActionsForCompanionActiveBonus : CActiveBonus
{
	public CAbilityForgoActionsForCompanion ForgoActionsForCompanionAbility;

	public bool TopActionToggled;

	public bool BottomActionToggled;

	private CPlayerActor m_playerActor;

	public CForgoActionsForCompanionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		if (ability is CAbilityForgoActionsForCompanion forgoActionsForCompanionAbility && actor is CPlayerActor cPlayerActor && cPlayerActor.CharacterClass.CompanionSummonData != null)
		{
			m_playerActor = cPlayerActor;
			ForgoActionsForCompanionAbility = forgoActionsForCompanionAbility;
			return;
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Forgo Actions For Companion active bonus");
	}

	public void ResetToggles()
	{
		TopActionToggled = false;
		BottomActionToggled = false;
	}

	public void ApplyToggledActiveBonusesToCompanionSummon()
	{
		if (TopActionToggled && !m_playerActor.SkipTopCardAction)
		{
			BaseCard.AddActiveBonus(ForgoActionsForCompanionAbility.ForgoTopActionAbility, m_playerActor.CompanionSummon, m_playerActor);
			m_playerActor.SkipTopCardAction = true;
		}
		if (BottomActionToggled && !m_playerActor.SkipBottomCardAction)
		{
			BaseCard.AddActiveBonus(ForgoActionsForCompanionAbility.ForgoBottomActionAbility, m_playerActor.CompanionSummon, m_playerActor);
			m_playerActor.SkipBottomCardAction = true;
		}
	}
}
