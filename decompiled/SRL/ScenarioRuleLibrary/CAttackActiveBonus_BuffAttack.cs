using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAttackActiveBonus_BuffAttack : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	private bool m_TriggeredOnAttack;

	private bool isBuffed;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public bool ApplyToMonsterStats
	{
		get
		{
			if (ActiveBonus?.Ability != null)
			{
				return ActiveBonus.Ability.GetMiscVar("ApplyToMonsterStats").GetBool();
			}
			return false;
		}
	}

	public CAttackActiveBonus_BuffAttack(CActor actor, CAbilityAttack ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType type)
		: base(actor, ability, activeBonus)
	{
		m_TriggeredOnAttack = false;
		m_type = type;
		if ((ability.Flags & CAbility.EAbilityFlag.ModifiesTrophyTokens) != 0 && ability.TargetingActor is CPlayerActor cPlayerActor)
		{
			cPlayerActor.SetTrophyTokens(cPlayerActor.GetTrophyTokens() + ability.Tokens);
		}
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentAbility, activeBonus, toggledOn);
		if (currentAbility is CAbilityAttack cAbilityAttack)
		{
			if (cAbilityAttack.AttackSummary != null)
			{
				cAbilityAttack.AttackSummary.UpdateTargetData(cAbilityAttack, cAbilityAttack.ActiveSingleTargetItems, cAbilityAttack.ActiveSingleTargetActiveBonuses, cAbilityAttack.AreaEffectLocked);
			}
			CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(m_Actor);
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = m_Actor;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = cAbilityAttack;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = cAbilityAttack.AttackSummary?.Copy();
			ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
		}
	}

	public override void OnAttackStart(CAbilityAttack attackAbility)
	{
	}

	private void ApplyBuffs(CAbilityAttack attackAbility, CActor target, bool removeImmediately)
	{
		if (m_ActiveBonus.HasTracker && m_ActiveBonus.Remaining == 0)
		{
			Finish();
		}
		else
		{
			if ((attackAbility != null && ApplyToMonsterStats) || !IsValidTarget(attackAbility, target) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor) || !m_ActiveBonus.SetupTriggerContext(this, null, attackAbility, attackAbility.TargetingActor, target) || (ActiveBonus.Ability.ActiveBonusData.IsSingleTargetBonus && ActiveBonus.Ability.ActiveBonusData.IsToggleBonus && ActiveBonus.SingleTarget != target))
			{
				return;
			}
			bool flag = isBuffed;
			isBuffed |= CalculateBuffs(attackAbility, target, out var _, out var _, out var _, out var _, out var _, out var _, out var _);
			isBuffed |= m_ActiveBonusData.StrengthIsScalar;
			isBuffed |= m_ActiveBonusData.AbilityStrengthIsScalar;
			isBuffed |= ReferenceAdvantage(attackAbility, target);
			CAbility cAbility = attackAbility;
			if (ActiveBonus.Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				CAttackSummary.TargetSummary targetSummary = attackAbility.AttackSummary?.FindTarget(target);
				if (targetSummary != null)
				{
					cAbility = targetSummary.AttackAbilityWithOverrides;
				}
			}
			if (m_Ability.NegativeConditions != null && m_Ability.NegativeConditions.Count > 0)
			{
				foreach (CCondition.ENegativeCondition negCondition in m_Ability.NegativeConditions.Keys)
				{
					if (!cAbility.NegativeConditions.ContainsKey(negCondition) && negCondition != CCondition.ENegativeCondition.NA)
					{
						CAbility.EAbilityType abilityType = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCondition.ToString());
						cAbility.NegativeConditions.Add(negCondition, CAbility.CreateAbility(abilityType, cAbility.AbilityFilter, isMonster: false, attackAbility.IsTargetedAbility));
					}
					isBuffed = true;
				}
			}
			if (m_Ability.PositiveConditions != null && m_Ability.PositiveConditions.Count > 0)
			{
				foreach (CCondition.EPositiveCondition posCondition in m_Ability.PositiveConditions.Keys)
				{
					if (!cAbility.PositiveConditions.ContainsKey(posCondition) && posCondition != CCondition.EPositiveCondition.NA)
					{
						CAbility.EAbilityType abilityType2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCondition.ToString());
						cAbility.PositiveConditions.Add(posCondition, CAbility.CreateAbility(abilityType2, cAbility.AbilityFilter, isMonster: false, attackAbility.IsTargetedAbility));
					}
					isBuffed = true;
				}
			}
			if (isBuffed)
			{
				m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
				m_TriggeredOnAttack = true;
				if (m_ActiveBonus.HasTracker && (!AppliedThisAction || !m_Ability.ActiveBonusData.EntireAction))
				{
					if (m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
					{
						foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
						{
							attackEffect.ResetStackedSubAbilityForBuffOnAttack();
						}
					}
					if (removeImmediately)
					{
						m_ActiveBonus.UpdateXPTracker();
						m_ActiveBonus.FinishIfNoRemaining();
					}
				}
			}
			AppliedThisAction = true;
			isBuffed = flag;
		}
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		if (m_Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffAttack))
		{
			ApplyBuffs(attackAbility, target, removeImmediately: true);
		}
	}

	public override void OnAttackingSingleTargetStart(CAbilityAttack ability, CActor target)
	{
		if (m_Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffSingleAttack))
		{
			_ = ability.CurrentAttackingTargetSummary.AttackAbilityWithOverrides;
			ApplyBuffs(ability, target, removeImmediately: false);
		}
	}

	public override void OnAttackAbilityFinished(CAbilityAttack attackAbility)
	{
		if (m_TriggeredOnAttack || (m_ActiveBonusData.IsToggleBonus && m_ActiveBonus.ToggledBonus))
		{
			OnBehaviourTriggered();
			m_TriggeredOnAttack = false;
			TriggerSubAbilities();
		}
	}

	private void RevertBonuses(CAbilityAttack attackAbility)
	{
		foreach (CAttackSummary.TargetSummary target in attackAbility.AttackSummary.Targets)
		{
			if (target == attackAbility.CurrentAttackingTargetSummary)
			{
				continue;
			}
			CAbility attackAbilityWithOverrides = target.AttackAbilityWithOverrides;
			if (m_Ability.NegativeConditions != null && m_Ability.NegativeConditions.Count > 0)
			{
				foreach (CCondition.ENegativeCondition key in m_Ability.NegativeConditions.Keys)
				{
					if (key != CCondition.ENegativeCondition.NA && !attackAbility.OriginalNegativeConditions.ContainsKey(key))
					{
						attackAbilityWithOverrides.NegativeConditions.Remove(key);
					}
				}
			}
			if (m_Ability.PositiveConditions == null || m_Ability.PositiveConditions.Count <= 0)
			{
				continue;
			}
			foreach (CCondition.EPositiveCondition key2 in m_Ability.PositiveConditions.Keys)
			{
				if (key2 != CCondition.EPositiveCondition.NA && !attackAbility.OriginalPositiveConditions.ContainsKey(key2))
				{
					attackAbilityWithOverrides.PositiveConditions.Remove(key2);
				}
			}
		}
	}

	public override void OnAttackFinished(CAbilityAttack attackAbility, CActor target, int damageDealt)
	{
		base.OnAttackFinished(attackAbility, target, damageDealt);
		if (!m_Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffSingleAttack))
		{
			return;
		}
		RevertBonuses(attackAbility);
		if (!ActiveBonus.Ability.ActiveBonusData.IsSingleTargetBonus || (ActiveBonus.SingleTarget == target && ActiveBonus.Ability.ActiveBonusData.IsToggleBonus))
		{
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
			TriggerSubAbilities();
		}
	}

	private void TriggerSubAbilities()
	{
		if (m_ActiveBonusData.GetMiscVar("TriggerAbilityAfterAttack").GetBool())
		{
			CustomAbility.PerformActiveBonusAbility(ActiveBonus);
		}
		if (m_Ability.SubAbilities != null)
		{
			PhaseManager.CurrentPhaseAction.StackNextAbilities(m_Ability.SubAbilities.Where((CAbility x) => !x.GetMiscVar("DontTriggerFromBuffAttack").GetBool()).ToList(), m_Ability.TargetingActor, killAfter: false, stackToNextCurrent: true);
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 0;
		}
		if (ability == null || (ability.AbilityType == CAbility.EAbilityType.Attack && !m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target)))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _, out var _, out var _, out var _, out var _);
			return strength;
		}
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 1;
		}
		if (ability == null || (ability.AbilityType == CAbility.EAbilityType.Attack && m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target)))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _, out var _, out var _, out var _, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 1;
		}
		if (ability == null || (ability.AbilityType == CAbility.EAbilityType.Attack && m_ActiveBonusData.AbilityStrengthIsScalar && !m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability, target)))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _, out var _, out var _, out var _, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 0;
		}
		if (ability == null || (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target)))
		{
			CalculateBuffs(ability, target, out var _, out var xp, out var _, out var _, out var _, out var _, out var _);
			return xp;
		}
		return 0;
	}

	public int ReferencePierce(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 0;
		}
		if (ability == null || (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target)))
		{
			CalculateBuffs(ability, target, out var _, out var _, out var pierce, out var _, out var _, out var _, out var _);
			return pierce;
		}
		return 0;
	}

	public int ReferenceRange(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 0;
		}
		if (ability == null || ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var range, out var _, out var _, out var _);
			return range;
		}
		return 0;
	}

	public bool ReferenceTreatAsRange(CAbility ability, CActor target)
	{
		AbilityData.MiscAbilityData miscAbilityData = m_Ability.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.TreatAsRange == true)
		{
			if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(ability.TargetingActor))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public int ReferenceTargetsCount(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 0;
		}
		if (ability == null || ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var _, out var targetsCount, out var _, out var _);
			return targetsCount;
		}
		return 0;
	}

	public List<CCondition.EPositiveCondition> ReferencePositiveConditions(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var _, out var _, out var positiveConditions, out var _);
			return positiveConditions;
		}
		return null;
	}

	public List<CCondition.ENegativeCondition> ReferenceNegativeConditions(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			CalculateBuffs(ability, target, out var _, out var _, out var _, out var _, out var _, out var _, out var negativeConditions);
			return negativeConditions;
		}
		return null;
	}

	public bool ReferenceAdvantage(CAbility ability, CActor target)
	{
		AbilityData.MiscAbilityData miscAbilityData = m_Ability.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.AttackHasAdvantage == true)
		{
			if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(ability.TargetingActor))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool ReferenceDisadvantage(CAbility ability, CActor target)
	{
		AbilityData.MiscAbilityData miscAbilityData = m_Ability.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.AttackHasDisadvantage == true)
		{
			if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(ability.TargetingActor))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool CalculateBuffs(CAbility ability, CActor target, out int strength, out int xp, out int pierce, out int range, out int targetsCount, out List<CCondition.EPositiveCondition> positiveConditions, out List<CCondition.ENegativeCondition> negativeConditions)
	{
		CAbilityAttack attackAbility = ability as CAbilityAttack;
		CPlayerActor actor = ability?.FilterActor as CPlayerActor;
		if (!ActiveBonus.WillBeValidOnReachTargetActor(ability, target) || !m_Ability.IsSpeedValidForActiveBonus(actor))
		{
			strength = 0;
			xp = 0;
			pierce = 0;
			range = 0;
			targetsCount = 0;
			positiveConditions = null;
			negativeConditions = null;
			return m_ActiveBonusData.AlwaysBuff;
		}
		strength = CheckStatIsBasedOnXType(ability);
		xp = m_XP;
		pierce = 0;
		range = 0;
		targetsCount = 0;
		positiveConditions = null;
		negativeConditions = null;
		if (target != null && m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
		{
			foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
			{
				attackEffect.GetBonus(target, attackAbility, out var attackBuff, out var xpBuff, out var rangeBuff, out var targetsCountBuff, out var pierceBuff, out var _, out var positiveConditionsBuff, out var negativeConditionsBuff, ActiveBonus);
				strength += attackBuff;
				xp += xpBuff;
				range += rangeBuff;
				targetsCount += targetsCountBuff;
				pierce += pierceBuff;
				positiveConditions = positiveConditionsBuff;
				negativeConditions = negativeConditionsBuff;
			}
		}
		if (!m_ActiveBonusData.AlwaysBuff && strength == 0 && xp == 0 && pierce == 0 && range == 0 && targetsCount == 0 && positiveConditions == null)
		{
			return negativeConditions != null;
		}
		return true;
	}

	public override bool StackActiveBonusInlineAbility(CActor target, CAbility ability)
	{
		CAbilityAttack attackAbility = ability as CAbilityAttack;
		List<CAbility> list = new List<CAbility>();
		if (m_ActiveBonusData.AbilityData != null)
		{
			list.Add(m_ActiveBonusData.AbilityData);
		}
		if (list.Count > 0)
		{
			foreach (CAbility item in list)
			{
				item.StackedAttackEffectAbility = true;
			}
		}
		if (m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
		{
			if (!m_ActiveBonus.SetupTriggerContext(this, m_ActiveBonusData.AbilityData, cAbilityAttack, null, target))
			{
				return false;
			}
			bool result = false;
			{
				foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
				{
					if (attackEffect.StackAbilityForBonus(target, attackAbility, list))
					{
						result = true;
					}
				}
				return result;
			}
		}
		return false;
	}

	public override void UpdateActiveBonusInlineAbilityTarget(CActor target)
	{
		if (!(m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack) || cAbilityAttack.AttackEffects.Count <= 0)
		{
			return;
		}
		foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
		{
			attackEffect.UpdateStackedAbilityForBonusTarget(target);
		}
	}

	public override void CleanUpBeforeSave()
	{
		base.CleanUpBeforeSave();
		m_Ability.ResetStatBasedOnXAddedStats(m_Ability.StatIsBasedOnXEntries);
		m_Ability.SetStatBasedOnX(m_Ability.TargetingActor, m_Ability.StatIsBasedOnXEntries);
		m_Strength = m_Ability.Strength;
	}
}
