using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FH.Tools;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
public class CAbility : IGetVar, IDumpState
{
	[KronosEnumPolice(3)]
	public enum EAbilityType
	{
		None,
		Move,
		Attack,
		Heal,
		Damage,
		Loot,
		PreventDamage,
		Shield,
		Retaliate,
		Trap,
		Create,
		RecoverLostCards,
		Invisible,
		Poison,
		Wound,
		Muddle,
		Immobilize,
		Disarm,
		Curse,
		Stun,
		Strengthen,
		Bless,
		Consume,
		Infuse,
		RefreshItemCards,
		RecoverDiscardedCards,
		Summon,
		Advantage,
		Teleport,
		Kill,
		DisarmTrap,
		DestroyObstacle,
		Redirect,
		AddTarget,
		Push,
		Pull,
		AddHeal,
		AddRange,
		ControlActor,
		AddAugment,
		MergedCreateAttack,
		MergedDestroyObstacleAttack,
		MergedMoveAttack,
		MoveObstacle,
		MergedMoveObstacleAttack,
		MergedDisarmTrapDestroyObstacles,
		MergedKillCreate,
		AttackersGainDisadvantage,
		AddCondition,
		AddSong,
		PlaySong,
		AddActiveBonus,
		OverrideAugmentAttackType,
		Null,
		ImmunityTo,
		ConsumeElement,
		ChooseAbility,
		LoseCards,
		IncreaseCardLimit,
		Augment,
		StopFlying,
		ExtraTurn,
		ImprovedShortRest,
		Overheal,
		GainXP,
		AdjustInitiative,
		Swap,
		RedistributeDamage,
		ForgoActionsForCompanion,
		ChangeModifier,
		Immovable,
		ChangeCondition,
		RemoveConditions,
		ShuffleModifierDeck,
		DiscardCards,
		NullTargeting,
		AddDoom,
		AddDoomSlots,
		TransferDooms,
		Cure,
		Choose,
		GiveSupplyCard,
		LongRest,
		ShortRest,
		Fear,
		DeactivateSpawner,
		RemoveActorFromMap,
		ConsumeItemCards,
		Invulnerability,
		PierceInvulnerability,
		Sleep,
		ActivateSpawner,
		Untargetable,
		ItemLock,
		LoseGoalChestReward,
		AddModifierToMonsterDeck,
		RecoverResources,
		MoveTrap,
		BlockHealing,
		NeutralizeShield,
		ChangeCharacterModel,
		NullHex,
		HealthReduction,
		ChangeAllegiance,
		Revive,
		DisableCardAction,
		Regenerate,
		Bane,
		Ward,
		Brittle,
		Impair,
		PlayDiscardedCard,
		CreateTerrain,
		Teleport2,
		ShiftForm,
		MirrorDamage,
		RepeatAbility,
		ExploitableVulnerability,
		DuplicateCondition,
		PreventLoseCard,
		AddAttackModifier,
		ExcludeCardsFromShortRest,
		CreateShadow,
		RemoveShadow,
		MoveShadow,
		MoveActiveBonusTracker,
		ReviveDowned,
		PerformFromShadow,
		DestroyTerrain,
		Capture,
		MoveTerrain,
		ModeTransfer,
		SelectAndPlayCard,
		RemoveAttackModifier,
		ControlSummonTurn,
		SelectAndMoveCard,
		CreateLimitedHazardousTerrain,
		SelectCard,
		TransferDamage,
		ShowNextAttackModifier,
		SetNextAttackModifiers
	}

	[Flags]
	public enum EAbilityFlag
	{
		None = 0,
		ModifiesTimeTokens = 1,
		AdjacentToAttackTargets = 2,
		AttackModifierAbilityOverride = 4,
		FromScenarioModifier = 8,
		SwitchFormMeleeToRange = 0x10,
		SwitchFormRangeToMelee = 0x20,
		IgnoreSwitchForm = 0x40,
		Special1 = 0x80,
		Special2 = 0x100,
		AdjacentToAllies = 0x200,
		TargetStartingPositions = 0x400,
		FrozenFistMechanic = 0x800,
		AddedFromChallenge = 0x1000,
		ImmuneFrozenFistMechanic = 0x2000,
		ModifiesTrophyTokens = 0x4000,
		SubAbilityPreview = 0x8000,
		HasOverridenValues = 0x10000,
		ModifiesResonanceTokens = 0x20000,
		Wave = 0x40000,
		DontPreview = 0x80000,
		DontRefreshTargetingUI = 0x100000,
		All = -1
	}

	public enum ECardAction
	{
		None,
		Top,
		Bottom
	}

	public enum EAttackType
	{
		None,
		Attack,
		Melee,
		Ranged,
		Default,
		Control,
		ControlledByCaster,
		Augment
	}

	public enum EAbilityStatType
	{
		None,
		Strength,
		Range,
		NumberOfTargets,
		Tokens,
		Pierce,
		XP,
		RetaliateRange
	}

	public enum EAbilityTargeting
	{
		None,
		Range,
		Room,
		All,
		AllConnectedRooms,
		Attacker,
		BlueHexesInArea,
		Custom,
		PerformVars
	}

	public enum EStatIsBasedOnXType
	{
		None = 0,
		HexesMovedThisTurn = 1,
		HexesMovedThisAction = 2,
		HexesMovedByParentAbility = 3,
		DamageInflictedThisTurn = 4,
		DamageInflictedThisAction = 5,
		DamageInflictedByParent = 6,
		ObstaclesDestroyedThisTurn = 7,
		ObstaclesDestroyedThisAction = 8,
		ObstaclesDestroyedByParent = 9,
		ObstaclesToBeDestroyedByMergedAbility = 10,
		TargetAdjacentEnemies = 11,
		TargetAdjacentAllies = 12,
		CasterAdjacentEnemies = 13,
		CasterAdjacentAllies = 14,
		DamageInflictedByParentOnLastTarget = 15,
		LostCardCount = 16,
		PercentageOfCurrentHP = 17,
		HPDifference = 18,
		TargetsDamagedByPreviousAttackThisTurn = 19,
		TargetsDamagedByPreviousAttackThisAction = 20,
		TargetsDamagedByPreviousDamageAbilityThisTurn = 21,
		TargetsDamagedByPreviousDamageAbilityThisAction = 22,
		HalfTargetsDamagedByPreviousDamageAbilityThisAction = 23,
		ActorCount = 24,
		DeadActorCount = 25,
		ScenarioLevel = 26,
		TargetHPDifference = 27,
		TargetDamageTaken = 28,
		TargetDistanceRangeDiff = 29,
		TargetShieldValue = 68,
		InitialPlayerCharacterCount = 30,
		XAddedToCharactersTimesLevel = 31,
		CharactersTimesLevelPlusX = 32,
		XAddedToYTimesLevel = 33,
		XPlusCharactersPlusLevel = 34,
		XPlusLevel = 35,
		XAddedToLevelTimesCharactersOverY = 36,
		LevelAddedToXTimesCharacters = 37,
		XAddedToInitialPlayerCharacterCount = 38,
		XTimesInitialPlayerCharacterCountPlusLevelMinusY = 39,
		XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound = 40,
		CharactersPlusLevelOverX = 41,
		TargetAdjacentEnemiesOfTarget = 42,
		TargetAdjacentAlliesOfTarget = 43,
		CasterAdjacentEnemiesOfTarget = 44,
		CasterAdjacentAlliesOfTarget = 45,
		TargetAdjacentEnemiesOfCaster = 46,
		TargetAdjacentAlliesOfCaster = 47,
		CasterAdjacentEnemiesOfCaster = 48,
		CasterAdjacentAlliesOfCaster = 49,
		XPlusCharactersPlusLevelTimesY = 50,
		TargetAdjacentActors = 51,
		XTimesInitialPlayerCharacterCountMinusY = 52,
		XAddedToLevelThenTimesCharactersOverY = 53,
		LevelTimesXTimesHexesMovedThisTurnInDefinedRoom = 54,
		LevelTimesXPlusY = 55,
		ActorsTargetedByParentAbility = 56,
		ActorsKilledByParentAbility = 57,
		TargetAdjacentWalls = 58,
		CasterAdjacentWalls = 59,
		TargetAdjacentValidTiles = 60,
		CasterAdjacentValidTiles = 61,
		ExcessDamageInflictedByParentOnLastTargetKilled = 62,
		ActorsKilledThisAction = 63,
		ActorsKilledThisTurn = 64,
		ActorsKilledThisRound = 65,
		ActorsKilledThisScenario = 66,
		CasterShieldValue = 67,
		XTimesActorCountMinusY = 69,
		DamageInflictedByPreviousAbility = 70,
		InitialPlayerCharacterCountMinusYTimesX = 71,
		DamageActuallyTakenByParent = 72,
		TargetsActuallyDamagedByPreviousAttackThisTurn = 73,
		ActorsHealedThisAction = 74,
		ScenarioTrapDamage = 75,
		ScenarioHazardousTerrainDamage = 76,
		BaseStat = 77,
		TargetAdjacentSummonsOfCaster = 78,
		TargetAdjacentBannerSummonsOfCaster = 79,
		CharactersPlusLevelTimesX = 80,
		CurrentHPOverXMaxY = 81,
		BaseStatPlusX = 82,
		CharactersAddedToXOver2ThenTimesLevelPlusY = 83,
		LevelPlusXTimesCharactersTimesY = 84,
		LevelPlusYTimesX = 85,
		XAddedToLevelTimesActorsOverY = 86,
		LastAbilityPerformedTargetShieldValue = 87,
		TargetInWater = 88,
		TimeTokens = 89,
		SingleUsePreventDamageCount = 90,
		XPlusYCharactersTimes2PlusLevel = 91,
		LevelTimesXPlusYTimesCharacters = 92,
		X = 93,
		CharactersTimesXPlusLevelTimesY = 94,
		YPlusCharactersTimesLevelPlusX = 95,
		Vars = 96,
		FH22ShipFormulaCharactersLevel = 97
	}

	public enum EStatIsBasedOnXRoundingType
	{
		None,
		RoundOff,
		ToFloor,
		ToCeil
	}

	public static List<EStatIsBasedOnXType> StatIsXTypesToIgnoreIfAddTo = new List<EStatIsBasedOnXType> { EStatIsBasedOnXType.HexesMovedThisTurn };

	public static List<EStatIsBasedOnXType> StatIsBasedOnXTypesForBaseStats = new List<EStatIsBasedOnXType>
	{
		EStatIsBasedOnXType.InitialPlayerCharacterCount,
		EStatIsBasedOnXType.XAddedToCharactersTimesLevel,
		EStatIsBasedOnXType.CharactersTimesLevelPlusX,
		EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX,
		EStatIsBasedOnXType.XAddedToYTimesLevel,
		EStatIsBasedOnXType.XPlusCharactersPlusLevel,
		EStatIsBasedOnXType.XPlusLevel,
		EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY,
		EStatIsBasedOnXType.LevelAddedToXTimesCharacters,
		EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount,
		EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY,
		EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound,
		EStatIsBasedOnXType.CharactersPlusLevelOverX,
		EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY,
		EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY,
		EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY,
		EStatIsBasedOnXType.LevelTimesXPlusY,
		EStatIsBasedOnXType.InitialPlayerCharacterCountMinusYTimesX,
		EStatIsBasedOnXType.CharactersPlusLevelTimesX,
		EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY,
		EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY,
		EStatIsBasedOnXType.LevelPlusYTimesX,
		EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel,
		EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters,
		EStatIsBasedOnXType.CharactersTimesXPlusLevelTimesY,
		EStatIsBasedOnXType.FH22ShipFormulaCharactersLevel
	};

	public static readonly EAttackType[] AttackTypes = (EAttackType[])Enum.GetValues(typeof(EAttackType));

	public static readonly EAbilityTargeting[] AbilityTargetingTypes = (EAbilityTargeting[])Enum.GetValues(typeof(EAbilityTargeting));

	public static readonly EAbilityType[] AbilityTypes = (EAbilityType[])Enum.GetValues(typeof(EAbilityType));

	public static readonly EStatIsBasedOnXType[] StatIsBasedOnXTypes = (EStatIsBasedOnXType[])Enum.GetValues(typeof(EStatIsBasedOnXType));

	public static readonly EAbilityStatType[] AbilityStatTypes = (EAbilityStatType[])Enum.GetValues(typeof(EAbilityStatType));

	public static readonly Dictionary<EAbilityType, EAbilityStatType> AbilityDisplayValues = GetAbilityDisplayValues();

	public static readonly EStatIsBasedOnXRoundingType[] RoundingTypes = (EStatIsBasedOnXRoundingType[])Enum.GetValues(typeof(EStatIsBasedOnXRoundingType));

	[JsonProperty]
	public int Tokens;

	[JsonProperty]
	public EAbilityFlag Flags;

	[JsonIgnore]
	private CVarsContext m_VarsContext;

	[JsonIgnore]
	protected List<CActor> m_CachedAIMoveFocusActors;

	[JsonProperty]
	protected bool m_SkipGUI;

	[StateDump(Skip = true)]
	[JsonIgnore]
	public bool HadAreaEffect;

	public bool m_HasAutoSelected;

	public bool ShouldBePlayerControlled;

	private CBaseCard m_CachedAbilityBaseCard;

	public bool DontOverride;

	private bool m_HasEnded;

	private int m_StrengthStatAdditions;

	private int m_RangeStatAdditions;

	private int m_RetaliateRangeStatAdditions;

	private int m_NumberTargetsStatAdditions;

	private int m_TokenStatAdditions;

	protected List<CActor> m_ValidActorsInRange = new List<CActor>();

	protected List<CActor> m_ImmuneActorsInRange = new List<CActor>();

	protected List<CActor> m_PotentiallyValidActorsInRange = new List<CActor>();

	[JsonProperty]
	protected bool m_IsSubAbility;

	[JsonProperty]
	protected bool m_IsMergedAbility;

	[JsonProperty]
	protected bool m_IsModifierAbility;

	[JsonProperty]
	private int m_Strength;

	[JsonProperty]
	private int m_Range;

	protected int m_OriginalRange;

	protected int m_RangeBuff;

	[JsonProperty]
	private int m_NumberTargets;

	protected int m_OriginalTargetCount;

	protected int m_TargetCountBuff;

	protected CAreaEffect m_OriginalAreaEffect;

	protected List<CActor> m_ActorsToIgnore = new List<CActor>();

	[JsonProperty]
	protected List<CEnhancement> m_AbilityEnhancements;

	[JsonProperty]
	protected Dictionary<CCondition.EPositiveCondition, CAbility> m_PositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>();

	[JsonProperty]
	protected Dictionary<CCondition.ENegativeCondition, CAbility> m_NegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();

	[JsonProperty]
	protected Dictionary<CCondition.EPositiveCondition, CAbility> m_OriginalPositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>();

	[JsonProperty]
	protected Dictionary<CCondition.ENegativeCondition, CAbility> m_OriginalNegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();

	protected List<CCondition.EPositiveCondition> m_PositiveConditionsBuff;

	protected List<CCondition.ENegativeCondition> m_NegativeConditionsBuff;

	protected List<CTile> m_TilesSelected = new List<CTile>();

	[JsonProperty]
	protected AbilityData.XpPerTargetData m_XpPerTargetData;

	protected bool m_CanUndo = true;

	protected bool m_CanSkip = true;

	protected List<CActor> m_ActorsToTarget;

	protected CActor m_TargetedActor;

	protected int m_NumberTargetsRemaining;

	protected int m_UndoNumberTargetsRemaining;

	protected bool m_ProcessIfDead;

	protected bool m_AllTargets;

	protected bool m_CancelAbility;

	protected bool m_ConfirmCancelled;

	protected bool m_AbilityHasHappened;

	protected bool m_AbilityStartComplete;

	protected Dictionary<CAbilityOverride, CItem> m_CurrentItemOverrides = new Dictionary<CAbilityOverride, CItem>();

	private List<CAbilityOverride> m_CurrentOverrides = new List<CAbilityOverride>();

	protected List<CItem> m_ActiveSingleTargetItems = new List<CItem>();

	protected List<CActiveBonus> m_ActiveSingleTargetActiveBonuses = new List<CActiveBonus>();

	[StateDump(Skip = true)]
	[JsonIgnore]
	public CTile LastSelectedAreaEffectTile;

	protected List<CTile> m_ValidTilesInAreaEffect = new List<CTile>();

	protected List<CTile> m_ValidTilesInAreaEffectIncludingBlocked = new List<CTile>();

	protected Dictionary<CTile, CAreaEffect.EHexFlag> m_FlagsOfValidTiles = new Dictionary<CTile, CAreaEffect.EHexFlag>();

	protected List<CTile> m_InlineSubAbilityTiles = new List<CTile>();

	protected CAreaEffect m_AreaEffectBackup;

	protected float m_AreaEffectAngle;

	protected bool m_IsScenarioModifierAbility;

	protected bool m_AugmentsAdded;

	protected bool m_AugmentAbilitiesProcessed;

	protected int m_AugmentAbilitiesTargetCount;

	protected bool m_AugmentAbilitiesNextTarget;

	protected bool m_AugmentOverridesProcessed;

	protected List<CAugment> m_AbilityAugments = new List<CAugment>();

	protected List<CAugment> m_OverrideAugments = new List<CAugment>();

	protected bool m_SongAbilitiesProcessed;

	protected int m_SongAbilitiesTargetCount;

	protected bool m_SongAbilitiesNextTarget;

	protected bool m_SongOverridesProcessed;

	protected List<CSong.SongEffect> m_AbilitySongs = new List<CSong.SongEffect>();

	protected List<CSong.SongEffect> m_OverrideSongs = new List<CSong.SongEffect>();

	private bool m_IsControlAbility;

	[StateDump(Skip = true)]
	public string InvalidTileReason;

	[JsonProperty]
	private List<ElementInfusionBoardManager.EElement> m_InfuseElements = new List<ElementInfusionBoardManager.EElement>();

	[StateDump(Skip = true)]
	protected List<CMessageData> m_LogAfterAppliedMessages = new List<CMessageData>();

	private static JsonSerializerSettings s_JsonSerializerSettings = new JsonSerializerSettings
	{
		Formatting = Formatting.Indented,
		TypeNameHandling = TypeNameHandling.All,
		ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
		PreserveReferencesHandling = PreserveReferencesHandling.Objects
	};

	private bool m_BlockPerformAndRestart;

	public static EStatIsBasedOnXType[] TargetSpecificStatIsBasedOnXTypes = new EStatIsBasedOnXType[4]
	{
		EStatIsBasedOnXType.TargetShieldValue,
		EStatIsBasedOnXType.TargetHPDifference,
		EStatIsBasedOnXType.TargetDamageTaken,
		EStatIsBasedOnXType.TargetDistanceRangeDiff
	};

	[JsonIgnore]
	public CAbility Original { get; private set; }

	[JsonIgnore]
	public string FileName { get; set; }

	[JsonIgnore]
	public string YMLPath { get; set; }

	[JsonIgnore]
	public int CardID { get; set; }

	[JsonProperty]
	public Guid ID { get; private set; }

	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public EAbilityType AbilityType { get; private set; }

	[JsonProperty]
	public string AnimOverload { get; set; }

	[JsonProperty]
	public string AnimOverload2 { get; set; }

	public int Strength
	{
		get
		{
			return m_Strength;
		}
		set
		{
			m_Strength = value;
		}
	}

	public int Range
	{
		get
		{
			return m_Range + m_RangeBuff;
		}
		set
		{
			m_Range = value - m_RangeBuff;
		}
	}

	protected int BaseTargetCount
	{
		get
		{
			return m_NumberTargets;
		}
		set
		{
			m_NumberTargets = value;
		}
	}

	public int NumberTargets
	{
		get
		{
			return m_NumberTargets + m_TargetCountBuff;
		}
		set
		{
			m_NumberTargets = value;
		}
	}

	public int OriginalTargetCount
	{
		get
		{
			return m_OriginalTargetCount;
		}
		set
		{
			m_OriginalTargetCount = value;
		}
	}

	public CAreaEffect OriginalAreaEffect
	{
		get
		{
			return m_OriginalAreaEffect;
		}
		set
		{
			m_OriginalAreaEffect = value;
		}
	}

	[JsonProperty]
	public CAbilityFilterContainer AbilityFilter { get; set; }

	[JsonProperty]
	public CAbilityFilter.EFilterTile TileFilter { get; set; }

	[JsonProperty]
	public EAbilityTargeting Targeting { get; set; }

	[JsonProperty]
	public List<(string, TValue)> TargetingVars { get; set; }

	[JsonProperty]
	public List<(string, TValue)> Vars { get; set; }

	[JsonProperty]
	public List<(string, List<(string, TValue)>)> PerformVars { get; set; }

	[JsonProperty]
	public List<(string, TValue)> MiscVars { get; set; }

	public bool ProcessIfDead
	{
		get
		{
			return m_ProcessIfDead;
		}
		set
		{
			m_ProcessIfDead = value;
		}
	}

	public AbilityData.XpPerTargetData XpPerTargetData => m_XpPerTargetData;

	[JsonProperty]
	public bool AllTargetsOnMovePath { get; private set; }

	[JsonProperty]
	public bool AllTargetsOnMovePathSameStartAndEnd { get; private set; }

	[JsonProperty]
	public bool AllTargetsOnAttackPath { get; private set; }

	[JsonProperty]
	public bool AllTargetsInRangeAlongPath { get; private set; }

	[JsonProperty]
	public bool IsTargetedAbility { get; private set; }

	[JsonProperty]
	public float SpawnDelay { get; set; }

	[JsonProperty]
	public List<CConditionalOverride> ConditionalOverrides { get; private set; }

	public List<CConditionalOverride> ActiveConditionalOverrides { get; private set; }

	[JsonProperty]
	public CAbilityRequirements StartAbilityRequirements { get; set; }

	[JsonProperty]
	public int AbilityXP { get; set; }

	[JsonProperty]
	public AbilityData.MiscAbilityData MiscAbilityData { get; set; }

	[JsonProperty]
	public bool SkipAnim { get; set; }

	[JsonProperty]
	public bool IsItemAbility { get; set; }

	[JsonProperty]
	public bool UseSpecialBaseStat { get; private set; }

	[JsonProperty]
	public string PreviewEffectId { get; private set; }

	[JsonProperty]
	public string PreviewEffectText { get; private set; }

	[JsonProperty]
	public string HelpBoxTooltipLocKey { get; set; }

