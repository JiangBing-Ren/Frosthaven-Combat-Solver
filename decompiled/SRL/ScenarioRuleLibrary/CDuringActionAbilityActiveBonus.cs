namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus : CActiveBonus
{
	[StateDump(Skip = true)]
	public CAbility AddAbility;

	public CDuringActionAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.DuringActionAbility:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbility(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttacked:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnKill:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnKill(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnMoved:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnMoved(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnMovedORCarried:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedORCarried(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnCreated:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnCreated(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnFinishedMovement(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnMovedIntoCasterHex:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedIntoCasterHex(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackAbilityFinished:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackAbilityFinished(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackFinished:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackStart:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackStart(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnActionEndedAndLongRest:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEndedAndLongRest(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnLongRest:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnShortRest:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnShortRest(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnDamaged:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnDamaged(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnDeath:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityAfterDeath:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityAfterDeath(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttacking:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnPreventedDamage:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnAbilityEnded:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnHealed:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnHealed(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnHealedToFull:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnActionEnded:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEnded(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.DuringActionAbilityOnConditionEnded:
				AddBespokeBehaviour(new CDuringActionAbilityActiveBonus_TriggerAbilityOnConditionEnded(actor, ability, this));
				break;
			}
		}
	}

	public override bool ShouldRestartAbilityOnToggle(CAbility currentAbility)
	{
		if (HasBehaviour(EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement))
		{
			return false;
		}
		return base.ShouldRestartAbilityOnToggle(currentAbility);
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseAction)
		{
			RestrictActiveBonus(Actor);
			if (m_BespokeBehaviour != null)
			{
				m_BespokeBehaviour.OnBehaviourTriggered();
			}
			if (HasTracker)
			{
				UpdateXPTracker();
				FinishIfNoRemaining();
			}
		}
		else
		{
			Log.ErrorWithReport("Invalid phase for DuringAction Ability");
		}
	}
}
