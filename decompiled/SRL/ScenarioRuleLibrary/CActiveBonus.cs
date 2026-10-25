using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.Threading;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CActiveBonus
{
	public enum EActiveBonusDurationType
	{
		NA,
		Ability,
		Round,
		Persistent,
		Summon,
		Turn
	}

	public enum EActiveBonusBehaviourType
	{
		None,
		BuffAttack,
		BuffSingleAttack,
		BuffHealPerActor,
		BuffHealPerAction,
		BuffRange,
		BuffTarget,
		ShieldIncomingAttacks,
		Retaliate,
		PreventAndRetaliate,
		MultiplyAddedConditions,
		EndTurnAbility,
		StartTurnAbility,
		OverrideAbility,
		ShieldTargetedAttacks,
		ShieldAndRetaliate,
		CastAbilityFromSummon,
		DuringActionAbility,
		BuffMove,
		MoveScript,
		ScenarioScript,
		ShieldAndDisadvantage,
		DefaultToggleBehaviour,
		BuffIncomingAttacks,
		BuffRetaliate,
		SingleShotRetaliate,
		PreventAndApplyToActiveBonusCaster,
		PreventAndDamageActiveBonusCaster,
		DuringActionAbilityOnAttacked,
		BuffIncomingHeal,
		DuringActionAbilityOnKill,
		DuringActionAbilityOnMoved,
		AdjustInitiative,
		DuringActionAbilityOnAttackAbilityFinished,
		DuringActionAbilityOnAttackStart,
		AdjustWhenAttacked,
		FocusInitiative,
		ApplyConditionalConditions,
		StartActionAbility,
		EndActionAbility,
		DuringActionAbilityOnLongRest,
		DuringActionAbilityOnShortRest,
		DuringActionAbilityOnDeath,
		DuringActionAbilityAfterDeath,
		PreCalcInitiativeAbility,
		StartRoundAbility,
		EndRoundAbility,
		DamageOnCurseModApplied,
		StartTurnAbilityAfterXCasterTurns,
		DuringActionAbilityOnDamaged,
		BuffLoot,
		DuringTurnAbility,
		DamageOnReceivedDamage,
		AddAccumulativeOverhealAndHealIfMax,
		SetMinimumOverheal,
		PreventDamageByDiscardingPlayerCards,
		LimitLoot,
		HealOnHealingReceived,
		ApplyConditionOnLoseCondition,
		DuringActionAbilityOnAttacking,
		AddConditionUntilDamaged,
		AddAccumulativeOverheal,
		DuringActionAbilityOnMovedIntoCasterHex,
		PreventAndFullyHeal,
		DuringActionAbilityOnPreventedDamage,
		DuringActionAbilityOnFinishedMovement,
		HealOnDamageReceived,
		DuringActionAbilityOnAbilityEnded,
		DuringActionAbilityOnAttackFinished,
		DuringActionAbilityOnHealed,
		DuringActionAbilityOnHealedToFull,
		DuringActionAbilityOnCreated,
		OverrideMoveAbility,
		DuringActionAbilityOnMovedORCarried,
		BlockRetaliate,
		BuffShield,
		PreventFirstDamage,
		ImmunityToConditionAfterAttackDamage,
		DuringActionAbilityOnActionEnded,
		MirrorAll,
		AdjustWhenAttacking,
		ChooseCondition,
		ApplyMainBeforeAttackResolve,
		PreventLoseCardToPreventDamage,
		StartShortRest,
		AddAccumulativeOverhealAndHeal,
		ShadowBuffAttack,
		ShadowBuff,
		PreventPartialDamage,
		BuffForcedMovement,
		DuringActionAbilityOnActionEndedAndLongRest,
		RepeatAbility,
		BetweenActionAbilities,
		SetInitiative,
		PreventBySettingRetaliateToValue,
		FocusInitiativeBeforeSummons,
		[Obsolete]
		ChooseImproveAbilityProperty,
		TriggerOnFinished,
		MirrorAbilities,
		TransferDamage,
		ShowNextAttackModifierBehaviour,
		StartRoundIfExhausted,
		DuringActionAbilityOnConditionEnded,
		Generic_First,
		GenericAttackFinished,
		GenericHealFinished,
		GenericHealAbilityFinished,
		GenericActionPerform,
		GenericTrapActivating,
		GenericTrapActivated,
		GenericTrapsIgnoreFlyAndJump,
		GenericOnDrawAttackModifier,
		GenericOnNegativeCondition,
		GenericEndRound,
		GenericAfterRevealMonsterCards,
		GenericRoomRevealed,
		GenericOnLoot,
		GenericOnFinishLooting,
		GenericBeforeRevealMonsterCards,
		GenericPreventKillAbility,
		GenericAfterConfirmShields,
		Generic_Last
	}

	public enum EActiveBonusRestrictionType
	{
		None,
		OncePerAction,
		OncePerTurn,
		OncePerRound,
		OncePerScenario
	}

	public enum EAuraTriggerAbilityAnimType
	{
		None,
		AnimateAuraOriginator,
		AnimateAuraReceiver,
		AnimateBoth
	}

	public class ActiveBonusThreadData : CachedThreadPropertyData
	{
		public readonly Dictionary<CActor, List<CActiveBonus>> CachedActorBonuses = new Dictionary<CActor, List<CActiveBonus>>();

		public List<CObjectProp> CachedProps;

		public List<CActor> CachedAllAliveActors;

		public List<CActor> CachedAllActors;

		public override void Reset()
		{
			base.Reset();
			CachedActorBonuses.Clear();
			CachedProps = null;
			CachedAllAliveActors = null;
			CachedAllActors = null;
		}
	}

	public class ActiveBonusCalculationScope : ThreadCalculationScope<ActiveBonusThreadData>
	{
		private static ActiveBonusThreadData GetData()
		{
			CachedProperties propertiesForThisThread = ThreadCalculationScope<ActiveBonusThreadData>.GetPropertiesForThisThread(createNewIfNeeded: false);
			if (propertiesForThisThread == null || propertiesForThisThread.Scopes == 0)
			{
				return null;
			}
			return propertiesForThisThread.CachedData ?? (propertiesForThisThread.CachedData = new ActiveBonusThreadData());
		}

		public static List<CActiveBonus> GetBonuses(CActor actor)
		{
			ActiveBonusThreadData data = GetData();
			if (data == null)
			{
				return null;
			}
			if (data.CachedActorBonuses.TryGetValue(actor, out var value))
			{
				return value;
			}
			return null;
		}

		public static List<CObjectProp> GetProps()
		{
			ActiveBonusThreadData data = GetData();
			if (data == null)
			{
				return null;
			}
			if (data.CachedProps != null)
			{
				return data.CachedProps;
			}
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				data.CachedProps = ScenarioManager.CurrentScenarioState.Props.ToList();
			}
			return data.CachedProps;
		}

		public static List<CActor> GetAllAliveActors()
		{
			ActiveBonusThreadData data = GetData();
			if (data == null)
			{
				return null;
			}
			if (data.CachedAllAliveActors != null)
			{
				return data.CachedAllAliveActors;
			}
			data.CachedAllAliveActors = ScenarioManager.Scenario.AllAliveActors;
			return data.CachedAllAliveActors;
		}

		public static List<CActor> GetAllActors()
		{
			ActiveBonusThreadData data = GetData();
			if (data == null)
			{
				return null;
			}
			if (data.CachedAllActors != null)
			{
				return data.CachedAllActors;
			}
			data.CachedAllActors = ScenarioManager.Scenario.AllActors;
			return data.CachedAllActors;
		}

		public static void TryCache(CActor actor, List<CActiveBonus> bonuses)
		{
			ActiveBonusThreadData data = GetData();
			if (data != null)
			{
				data.CachedActorBonuses[actor] = bonuses;
			}
		}
	}

	public static readonly EActiveBonusDurationType[] ActiveBonusDurationTypes = (EActiveBonusDurationType[])Enum.GetValues(typeof(EActiveBonusDurationType));

	public static readonly EActiveBonusBehaviourType[] ActiveBonusTypes = (EActiveBonusBehaviourType[])Enum.GetValues(typeof(EActiveBonusBehaviourType));

	public static readonly EActiveBonusRestrictionType[] ActiveBonusRestrictionTypes = (EActiveBonusRestrictionType[])Enum.GetValues(typeof(EActiveBonusRestrictionType));

	public static readonly EAuraTriggerAbilityAnimType[] AuraTriggerAbilityAnimTypes = (EAuraTriggerAbilityAnimType[])Enum.GetValues(typeof(EAuraTriggerAbilityAnimType));

	private CBaseCard m_BaseCard;

	private CAbility m_Ability;

	private bool m_Finishing;

	private bool m_Finished;

	private int m_TrackerIndex;

	private List<CActor> m_ValidActorsInRangeOfAura;

	private bool m_HasTracker;

	private List<CActor> m_RestrictedActors = new List<CActor>();

	private int m_ActiveBonusStartRound;

	private bool? m_RequirementsWereMet;

	protected List<CBespokeBehaviour> m_BespokeBehaviours = new List<CBespokeBehaviour>();

	public static bool RestoreIDCounter;

	public static bool ProtectIDCounter;

	public static bool CreateToggledActiveBonusHack;

	private static bool m_CheckingImpairReq = false;

	private static List<(CActiveBonus auraActiveBonus, List<CActor> oldActors)> recalcCache = new List<(CActiveBonus, List<CActor>)>();

	public int ID { get; set; }

	public CBaseCard BaseCard => m_BaseCard;

	public CAbility Ability => m_Ability;

	public EActiveBonusDurationType Duration { get; set; }

	public ActiveBonusLayout Layout
	{
		get
		{
			return m_Ability.ActiveBonusYML;
		}
		set
		{
			m_Ability.ActiveBonusYML = value;
		}
	}

	public int TrackerIndex
	{
		get
		{
			return m_TrackerIndex;
		}
		set
		{
			m_TrackerIndex = Math.Clamp(value, 0, TrackerMaxIndex + 1);
		}
	}

	public int ActiveBonusStartRound => m_ActiveBonusStartRound;

	public CBespokeBehaviour BespokeBehaviour => m_BespokeBehaviour;

	public List<CBespokeBehaviour> BespokeBehaviours => m_BespokeBehaviours;

	public int Remaining => CalcRemainingTrackersOnIndex(m_TrackerIndex);

	public bool ValidToWorkAccordingToTracker => WillBeValidOnTrackerIndex(TrackerIndex);

	public int RemainingRaw
	{
		get
		{
			if (m_HasTracker)
			{
				ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
				if (activeBonusYML != null && activeBonusYML.TrackerPattern?.Count > 0)
				{
					return m_Ability.ActiveBonusYML.TrackerPattern.Count - m_TrackerIndex;
				}
				AbilityData.ActiveBonusData activeBonusData = m_Ability.ActiveBonusData;
				if (activeBonusData != null && activeBonusData.Tracker?.Count > 0)
				{
					return m_Ability.ActiveBonusData.Tracker.Count - m_TrackerIndex;
				}
			}
			return -m_TrackerIndex;
		}
	}

	public List<int> Tracker
	{
		get
		{
			if (m_HasTracker)
			{
				ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
				if (activeBonusYML != null && activeBonusYML.TrackerPattern?.Count > 0)
				{
					return m_Ability.ActiveBonusYML.TrackerPattern;
				}
				AbilityData.ActiveBonusData activeBonusData = m_Ability.ActiveBonusData;
				if (activeBonusData != null && activeBonusData.Tracker?.Count > 0)
				{
					return m_Ability.ActiveBonusData.Tracker;
				}
			}
			return null;
		}
	}

	public int TrackerMaxIndex
	{
		get
		{
			if (m_HasTracker && Tracker != null)
			{
				return Tracker.Count - 1;
			}
			return 0;
		}
	}

	public int InternalRoundNumber => ScenarioManager.CurrentScenarioState.RoundNumber - ActiveBonusStartRound;

	public CActor Actor { get; set; }

	public CActor Caster { get; private set; }

	public bool IsAura
	{
		get
		{
			if (m_Ability == null || m_Ability.ActiveBonusData == null)
			{
				return false;
			}
			return m_Ability.ActiveBonusData.IsAura;
		}
	}

	public CAbilityFilterContainer AuraFilter => m_Ability.ActiveBonusData.AuraFilter;

	public CAbility.EAbilityTargeting AuraTargeting => m_Ability.ActiveBonusData.AuraTargeting;

	public bool HasTracker
	{
		get
		{
			return m_HasTracker;
		}
		set
		{
			m_HasTracker = value;
		}
	}

	public bool IsAugment { get; set; }

	public bool IsSong { get; set; }

	public bool IsDoom { get; set; }

	public bool ToggledBonus { get; set; }

	public bool ToggleLocked { get; set; }

	public bool ApplyToAction { get; set; }

	public ElementInfusionBoardManager.EElement? ToggledElement { get; set; }

	public List<CCharacterResource> ToggledResourcesToConsume { get; set; }

	public bool FullCopy => m_Ability.ActiveBonusData.FullCopy;

	public bool NeedsDamage => m_Ability.ActiveBonusData.NeedsDamage;

	public bool TriggerAbilitiesImmediately => m_Ability.ActiveBonusData.TriggerAbilitiesImmediately;

	public List<CActor> ValidActorsInRangeOfAura => m_ValidActorsInRangeOfAura;

	public List<CActor> RestrictedActors => m_RestrictedActors;

	public bool CostAbilityExecuted { get; set; }

	[StateDump(Skip = true)]
	public CActor SingleTarget { get; set; }

	protected CBespokeBehaviour m_BespokeBehaviour
	{
		get
		{
			if (m_BespokeBehaviours.Count > 0)
			{
				return m_BespokeBehaviours[0];
			}
			return null;
		}
		set
		{
			m_BespokeBehaviours.Clear();
			AddBespokeBehaviour(value);
		}
	}

	public static int IDCounter { get; set; }

	public virtual bool Finished()
	{
		return m_Finished;
	}

	public bool Finishing()
	{
		return m_Finishing;
	}

	public bool WillBeValidOnReachTargetActor(CAbility ability, CActor target)
	{
		int id = TrackerIndex;
		if (Ability.ActiveBonusData.IsSingleTargetBonus && ability?.ActorsToTarget != null && ability.ActorsToTarget.Count > 0 && !ability.AbilityHasHappened)
		{
			List<CActor> list = ability.ActorsToTarget.ToList();
			list.RemoveAll((CActor x) => !Ability.ActiveBonusData.Filter.IsValidTarget(x, Actor, ability.IsTargetedAbility, useTargetOriginalType: false, activeBonus: this, canTargetInvisible: false));
			int num = list.IndexOf(target);
			if (num == -1)
			{
				num = list.Count;
			}
			id = TrackerIndex + num;
		}
		return WillBeValidOnTrackerIndex(id);
	}

	public bool WillBeValidOnTrackerIndex(int id)
	{
		if (Ability.ActiveBonusData.IsToggleBonus)
		{
			return true;
		}
		if (Ability.ActiveBonusData.Restriction != EActiveBonusRestrictionType.None && id > 0)
		{
			return false;
		}
		int num = CalcRemainingTrackersOnIndex(id);
		if (Ability.ActiveBonusData.OnlyWorkAfterFinishedTracker)
		{
			return num == 0;
		}
		TValue var = Ability.ActiveBonusData.GetVar("ValidOnTrackerIndex");
		if (!var.isUnknown)
		{
			int num2 = var.GetInt();
			return id == num2;
		}
		if (num <= 0)
		{
			return !HasTracker;
		}
		return true;
	}

	public int CalcRemainingTrackersOnIndex(int id)
	{
		if (m_HasTracker)
		{
			ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
			if (activeBonusYML != null && activeBonusYML.TrackerPattern?.Count > 0)
			{
				return m_Ability.ActiveBonusYML.TrackerPattern.Count - id;
			}
			AbilityData.ActiveBonusData activeBonusData = m_Ability.ActiveBonusData;
			if (activeBonusData != null && activeBonusData.Tracker?.Count > 0)
			{
				return m_Ability.ActiveBonusData.Tracker.Count - id;
			}
		}
		return 0;
	}

	public override string ToString()
	{
		return $"[{MF.RefStr(this)}] {GetType().Name}({m_Ability})";
	}

	protected void AddBespokeBehaviour(CBespokeBehaviour behaviour)
	{
		if (!m_BespokeBehaviours.Contains(behaviour))
		{
			m_BespokeBehaviours.Add(behaviour);
		}
	}

	public CBespokeBehaviour GetBehaviour(params EActiveBonusBehaviourType[] types)
	{
		if (m_BespokeBehaviours == null)
		{
			return null;
		}
		return m_BespokeBehaviours.SingleOrDefault((CBespokeBehaviour x) => types.Contains(x.BehaviourType));
	}

	public T GetBehaviour<T>() where T : CBespokeBehaviour
	{
		if (m_BespokeBehaviours == null)
		{
			return null;
		}
		return m_BespokeBehaviours.SingleOrDefault((CBespokeBehaviour x) => x is T) as T;
	}

	public bool HasBehaviour(params EActiveBonusBehaviourType[] types)
	{
		if (GetBehaviour(types) == null)
		{
			return m_Ability.ActiveBonusData.HasBehaviour(types);
		}
		return true;
	}

	public bool HasBehaviour<T>() where T : CBespokeBehaviour
	{
		return GetBehaviour<T>() != null;
	}

	public bool TryGetBehaviour<T>(out T behaviour, params EActiveBonusBehaviourType[] types) where T : CBespokeBehaviour
	{
		behaviour = GetBehaviour(types) as T;
		return behaviour != null;
	}

	public bool TryGetBehaviour<T>(out T behaviour) where T : CBespokeBehaviour
	{
		behaviour = GetBehaviour<T>();
		return behaviour != null;
	}

	public bool TryGetBehaviour(out CBespokeBehaviour behaviour, params EActiveBonusBehaviourType[] types)
	{
		return this.TryGetBehaviour<CBespokeBehaviour>(out behaviour, types);
	}

	private static CActiveBonus GetBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int? iD, int? remaining, int activeBonusStartRound)
	{
		switch (ability.AbilityType)
		{
		case CAbility.EAbilityType.Attack:
			return new CAttackActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Move:
			return new CMoveActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Loot:
			return new CLootActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Shield:
			return new CShieldActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Retaliate:
			return new CRetaliateActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Damage:
			return new CDamageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Summon:
			return new CSummonActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD);
		case CAbility.EAbilityType.Advantage:
			return new CAdvantageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.PreventDamage:
			return new CPreventDamageActiveBonus(baseCard, ability as CAbilityPreventDamage, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.MirrorDamage:
			return new CMirrorDamageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ImmunityTo:
			return new CImmunityActiveBonus(baseCard, ability as CAbilityImmunityTo, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Redirect:
			return new CRedirectActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.TransferDamage:
			return new CTransferDamageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddTarget:
			return new CAddTargetActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Overheal:
			return new COverhealActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddHeal:
			return new CAddHealActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddRange:
			return new CAddRangeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AttackersGainDisadvantage:
			return new CAttackersGainDisadvantageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddCondition:
			return new CAddConditionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Infuse:
			return new CInfuseActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AdjustInitiative:
			return new CAdjustInitiativeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ForgoActionsForCompanion:
			return new CForgoActionsForCompanionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChangeModifier:
			return new CChangeModifierActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChangeCondition:
			return new CChangeConditionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Immovable:
			((CAbilityImmovable)ability).ApplyThisCondition(actor);
			return new CActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChooseAbility:
			return new CChooseAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Invulnerability:
			return new CInvulnerabilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.PierceInvulnerability:
			return new CPierceInvulnerabilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Untargetable:
			return new CUntargetableActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ItemLock:
			return new CItemLockActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChangeCharacterModel:
			return new CChangeCharacterModelActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.HealthReduction:
			return new CHealthReductionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.DisableCardAction:
			return new CDisableCardActionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.RepeatAbility:
			return new CRepeatAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.PreventLoseCard:
			return new CPreventLoseCardActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.CreateShadow:
			return new CCreateShadowActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ShowNextAttackModifier:
			return new CShowNextAttackModifierActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		default:
			return new CActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		}
	}

	public static CActiveBonus CreateActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null, bool isAugment = false, bool isSong = false, bool loadingItemBonus = false, bool isDoom = false)
	{
		CActiveBonus cActiveBonus = null;
		if (ability.ActiveBonusData.OverrideAsSong)
		{
			isSong = true;
		}
		CVarsContext currentVarsContext = CAbilityFilter.CurrentVarsContext;
		if (currentVarsContext == null)
		{
			CAbilityFilter.CurrentVarsContext = ability?.VarsContext;
		}
		if (ability.ActiveBonusData.HasGenericBehaviour())
		{
			cActiveBonus = new CGenericActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		}
		else if (ability.AbilityType == CAbility.EAbilityType.NullTargeting)
		{
			EActiveBonusBehaviourType behaviour = ability.ActiveBonusData.Behaviour;
			cActiveBonus = ((behaviour != EActiveBonusBehaviourType.OverrideAbility && behaviour != EActiveBonusBehaviourType.OverrideMoveAbility) ? GetBonus(baseCard, ability, actor, caster, iD, remaining, activeBonusStartRound) : new COverrideAbilityTypeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining));
		}
		else if (ability.AbilityType == CAbility.EAbilityType.AddActiveBonus || ability.AbilityType == CAbility.EAbilityType.PlaySong)
		{
			switch (ability.ActiveBonusData.Behaviour)
			{
			case EActiveBonusBehaviourType.StartTurnAbility:
			case EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns:
				cActiveBonus = new CStartTurnAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.EndTurnAbility:
				cActiveBonus = new CEndTurnAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.StartRoundAbility:
			case EActiveBonusBehaviourType.StartRoundIfExhausted:
				cActiveBonus = new CStartRoundAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.PreCalcInitiativeAbility:
				cActiveBonus = new CStartRoundAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.EndRoundAbility:
				cActiveBonus = new CEndRoundAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.StartActionAbility:
				cActiveBonus = new CStartActionAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.EndActionAbility:
				cActiveBonus = new CEndActionAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.DuringActionAbility:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttacked:
			case EActiveBonusBehaviourType.DuringActionAbilityOnKill:
			case EActiveBonusBehaviourType.DuringActionAbilityOnMoved:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackAbilityFinished:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackStart:
			case EActiveBonusBehaviourType.DuringActionAbilityOnLongRest:
			case EActiveBonusBehaviourType.DuringActionAbilityOnShortRest:
			case EActiveBonusBehaviourType.DuringActionAbilityOnDeath:
			case EActiveBonusBehaviourType.DuringActionAbilityAfterDeath:
			case EActiveBonusBehaviourType.DuringActionAbilityOnDamaged:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttacking:
			case EActiveBonusBehaviourType.DuringActionAbilityOnMovedIntoCasterHex:
			case EActiveBonusBehaviourType.DuringActionAbilityOnPreventedDamage:
			case EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAbilityEnded:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackFinished:
			case EActiveBonusBehaviourType.DuringActionAbilityOnHealed:
			case EActiveBonusBehaviourType.DuringActionAbilityOnHealedToFull:
			case EActiveBonusBehaviourType.DuringActionAbilityOnCreated:
			case EActiveBonusBehaviourType.DuringActionAbilityOnMovedORCarried:
			case EActiveBonusBehaviourType.DuringActionAbilityOnActionEnded:
			case EActiveBonusBehaviourType.DuringActionAbilityOnActionEndedAndLongRest:
			case EActiveBonusBehaviourType.DuringActionAbilityOnConditionEnded:
				cActiveBonus = new CDuringActionAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.DuringTurnAbility:
			case EActiveBonusBehaviourType.BetweenActionAbilities:
				cActiveBonus = new CDuringTurnAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.OverrideAbility:
			case EActiveBonusBehaviourType.OverrideMoveAbility:
				cActiveBonus = new COverrideAbilityTypeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.ApplyMainBeforeAttackResolve:
				cActiveBonus = new CApplyMainBeforeAttackResolveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			default:
				cActiveBonus = GetBonus(baseCard, ability, actor, caster, iD, remaining, activeBonusStartRound);
				break;
			}
			if (cActiveBonus == null)
			{
				cActiveBonus = GetBonus(baseCard, ability, actor, caster, iD, remaining, activeBonusStartRound);
			}
		}
		else
		{
			cActiveBonus = GetBonus(baseCard, ability, actor, caster, iD, remaining, activeBonusStartRound);
		}
		if (ability.ActiveBonusData.SetFilterToCaster)
		{
			CAbilityFilter cAbilityFilter = ability.ActiveBonusData.Filter.AbilityFilters[0];
			if (cAbilityFilter != null)
			{
				if (cAbilityFilter.FilterPlayerClasses == null)
				{
					cAbilityFilter.FilterPlayerClasses = new List<string>();
				}
				cAbilityFilter.FilterPlayerClasses.Add(caster.Class.ID);
			}
		}
		if (cActiveBonus.Duration != EActiveBonusDurationType.NA && baseCard.CardType != CBaseCard.ECardType.ScenarioModifier && ((baseCard.IsItemOrPet && !loadingItemBonus) || baseCard.CardType == CBaseCard.ECardType.AttackModifier || baseCard.CardType == CBaseCard.ECardType.Dummy || baseCard.CardType == CBaseCard.ECardType.CharacterAbility || (cActiveBonus.IsAura && caster is CEnemyActor)))
		{
			CCharacterClass cCharacterClass = caster.Class as CCharacterClass;
			CAbilityCard abilityCard = baseCard as CAbilityCard;
			if (abilityCard != null && caster.Class.ID != abilityCard.ClassID && abilityCard.CardType == CBaseCard.ECardType.CharacterAbility)
			{
				CCharacterClass cCharacterClass2 = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass x) => x.ID == abilityCard.ClassID);
				if (cCharacterClass2 != null)
				{
					cCharacterClass = cCharacterClass2;
				}
			}
			if (cCharacterClass != null)
			{
				if (!baseCard.IsItemOrPet || baseCard.ActiveBonuses.Count <= 0)
				{
					cCharacterClass.ActivateCard(baseCard);
				}
			}
			else if (caster.Class is CHeroSummonClass)
			{
				((caster as CHeroSummonActor).Summoner?.Class as CCharacterClass)?.ActivateCard(baseCard);
			}
			else if (caster.Class is CMonsterClass cMonsterClass && !cMonsterClass.ActivatedCards.Contains(baseCard))
			{
				cMonsterClass.ActivatedCards.Add(baseCard);
			}
		}
		if (isAugment)
		{
			cActiveBonus.IsAugment = isAugment;
			cActiveBonus.Duration = EActiveBonusDurationType.Persistent;
		}
		if (isSong)
		{
			cActiveBonus.IsSong = isSong;
			cActiveBonus.Duration = EActiveBonusDurationType.Persistent;
		}
		if (isDoom)
		{
			cActiveBonus.IsDoom = isDoom;
			cActiveBonus.Duration = EActiveBonusDurationType.Persistent;
		}
		if (cActiveBonus.IsAura)
		{
			cActiveBonus.FindValidActorsInRangeOfAura();
		}
		if (cActiveBonus.HasTracker && !ScenarioManager.IsLoading && cActiveBonus.Ability.ActiveBonusData.GetMiscVar("StartAtTrackerIndex").GetInt(out var val))
		{
			cActiveBonus.TrackerIndex = val;
		}
		cActiveBonus.RefreshPersistentVFX();
		if (ability.ActiveBonusData.CancelActiveBonusesOnBaseCardIDs != null)
		{
			if (caster.Class is CCharacterClass cCharacterClass3)
			{
				foreach (CBaseCard activatedCard in cCharacterClass3.ActivatedCards)
				{
					if (!ability.ActiveBonusData.CancelActiveBonusesOnBaseCardIDs.Contains(activatedCard.ID))
					{
						continue;
					}
					foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
					{
						activeBonuse.Finish();
					}
				}
			}
			else
			{
				Log.ErrorWithReport("ActiveBonusData.CancelActiveBonusesOnBaseCardIDs not currently supported for non player actors");
			}
		}
		CAbilityFilter.CurrentVarsContext = currentVarsContext;
		return cActiveBonus;
	}

	public int BespokeBehaviourStrength()
	{
		if (m_BespokeBehaviour != null)
		{
			return m_BespokeBehaviour.Strength;
		}
		return 0;
	}

	public int ReferenceStrength(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour != null)
			{
				return m_BespokeBehaviour.ReferenceStrength(ability, target);
			}
			if (m_Ability != null)
			{
				if (target is CPlayerActor { Agility: not CPlayerActor.EAgility.None } cPlayerActor)
				{
					if ((m_Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.WhenFast) != 0 && cPlayerActor.Agility != CPlayerActor.EAgility.Fast)
					{
						return 0;
					}
					if ((m_Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.WhenSlow) != 0 && cPlayerActor.Agility != CPlayerActor.EAgility.Slow)
					{
						return 0;
					}
				}
				if (ability == null)
				{
					return m_Ability.Strength;
				}
				AbilityData.EActiveBonusFlag eActiveBonusFlag = (((m_Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.ShieldAll) != 0) ? m_Ability.ActiveBonusData.Flags : AbilityData.EActiveBonusFlag.ShieldAll);
				if (ability.Range > 1 && (eActiveBonusFlag & AbilityData.EActiveBonusFlag.ShieldRanged) != 0)
				{
					return m_Ability.Strength;
				}
				if (ability.Range <= 1 && (eActiveBonusFlag & AbilityData.EActiveBonusFlag.ShieldMelee) != 0)
				{
					return m_Ability.Strength;
				}
			}
		}
		return 0;
	}

	public int ReferenceXP(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				if (m_Ability == null)
				{
					return 0;
				}
				return m_Ability.ActiveBonusData.ProcXP;
			}
			return m_BespokeBehaviour.ReferenceXP(ability, target);
		}
		return 0;
	}

	public int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return 1;
			}
			return m_BespokeBehaviour.ReferenceStrengthScalar(ability, target);
		}
		return 1;
	}

	public int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return 1;
			}
			return m_BespokeBehaviour.ReferenceAbilityStrengthScalar(ability, target);
		}
		return 1;
	}

	public string ReferenceAnimOverload(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return string.Empty;
			}
			return m_BespokeBehaviour.ReferenceAnimOverload(ability, target);
		}
		return string.Empty;
	}

	public AbilityData.EActiveBonusFlag ReferenceFlags(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return AbilityData.EActiveBonusFlag.None;
			}
			return m_BespokeBehaviour.ReferenceFlags(ability, target);
		}
		return AbilityData.EActiveBonusFlag.None;
	}

	public bool ActiveBonusIsActivatedByTile(CTile tile)
	{
		if (IsActiveBonusToggledAndNotRestricted(Actor))
		{
			if (m_BespokeBehaviour == null)
			{
				return false;
			}
			return m_BespokeBehaviour.ActiveBonusIsActivatedByTile(tile);
		}
		return false;
	}

	public bool StackActiveBonusInlineAbility(CActor target, CAbility ability)
	{
		bool flag = false;
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			flag |= bespokeBehaviour.StackActiveBonusInlineAbility(target, ability);
		}
		return flag;
	}

	public void UpdateActiveBonusInlineAbilityTarget(CActor target)
	{
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.UpdateActiveBonusInlineAbilityTarget(target);
		}
	}

	public void ToggleActiveBonusFromVars(CActor actorToggling, bool markCostAbilityAsExecuted)
	{
		if (markCostAbilityAsExecuted)
		{
			CostAbilityExecuted = true;
		}
		ToggleLocked = false;
		ToggleActiveBonus(null, actorToggling);
	}

	public void ToggleActiveBonus(ElementInfusionBoardManager.EElement? toggleElement, CActor actorToggling, object extraOption = null)
	{
		if (!m_Ability.ActiveBonusData.IsToggleBonus || ToggleLocked)
		{
			return;
		}
		if (m_Ability.ActiveBonusData.Consuming.Count > 0)
		{
			if (!ToggledBonus && toggleElement.HasValue && m_Ability.ActiveBonusData.Consuming.All((ElementInfusionBoardManager.EElement x) => x == toggleElement.Value || x == ElementInfusionBoardManager.EElement.Any))
			{
				ToggledElement = toggleElement.Value;
			}
			else
			{
				ToggledElement = null;
			}
		}
		if (m_Ability.ActiveBonusData.RequiredResources.Count > 0 && m_Ability.ActiveBonusData.ConsumeResources)
		{
			if (!ToggledBonus)
			{
				foreach (KeyValuePair<string, int> requiredResource in m_Ability.ActiveBonusData.RequiredResources)
				{
					ToggledResourcesToConsume.Add(new CCharacterResource(requiredResource.Key, requiredResource.Value));
				}
			}
			else
			{
				ToggledResourcesToConsume.Clear();
			}
		}
		ToggledBonus = !ToggledBonus;
		ToggleLocked = true;
		try
		{
			CBespokeBehaviour behaviour2;
			if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction)
			{
				CAbility ability = cPhaseAction.CurrentPhaseAbility.m_Ability;
				if (m_Ability.ActiveBonusData.IsSingleTargetBonus)
				{
					ability.ToggleSingleTargetActiveBonus(this);
				}
				else
				{
					if (m_Ability.Song != null)
					{
						foreach (CSong.SongEffect songEffect in m_Ability.Song.SongEffects)
						{
							songEffect.OnSongEffectToggled(ability, ToggledBonus);
						}
					}
					ability.ActiveBonusToggled(actorToggling, this);
					foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
					{
						bespokeBehaviour.OnActiveBonusToggled(ability, bespokeBehaviour.ActiveBonus, ToggledBonus);
					}
				}
				if (!CostAbilityExecuted && m_Ability.ActiveBonusData.CostAbility != null && m_Ability.ActiveBonusData.CostAbility.MeetsStartAbilityRequirements(actorToggling))
				{
					m_Ability.ActiveBonusData.CostAbility.CachedAbilityBaseCard = m_Ability.CachedAbilityBaseCard;
					cPhaseAction.HandleActiveBonusCostAbilityToggling(this, m_Ability.ActiveBonusData.CostAbility, ToggledBonus);
				}
			}
			else if (PhaseManager.CurrentPhase is CPhaseActionSelection || PhaseManager.CurrentPhase is CPhaseInfuserMechanicSelection)
			{
				if (TryGetBehaviour(out var behaviour, EActiveBonusBehaviourType.DuringTurnAbility))
				{
					behaviour.OnActiveBonusToggled((CAbility)null, behaviour.ActiveBonus, ToggledBonus);
				}
			}
			else if (TryGetBehaviour(out behaviour2, EActiveBonusBehaviourType.StartActionAbility, EActiveBonusBehaviourType.EndActionAbility, EActiveBonusBehaviourType.BetweenActionAbilities))
			{
				behaviour2?.OnActiveBonusToggled(actorToggling, behaviour2.ActiveBonus, ToggledBonus);
			}
			else if (m_Ability.AbilityType == CAbility.EAbilityType.AdjustInitiative && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.CheckForInitiativeAdjustments && m_BespokeBehaviour != null)
			{
				CAdjustInitiativeActiveBonus_FocusInitiative behaviour4;
				CAdjustInitiativeActiveBonus_SetInitiative behaviour5;
				if (TryGetBehaviour(out CAdjustInitiativeActiveBonus_AdjustInitiative behaviour3, EActiveBonusBehaviourType.AdjustInitiative))
				{
					if (ToggledBonus && extraOption != null)
					{
						behaviour3.OnToggleInitiativeBonus((bool)extraOption);
					}
					else
					{
						behaviour3.OnUntoggleInitiativeBonus();
					}
				}
				else if (TryGetBehaviour(out behaviour4, EActiveBonusBehaviourType.FocusInitiative))
				{
					behaviour4.OnToggleInitiativeBonus(ToggledBonus);
				}
				else if (TryGetBehaviour(out behaviour5, EActiveBonusBehaviourType.SetInitiative))
				{
					if (ToggledBonus)
					{
						behaviour5.OnToggleInitiativeBonus();
					}
					else
					{
						behaviour5.OnUntoggleInitiativeBonus();
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport($"Error while toggling active bonus {this}" + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
		ToggleLocked = false;
	}

	public void ResetRestriction(EActiveBonusRestrictionType restrictionType)
	{
		CostAbilityExecuted = false;
		if (m_Ability.ActiveBonusData.Restriction != restrictionType)
		{
			return;
		}
		m_RestrictedActors.Clear();
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.OnRestrictionReset();
		}
	}

	public virtual void OnTurnEnded(CActor cur_turn_actor)
	{
	}

	public void OnActionEnded(CActor actorEndingAction)
	{
		if (GameState.CurrentActionInitiator != GameState.EActionInitiator.AbilityCard)
		{
			return;
		}
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.OnActionEnded(actorEndingAction);
			bespokeBehaviour.AppliedThisAction = false;
		}
	}

	public void RestrictActiveBonus(CActor restrictForActor, bool manualRestrict = false, bool checkOthers = true)
	{
		if (!manualRestrict && (m_Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.ManualRestrict) != 0)
		{
			return;
		}
		if (m_Ability.ActiveBonusData.Restriction != EActiveBonusRestrictionType.None && !m_RestrictedActors.Contains(restrictForActor))
		{
			m_RestrictedActors.Add(restrictForActor);
		}
		ActiveBonusUsed(restrictForActor);
		if (!checkOthers || !m_Ability.ActiveBonusData.GetMiscVar("RestrictAllBonusesOnCard").GetBool())
		{
			return;
		}
		CBaseCard abilityBaseCard = m_Ability.GetAbilityBaseCard();
		if (abilityBaseCard == null)
		{
			return;
		}
		foreach (CActiveBonus item in abilityBaseCard.ActiveBonuses.Where((CActiveBonus x) => x != this))
		{
			item.RestrictActiveBonus(restrictForActor, manualRestrict: false, checkOthers: false);
		}
	}

	public void ActiveBonusUsed(CActor actor)
	{
		CostAbilityExecuted = false;
		if (!BaseCard.IsItemOrPet || m_HasTracker || !(Actor.Class is CCharacterClass))
		{
			return;
		}
		CItem cItem = Actor.Inventory.AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == BaseCard.ID);
		if (cItem != null && cItem.YMLData.Usage == CItem.EUsageType.Unrestricted && cItem.YMLData.Trigger == CItem.EItemTrigger.PassiveEffect && cItem.YMLData.UsedWhenEquipped != true)
		{
			if (m_TrackerIndex < 1)
			{
				m_TrackerIndex++;
			}
			Actor.UsedItem(cItem, m_TrackerIndex < 1);
		}
	}

	public bool IsRestricted(CActor checkRestrictActor)
	{
		if (m_Ability.ActiveBonusData.Restriction != EActiveBonusRestrictionType.None)
		{
			if (m_Ability.ActiveBonusData.RestrictPerActor)
			{
				return m_RestrictedActors.Contains(checkRestrictActor);
			}
			return m_RestrictedActors.Count > 0;
		}
		return false;
	}

	public bool IsValidSingleTarget(CActor target)
	{
		bool flag = false;
		List<(string, TValue)> vars = m_Ability.ActiveBonusData.Vars;
		if (vars != null)
		{
			foreach (var item in vars)
			{
				if (!(item.Item1 != "IsValidTarget"))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return true;
		}
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			if (CreateTriggerContext(bespokeBehaviour, null, m_Ability, null, target, checkValidateVars: false, null).GetVar("IsValidTarget").GetBool())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasValidSingleTarget(List<CActor> targets)
	{
		if (targets == null)
		{
			return false;
		}
		foreach (CActor target in targets)
		{
			if (IsValidSingleTarget(target))
			{
				return true;
			}
		}
		return false;
	}

	public bool DisabledDueToImpair()
	{
		if (m_CheckingImpairReq)
		{
			return false;
		}
		m_CheckingImpairReq = true;
		bool result = BaseCard is CItem && Caster != null && Caster.Tokens.HasKey(CCondition.ENegativeCondition.Impair) && !Ability.GetMiscVar("IgnoreImpair").GetBool();
		m_CheckingImpairReq = false;
		return result;
	}

	public bool RequirementsMet(CActor actor = null, CAbility ability = null, CVarsContext context = null)
	{
		if (DisabledDueToImpair())
		{
			return false;
		}
		if (m_Ability.ActiveBonusData.Requirements != null)
		{
			CAbility ability2 = m_Ability;
			if (m_Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				ability2 = cAbilityAddActiveBonus.AddAbility;
			}
			if (ability != null)
			{
				ability2 = ability;
			}
			if (!CostAbilityExecuted)
			{
				if (m_Ability.ActiveBonusData.Requirements.MeetsAbilityRequirements(actor, ability2, context, null, this))
				{
					return m_Ability.ActiveBonusData.Requirements.MeetsActiveBonusRequirements(this);
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public void PayRequirementCost()
	{
		if (m_Ability.ActiveBonusData.Requirements != null)
		{
			m_Ability.ActiveBonusData.Requirements.PayRequirementCost(m_Ability.TargetingActor.SelectedRoundCardsCount);
		}
	}

	public bool IsActiveBonusToggledAndNotRestricted(CActor checkActor)
	{
		if (m_Ability.ActiveBonusData.Consuming.Count > 0)
		{
			if (!m_Ability.ActiveBonusData.IsToggleBonus)
			{
				return m_Ability.ActiveBonusData.Consuming.All((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.GetAvailableElements(checkActor).Any((ElementInfusionBoardManager.EElement y) => x == y));
			}
			if (ToggledBonus)
			{
				return !IsRestricted(checkActor);
			}
			return false;
		}
		if (m_Ability.ActiveBonusData.IsToggleBonus)
		{
			if (ToggledBonus)
			{
				return !IsRestricted(checkActor);
			}
			return false;
		}
		return !IsRestricted(checkActor);
	}

	public bool AppliesTo(CActor actor)
	{
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			if (bespokeBehaviour.AppliesTo(actor))
			{
				return true;
			}
		}
		return false;
	}

	public void ActiveBonusConsumeElements()
	{
		if (m_Ability.ActiveBonusData.Consuming.Count <= 0)
		{
			return;
		}
		if (m_Ability.ActiveBonusData.IsToggleBonus)
		{
			if (ToggledBonus)
			{
				CActiveBonusConsumedElementsCombatLog_MessageData message = new CActiveBonusConsumedElementsCombatLog_MessageData(Actor)
				{
					m_ActiveBonus = this,
					m_ElementsConsumed = new List<ElementInfusionBoardManager.EElement> { ToggledElement.Value }
				};
				ScenarioRuleClient.MessageHandler(message);
				ElementInfusionBoardManager.Consume(ToggledElement.Value, Actor);
			}
			return;
		}
		CActiveBonusConsumedElementsCombatLog_MessageData message2 = new CActiveBonusConsumedElementsCombatLog_MessageData(Actor)
		{
			m_ActiveBonus = this,
			m_ElementsConsumed = m_Ability.ActiveBonusData.Consuming
		};
		ScenarioRuleClient.MessageHandler(message2);
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, Actor);
		}
	}

	public bool CanToggleActiveBonus(CActor actor)
	{
		if (m_Ability.ActiveBonusData.IsToggleBonus && !IsRestricted(actor))
		{
			if (m_Ability.ActiveBonusData.Consuming.Count > 0 && !m_Ability.ActiveBonusData.Consuming.All((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.GetAvailableElements(actor).Any((ElementInfusionBoardManager.EElement y) => x == y)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual void TriggerPreventDamage(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
	}

	public virtual bool TriggerTransferDamage(int damageRedirected, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		return false;
	}

	public CAbility.EAbilityType Type()
	{
		if (m_Ability == null)
		{
			return CAbility.EAbilityType.None;
		}
		return m_Ability.AbilityType;
	}

	protected CActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.DefaultToggleBehaviour:
				AddBespokeBehaviour(new CActiveBonus_DefaultToggleBehaviour(actor, ability, this, behaviour));
				break;
			case EActiveBonusBehaviourType.TriggerOnFinished:
				AddBespokeBehaviour(new CActiveBonus_TriggerOnFinished(actor, ability, this, behaviour));
				break;
			}
		}
		if (!iD.HasValue)
		{
			if (ProtectIDCounter)
			{
				Log.Error("Incrementing protected IDCounter");
			}
			ID = ++IDCounter;
		}
		else
		{
			ID = iD.Value;
			if (ID > IDCounter)
			{
				if (RestoreIDCounter)
				{
					if (ProtectIDCounter)
					{
						Log.Error("Restoring protected IDCounter");
					}
					IDCounter = ID;
				}
				else
				{
					Log.Error($"Loading active bonus with ID {ID} > {IDCounter}");
				}
			}
		}
		m_BaseCard = baseCard;
		m_Ability = ability;
		Actor = actor;
		Caster = caster;
		m_TrackerIndex = 0;
		ApplyToAction = false;
		m_ValidActorsInRangeOfAura = new List<CActor>();
		ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
		int hasTracker;
		if (activeBonusYML == null || !(activeBonusYML.TrackerPattern?.Count > 0))
		{
			AbilityData.ActiveBonusData activeBonusData = m_Ability.ActiveBonusData;
			hasTracker = ((activeBonusData != null && activeBonusData.Tracker?.Count > 0) ? 1 : 0);
		}
		else
		{
			hasTracker = 1;
		}
		m_HasTracker = (byte)hasTracker != 0;
		Duration = m_Ability.ActiveBonusData.Duration;
		m_ActiveBonusStartRound = activeBonusStartRound;
		if (CreateToggledActiveBonusHack)
		{
			ToggledBonus = true;
		}
		if (!m_HasTracker && remaining.HasValue)
		{
			m_TrackerIndex = -remaining.Value;
		}
		if (m_HasTracker)
		{
			if (remaining.HasValue)
			{
				ActiveBonusLayout activeBonusYML2 = m_Ability.ActiveBonusYML;
				if (activeBonusYML2 != null && activeBonusYML2.TrackerPattern?.Count > 0)
				{
					m_TrackerIndex = m_Ability.ActiveBonusYML.TrackerPattern.Count - remaining.Value;
				}
				else
				{
					m_TrackerIndex = m_Ability.ActiveBonusData.Tracker.Count - remaining.Value;
				}
			}
			UpdateXPTracker(processTracker: false);
			FinishIfNoRemaining();
		}
		else if (m_Ability.Augment != null)
		{
			CActiveBonusAugmentAdded_MessageData message = new CActiveBonusAugmentAdded_MessageData(Actor)
			{
				m_Actor = Actor,
				m_ActiveBonus = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void StackXPToAbilities(List<CAbility> abilityList)
	{
		ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
		if (activeBonusYML != null && activeBonusYML.TrackerPattern?.Count > 0)
		{
			ActiveBonusLayout activeBonusYML2 = m_Ability.ActiveBonusYML;
			if (activeBonusYML2 != null && activeBonusYML2.TrackerPattern?[TrackerIndex] > 0)
			{
				CAbility cAbility = CAbilityXP.CreateDefaultXP(m_Ability.ActiveBonusYML.TrackerPattern[TrackerIndex], 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbility.MiscAbilityData.AutotriggerAbility = true;
				abilityList.Add(cAbility);
			}
		}
	}

	public void UpdateXPTracker(bool processTracker = true, bool processXP = true)
	{
		if ((m_Ability.ActiveBonusYML == null || m_Ability.ActiveBonusYML.TrackerPattern == null || m_TrackerIndex >= m_Ability.ActiveBonusYML.TrackerPattern.Count) && (m_Ability.ActiveBonusData.Tracker == null || m_TrackerIndex >= m_Ability.ActiveBonusData.Tracker.Count))
		{
			return;
		}
		if (processTracker)
		{
			if (processXP)
			{
				int xpAmount = 0;
				if (TrackerIndex >= 0)
				{
					if (m_Ability.ActiveBonusData.Tracker != null && m_TrackerIndex < m_Ability.ActiveBonusData.Tracker.Count)
					{
						xpAmount = m_Ability.ActiveBonusData.Tracker[m_TrackerIndex];
					}
					else if (m_Ability.ActiveBonusYML != null && m_Ability.ActiveBonusYML.TrackerPattern != null && m_TrackerIndex < m_Ability.ActiveBonusYML.TrackerPattern.Count)
					{
						xpAmount = m_Ability.ActiveBonusYML.TrackerPattern[m_TrackerIndex];
					}
				}
				Actor.GainXP(xpAmount, m_Ability.XPSourceKey);
			}
			m_TrackerIndex++;
			if (BaseCard.IsItemOrPet && Actor.Class is CCharacterClass)
			{
				CItem item = Actor.Inventory.AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == BaseCard.ID);
				Actor.UsedItem(item, m_TrackerIndex < 2);
			}
			if (Actor is CPlayerActor player)
			{
				CPlayerActor.LogActiveBonusTrackers(player);
			}
			TValue miscVar = Ability.ActiveBonusData.GetMiscVar("OnTrackerIndexChanged");
			if (!miscVar.isUnknown)
			{
				CVars cVars = new CVars();
				cVars.Set("Actor", new TValue(Actor));
				cVars.Set("ActiveBonus", new TValue(this));
				CVarsContext context = new CVarsContext(cVars);
				miscVar.Calc(context);
			}
		}
		CActiveBonusTrackerIncremented_MessageData message = new CActiveBonusTrackerIncremented_MessageData(Actor)
		{
			m_Actor = Actor,
			m_ActiveBonus = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public virtual void FinishIfNoRemaining()
	{
		if (Remaining <= 0 && (Ability.ActiveBonusData?.Requirements == null || !Ability.ActiveBonusData.OnlyWorkAfterFinishedTracker) && !Ability.ActiveBonusData.GetMiscVar("DontFinishAtZeroCharges").GetBool())
		{
			Finish();
		}
	}

	private void RefreshRestrictions()
	{
		if (!Ability.ActiveBonusData.GetMiscVar("OverrideTideRestrictions").GetBool())
		{
			return;
		}
		foreach (CActiveBonus item in Actor.ActiveBonuses.Where((CActiveBonus x) => x.Ability.ActiveBonusData.GetMiscVar("RestrictionAffectedByOtherTides").GetBool() && x.Ability.ActiveBonusData.GetMiscVar("NoRestriction").GetBool()))
		{
			item.Ability.ActiveBonusData.RemoveMiscVar("NoRestriction");
		}
	}

	public void RefreshPersistentVFX()
	{
		(string, TValue)? tuple = Ability?.ActiveBonusData?.Vars?.Find(((string, TValue) v) => v.Item1 == "PersistentCasterVFX");
		if (!tuple.HasValue || !tuple.Value.Item2.isString)
		{
			return;
		}
		bool flag = RequirementsMet(Actor) && !Finishing();
		if (!m_RequirementsWereMet.HasValue || flag != m_RequirementsWereMet.Value)
		{
			m_RequirementsWereMet = flag;
			ScenarioRuleClient.MessageHandler(new CRemoveCasterVFX_MessageData(Actor)
			{
				m_VFXName = tuple.Value.Item2.GetString()
			});
			if (flag)
			{
				ScenarioRuleClient.MessageHandler(new CAddCasterVFX_MessageData(Actor)
				{
					m_VFXName = tuple.Value.Item2.GetString(),
					m_delay = 0.01f
				});
			}
		}
	}

	public virtual void Finish()
	{
		try
		{
			if (m_Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.FrozenFistMechanic))
			{
				Actor.Tokens.RemovePositiveToken(CCondition.EPositiveCondition.Regenerate, EConditionDecTrigger.WhenFrozenFistMechanicActiveBonusEnds);
				Actor.ApplyCondition(Actor, CCondition.EPositiveCondition.Regenerate, 0, EConditionDecTrigger.WhenDamaged);
			}
			RefreshRestrictions();
			List<CActiveBonus> activeBonuses = Actor.ActiveBonuses;
			m_Finishing = true;
			if (m_Ability.ActiveBonusData.MiscVars != null)
			{
				CVars cVars = new CVars();
				cVars.Set("ActiveBonus", new TValue(this));
				CVarsContext context = new CVarsContext(cVars, m_Ability.VarsContext);
				foreach (var miscVar in m_Ability.ActiveBonusData.MiscVars)
				{
					if (miscVar.Item1.StartsWith("OnFinish"))
					{
						var (key, tValue) = miscVar;
						cVars.Set(key, tValue.Calc(context));
					}
				}
			}
			foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
			{
				bespokeBehaviour.OnFinished();
				bespokeBehaviour.RemoveListeners();
			}
			m_ValidActorsInRangeOfAura.Clear();
			if (IsAura)
			{
				CFinishAura_MessageData message = new CFinishAura_MessageData(Actor)
				{
					m_AuraAbilityID = BaseCard.ID
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (m_Ability.ActiveBonusData.EndAllActiveBonusesOnBaseCardSimultaneously)
			{
				foreach (CActiveBonus item in BaseCard.ActiveBonuses.ToList())
				{
					if (item != this && !item.Finished() && !item.Finishing())
					{
						item.Finish();
					}
				}
			}
			if (m_Ability.ActiveBonusData.RemoveAllInstancesOfResourcesOnFinish != null)
			{
				foreach (string resourceID in m_Ability.ActiveBonusData.RemoveAllInstancesOfResourcesOnFinish)
				{
					foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
					{
						CCharacterResource cCharacterResource = allActor.CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
						if (cCharacterResource != null)
						{
							allActor.RemoveCharacterResource(resourceID, cCharacterResource.Amount);
						}
					}
					foreach (CObjectProp resourceProp in ScenarioManager.CurrentScenarioState.ResourceProps)
					{
						if (resourceProp is CObjectResource cObjectResource && cObjectResource.ResourceID == resourceID)
						{
							resourceProp.Activate(null);
							ScenarioManager.Tiles[cObjectResource.ArrayIndex.X, cObjectResource.ArrayIndex.Y].DelProp(resourceProp);
						}
					}
				}
			}
			RefreshPersistentVFX();
			Actor.LogActiveBonuses(atStartOfRest: false, activeBonuses);
			switch (BaseCard.CardType)
			{
			case CBaseCard.ECardType.Item:
			case CBaseCard.ECardType.Pet:
			{
				bool flag = true;
				foreach (CActiveBonus item2 in BaseCard.ActiveBonuses.ToList())
				{
					if (!item2.Finishing() && !item2.Finished())
					{
						flag = false;
					}
				}
				if (!flag)
				{
					break;
				}
				if (Actor.Class is CCharacterClass)
				{
					CActor cActor = ((Type() == CAbility.EAbilityType.Shield) ? Caster : Actor);
					CItem cItem = cActor.Inventory.AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == BaseCard.ID);
					if (cItem != null)
					{
						cActor.Inventory.HandleUsedItem(cItem, this);
					}
				}
				else if (Actor.Class is CHeroSummonClass)
				{
					CItem cItem2 = Caster.Inventory.AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == BaseCard.ID);
					if (cItem2 != null)
					{
						Caster.Inventory.HandleUsedItem(cItem2, this);
					}
				}
				else if (Actor.Class is CMonsterClass cMonsterClass)
				{
					CItem cItem3 = Caster.Inventory.AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == BaseCard.ID);
					if (cItem3 != null)
					{
						Caster.Inventory.HandleUsedItem(cItem3, this);
					}
					cMonsterClass.ActivatedCards.Remove(BaseCard);
				}
				else
				{
					Log.ErrorWithReport("Trying to deactivate an item card active bonus on an unexpected class type");
				}
				break;
			}
			}
			bool flag2 = false;
			foreach (CCharacterClass item3 in ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor x) => x.CharacterClass))
			{
				if (item3.CheckForFinishedActiveBonuses(Actor, important: true, allowFromMainThread: true))
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				BaseCard.ActiveBonuses.Remove(this);
				if (BaseCard.ActiveBonuses.Count <= 0)
				{
					foreach (CMonsterClass item4 in ScenarioManager.Scenario.AllMonsters.Select((CEnemyActor x) => x.MonsterClass))
					{
						item4.ActivatedCards.Remove(BaseCard);
					}
				}
			}
			Actor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("Exception while trying to finish Active Bonus. Ability Name: " + m_Ability.Name + " BaseCard Name: " + m_BaseCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void UndoFinish()
	{
		m_Finishing = false;
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.AddListeners();
		}
	}

	public static List<CActiveBonus> FindApplicableActiveBonuses(CActor actor, CAbility.EAbilityType type, EActiveBonusBehaviourType behaviourType = EActiveBonusBehaviourType.None, bool useOriginalType = false)
	{
		List<CActiveBonus> activeBonuses = actor.ActiveBonuses;
		for (int num = activeBonuses.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = activeBonuses[num];
			if (type != CAbility.EAbilityType.None && cActiveBonus.Ability.AbilityType != type)
			{
				activeBonuses.RemoveAt(num);
			}
			else if (behaviourType != EActiveBonusBehaviourType.None && !cActiveBonus.HasBehaviour(behaviourType))
			{
				activeBonuses.RemoveAt(num);
			}
			else if (cActiveBonus.m_Finishing)
			{
				cActiveBonus.m_Finished = true;
				activeBonuses.RemoveAt(num);
			}
			else if (behaviourType != EActiveBonusBehaviourType.None && !cActiveBonus.Ability.ActiveBonusData.HasBehaviour(behaviourType))
			{
				activeBonuses.RemoveAt(num);
			}
			else if (cActiveBonus.IsAura && !cActiveBonus.ValidActorsInRangeOfAura.Contains(actor))
			{
				activeBonuses.RemoveAt(num);
			}
		}
		return activeBonuses;
	}

	public static List<CActiveBonus> FindApplicableActiveBonuses(CActor actor, bool important = false)
	{
		List<CActiveBonus> list = ActiveBonusCalculationScope.GetBonuses(actor)?.ToList();
		if (list != null)
		{
			return list;
		}
		List<CActiveBonus> list2 = CharacterClassManager.FindAllActiveBonuses(actor, important).Distinct().ToList();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = list2[num];
			if (cActiveBonus == null)
			{
				Log.Warning($"{actor} has null bonus at {num}");
				list2.RemoveAt(num);
			}
			else if (cActiveBonus.m_Finishing)
			{
				cActiveBonus.m_Finished = true;
				list2.RemoveAt(num);
			}
		}
		ActiveBonusCalculationScope.TryCache(actor, list2);
		return list2.ToList();
	}

	public static List<CActiveBonus> FindAllApplicableSongActiveBonuses(CActor actor)
	{
		return CharacterClassManager.FindAllSongActiveBonuses(actor);
	}

	public static void RefreshAllSongActiveBonuses(CActor actor)
	{
		foreach (CActiveBonus item in CharacterClassManager.FindAllSongActiveBonuses(actor))
		{
			foreach (CBespokeBehaviour bespokeBehaviour in item.BespokeBehaviours)
			{
				bespokeBehaviour.RefreshListeners(actor);
			}
		}
	}

	public static void RefreshAllAuraActiveBonuses(CAbility abilityWithContext = null)
	{
		List<CActiveBonus> list = (from a in CharacterClassManager.FindAllActiveBonuses()
			where a.IsAura
			select a).Distinct().ToList();
		CVarsContext currentVarsContext = CAbilityFilter.CurrentVarsContext;
		if (currentVarsContext == null)
		{
			CAbilityFilter.CurrentVarsContext = abilityWithContext?.VarsContext;
		}
		lock (recalcCache)
		{
			recalcCache.Clear();
			foreach (CActiveBonus item in list)
			{
				item.PrepForRecalcValidActors(out var oldActors);
				recalcCache.Add((item, oldActors));
			}
			using (new ActiveBonusCalculationScope())
			{
				foreach (CActiveBonus item2 in list)
				{
					item2.FindValidActorsInRangeOfAura(globalRefresh: true);
				}
			}
			foreach (var item3 in recalcCache)
			{
				item3.auraActiveBonus.FinishRecalcValidActors(item3.oldActors);
			}
			recalcCache.Clear();
		}
		CAbilityFilter.CurrentVarsContext = currentVarsContext;
	}

	public static void RefreshOverhealActiveBonuses()
	{
		foreach (CActiveBonus item in from a in CharacterClassManager.FindAllActiveBonuses()
			where a.Ability.AbilityType == CAbility.EAbilityType.Overheal
			select a)
		{
			(item as COverhealActiveBonus)?.CheckForOverhealStrengthUpdate();
		}
	}

	public CVarsContext CreateTriggerContext(CBespokeBehaviour triggerBehaviour, CAbility triggeredAbility, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, bool checkValidateVars, params (string, TValue)[] additionalVars)
	{
		CVars cVars = new CVars();
		CVarsContext cVarsContext = new CVarsContext(cVars, triggeredAbility?.VarsContext?.parentContext);
		cVars.Set("obj", new TValue(this));
		cVars.Set("ActiveBonus", new TValue(this));
		cVars.Set("TriggerVars", new TValue(cVars));
		cVars.Set("TriggerBehaviour", new TValue(triggerBehaviour));
		cVars.Set("TriggeredAbility", new TValue(triggeredAbility));
		cVars.Set("TriggerAbility", new TValue(triggerAbility));
		cVars.Set("TriggerActor", new TValue(triggerActor ?? triggerAbility?.TargetingActor));
		cVars.Set("TriggerTarget", new TValue(triggerTarget));
		cVars.Set("TargetSelectionNumber", 1);
		if (additionalVars != null)
		{
			for (int i = 0; i < additionalVars.Length; i++)
			{
				var (key, value) = additionalVars[i];
				cVars.Set(key, value);
			}
		}
		if (m_Ability?.ActiveBonusData?.Vars != null)
		{
			foreach (var var in m_Ability.ActiveBonusData.Vars)
			{
				string item = var.Item1;
				TValue item2 = var.Item2;
				TValue value2 = item2.Calc(cVarsContext);
				if (checkValidateVars && item.StartsWith("Validate", StringComparison.Ordinal) && !value2.GetBool())
				{
					return null;
				}
				cVars.Set(item, value2);
			}
		}
		return cVarsContext;
	}

	public bool SetupTriggerContext(CBespokeBehaviour triggerBehaviour, CAbility triggeredAbility, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, params (string, TValue)[] additionalVars)
	{
		if (m_Ability?.ActiveBonusData?.Vars == null && triggeredAbility == null)
		{
			return true;
		}
		CVarsContext cVarsContext = CreateTriggerContext(triggerBehaviour, triggeredAbility, triggerAbility, triggerActor, triggerTarget, checkValidateVars: true, additionalVars);
		if (cVarsContext == null)
		{
			return false;
		}
		if (triggeredAbility != null)
		{
			triggeredAbility.VarsContext.parentContext = cVarsContext;
		}
		return true;
	}

	public void PrepForRecalcValidActors(out List<CActor> oldActors)
	{
		oldActors = m_ValidActorsInRangeOfAura.ToList();
		m_ValidActorsInRangeOfAura.Clear();
		foreach (CBespokeBehaviour item in BespokeBehaviours.ToList())
		{
			item.RemoveListeners();
		}
	}

	public virtual void FindValidActorsInRangeOfAura(bool globalRefresh = false)
	{
		if (m_Finishing || m_Finished)
		{
			return;
		}
		CVarsContext currentVarsContext = CAbilityFilter.CurrentVarsContext;
		if (currentVarsContext == null)
		{
			CAbilityFilter.CurrentVarsContext = Ability.VarsContext;
		}
		List<CActor> oldActors = null;
		if (!globalRefresh)
		{
			oldActors = m_ValidActorsInRangeOfAura.ToList();
			m_ValidActorsInRangeOfAura.Clear();
			foreach (CBespokeBehaviour item in BespokeBehaviours.ToList())
			{
				item.RemoveListeners();
			}
		}
		using (new ActiveBonusCalculationScope())
		{
			switch (AuraTargeting)
			{
			case CAbility.EAbilityTargeting.Range:
			{
				CActor actor = ((Ability.TargetingActor != null) ? Ability.TargetingActor : Actor);
				Point arrayIndex = actor.ArrayIndex;
				CAbilityFilterContainer filter = Ability.ActiveBonusData.AuraFilter;
				IEnumerable<CActor> collection = from a in GameState.GetActorsInRange(arrayIndex, Ability.Range)
					where RequirementsMet(actor) && filter.IsValidTarget(a, actor, Ability.IsTargetedAbility, Ability.ActiveBonusData.OriginalTargetType, Ability.MiscAbilityData?.CanTargetInvisible)
					select a;
				m_ValidActorsInRangeOfAura.AddRange(collection);
				break;
			}
			case CAbility.EAbilityTargeting.Room:
			case CAbility.EAbilityTargeting.All:
			case CAbility.EAbilityTargeting.AllConnectedRooms:
				foreach (CPlayerActor item2 in ScenarioManager.Scenario.PlayerActors.ToList())
				{
					if (!RequirementsMet(item2) || !AuraFilter.IsValidTarget(item2, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
					{
						continue;
					}
					bool flag = false;
					if (AuraTargeting == CAbility.EAbilityTargeting.All)
					{
						flag = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(item2, Actor))
					{
						flag = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
					{
						ScenarioManager.PathFinder.FindPath(item2, item2.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
						if (foundPath)
						{
							flag = true;
						}
					}
					if (flag)
					{
						m_ValidActorsInRangeOfAura.Add(item2);
					}
				}
				foreach (CHeroSummonActor item3 in ScenarioManager.Scenario.HeroSummons.ToList())
				{
					if (!RequirementsMet(item3) || !AuraFilter.IsValidTarget(item3, Actor, isTargetedAbility: false, useTargetOriginalType: true, Ability.MiscAbilityData?.CanTargetInvisible))
					{
						continue;
					}
					bool flag2 = false;
					if (AuraTargeting == CAbility.EAbilityTargeting.All)
					{
						flag2 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(item3, Actor))
					{
						flag2 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
					{
						ScenarioManager.PathFinder.FindPath(item3, item3.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath2, ignoreDoorLocks: false);
						if (foundPath2)
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						m_ValidActorsInRangeOfAura.Add(item3);
					}
				}
				foreach (CEnemyActor item4 in ScenarioManager.Scenario.AllAliveMonsters.ToList())
				{
					if (!RequirementsMet(item4) || !AuraFilter.IsValidTarget(item4, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
					{
						continue;
					}
					bool flag3 = false;
					if (AuraTargeting == CAbility.EAbilityTargeting.All)
					{
						flag3 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(item4, Actor))
					{
						flag3 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
					{
						ScenarioManager.PathFinder.FindPath(item4, item4.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath3, ignoreDoorLocks: false);
						if (foundPath3)
						{
							flag3 = true;
						}
					}
					if (flag3)
					{
						m_ValidActorsInRangeOfAura.Add(item4);
					}
				}
				foreach (CObjectActor item5 in ScenarioManager.Scenario.Objects.ToList())
				{
					if (!RequirementsMet(item5) || !AuraFilter.IsValidTarget(item5, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
					{
						continue;
					}
					bool flag4 = false;
					if (AuraTargeting == CAbility.EAbilityTargeting.All)
					{
						flag4 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(item5, Actor))
					{
						flag4 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
					{
						ScenarioManager.PathFinder.FindPath(item5, item5.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath4, ignoreDoorLocks: false);
						if (foundPath4)
						{
							flag4 = true;
						}
					}
					if (flag4)
					{
						m_ValidActorsInRangeOfAura.Add(item5);
					}
				}
				break;
			}
		}
		if (!globalRefresh)
		{
			FinishRecalcValidActors(oldActors);
		}
		CAbilityFilter.CurrentVarsContext = currentVarsContext;
	}

	public virtual void FinishRecalcValidActors(List<CActor> oldActors)
	{
		using (new ActiveBonusCalculationScope())
		{
			if (m_ValidActorsInRangeOfAura == null)
			{
				return;
			}
			m_ValidActorsInRangeOfAura.Sort(GameState.CompareActorsByGUID);
			IEnumerable<CActor> second = m_ValidActorsInRangeOfAura.Except(oldActors);
			IEnumerable<CActor> first = oldActors.Except(m_ValidActorsInRangeOfAura);
			foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
			{
				foreach (CActor item in m_ValidActorsInRangeOfAura)
				{
					bespokeBehaviour.RefreshListeners(item, Ability.ActiveBonusData.OriginalTargetType);
				}
			}
			foreach (CActor item2 in first.Concat(second))
			{
				item2.CheckForCachedValuesAfterActiveBonusesUpdate();
				CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(item2);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public static bool AreTheSame(CActiveBonus bonus1, CActiveBonus bonus2)
	{
		if (bonus1.ID == bonus2.ID && bonus1.BaseCard.ID == bonus2.BaseCard.ID && bonus1.Ability.Name == bonus2.Ability.Name && bonus1.Actor.ActorGuid == bonus2.Actor.ActorGuid && bonus1.Caster.ActorGuid == bonus2.Caster.ActorGuid)
		{
			return bonus1.Remaining == bonus2.Remaining;
		}
		return false;
	}

	public bool CheckAddAbilityValidity(CAbility addAbility)
	{
		bool result = true;
		if (addAbility is CAbilityInfuse cAbilityInfuse && cAbilityInfuse.ElementAlreadyStrongAndInfuseIfNotStrongSet())
		{
			result = false;
		}
		return result;
	}

	public bool IsValidAttackType(CAbilityAttack attackAbility)
	{
		bool result = false;
		switch (Ability.ActiveBonusData.AttackType)
		{
		case CAbility.EAttackType.Melee:
			result = attackAbility.IsMeleeAttack;
			break;
		case CAbility.EAttackType.Ranged:
			result = !attackAbility.IsMeleeAttack;
			break;
		case CAbility.EAttackType.Default:
			result = attackAbility.IsDefaultAttack;
			break;
		case CAbility.EAttackType.Control:
			result = attackAbility.IsControlAbility;
			break;
		case CAbility.EAttackType.ControlledByCaster:
			result = attackAbility.IsControlAbility && GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().ControllingActor == Caster;
			break;
		case CAbility.EAttackType.Augment:
			result = attackAbility.Augment != null || attackAbility.AugmentsAdded;
			break;
		case CAbility.EAttackType.None:
		case CAbility.EAttackType.Attack:
			result = true;
			break;
		}
		return result;
	}

	public bool IsHiddenInUI()
	{
		TValue miscVar = Ability.ActiveBonusData.GetMiscVar("Hidden");
		if (!miscVar.isUnknown)
		{
			CVars cVars = new CVars();
			cVars.Set("ActiveBonus", new TValue(this));
			CVarsContext context = new CVarsContext(cVars);
			if (miscVar.GetBool(context))
			{
				return true;
			}
		}
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			if (bespokeBehaviour.IsHiddenInUI())
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool ShouldRestartAbilityOnToggle(CAbility currentAbility)
	{
		return true;
	}

	public void TriggerOnBonusToggledMiscVar(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		if (Ability?.ActiveBonusData?.MiscVars == null)
		{
			return;
		}
		foreach (var miscVar in Ability.ActiveBonusData.MiscVars)
		{
			if (miscVar.Item1.StartsWith("OnBonusToggled", StringComparison.Ordinal))
			{
				TValue item = miscVar.Item2;
				if (!item.isUnknown)
				{
					CVars cVars = new CVars();
					cVars.Set("CurrentAbility", new TValue(currentAbility));
					cVars.Set("ActiveBonus", new TValue(activeBonus));
					cVars.Set("IsOn", new TValue(toggledOn));
					CVarsContext context = new CVarsContext(cVars);
					item.Calc(context);
				}
			}
		}
	}

	public void Save(ActiveBonusState state)
	{
		List<Tuple<string, object>> list = new List<Tuple<string, object>>();
		if (Ability?.ActiveBonusData?.Vars != null)
		{
			CVarsContext context = new CVarsContext(this);
			foreach (var var in Ability.ActiveBonusData.Vars)
			{
				var (text, _) = var;
				if (text.StartsWith("SaveVar", StringComparison.InvariantCultureIgnoreCase))
				{
					TValue item = var.Item2;
					TValue tValue = item.Calc(context);
					if (text.StartsWith("SaveVarString"))
					{
						list.Add(new Tuple<string, object>(text, tValue.GetString()));
					}
					else if (text.StartsWith("SaveVarInt"))
					{
						list.Add(new Tuple<string, object>(text, tValue.GetInt()));
					}
				}
			}
		}
		if (BaseCard != null && BaseCard.PendingCardPile != CBaseCard.ECardPile.None)
		{
			list.Add(new Tuple<string, object>("PendingCardPile", JsonConvert.SerializeObject(BaseCard.PendingCardPile)));
		}
		if (Ability.GetMiscVar("DontCancelTideOnRest").GetBool())
		{
			list.Add(new Tuple<string, object>("DontCancelTideOnRest", JsonConvert.SerializeObject(true)));
		}
		Save(list);
		if (list.Count > 0)
		{
			state.AdditionalSerialization = JsonConvert.SerializeObject(list);
		}
	}

	public virtual void Save(List<Tuple<string, object>> toSave)
	{
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.Save(toSave);
		}
	}

	public void Load(ActiveBonusState state)
	{
		if (string.IsNullOrEmpty(state.AdditionalSerialization))
		{
			return;
		}
		try
		{
			List<Tuple<string, object>> list = JsonConvert.DeserializeObject<List<Tuple<string, object>>>(state.AdditionalSerialization);
			foreach (Tuple<string, object> item2 in list)
			{
				string item = item2.Item1;
				if (item.StartsWith("SaveVarString"))
				{
					Ability.ActiveBonusData.SetVar(item, new TValue(Convert.ToString(item2.Item2)));
				}
				else if (item.StartsWith("SaveVarInt"))
				{
					Ability.ActiveBonusData.SetVar(item, new TValue(Convert.ToInt32(item2.Item2)));
				}
				else if (item.Equals("PendingCardPile", StringComparison.InvariantCultureIgnoreCase))
				{
					if (BaseCard != null)
					{
						BaseCard.PendingCardPile = JsonConvert.DeserializeObject<CBaseCard.ECardPile>(item2.Item2 as string);
					}
				}
				else if (item.Equals("DontCancelTideOnRest", StringComparison.InvariantCultureIgnoreCase))
				{
					Ability.SetMiscVar("DontCancelTideOnRest", true);
				}
			}
			Load(list);
		}
		catch (Exception)
		{
			Load(new List<Tuple<string, object>>
			{
				new Tuple<string, object>("additionalSerialization", state.AdditionalSerialization)
			});
		}
	}

	public virtual void Load(List<Tuple<string, object>> toSave)
	{
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.Load(toSave);
		}
	}

	public void CleanupBeforeSave()
	{
		foreach (CBespokeBehaviour bespokeBehaviour in BespokeBehaviours)
		{
			bespokeBehaviour.CleanUpBeforeSave();
		}
	}

	public static List<CActiveBonus> FindAllActiveBonusesForUI(CActor actor)
	{
		return (from activeBonus in actor.FindApplicableActiveBonuses()
			where activeBonus.Ability.ActiveBonusData.GetMiscVar("ShowUnderHealthbar").GetBool()
			select activeBonus).ToList();
	}
}
