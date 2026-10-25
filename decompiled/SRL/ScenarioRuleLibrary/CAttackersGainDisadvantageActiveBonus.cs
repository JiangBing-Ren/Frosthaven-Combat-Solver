namespace ScenarioRuleLibrary;

internal class CAttackersGainDisadvantageActiveBonus : CActiveBonus
{
	public CAttackersGainDisadvantageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}

	public void GainDisadvantageTriggered(bool processTracker = true)
	{
		RestrictActiveBonus(Actor);
		if (HasTracker)
		{
			UpdateXPTracker(processTracker);
			FinishIfNoRemaining();
		}
	}
}
