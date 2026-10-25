using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AbilityData
{
	[JsonObject(MemberSerialization.OptIn)]
	public class XpPerTargetData
	{
		[JsonProperty]
		public int XpPerTarget;

		[JsonProperty]
		public int PerEnemyCount = 1;

		[JsonProperty]
		public bool MustDie;

		private int targetsAttacked;

		public XpPerTargetData Copy()
		{
			return new XpPerTargetData
			{
				XpPerTarget = XpPerTarget,
				PerEnemyCount = PerEnemyCount,
				MustDie = MustDie,
				targetsAttacked = 0
			};
		}

		public void Init()
		{
			targetsAttacked = 0;
		}

		public int AttackTargets(CActor targetingActor, List<CActor> targets)
		{
			int num = 0;
			foreach (CActor target in targets)
			{
				num += AttackTarget(targetingActor, target);
			}
			return num;
		}

		public int AttackTarget(CActor targetingActor, CActor target)
		{
			int num = 0;
			if (targetingActor.Type == CActor.EType.Player && (!MustDie || (MustDie && target.Health <= 0)))
			{
				targetsAttacked++;
				if (targetsAttacked % PerEnemyCount == 0)
				{
					num += XpPerTarget;
				}
			}
			return num;
		}
	}

	[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
	public class MiscAbilityData
	{
		[JsonProperty]
		public bool? TargetOneEnemyWithAllAttacks;

		[JsonProperty]
		public bool? TreatAsMelee;

		[JsonProperty]
		public bool? TreatAsRange;

		[JsonProperty]
		public bool? UseParentRangeType;

		[JsonProperty]
		public bool? AllTargetsAdjacentToParentMovePath;

		[JsonProperty]
		public bool? AllTargetsAdjacentToParentTargets;

		[JsonProperty]
		public bool? UseParentTiles;

		[JsonProperty]
		public bool? IgnoreParentAreaOfEffect;

		[JsonProperty]
		public List<string> IgnorePreviousAbilityTargets;

		[JsonProperty]
		public bool? IgnoreMergedAbilityTargetSelection;

		[JsonProperty]
		public bool? UseMergedWithAbilityTiles;

		[JsonProperty]
		public CActor.EOverrideLookupProperty? AddTargetPropertyToStrength;

		[JsonProperty]
		public int? AddTargetPropertyToStrengthMultiplier;

		[JsonProperty]
		public int? RestrictMoveRange;

		[JsonProperty]
		public bool? AutotriggerAbility;

		[JsonProperty]
		public float? HealPercentageOfHealth;

		[JsonProperty]
		public bool? ExactRange;

		[JsonProperty]
		public bool? FilterSpecified;

		[JsonProperty]
		public bool? AttackHasAdvantage;

		[JsonProperty]
		public bool? AttackHasDisadvantage;

		[JsonProperty]
		public bool? ExhaustSelf;

		[JsonProperty]
		public int? GainXPPerXDamageDealt;

		[JsonProperty]
		public int? SetHPTo;

		[JsonProperty]
		public bool? PreventOnlyIfLethal;

		[JsonProperty]
		public int? CasterDamageToTakeOnPrevent;

		[JsonProperty]
		public bool? InfuseIfNotStrong;

		[JsonProperty]
		public bool? CanTargetInvisible;

		[JsonProperty]
		public bool? NotApplyEnemy;

		[JsonProperty]
		public bool? IgnoreMonsterColumnLayout;

		[JsonProperty]
		public int? IgnoreMCLAdjustY;

		[JsonProperty]
		public int? GapAbove;

		[JsonProperty]
		public int? GapBelow;

		[JsonProperty]
		public bool? CanUndo;

		[JsonProperty]
		public bool? CanSkip;

		[JsonProperty]
		public bool? TreatAsNonSubAbility;

		[JsonProperty]
		public CAbility.EStatIsBasedOnXType? PerformXTimesBasedOn;

		[JsonProperty]
		public bool? ConsiderMultiTargetForEnhancements;

		[JsonProperty]
		public int? AlreadyHasConditionDamageInstead;

		[JsonProperty]
		public bool? IgnoreImmobilize;

		[JsonProperty]
		public bool? IgnoreStun;

		[JsonProperty]
		public bool? HasCondition;

		[JsonProperty]
		public bool? AreaEffectSymmetrical;

		[JsonProperty]
		public int? RemoveCount;

		[JsonProperty]
		public bool? UseParentTargets;

		[JsonProperty]
		public bool? AlsoTargetSelf;

		[JsonProperty]
		public CAbilityFilter AlsoTargetAdjacent;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> ConditionsToRemoveFirst;

		[JsonProperty]
		public bool? ShowPlusX;

		[JsonProperty]
		public List<string> TriggeredScenarioModifiers;

		[JsonProperty]
		public bool? TriggerScenarioModifierOnSelfOnly;

		[JsonProperty]
		public bool? BypassImmunity;

		[JsonProperty]
		public bool? InlineSubAbilityOnKilledTargetsOnly;

		[JsonProperty]
		public CEqualityFilter MovePathIndexFilter;

		[JsonProperty]
		public bool? NegativeAbilityIsOptional;

		[JsonProperty]
		public bool? AllowContinueForNullAbility;

		[JsonProperty]
		public bool? NoGoldDrop;

		[JsonProperty]
		public bool? UseOriginalActor;

		[JsonProperty]
		public int? MinRange;

		[JsonProperty]
		public bool StartIfParentHasHappened;

		[JsonProperty]
		public List<string> ReplaceModifiers;

		[JsonProperty]
		public string ReplaceWithModifier;

		[JsonProperty]
		public List<string> ReplaceWithModifiers;

		[JsonProperty]
		public bool? ReplaceNegativeModifiers;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> ReplaceNegativeConditions;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> ReplacePositiveConditions;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> ReplaceWithNegativeConditions;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> ReplaceWithPositiveConditions;

		[JsonProperty]
		public List<ElementInfusionBoardManager.EElement> OnAttackInfuse;

		[JsonProperty]
		public List<EPropType> ObstacleTypes;

		[JsonProperty]
		public List<EPropType> TerrainTypes;

		[JsonProperty]
		public bool AttackFurthestTarget;

		[JsonProperty]
		public bool IsCostAbility;

		[JsonProperty]
		public string? NameKeyOverride;

		[JsonProperty]
		public int? PreventDamageSetTargetHealthTo;

		[JsonProperty]
		public bool IgnoreLOS;

		[JsonProperty]
		public int? XP;

		public MiscAbilityData Copy()
		{
			return new MiscAbilityData
			{
				TargetOneEnemyWithAllAttacks = TargetOneEnemyWithAllAttacks,
				TreatAsMelee = TreatAsMelee,
				TreatAsRange = TreatAsRange,
				UseParentRangeType = UseParentRangeType,
				AllTargetsAdjacentToParentMovePath = AllTargetsAdjacentToParentMovePath,
				AllTargetsAdjacentToParentTargets = AllTargetsAdjacentToParentTargets,
				UseParentTiles = UseParentTiles,
				IgnoreParentAreaOfEffect = IgnoreParentAreaOfEffect,
				IgnorePreviousAbilityTargets = ((IgnorePreviousAbilityTargets != null) ? IgnorePreviousAbilityTargets.ToList() : null),
				IgnoreMergedAbilityTargetSelection = IgnoreMergedAbilityTargetSelection,
				UseMergedWithAbilityTiles = UseMergedWithAbilityTiles,
				AddTargetPropertyToStrength = AddTargetPropertyToStrength,
				AddTargetPropertyToStrengthMultiplier = AddTargetPropertyToStrengthMultiplier,
				RestrictMoveRange = RestrictMoveRange,
				AutotriggerAbility = AutotriggerAbility,
				HealPercentageOfHealth = HealPercentageOfHealth,
				ExactRange = ExactRange,
				FilterSpecified = FilterSpecified,
				AttackHasAdvantage = AttackHasAdvantage,
				AttackHasDisadvantage = AttackHasDisadvantage,
				ExhaustSelf = ExhaustSelf,
				GainXPPerXDamageDealt = GainXPPerXDamageDealt,
				SetHPTo = SetHPTo,
				PreventOnlyIfLethal = PreventOnlyIfLethal,
				InfuseIfNotStrong = InfuseIfNotStrong,
				CanTargetInvisible = CanTargetInvisible,
				NotApplyEnemy = NotApplyEnemy,
				CanUndo = CanUndo,
				CanSkip = CanSkip,
				TreatAsNonSubAbility = TreatAsNonSubAbility,
				ReplaceModifiers = ReplaceModifiers?.ToList(),
				ReplaceWithModifier = ReplaceWithModifier,
				ReplaceWithModifiers = ReplaceWithModifiers?.ToList(),
				ReplaceNegativeModifiers = ReplaceNegativeModifiers,
				ReplaceNegativeConditions = ReplaceNegativeConditions?.ToList(),
				ReplacePositiveConditions = ReplacePositiveConditions?.ToList(),
				ReplaceWithNegativeConditions = ReplaceWithNegativeConditions?.ToList(),
				ReplaceWithPositiveConditions = ReplaceWithPositiveConditions?.ToList(),
				IgnoreMonsterColumnLayout = IgnoreMonsterColumnLayout,
				IgnoreMCLAdjustY = IgnoreMCLAdjustY,
				GapAbove = GapAbove,
				GapBelow = GapBelow,
				IgnoreImmobilize = IgnoreImmobilize,
				IgnoreStun = IgnoreStun,
				HasCondition = HasCondition,
				AreaEffectSymmetrical = AreaEffectSymmetrical,
				RemoveCount = RemoveCount,
				PerformXTimesBasedOn = PerformXTimesBasedOn,
				ConsiderMultiTargetForEnhancements = ConsiderMultiTargetForEnhancements,
				OnAttackInfuse = OnAttackInfuse?.ToList(),
				AlreadyHasConditionDamageInstead = AlreadyHasConditionDamageInstead,
				UseParentTargets = UseParentTargets,
				AlsoTargetSelf = AlsoTargetSelf,
				AlsoTargetAdjacent = AlsoTargetAdjacent,
				ConditionsToRemoveFirst = ConditionsToRemoveFirst?.ToList(),
				ShowPlusX = ShowPlusX,
				TriggeredScenarioModifiers = TriggeredScenarioModifiers?.ToList(),
				TriggerScenarioModifierOnSelfOnly = TriggerScenarioModifierOnSelfOnly,
				BypassImmunity = BypassImmunity,
				InlineSubAbilityOnKilledTargetsOnly = InlineSubAbilityOnKilledTargetsOnly,
				MovePathIndexFilter = MovePathIndexFilter?.Copy(),
				NegativeAbilityIsOptional = NegativeAbilityIsOptional,
				AllowContinueForNullAbility = AllowContinueForNullAbility,
				NoGoldDrop = NoGoldDrop,
				UseOriginalActor = UseOriginalActor,
				ObstacleTypes = ObstacleTypes?.ToList(),
				TerrainTypes = TerrainTypes?.ToList(),
				AttackFurthestTarget = AttackFurthestTarget,
				StartIfParentHasHappened = StartIfParentHasHappened,
				MinRange = MinRange,
				NameKeyOverride = NameKeyOverride,
				PreventDamageSetTargetHealthTo = PreventDamageSetTargetHealthTo,
				CasterDamageToTakeOnPrevent = CasterDamageToTakeOnPrevent,
				IgnoreLOS = IgnoreLOS,
				XP = XP,
				IsCostAbility = IsCostAbility
			};
		}
	}

	[Flags]
	public enum EActiveBonusFlag
	{
		None = 0,
		ShieldMelee = 1,
		ShieldRanged = 2,
		ShieldAll = ShieldMelee | ShieldRanged,
		NotInRangeToInRangeMove = 4,
		InRangeToNotInRangeMove = 8,
		IgnoreFastEffects = 0x10,
		IgnoreSlowEffects = 0x20,
		WhenFast = 0x40,
		WhenSlow = 0x80,
		NotAffectedByIgnoreFastEffects = 0x100,
		NotAffectedByIgnoreSlowEffects = 0x200,
		MuddledTargetsDontRetaliate = 0x400,
		RepeatAbility = 0x800,
		TreatSlowAgilityAsFast = 0x2000,
		PlacesShadowToken = 0x4000,
		FrozenFistMechanic = 0x8000,
		EnhancedEffects = 0x10000,
		DrifterShockWaveAttack = 0x20000,
		ManualRestrict = 0x40000,
		Tide = 0x80000,
		CrashingTideRest = 0x100000,
		DontCancelTidesOnRest = 0x200000,
		ForbidLongRest = 0x400000,
		PreRestBonus = 0x800000,
		EnhancedMoveEffects = 0x1000000,
		All = -1
	}

	[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
	public class ActiveBonusData
	{
		[JsonProperty]
		public EActiveBonusFlag Flags;

		[JsonProperty]
		public List<CActiveBonus.EActiveBonusBehaviourType> Behaviours = new List<CActiveBonus.EActiveBonusBehaviourType>();

		[JsonProperty]
		public CActiveBonus.EActiveBonusDurationType Duration;

		[JsonProperty]
		private CActiveBonus.EActiveBonusRestrictionType m_Restriction;

		[JsonProperty]
		public List<(string, TValue)> Vars;

		[JsonProperty]
		public List<(string, TValue)> MiscVars;

		[JsonProperty]
		public CAbilityRequirements Requirements;

		[JsonProperty]
		public List<int> Tracker = new List<int>();

		[JsonProperty]
		public CAbility.EAttackType AttackType;

		[JsonProperty]
		public List<CAbility.EAbilityType> ValidAbilityTypes = new List<CAbility.EAbilityType>();

		[JsonProperty]
		public List<CAbilityFilter.EFilterTargetType> ValidTargetTypes = new List<CAbilityFilter.EFilterTargetType>();

		[JsonProperty]
		public bool ValidTargetTypesExclusive;

		[JsonProperty]
		public bool InvertValidity;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> ValidPositiveConditionTypes = new List<CCondition.EPositiveCondition>();

		[JsonProperty]
		public List<CCondition.ENegativeCondition> ValidNegativeConditionTypes = new List<CCondition.ENegativeCondition>();

		[JsonProperty]
		public int ProcXP;

		[JsonProperty]
		public bool StrengthIsScalar;

		[JsonProperty]
		public bool AbilityStrengthIsScalar;

		[JsonProperty]
		public bool AlwaysBuff;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public CAbilityFilterContainer Filter = new CAbilityFilterContainer();

		[JsonProperty]
		public bool IsAura;

		[JsonProperty]
		public bool DisplayAuraOnCaster;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
		public CAbilityFilterContainer AuraFilter = new CAbilityFilterContainer();

		[JsonProperty]
		public CAbility.EAbilityTargeting AuraTargeting = CAbility.EAbilityTargeting.Range;

		[JsonProperty]
		public CAbility AbilityData;

		[JsonProperty]
		public List<ElementInfusionBoardManager.EElement> Consuming = new List<ElementInfusionBoardManager.EElement>();

		[JsonProperty]
		public CAbility CostAbility;

		[JsonProperty]
		public bool IsToggleBonus;

		[JsonProperty]
		public bool ToggleIsOptional = true;

		[JsonProperty]
		public bool RestrictPerActor;

		[JsonProperty]
		public bool OverrideAsSong;

		[JsonProperty]
		public bool IsSingleTargetBonus;

		[JsonProperty]
		public int TargetCount;

		[JsonProperty]
		public List<CAbilityOverride> ActiveBonusAbilityOverrides = new List<CAbilityOverride>();

		[JsonProperty]
		public string ActiveBonusAnimOverload = string.Empty;

		[JsonProperty]
		public bool UseTriggerAbilityAsParent;

		[JsonProperty]
		public bool GiveAbilityCardToActor;

		[JsonProperty]
		public bool StartsRestricted;

		[JsonProperty]
		public bool EntireAction;

		[JsonProperty]
		public bool SetFilterToCaster;

		[JsonProperty]
		public bool OriginalTargetType;

		[JsonProperty]
		public Dictionary<string, int> RequiredResources = new Dictionary<string, int>();

		[JsonProperty]
		public bool ConsumeResources;

		[JsonProperty]
		public (string, string) ListLayoutOverride = (null, null);

		[JsonProperty]
		public CActiveBonus.EAuraTriggerAbilityAnimType AuraAnimTriggerAbilityType = CActiveBonus.EAuraTriggerAbilityAnimType.AnimateAuraReceiver;

		[JsonProperty]
		public bool TriggerOnCaster;

		[JsonProperty]
		public bool CannotCancel;

		[JsonProperty]
		public bool EndAllActiveBonusesOnBaseCardSimultaneously;

		[JsonProperty]
		public List<int> CancelActiveBonusesOnBaseCardIDs;

		[JsonProperty]
		public bool Hidden;

		[JsonProperty]
		public bool HiddenWhenAura;

		[JsonProperty]
		public bool HiddenInLog;

		[JsonProperty]
		public List<string> RemoveAllInstancesOfResourcesOnFinish;

		[JsonProperty]
		public List<string> ArbitraryParams;

		[JsonProperty]
		public string GroupID;

		[JsonProperty]
		public bool FullCopy;

		[JsonProperty]
		public bool NeedsDamage;

		[JsonProperty]
		public bool TriggerAbilitiesImmediately;

		[JsonProperty]
		public bool OnlyWorkAfterFinishedTracker;

		public CActiveBonus.EActiveBonusRestrictionType Restriction
		{
			get
			{
				if (GetMiscVar("NoRestriction").GetBool())
				{
					return CActiveBonus.EActiveBonusRestrictionType.None;
				}
				return m_Restriction;
			}
			set
			{
				m_Restriction = value;
			}
		}

		[JsonProperty]
		public List<(string, List<(string, TValue)>)> PerformVars { get; set; }

		[JsonIgnore]
		public CActiveBonus.EActiveBonusBehaviourType Behaviour
		{
			get
			{
				if (Behaviours.Count > 0)
				{
					return Behaviours[0];
				}
				return CActiveBonus.EActiveBonusBehaviourType.None;
			}
			set
			{
				Behaviours = new List<CActiveBonus.EActiveBonusBehaviourType> { value };
			}
		}

		public ActiveBonusData Copy()
		{
			return new ActiveBonusData
			{
				Flags = Flags,
				Behaviours = Behaviours.ToList(),
				Duration = Duration,
				m_Restriction = m_Restriction,
				Vars = Vars?.ToList(),
				MiscVars = MiscVars?.ToList(),
				PerformVars = PerformVars?.ToList(),
				Requirements = Requirements?.Copy(),
				Tracker = Tracker.ToList(),
				AttackType = AttackType,
				ValidAbilityTypes = ValidAbilityTypes.ToList(),
				ValidTargetTypes = ValidTargetTypes.ToList(),
				ValidTargetTypesExclusive = ValidTargetTypesExclusive,
				InvertValidity = InvertValidity,
				ValidPositiveConditionTypes = ValidPositiveConditionTypes.ToList(),
				ValidNegativeConditionTypes = ValidNegativeConditionTypes.ToList(),
				ProcXP = ProcXP,
				StrengthIsScalar = StrengthIsScalar,
				AbilityStrengthIsScalar = AbilityStrengthIsScalar,
				AlwaysBuff = AlwaysBuff,
				Filter = Filter.Copy(),
				IsAura = IsAura,
				DisplayAuraOnCaster = DisplayAuraOnCaster,
				AuraFilter = AuraFilter.Copy(),
				AuraTargeting = AuraTargeting,
				AbilityData = ((AbilityData != null) ? CAbility.CopyAbility(AbilityData, generateNewID: false, fullCopy: true) : null),
				Consuming = Consuming.ToList(),
				CostAbility = ((CostAbility != null) ? CAbility.CopyAbility(CostAbility, generateNewID: false, fullCopy: true) : null),
				IsToggleBonus = IsToggleBonus,
				ToggleIsOptional = ToggleIsOptional,
				RestrictPerActor = RestrictPerActor,
				OverrideAsSong = OverrideAsSong,
				IsSingleTargetBonus = IsSingleTargetBonus,
				TargetCount = TargetCount,
				ActiveBonusAbilityOverrides = ActiveBonusAbilityOverrides.ToList(),
				ActiveBonusAnimOverload = ActiveBonusAnimOverload,
				UseTriggerAbilityAsParent = UseTriggerAbilityAsParent,
				GiveAbilityCardToActor = GiveAbilityCardToActor,
				StartsRestricted = StartsRestricted,
				EntireAction = EntireAction,
				OriginalTargetType = OriginalTargetType,
				SetFilterToCaster = SetFilterToCaster,
				RequiredResources = RequiredResources.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value),
				ConsumeResources = ConsumeResources,
				ListLayoutOverride = ListLayoutOverride,
				AuraAnimTriggerAbilityType = AuraAnimTriggerAbilityType,
				TriggerOnCaster = TriggerOnCaster,
				CannotCancel = CannotCancel,
				EndAllActiveBonusesOnBaseCardSimultaneously = EndAllActiveBonusesOnBaseCardSimultaneously,
				CancelActiveBonusesOnBaseCardIDs = CancelActiveBonusesOnBaseCardIDs?.ToList(),
				Hidden = Hidden,
				HiddenWhenAura = HiddenWhenAura,
				RemoveAllInstancesOfResourcesOnFinish = RemoveAllInstancesOfResourcesOnFinish?.ToList(),
				ArbitraryParams = ArbitraryParams?.ToList(),
				GroupID = GroupID,
				FullCopy = FullCopy,
				NeedsDamage = NeedsDamage,
				TriggerAbilitiesImmediately = TriggerAbilitiesImmediately,
				HiddenInLog = HiddenInLog,
				OnlyWorkAfterFinishedTracker = OnlyWorkAfterFinishedTracker
			};
		}

		public bool HasBehaviour()
		{
			return Behaviour != CActiveBonus.EActiveBonusBehaviourType.None;
		}

		public bool HasBehaviour(params CActiveBonus.EActiveBonusBehaviourType[] types)
		{
			return Behaviours.Any((CActiveBonus.EActiveBonusBehaviourType x) => types.Contains(x));
		}

		public bool HasGenericBehaviour()
		{
			foreach (CActiveBonus.EActiveBonusBehaviourType behaviour in Behaviours)
			{
				if (behaviour >= CActiveBonus.EActiveBonusBehaviourType.Generic_First && behaviour <= CActiveBonus.EActiveBonusBehaviourType.Generic_Last)
				{
					return true;
				}
			}
			return false;
		}

		public TValue GetVar(string key)
		{
			if (Vars == null)
			{
				return TValue.Unknown;
			}
			(string, TValue) tuple = Vars.Find(((string, TValue) x) => x.Item1 == key);
			(string, TValue) tuple2 = tuple;
			if (tuple2.Item1 == null && tuple2.Item2 == default(TValue))
			{
				return TValue.Unknown;
			}
			return tuple.Item2;
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

		public void SetVar(string key, TValue value)
		{
			if (Vars == null)
			{
				Vars = new List<(string, TValue)>();
			}
			RemoveVar(key);
			Vars.Add((key, value));
		}

		public void RemoveMiscVar(string key)
		{
			if (MiscVars != null)
			{
				MiscVars.RemoveAll(((string, TValue) x) => x.Item1 == key);
			}
		}

		public void RemoveVar(string key)
		{
			if (Vars != null)
			{
				Vars.RemoveAll(((string, TValue) x) => x.Item1 == key);
			}
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class TrapData
	{
		[JsonProperty]
		public string TrapName;

		[JsonProperty]
		public int Damage;

		[JsonProperty]
		public int Heal;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> Conditions = new List<CCondition.ENegativeCondition>();

		[JsonProperty]
		public List<CCondition.EPositiveCondition> PositiveConditions = new List<CCondition.EPositiveCondition>();

		[JsonProperty]
		public int AdjacentRange;

		[JsonProperty]
		public int AdjacentDamage;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> AdjacentConditions = new List<CCondition.ENegativeCondition>();

		[JsonProperty]
		public CAbilityFilterContainer AdjacentFilter;

		[JsonProperty]
		public int TriggeredXP;

		[JsonProperty]
		public CAbilityFilter.EFilterTile PlacementTileFilter = CAbilityFilter.EFilterTile.EmptyHex;

		public TrapData Copy()
		{
			return new TrapData
			{
				TrapName = TrapName,
				Damage = Damage,
				Heal = Heal,
				Conditions = Conditions.ToList(),
				PositiveConditions = PositiveConditions.ToList(),
				AdjacentRange = AdjacentRange,
				AdjacentDamage = AdjacentDamage,
				AdjacentConditions = AdjacentConditions.ToList(),
				AdjacentFilter = AdjacentFilter,
				TriggeredXP = TriggeredXP,
				PlacementTileFilter = PlacementTileFilter
			};
		}
	}

	[Serializable]
	[JsonObject(MemberSerialization.OptIn)]
	public class StatIsBasedOnXData : ISerializable
	{
		[JsonProperty]
		public CAbility.EAbilityStatType AbilityStatType;

		[JsonProperty]
		public EMonsterBaseStats BaseStatType;

		[JsonProperty]
		public CAbility.EStatIsBasedOnXType BasedOn;

		[JsonProperty]
		public CAbility.EStatIsBasedOnXRoundingType RoundingType = CAbility.EStatIsBasedOnXRoundingType.RoundOff;

		[JsonProperty]
		public bool AddTo;

		[JsonProperty]
		public float Multiplier = 1f;

		[JsonProperty]
		public float SecondVariable = 1f;

		[JsonProperty]
		public CAbilityFilterContainer Filter;

		[JsonProperty]
		public int AddedStat;

		[JsonProperty]
		public bool HasBeenAdded;

		[JsonProperty]
		public bool IncludeUnrevealed;

		[JsonProperty]
		public int MinValue = int.MinValue;

		[JsonProperty]
		public int MaxValue = int.MaxValue;

		[JsonProperty]
		public List<string> CheckGUIDs;

		[JsonProperty]
		public List<(string, TValue)> Vars;

		[JsonProperty]
		public int MinLValue;

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AbilityStatType", AbilityStatType);
			info.AddValue("MinLValue", MinLValue);
			info.AddValue("BaseStatType", BaseStatType);
			info.AddValue("BasedOn", BasedOn);
			info.AddValue("RoundingType", RoundingType);
			info.AddValue("AddTo", AddTo);
			info.AddValue("Multiplier", Multiplier);
			info.AddValue("SecondVariable", SecondVariable);
			info.AddValue("Filter", Filter);
			info.AddValue("AddedStat", AddedStat);
			info.AddValue("IncludeUnrevealed", IncludeUnrevealed);
			info.AddValue("MinValue", MinValue);
			info.AddValue("MaxValue", MaxValue);
			info.AddValue("CheckGUIDs", CheckGUIDs);
			info.AddValue("Vars", Vars);
			info.AddValue("HasBeenAdded", HasBeenAdded);
		}

		public StatIsBasedOnXData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "AbilityStatType":
						AbilityStatType = (CAbility.EAbilityStatType)info.GetValue("AbilityStatType", typeof(CAbility.EAbilityStatType));
						break;
					case "BaseStatType":
						BaseStatType = (EMonsterBaseStats)info.GetValue("BaseStatType", typeof(EMonsterBaseStats));
						break;
					case "BasedOn":
						BasedOn = (CAbility.EStatIsBasedOnXType)info.GetValue("BasedOn", typeof(CAbility.EStatIsBasedOnXType));
						break;
					case "RoundingType":
						RoundingType = (CAbility.EStatIsBasedOnXRoundingType)info.GetValue("RoundingType", typeof(CAbility.EStatIsBasedOnXRoundingType));
						break;
					case "AddTo":
						AddTo = info.GetBoolean("AddTo");
						break;
					case "HasBeenAdded":
						HasBeenAdded = info.GetBoolean("HasBeenAdded");
						break;
					case "MinLValue":
						MinLValue = info.GetInt32("MinLValue");
						break;
					case "Multiplier":
						Multiplier = info.GetSingle("Multiplier");
						break;
					case "SecondVariable":
						SecondVariable = info.GetSingle("SecondVariable");
						break;
					case "Filter":
						Filter = (CAbilityFilterContainer)info.GetValue("Filter", typeof(CAbilityFilterContainer));
						break;
					case "AddedStat":
						AddedStat = info.GetInt32("AddedStat");
						break;
					case "IncludeUnrevealed":
						IncludeUnrevealed = info.GetBoolean("IncludeUnrevealed");
						break;
					case "MinValue":
						MinValue = info.GetInt32("MinValue");
						break;
					case "MaxValue":
						MaxValue = info.GetInt32("MaxValue");
						break;
					case "CheckGUIDs":
						CheckGUIDs = (List<string>)info.GetValue("CheckGUIDs", typeof(List<string>));
						break;
					case "Vars":
						Vars = (List<(string, TValue)>)info.GetValue("Vars", typeof(List<(string, TValue)>));
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize StatIsBasedOnXData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public StatIsBasedOnXData()
		{
		}

		public StatIsBasedOnXData Copy()
		{
			return new StatIsBasedOnXData
			{
				MinLValue = MinLValue,
				AbilityStatType = AbilityStatType,
				BaseStatType = BaseStatType,
				BasedOn = BasedOn,
				RoundingType = RoundingType,
				AddTo = AddTo,
				Multiplier = Multiplier,
				SecondVariable = SecondVariable,
				Filter = Filter?.Copy(),
				AddedStat = AddedStat,
				IncludeUnrevealed = IncludeUnrevealed,
				MinValue = MinValue,
				MaxValue = MaxValue,
				CheckGUIDs = CheckGUIDs?.ToList(),
				Vars = Vars,
				HasBeenAdded = HasBeenAdded
			};
		}

		public static string GetExpressionStringForType(CAbility.EStatIsBasedOnXType type)
		{
			return type switch
			{
				CAbility.EStatIsBasedOnXType.None => "None", 
				CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount => "X * C", 
				CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount => "X + C", 
				CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel => "X + (C * L)", 
				CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX => "(X + L) * C", 
				CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX => "Y + (X + L) * C", 
				CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel => "X + (Y * L)", 
				CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel => "X + C + L", 
				CAbility.EStatIsBasedOnXType.XPlusLevel => "X + L", 
				CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters => "L + (X * C)", 
				CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY => "X + (L * C / Y)", 
				CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY => "(X * C) + L - Y", 
				CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound => "((X * C) + L - Y) * R", 
				CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX => "C + (L / X)", 
				CAbility.EStatIsBasedOnXType.CharactersPlusLevelTimesX => "(C + L) * X", 
				CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY => "X + C + (L * Y)", 
				CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY => "(X * C) - Y", 
				CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY => "(X + L) * (C / Y)", 
				CAbility.EStatIsBasedOnXType.LevelTimesXPlusY => "(L * X) + Y", 
				CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY => "((C + X) / 2) * (L + Y)", 
				CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY => "(L + X) * Y * C", 
				CAbility.EStatIsBasedOnXType.CharactersTimesXPlusLevelTimesY => "(L + (X * C))*  Y", 
				CAbility.EStatIsBasedOnXType.LevelPlusYTimesX => "(L + Y) * X", 
				CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel => "(X + Y * C) * (2 + L)", 
				CAbility.EStatIsBasedOnXType.FH22ShipFormulaCharactersLevel => "FH22 = 2 * (2 + L) * (3 * C − 1)", 
				CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters => "((L * X) + Y) * C", 
				CAbility.EStatIsBasedOnXType.Vars => "Vars", 
				_ => "UNSUPPORTED TYPE: " + type, 
			};
		}
	}

	private const string SpecialBaseStatString = "SpecialBaseStat";

	public static bool GetAction(Mapping actionMapping, int cardID, string filename, DiscardType discardType, out CAction action, out List<AbilityConsume> consumes, bool isMonster = false)
	{
		action = null;
		bool flag = true;
		List<CAbility> list = new List<CAbility>();
		List<CActionAugmentation> list2 = new List<CActionAugmentation>();
		List<CAbility> list3 = new List<CAbility>();
		consumes = new List<AbilityConsume>();
		List<ElementInfusionBoardManager.EElement> list4 = new List<ElementInfusionBoardManager.EElement>();
		int actionXP = 0;
		foreach (MappingEntry entry in actionMapping.Entries)
		{
			string text = entry.Key.ToString();
			switch (text)
			{
			case "Abilities":
			{
				if (CardProcessingShared.GetAbilities(entry, cardID, isMonster, filename, out var abilities))
				{
					list.AddRange(abilities);
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Mode":
			{
				if (CardProcessingShared.GetAbilities(entry, cardID, isMonster, filename, out var abilities2))
				{
					foreach (CAbility item in abilities2)
					{
						if (item.MiscAbilityData == null)
						{
							item.MiscAbilityData = new MiscAbilityData();
						}
						item.MiscAbilityData.IgnoreStun = true;
					}
					list3.AddRange(abilities2);
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Consumes":
			{
				if (YMLShared.GetMapping(entry, filename, out var mapping))
				{
					foreach (MappingEntry entry2 in mapping.Entries)
					{
						if (YMLShared.GetMapping(entry2, filename, out var mapping2))
						{
							if (AbilityConsume.CreateAbilityConsume(mapping2, entry2.Key.ToString(), cardID, filename, isMonster, list, out var abilityConsume))
							{
								consumes.Add(abilityConsume);
								list2.Add(abilityConsume.ConsumeData);
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
					}
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Infuse":
			{
				if (CardProcessingShared.GetElements(entry.Value, "Infuse", filename, out var elements))
				{
					list4.AddRange(elements);
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "XP":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "XP", filename, out var value))
				{
					actionXP = value;
				}
				else
				{
					flag = false;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry under Data: " + text + ". File:\n" + filename);
				flag = false;
				break;
			}
		}
		if (list.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Action has no abilities.  Filename: " + filename);
			flag = false;
		}
		if (flag)
		{
			action = new CAction(list4, list2, list, list3, actionXP, CardProcessingShared.GetCardPile(discardType));
		}
		return flag;
	}

	public static bool ProcessAbilityEntry(string filename, Mapping abilityMapping, string name, int cardID, bool isMonster, out CAbility ability, bool isSubAbility = false, bool isConsumeAbility = false, CAbilityFilterContainer parentFilter = null, int? parentStrength = null, int? parentRange = null, int? parentNumberOfTargets = null, string parentAreaEffectYMLString = null, string parentAreaEffectLayoutOverrideYMLString = null, string parentAreaEffectFlipAxis = null, CAbility.EAbilityTargeting? parentTargeting = null, CAreaEffect parentAreaEffect = null, bool? parentIsTargetedAbility = null, bool isMergedAbility = false)
	{
		ability = null;
		try
		{
			CAbility.EAbilityType? abilityType = null;
			string parentName = string.Empty;
			string animationOverload = string.Empty;
			string animationOverload2 = string.Empty;
			List<CAbility> subAbilities = new List<CAbility>();
			bool? allTargetsOnMovePath = false;
			bool? allTargetsOnMovePathSameStartAndEnd = false;
			bool? allTargetsOnAttackPath = false;
			bool? allTargetsInRangeAlongPath = false;
			XpPerTargetData xpPerTargetData = new XpPerTargetData();
			ActiveBonusData activeBonusData = new ActiveBonusData();
			ActiveBonusLayout activeBonusYML = null;
			CAbility addActiveBonusAbility = null;
			float? spawnDelay = 0f;
			List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
			CAbilityRequirements startAbilityRequirements = new CAbilityRequirements();
			int? abilityXP = 0;
			MiscAbilityData miscAbilityData = new MiscAbilityData();
			bool? skipAnim = false;
			bool? isInlineSubAbility = false;
			bool? targetActorWithTrapEffects = false;
			int? targetActorWithTrapEffectRange = 0;
			CAugment augment = null;
			CSong song = null;
			CDoom doom = null;
			List<CAbility> mergedAbilities = null;
			List<StatIsBasedOnXData> statIsBasedOnXEntries = null;
			bool? useSpecialBaseStat = false;
			Dictionary<string, int> resourcesToAddOnAbilityEnd = null;
			Dictionary<string, int> resourcesToTakeFromTargets = null;
			Dictionary<string, int> resourcesToGiveToTargets = null;
			CAbilityFilterContainer filter = (filter = parentFilter?.Copy());
			int? strength = parentStrength ?? new int?(0);
			CAbility.ECardAction cardAction = CAbility.ECardAction.None;
			int? range = parentRange ?? new int?((!isMonster) ? 1 : 0);
			int? numberOfTargets = parentNumberOfTargets ?? new int?(0);
			bool numberOfTargetsSet = false;
			int? tokens = 0;
			CAbility.EAbilityFlag? flags = CAbility.EAbilityFlag.None;
			string areaEffectYMLString = ((parentAreaEffectYMLString != null) ? parentAreaEffectYMLString : string.Empty);
			string areaEffectLayoutOverrideYMLString = ((parentAreaEffectLayoutOverrideYMLString != null) ? parentAreaEffectLayoutOverrideYMLString : string.Empty);
			string areaEffectFlipAxis = ((parentAreaEffectFlipAxis != null) ? parentAreaEffectFlipAxis : string.Empty);
			CAbility.EAbilityTargeting? targeting = parentTargeting ?? new CAbility.EAbilityTargeting?(CAbility.EAbilityTargeting.Range);
			List<(string, TValue)> targetingVars = null;
			List<(string, TValue)> vars = null;
			List<(string, List<(string, TValue)>)> performVars = null;
			List<(string, TValue)> miscVars = null;
			CAreaEffect areaEffect = parentAreaEffect?.Copy();
			bool? isTargetedAbility = !parentIsTargetedAbility.HasValue || parentIsTargetedAbility.Value;
			bool? jump = false;
			bool? fly = false;
			bool? ignoreDifficultTerrain = false;
			bool? ignoreHazardousTerrain = false;
			bool? ignoreBlockedTileMoveCost = false;
			bool? carryOtherActorsOnHex = false;
			CAbilityMove.EMoveRestrictionType? moveRestrictionType = CAbilityMove.EMoveRestrictionType.None;
			CAbilityPush.EAdditionalPushEffect? additionalPushEffect = CAbilityPush.EAdditionalPushEffect.None;
			int? additionalPushEffectDamage = 0;
			int? additionalPushEffectXP = 0;
			CAIFocusOverrideDetails aiFocusOverride = null;
			CAbilityPull.EPullType? pullType = CAbilityPull.EPullType.PullTowardsActor;
			CAbilityLoot.LootData lootData = null;
			string propName = string.Empty;
			int? moveObstacleRange = 0;
			string moveObstacleAnimOverload = string.Empty;
			TrapData trapData = null;
			int? pierce = 0;
			bool? multiPassAttack = !isSubAbility && !isMergedAbility;
			bool? exploitVulnerability = false;
			bool? chainAttack = false;
			int? chainAttackRange = 0;
			int? chainAttackDamageReduction = 0;
			int? damageSelfBeforeAttack = 0;
			miscAbilityData.TargetOneEnemyWithAllAttacks = false;
			List<CAttackEffect> attackEffects = new List<CAttackEffect>();
			CAbilityHeal.HealAbilityData healAbilityData = new CAbilityHeal.HealAbilityData();
			int? retaliateRange = 1;
			List<string> summons = null;
			bool? attackSourcesOnly = false;
			List<ElementInfusionBoardManager.EElement> elementsToInfuse = new List<ElementInfusionBoardManager.EElement>();
			List<CCondition.ENegativeCondition> elementalConditions = null;
			bool? showElementPicker = false;
			CAbilityControlActor.ControlActorAbilityData controlActorAbilityData = null;
			CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceAbilityData = null;
			List<CItem.EItemSlot> slotsToRefresh = new List<CItem.EItemSlot>();
			List<CItem.EItemSlotState> slotStatesToRefresh = new List<CItem.EItemSlotState>();
			List<CItem.EUsageType> slotUsagesToRefresh = new List<CItem.EUsageType>();
			CAbility.EAttackType? overrideAugmentAttackType = CAbility.EAttackType.None;
			List<CAbility> chooseAbilities = new List<CAbility>();
			List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter = new List<CAbility.EAbilityType>();
			bool? useParentTargeting = false;
			CAbility forgoTopActionAbility = null;
			CAbility forgoBottomActionAbility = null;
			CAbilityExtraTurn.EExtraTurnType? extraTurnType = CAbilityExtraTurn.EExtraTurnType.None;
			string actionMonsterClassID = string.Empty;
			CAbilityFilterContainer swapAbilityFirstTargetFilter = null;
			CAbilityFilterContainer swapAbilitySecondTargetFilter = null;
			List<int> supplyCardsToGive = new List<int>();
			CAbilityTeleport.TeleportData teleportData = null;
			List<CAbility.EAbilityType> immunityToAbilityTypes = new List<CAbility.EAbilityType>();
			List<CAbility.EAttackType> immuneToAttackTypes = new List<CAbility.EAttackType>();
			bool? immuneToSelfDamage = false;
			List<string> addOrRemoveModifiers = new List<string>();
			CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = new CAbilityDestroyObstacle.DestroyObstacleData();
			ECharacter changeCharacterModel = ECharacter.None;
			CAbilityFilter.EFilterTile tileFilter = CAbilityFilter.EFilterTile.None;
			CAbilityDisableCardAction.DisableCardActionData disableCardActionData = new CAbilityDisableCardAction.DisableCardActionData();
			List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
			List<AbilityConsume> consumes = new List<AbilityConsume>();
			bool? addAttackBaseStat = false;
			bool? strengthIsBase = false;
			bool? rangeIsBase = false;
			bool? targetIsBase = false;
			string abilityText = string.Empty;
			bool? abilityTextOnly = false;
			bool? abilityNoLayoutSpace = false;
			bool? showRange = true;
			bool? showTarget = true;
			bool? showArea = true;
			bool? onDeath = false;
			bool? rangeAtLeastOne = false;
			bool? targetAtLeastOne = false;
			bool? removeConditionsOverride = false;
			List<CCondition.EPositiveCondition> positiveConditions = new List<CCondition.EPositiveCondition>();
			List<CCondition.ENegativeCondition> negativeConditions = new List<CCondition.ENegativeCondition>();
			int? conditionDuration = 1;
			EConditionDecTrigger? conditionDecTrigger = EConditionDecTrigger.Turns;
			string previewEffectId = null;
			string previewEffectText = null;
			string helpBoxTooltipLocKey = null;
			List<CAbility.EAbilityType> extraTurnAbilityTypeFilter = new List<CAbility.EAbilityType>();
			int? maxExtraTurnAbilityLevel = int.MaxValue;
			bool? toDiscardPile = false;
			bool? removeWhenDone = false;
			bool? canExcludeCards = false;
			MappingEntry mappingEntry = abilityMapping.Entries.SingleOrDefault((MappingEntry x) => x.Key.ToString() == "ClearParentProperties");
			if (mappingEntry != null && (YMLShared.GetBoolPropertyValue(mappingEntry.Value, "Ability/ClearParentProperties", filename, out var value) & value))
			{
				filter = null;
				areaEffect = null;
				areaEffectYMLString = null;
				areaEffectLayoutOverrideYMLString = null;
			}
			if (!ParseAbilityProperties(abilityMapping, name, cardID, isMonster, isAbilityOverride: false, ref isSubAbility, ref parentName, ref abilityType, ref propName, ref trapData, ref abilityEnhancements, ref strength, ref cardAction, ref useSpecialBaseStat, ref range, ref numberOfTargets, ref numberOfTargetsSet, ref tokens, ref flags, ref filter, ref animationOverload, ref animationOverload2, ref subAbilities, ref areaEffect, ref attackSourcesOnly, ref jump, ref fly, ref ignoreDifficultTerrain, ref ignoreHazardousTerrain, ref ignoreBlockedTileMoveCost, ref aiFocusOverride, ref carryOtherActorsOnHex, ref moveRestrictionType, ref activeBonusData, ref activeBonusYML, ref areaEffectYMLString, ref areaEffectLayoutOverrideYMLString, ref areaEffectFlipAxis, ref positiveConditions, ref negativeConditions, ref multiPassAttack, ref chainAttack, ref chainAttackRange, ref chainAttackDamageReduction, ref damageSelfBeforeAttack, ref addAttackBaseStat, ref strengthIsBase, ref rangeIsBase, ref targetIsBase, ref rangeAtLeastOne, ref targetAtLeastOne, ref removeConditionsOverride, ref abilityText, ref abilityTextOnly, ref abilityNoLayoutSpace, ref showRange, ref showTarget, ref showArea, ref onDeath, ref allTargetsOnMovePath, ref allTargetsOnMovePathSameStartAndEnd, ref allTargetsOnAttackPath, ref allTargetsInRangeAlongPath, ref summons, ref xpPerTargetData, ref targeting, ref targetingVars, ref vars, ref performVars, ref elementsToInfuse, ref consumes, ref elementalConditions, ref showElementPicker, ref pierce, ref retaliateRange, ref isTargetedAbility, ref spawnDelay, ref pullType, ref additionalPushEffect, ref additionalPushEffectDamage, ref additionalPushEffectXP, ref lootData, ref conditionalOverrides, ref startAbilityRequirements, ref abilityXP, ref miscAbilityData, ref skipAnim, ref conditionDuration, ref conditionDecTrigger, ref isInlineSubAbility, ref augment, ref song, ref doom, ref attackEffects, ref controlActorAbilityData, ref changeAllegianceAbilityData, ref mergedAbilities, ref targetActorWithTrapEffects, ref targetActorWithTrapEffectRange, ref statIsBasedOnXEntries, ref moveObstacleRange, ref moveObstacleAnimOverload, ref slotsToRefresh, ref slotStatesToRefresh, ref slotUsagesToRefresh, ref addActiveBonusAbility, ref overrideAugmentAttackType, ref chooseAbilities, ref recoverCardsAbilityTypeFilter, ref useParentTargeting, ref forgoTopActionAbility, ref forgoBottomActionAbility, ref extraTurnType, ref actionMonsterClassID, ref swapAbilityFirstTargetFilter, ref swapAbilitySecondTargetFilter, ref supplyCardsToGive, ref teleportData, ref immunityToAbilityTypes, ref immuneToAttackTypes, ref immuneToSelfDamage, ref addOrRemoveModifiers, ref healAbilityData, ref resourcesToAddOnAbilityEnd, ref resourcesToTakeFromTargets, ref resourcesToGiveToTargets, ref destroyObstacleData, ref changeCharacterModel, ref tileFilter, ref disableCardActionData, ref previewEffectId, ref previewEffectText, ref helpBoxTooltipLocKey, ref exploitVulnerability, ref extraTurnAbilityTypeFilter, ref maxExtraTurnAbilityLevel, ref toDiscardPile, ref removeWhenDone, ref canExcludeCards, ref miscVars, filename))
			{
				return false;
			}
			if (filter == null)
			{
				filter = CAbilityFilterContainer.CreateDefaultFilter();
			}
			bool flag = true;
			if (name.Length == 0)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Ability Name is blank.  The Ability name must be specified in the YML.  File:\n" + filename);
			}
			else if (!abilityType.HasValue)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Ability Type has not been set.  All Abilities must have a type.  File:\n" + filename);
			}
			else if (abilityType == CAbility.EAbilityType.Trap)
			{
				if (trapData == null)
				{
					flag = false;
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid YML for Trap ability.  No TrapData was specified.  File: " + filename);
				}
			}
			else if (abilityType == CAbility.EAbilityType.Summon && activeBonusData.Duration != CActiveBonus.EActiveBonusDurationType.Summon)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid YML for Summon ability.  ActiveBonusDuration must be 'Summon'");
			}
			else if (abilityType == CAbility.EAbilityType.AddDoom && doom == null)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid YML for Add Doom ability.  Doom cannot be null");
			}
			if (flag)
			{
				if (isMonster)
				{
					numberOfTargets = numberOfTargets;
				}
				else
				{
					numberOfTargets = (numberOfTargetsSet ? numberOfTargets : new int?(1));
				}
				CAbility.EAbilityType value2 = abilityType.Value;
				int value3 = strength.Value;
				bool value4 = useSpecialBaseStat.Value;
				int value5 = range.Value;
				int value6 = numberOfTargets.Value;
				int value7 = tokens.Value;
				CAbility.EAbilityFlag value8 = flags.Value;
				CAbilityFilterContainer abilityFilter = filter;
				string animationOverload3 = animationOverload;
				string animationOverload4 = animationOverload2;
				List<CAbility> subAbilities2 = subAbilities;
				CAreaEffect areaEffect2 = areaEffect;
				bool value9 = attackSourcesOnly.Value;
				bool jump2 = jump.HasValue && jump.Value;
				bool value10 = fly.Value;
				bool value11 = ignoreDifficultTerrain.Value;
				bool value12 = ignoreHazardousTerrain.Value;
				bool value13 = ignoreBlockedTileMoveCost.Value;
				bool value14 = carryOtherActorsOnHex.Value;
				CAIFocusOverrideDetails aiFocusOverride2 = aiFocusOverride;
				CAbilityMove.EMoveRestrictionType value15 = moveRestrictionType.Value;
				ActiveBonusData activeBonusData2 = activeBonusData;
				string areaEffectYMLString2 = areaEffectYMLString;
				string areaEffectLayoutOverrideYMLString2 = areaEffectLayoutOverrideYMLString;
				string areaEffectFlipAxis2 = areaEffectFlipAxis;
				List<CEnhancement> abilityEnhancements2 = abilityEnhancements;
				List<CCondition.EPositiveCondition> positiveConditions2 = positiveConditions;
				List<CCondition.ENegativeCondition> negativeConditions2 = negativeConditions;
				bool value16 = multiPassAttack.Value;
				bool value17 = chainAttack.Value;
				int value18 = chainAttackRange.Value;
				int value19 = chainAttackDamageReduction.Value;
				int value20 = damageSelfBeforeAttack.Value;
				bool value21 = addAttackBaseStat.Value;
				bool value22 = strengthIsBase.Value;
				bool value23 = rangeIsBase.Value;
				bool value24 = targetIsBase.Value;
				string text = abilityText;
				bool value25 = abilityTextOnly.Value;
				bool value26 = abilityNoLayoutSpace.Value;
				bool value27 = showRange.Value;
				bool value28 = showTarget.Value;
				bool value29 = showArea.Value;
				bool value30 = onDeath.Value;
				bool isConsumeAbility2 = isConsumeAbility;
				bool value31 = allTargetsOnMovePath.Value;
				bool value32 = allTargetsOnMovePathSameStartAndEnd.Value;
				bool value33 = allTargetsOnAttackPath.Value;
				bool value34 = allTargetsInRangeAlongPath.Value;
				List<string> summons2 = summons;
				XpPerTargetData xpPerTargetData2 = xpPerTargetData;
				CAbility.EAbilityTargeting value35 = targeting.Value;
				List<(string, TValue)> targetingVars2 = targetingVars;
				List<(string, TValue)> vars2 = vars;
				List<(string, List<(string, TValue)>)> performVars2 = performVars;
				List<ElementInfusionBoardManager.EElement> elementsToInfuse2 = elementsToInfuse;
				List<CCondition.ENegativeCondition> elementalConditions2 = elementalConditions;
				bool value36 = showElementPicker.Value;
				bool isMonster2 = isMonster;
				string propName2 = propName;
				TrapData trapData2 = trapData;
				bool isSubAbility2 = isSubAbility;
				bool value37 = isInlineSubAbility.Value;
				int value38 = pierce.Value;
				int value39 = retaliateRange.Value;
				bool value40 = isTargetedAbility.Value;
				float value41 = spawnDelay.Value;
				CAbilityPull.EPullType value42 = pullType.Value;
				CAbilityPush.EAdditionalPushEffect value43 = additionalPushEffect.Value;
				int value44 = additionalPushEffectDamage.Value;
				int value45 = additionalPushEffectXP.Value;
				CAbilityLoot.LootData lootData2 = lootData;
				List<CConditionalOverride> conditionalOverrides2 = conditionalOverrides;
				CAbilityRequirements startAbilityConditions = startAbilityRequirements;
				int value46 = abilityXP.Value;
				string parentName2 = parentName;
				MiscAbilityData miscAbilityData2 = miscAbilityData;
				bool value47 = skipAnim.Value;
				int value48 = conditionDuration.Value;
				EConditionDecTrigger value49 = conditionDecTrigger.Value;
				CAugment augment2 = augment;
				CSong song2 = song;
				CDoom doom2 = doom;
				List<CAttackEffect> attackEffects2 = attackEffects;
				CAbilityControlActor.ControlActorAbilityData controlActorData = controlActorAbilityData;
				CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = changeAllegianceAbilityData;
				bool flag2 = isMergedAbility;
				List<CAbility> list = mergedAbilities;
				bool value50 = targetActorWithTrapEffects.Value;
				int value51 = targetActorWithTrapEffectRange.Value;
				bool isMergedAbility2 = flag2;
				List<CAbility> mergedAbilities2 = list;
				List<StatIsBasedOnXData> statIsBasedOnXEntries2 = statIsBasedOnXEntries;
				int value52 = moveObstacleRange.Value;
				string moveObstacleAnimOverload2 = moveObstacleAnimOverload;
				List<CItem.EItemSlot> slotsToRefresh2 = slotsToRefresh;
				List<CItem.EItemSlotState> slotStatesToRefresh2 = slotStatesToRefresh;
				CAbility addActiveBonusAbility2 = addActiveBonusAbility;
				CAbility.EAttackType value53 = overrideAugmentAttackType.Value;
				List<CAbility> chooseAbilities2 = chooseAbilities;
				List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter2 = recoverCardsAbilityTypeFilter;
				CAbility.ECardAction cardAction2 = cardAction;
				CAbility forgoTopActionAbility2 = forgoTopActionAbility;
				CAbility forgoBottomActionAbility2 = forgoBottomActionAbility;
				CAbilityExtraTurn.EExtraTurnType value54 = extraTurnType.Value;
				string actionMonsterClassID2 = actionMonsterClassID;
				CAbilityFilterContainer swapAbilityFirstTargetFilter2 = swapAbilityFirstTargetFilter;
				CAbilityFilterContainer swapAbilitySecondTargetFilter2 = swapAbilitySecondTargetFilter;
				List<int> supplyCardsToGive2 = supplyCardsToGive;
				CAbilityTeleport.TeleportData teleportData2 = teleportData;
				List<CAbility.EAbilityType> immunityToAbilityTypes2 = immunityToAbilityTypes;
				List<CAbility.EAttackType> immuneToAttackTypes2 = immuneToAttackTypes;
				bool value55 = immuneToSelfDamage.Value;
				List<string> addOrRemoveModifiers2 = addOrRemoveModifiers;
				CAbilityHeal.HealAbilityData healData = healAbilityData;
				Dictionary<string, int> resourcesToAddOnAbilityEnd2 = resourcesToAddOnAbilityEnd;
				Dictionary<string, int> resourcesToGiveToTargets2 = resourcesToGiveToTargets;
				Dictionary<string, int> resourcesToTakeFromTargets2 = resourcesToTakeFromTargets;
				CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData2 = destroyObstacleData;
				ECharacter changeCharacterModel2 = changeCharacterModel;
				CAbilityFilter.EFilterTile tileFilter2 = tileFilter;
				CAbilityDisableCardAction.DisableCardActionData disableCardActionData2 = disableCardActionData;
				string previewEffectId2 = previewEffectId;
				string previewEffectText2 = previewEffectText;
				string helpBoxTooltipLocKey2 = helpBoxTooltipLocKey;
				bool value56 = exploitVulnerability.Value;
				List<AbilityConsume> consumes2 = consumes;
				bool immuneToSelfDamage2 = value55;
				bool showElementPicker2 = value36;
				List<CAbility.EAbilityType> extraTurnAbilityTypeFilter2 = extraTurnAbilityTypeFilter;
				int? maxExtraTurnAbilityLevel2 = maxExtraTurnAbilityLevel;
				bool value57 = useParentTargeting.Value;
				bool value58 = toDiscardPile.Value;
				bool value59 = removeWhenDone.Value;
				bool value60 = canExcludeCards.Value;
				List<(string, TValue)> miscVars2 = miscVars;
				ability = CAbility.CreateAbility(value2, value3, value4, value5, value6, value7, value8, abilityFilter, animationOverload3, animationOverload4, subAbilities2, areaEffect2, value9, jump2, value10, value11, value12, value13, value14, aiFocusOverride2, value15, activeBonusData2, null, areaEffectYMLString2, areaEffectLayoutOverrideYMLString2, areaEffectFlipAxis2, name, abilityEnhancements2, positiveConditions2, negativeConditions2, value16, value17, value18, value19, value20, value21, value22, value23, value24, text, value25, value26, value27, value28, value29, value30, isConsumeAbility2, value31, value32, value33, value34, summons2, xpPerTargetData2, value35, targetingVars2, vars2, performVars2, elementsToInfuse2, elementalConditions2, isMonster2, propName2, trapData2, isSubAbility2, value37, value38, value39, value40, value41, value42, value43, value44, value45, lootData2, conditionalOverrides2, startAbilityConditions, value46, parentName2, miscAbilityData2, value47, value48, value49, augment2, song2, doom2, controlActorData, changeAllegianceData, attackEffects2, value50, value51, isMergedAbility2, mergedAbilities2, statIsBasedOnXEntries2, value52, moveObstacleAnimOverload2, slotsToRefresh2, slotStatesToRefresh2, addActiveBonusAbility2, value53, chooseAbilities2, recoverCardsAbilityTypeFilter2, cardAction2, forgoTopActionAbility2, forgoBottomActionAbility2, value54, actionMonsterClassID2, swapAbilityFirstTargetFilter2, swapAbilitySecondTargetFilter2, supplyCardsToGive2, consumes2, teleportData2, immunityToAbilityTypes2, immuneToAttackTypes2, immuneToSelfDamage2, addOrRemoveModifiers2, healData, resourcesToAddOnAbilityEnd2, resourcesToTakeFromTargets2, resourcesToGiveToTargets2, destroyObstacleData2, changeCharacterModel2, tileFilter2, disableCardActionData2, isDefault: false, isItemAbility: false, showElementPicker2, isModifierAbility: false, null, previewEffectId2, previewEffectText2, null, value56, helpBoxTooltipLocKey2, extraTurnAbilityTypeFilter2, maxExtraTurnAbilityLevel2, value57, createInitialState: true, null, value58, value59, value60, miscVars2);
				ability.FileName = filename;
				ability.YMLPath = abilityMapping.Entry.Path;
				ability.CardID = cardID;
				lock (CSRLYML.AllAbilities)
				{
					CSRLYML.AllAbilities.Add(ability);
				}
				return true;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability entry " + name + ".  File:\n" + filename, ex);
		}
		return false;
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots, out int pierce, out int pierceEnhancements, bool ignoreAmount = false)
	{
		bool? jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		ElementInfusionBoardManager.EElement? infuse;
		EConditionDecTrigger conditionDecTrigger;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, out pierce, out pierceEnhancements, out conditionDecTrigger, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots, bool ignoreAmount = false)
	{
		bool? jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		ElementInfusionBoardManager.EElement? infuse;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out EConditionDecTrigger conditonDecTrigger, out List<EEnhancementMarkType> enhancementSlots, bool ignoreAmount = false)
	{
		bool? jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		ElementInfusionBoardManager.EElement? infuse;
		int pierce;
		int pierceEnhancements;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, out pierce, out pierceEnhancements, out conditonDecTrigger, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots, out bool? jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, out bool ignoreBlockedTileMoveCost, bool ignoreAmount = false)
	{
		ElementInfusionBoardManager.EElement? infuse;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots, out ElementInfusionBoardManager.EElement? infuse, bool ignoreAmount = false)
	{
		bool? jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots, out bool? jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, out bool ignoreBlockedTileMoveCost, out ElementInfusionBoardManager.EElement? infuse, bool ignoreAmount = false)
	{
		int pierce;
		int pierceEnhancements;
		EConditionDecTrigger conditionDecTrigger;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, out pierce, out pierceEnhancements, out conditionDecTrigger, ignoreAmount);
	}

	public static bool ParseEnhancement(string entryName, string filename, MappingEntry abilityPropertyEntry, List<EEnhancementMarkType> enhancementSlots, out int pierceEnhancements)
	{
		pierceEnhancements = 0;
		switch (abilityPropertyEntry.Key.ToString())
		{
		case "Enhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Enhancements", filename, out var value7))
			{
				for (int n = 0; n < value7; n++)
				{
					enhancementSlots.Add(EEnhancementMarkType.DiamondPlus);
				}
			}
			break;
		}
		case "HexEnhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/HexEnhancements", filename, out var value5))
			{
				for (int l = 0; l < value5; l++)
				{
					enhancementSlots.Add(EEnhancementMarkType.Hex);
				}
			}
			break;
		}
		case "SquareEnhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/SquareEnhancements", filename, out var value2))
			{
				for (int i = 0; i < value2; i++)
				{
					enhancementSlots.Add(EEnhancementMarkType.Square);
				}
			}
			break;
		}
		case "CircleEnhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/CircleEnhancements", filename, out var value4))
			{
				for (int k = 0; k < value4; k++)
				{
					enhancementSlots.Add(EEnhancementMarkType.Circle);
				}
			}
			break;
		}
		case "DiamondEnhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Diamond", filename, out var value6))
			{
				for (int m = 0; m < value6; m++)
				{
					enhancementSlots.Add(EEnhancementMarkType.Diamond);
				}
			}
			break;
		}
		case "DiamondPlusEnhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/DiamondPlus", filename, out var value3))
			{
				for (int j = 0; j < value3; j++)
				{
					enhancementSlots.Add(EEnhancementMarkType.DiamondPlus);
				}
			}
			break;
		}
		case "PierceEnhancements":
		{
			if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Enhancements", filename, out var value))
			{
				pierceEnhancements = value;
			}
			break;
		}
		default:
			return false;
		}
		return true;
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots, out bool? jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, out bool ignoreBlockedTileMoveCost, out ElementInfusionBoardManager.EElement? infuse, out int pierce, out int pierceEnhancements, out EConditionDecTrigger conditionDecTrigger, bool ignoreAmount = false)
	{
		amount = 0;
		useSpecialBaseState = false;
		enhancementSlots = new List<EEnhancementMarkType>();
		pierce = 0;
		pierceEnhancements = 0;
		infuse = null;
		jump = null;
		fly = false;
		ignoreDifficultTerrain = false;
		ignoreHazardousTerrain = false;
		ignoreBlockedTileMoveCost = false;
		conditionDecTrigger = EConditionDecTrigger.None;
		if (di is Mapping)
		{
			bool flag = false;
			foreach (MappingEntry abilityPropertyEntry in (di as Mapping).Entries)
			{
				switch (abilityPropertyEntry.Key.ToString())
				{
				case "Duration":
				case "Amount":
				{
					int value;
					string value2;
					if (abilityPropertyEntry.Value.ToString() == "All")
					{
						amount = 1073741823;
						flag = true;
					}
					else if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Amount", filename, out value, suppressErrors: true))
					{
						amount = value;
						flag = true;
					}
					else if (YMLShared.GetStringPropertyValue(abilityPropertyEntry.Value, entryName + "/Amount", filename, out value2) && value2 == "SpecialBaseStat")
					{
						useSpecialBaseState = true;
					}
					break;
				}
				case "Jump":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/Jump", filename, out var value4))
					{
						jump = value4;
					}
					break;
				}
				case "Fly":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/Fly", filename, out var value6))
					{
						fly = value6;
					}
					break;
				}
				case "IgnoreDifficultTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/IgnoreDifficultTerrain", filename, out var value3))
					{
						ignoreDifficultTerrain = value3;
					}
					break;
				}
				case "IgnoreHazardousTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/IgnoreHazardousTerrain", filename, out var value10))
					{
						ignoreHazardousTerrain = value10;
					}
					break;
				}
				case "IgnoreBlockedTileMoveCost":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/IgnoreBlockedTileMoveCost", filename, out var value5))
					{
						ignoreBlockedTileMoveCost = value5;
					}
					break;
				}
				case "Pierce":
				{
					string value9;
					if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Pierce", filename, out var value8, suppressErrors: true))
					{
						pierce = value8;
					}
					else if (YMLShared.GetStringPropertyValue(abilityPropertyEntry.Value, entryName + "/Pierce", filename, out value9) && value9 == "All")
					{
						pierce = 99999;
					}
					break;
				}
				case "Infuse":
					try
					{
						ElementInfusionBoardManager.EElement value7 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == abilityPropertyEntry.Value.ToString());
						infuse = value7;
					}
					catch (Exception ex)
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Element " + abilityPropertyEntry.Value?.ToString() + " under entry " + entryName + ".  File:\n" + filename, ex);
						infuse = null;
					}
					break;
				case "DecrementTrigger":
				{
					if (YMLShared.GetStringPropertyValue(abilityPropertyEntry.Value, entryName + "/DecrementTrigger", filename, out var decValue))
					{
						EConditionDecTrigger eConditionDecTrigger = CAbilityCondition.ConditionDecTriggers.SingleOrDefault((EConditionDecTrigger s) => s.ToString() == decValue);
						if (eConditionDecTrigger != EConditionDecTrigger.None)
						{
							conditionDecTrigger = eConditionDecTrigger;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DecrementTrigger '" + decValue + "' in file " + filename);
						}
					}
					break;
				}
				default:
				{
					if (ParseEnhancement(entryName, filename, abilityPropertyEntry, enhancementSlots, out var pierceEnhancements2))
					{
						pierceEnhancements = pierceEnhancements2;
						break;
					}
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + abilityPropertyEntry.Key.ToString() + " under " + entryName + ". File:\n" + filename);
					break;
				}
				}
			}
			if (!flag && !ignoreAmount)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Missing entry 'Amount' under " + entryName + ". File:\n" + filename);
				return false;
			}
		}
		else
		{
			if (!(di is Scalar))
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Entry under ability " + entryName + " is invalid.  Entries under " + entryName + " must be a Mapping or Scalar. File:\n" + filename);
				return false;
			}
			if (!ignoreAmount)
			{
				if (YMLShared.GetIntPropertyValue(di, "Ability/" + entryName, filename, out var value11, suppressErrors: true))
				{
					amount = value11;
				}
				else
				{
					if (!YMLShared.GetStringPropertyValue(di, entryName + "/Amount", filename, out var value12))
					{
						return false;
					}
					if (value12 == "SpecialBaseStat")
					{
						useSpecialBaseState = true;
					}
				}
			}
		}
		return true;
	}

	public static bool ParseAbilityProperties(Mapping abilityMapping, string name, int cardID, bool isMonster, bool isAbilityOverride, ref bool isSubAbility, ref string parentName, ref CAbility.EAbilityType? abilityType, ref string propName, ref TrapData trapData, ref List<CEnhancement> abilityEnhancements, ref int? strength, ref CAbility.ECardAction cardAction, ref bool? useSpecialBaseStat, ref int? range, ref int? numberOfTargets, ref bool numberOfTargetsSet, ref int? tokens, ref CAbility.EAbilityFlag? flags, ref CAbilityFilterContainer filter, ref string animationOverload, ref string animationOverload2, ref List<CAbility> subAbilities, ref CAreaEffect areaEffect, ref bool? attackSourcesOnly, ref bool? jump, ref bool? fly, ref bool? ignoreDifficultTerrain, ref bool? ignoreHazardousTerrain, ref bool? ignoreBlockedTileMoveCost, ref CAIFocusOverrideDetails aiFocusOverride, ref bool? carryOtherActorsOnHex, ref CAbilityMove.EMoveRestrictionType? moveRestrictionType, ref ActiveBonusData activeBonusData, ref ActiveBonusLayout activeBonusYML, ref string areaEffectYMLString, ref string areaEffectLayoutOverrideYMLString, ref string areaEffectFlipAxis, ref List<CCondition.EPositiveCondition> positiveConditions, ref List<CCondition.ENegativeCondition> negativeConditions, ref bool? multiPassAttack, ref bool? chainAttack, ref int? chainAttackRange, ref int? chainAttackDamageReduction, ref int? damageSelfBeforeAttack, ref bool? addAttackBaseStat, ref bool? strengthIsBase, ref bool? rangeIsBase, ref bool? targetIsBase, ref bool? rangeAtLeastOne, ref bool? targetAtLeastOne, ref bool? removeConditionsOverride, ref string abilityText, ref bool? abilityTextOnly, ref bool? abilityNoLayoutSpace, ref bool? showRange, ref bool? showTarget, ref bool? showArea, ref bool? onDeath, ref bool? allTargetsOnMovePath, ref bool? allTargetsOnMovePathSameStartAndEnd, ref bool? allTargetsOnAttackPath, ref bool? allTargetsInRangeAlongPath, ref List<string> summons, ref XpPerTargetData xpPerTargetData, ref CAbility.EAbilityTargeting? targeting, ref List<(string, TValue)> targetingVars, ref List<(string, TValue)> vars, ref List<(string, List<(string, TValue)>)> performVars, ref List<ElementInfusionBoardManager.EElement> elementsToInfuse, ref List<AbilityConsume> consumes, ref List<CCondition.ENegativeCondition> elementalConditions, ref bool? showElementPicker, ref int? pierce, ref int? retaliateRange, ref bool? isTargetedAbility, ref float? spawnDelay, ref CAbilityPull.EPullType? pullType, ref CAbilityPush.EAdditionalPushEffect? additionalPushEffect, ref int? additionalPushEffectDamage, ref int? additionalPushEffectXP, ref CAbilityLoot.LootData lootData, ref List<CConditionalOverride> conditionalOverrides, ref CAbilityRequirements startAbilityRequirements, ref int? abilityXP, ref MiscAbilityData miscAbilityData, ref bool? skipAnim, ref int? conditionDuration, ref EConditionDecTrigger? conditionDecTrigger, ref bool? isInlineSubAbility, ref CAugment augment, ref CSong song, ref CDoom doom, ref List<CAttackEffect> attackEffects, ref CAbilityControlActor.ControlActorAbilityData controlActorAbilityData, ref CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceAbilityData, ref List<CAbility> mergedAbilities, ref bool? targetActorWithTrapEffects, ref int? targetActorWithTrapEffectRange, ref List<StatIsBasedOnXData> statIsBasedOnXEntries, ref int? moveObstacleRange, ref string moveObstacleAnimOverload, ref List<CItem.EItemSlot> slotsToRefresh, ref List<CItem.EItemSlotState> slotStatesToRefresh, ref List<CItem.EUsageType> slotUsagesToRefresh, ref CAbility addActiveBonusAbility, ref CAbility.EAttackType? overrideAugmentAttackType, ref List<CAbility> chooseAbilities, ref List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter, ref bool? useParentTargeting, ref CAbility forgoTopActionAbility, ref CAbility forgoBottomActionAbility, ref CAbilityExtraTurn.EExtraTurnType? extraTurnType, ref string actionMonsterClassID, ref CAbilityFilterContainer swapAbilityFirstTargetFilter, ref CAbilityFilterContainer swapAbilitySecondTargetFilter, ref List<int> supplyCardsToGive, ref CAbilityTeleport.TeleportData teleportData, ref List<CAbility.EAbilityType> immunityToAbilityTypes, ref List<CAbility.EAttackType> immuneToAttackTypes, ref bool? immuneToSelfDamage, ref List<string> addOrRemoveModifiers, ref CAbilityHeal.HealAbilityData healAbilityData, ref Dictionary<string, int> resourcesToAddOnAbilityEnd, ref Dictionary<string, int> resourcesToTakeFromTargets, ref Dictionary<string, int> resourcesToGiveToTargets, ref CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData, ref ECharacter changeCharacterModel, ref CAbilityFilter.EFilterTile tileFilter, ref CAbilityDisableCardAction.DisableCardActionData disableCardActionData, ref string previewEffectId, ref string previewEffectText, ref string helpBoxTooltipLocKey, ref bool? exploitVulnerability, ref List<CAbility.EAbilityType> extraTurnAbilityTypeFilter, ref int? maxExtraTurnAbilityLevel, ref bool? toDiscardPile, ref bool? removeWhenDone, ref bool? canExcludeCards, ref List<(string, TValue)> miscVars, string filename)
	{
		try
		{
			bool flag = true;
			Mapping mapping = null;
			CAbilityFilterContainer filter2 = CAbilityFilterContainer.CreateDefaultFilter();
			foreach (MappingEntry entry in abilityMapping.Entries)
			{
				int pierce2;
				int amount;
				bool useSpecialBaseState2;
				List<EEnhancementMarkType> enhancementSlots;
				bool fly2;
				bool ignoreDifficultTerrain2;
				bool ignoreHazardousTerrain2;
				bool ignoreBlockedTileMoveCost2;
				bool value;
				string stringValue;
				int pierceEnhancements;
				bool useSpecialBaseState;
				EConditionDecTrigger conditonDecTrigger;
				List<string> values27;
				float value51;
				switch (entry.Key.ToString())
				{
				case "AddActiveBonus":
				{
					abilityType = CAbility.EAbilityType.AddActiveBonus;
					if (!isAbilityOverride && filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
					}
					if (YMLShared.GetMapping(entry, filename, out var mapping28))
					{
						if (mapping28 != null)
						{
							if (ProcessAbilityEntry(filename, mapping28, entry.Key.ToString(), cardID, isMonster, out var ability2))
							{
								addActiveBonusAbility = ability2;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability for AddActiveBonus.  Filename: " + filename);
							flag = false;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "No valid ability found for AddActiveBonus.  Filename: " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Summon":
				{
					string value22;
					if (entry.Value is Mapping)
					{
						foreach (MappingEntry entry2 in (entry.Value as Mapping).Entries)
						{
							string text = entry2.Key.ToString();
							string value21;
							if (!(text == "ID"))
							{
								if (text == "ActiveBonusData")
								{
									ParseActiveBonusData(entry2.Value, cardID, filename, isMonster: false, ref activeBonusData);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry2.Key.ToString() + " under Summon. File:\n" + filename);
								flag = false;
							}
							else if (YMLShared.GetStringPropertyValue(entry2.Value, "Summon", filename, out value21))
							{
								if (ParseSummon(value21, cardID, name, filename, ref abilityEnhancements, ref abilityType, ref activeBonusData, out var summons2))
								{
									summons = summons2;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Summon", filename, out value22))
					{
						if (ParseSummon(value22, cardID, name, filename, ref abilityEnhancements, ref abilityType, ref activeBonusData, out var summons3))
						{
							summons = summons3;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Summons":
				{
					if (YMLShared.GetStringList(entry.Value, "Summons", filename, out var summonNamesList2))
					{
						if (summonNamesList2.Count == 4)
						{
							bool[] array3 = new bool[4];
							int i3 = 0;
							while (i3 < 4)
							{
								if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonNamesList2[i3]) == null)
								{
									array3[i3] = true;
								}
								else if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == summonNamesList2[i3]) != null)
								{
									array3[i3] = true;
								}
								pierce2 = i3++;
							}
							if (array3[0] && array3[1] && array3[2] && array3[3])
							{
								abilityType = CAbility.EAbilityType.Summon;
								activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Summon;
								summons = summonNamesList2;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find all Summons " + string.Join(", ", summonNamesList2) + ".  File: " + filename);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "More than 4 summon data defined for Summons " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Revive":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Revive", filename, out var value41))
					{
						if (ParseSummon(value41, cardID, name, filename, ref abilityEnhancements, ref abilityType, ref activeBonusData, out var summons4))
						{
							abilityType = CAbility.EAbilityType.Revive;
							summons = summons4;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Revives":
				{
					if (YMLShared.GetStringList(entry.Value, "Revives", filename, out var summonNamesList))
					{
						if (summonNamesList.Count == 4)
						{
							bool[] array = new bool[4];
							int i2 = 0;
							while (i2 < 4)
							{
								if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == summonNamesList[i2]) != null)
								{
									array[i2] = true;
								}
								pierce2 = i2++;
							}
							if (array[0] && array[1] && array[2] && array[3])
							{
								abilityType = CAbility.EAbilityType.Revive;
								summons = summonNamesList;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find all Revives " + string.Join(", ", summonNamesList) + ".  File: " + filename);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "More than 4 summon data defined for Revives " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Attack":
				{
					if (GetEnhancedAbilityValues(entry.Value, "Attack", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out var pierce4, out var pierceEnhancements2))
					{
						abilityType = CAbility.EAbilityType.Attack;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						pierce = pierce4;
						if (enhancementSlots.Count > 0)
						{
							for (int num39 = 0; num39 < enhancementSlots.Count; num39++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num39, enhancementSlots[num39]));
							}
						}
						if (pierceEnhancements2 > 0)
						{
							for (int num40 = 0; num40 < pierceEnhancements2; num40++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Pierce, cardID, name, num40, EEnhancementMarkType.Square));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Push":
					if (GetEnhancedAbilityValues(entry.Value, "Push", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Push;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count <= 0)
						{
							break;
						}
						if (isSubAbility)
						{
							for (int num8 = 0; num8 < enhancementSlots.Count; num8++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Push, cardID, name, num8, enhancementSlots[num8]));
							}
						}
						else
						{
							for (int num9 = 0; num9 < enhancementSlots.Count; num9++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num9, enhancementSlots[num9]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Pull":
					if (GetEnhancedAbilityValues(entry.Value, "Pull", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Pull;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count <= 0)
						{
							break;
						}
						if (isSubAbility)
						{
							for (int num31 = 0; num31 < enhancementSlots.Count; num31++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Pull, cardID, name, num31, enhancementSlots[num31]));
							}
						}
						else
						{
							for (int num32 = 0; num32 < enhancementSlots.Count; num32++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num32, enhancementSlots[num32]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Teleport2":
					if (GetEnhancedAbilityValues(entry.Value, "Teleport2", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Teleport2;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num35 = 0; num35 < enhancementSlots.Count; num35++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num35, enhancementSlots[num35]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddAugment":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddAugment", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.AddAugment;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddSong":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddSong", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.AddSong;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddDoomSlot":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddDoomSlot", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.AddDoomSlots;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "TransferDooms":
					if (YMLShared.GetIntPropertyValue(entry.Value, "TransferAllDooms", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.TransferDooms;
						miscAbilityData.CanTargetInvisible = true;
						strength = ((amount == -1) ? int.MaxValue : amount);
					}
					else
					{
						flag = false;
					}
					break;
				case "PlaySong":
					if (YMLShared.GetIntPropertyValue(entry.Value, "PlaySong", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.PlaySong;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddDoom":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping36))
					{
						abilityType = CAbility.EAbilityType.AddDoom;
						miscAbilityData.CanTargetInvisible = true;
						doom = null;
						using (List<MappingEntry>.Enumerator enumerator2 = mapping36.Entries.GetEnumerator())
						{
							if (enumerator2.MoveNext())
							{
								MappingEntry current33 = enumerator2.Current;
								List<CAbility> doomAbilities = null;
								if (current33.Key.ToString() == "Doom")
								{
									if (YMLShared.GetMapping(current33, filename, out var mapping37))
									{
										foreach (MappingEntry entry3 in mapping37.Entries)
										{
											if (entry3.Key.ToString() == "DoomAbilities")
											{
												if (CardProcessingShared.GetAbilities(entry3, cardID, isMonster, filename, out var abilities14))
												{
													doomAbilities = abilities14;
												}
												else
												{
													flag = false;
												}
											}
											else
											{
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Doom.  Entry " + current33.Key.ToString() + " was not recognised.  File: " + filename);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Doom.  Entry " + current33.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
								doom = new CDoom(doomAbilities);
							}
						}
						if (doom == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Doom definition in file " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Move":
				{
					if (GetEnhancedAbilityValues(entry.Value, "Move", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out var jump2, out fly2, out ignoreDifficultTerrain2, out ignoreHazardousTerrain2, out ignoreBlockedTileMoveCost2))
					{
						abilityType = CAbility.EAbilityType.Move;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						jump = jump2;
						fly = fly2;
						ignoreDifficultTerrain = ignoreDifficultTerrain2;
						ignoreHazardousTerrain = ignoreHazardousTerrain2;
						ignoreBlockedTileMoveCost = ignoreBlockedTileMoveCost2;
						if (enhancementSlots.Count > 0)
						{
							for (int num17 = 0; num17 < enhancementSlots.Count; num17++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num17, enhancementSlots[num17]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Fear":
				{
					if (GetEnhancedAbilityValues(entry.Value, "Fear", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out var jump3, out fly2, out ignoreDifficultTerrain2, out ignoreHazardousTerrain2, out ignoreBlockedTileMoveCost2))
					{
						abilityType = CAbility.EAbilityType.Fear;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						jump = jump3;
						fly = fly2;
						ignoreDifficultTerrain = ignoreDifficultTerrain2;
						ignoreHazardousTerrain = ignoreHazardousTerrain2;
						ignoreBlockedTileMoveCost = ignoreBlockedTileMoveCost2;
						if (enhancementSlots.Count > 0)
						{
							for (int num26 = 0; num26 < enhancementSlots.Count; num26++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num26, enhancementSlots[num26]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Heal":
					if (GetEnhancedAbilityValues(entry.Value, "Heal", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Heal;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num44 = 0; num44 < enhancementSlots.Count; num44++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num44, enhancementSlots[num44]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AdditionalHealData":
					if (!ParseHealData(entry.Value, "AdditionalHealData", ref healAbilityData))
					{
						flag = false;
					}
					break;
				case "DisableCardAction":
					if (ParseDisableCardActionData(entry.Value, "DisableCardAction", ref disableCardActionData))
					{
						abilityType = CAbility.EAbilityType.DisableCardAction;
					}
					else
					{
						flag = false;
					}
					break;
				case "Damage":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/Damage", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.Damage;
						strength = amount;
						isTargetedAbility = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "GainXP":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/GainXP", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.GainXP;
						strength = amount;
						isTargetedAbility = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "ResourcesToAddOnAbilityEnd":
					if (entry.Value is Sequence)
					{
						resourcesToAddOnAbilityEnd = new Dictionary<string, int>();
						Sequence sequence3 = entry.Value as Sequence;
						if (sequence3.Entries[0] is Sequence)
						{
							foreach (DataItem entry4 in sequence3.Entries)
							{
								if (YMLShared.GetTupleStringInt(entry4, "ResourcesToAddOnAbilityEnd", filename, out var tuple3))
								{
									if (resourcesToAddOnAbilityEnd.ContainsKey(tuple3.Item1))
									{
										resourcesToAddOnAbilityEnd[tuple3.Item1] += tuple3.Item2;
									}
									else
									{
										resourcesToAddOnAbilityEnd.Add(tuple3.Item1, tuple3.Item2);
									}
								}
								else
								{
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToAddOnAbilityEnd entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToAddOnAbilityEnd entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
						flag = false;
					}
					break;
				case "ResourcesToGiveToTargets":
					if (entry.Value is Sequence)
					{
						resourcesToGiveToTargets = new Dictionary<string, int>();
						Sequence sequence2 = entry.Value as Sequence;
						if (sequence2.Entries[0] is Sequence)
						{
							foreach (DataItem entry5 in sequence2.Entries)
							{
								if (YMLShared.GetTupleStringInt(entry5, "ResourcesToGiveToTargets", filename, out var tuple2))
								{
									if (resourcesToGiveToTargets.ContainsKey(tuple2.Item1))
									{
										resourcesToGiveToTargets[tuple2.Item1] += tuple2.Item2;
									}
									else
									{
										resourcesToGiveToTargets.Add(tuple2.Item1, tuple2.Item2);
									}
								}
								else
								{
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToGiveToTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToGiveToTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
						flag = false;
					}
					break;
				case "ResourcesToTakeFromTargets":
					if (entry.Value is Sequence)
					{
						resourcesToTakeFromTargets = new Dictionary<string, int>();
						Sequence sequence = entry.Value as Sequence;
						if (sequence.Entries[0] is Sequence)
						{
							foreach (DataItem entry6 in sequence.Entries)
							{
								if (YMLShared.GetTupleStringInt(entry6, "ResourcesToTakeFromTargets", filename, out var tuple))
								{
									if (resourcesToTakeFromTargets.ContainsKey(tuple.Item1))
									{
										resourcesToTakeFromTargets[tuple.Item1] += tuple.Item2;
									}
									else
									{
										resourcesToTakeFromTargets.Add(tuple.Item1, tuple.Item2);
									}
								}
								else
								{
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToTakeFromTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToTakeFromTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
						flag = false;
					}
					break;
				case "Loot":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/Loot", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.Loot;
						if (lootData == null)
						{
							lootData = CAbilityLoot.LootData.CreateDefaultLootData();
						}
						range = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreventDamage":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/PreventDamage", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.PreventDamage;
						strength = ((amount == -1) ? int.MaxValue : amount);
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						activeBonusData.IsToggleBonus = true;
						activeBonusData.ToggleIsOptional = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "MirrorDamage":
					abilityType = CAbility.EAbilityType.MirrorDamage;
					break;
				case "ControlSummonTurn":
					abilityType = CAbility.EAbilityType.ControlSummonTurn;
					activeBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.DefaultToggleBehaviour;
					break;
				case "Shield":
					if (GetEnhancedAbilityValues(entry.Value, "Shield", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Shield;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
						activeBonusData.ToggleIsOptional = false;
						if (enhancementSlots.Count > 0)
						{
							for (int num46 = 0; num46 < enhancementSlots.Count; num46++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num46, enhancementSlots[num46]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ControlActor":
					if (ParseControlActorData(entry.Value, filename, ref controlActorAbilityData, cardID, isMonster))
					{
						abilityType = CAbility.EAbilityType.ControlActor;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ChangeAllegiance":
					if (ParseChangeAllegianceData(entry.Value, filename, ref changeAllegianceAbilityData, cardID, isMonster))
					{
						abilityType = CAbility.EAbilityType.ChangeAllegiance;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ChooseAbility":
				{
					abilityType = CAbility.EAbilityType.ChooseAbility;
					if (YMLShared.GetMapping(entry, filename, out var mapping29))
					{
						foreach (MappingEntry entry7 in mapping29.Entries)
						{
							string text = entry7.Key.ToString();
							string value46;
							if (!(text == "Strength"))
							{
								if (text == "Abilities")
								{
									if (CardProcessingShared.GetAbilities(entry7, cardID, isMonster, filename, out var abilities10))
									{
										chooseAbilities = abilities10;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process ChooseAbility.  Entry " + entry7.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry7.Value, "Strength", filename, out amount, suppressErrors: true))
							{
								strength = amount;
							}
							else if (YMLShared.GetStringPropertyValue(entry7.Value, "Strength", filename, out value46))
							{
								if (value46 == "Max")
								{
									strength = int.MaxValue;
								}
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Choose":
				{
					abilityType = CAbility.EAbilityType.Choose;
					if (YMLShared.GetMapping(entry, filename, out var mapping24))
					{
						foreach (MappingEntry entry8 in mapping24.Entries)
						{
							switch (entry8.Key.ToString())
							{
							case "Strength":
							{
								string value42;
								if (YMLShared.GetIntPropertyValue(entry8.Value, "Strength", filename, out amount, suppressErrors: true))
								{
									strength = amount;
								}
								else if (YMLShared.GetStringPropertyValue(entry8.Value, "Strength", filename, out value42))
								{
									if (value42 == "Max")
									{
										strength = int.MaxValue;
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Conditions":
							{
								if (!YMLShared.GetStringList(entry8.Value, "Conditions", filename, out var values22))
								{
									break;
								}
								List<CAbility> list7 = new List<CAbility>();
								foreach (string item7 in values22)
								{
									string previewEffectText2 = null;
									CAbility.EAbilityType eAbilityType2 = CAbility.EAbilityType.None;
									if (Enum.TryParse<CCondition.EPositiveCondition>(item7, out var result8))
									{
										eAbilityType2 = CCondition.AbilityTypeFromPositiveCondition(result8);
									}
									else
									{
										if (!Enum.TryParse<CCondition.ENegativeCondition>(item7, out var result9))
										{
											flag = false;
											break;
										}
										eAbilityType2 = CCondition.AbilityTypeFromNegativeCondition(result9);
									}
									CAbility.EAbilityType abilityType3 = eAbilityType2;
									string name2 = item7;
									CAbility cAbility = CAbility.CreateAbility(abilityType3, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, CAbilityFilterContainer.CreateDefaultFilter(), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, name2, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, previewEffectText2, null, exploitVulnerability: false, null, null, int.MaxValue);
									if (cAbility.MiscAbilityData == null)
									{
										cAbility.MiscAbilityData = new MiscAbilityData();
									}
									cAbility.MiscAbilityData.AutotriggerAbility = true;
									list7.Add(cAbility);
								}
								chooseAbilities = list7;
								break;
							}
							case "Choices":
							{
								Mapping mapping25;
								List<string> values24;
								if (YMLShared.GetIntList(entry8.Value, "Choices", filename, out var values23, allowScalar: true, 1, suppressErrors: true))
								{
									List<CAbility> list8 = new List<CAbility>();
									foreach (int item8 in values23)
									{
										string text2 = item8.ToString();
										string previewEffectText3 = text2;
										int.TryParse(text2, out var result10);
										int strength3 = result10;
										string name2 = text2;
										CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.Null, strength3, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, CAbilityFilterContainer.CreateDefaultFilter(), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, name2, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, previewEffectText3, null, exploitVulnerability: false, null, null, int.MaxValue);
										if (cAbility2.MiscAbilityData == null)
										{
											cAbility2.MiscAbilityData = new MiscAbilityData();
										}
										cAbility2.MiscAbilityData.AutotriggerAbility = true;
										list8.Add(cAbility2);
									}
									chooseAbilities = list8;
								}
								else if (YMLShared.GetMapping(entry8, filename, out mapping25, suppressErrors: true))
								{
									List<CAbility> list9 = new List<CAbility>();
									foreach (MappingEntry entry9 in mapping25.Entries)
									{
										string text3 = entry9.Key.ToString();
										string text4 = text3;
										List<(string, TValue)> list10 = null;
										int result11 = 0;
										CAbilityRequirements cAbilityRequirements = null;
										int.TryParse(text3, out result11);
										if (YMLShared.GetMapping(entry9, filename, out var mapping26, suppressErrors: true))
										{
											foreach (MappingEntry entry10 in mapping26.Entries)
											{
												string text5 = entry10.Key.ToString();
												switch (text5)
												{
												case "MiscVars":
												{
													if (GetVars(entry10, filename, out var vars3))
													{
														if (list10 == null)
														{
															list10 = vars3;
														}
														else
														{
															list10.AddRange(vars3);
														}
													}
													else
													{
														flag = false;
													}
													break;
												}
												case "PreviewEffectText":
												{
													if (YMLShared.GetStringPropertyValue(entry10.Value, text5, filename, out var value43))
													{
														text4 = value43;
													}
													else
													{
														flag = false;
													}
													break;
												}
												case "Strength":
												{
													if (YMLShared.GetIntPropertyValue(entry10.Value, text5, filename, out var value44))
													{
														result11 = value44;
													}
													else
													{
														flag = false;
													}
													break;
												}
												case "Requirements":
												{
													if (GetAbilityRequirements(entry10, filename, out var abilityRequirements2))
													{
														cAbilityRequirements = abilityRequirements2;
													}
													else
													{
														flag = false;
													}
													break;
												}
												}
											}
										}
										int strength4 = result11;
										string name2 = text3;
										CAbilityFilterContainer abilityFilter = CAbilityFilterContainer.CreateDefaultFilter();
										string name3 = name2;
										string text6 = text4;
										CAbilityRequirements startAbilityConditions = cAbilityRequirements;
										string previewEffectText4 = text6;
										List<(string, TValue)> miscVars2 = list10;
										CAbility cAbility3 = CAbility.CreateAbility(CAbility.EAbilityType.Null, strength4, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, abilityFilter, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, name3, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, startAbilityConditions, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, previewEffectText4, null, exploitVulnerability: false, null, null, int.MaxValue, useParentTargeting: false, createInitialState: true, null, toDiscardPile: false, removeWhenDone: false, canExcludeCards: false, miscVars2);
										if (cAbility3.MiscAbilityData == null)
										{
											cAbility3.MiscAbilityData = new MiscAbilityData();
										}
										cAbility3.MiscAbilityData.AutotriggerAbility = true;
										list9.Add(cAbility3);
									}
									chooseAbilities = list9;
								}
								else if (YMLShared.GetStringList(entry8.Value, "Choices", filename, out values24))
								{
									List<CAbility> list11 = new List<CAbility>();
									foreach (string item9 in values24)
									{
										string text7 = item9.ToString();
										string previewEffectText5 = text7;
										string text6 = text7;
										CAbility cAbility4 = CAbility.CreateAbility(CAbility.EAbilityType.Null, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, CAbilityFilterContainer.CreateDefaultFilter(), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, text6, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, previewEffectText5, null, exploitVulnerability: false, null, null, int.MaxValue);
										if (cAbility4.MiscAbilityData == null)
										{
											cAbility4.MiscAbilityData = new MiscAbilityData();
										}
										cAbility4.MiscAbilityData.AutotriggerAbility = true;
										list11.Add(cAbility4);
									}
									chooseAbilities = list11;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Abilities":
							{
								if (CardProcessingShared.GetAbilities(entry8, cardID, isMonster, filename, out var abilities9))
								{
									chooseAbilities = abilities9;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Choose.  Entry " + entry8.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ForgoActionsForCompanion":
				{
					abilityType = CAbility.EAbilityType.ForgoActionsForCompanion;
					if (YMLShared.GetMapping(entry, filename, out var mapping30))
					{
						foreach (MappingEntry entry11 in mapping30.Entries)
						{
							string text = entry11.Key.ToString();
							List<CAbility> abilities12;
							if (!(text == "TopActionAbility"))
							{
								if (text == "BottomActionAbility")
								{
									if (CardProcessingShared.GetAbilities(entry11, cardID, isMonster, filename, out var abilities11))
									{
										forgoBottomActionAbility = abilities11[0];
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process ForgoActionsForCompanion ability.  Entry " + entry11.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (CardProcessingShared.GetAbilities(entry11, cardID, isMonster, filename, out abilities12))
							{
								forgoTopActionAbility = abilities12[0];
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedCreateAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping22))
					{
						foreach (MappingEntry entry12 in mapping22.Entries)
						{
							if (entry12.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry12, cardID, isMonster, filename, out var abilities8, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedCreateAttack;
									mergedAbilities = abilities8;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedCreateAttack.  Entry " + entry12.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedDestroyObstacleAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping18))
					{
						foreach (MappingEntry entry13 in mapping18.Entries)
						{
							if (entry13.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry13, cardID, isMonster, filename, out var abilities7, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedDestroyObstacleAttack;
									mergedAbilities = abilities7;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedDestroyObstacleAttack.  Entry " + entry13.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedMoveAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping13))
					{
						foreach (MappingEntry entry14 in mapping13.Entries)
						{
							if (entry14.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry14, cardID, isMonster, filename, out var abilities5, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedMoveAttack;
									mergedAbilities = abilities5;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveAttack.  Entry " + entry14.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedMoveObstacleAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping7))
					{
						foreach (MappingEntry entry15 in mapping7.Entries)
						{
							if (entry15.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry15, cardID, isMonster, filename, out var abilities3, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedMoveObstacleAttack;
									mergedAbilities = abilities3;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveObstacleAttack.  Entry " + entry15.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedDisarmTrapDestroyObstacle":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping3))
					{
						foreach (MappingEntry entry16 in mapping3.Entries)
						{
							if (entry16.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry16, cardID, isMonster, filename, out var abilities, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedDisarmTrapDestroyObstacles;
									mergedAbilities = abilities;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveObstacleAttack.  Entry " + entry16.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedKillCreate":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping4))
					{
						foreach (MappingEntry entry17 in mapping4.Entries)
						{
							if (entry17.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry17, cardID, isMonster, filename, out var abilities2, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedKillCreate;
									mergedAbilities = abilities2;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveObstacleAttack.  Entry " + entry17.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Redirect":
				{
					abilityType = CAbility.EAbilityType.Redirect;
					CAbilityFilter val4 = new CAbilityFilter(CAbilityFilter.EFilterTargetType.Ally);
					int val5 = 1;
					bool val6 = false;
					if (YMLShared.GetMapping(entry, filename, out var mapping35, suppressErrors: true))
					{
						foreach (MappingEntry entry18 in mapping35.Entries)
						{
							switch (entry18.Key.ToString())
							{
							case "Filter":
							{
								if (CardProcessingShared.GetAbilityFilter(entry18, filename, out var filter12))
								{
									val4 = filter12;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Range":
							{
								if (YMLShared.GetIntPropertyValue(entry18.Value, "Range", filename, out var value52))
								{
									val5 = value52;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "IgnoreLOS":
								if (YMLShared.GetBoolPropertyValue(entry18.Value, "IgnoreLOS", filename, out value))
								{
									val6 = value;
								}
								else
								{
									flag = false;
								}
								break;
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Redirect.  Entry " + entry18.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					if (miscVars == null)
					{
						miscVars = new List<(string, TValue)>();
					}
					miscVars.Add(("RedirectFilter", new TValue(val4)));
					miscVars.Add(("RedirectRange", new TValue(val5)));
					miscVars.Add(("RedirectIgnoreLOS", new TValue(val6)));
					break;
				}
				case "TransferDamage":
				{
					abilityType = CAbility.EAbilityType.TransferDamage;
					CAbilityFilter val7 = new CAbilityFilter(CAbilityFilter.EFilterTargetType.Ally);
					int val8 = 1;
					bool val9 = false;
					if (YMLShared.GetMapping(entry, filename, out var mapping39, suppressErrors: true))
					{
						foreach (MappingEntry entry19 in mapping39.Entries)
						{
							switch (entry19.Key.ToString())
							{
							case "Filter":
							{
								if (CardProcessingShared.GetAbilityFilter(entry19, filename, out var filter13))
								{
									val7 = filter13;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Range":
							{
								if (YMLShared.GetIntPropertyValue(entry19.Value, "Range", filename, out var value53))
								{
									val8 = value53;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "IgnoreLOS":
								if (YMLShared.GetBoolPropertyValue(entry19.Value, "IgnoreLOS", filename, out value))
								{
									val9 = value;
								}
								else
								{
									flag = false;
								}
								break;
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Redirect.  Entry " + entry19.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					if (miscVars == null)
					{
						miscVars = new List<(string, TValue)>();
					}
					miscVars.Add(("TransferFilter", new TValue(val7)));
					miscVars.Add(("TransferRange", new TValue(val8)));
					miscVars.Add(("TransferIgnoreLOS", new TValue(val9)));
					break;
				}
				case "ShiftForm":
					abilityType = CAbility.EAbilityType.ShiftForm;
					break;
				case "AddTarget":
					if (GetEnhancedAbilityValues(entry.Value, "AddTarget", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddTarget;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num36 = 0; num36 < enhancementSlots.Count; num36++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num36, enhancementSlots[num36]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddHeal":
					if (GetEnhancedAbilityValues(entry.Value, "AddHeal", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddHeal;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num25 = 0; num25 < enhancementSlots.Count; num25++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num25, enhancementSlots[num25]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddRange":
					if (GetEnhancedAbilityValues(entry.Value, "AddRange", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddRange;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num16 = 0; num16 < enhancementSlots.Count; num16++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num16, enhancementSlots[num16]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddCondition":
					if (GetEnhancedAbilityValues(entry.Value, "AddCondition", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddCondition;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num18 = 0; num18 < enhancementSlots.Count; num18++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num18, enhancementSlots[num18]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackersGainDisadvantage":
					if (GetEnhancedAbilityValues(entry.Value, "AttackersGainDisadvantage", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AttackersGainDisadvantage;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots.Count > 0)
						{
							for (int num11 = 0; num11 < enhancementSlots.Count; num11++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num11, enhancementSlots[num11]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Retaliate":
					if (GetEnhancedAbilityValues(entry.Value, "Retaliate", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Retaliate;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						if (activeBonusData != null)
						{
							activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
						}
						if (enhancementSlots.Count > 0)
						{
							for (int num6 = 0; num6 < enhancementSlots.Count; num6++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num6, enhancementSlots[num6]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Trap":
					enhancementSlots = new List<EEnhancementMarkType>();
					if (ParseTrapData(entry.Value, filename, ref trapData, enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Trap;
						if (enhancementSlots.Count > 0)
						{
							for (int num3 = 0; num3 < enhancementSlots.Count; num3++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num3, enhancementSlots[num3]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "DisarmTrap":
					abilityType = CAbility.EAbilityType.DisarmTrap;
					break;
				case "DeactivateSpawner":
					abilityType = CAbility.EAbilityType.DeactivateSpawner;
					break;
				case "ActivateSpawner":
					abilityType = CAbility.EAbilityType.ActivateSpawner;
					break;
				case "DestroyObstacle":
					abilityType = CAbility.EAbilityType.DestroyObstacle;
					if (destroyObstacleData == null)
					{
						destroyObstacleData = CAbilityDestroyObstacle.DestroyObstacleData.DefaultDestroyObstacleData();
					}
					break;
				case "DestroyTerrain":
					abilityType = CAbility.EAbilityType.DestroyTerrain;
					break;
				case "DestroyObstacleData":
					if (ParseDestroyObstacleData(entry.Value, filename, ref destroyObstacleData))
					{
						abilityType = CAbility.EAbilityType.DestroyObstacle;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveObstacle":
					abilityType = CAbility.EAbilityType.MoveObstacle;
					break;
				case "MoveTerrain":
					abilityType = CAbility.EAbilityType.MoveTerrain;
					break;
				case "MoveTrap":
					abilityType = CAbility.EAbilityType.MoveTrap;
					break;
				case "MoveShadow":
					if (GetEnhancedAbilityValues(entry.Value, "MoveShadow", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.MoveShadow;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						if (enhancementSlots.Count > 0)
						{
							for (int num41 = 0; num41 < enhancementSlots.Count; num41++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num41, enhancementSlots[num41]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "PerformFromShadow":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping33))
					{
						foreach (MappingEntry entry20 in mapping33.Entries)
						{
							string text = entry20.Key.ToString();
							bool value50;
							if (!(text == "RemoveWhenDone"))
							{
								if (text == "Abilities")
								{
									if (CardProcessingShared.GetAbilities(entry20, cardID, isMonster, filename, out var abilities13))
									{
										abilityType = CAbility.EAbilityType.PerformFromShadow;
										mergedAbilities = abilities13;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process PerformFromShadow.  Entry " + entry20.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetBoolPropertyValue(entry20.Value, "Ability/PerformFromShadow/RemoveWhenDone", filename, out value50))
							{
								removeWhenDone = value50;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Create":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/Create", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.Create;
						propName = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "CreateTerrain":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/CreateTerrain", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.CreateTerrain;
						if (Enum.IsDefined(typeof(ScenarioManager.ObjectImportType), "Terrain" + stringValue))
						{
							propName = "Terrain" + stringValue;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process CreateTerrain Ability.  Entry " + stringValue + " was not recognised.  File: " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "CreateShadow":
					abilityType = CAbility.EAbilityType.CreateShadow;
					break;
				case "CreateLimitedHazardousTerrain":
					abilityType = CAbility.EAbilityType.CreateLimitedHazardousTerrain;
					break;
				case "RemoveShadow":
					abilityType = CAbility.EAbilityType.RemoveShadow;
					break;
				case "Kill":
					abilityType = CAbility.EAbilityType.Kill;
					break;
				case "Capture":
					if (!GetEnhancedAbilityValues(entry.Value, "Capture", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out pierce2, out pierceEnhancements))
					{
						break;
					}
					abilityType = CAbility.EAbilityType.Capture;
					if (!isAbilityOverride && filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
					}
					strength = amount;
					useSpecialBaseStat = useSpecialBaseState2;
					if (enhancementSlots.Count > 0)
					{
						for (int num20 = 0; num20 < enhancementSlots.Count; num20++)
						{
							abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num20, enhancementSlots[num20]));
						}
					}
					break;
				case "RemoveActorFromMap":
					abilityType = CAbility.EAbilityType.RemoveActorFromMap;
					break;
				case "SelectAndPlayCard":
				{
					if (filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
					}
					abilityType = CAbility.EAbilityType.SelectAndPlayCard;
					List<CBaseCard.ECardPile> val3 = new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand };
					bool flag2 = false;
					int num12 = -1;
					strength = 1;
					if (YMLShared.GetMapping(entry, filename, out var mapping14, suppressErrors: true))
					{
						foreach (MappingEntry entry21 in mapping14.Entries)
						{
							switch (entry21.Key.ToString())
							{
							case "CardPile":
							{
								string value31;
								CBaseCard.ECardPile result7;
								if (YMLShared.GetStringList(entry21.Value, "CardPile", filename, out var values15))
								{
									val3 = values15.Select((string x) => Enum.Parse<CBaseCard.ECardPile>(x)).ToList();
								}
								else if (YMLShared.GetStringPropertyValue(entry21.Value, "CardPile", filename, out value31) && Enum.TryParse<CBaseCard.ECardPile>(value31, out result7))
								{
									val3 = new List<CBaseCard.ECardPile> { result7 };
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "CardPiles":
							{
								if (YMLShared.GetStringList(entry21.Value, "CardPiles", filename, out var values16))
								{
									try
									{
										val3 = values16.Select(Enum.Parse<CBaseCard.ECardPile>).ToList();
									}
									catch
									{
										SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid pile types '" + string.Join(',', values16) + "' specified for Piles in file " + filename);
										flag = false;
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "BlockXP":
							{
								if (YMLShared.GetBoolPropertyValue(entry21.Value, "BlockXP", filename, out var value32))
								{
									flag2 = value32;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Amount":
							{
								if (YMLShared.GetIntPropertyValue(entry21.Value, "Strength", filename, out var value33))
								{
									strength = value33;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MinRequiredCards":
							{
								if (YMLShared.GetIntPropertyValue(entry21.Value, "MinRequiredCards", filename, out var value30))
								{
									num12 = value30;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process SelectAndPlayCard.  Entry " + entry21.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					if (miscVars == null)
					{
						miscVars = new List<(string, TValue)>();
					}
					miscVars.Add(("SelectAndPlayCardPiles", new TValue(val3)));
					miscVars.Add(("SelectAndPlayBlockXP", flag2));
					miscVars.Add(("MinRequiredCards", (num12 >= 1) ? num12 : (strength ?? 1)));
					break;
				}
				case "SelectAndMoveCard":
				{
					if (filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
					}
					abilityType = CAbility.EAbilityType.SelectAndMoveCard;
					List<CBaseCard.ECardPile> val = new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand };
					CBaseCard.ECardPile eCardPile = CBaseCard.ECardPile.Discarded;
					int num = -1;
					strength = 1;
					if (YMLShared.GetMapping(entry, filename, out var mapping5, suppressErrors: true))
					{
						foreach (MappingEntry entry22 in mapping5.Entries)
						{
							switch (entry22.Key.ToString())
							{
							case "PileToMoveFrom":
							{
								if (YMLShared.GetStringPropertyValue(entry22.Value, "PileToMoveFrom", filename, out var value14) && Enum.TryParse<CBaseCard.ECardPile>(value14, out var result))
								{
									val = new List<CBaseCard.ECardPile> { result };
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "PilesToMoveFrom":
							{
								if (YMLShared.GetStringList(entry22.Value, "PilesToMoveFrom", filename, out var values5))
								{
									try
									{
										val = values5.Select(Enum.Parse<CBaseCard.ECardPile>).ToList();
									}
									catch
									{
										SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid pile types '" + string.Join(',', values5) + "' specified for PilesToMoveFrom in file " + filename);
										flag = false;
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "PileToMoveTo":
							{
								if (YMLShared.GetStringPropertyValue(entry22.Value, "PileToMoveTo", filename, out var value15) && Enum.TryParse<CBaseCard.ECardPile>(value15, out var result2))
								{
									eCardPile = result2;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Amount":
							{
								if (YMLShared.GetIntPropertyValue(entry22.Value, "Strength", filename, out var value16))
								{
									strength = value16;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MinRequiredCards":
							{
								if (YMLShared.GetIntPropertyValue(entry22.Value, "MinRequiredCards", filename, out var value13))
								{
									num = value13;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process SelectAndMoveCard.  Entry " + entry22.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					if (miscVars == null)
					{
						miscVars = new List<(string, TValue)>();
					}
					miscVars.Add(("SelectAndMoveCardPilesToMoveFrom", new TValue(val)));
					miscVars.Add(("SelectAndMoveCardPileToMoveTo", new TValue(eCardPile)));
					miscVars.Add(("MinRequiredCards", (num >= 1) ? num : (strength ?? 1)));
					break;
				}
				case "SelectCard":
				{
					if (filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
					}
					abilityType = CAbility.EAbilityType.SelectCard;
					List<CBaseCard.ECardPile> val10 = new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand };
					int num45 = -1;
					strength = 1;
					if (YMLShared.GetMapping(entry, filename, out var mapping40, suppressErrors: true))
					{
						foreach (MappingEntry entry23 in mapping40.Entries)
						{
							switch (entry23.Key.ToString())
							{
							case "PileToMoveFrom":
							{
								if (YMLShared.GetStringPropertyValue(entry23.Value, "PileToMoveFrom", filename, out var value58) && Enum.TryParse<CBaseCard.ECardPile>(value58, out var result13))
								{
									val10 = new List<CBaseCard.ECardPile> { result13 };
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "PilesToMoveFrom":
							{
								if (YMLShared.GetStringList(entry23.Value, "PilesToMoveFrom", filename, out var values31))
								{
									try
									{
										val10 = values31.Select(Enum.Parse<CBaseCard.ECardPile>).ToList();
									}
									catch
									{
										SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid pile types '" + string.Join(',', values31) + "' specified for PilesToMoveFrom in file " + filename);
										flag = false;
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Amount":
							{
								if (YMLShared.GetIntPropertyValue(entry23.Value, "Strength", filename, out var value59))
								{
									strength = value59;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MinRequiredCards":
							{
								if (YMLShared.GetIntPropertyValue(entry23.Value, "MinRequiredCards", filename, out var value57))
								{
									num45 = value57;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process SelectAndMoveCard.  Entry " + entry23.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					if (miscVars == null)
					{
						miscVars = new List<(string, TValue)>();
					}
					miscVars.Add(("SelectAndMoveCardPilesToMoveFrom", new TValue(val10)));
					miscVars.Add(("MinRequiredCards", (num45 >= 1) ? num45 : (strength ?? 1)));
					break;
				}
				case "ExtraTurn":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping38, suppressErrors: true))
					{
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						foreach (MappingEntry entry24 in mapping38.Entries)
						{
							switch (entry24.Key.ToString())
							{
							case "Type":
								if (YMLShared.GetStringPropertyValue(entry24.Value, "Ability/ExtraTurn/Type", filename, out stringValue))
								{
									abilityType = CAbility.EAbilityType.ExtraTurn;
									CAbilityExtraTurn.EExtraTurnType eExtraTurnType = CAbilityExtraTurn.ExtraTurnTypes.SingleOrDefault((CAbilityExtraTurn.EExtraTurnType x) => x.ToString() == stringValue);
									if (eExtraTurnType != CAbilityExtraTurn.EExtraTurnType.None)
									{
										extraTurnType = eExtraTurnType;
									}
								}
								break;
							case "ActionMonsterClassID":
								if (YMLShared.GetStringPropertyValue(entry24.Value, "ActionMonsterClassID", filename, out stringValue))
								{
									actionMonsterClassID = stringValue;
								}
								else
								{
									flag = false;
								}
								break;
							case "MaxExtraTurnAbilityLevel":
								if (YMLShared.GetIntPropertyValue(entry24.Value, "Ability/MaxExtraTurnAbilityLevel", filename, out amount, suppressErrors: true))
								{
									maxExtraTurnAbilityLevel = amount;
								}
								else
								{
									maxExtraTurnAbilityLevel = int.MaxValue;
								}
								break;
							case "AbilityTypeFilter":
							{
								if (YMLShared.GetStringList(entry24.Value, "AbilityTypeFilter", filename, out var values29))
								{
									foreach (string abilityTypeString4 in values29)
									{
										try
										{
											CAbility.EAbilityType item6 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityTypeString4);
											if (extraTurnAbilityTypeFilter == null)
											{
												extraTurnAbilityTypeFilter = new List<CAbility.EAbilityType>();
											}
											extraTurnAbilityTypeFilter.Add(item6);
										}
										catch
										{
											SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityTypeString4 + "' specified for AbilityTypeFilter in file " + filename);
											flag = false;
										}
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry24.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ExtraTurn", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.ExtraTurn;
						CAbilityExtraTurn.EExtraTurnType eExtraTurnType2 = CAbilityExtraTurn.ExtraTurnTypes.SingleOrDefault((CAbilityExtraTurn.EExtraTurnType x) => x.ToString() == stringValue);
						if (eExtraTurnType2 != CAbilityExtraTurn.EExtraTurnType.None)
						{
							extraTurnType = eExtraTurnType2;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ImprovedShortRest":
					abilityType = CAbility.EAbilityType.ImprovedShortRest;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ImprovedShortRest", filename, out amount, suppressErrors: true))
					{
						strength = amount;
					}
					else if (entry.Value is Mapping)
					{
						if (!YMLShared.GetMapping(entry, filename, out var mapping34))
						{
							break;
						}
						foreach (MappingEntry entry25 in mapping34.Entries)
						{
							if (entry25.Key.ToString() == "CanExcludeCards")
							{
								if (YMLShared.GetBoolPropertyValue(entry25.Value, "CanExcludeCards", filename, out value))
								{
									canExcludeCards = value;
								}
								else
								{
									flag = false;
								}
							}
						}
					}
					else
					{
						strength = 0;
					}
					break;
				case "ExcludeCardsFromShortRest":
					abilityType = CAbility.EAbilityType.ExcludeCardsFromShortRest;
					break;
				case "Swap":
				{
					abilityType = CAbility.EAbilityType.Swap;
					if (!(entry.Value is Mapping) || !YMLShared.GetMapping(entry, filename, out var mapping32))
					{
						break;
					}
					foreach (MappingEntry entry26 in mapping32.Entries)
					{
						string text = entry26.Key.ToString();
						CAbilityFilterContainer filter11;
						if (!(text == "FirstTargetFilter"))
						{
							if (text == "SecondTargetFilter")
							{
								if (CardProcessingShared.GetSingleAbilityFilter(entry26, filename, out var filter10))
								{
									swapAbilitySecondTargetFilter = filter10;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Swap Ability.  Entry " + entry26.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
						else if (CardProcessingShared.GetSingleAbilityFilter(entry26, filename, out filter11))
						{
							swapAbilityFirstTargetFilter = filter11;
						}
						else
						{
							flag = false;
						}
					}
					break;
				}
				case "RedistributeDamage":
					abilityType = CAbility.EAbilityType.RedistributeDamage;
					break;
				case "ChangeCharacterModel":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "ChangeCharacterModel", filename, out var modelString))
					{
						ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == modelString);
						if (eCharacter != ECharacter.None)
						{
							changeCharacterModel = eCharacter;
							abilityType = CAbility.EAbilityType.ChangeCharacterModel;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid model name " + modelString + " in file " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ChangeAttackModifier":
					abilityType = CAbility.EAbilityType.ChangeModifier;
					break;
				case "ChangeCondition":
					abilityType = CAbility.EAbilityType.ChangeCondition;
					break;
				case "RemoveConditions":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RefreshItems", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RemoveConditions;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RefreshItems", filename, out stringValue) && stringValue == "All")
					{
						strength = -1;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RemoveConditions;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShuffleModifierDeck":
					abilityType = CAbility.EAbilityType.ShuffleModifierDeck;
					break;
				case "Muddle":
					if (GetEnhancedAbilityValues(entry.Value, "Muddle", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Muddle;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num19 = 0; num19 < enhancementSlots.Count; num19++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num19, enhancementSlots[num19]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Poison":
					if (GetEnhancedAbilityValues(entry.Value, "Poison", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Poison;
						if (enhancementSlots.Count > 0)
						{
							for (int num22 = 0; num22 < enhancementSlots.Count; num22++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num22, enhancementSlots[num22]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "StopFlying":
					if (GetEnhancedAbilityValues(entry.Value, "StopFlying", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.StopFlying;
						if (enhancementSlots.Count > 0)
						{
							for (int num15 = 0; num15 < enhancementSlots.Count; num15++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num15, enhancementSlots[num15]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "BlockHealing":
					if (GetEnhancedAbilityValues(entry.Value, "BlockHealing", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.BlockHealing;
						if (enhancementSlots.Count > 0)
						{
							for (int num14 = 0; num14 < enhancementSlots.Count; num14++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num14, enhancementSlots[num14]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "NeutralizeShield":
					if (GetEnhancedAbilityValues(entry.Value, "NeutralizeShield", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.NeutralizeShield;
						if (enhancementSlots.Count > 0)
						{
							for (int num7 = 0; num7 < enhancementSlots.Count; num7++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num7, enhancementSlots[num7]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Immovable":
					if (GetEnhancedAbilityValues(entry.Value, "Immovable", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Immovable;
						if (enhancementSlots.Count > 0)
						{
							for (int num4 = 0; num4 < enhancementSlots.Count; num4++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num4, enhancementSlots[num4]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Wound":
					if (GetEnhancedAbilityValues(entry.Value, "Wound", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Wound;
						if (enhancementSlots.Count > 0)
						{
							for (int n = 0; n < enhancementSlots.Count; n++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, n, enhancementSlots[n]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Bane":
					if (GetEnhancedAbilityValues(entry.Value, "Bane", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Bane;
						if (amount == 0)
						{
							strength = 10;
						}
						else
						{
							strength = amount;
						}
						if (enhancementSlots.Count > 0)
						{
							for (int k = 0; k < enhancementSlots.Count; k++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, k, enhancementSlots[k]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Immobilize":
					if (GetEnhancedAbilityValues(entry.Value, "Immobilize", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Immobilize;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num42 = 0; num42 < enhancementSlots.Count; num42++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num42, enhancementSlots[num42]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Impair":
					if (GetEnhancedAbilityValues(entry.Value, "Impair", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Impair;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num37 = 0; num37 < enhancementSlots.Count; num37++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num37, enhancementSlots[num37]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Brittle":
					if (GetEnhancedAbilityValues(entry.Value, "Brittle", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Brittle;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num33 = 0; num33 < enhancementSlots.Count; num33++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num33, enhancementSlots[num33]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Teleport":
					if (GetEnhancedAbilityValues(entry.Value, "Teleport", filename, out amount, out useSpecialBaseState, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Teleport;
						if (teleportData == null)
						{
							teleportData = CAbilityTeleport.TeleportData.DefaultTeleportData();
						}
						if (enhancementSlots.Count > 0)
						{
							for (int num28 = 0; num28 < enhancementSlots.Count; num28++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num28, enhancementSlots[num28]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "TeleportData":
					if (ParseTeleportData(entry.Value, filename, ref teleportData))
					{
						abilityType = CAbility.EAbilityType.Teleport;
					}
					else
					{
						flag = false;
					}
					break;
				case "Disarm":
					if (GetEnhancedAbilityValues(entry.Value, "Disarm", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Disarm;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger == EConditionDecTrigger.Never || conditonDecTrigger == EConditionDecTrigger.WhenAnimatedClaymoreEnds) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num23 = 0; num23 < enhancementSlots.Count; num23++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num23, enhancementSlots[num23]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Stun":
					if (GetEnhancedAbilityValues(entry.Value, "Stun", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Stun;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num13 = 0; num13 < enhancementSlots.Count; num13++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num13, enhancementSlots[num13]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Sleep":
					if (GetEnhancedAbilityValues(entry.Value, "Sleep", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Sleep;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num10 = 0; num10 < enhancementSlots.Count; num10++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num10, enhancementSlots[num10]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AllowOverheal":
					if (GetEnhancedAbilityValues(entry.Value, "AllowOverheal", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, false))
					{
						abilityType = CAbility.EAbilityType.Overheal;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						strength = amount;
						if (enhancementSlots.Count > 0)
						{
							for (int num5 = 0; num5 < enhancementSlots.Count; num5++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num5, enhancementSlots[num5]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "HealthReduction":
					if (GetEnhancedAbilityValues(entry.Value, "HealthReduction", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, false))
					{
						abilityType = CAbility.EAbilityType.HealthReduction;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						strength = amount;
						if (enhancementSlots.Count > 0)
						{
							for (int m = 0; m < enhancementSlots.Count; m++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, m, enhancementSlots[m]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Bless":
					if (GetEnhancedAbilityValues(entry.Value, "Bless", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Bless;
						if (enhancementSlots.Count > 0)
						{
							for (int l = 0; l < enhancementSlots.Count; l++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, l, enhancementSlots[l]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Ward":
					if (GetEnhancedAbilityValues(entry.Value, "Ward", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Ward;
						if (enhancementSlots.Count > 0)
						{
							for (int i = 0; i < enhancementSlots.Count; i++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, i, enhancementSlots[i]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Invisible":
					if (GetEnhancedAbilityValues(entry.Value, "Invisible", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Invisible;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num43 = 0; num43 < enhancementSlots.Count; num43++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num43, enhancementSlots[num43]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Strengthen":
					if (GetEnhancedAbilityValues(entry.Value, "Strengthen", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Strengthen;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num38 = 0; num38 < enhancementSlots.Count; num38++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num38, enhancementSlots[num38]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Regenerate":
					if (GetEnhancedAbilityValues(entry.Value, "Regenerate", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Regenerate;
						conditionDecTrigger = ((conditonDecTrigger == EConditionDecTrigger.Never || conditonDecTrigger == EConditionDecTrigger.WhenFrozenFistMechanicActiveBonusEnds) ? conditonDecTrigger : EConditionDecTrigger.WhenDamaged);
						if (enhancementSlots.Count > 0)
						{
							for (int num34 = 0; num34 < enhancementSlots.Count; num34++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num34, enhancementSlots[num34]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Advantage":
					if (GetEnhancedAbilityValues(entry.Value, "Advantage", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Advantage;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots.Count > 0)
						{
							for (int num30 = 0; num30 < enhancementSlots.Count; num30++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num30, enhancementSlots[num30]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Curse":
					if (GetEnhancedAbilityValues(entry.Value, "Curse", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, true))
					{
						abilityType = CAbility.EAbilityType.Curse;
						if (enhancementSlots.Count > 0)
						{
							for (int num29 = 0; num29 < enhancementSlots.Count; num29++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num29, enhancementSlots[num29]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Consume":
					if (entry.Value is Mapping mapping31)
					{
						if (AbilityConsume.CreateAbilityConsume(mapping31, name, cardID, filename, isMonster, new List<CAbility>(), out var abilityConsume))
						{
							abilityType = CAbility.EAbilityType.Consume;
							consumes.Add(abilityConsume);
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "RecoverLostCards":
					if (entry.Value is Mapping)
					{
						if (!YMLShared.GetMapping(entry, filename, out var mapping27))
						{
							break;
						}
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
						foreach (MappingEntry entry27 in mapping27.Entries)
						{
							string text = entry27.Key.ToString();
							int value45;
							if (!(text == "Strength"))
							{
								if (text == "RecoverCardsWithAbilityOfTypeFilter")
								{
									if (YMLShared.GetStringList(entry27.Value, "RecoverCardsWithAbilityOfTypeFilter", filename, out var values25))
									{
										foreach (string abilityTypeString3 in values25)
										{
											try
											{
												CAbility.EAbilityType item4 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityTypeString3);
												recoverCardsAbilityTypeFilter.Add(item4);
											}
											catch
											{
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityTypeString3 + "' specified for RecoverCardsWithAbilityOfTypeFilter in file " + filename);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry27.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry27.Value, "Strength", filename, out value45))
							{
								strength = value45;
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RecoverLostCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RecoverLostCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "RecoverDiscardedCards":
					if (entry.Value is Mapping)
					{
						if (!YMLShared.GetMapping(entry, filename, out var mapping17))
						{
							break;
						}
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
						foreach (MappingEntry entry28 in mapping17.Entries)
						{
							string text = entry28.Key.ToString();
							int value34;
							if (!(text == "Strength"))
							{
								if (text == "RecoverCardsWithAbilityOfTypeFilter")
								{
									if (YMLShared.GetStringList(entry28.Value, "RecoverCardsWithAbilityOfTypeFilter", filename, out var values17))
									{
										foreach (string abilityTypeString2 in values17)
										{
											try
											{
												CAbility.EAbilityType item2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityTypeString2);
												recoverCardsAbilityTypeFilter.Add(item2);
											}
											catch
											{
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityTypeString2 + "' specified for RecoverCardsWithAbilityOfTypeFilter in file " + filename);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry28.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry28.Value, "Strength", filename, out value34))
							{
								strength = value34;
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RecoverDiscardedCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.RecoverDiscardedCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RecoverDiscardedCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.RecoverDiscardedCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "PlayDiscardedCard":
				{
					if (!(entry.Value is Mapping) || !YMLShared.GetMapping(entry, filename, out var mapping8))
					{
						break;
					}
					if (!isAbilityOverride && filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
					}
					abilityType = CAbility.EAbilityType.PlayDiscardedCard;
					foreach (MappingEntry entry29 in mapping8.Entries)
					{
						switch (entry29.Key.ToString())
						{
						case "UseLostCards":
						{
							if (YMLShared.GetBoolPropertyValue(entry29.Value, "UseLostCards", filename, out var value26))
							{
								useSpecialBaseStat = value26;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "Strength":
						{
							if (YMLShared.GetIntPropertyValue(entry29.Value, "Strength", filename, out var value28))
							{
								strength = value28;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "CardAction":
						{
							if (YMLShared.GetStringPropertyValue(entry29.Value, "CardAction", filename, out var value25))
							{
								if (Enum.TryParse<CAbility.ECardAction>(value25, out var result6))
								{
									cardAction = result6;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "SpecificCards":
						{
							if (YMLShared.GetIntList(entry29.Value, "SpecificCards", filename, out var values13))
							{
								supplyCardsToGive = values13;
							}
							else if (YMLShared.GetIntPropertyValue(entry29.Value, "SpecificCards", filename, out amount))
							{
								supplyCardsToGive = new List<int> { amount };
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "RecoverCardsWithAbilityOfTypeFilter":
						{
							if (YMLShared.GetStringList(entry29.Value, "RecoverCardsWithAbilityOfTypeFilter", filename, out var values14))
							{
								foreach (string abilityTypeString in values14)
								{
									try
									{
										CAbility.EAbilityType item = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityTypeString);
										recoverCardsAbilityTypeFilter.Add(item);
									}
									catch
									{
										SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityTypeString + "' specified for RecoverCardsWithAbilityOfTypeFilter in file " + filename);
										flag = false;
									}
								}
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "ReduceNextDamage":
						{
							if (YMLShared.GetIntPropertyValue(entry29.Value, "ReduceNextDamage", filename, out var value27))
							{
								chainAttackDamageReduction = value27;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "UseParentTargeting":
						{
							if (YMLShared.GetBoolPropertyValue(entry29.Value, "UseParentTargeting", filename, out var value24))
							{
								useParentTargeting = value24;
							}
							else
							{
								flag = false;
							}
							break;
						}
						default:
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry29.Key.ToString() + " was not recognised.  File: " + filename);
							flag = false;
							break;
						}
					}
					break;
				}
				case "Mode":
				{
					new CAbilityFilter(CAbilityFilter.EFilterTargetType.Ally);
					CAbilityModeTransfer.EModeUse eModeUse = CAbilityModeTransfer.EModeUse.Transfer;
					bool val2 = false;
					string value18;
					CAbilityModeTransfer.EModeUse result5;
					if (YMLShared.GetMapping(entry, filename, out var mapping6, suppressErrors: true))
					{
						abilityType = CAbility.EAbilityType.ModeTransfer;
						foreach (MappingEntry entry30 in mapping6.Entries)
						{
							string text = entry30.Key.ToString();
							string value17;
							CAbilityModeTransfer.EModeUse result4;
							if (!(text == "Usage"))
							{
								if (text == "DontSummon")
								{
									if (YMLShared.GetBoolPropertyValue(entry30.Value, "DontSummon", filename, out value))
									{
										val2 = value;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Mode.  Entry " + entry30.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetStringPropertyValue(entry30.Value, "Usage", filename, out value17) && Enum.TryParse<CAbilityModeTransfer.EModeUse>(value17, out result4))
							{
								eModeUse = result4;
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Mode", filename, out value18) && Enum.TryParse<CAbilityModeTransfer.EModeUse>(value18, out result5))
					{
						abilityType = CAbility.EAbilityType.ModeTransfer;
						eModeUse = result5;
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Mode.  Entry " + entry.Key.ToString() + " was not recognised.  File: " + filename);
						flag = false;
					}
					if (flag)
					{
						if (miscVars == null)
						{
							miscVars = new List<(string, TValue)>();
						}
						miscVars.Add(("ModeUsage", new TValue(eModeUse)));
						miscVars.Add(("ModeDontSummon", new TValue(val2)));
					}
					break;
				}
				case "LoseCards":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/LoseCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.LoseCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/LoseCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.LoseCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "DiscardCards":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/DiscardCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.DiscardCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/DiscardCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.DiscardCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveActiveBonusTracker":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/MoveActiveBonusTracker", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.MoveActiveBonusTracker;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/MoveActiveBonusTracker", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.MoveActiveBonusTracker;
					}
					else
					{
						flag = false;
					}
					break;
				case "IncreaseCardLimit":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/IncreaseCardLimit", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.IncreaseCardLimit;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/IncreaseCardLimit", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.IncreaseCardLimit;
					}
					else
					{
						flag = false;
					}
					break;
				case "GiveSupplyCard":
				{
					if (YMLShared.GetIntList(entry.Value, "Ability/GiveSupplyCard", filename, out var values32))
					{
						supplyCardsToGive = values32;
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.GiveSupplyCard;
					}
					else if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/GiveSupplyCard", filename, out amount))
					{
						supplyCardsToGive = new List<int> { amount };
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.GiveSupplyCard;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdjustInitiative":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/AdjustInitiative", filename, out amount, suppressErrors: true))
					{
						abilityType = CAbility.EAbilityType.AdjustInitiative;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "Null":
					abilityType = CAbility.EAbilityType.Null;
					break;
				case "NullTargeting":
					abilityType = CAbility.EAbilityType.NullTargeting;
					break;
				case "ShowNextAttackModifier":
					abilityType = CAbility.EAbilityType.ShowNextAttackModifier;
					if (!activeBonusData.IsAura)
					{
						activeBonusData.IsAura = true;
						activeBonusData.AuraTargeting = CAbility.EAbilityTargeting.All;
						activeBonusData.AuraFilter = new CAbilityFilterContainer(new List<CAbilityFilter>
						{
							new CAbilityFilter(CAbilityFilter.EFilterTargetType.Self),
							new CAbilityFilter(CAbilityFilter.EFilterTargetType.OwnSummon)
						});
					}
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ShowNextAttackModifier", filename, out amount, suppressErrors: true))
					{
						strength = amount;
					}
					else
					{
						strength = 1;
					}
					activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
					break;
				case "SetNextAttackModifiers":
					abilityType = CAbility.EAbilityType.SetNextAttackModifiers;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/SetNextAttackModifiers", filename, out amount, suppressErrors: true))
					{
						strength = amount;
					}
					else
					{
						strength = 1;
					}
					break;
				case "NullHex":
					if (YMLShared.GetStringList(entry.Value, "Ability/NullHex", filename, out values27))
					{
						foreach (string filterString2 in values27)
						{
							abilityType = CAbility.EAbilityType.NullHex;
							CAbilityFilter.EFilterTile eFilterTile3 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == filterString2);
							tileFilter = eFilterTile3;
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/NullTargeting", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.NullHex;
						CAbilityFilter.EFilterTile eFilterTile4 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
						tileFilter |= eFilterTile4;
					}
					else
					{
						flag = false;
					}
					break;
				case "TileFilter":
					if (YMLShared.GetStringList(entry.Value, "Ability/TileFilter", filename, out values27))
					{
						foreach (string filterString in values27)
						{
							CAbilityFilter.EFilterTile eFilterTile = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == filterString);
							tileFilter |= eFilterTile;
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/TileFilter", filename, out stringValue))
					{
						CAbilityFilter.EFilterTile eFilterTile2 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
						tileFilter = eFilterTile2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ImmunityTo":
					if (YMLShared.GetStringList(entry.Value, "Ability/ImmunityTo", filename, out values27))
					{
						abilityType = CAbility.EAbilityType.ImmunityTo;
						immunityToAbilityTypes = new List<CAbility.EAbilityType>();
						foreach (string validTypeString4 in values27)
						{
							CAbility.EAbilityType eAbilityType3 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == validTypeString4);
							if (eAbilityType3 != CAbility.EAbilityType.None)
							{
								if (!immunityToAbilityTypes.Contains(eAbilityType3))
								{
									immunityToAbilityTypes.Add(eAbilityType3);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Immunity To Ability Type '" + validTypeString4 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ImmunityToAttackTypes":
					if (YMLShared.GetStringList(entry.Value, "Ability/ImmunityToAttackTypes", filename, out values27))
					{
						abilityType = CAbility.EAbilityType.ImmunityTo;
						immuneToAttackTypes = new List<CAbility.EAttackType>();
						foreach (string validTypeString3 in values27)
						{
							CAbility.EAttackType eAttackType4 = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType x) => x.ToString() == validTypeString3);
							if (eAttackType4 != CAbility.EAttackType.None)
							{
								if (!immuneToAttackTypes.Contains(eAttackType4))
								{
									immuneToAttackTypes.Add(eAttackType4);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Immunity To Attack Type '" + validTypeString3 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ImmunityToSelfDamage":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ImmunityToSelfDamage", filename, out value))
					{
						abilityType = CAbility.EAbilityType.ImmunityTo;
						immuneToSelfDamage = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "Invulnerability":
					abilityType = CAbility.EAbilityType.Invulnerability;
					break;
				case "PierceInvulnerability":
					abilityType = CAbility.EAbilityType.PierceInvulnerability;
					break;
				case "Untargetable":
					abilityType = CAbility.EAbilityType.Untargetable;
					break;
				case "ItemLock":
					abilityType = CAbility.EAbilityType.ItemLock;
					break;
				case "RefreshItems":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RefreshItems", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RefreshItemCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RefreshItems", filename, out stringValue) && stringValue == "All")
					{
						strength = CAbilityRefreshItemCards.STRENGTH_ALL;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RefreshItemCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "ConsumeItems":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ConsumeItems", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.ConsumeItemCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ConsumeItems", filename, out stringValue) && stringValue == "All")
					{
						strength = CAbilityConsumeItemCards.STRENGTH_ALL;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.ConsumeItemCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "LoseGoalChestRewards":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/LoseGoalChestRewards", filename, out amount, suppressErrors: true))
					{
						int? num21 = strength;
						pierceEnhancements = amount;
						strength = num21 | pierceEnhancements;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.LoseGoalChestReward;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/LoseGoalChestRewards", filename, out stringValue) && stringValue == "All")
					{
						strength = CAbilityLoseGoalChestReward.STRENGTH_ALL;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.LoseGoalChestReward;
					}
					else
					{
						flag = false;
					}
					break;
				case "Infuse":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/Infuse", filename, out var values26))
					{
						foreach (string elementString2 in values26)
						{
							try
							{
								ElementInfusionBoardManager.EElement item5 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString2);
								elementsToInfuse.Add(item5);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString2 + "' specified for infuse ability in file " + filename);
								flag = false;
							}
						}
						if (elementsToInfuse.Count > 0)
						{
							abilityType = CAbility.EAbilityType.Infuse;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConsumeElements":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ConsumeElements", filename, out var values21))
					{
						foreach (string elementString in values21)
						{
							try
							{
								ElementInfusionBoardManager.EElement item3 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString);
								elementsToInfuse.Add(item3);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString + "' specified for infuse ability in file " + filename);
								flag = false;
							}
						}
						if (elementsToInfuse.Count > 0)
						{
							abilityType = CAbility.EAbilityType.ConsumeElement;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ShowElementPicker":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ShowElementPicker", filename, out value))
					{
						showElementPicker = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackEffect":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping19))
					{
						CAttackEffect.EAttackEffect eAttackEffect = CAttackEffect.EAttackEffect.None;
						int strength2 = 0;
						int xp = 0;
						int pierce3 = 0;
						int range2 = 0;
						int targetsCount = 0;
						CAbilityFilter filter4 = new CAbilityFilter();
						List<CCondition.ENegativeCondition> negCons = null;
						List<CCondition.EPositiveCondition> posCons = null;
						CAbility.EAttackType attackType = CAbility.EAttackType.Attack;
						List<CAbility> list6 = new List<CAbility>();
						CAbilityFilter targetingActorFilter = null;
						List<(string, TValue)> vars2 = null;
						foreach (MappingEntry entry31 in mapping19.Entries)
						{
							switch (entry31.Key.ToString())
							{
							case "Vars":
								if (!GetVars(entry31, filename, out vars2))
								{
									flag = false;
								}
								break;
							case "SubAbilities":
							{
								if (YMLShared.GetMapping(entry31, filename, out var mapping20))
								{
									foreach (MappingEntry entry32 in mapping20.Entries)
									{
										if (YMLShared.GetMapping(entry32, filename, out var mapping21))
										{
											if (ProcessAbilityEntry(filename, mapping21, entry32.Key.ToString(), cardID, isMonster, out var ability, isSubAbility: true, isConsumeAbility: false, filter, strength, range, numberOfTargets, areaEffectYMLString, areaEffectLayoutOverrideYMLString, areaEffectFlipAxis, targeting, areaEffect, isTargetedAbility))
											{
												if (subAbilities == null)
												{
													subAbilities = new List<CAbility>();
												}
												list6.Add(ability);
											}
											else
											{
												flag = false;
											}
										}
										else
										{
											flag = false;
										}
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Strength":
							case "Amount":
							{
								if (YMLShared.GetIntPropertyValue(entry31.Value, "Strength", filename, out var value40))
								{
									strength2 = value40;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Type":
							{
								if (YMLShared.GetStringPropertyValue(entry31.Value, "Type", filename, out var effectTypeString))
								{
									CAttackEffect.EAttackEffect eAttackEffect2 = CAttackEffect.AttackEffects.SingleOrDefault((CAttackEffect.EAttackEffect s) => s.ToString() == effectTypeString);
									if (eAttackEffect2 != CAttackEffect.EAttackEffect.None)
									{
										eAttackEffect = eAttackEffect2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid property " + effectTypeString + " set for AttackEffect Type in file " + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "XP":
							{
								if (YMLShared.GetIntPropertyValue(entry31.Value, "XP", filename, out var value38))
								{
									xp = value38;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Pierce":
							{
								string value36;
								if (YMLShared.GetIntPropertyValue(entry31.Value, "Pierce", filename, out var value35, suppressErrors: true))
								{
									pierce3 = value35;
								}
								else if (YMLShared.GetStringPropertyValue(entry31.Value, "Pierce", filename, out value36))
								{
									if (value36 == "All")
									{
										pierce3 = 99999;
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Range":
							{
								if (YMLShared.GetIntPropertyValue(entry31.Value, "Range", filename, out var value39))
								{
									range2 = value39;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Target":
							{
								if (YMLShared.GetIntPropertyValue(entry31.Value, "Target", filename, out var value37))
								{
									targetsCount = value37;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Filter":
							{
								if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter6))
								{
									filter4 = filter6;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "TargetingActorFilter":
							{
								if (CardProcessingShared.GetAbilityFilter(entry31, filename, out var filter5))
								{
									targetingActorFilter = filter5;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Condition":
							case "Conditions":
							{
								if (GetConditions(entry31, filename, out var negativeConditions4, out var positiveConditions4))
								{
									negCons = negativeConditions4;
									posCons = positiveConditions4;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "AttackType":
							{
								if (YMLShared.GetStringPropertyValue(entry31.Value, "Type", filename, out var attackTypeString))
								{
									CAbility.EAttackType eAttackType3 = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType s) => s.ToString() == attackTypeString);
									if (eAttackType3 != CAbility.EAttackType.None)
									{
										attackType = eAttackType3;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid property " + attackTypeString + " set for AttackType Type in file " + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry31.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
						if (eAttackEffect == CAttackEffect.EAttackEffect.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid or Missing Attack Effect Type in file " + filename);
							flag = false;
						}
						else
						{
							attackEffects.Add(new CAttackEffect(eAttackEffect, strength2, xp, pierce3, range2, targetsCount, filter4, negCons, posCons, attackType, list6, targetingActorFilter, vars2));
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConditionalOverrides":
				{
					Mapping mapping12;
					if (!(entry.Value is Mapping))
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in Conditional Override.  Value must be Mapping.  File: " + filename);
						flag = false;
					}
					else if (YMLShared.GetMapping(entry, filename, out mapping12))
					{
						foreach (MappingEntry entry33 in mapping12.Entries)
						{
							if (GetConditionalOverride(entry33, cardID, name, isMonster, filename, out var conditionalOverride))
							{
								conditionalOverrides.Add(conditionalOverride);
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "StartAbilityRequirements":
				{
					if (GetAbilityRequirements(entry, filename, out var abilityRequirements))
					{
						startAbilityRequirements = abilityRequirements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PullType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "PullType", filename, out var pullTypeValue))
					{
						CAbilityPull.EPullType ePullType = CAbilityPull.PullTypes.SingleOrDefault((CAbilityPull.EPullType x) => x.ToString() == pullTypeValue);
						if (ePullType != CAbilityPull.EPullType.None)
						{
							pullType = ePullType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for PullType in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdditionalPushEffect":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "AdditionalPushEffect", filename, out var additionalPushEffectValue))
					{
						CAbilityPush.EAdditionalPushEffect eAdditionalPushEffect = CAbilityPush.AdditionalPushEffects.SingleOrDefault((CAbilityPush.EAdditionalPushEffect x) => x.ToString() == additionalPushEffectValue);
						if (eAdditionalPushEffect != CAbilityPush.EAdditionalPushEffect.None)
						{
							additionalPushEffect = eAdditionalPushEffect;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for AdditionalPushEffect in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdditionalPushEffectDamage":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "AdditionalPushEffectDamage", filename, out var value20))
					{
						additionalPushEffectDamage = value20;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdditionalPushEffectXP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "AdditionalPushEffectXP", filename, out var value19))
					{
						additionalPushEffectXP = value19;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "LootData":
					if (ParseLootData(entry.Value, filename, ref lootData))
					{
						abilityType = CAbility.EAbilityType.Loot;
					}
					else
					{
						flag = false;
					}
					break;
				case "Range":
					if (GetEnhancedAbilityValues(entry.Value, "Range", filename, out amount, out useSpecialBaseState, out enhancementSlots))
					{
						range = amount;
						if (enhancementSlots.Count > 0)
						{
							for (int num2 = 0; num2 < enhancementSlots.Count; num2++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Range, cardID, name, num2, enhancementSlots[num2]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "MinRange":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "MinRange", filename, out var value11))
					{
						miscAbilityData.MinRange = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "NameKeyOverride":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "NameKeyOverride", filename, out var value10))
					{
						miscAbilityData.NameKeyOverride = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RangeIsBase":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "RangeIsBase", filename, out value))
					{
						rangeIsBase = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "RangeAtLeastOne":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "RangeAtLeastOne", filename, out value))
					{
						rangeAtLeastOne = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "RetaliateRange":
					if (entry.Value.ToString() == "All")
					{
						retaliateRange = 99999;
					}
					else if (GetEnhancedAbilityValues(entry.Value, "Ability/RetaliateRange", filename, out amount, out useSpecialBaseState, out enhancementSlots))
					{
						retaliateRange = amount;
						if (flag && enhancementSlots.Count > 0)
						{
							for (int j = 0; j < enhancementSlots.Count; j++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.RetaliateRange, cardID, name, j, enhancementSlots[j]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "MultiPassAttack":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/MultiPassAttack", filename, out value))
					{
						multiPassAttack = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ExploitVulnerability":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ExploitVulnerability", filename, out value))
					{
						exploitVulnerability = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "StartIfParentHasHappened":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/StartIfParentHasHappened", filename, out value))
					{
						miscAbilityData.StartIfParentHasHappened = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackFurthestTarget":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AttackFurthestTarget", filename, out value))
					{
						miscAbilityData.AttackFurthestTarget = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ChainAttack":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ChainAttack", filename, out value))
					{
						chainAttack = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ChainAttackRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ChainAttackRange", filename, out amount))
					{
						chainAttackRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "ChainAttackDamageReduction":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ChainAttackDamageReduction", filename, out amount))
					{
						chainAttackDamageReduction = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "Tokens":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/Tokens", filename, out amount))
					{
						tokens = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "Flags":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/Flags", filename, out var values30))
					{
						CAbility.EAbilityFlag eAbilityFlag = CAbility.EAbilityFlag.None;
						foreach (string item10 in values30)
						{
							if (Enum.TryParse<CAbility.EAbilityFlag>(item10, out var result12))
							{
								eAbilityFlag |= result12;
							}
						}
						flags = eAbilityFlag;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Consumes":
				{
					if (CardProcessingShared.GetElements(entry.Value, "Ability/Consumes", filename, out var elements2))
					{
						elementsToInfuse = elements2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ElementalCondition":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ElementalCondition", filename, out var condString))
					{
						CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == condString);
						if (eNegativeCondition == CCondition.ENegativeCondition.NA)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid condition in Filter/TargetHasNegativeConditions '" + condString + "' File: " + filename);
							flag = false;
						}
						else
						{
							elementalConditions = new List<CCondition.ENegativeCondition>();
							elementalConditions.Add(eNegativeCondition);
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "DamageSelfBeforeAttack":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/DamageSelfBeforeAttack", filename, out amount))
					{
						damageSelfBeforeAttack = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "SpawnDelay":
					if (YMLShared.GetFloatPropertyValue(entry.Value, "Ability/SpawnDelay", filename, out value51))
					{
						spawnDelay = value51;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsOnMovePath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsOnMovePath", filename, out value))
					{
						allTargetsOnMovePath = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsInRangeAlongPath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsInRangeAlongPath", filename, out value))
					{
						allTargetsInRangeAlongPath = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsOnMovePathSameStartAndEnd":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsOnMovePathSameStartAndEnd", filename, out value))
					{
						allTargetsOnMovePathSameStartAndEnd = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsOnAttackPath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsOnAttackPath", filename, out value))
					{
						allTargetsOnAttackPath = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "XPPerTarget":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/XPPerTarget", filename, out amount))
					{
						xpPerTargetData.XpPerTarget = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "XPPerTargetPerEnemyCount":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/XPPerTargetPerEnemyCount", filename, out amount))
					{
						xpPerTargetData.PerEnemyCount = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "XPPerTargetMustDie":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/XPPerTargetMustDie", filename, out value))
					{
						xpPerTargetData.MustDie = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "Targeting":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/Targeting", filename, out stringValue))
					{
						CAbility.EAbilityTargeting eAbilityTargeting = CAbility.AbilityTargetingTypes.SingleOrDefault((CAbility.EAbilityTargeting x) => x.ToString() == stringValue);
						if (eAbilityTargeting != CAbility.EAbilityTargeting.None)
						{
							targeting = eAbilityTargeting;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Targeting Type '" + stringValue + "'.  File: " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetingVars":
					if (targeting != CAbility.EAbilityTargeting.Custom)
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "TargetingVars can only be used with Targeting: Custom.  File: " + filename);
						flag = false;
					}
					else if (!GetVars(entry, filename, out targetingVars))
					{
						flag = false;
					}
					break;
				case "Vars":
					if (!GetVars(entry, filename, out vars))
					{
						flag = false;
					}
					break;
				case "PerformVars":
					if (!GetPerformVars(entry, filename, out performVars))
					{
						flag = false;
					}
					break;
				case "MiscVars":
				{
					if (GetVars(entry, filename, out var vars4))
					{
						if (miscVars == null)
						{
							miscVars = vars4;
						}
						else
						{
							miscVars.AddRange(vars4);
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Condition":
				case "Conditions":
				{
					if (GetConditions(entry, filename, out var negativeConditions6, out var positiveConditions6))
					{
						if (negativeConditions6.Count > 0)
						{
							negativeConditions = negativeConditions6;
						}
						if (positiveConditions6.Count > 0)
						{
							positiveConditions = positiveConditions6;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Filter":
				{
					if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter9))
					{
						filter = filter9;
						miscAbilityData.FilterSpecified = true;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "FilterContainer":
				{
					if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter8))
					{
						filter = filter8;
						miscAbilityData.FilterSpecified = true;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Target":
					if (GetEnhancedAbilityValues(entry.Value, "Target", filename, out amount, out useSpecialBaseState, out enhancementSlots))
					{
						numberOfTargets = amount;
						numberOfTargetsSet = true;
						if (enhancementSlots.Count > 0)
						{
							for (int num27 = 0; num27 < enhancementSlots.Count; num27++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Targets, cardID, name, num27, enhancementSlots[num27]));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetIsBase":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TargetIsBase", filename, out var value49))
					{
						targetIsBase = value49;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TargetAtLeastOne":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TargetAtLeastOne", filename, out var value48))
					{
						targetAtLeastOne = value48;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RemoveConditionsOverride":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "RemoveConditionsOverride", filename, out var value47))
					{
						removeConditionsOverride = value47;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AreaEffect":
					if (YMLShared.GetStringPropertyValue(entry.Value, "AreaEffect", filename, out stringValue))
					{
						areaEffectYMLString = stringValue;
						try
						{
							if (stringValue == null || stringValue.Length <= 0)
							{
								break;
							}
							int num24 = 0;
							string[] array2 = stringValue.Split('|');
							for (pierceEnhancements = 0; pierceEnhancements < array2.Length; pierceEnhancements++)
							{
								string text8 = array2[pierceEnhancements];
								if (!(text8 == "CantFlip") && text8.Split(',')[2].ToUpper() == "E")
								{
									abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.AreaHex, cardID, name, num24, EEnhancementMarkType.Hex));
									num24++;
								}
							}
						}
						catch (Exception ex)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Area Effect string in file " + filename, ex);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AreaEffectLayoutOverride":
					if (YMLShared.GetStringPropertyValue(entry.Value, "AreaEffectLayoutOverride", filename, out stringValue))
					{
						areaEffectLayoutOverrideYMLString = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "AreaEffectFlipAxis":
					if (YMLShared.GetStringPropertyValue(entry.Value, "AreaEffectFlipAxis", filename, out stringValue))
					{
						areaEffectFlipAxis = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "AnimOverload":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AnimOverload", filename, out stringValue))
					{
						animationOverload = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "AnimOverload2":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AnimOverload2", filename, out stringValue))
					{
						animationOverload2 = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "SkipAnim":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/SkipAnim", filename, out value))
					{
						skipAnim = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "IsInlineSubAbility":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IsInlineSubAbility", filename, out value))
					{
						isInlineSubAbility = value;
						isSubAbility = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "SubAbilities":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping23))
					{
						mapping = mapping23;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Augment":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping15))
					{
						bool flag3 = false;
						CAbility.EAttackType eAttackType2 = CAbility.EAttackType.None;
						List<CAbility> list4 = null;
						List<CAbilityOverride> list5 = new List<CAbilityOverride>();
						foreach (MappingEntry entry34 in mapping15.Entries)
						{
							switch (entry34.Key.ToString())
							{
							case "AugmentIsBonus":
								flag3 = true;
								if (YMLShared.GetBoolPropertyValue(entry34.Value, "Augment/AugmentIsBonus", filename, out value))
								{
									flag3 = value;
								}
								else
								{
									flag = false;
								}
								break;
							case "Type":
							{
								if (YMLShared.GetStringPropertyValue(entry34.Value, "Augment/Type", filename, out var augTypeString))
								{
									eAttackType2 = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType s) => s.ToString() == augTypeString);
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Abilities":
							{
								if (CardProcessingShared.GetAbilities(entry34, cardID, isMonster, filename, out var abilities6))
								{
									list4 = abilities6;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "AbilityOverrides":
							{
								if (YMLShared.GetMapping(entry34, filename, out var mapping16))
								{
									foreach (MappingEntry entry35 in mapping16.Entries)
									{
										CAbilityOverride cAbilityOverride2 = CAbilityOverride.CreateAbilityOverride(entry35, cardID, isMonster, filename, parentName);
										if (cAbilityOverride2 != null)
										{
											list5.Add(cAbilityOverride2);
										}
										else
										{
											flag = false;
										}
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides in Augment.  Entry must be mapping.  File: " + filename);
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Augment.  Entry " + entry34.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
						if (eAttackType2 == CAbility.EAttackType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "No valid Type set for Augment in file " + filename);
							flag = false;
						}
						else if (list4 != null && list4.Count > 0)
						{
							foreach (CAbility item11 in list4)
							{
								if (flag3)
								{
									item11.ActiveBonusData.Duration = ((item11.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA) ? CActiveBonus.EActiveBonusDurationType.Persistent : item11.ActiveBonusData.Duration);
									item11.ActiveBonusData.ValidAbilityTypes = ((item11.ActiveBonusData.ValidAbilityTypes.Count <= 0) ? new List<CAbility.EAbilityType> { CAbility.EAbilityType.Attack } : item11.ActiveBonusData.ValidAbilityTypes.ToList());
									item11.ActiveBonusData.AttackType = ((item11.ActiveBonusData.AttackType == CAbility.EAttackType.None) ? eAttackType2 : item11.ActiveBonusData.AttackType);
								}
							}
							augment = new CAugment(list4, eAttackType2, flag3);
						}
						else if (list5 != null && list5.Count > 0)
						{
							augment = new CAugment(list5, eAttackType2, flag3);
						}
						else
						{
							augment = new CAugment(eAttackType2);
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Song":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping9))
					{
						List<CSong.SongEffect> list = new List<CSong.SongEffect>();
						foreach (MappingEntry entry36 in mapping9.Entries)
						{
							CSong.ESongActivationType activationType = CSong.ESongActivationType.AbilityStart;
							CAbility.EAttackType type = CAbility.EAttackType.None;
							CAbility.EAbilityType abilityType2 = CAbility.EAbilityType.None;
							List<CAbility> list2 = null;
							List<CAbilityOverride> list3 = new List<CAbilityOverride>();
							string text = entry36.Key.ToString();
							CAbilityFilterContainer filter3;
							if (!(text == "Filter"))
							{
								if (text == "SongEffect")
								{
									if (YMLShared.GetMapping(entry36, filename, out var mapping10))
									{
										foreach (MappingEntry entry37 in mapping10.Entries)
										{
											switch (entry37.Key.ToString())
											{
											case "ActivationType":
											{
												if (YMLShared.GetStringPropertyValue(entry37.Value, "Song/ActivationType", filename, out var songActivationTypeString))
												{
													activationType = CSong.SongActivationTypes.SingleOrDefault((CSong.ESongActivationType s) => s.ToString() == songActivationTypeString);
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "AttackType":
											{
												if (YMLShared.GetStringPropertyValue(entry37.Value, "Song/AttackType", filename, out var songAttackTypeString))
												{
													type = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType s) => s.ToString() == songAttackTypeString);
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "AbilityType":
											{
												if (YMLShared.GetStringPropertyValue(entry37.Value, "Song/AbilityType", filename, out var songAbilityTypeString))
												{
													abilityType2 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == songAbilityTypeString);
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "Abilities":
											{
												if (CardProcessingShared.GetAbilities(entry37, cardID, isMonster, filename, out var abilities4))
												{
													list2 = abilities4;
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "AbilityOverrides":
											{
												if (YMLShared.GetMapping(entry37, filename, out var mapping11))
												{
													foreach (MappingEntry entry38 in mapping11.Entries)
													{
														CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry38, cardID, isMonster, filename, parentName);
														if (cAbilityOverride != null)
														{
															list3.Add(cAbilityOverride);
														}
													}
												}
												else
												{
													SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides in Song.  Entry must be mapping.  File: " + filename);
													flag = false;
												}
												break;
											}
											default:
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process SongEffect.  Entry " + entry36.Key.ToString() + " was not recognised.  File: " + filename);
												flag = false;
												break;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Song.  Entry " + entry36.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (CardProcessingShared.GetSingleAbilityFilter(entry36, filename, out filter3))
							{
								filter2 = filter3;
							}
							else
							{
								flag = false;
							}
							if (entry36.Key.ToString() != "Filter")
							{
								if (list2 != null && list2.Count > 0)
								{
									list.Add(new CSong.SongEffect(list2, activationType, type, abilityType2));
									continue;
								}
								if (list3 != null && list3.Count > 0)
								{
									list.Add(new CSong.SongEffect(list3, activationType, type, abilityType2));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Song definition in file " + filename);
								flag = false;
							}
						}
						if (list.Count <= 0)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Song definition in file " + filename);
							flag = false;
						}
						else
						{
							song = new CSong(list, filter2);
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AttackSourcesOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AttackSourcesOnly", filename, out value))
					{
						attackSourcesOnly = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ActiveBonusDuration":
					if (activeBonusData == null)
					{
						activeBonusData = new ActiveBonusData();
					}
					if (!ParseActiveBonusDuration(entry.Value, filename, isMonster, ref activeBonusData))
					{
						flag = false;
					}
					break;
				case "ActiveBonus":
					if (activeBonusData == null)
					{
						activeBonusData = new ActiveBonusData();
					}
					if (ParseActiveBonusData(entry.Value, cardID, filename, isMonster, ref activeBonusData))
					{
						if (activeBonusData.OverrideAsSong)
						{
							List<CSong.SongEffect> songEffects = new List<CSong.SongEffect>();
							song = new CSong(songEffects, activeBonusData.Filter);
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Text":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Text", filename, out stringValue))
					{
						abilityText = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "TextOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TextOnly", filename, out value))
					{
						abilityTextOnly = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "NoLayoutSpace":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "NoLayoutSpace", filename, out value))
					{
						abilityNoLayoutSpace = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShowRange":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowRange", filename, out value))
					{
						showRange = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShowTarget":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowTarget", filename, out value))
					{
						showTarget = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShowArea":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowArea", filename, out value))
					{
						showArea = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "OnDeath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "OnDeath", filename, out value))
					{
						onDeath = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "Strength":
				case "Amount":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Amount", filename, out var value12))
					{
						strength = value12;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AddAttackBaseStat":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AddAttackBaseStat", filename, out value))
					{
						addAttackBaseStat = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "StrengthIsBase":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "StrengthIsBase", filename, out value))
					{
						strengthIsBase = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ParentName":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "ParentName", filename, out var value9))
					{
						parentName = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IsTargetedAbility":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsTargetedAbility", filename, out var value8))
					{
						isTargetedAbility = value8;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityXP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Amount", filename, out var value7))
					{
						abilityXP = value7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Jump":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Jump", filename, out var value6))
					{
						jump = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Fly":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Fly", filename, out var value5))
					{
						fly = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AIFocusOverride":
					if (!ParseAIFocusOverride(entry.Value, filename, ref aiFocusOverride))
					{
						flag = false;
					}
					break;
				case "IgnoreDifficultTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreDifficultTerrain", filename, out var value3))
					{
						ignoreDifficultTerrain = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreHazardousTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreHazardousTerrain", filename, out var value2))
					{
						ignoreHazardousTerrain = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreBlockedTileMoveCost":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreHazardousTerrain", filename, out var value61))
					{
						ignoreBlockedTileMoveCost = value61;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CarryOtherActorsOnHex":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CarryOtherActorsOnHex", filename, out var value60))
					{
						carryOtherActorsOnHex = value60;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MoveRestrictionType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "MoveRestrictionType", filename, out var moveRestrictionTypeValue))
					{
						CAbilityMove.EMoveRestrictionType eMoveRestrictionType = CAbilityMove.MoveRestrictionTypes.SingleOrDefault((CAbilityMove.EMoveRestrictionType x) => x.ToString() == moveRestrictionTypeValue);
						if (eMoveRestrictionType != CAbilityMove.EMoveRestrictionType.None)
						{
							moveRestrictionType = eMoveRestrictionType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for MoveRestrictionType in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Pierce":
				{
					string value56;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", filename, out var value55, suppressErrors: true))
					{
						pierce = value55;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Pierce", filename, out value56))
					{
						if (value56 == "All")
						{
							pierce = 99999;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AutoTrigger":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AutoTrigger", filename, out var value54))
					{
						miscAbilityData.AutotriggerAbility = value54;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RestrictRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "RestrictRange", filename, out amount))
					{
						miscAbilityData.RestrictMoveRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetOneEnemyWithAllAttacks":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TargetOneEnemyWithAllAttacks", filename, out value))
					{
						miscAbilityData.TargetOneEnemyWithAllAttacks = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "TreatAsMelee":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TreatAsMelee", filename, out value))
					{
						miscAbilityData.TreatAsMelee = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "TreatAsRange":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TreatAsRange", filename, out value))
					{
						miscAbilityData.TreatAsRange = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseParentRangeType":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseParentRangeType", filename, out value))
					{
						miscAbilityData.UseParentRangeType = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddTargetPropertyToStrength":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AddTargetPropertyToStrength", filename, out stringValue))
					{
						CActor.EOverrideLookupProperty eOverrideLookupProperty = CActor.OverrideLookupProperties.SingleOrDefault((CActor.EOverrideLookupProperty x) => x.ToString() == stringValue);
						if (eOverrideLookupProperty == CActor.EOverrideLookupProperty.NA)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AddTargetPropertyToStrength property '" + stringValue + "' in file '" + filename);
							flag = false;
						}
						else
						{
							miscAbilityData.AddTargetPropertyToStrength = eOverrideLookupProperty;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddTargetPropertyToStrengthMultiplier":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddTargetPropertyToStrengthMultiplier", filename, out amount))
					{
						miscAbilityData.AddTargetPropertyToStrengthMultiplier = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsAdjacentToParentMovePath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsAdjacentToParentMovePath", filename, out value))
					{
						miscAbilityData.AllTargetsAdjacentToParentMovePath = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsAdjacentToParentTargets":
				case "AllTargetsAdjacentToParentTarget":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsAdjacentToParentTarget", filename, out value))
					{
						miscAbilityData.AllTargetsAdjacentToParentTargets = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseParentTiles":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseParentTiles", filename, out value))
					{
						miscAbilityData.UseParentTiles = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreParentAreaOfEffect":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreParentAreaOfEffect", filename, out value))
					{
						miscAbilityData.IgnoreParentAreaOfEffect = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnorePreviousAbilityTargets":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/IgnorePreviousAbilityTargets", filename, out var values28))
					{
						miscAbilityData.IgnorePreviousAbilityTargets = values28.ToList();
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreMergedAbilityTargetSelection":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreMergedAbilityTargetSelection", filename, out value))
					{
						miscAbilityData.IgnoreMergedAbilityTargetSelection = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseMergedWithAbilityTiles":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseMergedWithAbilityTiles", filename, out value))
					{
						miscAbilityData.UseMergedWithAbilityTiles = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "HealPercentageOfHealth":
					if (YMLShared.GetFloatPropertyValue(entry.Value, "HealPercentageOfHealth", filename, out value51))
					{
						miscAbilityData.HealPercentageOfHealth = value51;
					}
					else
					{
						flag = false;
					}
					break;
				case "ExactRange":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ExactRange", filename, out value))
					{
						miscAbilityData.ExactRange = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackHasAdvantage":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AttackHasAdvantage", filename, out value))
					{
						miscAbilityData.AttackHasAdvantage = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackHasDisadvantage":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AttackHasDisadvantage", filename, out value))
					{
						miscAbilityData.AttackHasDisadvantage = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ExhaustSelf":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ExhaustSelf", filename, out value))
					{
						miscAbilityData.ExhaustSelf = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "GainXPPerXDamageDealt":
					if (YMLShared.GetIntPropertyValue(entry.Value, "GainXPPerXDamageDealt", filename, out amount))
					{
						miscAbilityData.GainXPPerXDamageDealt = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "SetHPTo":
					if (YMLShared.GetIntPropertyValue(entry.Value, "SetHPTo", filename, out amount))
					{
						miscAbilityData.SetHPTo = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreventDamageSetTargetHealthTo":
					if (YMLShared.GetIntPropertyValue(entry.Value, "PreventDamageSetTargetHealthTo", filename, out amount))
					{
						miscAbilityData.PreventDamageSetTargetHealthTo = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreLOS":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreLOS", filename, out value))
					{
						miscAbilityData.IgnoreLOS = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "XP":
					if (YMLShared.GetIntPropertyValue(entry.Value, "XP", filename, out amount))
					{
						miscAbilityData.XP = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreventOnlyIfLethal":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "PreventOnlyIfLethal", filename, out value))
					{
						miscAbilityData.PreventOnlyIfLethal = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "CasterDamageToTakeOnPrevent":
					if (YMLShared.GetIntPropertyValue(entry.Value, "CasterDamageToTakeOnPrevent", filename, out amount))
					{
						miscAbilityData.CasterDamageToTakeOnPrevent = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "InfuseIfNotStrong":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "InfuseIfNotStrong", filename, out value))
					{
						miscAbilityData.InfuseIfNotStrong = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "CanTargetInvisible":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanTargetInvisible", filename, out value))
					{
						miscAbilityData.CanTargetInvisible = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "NotApplyEnemy":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "NotApplyEnemy", filename, out value))
					{
						miscAbilityData.NotApplyEnemy = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreMonsterColumnLayout":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreMonsterColumnLayout", filename, out value))
					{
						miscAbilityData.IgnoreMonsterColumnLayout = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreMCLAdjustY":
					if (YMLShared.GetIntPropertyValue(entry.Value, "IgnoreMCLAdjustY", filename, out amount))
					{
						miscAbilityData.IgnoreMCLAdjustY = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "GapAbove":
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapAbove", filename, out amount))
					{
						miscAbilityData.GapAbove = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "GapBelow":
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapBelow", filename, out amount))
					{
						miscAbilityData.GapBelow = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreImmobilize":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreImmobilize", filename, out value))
					{
						miscAbilityData.IgnoreImmobilize = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreStun":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreStun", filename, out value))
					{
						miscAbilityData.IgnoreStun = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "HasCondition":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/HasCondition", filename, out value))
					{
						miscAbilityData.HasCondition = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AreaEffectSymmetrical":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AreaEffectSymmetrical", filename, out value))
					{
						miscAbilityData.AreaEffectSymmetrical = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "TreatAsNonSubAbility":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TreatAsNonSubAbility", filename, out value))
					{
						miscAbilityData.TreatAsNonSubAbility = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "PerformXTimesBasedOn":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/PerformXTimesBasedOn", filename, out stringValue))
					{
						CAbility.EStatIsBasedOnXType eStatIsBasedOnXType = CAbility.StatIsBasedOnXTypes.SingleOrDefault((CAbility.EStatIsBasedOnXType x) => x.ToString() == stringValue);
						if (eStatIsBasedOnXType == CAbility.EStatIsBasedOnXType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid PerformXTimesBasedOn property '" + stringValue + "' in file '" + filename);
							flag = false;
						}
						else
						{
							miscAbilityData.PerformXTimesBasedOn = eStatIsBasedOnXType;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ConsiderMultiTargetForEnhancements":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ConsiderMultiTargetForEnhancements", filename, out value))
					{
						miscAbilityData.ConsiderMultiTargetForEnhancements = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "OnAttackInfuse":
				{
					if (CardProcessingShared.GetElements(entry.Value, "OnAttackInfuse", filename, out var elements))
					{
						miscAbilityData.OnAttackInfuse = elements.ToList();
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AlreadyHasConditionDamageInstead":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AlreadyHasConditionDamageInstead", filename, out amount))
					{
						miscAbilityData.AlreadyHasConditionDamageInstead = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "RemoveCount":
					if (YMLShared.GetIntPropertyValue(entry.Value, "RemoveCount", filename, out amount))
					{
						miscAbilityData.RemoveCount = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseParentTargets":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseParentTargets", filename, out value))
					{
						miscAbilityData.UseParentTargets = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AlsoTargetSelf":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AlsoTargetSelf", filename, out value))
					{
						miscAbilityData.AlsoTargetSelf = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AlsoTargetAdjacent":
				{
					if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter7))
					{
						miscAbilityData.AlsoTargetAdjacent = filter7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConditionsToRemoveFirst":
				{
					if (GetConditions(entry, filename, out var negativeConditions5, out var _))
					{
						if (negativeConditions5.Count > 0)
						{
							miscAbilityData.ConditionsToRemoveFirst = negativeConditions5;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ShowPlusX":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ShowPlusX", filename, out value))
					{
						miscAbilityData.ShowPlusX = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "BypassImmunity":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/BypassImmunity", filename, out value))
					{
						miscAbilityData.BypassImmunity = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "InlineSubAbilityOnKilledTargetsOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/InlineSubAbilityOnKilledTargetsOnly", filename, out value))
					{
						miscAbilityData.InlineSubAbilityOnKilledTargetsOnly = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "NegativeAbilityIsOptional":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/NegativeAbilityIsOptional", filename, out value))
					{
						miscAbilityData.NegativeAbilityIsOptional = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllowContinueForNullAbility":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllowContinueForNullAbility", filename, out value))
					{
						miscAbilityData.AllowContinueForNullAbility = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "NoGoldDrop":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/NoGoldDrop", filename, out value))
					{
						miscAbilityData.NoGoldDrop = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseOriginalActor":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseOriginalActor", filename, out value))
					{
						miscAbilityData.UseOriginalActor = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "MovePathIndexFilter":
				{
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "MovePathIndexFilter", filename, out var equalityFilter))
					{
						miscAbilityData.MovePathIndexFilter = equalityFilter;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TriggeredScenarioModifiers":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/TriggeredScenarioModifiers", filename, out var values20))
					{
						miscAbilityData.TriggeredScenarioModifiers = values20;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TriggerScenarioModifierOnSelfOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TriggerScenarioModifierOnSelfOnly", filename, out value))
					{
						miscAbilityData.TriggerScenarioModifierOnSelfOnly = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ObstacleTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ObstacleTypes", filename, out var values19))
					{
						foreach (string validTypeString2 in values19)
						{
							EPropType ePropType2 = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == validTypeString2);
							if (ePropType2 != EPropType.None)
							{
								if (miscAbilityData.ObstacleTypes == null)
								{
									miscAbilityData.ObstacleTypes = new List<EPropType>();
								}
								if (!miscAbilityData.ObstacleTypes.Contains(ePropType2))
								{
									miscAbilityData.ObstacleTypes.Add(ePropType2);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Obstacle Type '" + validTypeString2 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TerrainTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/TerrainTypes", filename, out var values18))
					{
						foreach (string validTypeString in values18)
						{
							EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == validTypeString);
							if (ePropType != EPropType.None)
							{
								if (miscAbilityData.TerrainTypes == null)
								{
									miscAbilityData.TerrainTypes = new List<EPropType>();
								}
								if (!miscAbilityData.TerrainTypes.Contains(ePropType))
								{
									miscAbilityData.TerrainTypes.Add(ePropType);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Terrain Type '" + validTypeString + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TargetActorWithTrapEffects":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TargetActorWithTrapEffects", filename, out value))
					{
						targetActorWithTrapEffects = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetActorWithTrapEffectRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "TargetActorWithTrapEffectRange", filename, out amount))
					{
						targetActorWithTrapEffectRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "StatIsBasedOnX":
				{
					if (ParseStatIsBasedOnX(entry, filename, out var statIsBasedOnXEntries2, isBaseStats: false))
					{
						statIsBasedOnXEntries = statIsBasedOnXEntries2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MoveObstacleRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "MoveObstacleRange", filename, out amount))
					{
						moveObstacleRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveObstacleAnimOverload":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "MoveObstacleAnimOverload", filename, out var value29))
					{
						moveObstacleAnimOverload = value29;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MoveTrapRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "MoveTrapRange", filename, out amount))
					{
						moveObstacleRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveTrapAnimOverload":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "MoveTrapAnimOverload", filename, out var value23))
					{
						moveObstacleAnimOverload = value23;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotToRefresh":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotToRefresh", filename, out var values12))
					{
						foreach (string slotTypeString6 in values12)
						{
							try
							{
								CItem.EItemSlot eItemSlot2 = CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotTypeString6);
								if (eItemSlot2 != CItem.EItemSlot.None)
								{
									slotsToRefresh.Add(eItemSlot2);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotStateToRefresh property '" + slotTypeString6 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotStateToRefresh":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotStateToRefresh", filename, out var values11))
					{
						foreach (string slotTypeString5 in values11)
						{
							try
							{
								CItem.EItemSlotState eItemSlotState2 = CItem.ItemSlotStates.SingleOrDefault((CItem.EItemSlotState x) => x.ToString() == slotTypeString5);
								if (eItemSlotState2 != CItem.EItemSlotState.None)
								{
									slotStatesToRefresh.Add(eItemSlotState2);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotStateToRefresh property '" + slotTypeString5 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotUsagesToRefresh":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotUsagesToRefresh", filename, out var values10))
					{
						foreach (string slotTypeString4 in values10)
						{
							try
							{
								CItem.EUsageType eUsageType2 = CItem.UsageTypes.SingleOrDefault((CItem.EUsageType x) => x.ToString() == slotTypeString4);
								if (eUsageType2 != CItem.EUsageType.None)
								{
									slotUsagesToRefresh.Add(eUsageType2);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotUsagesToRefresh property '" + slotTypeString4 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotToConsume":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotToConsume", filename, out var values9))
					{
						foreach (string slotTypeString3 in values9)
						{
							try
							{
								CItem.EItemSlot eItemSlot = CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotTypeString3);
								if (eItemSlot != CItem.EItemSlot.None)
								{
									slotsToRefresh.Add(eItemSlot);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotToConsume property '" + slotTypeString3 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotStateToConsume":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotStateToConsume", filename, out var values8))
					{
						foreach (string slotTypeString2 in values8)
						{
							try
							{
								CItem.EItemSlotState eItemSlotState = CItem.ItemSlotStates.SingleOrDefault((CItem.EItemSlotState x) => x.ToString() == slotTypeString2);
								if (eItemSlotState != CItem.EItemSlotState.None)
								{
									slotStatesToRefresh.Add(eItemSlotState);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotStateToConsume property '" + slotTypeString2 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotUsagesToConsume":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotUsagesToConsume", filename, out var values7))
					{
						foreach (string slotTypeString in values7)
						{
							try
							{
								CItem.EUsageType eUsageType = CItem.UsageTypes.SingleOrDefault((CItem.EUsageType x) => x.ToString() == slotTypeString);
								if (eUsageType != CItem.EUsageType.None)
								{
									slotUsagesToRefresh.Add(eUsageType);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotUsagesToConsume property '" + slotTypeString + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "OverrideAugmentAttackType":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/OverrideAugmentAttackType", filename, out stringValue))
					{
						CAbility.EAttackType eAttackType = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType x) => x.ToString() == stringValue);
						if (eAttackType != CAbility.EAttackType.None)
						{
							overrideAugmentAttackType = eAttackType;
							abilityType = CAbility.EAbilityType.OverrideAugmentAttackType;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid OverrideAugmentAttackType property '" + stringValue + "' in file '" + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AbilityType":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AbilityType", filename, out stringValue))
					{
						CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == stringValue);
						if (eAbilityType != CAbility.EAbilityType.None)
						{
							abilityType = eAbilityType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AbilityType property '" + stringValue + "' in file '" + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreviewEffectId":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/PreviewEffectId", filename, out stringValue))
					{
						previewEffectId = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreviewEffectText":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/PreviewEffectText", filename, out stringValue))
					{
						previewEffectText = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "HelpBoxTooltipLocKey":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/HelpBoxTooltipLocKey", filename, out stringValue))
					{
						helpBoxTooltipLocKey = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "ReplaceModifier":
				case "ReplaceModifiers":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ReplaceModifiers", filename, out var values6))
					{
						foreach (string item12 in values6)
						{
							if (int.TryParse(item12.Substring(1), out var result3))
							{
								if (result3 < -2 || result3 > 2)
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
							if (!flag)
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid modifer '" + item12 + "' specified for ReplaceModifiers in file " + filename);
								flag = false;
							}
						}
						if (flag)
						{
							miscAbilityData.ReplaceModifiers = values6;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ReplaceWithModifier":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ReplaceWithModifier", filename, out stringValue))
					{
						miscAbilityData.ReplaceWithModifier = stringValue;
						break;
					}
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ReplaceWithModifier property '" + stringValue + "' in file '" + filename);
					flag = false;
					break;
				case "ReplaceWithModifiers":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ReplaceWithModifiers", filename, out var values4))
					{
						miscAbilityData.ReplaceWithModifiers = values4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ReplaceNegativeModifiers":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ReplaceNegativeModifiers", filename, out value))
					{
						miscAbilityData.ReplaceNegativeModifiers = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "ReplaceCondition":
				case "ReplaceConditions":
				{
					if (GetConditions(entry, filename, out var negativeConditions3, out var positiveConditions3))
					{
						if (negativeConditions3.Count > 0)
						{
							miscAbilityData.ReplaceNegativeConditions = negativeConditions3;
						}
						if (positiveConditions3.Count > 0)
						{
							miscAbilityData.ReplacePositiveConditions = positiveConditions3;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ReplaceWithCondition":
				{
					if (GetConditions(entry, filename, out var negativeConditions2, out var positiveConditions2))
					{
						if (negativeConditions2.Count > 0)
						{
							miscAbilityData.ReplaceWithNegativeConditions = negativeConditions2;
						}
						if (positiveConditions2.Count > 0)
						{
							miscAbilityData.ReplaceWithPositiveConditions = positiveConditions2;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CanUndo":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanUndo", filename, out value))
					{
						miscAbilityData.CanUndo = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "CanSkip":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanSkip", filename, out value))
					{
						miscAbilityData.CanSkip = value;
					}
					else
					{
						flag = false;
					}
					break;
				case "RecoverResources":
					abilityType = CAbility.EAbilityType.RecoverResources;
					break;
				case "AddModifiersToMonsterDeck":
				{
					if (!(entry.Value is Mapping) || !YMLShared.GetMapping(entry, filename, out var mapping2))
					{
						break;
					}
					if (!isAbilityOverride && filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
					}
					abilityType = CAbility.EAbilityType.AddModifierToMonsterDeck;
					foreach (MappingEntry entry39 in mapping2.Entries)
					{
						string text = entry39.Key.ToString();
						int value4;
						if (!(text == "Strength"))
						{
							if (text == "ModifiersToAdd")
							{
								if (YMLShared.GetStringList(entry39.Value, "Ability/ModifiersToAdd", filename, out var values3))
								{
									if (values3.Count > 0)
									{
										addOrRemoveModifiers = values3.ToList();
									}
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AddModifiersToMonsterDeck.  Entry " + entry39.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
						else if (YMLShared.GetIntPropertyValue(entry39.Value, "Strength", filename, out value4))
						{
							strength = value4;
						}
						else
						{
							flag = false;
						}
					}
					break;
				}
				case "RepeatAbility":
					abilityType = CAbility.EAbilityType.RepeatAbility;
					break;
				case "DuplicateCondition":
					abilityType = CAbility.EAbilityType.DuplicateCondition;
					break;
				case "PreventLoseCard":
					abilityType = CAbility.EAbilityType.PreventLoseCard;
					break;
				case "AddAttackModifier":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ModifiersToAdd", filename, out var values2))
					{
						abilityType = CAbility.EAbilityType.AddAttackModifier;
						if (values2.Count > 0)
						{
							addOrRemoveModifiers = values2.ToList();
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RemoveAttackModifier":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ModifiersToAdd", filename, out var values))
					{
						abilityType = CAbility.EAbilityType.RemoveAttackModifier;
						if (values.Count > 0)
						{
							addOrRemoveModifiers = values.ToList();
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ToDiscardPile":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ToDiscardPile", filename, out value))
					{
						toDiscardPile = value;
					}
					else
					{
						flag = false;
					}
					break;
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid abilities entry " + entry.Key.ToString() + ". File:\n" + filename);
					flag = false;
					break;
				case "ClearParentProperties":
					break;
				}
			}
			if (areaEffectYMLString != null && areaEffectYMLString.Length > 0)
			{
				areaEffect = CardProcessingShared.CreateArea(areaEffectYMLString, name + "Area", filename);
			}
			if (areaEffect == null && range.HasValue && range.Value > 0 && miscAbilityData.ExactRange.HasValue && miscAbilityData.ExactRange.Value)
			{
				areaEffect = CardProcessingShared.CreateExactRangeAsAreaEffect(range.Value, name + "Area", filename);
				if (!miscAbilityData.TreatAsMelee.HasValue)
				{
					miscAbilityData.TreatAsMelee = false;
				}
			}
			if (mapping != null)
			{
				foreach (MappingEntry entry40 in mapping.Entries)
				{
					if (YMLShared.GetMapping(entry40, filename, out var mapping41))
					{
						if (ProcessAbilityEntry(filename, mapping41, entry40.Key.ToString(), cardID, isMonster, out var ability3, isSubAbility: true, isConsumeAbility: false, filter, strength, range, numberOfTargets, areaEffectYMLString, areaEffectLayoutOverrideYMLString, areaEffectFlipAxis, targeting, areaEffect, isTargetedAbility))
						{
							if (subAbilities == null)
							{
								subAbilities = new List<CAbility>();
							}
							subAbilities.Add(ability3);
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
			}
			return flag;
		}
		catch (Exception ex2)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing Ability Data.\nFile:" + filename, ex2);
		}
		return false;
	}

	private static bool ParseActiveBonusDuration(DataItem di, string filename, bool isMonster, ref ActiveBonusData activeBonusData)
	{
		string stringValue = string.Empty;
		if (YMLShared.GetStringPropertyValue(di, "ActiveBonus/Duration", filename, out stringValue))
		{
			CActiveBonus.EActiveBonusDurationType eActiveBonusDurationType = CActiveBonus.ActiveBonusDurationTypes.SingleOrDefault((CActiveBonus.EActiveBonusDurationType x) => x.ToString() == stringValue);
			if (eActiveBonusDurationType == CActiveBonus.EActiveBonusDurationType.NA)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Duration '" + stringValue + "' in file '" + filename);
				return false;
			}
			if ((eActiveBonusDurationType == CActiveBonus.EActiveBonusDurationType.Persistent) & isMonster)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Cannot add persistent bonus effects to monster cards.  File:" + filename);
				return false;
			}
			activeBonusData.Duration = eActiveBonusDurationType;
			return true;
		}
		return false;
	}

	private static bool ParseActiveBonusData(DataItem di, int cardID, string filename, bool isMonster, ref ActiveBonusData activeBonusData)
	{
		if (di is Scalar)
		{
			return ParseActiveBonusDuration(di, filename, isMonster, ref activeBonusData);
		}
		if (di is Mapping)
		{
			bool result = true;
			string stringValue = string.Empty;
			bool value = false;
			int value2 = 0;
			List<CAbilityOverride> list = new List<CAbilityOverride>();
			{
				foreach (MappingEntry entry2 in (di as Mapping).Entries)
				{
					List<string> values4;
					switch (entry2.Key.ToString())
					{
					case "Flags":
					{
						if (YMLShared.GetStringList(entry2.Value, "ActiveBonus/Flags", filename, out var values3))
						{
							EActiveBonusFlag eActiveBonusFlag = EActiveBonusFlag.None;
							foreach (string item in values3)
							{
								if (Enum.TryParse<EActiveBonusFlag>(item, out var result2))
								{
									eActiveBonusFlag |= result2;
								}
							}
							activeBonusData.Flags = eActiveBonusFlag;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Behaviour":
					{
						activeBonusData.Behaviours = new List<CActiveBonus.EActiveBonusBehaviourType>();
						if (YMLShared.GetStringList(entry2.Value, "ActiveBonus/Behaviour", filename, out var values6))
						{
							foreach (string entry in values6)
							{
								CActiveBonus.EActiveBonusBehaviourType eActiveBonusBehaviourType = CActiveBonus.ActiveBonusTypes.SingleOrDefault((CActiveBonus.EActiveBonusBehaviourType s) => s.ToString() == entry);
								if (eActiveBonusBehaviourType != CActiveBonus.EActiveBonusBehaviourType.None)
								{
									activeBonusData.Behaviours.Add(eActiveBonusBehaviourType);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Behaviour '" + entry + "' in file '" + filename);
								result = false;
							}
						}
						else if (YMLShared.GetStringPropertyValue(entry2.Value, "ActiveBonus/Behaviour", filename, out stringValue))
						{
							CActiveBonus.EActiveBonusBehaviourType eActiveBonusBehaviourType2 = CActiveBonus.ActiveBonusTypes.SingleOrDefault((CActiveBonus.EActiveBonusBehaviourType s) => s.ToString() == stringValue);
							if (eActiveBonusBehaviourType2 != CActiveBonus.EActiveBonusBehaviourType.None)
							{
								activeBonusData.Behaviours.Add(eActiveBonusBehaviourType2);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Behaviour '" + stringValue + "' in file '" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Duration":
						if (!ParseActiveBonusDuration(entry2.Value, filename, isMonster, ref activeBonusData))
						{
							result = false;
						}
						break;
					case "Restriction":
						if (YMLShared.GetStringPropertyValue(entry2.Value, "ActiveBonus/Restriction", filename, out stringValue))
						{
							CActiveBonus.EActiveBonusRestrictionType eActiveBonusRestrictionType = CActiveBonus.ActiveBonusRestrictionTypes.SingleOrDefault((CActiveBonus.EActiveBonusRestrictionType s) => s.ToString() == stringValue);
							if (eActiveBonusRestrictionType != CActiveBonus.EActiveBonusRestrictionType.None)
							{
								activeBonusData.Restriction = eActiveBonusRestrictionType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Restriction '" + stringValue + "' in file '" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "Vars":
					{
						if (GetVars(entry2, filename, out var vars))
						{
							activeBonusData.Vars = vars;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "MiscVars":
					{
						if (GetVars(entry2, filename, out var vars2))
						{
							activeBonusData.MiscVars = vars2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "PerformVars":
					{
						if (GetPerformVars(entry2, filename, out var performVars))
						{
							activeBonusData.PerformVars = performVars;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Requirement":
					case "Requirements":
					{
						if (GetAbilityRequirements(entry2, filename, out var abilityRequirements))
						{
							activeBonusData.Requirements = abilityRequirements;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "MoveType":
					case "AttackType":
						if (YMLShared.GetStringPropertyValue(entry2.Value, "ActiveBonus/AttackType", filename, out stringValue))
						{
							CAbility.EAttackType eAttackType = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType x) => x.ToString() == stringValue);
							if (eAttackType != CAbility.EAttackType.None)
							{
								activeBonusData.AttackType = eAttackType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Attack Type '" + stringValue + "' in file '" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "ValidAbilityTypes":
					case "ValidAbilityType":
						if (YMLShared.GetStringList(entry2.Value, "ActiveBonus/ValidAbilityType", filename, out values4))
						{
							activeBonusData.ValidAbilityTypes = new List<CAbility.EAbilityType>();
							foreach (string validTypeString2 in values4)
							{
								CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == validTypeString2);
								if (eAbilityType != CAbility.EAbilityType.None)
								{
									if (!activeBonusData.ValidAbilityTypes.Contains(eAbilityType))
									{
										activeBonusData.ValidAbilityTypes.Add(eAbilityType);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Valid Ability Type '" + validTypeString2 + "' in file '" + filename);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ValidTargetTypes":
					case "ValidTargetType":
					{
						if (YMLShared.GetStringList(entry2.Value, "ActiveBonus/ValidTargetType", filename, out var values))
						{
							activeBonusData.ValidTargetTypes = new List<CAbilityFilter.EFilterTargetType>();
							foreach (string validTypeString in values)
							{
								CAbilityFilter.EFilterTargetType eFilterTargetType = CAbilityFilter.FilterTargetTypes.SingleOrDefault((CAbilityFilter.EFilterTargetType x) => x.ToString() == validTypeString);
								if (eFilterTargetType != CAbilityFilter.EFilterTargetType.None)
								{
									if (!activeBonusData.ValidTargetTypes.Contains(eFilterTargetType))
									{
										activeBonusData.ValidTargetTypes.Add(eFilterTargetType);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Valid Target Type '" + validTypeString + "' in file '" + filename);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ValidTargetTypesExclusive":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "ValidTargetTypesExclusive", filename, out value))
						{
							activeBonusData.ValidTargetTypesExclusive = value;
						}
						else
						{
							result = false;
						}
						break;
					case "InvertValidity":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "InvertValidity", filename, out value))
						{
							activeBonusData.InvertValidity = value;
						}
						else
						{
							result = false;
						}
						break;
					case "ValidConditions":
					{
						if (GetConditions(entry2, filename, out var negativeConditions, out var positiveConditions))
						{
							if (positiveConditions.Count > 0)
							{
								activeBonusData.ValidPositiveConditionTypes = positiveConditions;
							}
							if (negativeConditions.Count > 0)
							{
								activeBonusData.ValidNegativeConditionTypes = negativeConditions;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ProcXP":
						if (YMLShared.GetIntPropertyValue(entry2.Value, "ActiveBonus/ProcXP", filename, out value2))
						{
							activeBonusData.ProcXP = value2;
						}
						else
						{
							result = false;
						}
						break;
					case "StrengthIsScalar":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "ActiveBonusIsScalar", filename, out value))
						{
							activeBonusData.StrengthIsScalar = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AbilityStrengthIsScalar":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "ActiveBonusIsScalar", filename, out value))
						{
							activeBonusData.AbilityStrengthIsScalar = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AlwaysBuff":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "AlwaysBuff", filename, out value))
						{
							activeBonusData.AlwaysBuff = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Filter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry2, filename, out var filter2))
						{
							activeBonusData.Filter = filter2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "FilterContainer":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry2, filename, out var filter))
						{
							activeBonusData.Filter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "IsAura":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "ActiveBonusIsAura", filename, out value))
						{
							activeBonusData.IsAura = value;
						}
						else
						{
							result = false;
						}
						break;
					case "DisplayAuraOnCaster":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "DisplayAuraOnCaster", filename, out value))
						{
							activeBonusData.DisplayAuraOnCaster = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AuraFilter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry2, filename, out var filter4))
						{
							activeBonusData.AuraFilter = filter4;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AuraFilterContainer":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry2, filename, out var filter3))
						{
							activeBonusData.AuraFilter = filter3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AuraTargeting":
						if (YMLShared.GetStringPropertyValue(entry2.Value, "Ability/AuraTargeting", filename, out stringValue))
						{
							CAbility.EAbilityTargeting eAbilityTargeting = CAbility.AbilityTargetingTypes.SingleOrDefault((CAbility.EAbilityTargeting x) => x.ToString() == stringValue);
							if (eAbilityTargeting != CAbility.EAbilityTargeting.None)
							{
								activeBonusData.AuraTargeting = eAbilityTargeting;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Aura Targeting Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "AbilityData":
					{
						if (ProcessAbilityEntry(filename, entry2.Value as Mapping, entry2.Key.ToString(), cardID, isMonster, out var ability2))
						{
							activeBonusData.AbilityData = ability2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Consuming":
					{
						if (CardProcessingShared.GetElements(entry2.Value, "Consuming", filename, out var elements))
						{
							activeBonusData.Consuming.AddRange(elements);
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CostAbility":
					{
						if (ProcessAbilityEntry(filename, entry2.Value as Mapping, entry2.Key.ToString(), cardID, isMonster, out var ability))
						{
							activeBonusData.CostAbility = ability;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "XPTracker":
					{
						if (YMLShared.GetIntList(entry2.Value, "XPTracker", filename, out var values2))
						{
							activeBonusData.Tracker = values2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "IsToggleBonus":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "IsToggleBonus", filename, out value))
						{
							activeBonusData.IsToggleBonus = value;
							if (!activeBonusData.HasBehaviour())
							{
								activeBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.DefaultToggleBehaviour;
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ToggleIsOptional":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "ToggleIsOptional", filename, out value))
						{
							activeBonusData.ToggleIsOptional = value;
						}
						else
						{
							result = false;
						}
						break;
					case "RestrictPerActor":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "RestrictPerActor", filename, out value))
						{
							activeBonusData.RestrictPerActor = value;
						}
						else
						{
							result = false;
						}
						break;
					case "OverrideAsSong":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "OverrideAsSong", filename, out value))
						{
							activeBonusData.OverrideAsSong = value;
						}
						else
						{
							result = false;
						}
						break;
					case "IsSingleTargetBonus":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "IsSingleTargetBonus", filename, out value))
						{
							activeBonusData.IsSingleTargetBonus = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Target":
						if (YMLShared.GetIntPropertyValue(entry2.Value, "Target", filename, out value2))
						{
							activeBonusData.TargetCount = value2;
						}
						else
						{
							result = false;
						}
						break;
					case "ActiveBonusAbilityOverrides":
					{
						if (YMLShared.GetMapping(entry2, filename, out var mapping))
						{
							foreach (MappingEntry entry3 in mapping.Entries)
							{
								CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry3, cardID, isMonster, filename);
								if (cAbilityOverride != null)
								{
									list.Add(cAbilityOverride);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Failed to create active bonus ability override.  File: " + filename);
								result = false;
							}
							activeBonusData.ActiveBonusAbilityOverrides = list;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides in Augment.  Entry must be mapping.  File: " + filename);
							result = false;
						}
						break;
					}
					case "ActiveBonusAnimOverload":
						if (YMLShared.GetStringPropertyValue(entry2.Value, "ActiveBonusAnimOverload", filename, out stringValue))
						{
							activeBonusData.ActiveBonusAnimOverload = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "UseTriggerAbilityAsParent":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "UseTriggerAbilityAsParent", filename, out value))
						{
							activeBonusData.UseTriggerAbilityAsParent = value;
						}
						else
						{
							result = false;
						}
						break;
					case "GiveAbilityCardToActor":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "GiveAbilityCardToActor", filename, out value))
						{
							activeBonusData.GiveAbilityCardToActor = value;
						}
						else
						{
							result = false;
						}
						break;
					case "StartsRestricted":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "StartsRestricted", filename, out value))
						{
							activeBonusData.StartsRestricted = value;
						}
						else
						{
							result = false;
						}
						break;
					case "EntireAction":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "EntireAction", filename, out value))
						{
							activeBonusData.EntireAction = value;
						}
						else
						{
							result = false;
						}
						break;
					case "OriginalTargetType":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "OriginalTargetType", filename, out value))
						{
							activeBonusData.OriginalTargetType = value;
						}
						else
						{
							result = false;
						}
						break;
					case "SetFilterToCaster":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "SetFilterToCaster", filename, out value))
						{
							activeBonusData.SetFilterToCaster = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TriggerOnCaster":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "TriggerOnCaster", filename, out value))
						{
							activeBonusData.TriggerOnCaster = value;
						}
						else
						{
							result = false;
						}
						break;
					case "CannotCancel":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "CannotCancel", filename, out value))
						{
							activeBonusData.CannotCancel = value;
						}
						else
						{
							result = false;
						}
						break;
					case "EndAllActiveBonusesOnBaseCardSimultaneously":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "EndAllActiveBonusesOnBaseCardSimultaneously", filename, out value))
						{
							activeBonusData.EndAllActiveBonusesOnBaseCardSimultaneously = value;
						}
						else
						{
							result = false;
						}
						break;
					case "RequiredResources":
						if (entry2.Value is Sequence)
						{
							activeBonusData.RequiredResources = new Dictionary<string, int>();
							Sequence sequence = entry2.Value as Sequence;
							if (sequence.Entries[0] is Sequence)
							{
								foreach (DataItem entry4 in sequence.Entries)
								{
									if (YMLShared.GetTupleStringInt(entry4, "RequiredResources", filename, out var tuple))
									{
										if (activeBonusData.RequiredResources.ContainsKey(tuple.Item1))
										{
											activeBonusData.RequiredResources[tuple.Item1] += tuple.Item2;
										}
										else
										{
											activeBonusData.RequiredResources.Add(tuple.Item1, tuple.Item2);
										}
									}
									else
									{
										result = false;
									}
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid RequiredResources entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid RequiredResources entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							result = false;
						}
						break;
					case "ConsumeResources":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "ConsumeResources", filename, out value))
						{
							activeBonusData.ConsumeResources = value;
						}
						else
						{
							result = false;
						}
						break;
					case "OnlyWorkAfterFinishedTracker":
					{
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "OnlyWorkAfterFinishedTracker", filename, out var value3))
						{
							activeBonusData.OnlyWorkAfterFinishedTracker = value3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ListLayoutOverride":
						if (YMLShared.GetStringList(entry2.Value, "ListLayoutOverride", filename, out values4))
						{
							if (values4.Count == 2)
							{
								activeBonusData.ListLayoutOverride = (values4[0], values4[1]);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ListLayoutOverride entry '" + entry2.Value?.ToString() + ", expected format [$LocEntry$, Icon]'. File:\n" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "AuraTriggerAbilityAnimType":
						if (YMLShared.GetStringPropertyValue(entry2.Value, "AuraTriggerAbilityAnimType", filename, out stringValue))
						{
							CActiveBonus.EAuraTriggerAbilityAnimType auraAnimTriggerAbilityType = CActiveBonus.AuraTriggerAbilityAnimTypes.SingleOrDefault((CActiveBonus.EAuraTriggerAbilityAnimType x) => x.ToString() == stringValue);
							activeBonusData.AuraAnimTriggerAbilityType = auraAnimTriggerAbilityType;
						}
						else
						{
							result = false;
						}
						break;
					case "CancelActiveBonusesOnBaseCardIDs":
					{
						if (YMLShared.GetIntList(entry2.Value, "CancelActiveBonusesOnBaseCardIDs", filename, out var values5))
						{
							activeBonusData.CancelActiveBonusesOnBaseCardIDs = values5;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for CancelActiveBonusesOnBaseCardIDs in ActiveBonusData.  File: " + filename);
						result = false;
						break;
					}
					case "RemoveAllInstancesOfResourcesOnFinish":
						if (YMLShared.GetStringList(entry2.Value, "RemoveAllInstancesOfResourcesOnFinish", filename, out values4))
						{
							activeBonusData.RemoveAllInstancesOfResourcesOnFinish = values4.ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for CancelActiveBonusesOnBaseCardIDs in ActiveBonusData.  File: " + filename);
						result = false;
						break;
					case "ArbitraryParams":
						if (YMLShared.GetStringList(entry2.Value, "ArbitraryParams", filename, out values4))
						{
							activeBonusData.ArbitraryParams = values4.ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for CancelActiveBonusesOnBaseCardIDs in ActiveBonusData.  File: " + filename);
						result = false;
						break;
					case "Hidden":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "Hidden", filename, out value))
						{
							activeBonusData.Hidden = value;
						}
						else
						{
							result = false;
						}
						break;
					case "HiddenWhenAura":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "HiddenWhenAura", filename, out value))
						{
							activeBonusData.HiddenWhenAura = value;
						}
						else
						{
							result = false;
						}
						break;
					case "HiddenInLog":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "HiddenInLog", filename, out value))
						{
							activeBonusData.HiddenInLog = value;
						}
						else
						{
							result = false;
						}
						break;
					case "GroupID":
						if (YMLShared.GetStringPropertyValue(entry2.Value, "GroupID", filename, out stringValue))
						{
							activeBonusData.GroupID = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "FullCopy":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "FullCopy", filename, out value))
						{
							activeBonusData.FullCopy = value;
						}
						else
						{
							result = false;
						}
						break;
					case "NeedsDamage":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "NeedsDamage", filename, out value))
						{
							activeBonusData.NeedsDamage = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TriggerAbilitiesImmediately":
						if (YMLShared.GetBoolPropertyValue(entry2.Value, "TriggerAbilitiesImmediately", filename, out value))
						{
							activeBonusData.TriggerAbilitiesImmediately = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ActiveBonus entry '" + entry2.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ActiveBonus value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseTrapData(DataItem di, string filename, ref TrapData trapData, List<EEnhancementMarkType> enhancementSlots)
	{
		if (di is Mapping)
		{
			bool result = true;
			string stringValue = string.Empty;
			int value = 0;
			trapData = new TrapData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "TrapName":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TrapData/TrapName", filename, out stringValue))
						{
							trapData.TrapName = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "Damage":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/Damage", filename, out value))
						{
							trapData.Damage = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Heal":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/Heal", filename, out value))
						{
							trapData.Heal = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Conditions":
					{
						if (GetConditions(entry, filename, out var negativeConditions, out var positiveConditions))
						{
							if (negativeConditions.Count > 0)
							{
								trapData.Conditions = negativeConditions;
							}
							if (positiveConditions.Count > 0)
							{
								trapData.PositiveConditions = positiveConditions;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AdjacentRange":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/AdjacentRange", filename, out value))
						{
							trapData.AdjacentRange = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AdjacentDamage":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/AdjacentDamage", filename, out value))
						{
							trapData.AdjacentDamage = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AdjacentConditions":
					{
						if (GetConditions(entry, filename, out var negativeConditions2, out var _))
						{
							if (negativeConditions2.Count > 0)
							{
								trapData.AdjacentConditions = negativeConditions2;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AdjacentFilter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter))
						{
							trapData.AdjacentFilter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TriggeredXP":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/TriggeredXP", filename, out value))
						{
							trapData.TriggeredXP = value;
						}
						else
						{
							result = false;
						}
						break;
					case "PlacementTileFilter":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TrapData/PlacementTileFilter", filename, out stringValue))
						{
							CAbilityFilter.EFilterTile placementTileFilter = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
							trapData.PlacementTileFilter = placementTileFilter;
						}
						else
						{
							result = false;
						}
						break;
					default:
					{
						if (!ParseEnhancement("TrapData", filename, entry, enhancementSlots, out var _))
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TrapData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
							result = false;
						}
						break;
					}
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TrapData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseDestroyObstacleData(DataItem di, string filename, ref CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			string value2 = string.Empty;
			destroyObstacleData = new CAbilityDestroyObstacle.DestroyObstacleData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "CanTargetEmptyHexes":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "CanTargetEmptyHexes", filename, out value))
						{
							destroyObstacleData.CanTargetEmptyHexes = value;
						}
						else
						{
							result = false;
						}
						break;
					case "CanDestroyObstaclesWithHP":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "CanDestroyObstaclesWithHP", filename, out value))
						{
							destroyObstacleData.CanDestroyObstaclesWithHP = value;
						}
						else
						{
							result = false;
						}
						break;
					case "BypassOverrideDisallowDestroyAndMove":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "BypassOverrideDisallowDestroyAndMove", filename, out value))
						{
							destroyObstacleData.BypassOverrideDisallowDestroyAndMove = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TargetPrefabName":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetPrefabName", filename, out value2))
						{
							destroyObstacleData.TargetPrefabName = value2;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DestroyObstacleData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DestroyObstacleData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseTeleportData(DataItem di, string filename, ref CAbilityTeleport.TeleportData teleportData)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			string stringValue = string.Empty;
			teleportData = new CAbilityTeleport.TeleportData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "TeleportType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TeleportType", filename, out stringValue))
						{
							CAbilityTeleport.ETeleportType eTeleportType = CAbilityTeleport.TeleportTypes.SingleOrDefault((CAbilityTeleport.ETeleportType x) => x.ToString() == stringValue);
							if (eTeleportType != CAbilityTeleport.ETeleportType.None)
							{
								teleportData.TeleportType = eTeleportType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Teleport Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "PropType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "PropType", filename, out stringValue))
						{
							EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == stringValue);
							if (ePropType != EPropType.None)
							{
								teleportData.PropType = ePropType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Prop Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "MoveOtherThingsOffTiles":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "MoveOtherThingsOffTiles", filename, out value))
						{
							teleportData.MoveOtherThingsOffTiles = value;
						}
						else
						{
							result = false;
						}
						break;
					case "ShouldOpenDoorsToTeleportedLocation":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ShouldOpenDoorsToTeleportedLocation", filename, out value))
						{
							teleportData.ShouldOpenDoorsToTeleportedLocation = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TeleportData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TeleportData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseControlActorData(DataItem di, string filename, ref CAbilityControlActor.ControlActorAbilityData controlActorData, int cardID, bool isMonster)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			_ = string.Empty;
			controlActorData = new CAbilityControlActor.ControlActorAbilityData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Abilities":
					{
						if (CardProcessingShared.GetAbilities(entry, cardID, isMonster, filename, out var abilities))
						{
							controlActorData.ControlAbilities = abilities;
							controlActorData.ControlAbilities.ForEach((CAbility x) =>
							{
								x.IsControlAbility = true;
							});
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ControlDurationType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ControlDurationType", filename, out var controlDurationTypeValue))
						{
							CAbilityControlActor.EControlDurationType eControlDurationType = CAbilityControlActor.ControlDurationTypes.SingleOrDefault((CAbilityControlActor.EControlDurationType x) => x.ToString() == controlDurationTypeValue);
							if (eControlDurationType != CAbilityControlActor.EControlDurationType.None)
							{
								controlActorData.ControlDurationType = eControlDurationType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for ControlDurationType in file " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ControlActorType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ControlActorType", filename, out var controlActorTypeValue))
						{
							CActor.EType value2 = CActor.Types.SingleOrDefault((CActor.EType x) => x.ToString() == controlActorTypeValue);
							controlActorData.ControlType = value2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TransferAugments":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TransferAugments", filename, out value))
						{
							controlActorData.TransferAugments = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TreatControlAbilityParentAsParent":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatControlAbilityParentAsParent", filename, out value))
						{
							controlActorData.TreatControlAbilityParentAsParent = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TreatPreviousAbilityToControlAbilityAsParent":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatPreviousAbilityToControlAbilityAsParent", filename, out value))
						{
							controlActorData.TreatPreviousAbilityToControlAbilityAsParent = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseControllingActorModifierDeckFilter":
					{
						if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter))
						{
							controlActorData.UseControllingActorModifierDeckFilter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "UseControllingActorModifierDeck":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseControllingActorModifierDeck", filename, out value))
						{
							controlActorData.UseControllingActorModifierDeck = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseControllingActorPlayerControl":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseControllingActorPlayerControl", filename, out value))
						{
							controlActorData.UseControllingActorPlayerControl = value;
						}
						else
						{
							result = false;
						}
						break;
					case "ChooseClosestActor":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ChooseClosestActor", filename, out value))
						{
							controlActorData.ChooseClosestActor = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseControllingActorType":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseControllingActorType", filename, out value))
						{
							controlActorData.UseControllingActorType = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ControlActorAbilityData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ControlActorAbilityData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseChangeAllegianceData(DataItem di, string filename, ref CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData, int cardID, bool isMonster)
	{
		if (di is Mapping)
		{
			bool result = true;
			_ = string.Empty;
			CActor.EType eType = CActor.EType.Player;
			foreach (MappingEntry entry in (di as Mapping).Entries)
			{
				if (entry.Key.ToString() == "ChangeToType")
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "ChangeToType", filename, out var controlActorTypeValue))
					{
						eType = CActor.Types.SingleOrDefault((CActor.EType x) => x.ToString() == controlActorTypeValue);
					}
					else
					{
						result = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ChangeAllegianceAbilityData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
					result = false;
				}
			}
			if (eType != CActor.EType.Enemy && eType != CActor.EType.Ally && eType != CActor.EType.Enemy2 && eType != CActor.EType.Neutral)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ChangeToType ActorType in ChangeAllegianceAbilityData entry. File:\n" + filename);
				result = false;
			}
			changeAllegianceData = new CAbilityChangeAllegiance.ChangeAllegianceAbilityData(eType);
			return result;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ChangeAllegianceAbilityData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseLootData(DataItem di, string filename, ref CAbilityLoot.LootData lootData)
	{
		if (di is Mapping)
		{
			bool result = true;
			string stringValue = string.Empty;
			List<string> values = new List<string>();
			lootData = CAbilityLoot.LootData.CreateDefaultLootData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					if (!(text == "LootableObjectImportTypes"))
					{
						if (text == "AdditionalLootEffect")
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "AdditionalLootEffect", filename, out stringValue))
							{
								CAbilityLoot.EAdditionalLootEffect eAdditionalLootEffect = CAbilityLoot.AdditionalLootEffects.SingleOrDefault((CAbilityLoot.EAdditionalLootEffect x) => x.ToString() == stringValue);
								if (eAdditionalLootEffect != CAbilityLoot.EAdditionalLootEffect.None)
								{
									lootData.AdditionalLootEffect = eAdditionalLootEffect;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AdditionalLootEffect Type '" + stringValue + "'.  File: " + filename);
								result = false;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid LootData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
							result = false;
						}
					}
					else if (YMLShared.GetStringList(entry.Value, "LootableObjectImportTypes", filename, out values))
					{
						lootData.LootableObjectImportTypes = new List<ScenarioManager.ObjectImportType>();
						foreach (string lootableTypeString in values)
						{
							ScenarioManager.ObjectImportType objectImportType = ScenarioManager.ObjectImportTypes.SingleOrDefault((ScenarioManager.ObjectImportType x) => x.ToString() == lootableTypeString);
							if (objectImportType != ScenarioManager.ObjectImportType.None)
							{
								lootData.LootableObjectImportTypes.Add(objectImportType);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Object Import Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid LootData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseHealData(DataItem di, string filename, ref CAbilityHeal.HealAbilityData healData)
	{
		if (di is Mapping)
		{
			bool result = true;
			healData = new CAbilityHeal.HealAbilityData();
			List<CCondition.ENegativeCondition> negativeConditions = null;
			List<CCondition.EPositiveCondition> positiveConditions = null;
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "IgnoreTokens":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreTokens", filename, out var value))
						{
							healData.IgnoreTokens = value;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ConditionsToAddIfHealRemovesPoison":
						if (GetConditions(entry, filename, out negativeConditions, out positiveConditions))
						{
							if (negativeConditions.Count > 0)
							{
								healData.NegativeConditionsToAddIfHealRemovesPoison = negativeConditions.ToList();
							}
							if (positiveConditions.Count > 0)
							{
								healData.PositiveConditionsToAddIfHealRemovesPoison = positiveConditions.ToList();
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ApplyConditionsOnRemovingPoisonFilter":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter))
						{
							healData.ApplyConditionsOnRemovingPoisonFilter = filter.Copy();
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ConditionsToAddIfHealRemovesWound":
						if (GetConditions(entry, filename, out negativeConditions, out positiveConditions))
						{
							if (negativeConditions.Count > 0)
							{
								healData.NegativeConditionsToAddIfHealRemovesWound = negativeConditions.ToList();
							}
							if (positiveConditions.Count > 0)
							{
								healData.PositiveConditionsToAddIfHealRemovesWound = positiveConditions.ToList();
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ApplyConditionsOnRemovingWoundFilter":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter2))
						{
							healData.ApplyConditionsOnRemovingWoundFilter = filter2.Copy();
						}
						else
						{
							result = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid HealData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid HealData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseDisableCardActionData(DataItem di, string filename, ref CAbilityDisableCardAction.DisableCardActionData disableData)
	{
		if (di is Mapping)
		{
			bool result = true;
			disableData = new CAbilityDisableCardAction.DisableCardActionData();
			CBaseCard.ActionType actionType = CBaseCard.ActionType.NA;
			{
				string stringValue;
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					if (!(text == "CardName"))
					{
						if (text == "DisableActionType")
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "DisableActionType", filename, out stringValue))
							{
								actionType = CBaseCard.ActionTypes.SingleOrDefault((CBaseCard.ActionType x) => x.ToString() == stringValue);
								if (actionType != CBaseCard.ActionType.NA)
								{
									disableData.DisableActionType = actionType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DisableActionType '" + stringValue + "'.  File: " + filename);
								result = false;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DisableCardActionData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
							result = false;
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "CardName", filename, out stringValue))
					{
						disableData.CardName = stringValue;
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DisableCardActionData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	public static bool ParseAIFocusOverride(DataItem di, string filename, ref CAIFocusOverrideDetails aiFocusOverride)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			string stringValue = string.Empty;
			aiFocusOverride = new CAIFocusOverrideDetails();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "OverrideType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "OverrideType", filename, out stringValue))
						{
							CAIFocusOverrideDetails.EOverrideType eOverrideType = CAIFocusOverrideDetails.OverrideTypes.SingleOrDefault((CAIFocusOverrideDetails.EOverrideType x) => x.ToString() == stringValue);
							if (eOverrideType != CAIFocusOverrideDetails.EOverrideType.None)
							{
								aiFocusOverride.OverrideType = eOverrideType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AI Override Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "OverrideTargetType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "OverrideTargetType", filename, out stringValue))
						{
							CAIFocusOverrideDetails.EOverrideTargetType eOverrideTargetType = CAIFocusOverrideDetails.OverrideTargetTypes.SingleOrDefault((CAIFocusOverrideDetails.EOverrideTargetType x) => x.ToString() == stringValue);
							if (eOverrideTargetType != CAIFocusOverrideDetails.EOverrideTargetType.None)
							{
								aiFocusOverride.OverrideTargetType = eOverrideTargetType;
								aiFocusOverride.IsTemporary = true;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AI Override Target Type '" + stringValue + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					case "TargetGUID":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetGUID", filename, out stringValue))
						{
							aiFocusOverride.TargetGUID = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "TargetClassID":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetClassID", filename, out stringValue))
						{
							aiFocusOverride.TargetClassID = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "TargetPropType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetPropType", filename, out stringValue))
						{
							EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == stringValue);
							if (ePropType != EPropType.None)
							{
								aiFocusOverride.TargetPropType = ePropType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AI Override Target Prop Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "DisallowDoorways":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "DisallowDoorways", filename, out value))
						{
							aiFocusOverride.DisallowDoorways = value;
						}
						else
						{
							result = false;
						}
						break;
					case "FocusBenign":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "FocusBenign", filename, out value))
						{
							aiFocusOverride.FocusBenign = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseFurthestFocus":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseFurthestFocus", filename, out value))
						{
							aiFocusOverride.UseFurthestFocus = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseSummonerToMeasureFrom":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseSummonerToMeasureFrom", filename, out value))
						{
							aiFocusOverride.UseSummonerToMeasureFrom = value;
						}
						else
						{
							result = false;
						}
						break;
					case "IgnoreActivatedProps":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreActivatedProps", filename, out value))
						{
							aiFocusOverride.IgnoreActivatedProps = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AIFocusOverrideDetails entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AIFocusOverrideDetails value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	public static bool ParseStatIsBasedOnX(MappingEntry statIsBasedOnXEntry, string filename, out List<StatIsBasedOnXData> statIsBasedOnXEntries, bool isBaseStats)
	{
		bool flag = true;
		statIsBasedOnXEntries = new List<StatIsBasedOnXData>();
		if (statIsBasedOnXEntry.Value is Mapping mapping)
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				StatIsBasedOnXData statIsBasedOnXData = new StatIsBasedOnXData();
				string statString = entry.Key.ToString();
				if (isBaseStats)
				{
					EMonsterBaseStats eMonsterBaseStats = MonsterYMLData.MonsterBaseStatsEnums.SingleOrDefault((EMonsterBaseStats x) => x.ToString() == statString);
					if (eMonsterBaseStats != EMonsterBaseStats.None)
					{
						statIsBasedOnXData.BaseStatType = eMonsterBaseStats;
						switch (eMonsterBaseStats)
						{
						case EMonsterBaseStats.Attack:
						case EMonsterBaseStats.Move:
							statIsBasedOnXData.AbilityStatType = CAbility.EAbilityStatType.Strength;
							break;
						case EMonsterBaseStats.Range:
							statIsBasedOnXData.AbilityStatType = CAbility.EAbilityStatType.Range;
							break;
						case EMonsterBaseStats.Target:
							statIsBasedOnXData.AbilityStatType = CAbility.EAbilityStatType.NumberOfTargets;
							break;
						}
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					if (statString == "Target")
					{
						statString = CAbility.EAbilityStatType.NumberOfTargets.ToString();
					}
					CAbility.EAbilityStatType eAbilityStatType = CAbility.AbilityStatTypes.SingleOrDefault((CAbility.EAbilityStatType x) => x.ToString() == statString);
					if (eAbilityStatType != CAbility.EAbilityStatType.None)
					{
						statIsBasedOnXData.AbilityStatType = eAbilityStatType;
					}
					else
					{
						flag = false;
					}
				}
				if (flag)
				{
					if (entry.Value is Mapping mapping2)
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							switch (entry2.Key.ToString())
							{
							case "BasedOn":
							{
								if (YMLShared.GetStringPropertyValue(entry2.Value, "BasedOn", filename, out var stringValue))
								{
									CAbility.EStatIsBasedOnXType eStatIsBasedOnXType = CAbility.StatIsBasedOnXTypes.SingleOrDefault((CAbility.EStatIsBasedOnXType x) => x.ToString() == stringValue);
									if (eStatIsBasedOnXType != CAbility.EStatIsBasedOnXType.None)
									{
										statIsBasedOnXData.BasedOn = eStatIsBasedOnXType;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid BasedOn property '" + stringValue + "' in file '" + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "RoundingType":
							{
								if (YMLShared.GetStringPropertyValue(entry2.Value, "RoundingType", filename, out var stringValue2))
								{
									CAbility.EStatIsBasedOnXRoundingType eStatIsBasedOnXRoundingType = CAbility.RoundingTypes.SingleOrDefault((CAbility.EStatIsBasedOnXRoundingType x) => x.ToString() == stringValue2);
									if (eStatIsBasedOnXRoundingType != CAbility.EStatIsBasedOnXRoundingType.None)
									{
										statIsBasedOnXData.RoundingType = eStatIsBasedOnXRoundingType;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid RoundingType property '" + stringValue2 + "' in file '" + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "BaseStatType":
							{
								if (YMLShared.GetStringPropertyValue(entry2.Value, "BaseStatType", filename, out var stringValue3))
								{
									EMonsterBaseStats eMonsterBaseStats2 = MonsterYMLData.MonsterBaseStatsEnums.SingleOrDefault((EMonsterBaseStats x) => x.ToString() == stringValue3);
									if (eMonsterBaseStats2 != EMonsterBaseStats.None)
									{
										statIsBasedOnXData.BaseStatType = eMonsterBaseStats2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid BaseStatType property '" + stringValue3 + "' in file '" + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "AddTo":
							{
								if (YMLShared.GetBoolPropertyValue(entry2.Value, "AddTo", filename, out var value2))
								{
									statIsBasedOnXData.AddTo = value2;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "xVariable":
							case "Multiplier":
							{
								if (YMLShared.GetFloatPropertyValue(entry2.Value, "Multiplier", filename, out var value5))
								{
									statIsBasedOnXData.Multiplier = value5;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "yVariable":
							case "SecondVariable":
							{
								if (YMLShared.GetFloatPropertyValue(entry2.Value, "SecondVariable", filename, out var value))
								{
									statIsBasedOnXData.SecondVariable = value;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Filter":
							{
								if (CardProcessingShared.GetSingleAbilityFilter(entry2, filename, out var filter))
								{
									statIsBasedOnXData.Filter = filter;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "IncludeUnrevealed":
							{
								if (YMLShared.GetBoolPropertyValue(entry2.Value, "IncludeUnrevealed", filename, out var value3))
								{
									statIsBasedOnXData.IncludeUnrevealed = value3;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MinLValue":
							{
								if (YMLShared.GetIntPropertyValue(entry2.Value, "MinLValue", filename, out var value7))
								{
									statIsBasedOnXData.MinLValue = value7;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MinValue":
							{
								if (YMLShared.GetIntPropertyValue(entry2.Value, "MinValue", filename, out var value6))
								{
									statIsBasedOnXData.MinValue = value6;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MaxValue":
							{
								if (YMLShared.GetIntPropertyValue(entry2.Value, "MaxValue", filename, out var value4))
								{
									statIsBasedOnXData.MaxValue = value4;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "CheckGUIDs":
							{
								if (YMLShared.GetStringList(entry2.Value, "CheckGUID", filename, out var values))
								{
									statIsBasedOnXData.CheckGUIDs = values.ToList();
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Vars":
							{
								if (GetVars(entry2, filename, out var vars))
								{
									statIsBasedOnXData.Vars = vars;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX Stat value for stat type " + entry2.Key.ToString() + ". File:\n" + filename);
								flag = false;
								break;
							}
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX Stat value for stat type " + statString + ", must be Mapping type. File:\n" + filename);
						flag = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX Stat type " + statString + ". File:\n" + filename);
					flag = false;
				}
				statIsBasedOnXEntries.Add(statIsBasedOnXData);
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX value, must be Mapping type. File:\n" + filename);
			flag = false;
		}
		return flag;
	}

	public static bool GetConditions(MappingEntry abilityEntry, string filename, out List<CCondition.ENegativeCondition> negativeConditions, out List<CCondition.EPositiveCondition> positiveConditions)
	{
		negativeConditions = new List<CCondition.ENegativeCondition>();
		positiveConditions = new List<CCondition.EPositiveCondition>();
		try
		{
			bool result = true;
			if (YMLShared.GetStringList(abilityEntry.Value, "Conditions", filename, out var values))
			{
				foreach (string condition in values)
				{
					CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == condition);
					if (eNegativeCondition != CCondition.ENegativeCondition.NA)
					{
						if (negativeConditions == null)
						{
							negativeConditions = new List<CCondition.ENegativeCondition>();
						}
						negativeConditions.Add(eNegativeCondition);
						continue;
					}
					CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == condition);
					if (ePositiveCondition != CCondition.EPositiveCondition.NA)
					{
						if (positiveConditions == null)
						{
							positiveConditions = new List<CCondition.EPositiveCondition>();
						}
						positiveConditions.Add(ePositiveCondition);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find condition '" + condition + "'. File: " + filename);
						result = false;
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing conditions.\nException: \nFile: " + filename, ex);
			return false;
		}
	}

	private static bool GetConditionalOverride(MappingEntry keyMappingEntry, int cardID, string parentName, bool isMonster, string filename, out CConditionalOverride conditionalOverride)
	{
		conditionalOverride = null;
		bool flag = true;
		string text = keyMappingEntry.Key.ToString();
		CAbilityFilterContainer filter = CAbilityFilterContainer.CreateDefaultFilter();
		CAbilityRequirements requirements = new CAbilityRequirements();
		CAbilityRequirements targetRequirements = null;
		int xp = 0;
		List<CAbilityOverride> list = new List<CAbilityOverride>();
		bool self = false;
		if (YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "FilterContainer":
				{
					if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter2))
					{
						filter = filter2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Filter":
				{
					if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter3))
					{
						filter = filter3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Requirement":
				case "Requirements":
				{
					if (GetAbilityRequirements(entry, filename, out var abilityRequirements))
					{
						requirements = abilityRequirements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TargetRequirements":
				{
					if (GetAbilityRequirements(entry, filename, out var abilityRequirements2))
					{
						targetRequirements = abilityRequirements2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "XP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ConditionalOverride/" + text + "/XP", filename, out var value2))
					{
						xp = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Self":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ConditionalOverride/" + text + "/Self", filename, out var value))
					{
						self = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityOverrides":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping2))
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry2, cardID, isMonster, filename, parentName);
							if (cAbilityOverride != null)
							{
								list.Add(cAbilityOverride);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides.  File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides.  Entry must be mapping.  File: " + filename);
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry.Key.ToString() + " in ConditionOverrides in file " + filename);
					flag = false;
					break;
				}
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in Conditional Override.  Value must be Mapping.  File: " + filename);
			flag = false;
		}
		if (list.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "No overrides specified for Conditional Override.  Only CancelAbilityOnFail conditional overrides can omit ability overrides.  File: " + filename);
			flag = false;
		}
		if (flag)
		{
			conditionalOverride = new CConditionalOverride(text, filter, requirements, xp, list, self, targetRequirements);
		}
		return flag;
	}

	public static bool GetVars(MappingEntry keyMappingEntry, string filename, out List<(string, TValue)> vars)
	{
		vars = new List<(string, TValue)>();
		if (!YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in Vars.  Value must be Mapping.  File: " + filename);
			return false;
		}
		foreach (MappingEntry entry in mapping.Entries)
		{
			string text = entry.Key.ToString();
			if (!YMLShared.GetStringPropertyValue(entry.Value, text, filename, out var value))
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + text + " in Vars " + value + ".  File: " + filename);
				return false;
			}
			CExpression cExpression = CExpression.Parse(value);
			if (cExpression == null || cExpression.type == CExpression.EType.Invalid)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid expression " + text + " in Vars " + cExpression?.ToString() + ".  File: " + filename);
				return false;
			}
			TValue item = ((cExpression.type != CExpression.EType.Constant) ? new TValue(cExpression) : cExpression.value);
			vars.Add((text, item));
		}
		return true;
	}

	public static bool GetPerformVars(MappingEntry keyMappingEntry, string filename, out List<(string, List<(string, TValue)>)> performVars)
	{
		performVars = new List<(string, List<(string, TValue)>)>();
		if (!YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in PerformVars.  Value must be Mapping.  File: " + filename);
			return false;
		}
		foreach (MappingEntry entry in mapping.Entries)
		{
			string item = entry.Key.ToString();
			if (!GetVars(entry, filename, out var vars))
			{
				return false;
			}
			performVars.Add((item, vars));
		}
		return true;
	}

	public static bool GetAbilityRequirements(MappingEntry keyMappingEntry, string filename, out CAbilityRequirements abilityRequirements)
	{
		abilityRequirements = new CAbilityRequirements();
		CEqualityFilter equalityFilter = null;
		bool result = true;
		if (YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "Type":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Type", filename, out var stringValue))
					{
						CAbilityRequirements.EStartAbilityRequirementType eStartAbilityRequirementType = CAbilityRequirements.StartAbilityRequirementTypes.SingleOrDefault((CAbilityRequirements.EStartAbilityRequirementType x) => x.ToString() == stringValue);
						if (eStartAbilityRequirementType != CAbilityRequirements.EStartAbilityRequirementType.None)
						{
							abilityRequirements.StartAbilityRequirementType = eStartAbilityRequirementType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in StartAbilityConditions/Type " + stringValue + ".  File: " + filename);
						result = false;
					}
					break;
				}
				case "MoveWasStraightLine":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "MoveWasStraightLine", filename, out var value3))
					{
						abilityRequirements.AbilityMovementWasStraightLine = value3;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequiredAgility":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "RequiredAgility", filename, out var value16))
					{
						abilityRequirements.RequiredAgility = Enum.Parse<CPlayerActor.EAgility>(value16);
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequiredForm":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "RequiredForm", filename, out var value20))
					{
						abilityRequirements.RequiredForm = Enum.Parse<CPlayerActor.EForm>(value20);
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequiredPressure":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "RequiredPressure", filename, out var value9))
					{
						abilityRequirements.RequiredPressure = Enum.Parse<CPlayerActor.EPressure>(value9);
					}
					else
					{
						result = false;
					}
					break;
				}
				case "TimeTokens":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "TimeTokens", filename, out equalityFilter))
					{
						abilityRequirements.TimeTokens = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "TrophyTokens":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "TrophyTokens", filename, out equalityFilter))
					{
						abilityRequirements.TrophyTokens = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "SingleUsePreventDamageCount":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "SingleUsePreventDamageCount", filename, out equalityFilter))
					{
						abilityRequirements.SingleUsePreventDamageCount = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "DiscardedAbilityCardsCount":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "DiscardedAbilityCardsCount", filename, out equalityFilter))
					{
						abilityRequirements.DiscardedAbilityCardsCount = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "DoesntHaveNegativeCondition":
				{
					if (YMLShared.GetStringList(entry.Value, "DoesntHaveNegativeCondition", filename, out var values3))
					{
						abilityRequirements.DoesntHaveNegativeCondition = new List<CCondition.ENegativeCondition>();
						foreach (string item5 in values3)
						{
							abilityRequirements.DoesntHaveNegativeCondition.Add(Enum.Parse<CCondition.ENegativeCondition>(item5));
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "OccupyingObstacleHex":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "OccupyingObstacleHex", filename, out var value4))
					{
						abilityRequirements.OccupyingObstacleHex = value4;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ElementsConsumedThisAction":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ElementsConsumedThisAction", filename, out var value22))
					{
						abilityRequirements.ElementsConsumedThisAction = value22;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "OnYourTurn":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "OnYourTurn", filename, out var value18))
					{
						abilityRequirements.OnYourTurn = value18;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "IsElite":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsElite", filename, out var value15))
					{
						abilityRequirements.IsElite = value15;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "IsLongRest":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsLongRest", filename, out var value12))
					{
						abilityRequirements.IsLongRest = value12;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "AbilityHasHappened":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AbilityHasHappened", filename, out var value7))
					{
						abilityRequirements.AbilityHasHappened = value7;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActionHasHappened":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ActionHasHappened", filename, out var value2))
					{
						abilityRequirements.ActionHasHappened = value2;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ForgoTopAction":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ForgoTopAction", filename, out var value21))
					{
						abilityRequirements.ForgoTopAction = value21;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ForgoBottomAction":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ForgoBottomAction", filename, out var value19))
					{
						abilityRequirements.ForgoBottomAction = value19;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "EnterHazardousTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "EnterHazardousTerrain", filename, out var value17))
					{
						abilityRequirements.EnterHazardousTerrain = value17;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "EnterDifficultTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "EnterDifficultTerrain", filename, out var value14))
					{
						abilityRequirements.EnterDifficultTerrain = value14;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActiveBonusAtTrackerIndex":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ActiveBonusAtTrackerIndex", filename, out var value13))
					{
						abilityRequirements.ActiveBonusAtTrackerIndex = value13;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActiveBonusGreaterOrEqualToTrackerIndex":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ActiveBonusGreaterOrEqualToTrackerIndex", filename, out var value10))
					{
						abilityRequirements.ActiveBonusGreaterOrEqualToTrackerIndex = value10;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "HP":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "HP", filename, out equalityFilter))
					{
						abilityRequirements.HP = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "HPPercentOfMax":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "HPPercentOfMax", filename, out equalityFilter))
					{
						abilityRequirements.HPPercentOfMax = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "HexesMovedThisTurn":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "HexesMovedThisTurn", filename, out equalityFilter))
					{
						abilityRequirements.HexesMovedThisTurn = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsTargetedByAbility":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsTargetedByAbility", filename, out equalityFilter))
					{
						abilityRequirements.ActorsTargetedByAbility = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledByAbility":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledByAbility", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledByAbility = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledThisAction":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledThisAction", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledThisAction = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledThisTurn":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledThisTurn", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledThisTurn = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledThisRound":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledThisRound", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledThisRound = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "AllActorsKilledThisRound":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "AllActorsKilledThisRound", filename, out equalityFilter))
					{
						abilityRequirements.AllActorsKilledThisRound = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "TrophyTokensGainedThisRound":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "TrophyTokensGainedThisRound", filename, out equalityFilter))
					{
						abilityRequirements.TrophyTokensGainedThisRound = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ObstaclesDestroyedThisAction":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ObstaclesDestroyedThisAction", filename, out equalityFilter))
					{
						abilityRequirements.ObstaclesDestroyedThisAction = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "DoorsOpenedByAbility":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "DoorsOpenedByAbility", filename, out equalityFilter))
					{
						abilityRequirements.DoorsOpenedByAbility = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "WallsAdjacent":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "WallsAdjacent", filename, out equalityFilter))
					{
						abilityRequirements.WallsAdjacent = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "InitiativeIs":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "InitiativeIs", filename, out equalityFilter))
					{
						abilityRequirements.InitiativeIs = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorHasPerformedValidAbilityTypesThisTurn":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorHasPerformedValidAbilityTypesThisTurn", filename, out equalityFilter))
					{
						abilityRequirements.ActorHasPerformedValidAbilityTypesThisTurn = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorHasPerformedValidAbilityTypesThisAction":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorHasPerformedValidAbilityTypesThisAction", filename, out equalityFilter))
					{
						abilityRequirements.ActorHasPerformedValidAbilityTypesThisAction = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ValidActorsInRangeOfAddedAbilityRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ValidActorsInRangeOfAddedAbilityRange", filename, out equalityFilter))
					{
						abilityRequirements.ValidActorsInRangeOfAddedAbilityRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ValidActorsInRangeOfRequirementRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ValidActorsInRangeOfRequirementRange", filename, out equalityFilter))
					{
						abilityRequirements.ValidActorsInRangeOfRequirementRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActiveSpawnersInRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActiveSpawnersInRange", filename, out equalityFilter))
					{
						abilityRequirements.ActiveSpawnersInRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "InactiveSpawnersInRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "InactiveSpawnersInRange", filename, out equalityFilter))
					{
						abilityRequirements.InactiveSpawnersInRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "LootInRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "LootInRange", filename, out equalityFilter))
					{
						abilityRequirements.LootInRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "InertElements":
				{
					if (YMLShared.GetStringList(entry.Value, "InertElements", filename, out var values7))
					{
						abilityRequirements.InertElements = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString3 in values7)
						{
							try
							{
								ElementInfusionBoardManager.EElement item4 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString3);
								abilityRequirements.InertElements.Add(item4);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString3 + "' specified for InertElements requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "WaningElements":
				{
					if (YMLShared.GetStringList(entry.Value, "WaningElements", filename, out var values6))
					{
						abilityRequirements.WaningElements = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString2 in values6)
						{
							try
							{
								ElementInfusionBoardManager.EElement item3 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString2);
								abilityRequirements.WaningElements.Add(item3);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString2 + "' specified for WaningElements requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "StrongElements":
				{
					if (YMLShared.GetStringList(entry.Value, "StrongElements", filename, out var values5))
					{
						abilityRequirements.StrongElements = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString in values5)
						{
							try
							{
								ElementInfusionBoardManager.EElement item2 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString);
								abilityRequirements.StrongElements.Add(item2);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString + "' specified for StrongElements requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "AbilityTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "AbilityTypes", filename, out var values4))
					{
						abilityRequirements.AbilityTypes = new List<CAbility.EAbilityType>();
						foreach (string abilityString in values4)
						{
							try
							{
								CAbility.EAbilityType item = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityString);
								abilityRequirements.AbilityTypes.Add(item);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityString + "' specified for AbilityTypes requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActorGuids":
				{
					if (YMLShared.GetStringList(entry.Value, "ActorGuids", filename, out var values2))
					{
						abilityRequirements.ActorGuids = values2;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ObjectImportTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "ObjectImportTypes", filename, out var values))
					{
						abilityRequirements.ObjectImportTypes = new List<ScenarioManager.ObjectImportType>();
						foreach (string lootableTypeString in values)
						{
							ScenarioManager.ObjectImportType objectImportType = ScenarioManager.ObjectImportTypes.SingleOrDefault((ScenarioManager.ObjectImportType x) => x.ToString() == lootableTypeString);
							if (objectImportType != ScenarioManager.ObjectImportType.None)
							{
								abilityRequirements.ObjectImportTypes.Add(objectImportType);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Object Import Type '" + lootableTypeString + "'.  File: " + filename);
							result = false;
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "XP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "XP", filename, out var value11))
					{
						abilityRequirements.XP = value11;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequirementRange":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "RequirementRange", filename, out var value8))
					{
						abilityRequirements.RequirementRange = value8;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RoundIsInterval":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "RoundIsInterval", filename, out var value6))
					{
						abilityRequirements.RoundIsInterval = value6;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActiveBonusInternalRoundInterval":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ActiveBonusInternalRoundInterval", filename, out var value5))
					{
						abilityRequirements.ActiveBonusInternalRoundInterval = value5;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequirementActorFilterContainer":
				{
					if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter2))
					{
						abilityRequirements.RequirementActorFilter = filter2;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequirementActorFilter":
				{
					if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter))
					{
						abilityRequirements.RequirementActorFilter = filter;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ValidateWhileSelected":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ValidateWhileSelected", filename, out var value))
					{
						abilityRequirements.ValidateWhileSelected = value;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "Vars":
				{
					if (GetVars(entry, filename, out var vars))
					{
						abilityRequirements.Vars = vars;
					}
					else
					{
						result = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process StartAbilityConditions.  Entry " + entry.Key.ToString() + " was not recognised.  File: " + filename);
					result = false;
					break;
				}
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in StartAbilityConditions.  Value must be Mapping.  File: " + filename);
			result = false;
		}
		return result;
	}

	private static bool ParseSummon(string summonString, int cardID, string abilityName, string filename, ref List<CEnhancement> abilityEnhancements, ref CAbility.EAbilityType? abilityType, ref ActiveBonusData activeBonusData, out List<string> summons)
	{
		bool result = true;
		summons = null;
		HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonString);
		if (heroSummonYMLData != null)
		{
			if (heroSummonYMLData.AttackEnhancements.Count > 0)
			{
				for (int num = 0; num < heroSummonYMLData.AttackEnhancements.Count; num++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonAttack, cardID, abilityName, num, heroSummonYMLData.AttackEnhancements[num]));
				}
			}
			if (heroSummonYMLData.MoveEnhancements.Count > 0)
			{
				for (int num2 = 0; num2 < heroSummonYMLData.MoveEnhancements.Count; num2++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonMove, cardID, abilityName, num2, heroSummonYMLData.MoveEnhancements[num2]));
				}
			}
			if (heroSummonYMLData.RangeEnhancements.Count > 0)
			{
				for (int num3 = 0; num3 < heroSummonYMLData.RangeEnhancements.Count; num3++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonRange, cardID, abilityName, num3, heroSummonYMLData.RangeEnhancements[num3]));
				}
			}
			if (heroSummonYMLData.HealthEnhancements.Count > 0)
			{
				for (int num4 = 0; num4 < heroSummonYMLData.HealthEnhancements.Count; num4++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonHealth, cardID, abilityName, num4, heroSummonYMLData.HealthEnhancements[num4]));
				}
			}
			abilityType = CAbility.EAbilityType.Summon;
			activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Summon;
			summons = new List<string> { summonString, summonString, summonString, summonString };
		}
		else
		{
			abilityType = CAbility.EAbilityType.Summon;
			activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Summon;
			summons = new List<string> { summonString, summonString, summonString, summonString };
		}
		return result;
	}
}
