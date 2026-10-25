using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CBespokeBehaviour
{
	public class BehaviourTriggeredEventArgs : EventArgs
	{
		public int Remaining { get; set; }
	}

	[StateDump(Skip = true)]
	public bool AppliedThisAction;

	protected CActor m_Actor;

	protected CAbility m_Ability;

	protected CActiveBonus m_ActiveBonus;

	private bool m_Listening;

	private List<CActor> m_AdditionalActors;

	protected int m_Strength;

	protected readonly int m_XP;

	protected readonly AbilityData.ActiveBonusData m_ActiveBonusData;

	public virtual CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.None;

	public int Strength => m_Strength;

	public CActiveBonus ActiveBonus => m_ActiveBonus;

	public event EventHandler BehaviourTriggered;

	public virtual void OnBehaviourTriggered()
	{
		BehaviourTriggered?.Invoke(this, new BehaviourTriggeredEventArgs
		{
			Remaining = m_ActiveBonus.Remaining
		});
		m_ActiveBonus.ActiveBonusConsumeElements();
		m_ActiveBonus.PayRequirementCost();
		m_ActiveBonus.ToggledBonus = false;
		m_ActiveBonus.ToggleLocked = false;
		m_ActiveBonus.ToggledElement = null;
	}

	public virtual void OnRestrictionReset()
	{
	}

	public CBespokeBehaviour(CActor actor, CAbility ability, CActiveBonus activeBonus)
	{
		m_Actor = actor;
		m_Ability = ability;
		m_ActiveBonus = activeBonus;
		if (m_Ability.TargetingActor == null)
		{
			m_Ability.TargetingActor = actor;
		}
		m_Strength = ability.Strength;
		m_XP = ability.AbilityXP;
		m_ActiveBonusData = ability.ActiveBonusData;
		m_AdditionalActors = new List<CActor>();
		if (m_Ability.Song != null)
		{
			m_AdditionalActors.AddRange(ScenarioManager.Scenario.PlayerActors.FindAll((CPlayerActor x) => m_ActiveBonusData.Filter.IsValidTarget(x, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible)).ToList());
			m_AdditionalActors.AddRange(ScenarioManager.Scenario.HeroSummons.FindAll((CHeroSummonActor x) => m_ActiveBonusData.Filter.IsValidTarget(x, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible)).ToList());
			m_AdditionalActors.AddRange(ScenarioManager.Scenario.AllMonsters.FindAll((CEnemyActor x) => m_ActiveBonusData.Filter.IsValidTarget(x, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible)).ToList());
		}
		if (m_ActiveBonus.IsAura)
		{
			m_AdditionalActors.AddRange(ScenarioManager.Scenario.PlayerActors.FindAll((CPlayerActor x) => m_ActiveBonusData.AuraFilter.IsValidTarget(x, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible)).ToList());
			m_AdditionalActors.AddRange(ScenarioManager.Scenario.HeroSummons.FindAll((CHeroSummonActor x) => m_ActiveBonusData.AuraFilter.IsValidTarget(x, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible)).ToList());
			m_AdditionalActors.AddRange(ScenarioManager.Scenario.AllMonsters.FindAll((CEnemyActor x) => m_ActiveBonusData.AuraFilter.IsValidTarget(x, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible)).ToList());
		}
		if (ability.ActiveBonusData.StartsRestricted)
		{
			activeBonus.RestrictActiveBonus(actor);
		}
		AddListeners();
	}

	public void AddListeners()
	{
		if (m_Listening)
		{
			return;
		}
		m_Listening = true;
		if (this is CGenericActiveBonus.IListener listener)
		{
			foreach (CActiveBonus.EActiveBonusBehaviourType behaviour in m_ActiveBonusData.Behaviours)
			{
				_ = behaviour;
				CGenericActiveBonus.AddListener(m_ActiveBonusData.Behaviour, listener);
			}
		}
		if (!m_ActiveBonus.IsAura || m_ActiveBonusData.AuraFilter.IsValidTarget(m_Actor, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible))
		{
			CActor actor = m_Actor;
			actor.m_OnAbilityTargetingStartListeners = (CActor.AbilityTargetingStartListener)Delegate.Combine(actor.m_OnAbilityTargetingStartListeners, new CActor.AbilityTargetingStartListener(OnAbilityTargetingStart));
			CActor actor2 = m_Actor;
			actor2.m_OnAbilityPerformStartedListeners = (Action<CAbility>)Delegate.Combine(actor2.m_OnAbilityPerformStartedListeners, new Action<CAbility>(OnAbilityPerformStarted));
			CActor actor3 = m_Actor;
			actor3.m_OnAbilityStartedListeners = (CActor.AbilityStartedListener)Delegate.Combine(actor3.m_OnAbilityStartedListeners, new CActor.AbilityStartedListener(OnAbilityStarted));
			CActor actor4 = m_Actor;
			actor4.m_OnAbilityEndedListeners = (CActor.AbilityEndedListener)Delegate.Combine(actor4.m_OnAbilityEndedListeners, new CActor.AbilityEndedListener(OnAbilityEnded));
			CActor actor5 = m_Actor;
			actor5.m_OnAbilityUndoneListeners = (CActor.AbilityEndedListener)Delegate.Combine(actor5.m_OnAbilityUndoneListeners, new CActor.AbilityEndedListener(OnAbilityUndone));
			CActor actor6 = m_Actor;
			actor6.m_OnAttackStartListeners = (CActor.AttackStartListener)Delegate.Combine(actor6.m_OnAttackStartListeners, new CActor.AttackStartListener(OnAttackStart));
			CActor actor7 = m_Actor;
			actor7.m_OnPreActorIsAttackingListeners = (CActor.PreAttackingListener)Delegate.Combine(actor7.m_OnPreActorIsAttackingListeners, new CActor.PreAttackingListener(OnPreActorIsAttacking));
			CActor actor8 = m_Actor;
			actor8.m_OnAttackingListeners = (CActor.AttackListener)Delegate.Combine(actor8.m_OnAttackingListeners, new CActor.AttackListener(OnAttacking));
			CActor actor9 = m_Actor;
			actor9.m_OnBeingAttackedPreDamageListeners = (CActor.BeingAttackedPreDamageListener)Delegate.Combine(actor9.m_OnBeingAttackedPreDamageListeners, new CActor.BeingAttackedPreDamageListener(OnBeingAttackedPreDamage));
			CActor actor10 = m_Actor;
			actor10.m_OnBeingAttackedPostDamageListeners = (CActor.BeingAttackedPostDamageListener)Delegate.Combine(actor10.m_OnBeingAttackedPostDamageListeners, new CActor.BeingAttackedPostDamageListener(OnBeingAttackedPostDamage));
			CActor actor11 = m_Actor;
			actor11.m_OnBeingAttackedListeners = (CActor.BeingAttackedListener)Delegate.Combine(actor11.m_OnBeingAttackedListeners, new CActor.BeingAttackedListener(OnBeingAttacked));
			CActor actor12 = m_Actor;
			actor12.m_OnAttackSingleTargetListeners = (CActor.AttackSingleTargetStartListener)Delegate.Combine(actor12.m_OnAttackSingleTargetListeners, new CActor.AttackSingleTargetStartListener(OnAttackingSingleTargetStart));
			CActor actor13 = m_Actor;
			actor13.m_OnAttackAbilityFinishedListeners = (CActor.AttackAbilityFinishedListener)Delegate.Combine(actor13.m_OnAttackAbilityFinishedListeners, new CActor.AttackAbilityFinishedListener(OnAttackAbilityFinished));
			CActor actor14 = m_Actor;
			actor14.m_OnAttackFinishedListeners = (CActor.AttackFinishedListener)Delegate.Combine(actor14.m_OnAttackFinishedListeners, new CActor.AttackFinishedListener(OnAttackFinished));
			CActor actor15 = m_Actor;
			actor15.m_OnHealApplyToActorListeners = (CActor.HealApplyToActorListener)Delegate.Combine(actor15.m_OnHealApplyToActorListeners, new CActor.HealApplyToActorListener(OnHealApplyToActor));
			CActor actor16 = m_Actor;
			actor16.m_OnHealApplyToActionListeners = (CActor.HealApplyToActionListener)Delegate.Combine(actor16.m_OnHealApplyToActionListeners, new CActor.HealApplyToActionListener(OnHealApplyToAction));
			CActor actor17 = m_Actor;
			actor17.m_OnBeingHealedListeners = (CActor.BeingHealedListener)Delegate.Combine(actor17.m_OnBeingHealedListeners, new CActor.BeingHealedListener(OnBeingHealed));
			CActor actor18 = m_Actor;
			actor18.m_OnAfterBeingHealedListeners = (CActor.AfterBeingHealedListener)Delegate.Combine(actor18.m_OnAfterBeingHealedListeners, new CActor.AfterBeingHealedListener(OnAfterBeingHealed));
			CActor actor19 = m_Actor;
			actor19.m_OnActorImmuneToConditionListeners = (CActor.OnActorImmuneToConditionListener)Delegate.Combine(actor19.m_OnActorImmuneToConditionListeners, new CActor.OnActorImmuneToConditionListener(OnActorImmuneToCondition));
			CActor actor20 = m_Actor;
			actor20.m_OnConditionApplyToActorListeners = (CActor.ConditionApplyToActorListener)Delegate.Combine(actor20.m_OnConditionApplyToActorListeners, new CActor.ConditionApplyToActorListener(OnConditionApplyToActor));
			CActor actor21 = m_Actor;
			actor21.m_OnPositiveConditionEndedOnActorListeners = (CActor.PositiveConditionEndedOnActorListener)Delegate.Combine(actor21.m_OnPositiveConditionEndedOnActorListeners, new CActor.PositiveConditionEndedOnActorListener(OnPositiveConditionEndedOnActor));
			CActor actor22 = m_Actor;
			actor22.m_OnNegativeConditionEndedOnActorListeners = (CActor.NegativeConditionEndedOnActorListener)Delegate.Combine(actor22.m_OnNegativeConditionEndedOnActorListeners, new CActor.NegativeConditionEndedOnActorListener(OnNegativeConditionEndedOnActor));
			CActor actor23 = m_Actor;
			actor23.m_OnMoveStartListeners = (CActor.MoveStartListener)Delegate.Combine(actor23.m_OnMoveStartListeners, new CActor.MoveStartListener(OnMoveStart));
			CActor actor24 = m_Actor;
			actor24.m_OnMovingListeners = (CActor.MoveListener)Delegate.Combine(actor24.m_OnMovingListeners, new CActor.MoveListener(OnMoving));
			CActor actor25 = m_Actor;
			actor25.m_OnMovedListeners = (CActor.MovedListener)Delegate.Combine(actor25.m_OnMovedListeners, new CActor.MovedListener(OnMoved));
			CActor actor26 = m_Actor;
			actor26.m_OnCarriedListeners = (CActor.CarriedListener)Delegate.Combine(actor26.m_OnCarriedListeners, new CActor.CarriedListener(OnCarried));
			CActor actor27 = m_Actor;
			actor27.m_OnLootListeners = (CActor.LootListener)Delegate.Combine(actor27.m_OnLootListeners, new CActor.LootListener(OnLoot));
			CActor actor28 = m_Actor;
			actor28.m_OnRetaliateListeners = (CActor.RetaliateListener)Delegate.Combine(actor28.m_OnRetaliateListeners, new CActor.RetaliateListener(OnRetaliate));
			CActor actor29 = m_Actor;
			actor29.m_OnDamagedListeners = (CActor.DamagedListener)Delegate.Combine(actor29.m_OnDamagedListeners, new CActor.DamagedListener(OnDamaged));
			CActor actor30 = m_Actor;
			actor30.m_OnTakenDamageListeners = (CActor.TakeDamageListener)Delegate.Combine(actor30.m_OnTakenDamageListeners, new CActor.TakeDamageListener(OnTakenDamage));
			CActor actor31 = m_Actor;
			actor31.m_OnKillListeners = (CActor.KillListener)Delegate.Combine(actor31.m_OnKillListeners, new CActor.KillListener(OnKill));
			CActor actor32 = m_Actor;
			actor32.m_OnDeathListeners = (CActor.DeathListener)Delegate.Combine(actor32.m_OnDeathListeners, new CActor.DeathListener(OnDeath));
			CActor actor33 = m_Actor;
			actor33.m_AfterDeathListeners = (CActor.DeathListener)Delegate.Combine(actor33.m_AfterDeathListeners, new CActor.DeathListener(AfterDeath));
			CActor actor34 = m_Actor;
			actor34.m_OnDrawModifierListeners = (CActor.DrawModifierListener)Delegate.Combine(actor34.m_OnDrawModifierListeners, new CActor.DrawModifierListener(OnDrawModifier));
			CActor actor35 = m_Actor;
			actor35.m_OnPreventDamageListeners = (CActor.PreventDamageListener)Delegate.Combine(actor35.m_OnPreventDamageListeners, new CActor.PreventDamageListener(OnPreventDamageTriggered));
			CActor actor36 = m_Actor;
			actor36.m_OnCreatedListeners = (CActor.CreatedListener)Delegate.Combine(actor36.m_OnCreatedListeners, new CActor.CreatedListener(OnCreated));
			CActor actor37 = m_Actor;
			actor37.m_OnAbilityOverrideAppliedListeners = (CActor.AbilityOverrideApplied)Delegate.Combine(actor37.m_OnAbilityOverrideAppliedListeners, new CActor.AbilityOverrideApplied(OnAbilityOverrideApplied));
			CActor actor38 = m_Actor;
			actor38.m_OnPropCreatedListeners = (CActor.PropCreated)Delegate.Combine(actor38.m_OnPropCreatedListeners, new CActor.PropCreated(OnPropCreated));
			CActor actor39 = m_Actor;
			actor39.m_OnPropDestroyedListeners = (CActor.PropDestroyed)Delegate.Combine(actor39.m_OnPropDestroyedListeners, new CActor.PropDestroyed(OnPropDestroyed));
		}
		if (m_AdditionalActors == null)
		{
			return;
		}
		foreach (CActor additionalActor in m_AdditionalActors)
		{
			additionalActor.m_OnAbilityTargetingStartListeners = (CActor.AbilityTargetingStartListener)Delegate.Combine(additionalActor.m_OnAbilityTargetingStartListeners, new CActor.AbilityTargetingStartListener(OnAbilityTargetingStart));
			additionalActor.m_OnAbilityPerformStartedListeners = (Action<CAbility>)Delegate.Combine(additionalActor.m_OnAbilityPerformStartedListeners, new Action<CAbility>(OnAbilityPerformStarted));
			additionalActor.m_OnAbilityStartedListeners = (CActor.AbilityStartedListener)Delegate.Combine(additionalActor.m_OnAbilityStartedListeners, new CActor.AbilityStartedListener(OnAbilityStarted));
			additionalActor.m_OnAbilityEndedListeners = (CActor.AbilityEndedListener)Delegate.Combine(additionalActor.m_OnAbilityEndedListeners, new CActor.AbilityEndedListener(OnAbilityEnded));
			additionalActor.m_OnAbilityUndoneListeners = (CActor.AbilityEndedListener)Delegate.Combine(additionalActor.m_OnAbilityUndoneListeners, new CActor.AbilityEndedListener(OnAbilityUndone));
			additionalActor.m_OnAttackStartListeners = (CActor.AttackStartListener)Delegate.Combine(additionalActor.m_OnAttackStartListeners, new CActor.AttackStartListener(OnAttackStart));
			additionalActor.m_OnPreActorIsAttackingListeners = (CActor.PreAttackingListener)Delegate.Combine(additionalActor.m_OnPreActorIsAttackingListeners, new CActor.PreAttackingListener(OnPreActorIsAttacking));
			additionalActor.m_OnAttackingListeners = (CActor.AttackListener)Delegate.Combine(additionalActor.m_OnAttackingListeners, new CActor.AttackListener(OnAttacking));
			additionalActor.m_OnBeingAttackedPreDamageListeners = (CActor.BeingAttackedPreDamageListener)Delegate.Combine(additionalActor.m_OnBeingAttackedPreDamageListeners, new CActor.BeingAttackedPreDamageListener(OnBeingAttackedPreDamage));
			additionalActor.m_OnBeingAttackedPostDamageListeners = (CActor.BeingAttackedPostDamageListener)Delegate.Combine(additionalActor.m_OnBeingAttackedPostDamageListeners, new CActor.BeingAttackedPostDamageListener(OnBeingAttackedPostDamage));
			additionalActor.m_OnBeingAttackedListeners = (CActor.BeingAttackedListener)Delegate.Combine(additionalActor.m_OnBeingAttackedListeners, new CActor.BeingAttackedListener(OnBeingAttacked));
			additionalActor.m_OnAttackSingleTargetListeners = (CActor.AttackSingleTargetStartListener)Delegate.Combine(additionalActor.m_OnAttackSingleTargetListeners, new CActor.AttackSingleTargetStartListener(OnAttackingSingleTargetStart));
			additionalActor.m_OnAttackAbilityFinishedListeners = (CActor.AttackAbilityFinishedListener)Delegate.Combine(additionalActor.m_OnAttackAbilityFinishedListeners, new CActor.AttackAbilityFinishedListener(OnAttackAbilityFinished));
			additionalActor.m_OnAttackFinishedListeners = (CActor.AttackFinishedListener)Delegate.Combine(additionalActor.m_OnAttackFinishedListeners, new CActor.AttackFinishedListener(OnAttackFinished));
			additionalActor.m_OnHealApplyToActorListeners = (CActor.HealApplyToActorListener)Delegate.Combine(additionalActor.m_OnHealApplyToActorListeners, new CActor.HealApplyToActorListener(OnHealApplyToActor));
			additionalActor.m_OnHealApplyToActionListeners = (CActor.HealApplyToActionListener)Delegate.Combine(additionalActor.m_OnHealApplyToActionListeners, new CActor.HealApplyToActionListener(OnHealApplyToAction));
			additionalActor.m_OnBeingHealedListeners = (CActor.BeingHealedListener)Delegate.Combine(additionalActor.m_OnBeingHealedListeners, new CActor.BeingHealedListener(OnBeingHealed));
			additionalActor.m_OnAfterBeingHealedListeners = (CActor.AfterBeingHealedListener)Delegate.Combine(additionalActor.m_OnAfterBeingHealedListeners, new CActor.AfterBeingHealedListener(OnAfterBeingHealed));
			additionalActor.m_OnActorImmuneToConditionListeners = (CActor.OnActorImmuneToConditionListener)Delegate.Combine(additionalActor.m_OnActorImmuneToConditionListeners, new CActor.OnActorImmuneToConditionListener(OnActorImmuneToCondition));
			additionalActor.m_OnConditionApplyToActorListeners = (CActor.ConditionApplyToActorListener)Delegate.Combine(additionalActor.m_OnConditionApplyToActorListeners, new CActor.ConditionApplyToActorListener(OnConditionApplyToActor));
			additionalActor.m_OnPositiveConditionEndedOnActorListeners = (CActor.PositiveConditionEndedOnActorListener)Delegate.Combine(additionalActor.m_OnPositiveConditionEndedOnActorListeners, new CActor.PositiveConditionEndedOnActorListener(OnPositiveConditionEndedOnActor));
			additionalActor.m_OnNegativeConditionEndedOnActorListeners = (CActor.NegativeConditionEndedOnActorListener)Delegate.Combine(additionalActor.m_OnNegativeConditionEndedOnActorListeners, new CActor.NegativeConditionEndedOnActorListener(OnNegativeConditionEndedOnActor));
			additionalActor.m_OnMoveStartListeners = (CActor.MoveStartListener)Delegate.Combine(additionalActor.m_OnMoveStartListeners, new CActor.MoveStartListener(OnMoveStart));
			additionalActor.m_OnMovingListeners = (CActor.MoveListener)Delegate.Combine(additionalActor.m_OnMovingListeners, new CActor.MoveListener(OnMoving));
			additionalActor.m_OnMovedListeners = (CActor.MovedListener)Delegate.Combine(additionalActor.m_OnMovedListeners, new CActor.MovedListener(OnMoved));
			additionalActor.m_OnCarriedListeners = (CActor.CarriedListener)Delegate.Combine(additionalActor.m_OnCarriedListeners, new CActor.CarriedListener(OnCarried));
			additionalActor.m_OnLootListeners = (CActor.LootListener)Delegate.Combine(additionalActor.m_OnLootListeners, new CActor.LootListener(OnLoot));
			additionalActor.m_OnRetaliateListeners = (CActor.RetaliateListener)Delegate.Combine(additionalActor.m_OnRetaliateListeners, new CActor.RetaliateListener(OnRetaliate));
			additionalActor.m_OnDamagedListeners = (CActor.DamagedListener)Delegate.Combine(additionalActor.m_OnDamagedListeners, new CActor.DamagedListener(OnDamaged));
			additionalActor.m_OnTakenDamageListeners = (CActor.TakeDamageListener)Delegate.Combine(additionalActor.m_OnTakenDamageListeners, new CActor.TakeDamageListener(OnTakenDamage));
			additionalActor.m_OnKillListeners = (CActor.KillListener)Delegate.Combine(additionalActor.m_OnKillListeners, new CActor.KillListener(OnKill));
			additionalActor.m_OnDeathListeners = (CActor.DeathListener)Delegate.Combine(additionalActor.m_OnDeathListeners, new CActor.DeathListener(OnDeath));
			additionalActor.m_AfterDeathListeners = (CActor.DeathListener)Delegate.Combine(additionalActor.m_AfterDeathListeners, new CActor.DeathListener(AfterDeath));
			additionalActor.m_OnDrawModifierListeners = (CActor.DrawModifierListener)Delegate.Combine(additionalActor.m_OnDrawModifierListeners, new CActor.DrawModifierListener(OnDrawModifier));
			additionalActor.m_OnPreventDamageListeners = (CActor.PreventDamageListener)Delegate.Combine(additionalActor.m_OnPreventDamageListeners, new CActor.PreventDamageListener(OnPreventDamageTriggered));
			additionalActor.m_OnCreatedListeners = (CActor.CreatedListener)Delegate.Combine(additionalActor.m_OnCreatedListeners, new CActor.CreatedListener(OnCreated));
			additionalActor.m_OnAbilityOverrideAppliedListeners = (CActor.AbilityOverrideApplied)Delegate.Combine(additionalActor.m_OnAbilityOverrideAppliedListeners, new CActor.AbilityOverrideApplied(OnAbilityOverrideApplied));
			additionalActor.m_OnPropCreatedListeners = (CActor.PropCreated)Delegate.Combine(additionalActor.m_OnPropCreatedListeners, new CActor.PropCreated(OnPropCreated));
			additionalActor.m_OnPropDestroyedListeners = (CActor.PropDestroyed)Delegate.Combine(additionalActor.m_OnPropDestroyedListeners, new CActor.PropDestroyed(OnPropDestroyed));
		}
	}

	public void RemoveListeners()
	{
		if (!m_Listening)
		{
			return;
		}
		m_Listening = false;
		if (this is CGenericActiveBonus.IListener listener)
		{
			foreach (CActiveBonus.EActiveBonusBehaviourType behaviour in m_ActiveBonusData.Behaviours)
			{
				CGenericActiveBonus.DelListener(behaviour, listener);
			}
		}
		CActor actor = m_Actor;
		actor.m_OnAbilityTargetingStartListeners = (CActor.AbilityTargetingStartListener)Delegate.Remove(actor.m_OnAbilityTargetingStartListeners, new CActor.AbilityTargetingStartListener(OnAbilityTargetingStart));
		CActor actor2 = m_Actor;
		actor2.m_OnAbilityPerformStartedListeners = (Action<CAbility>)Delegate.Remove(actor2.m_OnAbilityPerformStartedListeners, new Action<CAbility>(OnAbilityPerformStarted));
		CActor actor3 = m_Actor;
		actor3.m_OnAbilityStartedListeners = (CActor.AbilityStartedListener)Delegate.Remove(actor3.m_OnAbilityStartedListeners, new CActor.AbilityStartedListener(OnAbilityStarted));
		CActor actor4 = m_Actor;
		actor4.m_OnAbilityEndedListeners = (CActor.AbilityEndedListener)Delegate.Remove(actor4.m_OnAbilityEndedListeners, new CActor.AbilityEndedListener(OnAbilityEnded));
		CActor actor5 = m_Actor;
		actor5.m_OnAbilityUndoneListeners = (CActor.AbilityEndedListener)Delegate.Remove(actor5.m_OnAbilityUndoneListeners, new CActor.AbilityEndedListener(OnAbilityUndone));
		CActor actor6 = m_Actor;
		actor6.m_OnAttackStartListeners = (CActor.AttackStartListener)Delegate.Remove(actor6.m_OnAttackStartListeners, new CActor.AttackStartListener(OnAttackStart));
		CActor actor7 = m_Actor;
		actor7.m_OnPreActorIsAttackingListeners = (CActor.PreAttackingListener)Delegate.Remove(actor7.m_OnPreActorIsAttackingListeners, new CActor.PreAttackingListener(OnPreActorIsAttacking));
		CActor actor8 = m_Actor;
		actor8.m_OnAttackingListeners = (CActor.AttackListener)Delegate.Remove(actor8.m_OnAttackingListeners, new CActor.AttackListener(OnAttacking));
		CActor actor9 = m_Actor;
		actor9.m_OnBeingAttackedPreDamageListeners = (CActor.BeingAttackedPreDamageListener)Delegate.Remove(actor9.m_OnBeingAttackedPreDamageListeners, new CActor.BeingAttackedPreDamageListener(OnBeingAttackedPreDamage));
		CActor actor10 = m_Actor;
		actor10.m_OnBeingAttackedPostDamageListeners = (CActor.BeingAttackedPostDamageListener)Delegate.Remove(actor10.m_OnBeingAttackedPostDamageListeners, new CActor.BeingAttackedPostDamageListener(OnBeingAttackedPostDamage));
		CActor actor11 = m_Actor;
		actor11.m_OnBeingAttackedListeners = (CActor.BeingAttackedListener)Delegate.Remove(actor11.m_OnBeingAttackedListeners, new CActor.BeingAttackedListener(OnBeingAttacked));
		CActor actor12 = m_Actor;
		actor12.m_OnAttackSingleTargetListeners = (CActor.AttackSingleTargetStartListener)Delegate.Remove(actor12.m_OnAttackSingleTargetListeners, new CActor.AttackSingleTargetStartListener(OnAttackingSingleTargetStart));
		CActor actor13 = m_Actor;
		actor13.m_OnAttackAbilityFinishedListeners = (CActor.AttackAbilityFinishedListener)Delegate.Remove(actor13.m_OnAttackAbilityFinishedListeners, new CActor.AttackAbilityFinishedListener(OnAttackAbilityFinished));
		CActor actor14 = m_Actor;
		actor14.m_OnAttackFinishedListeners = (CActor.AttackFinishedListener)Delegate.Remove(actor14.m_OnAttackFinishedListeners, new CActor.AttackFinishedListener(OnAttackFinished));
		CActor actor15 = m_Actor;
		actor15.m_OnHealApplyToActorListeners = (CActor.HealApplyToActorListener)Delegate.Remove(actor15.m_OnHealApplyToActorListeners, new CActor.HealApplyToActorListener(OnHealApplyToActor));
		CActor actor16 = m_Actor;
		actor16.m_OnHealApplyToActionListeners = (CActor.HealApplyToActionListener)Delegate.Remove(actor16.m_OnHealApplyToActionListeners, new CActor.HealApplyToActionListener(OnHealApplyToAction));
		CActor actor17 = m_Actor;
		actor17.m_OnBeingHealedListeners = (CActor.BeingHealedListener)Delegate.Remove(actor17.m_OnBeingHealedListeners, new CActor.BeingHealedListener(OnBeingHealed));
		CActor actor18 = m_Actor;
		actor18.m_OnAfterBeingHealedListeners = (CActor.AfterBeingHealedListener)Delegate.Remove(actor18.m_OnAfterBeingHealedListeners, new CActor.AfterBeingHealedListener(OnAfterBeingHealed));
		CActor actor19 = m_Actor;
		actor19.m_OnActorImmuneToConditionListeners = (CActor.OnActorImmuneToConditionListener)Delegate.Remove(actor19.m_OnActorImmuneToConditionListeners, new CActor.OnActorImmuneToConditionListener(OnActorImmuneToCondition));
		CActor actor20 = m_Actor;
		actor20.m_OnConditionApplyToActorListeners = (CActor.ConditionApplyToActorListener)Delegate.Remove(actor20.m_OnConditionApplyToActorListeners, new CActor.ConditionApplyToActorListener(OnConditionApplyToActor));
		CActor actor21 = m_Actor;
		actor21.m_OnPositiveConditionEndedOnActorListeners = (CActor.PositiveConditionEndedOnActorListener)Delegate.Remove(actor21.m_OnPositiveConditionEndedOnActorListeners, new CActor.PositiveConditionEndedOnActorListener(OnPositiveConditionEndedOnActor));
		CActor actor22 = m_Actor;
		actor22.m_OnNegativeConditionEndedOnActorListeners = (CActor.NegativeConditionEndedOnActorListener)Delegate.Remove(actor22.m_OnNegativeConditionEndedOnActorListeners, new CActor.NegativeConditionEndedOnActorListener(OnNegativeConditionEndedOnActor));
		CActor actor23 = m_Actor;
		actor23.m_OnMoveStartListeners = (CActor.MoveStartListener)Delegate.Remove(actor23.m_OnMoveStartListeners, new CActor.MoveStartListener(OnMoveStart));
		CActor actor24 = m_Actor;
		actor24.m_OnMovingListeners = (CActor.MoveListener)Delegate.Remove(actor24.m_OnMovingListeners, new CActor.MoveListener(OnMoving));
		CActor actor25 = m_Actor;
		actor25.m_OnMovedListeners = (CActor.MovedListener)Delegate.Remove(actor25.m_OnMovedListeners, new CActor.MovedListener(OnMoved));
		CActor actor26 = m_Actor;
		actor26.m_OnCarriedListeners = (CActor.CarriedListener)Delegate.Remove(actor26.m_OnCarriedListeners, new CActor.CarriedListener(OnCarried));
		CActor actor27 = m_Actor;
		actor27.m_OnLootListeners = (CActor.LootListener)Delegate.Remove(actor27.m_OnLootListeners, new CActor.LootListener(OnLoot));
		CActor actor28 = m_Actor;
		actor28.m_OnRetaliateListeners = (CActor.RetaliateListener)Delegate.Remove(actor28.m_OnRetaliateListeners, new CActor.RetaliateListener(OnRetaliate));
		CActor actor29 = m_Actor;
		actor29.m_OnDamagedListeners = (CActor.DamagedListener)Delegate.Remove(actor29.m_OnDamagedListeners, new CActor.DamagedListener(OnDamaged));
		CActor actor30 = m_Actor;
		actor30.m_OnTakenDamageListeners = (CActor.TakeDamageListener)Delegate.Remove(actor30.m_OnTakenDamageListeners, new CActor.TakeDamageListener(OnTakenDamage));
		CActor actor31 = m_Actor;
		actor31.m_OnKillListeners = (CActor.KillListener)Delegate.Remove(actor31.m_OnKillListeners, new CActor.KillListener(OnKill));
		CActor actor32 = m_Actor;
		actor32.m_OnDeathListeners = (CActor.DeathListener)Delegate.Remove(actor32.m_OnDeathListeners, new CActor.DeathListener(OnDeath));
		CActor actor33 = m_Actor;
		actor33.m_AfterDeathListeners = (CActor.DeathListener)Delegate.Remove(actor33.m_AfterDeathListeners, new CActor.DeathListener(AfterDeath));
		CActor actor34 = m_Actor;
		actor34.m_OnDrawModifierListeners = (CActor.DrawModifierListener)Delegate.Remove(actor34.m_OnDrawModifierListeners, new CActor.DrawModifierListener(OnDrawModifier));
		CActor actor35 = m_Actor;
		actor35.m_OnPreventDamageListeners = (CActor.PreventDamageListener)Delegate.Remove(actor35.m_OnPreventDamageListeners, new CActor.PreventDamageListener(OnPreventDamageTriggered));
		CActor actor36 = m_Actor;
		actor36.m_OnCreatedListeners = (CActor.CreatedListener)Delegate.Remove(actor36.m_OnCreatedListeners, new CActor.CreatedListener(OnCreated));
		CActor actor37 = m_Actor;
		actor37.m_OnAbilityOverrideAppliedListeners = (CActor.AbilityOverrideApplied)Delegate.Remove(actor37.m_OnAbilityOverrideAppliedListeners, new CActor.AbilityOverrideApplied(OnAbilityOverrideApplied));
		CActor actor38 = m_Actor;
		actor38.m_OnPropCreatedListeners = (CActor.PropCreated)Delegate.Remove(actor38.m_OnPropCreatedListeners, new CActor.PropCreated(OnPropCreated));
		CActor actor39 = m_Actor;
		actor39.m_OnPropDestroyedListeners = (CActor.PropDestroyed)Delegate.Remove(actor39.m_OnPropDestroyedListeners, new CActor.PropDestroyed(OnPropDestroyed));
		if (m_AdditionalActors == null)
		{
			return;
		}
		foreach (CActor additionalActor in m_AdditionalActors)
		{
			additionalActor.m_OnAbilityTargetingStartListeners = (CActor.AbilityTargetingStartListener)Delegate.Remove(additionalActor.m_OnAbilityTargetingStartListeners, new CActor.AbilityTargetingStartListener(OnAbilityTargetingStart));
			additionalActor.m_OnAbilityPerformStartedListeners = (Action<CAbility>)Delegate.Remove(additionalActor.m_OnAbilityPerformStartedListeners, new Action<CAbility>(OnAbilityPerformStarted));
			additionalActor.m_OnAbilityStartedListeners = (CActor.AbilityStartedListener)Delegate.Remove(additionalActor.m_OnAbilityStartedListeners, new CActor.AbilityStartedListener(OnAbilityStarted));
			additionalActor.m_OnAbilityEndedListeners = (CActor.AbilityEndedListener)Delegate.Remove(additionalActor.m_OnAbilityEndedListeners, new CActor.AbilityEndedListener(OnAbilityEnded));
			additionalActor.m_OnAbilityUndoneListeners = (CActor.AbilityEndedListener)Delegate.Remove(additionalActor.m_OnAbilityUndoneListeners, new CActor.AbilityEndedListener(OnAbilityUndone));
			additionalActor.m_OnAttackStartListeners = (CActor.AttackStartListener)Delegate.Remove(additionalActor.m_OnAttackStartListeners, new CActor.AttackStartListener(OnAttackStart));
			additionalActor.m_OnPreActorIsAttackingListeners = (CActor.PreAttackingListener)Delegate.Remove(additionalActor.m_OnPreActorIsAttackingListeners, new CActor.PreAttackingListener(OnPreActorIsAttacking));
			additionalActor.m_OnAttackingListeners = (CActor.AttackListener)Delegate.Remove(additionalActor.m_OnAttackingListeners, new CActor.AttackListener(OnAttacking));
			additionalActor.m_OnBeingAttackedPreDamageListeners = (CActor.BeingAttackedPreDamageListener)Delegate.Remove(additionalActor.m_OnBeingAttackedPreDamageListeners, new CActor.BeingAttackedPreDamageListener(OnBeingAttackedPreDamage));
			additionalActor.m_OnBeingAttackedPostDamageListeners = (CActor.BeingAttackedPostDamageListener)Delegate.Remove(additionalActor.m_OnBeingAttackedPostDamageListeners, new CActor.BeingAttackedPostDamageListener(OnBeingAttackedPostDamage));
			additionalActor.m_OnBeingAttackedListeners = (CActor.BeingAttackedListener)Delegate.Remove(additionalActor.m_OnBeingAttackedListeners, new CActor.BeingAttackedListener(OnBeingAttacked));
			additionalActor.m_OnAttackSingleTargetListeners = (CActor.AttackSingleTargetStartListener)Delegate.Remove(additionalActor.m_OnAttackSingleTargetListeners, new CActor.AttackSingleTargetStartListener(OnAttackingSingleTargetStart));
			additionalActor.m_OnAttackAbilityFinishedListeners = (CActor.AttackAbilityFinishedListener)Delegate.Remove(additionalActor.m_OnAttackAbilityFinishedListeners, new CActor.AttackAbilityFinishedListener(OnAttackAbilityFinished));
			additionalActor.m_OnAttackFinishedListeners = (CActor.AttackFinishedListener)Delegate.Remove(additionalActor.m_OnAttackFinishedListeners, new CActor.AttackFinishedListener(OnAttackFinished));
			additionalActor.m_OnHealApplyToActorListeners = (CActor.HealApplyToActorListener)Delegate.Remove(additionalActor.m_OnHealApplyToActorListeners, new CActor.HealApplyToActorListener(OnHealApplyToActor));
			additionalActor.m_OnHealApplyToActionListeners = (CActor.HealApplyToActionListener)Delegate.Remove(additionalActor.m_OnHealApplyToActionListeners, new CActor.HealApplyToActionListener(OnHealApplyToAction));
			additionalActor.m_OnBeingHealedListeners = (CActor.BeingHealedListener)Delegate.Remove(additionalActor.m_OnBeingHealedListeners, new CActor.BeingHealedListener(OnBeingHealed));
			additionalActor.m_OnAfterBeingHealedListeners = (CActor.AfterBeingHealedListener)Delegate.Remove(additionalActor.m_OnAfterBeingHealedListeners, new CActor.AfterBeingHealedListener(OnAfterBeingHealed));
			additionalActor.m_OnActorImmuneToConditionListeners = (CActor.OnActorImmuneToConditionListener)Delegate.Remove(additionalActor.m_OnActorImmuneToConditionListeners, new CActor.OnActorImmuneToConditionListener(OnActorImmuneToCondition));
			additionalActor.m_OnConditionApplyToActorListeners = (CActor.ConditionApplyToActorListener)Delegate.Remove(additionalActor.m_OnConditionApplyToActorListeners, new CActor.ConditionApplyToActorListener(OnConditionApplyToActor));
			additionalActor.m_OnPositiveConditionEndedOnActorListeners = (CActor.PositiveConditionEndedOnActorListener)Delegate.Remove(additionalActor.m_OnPositiveConditionEndedOnActorListeners, new CActor.PositiveConditionEndedOnActorListener(OnPositiveConditionEndedOnActor));
			additionalActor.m_OnNegativeConditionEndedOnActorListeners = (CActor.NegativeConditionEndedOnActorListener)Delegate.Remove(additionalActor.m_OnNegativeConditionEndedOnActorListeners, new CActor.NegativeConditionEndedOnActorListener(OnNegativeConditionEndedOnActor));
			additionalActor.m_OnMoveStartListeners = (CActor.MoveStartListener)Delegate.Remove(additionalActor.m_OnMoveStartListeners, new CActor.MoveStartListener(OnMoveStart));
			additionalActor.m_OnMovingListeners = (CActor.MoveListener)Delegate.Remove(additionalActor.m_OnMovingListeners, new CActor.MoveListener(OnMoving));
			additionalActor.m_OnMovedListeners = (CActor.MovedListener)Delegate.Remove(additionalActor.m_OnMovedListeners, new CActor.MovedListener(OnMoved));
			additionalActor.m_OnCarriedListeners = (CActor.CarriedListener)Delegate.Remove(additionalActor.m_OnCarriedListeners, new CActor.CarriedListener(OnCarried));
			additionalActor.m_OnLootListeners = (CActor.LootListener)Delegate.Remove(additionalActor.m_OnLootListeners, new CActor.LootListener(OnLoot));
			additionalActor.m_OnRetaliateListeners = (CActor.RetaliateListener)Delegate.Remove(additionalActor.m_OnRetaliateListeners, new CActor.RetaliateListener(OnRetaliate));
			additionalActor.m_OnDamagedListeners = (CActor.DamagedListener)Delegate.Remove(additionalActor.m_OnDamagedListeners, new CActor.DamagedListener(OnDamaged));
			additionalActor.m_OnTakenDamageListeners = (CActor.TakeDamageListener)Delegate.Remove(additionalActor.m_OnTakenDamageListeners, new CActor.TakeDamageListener(OnTakenDamage));
			additionalActor.m_OnKillListeners = (CActor.KillListener)Delegate.Remove(additionalActor.m_OnKillListeners, new CActor.KillListener(OnKill));
			additionalActor.m_OnDeathListeners = (CActor.DeathListener)Delegate.Remove(additionalActor.m_OnDeathListeners, new CActor.DeathListener(OnDeath));
			additionalActor.m_AfterDeathListeners = (CActor.DeathListener)Delegate.Remove(additionalActor.m_AfterDeathListeners, new CActor.DeathListener(AfterDeath));
			additionalActor.m_OnDrawModifierListeners = (CActor.DrawModifierListener)Delegate.Remove(additionalActor.m_OnDrawModifierListeners, new CActor.DrawModifierListener(OnDrawModifier));
			additionalActor.m_OnPreventDamageListeners = (CActor.PreventDamageListener)Delegate.Remove(additionalActor.m_OnPreventDamageListeners, new CActor.PreventDamageListener(OnPreventDamageTriggered));
			additionalActor.m_OnCreatedListeners = (CActor.CreatedListener)Delegate.Remove(additionalActor.m_OnCreatedListeners, new CActor.CreatedListener(OnCreated));
			additionalActor.m_OnAbilityOverrideAppliedListeners = (CActor.AbilityOverrideApplied)Delegate.Remove(additionalActor.m_OnAbilityOverrideAppliedListeners, new CActor.AbilityOverrideApplied(OnAbilityOverrideApplied));
			additionalActor.m_OnPropCreatedListeners = (CActor.PropCreated)Delegate.Remove(additionalActor.m_OnPropCreatedListeners, new CActor.PropCreated(OnPropCreated));
			additionalActor.m_OnPropDestroyedListeners = (CActor.PropDestroyed)Delegate.Remove(additionalActor.m_OnPropDestroyedListeners, new CActor.PropDestroyed(OnPropDestroyed));
		}
		m_AdditionalActors.Clear();
	}

	public void RefreshListeners(CActor newActor, bool originalTargetType = false)
	{
		if (m_AdditionalActors.Contains(newActor))
		{
			return;
		}
		CAbilityFilterContainer cAbilityFilterContainer = (m_ActiveBonus.IsSong ? m_ActiveBonusData.Filter : m_ActiveBonusData.AuraFilter);
		CActor actor = m_Actor;
		bool? canTargetInvisible = m_Ability.MiscAbilityData?.CanTargetInvisible;
		if (!cAbilityFilterContainer.IsValidTarget(newActor, actor, isTargetedAbility: false, originalTargetType, canTargetInvisible))
		{
			return;
		}
		if (!m_Listening && this is CGenericActiveBonus.IListener listener)
		{
			foreach (CActiveBonus.EActiveBonusBehaviourType behaviour in m_ActiveBonusData.Behaviours)
			{
				CGenericActiveBonus.AddListener(behaviour, listener);
			}
		}
		m_Listening = true;
		m_AdditionalActors.Add(newActor);
		m_AdditionalActors.Sort(GameState.CompareActorsByGUID);
		newActor.m_OnAbilityTargetingStartListeners = (CActor.AbilityTargetingStartListener)Delegate.Combine(newActor.m_OnAbilityTargetingStartListeners, new CActor.AbilityTargetingStartListener(OnAbilityTargetingStart));
		newActor.m_OnAbilityPerformStartedListeners = (Action<CAbility>)Delegate.Combine(newActor.m_OnAbilityPerformStartedListeners, new Action<CAbility>(OnAbilityPerformStarted));
		newActor.m_OnAbilityStartedListeners = (CActor.AbilityStartedListener)Delegate.Combine(newActor.m_OnAbilityStartedListeners, new CActor.AbilityStartedListener(OnAbilityStarted));
		newActor.m_OnAbilityEndedListeners = (CActor.AbilityEndedListener)Delegate.Combine(newActor.m_OnAbilityEndedListeners, new CActor.AbilityEndedListener(OnAbilityEnded));
		newActor.m_OnAbilityUndoneListeners = (CActor.AbilityEndedListener)Delegate.Combine(newActor.m_OnAbilityUndoneListeners, new CActor.AbilityEndedListener(OnAbilityUndone));
		newActor.m_OnAttackStartListeners = (CActor.AttackStartListener)Delegate.Combine(newActor.m_OnAttackStartListeners, new CActor.AttackStartListener(OnAttackStart));
		newActor.m_OnPreActorIsAttackingListeners = (CActor.PreAttackingListener)Delegate.Combine(newActor.m_OnPreActorIsAttackingListeners, new CActor.PreAttackingListener(OnPreActorIsAttacking));
		newActor.m_OnAttackingListeners = (CActor.AttackListener)Delegate.Combine(newActor.m_OnAttackingListeners, new CActor.AttackListener(OnAttacking));
		newActor.m_OnBeingAttackedPreDamageListeners = (CActor.BeingAttackedPreDamageListener)Delegate.Combine(newActor.m_OnBeingAttackedPreDamageListeners, new CActor.BeingAttackedPreDamageListener(OnBeingAttackedPreDamage));
		newActor.m_OnBeingAttackedPostDamageListeners = (CActor.BeingAttackedPostDamageListener)Delegate.Combine(newActor.m_OnBeingAttackedPostDamageListeners, new CActor.BeingAttackedPostDamageListener(OnBeingAttackedPostDamage));
		newActor.m_OnBeingAttackedListeners = (CActor.BeingAttackedListener)Delegate.Combine(newActor.m_OnBeingAttackedListeners, new CActor.BeingAttackedListener(OnBeingAttacked));
		newActor.m_OnAttackSingleTargetListeners = (CActor.AttackSingleTargetStartListener)Delegate.Combine(newActor.m_OnAttackSingleTargetListeners, new CActor.AttackSingleTargetStartListener(OnAttackingSingleTargetStart));
		newActor.m_OnAttackAbilityFinishedListeners = (CActor.AttackAbilityFinishedListener)Delegate.Combine(newActor.m_OnAttackAbilityFinishedListeners, new CActor.AttackAbilityFinishedListener(OnAttackAbilityFinished));
		newActor.m_OnAttackFinishedListeners = (CActor.AttackFinishedListener)Delegate.Combine(newActor.m_OnAttackFinishedListeners, new CActor.AttackFinishedListener(OnAttackFinished));
		newActor.m_OnHealApplyToActorListeners = (CActor.HealApplyToActorListener)Delegate.Combine(newActor.m_OnHealApplyToActorListeners, new CActor.HealApplyToActorListener(OnHealApplyToActor));
		newActor.m_OnHealApplyToActionListeners = (CActor.HealApplyToActionListener)Delegate.Combine(newActor.m_OnHealApplyToActionListeners, new CActor.HealApplyToActionListener(OnHealApplyToAction));
		newActor.m_OnBeingHealedListeners = (CActor.BeingHealedListener)Delegate.Combine(newActor.m_OnBeingHealedListeners, new CActor.BeingHealedListener(OnBeingHealed));
		newActor.m_OnAfterBeingHealedListeners = (CActor.AfterBeingHealedListener)Delegate.Combine(newActor.m_OnAfterBeingHealedListeners, new CActor.AfterBeingHealedListener(OnAfterBeingHealed));
		newActor.m_OnActorImmuneToConditionListeners = (CActor.OnActorImmuneToConditionListener)Delegate.Combine(newActor.m_OnActorImmuneToConditionListeners, new CActor.OnActorImmuneToConditionListener(OnActorImmuneToCondition));
		newActor.m_OnConditionApplyToActorListeners = (CActor.ConditionApplyToActorListener)Delegate.Combine(newActor.m_OnConditionApplyToActorListeners, new CActor.ConditionApplyToActorListener(OnConditionApplyToActor));
		newActor.m_OnPositiveConditionEndedOnActorListeners = (CActor.PositiveConditionEndedOnActorListener)Delegate.Combine(newActor.m_OnPositiveConditionEndedOnActorListeners, new CActor.PositiveConditionEndedOnActorListener(OnPositiveConditionEndedOnActor));
		newActor.m_OnNegativeConditionEndedOnActorListeners = (CActor.NegativeConditionEndedOnActorListener)Delegate.Combine(newActor.m_OnNegativeConditionEndedOnActorListeners, new CActor.NegativeConditionEndedOnActorListener(OnNegativeConditionEndedOnActor));
		newActor.m_OnMoveStartListeners = (CActor.MoveStartListener)Delegate.Combine(newActor.m_OnMoveStartListeners, new CActor.MoveStartListener(OnMoveStart));
		newActor.m_OnMovingListeners = (CActor.MoveListener)Delegate.Combine(newActor.m_OnMovingListeners, new CActor.MoveListener(OnMoving));
		newActor.m_OnMovedListeners = (CActor.MovedListener)Delegate.Combine(newActor.m_OnMovedListeners, new CActor.MovedListener(OnMoved));
		newActor.m_OnCarriedListeners = (CActor.CarriedListener)Delegate.Combine(newActor.m_OnCarriedListeners, new CActor.CarriedListener(OnCarried));
		newActor.m_OnLootListeners = (CActor.LootListener)Delegate.Combine(newActor.m_OnLootListeners, new CActor.LootListener(OnLoot));
		newActor.m_OnRetaliateListeners = (CActor.RetaliateListener)Delegate.Combine(newActor.m_OnRetaliateListeners, new CActor.RetaliateListener(OnRetaliate));
		newActor.m_OnDamagedListeners = (CActor.DamagedListener)Delegate.Combine(newActor.m_OnDamagedListeners, new CActor.DamagedListener(OnDamaged));
		newActor.m_OnTakenDamageListeners = (CActor.TakeDamageListener)Delegate.Combine(newActor.m_OnTakenDamageListeners, new CActor.TakeDamageListener(OnTakenDamage));
		newActor.m_OnKillListeners = (CActor.KillListener)Delegate.Combine(newActor.m_OnKillListeners, new CActor.KillListener(OnKill));
		newActor.m_OnDeathListeners = (CActor.DeathListener)Delegate.Combine(newActor.m_OnDeathListeners, new CActor.DeathListener(OnDeath));
		newActor.m_AfterDeathListeners = (CActor.DeathListener)Delegate.Combine(newActor.m_AfterDeathListeners, new CActor.DeathListener(AfterDeath));
		newActor.m_OnDrawModifierListeners = (CActor.DrawModifierListener)Delegate.Combine(newActor.m_OnDrawModifierListeners, new CActor.DrawModifierListener(OnDrawModifier));
		newActor.m_OnPreventDamageListeners = (CActor.PreventDamageListener)Delegate.Combine(newActor.m_OnPreventDamageListeners, new CActor.PreventDamageListener(OnPreventDamageTriggered));
		newActor.m_OnCreatedListeners = (CActor.CreatedListener)Delegate.Combine(newActor.m_OnCreatedListeners, new CActor.CreatedListener(OnCreated));
		newActor.m_OnPropCreatedListeners = (CActor.PropCreated)Delegate.Combine(newActor.m_OnPropCreatedListeners, new CActor.PropCreated(OnPropCreated));
		newActor.m_OnPropDestroyedListeners = (CActor.PropDestroyed)Delegate.Combine(newActor.m_OnPropDestroyedListeners, new CActor.PropDestroyed(OnPropDestroyed));
		OnListenersRefreshed(newActor);
	}

	public bool AppliesTo(CActor actor)
	{
		if (actor == null)
		{
			return false;
		}
		if (actor == m_Actor && !m_ActiveBonus.IsAura)
		{
			return true;
		}
		if (m_AdditionalActors != null && m_AdditionalActors.Contains(actor))
		{
			return true;
		}
		return false;
	}

	public void Finish()
	{
		m_ActiveBonus.Finish();
	}

	public CAbility.EAbilityType GetAbilityType()
	{
		return m_Ability.AbilityType;
	}

	public virtual void OnFinished()
	{
	}

	public virtual void OnMoveStart(CAbilityMove moveAbility)
	{
	}

	public virtual void OnAttackStart(CAbilityAttack attackAbility)
	{
	}

	public virtual void OnAbilityTargetingStart(CAbility ability)
	{
	}

	public virtual void OnPreActorIsAttacking(CAbilityAttack attackAbility)
	{
	}

	public virtual void OnHealApplyToActor(CAbilityHeal healAbility)
	{
	}

	public virtual void OnHealApplyToAction(CAbilityHeal healAbility)
	{
	}

	public virtual void OnBeingHealed(CAbilityHeal healAbility)
	{
	}

	public virtual void OnAfterBeingHealed()
	{
	}

	public virtual void OnActorImmuneToCondition(CAbility conditionAbility, CActor target, bool isActorImmune)
	{
	}

	public virtual void OnConditionApplyToActor(CAbility conditionAbility, CActor target)
	{
	}

	public virtual void OnPositiveConditionEndedOnActor(CCondition.EPositiveCondition positiveConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
	}

	public virtual void OnNegativeConditionEndedOnActor(CCondition.ENegativeCondition negativeConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
	}

	public virtual void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
	}

	public virtual void OnAttackingSingleTargetStart(CAbilityAttack ability, CActor target)
	{
	}

	public virtual void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
	}

	public virtual void OnBeingAttackedPreDamage(CAbilityAttack attackAbility)
	{
	}

	public virtual void OnBeingAttackedPostDamage(CAbilityAttack attackAbility, CAttackSummary.TargetSummary targetSummary)
	{
	}

	public virtual void OnMoving(CAbilityMove moveAbility)
	{
	}

	public virtual void OnLoot(CAbilityLoot lootAbility)
	{
	}

	public virtual void OnAttackFinished(CAbilityAttack attackAbility, CActor target, int damageDealt)
	{
	}

	public virtual void OnAttackAbilityFinished(CAbilityAttack attackAbility)
	{
	}

	public virtual void OnRetaliate()
	{
	}

	public virtual void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		activeBonus.TriggerOnBonusToggledMiscVar(currentAbility, activeBonus, toggledOn);
	}

	public virtual void OnActiveBonusToggled(CActor currentActor, CActiveBonus activeBonus, bool toggledOn)
	{
		activeBonus.TriggerOnBonusToggledMiscVar(activeBonus.Ability, activeBonus, toggledOn);
	}

	public virtual void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage, CActor damageSourceActor)
	{
	}

	public virtual void OnKill(CActor target, CActor actor, CActor.ECauseOfDeath causeOfDeath, CAbility causeOfDeathAbility, bool onActorTurn)
	{
	}

	public virtual void OnMoved(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition)
	{
	}

	public virtual void OnCarried(CAbility moveAbility, CActor movedActor, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes)
	{
	}

	public virtual void OnDrawModifier(EAdvantageStatuses advantageStatus, ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target)
	{
	}

	public virtual void OnActionEnded(CActor actorEndingAction)
	{
		AppliedThisAction = false;
	}

	public virtual void OnAbilityPerformStarted(CAbility ability)
	{
		PerformStateVars("OnAbilityPerformStarted", ability);
	}

	public virtual void OnAbilityStarted(CAbility startedAbility)
	{
	}

	public virtual void OnAbilityEnded(CAbility endedAbility)
	{
		PerformStateVars("OnAbilityEnded", endedAbility);
	}

	public virtual void OnAbilityUndone(CAbility endedAbility)
	{
	}

	public virtual void OnListenersRefreshed(CActor actor)
	{
	}

	public virtual void OnCreated(CAbility createAbility)
	{
	}

	public virtual void OnAbilityOverrideApplied(CActor actor, CAbilityOverride abilityOverride, CAbility ability, bool active)
	{
	}

	public virtual void OnDamaged(GameState.DamageData damageData)
	{
	}

	public virtual void OnDeath(CActor actor, CAbility causeOfDeathAbility)
	{
	}

	public virtual void AfterDeath(CActor actor, CAbility causeOfDeathAbility)
	{
	}

	public virtual void OnPropDestroyed(CActor actor, CAbility endedAbility, CObjectProp prop)
	{
	}

	public virtual void OnPropCreated(CActor actor, CAbility endedAbility, CObjectProp prop)
	{
	}

	public virtual int ReferenceStrength(CAbility ability, CActor target)
	{
		if (m_ActiveBonusData.StrengthIsScalar || !IsValidTarget(ability, target))
		{
			return 0;
		}
		return CheckStatIsBasedOnXType();
	}

	public virtual int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (!m_ActiveBonusData.StrengthIsScalar || !IsValidTarget(ability, target))
		{
			return 1;
		}
		return CheckStatIsBasedOnXType();
	}

	public virtual int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (!m_ActiveBonusData.StrengthIsScalar || !IsValidTarget(ability, target))
		{
			return 1;
		}
		return CheckStatIsBasedOnXType();
	}

	public virtual int ReferenceXP(CAbility ability, CActor target)
	{
		if (!IsValidTarget(ability, target))
		{
			return 0;
		}
		return m_ActiveBonusData.ProcXP;
	}

	public virtual string ReferenceAnimOverload(CAbility ability, CActor target)
	{
		if (!IsValidTarget(ability, target))
		{
			return string.Empty;
		}
		return m_ActiveBonusData.ActiveBonusAnimOverload;
	}

	public virtual AbilityData.EActiveBonusFlag ReferenceFlags(CAbility ability, CActor target)
	{
		if (!IsValidTarget(ability, target))
		{
			return AbilityData.EActiveBonusFlag.None;
		}
		return m_ActiveBonusData.Flags;
	}

	public virtual bool ActiveBonusIsActivatedByTile(CTile tile)
	{
		return false;
	}

	public virtual bool StackActiveBonusInlineAbility(CActor target, CAbility ability)
	{
		return false;
	}

	public virtual void UpdateActiveBonusInlineAbilityTarget(CActor target)
	{
	}

	public virtual bool IsHiddenInUI()
	{
		return false;
	}

	protected int CheckStatIsBasedOnXType(CAbility sourceAbility = null)
	{
		CActor cActor = sourceAbility?.TargetingActor ?? m_Ability.TargetingActor ?? m_Actor;
		if (m_Ability != null && m_Actor != null && cActor != null && m_Ability.StatIsBasedOnXEntries != null && m_Ability.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength) && m_ActiveBonusData.Filter.IsValidTarget(m_Actor, cActor, m_Ability.IsTargetedAbility, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible))
		{
			m_Ability.ResetStatBasedOnXAddedStats(m_Ability.StatIsBasedOnXEntries);
			m_Ability.SetStatBasedOnX(cActor, m_Ability.StatIsBasedOnXEntries, m_ActiveBonusData.Filter, m_Actor);
			m_Ability.SetStatBasedOnXFromTarget(m_Actor, cActor, m_Ability.StatIsBasedOnXEntries, m_ActiveBonusData.Filter, sourceAbility);
			return m_Ability.Strength;
		}
		return m_Strength;
	}

	public virtual void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
	}

	protected virtual bool IsValidTarget(CAbility ability, CActor target, bool useTargetOriginalType = false)
	{
		CVarsContext currentVarsContext = CAbilityFilter.CurrentVarsContext;
		if (ability != null && currentVarsContext == null)
		{
			CAbilityFilter.CurrentVarsContext = ability.VarsContext;
		}
		bool result = false;
		if (ability is CAbilityAttack cAbilityAttack)
		{
			if (target != null && cAbilityAttack != null)
			{
				CAbilityFilterContainer filter = m_ActiveBonusData.Filter;
				CActor targetingActor = cAbilityAttack.TargetingActor;
				bool isTargetedAbility = cAbilityAttack.IsTargetedAbility;
				bool? canTargetInvisible = cAbilityAttack.MiscAbilityData?.CanTargetInvisible;
				if (filter.IsValidTarget(target, targetingActor, isTargetedAbility, useTargetOriginalType, canTargetInvisible, skipUntargetableCheck: false, ActiveBonus) && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonus.IsValidAttackType(cAbilityAttack))
				{
					result = true;
				}
			}
		}
		else if (target != null && ability != null && ability.TargetingActor != null)
		{
			CAbilityFilterContainer filter2 = m_ActiveBonusData.Filter;
			CActor targetingActor2 = ability.TargetingActor;
			bool isTargetedAbility2 = ability.IsTargetedAbility;
			bool? canTargetInvisible = ability.MiscAbilityData?.CanTargetInvisible;
			if (filter2.IsValidTarget(target, targetingActor2, isTargetedAbility2, useTargetOriginalType, canTargetInvisible, skipUntargetableCheck: false, ActiveBonus) && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished())
			{
				result = true;
			}
		}
		CAbilityFilter.CurrentVarsContext = currentVarsContext;
		return result;
	}

	protected virtual bool IsValidAbilityType(CAbility conditionAbility)
	{
		bool flag = m_ActiveBonusData.ValidAbilityTypes.Contains(conditionAbility.AbilityType);
		if (m_ActiveBonusData.ValidAbilityTypes.Count > 0)
		{
			if (!m_ActiveBonusData.InvertValidity)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	protected virtual bool IsValidAttackType(CAbility ability)
	{
		if (ability is CAbilityAttack attackAbility)
		{
			return m_ActiveBonus.IsValidAttackType(attackAbility);
		}
		return true;
	}

	public virtual bool ValidTargetTypeFilters(CAbility abilityToCheck)
	{
		bool result = true;
		if (abilityToCheck.AbilityFilter != null)
		{
			foreach (CAbilityFilter.EFilterTargetType validTargetType in m_ActiveBonusData.ValidTargetTypes)
			{
				if (!abilityToCheck.AbilityFilter.HasTargetTypeFlag(validTargetType, m_ActiveBonusData.ValidTargetTypesExclusive))
				{
					result = false;
				}
			}
		}
		return result;
	}

	protected bool IsValidPositiveConditionType(CCondition.EPositiveCondition positiveCondition)
	{
		return m_ActiveBonusData.ValidPositiveConditionTypes.Contains(positiveCondition);
	}

	protected bool IsValidNegativeConditionType(CCondition.ENegativeCondition negativeCondition)
	{
		return m_ActiveBonusData.ValidNegativeConditionTypes.Contains(negativeCondition);
	}

	protected void TickTracker()
	{
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, m_Actor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		m_ActiveBonus.FinishIfNoRemaining();
	}

	public virtual void CleanUpBeforeSave()
	{
	}

	public CAbility GetAbilityToAdd(CAbilityAddActiveBonus AbilityAddActiveBonus)
	{
		CAbility result = AbilityAddActiveBonus.AddAbility;
		TValue miscVar = ActiveBonus.Ability.ActiveBonusData.GetMiscVar("OverrideAddActiveBonusAbility");
		if (!miscVar.isUnknown)
		{
			CVars cVars = new CVars();
			CVarsContext context = new CVarsContext(cVars);
			cVars.Set("card", new TValue(AbilityAddActiveBonus.GetAbilityBaseCard()));
			CAbility cAbility = miscVar.Calc(context).GetObject<CAbility>(context);
			if (cAbility != null)
			{
				result = cAbility;
			}
		}
		return result;
	}

	public virtual void Save(List<Tuple<string, object>> toSave)
	{
	}

	public virtual void Load(List<Tuple<string, object>> toSave)
	{
	}

	public void PerformStateVars(string stateName, CAbility currentAbility)
	{
		CalcPerformVars(stateName, currentAbility);
		FirePerformEvent(stateName, currentAbility);
	}

	public virtual void CalcPerformVars(string stateName, CAbility currentAbility)
	{
		List<(string, List<(string, TValue)>)> performVars = ActiveBonus.Ability.ActiveBonusData.PerformVars;
		if (performVars == null)
		{
			return;
		}
		List<(string, TValue)> list = null;
		foreach (var (text, list2) in performVars)
		{
			if (!(text != stateName))
			{
				list = list2;
				break;
			}
		}
		if (list == null)
		{
			return;
		}
		CVars cVars = new CVars();
		cVars.Set("obj", new TValue(this));
		cVars.Set("BespokeBehaviour", new TValue(this));
		cVars.Set("ActiveBonus", new TValue(ActiveBonus));
		cVars.Set("Ability", new TValue(currentAbility));
		CVarsContext context = new CVarsContext(cVars);
		foreach (var item3 in list)
		{
			string item = item3.Item1;
			TValue item2 = item3.Item2;
			TValue tValue = item2.Calc(context);
			cVars.Set(item, tValue);
			if (tValue.isValid)
			{
				SharedAbilityTargeting.ProcessTargetingValue(currentAbility, item, item2, tValue);
			}
		}
	}

	public virtual void FirePerformEvent(string stateName, CAbility currentAbility)
	{
		CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericActionPerform, currentAbility, null, null, ("state", stateName));
	}
}
