using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
public class CAbilityOverride
{
	[JsonProperty]
	public Guid ID { get; private set; }

	[JsonProperty]
	public string AbilityName { get; private set; }

	[JsonProperty]
	public string ParentName { get; private set; }

	[JsonProperty]
	public CAbility OriginalAbility { get; set; }

	[JsonProperty]
	public CConditionalOverride OriginalConditionalOverride { get; set; }

	[JsonProperty]
	public CAbility.EAbilityType? AbilityType { get; private set; }

	[JsonProperty]
	public CAbility.EAbilityFlag? Flags { get; private set; }

	[JsonProperty]
	public int? Strength { get; private set; }

	[JsonProperty]
	public int? Range { get; private set; }

	[JsonProperty]
	public int? NumberOfTargets { get; private set; }

	[JsonProperty]
	public CAbilityFilterContainer AbilityFilter { get; private set; }

	[JsonProperty]
	public string AnimationOverload { get; private set; }

	[JsonProperty]
	public string AnimationOverload2 { get; private set; }

	[JsonProperty]
	public List<CAbility> SubAbilities { get; set; }

	[JsonProperty]
	public CAreaEffect AreaEffect { get; private set; }

	[JsonProperty]
	public bool? AttackSourcesOnly { get; private set; }

	[JsonProperty]
	public bool? Jump { get; private set; }

	[JsonProperty]
	public bool? Fly { get; private set; }

	[JsonProperty]
	public bool? IgnoreDifficultTerrain { get; private set; }

	[JsonProperty]
	public bool? IgnoreHazardousTerrain { get; private set; }

	[JsonProperty]
	public bool? IgnoreBlockedTileMoveCost { get; private set; }

	[JsonProperty]
	public bool? CarryOtherActorsOnHex { get; private set; }

	[JsonProperty]
	public CAbilityMove.EMoveRestrictionType? MoveRestrictionType { get; private set; }

	[JsonProperty]
	public AbilityData.ActiveBonusData ActiveBonusData { get; private set; }

	public ActiveBonusLayout ActiveBonusYML { get; private set; }

	[JsonProperty]
	public string AreaEffectYMLString { get; private set; }

	[JsonProperty]
	public string AreaEffectLayoutOverrideYMLString { get; private set; }

	[JsonProperty]
	public string AreaEffectFlipAxis { get; private set; }

	[JsonProperty]
	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	[JsonProperty]
	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	[JsonProperty]
	public bool? MultiPassAttack { get; private set; }

	[JsonProperty]
	public bool? ExploitVulnerability { get; private set; }

	[JsonProperty]
	public int? DamageSelfBeforeAttack { get; private set; }

	[JsonProperty]
	public bool? AddAttackBaseStat { get; private set; }

	[JsonProperty]
	public bool? StrengthIsBase { get; private set; }

	[JsonProperty]
	public bool? RangeIsBase { get; private set; }

	[JsonProperty]
	public bool? TargetIsBase { get; private set; }

	[JsonProperty]
	public bool? TargetAtLeastOne { get; private set; }

	[JsonProperty]
	public bool? RangeAtLeastOne { get; private set; }

	[JsonProperty]
	public bool? RemoveConditionsOverride { get; private set; }

	[JsonProperty]
	public string AbilityText { get; private set; }

	[JsonProperty]
	public bool? AbilityTextOnly { get; private set; }

	[JsonProperty]
	public bool? ShowRange { get; private set; }

	[JsonProperty]
	public bool? ShowTarget { get; private set; }

	[JsonProperty]
	public bool? ShowArea { get; private set; }

	[JsonProperty]
	public bool? OnDeath { get; private set; }

	[JsonProperty]
	public bool? AllTargetsOnMovePath { get; private set; }

	[JsonProperty]
	public bool? AllTargetsOnMovePathSameStartAndEnd { get; private set; }

	[JsonProperty]
	public bool? AllTargetsOnAttackPath { get; private set; }

	[JsonProperty]
	public List<string> Summons { get; private set; }

	[JsonProperty]
	public AbilityData.XpPerTargetData XpPerTargetData { get; private set; }

	[JsonProperty]
	public CAbility.EAbilityTargeting? Targeting { get; private set; }

	[JsonProperty]
	public List<(string, TValue)> TargetingVars { get; private set; }

	[JsonProperty]
	public List<(string, TValue)> Vars { get; private set; }

	[JsonProperty]
	public List<(string, TValue)> MiscVars { get; private set; }

	[JsonProperty]
	public List<(string, List<(string, TValue)>)> PerformVars { get; private set; }

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> ElementsToInfuse { get; private set; }

	[JsonProperty]
	public bool? ShowElementPicker { get; private set; }

	[JsonProperty]
	public string PropName { get; private set; }

	[JsonProperty]
	public AbilityData.TrapData TrapData { get; private set; }

	[JsonProperty]
	public bool? IsSubAbility { get; private set; }

	[JsonProperty]
	public int? Pierce { get; private set; }

	[JsonProperty]
	public int? RetaliateRange { get; private set; }

	[JsonProperty]
	public bool? IsTargetedAbility { get; private set; }

	[JsonProperty]
	public float? SpawnDelay { get; private set; }

	[JsonProperty]
	public CAbilityPull.EPullType? PullType { get; private set; }

	[JsonProperty]
	public CAbilityPush.EAdditionalPushEffect? AdditionalPushEffect { get; private set; }

	[JsonProperty]
	public int? AdditionalPushEffectDamage { get; private set; }

	[JsonProperty]
	public int? AdditionalPushEffectXP { get; private set; }

	[JsonProperty]
	public int? AbilityXP { get; private set; }

	[JsonProperty]
	public List<CConditionalOverride> ConditionalOverrides { get; private set; }

	[JsonProperty]
	public CAbilityRequirements StartAbilityRequirements { get; private set; }

	[JsonProperty]
	public AbilityData.MiscAbilityData MiscAbilityData { get; private set; }

	[JsonProperty]
	public bool? SharePreviousAnim { get; private set; }

	[JsonProperty]
	public int? ConditionDuration { get; private set; }

	[JsonProperty]
	public EConditionDecTrigger? ConditionDecTrigger { get; private set; }

	[JsonProperty]
	public bool? IsInlineSubAbility { get; private set; }

