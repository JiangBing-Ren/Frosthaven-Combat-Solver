using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FH.Tools;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class CActor : IEquatable<CActor>, IGetVar, IDumpState
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EType
	{
		Player,
		Enemy,
		HeroSummon,
		Ally,
		Enemy2,
		Neutral,
		Unknown
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EOverrideLookupProperty
	{
		NA,
		Shield
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ECauseOfDeath
	{
		None,
		StillAlive,
		Undetermined,
		DebugMenu,
		Damage,
		NoMoreCards,
		KillAbility,
		SummonerDied,
		SummonActiveBonusCancelled,
		Trap,
		Suicide,
		HazardousTerrain,
		ActorRemovedFromMap
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum ECauseOfDamage
	{
		None,
		Trap,
		EnemyAttack,
		EnemyDamage,
		Wound,
		Self,
		Ally,
		HazardousTerrain,
		ScenarioElements
	}

	public delegate void MoveStartListener(CAbilityMove moveAbility);

	public delegate void AttackStartListener(CAbilityAttack attackAbility);

	public delegate void AttackSingleTargetStartListener(CAbilityAttack attackAbility, CActor target);

	public delegate void PreAttackingListener(CAbilityAttack attackAbility);

	public delegate void HealApplyToActorListener(CAbilityHeal healAbility);

	public delegate void HealApplyToActionListener(CAbilityHeal healAbility);

	public delegate void BeingHealedListener(CAbilityHeal healAbility);

	public delegate void AfterBeingHealedListener();

	public delegate void OnActorImmuneToConditionListener(CAbility conditionAbility, CActor target, bool isActorImmune);

	public delegate void ConditionApplyToActorListener(CAbility conditionAbility, CActor target);

	public delegate void MoveListener(CAbilityMove moveAbility);

	public delegate void LootListener(CAbilityLoot lootAbility);

	public delegate void AttackListener(CAbilityAttack attackAbility, CActor target);

	public delegate void BeingAttackedPreDamageListener(CAbilityAttack attackAbility);

	public delegate void BeingAttackedPostDamageListener(CAbilityAttack attackAbility, CAttackSummary.TargetSummary targetSummary);

	public delegate void BeingAttackedListener(CAbilityAttack ability, int modifiedStrength);

	public delegate void RetaliateListener();

	public delegate void AttackFinishedListener(CAbilityAttack ability, CActor target, int damageDealt);

	public delegate void AttackAbilityFinishedListener(CAbilityAttack ability);

	public delegate void TakeDamageListener(int damageTaken, CAbility damagingAbility, int DamageReducedByShields, int actualDamage, CActor damageSourceActor);

	public delegate void KillListener(CActor target, CActor actor, ECauseOfDeath causeOfDeath, CAbility causeOfDeathAbility, bool onActorTurn);

	public delegate void MovedListener(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition);

	public delegate void CarriedListener(CAbility moveAbility, CActor movedActor, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes);

	public delegate void DrawModifierListener(EAdvantageStatuses advantageStatus, ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target);

	public delegate void AbilityStartedListener(CAbility startedAbility);

	public delegate void AbilityEndedListener(CAbility endedAbility);

	public delegate void DamagedListener(GameState.DamageData damageData);

	public delegate void DeathListener(CActor actor, CAbility causeOfDeathAbility);

	public delegate void AbilityTargetingStartListener(CAbility ability);

	public delegate void PositiveConditionEndedOnActorListener(CCondition.EPositiveCondition positiveConditionType, CActor target, EConditionDecTrigger decTrigger);

	public delegate void NegativeConditionEndedOnActorListener(CCondition.ENegativeCondition negativeConditionType, CActor target, EConditionDecTrigger decTrigger);

	public delegate void PreventDamageListener(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus);

	public delegate void CreatedListener(CAbility createdAbility);

	public delegate void AbilityOverrideApplied(CActor actor, CAbilityOverride abilityOveride, CAbility ability, bool active);

	public delegate void PropDestroyed(CActor actor, CAbility ability, CObjectProp prop);

	public delegate void PropCreated(CActor actor, CAbility ability, CObjectProp prop);

	[Flags]
	public enum TargetFromAnywhereFlags
	{
		None = 0,
		Melee = 1,
		Ranged = 2,
		IfNoOtherTargets = 4,
		AnyIfNoOtherTargets = Melee | Ranged | IfNoOtherTargets
	}

	public delegate void LOSCallbackType(Vector sourcePosition, Vector targetPosition, bool rayBlocked, string primitive = "line");

	public static readonly EType[] Types = (EType[])Enum.GetValues(typeof(EType));

	public static readonly EOverrideLookupProperty[] OverrideLookupProperties = (EOverrideLookupProperty[])Enum.GetValues(typeof(EOverrideLookupProperty));

	public static readonly int MaxLoot = LootCard.AllTypes.Length + 16;

	public int MinInitialMaxHealth = -1;

	public bool OnDeathAbilityActorDeadHandled;

	[StateDump(Skip = true)]
	public bool SpawnProjectilesOnRangedAttack = true;

	public bool IsShadowMarked;

	public bool HasBaneThisTurn;

	public bool CanUnlockDoors;

	[StateDump(Skip = true)]
	public bool AIPreferMoveFocusPath;

	public bool TriggerInfusionsWithSummon;

	public string CustomLocKey = string.Empty;

	private TargetFromAnywhereFlags _canTargetFromAnywhereFlags;

	private bool _canBeTargetedFromAnywhere;

	public static bool GodMode = false;

	public Point OverrideArrayIndex;

	public List<Point> ExtraArrayIndexs;

	public AbilityTargetingStartListener m_OnAbilityTargetingStartListeners;

	public Action<CAbility> m_OnAbilityPerformStartedListeners;

	public AbilityStartedListener m_OnAbilityStartedListeners;

	public AbilityEndedListener m_OnAbilityEndedListeners;

	public AbilityEndedListener m_OnAbilityUndoneListeners;

	public AttackStartListener m_OnAttackStartListeners;

	public PreAttackingListener m_OnPreActorIsAttackingListeners;

	public AttackListener m_OnAttackingListeners;

	public BeingAttackedPreDamageListener m_OnBeingAttackedPreDamageListeners;

	public BeingAttackedPostDamageListener m_OnBeingAttackedPostDamageListeners;

	public BeingAttackedListener m_OnBeingAttackedListeners;

	public AttackSingleTargetStartListener m_OnAttackSingleTargetListeners;

	public AttackFinishedListener m_OnAttackFinishedListeners;

	public AttackAbilityFinishedListener m_OnAttackAbilityFinishedListeners;

	public HealApplyToActorListener m_OnHealApplyToActorListeners;

	public HealApplyToActionListener m_OnHealApplyToActionListeners;

	public BeingHealedListener m_OnBeingHealedListeners;

	public AfterBeingHealedListener m_OnAfterBeingHealedListeners;

	public OnActorImmuneToConditionListener m_OnActorImmuneToConditionListeners;

	public ConditionApplyToActorListener m_OnConditionApplyToActorListeners;

	public PositiveConditionEndedOnActorListener m_OnPositiveConditionEndedOnActorListeners;

	public NegativeConditionEndedOnActorListener m_OnNegativeConditionEndedOnActorListeners;

	public MoveStartListener m_OnMoveStartListeners;

	public MoveListener m_OnMovingListeners;

	public MovedListener m_OnMovedListeners;

	public CarriedListener m_OnCarriedListeners;

	public LootListener m_OnLootListeners;

	public RetaliateListener m_OnRetaliateListeners;

	public DamagedListener m_OnDamagedListeners;

	public TakeDamageListener m_OnTakenDamageListeners;

	public PreventDamageListener m_OnPreventDamageListeners;

	public KillListener m_OnKillListeners;

	public DeathListener m_OnDeathListeners;

	public DeathListener m_AfterDeathListeners;

	public DrawModifierListener m_OnDrawModifierListeners;

	public CreatedListener m_OnCreatedListeners;

	public AbilityOverrideApplied m_OnAbilityOverrideAppliedListeners;

	public PropCreated m_OnPropCreatedListeners;

	public PropDestroyed m_OnPropDestroyedListeners;

	public CClass ClassOverride;

	public bool m_PlayedThisRound;

	public bool m_PlayedAtLeastOnceThisRound;

	public CActor WoundedBy;

	private EType m_Type;

	private EType m_OriginalType;

	private EType m_OverrideType = EType.Unknown;

	private Point m_StartArrayIndex;

	protected Point m_ArrayIndex;

	protected Point m_PrevArrayIndex;

	protected List<Point> m_MultiHexArrayIndexes;

	protected int m_Health;

	protected int m_MaxHealth;

	protected int m_Gold;

	protected LootState m_TotalLoot = new LootState();

	[StateDump(Skip = true)]
	protected LootState m_LastLoot = new LootState();

	protected LootState m_LootFromLootCards = new LootState();

	protected int m_XP;

	protected int m_Level;

	protected CTokens m_Tokens;

	protected CInventory m_Inventory;

	protected int m_BaseAugmentSlots;

	protected int? m_OverridedBaseAugmentSlots;

	protected int m_BaseSongSlots;

	protected int m_BaseDoomSlots;

	protected bool m_OnDeathAbilityUsed;

	protected List<CObjectProp> m_CarriedQuestProps = new List<CObjectProp>();

	protected List<CCharacterResource> m_CharacterResources = new List<CCharacterResource>();

	protected List<CActor> m_ActorsAttackedThisRound = new List<CActor>();

	protected List<CAbility.EAbilityType> m_AbilityTypesPerformedThisTurn = new List<CAbility.EAbilityType>();

	protected int m_TilesMovedThisRound;

	protected CClass m_Class;

	protected List<CActor> m_AIMoveFocusActors = new List<CActor>();

	[StateDump(Skip = true)]
	protected List<Point> m_AIMoveFocusPath;

	[StateDump(Skip = true)]
	protected List<Point> m_AIMoveFocusTiles = new List<Point>();

	protected bool m_MovementPathSelected;

	[StateDump(Skip = true)]
	protected List<CTile> m_AIMoveFocusWaypoints;

	protected int m_AIMoveRange;

	protected CAbility m_LastAbilityDamagedBy;

	protected CAbility m_CauseOfDeathAbility;

	protected int m_LastDamageAmount;

	protected int m_PreDamageHealth;

	protected CVars m_TempVars = new CVars();

	public static readonly List<CAbility.EAbilityType> ImmunityConditionAbilityTypes = new List<CAbility.EAbilityType>
	{
		CAbility.EAbilityType.Bless,
		CAbility.EAbilityType.Curse,
		CAbility.EAbilityType.Disarm,
		CAbility.EAbilityType.Immobilize,
		CAbility.EAbilityType.Invisible,
		CAbility.EAbilityType.Muddle,
		CAbility.EAbilityType.Poison,
		CAbility.EAbilityType.Strengthen,
		CAbility.EAbilityType.Stun,
		CAbility.EAbilityType.Wound,
		CAbility.EAbilityType.StopFlying,
		CAbility.EAbilityType.Sleep,
		CAbility.EAbilityType.Bane,
		CAbility.EAbilityType.Regenerate,
		CAbility.EAbilityType.Ward,
		CAbility.EAbilityType.Brittle
	};

	public static readonly List<CAbility.EAbilityType> ImmunityForcedMovementAbilityTypes = new List<CAbility.EAbilityType>
	{
		CAbility.EAbilityType.Push,
		CAbility.EAbilityType.Pull
	};

	[StateDump(Skip = true)]
	private CActor m_mirrorAbilitiesSourceCache;

	[StateDump(Skip = true)]
	private CActor m_mirrorAbilitiesCloneCache;

	[StateDump(Skip = true)]
	private CActor m_mirrorHealthSourceCache;

	[StateDump(Skip = true)]
	private CActor m_mirrorHealthCloneCache;

	private static int s_TargetPathID;

	public static Vector s_LOSTileScalar;

	public static ScenarioManager.EAdjacentPosition[] s_EdgeToAdjacentMapping = new ScenarioManager.EAdjacentPosition[6]
	{
		ScenarioManager.EAdjacentPosition.ETopRight,
		ScenarioManager.EAdjacentPosition.ERight,
		ScenarioManager.EAdjacentPosition.EBottomRight,
		ScenarioManager.EAdjacentPosition.EBottomLeft,
		ScenarioManager.EAdjacentPosition.ELeft,
		ScenarioManager.EAdjacentPosition.ETopLeft
	};

	private const float c_CoPlanarLimit = 0.1f;

	private const float c_MinEdgeNormalDot = 0.001f;

	[ThreadStatic]
	private static bool s_CacheLOS;

	[ThreadStatic]
	private static Dictionary<Tuple<CTile, CTile>, bool> s_LOSCache;

	public EType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public EType OverrideType
	{
		get
		{
			return m_OverrideType;
		}
		set
		{
			m_OverrideType = value;
		}
	}

	public EType OriginalType
	{
		get
		{
			return m_OriginalType;
		}
		set
		{
			m_OriginalType = value;
		}
	}

	public Point StartArrayIndex
	{
		get
		{
			return m_StartArrayIndex;
		}
		set
		{
			m_StartArrayIndex = value;
		}
	}

	public int Health
	{
		get
		{
			return m_Health;
		}
		set
		{
			int health = m_Health;
			m_Health = value;
			OnHealthChanged(health);
		}
	}

	public int OriginalMaxHealth
	{
		get
		{
			return m_MaxHealth;
		}
		set
		{
			m_MaxHealth = value;
		}
	}

	public int MaxHealth
	{
		get
		{
			if (!HealthReduction.HasValue)
			{
				return Math.Max(m_MaxHealth, Overheal);
			}
			return Math.Min(HealthReduction.Value, Math.Max(m_MaxHealth, Overheal));
		}
		set
		{
			m_MaxHealth = value;
		}
	}

	public int MaxHealthHalfRoundedUp => (int)Math.Ceiling((float)MaxHealth / 2f);

	public int Overheal
	{
		get
		{
			if (IsMirrorActor(out var mirrorSource, useCacheOnly: true))
			{
				return mirrorSource.Overheal;
			}
			return Math.Max((AccumulativeOverheal != 0) ? (OriginalMaxHealth + AccumulativeOverheal) : 0, MinimumOverheal);
		}
	}

	public int AccumulativeOverheal { get; set; }

	public int MinimumOverheal { get; set; }

	public int? HealthReduction { get; set; }

	public int? MinimumHealth { get; set; }

	public bool IgnoreSwitchForm { get; set; }

	public bool IgnoreDifficultTerrain { get; set; }

	public bool IgnoreHazardousTerrain { get; set; }

	public string OverrideCharacterModel { get; set; }

	public EType? OverrideCharacterModelActorType { get; set; }

	public int? InitiativeLockedDuringRound { get; set; }

	public CActor TargetingActor { get; protected set; }

	public CAttackSummary.TargetSummary AttackSummary { get; protected set; }

	[StateDump(Skip = true)]
	public bool AllowEscape { get; set; }

	public bool TreatTrapsAsObstacles { get; set; }

	public bool Flying { get; private set; }

	public bool FlyingAfterConditions
	{
		get
		{
			if (Flying)
			{
				return !Tokens.HasKey(CCondition.ENegativeCondition.StopFlying);
			}
			return false;
		}
	}

	public int Gold => m_Gold;

	public int XP => m_XP;

	public LootState TotalLoot => m_TotalLoot;

	public LootState LastLoot => m_LastLoot;

	public LootState LootFromLootCards
	{
		get
		{
			return m_LootFromLootCards;
		}
		set
		{
			m_LootFromLootCards = value;
		}
	}

	public virtual int Level => m_Level;

	public CClass Class
	{
		get
		{
			if (ClassOverride == null)
			{
				return m_Class;
			}
			return ClassOverride;
		}
	}

	public CMonsterClass MonsterClass => Class as CMonsterClass;

	public CCharacterClass CharacterClass => Class as CCharacterClass;

	public CTokens Tokens => m_Tokens;

	public CInventory Inventory => m_Inventory;

	public bool PlayedThisRound
	{
		get
		{
			return m_PlayedThisRound;
		}
		set
		{
			m_PlayedThisRound = value;
		}
	}

	public bool PlayedAtLeastOnceThisRound
	{
		get
		{
			return m_PlayedAtLeastOnceThisRound;
		}
		set
		{
			m_PlayedAtLeastOnceThisRound = value;
		}
	}

	public List<CActor> AIMoveFocusActors
	{
		get
		{
			return m_AIMoveFocusActors;
		}
		set
		{
			m_AIMoveFocusActors = value;
		}
	}

	[StateDump(Skip = true)]
	public List<CTile> AIMoveFocusWaypoints
	{
		get
		{
			return m_AIMoveFocusWaypoints;
		}
		set
		{
			m_AIMoveFocusWaypoints = value;
		}
	}

	[StateDump(Skip = true)]
	public List<Point> AIMoveFocusPath
	{
		get
		{
			return m_AIMoveFocusPath;
		}
		set
		{
			m_AIMoveFocusPath = value;
		}
	}

	[StateDump(Skip = true)]
	public List<Point> AIMoveFocusTiles
	{
		get
		{
			return m_AIMoveFocusTiles;
		}
		set
		{
			m_AIMoveFocusTiles = value;
		}
	}

	public bool MovementPathSelected
	{
		get
		{
			return m_MovementPathSelected;
		}
		set
		{
			m_MovementPathSelected = value;
		}
	}

	public int IncomingAttackDamage { get; set; }

	public string ActorGuid { get; private set; }

	public List<CAugment> Augments { get; private set; }

	public int AugmentSlots => m_BaseAugmentSlots + (from w in FindApplicableActiveBonuses(CAbility.EAbilityType.AddAugment)
		where w.Ability != null
		select w).Sum((CActiveBonus s) => s.Ability.Strength);

	public CAugment CachedRemovedAugment { get; private set; }

	public CActiveBonus CachedRemovedAugmentActiveBonus { get; private set; }

	public List<CSong> Songs { get; private set; }

	public int SongSlots => (m_OverridedBaseAugmentSlots.HasValue ? m_OverridedBaseAugmentSlots.Value : m_BaseSongSlots) + (from w in FindApplicableActiveBonuses(CAbility.EAbilityType.AddSong)
		where w.Ability != null
		select w).Sum((CActiveBonus s) => s.Ability.Strength);

	public List<CDoom> Dooms { get; private set; }

	public List<CDoom> CachedRemovedOnDeathDooms { get; private set; }

	public CActor DoomTarget { get; private set; }

	public int DoomSlots { get; private set; }

	public CSong CachedRemovedSong { get; private set; }

	public CActiveBonus CachedRemovedSongActiveBonus { get; private set; }

	public ECauseOfDeath CauseOfDeath { get; set; }

	public CAbility CauseOfDeathAbility => m_CauseOfDeathAbility;

	public string KilledByActorGuid { get; set; }

	public CAbilityControlActor.EControlDurationType MindControlDuration { get; set; }

	public bool MindControlIsFromEnemy { get; set; }

	public CAbilityControlActor MindControlAbility { get; set; }

	public bool CanBeDowned { get; set; }

	public bool IsDowned { get; set; }

	public bool IsDead => CauseOfDeath != ECauseOfDeath.StillAlive;

	public bool IsDeadForObjectives
	{
		get
		{
			if (!IsDowned)
			{
				if (CauseOfDeath != ECauseOfDeath.StillAlive)
				{
					return CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsBoss
	{
		get
		{
			if (Class is CMonsterClass cMonsterClass)
			{
				return cMonsterClass.Boss;
			}
			return false;
		}
	}

	public bool IsElite
	{
		get
		{
			if (Class is CMonsterClass cMonsterClass)
			{
				return cMonsterClass.NonEliteVariant != null;
			}
			return false;
		}
	}

	public bool IsUsingOnDeathAbility => m_OnDeathAbilityUsed;

	public CAbility LastAbilityPerformed { get; set; }

	public bool LastActionHasHappened { get; set; }

	public CAbility LastAbilityDamagedBy => m_LastAbilityDamagedBy;

	public int PreDamageHealth => m_PreDamageHealth;

	public int LastDamageAmount => m_LastDamageAmount;

	public bool ActorActionHasHappened { get; set; }

	public bool TurnStarted { get; protected set; }

	public bool ExhaustAfterAction { get; set; }

	public CAbilityExtraTurn.EExtraTurnType PendingExtraTurnOfType
	{
		get
		{
			if (PendingExtraTurnOfTypeStack.Count <= 0)
			{
				return CAbilityExtraTurn.EExtraTurnType.None;
			}
			return PendingExtraTurnOfTypeStack.Peek();
		}
	}

	public Stack<CAbilityExtraTurn.EExtraTurnType> PendingExtraTurnOfTypeStack { get; set; }

	public string PendingExtraTurnActionMonsterClassID
	{
		get
		{
			if (PendingExtraTurnActionMonsterClassIDStack.Count <= 0)
			{
				return string.Empty;
			}
			return PendingExtraTurnActionMonsterClassIDStack.Peek();
		}
	}

	public Stack<string> PendingExtraTurnActionMonsterClassIDStack { get; set; }

	public CAbilityExtraTurn.EExtraTurnType TakingExtraTurnOfType
	{
		get
		{
			if (TakingExtraTurnOfTypeStack.Count <= 0)
			{
				return CAbilityExtraTurn.EExtraTurnType.None;
			}
			return TakingExtraTurnOfTypeStack.Peek();
		}
	}

	public Stack<CAbilityExtraTurn.EExtraTurnType> TakingExtraTurnOfTypeStack { get; set; }

	public string TakingExtraTurnActionMonsterClassID
	{
		get
		{
			if (TakingExtraTurnActionMonsterClassIDStack.Count <= 0)
			{
				return string.Empty;
			}
			return TakingExtraTurnActionMonsterClassIDStack.Peek();
		}
	}

	public Stack<string> TakingExtraTurnActionMonsterClassIDStack { get; set; }

	public bool IsTakingExtraTurn => TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None;

	public bool HasPendingExtraTurn => PendingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None;

	public bool IsDoomed { get; set; }

	public bool NoGoldDrop { get; set; }

	public bool PhasedOut { get; set; }

	public bool Deactivated { get; set; }

	public bool SkipTurnWhenDeactivated { get; set; } = true;

	public int ChosenModelIndex { get; set; }

	public float CrowFlyDistance { get; set; }

	public List<string> TeleportedToPropGuids { get; set; }

	public virtual bool Invulnerable => false;

	public virtual bool PierceInvulnerability => false;

	public virtual List<CAbility.EAbilityType> Immunities => null;

	public virtual bool Untargetable => IsDowned;

	public virtual bool DoesNotBlock => false;

	public virtual bool SkipVisualsAndGUI => false;

	public virtual bool IgnoreActorCollision => false;

	public bool IsTransientBlocker { get; set; } = true;

	public bool IsSuperTransientBlocker { get; set; }

	public List<CObjectProp> CarriedQuestItems => m_CarriedQuestProps;

	public List<CCharacterResource> CharacterResources => m_CharacterResources;

	public List<CActor> ActorsAttackedThisRound => m_ActorsAttackedThisRound;

	public List<CAbility.EAbilityType> AbilityTypesPerformedThisTurn => m_AbilityTypesPerformedThisTurn;

	public List<CAbility.EAbilityType> AbilityTypesPerformedThisAction
	{
		get
		{
			if (PhaseManager.CurrentPhaseAction != null)
			{
				List<CAbility.EAbilityType> list = new List<CAbility.EAbilityType>();
				{
					foreach (CPhaseAction.CPhaseAbility currentAndPreviousPhaseAbility in PhaseManager.CurrentPhaseAction.CurrentAndPreviousPhaseAbilities)
					{
						if (currentAndPreviousPhaseAbility.m_Ability.AbilityHasHappened && currentAndPreviousPhaseAbility.m_Ability.TargetingActor == this && currentAndPreviousPhaseAbility.m_Ability.AbilityInterruptCopy == null)
						{
							list.Add(currentAndPreviousPhaseAbility.m_Ability.AbilityType);
						}
					}
					return list;
				}
			}
			return null;
		}
	}

	public int TilesMovedThisRound
	{
		get
		{
			return m_TilesMovedThisRound;
		}
		set
		{
			m_TilesMovedThisRound = value;
		}
	}

	public virtual bool IgnoreIceSlideEffect => false;

	public virtual bool IgnoreIceSlideWeight => false;

	public virtual int SelectedRoundCardsCount => 0;

	public virtual int MaxRoundCardsCount => 0;

	public virtual int MaxCardsClampedToHand => MaxRoundCardsCount;

	public virtual bool MustPlayAllRoundCards => false;

	public bool DontCancelTidesOnRest => ActiveBonuses.Any((CActiveBonus ab) => ab.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.DontCancelTidesOnRest));

	[StateDump(Skip = true)]
	public int TideActiveBonusesCountBeforeRest { get; set; }

	public int RoundSpawned { get; private set; }

	public virtual CPlayerActor.EPressure Pressure
	{
		get
		{
			if (GetMirrorActor() is CPlayerActor cPlayerActor)
			{
				return cPlayerActor.Pressure;
			}
			return CPlayerActor.EPressure.None;
		}
		set
		{
		}
	}

	[StateDump(Skip = true)]
	public int CachedShieldValue { get; private set; }

	public bool CachedHasRetaliate { get; private set; }

	[StateDump(Skip = true)]
	public Dictionary<int, int> CachedRetaliateRangeDamagePairs { get; private set; }

	public bool CachedShieldNeutralized { get; private set; }

	public bool CachedHealingBlocked { get; private set; }

	public List<CActiveBonus> CachedDoomActiveBonuses { get; set; }

	public List<CActiveBonus> CachedScenarioModifierBonuses { get; set; }

	public List<CActiveBonus> CachedAddDoomSlotActiveBonuses { get; set; }

	public List<CActiveBonus> CachedActiveItemEffectBonuses { get; set; }

	public List<CDisableCardActionActiveBonus> CachedDisableCardActionActiveBonuses { get; set; }

	public bool ItemLocked
	{
		get
		{
			if (!Tokens.HasKey(CCondition.ENegativeCondition.Impair))
			{
				return CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ItemLock).Count > 0;
			}
			return true;
		}
	}

	public bool VisibleBlocksPathing
	{
		get
		{
			if (!Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
			{
				return BlocksPathing;
			}
			return false;
		}
	}

	public bool BlocksPathing
	{
		get
		{
			if (!PhasedOut && !Deactivated)
			{
				return !IgnoreActorCollision;
			}
			return false;
		}
	}

	public Point RealArrayIndex => m_ArrayIndex;

	public bool invokeOnMovedListeners { get; set; } = true;

	public Point ArrayIndex
	{
		get
		{
			if (!OverrideArrayIndex.IsValid)
			{
				return m_ArrayIndex;
			}
			return OverrideArrayIndex;
		}
		set
		{
			if (!(m_ArrayIndex != value))
			{
				return;
			}
			CTile tile = Tile;
			CMap cMap = tile?.m_HexMap;
			m_PrevArrayIndex = m_ArrayIndex;
			m_ArrayIndex = value;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllAuraActiveBonuses();
			if (!(PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest))
			{
				TilesMovedThisRound++;
			}
			if (!invokeOnMovedListeners)
			{
				return;
			}
			if (this is CHeroSummonActor heroSummon)
			{
				CPlayerActor.LogHeroSummonMoveChanged(heroSummon);
			}
			if (this is CObjectActor cObjectActor)
			{
				cObjectActor.SyncPropLocation();
			}
			else if (!invokeOnMovedListeners && this is CPlayerActor playerActor)
			{
				CPlayerActor.LogHeroSummonMoveChanged(playerActor);
			}
			if (PhaseManager.CurrentPhase != null && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				return;
			}
			bool flag = Tile.m_HexMap != cMap || (Tile.m_Hex2Map != null && Tile.m_Hex2Map != cMap);
			if (flag && ScenarioScript.current != null && ScenarioScript.current.TreatTilesAsSameRoom(Tile, tile))
			{
				flag = false;
			}
			if (flag)
			{
				List<CTile> source = MF.FindMapDoors(Tile);
				List<CTile> doorsOld = MF.FindMapDoors(tile);
				if (source.Where((CTile x) => doorsOld.Contains(x)).Any((CTile x) =>
				{
					CObjectDoor cObjectDoor = x.FindDoor();
					return cObjectDoor == null || cObjectDoor.DoorType == CObjectDoor.EDoorType.EmptyDoor || cObjectDoor.DoorType == CObjectDoor.EDoorType.EmptyDoorThin;
				}))
				{
					flag = false;
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorArrayIndexChanged(Tile, flag ? cMap : null, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorArrayIndexChanged, this)));
		}
	}

	public List<Point> MultiHexArrayIndexes
	{
		get
		{
			return m_MultiHexArrayIndexes;
		}
		set
		{
			m_MultiHexArrayIndexes = value;
		}
	}

	public Point PrevArrayIndex => m_PrevArrayIndex;

	public CActor KilledByActor
	{
		get
		{
			if (!string.IsNullOrEmpty(KilledByActorGuid))
			{
				return ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.ActorGuid == KilledByActorGuid);
			}
			return null;
		}
	}

	public int ID
	{
		get
		{
			if (this is CPlayerActor cPlayerActor)
			{
				return cPlayerActor.CharacterClass.ModelInstanceID;
			}
			if (this is CEnemyActor cEnemyActor)
			{
				return cEnemyActor.StandeeID;
			}
			if (this is CHeroSummonActor cHeroSummonActor)
			{
				return cHeroSummonActor.StandeeID;
			}
			throw new Exception("Invalid Actor type.  Unable to get ID for actor " + Class.ID);
		}
	}

	public List<CActiveBonus> ActiveBonuses => FindApplicableActiveBonuses();

	public List<CPreventDamageActiveBonus> PreventDamageActiveAllSources => (from s in CharacterClassManager.FindAllActiveBonuses(CAbility.EAbilityType.PreventDamage, this)
		select s as CPreventDamageActiveBonus into w
		where (!w.HasTracker) ? (!w.PreventDamageAttackSourcesOnly) : (w.Remaining > 0)
		select w).ToList();

	public List<CPreventDamageActiveBonus> PreventDamageActiveAttackSourcesOnly => (from s in CharacterClassManager.FindAllActiveBonuses(CAbility.EAbilityType.PreventDamage, this)
		select s as CPreventDamageActiveBonus into w
		where (!w.HasTracker) ? w.PreventDamageAttackSourcesOnly : (w.Remaining > 0)
		select w).ToList();

	public List<CAbility.EAttackType> AttackTypeImmunities
	{
		get
		{
			List<CAbility.EAttackType> list = new List<CAbility.EAttackType>();
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
			{
				if (!(item.Ability is CAbilityImmunityTo cAbilityImmunityTo))
				{
					continue;
				}
				foreach (CAbility.EAttackType immuneToAttackType in cAbilityImmunityTo.ImmuneToAttackTypes)
				{
					if (!list.Contains(immuneToAttackType))
					{
						list.Add(immuneToAttackType);
					}
				}
			}
			return list;
		}
	}

	public bool IsMonsterType
	{
		get
		{
			if (m_Type != EType.Enemy && m_Type != EType.Ally && m_Type != EType.Enemy2)
			{
				return m_Type == EType.Neutral;
			}
			return true;
		}
	}

	public bool IsEnemyMonsterType
	{
		get
		{
			if (m_Type != EType.Enemy)
			{
				return m_Type == EType.Enemy2;
			}
			return true;
		}
	}

	public bool IsOriginalMonsterType
	{
		get
		{
			if (m_OriginalType != EType.Enemy && m_OriginalType != EType.Ally && m_Type != EType.Enemy2)
			{
				return m_OriginalType == EType.Neutral;
			}
			return true;
		}
	}

	public virtual CPlayerActor PlayerActor => null;

	public bool Alerted
	{
		get
		{
			if (ScenarioManager.Scenario != null)
			{
				return ScenarioManager.Scenario.CalcActorAlertedState(this);
			}
			return false;
		}
	}

	public Point InteractingTile
	{
		get
		{
			if (ScenarioScript.current == null)
			{
				return default;
			}
			return ScenarioScript.current.GetActorIteractionTile(this);
		}
	}

	public bool Interacting
	{
		get
		{
			if (ScenarioScript.current != null)
			{
				return ScenarioScript.current.IsActorInteracting(this);
			}
			return false;
		}
	}

	public bool IsScenarioActor
	{
		get
		{
			if (ScenarioScript.current != null)
			{
				return ScenarioScript.current.IsScenarioActor(this);
			}
			return false;
		}
	}

	public CTile Tile => ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];

	public List<CTile> MultiHexTiles
	{
		get
		{
			if (m_MultiHexArrayIndexes != null)
			{
				return m_MultiHexArrayIndexes.Select((Point i) => ScenarioManager.Tiles[i.X, i.Y]).ToList();
			}
			return null;
		}
	}

	public bool IsInWaterTile => Tile?.IsWater ?? false;

	public virtual bool IsSummonedByPlayer => false;

	public void MarkWontPlayThisRound(bool wontPlay)
	{
		if (wontPlay)
		{
			PlayedThisRound = true;
			if (GameState.TurnActor != null && GameState.TurnOrderSoFar.Any(((CActor, CClass) x) => x.Item1 == GameState.TurnActor))
			{
				GameState.TurnOrderSoFar.Insert(GameState.TurnOrderSoFar.Count - 1, (this, Class));
			}
			else
			{
				GameState.TurnOrderSoFar.Add((this, Class));
			}
		}
		else
		{
			PlayedThisRound = false;
		}
	}

	public virtual CClass ActiveBonusClass()
	{
		return m_Class;
	}

	public virtual void CleanUpBeforeSave()
	{
		m_StartArrayIndex = (m_PrevArrayIndex = m_ArrayIndex);
		LastAbilityPerformed = null;
		IncomingAttackDamage = 0;
		m_LastAbilityDamagedBy = null;
		m_LastDamageAmount = 0;
		m_PreDamageHealth = 0;
		m_AIMoveFocusTiles.Clear();
		m_AIMoveFocusActors.Clear();
		m_CauseOfDeathAbility = null;
		m_TempVars.Clear();
		m_Class?.SortLists();
		foreach (CActiveBonus item in FindApplicableActiveBonuses())
		{
			item.CleanupBeforeSave();
		}
	}

	public virtual void StartTurn()
	{
		TurnStarted = true;
	}

	public virtual void EndTurn()
	{
		TurnStarted = false;
		CActorStatic.Reset();
	}

	public void NextRound()
	{
		TurnStarted = false;
		ActorActionHasHappened = false;
		ProcessConditionTokens(EConditionDecTrigger.Rounds);
	}

	public override string ToString()
	{
		return "[" + MF.RefStr(this) + "] " + GetType().Name + "(" + Class?.ID + ", GUID:" + ActorGuid + ")";
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return GetType().Name + "(" + Class?.ID + ", GUID:" + ActorGuid + ")";
	}

	public bool CanTargetFromAnywhere(int attackRange, bool hasOtherTargets)
	{
		if (_canTargetFromAnywhereFlags.HasFlag(TargetFromAnywhereFlags.IfNoOtherTargets) & hasOtherTargets)
		{
			return false;
		}
		if (_canTargetFromAnywhereFlags.HasFlag(TargetFromAnywhereFlags.Melee) && attackRange <= 1)
		{
			return true;
		}
		if (_canTargetFromAnywhereFlags.HasFlag(TargetFromAnywhereFlags.Ranged) && attackRange > 1)
		{
			return true;
		}
		return false;
	}

	public TargetFromAnywhereFlags GetCanTargetFromAnywhere()
	{
		return _canTargetFromAnywhereFlags;
	}

	public void SetCanTargetFromAnywhere(TargetFromAnywhereFlags value)
	{
		_canTargetFromAnywhereFlags = value;
	}

	public bool CanBeTargetedFromAnywhere()
	{
		return _canBeTargetedFromAnywhere;
	}

	public void SetCanBeTargetedFromAnywhere(bool value)
	{
		_canBeTargetedFromAnywhere = value;
	}

	public bool IsSpecificItemLocked(CItem item)
	{
		if (item.Slot == CItem.EItemSlot.SmallItem && FindApplicableActiveBonuses(CAbility.EAbilityType.NullTargeting).Any((CActiveBonus x) => x.Ability.GetMiscVar("CannotUseSmallItems").GetBool()))
		{
			return true;
		}
		if (Tokens.HasKey(CCondition.ENegativeCondition.Stun) && (item.YMLData?.Data?.Abilities == null || !item.YMLData.Data.Abilities.Any((CAbility x) => x.MiscAbilityData.IgnoreStun.HasValue && x.MiscAbilityData.IgnoreStun.Value)))
		{
			return true;
		}
		if (item.IgnoreImpair)
		{
			return false;
		}
		if (ItemLocked)
		{
			return item.CardType == CBaseCard.ECardType.Item;
		}
		return false;
	}

	public Point GetArrayIndex(bool useRealPosition)
	{
		if (useRealPosition)
		{
			return RealArrayIndex;
		}
		return ArrayIndex;
	}

	public CActor(EType type, CClass actorClass, Point startArrayIndex, int currentHealth, int maxHealth, int level, string actorGuid, int chosenModelIndex)
	{
		if (actorGuid == null)
		{
			if (ScenarioManager.CurrentScenarioState != null)
			{
				bool flag = false;
				while (!flag)
				{
					ActorGuid = ScenarioManager.CurrentScenarioState.GetGUIDBasedOnGuidRNGState().ToString();
					if (ScenarioManager.CurrentScenarioState.ActorStates.Count <= 0 || ScenarioManager.CurrentScenarioState.ActorStates.Find((ActorState s) => s.ActorGuid == ActorGuid) == null)
					{
						flag = true;
					}
				}
			}
			else
			{
				ActorGuid = Guid.NewGuid().ToString();
			}
			m_PlayedThisRound = false;
			m_PlayedAtLeastOnceThisRound = false;
			m_Gold = 0;
			m_XP = 0;
			RoundSpawned = ScenarioManager.CurrentScenarioState.RoundNumber;
			if (GameState.CurrentSelectAndPlayCardAbility != null && PhaseManager.PreviousPhase != null && PhaseManager.PreviousPhase.Type == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				RoundSpawned--;
			}
		}
		else
		{
			ActorGuid = actorGuid;
		}
		m_Type = type;
		m_OriginalType = type;
		OverrideCharacterModelActorType = null;
		m_Class = actorClass;
		ChosenModelIndex = chosenModelIndex;
		CustomLocKey = string.Empty;
		m_StartArrayIndex = startArrayIndex;
		m_ArrayIndex = startArrayIndex;
		m_Health = currentHealth;
		MaxHealth = maxHealth;
		m_Level = level;
		int maxSmallItems = (int)Math.Ceiling((float)m_Level / 2f);
		m_Inventory = new CInventory(this, maxSmallItems);
		m_Tokens = new CTokens(this);
		Augments = new List<CAugment>();
		m_BaseAugmentSlots = 1;
		Songs = new List<CSong>();
		m_BaseSongSlots = 1;
		Dooms = new List<CDoom>();
		CachedRemovedOnDeathDooms = new List<CDoom>();
		m_BaseDoomSlots = 1;
		DoomSlots = m_BaseDoomSlots;
		CachedDoomActiveBonuses = new List<CActiveBonus>();
		CachedAddDoomSlotActiveBonuses = new List<CActiveBonus>();
		CachedActiveItemEffectBonuses = new List<CActiveBonus>();
		CachedDisableCardActionActiveBonuses = new List<CDisableCardActionActiveBonus>();
		CachedScenarioModifierBonuses = new List<CActiveBonus>();
		TeleportedToPropGuids = new List<string>();
		CauseOfDeath = ECauseOfDeath.StillAlive;
		ExhaustAfterAction = false;
		PhasedOut = false;
		AIPreferMoveFocusPath = false;
		TakingExtraTurnOfTypeStack = new Stack<CAbilityExtraTurn.EExtraTurnType>();
		PendingExtraTurnOfTypeStack = new Stack<CAbilityExtraTurn.EExtraTurnType>();
		TakingExtraTurnActionMonsterClassIDStack = new Stack<string>();
		PendingExtraTurnActionMonsterClassIDStack = new Stack<string>();
		CacheMirrorProperties();
		if (!ScenarioManager.IsLoadingNotFirsLoad)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventActor(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorInitialized, this)));
		}
	}

	public virtual void LoadFields(ActorState state)
	{
		RoundSpawned = state.RoundSpawned;
		InitiativeLockedDuringRound = state.InitiativeLockedDuringRound;
		m_ArrayIndex = new Point(state.Location.X, state.Location.Y);
		m_StartArrayIndex = (m_PrevArrayIndex = m_ArrayIndex);
		m_Health = state.Health;
		MinInitialMaxHealth = state.MinInitialMaxHealth;
		MaxHealth = state.MaxHealth;
		if (!(this is CEnemyActor))
		{
			m_Level = state.Level;
		}
		m_Tokens = new CTokens(this, state.PositiveConditions, state.NegativeConditions);
		m_PlayedThisRound = state.PlayedThisRound;
		m_PlayedAtLeastOnceThisRound = state.PlayedAtLeastOnceThisRound;
		TurnStarted = state.TurnStarted;
		ActorActionHasHappened = state.ActorActionHasHappened;
		LastActionHasHappened = state.LastActionHasHappened;
		m_CarriedQuestProps = new List<CObjectProp>(state.CarriedQuestProps);
		CauseOfDeath = state.CauseOfDeath;
		KilledByActorGuid = state.KilledByActorGuid;
		IsDowned = state.IsDowned;
		CanBeDowned = state.CanBeDowned;
		IsShadowMarked = state.IsShadowMarked;
		_canTargetFromAnywhereFlags = state.CanTargetFromAnywhereFlags;
		_canBeTargetedFromAnywhere = state.CanBeTargetedFromAnywhere;
		m_AbilityTypesPerformedThisTurn = state.AbilityTypesPerformedThisTurn?.ToList() ?? new List<CAbility.EAbilityType>();
		m_ActorsAttackedThisRound = GameState.LoadActorsFromGUIDs<CActor>(state.ActorsAttackedThisRound);
		TilesMovedThisRound = state.TilesMovedThisRound;
		WoundedBy = GameState.LoadActorFromGUID<CActor>(state.WoundedBy);
		m_AIMoveFocusWaypoints = state.AIMoveFocusWaypoints?.Select((Point x) => ScenarioManager.GetTile(x)).ToList() ?? new List<CTile>();
		AIPreferMoveFocusPath = false;
		RefreshFlyingFlag();
		PhasedOut = state.PhasedOut;
		Deactivated = state.Deactivated;
		SkipTurnWhenDeactivated = state.SkipTurnWhenDeactivated;
		OverrideCharacterModel = state.OverrideCharacterModel;
		OverrideCharacterModelActorType = ((state.OverrideCharacterModelActorType != Type && state.OverrideCharacterModelActorType != EType.Unknown) ? new EType?(state.OverrideCharacterModelActorType) : ((EType?)null));
		if (state.Actor.Type == EType.HeroSummon && state.OverrideCharacterModelActorType == EType.Player)
		{
			OverrideCharacterModelActorType = null;
		}
		CustomLocKey = state.CustomLocKey;
		NoGoldDrop = state.NoGoldDrop;
		HasBaneThisTurn = state.HasBaneThisTurn;
		if (PhasedOut)
		{
			CActorIsTeleporting_MessageData message = new CActorIsTeleporting_MessageData(this)
			{
				m_ActorTeleporting = this,
				m_TeleportAbility = null
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		if (m_Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
		{
			CSleep_MessageData message2 = new CSleep_MessageData("", null)
			{
				m_ActorToSleep = this
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
		if (IsDowned)
		{
			ScenarioRuleClient.MessageHandler(new CActorDowned_MessageData(this));
		}
		m_CharacterResources = new List<CCharacterResource>();
		foreach (KeyValuePair<string, int> characterResource in state.CharacterResources)
		{
			m_CharacterResources.Add(new CCharacterResource(characterResource.Key, characterResource.Value));
		}
	}

	public void AddAugmentOrSong(CAbility ability, CActor caster, int? id = null, int? activeBonusStartRound = null)
	{
		int? activeBonusStartRound2;
		if (ability.Augment != null)
		{
			CBaseCard cBaseCard = null;
			if (!Augments.Contains(ability.Augment))
			{
				if (Augments.Count >= AugmentSlots)
				{
					if (AugmentSlots != 1)
					{
						CActiveBonusAugmentSlotChoice_MessageData message = new CActiveBonusAugmentSlotChoice_MessageData(this)
						{
							m_Actor = this,
							m_Ability = ability
						};
						ScenarioRuleClient.MessageHandler(message);
						GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
						return;
					}
					CachedRemovedAugment = Augments[0];
					CachedRemovedAugmentActiveBonus = CachedRemovedAugment.RegisteredBaseCard.GetAugmentActiveBonus(CachedRemovedAugment);
					RemoveAugment(Augments[0]);
					Augments.Add(ability.Augment);
				}
				else
				{
					Augments.Add(ability.Augment);
				}
				cBaseCard = FindCardWithAbility(ability);
				if (cBaseCard != null)
				{
					ability.Augment.RegisteredBaseCard = cBaseCard;
					CBaseCard cBaseCard2 = cBaseCard;
					activeBonusStartRound2 = activeBonusStartRound;
					cBaseCard2.AddActiveBonus(ability, this, caster, id, null, isAugment: true, isSong: false, loadingItemBonus: false, isDoom: false, null, activeBonusStartRound2);
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(new SEventAbility.SEventAbilityBaseData(this, CAbility.EAbilityType.Augment, ESESubTypeAbility.AbilityEnded, cBaseCard, null, null, hasHappened: true, ability.Flags)));
		}
		if (ability.Song == null)
		{
			return;
		}
		if (Songs.Count >= SongSlots)
		{
			if (SongSlots != 1)
			{
				CActiveBonusSongSlotChoice_MessageData message2 = new CActiveBonusSongSlotChoice_MessageData(this)
				{
					m_Actor = this,
					m_Ability = ability
				};
				ScenarioRuleClient.MessageHandler(message2);
				GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
				return;
			}
			CachedRemovedSong = Songs[0];
			CachedRemovedSongActiveBonus = CachedRemovedSong.RegisteredBaseCard.GetSongActiveBonus(CachedRemovedSong);
			RemoveSong(Songs[0]);
			Songs.Add(ability.Song);
		}
		else
		{
			Songs.Add(ability.Song);
		}
		CBaseCard cBaseCard3 = FindCardWithAbility(ability);
		ability.Song.RegisteredBaseCard = cBaseCard3;
		activeBonusStartRound2 = activeBonusStartRound;
		cBaseCard3.AddActiveBonus(ability, this, caster, id, null, isAugment: false, isSong: true, loadingItemBonus: false, isDoom: false, null, activeBonusStartRound2);
	}

	public void AddDoom(CDoom doom, CActor doomTarget, bool addDoomActiveBonuses = true, bool bonusCheck = true)
	{
		if (Dooms.Contains(doom))
		{
			return;
		}
		if (DoomTarget != null && DoomTarget != doomTarget)
		{
			RemoveAllDooms();
		}
		DoomTarget = doomTarget;
		doomTarget.IsDoomed = true;
		if (Dooms.Count >= DoomSlots)
		{
			if (DoomSlots != 1)
			{
				CActiveBonusDoomSlotChoice_MessageData message = new CActiveBonusDoomSlotChoice_MessageData(this)
				{
					m_DoomTargetActor = doomTarget,
					m_NewDoom = doom
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
				return;
			}
			RemoveDoom(Dooms[0]);
			Dooms.Add(doom);
			doomTarget.IsDoomed = true;
		}
		else
		{
			Dooms.Add(doom);
		}
		if (!addDoomActiveBonuses)
		{
			return;
		}
		foreach (CAbility doomAbility in doom.DoomAbilities)
		{
			CBaseCard cBaseCard = FindCardWithAbility(doomAbility);
			if (cBaseCard == null && this is CHeroSummonActor cHeroSummonActor)
			{
				cBaseCard = cHeroSummonActor.BaseCard;
			}
			if (cBaseCard != null)
			{
				CBaseCard cBaseCard2 = cBaseCard;
				bool bonusCheck2 = bonusCheck;
				cBaseCard2.AddActiveBonus(doomAbility, doomTarget, this, null, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: true, null, null, null, bonusCheck2);
			}
		}
	}

	public void ReplaceAugment(CAbility newAugmentAbility, CAugment augmentToBeReplaced, CActor caster)
	{
		if (Augments.Contains(augmentToBeReplaced))
		{
			CachedRemovedAugment = augmentToBeReplaced;
			CachedRemovedAugmentActiveBonus = CachedRemovedAugment.RegisteredBaseCard.GetAugmentActiveBonus(CachedRemovedAugment);
			RemoveAugment(augmentToBeReplaced);
			AddAugmentOrSong(newAugmentAbility, caster);
		}
	}

	public void ReplaceSong(CAbility newSongAbility, CSong songToBeReplaced, CActor caster)
	{
		if (Songs.Contains(songToBeReplaced))
		{
			CachedRemovedSong = songToBeReplaced;
			CachedRemovedSongActiveBonus = CachedRemovedSong.RegisteredBaseCard.GetSongActiveBonus(CachedRemovedSong);
			RemoveSong(songToBeReplaced);
			AddAugmentOrSong(newSongAbility, caster);
		}
	}

	public void ReplaceDoom(CDoom newDoom, CDoom doomToBeReplaced, CActor doomTarget)
	{
		if (Dooms.Contains(doomToBeReplaced))
		{
			if (CachedRemovedOnDeathDooms.Contains(newDoom))
			{
				CachedRemovedOnDeathDooms.Remove(newDoom);
			}
			RemoveDoom(doomToBeReplaced);
			AddDoom(newDoom, doomTarget);
		}
	}

	public void TransferAllDooms(CActor doomTarget)
	{
		for (int num = Dooms.Count - 1; num >= 0; num--)
		{
			TransferDoom(Dooms[num], doomTarget);
		}
	}

	public void TransferDoom(CDoom doomToTransfer, CActor doomTarget)
	{
		if (!Dooms.Contains(doomToTransfer))
		{
			return;
		}
		if (CachedRemovedOnDeathDooms.Contains(doomToTransfer))
		{
			CachedRemovedOnDeathDooms.Remove(doomToTransfer);
		}
		Dooms.Remove(doomToTransfer);
		DoomTarget.IsDoomed = false;
		foreach (CActiveBonus item in CharacterClassManager.FindAllDoomActiveBonuses(null, checkDeadActor: true))
		{
			if (doomToTransfer.DoomAbilities.Contains(item.Ability))
			{
				item.Finish();
			}
		}
		AddDoom(doomToTransfer, doomTarget);
		(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public void UndoAugment(CAugment previousAugment)
	{
		if (!Augments.Contains(previousAugment))
		{
			return;
		}
		RemoveAugment(previousAugment, undo: true);
		if (CachedRemovedAugment != null)
		{
			Augments.Add(CachedRemovedAugment);
			CachedRemovedAugmentActiveBonus.UndoFinish();
			CachedRemovedAugment.RegisteredBaseCard.AddActiveBonus(CachedRemovedAugmentActiveBonus);
			if (Class is CCharacterClass cCharacterClass && CachedRemovedAugment.RegisteredBaseCard is CAbilityCard roundAbilityCard)
			{
				cCharacterClass.RestoreCachedAugmentOrSongAbilityCard(roundAbilityCard);
			}
			ClearCharacterSpecialMechanicsCache(clearAugments: true);
		}
	}

	public void UndoSong(CSong previousSong)
	{
		if (!Songs.Contains(previousSong))
		{
			return;
		}
		RemoveSong(previousSong);
		if (CachedRemovedSong != null)
		{
			Songs.Add(CachedRemovedSong);
			CachedRemovedSongActiveBonus.UndoFinish();
			CachedRemovedSong.RegisteredBaseCard.AddActiveBonus(CachedRemovedSongActiveBonus);
			if (Class is CCharacterClass cCharacterClass && CachedRemovedSong.RegisteredBaseCard is CAbilityCard roundAbilityCard)
			{
				cCharacterClass.RestoreCachedAugmentOrSongAbilityCard(roundAbilityCard);
			}
			ClearCharacterSpecialMechanicsCache(clearAugments: true);
		}
	}

	public void ClearCharacterSpecialMechanicsCache(bool clearAugments = false, bool clearSongs = false, bool clearDooms = false)
	{
		if (clearAugments)
		{
			CachedRemovedAugment = null;
			CachedRemovedAugmentActiveBonus = null;
		}
		if (clearSongs)
		{
			CachedRemovedSong = null;
			CachedRemovedSongActiveBonus = null;
		}
		if (!clearDooms)
		{
			return;
		}
		foreach (CDoom cachedRemovedOnDeathDoom in CachedRemovedOnDeathDooms)
		{
			RemoveDoom(cachedRemovedOnDeathDoom);
		}
		CachedRemovedOnDeathDooms.Clear();
	}

	public void RemoveAugment(CAugment augment, bool undo = false)
	{
		CAugment activeAugment = augment;
		CAugment cAugment = null;
		foreach (CAugment augment2 in Augments)
		{
			if (augment2 == augment)
			{
				cAugment = augment2;
			}
			else if (augment.AugmentType == CAugment.EAugmentType.Bonus && augment2.Abilities != null && augment2.Abilities[0].Augment == augment)
			{
				cAugment = augment2;
			}
		}
		if (cAugment == null)
		{
			return;
		}
		Augments.Remove(cAugment);
		if (augment.AugmentType == CAugment.EAugmentType.Bonus)
		{
			List<CAbility> abilities = augment.Abilities;
			if (abilities != null && abilities.Count > 0)
			{
				activeAugment = augment.Abilities[0].Augment;
			}
		}
		CActiveBonus cActiveBonus = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Augment == activeAugment);
		if (cActiveBonus != null)
		{
			if (undo)
			{
				augment.RegisteredBaseCard.ActiveBonuses.Remove(cActiveBonus);
				if (Class is CCharacterClass cCharacterClass && cCharacterClass.ActivatedCards.Contains(cActiveBonus.BaseCard))
				{
					cCharacterClass.ActivatedCards.Remove(cActiveBonus.BaseCard);
				}
			}
			else
			{
				cActiveBonus.Finish();
				(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
				foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
				{
					allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
				}
			}
		}
		cAugment.RegisteredBaseCard?.RemoveAugmentActiveBonus(activeAugment);
	}

	public void RemoveSong(CSong song)
	{
		if (!Songs.Contains(song))
		{
			return;
		}
		Songs.Remove(song);
		CActiveBonus cActiveBonus = ActiveBonuses.SingleOrDefault((CActiveBonus s) => s.Ability.Song == song);
		if (cActiveBonus != null)
		{
			cActiveBonus.Finish();
			(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
		song.RegisteredBaseCard.RemoveSongActiveBonus(song);
	}

	public void RemoveDoom(CDoom doom)
	{
		if (!Dooms.Contains(doom))
		{
			return;
		}
		Dooms.Remove(doom);
		DoomTarget.IsDoomed = false;
		foreach (CActiveBonus item in CharacterClassManager.FindAllDoomActiveBonuses(null, checkDeadActor: true))
		{
			if (!doom.DoomAbilities.Contains(item.Ability))
			{
				continue;
			}
			item.Finish();
			(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
	}

	public void RemoveAllDooms()
	{
		for (int num = Dooms.Count - 1; num >= 0; num--)
		{
			RemoveDoom(Dooms[num]);
		}
	}

	public void RefreshCharacterSpecialMechanicSlots()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			while (Augments.Count > AugmentSlots)
			{
				RemoveAugment(Augments[0]);
			}
			while (Songs.Count > SongSlots)
			{
				RemoveSong(Songs[0]);
			}
			while (Dooms.Count > DoomSlots)
			{
				RemoveDoom(Dooms[0]);
			}
		}
	}

	public void OverrideAugmentSlots(int? overrideValue)
	{
		if (overrideValue.HasValue)
		{
			m_OverridedBaseAugmentSlots = overrideValue.Value;
		}
		else
		{
			m_OverridedBaseAugmentSlots = null;
		}
	}

	public void LogActiveBonuses(bool atStartOfRest = false, List<CActiveBonus> cachedAllActiveBonuses = null)
	{
		if (!ScenarioManager.IsLoadingNotFirsLoad)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorActiveBonuses(this, atStartOfRest, new SEventActor.SEventActorBaseData(ESESubTypeActor.None, this), cachedAllActiveBonuses));
			if (this is CPlayerActor player)
			{
				CPlayerActor.LogActiveBonusTrackers(player, cachedAllActiveBonuses);
			}
		}
	}

	public virtual bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		startedOnDeathAbility = false;
		CauseOfDeath = causeOfDeath;
		if (CauseOfDeath == ECauseOfDeath.None || CauseOfDeath == ECauseOfDeath.StillAlive)
		{
			Log.ErrorWithReport("Invalid cause of death when actor OnDeath entered");
			CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		if (causeOfDeath != ECauseOfDeath.ActorRemovedFromMap && !fromDeathAbilityComplete)
		{
			if (GameState.InternalCurrentActor != null && (targetingActor == null || targetingActor == GameState.InternalCurrentActor))
			{
				GameState.InternalCurrentActor.m_OnKillListeners?.Invoke(this, targetingActor, causeOfDeath, causeOfDeathAbility, onActorTurn: true);
			}
			else
			{
				targetingActor?.m_OnKillListeners?.Invoke(this, targetingActor, causeOfDeath, causeOfDeathAbility, onActorTurn: false);
			}
			bool flag = false;
			int num = 0;
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				flag = true;
				num = ((cPhaseAction.CurrentPhaseAbilities != null) ? cPhaseAction.CurrentPhaseAbilities.Count : 0);
			}
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (GameState.ShowItemSelection(playerActor, this, causeOfDeathAbility, CItem.EItemTrigger.OnAnyActorDeath))
				{
					startedOnDeathAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability?.TargetingActor == this;
				}
			}
			if (IsShadowMarked)
			{
				CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor player) => player.Class.ID == "DeathwalkerID");
				if (cPlayerActor != null)
				{
					CAbilityCreateShadow cAbilityCreateShadow = CAbility.CreateAbility(CAbility.EAbilityType.CreateShadow, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: true) as CAbilityCreateShadow;
					cAbilityCreateShadow.TargetingOverrideArrayIndex = ArrayIndex;
					cAbilityCreateShadow.m_CanClearTargets = true;
					cAbilityCreateShadow.AnimOverload = "Shadow Manipulation";
					cAbilityCreateShadow.MiscAbilityData = new AbilityData.MiscAbilityData
					{
						CanUndo = false,
						IgnoreStun = true
					};
					cAbilityCreateShadow.FromShadowMark = true;
					cAbilityCreateShadow.SetMiscVar("InterruptCurrentAbility", true);
					if (PhaseManager.CurrentPhase is CPhaseStartRoundEffects)
					{
						cAbilityCreateShadow.IsScenarioModifierAbility = true;
						GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbilityCreateShadow, cPlayerActor));
					}
					else
					{
						PhaseManager.StartAbilities(cAbilityCreateShadow, cPlayerActor, stackInsteadOfInject: true, this);
					}
				}
			}
			List<CPhaseAction.CPhaseAbility> prevAbilities = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbilities?.Where((CPhaseAction.CPhaseAbility x) => !x.m_Ability.ProcessIfDead || x.m_Ability.TargetingActor != this)?.ToList();
			CAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
			CActor cachedActor = this;
			bool flag2 = PhaseManager.CurrentPhaseAction != null;
			m_OnDeathListeners?.Invoke(this, causeOfDeathAbility);
			if (this is CHeroSummonActor cHeroSummonActor)
			{
				GameState.ShowItemSelection(cHeroSummonActor.Summoner, this, null, CItem.EItemTrigger.OnSummonKilled);
			}
			if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbilities: not null } cPhaseAction2)
			{
				List<CPhaseAction.CPhaseAbility> source;
				if (prevAbilities != null)
				{
					if (prevAbilities != null)
					{
						foreach (CPhaseAction.CPhaseAbility item in PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.Where((CPhaseAction.CPhaseAbility x) => prevAbilities.Any((CPhaseAction.CPhaseAbility y) => y.m_Ability.AbilityInterruptCopy == x.m_Ability)))
						{
							if (!prevAbilities.Contains(item))
							{
								prevAbilities.Add(item);
							}
						}
					}
					source = cPhaseAction2.CurrentPhaseAbilities.Except(prevAbilities).ToList();
				}
				else
				{
					source = cPhaseAction2.CurrentPhaseAbilities.ToList();
				}
				IEnumerable<CPhaseAction.CPhaseAbility> source2 = source.Where((CPhaseAction.CPhaseAbility x) => x.TargetingActor == this && x.m_Ability != currentPhaseAbility);
				if (source2.Any() && (!flag || num != cPhaseAction2.CurrentPhaseAbilities?.Count || source2.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.ProcessIfDead)))
				{
					startedOnDeathAbility = true;
					CPhaseAction.CPhaseAbility cPhaseAbility = source2.Where((CPhaseAction.CPhaseAbility x) => !x.KillAfterAction).FirstOrDefault();
					if (cPhaseAbility != null)
					{
						cPhaseAbility.m_PostEndAction = () =>
						{
							if (GameState.CurrentActor == cachedActor)
							{
								if (GameState.OverridingCurrentActor)
								{
									GameState.EndOverrideCurrentActorForOneAction(forceUseOnDeathAbilityComplete: true);
								}
								else
								{
									OnDeathAbilityComplete();
								}
							}
						};
					}
				}
				if (!flag2)
				{
					CPhaseAction.CPhaseAbility cPhaseAbility2 = source.LastOrDefault();
					if (cPhaseAbility2 != null && cPhaseAbility2.m_Ability.AbilityHasBeenCancelled && cPhaseAbility2.m_Ability.AbilityInterruptCopy != null)
					{
						cPhaseAbility2.m_Ability.Perform();
					}
				}
			}
		}
		CTile cTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
		foreach (CCharacterResource characterResource in CharacterResources)
		{
			if (characterResource.ResourceData.DropOnDeath && characterResource.Amount > 0 && CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap)
			{
				CObjectResource prop = new CObjectResource(characterResource.ResourceData, characterResource.ResourceData.ResourceModel, ScenarioManager.ObjectImportType.Resource, new TileIndex(ArrayIndex), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(prop);
			}
		}
		List<CScenarioModifier> list = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FindAll((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.ActorsCreateGraves).ToList();
		if (list.Count > 0)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == ActorGuid);
			if (actorState != null && list.Any((CScenarioModifier x) => x.ScenarioModifierFilter.IsValidTarget(actorState)))
			{
				bool isEliteGrave = false;
				if (this is CEnemyActor cEnemyActor)
				{
					isEliteGrave = cEnemyActor.MonsterClass.NonEliteVariant != null;
				}
				CObjectProp prop2 = new CObjectMonsterGrave(isEliteGrave, EPropType.MonsterGrave.ToString(), ScenarioManager.ObjectImportType.MonsterGrave, new TileIndex(ArrayIndex), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(prop2);
			}
		}
		if (causeOfDeath == ECauseOfDeath.Trap)
		{
			targetingActor = GameState.InternalCurrentActor;
		}
		if (targetingActor == null && causeOfDeathAbility != null)
		{
			targetingActor = causeOfDeathAbility.TargetingActor;
		}
		List<string> list2 = new List<string>();
		if (targetingActor != null && targetingActor.OriginalType == EType.Player)
		{
			foreach (CItem allItem in targetingActor.Inventory.AllItems)
			{
				if (allItem.SlotState == CItem.EItemSlotState.Locked)
				{
					list2.Add(allItem.YMLData.StringID);
				}
			}
		}
		_ = targetingActor?.Class.ID;
		_ = targetingActor?.OriginalType;
		KilledByActorGuid = targetingActor?.ActorGuid;
		if (!fromDeathAbilityComplete)
		{
			GameState.UpdateActorsKilledThisRoundAndTurn(this);
			if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: var currentPhaseAbility2 } && currentPhaseAbility2?.m_BaseCard is CAbilityCard cAbilityCard && cAbilityCard != null && cAbilityCard.ActiveBonuses?.Count > 0 && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
			{
				_ = cAbilityCard.ClassID;
			}
			bool attackerDisadvantage = attackSummary != null && attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage;
			bool targetAdjacent = targetingActor != null && ScenarioManager.IsTileAdjacent(ArrayIndex.X, ArrayIndex.Y, targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y);
			SEventLogMessageHandler.AddEventLogMessage(new SEventActor(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorOnDeath, this, targetingActor, causeOfDeathAbility, "", doNotSerialize: false, LastDamageAmount, PreDamageHealth, list2, attackerDisadvantage, targetAdjacent)));
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			if ((Type == EType.Enemy || Type == EType.Enemy2) && !(this is CObjectActor))
			{
				CPlayerActor cPlayerActor2 = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor player) => player.Class.ID == "DeepwraithID");
				if (cPlayerActor2 != null && (targetingActor == cPlayerActor2 || causeOfDeathAbility?.Name == "GameStateWoundAbility" || causeOfDeathAbility?.Name == "GameStateBaneAbility"))
				{
					cPlayerActor2.SetTrophyTokens(cPlayerActor2.GetTrophyTokens() + 1);
				}
			}
		}
		return !startedOnDeathAbility;
	}

	protected virtual void OnHealthChanged(int oldHealth)
	{
	}

	public virtual string GetPrefabName()
	{
		return m_Class.Models[ChosenModelIndex];
	}

	public virtual EType GetPrefabActorType()
	{
		if (!OverrideCharacterModelActorType.HasValue)
		{
			return Type;
		}
		return OverrideCharacterModelActorType.Value;
	}

	public virtual int Initiative()
	{
		return 0;
	}

	public virtual int SubInitiative()
	{
		return 0;
	}

	public virtual void CalculateAttackStrengthForUI()
	{
	}

	public virtual string ActorLocKey()
	{
		if (CustomLocKey != null && CustomLocKey != string.Empty)
		{
			return CustomLocKey;
		}
		return Class.LocKey;
	}

	public void Damaged(int strength, bool fromAttackAbility, CActor damageSource, CAbility damagingAbility, bool skipHitAnimation = false)
	{
		if (strength > 0)
		{
			m_LastAbilityDamagedBy = damagingAbility;
			m_PreDamageHealth = Health;
			m_LastDamageAmount = strength;
			if (MinimumHealth.HasValue)
			{
				Health = Math.Max(MinimumHealth.Value, Health - strength);
			}
			else if (!GodMode)
			{
				Health -= strength;
			}
			CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(damageSource);
			cActorBeenDamaged_MessageData.m_ActorBeingDamaged = this;
			cActorBeenDamaged_MessageData.m_DamageAbility = damagingAbility as CAbilityDamage;
			cActorBeenDamaged_MessageData.m_ActualDamage = strength;
			cActorBeenDamaged_MessageData.m_ActorOriginalHealth = m_PreDamageHealth;
			cActorBeenDamaged_MessageData.m_ActorWasAsleep = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			cActorBeenDamaged_MessageData.m_skipHitAnimation = skipHitAnimation;
			ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
			if (Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep, EConditionDecTrigger.None, damagingAbility?.TargetingActor);
				CActorAwakened_MessageData message = new CActorAwakened_MessageData(this);
				ScenarioRuleClient.MessageHandler(message);
			}
			if (!fromAttackAbility && this is CEnemyActor actor)
			{
				CChallenge.CheckTrigger(CChallenge.Trigger.OnEnemySufferDamage, new CChallenge.TriggerParams
				{
					actor = actor
				});
				CChallenge.CheckTrigger(CChallenge.Trigger.OnEnemySufferDamagePostRetaliate, new CChallenge.TriggerParams
				{
					actor = actor
				});
			}
			if (!fromAttackAbility && damageSource == null && damagingAbility == null && Tokens.HasKey(CCondition.EPositiveCondition.Regenerate))
			{
				RemovePositiveConditionToken(CCondition.EPositiveCondition.Regenerate, EConditionDecTrigger.WhenDamaged);
				CRegenerateRemoved_MessageData message2 = new CRegenerateRemoved_MessageData(this);
				ScenarioRuleClient.MessageHandler(message2);
			}
		}
	}

	public CBaseCard FindCardWithAbility(CAbility ability)
	{
		if (ability?.CachedAbilityBaseCard != null && ability.CachedAbilityBaseCard.CardType == CBaseCard.ECardType.Dummy)
		{
			return ability.CachedAbilityBaseCard;
		}
		CClass cClass = m_Class;
		if (ability.IsControlAbility && ability.ControllingActor != null)
		{
			cClass = ability.ControllingActor.m_Class;
		}
		CBaseCard cBaseCard = cClass.FindCardWithAbility(ability, this);
		if (cBaseCard != null && cBaseCard.CardType != CBaseCard.ECardType.Dummy)
		{
			return cBaseCard;
		}
		if (cBaseCard != null && cBaseCard.CardType == CBaseCard.ECardType.Dummy)
		{
			ability.CachedAbilityBaseCard = cBaseCard;
		}
		foreach (CCharacterClass item in ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor x) => x.CharacterClass))
		{
			if (item != cClass)
			{
				CBaseCard cBaseCard2 = item.FindCardWithAbility(ability, this);
				if (cBaseCard2 != null && cBaseCard2.CardType != CBaseCard.ECardType.Dummy)
				{
					return cBaseCard2;
				}
			}
		}
		return cBaseCard;
	}

	public CBaseCard FindCard(int id, string name)
	{
		return m_Class.FindCard(id, name);
	}

	public List<CActiveBonus> FindApplicableActiveBonuses()
	{
		return CActiveBonus.FindApplicableActiveBonuses(this);
	}

	public List<CActiveBonus> FindApplicableActiveBonuses(CAbility.EAbilityType type, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None)
	{
		return CActiveBonus.FindApplicableActiveBonuses(this, type, behaviourType);
	}

	public bool HasActiveBonusFlag(AbilityData.EActiveBonusFlag flag)
	{
		return ActiveBonuses.FirstOrDefault((CActiveBonus b) => (b.Ability.ActiveBonusData.Flags & flag) != 0) != null;
	}

	public int CalculateShield(CAbility abilityToShieldFrom = null, bool ignoreNeutralizeShield = false, bool includeItemShieldValues = false, bool cache = true, bool forceActiveItems = false)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (IsMirrorActor(out var mirrorSource, useCacheOnly: true))
			{
				int num = mirrorSource.CalculateShield(abilityToShieldFrom, ignoreNeutralizeShield, includeItemShieldValues, cache: false);
				if (cache)
				{
					CachedShieldValue = num;
				}
				return num;
			}
			if (!ignoreNeutralizeShield && FindApplicableActiveBonuses(CAbility.EAbilityType.NeutralizeShield).Count > 0)
			{
				return 0;
			}
			int num2 = BaseShield();
			List<CActiveBonus> applicableActiveShieldBonuses = (from it in FindApplicableActiveBonuses(CAbility.EAbilityType.Shield)
				where !it.IsSong && it.IsActiveBonusToggledAndNotRestricted(this)
				select it).ToList();
			applicableActiveShieldBonuses.AddRange((from it in CharacterClassManager.FindAllSongActiveBonuses(this)
				where it.Ability.AbilityType == CAbility.EAbilityType.Shield && it.Ability.ActiveBonusData.Filter.IsValidTarget(this, it.Actor, isTargetedAbility: false, MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false) && !applicableActiveShieldBonuses.Contains(it)
				select it).ToList());
			if (applicableActiveShieldBonuses != null)
			{
				num2 += applicableActiveShieldBonuses.Sum((CActiveBonus x) => x.ReferenceStrength(abilityToShieldFrom, this));
			}
			if (includeItemShieldValues)
			{
				num2 = ((!cache || forceActiveItems) ? (num2 + Inventory.SelectedItems.Sum((CItem x) => x.YMLData.Data.ShieldValue)) : (num2 + Inventory.GetItemShieldValue()));
			}
			if (cache)
			{
				CachedShieldValue = num2;
				CActor mirrorAbilitiesActor = GetMirrorAbilitiesActor(includeMirroredClone: true);
				if (mirrorAbilitiesActor != null)
				{
					mirrorAbilitiesActor.CachedShieldValue = num2;
				}
			}
			RefreshFlyingFlag();
			return num2;
		}
	}

	public void CalculateOverheal()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			int num = 0;
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Overheal);
			list.RemoveAll((CActiveBonus b) => b.Finishing() || b.Finished());
			num = list.Select((CActiveBonus a) => a.GetBehaviour<COverhealActiveBonus_AddAccumulativeOverheal>()).ToList().Sum((COverhealActiveBonus_AddAccumulativeOverheal b) => b.CurrentAddedOverheal);
			if (AccumulativeOverheal != num)
			{
				AccumulativeOverheal = num;
				Health = Math.Min(Health, MaxHealth);
				CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(this, this, Health);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public bool WillActorDie(int incomingDamage, int pierce = 0, CAbility ability = null, bool includePlayers = false, bool calcShield = false)
	{
		if (Invulnerable)
		{
			return false;
		}
		if (OriginalType == EType.Player && !includePlayers)
		{
			return false;
		}
		int num = 0;
		if (calcShield)
		{
			num = Math.Max(0, CalculateShield(ability) - pierce);
		}
		int num2 = Math.Max(0, incomingDamage - num);
		if (Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
		{
			num2 *= 2;
		}
		if (Tokens.HasKey(CCondition.EPositiveCondition.Ward))
		{
			num2 /= 2;
		}
		return Health - num2 <= 0;
	}

	public int CalculateRetaliate(CAbility ability, int distanceToEnemy)
	{
		List<CActiveBonus> usedActiveBonuses;
		return CalculateRetaliate(ability, distanceToEnemy, out usedActiveBonuses);
	}

	private List<CActiveBonus> GetRetaliateActiveBonuses()
	{
		List<CActiveBonus> applicableActiveRetaliateBonuses = (from it in FindApplicableActiveBonuses(CAbility.EAbilityType.Retaliate)
			where !it.IsSong
			select it).ToList();
		applicableActiveRetaliateBonuses.AddRange((from it in CharacterClassManager.FindAllSongActiveBonuses(this)
			where it.Ability.AbilityType == CAbility.EAbilityType.Retaliate && !applicableActiveRetaliateBonuses.Contains(it) && it.Ability.ActiveBonusData.Filter.IsValidTarget(this, it.Actor, isTargetedAbility: false, MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false)
			select it).ToList());
		return applicableActiveRetaliateBonuses;
	}

	private int GetRetaliateRangeBonusValue()
	{
		int num = 0;
		foreach (CActiveBonus item in from it in FindApplicableActiveBonuses()
			where it.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideAbility)
			select it)
		{
			bool invertValidity = item.Ability.ActiveBonusData.InvertValidity;
			bool flag = item.Ability.ActiveBonusData.ValidAbilityTypes.Contains(CAbility.EAbilityType.Retaliate);
			if (invertValidity != flag)
			{
				num += item.Ability.ActiveBonusData.ActiveBonusAbilityOverrides.Select((CAbilityOverride o) => o.RetaliateRange.GetValueOrDefault()).Aggregate(0, (int acc, int x) => acc + x);
			}
		}
		return num;
	}

	private Dictionary<int, int> GetRetaliateRangeDamagePairs(List<CActiveBonus> applicableActiveRetaliateBonuses, int baseRetaliate)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int retaliateRangeBonusValue = GetRetaliateRangeBonusValue();
		if (baseRetaliate > 0)
		{
			int key = BaseRetaliateRange() + retaliateRangeBonusValue;
			dictionary[key] = baseRetaliate;
		}
		foreach (CActiveBonus applicableActiveRetaliateBonuse in applicableActiveRetaliateBonuses)
		{
			if (applicableActiveRetaliateBonuse.Ability is CAbilityRetaliate cAbilityRetaliate)
			{
				int key2 = cAbilityRetaliate.RetaliateRange + retaliateRangeBonusValue;
				int num = applicableActiveRetaliateBonuse.ReferenceStrength(cAbilityRetaliate, this);
				if (dictionary.TryGetValue(key2, out var value))
				{
					num += value;
				}
				dictionary[key2] = num;
			}
		}
		return dictionary;
	}

	public int CalculateRetaliate(CAbility ability, int distanceToEnemy, out List<CActiveBonus> usedActiveBonuses)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			usedActiveBonuses = new List<CActiveBonus>();
			List<CActiveBonus> usedActiveBonuses2;
			if (IsMirrorActor(out var mirrorSource, useCacheOnly: true))
			{
				return mirrorSource.CalculateRetaliate(ability, distanceToEnemy, out usedActiveBonuses2);
			}
			List<CActiveBonus> retaliateActiveBonuses = GetRetaliateActiveBonuses();
			int num = BaseRetaliate(distanceToEnemy);
			if (ability != null)
			{
				if (ability.TargetingActor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack).FirstOrDefault((CActiveBonus b) => (b.Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.MuddledTargetsDontRetaliate) != 0) != null && Tokens.HasKey(CCondition.ENegativeCondition.Muddle))
				{
					return 0;
				}
				if (ability.TargetingActor.FindApplicableActiveBonuses(CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate).Count > 0)
				{
					return 0;
				}
				int retaliateRangeBonusValue = GetRetaliateRangeBonusValue();
				foreach (CActiveBonus item in retaliateActiveBonuses.Where((CActiveBonus x) => !x.HasBehaviour<CRetaliateActiveBonus_BuffRetaliate>()))
				{
					if (!(item.Ability is CAbilityRetaliate cAbilityRetaliate) || distanceToEnemy > Math.Max(1, cAbilityRetaliate.RetaliateRange + retaliateRangeBonusValue))
					{
						continue;
					}
					int num2 = 0;
					usedActiveBonuses.Add(item);
					foreach (CActiveBonus item2 in retaliateActiveBonuses.Where((CActiveBonus x) => x.HasBehaviour<CRetaliateActiveBonus_BuffRetaliate>()))
					{
						if (item2.Ability is CAbilityRetaliate cAbilityRetaliate2 && distanceToEnemy <= cAbilityRetaliate2.RetaliateRange)
						{
							num2 += item2.ReferenceStrength(ability, ability.TargetingActor);
						}
					}
					num += item.ReferenceStrength(ability, ability.TargetingActor) + num2;
				}
			}
			CachedHasRetaliate = retaliateActiveBonuses.Count > 0 || BaseRetaliate(0) > 0;
			return num;
		}
	}

	public void CheckForCachedValuesAfterActiveBonusesUpdate()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (IsDead)
			{
				AccumulativeOverheal = 0;
				CachedShieldValue = 0;
				CachedHasRetaliate = false;
				CachedRetaliateRangeDamagePairs?.Clear();
				CachedHealingBlocked = false;
				CachedShieldNeutralized = false;
				CachedDoomActiveBonuses?.Clear();
				CachedScenarioModifierBonuses?.Clear();
				CachedDisableCardActionActiveBonuses?.Clear();
				CachedActiveItemEffectBonuses?.Clear();
				CachedAddDoomSlotActiveBonuses?.Clear();
				DoomSlots = m_BaseDoomSlots;
				return;
			}
			if (IsMirrorActor(out var mirrorSource))
			{
				AccumulativeOverheal = mirrorSource.AccumulativeOverheal;
				CachedShieldValue = mirrorSource.CachedShieldValue;
				CachedHasRetaliate = mirrorSource.CachedHasRetaliate;
				if (mirrorSource.CachedRetaliateRangeDamagePairs != null)
				{
					CachedRetaliateRangeDamagePairs = new Dictionary<int, int>();
					foreach (KeyValuePair<int, int> cachedRetaliateRangeDamagePair in mirrorSource.CachedRetaliateRangeDamagePairs)
					{
						CachedRetaliateRangeDamagePairs.Add(cachedRetaliateRangeDamagePair.Key, cachedRetaliateRangeDamagePair.Value);
					}
				}
				else
				{
					CachedRetaliateRangeDamagePairs = null;
				}
				CachedHealingBlocked = mirrorSource.CachedHealingBlocked;
				CachedShieldNeutralized = mirrorSource.CachedShieldNeutralized;
				CachedDoomActiveBonuses = mirrorSource.CachedDoomActiveBonuses?.ToList();
				CachedScenarioModifierBonuses = mirrorSource.CachedScenarioModifierBonuses?.ToList();
				CachedDisableCardActionActiveBonuses = mirrorSource.CachedDisableCardActionActiveBonuses?.ToList();
				CachedActiveItemEffectBonuses = mirrorSource.CachedActiveItemEffectBonuses?.ToList();
				CachedAddDoomSlotActiveBonuses = mirrorSource.CachedAddDoomSlotActiveBonuses?.ToList();
				DoomSlots = mirrorSource.m_BaseDoomSlots;
				CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(this);
				ScenarioRuleClient.MessageHandler(message);
				return;
			}
			CalculateShield();
			List<CActiveBonus> retaliateActiveBonuses = GetRetaliateActiveBonuses();
			int num = BaseRetaliate(0);
			if (Inventory != null)
			{
				foreach (CItem item in Inventory.AllItemsAndPets.Where((CItem x) => x.SlotState == CItem.EItemSlotState.Selected && x.YMLData.Trigger != CItem.EItemTrigger.PassiveEffect))
				{
					if (item.YMLData.Data.RetaliateValue != 0 && !retaliateActiveBonuses.Any((CActiveBonus b) => b.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.SingleShotRetaliate) && b.Ability?.Strength == item.YMLData.Data.RetaliateValue))
					{
						num += item.YMLData.Data.RetaliateValue;
					}
				}
			}
			CachedHasRetaliate = retaliateActiveBonuses.Count > 0 || num > 0;
			CachedRetaliateRangeDamagePairs = (CachedHasRetaliate ? GetRetaliateRangeDamagePairs(retaliateActiveBonuses, num) : new Dictionary<int, int>());
			CachedHealingBlocked = FindApplicableActiveBonuses(CAbility.EAbilityType.BlockHealing).Count > 0;
			CachedShieldNeutralized = FindApplicableActiveBonuses(CAbility.EAbilityType.NeutralizeShield).Count > 0;
			CachedDoomActiveBonuses = CharacterClassManager.FindAllDoomActiveBonuses(this);
			CachedScenarioModifierBonuses = CActiveBonus.FindAllActiveBonusesForUI(this);
			foreach (CActiveBonus item3 in FindApplicableActiveBonuses(CAbility.EAbilityType.DisableCardAction))
			{
				if (item3 is CDisableCardActionActiveBonus item2)
				{
					CachedDisableCardActionActiveBonuses.Add(item2);
				}
			}
			if (IsOriginalMonsterType)
			{
				CachedActiveItemEffectBonuses = (from x in CharacterClassManager.FindAllActiveBonuses()
					where x.BaseCard != null && x.Actor == this && x.Caster != x.Actor && x.BaseCard.IsItemOrPet
					select x).Concat(CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttacked)).Distinct().ToList();
			}
			CachedAddDoomSlotActiveBonuses = FindApplicableActiveBonuses(CAbility.EAbilityType.AddDoomSlots).ToList();
			DoomSlots = m_BaseDoomSlots + CachedAddDoomSlotActiveBonuses.Where((CActiveBonus w) => w.Ability != null).Sum((CActiveBonus s) => s.Ability.Strength);
			CUpdateWorldspaceConditionsUI_MessageData message2 = new CUpdateWorldspaceConditionsUI_MessageData(this);
			ScenarioRuleClient.MessageHandler(message2);
		}
	}

	public CActor ApplyRedirector(CAbility ability, CActor originalTargetActor)
	{
		CActor cActor = originalTargetActor;
		foreach (CRedirectActiveBonus item in FindApplicableActiveBonuses(CAbility.EAbilityType.Redirect))
		{
			if (item == null)
			{
				return originalTargetActor;
			}
			if (!item.IsActiveBonusToggledAndNotRestricted(originalTargetActor))
			{
				continue;
			}
			CAbilityRedirect cAbilityRedirect = item.Ability as CAbilityRedirect;
			cAbilityRedirect.CalcRedirectVars();
			if (GameState.GetActorsInRange(this, this, cAbilityRedirect.RedirectRange, null, cAbilityRedirect.Filter, null, null, isTargetedAbility: false, null, false, originalTargetType: false, skipUntargetableCheck: false, cAbilityRedirect.IgnoreLOS).Contains(originalTargetActor))
			{
				cActor = item.ApplyRedirect(ability, originalTargetActor);
				if (cActor != originalTargetActor)
				{
					item.RestrictActiveBonus(originalTargetActor);
					SEventLogMessageHandler.AddEventLogMessage(new SEventActor(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorApplyRedirector, this)));
					break;
				}
			}
		}
		return cActor;
	}

	public void ApplyRetaliateToAttack(CActor actorBeingAttacked, CAbility ability, int? retaliateBuffOverride = null)
	{
		if (IsUsingOnDeathAbility || IsDead || (ability.TargetingActor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack).FirstOrDefault((CActiveBonus b) => (b.Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.MuddledTargetsDontRetaliate) != 0) != null && actorBeingAttacked.Tokens.HasKey(CCondition.ENegativeCondition.Muddle)))
		{
			return;
		}
		List<Point> list = ScenarioManager.PathFinder.FindPath(this, m_ArrayIndex, actorBeingAttacked.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
		if (foundPath)
		{
			List<CActiveBonus> usedActiveBonuses = new List<CActiveBonus>();
			int num = (retaliateBuffOverride.HasValue ? retaliateBuffOverride.Value : actorBeingAttacked.CalculateRetaliate(ability, list.Count, out usedActiveBonuses));
			if (num > 0)
			{
				actorBeingAttacked.m_OnRetaliateListeners?.Invoke();
				bool flag = FindApplicableActiveBonuses(CAbility.EAbilityType.PreventDamage, CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate).Count > 0;
				if (!flag)
				{
					CTargetRetaliate_MessageData message = new CTargetRetaliate_MessageData(null)
					{
						m_ActorBeingAttacked = actorBeingAttacked,
						m_ActorAttacking = this,
						m_retaliateBuff = num
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				int health = Health;
				bool actorWasAsleep = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(this, num, checkIfPlayerCanAvoidDamage: true, actorBeingAttacked, null, CAbility.EAbilityType.Retaliate);
				if (GameState.ActorHealthCheck(actorBeingAttacked, this, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(null)
					{
						m_ActorBeingDamaged = this,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep,
						m_skipHitAnimation = flag
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				foreach (CActiveBonus item in usedActiveBonuses)
				{
					actorBeingAttacked.GainXP(item.ReferenceXP(ability, this), item.Ability.XPSourceKey);
					item.UpdateXPTracker();
					if (item.HasTracker && item.Remaining <= 0)
					{
						item.Finish();
					}
				}
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorApplyRetaliate, this)));
	}

	public virtual int BaseShield()
	{
		return 0;
	}

	public virtual int BaseRetaliate(int distanceToEnemy)
	{
		return 0;
	}

	public virtual int BaseRetaliateRange()
	{
		return 1;
	}

	public void Healed(int strength, bool ignoreTokens = false, bool report = true, bool actualHeal = false, bool triggerGenericHealFinished = true)
	{
		if (actualHeal)
		{
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.AddHeal))
			{
				if (item.Ability?.ActiveBonusData != null && item.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal))
				{
					strength += item.ReferenceStrength(new CAbility(), this);
					m_OnBeingHealedListeners?.Invoke(new CAbilityHeal(CAbilityHeal.HealAbilityData.DefaultHealData()));
				}
			}
		}
		Healed(strength, null, this, out var _, out var _, out var _, out var _, out var _, ignoreTokens, report, triggerGenericHealFinished);
	}

	public void Healed(int strength, CAbility healAbility, CActor actorHealing, out bool baneRemoved, out bool woundRemoved, out bool poisonRemoved, out bool brittleRemoved, out int healedAmount, bool ignoreTokens = false, bool report = true, bool triggerGenericHealFinished = true)
	{
		woundRemoved = false;
		poisonRemoved = false;
		baneRemoved = false;
		brittleRemoved = false;
		healedAmount = 0;
		if (!ignoreTokens && Tokens.HasKey(CCondition.ENegativeCondition.Bane))
		{
			RemoveNegativeConditionToken(CCondition.ENegativeCondition.Bane, EConditionDecTrigger.None, actorHealing);
			HasBaneThisTurn = false;
			baneRemoved = true;
		}
		if (!ignoreTokens && Tokens.TryGet(CCondition.ENegativeCondition.Wound, out var result) && result.ConditionDecTrigger != EConditionDecTrigger._Wound_Unremovable)
		{
			RemoveNegativeConditionToken(CCondition.ENegativeCondition.Wound, EConditionDecTrigger.None, actorHealing);
			woundRemoved = true;
		}
		if (!ignoreTokens && Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
		{
			RemoveNegativeConditionToken(CCondition.ENegativeCondition.Brittle, EConditionDecTrigger.None, actorHealing);
			brittleRemoved = true;
		}
		if (!ignoreTokens && Tokens.HasKey(CCondition.ENegativeCondition.Poison))
		{
			RemoveNegativeConditionToken(CCondition.ENegativeCondition.Poison, EConditionDecTrigger.None, actorHealing);
			poisonRemoved = true;
		}
		else
		{
			if (MaxHealth - Health < strength)
			{
				healedAmount = MaxHealth - Health;
			}
			else
			{
				healedAmount = strength;
			}
			int health = Health;
			Health = Math.Min(Health + strength, MaxHealth);
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorHealthChanged(health, new SEventActor.SEventActorBaseData(ESESubTypeActor.None, this)));
			if (health != Health)
			{
				CPhaseAction.UpdateActorsHealedThisAction(this);
			}
		}
		if (healAbility != null && healAbility is CAbilityHeal cAbilityHeal && cAbilityHeal.HealData.TriggerOnHealedListeners)
		{
			m_OnAfterBeingHealedListeners?.Invoke();
		}
		if (Type == EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid)?.IsSummon;
		}
		if (triggerGenericHealFinished)
		{
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericHealFinished, healAbility, actorHealing, this);
		}
		if (report)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorHealed(poisonRemoved, woundRemoved, baneRemoved, brittleRemoved, (!poisonRemoved) ? healedAmount : 0, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorHealed, this, actorHealing, healAbility)));
		}
	}

	public bool RemoveNegativeConditionToken(CCondition.ENegativeCondition negativeCondition, EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.None, CActor actedOnBy = null, List<EConditionDecTrigger> excludeDecTriggers = null)
	{
		if (actedOnBy == null)
		{
			actedOnBy = this;
		}
		bool flag = Tokens.RemoveNegativeToken(negativeCondition, conditionDecrementTrigger, excludeDecTriggers);
		if (flag)
		{
			m_OnNegativeConditionEndedOnActorListeners?.Invoke(negativeCondition, this, conditionDecrementTrigger);
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorRemovedCondition(CCondition.EPositiveCondition.NA, negativeCondition, new SEventActor.SEventActorBaseData(ESESubTypeActor.None, actedOnBy, this)));
		}
		return flag;
	}

	public bool RemovePositiveConditionToken(CCondition.EPositiveCondition positiveCondition, EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.None, List<EConditionDecTrigger> excludeDecTriggers = null)
	{
		bool flag = Tokens.RemovePositiveToken(positiveCondition, conditionDecrementTrigger, excludeDecTriggers);
		if (flag)
		{
			PositiveConditionEndedOnActorListener onPositiveConditionEndedOnActorListeners = m_OnPositiveConditionEndedOnActorListeners;
			if (onPositiveConditionEndedOnActorListeners == null)
			{
				return flag;
			}
			onPositiveConditionEndedOnActorListeners(positiveCondition, this, conditionDecrementTrigger);
		}
		return flag;
	}

	public void ProcessConditionTokens(EConditionDecTrigger conditionDecrementTrigger)
	{
		Tokens.ProcessTokens(conditionDecrementTrigger, out var positiveConditionsRemoved, out var negativeConditionsRemoved);
		foreach (CCondition.EPositiveCondition item in positiveConditionsRemoved)
		{
			m_OnPositiveConditionEndedOnActorListeners?.Invoke(item, this, conditionDecrementTrigger);
		}
		foreach (CCondition.ENegativeCondition item2 in negativeConditionsRemoved)
		{
			m_OnNegativeConditionEndedOnActorListeners?.Invoke(item2, this, conditionDecrementTrigger);
		}
	}

	public void RefreshFlyingFlag()
	{
		if (Tokens.HasKey(CCondition.ENegativeCondition.StopFlying))
		{
			Flying = false;
			return;
		}
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Move))
		{
			if (item.TryGetBehaviour(out CMoveActiveBonus_BuffMove behaviour))
			{
				behaviour.CalculateBuffs(out var _, out var _, out var _, out var fly, out var _, out var _);
				if (fly.HasValue && fly.Value)
				{
					Flying = true;
					return;
				}
			}
		}
		Flying = false;
		if (this is CEnemyActor cEnemyActor)
		{
			Flying = cEnemyActor.MonsterClass.Flying;
		}
		if (this is CHeroSummonActor cHeroSummonActor)
		{
			Flying = cHeroSummonActor.SummonData.Flying;
		}
	}

	public virtual void ActionSelection()
	{
		m_AIMoveFocusPath = null;
		m_MovementPathSelected = false;
		m_AIMoveFocusActors.Clear();
		m_AIMoveFocusTiles.Clear();
		m_AIMoveRange = 0;
	}

	public virtual void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, CAbilityAttack attack, bool firstMove, bool moveTest, bool carryOtherActors)
	{
	}

	private static IEnumerable<CTile> FindTileWithTrapOrHazardousTerrain(IEnumerable<CTile> tiles)
	{
		foreach (CTile tile in tiles)
		{
			foreach (CObjectProp prop in tile.m_Props)
			{
				if (!prop.Activated)
				{
					if (prop is CObjectTrap)
					{
						yield return tile;
						break;
					}
					if (prop is CObjectHazardousTerrain)
					{
						yield return tile;
						break;
					}
				}
			}
		}
	}

	private bool PushPullCanEndOnTile(CTile tile, CPathFinderContext context)
	{
		if ((tile.LivingActor != null && tile.LivingActor.BlocksPathing) || context.GetNode(tile.m_ArrayIndex).HasFlag(CPathFinderContext.TNode.Flags.CannotStop))
		{
			return false;
		}
		return true;
	}

	public void Pull(Point pullToTarget, int remainingPulls, CAbilityPull.EPullType pullType, out List<Point> points, CTile autoTargetDestination = null, CAbility ability = null)
	{
		points = new List<Point>();
		List<Point> list = ability.GetMiscVar("ForcedPath").GetObject<List<Point>>();
		if (list != null && list.Count > 0)
		{
			points.AddRange(list);
		}
		else
		{
			List<CTile> pullTiles = CAbilityPull.GetPullTiles(this, Type, ArrayIndex, pullToTarget, pullType, remainingPulls, ability);
			if (pullTiles.Count > 0)
			{
				CPathFinderContext context = ScenarioManager.PathFinder.InitQuery(this, pullToTarget, pullToTarget, hasTarget: false, remainingPulls, ignoreBlocked: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, null, new List<Point>(), ignoreDoorLocks: false, null, isCalculatingDistance: false, ignoreCannotStop: false, CAbility.EAbilityType.Pull);
				bool foundPath;
				if (autoTargetDestination != null)
				{
					if (!PushPullCanEndOnTile(autoTargetDestination, context))
					{
						return;
					}
					List<Point> collection = FindCharacterPath(this, ArrayIndex, autoTargetDestination.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
					points.AddRange(collection);
				}
				else
				{
					pullTiles.RemoveAll((CTile x) => !PushPullCanEndOnTile(x, context));
					foreach (CTile item in FindTileWithTrapOrHazardousTerrain(pullTiles))
					{
						if (points.Count > 0)
						{
							break;
						}
						if (item == null)
						{
							continue;
						}
						List<Point> list2 = FindCharacterPath(this, ArrayIndex, item.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
						points.AddRange(list2);
						int num = CAbilityMove.CalculateMoveCost(this, ArrayIndex, list2, ignoreBlocked: false, ignoreMoveCost: false, ignoreDifficultTerrain: true, ignoreBlockedTileMoveCost: false, 0, null);
						int num2 = remainingPulls - num;
						if (num2 == 0 && !PushPullCanEndOnTile(item, context))
						{
							points.Clear();
							continue;
						}
						if (num2 > 0)
						{
							List<CTile> pullTiles2 = CAbilityPull.GetPullTiles(this, Type, item.m_ArrayIndex, pullToTarget, pullType, num2, ability);
							pullTiles2.RemoveAll((CTile x) => !PushPullCanEndOnTile(x, context));
							if (pullTiles2.Count > 0)
							{
								CTile cTile = null;
								int num3 = int.MaxValue;
								Point a = Point.OddRToAxial(pullToTarget);
								foreach (CTile item2 in pullTiles2)
								{
									Point b = Point.OddRToAxial(item2.m_ArrayIndex);
									int num4 = Point.AxialDistance(a, b);
									if (num4 < num3)
									{
										num3 = num4;
										cTile = item2;
									}
								}
								List<Point> collection2 = FindCharacterPath(this, item.m_ArrayIndex, cTile.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
								points.AddRange(collection2);
							}
							else if (!PushPullCanEndOnTile(item, context))
							{
								points.Clear();
								continue;
							}
						}
						if (points.Count > 0)
						{
							break;
						}
					}
					if (points.Count == 0 && pullTiles.Count > 0)
					{
						CTile cTile2 = null;
						int num5 = int.MaxValue;
						Point a2 = Point.OddRToAxial(pullToTarget);
						foreach (CTile item3 in pullTiles)
						{
							Point b2 = Point.OddRToAxial(item3.m_ArrayIndex);
							int num6 = Point.AxialDistance(a2, b2);
							if (num6 < num5)
							{
								num5 = num6;
								cTile2 = item3;
							}
						}
						List<Point> collection3 = FindCharacterPath(this, ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
						points.AddRange(collection3);
					}
				}
			}
		}
		if (points.Count != 0)
		{
			m_AIMoveFocusWaypoints = points.Select((Point x) => ScenarioManager.Tiles[x.X, x.Y]).ToList();
			ArrayIndex = points[points.Count - 1];
		}
	}

	public void Push(Point pushFromPoint, int remainingPushes, out List<Point> points, bool intoBlocked = false, CTile autoTargetDestination = null, CAbility ability = null)
	{
		points = new List<Point>();
		List<Point> list = ability.GetMiscVar("ForcedPath").GetObject<List<Point>>();
		if (list != null && list.Count > 0)
		{
			points.AddRange(list);
		}
		else
		{
			List<CTile> pushTiles = CAbilityPush.GetPushTiles(this, Type, ArrayIndex, pushFromPoint, remainingPushes, intoBlocked, ability);
			if (pushTiles.Count > 0)
			{
				CPathFinderContext context = ScenarioManager.PathFinder.InitQuery(this, pushFromPoint, pushFromPoint, hasTarget: false, remainingPushes, ignoreBlocked: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, null, new List<Point>(), ignoreDoorLocks: false, null, isCalculatingDistance: false, ignoreCannotStop: false, CAbility.EAbilityType.Push);
				if (pushTiles.Count == 0)
				{
					return;
				}
				bool foundPath;
				if (autoTargetDestination != null)
				{
					if (!PushPullCanEndOnTile(autoTargetDestination, context))
					{
						return;
					}
					List<Point> collection = FindCharacterPath(this, ArrayIndex, autoTargetDestination.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
					points.AddRange(collection);
				}
				else
				{
					foreach (CTile item in FindTileWithTrapOrHazardousTerrain(pushTiles))
					{
						if (points.Count > 0)
						{
							break;
						}
						if (item == null)
						{
							continue;
						}
						List<Point> list2 = FindCharacterPath(this, ArrayIndex, item.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
						points.AddRange(list2);
						int num = CAbilityMove.CalculateMoveCost(this, ArrayIndex, list2, ignoreBlocked: false, ignoreMoveCost: false, ignoreDifficultTerrain: true, ignoreBlockedTileMoveCost: false, 0, null);
						int num2 = remainingPushes - num;
						if (num2 == 0 && !PushPullCanEndOnTile(item, context))
						{
							points.Clear();
						}
						else
						{
							if (num2 <= 0)
							{
								continue;
							}
							List<CTile> pushTiles2 = CAbilityPush.GetPushTiles(this, Type, item.m_ArrayIndex, pushFromPoint, num2, intoBlocked, ability);
							pushTiles2.RemoveAll((CTile x) => !PushPullCanEndOnTile(x, context));
							if (pushTiles2.Count > 0)
							{
								CTile cTile = null;
								int num3 = int.MinValue;
								Point a = Point.OddRToAxial(item.m_ArrayIndex);
								foreach (CTile item2 in pushTiles2)
								{
									Point b = Point.OddRToAxial(item2.m_ArrayIndex);
									int num4 = Point.AxialDistance(a, b);
									if (num4 > num3)
									{
										num3 = num4;
										cTile = item2;
									}
								}
								List<Point> collection2 = FindCharacterPath(this, item.m_ArrayIndex, cTile.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
								points.AddRange(collection2);
							}
							else if (!PushPullCanEndOnTile(item, context))
							{
								points.Clear();
							}
						}
					}
					pushTiles.RemoveAll((CTile x) => !PushPullCanEndOnTile(x, context));
					if (points.Count == 0 && pushTiles.Count > 0)
					{
						CTile cTile2 = null;
						int num5 = int.MinValue;
						Point a2 = Point.OddRToAxial(pushFromPoint);
						foreach (CTile item3 in pushTiles)
						{
							Point b2 = Point.OddRToAxial(item3.m_ArrayIndex);
							int num6 = Point.AxialDistance(a2, b2);
							if (num6 > num5)
							{
								num5 = num6;
								cTile2 = item3;
							}
						}
						List<Point> collection3 = FindCharacterPath(this, ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, FlyingAfterConditions);
						points.AddRange(collection3);
					}
				}
			}
		}
		if (points.Count != 0)
		{
			m_AIMoveFocusWaypoints = points.Select((Point x) => ScenarioManager.Tiles[x.X, x.Y]).ToList();
			ArrayIndex = points[points.Count - 1];
		}
	}

	public CActor Clone()
	{
		CActor cActor = (CActor)MemberwiseClone();
		cActor.Inventory.InventoryActor = cActor;
		return cActor;
	}

	public void StartLooting()
	{
		m_LastLoot = new LootState();
	}

	public void TryLootItem(int itemID)
	{
		CItem getItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData x) => x.ID == itemID).GetItem;
		TryLootItem(getItem);
	}

	public void TryLootItem(CItem newItem)
	{
		if (Inventory.AllItems.Any((CItem item) => item.ID == newItem.ID) || newItem.YMLData.ItemInstanceCount >= newItem.YMLData.TotalInGame)
		{
			m_LastLoot.SoldItemIds.Add(newItem.ID);
			m_TotalLoot.SoldItemIds.Add(newItem.ID);
			Log.Info(m_Class.ID + " looted item '" + newItem.YMLData.Name + "' that he already owns");
		}
		else
		{
			Inventory.AddItem(newItem);
			m_LastLoot.ItemIds.Add(newItem.ID);
			m_TotalLoot.ItemIds.Add(newItem.ID);
			Log.Info(m_Class.ID + " looted item '" + newItem.YMLData.Name + "'");
		}
	}

	public void ShowLastLootReward(string animOverload = "", string source = "MoneyToken", ScenarioRuleClient.MessageHandlerCallback messageHandler = null, bool focusCamera = false, bool outOfPhase = false)
	{
		if (LastLoot != null)
		{
			CStackRewardsToShow_MessageData cStackRewardsToShow_MessageData = new CStackRewardsToShow_MessageData(this);
			cStackRewardsToShow_MessageData.m_Rewards = LastLoot.GetRewards().ToList();
			cStackRewardsToShow_MessageData.m_SourceType = source;
			ScenarioRuleClient.MessageHandler(cStackRewardsToShow_MessageData);
			ShowLootAnimations(animOverload, null, null, this, messageHandler, focusCamera, outOfPhase);
		}
	}

	public void ShowLootAnimations(string animOverload = "", List<CObjectProp> propsLooted = null, CAbilityLoot lootAbility = null, CActor actor = null, ScenarioRuleClient.MessageHandlerCallback messageHandler = null, bool focusCamera = false, bool outOfPhase = false)
	{
		CLooting_MessageData cLooting_MessageData = new CLooting_MessageData(animOverload, this);
		cLooting_MessageData.m_ActorLooting = ((actor == null) ? this : actor);
		cLooting_MessageData.m_PropsLooted = propsLooted;
		cLooting_MessageData.m_FocusCamera = focusCamera;
		cLooting_MessageData.m_LootAbility = lootAbility;
		cLooting_MessageData.m_OutOfPhase = outOfPhase;
		if (propsLooted != null && propsLooted.Count > 0)
		{
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericOnFinishLooting, lootAbility, this, null, ("propsLooted", new TValue(propsLooted)));
		}
		if (messageHandler == null)
		{
			ScenarioRuleClient.MessageHandler(cLooting_MessageData);
		}
		else
		{
			messageHandler(cLooting_MessageData);
		}
	}

	public bool TryLootFromDeck(bool endOfScenario = false, bool incLootTokenCount = false)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		List<CItem> list = new List<CItem>();
		if (!endOfScenario && GameState.ShowItemSelection(this, this, null, CItem.EItemTrigger.OnLootFromDeck, onlyInActionPhase: false, killAfter: false, null, list))
		{
			foreach (CItem item2 in list)
			{
				LootState lootState = null;
				foreach (var var in item2.YMLData.Data.Vars)
				{
					if (Enum.TryParse<LootCard.EType>(var.key, out var result))
					{
						if (lootState == null)
						{
							lootState = new LootState();
						}
						LootCard.EType type = result;
						TValue item = var.value;
						ApplyLoot(type, item.GetInt(), lootState);
					}
				}
				if (lootState != null)
				{
					EndApplyLoot(lootState);
				}
			}
			if (list.Any((CItem x) => x.GetVar("StopLoot").GetBool()))
			{
				return true;
			}
		}
		bool flag = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ActorGuid == ActorGuid)?.HasLootCardAdvantage ?? false;
		if (!flag && list != null && list.Any((CItem x) => x.GetVar("LootCardAdvantage").GetBool()))
		{
			flag = true;
		}
		if (!CLootDeck.DrawNext(currentScenarioState.LootDeck, out var lc))
		{
			m_LastLoot.NumEmptyDeckDraws++;
			if (incLootTokenCount)
			{
				ApplyLoot(default, currentScenarioState, incLootTokenCount: true);
			}
			return false;
		}
		if (flag && !endOfScenario)
		{
			if (CLootDeck.DrawNext(currentScenarioState.LootDeck, out var lc2))
			{
				List<LootCard> list2 = new List<LootCard> { lc, lc2 };
				if (GameState.ShowLootCardSelection(this, list2, out var selectedLootCard))
				{
					list2.Remove(selectedLootCard);
					foreach (LootCard item3 in list2)
					{
						CLootDeck.RemoveCardFromDeck(currentScenarioState.LootDeck, item3);
						CLootDeck.AddCardBeforeDelimiter(currentScenarioState.LootDeck, item3);
					}
					lc = selectedLootCard;
				}
			}
			else
			{
				m_LastLoot.NumEmptyDeckDraws++;
			}
			foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
			{
				player.SetLootCardAdvantage(val: false);
			}
		}
		ApplyLoot(lc, currentScenarioState, incLootTokenCount);
		if (list != null)
		{
			int num = list.Sum((CItem x) => x.GetVar("DrawAdditionalLootCards").GetInt());
			for (int num2 = 0; num2 < num; num2++)
			{
				if (!CLootDeck.DrawNext(currentScenarioState.LootDeck, out var lc3))
				{
					m_LastLoot.NumEmptyDeckDraws++;
					break;
				}
				ApplyLoot(lc3, currentScenarioState);
			}
		}
		return true;
	}

	public LootState ApplyLoot(LootCard.EType type, int amount, LootState drawnLoot = null, bool endLoot = false)
	{
		if (drawnLoot == null)
		{
			drawnLoot = new LootState();
		}
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		FHScenarioSetup scenarioSetup = currentScenarioState.ScenarioSetup;
		drawnLoot.AddCardTypeDrawn(type);
		if (type == LootCard.EType.RandomItem)
		{
			if (!scenarioSetup.RandomItemLooted)
			{
				if (scenarioSetup.NextRandomItemID > 0)
				{
					TryLootItem(scenarioSetup.NextRandomItemID);
				}
				else
				{
					int num = drawnLoot.AddResource(LootCard.EType.Money, 3, applyModifiers: true);
					drawnLoot.NumEmptyRandomItemDraws++;
					drawnLoot.GoldFromEmptyRandomItemDraws += num;
					Log.Info($"Out of items in the random items deck, giving {num} gold instead");
				}
				drawnLoot.AddResource(type, 1);
			}
		}
		else
		{
			drawnLoot.AddResource(type, amount, applyModifiers: true);
			Log.Info($"Drawn '{type}' +{amount} from the Loot Deck.");
		}
		if (currentScenarioState.ScenarioSetup.ExtraGoldOnLootCard != 0 && drawnLoot.ResourceAmount(LootCard.EType.Money) > 0)
		{
			drawnLoot.AddResource(LootCard.EType.Money, (int)currentScenarioState.ScenarioSetup.ExtraGoldOnLootCard);
			Log.Info($"{currentScenarioState.ScenarioSetup.ExtraGoldOnLootCard} extra gold from favors.");
		}
		if (endLoot)
		{
			EndApplyLoot(drawnLoot);
		}
		return drawnLoot;
	}

	private void ApplyLoot(LootCard lc, ScenarioState css, bool incLootTokenCount = false)
	{
		int amount = CLootDeck.CalcLootAmount(lc, css);
		LootState lootState = new LootState();
		if (incLootTokenCount)
		{
			lootState.LootTokens++;
		}
		if (lc.Type == LootCard.EType.None)
		{
			EndApplyLoot(lootState);
			return;
		}
		ApplyLoot(lc.Type, amount, lootState);
		if (lc.AvailableCampaignSectionIDs != null && lc.AvailableCampaignSectionIDs.Length != 0)
		{
			string text = lc.AvailableCampaignSectionIDs[css.ScenarioRNG.Next(lc.AvailableCampaignSectionIDs.Length)];
			if (lc.AvailableCampaignSectionIDs.Length <= 6)
			{
				text = lc.AvailableCampaignSectionIDs.FirstOrDefault((string x) => x == "128.5" || x == "133.4");
			}
			lootState.AddCampaignSection(text);
			GameState.TriggerCampaignSection(text);
			switch (text)
			{
			case "133.4":
			case "128.5":
				if (Inventory.AllItems.Any((CItem x) => x.ID == 1255))
				{
					TryLootItem(1255000002);
				}
				else
				{
					TryLootItem(1255);
				}
				break;
			case "43.4":
			case "177.3":
			case "165.3":
			case "168.1":
			case "130.4":
			case "174.5":
			case "106.3":
			case "13.2":
			case "16.2":
			case "36.4":
			case "29.1":
			case "188.2":
			case "182.3":
			case "85.1":
			case "11.1":
			case "153.4":
			case "193.5":
			case "9.3":
				lootState.AddResource(LootCard.EType.Money, (int)ScenarioManager.GoldConversionNextLevel);
				break;
			}
		}
		EndApplyLoot(lootState);
	}

	public void EndApplyLoot(LootState drawnLoot)
	{
		m_LastLoot.AccumulateLootFrom(drawnLoot);
		m_TotalLoot.AccumulateLootFrom(drawnLoot);
		m_LootFromLootCards.AccumulateLootFrom(drawnLoot);
	}

	public void GainRandomItemBlueprint()
	{
		uint id = 0u;
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState.ScenarioSetup.NextRandomItemBlueprintID != 0)
		{
			id = currentScenarioState.ScenarioSetup.NextRandomItemBlueprintID;
		}
		GainItemBlueprint(id);
	}

	public void GainItemBlueprint(uint id)
	{
		m_LastLoot.AddItemBlueprint(id);
		m_TotalLoot.AddItemBlueprint(id);
	}

	public void GainRandomSideScenario()
	{
		uint num = 0u;
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState.ScenarioSetup.NextRandomScenarioID != 0)
		{
			num = currentScenarioState.ScenarioSetup.NextRandomScenarioID;
		}
		m_LastLoot.AddRandomSideScenario(num);
		m_TotalLoot.AddRandomSideScenario(num);
		if (num != 0)
		{
			TryPlayNarrativeFromLootingScenario(num);
		}
	}

	public void GainScenario(uint id)
	{
		m_LastLoot.AddScenario(id);
		m_TotalLoot.AddScenario(id);
		if (id != 0)
		{
			TryPlayNarrativeFromLootingScenario(id);
		}
	}

	private void TryPlayNarrativeFromLootingScenario(uint id)
	{
		string arg = ScenarioManager.CurrentScenarioState.ScenarioSetup.CachedScenarioData[id];
		GameState.TriggerCampaignSection($"FS_Random_Scenario_{id}_{arg}");
	}

	public void GainCampaignSection(string section, int delayWeeks = 0)
	{
		m_LastLoot.AddCampaignSection(section, delayWeeks);
		m_TotalLoot.AddCampaignSection(section, delayWeeks);
	}

	public void GainCampaignSticker(string sticker)
	{
		m_LastLoot.GainCampaignSticker(sticker);
		m_TotalLoot.GainCampaignSticker(sticker);
	}

	public void GainPerkChecks(uint amount)
	{
		m_LastLoot.GainPerkChecks(amount);
		m_TotalLoot.GainPerkChecks(amount);
	}

	public void GainResource(LootCard.EType resource, int amount)
	{
		m_LastLoot.AddResource(resource, amount);
		m_TotalLoot.AddResource(resource, amount);
	}

	public void GainLootMoney(int money)
	{
		m_LastLoot.AddResource(LootCard.EType.Money, money);
		m_TotalLoot.AddResource(LootCard.EType.Money, money);
	}

	public void GainLootItem(int itemId)
	{
		m_LastLoot.ItemIds.Add(itemId);
		m_TotalLoot.ItemIds.Add(itemId);
	}

	public void GainLoot(bool endOfScenario = false, bool incLootTokenCount = false, bool newLoot = true)
	{
		if (OriginalType == EType.Player)
		{
			if (newLoot)
			{
				StartLooting();
			}
			TryLootFromDeck(endOfScenario, incLootTokenCount);
		}
	}

	public int LootAmount(LootCard.EType type, bool fromLootCards = false, bool cardCount = false)
	{
		if (cardCount)
		{
			LootFromLootCards.DrawnLootCardTypes.TryGetValue(type, out var value);
			return value;
		}
		if (fromLootCards)
		{
			return LootFromLootCards.ResourceAmount(type);
		}
		return TotalLoot.ResourceAmount(type);
	}

	public void CarryQuestItem(CObjectProp questItemProp)
	{
		if (questItemProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
		{
			if (!m_CarriedQuestProps.Contains(questItemProp))
			{
				m_CarriedQuestProps.Add(questItemProp);
				CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(this);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		else
		{
			Log.ErrorWithReport("Attempted to carry a non-quest item prop");
		}
	}

	public void DropQuestItem(CObjectProp questItemProp)
	{
		if (m_CarriedQuestProps.Contains(questItemProp))
		{
			float y = 0.5f * (float)SharedClient.GlobalRNG.Next(180) + 0.75f * (float)SharedClient.GlobalRNG.Next(360);
			CVector3 rotation = new CVector3(0f, y, 0f);
			CTile cTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
			CMap hexMap = cTile.m_HexMap;
			questItemProp.SetNewStartingMapGuid(hexMap.MapGuid);
			ScenarioManager.CurrentScenarioState.Props.Add(questItemProp);
			ScenarioManager.CurrentScenarioState.ActivatedProps.Remove(questItemProp);
			questItemProp.Deactivate();
			questItemProp.SetLocation(new TileIndex(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y), null, rotation);
			cTile.SpawnProp(questItemProp, notifyClient: true, 0.5f);
			m_CarriedQuestProps.Remove(questItemProp);
		}
		else
		{
			Log.ErrorWithReport("Attempted to drop a quest item not in the carried quest item list");
		}
	}

	public void AddCharacterResource(string resourceID, int amountToAdd)
	{
		CCharacterResource cCharacterResource = CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
		if (cCharacterResource != null)
		{
			cCharacterResource.Amount += amountToAdd;
		}
		else
		{
			CharacterResources.Add(new CCharacterResource(resourceID, amountToAdd));
		}
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(this);
		ScenarioRuleClient.MessageHandler(message);
	}

	public void RemoveCharacterResource(string resourceID, int amountToRemove, bool updateWorldSpaceUI = false)
	{
		CCharacterResource cCharacterResource = CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
		if (cCharacterResource != null)
		{
			cCharacterResource.Amount -= amountToRemove;
			if (cCharacterResource.Amount <= 0)
			{
				CharacterResources.Remove(cCharacterResource);
			}
			if (updateWorldSpaceUI)
			{
				CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(this);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		else
		{
			Log.ErrorWithReport("Can't remove a character resource that the player has none of");
		}
	}

	public bool CharacterHasResource(string resourceID, int amountRequired)
	{
		CCharacterResource cCharacterResource = CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
		if (cCharacterResource != null)
		{
			return cCharacterResource.Amount >= amountRequired;
		}
		return false;
	}

	public int GetCharacterResourceAmount(string resourceID)
	{
		return CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID)?.Amount ?? 0;
	}

	public bool CharacterHasResourceAny(string resourceID, int amountRequired)
	{
		List<CCharacterResource> list = CharacterResources.FindAll((CCharacterResource x) => x.ID.Contains(resourceID));
		if (list.Count > 0)
		{
			return list.Any((CCharacterResource x) => x.Amount >= amountRequired);
		}
		return false;
	}

	public bool LootTile(CTile tile, bool asPartOfAbility = true, bool endOfTurn = false, ScenarioManager.ObjectImportType objectType = ScenarioManager.ObjectImportType.None, CAbilityLoot lootAbility = null, CActor forcedActorGainingLoot = null)
	{
		bool flag = false;
		CActor cActor = null;
		if (tile != null)
		{
			cActor = this;
			if (this is CHeroSummonActor cHeroSummonActor)
			{
				cActor = cHeroSummonActor.Summoner;
			}
			CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
			if (mirrorAbilityBehaviour != null && mirrorAbilityBehaviour.MirrorTarget == cActor)
			{
				cActor = mirrorAbilityBehaviour.ActiveBonus.Actor;
			}
			if (forcedActorGainingLoot != null)
			{
				cActor = forcedActorGainingLoot;
			}
			foreach (CObjectProp item in tile.m_Props.Where((CObjectProp p) => p.IsLootable && p.CanActorLoot(this) && (asPartOfAbility || !p.IgnoreEndOfTurnLooting)).ToList())
			{
				if (objectType != ScenarioManager.ObjectImportType.None && objectType != item.ObjectType)
				{
					continue;
				}
				switch (item.ObjectType)
				{
				case ScenarioManager.ObjectImportType.Chest:
				case ScenarioManager.ObjectImportType.GoalChest:
					if (OriginalType == EType.Player || OriginalType == EType.HeroSummon)
					{
						flag = true;
						item.Activate(this, cActor);
						tile.DelProp(item);
					}
					break;
				case ScenarioManager.ObjectImportType.MoneyToken:
					cActor.GainLoot(endOfScenario: false, incLootTokenCount: true);
					flag = true;
					item.Activate(this, cActor);
					tile.DelProp(item);
					break;
				case ScenarioManager.ObjectImportType.CarryableQuestItem:
					if (OriginalType == EType.Player || (this is CHeroSummonActor && !(this is CHeroSummonActor { IsCompanionSummon: false })) || (ScenarioManager.CurrentScenarioState?.ScenarioScripts != null && ScenarioManager.CurrentScenarioState.ScenarioScripts.Any((ScenarioScript x) => x.CanActorLootQuestItems(this))))
					{
						cActor.CarryQuestItem(item);
						flag = true;
						item.Activate(this);
						tile.DelProp(item);
					}
					break;
				case ScenarioManager.ObjectImportType.Resource:
					if (OriginalType == EType.Player || (this is CHeroSummonActor && !(this is CHeroSummonActor { IsCompanionSummon: false })) || (ScenarioManager.CurrentScenarioState?.ScenarioScripts != null && ScenarioManager.CurrentScenarioState.ScenarioScripts.Any((ScenarioScript x) => x.CanActorLootResources(this))))
					{
						CObjectResource cObjectResource = (CObjectResource)item;
						cActor.AddCharacterResource(cObjectResource.ResourceData.ID, 1);
						flag = true;
						item.Activate(cActor);
					}
					break;
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventActorLooted(item.ObjectType, item.OwnerGUID, endOfTurn, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorLooted, cActor, this, lootAbility, "", doNotSerialize: false, 0, 0, null, attackerDisadvantage: false, targetAdjacent: false, item.ArrayIndex.ToPoint())));
			}
			if (flag)
			{
				CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericOnLoot, lootAbility, this, null);
			}
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		return flag;
	}

	public void UsedItem(CItem item, bool firstTimeUse)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorUsedItem(item.YMLData.StringID, item.YMLData.Slot.ToString(), firstTimeUse, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorUsedItem, this)));
	}

	public virtual EAdvantageStatuses GetAdvantageStatus(bool addAdvantage, bool addDisadvantage, int targetNumber = 0)
	{
		return Tokens.GetAdvantageStatus(addAdvantage, addDisadvantage, targetNumber);
	}

	public void GainXP(int xpAmount, string source = null)
	{
		CActor mirrorActor = GetMirrorActor();
		if (mirrorActor != null && mirrorActor is CPlayerActor)
		{
			mirrorActor.GainXP(xpAmount, source);
		}
		else if (xpAmount > 0)
		{
			m_XP += xpAmount;
			Log.Info(m_Class.ID + " has gained " + xpAmount + "xp.  " + m_Class.ID + " Total XP: " + m_XP);
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorEarnedAbilityXP(xpAmount, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorGainXP, this)));
			CActorEarnedXP_MessageData message = new CActorEarnedXP_MessageData(this)
			{
				m_xpAmount = xpAmount,
				m_scenarioXP = m_XP
			};
			ScenarioRuleClient.MessageHandler(message);
			if (!string.IsNullOrEmpty(source))
			{
				TotalLoot?.AddXPFromSource(source, xpAmount);
			}
		}
	}

	public void ApplyImmediateDamage(int damage, bool cannotPrevent = false, bool pierceInvulnerable = false, int minHealth = 0, bool firstLoad = false, bool treatAsScenarioRule = false, bool treatAsScenarioEffect = false, bool treatAsEventEffect = false)
	{
		if (firstLoad)
		{
			CScenarioModifierTriggerAbility cScenarioModifierTriggerAbility = new CScenarioModifierTriggerAbility("ScenarioAbility_Damage_Self", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.ELootType.None, CAbility.EAbilityType.None, CAbilityFilter.EFilterRevealedType.None, null, null, null, null, new List<string> { ActorGuid }), isPositive: true, "ScenarioAbility_Damage_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, damage, null, null, null, null, null, isHidden: true);
			cScenarioModifierTriggerAbility.TreatAsEventEffect = treatAsEventEffect;
			cScenarioModifierTriggerAbility.TreatAsScenarioRule = treatAsScenarioRule;
			ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(cScenarioModifierTriggerAbility);
			return;
		}
		int health = Health;
		bool actorWasAsleep = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		GameState.ActorBeenDamaged(this, damage, checkIfPlayerCanAvoidDamage: false, null, null, CAbility.EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: false, cannotPrevent, pierceInvulnerable, 0, triggerOnDamageListeners: true, minHealth);
		CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(this);
		cActorBeenDamaged_MessageData.m_ActorBeingDamaged = this;
		cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
		cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
		cActorBeenDamaged_MessageData.m_TreatAsEventEffect = treatAsEventEffect;
		cActorBeenDamaged_MessageData.m_TreatAsScenarioEffect = treatAsScenarioEffect;
		cActorBeenDamaged_MessageData.m_TreatAsScenarioRule = treatAsScenarioRule;
		ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
	}

	public void ApplyConditions(CActor actorApplying, List<NegativeConditionPair> conditions)
	{
		foreach (NegativeConditionPair condition in conditions)
		{
			ApplyCondition(actorApplying, condition.NegativeCondition);
		}
	}

	public void ApplyConditions(CActor actorApplying, List<CCondition.ENegativeCondition> conditions)
	{
		foreach (CCondition.ENegativeCondition condition in conditions)
		{
			ApplyCondition(actorApplying, condition);
		}
	}

	public void ApplyCondition(CActor actorApplying, CCondition.ENegativeCondition condition, int duration = 0, EConditionDecTrigger decTrigger = EConditionDecTrigger.Never, string animOverload = "", bool isMapCondition = false, bool canGoOverCurseLimit = false, bool isCauseByAnotherCondition = false, int strength = -1, bool tempDisabled = false)
	{
		bool flag = false;
		flag = ((GameState.TurnActor == null) ? (GameState.InternalCurrentActor == this && !GameState.OverridingCurrentActor) : (GameState.TurnActor == this));
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
		{
			if (item.Ability.HasCondition(condition))
			{
				CImmunity_MessageData cImmunity_MessageData = new CImmunity_MessageData(this);
				cImmunity_MessageData.m_ImmunityAbility = item;
				cImmunity_MessageData.negativeCondition = condition;
				ScenarioRuleClient.MessageHandler(cImmunity_MessageData);
				return;
			}
		}
		switch (condition)
		{
		case CCondition.ENegativeCondition.Curse:
		{
			bool conditionAlreadyApplied8 = Tokens.HasKey(CCondition.ENegativeCondition.Curse);
			if (OriginalType == EType.HeroSummon)
			{
				if (this is CHeroSummonActor cHeroSummonActor)
				{
					cHeroSummonActor.Summoner.Class.AddCurseCard(cHeroSummonActor.Summoner, canGoOverCurseLimit);
				}
			}
			else
			{
				Class.AddCurseCard(this, canGoOverCurseLimit);
			}
			CCurse_MessageData message8 = new CCurse_MessageData(animOverload, null)
			{
				m_ActorToCurse = this,
				m_DifficultyModCurse = (duration == int.MaxValue),
				m_ConditionAlreadyApplied = conditionAlreadyApplied8
			};
			ScenarioRuleClient.MessageHandler(message8);
			break;
		}
		case CCondition.ENegativeCondition.Disarm:
		{
			bool conditionAlreadyApplied11 = Tokens.HasKey(CCondition.ENegativeCondition.Disarm);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disarm, duration + 1, decTrigger, this, 1, recall: false, tempDisabled);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disarm, duration, decTrigger, this, 1, recall: false, tempDisabled);
			}
			CDisarm_MessageData message11 = new CDisarm_MessageData(animOverload, null)
			{
				m_ActorToDisarm = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied11
			};
			ScenarioRuleClient.MessageHandler(message11);
			break;
		}
		case CCondition.ENegativeCondition.Immobilize:
		{
			bool conditionAlreadyApplied10 = Tokens.HasKey(CCondition.ENegativeCondition.Immobilize);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Immobilize, duration + 1, decTrigger, this, 1, recall: false, tempDisabled);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Immobilize, duration, decTrigger, this, 1, recall: false, tempDisabled);
			}
			CImmobilize_MessageData message10 = new CImmobilize_MessageData(animOverload, null)
			{
				m_ActorToImmobilize = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied10
			};
			ScenarioRuleClient.MessageHandler(message10);
			break;
		}
		case CCondition.ENegativeCondition.Muddle:
		{
			bool conditionAlreadyApplied4 = Tokens.HasKey(CCondition.ENegativeCondition.Muddle);
			EConditionDecTrigger eConditionDecTrigger = ((decTrigger == EConditionDecTrigger.None) ? EConditionDecTrigger.Turns : decTrigger);
			if (flag && !isMapCondition && eConditionDecTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Muddle, duration + 1, eConditionDecTrigger, this, 1, recall: false, tempDisabled);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Muddle, duration, eConditionDecTrigger, this, 1, recall: false, tempDisabled);
			}
			CMuddle_MessageData message4 = new CMuddle_MessageData(animOverload, null)
			{
				m_ActorToMuddle = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied4
			};
			ScenarioRuleClient.MessageHandler(message4);
			break;
		}
		case CCondition.ENegativeCondition.Poison:
		{
			bool conditionAlreadyApplied3 = Tokens.HasKey(CCondition.ENegativeCondition.Poison);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Poison, int.MaxValue, EConditionDecTrigger.Never, this, 1, recall: false, tempDisabled);
			CPoison_MessageData message3 = new CPoison_MessageData(animOverload, null)
			{
				m_PoisonedActor = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied3
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case CCondition.ENegativeCondition.Stun:
		{
			bool conditionAlreadyApplied9 = Tokens.HasKey(CCondition.ENegativeCondition.Stun);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Stun, duration + 1, decTrigger, this, 1, recall: false, tempDisabled);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Stun, duration, decTrigger, this, 1, recall: false, tempDisabled);
			}
			CStun_MessageData message9 = new CStun_MessageData(animOverload, null)
			{
				m_ActorToStun = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied9
			};
			ScenarioRuleClient.MessageHandler(message9);
			break;
		}
		case CCondition.ENegativeCondition.Wound:
		{
			bool conditionAlreadyApplied2 = Tokens.HasKey(CCondition.ENegativeCondition.Wound);
			if (strength == -1)
			{
				strength = 1;
			}
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Wound, int.MaxValue, EConditionDecTrigger.Never, this, strength, recall: false, tempDisabled);
			CWound_MessageData message2 = new CWound_MessageData(animOverload, null)
			{
				m_ActorToWound = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied2
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case CCondition.ENegativeCondition.StopFlying:
			Tokens.HasKey(CCondition.ENegativeCondition.StopFlying);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.StopFlying, int.MaxValue, EConditionDecTrigger.Never, this, 1, recall: false, tempDisabled);
			break;
		case CCondition.ENegativeCondition.Sleep:
		{
			bool conditionAlreadyApplied5 = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Sleep, int.MaxValue, EConditionDecTrigger.Never, this, 1, recall: false, tempDisabled);
			CSleep_MessageData message5 = new CSleep_MessageData(animOverload, null)
			{
				m_ActorToSleep = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied5
			};
			ScenarioRuleClient.MessageHandler(message5);
			break;
		}
		case CCondition.ENegativeCondition.Bane:
		{
			bool conditionAlreadyApplied7 = Tokens.HasKey(CCondition.ENegativeCondition.Bane);
			if (GameState.TurnActor == this)
			{
				HasBaneThisTurn = true;
			}
			if (strength <= 0)
			{
				strength = 10;
			}
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Bane, int.MaxValue, EConditionDecTrigger.Never, this, strength, recall: false, tempDisabled);
			CBane_MessageData message7 = new CBane_MessageData(animOverload, null)
			{
				m_ActorToBane = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied7
			};
			ScenarioRuleClient.MessageHandler(message7);
			break;
		}
		case CCondition.ENegativeCondition.Brittle:
		{
			bool conditionAlreadyApplied6 = Tokens.HasKey(CCondition.ENegativeCondition.Brittle);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Brittle, int.MaxValue, EConditionDecTrigger.Never, this, 1, recall: false, tempDisabled);
			CBrittle_MessageData message6 = new CBrittle_MessageData(animOverload, null)
			{
				m_Actor = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied6
			};
			ScenarioRuleClient.MessageHandler(message6);
			break;
		}
		case CCondition.ENegativeCondition.Impair:
		{
			bool conditionAlreadyApplied = Tokens.HasKey(CCondition.ENegativeCondition.Impair);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Impair, duration + 1, decTrigger, this, 1, recall: false, tempDisabled);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Impair, duration, decTrigger, this, 1, recall: false, tempDisabled);
			}
			CImpair_MessageData message = new CImpair_MessageData(animOverload, null)
			{
				m_ActorToImpair = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		default:
			Log.ErrorWithReport("Unable to find condition " + condition);
			break;
		}
		if (!isCauseByAnotherCondition)
		{
			CChallenge.CheckTrigger(CChallenge.Trigger.OnGainNegativeCondition, new CChallenge.TriggerParams
			{
				actor = this,
				appliedCondition = condition
			});
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorApplyNegativeCondition, this)));
	}

	public void ActivatePassiveItems(bool firstLoad, PlayerState playerState = null, CItem singleItem = null)
	{
		List<CItem> theseItems = ((singleItem == null) ? Inventory.AllItemsAndPets.ToList() : new List<CItem> { singleItem });
		theseItems.RemoveAll((CItem item) => item.YMLData.Trigger != CItem.EItemTrigger.PassiveEffect || (item.SlotState != CItem.EItemSlotState.Equipped && (singleItem == item || item.YMLData.Usage == CItem.EUsageType.Consumed || item.SlotState != CItem.EItemSlotState.Active || !item.ActiveBonuses.Any((CActiveBonus x) => x.HasTracker && x.TrackerMaxIndex >= 1 && x.TrackerIndex > 0))));
		if (Class is CCharacterClass { ActivatedCards: var activatedCards })
		{
			foreach (CBaseCard item in activatedCards.Where((CBaseCard c) => c.IsItemOrPet && theseItems.Contains(c)))
			{
				foreach (CActiveBonus activeBonuse in item.ActiveBonuses)
				{
					foreach (CBespokeBehaviour bespokeBehaviour in activeBonuse.BespokeBehaviours)
					{
						_ = bespokeBehaviour;
						activeBonuse.BespokeBehaviour?.RemoveListeners();
					}
				}
				item.ActiveBonuses.Clear();
			}
		}
		foreach (CItem item2 in theseItems)
		{
			if (!firstLoad || IsSpecificItemLocked(item2))
			{
				continue;
			}
			item2.ActiveBonuses.Clear();
			if (item2.YMLData.Data.Abilities != null)
			{
				foreach (CAbility ability in item2.YMLData.Data.Abilities)
				{
					CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
					cAbility.Start(this, this);
					cAbility.ApplyToActor(this);
				}
			}
			item2.SlotState = CItem.EItemSlotState.Selected;
			Inventory.HandleActiveItemTriggered(item2);
			if (item2.YMLData.Usage == CItem.EUsageType.Unrestricted && item2.YMLData.UsedWhenEquipped == true)
			{
				UsedItem(item2, firstTimeUse: true);
			}
		}
		playerState?.Save(firstLoad, forceSave: false, (CPlayerActor)this);
	}

	public void ApplyCondition(CActor actorApplying, CCondition.EPositiveCondition condition, int duration = 0, EConditionDecTrigger decTrigger = EConditionDecTrigger.Never, string animOverload = "", bool isMapCondition = false, bool canGoOverBlessLimit = false)
	{
		bool flag = false;
		flag = ((GameState.TurnActor == null) ? (GameState.InternalCurrentActor == this && !GameState.OverridingCurrentActor) : (GameState.TurnActor == this && TurnStarted));
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
		{
			if (item.Ability.HasCondition(condition))
			{
				CImmunity_MessageData cImmunity_MessageData = new CImmunity_MessageData(this);
				cImmunity_MessageData.m_ImmunityAbility = item;
				cImmunity_MessageData.positiveCondition = condition;
				ScenarioRuleClient.MessageHandler(cImmunity_MessageData);
				return;
			}
		}
		switch (condition)
		{
		case CCondition.EPositiveCondition.Bless:
		{
			bool conditionAlreadyApplied4 = Tokens.HasKey(CCondition.EPositiveCondition.Bless);
			if (OriginalType == EType.HeroSummon)
			{
				if (this is CHeroSummonActor cHeroSummonActor)
				{
					cHeroSummonActor.Summoner.Class.AddBlessCard(cHeroSummonActor.Summoner, canGoOverBlessLimit);
				}
			}
			else
			{
				Class.AddBlessCard(this, canGoOverBlessLimit);
			}
			CBless_MessageData message4 = new CBless_MessageData(animOverload, actorApplying)
			{
				m_ActorToBless = this,
				m_DifficultyModBless = (duration == int.MaxValue),
				m_ConditionAlreadyApplied = conditionAlreadyApplied4
			};
			ScenarioRuleClient.MessageHandler(message4);
			break;
		}
		case CCondition.EPositiveCondition.Invisible:
		{
			bool conditionAlreadyApplied5 = Tokens.HasKey(CCondition.EPositiveCondition.Invisible);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Invisible, duration + 1, decTrigger, this, actorApplying);
			}
			else
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Invisible, duration, decTrigger, this, actorApplying);
			}
			CInvisible_MessageData message5 = new CInvisible_MessageData(animOverload, actorApplying)
			{
				m_ActorToMakeInvisible = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied5
			};
			ScenarioRuleClient.MessageHandler(message5);
			break;
		}
		case CCondition.EPositiveCondition.Strengthen:
		{
			bool conditionAlreadyApplied3 = Tokens.HasKey(CCondition.EPositiveCondition.Strengthen);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Strengthen, duration + 1, decTrigger, this, actorApplying);
			}
			else
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Strengthen, duration, decTrigger, this, actorApplying);
			}
			CStrengthen_MessageData message3 = new CStrengthen_MessageData(animOverload, actorApplying)
			{
				m_ActorToStrengthen = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied3
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case CCondition.EPositiveCondition.Immovable:
			Tokens.HasKey(CCondition.EPositiveCondition.Immovable);
			if (flag && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Immovable, duration + 1, decTrigger, this, actorApplying);
			}
			else
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Immovable, duration, decTrigger, this, actorApplying);
			}
			break;
		case CCondition.EPositiveCondition.Regenerate:
		{
			bool conditionAlreadyApplied2 = Tokens.HasKey(CCondition.EPositiveCondition.Regenerate);
			Tokens.AddPositiveToken(CCondition.EPositiveCondition.Regenerate, int.MaxValue, decTrigger, this, actorApplying);
			CRegenerate_MessageData message2 = new CRegenerate_MessageData(animOverload, null)
			{
				m_ActorToRegen = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied2
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case CCondition.EPositiveCondition.Ward:
		{
			bool conditionAlreadyApplied = Tokens.HasKey(CCondition.EPositiveCondition.Ward);
			Tokens.AddPositiveToken(CCondition.EPositiveCondition.Ward, int.MaxValue, EConditionDecTrigger.Never, this, actorApplying);
			CWard_MessageData message = new CWard_MessageData(animOverload, null)
			{
				m_ActorToWard = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		default:
			Log.ErrorWithReport("Unable to find condition " + condition);
			break;
		}
		if (Type == EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid)?.IsSummon;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorApplyPositiveCondition, this)));
	}

	public bool Equals(CActor other)
	{
		if (other == null)
		{
			return false;
		}
		return ActorGuid.Equals(other.ActorGuid);
	}

	public override int GetHashCode()
	{
		return ActorGuid.GetHashCode();
	}

	public static bool IsSameType(CActor first, CActor second)
	{
		return IsSameType(first.Type, second.Type);
	}

	public static bool IsSameType(EType first, EType second)
	{
		return first == second;
	}

	public static List<Point> FindCharacterPath(CActor actor, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, out bool foundPath, bool ignoreDifficultTerrain = false, bool ignoreNegativeTiles = false, int waterTilesThatCostZero = 0, bool carryOtherActors = false, EType carryType = EType.Unknown, bool logFailure = false, int maxMovementPoints = int.MaxValue, bool ignoreCannotStop = false, bool jump = false, bool fly = false, bool ignoreDoorLocks = false)
	{
		EType eType = actor.Type;
		if (carryType != EType.Unknown)
		{
			eType = carryType;
		}
		if (actor.Type == EType.Player && GameState.OverridingCurrentActor && GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().OriginalType == EType.Enemy)
		{
			eType = EType.Enemy;
		}
		if (actor.Type == EType.Player && actor.OverrideType == EType.Enemy)
		{
			eType = EType.Enemy;
		}
		switch (eType)
		{
		case EType.Enemy:
		case EType.Enemy2:
		case EType.Neutral:
			return CEnemyActor.FindEnemyPath(actor, eType, startLocation, endLocation, jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreNegativeTiles, waterTilesThatCostZero, actor.TreatTrapsAsObstacles, actor.MonsterClass?.IsWaterMonster ?? false, out foundPath, logFailure, ignoreCannotStop, fly, ignoreBlocked);
		case EType.Player:
		case EType.HeroSummon:
		case EType.Ally:
			return CPlayerActor.FindPlayerPath(actor, startLocation, endLocation, ignoreBlocked, ignoreMoveCost, out foundPath, avoidHazards: false, ignoreDifficultTerrain, ignoreNegativeTiles, waterTilesThatCostZero, carryOtherActors, carryType, logFailure, assignLevelObjects: true, maxMovementPoints, ignoreCannotStop, jump, fly, ignoreDoorLocks);
		default:
			foundPath = false;
			return new List<Point>();
		}
	}

	public static List<Point> FindStraightPath(CActor actor, Point startLocation, Point endLocation, bool ignoreBlocked, out bool foundPath, IEnumerable<Point> superBlocked = null)
	{
		List<Point> list = null;
		if (!ignoreBlocked)
		{
			list = new List<Point>();
			foreach (CEnemyActor allEnemyMonstersAndObject in ScenarioManager.Scenario.AllEnemyMonstersAndObjects)
			{
				if (allEnemyMonstersAndObject.VisibleBlocksPathing)
				{
					list.Add(allEnemyMonstersAndObject.ArrayIndex);
				}
			}
		}
		List<CObjectObstacle> list2;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list2 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		List<Point> list3 = new List<Point>();
		foreach (CObjectObstacle item in list2)
		{
			foreach (TileIndex pathingBlocker in item.PathingBlockers)
			{
				if (item.IgnoresFlyAndJump)
				{
					list3.Add(new Point(pathingBlocker));
				}
			}
		}
		if (superBlocked != null)
		{
			foreach (Point item2 in superBlocked)
			{
				list3.Add(item2);
			}
		}
		return ScenarioManager.PathFinder.FindPath(actor, startLocation, endLocation, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false, list, list3);
	}

	public static bool AreActorsAllied(EType actor1Type, EType actor2Type, CActor actor1, CActor actor2, bool skipCheckingScenario = false)
	{
		if (!skipCheckingScenario && ScenarioScript.current != null && ScenarioScript.current.UseCustomAreActorsAllied())
		{
			return ScenarioScript.current.CustomAreActorsAllied(actor1Type, actor2Type, actor1, actor2);
		}
		return AreActorTypesAllied(actor1Type, actor2Type);
	}

	public static bool AreActorTypesAllied(EType actor1Type, EType actor2Type)
	{
		if (actor1Type == actor2Type)
		{
			return true;
		}
		switch (actor1Type)
		{
		case EType.Player:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		case EType.Enemy:
			switch (actor2Type)
			{
			case EType.Enemy:
				return true;
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Enemy2:
			case EType.Neutral:
				return false;
			}
			break;
		case EType.HeroSummon:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		case EType.Ally:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		case EType.Enemy2:
			switch (actor2Type)
			{
			case EType.Enemy2:
				return true;
			case EType.Player:
			case EType.Enemy:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return false;
			}
			break;
		case EType.Neutral:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		}
		return false;
	}

	public static bool AreActorsEnemies(EType actor1Type, EType actor2Type, CActor actor1, CActor actor2, bool skipCheckingScenario = false)
	{
		if (!skipCheckingScenario && ScenarioScript.current != null && ScenarioScript.current.UseCustomAreActorsEnemies())
		{
			return ScenarioScript.current.CustomAreActorsEnemies(actor1Type, actor2Type, actor1, actor2);
		}
		return AreActorsTypesEnemies(actor1Type, actor2Type);
	}

	public static bool AreActorsTypesEnemies(EType actor1Type, EType actor2Type)
	{
		if (actor1Type == actor2Type)
		{
			return false;
		}
		switch (actor1Type)
		{
		case EType.Player:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return false;
			case EType.Enemy:
			case EType.Enemy2:
				return true;
			}
			break;
		case EType.Enemy:
			switch (actor2Type)
			{
			case EType.Enemy:
				return false;
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Enemy2:
			case EType.Neutral:
				return true;
			}
			break;
		case EType.HeroSummon:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return false;
			case EType.Enemy:
			case EType.Enemy2:
				return true;
			}
			break;
		case EType.Ally:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return false;
			case EType.Enemy:
			case EType.Enemy2:
				return true;
			}
			break;
		case EType.Enemy2:
			switch (actor2Type)
			{
			case EType.Enemy2:
				return false;
			case EType.Player:
			case EType.Enemy:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			}
			break;
		case EType.Neutral:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return false;
			case EType.Enemy:
			case EType.Enemy2:
				return true;
			}
			break;
		}
		return false;
	}

	public List<CTile> GetTilesCanAttackFrom()
	{
		if (this is CObjectActor { AttachedProp: CObjectObstacle { PathingBlockers: var pathingBlockers } })
		{
			return pathingBlockers.Select((TileIndex x) => ScenarioManager.GetTile(x.ToPoint())).ToList();
		}
		if (m_MultiHexArrayIndexes != null)
		{
			return MultiHexTiles;
		}
		return new List<CTile> { Tile };
	}

	public bool IsAdjacentTo(CActor target)
	{
		if (target == null)
		{
			return false;
		}
		bool flag = ScenarioManager.IsTileAdjacent(ArrayIndex.X, ArrayIndex.Y, target.ArrayIndex.X, target.ArrayIndex.Y);
		if (flag)
		{
			return flag;
		}
		List<TileIndex> list = ((!(this is CObjectActor { AttachedProp: CObjectObstacle attachedProp })) ? new List<TileIndex>
		{
			new TileIndex(ArrayIndex)
		} : attachedProp.PathingBlockers);
		List<TileIndex> list2 = ((!(target is CObjectActor { AttachedProp: CObjectObstacle attachedProp2 })) ? new List<TileIndex>
		{
			new TileIndex(target.ArrayIndex)
		} : attachedProp2.PathingBlockers);
		foreach (TileIndex item in list)
		{
			foreach (TileIndex item2 in list2)
			{
				if (ScenarioManager.IsTileAdjacent(item, item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAdjacentTo(CObjectProp target)
	{
		if (target == null)
		{
			return false;
		}
		return ScenarioManager.IsTileAdjacent(ArrayIndex.X, ArrayIndex.Y, target.ArrayIndex.X, target.ArrayIndex.Y);
	}

	public bool IsInRangeOf(CActor target, int range)
	{
		if (target == null)
		{
			return false;
		}
		return Point.HexDist(ArrayIndex, target.ArrayIndex) <= range;
	}

	public int GetDistanceTo(CActor otherActor, bool viaPathfinding = false)
	{
		if (otherActor == null)
		{
			return -1;
		}
		return GetDistanceTo(otherActor.ArrayIndex, viaPathfinding);
	}

	public int GetDistanceTo(CTile tile, bool viaPathfinding = false)
	{
		if (tile == null)
		{
			return -1;
		}
		return GetDistanceTo(tile.m_ArrayIndex, viaPathfinding);
	}

	public int GetDistanceTo(Point point, bool viaPathfinding = false)
	{
		if (MultiHexArrayIndexes != null && MultiHexArrayIndexes.Count > 0)
		{
			int num = int.MaxValue;
			{
				foreach (Point multiHexArrayIndex in MultiHexArrayIndexes)
				{
					int distanceTo = GetDistanceTo(multiHexArrayIndex, point, viaPathfinding);
					if (distanceTo < num)
					{
						num = distanceTo;
					}
				}
				return num;
			}
		}
		return GetDistanceTo(ArrayIndex, point, viaPathfinding);
	}

	public static int GetDistanceTo(Point currentActorPoint, Point point, bool viaPathfinding = false)
	{
		if (viaPathfinding)
		{
			List<Point> list = ScenarioManager.PathFinder.FindPath(null, currentActorPoint, point, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath, ignoreDoorLocks: false, null, null, null, assignLevelObjects: false, int.MaxValue, jump: false, isCalculatingDistance: true);
			if (foundPath)
			{
				return list.Count;
			}
		}
		return Point.HexDist(currentActorPoint, point);
	}

	public List<CTile> TilesInRange(int range)
	{
		List<CTile> list = new List<CTile>();
		foreach (CTile item in GetTilesCanAttackFrom())
		{
			list.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, range));
		}
		return list.Distinct().ToList();
	}

	public List<CTile> ValidTilesInRange(int range)
	{
		return GameState.GetTilesInRange(ArrayIndex, range).Where((CTile tile) =>
		{
			CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
			return node != null && !node.Blocked && node.Walkable && !node.SuperBlocked;
		}).ToList();
	}

	public static bool ImmuneToAllConditionsCheck(List<CAbility.EAbilityType> immunities)
	{
		bool result = true;
		foreach (CAbility.EAbilityType immunityConditionAbilityType in ImmunityConditionAbilityTypes)
		{
			if (immunityConditionAbilityType != CAbility.EAbilityType.Sleep && !immunities.Contains(immunityConditionAbilityType))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool TreatFirstAttackModifierAsRolling()
	{
		return FindApplicableActiveBonuses().Any((CActiveBonus x) => x.Ability.GetMiscVar("TreatFirstAttackModifierAsRolling").GetBool());
	}

	public bool IgnoreNegativeConditions()
	{
		return FindApplicableActiveBonuses().Any((CActiveBonus x) => x.Ability.GetMiscVar("IgnoreNegativeConditions").GetBool() && x.RequirementsMet(this) && x.IsActiveBonusToggledAndNotRestricted(this));
	}

	public static List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, out bool shuffle, out List<AttackModifierYMLData> notUsed, CAbilityAttack attackAbility, CActor currentTarget, out List<AttackModifierYMLData> cardsDrawnTotal, CAttackSummary.TargetSummary targetSummary)
	{
		notUsed = new List<AttackModifierYMLData>();
		List<AttackModifierYMLData> list = new List<AttackModifierYMLData>();
		List<AttackModifierYMLData> list2 = (cardsDrawnTotal = new List<AttackModifierYMLData>());
		AttackModifierYMLData attackModifierYMLData = null;
		List<AttackModifierYMLData> list3 = new List<AttackModifierYMLData>();
		while ((attackModifierYMLData == null || attackModifierYMLData.Rolling) && list.Count < 200)
		{
			attackModifierYMLData = DrawAttackModifierCard(actor, attackAbility, currentTarget, advStatus, current, discarded, list2, list2);
			if (attackModifierYMLData.Rolling)
			{
				list3.Add(attackModifierYMLData);
			}
		}
		switch (advStatus)
		{
		case EAdvantageStatuses.Advantage:
		{
			AttackModifierYMLData attackModifierYMLData2 = DrawAttackModifierCard(actor, attackAbility, currentTarget, advStatus, current, discarded, list, list2);
			AttackModifierYMLData attackModifierYMLData3 = ChooseCardBasedOnAdvantage(attackStrength, attackModifierYMLData, attackModifierYMLData2, advStatus, actor);
			if (GameState.ChooseAdvantageModifiers && attackModifierYMLData.ID != attackModifierYMLData2.ID && !actor.MindControlIsFromEnemy)
			{
				GameState.ShowChooseAttackModifier(actor, new List<AttackModifierYMLData> { attackModifierYMLData, attackModifierYMLData2 }, attackModifierYMLData3, out var selectedLootCard);
				if (selectedLootCard != null)
				{
					attackModifierYMLData3 = selectedLootCard;
				}
			}
			list.AddRange(list3);
			list.Add(attackModifierYMLData3);
			break;
		}
		case EAdvantageStatuses.Disadvantage:
		{
			AttackModifierYMLData second = DrawAttackModifierCard(actor, attackAbility, currentTarget, advStatus, current, discarded, list, list2);
			AttackModifierYMLData item = ChooseCardBasedOnAdvantage(attackStrength, attackModifierYMLData, second, advStatus, actor);
			list.Add(item);
			break;
		}
		case EAdvantageStatuses.None:
			list.AddRange(list3);
			list.Add(attackModifierYMLData);
			break;
		}
		notUsed = list2.Except(list).ToList();
		shuffle = list2.Any((AttackModifierYMLData a) => a.Shuffle);
		targetSummary.FinishDrawingModifiers();
		return list;
	}

	private static AttackModifierYMLData DrawAttackModifierCard(CActor actor, CAbilityAttack attackAbility, CActor currentTarget, EAdvantageStatuses advStatus, List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, List<AttackModifierYMLData> cardsToUse, List<AttackModifierYMLData> cardsDrawn)
	{
		if (current.Count == 0 && ((actor is CPlayerActor && GameState.ShuffleAttackModsEnabledForPlayers) || (actor is CEnemyActor && GameState.ShuffleAttackModsEnabledForMonsters) || (actor is CHeroSummonActor && GameState.ShuffleAttackModsEnabledForPlayers)))
		{
			ShuffleAttackModifierCards(current, discarded, cardsToUse);
		}
		AttackModifierYMLData attackModifierYMLData = current[0];
		AttackModifierYMLData attackModifierYMLData2 = attackModifierYMLData;
		GameState.CacheCurrentModifierChoice(attackModifierYMLData);
		StartedDrawingModifier();
		if (attackModifierYMLData.IsCurse)
		{
			Log.Info("A Curse card has been drawn by " + actor.Class.ID + " and removed from their attack modifier deck");
		}
		else if (attackModifierYMLData.IsBless)
		{
			Log.Info("A Bless card has been drawn by " + actor.Class.ID + " and removed from their attack modifier deck");
		}
		else if (attackModifierYMLData.IsPersistent)
		{
			Log.Info("A Persistent active bonus card has been drawn by " + actor.Class.ID + " and removed from their attack modifier deck");
		}
		else
		{
			discarded.Add(attackModifierYMLData);
		}
		current.Remove(attackModifierYMLData);
		int count = cardsDrawn.Count;
		cardsDrawn.Add(attackModifierYMLData);
		List<AttackModifierYMLData> list = new List<AttackModifierYMLData> { attackModifierYMLData };
		GameState.ChangeModifierValues(actor, list, attackAbility, currentTarget, advStatus);
		attackModifierYMLData = list[0];
		actor.SetTempVar("ModifiedAttackModifier", TValue.Unknown);
		CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericOnDrawAttackModifier, attackAbility, actor, actor, ("attackModifier", new TValue(attackModifierYMLData)));
		TValue var = actor.GetVar("ModifiedAttackModifier");
		if (!var.isUnknown)
		{
			attackModifierYMLData = var.GetObject<AttackModifierYMLData>();
		}
		if (count == 0 && !attackModifierYMLData.Rolling && actor.TreatFirstAttackModifierAsRolling())
		{
			attackModifierYMLData = CustomAbility.CreateRollingAttackModifierCopy(attackModifierYMLData);
		}
		cardsDrawn.RemoveAt(count);
		cardsDrawn.Insert(count, attackModifierYMLData);
		foreach (CAbilityAttack.AttackModifierAbilityInfo modifierAbility in attackAbility.ModifierAbilities)
		{
			if (modifierAbility.Card == attackModifierYMLData2)
			{
				modifierAbility.Card = attackModifierYMLData;
			}
		}
		attackModifierYMLData.FinishedBeingDrawn = true;
		GameState.CacheCurrentModifierChoice(null);
		FinishdDrawingModifier();
		return attackModifierYMLData;
	}

	public static void StartedDrawingModifier()
	{
		CStartedDrawingModifier_MessageData message = new CStartedDrawingModifier_MessageData(GameState.CurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void FinishdDrawingModifier()
	{
		CFinishedDrawingModifier_MessageData message = new CFinishedDrawingModifier_MessageData(GameState.CurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void ShuffleAttackModifierCards(List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, List<AttackModifierYMLData> inUse = null)
	{
		bool flag = false;
		if (inUse != null && discarded.Count > inUse.Count)
		{
			discarded.RemoveAll((AttackModifierYMLData card) => inUse.Contains(card));
			flag = true;
		}
		while (discarded.Count > 0)
		{
			AttackModifierYMLData item = discarded[0];
			discarded.Remove(item);
			current.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, current.Count), item);
		}
		current.Shuffle();
		if (flag)
		{
			discarded.AddRange(inUse);
		}
	}

	public static AttackModifierYMLData ChooseCardBasedOnAdvantage(int attackStrength, AttackModifierYMLData first, AttackModifierYMLData second, EAdvantageStatuses advantage, CActor actor)
	{
		int num = Convert.ToInt32(GameState.s_MathParser.Parse("100" + first.MathModifier, isRadians: false));
		int num2 = Convert.ToInt32(GameState.s_MathParser.Parse(attackStrength + first.MathModifier, isRadians: false));
		int num3 = Convert.ToInt32(GameState.s_MathParser.Parse("100" + second.MathModifier, isRadians: false));
		int num4 = Convert.ToInt32(GameState.s_MathParser.Parse(attackStrength + second.MathModifier, isRadians: false));
		switch (advantage)
		{
		case EAdvantageStatuses.Advantage:
			if (num2 == num4)
			{
				if (first.Card.HasNegativeEffect && second.Card.HasNegativeEffect)
				{
					return first;
				}
				if (first.Card.HasNegativeEffect)
				{
					return second;
				}
				if (second.Card.HasNegativeEffect)
				{
					return first;
				}
				if (first.Card.HasAdditionalEffect && second.Card.HasAdditionalEffect)
				{
					return first;
				}
				if (first.Card.HasAdditionalEffect)
				{
					return first;
				}
				if (second.Card.HasAdditionalEffect)
				{
					return second;
				}
				if (num == num3)
				{
					return first;
				}
				if (num > num3)
				{
					return first;
				}
				return second;
			}
			if (num2 > num4)
			{
				return first;
			}
			return second;
		case EAdvantageStatuses.Disadvantage:
			if ((num2 < num4 && num3 != 0) || num == 0)
			{
				return first;
			}
			if (num2 == num4)
			{
				if (first.Card.HasNegativeEffect && second.Card.HasNegativeEffect)
				{
					return first;
				}
				if (first.Card.HasNegativeEffect)
				{
					return first;
				}
				if (second.Card.HasNegativeEffect)
				{
					return second;
				}
				if (first.Card.HasAdditionalEffect && second.Card.HasAdditionalEffect)
				{
					return first;
				}
				if (first.Card.HasAdditionalEffect)
				{
					return second;
				}
				_ = second.Card.HasAdditionalEffect;
				return first;
			}
			return second;
		default:
			Log.ErrorWithReport("Invalid advantage status sent to ChooseCardBasedOnAdvantage");
			return first;
		}
	}

	public void ClearTempVars()
	{
		m_TempVars.Clear();
	}

	public TValue GetVar(string key, CVarsContext context = null, bool asValue = true)
	{
		TValue var = m_TempVars.GetVar(key, context, asValue);
		if (!var.isUnknown)
		{
			return var;
		}
		return TValue.Unknown;
	}

	public TValue SetTempVar(string key, TValue value)
	{
		m_TempVars.Set(key, value);
		return value;
	}

	public int GetSingleUsePreventDamageCount()
	{
		int num = 0;
		List<CActiveBonus> list = FindPreventDamageBonuses();
		if (list != null)
		{
			foreach (CActiveBonus item in list)
			{
				AbilityData.ActiveBonusData activeBonusData = item.Ability.ActiveBonusData;
				if (activeBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent && activeBonusData.Tracker.Count == 1)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void SetDowned(bool downed, bool initial = false)
	{
		if (downed == IsDowned)
		{
			return;
		}
		IsDowned = downed;
		if (downed)
		{
			Health = 0;
			CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(this, this, Health);
			ScenarioRuleClient.MessageHandler(message);
			foreach (NegativeConditionPair checkNegativeToken in Tokens.CheckNegativeTokens)
			{
				Tokens.RemoveNegativeToken(checkNegativeToken.NegativeCondition);
			}
			foreach (PositiveConditionPair checkPositiveToken in Tokens.CheckPositiveTokens)
			{
				Tokens.RemovePositiveToken(checkPositiveToken.PositiveCondition);
			}
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Stun, int.MaxValue, EConditionDecTrigger.Never, this);
			ScenarioRuleClient.MessageHandler(new CActorDowned_MessageData(this));
		}
		else
		{
			Health = 1;
			Tokens.RemoveNegativeToken(CCondition.ENegativeCondition.Stun, EConditionDecTrigger.Never);
			CRefreshActorHealth_MessageData message2 = new CRefreshActorHealth_MessageData(this, this, Health);
			ScenarioRuleClient.MessageHandler(message2);
			ScenarioRuleClient.MessageHandler(new CDownedActorRevived_MessageData(this));
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		if (!initial)
		{
			ScenarioScript.current?.OnActorDownedChanged(this);
		}
	}

	public bool IsUnderwater()
	{
		return ScenarioScript.current?.IsUnderwater(Tile) ?? false;
	}

	public virtual bool CanFitIntoHex(Point targetPos, int angle)
	{
		return true;
	}

	public bool IsOnHazardousTerrain()
	{
		return ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y].m_Props.Find((CObjectProp p) => p is CObjectHazardousTerrain) != null;
	}

	public void PlayVFX(string VFXPropType)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = VFXPropType,
			m_ArrayIndex = m_ArrayIndex
		});
	}

	public void PlayVFXTowards(CActor towards, string VFXPropType)
	{
		PlayVFXTowards(new TileIndex(towards.m_ArrayIndex), VFXPropType);
	}

	public void PlayVFXTowards(TileIndex faceTowards, string VFXPropType)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = VFXPropType,
			m_ArrayIndex = m_ArrayIndex,
			m_faceTowards = faceTowards.ToPoint()
		});
	}

	public bool HasEnhancedSteps()
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(this);
		if (list == null || list.Count == 0)
		{
			return false;
		}
		foreach (CActiveBonus item in list)
		{
			if ((item.ReferenceFlags(item.Ability, this) & AbilityData.EActiveBonusFlag.EnhancedMoveEffects) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanActivatePressurePlates()
	{
		if (Class is CCharacterClass)
		{
			return true;
		}
		if (Class is CMonsterClass { CanActivatePressurePlates: not false })
		{
			return true;
		}
		return false;
	}

	public bool HasFreeTileAwayFromCaster(out Point? freeHexesAwayFromThisTile)
	{
		freeHexesAwayFromThisTile = null;
		CActiveBonus cActiveBonus = FindApplicableActiveBonuses(CAbility.EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffMove).FirstOrDefault((CActiveBonus x) => x.Ability.ActiveBonusData.GetMiscVar("FreeHexesAwayFromCaster").GetBool());
		if (cActiveBonus != null)
		{
			freeHexesAwayFromThisTile = cActiveBonus.Caster.ArrayIndex;
			return true;
		}
		return false;
	}

	public int GetInitiativeBonus()
	{
		int num = (Class.TempInitiativeBonus.HasValue ? Class.TempInitiativeBonus.Value : 0);
		foreach (CActiveBonus item in FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative))
		{
			CAdjustInitiativeActiveBonus_AdjustInitiative behaviour = item.GetBehaviour<CAdjustInitiativeActiveBonus_AdjustInitiative>();
			if (!item.Ability.ActiveBonusData.IsToggleBonus)
			{
				num += behaviour.Strength;
			}
		}
		return num;
	}

	public virtual bool CanInfuse(ElementInfusionBoardManager.EElement element, bool endOfMonsterSet = false)
	{
		return true;
	}

	public virtual bool CanConsume(ElementInfusionBoardManager.EElement element, bool endOfMonsterSet = false)
	{
		return true;
	}

	public virtual void OnDeathAbilityComplete()
	{
	}

	public void EndMindControl()
	{
		MindControlDuration = CAbilityControlActor.EControlDurationType.None;
		MindControlIsFromEnemy = false;
		MindControlAbility = null;
		CActorIsControlled_MessageData message = new CActorIsControlled_MessageData(this)
		{
			m_ControlActorAbility = null,
			m_ControlledActor = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public List<CActiveBonus> FindPreventDamageBonuses()
	{
		return (from x in FindApplicableActiveBonuses(CAbility.EAbilityType.PreventDamage)
			where !x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate)
			select x).ToList();
	}

	public bool HasMirrorActor()
	{
		foreach (CActiveBonus item in ScenarioManager.Scenario.AllAliveActors.SelectMany((CActor x) => x.FindApplicableActiveBonuses(CAbility.EAbilityType.MirrorDamage, CActiveBonus.EActiveBonusBehaviourType.MirrorAbilities)).ToList())
		{
			CMirrorDamageActiveBonus_MirrorAbilities behaviour = item.GetBehaviour<CMirrorDamageActiveBonus_MirrorAbilities>();
			if ((behaviour != null && behaviour.MirrorTarget == this) || behaviour.ActiveBonus.Actor == this)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearMirrorCache()
	{
		m_mirrorAbilitiesSourceCache = null;
		m_mirrorAbilitiesCloneCache = null;
		m_mirrorHealthSourceCache = null;
		m_mirrorHealthCloneCache = null;
	}

	public bool IsMirrorActor()
	{
		CActor mirrorSource;
		return IsMirrorActor(out mirrorSource);
	}

	public bool IsMirrorActor(out CActor mirrorSource, bool useCacheOnly = false)
	{
		if (useCacheOnly)
		{
			mirrorSource = m_mirrorHealthSourceCache;
			if ((mirrorSource is CPlayerActor || (mirrorSource?.MonsterClass?.CardsPoolClass?.AbilityCardsPool != null && mirrorSource.MonsterClass.CardsPoolClass.AbilityCardsPool.Count > 0)) && MonsterClass != null && MonsterClass.CardsPoolClass.AbilityCardsPool.Count == 0)
			{
				return true;
			}
			mirrorSource = m_mirrorAbilitiesSourceCache;
			if (mirrorSource != null)
			{
				return mirrorSource.m_mirrorAbilitiesSourceCache == null;
			}
			return false;
		}
		CActor mirrorHealthActor = GetMirrorHealthActor();
		mirrorSource = mirrorHealthActor;
		if ((mirrorSource is CPlayerActor || (mirrorSource?.MonsterClass?.CardsPoolClass?.AbilityCardsPool != null && mirrorSource.MonsterClass.CardsPoolClass.AbilityCardsPool.Count > 0)) && MonsterClass != null && MonsterClass.CardsPoolClass.AbilityCardsPool.Count == 0)
		{
			return true;
		}
		CActor cActor = (mirrorSource = GetMirrorAbilitiesActor());
		if (cActor != null)
		{
			return cActor.GetMirrorAbilitiesActor() == null;
		}
		return false;
	}

	public CActor GetMirrorAbilitiesActor(bool includeMirroredClone = false)
	{
		return GetMirrorActor(includeMirroredClone, checkMirrorAbilities: true, checkMirrorAll: false);
	}

	public void CacheMirrorProperties()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			ClearMirrorCache();
			foreach (CActiveBonus item in ScenarioManager.Scenario.AllAliveActorsThreadSafe.SelectMany((CActor x) => from y in x.FindApplicableActiveBonuses(CAbility.EAbilityType.MirrorDamage)
				where y.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.MirrorAbilities, CActiveBonus.EActiveBonusBehaviourType.MirrorAll)
				select y).ToList())
			{
				CMirrorDamageActiveBonus_MirrorAbilities behaviour = item.GetBehaviour<CMirrorDamageActiveBonus_MirrorAbilities>();
				if (behaviour != null)
				{
					if (behaviour.MirrorTarget == this)
					{
						m_mirrorAbilitiesSourceCache = behaviour.ActiveBonus.Actor;
					}
					if (behaviour.ActiveBonus.Actor == this)
					{
						m_mirrorAbilitiesCloneCache = behaviour.MirrorTarget;
					}
					continue;
				}
				CMirrorDamageActiveBonus_MirrorAll behaviour2 = item.GetBehaviour<CMirrorDamageActiveBonus_MirrorAll>();
				if (behaviour2 != null)
				{
					if (behaviour2.MirrorTarget == this)
					{
						m_mirrorHealthSourceCache = behaviour2.ActiveBonus.Actor;
					}
					if (behaviour2.ActiveBonus.Actor == this)
					{
						m_mirrorHealthCloneCache = behaviour2.MirrorTarget;
					}
				}
			}
		}
	}

	public CActor GetMirrorActor(bool includeMirroredClone = false, bool checkMirrorAbilities = true, bool checkMirrorAll = true)
	{
		if ((includeMirroredClone && m_mirrorAbilitiesCloneCache != null) & checkMirrorAbilities)
		{
			return m_mirrorAbilitiesCloneCache;
		}
		if ((includeMirroredClone && m_mirrorHealthCloneCache != null) & checkMirrorAll)
		{
			return m_mirrorHealthCloneCache;
		}
		if ((m_mirrorAbilitiesSourceCache != null) & checkMirrorAbilities)
		{
			return m_mirrorAbilitiesSourceCache;
		}
		if ((m_mirrorHealthSourceCache != null) & checkMirrorAll)
		{
			return m_mirrorHealthSourceCache;
		}
		return null;
	}

	public CActor GetClonedFromActor()
	{
		if (string.IsNullOrEmpty(OverrideCharacterModel))
		{
			return null;
		}
		return GetMirrorActor();
	}

	public CActor GetMirrorHealthActor()
	{
		return GetMirrorActor(includeMirroredClone: false, checkMirrorAbilities: false);
	}

	public CActor GetMirrorHealthActor(out CMirrorDamageActiveBonus_MirrorAll behaviour)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			behaviour = null;
			foreach (CActiveBonus item in ScenarioManager.Scenario.AllAliveActors.SelectMany((CActor x) => x.FindApplicableActiveBonuses(CAbility.EAbilityType.MirrorDamage, CActiveBonus.EActiveBonusBehaviourType.MirrorAll)).ToList())
			{
				CMirrorDamageActiveBonus_MirrorAll behaviour2 = item.GetBehaviour<CMirrorDamageActiveBonus_MirrorAll>();
				if (behaviour2 != null && behaviour2.MirrorTarget == this)
				{
					behaviour = behaviour2;
					return behaviour2.ActiveBonus.Actor;
				}
			}
			return null;
		}
	}

	public IEnumerable<CActiveBonus> GetUnrestrictedInfusionBonuses()
	{
		return from x in FindApplicableActiveBonuses()
			where x.Ability.GetMiscVar("Infusion").GetBool() && x.RequirementsMet(this) && !x.IsRestricted(this)
			select x;
	}

	public void RecalcLockedInitiative()
	{
		InitiativeLockedDuringRound = null;
		InitiativeLockedDuringRound = Initiative();
	}

	public bool ForcedCannotAttack(out CActiveBonus bonus)
	{
		bonus = FindApplicableActiveBonuses().FirstOrDefault((CActiveBonus x) => x.Ability.GetMiscVar("ForcedCannotAttack").GetBool());
		return bonus != null;
	}

	public static CActorStatic.CTargetPath CalculateTargetActorPath(CActor actor, Point actorHexArrayIndex, List<CActor> targetActors, List<CActor> blockingActors, CActor targetActor, Point targetHexArrayIndex, Point sampleHexArrayIndex, bool trapsBlock, List<Point> trapsToBlock, bool alliesBlock, List<Point> alliesToBlock, bool isWaterMonster, bool isPlayer, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool excludeDestinationInPath, bool targetActorShouldBlock, int maxMoveCount, int range, ref bool foundPath, bool obstaclesBlock, bool hazardTerrainBlock, bool shouldPathThroughDoors, bool openDoorwaysBlock)
	{
		List<Point> list = new List<Point>();
		List<Point> list2 = new List<Point>();
		if (blockingActors != null)
		{
			foreach (CActor blockingActor in blockingActors)
			{
				if (blockingActor != null && !blockingActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) && ((blockingActor != targetActor) | targetActorShouldBlock))
				{
					list.Add(blockingActor.ArrayIndex);
				}
			}
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (prop.NegativeHex && (prop.ArrayIndex.X != sampleHexArrayIndex.X || prop.ArrayIndex.Y != sampleHexArrayIndex.Y))
			{
				list.Add(new Point(prop.ArrayIndex));
			}
		}
		if (trapsBlock)
		{
			foreach (CObjectTrap item2 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>())
			{
				if ((!item2.Activated && item2.ArrayIndex.X != sampleHexArrayIndex.X) || item2.ArrayIndex.Y != sampleHexArrayIndex.Y)
				{
					list.Add(new Point(item2.ArrayIndex));
				}
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				if (heroSummon.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					list.Add(heroSummon.m_ArrayIndex);
				}
			}
		}
		else if (trapsToBlock != null)
		{
			foreach (Point item3 in trapsToBlock)
			{
				if (item3.X != sampleHexArrayIndex.X || item3.Y != sampleHexArrayIndex.Y)
				{
					list.Add(item3);
				}
			}
		}
		if (hazardTerrainBlock)
		{
			foreach (CObjectHazardousTerrain item4 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>())
			{
				if (item4.ArrayIndex.X != sampleHexArrayIndex.X || item4.ArrayIndex.Y != sampleHexArrayIndex.Y)
				{
					list.Add(new Point(item4.ArrayIndex));
				}
			}
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
			foreach (CObjectDifficultTerrain item5 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>())
			{
				if (item5.TreatAsTrap && item5.TreatAsTrapFilter.IsValidTarget(actorState) && (item5.ArrayIndex.X != sampleHexArrayIndex.X || item5.ArrayIndex.Y != sampleHexArrayIndex.Y))
				{
					list.Add(new Point(item5.ArrayIndex));
				}
			}
		}
		if (alliesBlock)
		{
			if (isPlayer)
			{
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					list.Add(playerActor.ArrayIndex);
				}
				foreach (CHeroSummonActor heroSummon2 in ScenarioManager.Scenario.HeroSummons)
				{
					list.Add(heroSummon2.ArrayIndex);
				}
				foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
				{
					list.Add(allyMonster.ArrayIndex);
				}
				foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
				{
					list.Add(neutralMonster.ArrayIndex);
				}
			}
			else if (actor.Type == EType.Enemy)
			{
				foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
				{
					list.Add(enemy.ArrayIndex);
				}
			}
			else if (actor.Type == EType.Enemy2)
			{
				foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
				{
					list.Add(enemy2Monster.ArrayIndex);
				}
			}
		}
		else if (alliesToBlock != null)
		{
			foreach (Point item6 in alliesToBlock)
			{
				if (item6.X != actor.ArrayIndex.X || item6.Y != actor.ArrayIndex.Y)
				{
					list.Add(item6);
				}
			}
		}
		if (!obstaclesBlock)
		{
			for (int num = 0; num < ScenarioManager.Height; num++)
			{
				for (int num2 = 0; num2 < ScenarioManager.Width; num2++)
				{
					if (ScenarioManager.Tiles[num2, num] == null)
					{
						continue;
					}
					List<CObjectObstacle> list3;
					lock (ScenarioManager.CurrentScenarioState.Props)
					{
						list3 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
					}
					foreach (CObjectObstacle item7 in list3)
					{
						foreach (TileIndex point in item7.PathingBlockers)
						{
							list.RemoveAll((Point it) => it.Equals(new Point(point)));
						}
					}
				}
			}
		}
		else
		{
			List<CObjectObstacle> list4;
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				list4 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
			}
			foreach (CObjectObstacle item8 in list4)
			{
				foreach (TileIndex pathingBlocker in item8.PathingBlockers)
				{
					list.Add(new Point(pathingBlocker));
					if (item8.IgnoresFlyAndJump)
					{
						list2.Add(new Point(pathingBlocker));
					}
				}
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				if (!@object.IsAttachedToProp || !(@object.AttachedProp is CObjectObstacle))
				{
					list.Add(@object.ArrayIndex);
				}
			}
		}
		List<CObjectObstacle> list5;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list5 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		foreach (CObjectObstacle item9 in list5)
		{
			foreach (TileIndex pathingBlocker2 in item9.PathingBlockers)
			{
				if (item9.IgnoresFlyAndJump)
				{
					list2.Add(new Point(pathingBlocker2));
				}
			}
		}
		if (shouldPathThroughDoors | openDoorwaysBlock)
		{
			for (int num3 = 0; num3 < ScenarioManager.Height; num3++)
			{
				for (int num4 = 0; num4 < ScenarioManager.Width; num4++)
				{
					if (ScenarioManager.Tiles[num4, num3] == null)
					{
						continue;
					}
					List<CObjectDoor> list6;
					lock (ScenarioManager.CurrentScenarioState.Props)
					{
						list6 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>().ToList();
					}
					if (openDoorwaysBlock)
					{
						foreach (CObjectDoor item10 in list6)
						{
							Point item = new Point(item10.ArrayIndex);
							if (!list.Contains(item))
							{
								list.Add(item);
							}
						}
					}
					else
					{
						if (!shouldPathThroughDoors)
						{
							continue;
						}
						foreach (CObjectDoor door in list6)
						{
							if (!door.DoorIsOpen)
							{
								list.RemoveAll((Point it) => it.Equals(new Point(door.ArrayIndex)));
							}
						}
					}
				}
			}
		}
		if (isWaterMonster)
		{
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile in tiles)
			{
				if (cTile != null && !cTile.IsWater)
				{
					list2.Add(cTile.m_ArrayIndex);
				}
			}
		}
		foundPath = false;
		CActorStatic.CTargetPath cTargetPath = new CActorStatic.CTargetPath();
		cTargetPath.m_Target = targetActor;
		cTargetPath.m_TargetTile = targetHexArrayIndex;
		if (actorHexArrayIndex.X != sampleHexArrayIndex.X || actorHexArrayIndex.Y != sampleHexArrayIndex.Y)
		{
			List<Point> list7 = CAbilityMove.FindPathAndWaypoints(actor, actorHexArrayIndex, ScenarioManager.Tiles[sampleHexArrayIndex.X, sampleHexArrayIndex.Y], out cTargetPath.m_Waypoints, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, excludeDestinationInPath, ignoreMoveCost: false, out foundPath, shouldPathThroughDoors, list, list2);
			if (foundPath && ScenarioManager.PathFinder.GetNode(list7[list7.Count - 1]).IsIceTerrain)
			{
				bool flag = (fly | jump) || CActorStatic.IsIceTerrainSlideInterrupted(actor, list7);
				foundPath &= flag;
			}
			if (foundPath)
			{
				cTargetPath.m_ArrayIndices = list7;
			}
		}
		else
		{
			foundPath = true;
		}
		cTargetPath.m_ArrayIndicesBeforeCull = new List<Point>(cTargetPath.m_ArrayIndices);
		cTargetPath.m_TargetPathID = s_TargetPathID++;
		cTargetPath.m_PathEndTile = sampleHexArrayIndex;
		cTargetPath.m_TrapsInPath = new List<Point>();
		if (!trapsBlock)
		{
			List<CObjectTrap> list8 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>().ToList();
			foreach (Point point2 in cTargetPath.m_ArrayIndices)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(point2);
				bool flag2 = false;
				if (cActor is CHeroSummonActor cHeroSummonActor && !AreActorsAllied(actor.Type, cActor.Type, actor, cActor) && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					flag2 = true;
				}
				if ((actorHexArrayIndex.X != point2.X || actorHexArrayIndex.Y != point2.Y) && ((list8.Find((CObjectTrap x) => x.ArrayIndex.X == point2.X && x.ArrayIndex.Y == point2.Y) != null) | flag2) && ((!fly && !jump) || ((!fly & jump) && point2.X == sampleHexArrayIndex.X && point2.Y == sampleHexArrayIndex.Y)))
				{
					cTargetPath.m_TrapsInPath.Add(point2);
				}
			}
		}
		if (!hazardTerrainBlock)
		{
			List<CObjectHazardousTerrain> list9 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>().ToList();
			ActorState actorState2 = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
			List<CObjectDifficultTerrain> list10 = (from x in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>()
				where x.TreatAsTrap && (x.TreatAsTrapFilter == null || x.TreatAsTrapFilter.IsValidTarget(actorState2))
				select x).ToList();
			foreach (Point point3 in cTargetPath.m_ArrayIndices)
			{
				if ((actorHexArrayIndex.X != point3.X || actorHexArrayIndex.Y != point3.Y) && (list9.Find((CObjectHazardousTerrain x) => x.ArrayIndex.X == point3.X && x.ArrayIndex.Y == point3.Y) != null || list10.Find((CObjectDifficultTerrain x) => x.ArrayIndex.X == point3.X && x.ArrayIndex.Y == point3.Y) != null) && ((!ignoreHazardousTerrain && !fly && !jump) || (((!ignoreHazardousTerrain && !fly) & jump) && point3.X == sampleHexArrayIndex.X && point3.Y == sampleHexArrayIndex.Y)))
				{
					cTargetPath.m_TrapsInPath.Add(point3);
				}
			}
		}
		cTargetPath.m_AlliesInPath = new List<Point>();
		if (!alliesBlock)
		{
			List<CActor> list11 = new List<CActor>();
			if (isPlayer)
			{
				list11.AddRange(ScenarioManager.Scenario.PlayerActors);
				list11.AddRange(ScenarioManager.Scenario.HeroSummons);
			}
			else
			{
				list11.AddRange(ScenarioManager.Scenario.Enemies);
			}
			list11.Remove(actor);
			foreach (Point point4 in cTargetPath.m_ArrayIndices)
			{
				if (list11.Any((CActor x) => x.ArrayIndex.X == point4.X && x.ArrayIndex.Y == point4.Y))
				{
					cTargetPath.m_AlliesInPath.Add(point4);
				}
			}
		}
		if (foundPath)
		{
			CActorStatic.PostProcessTargetPath(cTargetPath, actor, isPlayer, maxMoveCount, fly, jump, ignoreDifficultTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, range);
		}
		return cTargetPath;
	}

	public static void Reset()
	{
		s_TargetPathID = 0;
	}

	public static void SetLOSTileScalar(float x, float y)
	{
		s_LOSTileScalar = new Vector(x, y);
	}

	public static bool LineSegmentsIntersect(Vector p, Vector p2, Vector q, Vector q2, out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
	{
		intersection = default;
		Vector vector = p2 - p;
		Vector vector2 = q2 - q;
		float num = vector.Cross(vector2);
		float num2 = (q - p).Cross(vector);
		if (MF.IsZero(num) && MF.IsZero(num2))
		{
			if (considerCollinearOverlapAsIntersect && ((0f <= (q - p) * vector && (q - p) * vector <= vector * vector) || (0f <= (p - q) * vector2 && (p - q) * vector2 <= vector2 * vector2)))
			{
				return true;
			}
			return false;
		}
		if (MF.IsZero(num) && !MF.IsZero(num2))
		{
			return false;
		}
		float num3 = (q - p).Cross(vector2) / num;
		float num4 = (q - p).Cross(vector) / num;
		float num5 = 0.001f;
		if (!MF.IsZero(num) && 0f - num5 <= num3 && num3 <= 1f + num5 && 0f - num5 <= num4 && num4 <= 1f + num5)
		{
			intersection = p + num3 * vector;
			return true;
		}
		return false;
	}

	private static bool LOSRayBlocked(CTile sourceClientTile, CTile targetClientTile, Vector sourcePosition, Vector targetPosition, Point passThruTileArrayIndex, out bool rayIntersects, List<CMap> visableMaps, LOSCallbackType callback)
	{
		CTile passThruTile = ScenarioManager.Tiles[passThruTileArrayIndex.X, passThruTileArrayIndex.Y];
		CNode cNode = ScenarioManager.PathFinder.Nodes[passThruTileArrayIndex.X, passThruTileArrayIndex.Y];
		Vector vector = default;
		MF.ArrayIndexToCartesianCoord(passThruTileArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector.X, out vector.Y);
		Vector vector2 = vector;
		vector2.Y += s_LOSTileScalar.Y / 0.75f * 0.5f;
		int num = 0;
		rayIntersects = false;
		for (float num2 = 60f; num2 <= 360f; num2 += 60f)
		{
			Vector vector3 = new Vector(0f, s_LOSTileScalar.Y / 0.75f * 0.5f);
			float num3 = (float)Math.Cos((double)(0f - num2) * (Math.PI / 180.0));
			float num4 = (float)Math.Sin((double)(0f - num2) * (Math.PI / 180.0));
			float x = vector3.X * num3 - vector3.Y * num4;
			float y = vector3.X * num4 + vector3.Y * num3;
			vector3 = new Vector(x, y);
			Vector vector4 = vector + vector3;
			Vector vector5 = (vector2 + vector4) * 0.5f;
			Vector vector6 = (vector5 - vector).Normalise();
			float num5 = Math.Abs((vector5 - sourcePosition) * vector6);
			float num6 = vector6 * (targetPosition - sourcePosition);
			bool flag = ScenarioManager.IsTileAdjacent(sourceClientTile.m_ArrayIndex.X, sourceClientTile.m_ArrayIndex.Y, passThruTileArrayIndex.X, passThruTileArrayIndex.Y, ignoreWalls: true);
			bool flag2 = num5 <= 0.1f;
			bool flag3 = false;
			if ((!flag) ? (num6 < 0.001f && LineSegmentsIntersect(vector2, vector4, sourcePosition, targetPosition, out var intersection)) : (num6 < 0f && LineSegmentsIntersect(vector2, vector4, sourcePosition, targetPosition, out intersection)))
			{
				int num7 = num;
				if (flag2 && !flag)
				{
					if ((vector2 - sourcePosition).Magnitude() < (vector4 - sourcePosition).Magnitude())
					{
						num7--;
						if (num7 < 0)
						{
							num7 = s_EdgeToAdjacentMapping.Length - 1;
						}
					}
					else
					{
						num7++;
						if (num7 == s_EdgeToAdjacentMapping.Length)
						{
							num7 = 0;
						}
					}
				}
				rayIntersects = true;
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(passThruTileArrayIndex.X, passThruTileArrayIndex.Y, s_EdgeToAdjacentMapping[num7]);
				bool flag4 = adjacentTile?.m_Props.Any((CObjectProp p) => p.PropHealthDetails != null && p.PropHealthDetails.HasHealth) ?? false;
				bool flag5 = passThruTile != null && passThruTile.m_Props.Any((CObjectProp p) => p.PropHealthDetails != null && p.PropHealthDetails.HasHealth);
				bool flag6 = adjacentTile != null && ((targetClientTile != null && adjacentTile.m_ArrayIndex == targetClientTile.m_ArrayIndex) || (sourceClientTile != null && adjacentTile.m_ArrayIndex == sourceClientTile.m_ArrayIndex));
				bool flag7 = passThruTile != null && ((targetClientTile != null && passThruTile.m_ArrayIndex == targetClientTile.m_ArrayIndex) || (sourceClientTile != null && passThruTile.m_ArrayIndex == sourceClientTile.m_ArrayIndex));
				List<CObjectObstacle> list;
				lock (ScenarioManager.CurrentScenarioState.Props)
				{
					list = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
				}
				List<TileIndex> list2 = new List<TileIndex>();
				foreach (CObjectObstacle item in list)
				{
					foreach (TileIndex pathingBlocker in item.PathingBlockers)
					{
						if (item.BlockLOS)
						{
							list2.Add(pathingBlocker);
						}
					}
				}
				bool flag8 = passThruTile != null && list2.Any((TileIndex p) => passThruTile.m_Hex.ArrayIndex.Equals(p));
				CNode cNode2 = ((adjacentTile != null) ? ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y] : null);
				if (cNode != null && cNode2 != null)
				{
					bool flag9 = false;
					foreach (CNavLink navLink in cNode.NavLinks)
					{
						if (navLink.DestinationNode == cNode2 && navLink.AllowLOS)
						{
							flag9 = true;
							break;
						}
					}
					if (flag9)
					{
						continue;
					}
				}
				if (passThruTile == null || adjacentTile == null || (!cNode.Walkable | flag8) || (cNode.IsBridge && !cNode.IsBridgeOpen && !(flag5 & flag7)) || (cNode2.IsBridge && !cNode2.IsBridgeOpen && !(flag4 & flag6)) || (!cNode2.Walkable && !cNode2.IsBridge) || (sourceClientTile != null && passThruTile.m_HexMap != sourceClientTile.m_HexMap && !visableMaps.Contains(passThruTile.m_HexMap) && (passThruTile.m_Hex2Map == null || !visableMaps.Contains(passThruTile.m_Hex2Map))) || (adjacentTile.m_HexMap != passThruTile.m_HexMap && !cNode2.IsBridge && !cNode.IsBridge))
				{
					callback?.Invoke(vector2, vector4, rayBlocked: true);
					callback?.Invoke(vector5, vector5 + vector6, rayBlocked: true);
					return true;
				}
			}
			vector2 = vector4;
			num++;
		}
		return false;
	}

	private static bool IsEdgeOnWall(Point arrayIndex, float edgeAngle)
	{
		int num = 6;
		CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
		CNode cNode = ((cTile != null) ? ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y] : null);
		int num2 = (int)edgeAngle / 60 % num;
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(arrayIndex.X, arrayIndex.Y, s_EdgeToAdjacentMapping[num2]);
		CNode cNode2 = ((adjacentTile != null) ? ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y] : null);
		num2 = ((num2 == 0) ? (num - 1) : (num2 - 1));
		CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(arrayIndex.X, arrayIndex.Y, s_EdgeToAdjacentMapping[num2]);
		CNode cNode3 = ((adjacentTile2 != null) ? ScenarioManager.PathFinder.Nodes[adjacentTile2.m_ArrayIndex.X, adjacentTile2.m_ArrayIndex.Y] : null);
		if (adjacentTile2 != null && adjacentTile != null && cNode3.Walkable && cNode2.Walkable && (!cNode3.IsBridge || cNode3.IsBridgeOpen) && (!cNode2.IsBridge || cNode2.IsBridgeOpen) && (cTile.m_HexMap == adjacentTile2.m_HexMap || cNode.IsBridge || cNode3.IsBridge))
		{
			if (cTile.m_HexMap != adjacentTile.m_HexMap && !cNode.IsBridge)
			{
				return !cNode2.IsBridge;
			}
			return false;
		}
		return true;
	}

	private static bool InternalHaveLOS(CTile sourceTile, CTile targetTile, LOSCallbackType callback = null)
	{
		if (ScenarioScript.current != null && ScenarioScript.current.OverrideHasLOS(sourceTile, targetTile))
		{
			return true;
		}
		Vector vector = default;
		MF.ArrayIndexToCartesianCoord(sourceTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector.X, out vector.Y);
		Vector vector2 = default;
		MF.ArrayIndexToCartesianCoord(targetTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector2.X, out vector2.Y);
		Vector vector3 = vector2 - vector;
		Vector vector4 = vector3.Normalise();
		float num = vector3.Magnitude() + s_LOSTileScalar.X * 0.5f;
		Vector vector5 = new Vector(vector4.Y, 0f - vector4.X);
		List<Point> list = new List<Point>();
		List<CMap> list2 = new List<CMap>();
		int num2 = Math.Max(0, Math.Min(sourceTile.m_ArrayIndex.X, targetTile.m_ArrayIndex.X) - 1);
		int num3 = Math.Min(ScenarioManager.Width - 1, Math.Max(sourceTile.m_ArrayIndex.X, targetTile.m_ArrayIndex.X) + 1);
		int num4 = Math.Max(0, Math.Min(sourceTile.m_ArrayIndex.Y, targetTile.m_ArrayIndex.Y) - 1);
		int num5 = Math.Min(ScenarioManager.Height - 1, Math.Max(sourceTile.m_ArrayIndex.Y, targetTile.m_ArrayIndex.Y) + 1);
		for (int i = num4; i <= num5; i++)
		{
			for (int j = num2; j <= num3; j++)
			{
				Point point = new Point(j, i);
				Vector vector6 = default;
				MF.ArrayIndexToCartesianCoord(point, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector6.X, out vector6.Y);
				Vector vector7 = vector6 - vector;
				if (vector7.Magnitude() < num && Math.Abs(vector7 * vector5) < s_LOSTileScalar.X && vector7 * vector4 > 0f - s_LOSTileScalar.X)
				{
					callback?.Invoke(vector6, vector6 + new Vector(0.25f, 0f), rayBlocked: false);
					list.Add(point);
				}
			}
		}
		for (float num6 = 0f; num6 < 420f; num6 += 60f)
		{
			float num7 = num6;
			float num8 = num6;
			if (num6 >= 360f)
			{
				num7 = 90f;
				num8 = 0f;
			}
			Vector vector8 = new Vector(0f, s_LOSTileScalar.Y / 0.75f * 0.5f * 0.99f);
			float num9 = (float)Math.Cos((double)(0f - num7) * (Math.PI / 180.0));
			float num10 = (float)Math.Sin((double)(0f - num8) * (Math.PI / 180.0));
			float x = vector8.X * num9 - vector8.Y * num10;
			float y = vector8.X * num10 + vector8.Y * num9;
			vector8 = new Vector(x, y);
			Vector sourcePosition = default;
			MF.ArrayIndexToCartesianCoord(sourceTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out sourcePosition.X, out sourcePosition.Y);
			sourcePosition += vector8;
			for (float num11 = 0f; num11 < 420f; num11 += 60f)
			{
				float num12 = num11;
				float num13 = num11;
				if (num11 >= 360f)
				{
					num12 = 90f;
					num13 = 0f;
				}
				Vector vector9 = new Vector(0f, s_LOSTileScalar.Y / 0.75f * 0.5f * 0.99f);
				num9 = (float)Math.Cos((double)(0f - num12) * (Math.PI / 180.0));
				num10 = (float)Math.Sin((double)(0f - num13) * (Math.PI / 180.0));
				x = vector9.X * num9 - vector9.Y * num10;
				y = vector9.X * num10 + vector9.Y * num9;
				vector9 = new Vector(x, y);
				Vector targetPosition = default;
				MF.ArrayIndexToCartesianCoord(targetTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out targetPosition.X, out targetPosition.Y);
				targetPosition += vector9;
				foreach (Point item in list)
				{
					CTile cTile = ScenarioManager.Tiles[item.X, item.Y];
					CNode cNode = ScenarioManager.PathFinder.Nodes[item.X, item.Y];
					if (cTile == null || cNode == null || !cNode.IsBridge)
					{
						continue;
					}
					LOSRayBlocked(sourceTile, targetTile, sourcePosition, targetPosition, item, out var rayIntersects, new List<CMap>(), callback);
					if (rayIntersects || (sourceTile.m_ArrayIndex.X == cTile.m_ArrayIndex.X && sourceTile.m_ArrayIndex.Y == cTile.m_ArrayIndex.Y))
					{
						if (cTile.m_HexMap != null && cTile.m_HexMap.Revealed && !list2.Contains(cTile.m_HexMap))
						{
							list2.Add(cTile.m_HexMap);
						}
						if (cTile.m_Hex2Map != null && cTile.m_Hex2Map.Revealed && !list2.Contains(cTile.m_Hex2Map))
						{
							list2.Add(cTile.m_Hex2Map);
						}
					}
				}
				bool flag = false;
				foreach (Point item2 in list)
				{
					if (item2.X != sourceTile.m_ArrayIndex.X || item2.Y != sourceTile.m_ArrayIndex.Y)
					{
						flag = LOSRayBlocked(sourceTile, targetTile, sourcePosition, targetPosition, item2, out var _, list2, callback);
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					if (callback == null)
					{
						return true;
					}
					callback(sourcePosition, targetPosition, flag);
				}
			}
		}
		return false;
	}

	public static void EnableLOSCache(bool enable)
	{
		s_CacheLOS = enable;
		if (!enable)
		{
			s_LOSCache?.Clear();
		}
		else if (s_LOSCache == null)
		{
			s_LOSCache = new Dictionary<Tuple<CTile, CTile>, bool>();
		}
	}

	public static bool HaveLOS(CTile sourceTile, CTile targetTile, LOSCallbackType callback = null)
	{
		bool flag = s_CacheLOS;
		if (flag && s_LOSCache.TryGetValue(new Tuple<CTile, CTile>(sourceTile, targetTile), out var value))
		{
			return value;
		}
		bool flag2 = InternalHaveLOS(sourceTile, targetTile, callback) && InternalHaveLOS(targetTile, sourceTile, callback);
		if (flag)
		{
			s_LOSCache[new Tuple<CTile, CTile>(sourceTile, targetTile)] = flag2;
		}
		return flag2;
	}
}
