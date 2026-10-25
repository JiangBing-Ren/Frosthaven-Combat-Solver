using System;

namespace ScenarioRuleLibrary;

public class CSummonActiveBonus : CActiveBonus
{
	public CActor m_SummonActor { get; private set; }

	public CSummonActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD)
	{
		m_SummonActor = actor;
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.CastAbilityFromSummon)
			{
				if (!(ability is CAbilitySummon ability2))
				{
					throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Summon active bonus");
				}
				AddBespokeBehaviour(new CSummonActiveBonus_CastAbilityFromSummon(actor, ability2, this));
			}
		}
		if (m_SummonActor is CHeroSummonActor cHeroSummonActor)
		{
			cHeroSummonActor.CachedSummonCard = ability.GetAbilityBaseCard();
		}
	}

	public override bool Finished()
	{
		if (!ScenarioManager.Scenario.HasActor(m_SummonActor))
		{
			return !m_SummonActor.PhasedOut;
		}
		return false;
	}
}