	[JsonProperty]
	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; set; }

	[JsonProperty]
	public CAreaEffect AreaEffect { get; protected set; }

	public float AreaEffectAngle
	{
		get
		{
			return m_AreaEffectAngle;
		}
		set
		{
			m_AreaEffectAngle = value;
		}
	}

	public bool AreaEffectFlipped { get; set; }

	public bool AreaEffectLocked => m_AreaEffectLocked;

	[JsonProperty]
	public string AreaEffectYMLString { get; private set; }

	[JsonProperty]
	public string AreaEffectLayoutOverrideYMLString { get; private set; }

	[JsonProperty]
	public string AreaEffectFlipAxis { get; private set; }

	[JsonProperty]
	public AbilityData.ActiveBonusData ActiveBonusData { get; private set; }

	[JsonIgnore]
	public CVarsContext VarsContext
	{
		get
		{
			if (m_VarsContext == null)
			{
				m_VarsContext = new CVarsContext(this);
			}
			return m_VarsContext;
		}
		private set
		{
			m_VarsContext = value;
		}
	}

	[JsonIgnore]
	public virtual bool AutoTrigger
	{
		get
		{
			if (MiscAbilityData != null && MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
			{
				return true;
			}
			return GetMiscVar("AutoTrigger").GetBool(VarsContext);
		}
	}

	[JsonIgnore]
	public bool UseParentTargets
	{
		get
		{
			if (Targeting != EAbilityTargeting.Custom && MiscAbilityData != null && ParentAbility != null)
			{
				if (!MiscAbilityData.UseParentTargets.HasValue || !MiscAbilityData.UseParentTargets.Value)
				{
					return IsInlineSubAbility;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsInlineAndUsesInlineTargeting
	{
		get
		{
			if (IsInlineSubAbility)
			{
				return !GetMiscVar("TreatAsNonInlineForTargeting").GetBool();
			}
			return false;
		}
	}

	public ActiveBonusLayout ActiveBonusYML { get; set; }

	public List<CEnhancement> AbilityEnhancements => m_AbilityEnhancements;

	public bool EnhancementsApplied { get; private set; }

	public Dictionary<CCondition.EPositiveCondition, CAbility> PositiveConditions
	{
		get
		{
			if (m_PositiveConditionsBuff == null)
			{
				return m_PositiveConditions;
			}
			Dictionary<CCondition.EPositiveCondition, CAbility> dictionary = new Dictionary<CCondition.EPositiveCondition, CAbility>();
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_PositiveConditions)
			{
				dictionary.Add(positiveCondition.Key, positiveCondition.Value);
			}
			foreach (CCondition.EPositiveCondition posCon in m_PositiveConditionsBuff)
			{
				EAbilityType abilityType = AbilityTypes.Single((EAbilityType x) => x.ToString() == posCon.ToString());
				if (dictionary.ContainsKey(posCon))
				{
					if (posCon == CCondition.EPositiveCondition.Bless)
					{
						CAbility cAbility = dictionary[posCon];
						cAbility.Strength = Math.Max(1, cAbility.Strength) + 1;
					}
				}
				else
				{
					dictionary.Add(posCon, CreateAbility(abilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
			}
			return dictionary;
		}
		set
		{
			m_PositiveConditions = value;
		}
	}

	public Dictionary<CCondition.ENegativeCondition, CAbility> NegativeConditions
	{
		get
		{
			if (m_NegativeConditionsBuff == null)
			{
				return m_NegativeConditions;
			}
			Dictionary<CCondition.ENegativeCondition, CAbility> dictionary = new Dictionary<CCondition.ENegativeCondition, CAbility>();
			foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_NegativeConditions)
			{
				dictionary.Add(negativeCondition.Key, negativeCondition.Value);
			}
			foreach (CCondition.ENegativeCondition negCon in m_NegativeConditionsBuff)
			{
				EAbilityType abilityType = AbilityTypes.Single((EAbilityType x) => x.ToString() == negCon.ToString());
				if (dictionary.ContainsKey(negCon))
				{
					if (negCon == CCondition.ENegativeCondition.Curse)
					{
						CAbility cAbility = dictionary[negCon];
						cAbility.Strength = Math.Max(1, cAbility.Strength) + 1;
					}
				}
				else
				{
					dictionary.Add(negCon, CreateAbility(abilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
			}
			return dictionary;
		}
		set
		{
			m_NegativeConditions = value;
		}
	}

	public Dictionary<CCondition.EPositiveCondition, CAbility> OriginalPositiveConditions
	{
		get
		{
			return m_OriginalPositiveConditions;
		}
		set
		{
			m_OriginalPositiveConditions = value;
		}
	}

	public Dictionary<CCondition.ENegativeCondition, CAbility> OriginalNegativeConditions
	{
		get
		{
			return m_OriginalNegativeConditions;
		}
		set
		{
			m_OriginalNegativeConditions = value;
		}
	}

	[JsonProperty]
	public Dictionary<string, int> ResourcesToAddOnAbilityEnd { get; set; }

	[JsonProperty]
	public Dictionary<string, int> ResourcesToTakeFromTargets { get; set; }

	[JsonProperty]
	public Dictionary<string, int> ResourcesToGiveToTargets { get; set; }

	[JsonIgnore]
	protected List<CActor> CachedAIMoveFocusActors
	{
		get
		{
			if (m_CachedAIMoveFocusActors != null)
			{
				return m_CachedAIMoveFocusActors;
			}
			if (TargetingActor?.AIMoveFocusActors != null && TargetingActor.AIMoveFocusActors.Count > 0)
			{
				m_CachedAIMoveFocusActors = TargetingActor.AIMoveFocusActors.ToList();
			}
			return m_CachedAIMoveFocusActors;
		}
	}

	[JsonProperty]
	public CAugment Augment { get; private set; }

	[JsonProperty]
	public CSong Song { get; private set; }

	[JsonProperty]
	public bool IsConsumeAbility { get; private set; }

	[JsonProperty]
	public string ParentName { get; private set; }

	public bool IsSubAbility
	{
		get
		{
			return m_IsSubAbility;
		}
		set
		{
			m_IsSubAbility = value;
		}
	}

	public bool UseSubAbilityTargeting
	{
		get
		{
			if (m_IsSubAbility && (MiscAbilityData == null || !MiscAbilityData.UseParentTargets.HasValue || MiscAbilityData.UseParentTargets.Value))
			{
				if (MiscAbilityData == null || !MiscAbilityData.TreatAsNonSubAbility.HasValue)
				{
					return true;
				}
				return !MiscAbilityData.TreatAsNonSubAbility.Value;
			}
			return false;
		}
	}

	[JsonProperty]
	public bool IsInlineSubAbility { get; set; }

	[JsonProperty]
	public List<CAbility> SubAbilities { get; private set; }

	public bool IsMergedAbility => m_IsMergedAbility;

	public bool IsModifierAbility => m_IsModifierAbility;

	[JsonProperty]
	public bool AddAttackBaseStat { get; set; }

	[JsonProperty]
	public bool StrengthIsBase { get; set; }

	[JsonProperty]
	public bool RangeIsBase { get; set; }

	[JsonProperty]
	public bool TargetIsBase { get; set; }

	[JsonProperty]
	public bool IsMonsterAbility { get; set; }

	public bool TargetsSet { get; set; }

	[JsonProperty]
	public string AbilityText { get; set; }

	[JsonProperty]
	public bool AbilityTextOnly { get; set; }

	[JsonProperty]
	public bool AbilityNoLayoutSpace { get; set; }

	[JsonProperty]
	public bool SkipGUI
	{
		get
		{
			if (m_SkipGUI)
			{
				return true;
			}
			return GetMiscVar("SkipGUI").GetBool(VarsContext);
		}
		set
		{
			m_SkipGUI = value;
		}
	}

	[JsonIgnore]
	public bool ExplicitlyDoesNothing => GetMiscVar("ExplicitlyDoesNothing").GetBool();

	[JsonIgnore]
	public bool IsMirrorAbility => GetMiscVar("IsMirrorAbility").GetBool();

	[JsonProperty]
	public bool ShowRange { get; set; }

	[JsonProperty]
	public bool ShowTarget { get; set; }

	[JsonProperty]
	public bool ShowArea { get; set; }

	[JsonProperty]
	public bool OnDeath { get; set; }

	[JsonProperty]
	public string PerkID { get; set; }

	[JsonProperty]
	public bool WasInterrupted { get; set; }

	[JsonProperty]
	public bool WasForcedInterrupted { get; set; }

	[JsonIgnore]
	public bool HasResumedAfterInterrupt { get; set; }

	public bool StackedAttackEffectAbility { get; set; }

	public bool CancelIfNoValid
	{
		get
		{
			if (!GetMiscVar("DontCancelIfNoTiles").GetBool())
			{
				return !GetMiscVar("DontCancelIfNoValid").GetBool();
			}
			return false;
		}
	}

	public string HelpTextWhenCancelled => GetMiscVar("HelpTextWhenCancelled").GetString();

	public bool AllowUndoAfterFirst => GetMiscVar("AllowUndoAfterFirst").GetBool();

	protected bool AutoSelectTiles => GetMiscVar("AutoSelectValidTiles").GetBool();

	public bool NegativeAbilityIsMandatory
	{
		get
		{
			if (MiscAbilityData != null && MiscAbilityData.NegativeAbilityIsOptional.HasValue)
			{
				return !MiscAbilityData.NegativeAbilityIsOptional.Value;
			}
			return true;
		}
	}

	public List<CActor> ValidActorsInRange
	{
		get
		{
			return m_ValidActorsInRange;
		}
		set
		{
			m_ValidActorsInRange = value;
		}
	}

	public List<CActor> ImmuneActorsInRange
	{
		get
		{
			return m_ImmuneActorsInRange;
		}
		set
		{
			m_ImmuneActorsInRange = value;
		}
	}

	public CActor TargetingActor { get; set; }

	public CActor OriginalTargetingActor { get; set; }

	public CActor FilterActor { get; set; }

	public CActor ControllingActor { get; set; }

	public List<CTile> TilesSelected => m_TilesSelected;

	public List<CActor> ActorsToTarget => m_ActorsToTarget;

	public int NumberTargetsRemaining
	{
		get
		{
			return m_NumberTargetsRemaining;
		}
		set
		{
			m_NumberTargetsRemaining = value;
		}
	}

	public List<CTile> TilesInRange { get; set; }

	public bool CanUndo
	{
		get
		{
			if (m_CanUndo)
			{
				if (MiscAbilityData == null || !MiscAbilityData.CanUndo.HasValue)
				{
					return true;
				}
				return MiscAbilityData.CanUndo.Value;
			}
			return false;
		}
	}

	public bool IsControlAbility
	{
		get
		{
			return m_IsControlAbility;
		}
		set
		{
			m_IsControlAbility = value;
		}
	}

	public string ControlledByClassID { get; set; }

	public bool CanSkip
	{
		get
		{
			if (CanBePerformed())
			{
				return m_CanSkip;
			}
			return true;
		}
	}

	public List<CTile> AllPossibleTilesInAreaEffect { get; set; }

	public List<CActor> ActorsToIgnore
	{
		get
		{
			return m_ActorsToIgnore;
		}
		set
		{
			m_ActorsToIgnore = value;
		}
	}

	public bool AllTargets
	{
		get
		{
			return m_AllTargets;
		}
		set
		{
			m_AllTargets = value;
		}
	}

	public List<CItem> ActiveSingleTargetItems => m_ActiveSingleTargetItems;

	public List<CActiveBonus> ActiveSingleTargetActiveBonuses => m_ActiveSingleTargetActiveBonuses;

	public List<CItem> ActiveOverrideItems => m_CurrentItemOverrides.Values.ToList();

	public List<CTile> ValidTilesInAreaAffected => m_ValidTilesInAreaEffect;

	public List<CTile> ValidTilesInAreaEffectedIncludingBlocked => m_ValidTilesInAreaEffectIncludingBlocked;

	public Dictionary<CTile, CAreaEffect.EHexFlag> FlagsOfValidTiles => m_FlagsOfValidTiles;

	public bool AbilityHasHappened
	{
		get
		{
			return m_AbilityHasHappened;
		}
		set
		{
			m_AbilityHasHappened = value;
		}
	}

	public bool AbilityHasBeenCancelled
	{
		get
		{
			if (!m_CancelAbility)
			{
				return m_ConfirmCancelled;
			}
			return true;
		}
	}

	public bool AbilityHasBeenCancelledImmediately
	{
		get
		{
			if (!m_CancelAbility)
			{
				return AbilityType == EAbilityType.Null;
			}
			return true;
		}
	}

	public bool AugmentsAdded
	{
		get
		{
			return m_AugmentsAdded;
		}
		set
		{
			m_AugmentsAdded = value;
		}
	}

	public List<CActor> ActorsTargeted { get; protected set; }

	public List<string> ActorsTargetedGUIDS
	{
		get
		{
			if (ActorsTargeted != null && ActorsTargeted.Count != 0)
			{
				return ActorsTargeted.Select((CActor x) => x.ActorGuid).ToList();
			}
			return null;
		}
	}

	public List<CActor.EType> ActorsTargetedTypes
	{
		get
		{
			if (ActorsTargeted != null && ActorsTargeted.Count != 0)
			{
				return ActorsTargeted.Select((CActor x) => x.Type).ToList();
			}
			return null;
		}
	}

	public CAreaEffect AreaEffectBackup => m_AreaEffectBackup;

	public int DamageInflictedByAbility { get; set; }

	public int DamageActuallyTakenByAbility { get; set; }

	[JsonProperty]
	public bool? CanUndoOverride { get; set; }

	public int DamageInflictedByAbilityOnLastTarget { get; set; }

	public int ExcessDamageInflictedOnLastTargetKilled { get; set; }

	public bool AppliedEnhancements { get; set; }

	public bool AbilityStartListenersInvoked { get; set; }

	public CAbility ParentAbility { get; set; }

	public CAbility AbilityInitialState { get; set; }

	public CAbility AbilityInterruptCopy { get; set; }

	public List<CTile> InlineSubAbilityTiles
	{
		get
		{
			return m_InlineSubAbilityTiles;
		}
		set
		{
			m_InlineSubAbilityTiles = value;
		}
	}

	public CActor TargetThisActorAutomatically { get; set; }

	public List<CAbilityOverride> CurrentOverrides => m_CurrentOverrides;

	public CBaseCard CachedAbilityBaseCard
	{
		get
		{
			return m_CachedAbilityBaseCard;
		}
		set
		{
			m_CachedAbilityBaseCard = value;
		}
	}

	public string XPSourceKey
	{
		get
		{
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard == null)
			{
				return null;
			}
			if (abilityBaseCard.CardType != CBaseCard.ECardType.CharacterAbility)
			{
				return null;
			}
			return "AbilityCards";
		}
	}

	public CBaseCard ParentAbilityBaseCard { get; set; }

	public bool IsScenarioModifierAbility
	{
		get
		{
			return m_IsScenarioModifierAbility;
		}
		set
		{
			m_IsScenarioModifierAbility = value;
		}
	}

	public bool TreatAsEventEffect
	{
		get
		{
			return GetMiscVar("TreatAsEventEffect").GetBool();
		}
		set
		{
			SetMiscVar("TreatAsEventEffect", value);
		}
	}

	public PerksYMLData perkData
	{
		get
		{
			if (!string.IsNullOrEmpty(PerkID) && TargetingActor?.CharacterClass != null)
			{
				return TargetingActor.CharacterClass.Perks.FirstOrDefault((PerksYMLData p) => p.ID == PerkID);
			}
			return null;
		}
	}

	public List<CEnhancement> ActiveEnhancements => m_AbilityEnhancements.Where((CEnhancement w) => w.Enhancement != EEnhancement.NoEnhancement).ToList();

	public List<CEnhancement> AvailableEnhancementSlots => m_AbilityEnhancements.Where((CEnhancement w) => w.Enhancement == EEnhancement.NoEnhancement).ToList();

	public Point TargetingOverrideArrayIndex { get; set; }

	public bool? DontMoveAbilityState { get; set; }

	public bool HasEnded
	{
		get
		{
			return m_HasEnded;
		}
		set
		{
			m_HasEnded = value;
		}
	}

	public int m_ModifiedStrength { get; protected set; }

	public bool HasStarted => m_AbilityStartComplete;

	protected bool m_AreaEffectLocked { get; set; }

	public CAbility Ability => this;

	public bool TreatAsAlwaysPerformable => GetMiscVar("TreatAsAlwaysPerformable").GetBool();

	public bool IgnoreAreaEffectLos => GetMiscVar("IgnoreAreaEffectLos").GetBool();

	public bool IgnoreDoorLocks
	{
		get
		{
			if (!GetMiscVar("IgnoreDoorLocks").GetBool())
			{
				return TargetingActor?.CanUnlockDoors ?? false;
			}
			return true;
		}
	}

	public bool TempDisabledConditions => GetMiscVar("TempDisabledConditions").GetBool();

	public virtual ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			string value = GetMiscVar("SpawnTypePreview").GetString();
			if (!string.IsNullOrEmpty(value) && Enum.TryParse<ScenarioManager.ObjectImportType>(value, out var result))
			{
				return result;
			}
			return ScenarioManager.ObjectImportType.None;
		}
	}

	public virtual string SpawnPropPreview
	{
		get
		{
			string value = GetMiscVar("SpawnPropPreview").GetString();
			if (!string.IsNullOrEmpty(value) && Enum.TryParse<ESpecificPropType>(value, out var result))
			{
				return result.ToString();
			}
			if (!string.IsNullOrEmpty(value) && Enum.TryParse<EPropType>(value, out var result2))
			{
				return result2.ToString();
			}
			return string.Empty;
		}
	}

	public virtual bool NoBonuses => GetMiscVar("NoBonuses").GetBool();

	public List<CCondition.ENegativeCondition> GetAllNegativeConditions()
	{
		List<CCondition.ENegativeCondition> list = NegativeConditions.Keys.ToList();
		CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditionFromAbility(AbilityType);
		if (eNegativeCondition != CCondition.ENegativeCondition.NA)
		{
			list.Add(eNegativeCondition);
		}
		return list;
	}

	public List<CCondition.EPositiveCondition> GetAllPositiveConditions()
	{
		List<CCondition.EPositiveCondition> list = PositiveConditions.Keys.ToList();
		CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditionFromAbility(AbilityType);
		if (ePositiveCondition != CCondition.EPositiveCondition.NA)
		{
			list.Add(ePositiveCondition);
		}
		return list;
	}

	public CAbilityOverride FindOverride(Guid id)
	{
		return CurrentOverrides?.FirstOrDefault((CAbilityOverride x) => x.ID == id);
	}

	public CAbilityOverride FindOverride(CAbilityOverride abilityOverride)
	{
		return FindOverride(abilityOverride.ID);
	}

	public CBaseCard GetAbilityBaseCard(CActor actor = null)
	{
		if (m_CachedAbilityBaseCard == null)
		{
			if (actor == null)
			{
				actor = TargetingActor;
			}
			m_CachedAbilityBaseCard = actor?.FindCardWithAbility(this);
		}
		return m_CachedAbilityBaseCard;
	}

	public bool TreatedAsScenarioEffect(CActor targetingActor = null)
	{
		TValue miscVar = GetMiscVar("TreatAsScenarioEffect");
		if (!miscVar.isUnknown)
		{
			return miscVar.GetBool();
		}
		if (GetAbilityBaseCard(targetingActor) is CScenarioModifier { IsPositive: false })
		{
			return true;
		}
		return false;
	}

	public bool TreatedAsScenarioRule(CActor targetingActor = null)
	{
		if (!GetMiscVar("TreatAsScenarioRule").GetBool())
		{
			if (GetAbilityBaseCard(targetingActor) is CScenarioModifier cScenarioModifier)
			{
				return cScenarioModifier.IsPositive;
			}
			return false;
		}
		return true;
	}

	public void SetTreatAsScenarioEffect(bool val)
	{
		SetMiscVar("TreatAsScenarioEffect", val);
	}

	public void SetTreatAsScenarioRule(bool val)
	{
		SetMiscVar("TreatAsScenarioRule", val);
	}

	public List<string> PreviewEffectTextParams()
	{
		List<string> list = null;
		if (MiscVars != null)
		{
			foreach (var miscVar in MiscVars)
			{
				if (miscVar.Item1.StartsWith("PreviewEffectTextVar"))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					List<string> list2 = list;
					TValue item = miscVar.Item2;
					list2.Add(item.Calc(VarsContext).ValueString());
				}
			}
		}
		return list;
	}

	public List<string> ListLayoutOverrideParams(CActiveBonus bonus)
	{
		List<string> list = null;
		CVarsContext context = null;
		CVars cVars = null;
		if (MiscVars != null)
		{
			foreach (var miscVar in MiscVars)
			{
				if (miscVar.Item1.StartsWith("ListLayoutParamVar") && list == null)
				{
					list = new List<string>();
					cVars = new CVars();
					context = new CVarsContext(cVars, VarsContext);
					cVars.Set("ActiveBonus", new TValue(bonus));
					break;
				}
				if (miscVar.Item1.Equals("ActiveBonusName"))
				{
					ActiveBonusLayout layout = bonus.Layout;
					TValue item = miscVar.Item2;
					layout.CardName = item.GetString();
				}
			}
			if (list != null)
			{
				foreach (var miscVar2 in MiscVars)
				{
					TValue item = miscVar2.Item2;
					TValue value = item.Calc(context);
					cVars.Set(miscVar2.Item1, value);
					if (miscVar2.Item1.StartsWith("ListLayoutParamVar"))
					{
						list.Add(value.ValueString());
					}
				}
			}
		}
		return list;
	}

	public override string ToString()
	{
		if (Original == this)
		{
			return "[" + MF.RefStr(this) + "] " + GetType().Name + "(" + FileName + ":" + YMLPath + ")";
		}
		return $"[{MF.RefStr(this)}] {GetType().Name}({TargetingActor}.{m_CachedAbilityBaseCard?.Name}.{Name})";
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return GetType().Name + "(" + m_CachedAbilityBaseCard?.Name + "." + Name + ")";
	}

	void IDumpState.DumpMembers(StateDump dump)
	{
	}

	public static string ToJson(CAbility ability, bool force = false)
	{
		string result = null;
		if (force || ability.GetAbilityBaseCard() == null || ability.GetAbilityBaseCard().CardType == CBaseCard.ECardType.Dummy)
		{
			result = JsonConvert.SerializeObject(ability, s_JsonSerializerSettings);
		}
		return result;
	}

	public static CAbility FromJson(string json)
	{
		return JsonConvert.DeserializeObject<CAbility>(json, s_JsonSerializerSettings);
	}

	public virtual string GetStateName()
	{
		return CVarsContext.GetObjectVar(this, "m_State", VarsContext).ValueString();
	}

	public virtual bool Perform()
	{
		if (AbilityHasBeenCancelled)
		{
			return false;
		}
		PerformStateVars("OnAbilityPerform");
		string stateName = GetStateName();
		PerformStateVars(stateName);
		TargetingActor.m_OnAbilityPerformStartedListeners?.Invoke(this);
		if (WasInterrupted)
		{
			CAbilityControlActor cAbilityControlActor = GetMiscVar("WasControlledByAbility").GetObject<CAbilityControlActor>();
			if (cAbilityControlActor != null && TargetingActor.MindControlAbility != cAbilityControlActor)
			{
				SetMiscVar("WasControlledByAbility", TValue.Null);
				CActor targetingActor = TargetingActor;
				CActor targetingActor2 = cAbilityControlActor.TargetingActor;
				if (cAbilityControlActor.ControlActorData.ControlDurationType == CAbilityControlActor.EControlDurationType.ControlForOneAction)
				{
					targetingActor.MindControlDuration = CAbilityControlActor.EControlDurationType.ControlForOneAction;
					targetingActor.MindControlIsFromEnemy = !CActor.AreActorsAllied(targetingActor.Type, targetingActor2.Type, targetingActor, targetingActor2);
					targetingActor.MindControlAbility = cAbilityControlActor;
					bool useControllingActorModifierDeck = cAbilityControlActor.ControlActorData.UseControllingActorModifierDeck && cAbilityControlActor.ControlActorData.UseControllingActorModifierDeckFilter.IsValidTarget(targetingActor, targetingActor2, isTargetedAbility: false, useTargetOriginalType: false, false);
					GameState.OverrideCurrentActorForOneAction(targetingActor, cAbilityControlActor.ControlActorData.UseControllingActorType ? new CActor.EType?(targetingActor2.Type) : cAbilityControlActor.ControlActorData.ControlType, killActorAfterAction: false, cAbilityControlActor.ControlActorData.TransferAugments ? targetingActor2.Augments : null, targetingActor2, useControllingActorModifierDeck, cAbilityControlActor.ControlActorData.UseControllingActorPlayerControl, immediatelySetCurrentActor: true);
				}
			}
		}
		return false;
	}

	protected void PerformStateVars(string stateName)
	{
		CalcPerformVars(stateName);
		FirePerformEvent(stateName);
	}

	public virtual void CalcPerformVars(string stateName)
	{
		if (PerformVars == null)
		{
			return;
		}
		List<(string, TValue)> list = null;
		foreach (var (text, list2) in PerformVars)
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
		CVars cVars = VarsContext.obj as CVars;
		if (cVars == null)
		{
			cVars = new CVars();
			VarsContext.obj = cVars;
			cVars.Set("obj", new TValue(this));
		}
		foreach (var item3 in list)
		{
			string item = item3.Item1;
			TValue item2 = item3.Item2;
			TValue tValue = item2.Calc(VarsContext);
			cVars.Set(item, tValue);
			if (tValue.isValid)
			{
				InitValueFromVars(item, item2, tValue);
				SharedAbilityTargeting.ProcessTargetingValue(this, item, item2, tValue);
			}
		}
	}

	public virtual void FirePerformEvent(string stateName)
	{
		CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericActionPerform, this, null, null, ("state", stateName));
	}

	public virtual void Update()
	{
	}

	public virtual CAbility CreateInterruptCopy(CActor targetingActor, bool killAfter, bool forced, bool interruptRegardlessOfStrength = false)
	{
		CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
		TargetingActor = TargetingActor;
		if ((NumberTargetsRemaining - 1 > 0) | interruptRegardlessOfStrength)
		{
			CAbility cAbility = CopyAbility(this, generateNewID: false, fullCopy: true);
			CopyInterruptedProperties(cAbility, Strength, initialCopy: true, forced);
			PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Remove(currentPhaseAbility);
			return cAbility;
		}
		return null;
	}

	public virtual void ForceInterrupt(bool fromTeleport = false)
	{
		PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility>(), TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true, addDirectlyToCurrentAbilities: false, forcedInterrupt: true);
	}

	public virtual void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		AbilityInterruptCopy = destAbility;
		destAbility.m_CachedAIMoveFocusActors = m_CachedAIMoveFocusActors?.ToList();
		destAbility.m_AbilityStartComplete = m_AbilityStartComplete;
		destAbility.ShouldBePlayerControlled = ShouldBePlayerControlled;
		destAbility.WasInterrupted = true;
		destAbility.WasForcedInterrupted = forced;
		destAbility.NumberTargets = NumberTargetsRemaining - 1;
		destAbility.Strength = strength;
		destAbility.m_ModifiedStrength = m_ModifiedStrength;
		destAbility.TargetingActor = TargetingActor;
		destAbility.ActorsTargeted = ActorsTargeted?.ToList();
		destAbility.ActiveConditionalOverrides = ActiveConditionalOverrides?.ToList();
		destAbility.m_CurrentOverrides = m_CurrentOverrides?.ToList();
		destAbility.DontMoveAbilityState = DontMoveAbilityState;
		destAbility.m_ActorsToIgnore = m_ActorsToIgnore?.ToList();
		destAbility.m_ValidActorsInRange = m_ValidActorsInRange?.ToList();
		destAbility.m_ActorsToTarget = m_ActorsToTarget?.ToList();
		destAbility.m_HasEnded = m_HasEnded;
		destAbility.AbilityHasHappened = AbilityHasHappened;
		if (AreaEffect != null)
		{
			destAbility.m_AreaEffectLocked = AreaEffectLocked;
			destAbility.AreaEffectAngle = AreaEffectAngle;
			destAbility.AreaEffectFlipped = AreaEffectFlipped;
			destAbility.LastSelectedAreaEffectTile = LastSelectedAreaEffectTile;
		}
		destAbility.SetMiscVar("WasControlledByAbility", new TValue(TargetingActor?.MindControlAbility));
	}

	public virtual void PerformImmediate()
	{
	}

	public virtual void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		CFinishedProcessingTileSelected_MessageData message = new CFinishedProcessingTileSelected_MessageData
		{
			m_SelectedTile = selectedTile,
			m_OptionalTileList = optionalTileList,
			m_Ability = this,
			m_AreaEffectLocked = m_AreaEffectLocked
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public virtual void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		CFinishedProcessingTileDeselected_MessageData message = new CFinishedProcessingTileDeselected_MessageData
		{
			m_SelectedTile = selectedTile,
			m_OptionalTileList = optionalTileList,
			m_Ability = this,
			m_CanUndo = m_CanUndo,
			m_AreaEffectLocked = m_AreaEffectLocked
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool IgnoreDisarm()
	{
		return GetMiscVar("IgnoreDisarm").GetBool();
	}

	public virtual bool StoppedByDisarm(CActor targetingActor = null)
	{
		return false;
	}

	public virtual bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		fullAbilityRestart = true;
		return true;
	}

	public virtual void AbilityPassStep()
	{
	}

	public virtual void AbilityEnded()
	{
		if (m_HasEnded)
		{
			return;
		}
		m_HasEnded = true;
		TargetingActor.OverrideArrayIndex = default;
		foreach (CActiveBonus activeSingleTargetActiveBonuse in ActiveSingleTargetActiveBonuses)
		{
			activeSingleTargetActiveBonuse.SingleTarget = null;
		}
		foreach (CItem activeSingleTargetItem in ActiveSingleTargetItems)
		{
			activeSingleTargetItem.SingleTarget = null;
		}
		if (m_AbilityHasHappened && AbilityInterruptCopy == null)
		{
			if (TargetingActor is CPlayerActor cPlayerActor && Flags.HasFlag(EAbilityFlag.ModifiesTimeTokens) && AbilityInterruptCopy == null && Tokens != 0)
			{
				cPlayerActor.SetTimeTokens(cPlayerActor.GetTimeTokens() + Tokens);
			}
			TargetingActor.LastAbilityPerformed = this;
			TargetingActor.AbilityTypesPerformedThisTurn.Add(AbilityType);
			if (ActiveBonusData != null && ActiveBonusData.OverrideAsSong)
			{
				TargetingActor.AbilityTypesPerformedThisTurn.Add(EAbilityType.PlaySong);
			}
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.XP.HasValue && MiscAbilityData.XP.Value > 0)
			{
				TargetingActor.GainXP(MiscAbilityData.XP.Value, XPSourceKey);
			}
			if (ControllingActor != null)
			{
				ControllingActor.GainXP(AbilityXP, XPSourceKey);
				CAbilityRequirements startAbilityRequirements = StartAbilityRequirements;
				if (startAbilityRequirements != null && startAbilityRequirements.XP > 0)
				{
					ControllingActor.GainXP(StartAbilityRequirements.XP, XPSourceKey);
				}
				if (ActiveConditionalOverrides.Count > 0)
				{
					foreach (CConditionalOverride activeConditionalOverride in ActiveConditionalOverrides)
					{
						if (activeConditionalOverride.XP > 0)
						{
							if (CActiveBonus.FindApplicableActiveBonuses(ControllingActor, EAbilityType.RepeatAbility).SingleOrDefault() is CRepeatAbilityActiveBonus cRepeatAbilityActiveBonus && cRepeatAbilityActiveBonus.RequirementsMet(ControllingActor) && (Ability?.StartAbilityRequirements == null || !cRepeatAbilityActiveBonus.IsAgilityValid(Ability.StartAbilityRequirements.RequiredAgility)))
							{
								ControllingActor.GainXP(activeConditionalOverride.XP * 2, XPSourceKey);
							}
							else
							{
								ControllingActor.GainXP(activeConditionalOverride.XP, XPSourceKey);
							}
						}
					}
				}
			}
			else
			{
				TargetingActor.GainXP(AbilityXP, XPSourceKey);
				CAbilityRequirements startAbilityRequirements2 = StartAbilityRequirements;
				if (startAbilityRequirements2 != null && startAbilityRequirements2.XP > 0)
				{
					TargetingActor.GainXP(StartAbilityRequirements.XP, XPSourceKey);
				}
				if (ActiveConditionalOverrides != null && ActiveConditionalOverrides.Count > 0)
				{
					foreach (CConditionalOverride activeConditionalOverride2 in ActiveConditionalOverrides)
					{
						if (activeConditionalOverride2.XP > 0)
						{
							if (CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.RepeatAbility).SingleOrDefault() is CRepeatAbilityActiveBonus cRepeatAbilityActiveBonus2 && cRepeatAbilityActiveBonus2.RequirementsMet(TargetingActor) && (Ability?.StartAbilityRequirements == null || !cRepeatAbilityActiveBonus2.IsAgilityValid(Ability.StartAbilityRequirements.RequiredAgility)))
							{
								TargetingActor.GainXP(activeConditionalOverride2.XP * 2, XPSourceKey);
							}
							else
							{
								TargetingActor.GainXP(activeConditionalOverride2.XP, XPSourceKey);
							}
						}
					}
				}
			}
			if ((Flags & EAbilityFlag.ModifiesResonanceTokens) != 0 && TargetingActor is CPlayerActor cPlayerActor2)
			{
				cPlayerActor2.SetResonanceTokens(cPlayerActor2.GetResonanceTokens() + Tokens);
			}
			if (m_InfuseElements.Count > 0)
			{
				List<CAbility> list = new List<CAbility>();
				CActor cActor;
				if (ControllingActor != null)
				{
					cActor = ControllingActor;
				}
				else
				{
					cActor = ((TargetingActor is CHeroSummonActor cHeroSummonActor) ? cHeroSummonActor.Summoner : TargetingActor);
				}
				CBaseCard parentBaseCard = cActor.FindCardWithAbility(this);
				CAbility cAbility = CreateAbility(EAbilityType.Infuse, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), IsMonsterAbility, isTargetedAbility: false, 0, 1, 1, 0, EAbilityFlag.None, 1, EConditionDecTrigger.Turns, m_InfuseElements, showElementPicker: true, parentBaseCard);
				cAbility.ParentAbility = this;
				list.Add(cAbility);
				(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(list, cActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true);
			}
			if (ResourcesToAddOnAbilityEnd != null && ResourcesToAddOnAbilityEnd.Count > 0 && TargetingActor is CPlayerActor cPlayerActor3)
			{
				foreach (KeyValuePair<string, int> item in ResourcesToAddOnAbilityEnd)
				{
					cPlayerActor3.AddCharacterResource(item.Key, item.Value);
				}
			}
		}
		CPhaseAction.LogActionPerform(clearElements: true);
		CAbilityEndUpdateCombatLog_MessageData message = new CAbilityEndUpdateCombatLog_MessageData(GameState.InternalCurrentActor)
		{
			m_AbilityType = AbilityType,
			m_Ability = this
		};
		ScenarioRuleClient.MessageHandler(message);
		if (MiscAbilityData != null && MiscAbilityData.SetHPTo.HasValue && MiscAbilityData.SetHPTo.Value > 0 && MiscAbilityData.SetHPTo.Value <= GameState.InternalCurrentActor.OriginalMaxHealth)
		{
			GameState.InternalCurrentActor.ApplyImmediateDamage(GameState.InternalCurrentActor.Health - MiscAbilityData.SetHPTo.Value, cannotPrevent: true);
		}
		AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
		if (miscAbilityData2 != null && miscAbilityData2.ExhaustSelf == true)
		{
			GameState.InternalCurrentActor.ExhaustAfterAction = true;
		}
		CActiveBonus.RefreshAllAuraActiveBonuses(this);
		CActiveBonus.RefreshOverhealActiveBonuses();
		TargetingActor.CalculateAttackStrengthForUI();
		if (TargetingActor.m_OnAbilityEndedListeners != null)
		{
			TargetingActor.m_OnAbilityEndedListeners(this);
		}
		TargetingActor.ClearCharacterSpecialMechanicsCache(clearAugments: true, clearSongs: true);
		if (AbilityHasHappened)
		{
			foreach (CScenarioModifier item2 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) =>
			{
				if (m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAbility && (m.AfterAbilityTypes == null || m.AfterAbilityTypes.Contains(AbilityType)))
				{
					CAbilityAttack attackAbility = this as CAbilityAttack;
					if (attackAbility == null)
					{
						return true;
					}
					if (m.AfterAttackTypes == null)
					{
						return true;
					}
					return m.AfterAttackTypes.Any((EAttackType x) => attackAbility.IsAttackType(x));
				}
				return false;
			}))
			{
				item2.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
			}
		}
		if (AbilityHasHappened || (TargetingActor != ControllingActor && ControllingActor != null))
		{
			TargetingActor?.Inventory?.LockInSelectedItemsAndResetUnselected();
		}
		PerformStateVars("OnAbilityEnded");
		if (GetMiscVar("TriggerBeforeRetaliate").GetBool() && TargetThisActorAutomatically != null)
		{
			CAbilityAttack.ApplyRetaliateToParentAttack(this, TargetThisActorAutomatically);
		}
		if (GetMiscVar("Infusion").GetBool() && !AbilityHasBeenCancelled && AbilityInterruptCopy == null)
		{
			GameState.PlayedInfusion = true;
		}
	}

	public virtual void LogEvent(ESESubTypeAbility subTypeAbility)
	{
	}

	public virtual string GetDescription()
	{
		return "";
	}

	public virtual void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		bool abilityStartListenersInvoked = AbilityStartListenersInvoked;
		if (filterActor == null)
		{
			filterActor = targetingActor;
		}
		if (AreaEffect != null && GetMiscVar("NoAreaEffectExplicit").GetBool())
		{
			AreaEffect = null;
		}
		if (GetMiscVar("ShouldBePlayerControlled").GetBool())
		{
			ShouldBePlayerControlled = true;
		}
		m_LogAfterAppliedMessages.Clear();
		if (Original == this)
		{
			Log.Error($"Trying to start an original SRLYML ability {this} for {targetingActor}");
		}
		TargetingActor = targetingActor;
		FilterActor = filterActor;
		ControllingActor = controllingActor;
		m_ActorsToTarget = new List<CActor>();
		ActorsTargeted = new List<CActor>();
		m_TilesSelected = new List<CTile>();
		TilesInRange = new List<CTile>();
		InitVars();
		if (TargetingOverrideArrayIndex.IsValid && !GetVar("DontOverrideArrayIndex").GetBool())
		{
			TargetingActor.OverrideArrayIndex = TargetingOverrideArrayIndex;
			CActiveBonus.RefreshAllAuraActiveBonuses();
		}
		if (ActiveBonusData == null || ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			ProcessSongOverridesAndAbilities(TargetingActor, CSong.ESongActivationType.AbilityStart);
		}
		CheckForScenarioModifiersToTrigger();
		if (targetingActor != null && targetingActor.Class is CMonsterClass cMonsterClass && cMonsterClass.CurrentMonsterStat.SpecialBaseStats.ContainsKey(AbilityType))
		{
			Strength = cMonsterClass.CurrentMonsterStat.SpecialBaseStats[AbilityType];
		}
		if (!AppliedEnhancements)
		{
			ApplyEnhancement(AbilityEnhancements);
			AppliedEnhancements = true;
		}
		foreach (CScenarioModifierAddConditionsToAbilities item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.AddConditionsToAbilities))
		{
			if (!item.ShouldAddConditions(TargetingActor, AbilityType))
			{
				continue;
			}
			foreach (CCondition.EPositiveCondition posCondition in item.PositiveConditions)
			{
				EAbilityType abilityType = AbilityTypes.Single((EAbilityType x) => x.ToString() == posCondition.ToString());
				if (!m_PositiveConditions.ContainsKey(posCondition))
				{
					m_PositiveConditions.Add(posCondition, CreateAbility(abilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
			}
			if (item.IgnoreNegativeScenarioEffects(TargetingActor))
			{
				continue;
			}
			foreach (CCondition.ENegativeCondition negativeCondition in item.NegativeConditions)
			{
				EAbilityType abilityType2 = AbilityTypes.Single((EAbilityType x) => x.ToString() == negativeCondition.ToString());
				if (!m_NegativeConditions.ContainsKey(negativeCondition))
				{
					m_NegativeConditions.Add(negativeCondition, CreateAbility(abilityType2, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
			}
		}
		if (ConditionalOverrides != null)
		{
			ActiveConditionalOverrides = new List<CConditionalOverride>();
			foreach (CConditionalOverride conditionalOverride in ConditionalOverrides)
			{
				CAbility ability = this;
				if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
				{
					ability = ParentAbility;
				}
				else if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
				{
					ability = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last().m_Ability;
				}
				CAbilityFilter.CurrentVarsContext = VarsContext;
				if (conditionalOverride.Self && conditionalOverride.Filter.IsValidTarget(TargetingActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible) && conditionalOverride.Requirements.MeetsAbilityRequirements(TargetingActor, ability))
				{
					foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
					{
						OverrideAbilityValues(abilityOverride, perform: false, null, conditionalOverride.Filter);
					}
					ActiveConditionalOverrides.Add(conditionalOverride);
				}
				CAbilityFilter.CurrentVarsContext = null;
			}
		}
		CAbilityFilter.CurrentVarsContext = VarsContext;
		AbilityFilter.IsValidTarget(TargetingActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible);
		SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
		CAbilityFilter.CurrentVarsContext = null;
		if (m_ActorsToIgnore == null)
		{
			m_ActorsToIgnore = new List<CActor>();
		}
		if (MiscAbilityData?.IgnorePreviousAbilityTargets != null)
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.IgnorePreviousAbilityTargets.Count > 0 && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				foreach (string abilityName in MiscAbilityData?.IgnorePreviousAbilityTargets)
				{
					CPhaseAction.CPhaseAbility cPhaseAbility = cPhaseAction.PreviousPhaseAbilities.FirstOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == abilityName);
					if (cPhaseAbility != null)
					{
						m_ActorsToIgnore.AddRange(cPhaseAbility.m_Ability.ActorsTargeted);
					}
				}
				m_ActorsToIgnore = m_ActorsToIgnore.Distinct().ToList();
			}
		}
		ValidTilesInAreaAffected.Clear();
		if (!MeetsStartAbilityRequirements(targetingActor))
		{
			SetCancelAbilityFlag(cancel: true);
		}
		if (TargetingActor.PhasedOut && GetMiscVar("ProcessWhilePhasedOut").GetBool())
		{
			ProcessIfDead = true;
		}
		if (targetingActor.IsDead && !ProcessIfDead)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		if (MiscAbilityData != null && MiscAbilityData.StartIfParentHasHappened && ParentAbility != null && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
		{
			CPhaseAction.CPhaseAbility cPhaseAbility2 = cPhaseAction2.PreviousPhaseAbilities.Find((CPhaseAction.CPhaseAbility cPhaseAbility3) => cPhaseAbility3.m_Ability.ID == ParentAbility.ID);
			if (cPhaseAbility2 == null || !cPhaseAbility2.m_Ability.AbilityHasHappened)
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		m_ModifiedStrength = Strength;
		CVarsContext currentVarsContext = CAbilityFilter.CurrentVarsContext;
		CAbilityFilter.CurrentVarsContext = VarsContext;
		ResetBuffs();
		if (!HasStarted)
		{
			m_OriginalAreaEffect = AreaEffect;
			m_OriginalTargetCount = NumberTargets;
			m_OriginalRange = Range;
		}
		ApplyBuffs(TargetingActor);
		CAbilityFilter.CurrentVarsContext = currentVarsContext;
		AbilityHasHappened = false;
		m_AreaEffectLocked = false;
		if (!AbilityStartListenersInvoked)
		{
			AbilityStartListenersInvoked = true;
			TargetingActor.m_OnAbilityStartedListeners?.Invoke(this);
		}
		AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
		if (miscAbilityData2 != null && miscAbilityData2.CanSkip == false)
		{
			m_CanSkip = false;
		}
		if (!abilityStartListenersInvoked)
		{
			CStartedAbility_MessageData message = new CStartedAbility_MessageData
			{
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected void ExpandTileListWithLinkedProps(List<CTile> tileList)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (!ShouldCheckLinkedTiles())
			{
				return;
			}
			List<CTile> list = new List<CTile>();
			foreach (CTile tile in tileList)
			{
				foreach (CTile linkedTile in GetLinkedTiles(tile))
				{
					if (!list.Contains(linkedTile) && !tileList.Contains(linkedTile))
					{
						list.Add(linkedTile);
					}
				}
			}
			tileList.AddRange(list);
		}
	}

	public IEnumerable<CTile> GetLinkedTiles(CTile tile)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (tile == null)
			{
				yield break;
			}
			bool showProps = ShouldCheckLinkedTiles();
			if (showProps)
			{
				lock (tile.m_Props)
				{
					foreach (CObjectProp prop in tile.m_Props)
					{
						if (!prop.HasLinkedProps)
						{
							continue;
						}
						foreach (CObjectProp item in prop.GetLinkedPropsTotal())
						{
							yield return item.PropTile;
						}
					}
				}
			}
			bool showActors = IsCurrentlyTargetingActors();
			if (!(showProps | showActors))
			{
				yield break;
			}
			List<CObjectProp> getPropsThreadSafe = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe;
			foreach (CObjectProp item2 in getPropsThreadSafe)
			{
				if (!showProps && !((item2.RuntimeAttachedActor != null) & showActors))
				{
					continue;
				}
				List<Point> tiles = item2.GetTiles();
				if (item2.RuntimeAttachedActor != null)
				{
					CActor mirrorHealthActor = item2.RuntimeAttachedActor.GetMirrorHealthActor();
					if (mirrorHealthActor != null)
					{
						tiles.AddRange(from x in mirrorHealthActor.GetTilesCanAttackFrom()
							select x.m_ArrayIndex);
					}
				}
				if (!tiles.Contains(tile.m_ArrayIndex))
				{
					continue;
				}
				foreach (Point item3 in tiles)
				{
					if (!(item3 == tile.m_ArrayIndex))
					{
						yield return ScenarioManager.GetTile(item3);
					}
				}
			}
			if (!showActors)
			{
				yield break;
			}
			List<CActor> allAliveActorsThreadSafe = ScenarioManager.Scenario.AllAliveActorsThreadSafe;
			foreach (CActor item4 in allAliveActorsThreadSafe)
			{
				if (item4 is CObjectActor { AttachedProp: not null } || item4.MultiHexArrayIndexes == null || item4.MultiHexArrayIndexes.Count == 0)
				{
					continue;
				}
				bool flag = false;
				foreach (CTile multiHexTile in item4.MultiHexTiles)
				{
					if (multiHexTile == tile)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				foreach (CTile multiHexTile2 in item4.MultiHexTiles)
				{
					if (multiHexTile2 != tile)
					{
						yield return multiHexTile2;
					}
				}
			}
		}
	}

	protected virtual void ResetBuffs()
	{
		m_RangeBuff = 0;
		m_TargetCountBuff = 0;
		m_PositiveConditionsBuff = null;
		m_NegativeConditionsBuff = null;
	}

	protected virtual void ApplyBuffs(CActor actor)
	{
		m_BlockPerformAndRestart = true;
		foreach (CActiveBonus item in from x in CActiveBonus.FindApplicableActiveBonuses(actor)
			where x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.OverrideAbility)
			select x)
		{
			if (item.Ability.ActiveBonusData.IsToggleBonus && item.IsActiveBonusToggledAndNotRestricted(actor))
			{
				COverrideAbilityTypeActiveBonus_OverrideAbility behaviour = item.GetBehaviour<COverrideAbilityTypeActiveBonus_OverrideAbility>();
				if (!item.Ability.ActiveBonusData.ActiveBonusAbilityOverrides.Any((CAbilityOverride x) => x.AbilityType.HasValue && x.AbilityType.Value != AbilityType))
				{
					behaviour.OnActiveBonusToggled(this, item, toggledOn: true);
				}
			}
		}
		m_BlockPerformAndRestart = false;
	}

	public void SetCancelAbilityFlag(bool cancel, bool forceImmediate = false)
	{
		if (cancel)
		{
			if (CancelIfNoValid | forceImmediate)
			{
				m_CancelAbility = true;
			}
			else
			{
				m_ConfirmCancelled = true;
			}
		}
		else
		{
			m_CancelAbility = false;
			m_ConfirmCancelled = false;
		}
	}

	public void SetHasStarted(bool val)
	{
		m_AbilityStartComplete = val;
	}

	public void SetCancelReason(string reason)
	{
		SetMiscVar("HelpTextWhenCancelled", reason);
		SetMiscVar("DontCancelIfNoValid", true);
	}

	private void InitVars()
	{
		if (Vars == null)
		{
			return;
		}
		CVars cVars = VarsContext.obj as CVars;
		if (cVars == null)
		{
			cVars = new CVars();
			VarsContext.obj = cVars;
		}
		cVars.Clear();
		cVars.Set("obj", new TValue(this));
		foreach (var (key, expressionValue) in Vars)
		{
			if (AbilityHasBeenCancelled)
			{
				break;
			}
			TValue value = expressionValue.Calc(VarsContext);
			cVars.Set(key, value);
			if (value.isValid)
			{
				InitValueFromVars(key, expressionValue, value);
			}
		}
	}

	public void RecalcForcePreviewVars()
	{
		CVars cVars = VarsContext.obj as CVars;
		if (cVars == null)
		{
			cVars = new CVars();
			VarsContext.obj = cVars;
		}
		cVars.Set("obj", new TValue(this));
		foreach (var (text, expressionValue) in Vars)
		{
			if (text.StartsWith("ForcePreviewTiles"))
			{
				if (AbilityHasBeenCancelled)
				{
					break;
				}
				TValue value = expressionValue.Calc(VarsContext);
				cVars.Set(text, value);
				if (value.isValid)
				{
					InitValueFromVars(text, expressionValue, value);
				}
			}
		}
	}

	public TValue GetVar(string key, CVarsContext context = null, bool asValue = true)
	{
		if (!(VarsContext.obj is CVars cVars))
		{
			return TValue.Unknown;
		}
		return cVars.Get(key);
	}

	public TValue GetMiscVar(string key)
	{
		if (MiscVars == null)
		{
			return TValue.Unknown;
		}
		(string, TValue) tuple = MiscVars.Find(((string, TValue) x) => x.Item1 == key);
		(string, TValue) tuple2 = tuple;
		if (tuple2.Item1 == null && tuple2.Item2 == default(TValue))
		{
			return TValue.Unknown;
		}
		return tuple.Item2;
	}

	public void SetMiscVar(string key, TValue value)
	{
		if (MiscVars == null)
		{
			MiscVars = new List<(string, TValue)>();
		}
		RemoveMiscVar(key);
		MiscVars.Add((key, value));
	}

	public void RemoveMiscVar(string key)
	{
		if (MiscVars != null)
		{
			MiscVars.RemoveAll(((string, TValue) x) => x.Item1 == key);
		}
	}

	public virtual bool InitValueFromVars(string key, TValue expressionValue, TValue value)
	{
		switch (key)
		{
		case "Strength":
			m_Strength = value.GetInt();
			return true;
		case "Range":
			m_Range = value.GetInt();
			return true;
		case "Target":
			m_NumberTargets = value.GetInt();
			return true;
		case "Tokens":
			Tokens = value.GetInt();
			return true;
		case "AnimOverload":
			AnimOverload = value.GetString();
			return true;
		default:
			if (key.StartsWith("Validate", StringComparison.Ordinal))
			{
				if (!value.GetBool())
				{
					SetCancelAbilityFlag(cancel: true);
				}
				return true;
			}
			if (key.StartsWith("AddNegativeConditions", StringComparison.Ordinal))
			{
				IList list = value.GetObject<IList>();
				if (list == null)
				{
					Log.Error($"Non-list value of AddNegativeConditions: {expressionValue} -> {value}");
					return true;
				}
				foreach (object item in list)
				{
					if (!(item is CCondition.ENegativeCondition condition))
					{
						Log.Error($"Non-ENegativeCondition value in AddNegativeConditions: {value}");
					}
					else
					{
						AddNegativeCondition(condition);
					}
				}
				return true;
			}
			if (key.StartsWith("AddNegativeCondition", StringComparison.Ordinal))
			{
				if (!value.GetEnum(out var val, null, CCondition.ENegativeCondition.NA))
				{
					Log.Error($"Non-ENegativeCondition value of AddNegativeCondition: {expressionValue} -> {value}");
					return true;
				}
				AddNegativeCondition(val);
				return true;
			}
			if (key.StartsWith("AddPositiveConditions", StringComparison.Ordinal))
			{
				IList list2 = value.GetObject<IList>();
				if (list2 == null)
				{
					Log.Error($"Non-list value of AddPositiveConditions: {expressionValue} -> {value}");
					return true;
				}
				foreach (object item2 in list2)
				{
					if (!(item2 is CCondition.EPositiveCondition condition2))
					{
						Log.Error($"Non-EPositiveCondition value in AddPositiveConditions: {value}");
					}
					else
					{
						AddPositiveCondition(condition2);
					}
				}
				return true;
			}
			if (key.StartsWith("AddPositiveCondition", StringComparison.Ordinal))
			{
				if (!value.GetEnum(out var val2, null, CCondition.EPositiveCondition.NA))
				{
					Log.Error($"Non-EPositiveCondition value of AddPositiveCondition: {expressionValue} -> {value}");
					return true;
				}
				AddPositiveCondition(val2);
				return true;
			}
			if (key.StartsWith("TargetThisActorAutomatically", StringComparison.Ordinal))
			{
				CActor targetThisActorAutomatically = value.GetObject<CActor>(VarsContext);
				TargetThisActorAutomatically = targetThisActorAutomatically;
				return true;
			}
			if (key.StartsWith("ForcePreviewTiles", StringComparison.Ordinal))
			{
				CForceShowTilePreview_MessageData cForceShowTilePreview_MessageData = new CForceShowTilePreview_MessageData(TargetingActor);
				cForceShowTilePreview_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cForceShowTilePreview_MessageData);
				return true;
			}
			if (key.StartsWith("TargetingOverrideArrayIndex", StringComparison.Ordinal))
			{
				CTile cTile = value.GetObject<CTile>(VarsContext);
				if (cTile != null)
				{
					TargetingOverrideArrayIndex = cTile.m_ArrayIndex;
				}
				return true;
			}
			if (key.StartsWith("TilesSelected", StringComparison.Ordinal))
			{
				List<CTile> list3 = value.GetObject<List<CTile>>(VarsContext);
				if (list3 != null)
				{
					m_TilesSelected = list3;
				}
				return true;
			}
			return false;
		}
	}

	public void AddNegativeCondition(CCondition.ENegativeCondition condition)
	{
		if (condition != CCondition.ENegativeCondition.NA && !NegativeConditions.ContainsKey(condition))
		{
			EAbilityType eAbilityType = AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition.ToString());
			if (eAbilityType != EAbilityType.None)
			{
				CAbility value = CreateAbility(eAbilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility);
				NegativeConditions.Add(condition, value);
			}
		}
	}

	public void AddPositiveCondition(CCondition.EPositiveCondition condition)
	{
		if (condition == CCondition.EPositiveCondition.NA)
		{
			return;
		}
		if (PositiveConditions.ContainsKey(condition))
		{
			if (condition == CCondition.EPositiveCondition.Bless && PositiveConditions.TryGetValue(condition, out var value))
			{
				value.Strength++;
			}
			return;
		}
		EAbilityType eAbilityType = AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition.ToString());
		if (eAbilityType != EAbilityType.None)
		{
			CAbility cAbility = CreateAbility(eAbilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility);
			cAbility.Strength = 1;
			PositiveConditions.Add(condition, cAbility);
		}
	}

	public bool HasVarsContext(CVarsContext context)
	{
		if (context == null)
		{
			return false;
		}
		for (CVarsContext cVarsContext = VarsContext; cVarsContext != null; cVarsContext = cVarsContext.parentContext)
		{
			if (cVarsContext.ID == context.ID)
			{
				return true;
			}
		}
		return false;
	}

	public void AddParentContext(CVarsContext context)
	{
		if (context == null)
		{
			return;
		}
		if (context.ID == VarsContext.ID)
		{
			Log.Error($"Attempting to add own vars context as parent {context}");
			return;
		}
		if (VarsContext.parentContext == null)
		{
			VarsContext.parentContext = context;
			return;
		}
		AddParentContext(context.parentContext);
		if (!HasVarsContext(context))
		{
			VarsContext.parentContext = new CVarsContext(context.obj, VarsContext.parentContext, context.ID);
		}
	}

	public void AddSubAbility(CAbility subAbility, bool isInlineSubAbility = false)
	{
		SubAbilities.Add(subAbility);
		subAbility.ParentAbility = this;
		subAbility.IsInlineSubAbility = isInlineSubAbility;
		subAbility.ParentName = Name;
		subAbility.ParentAbilityBaseCard = GetAbilityBaseCard();
	}

	public void ProcessSongOverridesAndAbilities(CActor targetingActor, CSong.ESongActivationType songActivationType, bool temporaryOverrides = false)
	{
		if (m_SongAbilitiesProcessed && m_SongOverridesProcessed && songActivationType != CSong.ESongActivationType.ActionStart)
		{
			return;
		}
		m_AbilitySongs = new List<CSong.SongEffect>();
		m_OverrideSongs = new List<CSong.SongEffect>();
		foreach (CActiveBonus item in CActiveBonus.FindAllApplicableSongActiveBonuses(targetingActor))
		{
			if ((item.Ability.ActiveBonusData.IsToggleBonus && targetingActor.Type == CActor.EType.Player) || item.IsRestricted(targetingActor))
			{
				continue;
			}
			foreach (CSong.SongEffect songEffect in item.Ability.Song.SongEffects)
			{
				bool flag = false;
				bool flag2 = false;
				CAbility cAbility = null;
				if (songEffect.SongActivationType != songActivationType)
				{
					continue;
				}
				if (songEffect.SongEffectType == CSong.ESongEffectType.Ability)
				{
					if (this is CAbilityMerged cAbilityMerged)
					{
						foreach (CAbility mergedAbility in cAbilityMerged.MergedAbilities)
						{
							if (songEffect.AbilityType == mergedAbility.AbilityType)
							{
								flag = true;
								cAbility = mergedAbility;
								break;
							}
						}
					}
					else if ((ParentAbility == null || !(ParentAbility is CAbilityMerged)) && songEffect.AbilityType == AbilityType)
					{
						flag = true;
						cAbility = this;
					}
				}
				else if (songEffect.AbilityType == AbilityType)
				{
					flag = true;
					cAbility = this;
				}
				if (flag)
				{
					if (cAbility is CAbilityAttack cAbilityAttack)
					{
						if (cAbilityAttack.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA && (songEffect.AttackType == EAttackType.Attack || (songEffect.AttackType == EAttackType.Melee && cAbilityAttack.IsMeleeAttack) || (songEffect.AttackType == EAttackType.Ranged && !cAbilityAttack.IsMeleeAttack)))
						{
							flag2 = true;
						}
					}
					else
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					if (!temporaryOverrides)
					{
						item.RestrictActiveBonus(targetingActor);
					}
					if (songEffect.SongEffectType == CSong.ESongEffectType.Ability)
					{
						m_AbilitySongs.Add(songEffect);
					}
					else
					{
						m_OverrideSongs.Add(songEffect);
					}
				}
			}
		}
		if ((!m_SongOverridesProcessed || songActivationType == CSong.ESongActivationType.ActionStart) && m_OverrideSongs.Count > 0)
		{
			foreach (CSong.SongEffect overrideSong in m_OverrideSongs)
			{
				foreach (CAbilityOverride abilityOverride in overrideSong.AbilityOverrides)
				{
					OverrideAbilityValues(abilityOverride, perform: false);
				}
			}
		}
		if ((!m_SongAbilitiesProcessed || songActivationType == CSong.ESongActivationType.ActionStart) && m_AbilitySongs.Count > 0 && !targetingActor.IsUsingOnDeathAbility && !temporaryOverrides)
		{
			List<CAbility> list = new List<CAbility>();
			foreach (CSong.SongEffect abilitySong in m_AbilitySongs)
			{
				list.AddRange(abilitySong.Abilities);
			}
			if (list.Count > 0)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, null, stopPlayerUndo: false, null, ignorePerformNow: true);
				if (GameState.OverridingCurrentActor && GameState.InternalCurrentActor != TargetingActor)
				{
					GameState.OverrideCurrentActorForOneAction(TargetingActor, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				}
			}
		}
		if (songActivationType != CSong.ESongActivationType.ActionStart)
		{
			m_SongOverridesProcessed = true;
			m_SongAbilitiesProcessed = true;
		}
	}

	public void CheckForScenarioModifiersToTrigger()
	{
		if (MiscAbilityData?.TriggeredScenarioModifiers == null)
		{
			return;
		}
		int i;
		for (i = 0; i < MiscAbilityData?.TriggeredScenarioModifiers.Count; i++)
		{
			CScenarioModifier cScenarioModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier m) => m.EventIdentifier == MiscAbilityData?.TriggeredScenarioModifiers[i]);
			if (cScenarioModifier != null)
			{
				List<CActor> list = new List<CActor>();
				if (MiscAbilityData.TriggerScenarioModifierOnSelfOnly.HasValue && MiscAbilityData.TriggerScenarioModifierOnSelfOnly.Value)
				{
					list.Add(TargetingActor);
				}
				else
				{
					list.AddRange(ScenarioManager.Scenario.AllActors);
				}
				for (int num = 0; num < list.Count; num++)
				{
					cScenarioModifier.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, list[num], ScenarioManager.CurrentScenarioState.Players.Count);
				}
			}
		}
	}

	public virtual void ApplyEnhancement(List<CEnhancement> enhancements)
	{
		List<CAreaEffect.CAreaEffectHex> source = AreaEffect?.DisabledHexes?.ToList();
		foreach (CEnhancement enhancement in enhancements)
		{
			switch (enhancement.Enhancement)
			{
			case EEnhancement.Area:
				if (AreaEffect != null)
				{
					List<CAreaEffect.CAreaEffectHex> list = source.Where((CAreaEffect.CAreaEffectHex x) => x.Flags != CAreaEffect.EHexFlag.None).ToList();
					int enhancementSlot = enhancement.EnhancementSlot;
					_ = list[enhancementSlot];
					list[enhancementSlot].SetFlags(CAreaEffect.EHexFlag.Enabled);
				}
				break;
			case EEnhancement.PlusTeleport:
				if (this is CAbilityTeleport2)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusMove:
				if (this is CAbilityMove)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusAttack:
				if (this is CAbilityAttack)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusDamage:
				if (this is CAbilityDamage)
				{
					Strength++;
				}
				if (this is CAbilityTrap cAbilityTrap2 && cAbilityTrap2.TrapData.Damage > 0)
				{
					cAbilityTrap2.TrapData = cAbilityTrap2.TrapData.Copy();
					cAbilityTrap2.TrapData.Damage++;
				}
				break;
			case EEnhancement.PlusRange:
				Range++;
				break;
			case EEnhancement.PlusShield:
				if (this is CAbilityShield)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusPush:
				if (this is CAbilityPush)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusPull:
				if (this is CAbilityPull)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusPierce:
				if (this is CAbilityAttack cAbilityAttack)
				{
					cAbilityAttack.Pierce++;
				}
				break;
			case EEnhancement.PlusRetaliate:
				if (this is CAbilityRetaliate)
				{
					Strength++;
				}
				break;
			case EEnhancement.PlusRetaliateRange:
				if (this is CAbilityRetaliate cAbilityRetaliate)
				{
					cAbilityRetaliate.RetaliateRange++;
				}
				break;
			case EEnhancement.PlusHeal:
				if (this is CAbilityHeal)
				{
					Strength++;
				}
				if (this is CAbilityTrap cAbilityTrap && cAbilityTrap.TrapData.Heal > 0)
				{
					cAbilityTrap.TrapData = cAbilityTrap.TrapData.Copy();
					cAbilityTrap.TrapData.Heal++;
				}
				break;
			case EEnhancement.PlusTarget:
				NumberTargets++;
				break;
			case EEnhancement.Poison:
			case EEnhancement.Wound:
			case EEnhancement.Muddle:
			case EEnhancement.Immobilize:
			case EEnhancement.Disarm:
				try
				{
					EAbilityType abilityType4 = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
					CCondition.ENegativeCondition key4 = CCondition.NegativeConditions.Single((CCondition.ENegativeCondition s) => s.ToString() == enhancement.Enhancement.ToString());
					if (!m_NegativeConditions.ContainsKey(key4))
					{
						m_NegativeConditions.Add(key4, CreateAbility(abilityType4, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
				}
				catch
				{
					Log.ErrorWithReport("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				}
				break;
			case EEnhancement.Curse:
				try
				{
					CCondition.ENegativeCondition key3 = CCondition.ENegativeCondition.Curse;
					if (AbilityType == EAbilityType.Curse)
					{
						if (Strength == 0)
						{
							Strength = 1;
						}
						Strength++;
					}
					else if (m_NegativeConditions.ContainsKey(key3))
					{
						if (m_NegativeConditions[key3].Strength == 0)
						{
							m_NegativeConditions[key3].Strength++;
						}
						m_NegativeConditions[key3].Strength++;
					}
					else
					{
						EAbilityType abilityType3 = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
						m_NegativeConditions.Add(key3, CreateAbility(abilityType3, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
				}
				catch
				{
					Log.ErrorWithReport("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				}
				break;
			case EEnhancement.Strengthen:
			case EEnhancement.Ward:
			case EEnhancement.Regenerate:
				try
				{
					EAbilityType abilityType2 = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
					CCondition.EPositiveCondition key2 = CCondition.PositiveConditions.Single((CCondition.EPositiveCondition s) => s.ToString() == enhancement.Enhancement.ToString());
					if (!m_PositiveConditions.ContainsKey(key2))
					{
						m_PositiveConditions.Add(key2, CreateAbility(abilityType2, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
				}
				catch
				{
					Log.ErrorWithReport("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				}
				break;
			case EEnhancement.Bless:
				try
				{
					CCondition.EPositiveCondition key = CCondition.EPositiveCondition.Bless;
					if (AbilityType == EAbilityType.Bless)
					{
						if (Strength == 0)
						{
							Strength = 1;
						}
						Strength++;
					}
					else if (m_PositiveConditions.ContainsKey(key))
					{
						if (m_PositiveConditions[key].Strength == 0)
						{
							m_PositiveConditions[key].Strength++;
						}
						m_PositiveConditions[key].Strength++;
					}
					else
					{
						EAbilityType abilityType = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
						m_PositiveConditions.Add(key, CreateAbility(abilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
				}
				catch
				{
					Log.ErrorWithReport("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				}
				break;
			case EEnhancement.Jump:
				if (this is CAbilityMove cAbilityMove)
				{
					cAbilityMove.Jump = true;
				}
				break;
			case EEnhancement.Fire:
			case EEnhancement.Ice:
			case EEnhancement.Air:
			case EEnhancement.Earth:
			case EEnhancement.Light:
			case EEnhancement.Dark:
			case EEnhancement.AnyElement:
			{
				ElementInfusionBoardManager.EElement item;
				try
				{
					item = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement s) => s.ToString() == enhancement.Enhancement.ToString());
				}
				catch
				{
					item = ElementInfusionBoardManager.EElement.Any;
				}
				m_InfuseElements.Add(item);
				break;
			}
			}
		}
	}

	public virtual int ModifiedStrength()
	{
		return m_ModifiedStrength;
	}

	public virtual void SetModifiedStrength(int val)
	{
		m_ModifiedStrength = val;
	}

	public virtual bool HasCondition(CCondition.ENegativeCondition condition, bool ignoreSubAbilities = false, CActor target = null)
	{
		if (condition == CCondition.ENegativeCondition.NA)
		{
			return false;
		}
		if (condition.ToString() == AbilityType.ToString())
		{
			return true;
		}
		if (Augment != null && Augment.AugmentType == CAugment.EAugmentType.Bonus && Augment.Abilities.Count > 0)
		{
			CBaseCard cBaseCard = TargetingActor.FindCardWithAbility(this);
			if (cBaseCard != null)
			{
				foreach (CAbility ability in Augment.Abilities)
				{
					CActiveBonus cActiveBonus = cBaseCard.ActiveBonuses.Find((CActiveBonus x) => x.Ability.ID == ability.ID && x.Actor == TargetingActor);
					if (cActiveBonus != null && ability.ActiveBonusData.IsToggleBonus && cActiveBonus.ToggledBonus && ability.NegativeConditions.ContainsKey(condition))
					{
						return true;
					}
				}
			}
		}
		List<CCondition.ENegativeCondition> list = NegativeConditions.Keys.ToList();
		if (GetMiscVar("PreviewAddNegativeConditionsVar").GetBool())
		{
			CVars cVars = new CVars();
			CVarsContext context = new CVarsContext(cVars);
			cVars.Set("obj", new TValue(this));
			foreach (var var in Vars)
			{
				string item = var.Item1;
				TValue item2 = var.Item2;
				TValue value = item2.Calc(context);
				cVars.Set(item, value);
				if (item.StartsWith("Validate", StringComparison.Ordinal) && !value.GetBool())
				{
					break;
				}
				if (!value.isValid || !(item == "AddNegativeConditions"))
				{
					continue;
				}
				foreach (object item4 in value.GetObject<IList>())
				{
					if (item4 is CCondition.ENegativeCondition item3)
					{
						list.Add(item3);
					}
				}
			}
		}
		if (!list.Contains(condition) && !SubAbilities.Exists((CAbility it) => it.HasCondition(condition, ignoreSubAbilities: false, target) && (!ignoreSubAbilities || it.MiscAbilityData.TreatAsNonSubAbility == false)))
		{
			if (AbilityType == EAbilityType.AddCondition && ActiveBonusData.AbilityData != null)
			{
				return ActiveBonusData.AbilityData.NegativeConditions.Any((KeyValuePair<CCondition.ENegativeCondition, CAbility> it) => it.Key == condition);
			}
			return false;
		}
		return true;
	}

	public virtual bool HasCondition(CCondition.EPositiveCondition condition, bool ignoreSubAbilities = false)
	{
		if (condition == CCondition.EPositiveCondition.NA)
		{
			return false;
		}
		if (condition.ToString() == AbilityType.ToString())
		{
			return true;
		}
		List<CCondition.EPositiveCondition> list = PositiveConditions.Keys.ToList();
		if (GetMiscVar("PreviewAddPositiveConditionsVar").GetBool())
		{
			CVars cVars = new CVars();
			CVarsContext context = new CVarsContext(cVars);
			cVars.Set("obj", new TValue(this));
			foreach (var var in Vars)
			{
				string item = var.Item1;
				TValue item2 = var.Item2;
				TValue value = item2.Calc(context);
				cVars.Set(item, value);
				if (item.StartsWith("Validate", StringComparison.Ordinal) && !value.GetBool())
				{
					break;
				}
				if (!value.isValid || !(item == "AddPositiveConditions"))
				{
					continue;
				}
				foreach (object item4 in value.GetObject<IList>())
				{
					if (item4 is CCondition.EPositiveCondition item3)
					{
						list.Add(item3);
					}
				}
			}
		}
		if (!list.Contains(condition) && !SubAbilities.Exists((CAbility it) => it.HasCondition(condition) && (!ignoreSubAbilities || it.MiscAbilityData.TreatAsNonSubAbility == false)))
		{
			if (AbilityType == EAbilityType.AddCondition && ActiveBonusData.AbilityData != null)
			{
				return ActiveBonusData.AbilityData.PositiveConditions.Any((KeyValuePair<CCondition.EPositiveCondition, CAbility> it) => it.Key == condition);
			}
			return false;
		}
		return true;
	}

	public bool HaveActiveItemsCondition(CCondition.ENegativeCondition condition, CActor appliedToActor = null)
	{
		if (appliedToActor != null && this is CAbilityAttack)
		{
			_ = ((CAbilityAttack)this).AttackSummary;
			return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == appliedToActor && it.HasCondition(condition));
		}
		return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == null && it.HasCondition(condition));
	}

	public bool HaveActiveItemsCondition(CCondition.EPositiveCondition condition, CActor appliedToActor = null)
	{
		if (appliedToActor != null && this is CAbilityAttack)
		{
			_ = ((CAbilityAttack)this).AttackSummary;
			return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == appliedToActor && it.HasCondition(condition));
		}
		return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == null && it.HasCondition(condition));
	}

	public static CAbility CreateAbility(EAbilityType abilityType, CAbilityFilterContainer abilityFilter, bool isMonster, bool isTargetedAbility, int strength = 0, int range = 1, int numberTargets = 1, int tokens = 0, EAbilityFlag flags = EAbilityFlag.None, int conditionDuration = 1, EConditionDecTrigger conditionDecTrigger = EConditionDecTrigger.Turns, List<ElementInfusionBoardManager.EElement> elementsToInfuse = null, bool showElementPicker = false, CBaseCard parentBaseCard = null, bool isModifierAbility = false, List<CItem.EItemSlot> slotsToRefresh = null, List<CItem.EItemSlotState> slotStatesToRefresh = null, List<CItem.EUsageType> slotUsagesToRefresh = null, bool isSub = false, bool isInline = false, string parentName = "")
	{
		return CreateAbility(abilityType, strength, useSpecialBaseStat: false, range, numberTargets, tokens, flags, abilityFilter, string.Empty, string.Empty, new List<CAbility>(), null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, new AbilityData.ActiveBonusData(), null, string.Empty, string.Empty, string.Empty, abilityType.ToString(), new List<CEnhancement>(), null, null, multiPassAttack: true, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, elementsToInfuse, null, isMonster, string.Empty, null, isSub, isInline, 0, 1, isTargetedAbility, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, parentName, null, skipAnim: false, conditionDuration, conditionDecTrigger, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, slotsToRefresh, slotStatesToRefresh, null, EAttackType.None, slotUsagesToRefresh: slotUsagesToRefresh, showElementPicker: showElementPicker, isModifierAbility: isModifierAbility, parentBaseCard: parentBaseCard, chooseAbilities: new List<CAbility>(), recoverCardsAbilityTypeFilter: new List<EAbilityType>(), cardAction: ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	protected static List<CAbility> CopySubAbilities(List<CAbility> subAbilities)
	{
		List<CAbility> list = new List<CAbility>();
		foreach (CAbility subAbility in subAbilities)
		{
			CAbility cAbility = CopyAbility(subAbility, generateNewID: false);
			CAbility parentAbility = cAbility.ParentAbility;
			if (parentAbility != null)
			{
				if (cAbility.AbilityFilter == null && parentAbility.AbilityFilter != null)
				{
					cAbility.AbilityFilter = parentAbility.AbilityFilter.Copy();
				}
				if (cAbility.AreaEffect == null && parentAbility.AreaEffect != null)
				{
					cAbility.AreaEffect = parentAbility.AreaEffect.Copy();
				}
			}
			list.Add(cAbility);
		}
		return list;
	}

	public static CAbility CopyAbility(CAbility ability, bool generateNewID, bool fullCopy = false, bool copyCurrentOverrides = false, bool applyCurrentStatIsBasedOnX = false, CVarsContext parentContext = null, EAbilityType? overrideAbilityType = null)
	{
		if (ability == null)
		{
			return null;
		}
		List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
		List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>();
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in ability.PositiveConditions)
		{
			if (positiveCondition.Key == CCondition.EPositiveCondition.Bless && positiveCondition.Value.Strength > 0)
			{
				for (int i = 0; i < positiveCondition.Value.Strength; i++)
				{
					list.Add(positiveCondition.Key);
				}
			}
			else
			{
				list.Add(positiveCondition.Key);
			}
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in ability.NegativeConditions)
		{
			if (negativeCondition.Key == CCondition.ENegativeCondition.Curse && negativeCondition.Value.Strength > 0)
			{
				for (int j = 0; j < negativeCondition.Value.Strength; j++)
				{
					list2.Add(negativeCondition.Key);
				}
			}
			else
			{
				list2.Add(negativeCondition.Key);
			}
		}
		EAbilityType abilityType = (overrideAbilityType.HasValue ? overrideAbilityType.Value : ability.AbilityType);
		int strength = ability.Strength;
		bool useSpecialBaseStat = ability.UseSpecialBaseStat;
		int range = ability.Range;
		int numberTargets = ability.NumberTargets;
		int tokens = ability.Tokens;
		EAbilityFlag flags = ability.Flags;
		CAbilityFilterContainer abilityFilter = ability.AbilityFilter.Copy();
		string animOverload = ability.AnimOverload;
		string animOverload2 = ability.AnimOverload2;
		List<CAbility> subAbilities = CopySubAbilities(ability.SubAbilities);
		CAreaEffect areaEffect = ability.AreaEffect?.Copy();
		bool attackSourcesOnly = ability is CAbilityPreventDamage && (ability as CAbilityPreventDamage).AttackSourcesOnly;
		bool jump = ability is CAbilityMove && (ability as CAbilityMove).Jump;
		bool fly = ability is CAbilityMove && (ability as CAbilityMove).Fly;
		bool ignoreDifficultTerrain = ability is CAbilityMove && (ability as CAbilityMove).IgnoreDifficultTerrain;
		bool ignoreHazardousTerrain = ability is CAbilityMove cAbilityMove && cAbilityMove.IgnoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost = ability is CAbilityMove cAbilityMove2 && cAbilityMove2.IgnoreBlockedTileMoveCost;
		bool carryOtherActorsOnHex = ability is CAbilityMove cAbilityMove3 && cAbilityMove3.CarryOtherActorsOnHex;
		CAIFocusOverrideDetails aiFocusOverride = ((ability is CAbilityMove cAbilityMove4) ? cAbilityMove4.AIFocusOverride : null);
		CAbilityMove.EMoveRestrictionType moveRestrictionType = ((ability is CAbilityMove cAbilityMove5) ? cAbilityMove5.MoveRestrictionType : CAbilityMove.EMoveRestrictionType.None);
		AbilityData.ActiveBonusData activeBonusData = ability.ActiveBonusData?.Copy();
		ActiveBonusLayout activeBonusYML = ability.ActiveBonusYML?.Copy();
		string areaEffectYMLString = ability.AreaEffectYMLString;
		string areaEffectLayoutOverrideYMLString = ability.AreaEffectLayoutOverrideYMLString;
		string areaEffectFlipAxis = ability.AreaEffectFlipAxis;
		string name = ability.Name;
		List<CEnhancement> abilityEnhancements = ability.m_AbilityEnhancements.Select((CEnhancement s) => s.Copy()).ToList();
		bool multiPassAttack = ability is CAbilityAttack && (ability as CAbilityAttack).MultiPassAttack;
		bool chainAttack = ability is CAbilityAttack && (ability as CAbilityAttack).ChainAttack;
		int chainAttackRange = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).ChainAttackRange : 0);
		int chainAttackDamageReduction = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).ChainAttackDamageReduction : ((ability is CAbilityPlayDiscardedCard) ? (ability as CAbilityPlayDiscardedCard).DamageReduction : 0));
		int damageSelfBeforeAttack = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).DamageSelfBeforeAttack : 0);
		bool addAttackBaseStat = ability.AddAttackBaseStat;
		bool strengthIsBase = ability.StrengthIsBase;
		bool rangeIsBase = ability.RangeIsBase;
		bool targetIsBase = ability.TargetIsBase;
		string abilityText = ability.AbilityText;
		bool abilityTextOnly = ability.AbilityTextOnly;
		bool abilityNoLayoutSpace = ability.AbilityNoLayoutSpace;
		bool showRange = ability.ShowRange;
		bool showTarget = ability.ShowTarget;
		bool showArea = ability.ShowArea;
		bool onDeath = ability.OnDeath;
		bool isConsumeAbility = ability.IsConsumeAbility;
		bool allTargetsOnMovePath = ability.AllTargetsOnMovePath;
		bool allTargetsOnMovePathSameStartAndEnd = ability.AllTargetsOnMovePathSameStartAndEnd;
		bool allTargetsOnAttackPath = ability.AllTargetsOnAttackPath;
		bool allTargetsInRangeAlongPath = ability.AllTargetsInRangeAlongPath;
		object summons;
		if (ability is CAbilitySummon cAbilitySummon)
		{
			summons = cAbilitySummon.SummonIDs;
		}
		else
		{
			summons = ((ability is CAbilityRevive cAbilityRevive) ? cAbilityRevive.SummonIDs : null);
		}
		AbilityData.XpPerTargetData xpPerTargetData = ability.m_XpPerTargetData?.Copy();
		EAbilityTargeting targeting = ability.Targeting;
		List<(string, TValue)> targetingVars = ability.TargetingVars;
		List<(string, TValue)> vars = ability.Vars;
		List<(string, List<(string, TValue)>)> performVars = ability.PerformVars;
		object elementsToInfuse;
		if (ability is CAbilityInfuse)
		{
			elementsToInfuse = (ability as CAbilityInfuse).ElementsToInfuse.ToList();
		}
		else if (ability is CAbilityConsumeElement)
		{
			elementsToInfuse = (ability as CAbilityConsumeElement).ElementsToConsume.ToList();
		}
		else if (!(ability is CAbilityAttack))
		{
			elementsToInfuse = ((ability is CAbilityConsume) ? (ability as CAbilityConsume).ElementsToConsume.ToList() : null);
		}
		else
		{
			elementsToInfuse = (((ability as CAbilityAttack).ElementsToConsume != null) ? (ability as CAbilityAttack).ElementsToConsume.ToList() : null);
		}
		List<CCondition.ENegativeCondition> elementalConditions;
		if (!(ability is CAbilityAttack))
		{
			elementalConditions = new List<CCondition.ENegativeCondition>();
		}
		else
		{
			elementalConditions = (((ability as CAbilityAttack).ElementalConditions != null) ? (ability as CAbilityAttack).ElementalConditions.ToList() : new List<CCondition.ENegativeCondition>());
		}
		bool isMonsterAbility = ability.IsMonsterAbility;
		string propName;
		if (ability is CAbilityCreate)
		{
			propName = (ability as CAbilityCreate).PropName;
		}
		else
		{
			propName = ((ability is CAbilityCreateTerrain) ? (ability as CAbilityCreateTerrain).PropName : string.Empty);
		}
		AbilityData.TrapData trapData = ((ability is CAbilityTrap cAbilityTrap) ? cAbilityTrap.TrapData : null);
		bool isSubAbility = ability.IsSubAbility;
		bool isInlineSubAbility = ability.IsInlineSubAbility;
		bool isModifierAbility = ability.IsModifierAbility;
		int pierce = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).Pierce : 0);
		int retaliateRange = ((!(ability is CAbilityRetaliate)) ? 1 : (ability as CAbilityRetaliate).RetaliateRange);
		bool isTargetedAbility = ability.IsTargetedAbility;
		float spawnDelay = ability.SpawnDelay;
		CAbilityPull.EPullType pullType = ((ability is CAbilityPull cAbilityPull) ? cAbilityPull.PullType : CAbilityPull.EPullType.None);
		CAbilityPush.EAdditionalPushEffect additionalPushEffect = ((ability is CAbilityPush) ? (ability as CAbilityPush).AdditionalPushEffect : CAbilityPush.EAdditionalPushEffect.None);
		int additionalPushEffectDamage = ((ability is CAbilityPush) ? (ability as CAbilityPush).AdditionalPushEffectDamage : 0);
		int additionalPushEffectXP = ((ability is CAbilityPush) ? (ability as CAbilityPush).AdditionalPushEffectXP : 0);
		CAbilityLoot.LootData lootData = ((ability is CAbilityLoot) ? (ability as CAbilityLoot).AbilityLootData : null);
		int abilityXP = ability.AbilityXP;
		List<CConditionalOverride> conditionalOverrides = ability.ConditionalOverrides.Select((CConditionalOverride s) => s.Copy()).ToList();
		CAbilityRequirements startAbilityConditions = ability.StartAbilityRequirements.Copy();
		string parentName = ability.ParentName;
		AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData?.Copy();
		bool skipAnim = ability.SkipAnim;
		int conditionDuration = ((ability is CAbilityCondition) ? (ability as CAbilityCondition).Duration : 0);
		EConditionDecTrigger conditionDecTrigger = ((ability is CAbilityCondition) ? (ability as CAbilityCondition).DecrementTrigger : EConditionDecTrigger.None);
		bool isDefault = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).IsDefaultAttack : (ability is CAbilityMove && (ability as CAbilityMove).IsDefaultMove));
		bool isItemAbility = ability.IsItemAbility;
		CAugment augment = ability.Augment;
		CSong song = ability.Song;
		CDoom doom = ((ability is CAbilityAddDoom cAbilityAddDoom) ? cAbilityAddDoom.Doom : null);
		List<CAttackEffect> attackEffects = ((!(ability is CAbilityAttack)) ? null : (ability as CAbilityAttack).AttackEffects?.ToList());
		CAbilityControlActor.ControlActorAbilityData controlActorData = (ability as CAbilityControlActor)?.ControlActorData?.Copy();
		CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = ((ability is CAbilityChangeAllegiance cAbilityChangeAllegiance) ? cAbilityChangeAllegiance.ChangeAllegianceData.Copy() : null);
		bool isMergedAbility = ability.IsMergedAbility;
		List<CAbility> list3;
		if (!(ability is CAbilityMerged))
		{
			list3 = ((!(ability is CAbilityPerformFromShadow)) ? null : (ability as CAbilityPerformFromShadow).Abilities?.ToList());
		}
		else
		{
			list3 = (ability as CAbilityMerged).MergedAbilities?.ToList();
		}
		CAbility cAbility = CreateAbility(targetActorWithTrapEffects: ability is CAbilityDisarmTrap && (ability as CAbilityDisarmTrap).TargetActorWithTrapEffects, targetActorWithTrapEffectRange: (ability is CAbilityDisarmTrap) ? (ability as CAbilityDisarmTrap).TargetActorWithTrapEffectRange : 0, isMergedAbility: isMergedAbility, mergedAbilities: list3, statIsBasedOnXEntries: ability.StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData s) => s.Copy()).ToList(), moveObstacleRange: (ability is CAbilityMoveObstacle cAbilityMoveObstacle) ? cAbilityMoveObstacle.MoveObstacleRange : ((ability is CAbilityMoveTrap cAbilityMoveTrap) ? cAbilityMoveTrap.MoveTrapRange : ((ability is CAbilityMoveTerrain cAbilityMoveTerrain) ? cAbilityMoveTerrain.MoveTerrainRange : 0)), moveObstacleAnimOverload: (ability is CAbilityMoveObstacle cAbilityMoveObstacle2) ? cAbilityMoveObstacle2.MoveObstacleAnimOverload : null, slotsToRefresh: (ability is CAbilityRefreshItemCards cAbilityRefreshItemCards) ? cAbilityRefreshItemCards.SlotsToRefresh : ((ability is CAbilityConsumeItemCards cAbilityConsumeItemCards) ? cAbilityConsumeItemCards.SlotsToConsume : null), slotStatesToRefresh: (ability is CAbilityRefreshItemCards cAbilityRefreshItemCards2) ? cAbilityRefreshItemCards2.SlotStatesToRefresh : ((ability is CAbilityConsumeItemCards cAbilityConsumeItemCards2) ? cAbilityConsumeItemCards2.SlotStatesToConsume : null), slotUsagesToRefresh: (ability is CAbilityRefreshItemCards cAbilityRefreshItemCards3) ? cAbilityRefreshItemCards3.SlotUsagesToRefresh : ((ability is CAbilityConsumeItemCards cAbilityConsumeItemCards3) ? cAbilityConsumeItemCards3.SlotUsagesToConsume : null), addActiveBonusAbility: (ability is CAbilityAddActiveBonus cAbilityAddActiveBonus) ? cAbilityAddActiveBonus.AddAbility : null, overrideAugmentAttackType: (ability is CAbilityOverrideAugmentAttackType cAbilityOverrideAugmentAttackType) ? cAbilityOverrideAugmentAttackType.OverrideAttackType : EAttackType.None, showElementPicker: (ability is CAbilityInfuse cAbilityInfuse) ? cAbilityInfuse.ShowPicker : ((ability is CAbilityConsumeElement cAbilityConsumeElement) ? cAbilityConsumeElement.ShowPicker : (ability is CAbilityConsume cAbilityConsume && cAbilityConsume.ShowPicker)), chooseAbilities: (ability is CAbilityChoose cAbilityChoose) ? CopySubAbilities(cAbilityChoose.ChooseAbilities) : ((ability is CAbilityChooseAbility cAbilityChooseAbility) ? CopySubAbilities(cAbilityChooseAbility.ChooseAbilities) : null), recoverCardsAbilityTypeFilter: (ability is CAbilityRecoverDiscardedCards cAbilityRecoverDiscardedCards) ? cAbilityRecoverDiscardedCards.RecoverCardsWithAbilityOfTypeFilter : ((ability is CAbilityRecoverLostCards cAbilityRecoverLostCards) ? cAbilityRecoverLostCards.RecoverCardsWithAbilityOfTypeFilter : ((ability is CAbilityPlayDiscardedCard cAbilityPlayDiscardedCard) ? cAbilityPlayDiscardedCard.RecoverCardsWithAbilityOfTypeFilter : new List<EAbilityType>())), cardAction: (ability is CAbilityPlayDiscardedCard cAbilityPlayDiscardedCard2) ? cAbilityPlayDiscardedCard2.CardAction : ECardAction.None, forgoTopActionAbility: (ability is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion) ? cAbilityForgoActionsForCompanion.ForgoTopActionAbility : null, forgoBottomActionAbility: (ability is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion2) ? cAbilityForgoActionsForCompanion2.ForgoBottomActionAbility : null, extraTurnType: (ability is CAbilityExtraTurn cAbilityExtraTurn) ? cAbilityExtraTurn.ExtraTurnType : CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: (ability is CAbilityExtraTurn cAbilityExtraTurn2) ? cAbilityExtraTurn2.ActionMonsterClassID : null, swapAbilityFirstTargetFilter: (ability is CAbilitySwap cAbilitySwap) ? cAbilitySwap.FirstTargetFilter : null, swapAbilitySecondTargetFilter: (ability is CAbilitySwap cAbilitySwap2) ? cAbilitySwap2.SecondTargetFilter : null, supplyCardsToGive: (ability is CAbilityGiveSupplyCard cAbilityGiveSupplyCard) ? cAbilityGiveSupplyCard.SupplyCardIDs : ((ability is CAbilityPlayDiscardedCard cAbilityPlayDiscardedCard3) ? cAbilityPlayDiscardedCard3.SpecificCardsFilter : null), teleportData: (ability is CAbilityTeleport cAbilityTeleport) ? cAbilityTeleport.AbilityTeleportData.Copy() : null, immunityToAbilityTypes: (!(ability is CAbilityImmunityTo { ImmuneToAbilityTypes: var immuneToAbilityTypes })) ? null : immuneToAbilityTypes?.ToList(), immuneToAttackTypes: (!(ability is CAbilityImmunityTo { ImmuneToAttackTypes: var immuneToAttackTypes })) ? null : immuneToAttackTypes?.ToList(), immuneToSelfDamage: ability is CAbilityImmunityTo cAbilityImmunityTo3 && cAbilityImmunityTo3.ImmuneToSelfDamage, addOrRemoveModifiers: (ability is CAbilityAddModifierToMonsterDeck cAbilityAddModifierToMonsterDeck) ? cAbilityAddModifierToMonsterDeck.ModifierCardNamesToAdd.ToList() : ((ability is CAbilityAddAttackModifier cAbilityAddAttackModifier) ? cAbilityAddAttackModifier.ModifierCardNames : ((ability is CAbilityRemoveAttackModifier cAbilityRemoveAttackModifier) ? cAbilityRemoveAttackModifier.ModifierCardNames : null)), healData: (ability is CAbilityHeal cAbilityHeal) ? cAbilityHeal.HealData.Copy() : null, resourcesToAddOnAbilityEnd: ability.ResourcesToAddOnAbilityEnd, resourcesToTakeFromTargets: ability.ResourcesToTakeFromTargets, resourcesToGiveToTargets: ability.ResourcesToGiveToTargets, destroyObstacleData: (ability is CAbilityDestroyObstacle cAbilityDestroyObstacle) ? cAbilityDestroyObstacle.AbilityDestroyObstacleData.Copy() : null, changeCharacterModel: (ability is CAbilityChangeCharacterModel cAbilityChangeCharacterModel) ? cAbilityChangeCharacterModel.CharacterModel : ECharacter.None, tileFilter: ability.TileFilter, disableCardActionData: (ability is CAbilityDisableCardAction cAbilityDisableCardAction) ? cAbilityDisableCardAction.DisableCardActionAbilityData.Copy() : null, parentBaseCard: (ability is CAbilityInfuse cAbilityInfuse2) ? cAbilityInfuse2.ParentBaseCard : null, previewEffectId: ability.PreviewEffectId, previewEffectText: ability.PreviewEffectText, canUndoOverride: ability.CanUndoOverride, helpBoxTooltipLocKey: ability.HelpBoxTooltipLocKey, abilityType: abilityType, strength: strength, useSpecialBaseStat: useSpecialBaseStat, range: range, numberTargets: numberTargets, tokens: tokens, flags: flags, abilityFilter: abilityFilter, animationOverload: animOverload, animationOverload2: animOverload2, subAbilities: subAbilities, areaEffect: areaEffect, attackSourcesOnly: attackSourcesOnly, jump: jump, fly: fly, ignoreDifficultTerrain: ignoreDifficultTerrain, ignoreHazardousTerrain: ignoreHazardousTerrain, ignoreBlockedTileMoveCost: ignoreBlockedTileMoveCost, carryOtherActorsOnHex: carryOtherActorsOnHex, aiFocusOverride: aiFocusOverride, moveRestrictionType: moveRestrictionType, activeBonusData: activeBonusData, activeBonusYML: activeBonusYML, areaEffectYMLString: areaEffectYMLString, areaEffectLayoutOverrideYMLString: areaEffectLayoutOverrideYMLString, areaEffectFlipAxis: areaEffectFlipAxis, name: name, abilityEnhancements: abilityEnhancements, positiveConditions: list, negativeConditions: list2, multiPassAttack: multiPassAttack, chainAttack: chainAttack, chainAttackRange: chainAttackRange, chainAttackDamageReduction: chainAttackDamageReduction, damageSelfBeforeAttack: damageSelfBeforeAttack, addAttackBaseStat: addAttackBaseStat, strengthIsBase: strengthIsBase, rangeIsBase: rangeIsBase, targetIsBase: targetIsBase, text: abilityText, textOnly: abilityTextOnly, noLayoutSpace: abilityNoLayoutSpace, showRange: showRange, showTarget: showTarget, showArea: showArea, onDeath: onDeath, isConsumeAbility: isConsumeAbility, allTargetsOnMovePath: allTargetsOnMovePath, allTargetsOnMovePathSameStartAndEnd: allTargetsOnMovePathSameStartAndEnd, allTargetsOnAttackPath: allTargetsOnAttackPath, allTargetsInRangeAlongPath: allTargetsInRangeAlongPath, summons: (List<string>)summons, xpPerTargetData: xpPerTargetData, targeting: targeting, targetingVars: targetingVars, vars: vars, performVars: performVars, elementsToInfuse: (List<ElementInfusionBoardManager.EElement>)elementsToInfuse, elementalConditions: elementalConditions, isMonster: isMonsterAbility, propName: propName, trapData: trapData, isSubAbility: isSubAbility, isInlineSubAbility: isInlineSubAbility, pierce: pierce, retaliateRange: retaliateRange, isTargetedAbility: isTargetedAbility, spawnDelay: spawnDelay, pullType: pullType, additionalPushEffect: additionalPushEffect, additionalPushEffectDamage: additionalPushEffectDamage, additionalPushEffectXP: additionalPushEffectXP, lootData: lootData, conditionalOverrides: conditionalOverrides, startAbilityConditions: startAbilityConditions, abilityXP: abilityXP, parentName: parentName, miscAbilityData: miscAbilityData, skipAnim: skipAnim, conditionDuration: conditionDuration, conditionDecTrigger: conditionDecTrigger, augment: augment, song: song, doom: doom, controlActorData: controlActorData, changeAllegianceData: changeAllegianceData, attackEffects: attackEffects, consumes: (ability is CAbilityConsume cAbilityConsume2) ? cAbilityConsume2.Consumes : new List<AbilityConsume>(), isDefault: isDefault, isItemAbility: isItemAbility, isModifierAbility: isModifierAbility, exploitVulnerability: ability is CAbilityAttack && (ability as CAbilityAttack).ExploitVulnerability, extraTurnAbilityTypeFilter: (ability is CAbilityExtraTurn) ? (ability as CAbilityExtraTurn).AbilityTypeFilter : null, maxExtraTurnAbilityLevel: (ability is CAbilityExtraTurn) ? (ability as CAbilityExtraTurn).MaxAbilityLevel : int.MaxValue, useParentTargeting: ability is CAbilityPlayDiscardedCard && (ability as CAbilityPlayDiscardedCard).UseParentTargeting, createInitialState: false, toDiscardPile: (ability as CAbilityAddAttackModifier)?.ToDiscardPile ?? false, removeWhenDone: (ability as CAbilityPerformFromShadow)?.m_RemoveWhenDone ?? false, canExcludeCards: (ability as CAbilityImprovedShortRest)?.m_CanExcludeCards ?? false, miscVars: ability.MiscVars?.ToList());
		if (CSRLYML.IsLoading)
		{
			cAbility.Original = cAbility;
		}
		else
		{
			cAbility.Original = ability;
		}
		cAbility.ShouldBePlayerControlled = ability.ShouldBePlayerControlled;
		cAbility.FileName = ability.FileName;
		cAbility.YMLPath = ability.YMLPath;
		cAbility.CardID = ability.CardID;
		cAbility.PerkID = ability.PerkID;
		if (ability.PerkID != null && ability is CAbilityAddActiveBonus { AddAbility: not null } cAbilityAddActiveBonus2 && cAbilityAddActiveBonus2.AddAbility.PerkID == null)
		{
			cAbilityAddActiveBonus2.AddAbility.PerkID = ability.PerkID;
		}
		if (!generateNewID)
		{
			cAbility.ID = ability.ID;
		}
		cAbility.ControlledByClassID = ability.ControlledByClassID;
		cAbility.IsControlAbility = ability.IsControlAbility;
		cAbility.WasInterrupted = ability.WasInterrupted;
		cAbility.WasForcedInterrupted = ability.WasForcedInterrupted;
		if (ability.VarsContext?.obj is CVars cVars)
		{
			CVars cVars2 = cVars.Copy();
			if (cVars2.Get("obj").GetObject<CAbility>() == ability)
			{
				cVars2.Set("obj", new TValue(cAbility));
			}
			cAbility.VarsContext.obj = cVars2;
		}
		cAbility.AddParentContext(ability.VarsContext?.parentContext);
		cAbility.AddParentContext(parentContext);
		cAbility.IsScenarioModifierAbility = ability.IsScenarioModifierAbility;
		cAbility.TargetingOverrideArrayIndex = ability.TargetingOverrideArrayIndex;
		cAbility.AbilityInitialState = ability.AbilityInitialState;
		cAbility.CachedAbilityBaseCard = ability.CachedAbilityBaseCard;
		if (ability is CAbilityPerformFromShadow cAbilityPerformFromShadow)
		{
			(cAbility as CAbilityPerformFromShadow).m_ShadowsToUse = cAbilityPerformFromShadow.m_ShadowsToUse;
			(cAbility as CAbilityPerformFromShadow).m_UsedShadows = new List<CObjectProp>();
			(cAbility as CAbilityPerformFromShadow).m_UsedShadows.AddRange(cAbilityPerformFromShadow.m_UsedShadows);
			if (cAbilityPerformFromShadow.m_ShadowsAtStart != null)
			{
				(cAbility as CAbilityPerformFromShadow).m_ShadowsAtStart = new List<CTile>();
				(cAbility as CAbilityPerformFromShadow).m_ShadowsAtStart.AddRange(cAbilityPerformFromShadow.m_ShadowsAtStart);
			}
		}
		if (ability is CAbilityCreateShadow cAbilityCreateShadow)
		{
			(cAbility as CAbilityCreateShadow).m_CanClearTargets = cAbilityCreateShadow.m_CanClearTargets;
			(cAbility as CAbilityCreateShadow).FromShadowMark = cAbilityCreateShadow.FromShadowMark;
		}
		if (ability is CAbilityCreateLimitedHazardousTerrain cAbilityCreateLimitedHazardousTerrain)
		{
			(cAbility as CAbilityCreateLimitedHazardousTerrain).m_CanClearTargets = cAbilityCreateLimitedHazardousTerrain.m_CanClearTargets;
		}
		if (ability is CAbilityRemoveShadow cAbilityRemoveShadow)
		{
			(cAbility as CAbilityRemoveShadow).m_DestroyDelay = cAbilityRemoveShadow.m_DestroyDelay;
		}
		if (ability is CAbilityMove cAbilityMove6 && cAbility is CAbilityMove cAbilityMove7)
		{
			cAbilityMove7.DontApplyBonuses = cAbilityMove6.DontApplyBonuses;
			cAbilityMove7.FixedPath = cAbilityMove6.FixedPath?.ToList();
		}
		if (ability is CAbilityPush cAbilityPush && cAbility is CAbilityPush cAbilityPush2)
		{
			cAbilityPush2.MustMove = cAbilityPush.MustMove;
			cAbilityPush2.RemainingPushes = cAbilityPush.RemainingPushes;
			cAbilityPush2.TryPushToThisTileAutomatically = cAbilityPush.TryPushToThisTileAutomatically;
		}
		if (ability is CAbilityPull cAbilityPull2 && cAbility is CAbilityPull cAbilityPull3)
		{
			cAbilityPull3.MustMove = cAbilityPull2.MustMove;
			cAbilityPull3.TryPullToThisTileAutomatically = cAbilityPull2.TryPullToThisTileAutomatically;
		}
		if (ability is CAbilityNull cAbilityNull && cAbility is CAbilityNull cAbilityNull2)
		{
			cAbilityNull2.customWaitMessage = cAbilityNull.customWaitMessage;
		}
		if (ability is CAbilitySelectAndPlayCard cAbilitySelectAndPlayCard)
		{
			(cAbility as CAbilitySelectAndPlayCard).PilesToChooseFrom = cAbilitySelectAndPlayCard.PilesToChooseFrom?.ToList();
		}
		if (fullCopy)
		{
			cAbility.m_AbilityStartComplete = ability.m_AbilityStartComplete;
			cAbility.ValidActorsInRange = ability.ValidActorsInRange?.ToList();
			cAbility.TargetingActor = ability.TargetingActor;
			cAbility.FilterActor = ability.FilterActor;
			cAbility.m_TilesSelected = ability.TilesSelected?.ToList();
			cAbility.m_ActorsToTarget = ability.ActorsToTarget?.ToList();
			cAbility.ActorsTargeted = ability.ActorsTargeted?.ToList();
			cAbility.m_NumberTargetsRemaining = ability.NumberTargetsRemaining;
			cAbility.m_UndoNumberTargetsRemaining = ability.m_UndoNumberTargetsRemaining;
			cAbility.TilesInRange = ability.TilesInRange?.ToList();
			cAbility.m_CanUndo = ability.m_CanUndo;
			cAbility.m_CanSkip = ability.m_CanSkip;
			cAbility.AllPossibleTilesInAreaEffect = ability.AllPossibleTilesInAreaEffect?.ToList();
			cAbility.ActorsToIgnore = ability.ActorsToIgnore?.ToList();
			cAbility.m_ModifiedStrength = ability.ModifiedStrength();
			cAbility.m_AllTargets = ability.m_AllTargets;
			cAbility.m_CancelAbility = ability.m_CancelAbility;
			cAbility.m_ConfirmCancelled = ability.m_ConfirmCancelled;
			cAbility.ParentAbility = ability.ParentAbility;
			cAbility.InlineSubAbilityTiles = ability.InlineSubAbilityTiles.ToList();
			cAbility.TargetThisActorAutomatically = ability.TargetThisActorAutomatically;
			cAbility.ProcessIfDead = ability.ProcessIfDead;
			cAbility.StackedAttackEffectAbility = ability.StackedAttackEffectAbility;
			cAbility.AppliedEnhancements = ability.AppliedEnhancements;
			cAbility.AbilityStartListenersInvoked = ability.AbilityStartListenersInvoked;
			cAbility.ParentAbilityBaseCard = ability.ParentAbilityBaseCard;
			cAbility.m_RangeStatAdditions = ability.m_RangeStatAdditions;
			cAbility.IsScenarioModifierAbility = ability.IsScenarioModifierAbility;
			cAbility.m_AugmentAbilitiesProcessed = ability.m_AugmentAbilitiesProcessed;
			cAbility.m_AugmentOverridesProcessed = ability.m_AugmentOverridesProcessed;
			cAbility.m_SongAbilitiesProcessed = ability.m_SongAbilitiesProcessed;
			cAbility.m_SongOverridesProcessed = ability.m_SongOverridesProcessed;
			cAbility.m_OriginalTargetCount = ability.m_OriginalTargetCount;
			cAbility.m_OriginalAreaEffect = ability.OriginalAreaEffect;
			cAbility.m_OriginalRange = ability.m_OriginalRange;
			cAbility.m_StrengthStatAdditions = ability.m_StrengthStatAdditions;
			cAbility.m_TokenStatAdditions = ability.m_TokenStatAdditions;
			cAbility.m_NumberTargetsStatAdditions = ability.m_NumberTargetsStatAdditions;
			cAbility.m_RangeStatAdditions = ability.m_RangeStatAdditions;
			if (cAbility is CAbilityAttack cAbilityAttack && ability is CAbilityAttack cAbilityAttack2)
			{
				cAbilityAttack.IsAttackAbilityWithOverrides = cAbilityAttack2.IsAttackAbilityWithOverrides;
				cAbilityAttack.AttackSummary = cAbilityAttack2.AttackSummary?.Copy();
				cAbilityAttack.OriginalStrength = cAbilityAttack2.OriginalStrength;
				cAbilityAttack.TreatAsRangeBuff = cAbilityAttack2.TreatAsRangeBuff;
			}
		}
		if (copyCurrentOverrides)
		{
			cAbility.m_CurrentOverrides = ability.CurrentOverrides;
		}
		if (applyCurrentStatIsBasedOnX)
		{
			cAbility.StatIsBasedOnXEntries.Clear();
		}
		return cAbility;
	}

	public virtual void Undo()
	{
		if (Augment != null)
		{
			TargetingActor.UndoAugment(Augment);
		}
		if (Song != null)
		{
			TargetingActor.UndoSong(Song);
		}
		GameState.WaitingForMercenarySpecialMechanicSlotChoice = false;
		if (TargetingOverrideArrayIndex == TargetingActor.OverrideArrayIndex && TargetingActor.OverrideArrayIndex != default(Point))
		{
			TargetingActor.OverrideArrayIndex = default;
		}
		if (TargetingActor.m_OnAbilityUndoneListeners != null)
		{
			TargetingActor.m_OnAbilityUndoneListeners(this);
		}
	}

	public virtual void ClearTargets()
	{
		if (m_ActorsToTarget != null)
		{
			m_ActorsToTarget.Clear();
		}
		if (m_TilesSelected != null)
		{
			m_TilesSelected.Clear();
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_AreaEffectLocked = false;
		if (AbilityType != EAbilityType.Attack)
		{
			TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
		}
	}

	public virtual bool CanClearTargets()
	{
		return false;
	}

	public virtual bool CanReceiveTileSelection()
	{
		if (m_AbilityStartComplete)
		{
			return !AbilityHasBeenCancelled;
		}
		return false;
	}

	public virtual bool RequiresWaypointSelection()
	{
		return false;
	}

	public virtual bool CanApplyActiveBonusTogglesTo()
	{
		return true;
	}

	public virtual bool EnoughTargetsSelected()
	{
		return true;
	}

	public virtual bool AllowNoTargets()
	{
		if (GetVar("AllowNoTargets").GetBool())
		{
			return true;
		}
		if (GetMiscVar("AllowNoTargets").GetBool())
		{
			return true;
		}
		return false;
	}

	public virtual bool IsCurrentlyTargetingActors()
	{
		return true;
	}

	public virtual bool ShouldPreviewPotentialDamage()
	{
		return true;
	}

	public virtual bool CanBePerformed(CActor performer = null)
	{
		return GetMiscVar("SelectionValid").GetBool(VarsContext, defVal: true);
	}

	public virtual bool CanSkipUntergetableCheck()
	{
		return false;
	}

	public virtual bool RequiresAnySelectedTileToBeAdjacentToCaster()
	{
		if (GetMiscVar("RequiresAnyTileAdjacentToCaster").GetBool())
		{
			return true;
		}
		return false;
	}

	public bool IncludeCasterTile()
	{
		bool result = Targeting == EAbilityTargeting.Custom || AbilityType == EAbilityType.RemoveShadow || AbilityType == EAbilityType.MoveShadow || AbilityType == EAbilityType.PerformFromShadow || AbilityType == EAbilityType.CreateShadow || AbilityType == EAbilityType.Loot || AbilityType == EAbilityType.DisarmTrap || AbilityType == EAbilityType.MoveTrap;
		if (MiscVars != null)
		{
			TValue miscVar = GetMiscVar("includeCasterTile");
			if (!miscVar.isUnknown)
			{
				return miscVar.GetBool();
			}
		}
		return result;
	}

	public bool RequiresAllyTargeting()
	{
		if (AreaEffect == null)
		{
			return false;
		}
		foreach (CAreaEffect.CAreaEffectHex hex in AreaEffect.Hexes)
		{
			if ((hex.Flags & CAreaEffect.EHexFlag.Ally) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool ValidAlliesTargeted()
	{
		if (AreaEffect == null)
		{
			return true;
		}
		int num = 0;
		foreach (CAreaEffect.CAreaEffectHex hex in AreaEffect.Hexes)
		{
			if ((hex.Flags & CAreaEffect.EHexFlag.Ally) != 0)
			{
				num++;
			}
		}
		int num2 = 0;
		foreach (KeyValuePair<CTile, CAreaEffect.EHexFlag> flagsOfValidTile in FlagsOfValidTiles)
		{
			if ((flagsOfValidTile.Value & CAreaEffect.EHexFlag.Ally) != 0)
			{
				num2++;
			}
		}
		if (num != num2)
		{
			return false;
		}
		foreach (KeyValuePair<CTile, CAreaEffect.EHexFlag> flagsOfValidTile2 in FlagsOfValidTiles)
		{
			if ((flagsOfValidTile2.Value & CAreaEffect.EHexFlag.Ally) != 0)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(flagsOfValidTile2.Key.m_ArrayIndex);
				if (cActor == null)
				{
					return false;
				}
				if (AbilityFilter.FilterAllyType(CAbilityFilter.EFilterAllyType.OwnSummon) && (!(cActor is CHeroSummonActor cHeroSummonActor) || cHeroSummonActor.Summoner != TargetingActor as CPlayerActor))
				{
					return false;
				}
				if (AbilityFilter.FilterAllyType(CAbilityFilter.EFilterAllyType.Ally) && !CActor.AreActorsAllied(TargetingActor.Type, cActor.Type, TargetingActor, cActor))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsActorTargetedByAreaEffect(CActor actorToCheck)
	{
		if (AreaEffect == null)
		{
			return false;
		}
		foreach (KeyValuePair<CTile, CAreaEffect.EHexFlag> flagsOfValidTile in FlagsOfValidTiles)
		{
			if ((flagsOfValidTile.Value & CAreaEffect.EHexFlag.Enemy) != 0 && ScenarioManager.Scenario.FindActorAt(flagsOfValidTile.Key.m_ArrayIndex) == actorToCheck)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool MaxTargetsSelected()
	{
		if (AreaEffect != null)
		{
			return m_AreaEffectLocked;
		}
		return m_NumberTargetsRemaining <= 0;
	}

	public virtual bool ShouldRestartAbilityWhenApplyingOverride(CAbilityOverride abilityOverride)
	{
		if (!abilityOverride.Range.HasValue && (!abilityOverride.NumberOfTargets.HasValue || AreaEffect != null) && abilityOverride.AbilityFilter == null && abilityOverride.AreaEffect == null && !abilityOverride.MultiPassAttack.HasValue && !abilityOverride.RangeIsBase.HasValue && !abilityOverride.TargetIsBase.HasValue && !abilityOverride.AllTargetsOnMovePath.HasValue && !abilityOverride.AllTargetsOnMovePathSameStartAndEnd.HasValue)
		{
			return abilityOverride.AllTargetsOnAttackPath.HasValue;
		}
		return true;
	}

	public virtual void GetSubAbilities(List<CAbility> outAbilities)
	{
	}

	public static CAbility CreateAbility(EAbilityType abilityType, int strength = 1, bool useSpecialBaseStat = false, int range = 1, int numberTargets = 1, int tokens = 0, EAbilityFlag flags = EAbilityFlag.None, CAbilityFilterContainer abilityFilter = null, string animationOverload = null, string animationOverload2 = null, List<CAbility> subAbilities = null, CAreaEffect areaEffect = null, bool attackSourcesOnly = false, bool jump = false, bool fly = false, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreBlockedTileMoveCost = false, bool carryOtherActorsOnHex = false, CAIFocusOverrideDetails aiFocusOverride = null, CAbilityMove.EMoveRestrictionType moveRestrictionType = CAbilityMove.EMoveRestrictionType.None, AbilityData.ActiveBonusData activeBonusData = null, ActiveBonusLayout activeBonusYML = null, string areaEffectYMLString = null, string areaEffectLayoutOverrideYMLString = null, string areaEffectFlipAxis = null, string name = null, List<CEnhancement> abilityEnhancements = null, List<CCondition.EPositiveCondition> positiveConditions = null, List<CCondition.ENegativeCondition> negativeConditions = null, bool multiPassAttack = false, bool chainAttack = false, int chainAttackRange = 0, int chainAttackDamageReduction = 0, int damageSelfBeforeAttack = 0, bool addAttackBaseStat = false, bool strengthIsBase = false, bool rangeIsBase = false, bool targetIsBase = false, string text = null, bool textOnly = false, bool noLayoutSpace = false, bool showRange = false, bool showTarget = false, bool showArea = false, bool onDeath = false, bool isConsumeAbility = false, bool allTargetsOnMovePath = false, bool allTargetsOnMovePathSameStartAndEnd = false, bool allTargetsOnAttackPath = false, bool allTargetsInRangeAlongPath = false, List<string> summons = null, AbilityData.XpPerTargetData xpPerTargetData = null, EAbilityTargeting targeting = EAbilityTargeting.All, List<(string, TValue)> targetingVars = null, List<(string, TValue)> vars = null, List<(string, List<(string, TValue)>)> performVars = null, List<ElementInfusionBoardManager.EElement> elementsToInfuse = null, List<CCondition.ENegativeCondition> elementalConditions = null, bool isMonster = false, string propName = null, AbilityData.TrapData trapData = null, bool isSubAbility = false, bool isInlineSubAbility = false, int pierce = 0, int retaliateRange = 0, bool isTargetedAbility = false, float spawnDelay = 0f, CAbilityPull.EPullType pullType = CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect additionalPushEffect = CAbilityPush.EAdditionalPushEffect.None, int additionalPushEffectDamage = 0, int additionalPushEffectXP = 0, CAbilityLoot.LootData lootData = null, List<CConditionalOverride> conditionalOverrides = null, CAbilityRequirements startAbilityConditions = null, int abilityXP = 0, string parentName = null, AbilityData.MiscAbilityData miscAbilityData = null, bool skipAnim = false, int conditionDuration = 0, EConditionDecTrigger conditionDecTrigger = EConditionDecTrigger.None, CAugment augment = null, CSong song = null, CDoom doom = null, CAbilityControlActor.ControlActorAbilityData controlActorData = null, CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = null, List<CAttackEffect> attackEffects = null, bool targetActorWithTrapEffects = false, int targetActorWithTrapEffectRange = 0, bool isMergedAbility = false, List<CAbility> mergedAbilities = null, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null, int moveObstacleRange = 0, string moveObstacleAnimOverload = null, List<CItem.EItemSlot> slotsToRefresh = null, List<CItem.EItemSlotState> slotStatesToRefresh = null, CAbility addActiveBonusAbility = null, EAttackType overrideAugmentAttackType = EAttackType.None, List<CAbility> chooseAbilities = null, List<EAbilityType> recoverCardsAbilityTypeFilter = null, ECardAction cardAction = ECardAction.None, CAbility forgoTopActionAbility = null, CAbility forgoBottomActionAbility = null, CAbilityExtraTurn.EExtraTurnType extraTurnType = CAbilityExtraTurn.EExtraTurnType.None, string actionMonsterClassID = null, CAbilityFilterContainer swapAbilityFirstTargetFilter = null, CAbilityFilterContainer swapAbilitySecondTargetFilter = null, List<int> supplyCardsToGive = null, List<AbilityConsume> consumes = null, CAbilityTeleport.TeleportData teleportData = null, List<EAbilityType> immunityToAbilityTypes = null, List<EAttackType> immuneToAttackTypes = null, bool immuneToSelfDamage = false, List<string> addOrRemoveModifiers = null, CAbilityHeal.HealAbilityData healData = null, Dictionary<string, int> resourcesToAddOnAbilityEnd = null, Dictionary<string, int> resourcesToTakeFromTargets = null, Dictionary<string, int> resourcesToGiveToTargets = null, CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = null, ECharacter changeCharacterModel = ECharacter.None, CAbilityFilter.EFilterTile tileFilter = CAbilityFilter.EFilterTile.None, CAbilityDisableCardAction.DisableCardActionData disableCardActionData = null, bool isDefault = false, bool isItemAbility = false, bool showElementPicker = false, bool isModifierAbility = false, CBaseCard parentBaseCard = null, string previewEffectId = null, string previewEffectText = null, bool? canUndoOverride = null, bool exploitVulnerability = false, string helpBoxTooltipLocKey = null, List<EAbilityType> extraTurnAbilityTypeFilter = null, int? maxExtraTurnAbilityLevel = int.MaxValue, bool useParentTargeting = false, bool createInitialState = true, List<CItem.EUsageType> slotUsagesToRefresh = null, bool toDiscardPile = false, bool removeWhenDone = false, bool canExcludeCards = false, List<(string, TValue)> miscVars = null)
	{
		CAbility cAbility = null;
		switch (abilityType)
		{
		case EAbilityType.ShowNextAttackModifier:
			cAbility = new CAbilityShowNextAttackModifier();
			break;
		case EAbilityType.SetNextAttackModifiers:
			cAbility = new CAbilitySetNextAttackModifiers();
			break;
		case EAbilityType.Summon:
			cAbility = new CAbilitySummon(summons);
			break;
		case EAbilityType.Move:
			cAbility = new CAbilityMove(jump, fly, isDefault, moveRestrictionType, ignoreDifficultTerrain, ignoreHazardousTerrain, ignoreBlockedTileMoveCost, carryOtherActorsOnHex: false, aiFocusOverride);
			break;
		case EAbilityType.Attack:
			cAbility = new CAbilityAttack(multiPassAttack || (miscAbilityData != null && miscAbilityData.TargetOneEnemyWithAllAttacks.HasValue && (miscAbilityData?.TargetOneEnemyWithAllAttacks.Value ?? false)), chainAttack, pierce, isDefault, attackEffects, chainAttackRange, chainAttackDamageReduction, damageSelfBeforeAttack, elementsToInfuse, elementalConditions, exploitVulnerability);
			break;
		case EAbilityType.Heal:
			cAbility = new CAbilityHeal(healData);
			break;
		case EAbilityType.Damage:
			cAbility = new CAbilityDamage();
			break;
		case EAbilityType.GainXP:
			cAbility = new CAbilityXP();
			break;
		case EAbilityType.Loot:
			cAbility = new CAbilityLoot(lootData);
			break;
		case EAbilityType.PreventDamage:
			cAbility = new CAbilityPreventDamage(attackSourcesOnly);
			break;
		case EAbilityType.Shield:
			cAbility = new CAbilityShield();
			break;
		case EAbilityType.AddTarget:
			cAbility = new CAbilityAddTarget();
			break;
		case EAbilityType.AddHeal:
			cAbility = new CAbilityAddHeal();
			break;
		case EAbilityType.AddRange:
			cAbility = new CAbilityAddRange();
			break;
		case EAbilityType.AddCondition:
			cAbility = new CAbilityAddCondition();
			break;
		case EAbilityType.AttackersGainDisadvantage:
			cAbility = new CAbilityAttackersGainDisadvantage();
			break;
		case EAbilityType.Redirect:
			cAbility = new CAbilityRedirect();
			break;
		case EAbilityType.TransferDamage:
			cAbility = new CAbilityTransferDamage();
			break;
		case EAbilityType.MirrorDamage:
			cAbility = new CAbilityMirrorDamage();
			break;
		case EAbilityType.Retaliate:
			cAbility = new CAbilityRetaliate(retaliateRange);
			break;
		case EAbilityType.Trap:
			cAbility = new CAbilityTrap(trapData);
			break;
		case EAbilityType.RecoverLostCards:
			cAbility = new CAbilityRecoverLostCards(recoverCardsAbilityTypeFilter);
			break;
		case EAbilityType.Create:
			cAbility = new CAbilityCreate(propName);
			break;
		case EAbilityType.CreateTerrain:
			cAbility = new CAbilityCreateTerrain(propName);
			break;
		case EAbilityType.CreateShadow:
			cAbility = new CAbilityCreateShadow();
			break;
		case EAbilityType.CreateLimitedHazardousTerrain:
			cAbility = new CAbilityCreateLimitedHazardousTerrain();
			break;
		case EAbilityType.RemoveShadow:
			cAbility = new CAbilityRemoveShadow();
			break;
		case EAbilityType.Invisible:
			cAbility = new CAbilityInvisible(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Poison:
			cAbility = new CAbilityPoison();
			break;
		case EAbilityType.Bless:
			cAbility = new CAbilityBless();
			break;
		case EAbilityType.Curse:
			cAbility = new CAbilityCurse();
			break;
		case EAbilityType.Disarm:
		{
			EConditionDecTrigger decTrigger = ((conditionDecTrigger == EConditionDecTrigger.Never || conditionDecTrigger == EConditionDecTrigger.WhenAnimatedClaymoreEnds) ? conditionDecTrigger : EConditionDecTrigger.Turns);
			cAbility = new CAbilityDisarm(conditionDuration, decTrigger);
			break;
		}
		case EAbilityType.Immobilize:
			cAbility = new CAbilityImmobilize(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Muddle:
			cAbility = new CAbilityMuddle(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Strengthen:
			cAbility = new CAbilityStrengthen(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Advantage:
			cAbility = new CAbilityAdvantage(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Stun:
			cAbility = new CAbilityStun(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Bane:
			cAbility = new CAbilityBane();
			break;
		case EAbilityType.Overheal:
			cAbility = new CAbilityOverheal(strength, conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Wound:
			cAbility = new CAbilityWound();
			break;
		case EAbilityType.Infuse:
			cAbility = new CAbilityInfuse(elementsToInfuse, showElementPicker, parentBaseCard);
			break;
		case EAbilityType.ConsumeElement:
			cAbility = new CAbilityConsumeElement(elementsToInfuse, showElementPicker, parentBaseCard);
			break;
		case EAbilityType.Consume:
			cAbility = new CAbilityConsume(elementsToInfuse, showElementPicker, consumes);
			break;
		case EAbilityType.RefreshItemCards:
			cAbility = new CAbilityRefreshItemCards(slotsToRefresh, slotStatesToRefresh, slotUsagesToRefresh);
			break;
		case EAbilityType.Teleport:
			cAbility = new CAbilityTeleport(teleportData);
			break;
		case EAbilityType.Kill:
			cAbility = new CAbilityKill();
			break;
		case EAbilityType.Capture:
			cAbility = new CAbilityCapture();
			break;
		case EAbilityType.DisarmTrap:
			cAbility = new CAbilityDisarmTrap(targetActorWithTrapEffects, targetActorWithTrapEffectRange);
			break;
		case EAbilityType.DestroyObstacle:
			cAbility = new CAbilityDestroyObstacle(destroyObstacleData);
			break;
		case EAbilityType.DestroyTerrain:
			cAbility = new CAbilityDestroyTerrain();
			break;
		case EAbilityType.RecoverDiscardedCards:
			cAbility = new CAbilityRecoverDiscardedCards(recoverCardsAbilityTypeFilter);
			break;
		case EAbilityType.PlayDiscardedCard:
			cAbility = new CAbilityPlayDiscardedCard(recoverCardsAbilityTypeFilter, supplyCardsToGive, chainAttackDamageReduction, useSpecialBaseStat, useParentTargeting, cardAction);
			break;
		case EAbilityType.Teleport2:
			cAbility = new CAbilityTeleport2();
			break;
		case EAbilityType.Pull:
			cAbility = new CAbilityPull(pullType);
			break;
		case EAbilityType.Push:
			cAbility = new CAbilityPush(additionalPushEffect, additionalPushEffectDamage, additionalPushEffectXP);
			break;
		case EAbilityType.ControlActor:
			cAbility = new CAbilityControlActor(controlActorData);
			break;
		case EAbilityType.AddAugment:
			cAbility = new CAbilityAddAugment();
			break;
		case EAbilityType.AddSong:
			cAbility = new CAbilityAddSong();
			break;
		case EAbilityType.PlaySong:
			cAbility = new CAbilityPlaySong();
			break;
		case EAbilityType.OverrideAugmentAttackType:
			cAbility = new CAbilityOverrideAugmentAttackType(overrideAugmentAttackType);
			break;
		case EAbilityType.MergedCreateAttack:
			cAbility = new CAbilityMergedCreateAttack((CAbilityCreate)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MergedDestroyObstacleAttack:
			cAbility = new CAbilityMergedDestroyAttack((CAbilityDestroyObstacle)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MergedMoveAttack:
			cAbility = new CAbilityMergedMoveAttack((CAbilityMove)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MoveObstacle:
			cAbility = new CAbilityMoveObstacle(moveObstacleRange, moveObstacleAnimOverload);
			break;
		case EAbilityType.MoveTerrain:
			cAbility = new CAbilityMoveTerrain(moveObstacleRange);
			break;
		case EAbilityType.MergedMoveObstacleAttack:
			cAbility = new CAbilityMergedMoveObstacleAttack((CAbilityMoveObstacle)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MergedDisarmTrapDestroyObstacles:
			cAbility = new CAbilityMergedDisarmTrapDestroyObstacle((CAbilityDisarmTrap)mergedAbilities[0], (CAbilityDestroyObstacle)mergedAbilities[1]);
			break;
		case EAbilityType.MergedKillCreate:
			cAbility = new CAbilityMergedKillCreate((CAbilityKill)mergedAbilities[0], (CAbilityCreate)mergedAbilities[1]);
			break;
		case EAbilityType.AddActiveBonus:
			cAbility = new CAbilityAddActiveBonus(addActiveBonusAbility);
			break;
		case EAbilityType.Null:
			cAbility = new CAbilityNull();
			break;
		case EAbilityType.ShiftForm:
			cAbility = new CAbilityShiftForm();
			break;
		case EAbilityType.ImmunityTo:
			cAbility = new CAbilityImmunityTo(immunityToAbilityTypes, immuneToAttackTypes, immuneToSelfDamage);
			break;
		case EAbilityType.ChooseAbility:
			cAbility = new CAbilityChoose(chooseAbilities);
			break;
		case EAbilityType.Choose:
			cAbility = new CAbilityChooseAbility(chooseAbilities);
			break;
		case EAbilityType.LoseCards:
			cAbility = new CAbilityLoseCards();
			break;
		case EAbilityType.IncreaseCardLimit:
			cAbility = new CAbilityIncreaseCardLimit();
			break;
		case EAbilityType.StopFlying:
			cAbility = new CAbilityStopFlying();
			break;
		case EAbilityType.Immovable:
			cAbility = new CAbilityImmovable();
			break;
		case EAbilityType.ExtraTurn:
			cAbility = new CAbilityExtraTurn(extraTurnType, extraTurnAbilityTypeFilter, maxExtraTurnAbilityLevel.Value, actionMonsterClassID);
			break;
		case EAbilityType.ImprovedShortRest:
			cAbility = new CAbilityImprovedShortRest(canExcludeCards);
			break;
		case EAbilityType.AdjustInitiative:
			cAbility = new CAbilityAdjustInitiative();
			break;
		case EAbilityType.Swap:
			cAbility = new CAbilitySwap(swapAbilityFirstTargetFilter, swapAbilitySecondTargetFilter);
			break;
		case EAbilityType.RedistributeDamage:
			cAbility = new CAbilityRedistributeDamage();
			break;
		case EAbilityType.ForgoActionsForCompanion:
			cAbility = new CAbilityForgoActionsForCompanion(forgoTopActionAbility, forgoBottomActionAbility);
			break;
		case EAbilityType.ChangeModifier:
			cAbility = new CAbilityChangeModifier();
			break;
		case EAbilityType.ChangeCondition:
			cAbility = new CAbilityChangeModifier();
			break;
		case EAbilityType.RemoveConditions:
			cAbility = new CAbilityRemoveConditions();
			break;
		case EAbilityType.ShuffleModifierDeck:
			cAbility = new CAbilityShuffleModifierDeck();
			break;
		case EAbilityType.DiscardCards:
			cAbility = new CAbilityDiscardCards();
			break;
		case EAbilityType.NullTargeting:
			cAbility = new CAbilityNullTargeting();
			break;
		case EAbilityType.AddDoom:
			cAbility = new CAbilityAddDoom(doom);
			break;
		case EAbilityType.AddDoomSlots:
			cAbility = new CAbilityAddDoomSlots();
			break;
		case EAbilityType.TransferDooms:
			cAbility = new CAbilityTransferDooms();
			break;
		case EAbilityType.GiveSupplyCard:
			cAbility = new CAbilityGiveSupplyCard(supplyCardsToGive);
			break;
		case EAbilityType.Fear:
			cAbility = new CAbilityFear(jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain);
			break;
		case EAbilityType.DeactivateSpawner:
			cAbility = new CAbilityDeactivateSpawner();
			break;
		case EAbilityType.RemoveActorFromMap:
			cAbility = new CAbilityRemoveActorFromMap();
			break;
		case EAbilityType.ConsumeItemCards:
			cAbility = new CAbilityConsumeItemCards(slotsToRefresh, slotStatesToRefresh, slotUsagesToRefresh);
			break;
		case EAbilityType.Invulnerability:
			cAbility = new CAbilityInvulnerability();
			break;
		case EAbilityType.PierceInvulnerability:
			cAbility = new CAbilityPierceInvulnerability();
			break;
		case EAbilityType.Sleep:
			cAbility = new CAbilitySleep();
			break;
		case EAbilityType.ActivateSpawner:
			cAbility = new CAbilityActivateSpawner();
			break;
		case EAbilityType.ItemLock:
			cAbility = new CAbilityItemLock();
			break;
		case EAbilityType.Untargetable:
			cAbility = new CAbilityUntargetable();
			break;
		case EAbilityType.LoseGoalChestReward:
			cAbility = new CAbilityLoseGoalChestReward();
			break;
		case EAbilityType.AddModifierToMonsterDeck:
			cAbility = new CAbilityAddModifierToMonsterDeck(addOrRemoveModifiers);
			break;
		case EAbilityType.RecoverResources:
			cAbility = new CAbilityRecoverResources();
			break;
		case EAbilityType.MoveTrap:
			cAbility = new CAbilityMoveTrap(moveObstacleRange);
			break;
		case EAbilityType.MoveShadow:
			cAbility = new CAbilityMoveShadow();
			abilityFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.All);
			break;
		case EAbilityType.PerformFromShadow:
			cAbility = new CAbilityPerformFromShadow(mergedAbilities, removeWhenDone);
			break;
		case EAbilityType.BlockHealing:
			cAbility = new CAbilityBlockHealing(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.NeutralizeShield:
			cAbility = new CAbilityNeutralizeShield(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.ChangeCharacterModel:
			cAbility = new CAbilityChangeCharacterModel(changeCharacterModel);
			break;
		case EAbilityType.NullHex:
			cAbility = new CAbilityNullHex();
			break;
		case EAbilityType.HealthReduction:
			cAbility = new CAbilityHealthReduction();
			break;
		case EAbilityType.ChangeAllegiance:
			cAbility = new CAbilityChangeAllegiance(changeAllegianceData);
			break;
		case EAbilityType.Revive:
			cAbility = new CAbilityRevive(summons);
			break;
		case EAbilityType.DisableCardAction:
			cAbility = new CAbilityDisableCardAction(disableCardActionData);
			break;
		case EAbilityType.Regenerate:
			cAbility = new CAbilityRegenerate((conditionDecTrigger == EConditionDecTrigger.Never || conditionDecTrigger == EConditionDecTrigger.WhenFrozenFistMechanicActiveBonusEnds) ? conditionDecTrigger : EConditionDecTrigger.WhenDamaged);
			break;
		case EAbilityType.Ward:
			cAbility = new CAbilityWard();
			break;
		case EAbilityType.Brittle:
			cAbility = new CAbilityBrittle();
			break;
		case EAbilityType.Impair:
			cAbility = new CAbilityImpair(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.RepeatAbility:
			cAbility = new CAbilityRepeatAbility(abilityType);
			break;
		case EAbilityType.ExploitableVulnerability:
			cAbility = new CAbilityExploitableVulnerability();
			break;
		case EAbilityType.DuplicateCondition:
			cAbility = new CAbilityDuplicateCondition(abilityType);
			break;
		case EAbilityType.PreventLoseCard:
			cAbility = new CAbilityPreventLoseCard(abilityType);
			break;
		case EAbilityType.AddAttackModifier:
			cAbility = new CAbilityAddAttackModifier(addOrRemoveModifiers, toDiscardPile);
			break;
		case EAbilityType.RemoveAttackModifier:
			cAbility = new CAbilityRemoveAttackModifier(addOrRemoveModifiers);
			break;
		case EAbilityType.ExcludeCardsFromShortRest:
			cAbility = new CAbilityExcludeCardsFromShortRest();
			break;
		case EAbilityType.MoveActiveBonusTracker:
			cAbility = new CAbilityMoveActiveBonusTracker();
			break;
		case EAbilityType.ReviveDowned:
			cAbility = new CAbilityReviveDowned();
			break;
		case EAbilityType.ControlSummonTurn:
			cAbility = new CAbilityControlSummonTurn();
			break;
		case EAbilityType.ModeTransfer:
		{
			CAbilityModeTransfer.EModeUse modeUsage = CAbilityModeTransfer.EModeUse.Transfer;
			(string, TValue)? tuple9 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "ModeUsage");
			if (tuple9.HasValue)
			{
				modeUsage = (CAbilityModeTransfer.EModeUse)tuple9.Value.Item2.GetInt();
			}
			bool dontSummon = false;
			(string, TValue)? tuple10 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "ModeDontSummon");
			if (tuple10.HasValue)
			{
				dontSummon = tuple10.Value.Item2.GetBool();
			}
			cAbility = new CAbilityModeTransfer(modeUsage, dontSummon);
			break;
		}
		case EAbilityType.SelectAndPlayCard:
		{
			List<CBaseCard.ECardPile> pilesToChooseFrom = new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand };
			(string, TValue)? tuple6 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "SelectAndPlayCardPiles");
			bool blockXP = false;
			int num3 = -1;
			if (tuple6.HasValue)
			{
				pilesToChooseFrom = tuple6.Value.Item2.GetObject<List<CBaseCard.ECardPile>>();
			}
			(string, TValue)? tuple7 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "SelectAndPlayBlockXP");
			if (tuple7.HasValue)
			{
				blockXP = tuple7.Value.Item2.GetBool();
			}
			(string, TValue)? tuple8 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "MinRequiredCards");
			if (tuple8.HasValue)
			{
				num3 = tuple8.Value.Item2.GetInt();
			}
			cAbility = new CAbilitySelectAndPlayCard(pilesToChooseFrom, blockXP, (num3 >= 1) ? num3 : strength);
			break;
		}
		case EAbilityType.SelectAndMoveCard:
		{
			List<CBaseCard.ECardPile> pilesToMoveFrom2 = new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand };
			(string, TValue)? tuple3 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "SelectAndMoveCardPilesToMoveFrom");
			int num2 = -1;
			if (tuple3.HasValue)
			{
				pilesToMoveFrom2 = tuple3.Value.Item2.GetObject<List<CBaseCard.ECardPile>>();
			}
			CBaseCard.ECardPile pileToMoveTo = CBaseCard.ECardPile.Discarded;
			(string, TValue)? tuple4 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "SelectAndMoveCardPileToMoveTo");
			if (tuple4.HasValue)
			{
				pileToMoveTo = (CBaseCard.ECardPile)tuple4.Value.Item2.GetInt();
			}
			(string, TValue)? tuple5 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "MinRequiredCards");
			if (tuple5.HasValue)
			{
				num2 = tuple5.Value.Item2.GetInt();
			}
			cAbility = new CAbilitySelectAndMoveCard(pilesToMoveFrom2, pileToMoveTo, (num2 >= 1) ? num2 : strength);
			break;
		}
		case EAbilityType.SelectCard:
		{
			List<CBaseCard.ECardPile> pilesToMoveFrom = new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand };
			(string, TValue)? tuple = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "SelectAndMoveCardPilesToMoveFrom");
			int num = -1;
			if (tuple.HasValue)
			{
				pilesToMoveFrom = tuple.Value.Item2.GetObject<List<CBaseCard.ECardPile>>();
			}
			(string, TValue)? tuple2 = miscVars?.SingleOrDefault(((string, TValue) x) => x.Item1 == "MinRequiredCards");
			if (tuple2.HasValue)
			{
				num = tuple2.Value.Item2.GetInt();
			}
			cAbility = new CAbilitySelectCard(pilesToMoveFrom, (num >= 1) ? num : strength);
			break;
		}
		default:
			Log.ErrorWithReport("Error creating ability " + name + ".  Invalid Ability Type " + abilityType.ToString() + ".");
			return null;
		}
		if (abilityFilter == null)
		{
			abilityFilter = new CAbilityFilterContainer();
		}
		if (abilityEnhancements == null)
		{
			abilityEnhancements = new List<CEnhancement>();
		}
		if (startAbilityConditions == null)
		{
			startAbilityConditions = new CAbilityRequirements();
		}
		if (conditionalOverrides == null)
		{
			conditionalOverrides = new List<CConditionalOverride>();
		}
		if (CSRLYML.IsLoading)
		{
			cAbility.Original = cAbility;
		}
		cAbility.ID = Guid.NewGuid();
		cAbility.Name = name;
		cAbility.AbilityType = abilityType;
		cAbility.Strength = strength;
		cAbility.UseSpecialBaseStat = useSpecialBaseStat;
		cAbility.Range = range;
		cAbility.NumberTargets = numberTargets;
		cAbility.Tokens = tokens;
		cAbility.Flags = flags;
		cAbility.AbilityFilter = abilityFilter;
		cAbility.TileFilter = tileFilter;
		cAbility.AnimOverload = animationOverload;
		cAbility.AnimOverload2 = animationOverload2;
		cAbility.IsMonsterAbility = isMonster;
		cAbility.ParentName = parentName;
		cAbility.m_IsSubAbility = isSubAbility;
		cAbility.IsInlineSubAbility = isInlineSubAbility;
		cAbility.IsTargetedAbility = isTargetedAbility;
		cAbility.SpawnDelay = spawnDelay;
		cAbility.AllTargetsOnMovePath = allTargetsOnMovePath;
		cAbility.AllTargetsOnMovePathSameStartAndEnd = allTargetsOnMovePathSameStartAndEnd;
		cAbility.AllTargetsOnAttackPath = allTargetsOnAttackPath;
		cAbility.AllTargetsInRangeAlongPath = allTargetsInRangeAlongPath;
		cAbility.ConditionalOverrides = conditionalOverrides;
		cAbility.StartAbilityRequirements = startAbilityConditions;
		cAbility.AbilityXP = abilityXP;
		cAbility.IsItemAbility = isItemAbility;
		cAbility.Augment = augment;
		cAbility.Song = song;
		cAbility.m_IsMergedAbility = isMergedAbility;
		cAbility.m_IsModifierAbility = isModifierAbility;
		cAbility.CanUndoOverride = canUndoOverride;
		cAbility.m_XpPerTargetData = xpPerTargetData;
		cAbility.MiscAbilityData = miscAbilityData;
		cAbility.SkipAnim = skipAnim;
		cAbility.m_AbilityEnhancements = abilityEnhancements;
		cAbility.ResourcesToAddOnAbilityEnd = resourcesToAddOnAbilityEnd;
		cAbility.ResourcesToGiveToTargets = resourcesToGiveToTargets;
		cAbility.ResourcesToTakeFromTargets = resourcesToTakeFromTargets;
		cAbility.SubAbilities = ((subAbilities != null) ? subAbilities : new List<CAbility>());
		cAbility.AreaEffect = areaEffect;
		cAbility.AreaEffectYMLString = areaEffectYMLString;
		cAbility.AreaEffectLayoutOverrideYMLString = areaEffectLayoutOverrideYMLString;
		cAbility.AreaEffectFlipAxis = areaEffectFlipAxis;
		cAbility.ActiveBonusData = ((activeBonusData != null) ? activeBonusData : new AbilityData.ActiveBonusData());
		cAbility.ActiveBonusYML = activeBonusYML;
		cAbility.VarsContext = new CVarsContext(cAbility);
		cAbility.StatIsBasedOnXEntries = statIsBasedOnXEntries ?? new List<AbilityData.StatIsBasedOnXData>();
		cAbility.AddAttackBaseStat = addAttackBaseStat;
		cAbility.StrengthIsBase = strengthIsBase;
		cAbility.RangeIsBase = rangeIsBase;
		cAbility.TargetIsBase = targetIsBase;
		cAbility.TargetsSet = numberTargets != 0;
		cAbility.AbilityText = text;
		cAbility.AbilityTextOnly = textOnly;
		cAbility.AbilityNoLayoutSpace = noLayoutSpace;
		cAbility.ShowRange = showRange;
		cAbility.ShowTarget = showTarget;
		cAbility.ShowArea = showArea;
		cAbility.OnDeath = onDeath;
		cAbility.IsConsumeAbility = isConsumeAbility;
		cAbility.Targeting = targeting;
		cAbility.TargetingVars = targetingVars;
		cAbility.Vars = vars;
		cAbility.PerformVars = performVars;
		cAbility.MiscVars = miscVars;
		cAbility.PreviewEffectId = previewEffectId;
		cAbility.PreviewEffectText = previewEffectText;
		cAbility.HelpBoxTooltipLocKey = helpBoxTooltipLocKey;
		if (positiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition condition in positiveConditions)
			{
				if (condition == CCondition.EPositiveCondition.NA)
				{
					continue;
				}
				if (condition == CCondition.EPositiveCondition.Bless && cAbility.PositiveConditions.ContainsKey(condition))
				{
					if (cAbility.PositiveConditions[condition].Strength == 0)
					{
						cAbility.PositiveConditions[condition].Strength++;
					}
					cAbility.PositiveConditions[condition].Strength++;
				}
				else
				{
					if (cAbility.m_PositiveConditions.ContainsKey(condition))
					{
						continue;
					}
					try
					{
						EAbilityType abilityType2 = AbilityTypes.Single((EAbilityType x) => x.ToString() == condition.ToString());
						cAbility.m_PositiveConditions.Add(condition, CreateAbility(abilityType2, abilityFilter, cAbility.IsMonsterAbility, cAbility.IsTargetedAbility, 0, 1, 1, 0, EAbilityFlag.None, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: false, null, null, null, isSub: false, isInline: false, cAbility.Name));
					}
					catch
					{
						Log.ErrorWithReport("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
					}
				}
			}
		}
		if (negativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition condition2 in negativeConditions)
			{
				if (condition2 == CCondition.ENegativeCondition.NA)
				{
					continue;
				}
				if (condition2 == CCondition.ENegativeCondition.Curse && cAbility.NegativeConditions.ContainsKey(condition2))
				{
					if (cAbility.NegativeConditions[condition2].Strength == 0)
					{
						cAbility.NegativeConditions[condition2].Strength++;
					}
					cAbility.NegativeConditions[condition2].Strength++;
					continue;
				}
				try
				{
					if (!cAbility.m_NegativeConditions.ContainsKey(condition2))
					{
						EAbilityType abilityType3 = AbilityTypes.Single((EAbilityType x) => x.ToString() == condition2.ToString());
						cAbility.m_NegativeConditions.Add(condition2, CreateAbility(abilityType3, abilityFilter, cAbility.IsMonsterAbility, cAbility.IsTargetedAbility, 0, 1, 1, 0, EAbilityFlag.None, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: false, null, null, null, isSub: false, isInline: false, cAbility.Name));
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Condition " + condition2.ToString() + " could not be found in EAbilityType enum.\n" + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}
		if (createInitialState)
		{
			cAbility.AbilityInitialState = CopyAbility(cAbility, generateNewID: false);
		}
		return cAbility;
	}

	public void EnhanceAbility(CEnhancement enhancement)
	{
		CEnhancement cEnhancement = AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == enhancement.EnhancementLine && s.EnhancementSlot == enhancement.EnhancementSlot);
		if (cEnhancement != null)
		{
			cEnhancement.Enhancement = enhancement.Enhancement;
			cEnhancement.PaidPrice = enhancement.PaidPrice;
		}
	}

	public string GetTargetAbilityDescription()
	{
		string text = "   ";
		foreach (CAbility subAbility in SubAbilities)
		{
			if (text.Length > 0)
			{
				text += " Then ";
			}
			text = text + "For Each Target " + subAbility.GetDescription();
		}
		return text;
	}

	public string GetAreaEffectDescription()
	{
		if (AreaEffect == null)
		{
			return "";
		}
		return "   Area Effect : " + AreaEffect.Name;
	}

	public void PreProcessTargetAbility(CActor targetActor)
	{
		if (SubAbilities == null)
		{
			return;
		}
		foreach (CAbility subAbility in SubAbilities)
		{
			if (subAbility.AbilityType == EAbilityType.Damage && subAbility.IsInlineSubAbility)
			{
				CTargetAbilityRange_MessageData cTargetAbilityRange_MessageData = new CTargetAbilityRange_MessageData(targetActor);
				cTargetAbilityRange_MessageData.TargetingActor = targetActor;
				cTargetAbilityRange_MessageData.m_TargetAbilityRange = GameState.GetTilesInRange(targetActor, subAbility.Range, Targeting, emptyTilesOnly: false);
				ScenarioRuleClient.MessageHandler(cTargetAbilityRange_MessageData);
			}
		}
	}

	protected virtual void LogPositiveCondition(CCondition.EPositiveCondition key, CActor target)
	{
		CConditionCombatLog_MessageData message = new CConditionCombatLog_MessageData(TargetingActor, key, CCondition.ENegativeCondition.NA, target);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected virtual void LogNegativeCondition(CCondition.ENegativeCondition key, CActor target)
	{
		CConditionCombatLog_MessageData message = new CConditionCombatLog_MessageData(TargetingActor, CCondition.EPositiveCondition.NA, key, target);
		ScenarioRuleClient.MessageHandler(message);
	}

	public void ProcessPositiveStatusEffects(CActor target)
	{
		if (m_PositiveConditions == null)
		{
			return;
		}
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_PositiveConditions)
		{
			CAbility value = positiveCondition.Value;
			if (value.AbilityType == AbilityType)
			{
				continue;
			}
			value.Start(TargetingActor, FilterActor);
			if (!((CAbilityTargeting)value).ApplyToActor(target))
			{
				continue;
			}
			LogPositiveCondition(positiveCondition.Key, target);
			if (value.AbilityType == EAbilityType.Curse || value.AbilityType == EAbilityType.Bless)
			{
				for (int i = 1; i < value.Strength; i++)
				{
					LogPositiveCondition(positiveCondition.Key, target);
				}
			}
			TargetingActor.m_OnConditionApplyToActorListeners?.Invoke(value, target);
			target.m_OnConditionApplyToActorListeners?.Invoke(value, target);
			AbilityHasHappened = true;
		}
	}

	public void ProcessNegativeStatusEffects(CActor target)
	{
		if (m_NegativeConditions == null)
		{
			return;
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_NegativeConditions)
		{
			CAbility value = negativeCondition.Value;
			if (value.AbilityType == AbilityType)
			{
				continue;
			}
			value.SetMiscVar("TempDisabledConditions", true);
			value.Start(TargetingActor, TargetingActor);
			if (!(value is CAbilityCondition cAbilityCondition) || !cAbilityCondition.ApplyToActor(target))
			{
				continue;
			}
			LogNegativeCondition(negativeCondition.Key, target);
			if (value.AbilityType == EAbilityType.Curse || value.AbilityType == EAbilityType.Bless)
			{
				for (int i = 1; i < value.Strength; i++)
				{
					LogNegativeCondition(negativeCondition.Key, target);
				}
			}
			if (target != TargetingActor)
			{
				TargetingActor.m_OnConditionApplyToActorListeners?.Invoke(cAbilityCondition, target);
			}
			target.m_OnConditionApplyToActorListeners?.Invoke(cAbilityCondition, target);
			AbilityHasHappened = true;
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.Tokens.ActivateTempDisabledConditions();
		}
	}

	public void UpdateAreaEffect(CTile positionTile, float rotation, bool flipped)
	{
		if (m_AreaEffectLocked)
		{
			return;
		}
		bool flag = rotation != AreaEffectAngle;
		AreaEffectAngle = rotation;
		AreaEffectFlipped = flipped;
		m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, positionTile, AreaEffect, rotation, flipped, AreaEffectFlipAxis, TilesInRange, ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks, Range, MiscAbilityData?.MinRange ?? (-1));
		if (flag)
		{
			string text = "AoE Rotation changed, tiles in AoE are now: ";
			foreach (CTile item in m_ValidTilesInAreaEffectIncludingBlocked)
			{
				text = text + item.m_ArrayIndex.ToString() + ", ";
			}
			Log.SimpleInfo(text);
		}
		if (this is CAbilityAttack { AttackSummary: { } attackSummary } cAbilityAttack)
		{
			attackSummary.UpdateTargetData(cAbilityAttack, cAbilityAttack.ActiveSingleTargetItems, cAbilityAttack.ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
		}
	}

	public List<CActor> GetValidActorsInArea(List<CTile> tiles, bool ignoreImmunity = false)
	{
		List<CActor> list = new List<CActor>();
		if (tiles != null)
		{
			foreach (CTile tile in tiles)
			{
				CActor actorOnTile = GameState.GetActorOnTile(tile, FilterActor, AbilityFilter, null, IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
				if (actorOnTile != null && (ignoreImmunity || !ImmuneToAbility(actorOnTile, this, targeting: true)) && !IsMirroredHealthCloneTargeted(actorOnTile))
				{
					list.Add(actorOnTile);
				}
			}
		}
		return list;
	}

	public virtual void RemoveImmuneActorsFromList(ref List<CActor> actorList, bool onlyCheck = true)
	{
		for (int num = actorList.Count - 1; num >= 0; num--)
		{
			CActor cActor = actorList[num];
			if (cActor != null && ImmuneToAbility(cActor, this))
			{
				if (!onlyCheck)
				{
					actorList.Remove(cActor);
				}
				if (!m_ImmuneActorsInRange.Contains(cActor))
				{
					m_ImmuneActorsInRange.Add(cActor);
				}
			}
		}
	}

	public void SetID(Guid id)
	{
		ID = id;
	}

	public virtual bool ApplyToActor(CActor actor)
	{
		bool flag = ImmuneToAbility(actor, this);
		actor.m_OnActorImmuneToConditionListeners?.Invoke(this, actor, flag);
		if (ActorsTargeted == null)
		{
			ActorsTargeted = new List<CActor>();
		}
		ActorsTargeted.Add(actor);
		if (!flag)
		{
			if (ResourcesToTakeFromTargets != null && ResourcesToTakeFromTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in ResourcesToTakeFromTargets)
				{
					if (actor.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
					{
						actor.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
						TargetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
					}
				}
			}
			if (ResourcesToGiveToTargets != null && ResourcesToGiveToTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToGiveToTarget in ResourcesToGiveToTargets)
				{
					if (TargetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
					{
						TargetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
						actor.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					}
				}
			}
			if (!IsScenarioModifierAbility)
			{
				LogEvent(ESESubTypeAbility.ApplyToActor);
			}
			if (XpPerTargetData != null && TargetingActor != null)
			{
				TargetingActor.GainXP(XpPerTargetData.AttackTarget(TargetingActor, actor), XPSourceKey);
			}
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			GameState.ShowItemSelection(playerActor, actor, this, CItem.EItemTrigger.OnAbilityApplied);
		}
		return !flag;
	}

	public static bool ImmuneToAbility(CActor actor, CAbility ability, bool targeting = false)
	{
		if (ScenarioScript.current != null && ScenarioScript.current.ShouldIgnoreImmunity(actor, ability))
		{
			return false;
		}
		if (ability.ActiveBonusData != null && ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.MoveScript))
		{
			return false;
		}
		if (targeting && ScenarioScript.current != null && ScenarioScript.current.ShouldIgnoreImmunityWhileTargeting(actor, ability))
		{
			return false;
		}
		if (ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA && (ability.AbilityType == EAbilityType.Attack || ability.AbilityType == EAbilityType.Move))
		{
			return false;
		}
		bool flag;
		if (actor is CPlayerActor)
		{
			foreach (CActiveBonus item in actor.FindApplicableActiveBonuses(EAbilityType.ImmunityTo))
			{
				CAbilityImmunityTo cAbilityImmunityTo = item.Ability as CAbilityImmunityTo;
				if (!cAbilityImmunityTo.ImmuneToAbilityTypes.Contains(ability.AbilityType))
				{
					continue;
				}
				AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
				if (miscAbilityData != null)
				{
					bool? bypassImmunity = miscAbilityData.BypassImmunity;
					flag = true;
					if (bypassImmunity == flag)
					{
						continue;
					}
				}
				if ((cAbilityImmunityTo.NotImmuneToSelf && ability.TargetingActor == actor) || (cAbilityImmunityTo.GetMiscVar("OnlyImmuneToEnemies").GetBool() && (ability?.TargetingActor == null || !CustomAbility.IsEnemy(actor, ability.TargetingActor))))
				{
					continue;
				}
				flag = true;
				goto IL_01e6;
			}
		}
		else if (actor.Immunities.Contains(ability.AbilityType))
		{
			AbilityData.MiscAbilityData miscAbilityData2 = ability.MiscAbilityData;
			if (miscAbilityData2 == null || miscAbilityData2.BypassImmunity != true)
			{
				return true;
			}
		}
		if (ability is CAbilityAttack cAbilityAttack)
		{
			foreach (EAttackType item2 in cAbilityAttack.AttackIsAttackTypes())
			{
				if (!actor.AttackTypeImmunities.Contains(item2))
				{
					continue;
				}
				flag = true;
				goto IL_01e6;
			}
		}
		return false;
		IL_01e6:
		return flag;
	}

	public void CheckForAdjacentSleepingActorsToAwaken()
	{
		foreach (CActor item in GameState.GetActorsInRange(TargetingActor, TargetingActor, 1, new List<CActor> { TargetingActor }, CAbilityFilterContainer.CreateDefaultFilter(), null, null, isTargetedAbility: false, null, true))
		{
			if (item.Tokens.HasKey(CCondition.ENegativeCondition.Sleep) && !CActor.AreActorsAllied(TargetingActor.Type, item.Type, TargetingActor, item))
			{
				item.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep, EConditionDecTrigger.None, TargetingActor);
				CActorAwakened_MessageData message = new CActorAwakened_MessageData(item);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	private static Dictionary<EAbilityType, EAbilityStatType> GetAbilityDisplayValues()
	{
		Dictionary<EAbilityType, EAbilityStatType> dictionary = new Dictionary<EAbilityType, EAbilityStatType>();
		EAbilityType[] abilityTypes = AbilityTypes;
		foreach (EAbilityType eAbilityType in abilityTypes)
		{
			switch (eAbilityType)
			{
			case EAbilityType.Move:
			case EAbilityType.Attack:
			case EAbilityType.Heal:
			case EAbilityType.Damage:
			case EAbilityType.PreventDamage:
			case EAbilityType.Shield:
			case EAbilityType.Retaliate:
			case EAbilityType.Trap:
			case EAbilityType.RecoverLostCards:
			case EAbilityType.Push:
			case EAbilityType.Pull:
			case EAbilityType.Teleport2:
				dictionary.Add(eAbilityType, EAbilityStatType.Strength);
				break;
			case EAbilityType.Loot:
				dictionary.Add(eAbilityType, EAbilityStatType.Range);
				break;
			default:
				dictionary.Add(eAbilityType, EAbilityStatType.None);
				break;
			case EAbilityType.None:
				break;
			}
		}
		return dictionary;
	}

	public virtual void ToggleSingleTargetItem(CItem item)
	{
		if (m_ActiveSingleTargetItems.Contains(item))
		{
			item.SingleTarget = null;
			m_ActiveSingleTargetItems.Remove(item);
			return;
		}
		item.SingleTarget = null;
		m_ActiveSingleTargetItems.Add(item);
		if (ActorsToTarget.Count == 1)
		{
			ApplySingleTargetItem(ActorsToTarget[0]);
			Perform();
		}
	}

	public virtual bool IsSingleTargetItemActive(CItem item)
	{
		return m_ActiveSingleTargetItems.Contains(item);
	}

	public virtual void ToggleSingleTargetActiveBonus(CActiveBonus activeBonus)
	{
		if (m_ActiveSingleTargetActiveBonuses.Contains(activeBonus))
		{
			activeBonus.SingleTarget = null;
			m_ActiveSingleTargetActiveBonuses.Remove(activeBonus);
			ActiveBonusToggled(TargetingActor, activeBonus);
		}
		else if (ActorsToTarget != null)
		{
			activeBonus.SingleTarget = null;
			m_ActiveSingleTargetActiveBonuses.Add(activeBonus);
			if (ActorsToTarget.Count == 1)
			{
				ApplySingleTargetActiveBonus(ActorsToTarget[0]);
				Perform();
			}
			else
			{
				CWaitForSingleTargetActiveBonus_MessageData cWaitForSingleTargetActiveBonus_MessageData = new CWaitForSingleTargetActiveBonus_MessageData(TargetingActor);
				cWaitForSingleTargetActiveBonus_MessageData.m_ActiveBonus = activeBonus;
				ScenarioRuleClient.MessageHandler(cWaitForSingleTargetActiveBonus_MessageData);
			}
		}
	}

	public virtual bool IsSingleTargetActiveBonusActive(CActiveBonus activeBonus)
	{
		return m_ActiveSingleTargetActiveBonuses.Contains(activeBonus);
	}

	public bool HasID(Guid id)
	{
		if (id == ID)
		{
			return true;
		}
		foreach (CAbility subAbility in SubAbilities)
		{
			if (subAbility.HasID(id))
			{
				return true;
			}
		}
		if (Augment != null && Augment.Abilities != null)
		{
			foreach (CAbility ability in Augment.Abilities)
			{
				if (ability.HasID(id))
				{
					return true;
				}
			}
		}
		if (Song != null && Song.SongEffects != null)
		{
			foreach (CSong.SongEffect songEffect in Song.SongEffects)
			{
				if (songEffect.Abilities == null)
				{
					continue;
				}
				foreach (CAbility ability2 in songEffect.Abilities)
				{
					if (ability2.HasID(id))
					{
						return true;
					}
				}
			}
		}
		if (this is CAbilityControlActor cAbilityControlActor && cAbilityControlActor.ControlActorData.ControlAbilities != null)
		{
			foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
			{
				if (controlAbility.HasID(id))
				{
					return true;
				}
			}
		}
		if (this is CAbilityAddActiveBonus cAbilityAddActiveBonus && cAbilityAddActiveBonus.AddAbility.HasID(id))
		{
			return true;
		}
		return false;
	}

	public bool IsAnOffensiveAbility()
	{
		if (AbilityType != EAbilityType.Attack && AbilityType != EAbilityType.Damage && AbilityType != EAbilityType.Poison && AbilityType != EAbilityType.Wound && AbilityType != EAbilityType.Muddle && AbilityType != EAbilityType.Immobilize && AbilityType != EAbilityType.Disarm && AbilityType != EAbilityType.Impair && AbilityType != EAbilityType.Curse && AbilityType != EAbilityType.Stun && AbilityType != EAbilityType.StopFlying && AbilityType != EAbilityType.AddCondition && AbilityType != EAbilityType.Sleep)
		{
			return AbilityType == EAbilityType.Bane;
		}
		return true;
	}

	public CAbility FindAbility(string abilityName)
	{
		return CAbilityCard.GetWithChildAbilities(this).FirstOrDefault((CAbility a) => a.Name == abilityName);
	}

	public bool IsWaitingForSingleTargetItemOrActiveBonus()
	{
		if (!IsWaitingForSingleTargetItem())
		{
			return IsWaitingForSingleTargetActiveBonus();
		}
		return true;
	}

	public bool IsWaitingForSingleTargetItem()
	{
		return PendingSingleTargetItems()?.Any() ?? false;
	}

	public IEnumerable<CItem> PendingSingleTargetItems()
	{
		if (m_ActiveSingleTargetItems == null)
		{
			return null;
		}
		return m_ActiveSingleTargetItems.Where((CItem w) => w.SingleTarget == null);
	}

	public bool IsWaitingForSingleTargetActiveBonus()
	{
		if (m_ActorsToTarget != null && m_ActorsToTarget.Count > 0)
		{
			return m_ActiveSingleTargetActiveBonuses.Exists((CActiveBonus w) => w.SingleTarget == null && w.ToggledBonus);
		}
		return false;
	}

	public void SetCanSkip(bool canSkip)
	{
		m_CanSkip = canSkip;
		if (AbilityInterruptCopy != null)
		{
			AbilityInterruptCopy.SetCanSkip(canSkip);
		}
	}

	public void SetCanUndo(bool canUndo)
	{
		m_CanUndo = canUndo;
	}

	public void SetStatBasedOnXFromTarget(CActor targetedActor, CActor targetingActor, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXDataEntries, CAbilityFilterContainer abilityFilter = null, CAbility sourceAbility = null)
	{
		if (statIsBasedOnXDataEntries == null)
		{
			return;
		}
		CAbility cAbility = sourceAbility ?? this;
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXDataEntry in statIsBasedOnXDataEntries)
		{
			int num = 0;
			float num2 = 0f;
			bool flag = false;
			_ = statIsBasedOnXDataEntry.Filter;
			switch (statIsBasedOnXDataEntry.BasedOn)
			{
			case EStatIsBasedOnXType.TargetShieldValue:
			{
				CAbilityAttack cAbilityAttack = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityAttack;
				if (cAbilityAttack?.AttackSummary != null)
				{
					CAttackSummary.TargetSummary targetSummary = cAbilityAttack.AttackSummary.FindTarget(targetedActor);
					if (targetSummary != null)
					{
						num2 = targetSummary.Shield;
						flag = true;
						break;
					}
				}
				num2 = ((targetedActor != null) ? ((float)targetedActor.CalculateShield(null, ignoreNeutralizeShield: false, includeItemShieldValues: true)) : 0f);
				flag = true;
				break;
			}
			case EStatIsBasedOnXType.TargetHPDifference:
				num2 = (float)Math.Abs(targetedActor.OriginalMaxHealth - targetedActor.Health) * statIsBasedOnXDataEntry.Multiplier;
				flag = true;
				break;
			case EStatIsBasedOnXType.TargetDamageTaken:
				num2 = (float)Math.Abs(targetedActor.MaxHealth - targetedActor.Health) * statIsBasedOnXDataEntry.Multiplier;
				flag = true;
				break;
			case EStatIsBasedOnXType.TargetDistanceRangeDiff:
				num2 = (float)Math.Abs(cAbility.Range - SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(targetedActor, targetingActor)) * statIsBasedOnXDataEntry.Multiplier;
				flag = true;
				break;
			}
			switch (statIsBasedOnXDataEntry?.RoundingType ?? EStatIsBasedOnXRoundingType.RoundOff)
			{
			case EStatIsBasedOnXRoundingType.RoundOff:
				num = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
				break;
			case EStatIsBasedOnXRoundingType.ToFloor:
				num = (int)Math.Floor(num2);
				break;
			case EStatIsBasedOnXRoundingType.ToCeil:
				num = (int)Math.Ceiling(num2);
				break;
			}
			if (flag)
			{
				switch (statIsBasedOnXDataEntry.AbilityStatType)
				{
				case EAbilityStatType.Strength:
					Strength = (statIsBasedOnXDataEntry.AddTo ? Strength : 0) + num;
					break;
				case EAbilityStatType.Range:
					Range = (statIsBasedOnXDataEntry.AddTo ? Range : 0) + num;
					break;
				case EAbilityStatType.NumberOfTargets:
					NumberTargets = (statIsBasedOnXDataEntry.AddTo ? NumberTargets : 0) + num;
					break;
				}
				if (statIsBasedOnXDataEntry.AddTo)
				{
					statIsBasedOnXDataEntry.AddedStat = num;
				}
			}
		}
	}

	public void SetStatBasedOnX(CActor targetingActor, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXDataEntries, CAbilityFilterContainer abilityFilter = null, CActor targetActor = null)
	{
		if (statIsBasedOnXDataEntries == null)
		{
			return;
		}
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXDataEntry in statIsBasedOnXDataEntries)
		{
			if (statIsBasedOnXDataEntry.AddTo)
			{
				if (statIsBasedOnXDataEntry.HasBeenAdded)
				{
					continue;
				}
				statIsBasedOnXDataEntry.HasBeenAdded = true;
			}
			int statIsBasedOnXValue = GetStatIsBasedOnXValue(targetingActor, statIsBasedOnXDataEntry, abilityFilter, EStatIsBasedOnXType.None, 1f, 1f, targetActor);
			switch (statIsBasedOnXDataEntry.AbilityStatType)
			{
			case EAbilityStatType.RetaliateRange:
			{
				CAbilityRetaliate cAbilityRetaliate = this as CAbilityRetaliate;
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						cAbilityRetaliate.RetaliateRange += statIsBasedOnXValue;
						m_RetaliateRangeStatAdditions++;
					}
				}
				else
				{
					cAbilityRetaliate.RetaliateRange = statIsBasedOnXValue;
				}
				break;
			}
			case EAbilityStatType.Strength:
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						Strength += statIsBasedOnXValue;
						m_StrengthStatAdditions++;
					}
				}
				else
				{
					Strength = statIsBasedOnXValue;
				}
				break;
			case EAbilityStatType.Range:
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						Range += statIsBasedOnXValue;
						AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
						if (miscAbilityData != null && miscAbilityData.MinRange.HasValue)
						{
							MiscAbilityData.MinRange = MiscAbilityData.MinRange.Value + statIsBasedOnXValue;
						}
						m_RangeStatAdditions++;
					}
				}
				else
				{
					Range = statIsBasedOnXValue;
				}
				break;
			case EAbilityStatType.NumberOfTargets:
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						NumberTargets += statIsBasedOnXValue;
						m_NumberTargetsStatAdditions++;
					}
				}
				else
				{
					NumberTargets = statIsBasedOnXValue;
				}
				break;
			case EAbilityStatType.Tokens:
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						Tokens += statIsBasedOnXValue;
						m_TokenStatAdditions++;
					}
				}
				else
				{
					Tokens = statIsBasedOnXValue;
				}
				break;
			case EAbilityStatType.Pierce:
				if (!(this is CAbilityAttack cAbilityAttack))
				{
					break;
				}
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						cAbilityAttack.Pierce += statIsBasedOnXValue;
						m_NumberTargetsStatAdditions++;
					}
				}
				else
				{
					cAbilityAttack.Pierce = statIsBasedOnXValue;
				}
				break;
			case EAbilityStatType.XP:
				if (statIsBasedOnXDataEntry.AddTo)
				{
					if (statIsBasedOnXValue != 0)
					{
						AbilityXP += statIsBasedOnXValue;
						m_NumberTargetsStatAdditions++;
					}
				}
				else
				{
					AbilityXP = statIsBasedOnXValue;
				}
				break;
			}
			if (statIsBasedOnXDataEntry.AddTo)
			{
				statIsBasedOnXDataEntry.AddedStat = statIsBasedOnXValue;
			}
		}
	}

	public int GetStatIsBasedOnXValue(CActor targetingActor, AbilityData.StatIsBasedOnXData statIsBasedOnXData, CAbilityFilterContainer abilityFilter, EStatIsBasedOnXType basedOnOverride = EStatIsBasedOnXType.None, float xVariableOverride = 1f, float yVariableOverride = 1f, CActor targetActor = null)
	{
		int num = 0;
		float num2 = 0f;
		CAbilityFilterContainer filter = statIsBasedOnXData?.Filter ?? abilityFilter;
		EStatIsBasedOnXType eStatIsBasedOnXType = ((basedOnOverride == EStatIsBasedOnXType.None && statIsBasedOnXData != null) ? statIsBasedOnXData.BasedOn : basedOnOverride);
		float num3 = ((xVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.Multiplier : xVariableOverride);
		float num4 = ((yVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.SecondVariable : yVariableOverride);
		int level = ScenarioManager.CurrentScenarioState.Level;
		int val = 0;
		if (statIsBasedOnXData != null)
		{
			val = statIsBasedOnXData.MinLValue;
		}
		level = Math.Max(level, val);
		switch (eStatIsBasedOnXType)
		{
		case EStatIsBasedOnXType.X:
			num2 = num3;
			break;
		case EStatIsBasedOnXType.HexesMovedThisTurn:
			num2 = (float)GameState.HexesMovedThisTurn.Count * num3;
			break;
		case EStatIsBasedOnXType.HexesMovedThisAction:
			num2 = (float)CPhaseAction.HexesMovedThisAction * num3;
			break;
		case EStatIsBasedOnXType.HexesMovedByParentAbility:
			if (ParentAbility != null && ParentAbility is CAbilityMove cAbilityMove)
			{
				num2 = (float)cAbilityMove.MoveCount * num3;
			}
			break;
		case EStatIsBasedOnXType.DamageInflictedThisTurn:
			num2 = (float)GameState.DamageInflictedThisTurn * num3;
			break;
		case EStatIsBasedOnXType.DamageInflictedThisAction:
			num2 = (float)CPhaseAction.DamageInflictedThisAction * num3;
			break;
		case EStatIsBasedOnXType.DamageInflictedByPreviousAbility:
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.PreviousPhaseAbilities.Count > 0)
			{
				CPhaseAction.CPhaseAbility cPhaseAbility = cPhaseAction.PreviousPhaseAbilities.Last();
				if (cPhaseAbility?.m_Ability != null)
				{
					num2 = (float)cPhaseAbility.m_Ability.DamageInflictedByAbility * num3;
				}
			}
			break;
		case EStatIsBasedOnXType.DamageInflictedByParent:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.DamageInflictedByAbility * num3;
			}
			break;
		case EStatIsBasedOnXType.DamageActuallyTakenByParent:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.DamageActuallyTakenByAbility * num3;
			}
			break;
		case EStatIsBasedOnXType.ActorsKilledThisAction:
			num2 = (float)CPhaseAction.ActorsKilledThisAction.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ActorsKilledThisTurn:
			num2 = (float)GameState.ActorsKilledThisTurn.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ActorsKilledThisRound:
			num2 = (float)GameState.ActorsKilledThisRound.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ActorsKilledThisScenario:
			num2 = (float)ScenarioManager.Scenario.AllDeadActors.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ObstaclesDestroyedThisTurn:
			num2 = (float)GameState.ObstaclesDestroyedThisTurn * num3;
			break;
		case EStatIsBasedOnXType.ObstaclesDestroyedThisAction:
			num2 = (float)CPhaseAction.ObstaclesDestroyedThisAction * num3;
			break;
		case EStatIsBasedOnXType.ObstaclesDestroyedByParent:
			Log.ErrorWithReport("Attempted to apply non implemented StrengthCondition, ObstaclesDestroyedByParent");
			break;
		case EStatIsBasedOnXType.ObstaclesToBeDestroyedByMergedAbility:
		{
			if (ParentAbility == null || !(ParentAbility is CAbilityMerged cAbilityMerged))
			{
				break;
			}
			CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
			if (mergedWithAbility == null || !(mergedWithAbility is CAbilityDestroyObstacle cAbilityDestroyObstacle))
			{
				break;
			}
			int num10 = 0;
			foreach (CTile item in cAbilityDestroyObstacle.TilesSelected)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle cObjectObstacle)
				{
					num10 += cObjectObstacle.PathingBlockers.Count;
				}
			}
			num2 = (float)num10 * num3;
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentActors:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentActors.Count * num3;
			string text3 = "Setting Strength based on Target Adjacent Actors:";
			if (filter.LastCheckedTargetAdjacentActors.Count > 0)
			{
				foreach (CActor lastCheckedTargetAdjacentActor in filter.LastCheckedTargetAdjacentActors)
				{
					text3 = text3 + "\n" + lastCheckedTargetAdjacentActor.ActorLocKey() + ((lastCheckedTargetAdjacentActor is CEnemyActor { ID: var iD3 }) ? iD3.ToString() : "") + "at array index: " + lastCheckedTargetAdjacentActor.ArrayIndex.ToString();
				}
			}
			else
			{
				text3 += "\nNo Target Adjacent Actors";
			}
			Log.SimpleInfo(text3);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentEnemies:
		case EStatIsBasedOnXType.TargetAdjacentEnemiesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentEnemies.Count * num3;
			string text7 = "Setting Strength based on Target Adjacent Enemies:";
			if (filter.LastCheckedTargetAdjacentEnemies.Count > 0)
			{
				foreach (CActor lastCheckedTargetAdjacentEnemy in filter.LastCheckedTargetAdjacentEnemies)
				{
					text7 = text7 + "\n" + lastCheckedTargetAdjacentEnemy.ActorLocKey() + ((lastCheckedTargetAdjacentEnemy is CEnemyActor { ID: var iD7 }) ? iD7.ToString() : "") + "at array index: " + lastCheckedTargetAdjacentEnemy.ArrayIndex.ToString();
				}
			}
			else
			{
				text7 += "\nNo Target Adjacent Enemies of Caster";
			}
			Log.SimpleInfo(text7);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentAllies:
		case EStatIsBasedOnXType.TargetAdjacentAlliesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			List<CActor> list = new List<CActor>();
			if (filter.LastCheckedTargetAdjacentAllies.Count == 0 && targetActor != null && (Flags & EAbilityFlag.AttackModifierAbilityOverride) != 0)
			{
				foreach (CTile item2 in (IEnumerable<CTile>)GameState.GetTilesInRange(targetActor.ArrayIndex, 1))
				{
					foreach (CActor item3 in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex, TargetingActor))
					{
						if (CActor.AreActorsAllied(TargetingActor.Type, item3.Type, TargetingActor, item3))
						{
							list.Add(item3);
						}
					}
				}
			}
			else
			{
				list.AddRange(filter.LastCheckedTargetAdjacentAllies);
			}
			num2 = (float)list.Count * num3;
			string text4 = "Setting Strength based on Target Adjacent Allies:";
			if (list.Count > 0)
			{
				foreach (CActor item4 in list)
				{
					text4 = text4 + "\n" + item4.ActorLocKey() + ((item4 is CEnemyActor { ID: var iD4 }) ? iD4.ToString() : "") + "at array index: " + item4.ArrayIndex.ToString();
				}
			}
			else
			{
				text4 += "\nNo Target Adjacent Allies of Caster";
			}
			Log.SimpleInfo(text4);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentSummonsOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentAllies.Count((CActor act) => act is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == targetingActor) * num3;
			string text2 = "Setting Strength based on Target Adjacent Summons of Caster:";
			if (filter.LastCheckedTargetAdjacentAllies.Count > 0)
			{
				foreach (CActor lastCheckedTargetAdjacentAlly in filter.LastCheckedTargetAdjacentAllies)
				{
					text2 = text2 + "\n" + lastCheckedTargetAdjacentAlly.ActorLocKey() + ((lastCheckedTargetAdjacentAlly is CEnemyActor { ID: var iD2 }) ? iD2.ToString() : "") + "at array index: " + lastCheckedTargetAdjacentAlly.ArrayIndex.ToString();
				}
			}
			else
			{
				text2 += "\nNo Target Adjacent Summons of Caster";
			}
			Log.SimpleInfo(text2);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentBannerSummonsOfCaster:
		{
			num2 = 0f;
			List<string> bannerIDs = new List<string> { "BannerOfCourageID", "BannerOfDoomID", "BannerOfHopeID", "BannerOfStrengthID", "BannerOfValorID" };
			CPlayerActor player = targetingActor as CPlayerActor;
			if (player != null)
			{
				num2 = (float)(from s in player.GetAllHeroSummons()
					where bannerIDs.Contains(s.HeroSummonClass.ID)
					select s).Count((CHeroSummonActor x) => x.GetDistanceTo(player, viaPathfinding: true) <= 1) * num3;
			}
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentAlliesOfTarget:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentAlliesOfTarget.Count * num3;
			string text = "Setting Strength based on Target Adjacent Allies:";
			if (filter.LastCheckedTargetAdjacentAlliesOfTarget.Count > 0)
			{
				foreach (CActor item5 in filter.LastCheckedTargetAdjacentAlliesOfTarget)
				{
					text = text + "\n" + item5.ActorLocKey() + ((item5 is CEnemyActor { ID: var iD }) ? iD.ToString() : "") + "at array index: " + item5.ArrayIndex.ToString();
				}
			}
			else
			{
				text += "\nNo Target Adjacent Allies of Target";
			}
			Log.SimpleInfo(text);
			break;
		}
		case EStatIsBasedOnXType.CasterAdjacentEnemies:
		case EStatIsBasedOnXType.CasterAdjacentEnemiesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedCasterAdjacentEnemies.Count * num3;
			string text6 = "Setting Strength based on Caster Adjacent Enemies:";
			if (filter.LastCheckedCasterAdjacentEnemies.Count > 0)
			{
				foreach (CActor lastCheckedCasterAdjacentEnemy in filter.LastCheckedCasterAdjacentEnemies)
				{
					text6 = text6 + "\n" + lastCheckedCasterAdjacentEnemy.ActorLocKey() + ((lastCheckedCasterAdjacentEnemy is CEnemyActor { ID: var iD6 }) ? iD6.ToString() : "") + "at array index: " + lastCheckedCasterAdjacentEnemy.ArrayIndex.ToString();
				}
			}
			else
			{
				text6 += "\nNo Caster Adjacent Enemies of Caster";
			}
			Log.SimpleInfo(text6);
			break;
		}
		case EStatIsBasedOnXType.CasterAdjacentAllies:
		case EStatIsBasedOnXType.CasterAdjacentAlliesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedCasterAdjacentAllies.Count * num3;
			string text5 = "Setting Strength based on Caster Adjacent Allies:";
			if (filter.LastCheckedCasterAdjacentAllies.Count > 0)
			{
				foreach (CActor lastCheckedCasterAdjacentAlly in filter.LastCheckedCasterAdjacentAllies)
				{
					text5 = text5 + "\n" + lastCheckedCasterAdjacentAlly.ActorLocKey() + ((lastCheckedCasterAdjacentAlly is CEnemyActor { ID: var iD5 }) ? iD5.ToString() : "") + "at array index: " + lastCheckedCasterAdjacentAlly.ArrayIndex.ToString();
				}
			}
			else
			{
				text5 += "\nNo Caster Adjacent Allies of Caster";
			}
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentWalls:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedTargetAdjacentWalls * num3;
				Log.SimpleInfo("Setting Strength based on Target Adjacent Walls:");
			}
			break;
		case EStatIsBasedOnXType.CasterAdjacentWalls:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedCasterAdjacentWalls * num3;
				Log.SimpleInfo("Setting Strength based on Caster Adjacent Walls:");
			}
			break;
		case EStatIsBasedOnXType.TargetAdjacentValidTiles:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedTargetAdjacentTiles * num3;
				Log.SimpleInfo("Setting Strength based on Target Adjacent Valid Tiles:");
			}
			break;
		case EStatIsBasedOnXType.TargetInWater:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedTargetInWater * num3;
				Log.SimpleInfo("Setting Strength based on Target In Water:");
			}
			break;
		case EStatIsBasedOnXType.CasterAdjacentValidTiles:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedCasterAdjacentValidTiles * num3;
				Log.SimpleInfo("Setting Strength based on Caster Adjacent Valid Tiles:");
			}
			break;
		case EStatIsBasedOnXType.DamageInflictedByParentOnLastTarget:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.DamageInflictedByAbilityOnLastTarget * num3;
			}
			break;
		case EStatIsBasedOnXType.ExcessDamageInflictedByParentOnLastTargetKilled:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.ExcessDamageInflictedOnLastTargetKilled * num3;
			}
			break;
		case EStatIsBasedOnXType.ActorsTargetedByParentAbility:
			if (ParentAbility != null)
			{
				num2 = ((ParentAbility.AbilityFilter == null) ? ((float)ParentAbility.ActorsTargeted.Count((CActor x) => filter == null || filter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false)) * num3) : ((float)ParentAbility.ActorsTargeted.Count((CActor x) => ParentAbility.AbilityFilter == null || ParentAbility.AbilityFilter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false)) * num3));
			}
			break;
		case EStatIsBasedOnXType.ActorsKilledByParentAbility:
			if (ParentAbility != null)
			{
				num2 = ((ParentAbility.AbilityFilter == null) ? ((float)ParentAbility.ActorsTargeted.Count((CActor x) => x.IsDead && (filter == null || filter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false))) * num3) : ((float)ParentAbility.ActorsTargeted.Count((CActor x) => x.IsDead && (ParentAbility.AbilityFilter == null || ParentAbility.AbilityFilter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false))) * num3));
			}
			break;
		case EStatIsBasedOnXType.LostCardCount:
			if (targetingActor is CPlayerActor cPlayerActor2)
			{
				num2 = (float)(cPlayerActor2.CharacterClass.LostAbilityCards.Count + cPlayerActor2.CharacterClass.PermanentlyLostAbilityCards.Count + cPlayerActor2.CharacterClass.ActivatedAbilityCards.Count((CAbilityCard x) => x != null && x.SelectedAction?.CardPile == CBaseCard.ECardPile.Lost)) * num3;
			}
			break;
		case EStatIsBasedOnXType.PercentageOfCurrentHP:
			num2 = (float)targetingActor.Health * num3;
			break;
		case EStatIsBasedOnXType.HPDifference:
			num2 = (float)Math.Abs(targetingActor.OriginalMaxHealth - targetingActor.Health) * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousAttackThisTurn:
			num2 = (float)GameState.TargetsDamagedInPrevAttackThisTurn * num3;
			break;
		case EStatIsBasedOnXType.TargetsActuallyDamagedByPreviousAttackThisTurn:
			num2 = (float)GameState.TargetsActuallyDamagedInPrevAttackThisTurn * num3;
			break;
		case EStatIsBasedOnXType.ActorsHealedThisAction:
			num2 = CPhaseAction.ActorsHealedThisAction.Count;
			break;
		case EStatIsBasedOnXType.ScenarioTrapDamage:
			num2 = ScenarioManager.TrapDamage;
			break;
		case EStatIsBasedOnXType.ScenarioHazardousTerrainDamage:
			num2 = ScenarioManager.HazardousTerrainDamage;
			break;
		case EStatIsBasedOnXType.BaseStat:
		{
			CMonsterClass cMonsterClass2 = (CMonsterClass)targetingActor.Class;
			switch (statIsBasedOnXData.BaseStatType)
			{
			case EMonsterBaseStats.Health:
				num2 = cMonsterClass2.CurrentMonsterStat.Health;
				break;
			case EMonsterBaseStats.Attack:
				num2 = cMonsterClass2.CurrentMonsterStat.Attack;
				break;
			case EMonsterBaseStats.Move:
				num2 = cMonsterClass2.CurrentMonsterStat.Move;
				break;
			case EMonsterBaseStats.Range:
				num2 = cMonsterClass2.CurrentMonsterStat.Range;
				break;
			case EMonsterBaseStats.Target:
				num2 = cMonsterClass2.CurrentMonsterStat.Target;
				break;
			case EMonsterBaseStats.Shield:
				num2 = cMonsterClass2.CurrentMonsterStat.Shield;
				break;
			case EMonsterBaseStats.Retaliate:
				num2 = cMonsterClass2.CurrentMonsterStat.Retaliate;
				break;
			case EMonsterBaseStats.RetaliateRange:
				num2 = cMonsterClass2.CurrentMonsterStat.RetaliateRange;
				break;
			case EMonsterBaseStats.Pierce:
				num2 = cMonsterClass2.CurrentMonsterStat.Pierce;
				break;
			case EMonsterBaseStats.Push:
				num2 = cMonsterClass2.CurrentMonsterStat.Push;
				break;
			}
			break;
		}
		case EStatIsBasedOnXType.BaseStatPlusX:
		{
			CMonsterClass cMonsterClass = (CMonsterClass)targetingActor.Class;
			switch (statIsBasedOnXData.BaseStatType)
			{
			case EMonsterBaseStats.Health:
				num2 = cMonsterClass.CurrentMonsterStat.Health;
				break;
			case EMonsterBaseStats.Attack:
				num2 = cMonsterClass.CurrentMonsterStat.Attack;
				break;
			case EMonsterBaseStats.Move:
				num2 = cMonsterClass.CurrentMonsterStat.Move;
				break;
			case EMonsterBaseStats.Range:
				num2 = cMonsterClass.CurrentMonsterStat.Range;
				break;
			case EMonsterBaseStats.Target:
				num2 = cMonsterClass.CurrentMonsterStat.Target;
				break;
			case EMonsterBaseStats.Shield:
				num2 = cMonsterClass.CurrentMonsterStat.Shield;
				break;
			case EMonsterBaseStats.Retaliate:
				num2 = cMonsterClass.CurrentMonsterStat.Retaliate;
				break;
			case EMonsterBaseStats.RetaliateRange:
				num2 = cMonsterClass.CurrentMonsterStat.RetaliateRange;
				break;
			case EMonsterBaseStats.Pierce:
				num2 = cMonsterClass.CurrentMonsterStat.Pierce;
				break;
			case EMonsterBaseStats.Push:
				num2 = cMonsterClass.CurrentMonsterStat.Push;
				break;
			}
			num2 += num3;
			break;
		}
		case EStatIsBasedOnXType.TargetsDamagedByPreviousAttackThisAction:
			num2 = (float)CPhaseAction.TargetsDamagedInPrevAttackThisAction * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousDamageAbilityThisTurn:
			num2 = (float)GameState.TargetsDamagedInPrevDamageAbilityThisTurn * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousDamageAbilityThisAction:
			num2 = (float)CPhaseAction.TargetsDamagedInPrevDamageAbilityThisAction * num3;
			break;
		case EStatIsBasedOnXType.HalfTargetsDamagedByPreviousDamageAbilityThisAction:
			num2 = (float)(CPhaseAction.TargetsDamagedInPrevDamageAbilityThisAction / 2) * num3;
			break;
		case EStatIsBasedOnXType.ActorCount:
		{
			int num15 = 0;
			foreach (ActorState actorState4 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState4.IsRevealed)
				{
					CActor cActor3 = null;
					cActor3 = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState4.ActorGuid);
					if (cActor3 == null)
					{
						cActor3 = ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState4.ActorGuid);
					}
					if (cActor3 == null)
					{
						cActor3 = ScenarioManager.Scenario.AllAliveMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState4.ActorGuid);
					}
					if (cActor3 == null)
					{
						cActor3 = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState4.ActorGuid);
					}
					if (cActor3 != null && filter != null && filter.IsValidTarget(cActor3, targetingActor, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num15++;
					}
				}
				else if (filter != null && statIsBasedOnXData.IncludeUnrevealed && (!(actorState4 is EnemyState enemyState3) || enemyState3.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden))
				{
					ActorState actorState5 = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == targetingActor.ActorGuid);
					if (actorState5 != null && filter.IsValidTarget_ActorState(actorState4, actorState5, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num15++;
					}
				}
			}
			if (statIsBasedOnXData.IncludeUnrevealed)
			{
				foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && !prop.PropActorHasBeenAssigned && prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						CTile propTile2 = ScenarioManager.Tiles[prop.ArrayIndex.X, prop.ArrayIndex.Y];
						ObjectState objectState2 = prop.PropHealthDetails.CreateStateForPropWithHealth(propTile2, prop.PropHealthDetails.GetPropStartingHealth());
						if (filter.IsValidTarget_ActorState(objectState2, objectState2, isTargetedAbility: false, useTargetOriginalType: false, true))
						{
							num15++;
						}
					}
				}
			}
			num2 = (float)num15 * num3;
			break;
		}
		case EStatIsBasedOnXType.DeadActorCount:
		{
			int num14 = 0;
			foreach (ActorState actorState3 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState3 is EnemyState enemyState2 && enemyState2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				CActor cActor2 = null;
				cActor2 = ScenarioManager.Scenario.ExhaustedPlayers.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState3.ActorGuid);
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadHeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadEnemies.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadEnemy2Monsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadAllyMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadNeutralMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 != null && filter != null && filter.IsValidTarget(cActor2, cActor2, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					num14++;
				}
			}
			num2 = (float)num14 * num3;
			break;
		}
		case EStatIsBasedOnXType.ScenarioLevel:
			num2 = (float)Math.Max(level, 1) * num3;
			break;
		case EStatIsBasedOnXType.InitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) * num3;
			break;
		case EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) + num3;
			break;
		case EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
			num2 = num3 + (float)(ScenarioManager.CurrentScenarioState.Players.Count * level);
			break;
		case EStatIsBasedOnXType.CharactersTimesLevelPlusX:
			num2 = (num3 + (float)level) * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
			num2 = num4 + (num3 + (float)level) * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.XAddedToYTimesLevel:
			num2 = num3 + num4 * (float)level;
			break;
		case EStatIsBasedOnXType.XPlusCharactersPlusLevel:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level;
			break;
		case EStatIsBasedOnXType.XPlusLevel:
			num2 = num3 + (float)level;
			break;
		case EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
			num2 = (float)level + num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
		{
			int num13 = level;
			int count4 = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = num3 + (float)(int)Math.Ceiling((float)(num13 * count4) / num4);
			break;
		}
		case EStatIsBasedOnXType.XAddedToLevelTimesActorsOverY:
		{
			int num11 = level;
			int num12 = 0;
			num12 = ((filter != null) ? ScenarioManager.Scenario.AllAliveActors.Count((CActor x) => filter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false)) : ScenarioManager.Scenario.AllAliveActors.Count);
			num2 = num3 + (float)(int)Math.Floor((float)(num11 * num12) / num4);
			break;
		}
		case EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level - num4;
			break;
		case EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
			num2 = (num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level - num4) * (float)ScenarioManager.CurrentScenarioState.RoundNumber;
			break;
		case EStatIsBasedOnXType.CharactersPlusLevelOverX:
			num2 = (float)level / num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.CharactersPlusLevelTimesX:
			num2 = (float)(level + ScenarioManager.CurrentScenarioState.Players.Count) * num3;
			break;
		case EStatIsBasedOnXType.CurrentHPOverXMaxY:
			num2 = (float)Math.Clamp(Math.Floor((float)targetingActor.Health / num3), 0.0, num4);
			break;
		case EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)level * num4;
			break;
		case EStatIsBasedOnXType.CasterShieldValue:
			num2 = targetingActor.CalculateShield(null, ignoreNeutralizeShield: false, includeItemShieldValues: true);
			break;
		case EStatIsBasedOnXType.TargetShieldValue:
		{
			CAbilityAttack cAbilityAttack2 = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityAttack;
			if (cAbilityAttack2?.AttackSummary != null)
			{
				CAttackSummary.TargetSummary targetSummary = cAbilityAttack2.AttackSummary.FindTarget(targetActor);
				if (targetSummary != null)
				{
					return targetSummary.Shield;
				}
			}
			num2 = ((targetActor != null) ? ((float)targetActor.CalculateShield(null, ignoreNeutralizeShield: false, includeItemShieldValues: true)) : 0f);
			break;
		}
		case EStatIsBasedOnXType.LastAbilityPerformedTargetShieldValue:
			if (ParentAbility is CAbilityAttack { CurrentAttackingTargetSummary: not null } cAbilityAttack)
			{
				num2 = num3 * (float)cAbilityAttack.CurrentAttackingTargetSummary.Shield;
			}
			else
			{
				num2 = ((targetingActor?.LastAbilityPerformed == null || targetingActor.LastAbilityPerformed.ActorsTargeted.Count != 1) ? 0f : (num3 * (float)targetingActor.LastAbilityPerformed.ActorsTargeted[0].CalculateShield(null, ignoreNeutralizeShield: false, includeItemShieldValues: true)));
			}
			break;
		case EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count - num4;
			break;
		case EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
			num2 = (num3 + (float)level) * ((float)ScenarioManager.CurrentScenarioState.Players.Count / num4);
			break;
		case EStatIsBasedOnXType.LevelTimesXPlusY:
			num2 = (float)level * num3 + num4;
			break;
		case EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
			num2 = ((float)ScenarioManager.CurrentScenarioState.Players.Count + num3) / 2f * ((float)level + num4);
			break;
		case EStatIsBasedOnXType.LevelTimesXTimesHexesMovedThisTurnInDefinedRoom:
		{
			int num9 = 0;
			if (statIsBasedOnXData.CheckGUIDs != null && statIsBasedOnXData.CheckGUIDs.Count > 0)
			{
				foreach (TileIndex item6 in GameState.HexesMovedThisTurn)
				{
					CTile cTile = ScenarioManager.Tiles[item6.X, item6.Y];
					if (statIsBasedOnXData.CheckGUIDs.Contains(cTile.m_HexMap.MapGuid) || (cTile.m_Hex2Map != null && statIsBasedOnXData.CheckGUIDs.Contains(cTile.m_Hex2Map.MapGuid)))
					{
						num9++;
					}
				}
			}
			else
			{
				num9 = GameState.HexesMovedThisTurn.Count;
			}
			num2 = (float)Math.Floor((float)level * num3) * (float)num9;
			break;
		}
		case EStatIsBasedOnXType.XTimesActorCountMinusY:
		{
			int num8 = 0;
			foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState.IsRevealed)
				{
					CActor cActor = null;
					cActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState.ActorGuid);
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState.ActorGuid);
					}
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.AllAliveMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
					}
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState.ActorGuid);
					}
					if (cActor != null && filter != null && filter.IsValidTarget(cActor, targetingActor, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num8++;
					}
				}
				else if (filter != null && statIsBasedOnXData.IncludeUnrevealed && (!(actorState is EnemyState enemyState) || enemyState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden))
				{
					ActorState actorState2 = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == targetingActor.ActorGuid);
					if (actorState2 != null && filter.IsValidTarget_ActorState(actorState, actorState2, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num8++;
					}
				}
			}
			if (statIsBasedOnXData.IncludeUnrevealed)
			{
				foreach (CObjectProp prop2 in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop2.PropHealthDetails != null && prop2.PropHealthDetails.HasHealth && !prop2.PropActorHasBeenAssigned && prop2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						CTile propTile = ScenarioManager.Tiles[prop2.ArrayIndex.X, prop2.ArrayIndex.Y];
						ObjectState objectState = prop2.PropHealthDetails.CreateStateForPropWithHealth(propTile, prop2.PropHealthDetails.GetPropStartingHealth());
						if (filter.IsValidTarget_ActorState(objectState, objectState, isTargetedAbility: false, useTargetOriginalType: false, true))
						{
							num8++;
						}
					}
				}
			}
			num2 = (float)num8 * num3 - num4;
			break;
		}
		case EStatIsBasedOnXType.InitialPlayerCharacterCountMinusYTimesX:
			num2 = num3 * ((float)ScenarioManager.CurrentScenarioState.Players.Count - num4);
			break;
		case EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
			num2 = (num3 + (float)level) * num4 * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.CharactersTimesXPlusLevelTimesY:
			num2 = ((float)level + (float)ScenarioManager.CurrentScenarioState.Players.Count * num3) * num4;
			break;
		case EStatIsBasedOnXType.LevelPlusYTimesX:
			num2 = ((float)level + num4) * num3;
			break;
		case EStatIsBasedOnXType.TimeTokens:
			if (TargetingActor is CPlayerActor cPlayerActor)
			{
				num2 = cPlayerActor.GetTimeTokens();
			}
			break;
		case EStatIsBasedOnXType.SingleUsePreventDamageCount:
			num2 = ((targetingActor != null) ? targetingActor.GetSingleUsePreventDamageCount() : 0);
			break;
		case EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		{
			int num7 = level;
			int count3 = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = (num3 + num4 * (float)count3) * (float)(2 + num7);
			break;
		}
		case EStatIsBasedOnXType.FH22ShipFormulaCharactersLevel:
		{
			int num6 = level;
			int count2 = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = 2 * (2 + num6) * (3 * count2 - 1);
			break;
		}
		case EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		{
			int num5 = level;
			int count = ScenarioManager.CurrentScenarioState.Players.Count;
			num2 = ((float)num5 * num3 + num4) * (float)count;
			break;
		}
		case EStatIsBasedOnXType.Vars:
			num2 = (float)CalcStatIsBasedOnXValueFromVars(statIsBasedOnXData, targetingActor, targetActor) * num3;
			break;
		}
		switch (statIsBasedOnXData?.RoundingType ?? EStatIsBasedOnXRoundingType.RoundOff)
		{
		case EStatIsBasedOnXRoundingType.RoundOff:
			num = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
			break;
		case EStatIsBasedOnXRoundingType.ToFloor:
			num = (int)Math.Floor(num2);
			break;
		case EStatIsBasedOnXRoundingType.ToCeil:
			num = (int)Math.Ceiling(num2);
			break;
		}
		if (statIsBasedOnXData != null)
		{
			num = Math.Max(statIsBasedOnXData.MinValue, num);
			num = Math.Min(statIsBasedOnXData.MaxValue, num);
		}
		return num;
	}

	public int CalcStatIsBasedOnXValueFromVars(AbilityData.StatIsBasedOnXData statIsBasedOnXData, CActor targetingActor, CActor targetActor)
	{
		int result = 0;
		if (statIsBasedOnXData?.Vars == null)
		{
			return result;
		}
		CVars cVars = new CVars();
		CVarsContext context = new CVarsContext(cVars, VarsContext);
		cVars.Set("TargetingActor", new TValue(targetingActor));
		cVars.Set("TargetActor", new TValue(targetActor));
		foreach (var var in statIsBasedOnXData.Vars)
		{
			string item = var.Item1;
			TValue item2 = var.Item2;
			TValue value = item2.Calc(context);
			if (item.StartsWith("Validate") && !value.GetBool())
			{
				break;
			}
			if (item == "Value")
			{
				result = value.GetInt();
			}
			cVars.Set(item, value);
		}
		return result;
	}

	public void ResetStatBasedOnXAddedStats(List<AbilityData.StatIsBasedOnXData> statIsBasedOnXDataEntries)
	{
		if (statIsBasedOnXDataEntries == null)
		{
			return;
		}
		m_StrengthStatAdditions = 0;
		m_RangeStatAdditions = 0;
		m_RetaliateRangeStatAdditions = 0;
		m_NumberTargetsStatAdditions = 0;
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXDataEntry in statIsBasedOnXDataEntries)
		{
			if (statIsBasedOnXDataEntry.AddTo && statIsBasedOnXDataEntry.HasBeenAdded)
			{
				switch (statIsBasedOnXDataEntry.AbilityStatType)
				{
				case EAbilityStatType.Strength:
					Strength -= statIsBasedOnXDataEntry.AddedStat;
					break;
				case EAbilityStatType.RetaliateRange:
					(this as CAbilityRetaliate).RetaliateRange -= statIsBasedOnXDataEntry.AddedStat;
					break;
				case EAbilityStatType.Range:
					Range -= statIsBasedOnXDataEntry.AddedStat;
					break;
				case EAbilityStatType.NumberOfTargets:
					NumberTargets -= statIsBasedOnXDataEntry.AddedStat;
					break;
				}
			}
			statIsBasedOnXDataEntry.AddedStat = 0;
			statIsBasedOnXDataEntry.HasBeenAdded = false;
		}
	}

	public bool MeetsStartAbilityRequirements(CActor targetingActor)
	{
		if (StartAbilityRequirements != null)
		{
			if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.ThisAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, this))
				{
					return false;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, ParentAbility))
				{
					return false;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
			{
				CPhaseAction.CPhaseAbility cPhaseAbility = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last();
				if (!StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, cPhaseAbility.m_Ability))
				{
					return false;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.ParentAbility) && ParentAbility != null && !StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, ParentAbility))
			{
				return false;
			}
		}
		return true;
	}

	public virtual void Reset()
	{
		foreach (CEnhancement abilityEnhancement in AbilityEnhancements)
		{
			abilityEnhancement.Enhancement = EEnhancement.NoEnhancement;
		}
		if (SubAbilities != null)
		{
			foreach (CAbility subAbility in SubAbilities)
			{
				subAbility.Reset();
			}
		}
		if (this is CAbilityChoose { ChooseAbilities: not null } cAbilityChoose)
		{
			foreach (CAbility chooseAbility in cAbilityChoose.ChooseAbilities)
			{
				chooseAbility.Reset();
			}
		}
		if (this is CAbilityMerged { MergedAbilities: not null } cAbilityMerged)
		{
			foreach (CAbility mergedAbility in cAbilityMerged.MergedAbilities)
			{
				mergedAbility.Reset();
			}
		}
		if (!(this is CAbilityControlActor cAbilityControlActor) || cAbilityControlActor.ControlActorData.ControlAbilities == null)
		{
			return;
		}
		foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
		{
			controlAbility.Reset();
		}
	}

	public virtual void Restart()
	{
		m_HasEnded = false;
		SetCancelAbilityFlag(cancel: false);
		if (StartAbilityRequirements != null)
		{
			if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.ThisAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, this))
				{
					SetCancelAbilityFlag(cancel: true);
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, ParentAbility))
				{
					SetCancelAbilityFlag(cancel: true);
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
			{
				CPhaseAction.CPhaseAbility cPhaseAbility = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last();
				if ((!cPhaseAbility.m_Ability.AbilityHasHappened && StartAbilityRequirements.AbilityHasHappened != false) || !StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, cPhaseAbility.m_Ability))
				{
					SetCancelAbilityFlag(cancel: true);
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.ParentAbility) && ParentAbility != null && !StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, ParentAbility))
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		CAbilityRestarted_MessageData message = new CAbilityRestarted_MessageData(TargetingActor);
		ScenarioRuleClient.MessageHandler(message);
		CAbilityFilter.CurrentVarsContext = VarsContext;
		AbilityFilter.IsValidTarget(TargetingActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible);
		ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
		SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
		CAbilityFilter.CurrentVarsContext = null;
	}

	public virtual void InterruptAbility()
	{
	}

	public virtual bool ApplySingleTargetItem(CActor target)
	{
		return true;
	}

	public virtual bool IsValidSingleTargetActiveBonusTarget(CActor target)
	{
		return true;
	}

	public virtual void ActiveBonusToggled(CActor actor, CActiveBonus activeBonus)
	{
	}

	public virtual void ApplySingleTargetActiveBonus(CActor target)
	{
	}

	public virtual void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		if (DontOverride)
		{
			if (TargetingActor != null && m_AbilityStartComplete)
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					_ = (CPhaseAction)PhaseManager.Phase;
				}
				if (perform)
				{
					Perform();
				}
			}
			return;
		}
		try
		{
			bool flag = false;
			CAbilityOverride cAbilityOverride = FindOverride(abilityOverride);
			if (cAbilityOverride == null)
			{
				return;
			}
			m_CurrentOverrides.Remove(cAbilityOverride);
			abilityOverride = cAbilityOverride;
			CAbilityOverride cAbilityOverride2 = m_CurrentItemOverrides.Keys.FirstOrDefault((CAbilityOverride x) => x.ID == abilityOverride.ID);
			if (item != null && cAbilityOverride2 != null)
			{
				m_CurrentItemOverrides.Remove(cAbilityOverride2);
			}
			ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
			if (abilityOverride.Flags.HasValue)
			{
				Flags = abilityOverride.OriginalAbility.Flags;
			}
			if (abilityOverride.Tokens.HasValue)
			{
				Tokens -= abilityOverride.Tokens.Value;
			}
			abilityOverride.UndoOverrideStrength(this);
			if (abilityOverride.Range.HasValue)
			{
				if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
				{
					Range = abilityOverride.OriginalAbility.Range;
					m_OriginalRange = abilityOverride.OriginalAbility.Range;
				}
				else
				{
					Range -= abilityOverride.Range.Value;
					m_OriginalRange -= abilityOverride.Range.Value;
				}
			}
			if (this is CAbilityRetaliate cAbilityRetaliate && abilityOverride.RetaliateRange.HasValue)
			{
				cAbilityRetaliate.RetaliateRange -= abilityOverride.RetaliateRange.Value;
			}
			if (abilityOverride.RangeAtLeastOne.HasValue && abilityOverride.OriginalAbility.Range <= 0)
			{
				Range = abilityOverride.OriginalAbility.Range;
				m_OriginalRange = abilityOverride.OriginalAbility.Range;
			}
			if (abilityOverride.NumberOfTargets.HasValue)
			{
				abilityOverride.UndoOverrideNumberTargets(this);
			}
			if (abilityOverride.TargetAtLeastOne.HasValue && abilityOverride.OriginalAbility.NumberTargets <= 0)
			{
				NumberTargets = abilityOverride.OriginalAbility.NumberTargets;
			}
			if (abilityOverride.RemoveConditionsOverride.HasValue)
			{
				m_PositiveConditions = abilityOverride.OriginalAbility.PositiveConditions;
				m_NegativeConditions = abilityOverride.OriginalAbility.NegativeConditions;
			}
			if (abilityOverride.Pierce.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).Pierce -= abilityOverride.Pierce.Value;
			}
			if (abilityOverride.SubAbilities != null)
			{
				List<CAbility> list = abilityOverride.SubAbilities.ToList();
				RemoveMergedSubabilitiesOfType(EAbilityType.Pull, list);
				RemoveMergedSubabilitiesOfType(EAbilityType.Push, list);
				foreach (CAbility subAbility in list)
				{
					SubAbilities.RemoveAll((CAbility x) => x.ID == subAbility.ID);
				}
			}
			if (abilityOverride.ChooseAbilities != null && this is CAbilityChoose cAbilityChoose)
			{
				foreach (CAbility chooseAbility in abilityOverride.ChooseAbilities)
				{
					if (cAbilityChoose.ChooseAbilities.Contains(chooseAbility))
					{
						cAbilityChoose.ChooseAbilities.Remove(chooseAbility);
					}
				}
			}
			if (abilityOverride.ElementsToInfuse != null && this is CAbilityInfuse)
			{
				CAbilityInfuse cAbilityInfuse = this as CAbilityInfuse;
				foreach (ElementInfusionBoardManager.EElement item2 in abilityOverride.ElementsToInfuse)
				{
					if (cAbilityInfuse.ElementsToInfuse.Contains(item2))
					{
						cAbilityInfuse.ElementsToInfuse.Remove(item2);
					}
				}
			}
			if (abilityOverride.AbilityType.HasValue)
			{
				flag = AbilityType != abilityOverride.OriginalAbility.AbilityType;
				AbilityType = abilityOverride.OriginalAbility.AbilityType;
			}
			if (abilityOverride.AbilityFilter != null)
			{
				AbilityFilter = abilityOverride.OriginalAbility.AbilityFilter;
			}
			if (abilityOverride.AnimationOverload != null)
			{
				AnimOverload = abilityOverride.OriginalAbility.AnimOverload;
			}
			if (abilityOverride.AreaEffect != null)
			{
				AreaEffect = abilityOverride.OriginalAbility.AreaEffect;
			}
			if (abilityOverride.AttackSourcesOnly.HasValue && this is CAbilityPreventDamage)
			{
				(this as CAbilityPreventDamage).AttackSourcesOnly = (abilityOverride.OriginalAbility as CAbilityPreventDamage).AttackSourcesOnly;
			}
			if (abilityOverride.Jump.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Jump = (abilityOverride.OriginalAbility as CAbilityMove).Jump;
			}
			if (abilityOverride.Fly.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Fly = (abilityOverride.OriginalAbility as CAbilityMove).Fly;
			}
			if (abilityOverride.IgnoreDifficultTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreDifficultTerrain = (abilityOverride.OriginalAbility as CAbilityMove).IgnoreDifficultTerrain;
			}
			if (abilityOverride.IgnoreHazardousTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreHazardousTerrain = (abilityOverride.OriginalAbility as CAbilityMove).IgnoreHazardousTerrain;
			}
			if (abilityOverride.CarryOtherActorsOnHex.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).CarryOtherActorsOnHex = (abilityOverride.OriginalAbility as CAbilityMove).CarryOtherActorsOnHex;
				(this as CAbilityMove).CarryOtherActorsOnHexUpdated();
			}
			if (abilityOverride.MoveRestrictionType.HasValue && this is CAbilityMove cAbilityMove)
			{
				cAbilityMove.MoveRestrictionType = (abilityOverride.OriginalAbility as CAbilityMove).MoveRestrictionType;
			}
			if (abilityOverride.ActiveBonusData != null)
			{
				ActiveBonusData = abilityOverride.OriginalAbility.ActiveBonusData;
			}
			if (abilityOverride.ActiveBonusYML != null)
			{
				ActiveBonusYML = abilityOverride.OriginalAbility.ActiveBonusYML;
			}
			if (abilityOverride.AreaEffectYMLString != null)
			{
				AreaEffectYMLString = abilityOverride.OriginalAbility.AreaEffectYMLString;
			}
			if (abilityOverride.AreaEffectLayoutOverrideYMLString != null)
			{
				AreaEffectLayoutOverrideYMLString = abilityOverride.OriginalAbility.AreaEffectLayoutOverrideYMLString;
			}
			if (abilityOverride.AreaEffectFlipAxis != null)
			{
				AreaEffectFlipAxis = abilityOverride.OriginalAbility.AreaEffectFlipAxis;
			}
			if (abilityOverride.MultiPassAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).MultiPassAttack = (abilityOverride.OriginalAbility as CAbilityAttack).MultiPassAttack;
			}
			if (abilityOverride.ExploitVulnerability.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).ExploitVulnerability = (abilityOverride.OriginalAbility as CAbilityAttack).ExploitVulnerability;
			}
			if (abilityOverride.DamageSelfBeforeAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).DamageSelfBeforeAttack = (abilityOverride.OriginalAbility as CAbilityAttack).DamageSelfBeforeAttack;
			}
			if (abilityOverride.AddAttackBaseStat.HasValue)
			{
				AddAttackBaseStat = abilityOverride.OriginalAbility.AddAttackBaseStat;
			}
			if (abilityOverride.StrengthIsBase.HasValue)
			{
				StrengthIsBase = abilityOverride.OriginalAbility.StrengthIsBase;
			}
			if (abilityOverride.RangeIsBase.HasValue)
			{
				RangeIsBase = abilityOverride.OriginalAbility.RangeIsBase;
			}
			if (abilityOverride.TargetIsBase.HasValue)
			{
				TargetIsBase = abilityOverride.OriginalAbility.TargetIsBase;
			}
			if (abilityOverride.AbilityText != null)
			{
				AbilityText = abilityOverride.OriginalAbility.AbilityText;
			}
			if (abilityOverride.AbilityTextOnly.HasValue)
			{
				AbilityTextOnly = abilityOverride.OriginalAbility.AbilityTextOnly;
			}
			if (abilityOverride.ShowRange.HasValue)
			{
				ShowRange = abilityOverride.OriginalAbility.ShowRange;
			}
			if (abilityOverride.ShowTarget.HasValue)
			{
				ShowTarget = abilityOverride.OriginalAbility.ShowTarget;
			}
			if (abilityOverride.ShowArea.HasValue)
			{
				ShowArea = abilityOverride.OriginalAbility.ShowArea;
			}
			if (abilityOverride.AllTargetsOnMovePath.HasValue)
			{
				AllTargetsOnMovePath = abilityOverride.OriginalAbility.AllTargetsOnMovePath;
			}
			if (abilityOverride.AllTargetsOnMovePathSameStartAndEnd.HasValue)
			{
				AllTargetsOnMovePathSameStartAndEnd = abilityOverride.OriginalAbility.AllTargetsOnMovePathSameStartAndEnd;
			}
			if (abilityOverride.AllTargetsOnAttackPath.HasValue)
			{
				AllTargetsOnAttackPath = abilityOverride.OriginalAbility.AllTargetsOnAttackPath;
			}
			if (abilityOverride.Summons != null && this is CAbilitySummon)
			{
				(this as CAbilitySummon).SummonIDs = (abilityOverride.OriginalAbility as CAbilitySummon).SummonIDs;
			}
			if (abilityOverride.XpPerTargetData != null)
			{
				m_XpPerTargetData = abilityOverride.OriginalAbility.XpPerTargetData;
			}
			if (abilityOverride.Targeting.HasValue)
			{
				Targeting = abilityOverride.OriginalAbility.Targeting;
			}
			if (abilityOverride.MiscVars != null)
			{
				MiscVars = abilityOverride.OriginalAbility.MiscVars;
			}
			if (abilityOverride.PropName != null && this is CAbilityCreate)
			{
				(this as CAbilityCreate).PropName = (abilityOverride.OriginalAbility as CAbilityCreate).PropName;
			}
			if (abilityOverride.TrapData != null && this is CAbilityTrap)
			{
				(this as CAbilityTrap).TrapData = (abilityOverride.OriginalAbility as CAbilityTrap).TrapData;
			}
			if (abilityOverride.IsSubAbility.HasValue)
			{
				m_IsSubAbility = abilityOverride.OriginalAbility.IsSubAbility;
			}
			if (abilityOverride.IsTargetedAbility.HasValue)
			{
				IsTargetedAbility = abilityOverride.OriginalAbility.IsTargetedAbility;
			}
			if (abilityOverride.SpawnDelay.HasValue)
			{
				SpawnDelay = abilityOverride.OriginalAbility.SpawnDelay;
			}
			if (abilityOverride.PullType.HasValue && this is CAbilityPull)
			{
				(this as CAbilityPull).PullType = (abilityOverride.OriginalAbility as CAbilityPull).PullType;
			}
			if (abilityOverride.AdditionalPushEffect.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffect = (abilityOverride.OriginalAbility as CAbilityPush).AdditionalPushEffect;
			}
			if (abilityOverride.AdditionalPushEffectDamage.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectDamage = (abilityOverride.OriginalAbility as CAbilityPush).AdditionalPushEffectDamage;
			}
			if (abilityOverride.AdditionalPushEffectXP.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectXP = (abilityOverride.OriginalAbility as CAbilityPush).AdditionalPushEffectXP;
			}
			if (abilityOverride.AbilityXP.HasValue)
			{
				AbilityXP = abilityOverride.OriginalAbility.AbilityXP;
			}
			if (abilityOverride.IsInlineSubAbility.HasValue)
			{
				IsInlineSubAbility = abilityOverride.OriginalAbility.IsInlineSubAbility;
			}
			if (abilityOverride.SharePreviousAnim.HasValue)
			{
				SkipAnim = abilityOverride.OriginalAbility.SkipAnim;
			}
			if (abilityOverride.IsConsumeAbility.HasValue)
			{
				IsConsumeAbility = abilityOverride.OriginalAbility.IsConsumeAbility;
			}
			if (abilityOverride.Augment != null)
			{
				Augment = abilityOverride.OriginalAbility.Augment;
			}
			if (abilityOverride.Song != null)
			{
				Song = abilityOverride.OriginalAbility.Song;
			}
			if (abilityOverride.Doom != null && this is CAbilityAddDoom cAbilityAddDoom)
			{
				cAbilityAddDoom.Doom = (abilityOverride.OriginalAbility as CAbilityAddDoom).Doom;
			}
			if (abilityOverride.AttackEffects != null && abilityOverride.AttackEffects.Count > 0 && this is CAbilityAttack cAbilityAttack)
			{
				foreach (CAttackEffect attackEffect in abilityOverride.AttackEffects)
				{
					cAbilityAttack.AttackEffects.Remove(attackEffect);
				}
			}
			if (abilityOverride.AddActiveBonusAbility != null && this is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				cAbilityAddActiveBonus.AddAbility = (abilityOverride.OriginalAbility as CAbilityAddActiveBonus).AddAbility;
			}
			if (abilityOverride.ShowElementPicker.HasValue && this is CAbilityInfuse cAbilityInfuse2)
			{
				cAbilityInfuse2.ShowPicker = (abilityOverride.OriginalAbility as CAbilityInfuse).ShowPicker;
			}
			if (abilityOverride.RecoverCardsWithAbilityOfTypeFilter != null)
			{
				if (this is CAbilityRecoverDiscardedCards cAbilityRecoverDiscardedCards)
				{
					cAbilityRecoverDiscardedCards.RecoverCardsWithAbilityOfTypeFilter = (abilityOverride.OriginalAbility as CAbilityRecoverDiscardedCards).RecoverCardsWithAbilityOfTypeFilter;
				}
				else if (this is CAbilityRecoverLostCards cAbilityRecoverLostCards)
				{
					cAbilityRecoverLostCards.RecoverCardsWithAbilityOfTypeFilter = (abilityOverride.OriginalAbility as CAbilityRecoverLostCards).RecoverCardsWithAbilityOfTypeFilter;
				}
			}
			if (abilityOverride.UseParentTargeting.HasValue && this is CAbilityPlayDiscardedCard cAbilityPlayDiscardedCard)
			{
				cAbilityPlayDiscardedCard.UseParentTargeting = (abilityOverride.OriginalAbility as CAbilityPlayDiscardedCard).UseParentTargeting;
			}
			if (abilityOverride.ExtraTurnAbilityTypeFilter != null && this is CAbilityExtraTurn cAbilityExtraTurn)
			{
				cAbilityExtraTurn.AbilityTypeFilter = (abilityOverride.OriginalAbility as CAbilityExtraTurn).AbilityTypeFilter;
			}
			if (abilityOverride.ForgoTopActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion)
			{
				cAbilityForgoActionsForCompanion.ForgoTopActionAbility = (abilityOverride.OriginalAbility as CAbilityForgoActionsForCompanion).ForgoTopActionAbility;
			}
			if (abilityOverride.ForgoBottomActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion2)
			{
				cAbilityForgoActionsForCompanion2.ForgoBottomActionAbility = (abilityOverride.OriginalAbility as CAbilityForgoActionsForCompanion).ForgoBottomActionAbility;
			}
			if (abilityOverride.ExtraTurnType.HasValue && this is CAbilityExtraTurn cAbilityExtraTurn2)
			{
				cAbilityExtraTurn2.ExtraTurnType = (abilityOverride.OriginalAbility as CAbilityExtraTurn).ExtraTurnType;
			}
			if (abilityOverride.ActionMonsterClassID != null && this is CAbilityExtraTurn cAbilityExtraTurn3)
			{
				cAbilityExtraTurn3.ActionMonsterClassID = (abilityOverride.OriginalAbility as CAbilityExtraTurn).ActionMonsterClassID;
			}
			if (abilityOverride.SwapFirstTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap)
			{
				cAbilitySwap.FirstTargetFilter = (abilityOverride.OriginalAbility as CAbilitySwap).FirstTargetFilter;
			}
			if (abilityOverride.SwapSecondTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap2)
			{
				cAbilitySwap2.SecondTargetFilter = (abilityOverride.OriginalAbility as CAbilitySwap).SecondTargetFilter;
			}
			if (abilityOverride.TeleportData != null && this is CAbilityTeleport cAbilityTeleport && abilityOverride.OriginalAbility is CAbilityTeleport cAbilityTeleport2)
			{
				cAbilityTeleport.AbilityTeleportData = cAbilityTeleport2.AbilityTeleportData;
			}
			if (abilityOverride.LootData != null && this is CAbilityLoot cAbilityLoot)
			{
				cAbilityLoot.AbilityLootData = (abilityOverride.OriginalAbility as CAbilityLoot).AbilityLootData;
			}
			if (abilityOverride.ControlActorData != null && this is CAbilityControlActor cAbilityControlActor && abilityOverride.OriginalAbility is CAbilityControlActor cAbilityControlActor2)
			{
				cAbilityControlActor.ControlActorData = cAbilityControlActor2.ControlActorData;
			}
			if (abilityOverride.ChangeAllegianceData != null && this is CAbilityChangeAllegiance cAbilityChangeAllegiance && abilityOverride.OriginalAbility is CAbilityChangeAllegiance cAbilityChangeAllegiance2)
			{
				cAbilityChangeAllegiance.ChangeAllegianceData = cAbilityChangeAllegiance2.ChangeAllegianceData.Copy();
			}
			if (abilityOverride.HealData != null && this is CAbilityHeal cAbilityHeal && abilityOverride.OriginalAbility is CAbilityHeal cAbilityHeal2)
			{
				cAbilityHeal.HealData = cAbilityHeal2.HealData;
			}
			if (abilityOverride.ImmunityToAbilityTypes != null && this is CAbilityImmunityTo cAbilityImmunityTo)
			{
				cAbilityImmunityTo.ImmuneToAbilityTypes = (abilityOverride.OriginalAbility as CAbilityImmunityTo).ImmuneToAbilityTypes.ToList();
			}
			if (abilityOverride.ModifierCardNamesToAdd != null && this is CAbilityAddModifierToMonsterDeck cAbilityAddModifierToMonsterDeck)
			{
				cAbilityAddModifierToMonsterDeck.ModifierCardNamesToAdd = (abilityOverride.OriginalAbility as CAbilityAddModifierToMonsterDeck).ModifierCardNamesToAdd.ToList();
			}
			if (abilityOverride.ResourcesToAddOnAbilityEnd != null)
			{
				ResourcesToAddOnAbilityEnd = abilityOverride.OriginalAbility.ResourcesToAddOnAbilityEnd.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToGiveToTargets != null)
			{
				ResourcesToGiveToTargets = abilityOverride.OriginalAbility.ResourcesToGiveToTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToTakeFromTargets != null)
			{
				ResourcesToTakeFromTargets = abilityOverride.OriginalAbility.ResourcesToTakeFromTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.DestroyObstacleData != null && this is CAbilityDestroyObstacle cAbilityDestroyObstacle && abilityOverride.OriginalAbility is CAbilityDestroyObstacle cAbilityDestroyObstacle2)
			{
				cAbilityDestroyObstacle.AbilityDestroyObstacleData = cAbilityDestroyObstacle2.AbilityDestroyObstacleData;
			}
			if (abilityOverride.HelpBoxLocalizationKey != null && HelpBoxTooltipLocKey == abilityOverride.HelpBoxLocalizationKey)
			{
				HelpBoxTooltipLocKey = abilityOverride.OriginalAbility.HelpBoxTooltipLocKey;
			}
			if (abilityOverride.StatIsBasedOnXEntries != null)
			{
				foreach (AbilityData.StatIsBasedOnXData stat in abilityOverride.StatIsBasedOnXEntries)
				{
					StatIsBasedOnXEntries.RemoveAll((AbilityData.StatIsBasedOnXData x) => x == stat);
				}
			}
			if (TargetingActor != null && HasStarted)
			{
				SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
			}
			if (abilityOverride.MiscAbilityData != null && MiscAbilityData != null && abilityOverride.OriginalAbility != null && abilityOverride.OriginalAbility.MiscAbilityData != null)
			{
				MiscAbilityData.TargetOneEnemyWithAllAttacks = abilityOverride.OriginalAbility.MiscAbilityData.TargetOneEnemyWithAllAttacks;
				MiscAbilityData.TreatAsMelee = abilityOverride.OriginalAbility.MiscAbilityData.TreatAsMelee;
				MiscAbilityData.TreatAsRange = abilityOverride.OriginalAbility.MiscAbilityData.TreatAsRange;
				MiscAbilityData.UseParentRangeType = abilityOverride.OriginalAbility.MiscAbilityData.UseParentRangeType;
				MiscAbilityData.AllTargetsAdjacentToParentMovePath = abilityOverride.OriginalAbility.MiscAbilityData.AllTargetsAdjacentToParentMovePath;
				MiscAbilityData.AllTargetsAdjacentToParentTargets = abilityOverride.OriginalAbility.MiscAbilityData.AllTargetsAdjacentToParentTargets;
				MiscAbilityData.UseParentTiles = abilityOverride.OriginalAbility.MiscAbilityData.UseParentTiles;
				MiscAbilityData.IgnoreParentAreaOfEffect = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreParentAreaOfEffect;
				MiscAbilityData.IgnorePreviousAbilityTargets = abilityOverride.OriginalAbility.MiscAbilityData.IgnorePreviousAbilityTargets?.ToList();
				MiscAbilityData.IgnoreMergedAbilityTargetSelection = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreMergedAbilityTargetSelection;
				MiscAbilityData.UseMergedWithAbilityTiles = abilityOverride.OriginalAbility.MiscAbilityData.UseMergedWithAbilityTiles;
				MiscAbilityData.AddTargetPropertyToStrength = abilityOverride.OriginalAbility.MiscAbilityData.AddTargetPropertyToStrength;
				MiscAbilityData.AddTargetPropertyToStrengthMultiplier = abilityOverride.OriginalAbility.MiscAbilityData.AddTargetPropertyToStrengthMultiplier;
				MiscAbilityData.RestrictMoveRange = abilityOverride.OriginalAbility.MiscAbilityData.RestrictMoveRange;
				MiscAbilityData.AutotriggerAbility = abilityOverride.OriginalAbility.MiscAbilityData.AutotriggerAbility;
				MiscAbilityData.HealPercentageOfHealth = abilityOverride.OriginalAbility.MiscAbilityData.HealPercentageOfHealth;
				MiscAbilityData.ExactRange = abilityOverride.OriginalAbility.MiscAbilityData.ExactRange;
				MiscAbilityData.FilterSpecified = abilityOverride.OriginalAbility.MiscAbilityData.FilterSpecified;
				MiscAbilityData.AttackHasAdvantage = abilityOverride.OriginalAbility.MiscAbilityData.AttackHasAdvantage;
				MiscAbilityData.AttackHasDisadvantage = abilityOverride.OriginalAbility.MiscAbilityData.AttackHasDisadvantage;
				MiscAbilityData.ExhaustSelf = abilityOverride.OriginalAbility.MiscAbilityData.ExhaustSelf;
				MiscAbilityData.GainXPPerXDamageDealt = abilityOverride.OriginalAbility.MiscAbilityData.GainXPPerXDamageDealt;
				MiscAbilityData.SetHPTo = abilityOverride.OriginalAbility.MiscAbilityData.SetHPTo;
				MiscAbilityData.PreventOnlyIfLethal = abilityOverride.OriginalAbility.MiscAbilityData.PreventOnlyIfLethal;
				MiscAbilityData.InfuseIfNotStrong = abilityOverride.OriginalAbility.MiscAbilityData.InfuseIfNotStrong;
				MiscAbilityData.CanTargetInvisible = abilityOverride.OriginalAbility.MiscAbilityData.CanTargetInvisible;
				MiscAbilityData.NotApplyEnemy = abilityOverride.OriginalAbility.MiscAbilityData.NotApplyEnemy;
				MiscAbilityData.CanUndo = abilityOverride.OriginalAbility.MiscAbilityData.CanUndo;
				MiscAbilityData.CanSkip = abilityOverride.OriginalAbility.MiscAbilityData.CanSkip;
				MiscAbilityData.ReplaceModifiers = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceModifiers;
				MiscAbilityData.ReplaceWithModifier = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithModifier;
				MiscAbilityData.ReplaceWithModifiers = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithModifiers;
				MiscAbilityData.ReplaceNegativeModifiers = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceNegativeModifiers;
				MiscAbilityData.ReplaceNegativeConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceNegativeConditions;
				MiscAbilityData.ReplacePositiveConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplacePositiveConditions;
				MiscAbilityData.ReplaceWithNegativeConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithNegativeConditions;
				MiscAbilityData.ReplaceWithPositiveConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithPositiveConditions;
				MiscAbilityData.GapAbove = abilityOverride.OriginalAbility.MiscAbilityData.GapAbove;
				MiscAbilityData.GapBelow = abilityOverride.OriginalAbility.MiscAbilityData.GapBelow;
				MiscAbilityData.IgnoreImmobilize = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreImmobilize;
				MiscAbilityData.IgnoreStun = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreStun;
				MiscAbilityData.HasCondition = abilityOverride.OriginalAbility.MiscAbilityData.HasCondition;
				MiscAbilityData.RemoveCount = abilityOverride.OriginalAbility.MiscAbilityData.RemoveCount;
				MiscAbilityData.PerformXTimesBasedOn = abilityOverride.OriginalAbility.MiscAbilityData.PerformXTimesBasedOn;
				MiscAbilityData.OnAttackInfuse = abilityOverride.OriginalAbility.MiscAbilityData.OnAttackInfuse;
				MiscAbilityData.AlreadyHasConditionDamageInstead = abilityOverride.OriginalAbility.MiscAbilityData.AlreadyHasConditionDamageInstead;
				MiscAbilityData.UseParentTargets = abilityOverride.OriginalAbility.MiscAbilityData.UseParentTargets;
				MiscAbilityData.AlsoTargetSelf = abilityOverride.OriginalAbility.MiscAbilityData.AlsoTargetSelf;
				MiscAbilityData.AlsoTargetAdjacent = abilityOverride.OriginalAbility.MiscAbilityData.AlsoTargetAdjacent;
				MiscAbilityData.ConditionsToRemoveFirst = abilityOverride.OriginalAbility.MiscAbilityData.ConditionsToRemoveFirst;
				MiscAbilityData.ShowPlusX = abilityOverride.OriginalAbility.MiscAbilityData.ShowPlusX;
				MiscAbilityData.TriggeredScenarioModifiers = abilityOverride.OriginalAbility.MiscAbilityData.TriggeredScenarioModifiers;
				MiscAbilityData.TriggerScenarioModifierOnSelfOnly = abilityOverride.OriginalAbility.MiscAbilityData.TriggerScenarioModifierOnSelfOnly;
				MiscAbilityData.BypassImmunity = abilityOverride.OriginalAbility.MiscAbilityData.BypassImmunity;
				MiscAbilityData.AllowContinueForNullAbility = abilityOverride.OriginalAbility.MiscAbilityData.AllowContinueForNullAbility;
				MiscAbilityData.NoGoldDrop = abilityOverride.OriginalAbility.MiscAbilityData.NoGoldDrop;
				MiscAbilityData.UseOriginalActor = abilityOverride.OriginalAbility.MiscAbilityData.UseOriginalActor;
				MiscAbilityData.ObstacleTypes = abilityOverride.OriginalAbility.MiscAbilityData.ObstacleTypes;
				MiscAbilityData.TerrainTypes = abilityOverride.OriginalAbility.MiscAbilityData.TerrainTypes;
				MiscAbilityData.PreventDamageSetTargetHealthTo = abilityOverride.OriginalAbility.MiscAbilityData.PreventDamageSetTargetHealthTo;
				MiscAbilityData.CasterDamageToTakeOnPrevent = abilityOverride.OriginalAbility.MiscAbilityData.CasterDamageToTakeOnPrevent;
				MiscAbilityData.IgnoreLOS = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreLOS;
				MiscAbilityData.XP = abilityOverride.OriginalAbility.MiscAbilityData.XP;
			}
			else if (abilityOverride.OriginalAbility != null && abilityOverride.OriginalAbility.MiscAbilityData == null)
			{
				MiscAbilityData = null;
			}
			if (abilityOverride.ConditionDuration.HasValue && this is CAbilityCondition)
			{
				(this as CAbilityCondition).Duration = (abilityOverride.OriginalAbility as CAbilityCondition).Duration;
			}
			if (abilityOverride.ConditionDecTrigger.HasValue)
			{
				(this as CAbilityCondition).DecrementTrigger = (abilityOverride.OriginalAbility as CAbilityCondition).DecrementTrigger;
			}
			if (abilityOverride.ConditionalOverrides != null)
			{
				foreach (CConditionalOverride conditionalOverride in abilityOverride.ConditionalOverrides)
				{
					if (ConditionalOverrides.Contains(conditionalOverride))
					{
						ConditionalOverrides.Remove(conditionalOverride);
					}
				}
			}
			if (abilityOverride.PositiveConditions != null)
			{
				foreach (CCondition.EPositiveCondition positiveCondition in abilityOverride.PositiveConditions)
				{
					if (m_PositiveConditions.Keys.Contains(positiveCondition))
					{
						m_PositiveConditions.Remove(positiveCondition);
					}
				}
			}
			if (abilityOverride.NegativeConditions != null)
			{
				foreach (CCondition.ENegativeCondition negativeCondition in abilityOverride.NegativeConditions)
				{
					if (m_NegativeConditions.Keys.Contains(negativeCondition))
					{
						m_NegativeConditions.Remove(negativeCondition);
					}
				}
			}
			abilityOverride.UndoOverrideVars(this);
			if (TargetingActor != null && m_AbilityStartComplete && !flag)
			{
				CPhaseAction cPhaseAction = null;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					cPhaseAction = (CPhaseAction)PhaseManager.Phase;
				}
				if (!m_BlockPerformAndRestart)
				{
					if (ShouldRestartAbilityWhenApplyingOverride(abilityOverride) && (cPhaseAction == null || cPhaseAction.CurrentPhaseAbility.m_Ability == this || (cPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility == this)))
					{
						if (CanClearTargets())
						{
							ClearTargets();
							m_HasAutoSelected = false;
						}
						Restart();
					}
					if (perform)
					{
						Perform();
					}
				}
			}
			TargetingActor?.m_OnAbilityOverrideAppliedListeners?.Invoke(FilterActor, abilityOverride, this, active: false);
			ScenarioManager.CurrentScenarioState?.OnAbilityOverridesChanged(this);
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("An error occurred attempting to undo the ability override.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public virtual CAbilityOverride OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null, CActor targetActor = null, bool fromModifier = false)
	{
		if (DontOverride)
		{
			if (TargetingActor != null && m_AbilityStartComplete)
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					_ = (CPhaseAction)PhaseManager.Phase;
				}
				if (perform)
				{
					Perform();
				}
			}
			return null;
		}
		try
		{
			bool flag = false;
			if (!abilityOverride.GetVar("CanRepeatOverride").GetBool() && !fromModifier && FindOverride(abilityOverride) != null)
			{
				return null;
			}
			abilityOverride = CAbilityOverride.CopyAbilityOverride(abilityOverride);
			m_CurrentOverrides.Add(abilityOverride);
			if (item != null)
			{
				m_CurrentItemOverrides.Add(abilityOverride, item);
			}
			abilityOverride.OriginalAbility = CopyAbility(this, generateNewID: false, fullCopy: true);
			bool flag2 = false;
			if (HasStarted)
			{
				ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
			}
			if (abilityOverride.Flags.HasValue)
			{
				Flags |= abilityOverride.Flags.Value;
			}
			if (abilityOverride.Tokens.HasValue)
			{
				Tokens += abilityOverride.Tokens.Value;
			}
			abilityOverride.OverrideStrength(this);
			if (abilityOverride.Range.HasValue)
			{
				if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
				{
					Range = abilityOverride.Range.Value;
					m_OriginalRange = abilityOverride.Range.Value;
				}
				else
				{
					Range += abilityOverride.Range.Value;
					m_OriginalRange += abilityOverride.Range.Value;
				}
			}
			if (this is CAbilityRetaliate cAbilityRetaliate && abilityOverride.RetaliateRange.HasValue)
			{
				cAbilityRetaliate.RetaliateRange += abilityOverride.RetaliateRange.Value;
			}
			if (abilityOverride.RangeAtLeastOne.HasValue && Range <= 0)
			{
				Range = 1;
				m_OriginalRange = 1;
			}
			if (abilityOverride.NumberOfTargets.HasValue)
			{
				abilityOverride.OverrideNumberTargets(this);
			}
			if (abilityOverride.TargetAtLeastOne.HasValue && NumberTargets <= 0)
			{
				NumberTargets = 1;
			}
			if (abilityOverride.RemoveConditionsOverride.HasValue)
			{
				m_PositiveConditions.Clear();
				m_NegativeConditions.Clear();
			}
			if (abilityOverride.Pierce.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).Pierce += abilityOverride.Pierce.Value;
			}
			if (abilityOverride.SubAbilities != null)
			{
				if (SubAbilities != null)
				{
					List<CAbility> list = abilityOverride.SubAbilities.ToList();
					MergeOrAddAbilityOfType(EAbilityType.Pull, list);
					MergeOrAddAbilityOfType(EAbilityType.Push, list);
					SubAbilities.AddRange(CopySubAbilities(list));
				}
				else
				{
					SubAbilities = CopySubAbilities(abilityOverride.SubAbilities);
				}
			}
			if (abilityOverride.ChooseAbilities != null && this is CAbilityChoose { ChooseAbilities: not null } cAbilityChoose)
			{
				cAbilityChoose.ChooseAbilities.AddRange(abilityOverride.ChooseAbilities);
			}
			if (abilityOverride.ElementsToInfuse != null && this is CAbilityInfuse)
			{
				CAbilityInfuse cAbilityInfuse = this as CAbilityInfuse;
				if (cAbilityInfuse.ElementsToInfuse == null)
				{
					cAbilityInfuse.ElementsToInfuse = abilityOverride.ElementsToInfuse;
				}
				else
				{
					cAbilityInfuse.ElementsToInfuse.AddRange(abilityOverride.ElementsToInfuse);
				}
			}
			if (abilityOverride.DamageSelfBeforeAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).DamageSelfBeforeAttack += abilityOverride.DamageSelfBeforeAttack.Value;
			}
			if (abilityOverride.AbilityType.HasValue)
			{
				flag = AbilityType != abilityOverride.AbilityType.Value;
				AbilityType = abilityOverride.AbilityType.Value;
			}
			if (abilityOverride.AbilityFilter != null)
			{
				AbilityFilter = abilityOverride.AbilityFilter;
				flag2 = true;
			}
			if (abilityOverride.AnimationOverload != null)
			{
				AnimOverload = abilityOverride.AnimationOverload;
			}
			if (abilityOverride.AreaEffect != null)
			{
				AreaEffect = abilityOverride.AreaEffect;
			}
			if (abilityOverride.AttackSourcesOnly.HasValue && this is CAbilityPreventDamage)
			{
				(this as CAbilityPreventDamage).AttackSourcesOnly = abilityOverride.AttackSourcesOnly.Value;
			}
			if (abilityOverride.Jump.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Jump = abilityOverride.Jump.Value;
			}
			if (abilityOverride.Fly.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Fly = abilityOverride.Fly.Value;
			}
			if (abilityOverride.IgnoreDifficultTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreDifficultTerrain = abilityOverride.IgnoreDifficultTerrain.Value;
			}
			if (abilityOverride.IgnoreHazardousTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreHazardousTerrain = abilityOverride.IgnoreHazardousTerrain.Value;
			}
			if (abilityOverride.CarryOtherActorsOnHex.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).CarryOtherActorsOnHex = abilityOverride.CarryOtherActorsOnHex.Value;
				(this as CAbilityMove).CarryOtherActorsOnHexUpdated();
			}
			if (abilityOverride.MoveRestrictionType.HasValue && this is CAbilityMove cAbilityMove)
			{
				cAbilityMove.MoveRestrictionType = abilityOverride.MoveRestrictionType.Value;
			}
			if (abilityOverride.ActiveBonusData != null)
			{
				ActiveBonusData = abilityOverride.ActiveBonusData;
			}
			if (abilityOverride.ActiveBonusYML != null)
			{
				ActiveBonusYML = abilityOverride.ActiveBonusYML;
			}
			if (abilityOverride.AreaEffectYMLString != null)
			{
				AreaEffectYMLString = abilityOverride.AreaEffectYMLString;
			}
			if (abilityOverride.AreaEffectLayoutOverrideYMLString != null)
			{
				AreaEffectLayoutOverrideYMLString = abilityOverride.AreaEffectLayoutOverrideYMLString;
			}
			if (abilityOverride.AreaEffectFlipAxis != null)
			{
				AreaEffectFlipAxis = abilityOverride.AreaEffectFlipAxis;
			}
			if (abilityOverride.MultiPassAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).MultiPassAttack = abilityOverride.MultiPassAttack.Value;
			}
			if (abilityOverride.AddAttackBaseStat.HasValue)
			{
				AddAttackBaseStat = abilityOverride.AddAttackBaseStat.Value;
			}
			if (abilityOverride.StrengthIsBase.HasValue)
			{
				StrengthIsBase = abilityOverride.StrengthIsBase.Value;
			}
			if (abilityOverride.RangeIsBase.HasValue)
			{
				RangeIsBase = abilityOverride.RangeIsBase.Value;
			}
			if (abilityOverride.TargetIsBase.HasValue)
			{
				TargetIsBase = abilityOverride.TargetIsBase.Value;
			}
			if (abilityOverride.AbilityText != null)
			{
				AbilityText = abilityOverride.AbilityText;
			}
			if (abilityOverride.AbilityTextOnly.HasValue)
			{
				AbilityTextOnly = abilityOverride.AbilityTextOnly.Value;
			}
			if (abilityOverride.ShowRange.HasValue)
			{
				ShowRange = abilityOverride.ShowRange.Value;
			}
			if (abilityOverride.ShowTarget.HasValue)
			{
				ShowTarget = abilityOverride.ShowTarget.Value;
			}
			if (abilityOverride.ShowArea.HasValue)
			{
				ShowArea = abilityOverride.ShowArea.Value;
			}
			if (abilityOverride.AllTargetsOnMovePath.HasValue)
			{
				AllTargetsOnMovePath = abilityOverride.AllTargetsOnMovePath.Value;
			}
			if (abilityOverride.AllTargetsOnMovePathSameStartAndEnd.HasValue)
			{
				AllTargetsOnMovePathSameStartAndEnd = abilityOverride.AllTargetsOnMovePathSameStartAndEnd.Value;
			}
			if (abilityOverride.AllTargetsOnAttackPath.HasValue)
			{
				AllTargetsOnAttackPath = abilityOverride.AllTargetsOnAttackPath.Value;
			}
			if (abilityOverride.Summons != null && this is CAbilitySummon)
			{
				(this as CAbilitySummon).SummonIDs = abilityOverride.Summons;
			}
			if (abilityOverride.XpPerTargetData != null)
			{
				m_XpPerTargetData = abilityOverride.XpPerTargetData;
			}
			if (abilityOverride.Targeting.HasValue)
			{
				Targeting = abilityOverride.Targeting.Value;
			}
			if (abilityOverride.MiscVars != null)
			{
				if (MiscVars == null)
				{
					MiscVars = abilityOverride.MiscVars;
				}
				else
				{
					MiscVars.AddRange(abilityOverride.MiscVars);
				}
			}
			if (abilityOverride.PerformVars != null)
			{
				if (PerformVars == null)
				{
					PerformVars = abilityOverride.PerformVars;
				}
				else
				{
					PerformVars.AddRange(abilityOverride.PerformVars);
				}
			}
			if (abilityOverride.PropName != null && this is CAbilityCreate)
			{
				(this as CAbilityCreate).PropName = abilityOverride.PropName;
			}
			if (abilityOverride.TrapData != null && this is CAbilityTrap)
			{
				(this as CAbilityTrap).TrapData = abilityOverride.TrapData;
			}
			if (abilityOverride.IsSubAbility.HasValue)
			{
				m_IsSubAbility = abilityOverride.IsSubAbility.Value;
			}
			if (abilityOverride.IsTargetedAbility.HasValue)
			{
				IsTargetedAbility = abilityOverride.IsTargetedAbility.Value;
				flag2 = true;
			}
			if (abilityOverride.SpawnDelay.HasValue)
			{
				SpawnDelay = abilityOverride.SpawnDelay.Value;
			}
			if (abilityOverride.AdditionalPushEffect.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffect = abilityOverride.AdditionalPushEffect.Value;
			}
			if (abilityOverride.AdditionalPushEffectDamage.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectDamage = abilityOverride.AdditionalPushEffectDamage.Value;
			}
			if (abilityOverride.AdditionalPushEffectXP.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectXP = abilityOverride.AdditionalPushEffectXP.Value;
			}
			if (abilityOverride.PullType.HasValue && this is CAbilityPull cAbilityPull)
			{
				cAbilityPull.PullType = abilityOverride.PullType.Value;
			}
			if (abilityOverride.AbilityXP.HasValue)
			{
				AbilityXP = abilityOverride.AbilityXP.Value;
			}
			if (abilityOverride.IsInlineSubAbility.HasValue)
			{
				IsInlineSubAbility = abilityOverride.IsInlineSubAbility.Value;
			}
			if (abilityOverride.SharePreviousAnim.HasValue)
			{
				SkipAnim = abilityOverride.SharePreviousAnim.Value;
			}
			if (abilityOverride.ConditionDuration.HasValue && this is CAbilityCondition)
			{
				(this as CAbilityCondition).Duration = abilityOverride.ConditionDuration.Value;
			}
			if (abilityOverride.ConditionDecTrigger.HasValue && this is CAbilityCondition)
			{
				(this as CAbilityCondition).DecrementTrigger = abilityOverride.ConditionDecTrigger.Value;
			}
			if (abilityOverride.ConditionalOverrides != null && abilityOverride.ConditionalOverrides.Count > 0)
			{
				ConditionalOverrides.AddRange(abilityOverride.ConditionalOverrides);
			}
			if (abilityOverride.IsConsumeAbility.HasValue)
			{
				IsConsumeAbility = abilityOverride.IsConsumeAbility.Value;
			}
			if (abilityOverride.Augment != null)
			{
				Augment = abilityOverride.Augment;
			}
			if (abilityOverride.Song != null)
			{
				Song = abilityOverride.Song;
			}
			if (abilityOverride.Doom != null && this is CAbilityAddDoom cAbilityAddDoom)
			{
				cAbilityAddDoom.Doom = abilityOverride.Doom;
			}
			if (abilityOverride.AttackEffects != null && abilityOverride.AttackEffects.Count > 0 && this is CAbilityAttack cAbilityAttack)
			{
				cAbilityAttack.AttackEffects.AddRange(abilityOverride.AttackEffects);
			}
			if (abilityOverride.ControlActorData != null && this is CAbilityControlActor cAbilityControlActor)
			{
				cAbilityControlActor.ControlActorData = cAbilityControlActor.ControlActorData.Merge(abilityOverride.ControlActorData);
			}
			if (abilityOverride.ChangeAllegianceData != null && this is CAbilityChangeAllegiance cAbilityChangeAllegiance)
			{
				cAbilityChangeAllegiance.ChangeAllegianceData = abilityOverride.ChangeAllegianceData.Copy();
			}
			if (abilityOverride.AddActiveBonusAbility != null && this is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				cAbilityAddActiveBonus.AddAbility = abilityOverride.AddActiveBonusAbility;
			}
			if (abilityOverride.ShowElementPicker.HasValue && this is CAbilityInfuse cAbilityInfuse2)
			{
				cAbilityInfuse2.ShowPicker = abilityOverride.ShowElementPicker.Value;
			}
			if (abilityOverride.RecoverCardsWithAbilityOfTypeFilter != null)
			{
				if (this is CAbilityRecoverDiscardedCards cAbilityRecoverDiscardedCards)
				{
					cAbilityRecoverDiscardedCards.RecoverCardsWithAbilityOfTypeFilter = abilityOverride.RecoverCardsWithAbilityOfTypeFilter;
				}
				else if (this is CAbilityRecoverLostCards cAbilityRecoverLostCards)
				{
					cAbilityRecoverLostCards.RecoverCardsWithAbilityOfTypeFilter = abilityOverride.RecoverCardsWithAbilityOfTypeFilter;
				}
			}
			if (abilityOverride.UseParentTargeting.HasValue && this is CAbilityPlayDiscardedCard cAbilityPlayDiscardedCard)
			{
				cAbilityPlayDiscardedCard.UseParentTargeting = abilityOverride.UseParentTargeting.Value;
			}
			if (abilityOverride.ExtraTurnAbilityTypeFilter != null && this is CAbilityExtraTurn cAbilityExtraTurn)
			{
				cAbilityExtraTurn.AbilityTypeFilter = abilityOverride.ExtraTurnAbilityTypeFilter;
			}
			if (abilityOverride.ForgoTopActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion)
			{
				cAbilityForgoActionsForCompanion.ForgoTopActionAbility = abilityOverride.ForgoTopActionAbility;
			}
			if (abilityOverride.ForgoBottomActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion2)
			{
				cAbilityForgoActionsForCompanion2.ForgoBottomActionAbility = abilityOverride.ForgoBottomActionAbility;
			}
			if (abilityOverride.ExtraTurnType.HasValue && this is CAbilityExtraTurn cAbilityExtraTurn2)
			{
				cAbilityExtraTurn2.ExtraTurnType = abilityOverride.ExtraTurnType.Value;
			}
			if (abilityOverride.ActionMonsterClassID != null && this is CAbilityExtraTurn cAbilityExtraTurn3)
			{
				cAbilityExtraTurn3.ActionMonsterClassID = abilityOverride.ActionMonsterClassID;
			}
			if (abilityOverride.SwapFirstTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap)
			{
				cAbilitySwap.FirstTargetFilter = abilityOverride.SwapFirstTargetAbilityFilter;
			}
			if (abilityOverride.SwapSecondTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap2)
			{
				cAbilitySwap2.SecondTargetFilter = abilityOverride.SwapSecondTargetAbilityFilter;
			}
			if (abilityOverride.TeleportData != null && this is CAbilityTeleport cAbilityTeleport)
			{
				cAbilityTeleport.AbilityTeleportData = abilityOverride.TeleportData;
			}
			if (abilityOverride.LootData != null && this is CAbilityLoot cAbilityLoot)
			{
				cAbilityLoot.AbilityLootData = abilityOverride.LootData;
			}
			if (abilityOverride.HealData != null && this is CAbilityHeal cAbilityHeal)
			{
				cAbilityHeal.HealData = abilityOverride.HealData;
			}
			if (abilityOverride.ImmunityToAbilityTypes != null && this is CAbilityImmunityTo cAbilityImmunityTo)
			{
				cAbilityImmunityTo.ImmuneToAbilityTypes = abilityOverride.ImmunityToAbilityTypes.ToList();
			}
			if (abilityOverride.ModifierCardNamesToAdd != null && this is CAbilityAddModifierToMonsterDeck cAbilityAddModifierToMonsterDeck)
			{
				cAbilityAddModifierToMonsterDeck.ModifierCardNamesToAdd = abilityOverride.ModifierCardNamesToAdd.ToList();
			}
			if (abilityOverride.ResourcesToAddOnAbilityEnd != null)
			{
				ResourcesToAddOnAbilityEnd = abilityOverride.ResourcesToAddOnAbilityEnd.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToGiveToTargets != null)
			{
				ResourcesToGiveToTargets = abilityOverride.ResourcesToGiveToTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToTakeFromTargets != null)
			{
				ResourcesToTakeFromTargets = abilityOverride.ResourcesToTakeFromTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.DestroyObstacleData != null && this is CAbilityDestroyObstacle cAbilityDestroyObstacle)
			{
				cAbilityDestroyObstacle.AbilityDestroyObstacleData = abilityOverride.DestroyObstacleData;
			}
			if (abilityOverride.HelpBoxLocalizationKey != null)
			{
				HelpBoxTooltipLocKey = abilityOverride.HelpBoxLocalizationKey;
			}
			if (abilityOverride.StatIsBasedOnXEntries != null)
			{
				foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXEntry in abilityOverride.StatIsBasedOnXEntries)
				{
					StatIsBasedOnXEntries.Add(statIsBasedOnXEntry);
				}
			}
			if (TargetingActor != null && HasStarted)
			{
				int strength = Strength;
				SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, (filter != null) ? filter : AbilityFilter, targetActor);
				if (Strength != strength)
				{
					SetModifiedStrength(Strength);
				}
			}
			if (abilityOverride.MiscAbilityData != null)
			{
				if (MiscAbilityData == null)
				{
					MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				MiscAbilityData.TargetOneEnemyWithAllAttacks = abilityOverride.MiscAbilityData.TargetOneEnemyWithAllAttacks ?? MiscAbilityData.TargetOneEnemyWithAllAttacks;
				MiscAbilityData.TreatAsMelee = abilityOverride.MiscAbilityData.TreatAsMelee ?? MiscAbilityData.TreatAsMelee;
				MiscAbilityData.TreatAsRange = abilityOverride.MiscAbilityData.TreatAsRange ?? MiscAbilityData.TreatAsRange;
				MiscAbilityData.UseParentRangeType = abilityOverride.MiscAbilityData.UseParentRangeType ?? MiscAbilityData.UseParentRangeType;
				MiscAbilityData.AllTargetsAdjacentToParentMovePath = abilityOverride.MiscAbilityData.AllTargetsAdjacentToParentMovePath ?? MiscAbilityData.AllTargetsAdjacentToParentMovePath;
				MiscAbilityData.AllTargetsAdjacentToParentTargets = abilityOverride.MiscAbilityData.AllTargetsAdjacentToParentTargets ?? MiscAbilityData.AllTargetsAdjacentToParentTargets;
				MiscAbilityData.UseParentTiles = abilityOverride.MiscAbilityData.UseParentTiles ?? MiscAbilityData.UseParentTiles;
				MiscAbilityData.IgnoreParentAreaOfEffect = abilityOverride.MiscAbilityData.IgnoreParentAreaOfEffect ?? MiscAbilityData.IgnoreParentAreaOfEffect;
				MiscAbilityData.IgnorePreviousAbilityTargets = abilityOverride.MiscAbilityData.IgnorePreviousAbilityTargets ?? MiscAbilityData.IgnorePreviousAbilityTargets?.ToList();
				MiscAbilityData.IgnoreMergedAbilityTargetSelection = abilityOverride.MiscAbilityData.IgnoreMergedAbilityTargetSelection ?? MiscAbilityData.IgnoreMergedAbilityTargetSelection;
				MiscAbilityData.UseMergedWithAbilityTiles = abilityOverride.MiscAbilityData.UseMergedWithAbilityTiles ?? MiscAbilityData.UseMergedWithAbilityTiles;
				MiscAbilityData.AddTargetPropertyToStrength = abilityOverride.MiscAbilityData.AddTargetPropertyToStrength ?? MiscAbilityData.AddTargetPropertyToStrength;
				MiscAbilityData.AddTargetPropertyToStrengthMultiplier = abilityOverride.MiscAbilityData.AddTargetPropertyToStrengthMultiplier ?? MiscAbilityData.AddTargetPropertyToStrengthMultiplier;
				MiscAbilityData.RestrictMoveRange = abilityOverride.MiscAbilityData.RestrictMoveRange ?? MiscAbilityData.RestrictMoveRange;
				MiscAbilityData.AutotriggerAbility = abilityOverride.MiscAbilityData.AutotriggerAbility ?? MiscAbilityData.AutotriggerAbility;
				MiscAbilityData.HealPercentageOfHealth = abilityOverride.MiscAbilityData.HealPercentageOfHealth ?? MiscAbilityData.HealPercentageOfHealth;
				MiscAbilityData.ExactRange = abilityOverride.MiscAbilityData.ExactRange ?? MiscAbilityData.ExactRange;
				MiscAbilityData.FilterSpecified = abilityOverride.MiscAbilityData.FilterSpecified ?? MiscAbilityData.FilterSpecified;
				MiscAbilityData.AttackHasAdvantage = abilityOverride.MiscAbilityData.AttackHasAdvantage ?? MiscAbilityData.AttackHasAdvantage;
				MiscAbilityData.AttackHasDisadvantage = abilityOverride.MiscAbilityData.AttackHasDisadvantage ?? MiscAbilityData.AttackHasDisadvantage;
				MiscAbilityData.ExhaustSelf = abilityOverride.MiscAbilityData.ExhaustSelf ?? MiscAbilityData.ExhaustSelf;
				MiscAbilityData.GainXPPerXDamageDealt = abilityOverride.MiscAbilityData.GainXPPerXDamageDealt ?? MiscAbilityData.GainXPPerXDamageDealt;
				MiscAbilityData.SetHPTo = abilityOverride.MiscAbilityData.SetHPTo ?? MiscAbilityData.SetHPTo;
				MiscAbilityData.PreventOnlyIfLethal = abilityOverride.MiscAbilityData.PreventOnlyIfLethal ?? MiscAbilityData.PreventOnlyIfLethal;
				MiscAbilityData.InfuseIfNotStrong = abilityOverride.MiscAbilityData.InfuseIfNotStrong ?? MiscAbilityData.InfuseIfNotStrong;
				MiscAbilityData.CanTargetInvisible = abilityOverride.MiscAbilityData.CanTargetInvisible ?? MiscAbilityData.CanTargetInvisible;
				MiscAbilityData.NotApplyEnemy = abilityOverride.MiscAbilityData.NotApplyEnemy ?? MiscAbilityData.NotApplyEnemy;
				MiscAbilityData.CanUndo = abilityOverride.MiscAbilityData.CanUndo ?? MiscAbilityData.CanUndo;
				MiscAbilityData.CanSkip = abilityOverride.MiscAbilityData.CanSkip ?? MiscAbilityData.CanSkip;
				MiscAbilityData.ReplaceModifiers = abilityOverride.MiscAbilityData.ReplaceModifiers ?? MiscAbilityData.ReplaceModifiers;
				MiscAbilityData.ReplaceWithModifier = abilityOverride.MiscAbilityData.ReplaceWithModifier ?? MiscAbilityData.ReplaceWithModifier;
				MiscAbilityData.ReplaceWithModifiers = abilityOverride.MiscAbilityData.ReplaceWithModifiers ?? MiscAbilityData.ReplaceWithModifiers;
				MiscAbilityData.ReplaceNegativeModifiers = abilityOverride.MiscAbilityData.ReplaceNegativeModifiers ?? MiscAbilityData.ReplaceNegativeModifiers;
				MiscAbilityData.ReplaceNegativeConditions = abilityOverride.MiscAbilityData.ReplaceNegativeConditions ?? MiscAbilityData.ReplaceNegativeConditions;
				MiscAbilityData.ReplacePositiveConditions = abilityOverride.MiscAbilityData.ReplacePositiveConditions ?? MiscAbilityData.ReplacePositiveConditions;
				MiscAbilityData.ReplaceWithNegativeConditions = abilityOverride.MiscAbilityData.ReplaceWithNegativeConditions ?? MiscAbilityData.ReplaceWithNegativeConditions;
				MiscAbilityData.ReplaceWithPositiveConditions = abilityOverride.MiscAbilityData.ReplaceWithPositiveConditions ?? MiscAbilityData.ReplaceWithPositiveConditions;
				MiscAbilityData.GapAbove = abilityOverride.MiscAbilityData.GapAbove ?? MiscAbilityData.GapAbove;
				MiscAbilityData.GapBelow = abilityOverride.MiscAbilityData.GapBelow ?? MiscAbilityData.GapBelow;
				MiscAbilityData.IgnoreImmobilize = abilityOverride.MiscAbilityData.IgnoreImmobilize ?? MiscAbilityData.IgnoreImmobilize;
				MiscAbilityData.IgnoreStun = abilityOverride.MiscAbilityData.IgnoreStun ?? MiscAbilityData.IgnoreStun;
				MiscAbilityData.HasCondition = abilityOverride.MiscAbilityData.HasCondition ?? MiscAbilityData.HasCondition;
				MiscAbilityData.RemoveCount = abilityOverride.MiscAbilityData.RemoveCount ?? MiscAbilityData.RemoveCount;
				MiscAbilityData.PerformXTimesBasedOn = abilityOverride.MiscAbilityData.PerformXTimesBasedOn ?? MiscAbilityData.PerformXTimesBasedOn;
				MiscAbilityData.OnAttackInfuse = abilityOverride.MiscAbilityData.OnAttackInfuse ?? MiscAbilityData.OnAttackInfuse;
				MiscAbilityData.AlreadyHasConditionDamageInstead = abilityOverride.MiscAbilityData.AlreadyHasConditionDamageInstead ?? MiscAbilityData.AlreadyHasConditionDamageInstead;
				MiscAbilityData.UseParentTargets = abilityOverride.MiscAbilityData.UseParentTargets ?? MiscAbilityData.UseParentTargets;
				MiscAbilityData.AlsoTargetSelf = abilityOverride.MiscAbilityData.AlsoTargetSelf ?? MiscAbilityData.AlsoTargetSelf;
				MiscAbilityData.AlsoTargetAdjacent = abilityOverride.MiscAbilityData.AlsoTargetAdjacent ?? MiscAbilityData.AlsoTargetAdjacent;
				MiscAbilityData.ConditionsToRemoveFirst = abilityOverride.MiscAbilityData.ConditionsToRemoveFirst ?? MiscAbilityData.ConditionsToRemoveFirst;
				MiscAbilityData.ShowPlusX = abilityOverride.MiscAbilityData.ShowPlusX ?? MiscAbilityData.ShowPlusX;
				MiscAbilityData.TriggeredScenarioModifiers = abilityOverride.MiscAbilityData.TriggeredScenarioModifiers ?? MiscAbilityData.TriggeredScenarioModifiers;
				MiscAbilityData.TriggerScenarioModifierOnSelfOnly = abilityOverride.MiscAbilityData.TriggerScenarioModifierOnSelfOnly ?? MiscAbilityData.TriggerScenarioModifierOnSelfOnly;
				MiscAbilityData.BypassImmunity = abilityOverride.MiscAbilityData.BypassImmunity ?? MiscAbilityData.BypassImmunity;
				MiscAbilityData.AllowContinueForNullAbility = abilityOverride.MiscAbilityData.AllowContinueForNullAbility ?? MiscAbilityData.AllowContinueForNullAbility;
				MiscAbilityData.NoGoldDrop = abilityOverride.MiscAbilityData.NoGoldDrop ?? MiscAbilityData.NoGoldDrop;
				MiscAbilityData.UseOriginalActor = abilityOverride.MiscAbilityData.UseOriginalActor ?? MiscAbilityData.UseOriginalActor;
				MiscAbilityData.ObstacleTypes = abilityOverride.MiscAbilityData.ObstacleTypes ?? MiscAbilityData.ObstacleTypes;
				MiscAbilityData.TerrainTypes = abilityOverride.MiscAbilityData.TerrainTypes ?? MiscAbilityData.TerrainTypes;
				MiscAbilityData.PreventDamageSetTargetHealthTo = abilityOverride.MiscAbilityData.PreventDamageSetTargetHealthTo ?? MiscAbilityData.PreventDamageSetTargetHealthTo;
				MiscAbilityData.CasterDamageToTakeOnPrevent = abilityOverride.MiscAbilityData.CasterDamageToTakeOnPrevent ?? MiscAbilityData.CasterDamageToTakeOnPrevent;
				MiscAbilityData.IgnoreLOS = abilityOverride.MiscAbilityData.IgnoreLOS;
				MiscAbilityData.XP = abilityOverride.MiscAbilityData.XP;
			}
			if (abilityOverride.PositiveConditions != null)
			{
				if (m_PositiveConditions == null)
				{
					m_PositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>();
				}
				else if (flag2)
				{
					Dictionary<CCondition.EPositiveCondition, CAbility> dictionary = new Dictionary<CCondition.EPositiveCondition, CAbility>();
					foreach (CCondition.EPositiveCondition posCondition in m_PositiveConditions.Keys)
					{
						dictionary.Add(posCondition, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == posCondition.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
					m_PositiveConditions = dictionary;
				}
				foreach (CCondition.EPositiveCondition posCondition2 in abilityOverride.PositiveConditions)
				{
					try
					{
						if (!m_PositiveConditions.ContainsKey(posCondition2))
						{
							m_PositiveConditions.Add(posCondition2, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == posCondition2.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility, 1));
						}
						else if (posCondition2 == CCondition.EPositiveCondition.Bless)
						{
							CAbility cAbility = m_PositiveConditions[posCondition2];
							if (cAbility.Strength == 0)
							{
								cAbility.Strength++;
							}
							cAbility.Strength++;
						}
					}
					catch
					{
						Log.ErrorWithReport("Condition " + posCondition2.ToString() + " could not be found in EAbilityType enum.");
					}
				}
			}
			if (abilityOverride.NegativeConditions != null)
			{
				if (m_NegativeConditions == null)
				{
					m_NegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();
				}
				else if (flag2)
				{
					Dictionary<CCondition.ENegativeCondition, CAbility> dictionary2 = new Dictionary<CCondition.ENegativeCondition, CAbility>();
					foreach (CCondition.ENegativeCondition negCondition in m_NegativeConditions.Keys)
					{
						dictionary2.Add(negCondition, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == negCondition.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
					m_NegativeConditions = dictionary2;
				}
				foreach (CCondition.ENegativeCondition negCondition2 in abilityOverride.NegativeConditions)
				{
					try
					{
						if (!m_NegativeConditions.ContainsKey(negCondition2))
						{
							m_NegativeConditions.Add(negCondition2, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == negCondition2.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility, 1));
						}
						else if (negCondition2 == CCondition.ENegativeCondition.Curse)
						{
							CAbility cAbility2 = m_NegativeConditions[negCondition2];
							if (cAbility2.Strength == 0)
							{
								cAbility2.Strength++;
							}
							cAbility2.Strength++;
						}
					}
					catch
					{
						Log.ErrorWithReport("Error: Condition " + negCondition2.ToString() + " could not be found in EAbilityType enum.");
					}
				}
			}
			abilityOverride.OverrideVars(this);
			if (TargetingActor != null && m_AbilityStartComplete && !flag)
			{
				CPhaseAction cPhaseAction = null;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					cPhaseAction = (CPhaseAction)PhaseManager.Phase;
				}
				if (!m_BlockPerformAndRestart)
				{
					if (ShouldRestartAbilityWhenApplyingOverride(abilityOverride) && (cPhaseAction == null || cPhaseAction.CurrentPhaseAbility.m_Ability == this || (cPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility == this)))
					{
						if (CanClearTargets())
						{
							ClearTargets();
							m_HasAutoSelected = false;
						}
						Restart();
					}
					if (perform)
					{
						Perform();
					}
				}
			}
			TargetingActor?.m_OnAbilityOverrideAppliedListeners?.Invoke(TargetingActor, abilityOverride, this, active: true);
			ScenarioManager.CurrentScenarioState?.OnAbilityOverridesChanged(this);
			return abilityOverride;
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("An error occurred attempting to perform the ability override.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return null;
	}

	public void MergeOrAddAbilityOfType(EAbilityType type, List<CAbility> SubAbilitiesToAdd)
	{
		CAbility existingAbility = SubAbilities.FirstOrDefault((CAbility x) => x.AbilityType == type && x.IsInlineSubAbility);
		if (existingAbility == null)
		{
			return;
		}
		List<CAbility> list = SubAbilitiesToAdd.Where((CAbility x) => x.AbilityType == existingAbility.AbilityType && x.NumberTargets == existingAbility.NumberTargets && x.IsInlineSubAbility).ToList();
		if (existingAbility == null || list.Count <= 0)
		{
			return;
		}
		foreach (CAbility item in list)
		{
			existingAbility.Strength += item.Strength;
			SubAbilitiesToAdd.Remove(item);
		}
	}

	private void RemoveMergedSubabilitiesOfType(EAbilityType type, List<CAbility> SubAbilitiesToRemove)
	{
		CAbility existingAbility = SubAbilities.FirstOrDefault((CAbility x) => x.AbilityType == type && x.IsInlineSubAbility);
		if (existingAbility == null)
		{
			return;
		}
		List<CAbility> list = SubAbilitiesToRemove.Where((CAbility x) => x.AbilityType == existingAbility.AbilityType && x.NumberTargets == existingAbility.NumberTargets && x.IsInlineSubAbility).ToList();
		if (existingAbility == null || list.Count <= 0)
		{
			return;
		}
		foreach (CAbility item in list)
		{
			existingAbility.Strength -= item.Strength;
			SubAbilitiesToRemove.Remove(item);
			if (existingAbility.Strength <= 0)
			{
				SubAbilities.Remove(existingAbility);
				break;
			}
		}
	}

	public static List<EEnhancement> GetValidEnhancements(EAbilityType abilityType, Dictionary<CCondition.EPositiveCondition, CAbility> positiveConditions, Dictionary<CCondition.ENegativeCondition, CAbility> negativeConditions, EEnhancementLine line)
	{
		List<EEnhancement> list = new List<EEnhancement>();
		switch (line)
		{
		case EEnhancementLine.Targets:
			list.Add(EEnhancement.PlusTarget);
			return list;
		case EEnhancementLine.Range:
			list.Add(EEnhancement.PlusRange);
			return list;
		case EEnhancementLine.Pierce:
			list.Add(EEnhancement.PlusPierce);
			return list;
		case EEnhancementLine.SummonAttack:
			list.Add(EEnhancement.SummonAttack);
			return list;
		case EEnhancementLine.SummonHealth:
			list.Add(EEnhancement.SummonHP);
			return list;
		case EEnhancementLine.SummonMove:
			list.Add(EEnhancement.SummonMove);
			return list;
		case EEnhancementLine.SummonRange:
			list.Add(EEnhancement.SummonRange);
			return list;
		case EEnhancementLine.RetaliateRange:
			list.Add(EEnhancement.PlusRetaliateRange);
			break;
		case EEnhancementLine.AreaHex:
			list.Add(EEnhancement.Area);
			return list;
		case EEnhancementLine.Push:
			list.Add(EEnhancement.PlusPush);
			return list;
		case EEnhancementLine.Pull:
			list.Add(EEnhancement.PlusPull);
			return list;
		case EEnhancementLine.Mainline:
			switch (abilityType)
			{
			case EAbilityType.Attack:
				list.Add(EEnhancement.PlusAttack);
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Move:
				list.Add(EEnhancement.PlusMove);
				list.Add(EEnhancement.Jump);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				break;
			case EAbilityType.Teleport2:
				list.Add(EEnhancement.PlusTeleport);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				break;
			case EAbilityType.Heal:
				list.Add(EEnhancement.PlusHeal);
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Regenerate))
				{
					list.Add(EEnhancement.Regenerate);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Ward))
				{
					list.Add(EEnhancement.Ward);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Retaliate:
				list.Add(EEnhancement.PlusRetaliate);
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Regenerate))
				{
					list.Add(EEnhancement.Regenerate);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Ward))
				{
					list.Add(EEnhancement.Ward);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Shield:
				list.Add(EEnhancement.PlusShield);
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Regenerate))
				{
					list.Add(EEnhancement.Regenerate);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Ward))
				{
					list.Add(EEnhancement.Ward);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Poison:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Wound:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Muddle:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Immobilize:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Disarm:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Stun:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Curse:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Strengthen:
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Invisible:
			case EAbilityType.Bless:
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Regenerate))
				{
					list.Add(EEnhancement.Regenerate);
				}
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Ward))
				{
					list.Add(EEnhancement.Ward);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Push:
				list.Add(EEnhancement.PlusPush);
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Pull:
				list.Add(EEnhancement.PlusPull);
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Sleep:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Bane:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			default:
				Log.ErrorWithReport("Unsupported Ability Type for enhancemnets");
				return list;
			}
			break;
		default:
			Log.ErrorWithReport("Unsupported Enhancement Line for enhancemnets");
			return list;
		}
		return list;
	}

	public virtual bool Validate()
	{
		return true;
	}

	public virtual void ShowToggleBonusesForAI()
	{
		CShowToggleBonusesForAI_MessageData cShowToggleBonusesForAI_MessageData = new CShowToggleBonusesForAI_MessageData(TargetingActor);
		cShowToggleBonusesForAI_MessageData.m_Ability = this;
		ScenarioRuleClient.MessageHandler(cShowToggleBonusesForAI_MessageData);
	}

	public virtual bool IsValidAbilityForActiveBonusOfType(EAbilityType abilityType)
	{
		if (AbilityType == abilityType)
		{
			if (ActiveBonusData != null)
			{
				return ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
			}
			return true;
		}
		return false;
	}

	public virtual bool IsPositive()
	{
		return false;
	}

	public bool IsSpeedValidForActiveBonus(CActor actor)
	{
		if (actor is CPlayerActor { Agility: not CPlayerActor.EAgility.None } cPlayerActor)
		{
			if ((ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.WhenFast) == 0 || cPlayerActor.Agility == CPlayerActor.EAgility.Fast)
			{
				if ((ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.WhenSlow) != 0)
				{
					return cPlayerActor.Agility == CPlayerActor.EAgility.Slow;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public CAction GetAction()
	{
		return (GetAbilityBaseCard() as CAbilityCard)?.GetAction(this);
	}

	protected bool ShowCancelledHelpText(bool passing = false)
	{
		if (m_ConfirmCancelled && !CancelIfNoValid)
		{
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CGenericAbilityInvalidHelpText_MessageData message = new CGenericAbilityInvalidHelpText_MessageData(TargetingActor, this);
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
				PhaseManager.NextStep(passing);
			}
			return true;
		}
		if (AbilityHasBeenCancelled)
		{
			PhaseManager.NextStep(passing);
			return true;
		}
		return false;
	}

	public bool StackInlineSubAbilities(CActor target = null, CTile tile = null, CAbility parentAbility = null, bool ignorePerformNow = true, CAttackSummary.TargetSummary targetSummary = null)
	{
		if (tile == null && target != null)
		{
			tile = target.Tile;
		}
		else if (target == null && tile != null)
		{
			target = tile.LivingActor;
		}
		if (parentAbility == null)
		{
			parentAbility = this;
		}
		if (SubAbilities.Count > 0 && SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
		{
			List<CAbility> list = new List<CAbility>();
			foreach (CAbility item in SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
			{
				if (item.MiscAbilityData == null || !item.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || (target != null && target.IsDead))
				{
					CAbility cAbility = CopyAbility(item, this is CAbilityAttack, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, parentAbility.VarsContext);
					cAbility.ParentAbility = parentAbility;
					cAbility.InlineSubAbilityTiles.Add(tile);
					cAbility.TargetThisActorAutomatically = target;
					cAbility.IsControlAbility = IsControlAbility;
					cAbility.ControllingActor = ControllingActor;
					cAbility.ControlledByClassID = ControlledByClassID;
					cAbility.TargetingOverrideArrayIndex = TargetingOverrideArrayIndex;
					cAbility.AddTargetSummaryContext(targetSummary);
					list.Add(cAbility);
				}
			}
			if (list.Count > 0)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow, TargetingActor);
				PhaseManager.StepComplete();
				return true;
			}
		}
		return false;
	}

	public bool StackInlineSubAbilities(List<CActor> targets)
	{
		foreach (CActor target in targets)
		{
			if (StackInlineSubAbilities(target))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void RecalcStatIsBasedOnX()
	{
	}

	protected virtual bool ShouldCheckBlocking()
	{
		return GetMiscVar("CheckIfWillBeBlocking").GetBool();
	}

	protected bool CheckIfWillBeBlocking(CTile tile, bool warn = false)
	{
		return CheckIfWillBeBlocking(tile, TilesSelected, warn);
	}

	protected bool CheckIfWillBeBlocking(CTile tile, List<CTile> tiles, bool warn = false)
	{
		if (!ShouldCheckBlocking())
		{
			return false;
		}
		bool flag = WillTileBlockTerrain(tile, TargetingActor, tiles);
		if (!flag)
		{
			CMirrorDamageActiveBonus_MirrorAbilities mirrorBeh = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour(TargetingActor);
			if (mirrorBeh != null)
			{
				flag = WillTileBlockTerrain(mirrorBeh.FindMirrorTile(tile), mirrorBeh.MirrorTarget, tiles.Select((CTile x) => mirrorBeh.FindMirrorTile(x)).ToList());
			}
		}
		if (flag & warn)
		{
			InvalidTileReason = "GUI_TOOLTIP_SELECT_NON_BLOCKING_TILES";
			OnFindBlockingTiles();
		}
		return flag;
	}

	public virtual bool ShouldCheckLinkedTiles()
	{
		return GetMiscVar("SelectMultiHexProps").GetBool();
	}

	public static bool WillTileBlockTerrain(CTile tile, CActor TargetingActor, List<CTile> TilesSelected)
	{
		List<CTile> extraUnblockedTiles = new List<CTile>();
		MF.IsBlockingSpawnPosition(null, null, TargetingActor, ref extraUnblockedTiles);
		return MF.IsBlockingSpawnPosition(tile, TilesSelected, TargetingActor, ref extraUnblockedTiles);
	}

	protected virtual void OnFindBlockingTiles()
	{
	}

	public CVarsContext AddTargetSummaryContext(CAttackSummary.TargetSummary targetSummary)
	{
		if (targetSummary != null)
		{
			CVars cVars = new CVars();
			CVarsContext cVarsContext = new CVarsContext(cVars);
			cVars.Set("AbilityCameFromSummary", new TValue(targetSummary));
			AddParentContext(cVarsContext);
			return cVarsContext;
		}
		return null;
	}

	public virtual List<CActor> TargetFromAnywhereActors()
	{
		return new List<CActor>();
	}

	public virtual bool CanTargetFromAnywhere(CActor target)
	{
		return false;
	}

	public void ExcludeMirroredHealthClones(List<CActor> targets)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			for (int i = 0; i < targets.Count; i++)
			{
				CActor mirrorHealthActor = targets[i].GetMirrorHealthActor();
				if (mirrorHealthActor != null)
				{
					targets.Remove(mirrorHealthActor);
				}
			}
		}
	}

	public bool IsMirroredHealthCloneTargeted(CActor actor)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			CActor mirrorHealthActor = actor.GetMirrorHealthActor();
			if (mirrorHealthActor != null)
			{
				return ActorsToTarget.Contains(mirrorHealthActor);
			}
			return false;
		}
	}

	public void TryAutoSelectTiles()
	{
		if (!AutoSelectTiles || m_HasAutoSelected)
		{
			return;
		}
		foreach (CActor item in ValidActorsInRange.ToList())
		{
			TileSelected(item.Tile, null);
		}
		m_HasAutoSelected = true;
	}

	public virtual CAbility GetQueuedAbility(CActor targetingActor)
	{
		return this;
	}
}
