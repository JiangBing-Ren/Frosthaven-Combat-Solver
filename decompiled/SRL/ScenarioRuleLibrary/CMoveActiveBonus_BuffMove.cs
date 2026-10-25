using System.Linq;

namespace ScenarioRuleLibrary;

public class CMoveActiveBonus_BuffMove : CBespokeBehaviour
{
	public bool m_TriggeredOnMove;

	public bool m_PayedMovementPoints;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffMove;

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

	public CMoveActiveBonus_BuffMove(CActor actor, CAbilityMove ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		m_TriggeredOnMove = false;
		m_PayedMovementPoints = false;
	}

	public override void OnMoving(CAbilityMove moveAbility)
	{
		if (!IsValidMoveType(moveAbility) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(moveAbility.TargetingActor))
		{
			return;
		}
		bool flag = CalculateBuffs(out var _, out var _, out var _, out var _, out var _, out var _);
		flag |= m_ActiveBonusData.StrengthIsScalar;
		if (m_Ability.NegativeConditions != null && m_Ability.NegativeConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition negCondition in m_Ability.NegativeConditions.Keys)
			{
				if (!moveAbility.NegativeConditions.ContainsKey(negCondition) && negCondition != CCondition.ENegativeCondition.NA)
				{
					CAbility.EAbilityType abilityType = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCondition.ToString());
					moveAbility.NegativeConditions.Add(negCondition, CAbility.CreateAbility(abilityType, moveAbility.AbilityFilter, isMonster: false, moveAbility.IsTargetedAbility));
					flag = true;
				}
			}
		}
		if (m_Ability.PositiveConditions != null && m_Ability.PositiveConditions.Count > 0)
		{
			foreach (CCondition.EPositiveCondition posCondition in m_Ability.PositiveConditions.Keys)
			{
				if (!moveAbility.PositiveConditions.ContainsKey(posCondition) && posCondition != CCondition.EPositiveCondition.NA)
				{
					CAbility.EAbilityType abilityType2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCondition.ToString());
					moveAbility.PositiveConditions.Add(posCondition, CAbility.CreateAbility(abilityType2, moveAbility.AbilityFilter, isMonster: false, moveAbility.IsTargetedAbility));
					flag = true;
				}
			}
		}
		if (flag)
		{
			m_ActiveBonus.RestrictActiveBonus(moveAbility.TargetingActor);
			m_TriggeredOnMove = true;
		}
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		if (endedAbility.AbilityInterruptCopy == null && (m_PayedMovementPoints || ((m_TriggeredOnMove || ActiveBonus.Ability.ActiveBonusData.GetMiscVar("TriggerOnSkippedToo").GetBool()) && endedAbility.AbilityType == CAbility.EAbilityType.Move && endedAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)))
		{
			OnBehaviourTriggered();
			m_TriggeredOnMove = false;
			m_PayedMovementPoints = false;
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentAbility, activeBonus, toggledOn);
		if (currentAbility is CAbilityMove cAbilityMove)
		{
			cAbilityMove.Perform();
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (ability != null && ApplyToMonsterStats)
		{
			return 0;
		}
		if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility) && !m_ActiveBonusData.StrengthIsScalar))
		{
			CalculateBuffs(out var strength, out var _, out var _, out var _, out var _, out var _);
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
		if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility) && m_ActiveBonusData.StrengthIsScalar))
		{
			CalculateBuffs(out var strength, out var _, out var _, out var _, out var _, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		if (ActiveBonus.IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (ability != null && ApplyToMonsterStats)
			{
				return 0;
			}
			if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility)))
			{
				CalculateBuffs(out var _, out var xp, out var _, out var _, out var _, out var _);
				return xp;
			}
		}
		return 0;
	}

	public bool? ReferenceJump(CAbility ability, CActor target)
	{
		if (ActiveBonus.IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (ability != null && ApplyToMonsterStats)
			{
				return null;
			}
			if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility)))
			{
				CalculateBuffs(out var _, out var _, out var jump, out var _, out var _, out var _);
				return jump;
			}
		}
		return null;
	}

	public bool? ReferenceFly(CAbility ability, CActor target)
	{
		if (ActiveBonus.IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (ability != null && ApplyToMonsterStats)
			{
				return null;
			}
			if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility)))
			{
				CalculateBuffs(out var _, out var _, out var _, out var fly, out var _, out var _);
				return fly;
			}
		}
		return null;
	}

	public bool? ReferenceIgnoreDifficultTerrain(CAbility ability, CActor target)
	{
		if (ActiveBonus.IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (ability != null && ApplyToMonsterStats)
			{
				return null;
			}
			if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility)))
			{
				CalculateBuffs(out var _, out var _, out var _, out var _, out var ignoreDifficultTerrain, out var _);
				return ignoreDifficultTerrain;
			}
		}
		return null;
	}

	public bool? ReferenceIgnoreHazardousTerrain(CAbility ability, CActor target)
	{
		if (ActiveBonus.IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (ability != null && ApplyToMonsterStats)
			{
				return null;
			}
			if (ability == null || (ability is CAbilityMove moveAbility && IsValidMoveType(moveAbility)))
			{
				CalculateBuffs(out var _, out var _, out var _, out var _, out var _, out var ignoreHazardousTerrain);
				return ignoreHazardousTerrain;
			}
		}
		return null;
	}

	public bool CalculateBuffs(out int strength, out int xp, out bool? jump, out bool? fly, out bool? ignoreDifficultTerrain, out bool? ignoreHazardousTerrain)
	{
		strength = 0;
		xp = 0;
		jump = null;
		fly = null;
		ignoreDifficultTerrain = null;
		ignoreHazardousTerrain = null;
		CAbilityMove cAbilityMove = (CAbilityMove)m_Ability;
		CPlayerActor actor = cAbilityMove.TargetingActor as CPlayerActor;
		if (!cAbilityMove.IsSpeedValidForActiveBonus(actor))
		{
			return false;
		}
		strength = CheckStatIsBasedOnXType();
		xp = m_XP;
		if (cAbilityMove.Jump)
		{
			jump = cAbilityMove.Jump;
		}
		if (cAbilityMove.Fly)
		{
			fly = cAbilityMove.Fly;
		}
		if (cAbilityMove.IgnoreDifficultTerrain)
		{
			ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
		}
		if (cAbilityMove.IgnoreHazardousTerrain)
		{
			ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
		}
		if (strength == 0 && xp == 0 && !jump.HasValue && !fly.HasValue && !ignoreDifficultTerrain.HasValue)
		{
			return ignoreHazardousTerrain.HasValue;
		}
		return true;
	}

	private bool IsValidMoveType(CAbilityMove moveAbility)
	{
		if (m_ActiveBonusData.AttackType != CAbility.EAttackType.Default || !moveAbility.IsDefaultMove)
		{
			return m_ActiveBonusData.AttackType == CAbility.EAttackType.None;
		}
		return true;
	}

	public override void CleanUpBeforeSave()
	{
		base.CleanUpBeforeSave();
		m_Ability.ResetStatBasedOnXAddedStats(m_Ability.StatIsBasedOnXEntries);
		m_Ability.SetStatBasedOnX(m_Ability.TargetingActor, m_Ability.StatIsBasedOnXEntries);
		m_Strength = m_Ability.Strength;
	}
}
