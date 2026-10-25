using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Libs.Utils;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityAttack : CAbility
{
	public enum EAttackState
	{
		None,
		SelectAttackFocus,
		CheckForAIToggleBonuses,
		PreActorIsAttacking,
		SelectActiveBonusBuffTarget,
		UpdateAttackFocusAfterAttackEffectInlineSubAbility,
		CheckForDamageSelf,
		DrawingModifiers,
		ActorHasAttacked,
		ActorBeenAttacked,
		CheckForPlayerIdle,
		FinaliseAttack,
		AttackDone,
		AttackBuff,
		AttackBuffDone,
		SelectAttackFocusAdditionalTargets,
		SelectAdditionalTargetsDone,
		SelectAttackFocusAddTargetBuff,
		SelectAttackFocusAddTargetBuffDone
	}

	public class AttackModifierAbilityInfo
	{
		public CAbility Ability;

		public CActor TargetingActor;

		public bool ControlTargetingActor;

		public AttackModifierYMLData Card;
	}

	[ConsoleVariable(Alias = "debug_attack_modifier_card", Persistent = false)]
	public static string ConsoleVar_DebugAttackModifierCard = "";

	[StateDump(Skip = true)]
	public static bool ConfirmAttackAbilityBeforeFinishing = false;

	public static readonly EAttackState[] AttackStates = (EAttackState[])Enum.GetValues(typeof(EAttackState));

	public const int PIERCE_ALL = 99999;

	public const string DEFAULT_ATTACK_NAME = "DefaultAttack";

	[StateDump(Skip = true)]
	public List<AttackModifierAbilityInfo> CachedModifierAbilities;

	public bool ShadowMarkTarget;

	private bool m_PlacesShadowTokens;

	protected bool m_CanClearTargets = true;

	private static readonly List<EAbilityType> AbilityTypesToWaitForRetaliate = new List<EAbilityType>
	{
		EAbilityType.Push,
		EAbilityType.Pull,
		EAbilityType.Teleport2,
		EAbilityType.Move
	};

	private List<CAttackSummary.TargetSummary> m_PostConditionActorsToCheck;

	private List<CAttackSummary.TargetSummary> m_PostConditionActorsChecked = new List<CAttackSummary.TargetSummary>();

	[StateDump(Skip = true)]
	[JsonIgnore]
	public bool IsAttackAbilityWithOverrides;

	[StateDump(Skip = true)]
	[JsonIgnore]
	private List<CActor> SingleTargetActorsToTarget;

	protected bool m_TreatAsRangeBuff;

	private int PendingExtraTargetsToSelect;

	private static CAbilityControlActor s_ControlModifierAbility;

	private EAttackState m_State;

	private List<CActor> m_AttackedActors = new List<CActor>();

	private List<CActor> m_ActorsToTargetLocked = new List<CActor>();

	private List<int> m_ActorsToTargetModifiedStrength = new List<int>();

	[JsonProperty]
	private bool m_MultiPassAttack;

	private bool m_MultiPassAttackStarted;

	[JsonProperty]
	private bool m_ExploitVulnerability;

	private List<CActor> m_MultiPassActors;

	[JsonProperty]
	private bool m_ChainAttack;

	[JsonProperty]
	private int m_ChainAttackRange;

	[JsonProperty]
	private int m_ChainAttackDamageReduction;

	private CAttackSummary m_AttackSummary;

	private List<CAttackSummary.TargetSummary> m_TotalTargetSummaries = new List<CAttackSummary.TargetSummary>();

	private bool m_ProcessedStatusEffects;

	private int m_TotalNumberTargets;

	[JsonProperty]
	private bool m_IsDefaultAttack;

	private int m_AreaRangeTileX;

	private int m_AreaRangeTileY;

	private int m_CurrentAttackIndex;

	private int m_CurrentPassAttackIndex;

	private List<CActiveBonus> m_StackedInlineAbilityActiveBonuses = new List<CActiveBonus>();

	private CActiveBonus m_CurrentStackedInlineAbilityActiveBonus;

	private bool m_FirstAttackofAction = true;

	private bool m_ModifiersChecked;

	private bool m_InlineModifiersChecked;

	private List<AttackModifierAbilityInfo> m_ModifierAbilities;

	private List<AttackModifierYMLData> m_PreviousAttackModsForTarget;

	private bool m_IsInProcessModifierAbilities;

	private bool m_SelectAdditionalTargetsDone;

	private static int s_AttackValueOverride = int.MaxValue;

	public static string s_AttackModifierCardOverride;

	public static CActor s_LastAttacker;

	[StateDump(Skip = true)]
	[JsonIgnore]
	public int? OriginalStrength;

	public static bool InCalcTargetSummary = false;

	public static int AttackValueOverride => s_AttackValueOverride;

	public EAttackState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public CAttackSummary AttackSummary
	{
		get
		{
			return m_AttackSummary;
		}
		set
		{
			m_AttackSummary = value;
		}
	}

	public bool MultiPassAttack
	{
		get
		{
			return m_MultiPassAttack;
		}
		set
		{
			m_MultiPassAttack = value;
		}
	}

	public bool ExploitVulnerability
	{
		get
		{
			return m_ExploitVulnerability;
		}
		set
		{
			m_ExploitVulnerability = value;
		}
	}

	public bool ChainAttack
	{
		get
		{
			return m_ChainAttack;
		}
		set
		{
			m_ChainAttack = value;
		}
	}

	public int ChainAttackRange
	{
		get
		{
			return m_ChainAttackRange;
		}
		set
		{
			m_ChainAttackRange = value;
		}
	}

	public int ChainAttackDamageReduction
	{
		get
		{
			return m_ChainAttackDamageReduction;
		}
		set
		{
			m_ChainAttackDamageReduction = value;
		}
	}

	public List<CActor> MultiPassActors => m_MultiPassActors;

	[JsonProperty]
	public int Pierce { get; set; }

	[JsonProperty]
	public int DamageSelfBeforeAttack { get; set; }

	public bool IsDefaultAttack => m_IsDefaultAttack;

	public int AreaRangeTileX
	{
		get
		{
			return m_AreaRangeTileX;
		}
		set
		{
			m_AreaRangeTileX = value;
		}
	}

	public int AreaRangeTileY
	{
		get
		{
			return m_AreaRangeTileY;
		}
		set
		{
			m_AreaRangeTileY = value;
		}
	}

	[JsonProperty]
	public List<CAttackEffect> AttackEffects { get; set; }

	public List<AttackModifierAbilityInfo> ModifierAbilities => m_ModifierAbilities;

	public CAttackSummary.TargetSummary CurrentAttackingTargetSummary { get; set; }

	public List<CAttackSummary.TargetSummary> TotalTargetSummaries => m_TotalTargetSummaries;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> ElementsToConsume { get; set; }

	[JsonProperty]
	public List<CCondition.ENegativeCondition> ElementalConditions { get; set; }

	public List<(CActor, CActor)> RedirectedActors { get; set; } = new List<(CActor, CActor)>();

	public bool TreatAsRangeBuff
	{
		get
		{
			return m_TreatAsRangeBuff;
		}
		set
		{
			m_TreatAsRangeBuff = value;
		}
	}

	public bool IsMeleeAttack
	{
		get
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.TreatAsMelee.HasValue)
			{
				return MiscAbilityData.TreatAsMelee.Value;
			}
			if (!TreatAsRangeBuff)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 == null || !miscAbilityData2.TreatAsRange.HasValue || !MiscAbilityData.TreatAsRange.Value)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.UseParentRangeType == true && ParentAbility != null)
					{
						if (ParentAbility is CAbilityAttack cAbilityAttack)
						{
							return cAbilityAttack.IsMeleeAttack;
						}
						if (ParentAbility is CAbilityMerged cAbilityMerged)
						{
							CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
							if (mergedWithAbility.Range > 1)
							{
								AbilityData.MiscAbilityData miscAbilityData4 = mergedWithAbility.MiscAbilityData;
								if (miscAbilityData4 == null)
								{
									return false;
								}
								return miscAbilityData4.TreatAsMelee == true;
							}
							return true;
						}
						if (ParentAbility.Range > 1)
						{
							AbilityData.MiscAbilityData miscAbilityData5 = ParentAbility.MiscAbilityData;
							if (miscAbilityData5 == null)
							{
								return false;
							}
							return miscAbilityData5.TreatAsMelee == true;
						}
						return true;
					}
					if (AreaEffect != null)
					{
						return AreaEffect.Melee;
					}
					if (TargetingActor != null && TargetingActor.IsOriginalMonsterType && Range <= 1)
					{
						int value = ConditionalOverrides.Where((CConditionalOverride co) => co.Requirements.MeetsAbilityRequirements(TargetingActor, this)).Sum((CConditionalOverride co) => co.AbilityOverrides.Sum((CAbilityOverride ao) => ao.Range)).Value;
						if (value <= 0)
						{
							return (TargetingActor.Class as CMonsterClass).Range <= 1;
						}
						return value <= 1;
					}
					return Range <= 1;
				}
			}
			return false;
		}
	}

	private static CAbilityControlActor controlModifierAbility
	{
		get
		{
			if (s_ControlModifierAbility == null)
			{
				s_ControlModifierAbility = CAbility.CreateAbility(EAbilityType.ControlActor, 1, useSpecialBaseStat: false, 99, -1, 0, EAbilityFlag.None, targetingVars: new List<(string, TValue)>
				{
					("AddTileInRange", new TValue(CExpression.Parse("Ability.ParentAbility.TargetingActor.Tile"))),
					("AddValidActorInRange", new TValue(CExpression.Parse("Ability.ParentAbility.TargetingActor"))),
					("AddActorToTarget", new TValue(CExpression.Parse("Ability.ParentAbility.TargetingActor"))),
					("AddSelectedTile", new TValue(CExpression.Parse("Ability.ParentAbility.TargetingActor.Tile")))
				}, abilityFilter: new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self | CAbilityFilter.EFilterTargetType.Ally), animationOverload: null, animationOverload2: null, subAbilities: null, areaEffect: null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, aiFocusOverride: null, moveRestrictionType: CAbilityMove.EMoveRestrictionType.None, activeBonusData: null, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: EAbilityTargeting.Custom, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, miscAbilityData: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue) as CAbilityControlActor;
				s_ControlModifierAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				s_ControlModifierAbility.MiscAbilityData.AutotriggerAbility = true;
			}
			return s_ControlModifierAbility;
		}
	}

	public IEnumerable<CAttackSummary.TargetSummary> CurrentAndPreviousTargetSummaries()
	{
		foreach (CAttackSummary.TargetSummary totalTargetSummary in TotalTargetSummaries)
		{
			yield return totalTargetSummary;
		}
		if (AttackSummary?.Targets == null)
		{
			yield break;
		}
		foreach (CAttackSummary.TargetSummary target in AttackSummary.Targets)
		{
			if (!TotalTargetSummaries.Contains(target) && target.State != CAttackSummary.TargetSummary.SummaryState.SelectingTargets)
			{
				yield return target;
			}
		}
	}

	public bool IsAttackType(EAttackType attackType)
	{
		return attackType switch
		{
			EAttackType.Melee => IsMeleeAttack, 
			EAttackType.Ranged => !IsMeleeAttack, 
			EAttackType.Default => IsDefaultAttack, 
			EAttackType.Control => IsControlAbility, 
			_ => true, 
		};
	}

	public List<EAttackType> AttackIsAttackTypes()
	{
		List<EAttackType> list = new List<EAttackType> { EAttackType.Attack };
		if (IsMeleeAttack)
		{
			list.Add(EAttackType.Melee);
		}
		else
		{
			list.Add(EAttackType.Ranged);
		}
		if (IsDefaultAttack)
		{
			list.Add(EAttackType.Default);
		}
		if (IsControlAbility)
		{
			list.Add(EAttackType.Control);
		}
		return list;
	}

	public CAbilityAttack(bool multiPassAttack, bool chainAttack, int pierce, bool isDefaultAttack, List<CAttackEffect> attackEffects, int chainAttackRange, int chainAttackDamageReduction, int damageSelfBeforeAttack, List<ElementInfusionBoardManager.EElement> elementsToConsume, List<CCondition.ENegativeCondition> elementalConditions, bool exploitVulnerability)
	{
		m_MultiPassAttack = multiPassAttack;
		m_ChainAttack = chainAttack;
		m_ChainAttackRange = chainAttackRange;
		m_ChainAttackDamageReduction = chainAttackDamageReduction;
		Pierce = pierce;
		DamageSelfBeforeAttack = damageSelfBeforeAttack;
		m_IsDefaultAttack = isDefaultAttack;
		AttackEffects = attackEffects ?? new List<CAttackEffect>();
		m_ModifierAbilities = new List<AttackModifierAbilityInfo>();
		m_PreviousAttackModsForTarget = new List<AttackModifierYMLData>();
		ElementsToConsume = elementsToConsume;
		ElementalConditions = ((elementalConditions == null) ? new List<CCondition.ENegativeCondition>() : elementalConditions);
		m_ExploitVulnerability = exploitVulnerability;
	}

	public static CAbility CreateDefaultAttack(int strength, int range, int numberOfTargets, bool isMonster, string animationOverload = "", bool isMultipass = true, CAbilityFilterContainer abilityfilter = null, AbilityData.ActiveBonusData activeBonuseData = null, bool isDefault = true, CAIFocusOverrideDetails focusOverrideDetails = null, int xp = 0)
	{
		CAbility cAbility = CAbility.CreateAbility(EAbilityType.Attack, strength, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, (abilityfilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityfilter, animationOverload, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, focusOverrideDetails, CAbilityMove.EMoveRestrictionType.None, activeBonuseData, null, "", "", "", "DefaultAttack", new List<CEnhancement>(), null, null, isMultipass, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), xp, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, isDefault: isDefault, statIsBasedOnXEntries: new List<AbilityData.StatIsBasedOnXData>(), moveObstacleRange: 0, moveObstacleAnimOverload: string.Empty, slotsToRefresh: new List<CItem.EItemSlot>(), slotStatesToRefresh: new List<CItem.EItemSlotState>(), addActiveBonusAbility: null, overrideAugmentAttackType: EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: new List<EAbilityType>(), cardAction: ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
		cAbility.SetMiscVar("PrintedProperties", new TValue(new List<object> { "Attack" }));
		return cAbility;
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (TreatAsAlwaysPerformable)
		{
			return true;
		}
		if (m_ValidActorsInRange.Count <= 0)
		{
			if (m_State == EAttackState.SelectAttackFocus)
			{
				return ActorsToTarget.Count > 0;
			}
			return false;
		}
		return true;
	}

	public override CAbility CreateInterruptCopy(CActor targetingActor, bool killAfter, bool forced, bool interruptRegardlessOfStrength = false)
	{
		CAbility cAbility = base.CreateInterruptCopy(targetingActor, killAfter, forced, interruptRegardlessOfStrength);
		if (cAbility != null)
		{
			return cAbility;
		}
		if (MultiPassAttack && (m_State == EAttackState.ActorBeenAttacked || m_State == EAttackState.CheckForPlayerIdle) && ((m_MultiPassActors.Count > 0) | interruptRegardlessOfStrength))
		{
			CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
			CAbility cAbility2 = CAbility.CopyAbility(this, generateNewID: false, fullCopy: true);
			CopyInterruptedProperties(cAbility2, Strength, initialCopy: true, forced);
			PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Remove(currentPhaseAbility);
			return cAbility2;
		}
		return null;
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		CAbilityAttack cAbilityAttack = (CAbilityAttack)destAbility;
		cAbilityAttack.PendingExtraTargetsToSelect = PendingExtraTargetsToSelect;
		cAbilityAttack.m_State = m_State;
		cAbilityAttack.m_AttackedActors = m_AttackedActors?.ToList();
		cAbilityAttack.m_ActorsToTargetLocked = m_ActorsToTargetLocked?.ToList();
		cAbilityAttack.m_ActorsToTargetModifiedStrength = m_ActorsToTargetModifiedStrength?.ToList();
		cAbilityAttack.m_MultiPassAttackStarted = m_MultiPassAttackStarted;
		cAbilityAttack.m_MultiPassActors = m_MultiPassActors?.ToList();
		cAbilityAttack.m_AttackSummary = m_AttackSummary;
		cAbilityAttack.m_TotalTargetSummaries = m_TotalTargetSummaries?.ToList();
		cAbilityAttack.m_ProcessedStatusEffects = m_ProcessedStatusEffects;
		cAbilityAttack.m_TotalNumberTargets = m_TotalNumberTargets;
		cAbilityAttack.m_AreaRangeTileX = m_AreaRangeTileX;
		cAbilityAttack.m_AreaRangeTileY = m_AreaRangeTileY;
		cAbilityAttack.m_CurrentAttackIndex = m_CurrentAttackIndex;
		cAbilityAttack.m_CurrentPassAttackIndex = m_CurrentPassAttackIndex;
		cAbilityAttack.m_StackedInlineAbilityActiveBonuses = m_StackedInlineAbilityActiveBonuses?.ToList();
		cAbilityAttack.m_CurrentStackedInlineAbilityActiveBonus = m_CurrentStackedInlineAbilityActiveBonus;
		cAbilityAttack.m_FirstAttackofAction = m_FirstAttackofAction;
		cAbilityAttack.m_ModifiersChecked = m_ModifiersChecked;
		cAbilityAttack.m_InlineModifiersChecked = m_InlineModifiersChecked;
		cAbilityAttack.m_ModifierAbilities = m_ModifierAbilities?.ToList();
		cAbilityAttack.m_PreviousAttackModsForTarget = m_PreviousAttackModsForTarget?.ToList();
		cAbilityAttack.m_PlacesShadowTokens = m_PlacesShadowTokens;
		cAbilityAttack.m_TargetedActor = m_TargetedActor;
		if (m_IsInProcessModifierAbilities)
		{
			cAbilityAttack.m_ProcessedStatusEffects = true;
		}
		if (m_State == EAttackState.ActorBeenAttacked && MultiPassAttack)
		{
			cAbilityAttack.m_State = EAttackState.CheckForPlayerIdle;
			cAbilityAttack.m_ProcessedStatusEffects = true;
			cAbilityAttack.DontMoveAbilityState = false;
		}
		cAbilityAttack.m_PostConditionActorsToCheck = m_PostConditionActorsToCheck?.ToList();
		cAbilityAttack.m_PostConditionActorsChecked = m_PostConditionActorsChecked?.ToList();
		cAbilityAttack.OriginalStrength = OriginalStrength;
	}

	public override void SetModifiedStrength(int val)
	{
		base.SetModifiedStrength(val);
		if (AttackSummary != null && (ActiveBonusData == null || ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA))
		{
			InitAttackSummary();
		}
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		if (!OriginalStrength.HasValue)
		{
			OriginalStrength = Strength;
		}
		else
		{
			ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
			int strength = (m_ModifiedStrength = OriginalStrength.Value);
			Strength = strength;
			foreach (CAbilityOverride currentOverride in CurrentOverrides)
			{
				if (currentOverride.Strength.HasValue)
				{
					currentOverride.OverrideStrength(this);
				}
			}
			if (AppliedEnhancements)
			{
				ApplyEnhancement(AbilityEnhancements.Where((CEnhancement x) => x.Enhancement == EEnhancement.PlusAttack).ToList());
			}
		}
		OriginalPositiveConditions.Clear();
		OriginalPositiveConditions.AddRange(PositiveConditions);
		OriginalNegativeConditions.Clear();
		OriginalNegativeConditions.AddRange(NegativeConditions);
		s_LastAttacker = null;
		if (targetingActor.Class is CMonsterClass { StatIsBasedOnXEntries: var statIsBasedOnXEntries } cMonsterClass && statIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Attack) && !StrengthIsBase)
		{
			foreach (AbilityData.StatIsBasedOnXData item2 in cMonsterClass.StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.AddTo && x.BaseStatType == EMonsterBaseStats.Attack && x.HasBeenAdded))
			{
				item2.AddedStat = 0;
				item2.HasBeenAdded = false;
			}
			SetStatBasedOnX(targetingActor, cMonsterClass.StatIsBasedOnXEntries, AbilityFilter);
		}
		if (!IsDefaultAttack && targetingActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.SummonData.AttackOverride != null)
		{
			OverrideAbilityValues(cHeroSummonActor.SummonData.AttackOverride, perform: false);
		}
		if (Augment != null && !m_AugmentsAdded)
		{
			targetingActor.AddAugmentOrSong(this, targetingActor);
			m_AugmentsAdded = true;
		}
		CachedAIMoveFocusActors?.RemoveAll((CActor a) => a?.IsDead ?? true);
		CachedAIMoveFocusActors?.RemoveAll((CActor a) => a.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
		List<EAttackType> overrideAttackTypes = new List<EAttackType>();
		foreach (CActiveBonus item3 in targetingActor.FindApplicableActiveBonuses(EAbilityType.OverrideAugmentAttackType))
		{
			if (item3.Ability is CAbilityOverrideAugmentAttackType cAbilityOverrideAugmentAttackType && !overrideAttackTypes.Contains(cAbilityOverrideAugmentAttackType.OverrideAttackType))
			{
				overrideAttackTypes.Add(cAbilityOverrideAugmentAttackType.OverrideAttackType);
			}
		}
		m_AbilityAugments = targetingActor.Augments.Where((CAugment w) => w.AugmentType == CAugment.EAugmentType.Ability && (w.AttackType == EAttackType.Attack || overrideAttackTypes.Contains(EAttackType.Attack) || ((w.AttackType == EAttackType.Melee || overrideAttackTypes.Contains(EAttackType.Melee)) && IsMeleeAttack) || ((w.AttackType == EAttackType.Ranged || overrideAttackTypes.Contains(EAttackType.Ranged)) && !IsMeleeAttack))).ToList();
		m_OverrideAugments = targetingActor.Augments.Where((CAugment w) => w.AugmentType == CAugment.EAugmentType.Override && (w.AttackType == EAttackType.Attack || overrideAttackTypes.Contains(EAttackType.Attack) || ((w.AttackType == EAttackType.Melee || overrideAttackTypes.Contains(EAttackType.Melee)) && IsMeleeAttack) || ((w.AttackType == EAttackType.Ranged || overrideAttackTypes.Contains(EAttackType.Ranged)) && !IsMeleeAttack))).ToList();
		CActiveBonus.FindApplicableActiveBonuses(targetingActor, EAbilityType.Attack).AddRange(CActiveBonus.FindApplicableActiveBonuses(targetingActor, EAbilityType.AddActiveBonus));
		if ((ActiveBonusData == null || ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA) && !m_AugmentOverridesProcessed && m_OverrideAugments.Count > 0)
		{
			foreach (CAugment overrideAugment in m_OverrideAugments)
			{
				foreach (CAbilityOverride abilityOverride in overrideAugment.AbilityOverrides)
				{
					OverrideAbilityValues(abilityOverride, perform: false);
				}
			}
		}
		m_AugmentOverridesProcessed = true;
		CAbility cAbility = null;
		if (HasStarted && !IsAttackAbilityWithOverrides && SingleTargetActorsToTarget == null)
		{
			CAbility abilityInterruptCopy = AbilityInterruptCopy;
			cAbility = CAbility.CopyAbility(this, generateNewID: false, fullCopy: true);
			CopyInterruptedProperties(cAbility, Strength, initialCopy: true, forced: false);
			AbilityInterruptCopy = abilityInterruptCopy;
		}
		base.Start(targetingActor, filterActor, controllingActor);
		m_AugmentAbilitiesTargetCount = 0;
		m_CurrentAttackIndex = 0;
		m_CurrentPassAttackIndex = 0;
		m_ModifierAbilities = new List<AttackModifierAbilityInfo>();
		m_PreviousAttackModsForTarget = new List<AttackModifierYMLData>();
		m_FirstAttackofAction = true;
		m_ModifiersChecked = false;
		m_InlineModifiersChecked = false;
		m_SelectAdditionalTargetsDone = false;
		m_State = EAttackState.SelectAttackFocus;
		if (m_MultiPassAttack && (AllTargetsOnMovePath || AllTargetsInRangeAlongPath))
		{
			m_MultiPassAttack = false;
		}
		if (m_MultiPassAttack)
		{
			m_MultiPassAttackStarted = false;
		}
		if (CanUndoOverride.HasValue)
		{
			m_CanUndo = CanUndoOverride.Value;
		}
		else
		{
			CPhase phase = PhaseManager.Phase;
			if (phase != null && phase.Type == CPhase.PhaseType.Action)
			{
				CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
				if (cPhaseAction.PreviousPhaseAbilities != null && cPhaseAction.PreviousPhaseAbilities.Count > 0)
				{
					m_CanUndo = !cPhaseAction.PreviousPhaseAbilities.Last().IsCostAbility;
				}
			}
		}
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (Targeting == EAbilityTargeting.Custom)
		{
			AreaEffect = null;
		}
		if (UseSubAbilityTargeting)
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.IgnoreParentAreaOfEffect.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.IgnoreParentAreaOfEffect.Value)
				{
					AreaEffect = null;
					goto IL_05fd;
				}
			}
			if (AreaEffect == null && ParentAbility?.AreaEffect != null)
			{
				AreaEffect = ParentAbility.AreaEffect.Copy();
			}
			goto IL_05fd;
		}
		if (IsMergedAbility)
		{
			AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
			if (miscAbilityData3 == null || !miscAbilityData3.IgnoreMergedAbilityTargetSelection.HasValue)
			{
				if (ParentAbility != null && ParentAbility is CAbilityMerged cAbilityMerged)
				{
					CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
					if (mergedWithAbility != null && AllTargetsOnMovePath && mergedWithAbility is CAbilityMove)
					{
						SharedAbilityTargeting.GetValidActorsInRange(this);
						TilesInRange.Clear();
						foreach (CActor item4 in ValidActorsInRange)
						{
							CTile item = ScenarioManager.Tiles[item4.ArrayIndex.X, item4.ArrayIndex.Y];
							TilesInRange.Add(item);
							m_ActorsToTarget.Add(item4);
						}
						if (m_ActorsToTarget.Count <= 0)
						{
							SetCancelAbilityFlag(cancel: true);
						}
					}
					else if (mergedWithAbility != null && mergedWithAbility.TilesSelected != null && mergedWithAbility.TilesSelected.Count > 0)
					{
						TilesInRange.Clear();
						foreach (CTile item5 in mergedWithAbility.TilesSelected)
						{
							AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
							if (miscAbilityData4 != null && miscAbilityData4.UseMergedWithAbilityTiles.HasValue)
							{
								AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
								if (miscAbilityData5 != null && miscAbilityData5.UseMergedWithAbilityTiles.Value)
								{
									TilesInRange.Add(item5);
									goto IL_0d0a;
								}
							}
							CActor cActor = ScenarioManager.Scenario.FindActorAt(item5.m_ArrayIndex);
							if (cActor != null)
							{
								m_ActorsToIgnore.Add(cActor);
							}
							TilesInRange.AddRange(GameState.GetTilesInRange(item5.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
							goto IL_0d0a;
							IL_0d0a:
							foreach (CActor item6 in GameState.GetActorsInRange(item5.m_ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible))
							{
								if (!m_ValidActorsInRange.Contains(item6))
								{
									m_ValidActorsInRange.Add(item6);
								}
							}
						}
						if (m_ValidActorsInRange.Count <= 0)
						{
							SetCancelAbilityFlag(cancel: true);
						}
					}
				}
				else
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				if (NumberTargets != -1)
				{
					NumberTargets = ValidActorsInRange.Count;
				}
				goto IL_0e28;
			}
		}
		if (!AbilityHasBeenCancelled)
		{
			if (AllTargetsInRangeAlongPath)
			{
				SharedAbilityTargeting.GetValidActorsInRangeAlongPath(this, CAbilityMove.s_AllArrayIndicesAlongPath, ref m_ValidActorsInRange);
			}
			else
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
		}
		goto IL_0e28;
		IL_0ae4:
		NumberTargets = ValidActorsInRange.Count;
		if (ValidActorsInRange.Count <= 0)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		goto IL_0e28;
		IL_05fd:
		if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
		{
			TilesInRange = InlineSubAbilityTiles.ToList();
			foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
			{
				CActor cActor2 = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
				if (cActor2 != null && !m_ValidActorsInRange.Contains(cActor2) && AbilityFilter.IsValidTarget(cActor2, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, false))
				{
					m_ValidActorsInRange.Add(cActor2);
				}
			}
			m_ActorsToTarget = ValidActorsInRange.ToList();
		}
		else if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ValidActorsInRange = new List<CActor> { TargetingActor };
			m_ActorsToTarget = ValidActorsInRange.ToList();
		}
		else if (AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = ParentAbility.ValidTilesInAreaAffected.ToList();
			ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffect);
			m_ActorsToTarget = ValidActorsInRange.ToList();
		}
		else
		{
			AbilityData.MiscAbilityData miscAbilityData6 = MiscAbilityData;
			if (miscAbilityData6 != null && miscAbilityData6.AllTargetsAdjacentToParentTargets.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData7 = MiscAbilityData;
				if (miscAbilityData7 != null && miscAbilityData7.AllTargetsAdjacentToParentTargets.Value)
				{
					m_IsSubAbility = false;
					m_AllTargets = true;
					foreach (CActor item7 in ParentAbility.ActorsTargeted)
					{
						if (item7 != null)
						{
							m_ActorsToIgnore.Add(item7);
						}
					}
					TilesInRange = GameState.GetTilesInRange(ParentAbility.ActorsTargeted[0].ArrayIndex, Range, Targeting, emptyTilesOnly: false);
					m_ValidActorsInRange = GameState.GetActorsInRange(ParentAbility.ActorsTargeted[0].ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
					goto IL_0ae4;
				}
			}
			if (AbilityFilter.Equals(ParentAbility.AbilityFilter) && ParentAbility?.ActorsTargeted != null)
			{
				ValidActorsInRange = ParentAbility.ActorsTargeted.Where((CActor x) => !x.IsDead).ToList();
				m_ActorsToTarget = m_ValidActorsInRange.ToList();
			}
			else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
			{
				foreach (CTile item8 in ParentAbility.TilesSelected)
				{
					TilesInRange.Clear();
					AbilityData.MiscAbilityData miscAbilityData8 = MiscAbilityData;
					if (miscAbilityData8 != null && miscAbilityData8.UseParentTiles.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData9 = MiscAbilityData;
						if (miscAbilityData9 != null && miscAbilityData9.UseParentTiles.Value)
						{
							TilesInRange.AddRange(ParentAbility.TilesSelected);
							goto IL_0a24;
						}
					}
					CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item8.m_ArrayIndex);
					if (cActor3 != null)
					{
						m_ActorsToIgnore.Add(cActor3);
					}
					TilesInRange.AddRange(GameState.GetTilesInRange(item8.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
					goto IL_0a24;
					IL_0a24:
					foreach (CActor item9 in GameState.GetActorsInRange(item8.m_ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible))
					{
						if (!m_ValidActorsInRange.Contains(item9))
						{
							m_ValidActorsInRange.Add(item9);
						}
					}
				}
			}
			else
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
		}
		goto IL_0ae4;
		IL_0e28:
		if (NumberTargets == -1 || AllTargetsInRangeAlongPath)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
		}
		if (AreaEffect != null)
		{
			if (MiscAbilityData != null && MiscAbilityData.ExactRange.HasValue && MiscAbilityData.ExactRange.Value)
			{
				m_AreaEffectLocked = true;
			}
			if (targetingActor.Type == CActor.EType.Player)
			{
				m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y], AreaEffect, 0f, flipped: false, AreaEffectFlipAxis, GameState.GetTilesInRange(targetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true), ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks, Range);
			}
			else
			{
				UpdateAreaEffect(ScenarioManager.Tiles[m_AreaRangeTileX, m_AreaRangeTileY], AreaEffectAngle, AreaEffectFlipped);
				ValidActorsInRange = GameState.GetActorsInRange(targetingActor, filterActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
				RemoveImmuneActorsFromList(ref m_ValidActorsInRange, onlyCheck: false);
				RemoveImmuneActorsFromList(ref m_ActorsToTarget, onlyCheck: false);
			}
		}
		RecalcCanTargetFromAnywhere();
		if (SingleTargetActorsToTarget != null)
		{
			m_ValidActorsInRange = SingleTargetActorsToTarget?.ToList();
			m_ActorsToTarget = SingleTargetActorsToTarget?.ToList();
		}
		if (cAbility != null)
		{
			if (cAbility.AreaEffect != null)
			{
				if (cAbility.AreaEffectLocked)
				{
					AreaEffect = cAbility.AreaEffect;
					AreaEffectAngle = cAbility.AreaEffectAngle;
					AreaEffectFlipped = cAbility.AreaEffectFlipped;
					TileSelected(cAbility.LastSelectedAreaEffectTile, null);
				}
			}
			else
			{
				foreach (CTile item10 in cAbility.TilesSelected)
				{
					TileSelected(item10, null);
				}
			}
		}
		CActiveBonus.RefreshAllAuraActiveBonuses();
		if ((TargetingActor is CPlayerActor || TargetingActor is CHeroSummonActor) && CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.ShadowBuffAttack, useOriginalType: true).Count > 0)
		{
			m_PlacesShadowTokens = true;
		}
		if (!AbilityStartListenersInvoked)
		{
			AbilityStartListenersInvoked = true;
			if ((ActiveBonusData == null || ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA) && targetingActor.m_OnAttackStartListeners != null)
			{
				targetingActor.m_OnAttackStartListeners(this);
			}
		}
		bool flag = !InCalcTargetSummary;
		if (ActiveSingleTargetItems.Any((CItem x) => x.YMLData.Trigger == CItem.EItemTrigger.OnAttackedBeforeAttackModifiers))
		{
			flag = false;
		}
		if (flag && ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			foreach (CActiveBonus item11 in TargetingActor.FindApplicableActiveBonuses(EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack))
			{
				if (!(item11.Ability is CAbilityAttack cAbilityAttack))
				{
					continue;
				}
				foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
				{
					attackEffect.StackSubAbilities(this);
				}
			}
		}
		CAbilityFilter.CurrentVarsContext = VarsContext;
		InitAttackSummary();
		CAbilityFilter.CurrentVarsContext = null;
		RedirectedActors.Clear();
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public void RecalcCanTargetFromAnywhere()
	{
		List<CActor> list = TargetFromAnywhereActors();
		if (list.Count > 0)
		{
			ValidActorsInRange.AddRange(list);
			TilesInRange.AddRange(list.Select((CActor x) => x.Tile));
		}
	}

	public override bool CanTargetFromAnywhere(CActor target)
	{
		if (!TargetingActor.CanTargetFromAnywhere(Range, m_ValidActorsInRange.Count > 0))
		{
			return false;
		}
		if (ActorsToIgnore.Contains(target))
		{
			return false;
		}
		AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.TargetOneEnemyWithAllAttacks.HasValue)
		{
			AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.TargetOneEnemyWithAllAttacks.Value && ActorsTargeted != null && ActorsTargeted.Count > 0 && !ActorsTargeted.Contains(target))
			{
				return false;
			}
		}
		if (!target.CanBeTargetedFromAnywhere())
		{
			return false;
		}
		if (!AbilityFilter.IsValidTarget(target, FilterActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
		{
			return false;
		}
		return true;
	}

	public override List<CActor> TargetFromAnywhereActors()
	{
		List<CActor> actorList = new List<CActor>();
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (CanTargetFromAnywhere(allAliveActor))
			{
				actorList.Add(allAliveActor);
			}
		}
		RemoveImmuneActorsFromList(ref actorList, onlyCheck: false);
		return actorList;
	}

	public override int ModifiedStrength()
	{
		if (m_AttackSummary != null)
		{
			return m_AttackSummary.ModifiedAttackStrength;
		}
		return m_ModifiedStrength;
	}

	protected override void ApplyBuffs(CActor actor)
	{
		base.ApplyBuffs(actor);
		GetAttackBonuses(actor, this, out var _, out var _, out var _, out var _, out var _);
	}

	public static void GetAttackBonuses(CActor actor, CAbilityAttack attackAbility, out int strengthBuff, out int range, out int targetsCount, out List<CCondition.EPositiveCondition> positiveConditions, out List<CCondition.ENegativeCondition> negativeConditions)
	{
		strengthBuff = 0;
		GetAndApplyAttackBonuses(actor, attackAbility, ref strengthBuff, ref attackAbility.m_RangeBuff, ref attackAbility.m_TreatAsRangeBuff, ref attackAbility.m_TargetCountBuff, ref attackAbility.m_PositiveConditionsBuff, ref attackAbility.m_NegativeConditionsBuff);
		range = attackAbility.m_RangeBuff;
		targetsCount = attackAbility.m_TargetCountBuff;
		positiveConditions = attackAbility.m_PositiveConditionsBuff;
		negativeConditions = attackAbility.m_NegativeConditionsBuff;
	}

	public static void GetAndApplyAttackBonuses(CActor actor, CAbilityAttack attackAbility, ref int strength, ref int range, ref bool treatAsRange, ref int targetsCount, ref List<CCondition.EPositiveCondition> positiveConditions, ref List<CCondition.ENegativeCondition> negativeConditions)
	{
		IEnumerable<CAttackActiveBonus> enumerable = from x in CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.Attack)
			select x as CAttackActiveBonus;
		if (enumerable == null)
		{
			return;
		}
		foreach (CAttackActiveBonus item in enumerable.OrderBy((CAttackActiveBonus x) => GetBonusPriority(x)))
		{
			if (!item.TryGetBehaviour(out CAttackActiveBonus_BuffAttack behaviour))
			{
				continue;
			}
			strength += behaviour.ReferenceStrength(attackAbility, actor);
			treatAsRange |= behaviour.ReferenceTreatAsRange(attackAbility, actor);
			range += behaviour.ReferenceRange(attackAbility, actor);
			targetsCount += behaviour.ReferenceTargetsCount(attackAbility, actor);
			List<CCondition.EPositiveCondition> list = behaviour.ReferencePositiveConditions(attackAbility, actor);
			if (list != null)
			{
				if (positiveConditions == null)
				{
					positiveConditions = new List<CCondition.EPositiveCondition>();
				}
				positiveConditions.AddRange(list);
			}
			List<CCondition.ENegativeCondition> list2 = behaviour.ReferenceNegativeConditions(attackAbility, actor);
			if (list2 != null)
			{
				if (negativeConditions == null)
				{
					negativeConditions = new List<CCondition.ENegativeCondition>();
				}
				negativeConditions.AddRange(list2);
			}
		}
		static int GetBonusPriority(CAttackActiveBonus a)
		{
			CAbilityAttack cAbilityAttack = a.Ability as CAbilityAttack;
			if (cAbilityAttack.AttackEffects.Any((CAttackEffect x) => x.Range != 0 && x.Effect != CAttackEffect.EAttackEffect.BuffIfAttackIsRanged))
			{
				return 0;
			}
			if (cAbilityAttack.AttackEffects.Any((CAttackEffect x) => x.Effect == CAttackEffect.EAttackEffect.BuffIfAttackIsRanged))
			{
				return 1;
			}
			return 2;
		}
	}

	public static void GetAttackBonuses(CActor actor, out int strength, out int range, out int targetsCount, out List<CCondition.EPositiveCondition> positiveConditions, out List<CCondition.ENegativeCondition> negativeConditions)
	{
		CAbilityAttack cAbilityAttack = (CAbilityAttack)CreateDefaultAttack(2, 1, 1, actor.IsMonsterType, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
		cAbilityAttack.TargetingActor = actor;
		GetAttackBonuses(actor, cAbilityAttack, out strength, out range, out targetsCount, out positiveConditions, out negativeConditions);
	}

	private void PreProcessAttackActor(CActor actorBeingAttacked)
	{
		PreProcessTargetAbility(actorBeingAttacked);
	}

	private CTile DoorPath(CTile targetActorTile, CTile aiMoveFocusActorTile)
	{
		List<CTile> list = new List<CTile>();
		int num = Math.Max(1, Range);
		int num2 = ((targetActorTile.m_ArrayIndex.Y - num >= 0) ? (targetActorTile.m_ArrayIndex.Y - num) : 0);
		int num3 = ((targetActorTile.m_ArrayIndex.Y + num < ScenarioManager.Height) ? (targetActorTile.m_ArrayIndex.Y + num) : ScenarioManager.Height);
		int num4 = ((targetActorTile.m_ArrayIndex.X - num >= 0) ? (targetActorTile.m_ArrayIndex.X - num) : 0);
		int num5 = ((targetActorTile.m_ArrayIndex.X + num < ScenarioManager.Width) ? (targetActorTile.m_ArrayIndex.X + num) : ScenarioManager.Width);
		for (int i = num2; i <= num3; i++)
		{
			for (int j = num4; j <= num5; j++)
			{
				if (i < 0 || j < 0 || i >= ScenarioManager.Height || j >= ScenarioManager.Width)
				{
					continue;
				}
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile != null)
				{
					CObjectDoor cObjectDoor = (CObjectDoor)adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door);
					if (cObjectDoor != null && cObjectDoor.Activated)
					{
						list.Add(adjacentTile);
					}
				}
			}
		}
		CTile cTile = null;
		if (aiMoveFocusActorTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && targetActorTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && aiMoveFocusActorTile.m_HexMap != targetActorTile.m_HexMap)
		{
			bool flag = false;
			foreach (CTile item in list)
			{
				if ((aiMoveFocusActorTile.m_HexMap == item.m_HexMap && targetActorTile.m_HexMap == item.m_Hex2Map) || (aiMoveFocusActorTile.m_HexMap == item.m_Hex2Map && targetActorTile.m_HexMap == item.m_HexMap))
				{
					if (cTile == null)
					{
						cTile = item;
					}
					else
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				cTile = null;
			}
		}
		return cTile;
	}

	private bool IsTileWithinRange(CTile targetActorTile, CTile aiMoveFocusActorTile, int range = 1)
	{
		CTile cTile = DoorPath(targetActorTile, aiMoveFocusActorTile);
		List<Point> list = null;
		bool foundPath;
		if (cTile != null)
		{
			list = ScenarioManager.PathFinder.FindPath(null, targetActorTile.m_ArrayIndex, cTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, IgnoreDoorLocks);
			if (foundPath)
			{
				list.AddRange(ScenarioManager.PathFinder.FindPath(null, cTile.m_ArrayIndex, aiMoveFocusActorTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, IgnoreDoorLocks));
			}
		}
		else
		{
			list = ScenarioManager.PathFinder.FindPath(null, targetActorTile.m_ArrayIndex, aiMoveFocusActorTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, IgnoreDoorLocks);
		}
		return list.Count <= range;
	}

	public bool IsActorWithinRangeAndLoS(CActor aiMoveFocusActor, int range = 1)
	{
		if (CanTargetFromAnywhere(aiMoveFocusActor))
		{
			return true;
		}
		CTile cTile = ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y];
		if (aiMoveFocusActor is CObjectActor { AttachedProp: CObjectObstacle attachedProp })
		{
			foreach (TileIndex pathingBlocker in attachedProp.PathingBlockers)
			{
				CTile cTile2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
				if (CActor.HaveLOS(cTile, cTile2) && IsTileWithinRange(cTile, cTile2, range))
				{
					return true;
				}
			}
			return false;
		}
		if (aiMoveFocusActor.MultiHexArrayIndexes != null && aiMoveFocusActor.MultiHexArrayIndexes.Count > 0)
		{
			foreach (Point multiHexArrayIndex in aiMoveFocusActor.MultiHexArrayIndexes)
			{
				CTile cTile3 = ScenarioManager.Tiles[multiHexArrayIndex.X, multiHexArrayIndex.Y];
				if (CActor.HaveLOS(cTile, cTile3) && IsTileWithinRange(cTile, cTile3, range))
				{
					return true;
				}
			}
			return false;
		}
		if (TargetingActor.MultiHexArrayIndexes != null && TargetingActor.MultiHexArrayIndexes.Count > 0)
		{
			CTile cTile4 = ScenarioManager.Tiles[aiMoveFocusActor.ArrayIndex.X, aiMoveFocusActor.ArrayIndex.Y];
			foreach (Point multiHexArrayIndex2 in TargetingActor.MultiHexArrayIndexes)
			{
				CTile cTile5 = ScenarioManager.Tiles[multiHexArrayIndex2.X, multiHexArrayIndex2.Y];
				if (CActor.HaveLOS(cTile5, cTile4) && IsTileWithinRange(cTile5, cTile4, range))
				{
					return true;
				}
			}
			return false;
		}
		CTile cTile6 = ScenarioManager.Tiles[aiMoveFocusActor.ArrayIndex.X, aiMoveFocusActor.ArrayIndex.Y];
		if (!CActor.HaveLOS(cTile, cTile6))
		{
			return false;
		}
		if (!IsTileWithinRange(cTile, cTile6, range))
		{
			return false;
		}
		return true;
	}

	private static bool AttackActor(CAbilityAttack ability, bool isMeleeAttack, CActor targetingActor, CActor filterActor, CAttackSummary.TargetSummary attackingTargetSummary, int modifiedStrength, List<CActor> actorsToIgnore, CAreaEffect areaEffect, AbilityData.XpPerTargetData xpPerTargetData, int attackIndex)
	{
		CActor actorToAttack = attackingTargetSummary.ActorToAttack;
		if (targetingActor.Type == CActor.EType.Player)
		{
			foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
			{
				GameState.ShowItemSelection(allPlayer, actorToAttack, attackingTargetSummary.AttackAbilityWithOverrides ?? ability, CItem.EItemTrigger.OnAttackTargetOneByOne);
			}
		}
		int num = attackingTargetSummary.ActiveBonusStrengthBuffs.Sum();
		ability.ActorsTargeted.Add(actorToAttack);
		targetingActor.ActorsAttackedThisRound.Add(actorToAttack);
		int num2 = attackingTargetSummary.AttackAbilityWithOverrides.AbilityXP - ability.AbilityXP;
		if (num2 > 0)
		{
			targetingActor.GainXP(num2, ability.XPSourceKey);
		}
		if (attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToTakeFromTargets != null && attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToTakeFromTargets.Count > 0)
		{
			foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToTakeFromTargets)
			{
				if (actorToAttack.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
				{
					actorToAttack.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
					targetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
				}
			}
		}
		if (attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToGiveToTargets != null && attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToGiveToTargets.Count > 0)
		{
			foreach (KeyValuePair<string, int> resourcesToGiveToTarget in attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToGiveToTargets)
			{
				if (targetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
				{
					targetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					actorToAttack.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
				}
			}
		}
		if (s_AttackValueOverride != int.MaxValue)
		{
			modifiedStrength = s_AttackValueOverride;
			s_AttackValueOverride = int.MaxValue;
		}
		ability.AbilityHasHappened = true;
		if (num > 0)
		{
			CTargetAttackBuff_MessageData cTargetAttackBuff_MessageData = new CTargetAttackBuff_MessageData(targetingActor);
			cTargetAttackBuff_MessageData.m_ActorBeingAttacked = actorToAttack;
			cTargetAttackBuff_MessageData.m_ActorAttacking = targetingActor;
			ScenarioRuleClient.MessageHandler(cTargetAttackBuff_MessageData);
		}
		if (actorToAttack.CalculateShield(ability) > 0)
		{
			CTargetShield_MessageData cTargetShield_MessageData = new CTargetShield_MessageData(targetingActor);
			cTargetShield_MessageData.m_ActorBeingAttacked = actorToAttack;
			cTargetShield_MessageData.m_ActorAttacking = targetingActor;
			cTargetShield_MessageData.m_PlaySound = modifiedStrength <= 0;
			ScenarioRuleClient.MessageHandler(cTargetShield_MessageData);
		}
		int health = actorToAttack.Health;
		actorToAttack.m_OnBeingAttackedListeners?.Invoke(ability, modifiedStrength);
		targetingActor.m_OnAttackSingleTargetListeners?.Invoke(ability, actorToAttack);
		bool actorWasAsleep = actorToAttack.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		EAbilityType damageAbilityType = ability?.AbilityType ?? EAbilityType.None;
		bool triggeredPreventDamage = false;
		int health2 = actorToAttack.Health;
		GameState.ActorBeenDamaged(actorToAttack, modifiedStrength, out triggeredPreventDamage, out var _, checkIfPlayerCanAvoidDamage: true, targetingActor, ability, damageAbilityType, actorToAttack.CalculateShield(ability));
		if (attackingTargetSummary.ShadowMarked)
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.IsShadowMarked = false;
				CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(allAliveActor);
				ScenarioRuleClient.MessageHandler(message);
			}
			actorToAttack.IsShadowMarked = true;
		}
		if (attackingTargetSummary.AttackAbilityWithOverrides is CAbilityAttack attackAbility && GameState.TotalShieldDuringLastAttack <= 0 && attackingTargetSummary.ActualDamageTaken > 0)
		{
			CChallenge.CheckTrigger(CChallenge.Trigger.OnSufferDamageFromAttackWithNoShield, new CChallenge.TriggerParams
			{
				attackAbility = attackAbility,
				actorBeingAttacked = attackingTargetSummary.ActorToAttack
			});
		}
		if (targetingActor.m_OnAttackFinishedListeners != null)
		{
			targetingActor.m_OnAttackFinishedListeners?.Invoke(ability, actorToAttack, modifiedStrength);
		}
		CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericAttackFinished, ability, null, actorToAttack, ("DamageDealt", modifiedStrength));
		if (xpPerTargetData != null)
		{
			targetingActor.GainXP(xpPerTargetData.AttackTarget(targetingActor, actorToAttack), ability.XPSourceKey);
		}
		if (attackingTargetSummary != null)
		{
			foreach (int conditionalOverridesXPBuff in attackingTargetSummary.ConditionalOverridesXPBuffs)
			{
				targetingActor.GainXP(conditionalOverridesXPBuff, ability.XPSourceKey);
			}
			targetingActor.GainXP(attackingTargetSummary.AttackEffectXP, ability.XPSourceKey);
			targetingActor.GainXP(attackingTargetSummary.ActiveBonusXPBuffs.Sum(), ability.XPSourceKey);
			if (ability.MiscAbilityData != null && ability.MiscAbilityData.GainXPPerXDamageDealt.HasValue && ability.MiscAbilityData.GainXPPerXDamageDealt.Value > 0)
			{
				targetingActor.GainXP(attackingTargetSummary.FinalAttackStrength / ability.MiscAbilityData.GainXPPerXDamageDealt.Value, ability.XPSourceKey);
			}
		}
		if (ScenarioManager.Scenario.HasActor(actorToAttack) && actorToAttack.Health != health2)
		{
			if (GameState.ActorHealthCheck(targetingActor, actorToAttack, out var onDeathAbility, isTrap: false, isTerrain: false, actorWasAsleep, attackingTargetSummary))
			{
				if (actorToAttack.OriginalType != CActor.EType.Player)
				{
					CActorBeenAttacked_MessageData message2 = new CActorBeenAttacked_MessageData(targetingActor)
					{
						m_AttackingActor = targetingActor,
						m_ActorBeingAttacked = actorToAttack,
						AreaEffect = areaEffect,
						m_ActorOriginalHealth = health,
						m_AttackAbility = ability,
						m_AttackIndex = attackIndex,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				actorsToIgnore?.Add(actorToAttack);
				if (targetingActor.Type == CActor.EType.Player)
				{
					CVars cVars = new CVars();
					cVars.Set("ActorBeingAttacked", new TValue(actorToAttack));
					cVars.Set("ActorAttacking", new TValue(targetingActor));
					foreach (CPlayerActor allPlayer2 in ScenarioManager.Scenario.AllPlayers)
					{
						GameState.ShowItemSelection(allPlayer2, actorToAttack, attackingTargetSummary.AttackAbilityWithOverrides ?? ability, CItem.EItemTrigger.AfterAttackOneByOne, onlyInActionPhase: true, killAfter: false, cVars);
					}
				}
			}
			else if (actorToAttack.OriginalType != CActor.EType.Player)
			{
				CActorBeenAttackedAndKilled_MessageData message3 = new CActorBeenAttackedAndKilled_MessageData(targetingActor)
				{
					m_AttackingActor = targetingActor,
					m_ActorBeingAttacked = actorToAttack,
					AreaEffect = areaEffect,
					m_ActorOriginalHealth = health,
					m_AttackAbility = ability,
					m_AttackIndex = attackIndex
				};
				ScenarioRuleClient.MessageHandler(message3);
			}
			return onDeathAbility;
		}
		if (actorToAttack is CHeroSummonActor actor && !ScenarioManager.Scenario.HasActor(actor) && !triggeredPreventDamage)
		{
			CActorBeenAttacked_MessageData message4 = new CActorBeenAttacked_MessageData(targetingActor)
			{
				m_AttackingActor = targetingActor,
				m_ActorBeingAttacked = actorToAttack,
				AreaEffect = areaEffect,
				m_ActorOriginalHealth = health,
				m_AttackAbility = ability,
				m_AttackIndex = attackIndex,
				m_ActorWasAsleep = actorWasAsleep,
				m_LogDamageCalculationOnly = true
			};
			ScenarioRuleClient.MessageHandler(message4);
		}
		return false;
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (AbilityHasBeenCancelled)
		{
			if (m_IsMergedAbility)
			{
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		if (!(TargetingActor is CEnemyActor { IsImmuneToDamage: not false }) && CAbility.ImmuneToAbility(TargetingActor, this))
		{
			CPlayerIsDisarmed_MessageData message = new CPlayerIsDisarmed_MessageData(TargetingActor);
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		CActiveBonus bonus;
		if (!ProcessIfDead)
		{
			if ((!TargetingActor.ForcedCannotAttack(out bonus) || ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA) && (!StoppedByDisarm() || AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true)))
			{
				if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
				{
					AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
					if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
					{
						goto IL_0116;
					}
				}
				if (!TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
				{
					goto IL_021f;
				}
			}
			goto IL_0116;
		}
		goto IL_021f;
		IL_0116:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (bonus != null)
			{
				if (!string.IsNullOrEmpty(bonus.Ability?.ActiveBonusData?.ListLayoutOverride.Item1))
				{
					SetCancelReason(bonus.Ability.ActiveBonusData.ListLayoutOverride.Item1);
				}
				SetCancelAbilityFlag(cancel: true);
				ShowCancelledHelpText();
			}
			else if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				CPlayerIsStunned_MessageData message2 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsDisarmed_MessageData message4 = new CPlayerIsDisarmed_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
				if (IsMergedAbility)
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.StepComplete();
				}
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
		IL_021f:
		bool flag4;
		switch (m_State)
		{
		case EAttackState.SelectAttackFocus:
		{
			TryAutoSelectTiles();
			if (m_AllTargets || m_ActorsToTarget.Count > 0)
			{
				TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
			}
			else
			{
				TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			}
			if (UseSubAbilityTargeting)
			{
				if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
				{
					m_State = EAttackState.AttackBuff;
					Perform();
					break;
				}
				if (AreaEffect == null)
				{
					AreaEffect = new CAreaEffect("SubAbilityAttackArea", IsMeleeAttack, new List<CAreaEffect.CAreaEffectHex>());
					m_ValidTilesInAreaEffect = ValidActorsInRange.Select((CActor s) => ScenarioManager.GetAdjacentTile(s.ArrayIndex.X, s.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter)).ToList();
				}
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
				}
				m_CanUndo = false;
				if (TargetingActor.Type.Equals(CActor.EType.Player))
				{
					CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData2 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
					cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackingActor = TargetingActor;
					cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackAbility = this;
					cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackSummary = AttackSummary.Copy();
					ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData2);
				}
				else
				{
					m_State = EAttackState.CheckForAIToggleBonuses;
					Perform();
				}
				break;
			}
			if (m_IsMergedAbility)
			{
				if (m_AllTargets)
				{
					m_ActorsToTarget = ValidActorsInRange.ToList();
				}
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
				}
				if (TargetingActor.Type.Equals(CActor.EType.Player))
				{
					CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData3 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
					cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackingActor = TargetingActor;
					cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackAbility = this;
					cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackSummary = AttackSummary.Copy();
					ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData3);
				}
				break;
			}
			AbilityData.MiscAbilityData miscAbilityData6 = MiscAbilityData;
			if (miscAbilityData6 != null && miscAbilityData6.AutotriggerAbility == true && ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				m_State = EAttackState.AttackBuff;
				if (!IsMirrorAbility)
				{
					m_ActorsToTarget.AddRange(ValidActorsInRange);
				}
				Perform();
				break;
			}
			if (TargetingActor.Type != CActor.EType.Player || AllTargetsOnMovePath)
			{
				if (TargetingActor.Type != CActor.EType.Player && !AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
				{
					if (CachedAIMoveFocusActors == null || CachedAIMoveFocusActors.Count == 0)
					{
						TargetingActor.Move(0, jump: false, fly: false, Range, allowMove: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, this, firstMove: true, moveTest: false, carryOtherActors: false);
					}
					if ((TargetingActor.Type != CActor.EType.HeroSummon || ActiveBonusData == null || !ActiveBonusData.IsAura) && AreaEffect != null && !IsSubAbility)
					{
						UpdateAreaEffect(ScenarioManager.Tiles[m_AreaRangeTileX, m_AreaRangeTileY], AreaEffectAngle, AreaEffectFlipped);
						List<CActor> actorsInRange = GameState.GetActorsInRange(TargetingActor, FilterActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffectIncludingBlocked, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
						if (!TargetingActor.CanTargetFromAnywhere(Range, actorsInRange.Count > 0))
						{
							m_ValidActorsInRange = actorsInRange;
						}
						else
						{
							List<CTile> list4 = new List<CTile>();
							foreach (CActor item4 in m_ValidActorsInRange)
							{
								if (!item4.CanBeTargetedFromAnywhere())
								{
									continue;
								}
								foreach (CTile item5 in GameState.GetTilesInRange(ScenarioManager.GetTile(item4.ArrayIndex).m_ArrayIndex, Range))
								{
									if (!list4.Contains(item5))
									{
										list4.Add(item5);
									}
								}
							}
							int num = -1;
							List<CActor> list5 = null;
							List<CTile> validTilesIncludingBlocked = new List<CTile>();
							Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = new Dictionary<CTile, CAreaEffect.EHexFlag>();
							foreach (CTile item6 in list4)
							{
								for (int num2 = 0; num2 < 360; num2 += 60)
								{
									CAreaEffect.GetValidTiles(item6, item6, AreaEffect, num2, AreaEffectFlipped, AreaEffectFlipAxis, GameState.GetTilesInRange(item6.m_ArrayIndex, Range).ToList(), ref validTilesIncludingBlocked, ref flagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks, Range);
									List<CActor> actorsInRange2 = GameState.GetActorsInRange(item6.m_ArrayIndex, FilterActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, validTilesIncludingBlocked, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
									actorsInRange2.RemoveAll((CActor a) => !a.CanBeTargetedFromAnywhere());
									if (num < actorsInRange2.Count)
									{
										num = actorsInRange2.Count;
										list5 = actorsInRange2.ToList();
									}
								}
							}
							if (list5 != null)
							{
								m_ValidActorsInRange = list5;
							}
							else
							{
								m_ValidActorsInRange.Clear();
							}
						}
					}
					if (CachedAIMoveFocusActors != null)
					{
						CachedAIMoveFocusActors.RemoveAll((CActor x) => x == null || x.IsDead || x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
					}
					ValidActorsInRange.RemoveAll((CActor x) => x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
					if (ValidActorsInRange.Any((CActor x) => !x.CanBeTargetedFromAnywhere()))
					{
						ValidActorsInRange.RemoveAll((CActor x) => x is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.PropHealthDetails != null && cObjectActor.AttachedProp.PropHealthDetails.IgnoredByAIFocus);
						if (TargetingActor.Type == CActor.EType.HeroSummon || (TargetingActor.Type == CActor.EType.Ally && TargetingActor is CEnemyActor { PlayerSummoner: not null }))
						{
							ValidActorsInRange.RemoveAll((CActor x) => x is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.PropHealthDetails != null && cObjectActor.AttachedProp.PropHealthDetails.IgnoredByHeroSummonFocus);
						}
					}
					if (CachedAIMoveFocusActors != null)
					{
						CachedAIMoveFocusActors.RemoveAll((CActor x) => !ValidActorsInRange.Contains(x));
					}
					if (ChainAttack)
					{
						if (CachedAIMoveFocusActors == null || CachedAIMoveFocusActors.Count <= 0)
						{
							CInvalidAttack_MessageData cInvalidAttack_MessageData = new CInvalidAttack_MessageData(TargetingActor);
							cInvalidAttack_MessageData.m_AttackingActor = TargetingActor;
							cInvalidAttack_MessageData.m_Ability = this;
							ScenarioRuleClient.MessageHandler(cInvalidAttack_MessageData);
							CachedAIMoveFocusActors?.Clear();
							PhaseManager.NextStep();
							return true;
						}
						CActor cActor4 = TargetingActor;
						int num3 = Range;
						CTile sourceTile = ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y];
						for (int num4 = 0; num4 < m_TotalNumberTargets; num4++)
						{
							bool flag3 = false;
							foreach (CActor cachedAIMoveFocusActor in CachedAIMoveFocusActors)
							{
								if (cachedAIMoveFocusActor != null)
								{
									CTile targetTile = ScenarioManager.Tiles[cachedAIMoveFocusActor.ArrayIndex.X, cachedAIMoveFocusActor.ArrayIndex.Y];
									if (CActor.HaveLOS(sourceTile, targetTile) && ValidActorsInRange.Contains(cachedAIMoveFocusActor) && !m_ActorsToIgnore.Contains(cachedAIMoveFocusActor) && !m_ActorsToTarget.Contains(cachedAIMoveFocusActor))
									{
										m_ActorsToTarget.Add(cachedAIMoveFocusActor);
										cActor4 = cachedAIMoveFocusActor;
										flag3 = true;
										break;
									}
								}
							}
							if (!flag3 || num4 + 1 >= m_TotalNumberTargets)
							{
								break;
							}
							num3 += ChainAttackRange;
							CachedAIMoveFocusActors.Clear();
							TargetingActor.Move(0, jump: false, fly: false, num3, allowMove: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, this, firstMove: true, moveTest: false, carryOtherActors: false);
							ValidActorsInRange = GameState.GetActorsInRange(cActor4.ArrayIndex, FilterActor, ChainAttackRange, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
						}
					}
					else
					{
						CActor cActor5 = ((CachedAIMoveFocusActors == null || CachedAIMoveFocusActors.Count == 0) ? null : CachedAIMoveFocusActors[0]);
						CActor cActor6;
						if (cActor5 != null && !m_ActorsToIgnore.Contains(cActor5) && IsActorWithinRangeAndLoS(cActor5, Range))
						{
							cActor6 = cActor5;
						}
						else
						{
							cActor6 = ((ValidActorsInRange.Count == 0) ? null : ValidActorsInRange[0]);
						}
						if (!IsMirrorAbility)
						{
							if (!ValidActorsInRange.Contains(cActor6) || cActor6.IsDead)
							{
								CInvalidAttack_MessageData cInvalidAttack_MessageData2 = new CInvalidAttack_MessageData(TargetingActor);
								cInvalidAttack_MessageData2.m_AttackingActor = TargetingActor;
								cInvalidAttack_MessageData2.m_Ability = this;
								ScenarioRuleClient.MessageHandler(cInvalidAttack_MessageData2);
								CachedAIMoveFocusActors?.Clear();
								PhaseManager.NextStep();
								return true;
							}
							if (m_AllTargets || AreaEffect != null)
							{
								m_ActorsToTarget = ValidActorsInRange.ToList();
							}
							else
							{
								m_ActorsToTarget.Clear();
								if (CachedAIMoveFocusActors == null || CachedAIMoveFocusActors.Count == 0)
								{
									m_ActorsToTarget.Add(cActor6);
									Log.SimpleInfo("No AIMoveFocusActors - selecting the first actor in ValidActorsInRange list to attack instead");
								}
								else
								{
									foreach (CActor cachedAIMoveFocusActor2 in CachedAIMoveFocusActors)
									{
										if (cachedAIMoveFocusActor2 == null)
										{
											continue;
										}
										if ((!m_ActorsToIgnore.Contains(cachedAIMoveFocusActor2) || m_TargetedActor == cachedAIMoveFocusActor2) && IsActorWithinRangeAndLoS(cachedAIMoveFocusActor2, Range))
										{
											AbilityData.MiscAbilityData miscAbilityData7 = MiscAbilityData;
											if (miscAbilityData7 != null && miscAbilityData7.TargetOneEnemyWithAllAttacks.HasValue)
											{
												AbilityData.MiscAbilityData miscAbilityData8 = MiscAbilityData;
												if (miscAbilityData8 != null && miscAbilityData8.TargetOneEnemyWithAllAttacks.Value)
												{
													if (m_TargetedActor == null || (m_TargetedActor == cachedAIMoveFocusActor2 && ValidActorsInRange.Contains(m_TargetedActor)))
													{
														m_ActorsToTarget.Clear();
														m_ActorsToTarget.Add(cachedAIMoveFocusActor2);
														m_TargetedActor = cachedAIMoveFocusActor2;
														break;
													}
													PhaseManager.NextStep();
													return true;
												}
											}
											m_ActorsToTarget.Add(cachedAIMoveFocusActor2);
										}
										if (m_ActorsToTarget.Count == m_NumberTargetsRemaining)
										{
											break;
										}
									}
								}
							}
							if (TargetingActor.CanTargetFromAnywhere(Range, m_ActorsToTarget.Count > 0) && m_ActorsToTarget.Count < m_NumberTargetsRemaining)
							{
								AbilityData.MiscAbilityData miscAbilityData9 = MiscAbilityData;
								if (miscAbilityData9 != null && !miscAbilityData9.TargetOneEnemyWithAllAttacks.HasValue)
								{
									AbilityData.MiscAbilityData miscAbilityData10 = MiscAbilityData;
									if (miscAbilityData10 != null && !miscAbilityData10.TargetOneEnemyWithAllAttacks.Value)
									{
										foreach (CActor item7 in m_ValidActorsInRange)
										{
											if (CanTargetFromAnywhere(item7) && !m_ActorsToTarget.Contains(item7))
											{
												m_ActorsToTarget.Add(item7);
												if (m_ActorsToTarget.Count == m_NumberTargetsRemaining)
												{
													break;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					m_ActorsToTarget = ValidActorsInRange.ToList();
				}
				if (m_ActorsToTarget.Count <= 0)
				{
					SetCancelAbilityFlag(cancel: true);
				}
				m_TilesSelected.Clear();
				foreach (CActor item8 in m_ActorsToTarget)
				{
					m_TilesSelected.Add(ScenarioManager.Tiles[item8.ArrayIndex.X, item8.ArrayIndex.Y]);
				}
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
				}
				CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData4 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
				cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackingActor = TargetingActor;
				cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackAbility = this;
				cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackSummary = AttackSummary.Copy();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData4);
				break;
			}
			if (m_AttackSummary != null && !ChainAttack)
			{
				CAbilityFilter.CurrentVarsContext = VarsContext;
				m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true, useBonusTracker: true);
				CAbilityFilter.CurrentVarsContext = null;
			}
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || m_AllTargets)
			{
				m_AllTargets = true;
				m_ActorsToTarget = ValidActorsInRange.ToList();
				m_TilesSelected.Clear();
				foreach (CActor item9 in m_ActorsToTarget)
				{
					m_TilesSelected.Add(ScenarioManager.Tiles[item9.ArrayIndex.X, item9.ArrayIndex.Y]);
				}
				CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(TargetingActor);
				cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
				cShowSingleTargetActiveBonus_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
			}
			else if (ChainAttack)
			{
				if (m_ActorsToTarget.Count > 0)
				{
					foreach (CActor item10 in m_ActorsToTarget)
					{
						if (!m_ActorsToIgnore.Contains(item10))
						{
							m_ActorsToIgnore.Add(item10);
						}
					}
					List<CActor> actorsInRange3 = GameState.GetActorsInRange(m_ActorsToTarget.Last().ArrayIndex, TargetingActor, m_ChainAttackRange, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
					m_ValidActorsInRange.Clear();
					foreach (CActor item11 in actorsInRange3)
					{
						if (!m_ValidActorsInRange.Contains(item11))
						{
							m_ValidActorsInRange.Add(item11);
						}
					}
					TilesInRange = GameState.GetTilesInRange(m_ActorsToTarget.Last(), m_ChainAttackRange, Targeting, emptyTilesOnly: false, !IsMeleeAttack);
					foreach (CActor item12 in m_ActorsToTarget)
					{
						CTile item2 = ScenarioManager.Tiles[item12.ArrayIndex.X, item12.ArrayIndex.Y];
						TilesInRange.Add(item2);
						m_ActorsToIgnore.Remove(item12);
					}
					ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
					if (m_AttackSummary != null)
					{
						m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
					}
				}
				else
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
					if (m_AttackSummary != null)
					{
						m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
					}
				}
			}
			CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData5 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
			cActorIsSelectingAttackFocusTargets_MessageData5.m_AttackingActor = TargetingActor;
			cActorIsSelectingAttackFocusTargets_MessageData5.m_AttackAbility = this;
			cActorIsSelectingAttackFocusTargets_MessageData5.m_AttackSummary = AttackSummary.Copy();
			ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData5);
			break;
		}
		case EAttackState.CheckForAIToggleBonuses:
		{
			if (TargetingActor.Type == CActor.EType.Player)
			{
				m_State = EAttackState.PreActorIsAttacking;
				Perform();
				break;
			}
			List<CActiveBonus> list7 = CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Attack);
			List<CActiveBonus> list8 = new List<CActiveBonus>();
			if (list7 != null)
			{
				foreach (CActiveBonus item13 in list7)
				{
					if ((TargetingActor.Type == CActor.EType.HeroSummon || !item13.ToggledBonus) && item13.Ability.ActiveBonusData.CostAbility == null && item13.Ability.ActiveBonusData.IsToggleBonus && item13.CanToggleActiveBonus(TargetingActor))
					{
						list8.Add(item13);
					}
				}
			}
			if (list8.Count > 0)
			{
				if (TargetingActor.Type == CActor.EType.HeroSummon)
				{
					if (m_AttackSummary != null)
					{
						m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
						m_AttackSummary.AutoToggleSingleTargetBonuses();
					}
					CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData2 = new CShowSingleTargetActiveBonus_MessageData(TargetingActor);
					cShowSingleTargetActiveBonus_MessageData2.m_ShowSingleTargetActiveBonus = true;
					cShowSingleTargetActiveBonus_MessageData2.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData2);
					ShowToggleBonusesForAI();
					break;
				}
				EAttackState state = m_State;
				foreach (CActiveBonus item14 in list7)
				{
					item14.ToggleActiveBonus(null, TargetingActor);
				}
				if (m_State == state)
				{
					m_State = EAttackState.SelectAttackFocus;
					Perform();
				}
			}
			else
			{
				m_State = EAttackState.PreActorIsAttacking;
				Perform();
			}
			break;
		}
		case EAttackState.PreActorIsAttacking:
			if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				m_State = EAttackState.AttackBuff;
				Perform();
				break;
			}
			ExcludeMirroredHealthClones(ActorsToTarget);
			if (AreaEffect == null)
			{
				if (TargetingActor.IsMonsterType && m_MultiPassAttack && (m_NumberTargetsRemaining > 1 || m_AllTargets) && ValidActorsInRange.Count > 1)
				{
					if (!m_AllTargets)
					{
						while (m_ActorsToTarget.Count > m_NumberTargetsRemaining)
						{
							m_ActorsToTarget.Remove(m_ActorsToTarget[ValidActorsInRange.Count - 1]);
						}
					}
				}
				else
				{
					if (ChainAttack)
					{
						goto IL_162a;
					}
					AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.TargetOneEnemyWithAllAttacks.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
						if (miscAbilityData3 != null && miscAbilityData3.TargetOneEnemyWithAllAttacks.Value)
						{
							goto IL_162a;
						}
					}
				}
			}
			goto IL_164d;
		case EAttackState.SelectActiveBonusBuffTarget:
			if (TargetingActor.Type.Equals(CActor.EType.Player))
			{
				CActorIsSelectingActiveBonusBuffTarget_MessageData cActorIsSelectingActiveBonusBuffTarget_MessageData = new CActorIsSelectingActiveBonusBuffTarget_MessageData(TargetingActor);
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_AttackingActor = TargetingActor;
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_AttackAbility = this;
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_ActiveBonus = m_CurrentStackedInlineAbilityActiveBonus;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingActiveBonusBuffTarget_MessageData);
			}
			break;
		case EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility:
			if (TargetingActor.Type.Equals(CActor.EType.Player))
			{
				m_CanUndo = false;
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
				}
				CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(TargetingActor);
				cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = TargetingActor;
				cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = this;
				cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = m_AttackSummary?.Copy();
				ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
			}
			m_State = EAttackState.CheckForDamageSelf;
			Perform();
			break;
		case EAttackState.CheckForDamageSelf:
			if (m_FirstAttackofAction && DamageSelfBeforeAttack > 0)
			{
				int health = TargetingActor.Health;
				bool actorWasAsleep = TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(TargetingActor, DamageSelfBeforeAttack, checkIfPlayerCanAvoidDamage: true, TargetingActor, this, AbilityType);
				if (!GameState.ActorHealthCheck(TargetingActor, TargetingActor, out var _, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					SetCancelAbilityFlag(cancel: true);
					if (IsMergedAbility)
					{
						PhaseManager.StepComplete();
					}
					else
					{
						PhaseManager.NextStep();
					}
					break;
				}
				CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(TargetingActor);
				cActorBeenDamaged_MessageData.m_ActorBeingDamaged = TargetingActor;
				cActorBeenDamaged_MessageData.m_DamageAbility = null;
				cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
				cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
				ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
			}
			PhaseManager.StepComplete();
			break;
		case EAttackState.DrawingModifiers:
			DontMoveAbilityState = false;
			if (AbilityHasHappened && PendingExtraTargetsToSelect > 0 && (MultiPassActors == null || MultiPassActors.Count == 0))
			{
				StartAddExtraTargets();
			}
			else
			{
				StartAttackingActors();
			}
			break;
		case EAttackState.ActorHasAttacked:
		{
			s_LastAttacker = TargetingActor;
			bool flag5 = false;
			for (int num7 = 0; num7 < m_AttackedActors.Count; num7++)
			{
				PreProcessAttackActor(m_AttackedActors[num7]);
				if (flag5 || !IsTargetOutsideOfInitialRange(m_AttackedActors[num7]))
				{
					continue;
				}
				foreach (CActiveBonus addRangeActiveBonuse in m_AttackSummary.AddRangeActiveBonuses)
				{
					string text = addRangeActiveBonuse.ReferenceAnimOverload(this, m_AttackedActors[num7]);
					if (!string.IsNullOrEmpty(text))
					{
						AnimOverload = text;
						flag5 = true;
					}
				}
			}
			CActorHasAttacked_MessageData message9 = new CActorHasAttacked_MessageData(AnimOverload, TargetingActor)
			{
				m_AttackAbility = this,
				m_AttackingActor = TargetingActor,
				m_AttackedActors = m_AttackedActors,
				m_ActiveBonusesFlags = GetBonusesFlags()
			};
			ScenarioRuleClient.MessageHandler(message9);
			break;
		}
		case EAttackState.ActorBeenAttacked:
		{
			ScenarioManager.CurrentScenarioState.OnPerformingAbility(this);
			m_FirstAttackofAction = false;
			int targetIndex3 = 0;
			bool flag = false;
			if (ElementalConditions != null && ElementalConditions.Count > 0)
			{
				foreach (ElementInfusionBoardManager.EElement item15 in ElementsToConsume)
				{
					if (ElementInfusionBoardManager.ElementColumn(item15) != ElementInfusionBoardManager.EColumn.Inert)
					{
						flag = true;
						ElementInfusionBoardManager.Consume(item15, TargetingActor);
					}
				}
			}
			int count;
			for (count = 0; count < m_AttackedActors.Count; count++)
			{
				if (AbilityHasBeenCancelled)
				{
					if (m_IsMergedAbility)
					{
						PhaseManager.StepComplete();
					}
					else
					{
						PhaseManager.NextStep();
					}
					return false;
				}
				if (count < m_CurrentPassAttackIndex)
				{
					continue;
				}
				m_CurrentAttackIndex++;
				m_CurrentPassAttackIndex++;
				(CActor, CActor) tuple = RedirectedActors.FirstOrDefault(((CActor, CActor) pair) => pair.Item1 == m_AttackedActors[count]);
				var (cActor, cActor2) = tuple;
				if (cActor != null || cActor2 != null)
				{
					CRedirectActiveBonus cRedirectActiveBonus = null;
					foreach (CRedirectActiveBonus item16 in from x in tuple.Item1.FindApplicableActiveBonuses(EAbilityType.Redirect)
						select x as CRedirectActiveBonus)
					{
						if (item16.ApplyRedirect(this, tuple.Item2) == tuple.Item1)
						{
							cRedirectActiveBonus = item16;
							break;
						}
					}
					if (cRedirectActiveBonus != null)
					{
						CRedirectedAttack_MessageData message6 = new CRedirectedAttack_MessageData(TargetingActor)
						{
							m_Target = tuple.Item1,
							m_OriginalTarget = tuple.Item2,
							m_ProjectileName = cRedirectActiveBonus.Ability.GetMiscVar("ProjectileName").GetString()
						};
						ScenarioRuleClient.MessageHandler(message6);
					}
					RedirectedActors.Remove(tuple);
				}
				if (!ScenarioManager.Scenario.HasActor(TargetingActor) && !ProcessIfDead)
				{
					continue;
				}
				CActor.EType originalType = m_AttackedActors[count].OriginalType;
				List<CActiveBonus> list2 = m_AttackedActors[count].FindPreventDamageBonuses();
				bool flag2 = list2 != null && list2.Count > 0;
				if (originalType == CActor.EType.Player || (flag2 && originalType == CActor.EType.HeroSummon))
				{
					bool actorWasAsleep2 = m_AttackedActors[count].Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					CActorBeenAttacked_MessageData message7 = new CActorBeenAttacked_MessageData(TargetingActor)
					{
						m_AttackingActor = TargetingActor,
						m_ActorBeingAttacked = m_AttackedActors[count],
						AreaEffect = AreaEffect,
						m_ActorOriginalHealth = m_AttackedActors[count].Health,
						m_AttackAbility = this,
						m_AttackIndex = m_CurrentAttackIndex - 1,
						m_ActorWasAsleep = actorWasAsleep2
					};
					ScenarioRuleClient.MessageHandler(message7);
				}
				CurrentAttackingTargetSummary = m_AttackSummary.FindTarget(m_AttackedActors[count], ref targetIndex3);
				m_TotalTargetSummaries.Add(CurrentAttackingTargetSummary);
				if (flag)
				{
					foreach (CCondition.ENegativeCondition condition in ElementalConditions)
					{
						EAbilityType abilityType = CAbility.AbilityTypes.Single((EAbilityType x) => x.ToString() == condition.ToString());
						CurrentAttackingTargetSummary.AttackAbilityWithOverrides.NegativeConditions.Add(condition, CAbility.CreateAbility(abilityType, CurrentAttackingTargetSummary.AttackAbilityWithOverrides.AbilityFilter, CurrentAttackingTargetSummary.AttackAbilityWithOverrides.IsMonsterAbility, CurrentAttackingTargetSummary.AttackAbilityWithOverrides.IsTargetedAbility));
					}
				}
				if (AttackActor(this, IsMeleeAttack, TargetingActor, FilterActor, CurrentAttackingTargetSummary, m_ActorsToTargetModifiedStrength[count], m_ActorsToIgnore, AreaEffect, XpPerTargetData, m_CurrentAttackIndex - 1))
				{
					DontMoveAbilityState = true;
					PhaseManager.StepComplete();
					return true;
				}
			}
			DontMoveAbilityState = false;
			if (AbilityHasBeenCancelled && AbilityInterruptCopy != null)
			{
				(AbilityInterruptCopy as CAbilityAttack).State = EAttackState.ActorBeenAttacked;
				PhaseManager.StepComplete();
				return false;
			}
			m_ProcessedStatusEffects = false;
			if (m_MultiPassAttack)
			{
				targetIndex3 = 0;
				CAttackSummary.TargetSummary targetSummary3 = m_AttackSummary.FindTarget(m_AttackedActors[0], ref targetIndex3);
				if (targetSummary3 != null && !targetSummary3.ProcessedStatusEffects)
				{
					targetSummary3.ProcessedStatusEffects = true;
					CAbilityAttack obj = targetSummary3.AttackAbilityWithOverrides as CAbilityAttack;
					CActor cActor3 = m_AttackedActors[0];
					if (ScenarioManager.Scenario.HasActor(cActor3) && !cActor3.IsDowned && !cActor3.IsDead && targetSummary3.AttackAbilityWithOverrides.NegativeConditions.Count > 0)
					{
						targetSummary3.AttackAbilityWithOverrides.ProcessNegativeStatusEffects(cActor3);
					}
					ApplyPositiveEffects(targetSummary3);
					GameState.ShowToggleActiveBonusSelection(targetSummary3.ActorToAttack as CPlayerActor, targetSummary3.ActorToAttack, this, EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttacked);
					if (targetSummary3.Actor.m_OnBeingAttackedPostDamageListeners != null)
					{
						targetSummary3.Actor.m_OnBeingAttackedPostDamageListeners?.Invoke(this, targetSummary3);
					}
					CheckModifierAbilities(targetSummary3);
					if (obj.StackInlineSubAbilities(m_AttackedActors[0], null, this, ignorePerformNow: false, targetSummary3))
					{
						m_ProcessedStatusEffects = true;
						m_State = EAttackState.CheckForPlayerIdle;
						return true;
					}
				}
				m_ProcessedStatusEffects = true;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData3 = new CPlayerWaitForIdle_MessageData(TargetingActor);
					cPlayerWaitForIdle_MessageData3.m_Actor = TargetingActor;
					ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData3);
					return true;
				}
				m_State = EAttackState.CheckForPlayerIdle;
				Perform();
				break;
			}
			CheckModifierAbilities(null, nonMultiPassAttack: true);
			List<CAbility> list3 = new List<CAbility>();
			foreach (CAttackSummary.TargetSummary target2 in AttackSummary.Targets)
			{
				if (!target2.IsTargeted)
				{
					continue;
				}
				CActor actorToAttack = target2.ActorToAttack;
				foreach (CAbility item17 in target2.AttackAbilityWithOverrides.SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					if (item17.MiscAbilityData == null || !item17.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item17.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || actorToAttack.IsDead)
					{
						CAbility cAbility = CAbility.CopyAbility(item17, generateNewID: true, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
						cAbility.ParentAbility = this;
						cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actorToAttack.ArrayIndex.X, actorToAttack.ArrayIndex.Y]);
						cAbility.TargetThisActorAutomatically = actorToAttack;
						cAbility.TargetingOverrideArrayIndex = TargetingOverrideArrayIndex;
						cAbility.AddTargetSummaryContext(target2);
						list3.Add(cAbility);
					}
				}
			}
			(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list3, null, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: true);
			if (true)
			{
				m_State = EAttackState.CheckForPlayerIdle;
				CWaitForProgressChoreographer_MessageData message8 = new CWaitForProgressChoreographer_MessageData(TargetingActor)
				{
					WaitTickFrame = 10000,
					WaitActor = TargetingActor,
					ClearEvents = false
				};
				ScenarioRuleClient.MessageHandler(message8);
				return true;
			}
			m_State = EAttackState.CheckForPlayerIdle;
			Perform();
			break;
		}
		case EAttackState.CheckForPlayerIdle:
			if (m_AttackedActors.Count() > 0 && m_AttackedActors[0] != null)
			{
				CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = new CPlayerWaitForIdle_MessageData(TargetingActor);
				cPlayerWaitForIdle_MessageData.m_Actor = m_AttackedActors[0];
				ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData);
				return true;
			}
			PhaseManager.StepComplete();
			break;
		case EAttackState.FinaliseAttack:
		{
			if (m_AttackSummary.Targets.Count > 0)
			{
				AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
				if (miscAbilityData4 != null && miscAbilityData4.TargetOneEnemyWithAllAttacks == true)
				{
					CAttackSummary.TargetSummary targetSummary = m_AttackSummary.Targets[0];
					if (targetSummary.UsedAttackMods.Except(m_PreviousAttackModsForTarget).Any((AttackModifierYMLData y) => y?.IsCurse ?? false))
					{
						foreach (CActiveBonus item18 in from y in CharacterClassManager.FindAllActiveBonuses().Distinct()
							where y.HasBehaviour<CDamageActiveBonus_DamageOnCurseModApplied>()
							select y)
						{
							item18.GetBehaviour<CDamageActiveBonus_DamageOnCurseModApplied>().TriggerAbility(TargetingActor);
						}
					}
					m_PreviousAttackModsForTarget = targetSummary.UsedAttackMods;
				}
				else if (m_AttackSummary.Targets.Any((CAttackSummary.TargetSummary x) => x?.UsedAttackMods != null && x.UsedAttackMods.Any((AttackModifierYMLData y) => y?.IsCurse ?? false)))
				{
					foreach (CActiveBonus item19 in from y in CharacterClassManager.FindAllActiveBonuses().Distinct()
						where y.HasBehaviour<CDamageActiveBonus_DamageOnCurseModApplied>()
						select y)
					{
						item19.GetBehaviour<CDamageActiveBonus_DamageOnCurseModApplied>().TriggerAbility(TargetingActor);
					}
				}
			}
			if (!m_MultiPassAttack && m_AttackedActors.Count > 1)
			{
				m_ActorsToTargetModifiedStrength.RemoveAt(0);
				m_AttackedActors.RemoveAt(0);
				m_State = EAttackState.CheckForPlayerIdle;
				Perform();
				break;
			}
			List<CAttackSummary.TargetSummary> list = new List<CAttackSummary.TargetSummary>();
			list.AddRange(m_AttackSummary.Targets.Where((CAttackSummary.TargetSummary x) => x.IsTargeted));
			if (!m_ProcessedStatusEffects)
			{
				foreach (CAttackSummary.TargetSummary item20 in list)
				{
					if (item20 == null || item20.ProcessedStatusEffects)
					{
						continue;
					}
					item20.ProcessedStatusEffects = true;
					if ((ScenarioManager.Scenario.HasActor(item20.Actor) && !item20.Actor.IsDowned && !item20.Actor.IsDead) || ProcessIfDead)
					{
						if (item20.AttackAbilityWithOverrides.NegativeConditions.Count > 0)
						{
							item20.AttackAbilityWithOverrides.ProcessNegativeStatusEffects(item20.Actor);
						}
						if (item20.Actor.m_OnBeingAttackedPostDamageListeners != null)
						{
							item20.Actor.m_OnBeingAttackedPostDamageListeners?.Invoke(this, item20);
						}
					}
					ApplyPositiveEffects(item20);
				}
				CheckModifierAbilities();
			}
			if (ApplyPostConditionEffects())
			{
				return false;
			}
			if (MultiPassAttack)
			{
				list.Clear();
				if (m_AttackedActors.Count > 0)
				{
					int targetIndex = 0;
					CAttackSummary.TargetSummary targetSummary2 = m_AttackSummary.FindTarget(m_AttackedActors[0], ref targetIndex);
					if (targetSummary2 != null)
					{
						list.Add(targetSummary2);
					}
				}
			}
			foreach (CAttackSummary.TargetSummary item21 in list)
			{
				if (item21 == null)
				{
					continue;
				}
				ApplyPreRetaliateEffects(item21);
				if (!item21.Actor.IsDead || ProcessIfDead)
				{
					TryPerformRetaliate(item21, checkWaitForAbilities: true);
					if (item21.ActualDamageTaken > 0 && item21.ActorToAttack is CEnemyActor)
					{
						CChallenge.CheckTrigger(CChallenge.Trigger.OnEnemySufferDamagePostRetaliate, new CChallenge.TriggerParams
						{
							actor = item21.ActorToAttack
						});
					}
				}
			}
			if (m_MultiPassAttack && (!TargetingActor.IsDead || ProcessIfDead))
			{
				if (m_ActorsToTargetModifiedStrength.Count > 0)
				{
					m_ActorsToTargetModifiedStrength.RemoveAt(0);
				}
				if (m_AttackedActors.Count > 0)
				{
					int targetIndex2 = 0;
					CAttackSummary.TargetSummary item = m_AttackSummary.FindTarget(m_AttackedActors[0], ref targetIndex2);
					AttackSummary.Targets.Remove(item);
				}
				while (m_MultiPassActors.Count > 0 && !ScenarioManager.Scenario.HasActor(m_MultiPassActors[0]))
				{
					m_MultiPassActors.Remove(m_MultiPassActors[0]);
				}
				if (m_MultiPassActors.Count > 0)
				{
					m_State = EAttackState.DrawingModifiers;
					DontMoveAbilityState = true;
					CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData2 = new CPlayerWaitForIdle_MessageData(TargetingActor);
					cPlayerWaitForIdle_MessageData2.m_Actor = TargetingActor;
					ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData2);
					return true;
				}
			}
			if (AreaEffect == null && m_AreaEffectBackup != null)
			{
				AreaEffect = m_AreaEffectBackup;
			}
			AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
			if (miscAbilityData5 != null && miscAbilityData5.TargetOneEnemyWithAllAttacks == true && m_CurrentAttackIndex < m_TotalNumberTargets && (!TargetingActor.IsDead || ProcessIfDead))
			{
				m_State = EAttackState.SelectAttackFocus;
				foreach (CAttackSummary.TargetSummary target in list)
				{
					ActorsToIgnore.Remove(target.Actor);
					ActiveSingleTargetItems?.RemoveAll((CItem x) => x.SingleTarget == target.ActorToAttack);
					ActiveSingleTargetActiveBonuses?.RemoveAll((CActiveBonus x) => x.SingleTarget == target.ActorToAttack);
				}
				Perform();
				return true;
			}
			if (TargetingActor.m_OnAttackAbilityFinishedListeners != null)
			{
				TargetingActor.m_OnAttackAbilityFinishedListeners?.Invoke(this);
			}
			CAttackDone_MessageData cAttackDone_MessageData = new CAttackDone_MessageData(TargetingActor);
			cAttackDone_MessageData.m_AttackingActor = TargetingActor;
			ScenarioRuleClient.MessageHandler(cAttackDone_MessageData);
			CachedAIMoveFocusActors?.Clear();
			if (IsMergedAbility)
			{
				PhaseManager.StepComplete();
			}
			else
			{
				m_State = EAttackState.AttackDone;
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility.m_Ability == this)
				{
					Perform();
				}
			}
			return true;
		}
		case EAttackState.AttackBuff:
		{
			AbilityHasHappened = true;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			}
			ScenarioRuleClient.FirstAbilityStarted();
			foreach (CActor item22 in m_ActorsToTarget)
			{
				ApplyToActor(item22);
			}
			CAttackBuff_MessageData message5 = new CAttackBuff_MessageData(AnimOverload, TargetingActor)
			{
				m_AttackAbility = this
			};
			ScenarioRuleClient.MessageHandler(message5);
			if (AnimOverload.Contains("None") || SkipAnim)
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		case EAttackState.SelectAttackFocusAdditionalTargets:
			DontMoveAbilityState = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (ChainAttack)
				{
					foreach (CActor item23 in m_ActorsToTarget)
					{
						if (!m_ActorsToIgnore.Contains(item23))
						{
							m_ActorsToIgnore.Add(item23);
						}
					}
					List<CActor> actorsInRange4 = GameState.GetActorsInRange(m_ActorsToTarget.Last().ArrayIndex, TargetingActor, m_ChainAttackRange, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
					m_ValidActorsInRange.Clear();
					foreach (CActor item24 in actorsInRange4)
					{
						if (!m_ValidActorsInRange.Contains(item24))
						{
							m_ValidActorsInRange.Add(item24);
						}
					}
					TilesInRange = GameState.GetTilesInRange(m_ActorsToTarget.Last(), m_ChainAttackRange, Targeting, emptyTilesOnly: false, !IsMeleeAttack);
					foreach (CActor item25 in m_ActorsToTarget)
					{
						CTile item3 = ScenarioManager.Tiles[item25.ArrayIndex.X, item25.ArrayIndex.Y];
						TilesInRange.Add(item3);
						m_ActorsToIgnore.Remove(item25);
					}
					ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
					foreach (CActor item26 in m_ValidActorsInRange)
					{
						if (!m_ActorsToTargetLocked.Contains(item26))
						{
							m_AttackSummary.UpdateSingleTargetData(this, item26);
						}
					}
				}
				CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData6 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
				cActorIsSelectingAttackFocusTargets_MessageData6.m_AttackingActor = TargetingActor;
				cActorIsSelectingAttackFocusTargets_MessageData6.m_AttackAbility = this;
				cActorIsSelectingAttackFocusTargets_MessageData6.m_AttackSummary = AttackSummary.Copy();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData6);
			}
			else
			{
				if (CachedAIMoveFocusActors == null || CachedAIMoveFocusActors.Count == 0)
				{
					TargetingActor.Move(0, jump: false, fly: false, Range, allowMove: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, this, firstMove: true, moveTest: false, carryOtherActors: false);
				}
				if (CachedAIMoveFocusActors.Count != 0)
				{
					_ = CachedAIMoveFocusActors[0];
				}
				foreach (CActor cachedAIMoveFocusActor3 in CachedAIMoveFocusActors)
				{
					if (cachedAIMoveFocusActor3 != null && !m_ActorsToTarget.Contains(cachedAIMoveFocusActor3))
					{
						if (!m_ActorsToIgnore.Contains(cachedAIMoveFocusActor3) && IsActorWithinRangeAndLoS(cachedAIMoveFocusActor3, Range))
						{
							m_ActorsToTarget.Add(cachedAIMoveFocusActor3);
							AttackSummary.UpdateSingleTargetData(this, cachedAIMoveFocusActor3);
						}
						if (m_ActorsToTarget.Count == m_NumberTargetsRemaining)
						{
							break;
						}
					}
				}
				m_State = EAttackState.DrawingModifiers;
				Perform();
			}
			return true;
		case EAttackState.SelectAdditionalTargetsDone:
			m_State = EAttackState.DrawingModifiers;
			AttackSummary.UpdateTargetData(this, ActiveSingleTargetItems, ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
			if (ChainAttack)
			{
				foreach (CActor item27 in m_ActorsToTarget)
				{
					if (!m_ActorsToTargetLocked.Contains(item27))
					{
						m_AttackSummary.UpdateSingleTargetData(this, item27);
					}
				}
			}
			m_SelectAdditionalTargetsDone = true;
			Perform();
			return true;
		case EAttackState.SelectAttackFocusAddTargetBuff:
		{
			if (m_AttackSummary != null && !ChainAttack)
			{
				m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
			}
			CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
			cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor = TargetingActor;
			cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility = this;
			cActorIsSelectingAttackFocusTargets_MessageData.m_AttackSummary = AttackSummary.Copy();
			ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData);
			return true;
		}
		case EAttackState.SelectAttackFocusAddTargetBuffDone:
			if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				m_State = EAttackState.AttackBuff;
			}
			else
			{
				m_State = EAttackState.PreActorIsAttacking;
			}
			Perform();
			return true;
		case EAttackState.AttackDone:
			{
				if (PendingExtraTargetsToSelect > 0)
				{
					StartAddExtraTargets();
				}
				else
				{
					PhaseManager.NextStep();
				}
				return true;
			}
			IL_162a:
			if (AttackSummary != null)
			{
				AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
			}
			goto IL_164d;
			IL_164d:
			for (int num5 = m_ActiveSingleTargetItems.Count - 1; num5 >= 0; num5--)
			{
				CItem cItem = m_ActiveSingleTargetItems[num5];
				if (cItem != null && cItem.SingleTarget == null)
				{
					TargetingActor.Inventory.DeselectItem(cItem);
					ToggleSingleTargetItem(cItem);
				}
			}
			for (int num6 = m_ActiveSingleTargetActiveBonuses.Count - 1; num6 >= 0; num6--)
			{
				CActiveBonus cActiveBonus = m_ActiveSingleTargetActiveBonuses[num6];
				if (cActiveBonus != null && cActiveBonus.SingleTarget == null)
				{
					cActiveBonus.ToggleActiveBonus(null, TargetingActor);
				}
			}
			m_CanUndo = false;
			if (TargetingActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.SummonData.AttackInfuse.HasValue)
			{
				ElementInfusionBoardManager.Infuse((TargetingActor as CHeroSummonActor).SummonData.AttackInfuse.Value, TargetingActor);
			}
			else if (MiscAbilityData?.OnAttackInfuse != null)
			{
				ElementInfusionBoardManager.Infuse(MiscAbilityData.OnAttackInfuse, TargetingActor);
			}
			if (TargetingActor.m_OnPreActorIsAttackingListeners != null)
			{
				TargetingActor.m_OnPreActorIsAttackingListeners(this);
			}
			flag4 = false;
			if (TargetingActor.Type.Equals(CActor.EType.Player) && m_ActorsToTarget.Count > 0)
			{
				List<CActiveBonus> list6 = (from w in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Attack)
					where w.Ability.Augment == null && w.IsActiveBonusToggledAndNotRestricted(TargetingActor)
					select w).ToList();
				if (list6.Count > 0)
				{
					foreach (CActiveBonus item28 in list6)
					{
						if (item28.StackActiveBonusInlineAbility(m_ActorsToTarget[0], this))
						{
							if (m_AttackSummary != null)
							{
								m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
							}
							flag4 = true;
							m_StackedInlineAbilityActiveBonuses.Add(item28);
						}
					}
				}
			}
			if (m_ActorsToTarget.Count <= 0)
			{
				SetCancelAbilityFlag(cancel: true);
			}
			if (!flag4)
			{
				if (IsMergedAbility)
				{
					m_State = EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility;
					PhaseManager.StepComplete();
				}
				else
				{
					m_State = EAttackState.CheckForDamageSelf;
					Perform();
				}
			}
			else
			{
				m_CurrentStackedInlineAbilityActiveBonus = m_StackedInlineAbilityActiveBonuses[0];
				if (m_ActorsToTarget.Count > 1)
				{
					m_State = EAttackState.SelectActiveBonusBuffTarget;
				}
				else
				{
					m_State = EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility;
				}
			}
			break;
		}
		return false;
	}

	public static void ApplyRetaliateToParentAttack(CAbility ability, CActor target)
	{
		if (target != null && ability.ParentAbility != null && ability.ParentAbility is CAbilityAttack cAbilityAttack && ability.IsInlineSubAbility && !((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor).IsDead && !target.IsDead)
		{
			cAbilityAttack.TryPerformRetaliate(cAbilityAttack.AttackSummary.FindTarget(target), checkWaitForAbilities: false);
		}
	}

	private bool ShouldWaitForAbilityBeforeRetaliate(CAbility ability)
	{
		if (AbilityTypesToWaitForRetaliate.Contains(ability.AbilityType) && ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			return true;
		}
		if (ability.GetMiscVar("TriggerBeforeRetaliate").GetBool())
		{
			return true;
		}
		return false;
	}

	public void TryPerformRetaliate(CAttackSummary.TargetSummary targetSummary, bool checkWaitForAbilities)
	{
		if (targetSummary == null || targetSummary.PerformedRetaliate)
		{
			return;
		}
		CActor cActor = ((OriginalTargetingActor != null) ? OriginalTargetingActor : TargetingActor);
		CAbilityAttack obj = targetSummary.AttackAbilityWithOverrides as CAbilityAttack;
		bool flag = PhaseManager.CurrentPhaseAction?.PreviousPhaseAbilities != null && PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Any((CPhaseAction.CPhaseAbility a) => a.m_Ability.ParentAbility == this && a.m_Ability.AbilityHasHappened && ShouldWaitForAbilityBeforeRetaliate(a.m_Ability));
		bool flag2 = obj.ModifierAbilities.Any((AttackModifierAbilityInfo modAbilityInfo) => ShouldWaitForAbilityBeforeRetaliate(modAbilityInfo.Ability) && modAbilityInfo.Ability.AbilityHasHappened);
		bool flag3 = obj.CurrentOverrides.Any((CAbilityOverride a) => a.AbilityType.HasValue && AbilityTypesToWaitForRetaliate.Contains(a.AbilityType.Value) && a.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA);
		if (!checkWaitForAbilities && PhaseManager.CurrentPhaseAction != null)
		{
			List<CPhaseAction.CPhaseAbility> currentPhaseAbilities = PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities;
			if (currentPhaseAbilities.Count > 1 && currentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x != PhaseManager.CurrentPhaseAction.CurrentPhaseAbility && x.m_Ability != null && ShouldWaitForAbilityBeforeRetaliate(x.m_Ability) && x.m_Ability.TargetThisActorAutomatically == targetSummary.ActorToAttack))
			{
				return;
			}
		}
		if ((!checkWaitForAbilities || (!flag && !flag2 && !flag3)) && ScenarioManager.Scenario.HasActor(cActor))
		{
			targetSummary.PerformedRetaliate = true;
			cActor.ApplyRetaliateToAttack(targetSummary.Actor, this);
		}
	}

	private void ApplyPositiveEffects(CAttackSummary.TargetSummary targetSummary)
	{
		CActor cActor = (GetMiscVar("ApplyPositiveEffectsToTarget").GetBool() ? targetSummary.Actor : TargetingActor);
		if (ScenarioManager.Scenario.HasActor(cActor) && !cActor.IsDead && targetSummary != null && targetSummary.AttackAbilityWithOverrides.PositiveConditions.Count > 0)
		{
			targetSummary.AttackAbilityWithOverrides.ProcessPositiveStatusEffects(cActor);
		}
	}

	private AbilityData.EActiveBonusFlag GetBonusesFlags()
	{
		AbilityData.EActiveBonusFlag eActiveBonusFlag = AbilityData.EActiveBonusFlag.None;
		for (int i = 0; i < m_AttackedActors.Count; i++)
		{
			foreach (CActiveBonus attackActiveBonuse in m_AttackSummary.AttackActiveBonuses)
			{
				eActiveBonusFlag |= attackActiveBonuse.ReferenceFlags(this, m_AttackedActors[i]);
			}
			foreach (CActiveBonus addRangeActiveBonuse in m_AttackSummary.AddRangeActiveBonuses)
			{
				eActiveBonusFlag |= addRangeActiveBonuse.ReferenceFlags(this, m_AttackedActors[i]);
			}
			foreach (CActiveBonus addTargetActiveBonuse in m_AttackSummary.AddTargetActiveBonuses)
			{
				eActiveBonusFlag |= addTargetActiveBonuse.ReferenceFlags(this, m_AttackedActors[i]);
			}
			for (int j = 0; j < TargetingActor.ActiveBonuses.Count; j++)
			{
				if ((TargetingActor.ActiveBonuses[j].ReferenceFlags(this, m_AttackedActors[i]) & AbilityData.EActiveBonusFlag.DrifterShockWaveAttack) != 0)
				{
					eActiveBonusFlag |= AbilityData.EActiveBonusFlag.DrifterShockWaveAttack;
					break;
				}
			}
		}
		return eActiveBonusFlag;
	}

	public void ApplyPreRetaliateEffects(CAttackSummary.TargetSummary targetSummary)
	{
		CAbility attackAbilityWithOverrides = targetSummary.AttackAbilityWithOverrides;
		if ((attackAbilityWithOverrides.Flags & EAbilityFlag.ModifiesTrophyTokens) != 0 && TargetingActor is CPlayerActor cPlayerActor)
		{
			cPlayerActor.SetTrophyTokens(cPlayerActor.GetTrophyTokens() + attackAbilityWithOverrides.Tokens);
		}
	}

	private bool ApplyPostConditionEffects()
	{
		List<CAttackSummary.TargetSummary> list = TotalTargetSummaries.Where((CAttackSummary.TargetSummary x) => x.IsTargeted).ToList();
		if (m_PostConditionActorsChecked != null)
		{
			foreach (CAttackSummary.TargetSummary item in m_PostConditionActorsChecked)
			{
				list.Remove(item);
			}
		}
		m_PostConditionActorsToCheck = list;
		bool flag = false;
		foreach (CAttackSummary.TargetSummary item2 in m_PostConditionActorsToCheck)
		{
			flag |= ApplyPostConditionEffects(item2);
			m_PostConditionActorsChecked.Add(item2);
			if (flag)
			{
				PhaseManager.StepComplete();
				break;
			}
		}
		return flag;
	}

	public bool ApplyPostConditionEffects(CAttackSummary.TargetSummary targetSummary)
	{
		if (targetSummary == null)
		{
			return false;
		}
		CActor actorToAttack = targetSummary.ActorToAttack;
		CAbility cAbility = targetSummary.AttackAbilityWithOverrides;
		if (cAbility == null)
		{
			cAbility = this;
		}
		CVars cVars = new CVars();
		cVars.Set("ActorBeingAttacked", new TValue(actorToAttack));
		cVars.Set("ActorAttacking", new TValue(TargetingActor));
		cVars.Set("DamageTaken", targetSummary.ActualDamageTaken);
		bool flag = false;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			flag |= GameState.ShowItemSelection(playerActor, TargetingActor, cAbility, CItem.EItemTrigger.AfterAttackedIgnoringDamageTaken, onlyInActionPhase: true, killAfter: false, cVars);
		}
		if (targetSummary.ActualDamageTaken > 0 && targetSummary.ActorToAttack is CEnemyActor actor)
		{
			CChallenge.CheckTrigger(CChallenge.Trigger.OnEnemySufferDamage, new CChallenge.TriggerParams
			{
				actor = actor
			});
		}
		if (flag)
		{
			return AbilityInterruptCopy != null;
		}
		return false;
	}

	public static bool TargetsOnPath(CAbilityAttack attackAbility, List<CTile> optionalTileList, out List<CActor> targetActors)
	{
		targetActors = new List<CActor>();
		if (optionalTileList != null)
		{
			for (int i = 0; i < optionalTileList.Count; i++)
			{
				bool foundPath = false;
				List<Point> list = new List<Point>();
				list = ((i != 0) ? ScenarioManager.PathFinder.FindPath(attackAbility.TargetingActor, optionalTileList[i - 1].m_ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, attackAbility.IgnoreDoorLocks) : ScenarioManager.PathFinder.FindPath(attackAbility.TargetingActor, attackAbility.TargetingActor.ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, attackAbility.IgnoreDoorLocks));
				if (foundPath)
				{
					foreach (Point item in list)
					{
						List<CActor> actorsOnTile = GameState.GetActorsOnTile(ScenarioManager.Tiles[item.X, item.Y], attackAbility.TargetingActor, attackAbility.AbilityFilter, attackAbility.ActorsToIgnore, attackAbility.IsTargetedAbility, false);
						targetActors.AddRange(actorsOnTile);
					}
					continue;
				}
				return false;
			}
			targetActors = targetActors.Distinct().ToList();
			return true;
		}
		return false;
	}

	private bool RefreshFirstAttackBuffs()
	{
		if (TargetingActor.FindApplicableActiveBonuses(EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack).Any((CActiveBonus buff) => buff.Ability is CAbilityAttack cAbilityAttack && ((buff.HasTracker && cAbilityAttack.ActiveBonusData.IsSingleTargetBonus && cAbilityAttack.AttackEffects.Any((CAttackEffect x) => x.Range != 0)) || cAbilityAttack.AttackEffects.Any((CAttackEffect x) => x.Effect == CAttackEffect.EAttackEffect.FirstTargetOnly))))
		{
			ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
			ResetBuffs();
			Range = m_OriginalRange;
			SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
			ApplyBuffs(TargetingActor);
			return AreaEffect == null;
		}
		return false;
	}

	public override bool StoppedByDisarm(CActor overrideActor = null)
	{
		if (IgnoreDisarm())
		{
			return false;
		}
		CActor cActor = TargetingActor;
		if (cActor == null)
		{
			cActor = overrideActor;
		}
		return cActor?.Tokens?.HasKey(CCondition.ENegativeCondition.Disarm) == true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		List<CActor> second = ActorsToTarget.ToList();
		bool flag = false;
		if (!StoppedByDisarm() && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
		{
			if (m_State.Equals(EAttackState.SelectActiveBonusBuffTarget))
			{
				if (CheckSelectedActiveBonusBuffTarget(selectedTile))
				{
					flag = true;
				}
			}
			else
			{
				if (AreaEffect != null)
				{
					LastSelectedAreaEffectTile = selectedTile;
					UpdateAreaEffect(selectedTile, AreaEffectAngle, AreaEffectFlipped);
					bool flag2 = false;
					List<CActor> list = GetValidActorsInArea(ValidTilesInAreaEffectedIncludingBlocked, ignoreImmunity: true);
					foreach (CActor item in list.ToList())
					{
						if (FlagsOfValidTiles.TryGetValue(item.Tile, out var value) && (value & CAreaEffect.EHexFlag.Space) != 0)
						{
							list.Remove(item);
						}
					}
					if (MiscAbilityData != null && MiscAbilityData.MinRange.HasValue)
					{
						List<CActor> second2 = list.Where((CActor actor) => IsActorWithinRangeAndLoS(actor, MiscAbilityData.MinRange.Value - 1) && !IsActorWithinRangeAndLoS(actor, MiscAbilityData.MinRange.Value)).ToList();
						List<CActor> second3 = list.Where((CActor actor) => !IsActorWithinRangeAndLoS(actor, Range)).ToList();
						list = list.Except(second2).Except(second3).ToList();
					}
					if (list.Count > 0 && (!RequiresAllyTargeting() || ValidAlliesTargeted()))
					{
						flag2 = true;
						m_AreaEffectLocked = true;
						ValidActorsInRange = list;
					}
					if (!flag2)
					{
						base.TileSelected(selectedTile, optionalTileList);
						CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
						cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor = TargetingActor;
						cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility = this;
						cActorIsSelectingAttackFocusTargets_MessageData.m_AttackSummary = AttackSummary.Copy();
						ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData);
						LogEvent(ESESubTypeAbility.AbilityTileSelected);
						return;
					}
				}
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
				if (TargetsOnPath(this, optionalTileList, out var targetActors))
				{
					m_ActorsToTarget.Clear();
					foreach (CActor item2 in targetActors)
					{
						if (!IsMirroredHealthCloneTargeted(item2) && !CAbility.ImmuneToAbility(item2, this, targeting: true) && CActor.HaveLOS(ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y], ScenarioManager.Tiles[item2.ArrayIndex.X, item2.ArrayIndex.Y]))
						{
							m_ActorsToTarget.Add(item2);
						}
					}
					if (m_ActorsToTarget.Count > 0)
					{
						m_State = EAttackState.DrawingModifiers;
						flag = true;
					}
				}
				else if (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAdditionalTargets || (m_State == EAttackState.SelectAttackFocusAddTargetBuff && m_AreaEffectBackup == null))
				{
					bool flag3 = ActorsToTarget.Contains(actorOnTile);
					bool num = (SharedAbilityTargeting.TileSelected(this, selectedTile, (m_State == EAttackState.SelectAttackFocus) ? AreaEffect : null, TargetingActor, FilterActor, Range, ref m_ValidActorsInRange, m_ActorsToIgnore, AbilityFilter, m_TotalNumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, IsTargetedAbility) && AreaEffect != null) || (ActorsToTarget.Contains(actorOnTile) && !flag3);
					if ((MiscVars != null && MiscVars.Find(((string, TValue) x) => x.Item1 == "RecalculateTargetingOnTileSelection").Item2.GetBool()) | RefreshFirstAttackBuffs())
					{
						SharedAbilityTargeting.GetValidActorsInRange(this);
					}
					if (num)
					{
						if (m_State == EAttackState.SelectAttackFocusAdditionalTargets && AreaEffect == null)
						{
							m_AttackSummary.UpdateSingleTargetData(this, actorOnTile);
							m_AttackSummary.AutoToggleSingleTargetBonuses();
						}
						if (m_State == EAttackState.SelectAttackFocus && AreaEffect != null && m_AttackSummary.ActiveBonusAddTargetBuff != 0)
						{
							if (m_AttackSummary != null)
							{
								m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: false, useBonusTracker: true);
								m_AttackSummary.AutoToggleSingleTargetBonuses();
							}
							if (m_NumberTargetsRemaining > 1)
							{
								m_NumberTargetsRemaining--;
							}
							m_AreaEffectBackup = AreaEffect;
							AreaEffect = null;
							m_State = EAttackState.SelectAttackFocusAddTargetBuff;
							m_ActorsToTarget = new List<CActor>(ValidActorsInRange);
							List<CActor> list2 = new List<CActor>(ValidActorsInRange);
							list2.AddRange(m_ActorsToIgnore);
							list2 = list2.Distinct().ToList();
							ValidActorsInRange = GameState.GetActorsInRange(TargetingActor, FilterActor, Range, list2, AbilityFilter, null, AllPossibleTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
							m_ActorsToTarget.Union(ValidActorsInRange);
							m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
							m_AttackSummary.AutoToggleSingleTargetBonuses();
							CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData2 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
							cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackingActor = TargetingActor;
							cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackAbility = this;
							cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackSummary = AttackSummary.Copy();
							ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData2);
						}
						else
						{
							if (AreaEffect != null)
							{
								m_ActorsToTarget = ValidActorsInRange.ToList();
								foreach (CActor item3 in m_ActorsToTarget.ToList())
								{
									if (FlagsOfValidTiles.TryGetValue(item3.Tile, out var value2) && (value2 & CAreaEffect.EHexFlag.Space) != 0)
									{
										m_ActorsToTarget.Remove(item3);
									}
								}
							}
							if (m_State == EAttackState.SelectAttackFocus)
							{
								CAbilityFilter.CurrentVarsContext = VarsContext;
								if (m_AttackSummary != null)
								{
									m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true, useBothLists: false, useBonusTracker: true);
									m_AttackSummary.AutoToggleSingleTargetBonuses();
								}
								CAbilityFilter.CurrentVarsContext = null;
							}
							CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData3 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
							cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackingActor = TargetingActor;
							cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackAbility = this;
							cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackSummary = AttackSummary.Copy();
							ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData3);
						}
					}
					else if (ChainAttack)
					{
						flag = true;
					}
					else if (UseSubAbilityTargeting)
					{
						PhaseManager.NextStep();
					}
					else
					{
						if (m_State == EAttackState.SelectAttackFocus)
						{
							CAbilityFilter.CurrentVarsContext = VarsContext;
							if (m_AttackSummary != null)
							{
								m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true, useBonusTracker: true);
								m_AttackSummary.AutoToggleSingleTargetBonuses();
							}
							CAbilityFilter.CurrentVarsContext = null;
						}
						CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData4 = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
						cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackingActor = TargetingActor;
						cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackAbility = this;
						cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackSummary = AttackSummary.Copy();
						ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData4);
					}
				}
				else if (m_State == EAttackState.SelectAttackFocusAddTargetBuff && m_NumberTargetsRemaining > 0 && GameState.GetTilesInRange(TargetingActor.ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true).Contains(selectedTile) && actorOnTile != null)
				{
					m_NumberTargetsRemaining--;
					m_ActorsToTarget.Add(actorOnTile);
					if (m_AttackSummary != null)
					{
						foreach (CActor item4 in m_ActorsToTarget)
						{
							m_AttackSummary.UpdateSingleTargetData(this, item4);
						}
					}
				}
			}
			if (flag)
			{
				Perform();
			}
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
		if (!ActorsToTarget.SequenceEqual(second))
		{
			LogEvent(ESESubTypeAbility.AbilityTargetsChanged);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		List<CActor> second = ActorsToTarget.ToList();
		if (m_State.Equals(EAttackState.SelectActiveBonusBuffTarget))
		{
			if (CheckSelectedActiveBonusBuffTarget(selectedTile))
			{
				flag = true;
			}
		}
		else if ((AreaEffect != null || AreaEffectBackup != null) && m_AreaEffectLocked && m_State != EAttackState.SelectAttackFocusAdditionalTargets)
		{
			if (SharedAbilityTargeting.TileDeselected(this, selectedTile, m_TotalNumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
			{
				if (AreaEffectBackup != null)
				{
					AreaEffect = m_AreaEffectBackup;
					m_AreaEffectBackup = null;
					m_State = EAttackState.SelectAttackFocus;
					m_ActorsToIgnore.Clear();
					m_NumberTargetsRemaining = NumberTargets + m_AttackSummary.ActiveBonusAddTargetBuff;
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				m_AreaEffectLocked = false;
				SharedAbilityTargeting.GetValidActorsInRange(this);
				flag = true;
				if (m_ActorsToTarget.Count <= 0)
				{
					TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				}
			}
		}
		else if (!AllTargets)
		{
			bool flag2 = TargetingOverrideArrayIndex.X != 0 && TargetingOverrideArrayIndex.Y != 0;
			CActor cActor = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex, flag2 ? TargetingActor : null);
			if (cActor != null && !m_ActorsToTargetLocked.Contains(cActor))
			{
				bool flag3 = false;
				if (ChainAttack)
				{
					int num = m_ActorsToTarget.IndexOf(cActor);
					for (int num2 = m_ActorsToTarget.Count - 1; num2 >= num; num2--)
					{
						CTile selectedTile2 = ScenarioManager.Tiles[m_ActorsToTarget[num2].ArrayIndex.X, m_ActorsToTarget[num2].ArrayIndex.Y];
						flag3 |= SharedAbilityTargeting.TileDeselected(this, selectedTile2, m_TotalNumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected);
					}
				}
				else
				{
					CAreaEffect cAreaEffect = null;
					if (m_State == EAttackState.SelectAttackFocusAdditionalTargets && AreaEffectBackup != null)
					{
						cAreaEffect = AreaEffectBackup;
						m_AreaEffectBackup = null;
					}
					flag3 |= SharedAbilityTargeting.TileDeselected(this, selectedTile, m_TotalNumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected);
					if (cAreaEffect != null)
					{
						m_AreaEffectBackup = cAreaEffect;
					}
				}
				if (flag3)
				{
					flag = true;
					if ((MiscVars != null && MiscVars.Find(((string, TValue) x) => x.Item1 == "RecalculateTargetingOnTileSelection").Item2.GetBool()) | RefreshFirstAttackBuffs())
					{
						SharedAbilityTargeting.GetValidActorsInRange(this);
					}
					if (m_ActorsToTarget.Count <= 0)
					{
						TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
					}
					if (m_State == EAttackState.SelectAttackFocusAdditionalTargets)
					{
						CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(cActor);
						if (targetSummary != null)
						{
							m_AttackSummary.Targets.Remove(targetSummary);
						}
					}
					if (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAdditionalTargets || m_State == EAttackState.SelectAttackFocusAddTargetBuff)
					{
						CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor);
						cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor = TargetingActor;
						cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility = this;
						cActorIsSelectingAttackFocusTargets_MessageData.m_AttackSummary = AttackSummary.Copy();
						ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData);
					}
				}
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
		if (!ActorsToTarget.SequenceEqual(second))
		{
			LogEvent(ESESubTypeAbility.AbilityTargetsChanged);
		}
	}

	private bool CheckSelectedActiveBonusBuffTarget(CTile selectedTile)
	{
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
		if (ValidActorsInRange.Contains(actorOnTile) && m_CurrentStackedInlineAbilityActiveBonus != null)
		{
			m_CurrentStackedInlineAbilityActiveBonus.UpdateActiveBonusInlineAbilityTarget(actorOnTile);
			m_StackedInlineAbilityActiveBonuses.Remove(m_CurrentStackedInlineAbilityActiveBonus);
			if (m_StackedInlineAbilityActiveBonuses.Count > 0)
			{
				m_CurrentStackedInlineAbilityActiveBonus = m_StackedInlineAbilityActiveBonuses[0];
			}
			else
			{
				m_State = EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility;
			}
			return true;
		}
		return false;
	}

	public void UpdateAllTargetsOnAttackPath(List<CTile> optionalTileList)
	{
		if (!TargetsOnPath(this, optionalTileList, out var targetActors))
		{
			return;
		}
		m_ActorsToTarget.Clear();
		foreach (CActor item in targetActors)
		{
			if (!CAbility.ImmuneToAbility(item, this, targeting: true) && CActor.HaveLOS(ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y], ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y]))
			{
				m_ActorsToTarget.Add(item);
			}
		}
		TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction);
		CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(TargetingActor);
		cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
		cShowSingleTargetActiveBonus_MessageData.m_Ability = this;
		ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		m_ActorsToTargetLocked.Clear();
		if (AreaEffect != null && m_State == EAttackState.SelectAttackFocus)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else if (m_AreaEffectBackup != null && m_State == EAttackState.SelectAttackFocusAddTargetBuff)
		{
			AreaEffect = m_AreaEffectBackup;
			m_AreaEffectBackup = null;
			m_State = EAttackState.SelectAttackFocus;
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if ((MiscVars != null && MiscVars.Find(((string, TValue) x) => x.Item1 == "RecalculateTargetingOnTileSelection").Item2.GetBool()) | RefreshFirstAttackBuffs())
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (m_ActiveSingleTargetItems.Count > 0 || m_ActiveSingleTargetActiveBonuses.Count > 0)
		{
			for (int num = m_ActiveSingleTargetItems.Count - 1; num >= 0; num--)
			{
				CItem cItem = m_ActiveSingleTargetItems[num];
				if (cItem != null)
				{
					TargetingActor.Inventory.DeselectItem(cItem);
				}
			}
			m_ActiveSingleTargetItems.Clear();
			for (int num2 = m_ActiveSingleTargetActiveBonuses.Count - 1; num2 >= 0; num2--)
			{
				CActiveBonus cActiveBonus = m_ActiveSingleTargetActiveBonuses[num2];
				if (cActiveBonus.CostAbilityExecuted || (!AbilityHasHappened && HasEnded))
				{
					cActiveBonus.ToggleLocked = false;
				}
				if (cActiveBonus != null && cActiveBonus.ToggledBonus)
				{
					cActiveBonus.ToggleActiveBonus(null, TargetingActor);
				}
			}
			m_ActiveSingleTargetActiveBonuses.Clear();
			if (HasEnded)
			{
				return;
			}
			CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(TargetingActor);
			cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = false;
			cShowSingleTargetActiveBonus_MessageData.m_Ability = this;
			ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
			Restart();
			Perform();
		}
		else
		{
			CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData2 = new CShowSingleTargetActiveBonus_MessageData(TargetingActor);
			cShowSingleTargetActiveBonus_MessageData2.m_ShowSingleTargetActiveBonus = false;
			cShowSingleTargetActiveBonus_MessageData2.m_Ability = this;
			ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData2);
		}
		if (!HasEnded && m_AttackSummary != null)
		{
			CAbilityFilter.CurrentVarsContext = VarsContext;
			m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
			CAbilityFilter.CurrentVarsContext = null;
			TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			CActorIsSelectingAttackFocusTargets_MessageData message = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor)
			{
				m_AttackingActor = TargetingActor,
				m_AttackAbility = this,
				m_AttackSummary = AttackSummary.Copy()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override bool CanClearTargets()
	{
		if (UseSubAbilityTargeting)
		{
			return false;
		}
		if (m_CanClearTargets && (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAddTargetBuff))
		{
			return !StoppedByDisarm();
		}
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		if (UseSubAbilityTargeting)
		{
			return false;
		}
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EAttackState.SelectAttackFocus && m_State != EAttackState.SelectAttackFocusAddTargetBuff && m_State != EAttackState.SelectActiveBonusBuffTarget)
			{
				return m_State == EAttackState.SelectAttackFocusAdditionalTargets;
			}
			return true;
		}
		return false;
	}

	public override bool RequiresWaypointSelection()
	{
		if (AllTargetsOnAttackPath && (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAddTargetBuff || m_State == EAttackState.SelectAttackFocusAdditionalTargets))
		{
			if (ActiveBonusData != null)
			{
				if (ActiveBonusData != null)
				{
					return ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		if (StoppedByDisarm())
		{
			return false;
		}
		if (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.CheckForAIToggleBonuses || m_State == EAttackState.SelectAttackFocusAddTargetBuff)
		{
			if (ActiveBonusData != null)
			{
				if (ActiveBonusData != null)
				{
					return ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool MaxTargetsSelected()
	{
		if (AreaEffect != null || AreaEffectBackup != null)
		{
			if (m_State == EAttackState.SelectAttackFocusAddTargetBuff || m_State == EAttackState.SelectAttackFocusAdditionalTargets)
			{
				return m_NumberTargetsRemaining <= 0;
			}
			return m_AreaEffectLocked;
		}
		return m_NumberTargetsRemaining <= 0;
	}

	public override void ToggleSingleTargetItem(CItem item)
	{
		CActor singleTarget = item.SingleTarget;
		base.ToggleSingleTargetItem(item);
		if (singleTarget != null && !m_ActiveSingleTargetItems.Contains(item))
		{
			for (int num = m_AttackSummary.Targets.Count - 1; num >= 0; num--)
			{
				CAttackSummary.TargetSummary targetSummary = m_AttackSummary.Targets[num];
				if (targetSummary.Actor == singleTarget)
				{
					m_AttackSummary.Targets.Remove(targetSummary);
					break;
				}
			}
			m_AttackSummary.UpdateSingleTargetData(this, singleTarget);
			ApplySingleTargetActiveBonus(singleTarget);
			ApplySingleTargetItem(singleTarget);
		}
		if (m_State == EAttackState.SelectAttackFocus)
		{
			CActorIsSelectingAttackFocusTargets_MessageData message = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor)
			{
				m_AttackingActor = TargetingActor,
				m_AttackAbility = this,
				m_AttackSummary = AttackSummary.Copy()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override void ToggleSingleTargetActiveBonus(CActiveBonus activeBonus)
	{
		CActor singleTarget = activeBonus.SingleTarget;
		base.ToggleSingleTargetActiveBonus(activeBonus);
		if (singleTarget != null && !m_ActiveSingleTargetActiveBonuses.Contains(activeBonus))
		{
			for (int num = m_AttackSummary.Targets.Count - 1; num >= 0; num--)
			{
				CAttackSummary.TargetSummary targetSummary = m_AttackSummary.Targets[num];
				if (targetSummary.Actor == singleTarget)
				{
					m_AttackSummary.Targets.Remove(targetSummary);
					break;
				}
			}
			m_AttackSummary.UpdateTargetData(this, ActiveSingleTargetItems, ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true, useBonusTracker: true);
			ApplySingleTargetActiveBonus(singleTarget);
			ApplySingleTargetItem(singleTarget);
		}
		if (m_State == EAttackState.SelectAttackFocus)
		{
			CActorIsSelectingAttackFocusTargets_MessageData message = new CActorIsSelectingAttackFocusTargets_MessageData(TargetingActor)
			{
				m_AttackingActor = TargetingActor,
				m_AttackAbility = this,
				m_AttackSummary = AttackSummary.Copy()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, TargetingActor);
			}
			else if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find base ability card for ability " + Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState && m_State != EAttackState.AttackDone && m_State != EAttackState.AttackBuffDone)
		{
			m_State++;
		}
		if (m_State != EAttackState.AttackDone)
		{
			return m_State == EAttackState.AttackBuffDone;
		}
		return true;
	}

	public override void AbilityEnded()
	{
		if (!ScenarioManager.IsInTutorial && ConfirmAttackAbilityBeforeFinishing && !TargetingActor.MindControlIsFromEnemy && !AbilityHasBeenCancelled && AbilityHasHappened && (TargetingActor is CPlayerActor || TargetingActor is CHeroSummonActor) && ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			GameState.ShowActionConfirmationPanel(TargetingActor, TargetingActor, this, "AttackAbilityEnded");
		}
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		if (!AbilityHasHappened)
		{
			ClearTargets();
		}
		foreach (CActiveBonus item in (from x in TargetingActor.FindApplicableActiveBonuses(EAbilityType.Attack)
			where x.CostAbilityExecuted && x.Ability.ActiveBonusData.IsSingleTargetBonus && x.Ability.ActiveBonusData.IsToggleBonus
			select x).ToList())
		{
			item.RestrictActiveBonus(TargetingActor);
		}
	}

	public override void Restart()
	{
		if (m_ActiveSingleTargetActiveBonuses.Count > 0)
		{
			for (int num = m_ActiveSingleTargetActiveBonuses.Count - 1; num >= 0; num--)
			{
				CActiveBonus cActiveBonus = m_ActiveSingleTargetActiveBonuses[num];
				if (cActiveBonus.CostAbilityExecuted)
				{
					cActiveBonus.ToggleLocked = false;
				}
				if (cActiveBonus != null && cActiveBonus.ToggledBonus)
				{
					cActiveBonus.ToggleActiveBonus(null, TargetingActor);
				}
			}
			m_ActiveSingleTargetActiveBonuses.Clear();
		}
		Start(TargetingActor, FilterActor, ControllingActor);
		ClearTargets();
	}

	private void InitAttackSummary(bool useBothLists = false)
	{
		if (IsAttackAbilityWithOverrides)
		{
			return;
		}
		m_AttackSummary = new CAttackSummary(this, useBothLists);
		m_TotalTargetSummaries = new List<CAttackSummary.TargetSummary>();
		m_AttackSummary.AutoToggleSingleTargetBonuses();
		m_ModifiedStrength = m_AttackSummary.ModifiedAttackStrength;
		m_NumberTargetsRemaining = NumberTargets + m_AttackSummary.ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		bool isMeleeAttack = IsMeleeAttack;
		m_RangeBuff += m_AttackSummary.ActiveBonusAddRangeBuff;
		if (m_AttackSummary.ActiveBonusAddRangeBuff != 0)
		{
			if (isMeleeAttack)
			{
				if (m_OriginalRange >= m_AttackSummary.ActiveBonusAddRangeBuff + 1)
				{
					Range = m_OriginalRange;
				}
				if (MiscAbilityData == null)
				{
					MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				MiscAbilityData.TreatAsMelee = true;
			}
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (Range < 0 && TargetingActor.Type != CActor.EType.Player)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).AutoConsume();
		}
		m_TotalNumberTargets = m_NumberTargetsRemaining;
	}

	private CAbilityAttack CreateSingleTargetAbilityCopy(CActor target, out CAttackSummary.TargetSummary currentSummary)
	{
		currentSummary = null;
		if (InCalcTargetSummary)
		{
			return null;
		}
		List<CActor> list = ActorsToTarget.Union(ValidActorsInRange).Except(ActorsToIgnore).ToList();
		if (m_AttackSummary != null && target != null && list.Contains(target))
		{
			int targetIndex = 0;
			currentSummary = m_AttackSummary.FindTarget(target, ref targetIndex);
			if (currentSummary != null)
			{
				CAbilityAttack cAbilityAttack = null;
				cAbilityAttack = ((currentSummary.ItemOverrideAbility == null) ? (CAbility.CopyAbility(this, generateNewID: false, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: true, VarsContext) as CAbilityAttack) : (CAbility.CopyAbility(currentSummary.ItemOverrideAbility, generateNewID: false, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: true, VarsContext) as CAbilityAttack));
				cAbilityAttack.AppliedEnhancements = AppliedEnhancements;
				cAbilityAttack.m_AugmentsAdded = m_AugmentsAdded;
				cAbilityAttack.m_AugmentOverridesProcessed = m_AugmentOverridesProcessed;
				cAbilityAttack.m_AugmentAbilitiesProcessed = m_AugmentAbilitiesProcessed;
				cAbilityAttack.m_SongOverridesProcessed = m_SongOverridesProcessed;
				cAbilityAttack.m_SongAbilitiesProcessed = m_SongAbilitiesProcessed;
				cAbilityAttack.m_ActiveSingleTargetActiveBonuses = m_ActiveSingleTargetActiveBonuses.ToList();
				cAbilityAttack.m_ActiveSingleTargetItems = m_ActiveSingleTargetItems.ToList();
				cAbilityAttack.StatIsBasedOnXEntries.AddRange(StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == EAbilityStatType.Strength));
				cAbilityAttack.SingleTargetActorsToTarget = list?.ToList();
				cAbilityAttack.AbilityStartListenersInvoked = AbilityStartListenersInvoked;
				cAbilityAttack.OriginalStrength = OriginalStrength;
				foreach (CConditionalOverride conditionalOverride in ConditionalOverrides)
				{
					if (conditionalOverride.AbilityOverrides == null)
					{
						continue;
					}
					foreach (CAbilityOverride abilityOverride2 in conditionalOverride.AbilityOverrides)
					{
						if (abilityOverride2.SubAbilities == null)
						{
							continue;
						}
						foreach (CAbility sub in abilityOverride2.SubAbilities)
						{
							cAbilityAttack.SubAbilities.RemoveAll((CAbility x) => x.ID == sub.ID);
						}
					}
				}
				List<CAbility> allActionAbilities = new List<CAbility>();
				List<CAbility> previouslyFoundAbilities = new List<CAbility>();
				allActionAbilities.Add(cAbilityAttack);
				previouslyFoundAbilities.Add(cAbilityAttack);
				AbilityConsume.GetAllActionAbilitiesRecursive(ref allActionAbilities, ref previouslyFoundAbilities);
				foreach (CActionAugmentation item in PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.Concat(PhaseManager.CurrentPhaseAction.ActionAugmentationsConsumed))
				{
					foreach (CActionAugmentationOp augmentationOp in item.AugmentationOps)
					{
						if (augmentationOp.AbilityOverride == null)
						{
							continue;
						}
						CAbilityOverride abilityOverride = FindOverride(augmentationOp.AbilityOverride);
						if (abilityOverride != null)
						{
							CAbility cAbility = allActionAbilities.FirstOrDefault((CAbility x) => x.Name == abilityOverride.ParentName);
							if (cAbility != null)
							{
								cAbility.CurrentOverrides.Add(abilityOverride);
								cAbility.UndoOverride(abilityOverride, perform: false);
								cAbility.OverrideAbilityValues(augmentationOp.AbilityOverride, perform: false);
							}
						}
					}
				}
				targetIndex = 0;
				currentSummary = m_AttackSummary.FindTarget(target, ref targetIndex);
				return cAbilityAttack;
			}
		}
		return null;
	}

	private void FinishApplySingleTarget(CAbilityAttack copy, CActor target, CAttackSummary.TargetSummary currentSummary)
	{
		InCalcTargetSummary = true;
		copy.m_ActiveSingleTargetItems = ActiveSingleTargetItems?.ToList();
		copy.Start(TargetingActor, FilterActor);
		InCalcTargetSummary = false;
		int targetIndex = 0;
		CAttackSummary.TargetSummary targetSummary = copy.m_AttackSummary.FindTarget(target, ref targetIndex);
		if (currentSummary != null && targetSummary != null)
		{
			targetSummary.ItemOverrideAbility = copy;
			int num = m_AttackSummary.Targets.IndexOf(currentSummary);
			if (num == -1)
			{
				currentSummary = m_AttackSummary.FindTarget(currentSummary.ActorToAttack);
				num = m_AttackSummary.Targets.IndexOf(currentSummary);
			}
			m_AttackSummary.Targets[num] = targetSummary;
			CAbility attackAbilityWithOverrides = targetSummary.AttackAbilityWithOverrides;
			foreach (CItem activeSingleTargetItem in ActiveSingleTargetItems)
			{
				if (activeSingleTargetItem?.YMLData.Data?.Overrides == null || activeSingleTargetItem.YMLData.Data.Overrides.Count <= 0)
				{
					continue;
				}
				foreach (CAbilityOverride @override in activeSingleTargetItem.YMLData.Data.Overrides)
				{
					if (@override.SubAbilities == null)
					{
						continue;
					}
					CPhaseAction cPhaseAction = PhaseManager.CurrentPhase as CPhaseAction;
					foreach (CAbility subAbility in @override.SubAbilities)
					{
						if (!subAbility.IsInlineSubAbility && ((subAbility.AbilityType != EAbilityType.Push && subAbility.AbilityType != EAbilityType.Pull) || attackAbilityWithOverrides.SubAbilities.Any((CAbility x) => x.ID == subAbility.ID)) && !cPhaseAction.RemainingPhaseAbilities.Concat(cPhaseAction.CurrentPhaseAbilities).Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == subAbility.Name))
						{
							CAbility cAbility = attackAbilityWithOverrides.SubAbilities.FirstOrDefault((CAbility x) => x.ID == subAbility.ID);
							cAbility.ParentAbility = cPhaseAction.CurrentPhaseAbility.m_Ability;
							cAbility.TargetThisActorAutomatically = target;
							if (cPhaseAction.CurrentPhaseAbilities.Count > 1)
							{
								PhaseManager.StartAbilities(cAbility, TargetingActor, stackInsteadOfInject: true);
							}
							else if (cAbility.GetMiscVar("StackNextToCurrent").GetBool())
							{
								cPhaseAction.RemainingPhaseAbilities.Insert(0, new CPhaseAction.CPhaseAbility(cAbility, cPhaseAction.CurrentPhaseAbility.TargetingActor, activeSingleTargetItem, null, activeSingleTargetItem.ID));
							}
							else
							{
								cPhaseAction.RemainingPhaseAbilities.Add(new CPhaseAction.CPhaseAbility(cAbility, cPhaseAction.CurrentPhaseAbility.TargetingActor, activeSingleTargetItem, null, activeSingleTargetItem.ID));
							}
						}
					}
				}
			}
		}
		CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(TargetingActor);
		cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = TargetingActor;
		cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = this;
		cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = AttackSummary?.Copy();
		ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
	}

	public override bool IsValidSingleTargetActiveBonusTarget(CActor target)
	{
		if (target == null)
		{
			return false;
		}
		bool result = true;
		foreach (CActiveBonus activeSingleTargetActiveBonuse in m_ActiveSingleTargetActiveBonuses)
		{
			if (activeSingleTargetActiveBonuse.SingleTarget == null)
			{
				if (activeSingleTargetActiveBonuse.IsValidSingleTarget(target))
				{
					return true;
				}
				result = false;
			}
		}
		return result;
	}

	public bool ApplySingleTargetForSpecificItem(CActor target, CItem item)
	{
		CAbilityAttack cAbilityAttack = CreateSingleTargetAbilityCopy(target, out var currentSummary);
		if (cAbilityAttack == null)
		{
			return false;
		}
		if (!ActiveSingleTargetItems.Contains(item))
		{
			ActiveSingleTargetItems.Add(item);
		}
		item.SingleTarget = target;
		FinishApplySingleTarget(cAbilityAttack, target, currentSummary);
		LogEvent(ESESubTypeAbility.AbilityApplySingleTargetItem);
		if (m_ActorsToTarget.Count > 0)
		{
			TargetingActor.Inventory.HighlightUsableItems(this, m_ActorsToTarget, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
		}
		return true;
	}

	public override bool ApplySingleTargetItem(CActor target)
	{
		if (m_ActiveSingleTargetItems.Count == 0)
		{
			return false;
		}
		CAbilityAttack cAbilityAttack = CreateSingleTargetAbilityCopy(target, out var currentSummary);
		if (cAbilityAttack == null)
		{
			return false;
		}
		bool flag = false;
		List<CItem> list = m_ActiveSingleTargetItems.Where((CItem x) => x.SingleTarget == null || x.SingleTarget == target).ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			CItem cItem = list[num];
			if (!cItem.YMLData.Data.CompareAbility.CompareAbility(cAbilityAttack, target) || (cItem.YMLData.Data.ItemRequirements != null && !cItem.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(target, cAbilityAttack, null, cItem)))
			{
				list.Remove(cItem);
			}
		}
		foreach (CItem item in list)
		{
			item.SingleTarget = target;
			if (item?.YMLData.Data?.Overrides != null && item.YMLData.Data.Overrides.Count > 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			FinishApplySingleTarget(cAbilityAttack, target, currentSummary);
		}
		LogEvent(ESESubTypeAbility.AbilityApplySingleTargetItem);
		if (m_ActorsToTarget.Count > 0)
		{
			TargetingActor.Inventory.HighlightUsableItems(this, m_ActorsToTarget, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
		}
		return flag;
	}

	public override void ApplySingleTargetActiveBonus(CActor target)
	{
		if (m_ActiveSingleTargetActiveBonuses.Count == 0)
		{
			return;
		}
		CAbilityAttack cAbilityAttack = CreateSingleTargetAbilityCopy(target, out var currentSummary);
		if (cAbilityAttack == null)
		{
			return;
		}
		bool flag = false;
		foreach (CActiveBonus item in m_ActiveSingleTargetActiveBonuses.Where((CActiveBonus x) => x.SingleTarget == null || x.SingleTarget == target))
		{
			if (item.SingleTarget != null || item.IsValidSingleTarget(target))
			{
				flag = true;
				item.SingleTarget = target;
			}
		}
		if (flag)
		{
			FinishApplySingleTarget(cAbilityAttack, target, currentSummary);
		}
		LogEvent(ESESubTypeAbility.AbilityApplySingleTargetActiveBonus);
		if (m_ActorsToTarget.Count > 0)
		{
			TargetingActor.Inventory.HighlightUsableItems(this, m_ActorsToTarget, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
		}
	}

	public override void ActiveBonusToggled(CActor actor, CActiveBonus activeBonus)
	{
		if (m_AttackSummary != null)
		{
			if (CurrentAttackingTargetSummary != null)
			{
				m_AttackSummary.RefreshingTargetDataAfterToggleItemOrActiveBonus(this, actor, CurrentAttackingTargetSummary);
			}
			else if (actor.Equals(TargetingActor) && CanApplyActiveBonusTogglesTo())
			{
				m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, m_AreaEffectLocked || m_ActorsToTarget.Count > 0);
			}
			else
			{
				m_AttackSummary.RefreshingTargetDataForMultipassAttack(this, actor);
			}
			CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(TargetingActor);
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = TargetingActor;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = this;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = m_AttackSummary?.Copy();
			ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
		}
	}

	public override CAbilityOverride OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null, CActor targetActor = null, bool fromModifier = false)
	{
		if (m_ChainAttack && abilityOverride.Range.HasValue)
		{
			if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
			{
				m_ChainAttackRange = abilityOverride.Range.Value;
			}
			else
			{
				m_ChainAttackRange += abilityOverride.Range.Value;
			}
		}
		return base.OverrideAbilityValues(abilityOverride, perform, item, filter, targetActor, fromModifier);
	}

	public override void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		if (m_ChainAttack && abilityOverride.Range.HasValue)
		{
			if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
			{
				m_ChainAttackRange = abilityOverride.OriginalAbility.Range;
			}
			else
			{
				m_ChainAttackRange -= abilityOverride.Range.Value;
			}
		}
		base.UndoOverride(abilityOverride, perform, item);
	}

	public override string GetDescription()
	{
		if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			if (Range > 0 || NumberTargets > 0)
			{
				return "AttackBuff (" + Strength + ") R: " + Range + " N: " + NumberTargets;
			}
			return "AttackBuff (" + Strength + ")";
		}
		if (Range > 0 || NumberTargets > 0)
		{
			return "Attack(" + Strength + ") R: " + Range + " N: " + NumberTargets + GetTargetAbilityDescription() + GetAreaEffectDescription();
		}
		return "Attack(" + Strength + ")" + GetTargetAbilityDescription() + GetAreaEffectDescription();
	}

	private bool ProcessAbilityAugments()
	{
		List<CAbility> list = new List<CAbility>();
		foreach (CAugment abilityAugment in m_AbilityAugments)
		{
			list.AddRange(abilityAugment.Abilities);
		}
		if (list.Count > 0)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(list, TargetingActor, killAfter: false, stackToNextCurrent: true);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			return true;
		}
		return false;
	}

	public static void SetNextAttackValueOverride(int value)
	{
		s_AttackValueOverride = value;
	}

	private int CalcModifiedStrength(CActor target)
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack);
		int num = Strength;
		foreach (CActiveBonus item in list)
		{
			num += item.ReferenceStrength(this, target);
		}
		return num;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(GetLogEvent(subTypeAbility));
	}

	public SEventAbilityAttack GetLogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<string> list = null;
		List<bool> list2 = null;
		List<int> list3 = null;
		if (m_TotalTargetSummaries != null && m_TotalTargetSummaries.Count > 0)
		{
			list = new List<string>();
			list2 = new List<bool>();
			list3 = new List<int>();
			foreach (CAttackSummary.TargetSummary totalTargetSummary in m_TotalTargetSummaries)
			{
				bool item = totalTargetSummary.OverallAdvantage == EAdvantageStatuses.Advantage;
				list.Add(totalTargetSummary.ActorToAttack.Class.ID);
				list2.Add(item);
				list3.Add(totalTargetSummary.AttackAbilityWithOverrides?.ModifiedStrength() ?? totalTargetSummary.OriginalAbility?.Strength ?? 0);
			}
		}
		return new SEventAbilityAttack(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, strength: CalcModifiedStrength(ActorsToTarget?.FirstOrDefault()), defaultAction: IsDefaultAttack, isBuff: ActiveBonusData.HasBehaviour()), State, AttackEffects.ToList(), IsTargetedAbility, AreaEffect != null || HadAreaEffect, list, list2, list3);
	}

	public bool HasPassedState(EAttackState attackState)
	{
		return m_State > attackState;
	}

	public override bool HasCondition(CCondition.ENegativeCondition condition, bool ignoreSubAbilities, CActor target = null)
	{
		if (base.HasCondition(condition, ignoreSubAbilities, target))
		{
			return true;
		}
		if (TargetingActor != null)
		{
			foreach (CActiveBonus item in (from w in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Attack)
				where w.Ability.Augment == null
				select w).ToList())
			{
				if (item.Ability.ActiveBonusData.IsToggleBonus && !item.ToggledBonus)
				{
					continue;
				}
				if (item.Ability.ActiveBonusData.IsSingleTargetBonus && item.Ability.ActiveBonusData.IsToggleBonus)
				{
					if (item.SingleTarget == null || item.SingleTarget != target)
					{
						continue;
					}
				}
				else if (!item.WillBeValidOnReachTargetActor(this, target))
				{
					continue;
				}
				if (item.Ability.NegativeConditions.ContainsKey(condition))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool HasCondition(CCondition.EPositiveCondition condition, bool ignoreSubAbilities = false)
	{
		if (base.HasCondition(condition, ignoreSubAbilities))
		{
			return true;
		}
		if (TargetingActor != null)
		{
			foreach (CActiveBonus item in (from w in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Attack)
				where w.Ability.Augment == null
				select w).ToList())
			{
				if (item.Ability.PositiveConditions.ContainsKey(condition))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsTargetOutsideOfInitialRange(CActor targetActor)
	{
		List<Point> list = ScenarioManager.PathFinder.FindPath(TargetingActor, TargetingActor.ArrayIndex, targetActor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var _, IgnoreDoorLocks);
		if (m_AttackSummary.ActiveBonusAddRangeBuff != 0 && list.Count > Range - m_AttackSummary.ActiveBonusAddRangeBuff)
		{
			return true;
		}
		return false;
	}

	public bool CheckModifierAbilities(CAttackSummary.TargetSummary tSummary = null, bool nonMultiPassAttack = false)
	{
		if (!m_ModifiersChecked || !m_InlineModifiersChecked)
		{
			bool result = false;
			TargetingActor.Inventory.HandleUsedItems();
			if (tSummary != null)
			{
				if (tSummary.AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack && cAbilityAttack.ModifierAbilities.Count > 0)
				{
					if (ScenarioManager.Scenario.HasActor(TargetingActor))
					{
						ProcessModifierAbilities(cAbilityAttack.ModifierAbilities, nonMultiPassAttack, tSummary.ActorToAttack, tSummary);
					}
					cAbilityAttack.ModifierAbilities.Clear();
					result = true;
				}
			}
			else
			{
				List<CAttackSummary.TargetSummary> list = new List<CAttackSummary.TargetSummary>();
				list.AddRange(m_AttackSummary.Targets.Where((CAttackSummary.TargetSummary x) => x.IsTargeted));
				foreach (CAttackSummary.TargetSummary item in list)
				{
					if (item != null && item.AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack2 && cAbilityAttack2.ModifierAbilities.Count > 0)
					{
						if (ScenarioManager.Scenario.HasActor(TargetingActor))
						{
							ProcessModifierAbilities(cAbilityAttack2.ModifierAbilities, nonMultiPassAttack, item.ActorToAttack, item);
						}
						cAbilityAttack2.ModifierAbilities.Clear();
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	private void ProcessModifierAbilities(List<AttackModifierAbilityInfo> modifierAbilities, bool nonMultiPassAttack, CActor targetActor, CAttackSummary.TargetSummary targetSummary)
	{
		if (TargetingActor.IsMirrorActor())
		{
			return;
		}
		m_IsInProcessModifierAbilities = true;
		CAbility attackAbilityWithOverrides = targetSummary.AttackAbilityWithOverrides;
		foreach (AttackModifierAbilityInfo modifierAbility in modifierAbilities)
		{
			CAbility cAbility = CAbility.CopyAbility(modifierAbility.Ability, generateNewID: false);
			CVars cVars = cAbility.AddTargetSummaryContext(targetSummary).obj as CVars;
			cVars.Set("AttackSummaryModInfo", new TValue(modifierAbility));
			CActor targetingActor = modifierAbility.TargetingActor;
			CAbilityControlActor cAbilityControlActor = null;
			if (modifierAbility.ControlTargetingActor)
			{
				cAbilityControlActor = CAbility.CopyAbility(controlModifierAbility, generateNewID: true) as CAbilityControlActor;
			}
			switch (cAbility.AbilityType)
			{
			case EAbilityType.Consume:
			case EAbilityType.Infuse:
			case EAbilityType.ModeTransfer:
				cVars.Set("TargetActor", new TValue(targetActor));
				cAbility.ParentAbility = this;
				cAbility.MiscAbilityData.TreatAsNonSubAbility = true;
				(PhaseManager.CurrentPhase as CPhaseAction).InjectAbility(cAbility, targetingActor, GetAbilityBaseCard());
				break;
			case EAbilityType.Push:
			case EAbilityType.Pull:
				if (!m_InlineModifiersChecked)
				{
					cAbility.IsInlineSubAbility = true;
					List<CAbility> list = new List<CAbility> { cAbility };
					attackAbilityWithOverrides.MergeOrAddAbilityOfType(cAbility.AbilityType, list);
					attackAbilityWithOverrides.SubAbilities.AddRange(list);
				}
				break;
			case EAbilityType.Attack:
			case EAbilityType.Heal:
			case EAbilityType.Damage:
			case EAbilityType.Shield:
			case EAbilityType.Retaliate:
			case EAbilityType.Trap:
			case EAbilityType.Strengthen:
			case EAbilityType.RefreshItemCards:
			case EAbilityType.AddActiveBonus:
			case EAbilityType.NullTargeting:
			case EAbilityType.Choose:
			case EAbilityType.NullHex:
			case EAbilityType.Brittle:
			case EAbilityType.CreateTerrain:
			case EAbilityType.Teleport2:
			case EAbilityType.MoveActiveBonusTracker:
			case EAbilityType.CreateLimitedHazardousTerrain:
				if (m_InlineModifiersChecked)
				{
					break;
				}
				if (cAbilityControlActor != null)
				{
					cAbilityControlActor.ControlActorData = new CAbilityControlActor.ControlActorAbilityData();
					cAbilityControlActor.ControlActorData.ControlAbilities = new List<CAbility> { cAbility };
					cAbilityControlActor.ControlActorData.UseControllingActorPlayerControl = TargetingActor is CPlayerActor || TargetingActor is CHeroSummonActor;
					cAbilityControlActor.ControlActorData.UseControllingActorType = true;
					cAbilityControlActor.SetMiscVar("CreateNewId", false);
					cAbility = cAbilityControlActor;
				}
				cVars.Set("TargetActor", new TValue(targetActor));
				cAbility.ParentAbility = this;
				m_State = EAttackState.CheckForPlayerIdle;
				if (targetingActor != TargetingActor)
				{
					PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, targetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
					break;
				}
				cAbility.IsInlineSubAbility = true;
				if (cAbility.MiscAbilityData == null)
				{
					cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				cAbility.MiscAbilityData.TreatAsNonSubAbility = true;
				attackAbilityWithOverrides.SubAbilities.Add(cAbility);
				break;
			case EAbilityType.Kill:
			case EAbilityType.ControlActor:
				if (!m_InlineModifiersChecked)
				{
					if (cAbilityControlActor != null)
					{
						cAbilityControlActor.ControlActorData = new CAbilityControlActor.ControlActorAbilityData();
						cAbilityControlActor.ControlActorData.ControlAbilities = new List<CAbility> { cAbility };
						cAbilityControlActor.ControlActorData.UseControllingActorPlayerControl = TargetingActor is CPlayerActor || TargetingActor is CHeroSummonActor;
						cAbilityControlActor.ControlActorData.UseControllingActorType = true;
						cAbility = cAbilityControlActor;
					}
					cVars.Set("TargetActor", new TValue(targetActor));
					cAbility.ParentAbility = this;
					m_State = EAttackState.CheckForPlayerIdle;
					PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, targetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
				}
				break;
			default:
				Log.ErrorWithReport("Unsupported attackAbility modifier ability type being processed");
				break;
			}
		}
		m_IsInProcessModifierAbilities = false;
	}

	public override void Reset()
	{
		base.Reset();
		s_AttackValueOverride = int.MaxValue;
		s_AttackModifierCardOverride = null;
		s_LastAttacker = null;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count > 0;
	}

	public override bool InitValueFromVars(string key, TValue expressionValue, TValue value)
	{
		if (key == "CanClearTargets")
		{
			m_CanClearTargets = value.GetBool();
			return true;
		}
		return base.InitValueFromVars(key, expressionValue, value);
	}

	public override void ApplyEnhancement(List<CEnhancement> enhancements)
	{
		if (!IsAttackAbilityWithOverrides)
		{
			base.ApplyEnhancement(enhancements);
		}
	}

	protected override void ResetBuffs()
	{
		base.ResetBuffs();
		TreatAsRangeBuff = false;
	}

	private void StartAttackingActors()
	{
		ApplyRedirects();
		PhaseManager.CurrentPhaseAction?.AutoConsume();
		if (!m_AugmentAbilitiesProcessed && m_AbilityAugments.Count > 0)
		{
			if (m_MultiPassAttack)
			{
				if (m_AugmentAbilitiesNextTarget)
				{
					m_AugmentAbilitiesNextTarget = false;
				}
				else
				{
					if (m_AugmentAbilitiesTargetCount == 0)
					{
						m_AugmentAbilitiesTargetCount = m_MultiPassActors.Count;
					}
					m_AugmentAbilitiesTargetCount--;
					if (m_AugmentAbilitiesTargetCount <= 0)
					{
						m_AugmentAbilitiesProcessed = true;
					}
					m_AugmentAbilitiesNextTarget = true;
					if (!ProcessAbilityAugments())
					{
						m_AugmentAbilitiesProcessed = true;
					}
				}
			}
			else
			{
				m_AugmentAbilitiesProcessed = true;
				bool flag = false;
				for (int i = 0; i < ActorsToTarget.Count; i++)
				{
					flag |= ProcessAbilityAugments();
				}
			}
		}
		if (TargetingActor.Type == CActor.EType.Player)
		{
			TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			if (!m_MultiPassAttack || !m_MultiPassAttackStarted)
			{
				ScenarioRuleClient.FirstAbilityStarted();
			}
		}
		if (m_MultiPassAttack && (m_MultiPassActors == null || m_MultiPassActors.Count == 0))
		{
			m_MultiPassActors = new List<CActor>(m_ActorsToTarget);
		}
		if (m_MultiPassAttack && m_MultiPassActors.Count > 0)
		{
			m_MultiPassAttackStarted = true;
			m_CurrentPassAttackIndex = 0;
			m_AttackedActors.Clear();
			m_AttackedActors.Add(m_MultiPassActors[0]);
			m_MultiPassActors.Remove(m_MultiPassActors[0]);
		}
		else
		{
			m_AttackedActors = m_ActorsToTarget.ToList();
		}
		if (m_AttackedActors.Count > 0)
		{
			if (RedirectedActors.Count > 0)
			{
				new List<CActor>();
			}
			ScenarioRuleClient.MessageHandler(new CResetAttackEvents_MessageData(TargetingActor));
			ScenarioRuleClient.MessageHandler(new CClearActorsTargetedForVFX_MessageData(TargetingActor));
			for (int j = 0; j < m_AttackedActors.Count; j++)
			{
				CActor cActor = m_AttackedActors[j];
				int targetIndex = 0;
				CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(cActor, ref targetIndex);
				if (targetSummary != null && targetSummary.State == CAttackSummary.TargetSummary.SummaryState.FinishedDrawingModifiers)
				{
					continue;
				}
				if (m_PlacesShadowTokens && (!(cActor is CObjectActor { AttachedProp: var attachedProp }) || (attachedProp != null && attachedProp.HasInitiative)))
				{
					if (TargetingActor.Type == CActor.EType.Player || TargetingActor.Type == CActor.EType.HeroSummon)
					{
						CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.OverrideAbility);
						if (!targetSummary.ShadowMarked)
						{
							GameState.ShowActionConfirmationPanel(TargetingActor, cActor, this, "ShadowMarkTarget");
							if (ShadowMarkTarget)
							{
								foreach (CAttackSummary.TargetSummary target in m_AttackSummary.Targets)
								{
									target.ShadowMarked = false;
								}
							}
							targetSummary.ShadowMarked = ShadowMarkTarget;
						}
						ShadowMarkTarget = false;
					}
					else
					{
						targetSummary.ShadowMarked = true;
						ShadowMarkTarget = false;
					}
				}
				CActorIsAttacking_MessageData cActorIsAttacking_MessageData = new CActorIsAttacking_MessageData(TargetingActor);
				cActorIsAttacking_MessageData.m_AttackAbility = this;
				cActorIsAttacking_MessageData.m_AttackingActor = TargetingActor;
				cActorIsAttacking_MessageData.m_ActorBeingAttacked = cActor;
				cActorIsAttacking_MessageData.m_ActorsAttackingAll = m_AttackedActors;
				cActorIsAttacking_MessageData.m_TargetSummary = targetSummary;
				ScenarioRuleClient.MessageHandler(cActorIsAttacking_MessageData);
			}
			DrawModifiers();
		}
		else
		{
			m_State = EAttackState.FinaliseAttack;
			Perform();
		}
		TValue tValue = ((MiscVars != null) ? MiscVars.Find(((string, TValue) x) => x.Item1 == "OnActorIsAttacking").Item2 : TValue.Unknown);
		if (tValue != TValue.Unknown)
		{
			tValue.Calc(new CVarsContext(this));
		}
	}

	private void ApplyRedirects()
	{
		if (TotalTargetSummaries.Any((CAttackSummary.TargetSummary x) => x.State == CAttackSummary.TargetSummary.SummaryState.FinishedDrawingModifiers))
		{
			return;
		}
		List<CActor> list = new List<CActor>(m_ActorsToTarget);
		for (int num = 0; num < m_ActorsToTarget.Count; num++)
		{
			if (m_ActorsToTargetModifiedStrength.Count > num)
			{
				continue;
			}
			CActor cActor = list[num];
			int targetIndex = 0;
			CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(cActor, ref targetIndex);
			if (targetSummary != null && targetSummary.State == CAttackSummary.TargetSummary.SummaryState.FinishedDrawingModifiers)
			{
				continue;
			}
			CVars cVars = new CVars();
			cVars.Set("ActorToAttack", new TValue(cActor));
			CAbilityAttack cAbilityAttack = targetSummary?.AttackAbilityWithOverrides as CAbilityAttack;
			if (cAbilityAttack == null)
			{
				cAbilityAttack = this;
			}
			else
			{
				cVars.Set("ParentAttack", new TValue(this));
			}
			cVars.Set("TargetSummary", new TValue(targetSummary));
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				GameState.ShowItemSelection(playerActor, TargetingActor, cAbilityAttack, CItem.EItemTrigger.OnAttackedBeforeAttackModifiers, onlyInActionPhase: true, killAfter: false, cVars);
			}
			List<CActor> list2 = ScenarioManager.Scenario.AllActors.Where((CActor actor) => actor.FindApplicableActiveBonuses(EAbilityType.Redirect).Count() > 0).ToList();
			CActor actorBeingAttacked = m_ActorsToTarget[num];
			foreach (CActor item in list2)
			{
				CActor cActor2 = item.ApplyRedirector(this, cActor);
				if (cActor2 == cActor)
				{
					continue;
				}
				RedirectedActors.Add((cActor2, actorBeingAttacked));
				m_ActorsToTarget[num] = cActor2;
				if (m_AttackSummary != null)
				{
					CAttackSummary cAttackSummary = m_AttackSummary.Copy();
					cAttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
					CAttackSummary.TargetSummary targetSummary2 = m_AttackSummary.Targets.FirstOrDefault((CAttackSummary.TargetSummary x) => x.ActorToAttack == actorBeingAttacked && x.RedirectedFromActor == null);
					if (targetSummary2 != null)
					{
						int index = m_AttackSummary.Targets.IndexOf(targetSummary2);
						CAttackSummary.TargetSummary targetSummary3 = cAttackSummary.Targets[index];
						targetSummary3.RedirectedFromActor = actorBeingAttacked;
						targetSummary3.Parent = m_AttackSummary;
						m_AttackSummary.Targets[index] = targetSummary3;
					}
				}
			}
		}
	}

	private void DrawModifiers()
	{
		if (m_AttackedActors.Count <= 0)
		{
			return;
		}
		AbilityHasHappened = true;
		int targetIndex = 0;
		int num = 0;
		for (int i = 0; i < m_AttackedActors.Count; i++)
		{
			CActor cActor = m_AttackedActors[i];
			CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(cActor, ref targetIndex);
			if (targetSummary == null)
			{
				Log.ErrorWithReport("Error: Unable to find res summary for actor " + cActor.GetPrefabName());
			}
			else
			{
				if (targetSummary.State == CAttackSummary.TargetSummary.SummaryState.FinishedDrawingModifiers)
				{
					continue;
				}
				if (m_ActorsToTargetModifiedStrength.Count > i)
				{
					targetIndex++;
					continue;
				}
				if (TargetingActor.m_OnAttackingListeners != null)
				{
					TargetingActor.m_OnAttackingListeners?.Invoke(this, cActor);
				}
				if (cActor.m_OnBeingAttackedPreDamageListeners != null)
				{
					cActor.m_OnBeingAttackedPreDamageListeners?.Invoke(this);
				}
				foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in PositiveConditions)
				{
					if (!targetSummary.AttackAbilityWithOverrides.PositiveConditions.ContainsKey(positiveCondition.Key))
					{
						targetSummary.AttackAbilityWithOverrides.PositiveConditions.Add(positiveCondition.Key, positiveCondition.Value);
					}
				}
				foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in NegativeConditions)
				{
					if (!targetSummary.AttackAbilityWithOverrides.NegativeConditions.ContainsKey(negativeCondition.Key))
					{
						targetSummary.AttackAbilityWithOverrides.NegativeConditions.Add(negativeCondition.Key, negativeCondition.Value);
					}
				}
				if (TargetingActor is CHeroSummonActor { SummonData: var summonData } cHeroSummonActor && summonData?.OnAttackConditions != null)
				{
					foreach (CCondition.ENegativeCondition condition in cHeroSummonActor.SummonData.OnAttackConditions)
					{
						if (!targetSummary.AttackAbilityWithOverrides.NegativeConditions.ContainsKey(condition))
						{
							EAbilityType abilityType = CAbility.AbilityTypes.Single((EAbilityType x) => x.ToString() == condition.ToString());
							targetSummary.AttackAbilityWithOverrides.NegativeConditions.Add(condition, CAbility.CreateAbility(abilityType, targetSummary.AttackAbilityWithOverrides.AbilityFilter, targetSummary.AttackAbilityWithOverrides.IsMonsterAbility, targetSummary.AttackAbilityWithOverrides.IsTargetedAbility));
						}
					}
				}
				if (targetSummary.AttackersGainDisadvantage)
				{
					List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(cActor, EAbilityType.AttackersGainDisadvantage);
					if (list.Count > 0)
					{
						foreach (CAttackersGainDisadvantageActiveBonus item in list)
						{
							item.GainDisadvantageTriggered();
						}
					}
					list = CActiveBonus.FindApplicableActiveBonuses(cActor, EAbilityType.Shield).FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffShield, CActiveBonus.EActiveBonusBehaviourType.ShieldAndDisadvantage, CActiveBonus.EActiveBonusBehaviourType.ShieldAndRetaliate)).ToList();
				}
				bool flag = false;
				if (targetSummary.AttackersGainDisadvantage && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disadvantage))
				{
					TargetingActor.Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disadvantage, 0, EConditionDecTrigger.None, TargetingActor);
					flag = true;
				}
				int addTargetsDrawn = 0;
				CurrentAttackingTargetSummary = targetSummary;
				targetSummary.DrawAttackModifiers(TargetingActor, targetSummary, (CAbilityAttack)targetSummary.AttackAbilityWithOverrides, Math.Max(0, targetIndex - 1), out addTargetsDrawn);
				num += addTargetsDrawn;
				if (flag)
				{
					TargetingActor.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Disadvantage);
				}
				TargetingActor.RemovePositiveConditionToken(CCondition.EPositiveCondition.Advantage, EConditionDecTrigger.First);
				if (targetSummary.Poison)
				{
					CPoisonTriggered_MessageData message = new CPoisonTriggered_MessageData(TargetingActor)
					{
						m_PoisonedActor = cActor,
						m_AttackingActor = TargetingActor
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				_ = targetSummary.ExploitedVulnerability;
				m_ActorsToTargetModifiedStrength.Add(targetSummary.FinalAttackStrength);
			}
		}
		foreach (CActor attackedActor in m_AttackedActors)
		{
			if (!m_ActorsToTargetLocked.Contains(attackedActor))
			{
				m_ActorsToTargetLocked.Add(attackedActor);
			}
		}
		if (AreaEffect != null || AreaEffectBackup != null)
		{
			num += m_TargetCountBuff;
		}
		AddPendingExtraTargets(num);
	}

	public void AddExtraTargets(int val)
	{
		if (AreaEffect != null)
		{
			AddPendingExtraTargets(val);
		}
		else
		{
			BaseTargetCount += val;
		}
	}

	public void AddPendingExtraTargets(int val)
	{
		if (!AutoTrigger || TargetingActor is CPlayerActor)
		{
			PendingExtraTargetsToSelect += val;
		}
	}

	private void StartAddExtraTargets()
	{
		ActorsToIgnore.AddRange(TotalTargetSummaries.Select((CAttackSummary.TargetSummary x) => x.ActorToAttack));
		ActorsToTarget.Clear();
		m_AttackedActors.Clear();
		if (!HadAreaEffect)
		{
			HadAreaEffect = AreaEffect != null;
		}
		AreaEffect = null;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		foreach (CActor item in ActorsToIgnore)
		{
			ValidActorsInRange.Remove(item);
		}
		if (ValidActorsInRange.Count > 0)
		{
			m_TargetCountBuff = 0;
			m_ActorsToTargetModifiedStrength.Clear();
			m_CurrentPassAttackIndex = 0;
			m_CurrentAttackIndex = 0;
			AttackSummary = new CAttackSummary(this);
			m_TotalNumberTargets = PendingExtraTargetsToSelect;
			int numberTargetsRemaining = (NumberTargets = (m_UndoNumberTargetsRemaining = PendingExtraTargetsToSelect));
			m_NumberTargetsRemaining = numberTargetsRemaining;
			PendingExtraTargetsToSelect = 0;
			m_State = EAttackState.SelectAttackFocusAdditionalTargets;
			Perform();
		}
		else
		{
			PendingExtraTargetsToSelect = 0;
			m_State = EAttackState.AttackDone;
			Perform();
		}
	}
}
