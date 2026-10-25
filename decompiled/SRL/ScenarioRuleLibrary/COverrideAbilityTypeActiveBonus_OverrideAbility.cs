using System.Linq;

namespace ScenarioRuleLibrary;

public class COverrideAbilityTypeActiveBonus_OverrideAbility : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	[StateDump(Skip = true)]
	private int originalStrength;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public COverrideAbilityTypeActiveBonus_OverrideAbility(CActor actor, CAbility ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType type)
		: base(actor, ability, activeBonus)
	{
		m_type = type;
	}

	public override void OnAbilityStarted(CAbility startedAbility)
	{
		if (IsValidAbilityType(startedAbility) && m_ActiveBonus.RequirementsMet(startedAbility.TargetingActor, startedAbility))
		{
			ActivateNonToggleOverrides(startedAbility);
		}
	}

	private void ActivateNonToggleOverrides(CAbility ability)
	{
		if (!m_ActiveBonusData.IsToggleBonus && IsValidAbilityType(ability) && IsValidAttackType(ability) && ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			OnActiveBonusToggled(ability, ActiveBonus, toggledOn: true);
		}
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		base.OnAbilityEnded(endedAbility);
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(endedAbility.TargetingActor) && (IsValidAbilityType(endedAbility) || endedAbility.CurrentOverrides.Any((CAbilityOverride x) => IsValidAbilityType(x.OriginalAbility))) && m_ActiveBonus.SetupTriggerContext(this, endedAbility, m_Ability, m_Actor, m_Actor))
		{
			m_ActiveBonus.RestrictActiveBonus(endedAbility.TargetingActor);
			OnBehaviourTriggered();
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
		string text = ActiveBonus.Ability.ActiveBonusData.GetMiscVar("PropertyToModify").GetString();
		if (!string.IsNullOrEmpty(text))
		{
			currentAbility = CustomAbility.FindAbilityWithPrintedProperty(text, currentAbility);
			if (currentAbility == null)
			{
				return;
			}
		}
		bool flag = ActiveBonus.Ability.ActiveBonusData.ActiveBonusAbilityOverrides.Any((CAbilityOverride x) => x.AbilityType.HasValue && x.AbilityType.Value != currentAbility.AbilityType);
		if ((IsValidAbilityType(currentAbility) && IsValidAttackType(currentAbility) && ValidTargetTypeFilters(currentAbility)) & toggledOn)
		{
			if (flag)
			{
				foreach (CItem item in currentAbility.ActiveSingleTargetItems.Concat(currentAbility.ActiveOverrideItems))
				{
					currentAbility.TargetingActor.Inventory.DeselectItem(item);
				}
			}
			if (m_ActiveBonusData.ActiveBonusAbilityOverrides.Count <= 0)
			{
				return;
			}
			originalStrength = currentAbility.Strength;
			{
				foreach (CAbilityOverride activeBonusAbilityOverride in m_ActiveBonusData.ActiveBonusAbilityOverrides)
				{
					currentAbility.UndoOverride(activeBonusAbilityOverride, perform: false);
					bool flag2 = ActiveBonus.Ability.ActiveBonusData.IsToggleBonus && currentAbility.HasStarted;
					if (flag2)
					{
						ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
					}
					CAbility.EAbilityType abilityType = currentAbility.AbilityType;
					CAbilityOverride cAbilityOverride = currentAbility.OverrideAbilityValues(activeBonusAbilityOverride, flag2);
					if (cAbilityOverride.AbilityType.HasValue && cAbilityOverride.AbilityType != abilityType && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
					{
						if (!(currentAbility is CAbilityAttack))
						{
							currentAbility.Strength = currentAbility.ModifiedStrength();
						}
						cPhaseAction.FinalizeOverrideAbilityType(currentAbility, cAbilityOverride);
					}
				}
				return;
			}
		}
		if (toggledOn || m_ActiveBonusData.ActiveBonusAbilityOverrides.Count <= 0)
		{
			return;
		}
		foreach (CAbilityOverride activeBonusAbilityOverride2 in m_ActiveBonusData.ActiveBonusAbilityOverrides)
		{
			CAbilityOverride cAbilityOverride2 = currentAbility.FindOverride(activeBonusAbilityOverride2);
			bool flag3 = cAbilityOverride2.AbilityType.HasValue && cAbilityOverride2.AbilityType != cAbilityOverride2.OriginalAbility.AbilityType;
			if (flag)
			{
				foreach (CItem item2 in currentAbility.ActiveSingleTargetItems.Concat(currentAbility.ActiveOverrideItems))
				{
					currentAbility.TargetingActor.Inventory.DeselectItem(item2);
				}
			}
			bool flag4 = ActiveBonus.Ability.ActiveBonusData.IsToggleBonus && currentAbility.HasStarted;
			if (flag4)
			{
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			}
			currentAbility.UndoOverride(cAbilityOverride2, flag4);
			if (flag3 && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
			{
				currentAbility.Strength = originalStrength;
				cPhaseAction2.FinalizeOverrideAbilityType(currentAbility);
			}
		}
	}
}