	[JsonProperty]
	public bool? IsConsumeAbility { get; private set; }

	[JsonProperty]
	public CAugment Augment { get; private set; }

	[JsonProperty]
	public CSong Song { get; private set; }

	[JsonProperty]
	public CDoom Doom { get; private set; }

	[JsonProperty]
	public List<CAttackEffect> AttackEffects { get; private set; }

	[JsonProperty]
	public CAbilityControlActor.ControlActorAbilityData ControlActorData { get; private set; }

	[JsonProperty]
	public CAbilityChangeAllegiance.ChangeAllegianceAbilityData ChangeAllegianceData { get; private set; }

	[JsonProperty]
	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; private set; }

	[JsonProperty]
	public CAbility AddActiveBonusAbility { get; private set; }

	[JsonProperty]
	public List<CAbility> ChooseAbilities { get; private set; }

	[JsonProperty]
	public List<CAbility.EAbilityType> RecoverCardsWithAbilityOfTypeFilter { get; private set; }

	[JsonProperty]
	public bool? UseParentTargeting { get; private set; }

	[JsonProperty]
	public CAbility ForgoTopActionAbility { get; private set; }

	[JsonProperty]
	public CAbility ForgoBottomActionAbility { get; private set; }

	[JsonProperty]
	public CAbilityExtraTurn.EExtraTurnType? ExtraTurnType { get; private set; }

	[JsonProperty]
	public string ActionMonsterClassID { get; private set; }

	[JsonProperty]
	public CAbilityFilterContainer SwapFirstTargetAbilityFilter { get; private set; }

	[JsonProperty]
	public CAbilityFilterContainer SwapSecondTargetAbilityFilter { get; private set; }

	[JsonProperty]
	public CAbilityTeleport.TeleportData TeleportData { get; private set; }

	[JsonProperty]
	public CAbilityLoot.LootData LootData { get; private set; }

	[JsonProperty]
	public List<CAbility.EAbilityType> ImmunityToAbilityTypes { get; private set; }

	[JsonProperty]
	public List<string> ModifierCardNamesToAdd { get; private set; }

	[JsonProperty]
	public CAbilityHeal.HealAbilityData HealData { get; private set; }

	[JsonProperty]
	public Dictionary<string, int> ResourcesToAddOnAbilityEnd { get; private set; }

	[JsonProperty]
	public Dictionary<string, int> ResourcesToTakeFromTargets { get; private set; }

	[JsonProperty]
	public Dictionary<string, int> ResourcesToGiveToTargets { get; private set; }

	[JsonProperty]
	public CAbilityDestroyObstacle.DestroyObstacleData DestroyObstacleData { get; private set; }

	[JsonProperty]
	public string HelpBoxLocalizationKey { get; private set; }

	[JsonProperty]
	public List<CAbility.EAbilityType> ExtraTurnAbilityTypeFilter { get; private set; }

	[JsonProperty]
	public int? MaxExtraTurnAbilityLevel { get; private set; }

	[JsonProperty]
	public int? Tokens { get; private set; }

	[JsonConstructor]
	public CAbilityOverride(string abilityName, string parentName, CAbility.EAbilityType? abilityType = null, CAbility.EAbilityFlag? flags = null, int? strength = null, int? range = null, int? numberOfTargets = null, CAbilityFilterContainer abilityFilter = null, string animationOverload = null, string animationOverload2 = null, List<CAbility> subAbilities = null, CAreaEffect areaEffect = null, bool? attackSourcesOnly = null, bool? jump = null, bool? fly = null, bool? ignoreDifficultTerrain = null, bool? ignoreHazardousTerrain = null, bool? ignoreBlockedTileMoveCost = null, bool? carryOtherActorsOnHex = null, CAbilityMove.EMoveRestrictionType? moveRestrictionType = null, AbilityData.ActiveBonusData activeBonusData = null, ActiveBonusLayout activeBonusYML = null, string areaEffectYMLString = null, string areaEffectLayoutOverrideYMLString = null, string areaEffectFlipAxis = null, List<CCondition.EPositiveCondition> positiveConditions = null, List<CCondition.ENegativeCondition> negativeConditions = null, bool? multiPassAttack = null, int? damageSelfBeforeAttack = null, bool? addAttackBaseStat = null, bool? strengthIsBase = null, bool? rangeIsBase = null, bool? targetIsBase = null, bool? rangeAtLeastOne = null, bool? targetAtLeastOne = null, bool? removeConditions = null, string text = null, bool? textOnly = null, bool? showRange = null, bool? showTarget = null, bool? showArea = null, bool? onDeath = null, bool? allTargetsOnMovePath = null, bool? allTargetsOnMovePathSameStartAndEnd = null, bool? allTargetsOnAttackPath = null, List<string> summons = null, AbilityData.XpPerTargetData xpPerTargetData = null, CAbility.EAbilityTargeting? targeting = null, List<(string, TValue)> targetingVars = null, List<(string, TValue)> vars = null, List<(string, TValue)> miscVars = null, List<(string, List<(string, TValue)>)> performVars = null, List<ElementInfusionBoardManager.EElement> elementsToInfuse = null, bool? showElementPicker = null, string propName = null, AbilityData.TrapData trapData = null, bool? isSubAbility = null, int? pierce = null, int? retaliateRange = null, bool? isTargetedAbility = null, float? spawnDelay = null, CAbilityPull.EPullType? pullType = null, CAbilityPush.EAdditionalPushEffect? additionalPushEffect = null, int? additionalPushEffectDamage = null, int? additionalPushEffectXP = null, int? abilityXP = null, List<CConditionalOverride> conditionalOverrides = null, AbilityData.MiscAbilityData miscAbilityData = null, bool? sharePreviousAnim = null, int? conditionDuration = null, EConditionDecTrigger? conditionDecTrigger = null, bool? isInlineSubAbility = null, bool? isConsumeAbility = null, CAbilityControlActor.ControlActorAbilityData controlActorData = null, CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = null, CAugment augment = null, CSong song = null, CDoom doom = null, List<CAttackEffect> attackEffects = null, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null, CAbility addActiveBonusAbility = null, List<CAbility> chooseAbilities = null, List<CAbility.EAbilityType> recoverCardAbilityTypeFilter = null, CAbility forgoTopActionAbility = null, CAbility forgoBottomActionAbility = null, CAbilityExtraTurn.EExtraTurnType? extraTurnType = null, string actionMonsterClassID = null, CAbilityFilterContainer swapFirstTargetAbilityFilter = null, CAbilityFilterContainer swapSecondTargetAbilityFilter = null, CAbilityTeleport.TeleportData teleportData = null, CAbilityLoot.LootData lootData = null, List<CAbility.EAbilityType> immunityToAbilityTypes = null, List<string> addModifiers = null, CAbilityHeal.HealAbilityData healData = null, Dictionary<string, int> resourcesToAddOnAbilityEnd = null, Dictionary<string, int> resourcesToTakeFromTargets = null, Dictionary<string, int> resourcesToGiveToTargets = null, CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = null, bool? exploitVulnerability = null, string helpBoxLocalizationKey = null, List<CAbility.EAbilityType> extraTurnAbilityTypeFilter = null, int? maxExtraTurnAbilityLevel = int.MaxValue, bool? useParentTargeting = null, CAbilityRequirements startAbilityRequirement = null, CConditionalOverride originalConditionalOverride = null, int? tokens = null, Guid? id = null)
	{
		if (!id.HasValue)
		{
			ID = Guid.NewGuid();
		}
		else
		{
			ID = id.Value;
		}
		AbilityName = abilityName;
		ParentName = parentName;
		Tokens = tokens;
		AbilityType = abilityType;
		Flags = flags;
		Strength = strength;
		Range = range;
		NumberOfTargets = numberOfTargets;
		AbilityFilter = abilityFilter;
		AnimationOverload = animationOverload;
		AnimationOverload2 = animationOverload2;
		SubAbilities = subAbilities;
		AreaEffect = areaEffect;
		AttackSourcesOnly = attackSourcesOnly;
		Jump = jump;
		Fly = fly;
		IgnoreDifficultTerrain = ignoreDifficultTerrain;
		IgnoreHazardousTerrain = ignoreHazardousTerrain;
		IgnoreBlockedTileMoveCost = ignoreBlockedTileMoveCost;
		CarryOtherActorsOnHex = carryOtherActorsOnHex;
		MoveRestrictionType = moveRestrictionType;
		ActiveBonusData = activeBonusData;
		ActiveBonusYML = activeBonusYML;
		AreaEffectYMLString = areaEffectYMLString;
		AreaEffectLayoutOverrideYMLString = areaEffectLayoutOverrideYMLString;
		AreaEffectFlipAxis = areaEffectFlipAxis;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
		MultiPassAttack = multiPassAttack;
		ExploitVulnerability = exploitVulnerability;
		DamageSelfBeforeAttack = damageSelfBeforeAttack;
		AddAttackBaseStat = addAttackBaseStat;
		StrengthIsBase = strengthIsBase;
		RangeIsBase = rangeIsBase;
		TargetIsBase = targetIsBase;
		RangeAtLeastOne = rangeAtLeastOne;
		TargetAtLeastOne = targetAtLeastOne;
		RemoveConditionsOverride = removeConditions;
		AbilityText = text;
		AbilityTextOnly = textOnly;
		ShowRange = showRange;
		ShowTarget = showTarget;
		ShowArea = showArea;
		OnDeath = onDeath;
		AllTargetsOnMovePath = allTargetsOnMovePath;
		AllTargetsOnMovePathSameStartAndEnd = allTargetsOnMovePathSameStartAndEnd;
		AllTargetsOnAttackPath = allTargetsOnAttackPath;
		Summons = summons;
		XpPerTargetData = xpPerTargetData;
		Targeting = targeting;
		TargetingVars = targetingVars;
		Vars = vars;
		MiscVars = miscVars;
		PerformVars = performVars;
		ElementsToInfuse = elementsToInfuse;
		ShowElementPicker = showElementPicker;
		PropName = propName;
		TrapData = trapData;
		IsSubAbility = isSubAbility;
		Pierce = pierce;
		RetaliateRange = retaliateRange;
		IsTargetedAbility = isTargetedAbility;
		SpawnDelay = spawnDelay;
		PullType = pullType;
		AdditionalPushEffect = additionalPushEffect;
		AdditionalPushEffectDamage = additionalPushEffectDamage;
		AdditionalPushEffectXP = additionalPushEffectXP;
		AbilityXP = abilityXP;
		ConditionalOverrides = conditionalOverrides;
		MiscAbilityData = miscAbilityData ?? new AbilityData.MiscAbilityData();
		SharePreviousAnim = sharePreviousAnim;
		ConditionDuration = conditionDuration;
		ConditionDecTrigger = conditionDecTrigger;
		IsInlineSubAbility = isInlineSubAbility;
		IsConsumeAbility = isConsumeAbility;
		ControlActorData = controlActorData;
		ChangeAllegianceData = changeAllegianceData;
		Augment = augment;
		Song = song;
		Doom = doom;
		AttackEffects = attackEffects;
		StatIsBasedOnXEntries = statIsBasedOnXEntries;
		AddActiveBonusAbility = addActiveBonusAbility;
		ChooseAbilities = chooseAbilities;
		RecoverCardsWithAbilityOfTypeFilter = recoverCardAbilityTypeFilter;
		ForgoTopActionAbility = forgoTopActionAbility;
		ForgoBottomActionAbility = forgoBottomActionAbility;
		ExtraTurnType = extraTurnType;
		ActionMonsterClassID = actionMonsterClassID;
		SwapFirstTargetAbilityFilter = swapFirstTargetAbilityFilter;
		SwapSecondTargetAbilityFilter = swapSecondTargetAbilityFilter;
		TeleportData = teleportData;
		LootData = lootData;
		ImmunityToAbilityTypes = immunityToAbilityTypes;
		ModifierCardNamesToAdd = addModifiers;
		HealData = healData;
		ResourcesToAddOnAbilityEnd = resourcesToAddOnAbilityEnd;
		ResourcesToTakeFromTargets = resourcesToTakeFromTargets;
		ResourcesToGiveToTargets = resourcesToGiveToTargets;
		DestroyObstacleData = destroyObstacleData;
		HelpBoxLocalizationKey = helpBoxLocalizationKey;
		ExtraTurnAbilityTypeFilter = extraTurnAbilityTypeFilter;
		MaxExtraTurnAbilityLevel = maxExtraTurnAbilityLevel;
		UseParentTargeting = useParentTargeting;
		StartAbilityRequirements = startAbilityRequirement;
		OriginalConditionalOverride = originalConditionalOverride;
	}

	public static CAbilityOverride CreateAbilityOverride(CAbility abilityToOverride, CAbility.EAbilityFlag? overrideFlags, int? overrideStrength, int? overrideRange, int? overrideNumberOfTargets)
	{
		string name = abilityToOverride.Name;
		CAbility.EAbilityType? abilityType = abilityToOverride.AbilityType;
		return new CAbilityOverride("", name, abilityType, overrideFlags, overrideStrength, overrideRange, overrideNumberOfTargets, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, int.MaxValue);
	}

	public static CAbilityOverride CopyAbilityOverride(CAbilityOverride other, bool newGuid = false)
	{
		return new CAbilityOverride(other.AbilityName, other.ParentName, other.AbilityType, other.Flags, other.Strength, other.Range, other.NumberOfTargets, other.AbilityFilter, other.AnimationOverload, other.AnimationOverload2, other.SubAbilities, other.AreaEffect, other.AttackSourcesOnly, other.Jump, other.Fly, other.IgnoreDifficultTerrain, other.IgnoreHazardousTerrain, other.IgnoreBlockedTileMoveCost, other.CarryOtherActorsOnHex, other.MoveRestrictionType, other.ActiveBonusData, other.ActiveBonusYML, other.AreaEffectYMLString, other.AreaEffectLayoutOverrideYMLString, other.AreaEffectFlipAxis, other.PositiveConditions, other.NegativeConditions, other.MultiPassAttack, other.DamageSelfBeforeAttack, other.AddAttackBaseStat, other.StrengthIsBase, other.RangeIsBase, other.TargetIsBase, other.RangeAtLeastOne, other.TargetAtLeastOne, other.RemoveConditionsOverride, other.AbilityText, other.AbilityTextOnly, other.ShowRange, other.ShowTarget, other.ShowArea, other.OnDeath, other.AllTargetsOnMovePath, other.AllTargetsOnMovePathSameStartAndEnd, other.AllTargetsOnAttackPath, other.Summons, other.XpPerTargetData, other.Targeting, other.TargetingVars, other.Vars, other.MiscVars, other.PerformVars, other.ElementsToInfuse, other.ShowElementPicker, other.PropName, other.TrapData, other.IsSubAbility, other.Pierce, other.RetaliateRange, other.IsTargetedAbility, other.SpawnDelay, other.PullType, other.AdditionalPushEffect, other.AdditionalPushEffectDamage, other.AdditionalPushEffectXP, other.AbilityXP, other.ConditionalOverrides, other.MiscAbilityData, other.SharePreviousAnim, other.ConditionDuration, other.ConditionDecTrigger, other.IsInlineSubAbility, other.IsConsumeAbility, other.ControlActorData, other.ChangeAllegianceData, other.Augment, other.Song, other.Doom, other.AttackEffects, other.StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData x) => x.Copy())?.ToList(), other.AddActiveBonusAbility, other.ChooseAbilities, other.RecoverCardsWithAbilityOfTypeFilter, other.ForgoTopActionAbility, other.ForgoBottomActionAbility, other.ExtraTurnType, other.ActionMonsterClassID, other.SwapFirstTargetAbilityFilter, other.SwapSecondTargetAbilityFilter, other.TeleportData, other.LootData, other.ImmunityToAbilityTypes, other.ModifierCardNamesToAdd, other.HealData, other.ResourcesToAddOnAbilityEnd, other.ResourcesToTakeFromTargets, other.ResourcesToGiveToTargets, other.DestroyObstacleData, other.ExploitVulnerability, other.HelpBoxLocalizationKey, other.ExtraTurnAbilityTypeFilter, other.MaxExtraTurnAbilityLevel, other.UseParentTargeting, other.StartAbilityRequirements, other.OriginalConditionalOverride, other.Tokens, newGuid ? ((Guid?)null) : new Guid?(other.ID));
	}

	public static CAbilityOverride CreateAbilityOverride(MappingEntry overrideEntry, int cardID, bool isMonster, string filename, string parentNameParam = "", bool isConsume = false, string abilityNameOverride = null)
	{
		string text = abilityNameOverride ?? overrideEntry.Key.ToString();
		string parentName = parentNameParam;
		CAbility.EAbilityType? abilityType = null;
		string propName = null;
		AbilityData.TrapData trapData = null;
		bool isSubAbility = false;
		int? strength = null;
		CAbility.ECardAction cardAction = CAbility.ECardAction.None;
		int? range = null;
		int? numberOfTargets = null;
		bool numberOfTargetsSet = false;
		int? tokens = null;
		CAbility.EAbilityFlag? flags = null;
		CAbilityFilterContainer filter = null;
		string animationOverload = null;
		string animationOverload2 = null;
		List<CAbility> subAbilities = null;
		CAreaEffect areaEffect = null;
		bool? attackSourcesOnly = null;
		bool? jump = null;
		bool? fly = null;
		bool? ignoreDifficultTerrain = null;
		bool? ignoreHazardousTerrain = null;
		bool? ignoreBlockedTileMoveCost = null;
		bool? carryOtherActorsOnHex = null;
		CAIFocusOverrideDetails aiFocusOverride = null;
		CAbilityMove.EMoveRestrictionType? moveRestrictionType = null;
		AbilityData.ActiveBonusData activeBonusData = null;
		ActiveBonusLayout activeBonusYML = null;
		string areaEffectYMLString = null;
		string areaEffectLayoutOverrideYMLString = null;
		string areaEffectFlipAxis = null;
		List<CCondition.EPositiveCondition> positiveConditions = null;
		List<CCondition.ENegativeCondition> negativeConditions = null;
		bool? multiPassAttack = null;
		bool? exploitVulnerability = null;
		bool? chainAttack = null;
		int? chainAttackRange = null;
		int? chainAttackDamageReduction = null;
		int? damageSelfBeforeAttack = null;
		bool? addAttackBaseStat = null;
		bool? strengthIsBase = null;
		bool? rangeIsBase = null;
		bool? targetIsBase = null;
		bool? rangeAtLeastOne = null;
		bool? targetAtLeastOne = null;
		bool? removeConditionsOverride = null;
		string abilityText = null;
		bool? abilityTextOnly = null;
		bool? abilityNoLayoutSpace = null;
		bool? showRange = null;
		bool? showTarget = null;
		bool? showArea = null;
		bool? onDeath = null;
		bool? allTargetsOnMovePath = null;
		bool? allTargetsOnMovePathSameStartAndEnd = null;
		bool? allTargetsOnAttackPath = null;
		bool? allTargetsInRangeAlongPath = null;
		List<string> summons = null;
		AbilityData.XpPerTargetData xpPerTargetData = null;
		CAbility.EAbilityTargeting? targeting = null;
		List<(string, TValue)> targetingVars = null;
		List<(string, TValue)> vars = null;
		List<(string, List<(string, TValue)>)> performVars = null;
		List<(string, TValue)> miscVars = null;
		List<ElementInfusionBoardManager.EElement> elementsToInfuse = null;
		List<CCondition.ENegativeCondition> elementalConditions = null;
		bool? showElementPicker = null;
		int? pierce = null;
		int? retaliateRange = null;
		bool? isTargetedAbility = null;
		float? spawnDelay = null;
		CAbilityPull.EPullType? pullType = null;
		CAbilityPush.EAdditionalPushEffect? additionalPushEffect = null;
		int? additionalPushEffectDamage = null;
		int? additionalPushEffectXP = null;
		CAbilityLoot.LootData lootData = null;
		List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
		CAbilityRequirements startAbilityRequirements = new CAbilityRequirements();
		int? abilityXP = null;
		AbilityData.MiscAbilityData miscAbilityData = new AbilityData.MiscAbilityData();
		bool? skipAnim = null;
		int? conditionDuration = null;
		EConditionDecTrigger? conditionDecTrigger = null;
		bool? isInlineSubAbility = null;
		bool? flag = isConsume;
		CAugment augment = null;
		CSong song = null;
		CDoom doom = null;
		List<CAttackEffect> attackEffects = new List<CAttackEffect>();
		CAbilityControlActor.ControlActorAbilityData controlActorAbilityData = null;
		CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceAbilityData = null;
		List<CAbility> mergedAbilities = null;
		bool? targetActorWithTrapEffects = false;
		int? targetActorWithTrapEffectRange = 0;
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null;
		int? moveObstacleRange = null;
		string moveObstacleAnimOverload = null;
		List<CItem.EItemSlot> slotsToRefresh = null;
		List<CItem.EItemSlotState> slotStatesToRefresh = null;
		List<CItem.EUsageType> slotUsagesToRefresh = null;
		CAbility addActiveBonusAbility = null;
		CAbility.EAttackType? overrideAugmentAttackType = null;
		List<CAbility> chooseAbilities = null;
		List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter = null;
		bool? useParentTargeting = null;
		CAbility forgoTopActionAbility = null;
		CAbility forgoBottomActionAbility = null;
		CAbilityExtraTurn.EExtraTurnType? extraTurnType = null;
		string actionMonsterClassID = null;
		CAbilityFilterContainer swapAbilityFirstTargetFilter = null;
		CAbilityFilterContainer swapAbilitySecondTargetFilter = null;
		List<int> supplyCardsToGive = null;
		CAbilityTeleport.TeleportData teleportData = null;
		List<CAbility.EAbilityType> immunityToAbilityTypes = null;
		List<CAbility.EAttackType> immuneToAttackTypes = null;
		bool? immuneToSelfDamage = false;
		List<string> addOrRemoveModifiers = null;
		CAbilityHeal.HealAbilityData healAbilityData = null;
		Dictionary<string, int> resourcesToAddOnAbilityEnd = null;
		Dictionary<string, int> resourcesToTakeFromTargets = null;
		Dictionary<string, int> resourcesToGiveToTargets = null;
		string previewEffectId = null;
		string previewEffectText = null;
		string helpBoxTooltipLocKey = null;
		CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = null;
		ECharacter changeCharacterModel = ECharacter.None;
		CAbilityFilter.EFilterTile tileFilter = CAbilityFilter.EFilterTile.None;
		CAbilityDisableCardAction.DisableCardActionData disableCardActionData = null;
		List<AbilityConsume> consumes = new List<AbilityConsume>();
		List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
		bool? useSpecialBaseStat = false;
		List<CAbility.EAbilityType> extraTurnAbilityTypeFilter = null;
		int? maxExtraTurnAbilityLevel = int.MaxValue;
		bool? toDiscardPile = false;
		bool? removeWhenDone = false;
		bool? canExcludeCards = false;
		if (YMLShared.GetMapping(overrideEntry, filename, out var mapping))
		{
			if (AbilityData.ParseAbilityProperties(mapping, text, cardID, isMonster, isAbilityOverride: true, ref isSubAbility, ref parentName, ref abilityType, ref propName, ref trapData, ref abilityEnhancements, ref strength, ref cardAction, ref useSpecialBaseStat, ref range, ref numberOfTargets, ref numberOfTargetsSet, ref tokens, ref flags, ref filter, ref animationOverload, ref animationOverload2, ref subAbilities, ref areaEffect, ref attackSourcesOnly, ref jump, ref fly, ref ignoreDifficultTerrain, ref ignoreHazardousTerrain, ref ignoreBlockedTileMoveCost, ref aiFocusOverride, ref carryOtherActorsOnHex, ref moveRestrictionType, ref activeBonusData, ref activeBonusYML, ref areaEffectYMLString, ref areaEffectLayoutOverrideYMLString, ref areaEffectFlipAxis, ref positiveConditions, ref negativeConditions, ref multiPassAttack, ref chainAttack, ref chainAttackRange, ref chainAttackDamageReduction, ref damageSelfBeforeAttack, ref addAttackBaseStat, ref strengthIsBase, ref rangeIsBase, ref targetIsBase, ref rangeAtLeastOne, ref targetAtLeastOne, ref removeConditionsOverride, ref abilityText, ref abilityTextOnly, ref abilityNoLayoutSpace, ref showRange, ref showTarget, ref showArea, ref onDeath, ref allTargetsOnMovePath, ref allTargetsOnMovePathSameStartAndEnd, ref allTargetsOnAttackPath, ref allTargetsInRangeAlongPath, ref summons, ref xpPerTargetData, ref targeting, ref targetingVars, ref vars, ref performVars, ref elementsToInfuse, ref consumes, ref elementalConditions, ref showElementPicker, ref pierce, ref retaliateRange, ref isTargetedAbility, ref spawnDelay, ref pullType, ref additionalPushEffect, ref additionalPushEffectDamage, ref additionalPushEffectXP, ref lootData, ref conditionalOverrides, ref startAbilityRequirements, ref abilityXP, ref miscAbilityData, ref skipAnim, ref conditionDuration, ref conditionDecTrigger, ref isInlineSubAbility, ref augment, ref song, ref doom, ref attackEffects, ref controlActorAbilityData, ref changeAllegianceAbilityData, ref mergedAbilities, ref targetActorWithTrapEffects, ref targetActorWithTrapEffectRange, ref statIsBasedOnXEntries, ref moveObstacleRange, ref moveObstacleAnimOverload, ref slotsToRefresh, ref slotStatesToRefresh, ref slotUsagesToRefresh, ref addActiveBonusAbility, ref overrideAugmentAttackType, ref chooseAbilities, ref recoverCardsAbilityTypeFilter, ref useParentTargeting, ref forgoTopActionAbility, ref forgoBottomActionAbility, ref extraTurnType, ref actionMonsterClassID, ref swapAbilityFirstTargetFilter, ref swapAbilitySecondTargetFilter, ref supplyCardsToGive, ref teleportData, ref immunityToAbilityTypes, ref immuneToAttackTypes, ref immuneToSelfDamage, ref addOrRemoveModifiers, ref healAbilityData, ref resourcesToAddOnAbilityEnd, ref resourcesToTakeFromTargets, ref resourcesToGiveToTargets, ref destroyObstacleData, ref changeCharacterModel, ref tileFilter, ref disableCardActionData, ref previewEffectId, ref previewEffectText, ref helpBoxTooltipLocKey, ref exploitVulnerability, ref extraTurnAbilityTypeFilter, ref maxExtraTurnAbilityLevel, ref toDiscardPile, ref removeWhenDone, ref canExcludeCards, ref miscVars, filename))
			{
				string parentName2 = parentName;
				CAbility.EAbilityType? abilityType2 = abilityType;
				CAbility.EAbilityFlag? flags2 = flags;
				int? strength2 = strength;
				int? range2 = range;
				int? numberOfTargets2 = numberOfTargets;
				CAbilityFilterContainer abilityFilter = filter;
				string animationOverload3 = animationOverload;
				string animationOverload4 = animationOverload2;
				List<CAbility> subAbilities2 = subAbilities;
				CAreaEffect areaEffect2 = areaEffect;
				bool? attackSourcesOnly2 = attackSourcesOnly;
				bool? jump2 = jump;
				bool? fly2 = fly;
				bool? ignoreDifficultTerrain2 = ignoreDifficultTerrain;
				bool? ignoreHazardousTerrain2 = ignoreHazardousTerrain;
				bool? carryOtherActorsOnHex2 = carryOtherActorsOnHex;
				CAbilityMove.EMoveRestrictionType? moveRestrictionType2 = moveRestrictionType;
				AbilityData.ActiveBonusData activeBonusData2 = activeBonusData;
				ActiveBonusLayout activeBonusYML2 = activeBonusYML;
				string areaEffectYMLString2 = areaEffectYMLString;
				string areaEffectLayoutOverrideYMLString2 = areaEffectLayoutOverrideYMLString;
				string areaEffectFlipAxis2 = areaEffectFlipAxis;
				List<CCondition.EPositiveCondition> positiveConditions2 = positiveConditions;
				List<CCondition.ENegativeCondition> negativeConditions2 = negativeConditions;
				bool? multiPassAttack2 = multiPassAttack;
				bool? exploitVulnerability2 = exploitVulnerability;
				int? damageSelfBeforeAttack2 = damageSelfBeforeAttack;
				bool? addAttackBaseStat2 = addAttackBaseStat;
				bool? strengthIsBase2 = strengthIsBase;
				bool? rangeIsBase2 = rangeIsBase;
				bool? targetIsBase2 = targetIsBase;
				bool? rangeAtLeastOne2 = rangeAtLeastOne;
				bool? targetAtLeastOne2 = targetAtLeastOne;
				bool? removeConditions = removeConditionsOverride;
				string text2 = abilityText;
				bool? textOnly = abilityTextOnly;
				bool? showRange2 = showRange;
				bool? showTarget2 = showTarget;
				bool? showArea2 = showArea;
				bool? onDeath2 = onDeath;
				bool? allTargetsOnMovePath2 = allTargetsOnMovePath;
				bool? allTargetsOnMovePathSameStartAndEnd2 = allTargetsOnMovePathSameStartAndEnd;
				bool? allTargetsOnAttackPath2 = allTargetsOnAttackPath;
				List<string> summons2 = summons;
				AbilityData.XpPerTargetData xpPerTargetData2 = xpPerTargetData;
				CAbility.EAbilityTargeting? targeting2 = targeting;
				List<(string, TValue)> targetingVars2 = targetingVars;
				List<(string, TValue)> vars2 = vars;
				List<(string, TValue)> miscVars2 = miscVars;
				List<(string, List<(string, TValue)>)> performVars2 = performVars;
				List<ElementInfusionBoardManager.EElement> elementsToInfuse2 = elementsToInfuse;
				bool? showElementPicker2 = showElementPicker;
				string propName2 = propName;
				AbilityData.TrapData trapData2 = trapData;
				bool? isSubAbility2 = null;
				int? pierce2 = pierce;
				int? retaliateRange2 = retaliateRange;
				bool? isTargetedAbility2 = isTargetedAbility;
				float? spawnDelay2 = spawnDelay;
				CAbilityPull.EPullType? pullType2 = pullType;
				CAbilityPush.EAdditionalPushEffect? additionalPushEffect2 = additionalPushEffect;
				int? additionalPushEffectDamage2 = additionalPushEffectDamage;
				int? additionalPushEffectXP2 = additionalPushEffectXP;
				int? abilityXP2 = abilityXP;
				List<CConditionalOverride> conditionalOverrides2 = conditionalOverrides;
				AbilityData.MiscAbilityData miscAbilityData2 = miscAbilityData;
				bool? sharePreviousAnim = skipAnim;
				int? conditionDuration2 = conditionDuration;
				EConditionDecTrigger? conditionDecTrigger2 = conditionDecTrigger;
				bool? isInlineSubAbility2 = isInlineSubAbility;
				bool? isConsumeAbility = flag;
				CAbilityControlActor.ControlActorAbilityData controlActorData = controlActorAbilityData;
				CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = changeAllegianceAbilityData;
				CAugment augment2 = augment;
				CSong song2 = song;
				CDoom doom2 = doom;
				List<CAttackEffect> attackEffects2 = attackEffects;
				List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries2 = statIsBasedOnXEntries;
				CAbility addActiveBonusAbility2 = addActiveBonusAbility;
				List<CAbility> chooseAbilities2 = chooseAbilities;
				List<CAbility.EAbilityType> recoverCardAbilityTypeFilter = recoverCardsAbilityTypeFilter;
				CAbility forgoTopActionAbility2 = forgoTopActionAbility;
				CAbility forgoBottomActionAbility2 = forgoBottomActionAbility;
				CAbilityExtraTurn.EExtraTurnType? extraTurnType2 = extraTurnType;
				string actionMonsterClassID2 = actionMonsterClassID;
				CAbilityFilterContainer swapFirstTargetAbilityFilter = swapAbilityFirstTargetFilter;
				CAbilityFilterContainer swapSecondTargetAbilityFilter = swapAbilitySecondTargetFilter;
				CAbilityTeleport.TeleportData teleportData2 = teleportData;
				List<CAbility.EAbilityType> immunityToAbilityTypes2 = immunityToAbilityTypes;
				List<string> addModifiers = addOrRemoveModifiers;
				Dictionary<string, int> resourcesToAddOnAbilityEnd2 = resourcesToAddOnAbilityEnd;
				Dictionary<string, int> resourcesToGiveToTargets2 = resourcesToGiveToTargets;
				Dictionary<string, int> resourcesToTakeFromTargets2 = resourcesToTakeFromTargets;
				CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData2 = destroyObstacleData;
				string helpBoxLocalizationKey = helpBoxTooltipLocKey;
				List<CAbility.EAbilityType> extraTurnAbilityTypeFilter2 = extraTurnAbilityTypeFilter;
				int? maxExtraTurnAbilityLevel2 = maxExtraTurnAbilityLevel;
				bool? useParentTargeting2 = useParentTargeting;
				CAbilityRequirements startAbilityRequirement = startAbilityRequirements;
				int? tokens2 = tokens;
				return new CAbilityOverride(text, parentName2, abilityType2, flags2, strength2, range2, numberOfTargets2, abilityFilter, animationOverload3, animationOverload4, subAbilities2, areaEffect2, attackSourcesOnly2, jump2, fly2, ignoreDifficultTerrain2, ignoreHazardousTerrain2, null, carryOtherActorsOnHex2, moveRestrictionType2, activeBonusData2, activeBonusYML2, areaEffectYMLString2, areaEffectLayoutOverrideYMLString2, areaEffectFlipAxis2, positiveConditions2, negativeConditions2, multiPassAttack2, damageSelfBeforeAttack2, addAttackBaseStat2, strengthIsBase2, rangeIsBase2, targetIsBase2, rangeAtLeastOne2, targetAtLeastOne2, removeConditions, text2, textOnly, showRange2, showTarget2, showArea2, onDeath2, allTargetsOnMovePath2, allTargetsOnMovePathSameStartAndEnd2, allTargetsOnAttackPath2, summons2, xpPerTargetData2, targeting2, targetingVars2, vars2, miscVars2, performVars2, elementsToInfuse2, showElementPicker2, propName2, trapData2, isSubAbility2, pierce2, retaliateRange2, isTargetedAbility2, spawnDelay2, pullType2, additionalPushEffect2, additionalPushEffectDamage2, additionalPushEffectXP2, abilityXP2, conditionalOverrides2, miscAbilityData2, sharePreviousAnim, conditionDuration2, conditionDecTrigger2, isInlineSubAbility2, isConsumeAbility, controlActorData, changeAllegianceData, augment2, song2, doom2, attackEffects2, statIsBasedOnXEntries2, addActiveBonusAbility2, chooseAbilities2, recoverCardAbilityTypeFilter, forgoTopActionAbility2, forgoBottomActionAbility2, extraTurnType2, actionMonsterClassID2, swapFirstTargetAbilityFilter, swapSecondTargetAbilityFilter, teleportData2, null, immunityToAbilityTypes2, addModifiers, null, resourcesToAddOnAbilityEnd2, resourcesToTakeFromTargets2, resourcesToGiveToTargets2, destroyObstacleData2, exploitVulnerability2, helpBoxLocalizationKey, extraTurnAbilityTypeFilter2, maxExtraTurnAbilityLevel2, useParentTargeting2, startAbilityRequirement, null, tokens2);
			}
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Failed to parse ability properties for Ability Override. File: " + filename);
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid property in AbilityOverride.  Property must be Mapping. File: " + filename);
		}
		return null;
	}

	public static void AddStatIsBasedOnX(CAbility ability, int val, CAbility.EAbilityStatType type)
	{
		ability.StatIsBasedOnXEntries.Add(new AbilityData.StatIsBasedOnXData
		{
			AbilityStatType = type,
			BasedOn = CAbility.EStatIsBasedOnXType.Vars,
			Vars = new List<(string, TValue)>
			{
				("FromStrengthOverride", true),
				("Value", new TValue(val))
			},
			AddTo = true
		});
	}

	public static void DelStatIsBasedOnX(CAbility ability, int val, CAbility.EAbilityStatType type)
	{
		AbilityData.StatIsBasedOnXData statIsBasedOnXData = ability.StatIsBasedOnXEntries.First((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == type && x.BasedOn == CAbility.EStatIsBasedOnXType.Vars && x.AddTo && x.Vars.Any(((string, TValue) tuple) => tuple.Item1 == "FromStrengthOverride") && x.Vars.Any(((string, TValue) tuple) => tuple.Item1 == "Value" && tuple.Item2 == val));
		if (statIsBasedOnXData != null)
		{
			ability.StatIsBasedOnXEntries.Remove(statIsBasedOnXData);
		}
	}

	public void OverrideStrength(CAbility ability)
	{
		if (Strength.HasValue)
		{
			OverrideStrength(ability, Strength.Value);
		}
	}

	public void OverrideStrength(CAbility ability, int val)
	{
		if (ability is CAbilityAttack { IsAttackAbilityWithOverrides: false } cAbilityAttack && (cAbilityAttack.ActiveBonusData == null || cAbilityAttack.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA))
		{
			return;
		}
		if (ability is CAbilityMove ability2)
		{
			AddStatIsBasedOnX(ability2, val, CAbility.EAbilityStatType.Strength);
			return;
		}
		ability.Strength += val;
		if (ability.TargetingActor != null)
		{
			ability.SetModifiedStrength(ability.m_ModifiedStrength + val);
		}
		if (ability is CAbilityPush cAbilityPush)
		{
			cAbilityPush.RemainingPushes += val;
		}
		if (ability is CAbilityPull cAbilityPull)
		{
			cAbilityPull.RemainingPulls += val;
		}
	}

	public void OverrideNumberTargets(CAbility ability)
	{
		if (TargetIsBase.HasValue && TargetIsBase.Value)
		{
			ability.NumberTargets = NumberOfTargets.Value;
		}
		else if (ability is CAbilityAttack cAbilityAttack)
		{
			cAbilityAttack.AddExtraTargets(NumberOfTargets.Value);
		}
		else
		{
			ability.NumberTargets += NumberOfTargets.Value;
		}
	}

	public void UndoOverrideStrength(CAbility ability)
	{
		if (Strength.HasValue)
		{
			UndoOverrideStrength(ability, Strength.Value);
		}
	}

	public void UndoOverrideStrength(CAbility ability, int val)
	{
		if (!(ability is CAbilityAttack))
		{
			if (ability is CAbilityMove)
			{
				DelStatIsBasedOnX(ability, val, CAbility.EAbilityStatType.Strength);
			}
			else
			{
				OverrideStrength(ability, -val);
			}
		}
	}

	public void UndoOverrideNumberTargets(CAbility ability)
	{
		if (TargetIsBase.HasValue && TargetIsBase.Value)
		{
			ability.NumberTargets = OriginalAbility.NumberTargets;
		}
		else if (ability is CAbilityAttack cAbilityAttack)
		{
			cAbilityAttack.AddExtraTargets(-NumberOfTargets.Value);
		}
		else
		{
			ability.NumberTargets -= NumberOfTargets.Value;
		}
	}

	public void OverrideVars(CAbility ability)
	{
		if (Vars == null)
		{
			return;
		}
		foreach (var var in Vars)
		{
			string item = var.Item1;
			TValue item2 = var.Item2;
			TValue tValue = item2.Calc(ability.VarsContext);
			if (!(item == "Strength"))
			{
				if (!(item == "AddConditions"))
				{
					continue;
				}
				List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
				List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>();
				foreach (string item3 in tValue.GetObject<IList>())
				{
					CCondition.ENegativeCondition result2;
					if (Enum.TryParse<CCondition.EPositiveCondition>(item3, out var result))
					{
						list.Add(result);
					}
					else if (Enum.TryParse<CCondition.ENegativeCondition>(item3, out result2))
					{
						list2.Add(result2);
					}
					foreach (CCondition.EPositiveCondition posCondition in list)
					{
						try
						{
							if (!ability.PositiveConditions.ContainsKey(posCondition))
							{
								ability.PositiveConditions.Add(posCondition, CAbility.CreateAbility(CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCondition.ToString()), ability.AbilityFilter, ability.IsMonsterAbility, ability.IsTargetedAbility));
							}
						}
						catch
						{
							Log.ErrorWithReport("Condition " + posCondition.ToString() + " could not be found in EAbilityType enum.");
						}
					}
					foreach (CCondition.ENegativeCondition negCondition in list2)
					{
						try
						{
							if (negCondition != CCondition.ENegativeCondition.Disadvantage && !ability.NegativeConditions.ContainsKey(negCondition))
							{
								ability.NegativeConditions.Add(negCondition, CAbility.CreateAbility(CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCondition.ToString()), ability.AbilityFilter, ability.IsMonsterAbility, ability.IsTargetedAbility));
							}
						}
						catch
						{
							Log.ErrorWithReport("Error: Condition " + negCondition.ToString() + " could not be found in EAbilityType enum.");
						}
					}
				}
			}
			else
			{
				OverrideStrength(ability, tValue.GetInt(ability.VarsContext));
			}
		}
	}

	public void UndoOverrideVars(CAbility ability)
	{
		if (Vars == null)
		{
			return;
		}
		foreach (var var in Vars)
		{
			string item = var.Item1;
			TValue item2 = var.Item2;
			TValue tValue = item2.Calc(ability.VarsContext);
			if (!(item == "Strength"))
			{
				if (!(item == "AddConditions"))
				{
					continue;
				}
				List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
				List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>();
				foreach (string item3 in tValue.GetObject<IList>())
				{
					CCondition.ENegativeCondition result2;
					if (Enum.TryParse<CCondition.EPositiveCondition>(item3, out var result))
					{
						list.Add(result);
					}
					else if (Enum.TryParse<CCondition.ENegativeCondition>(item3, out result2))
					{
						list2.Add(result2);
					}
					foreach (CCondition.EPositiveCondition item4 in list)
					{
						if (ability.PositiveConditions.Keys.Contains(item4))
						{
							ability.PositiveConditions.Remove(item4);
						}
					}
					foreach (CCondition.ENegativeCondition item5 in list2)
					{
						if (item5 != CCondition.ENegativeCondition.Disadvantage && ability.NegativeConditions.Keys.Contains(item5))
						{
							ability.NegativeConditions.Remove(item5);
						}
					}
				}
			}
			else
			{
				UndoOverrideStrength(ability, tValue.GetInt(ability.VarsContext));
			}
		}
	}

	public TValue GetVar(string key, CVarsContext context = null, bool asValue = true)
	{
		TValue result;
		if (Vars != null)
		{
			foreach (var var in Vars)
			{
				if (var.Item1 != key)
				{
					continue;
				}
				result = var.Item2;
				TValue tValue = result.Calc(context, asValue);
				result = (tValue.isUnknown ? TValue.Unknown : tValue);
				goto IL_0071;
			}
		}
		return TValue.Unknown;
		IL_0071:
		return result;
	}
}
