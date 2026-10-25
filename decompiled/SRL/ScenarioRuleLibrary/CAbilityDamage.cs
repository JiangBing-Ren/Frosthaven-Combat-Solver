using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityDamage : CAbility
{
	public enum EDamageState
	{
		PreDamageBuffTargeting,
		ApplyDamageBuff,
		SelectDamageFocus,
		ActorHasDamaged,
		SelectActorToDamage,
		ActorBeenDamaged,
		NextActorCheck,
		InlineSubAbilityCheck,
		DamageDone
	}

	private EDamageState m_State;

	private CActor m_ActorBeingDamaged;

	private CAttackSummary m_DamageSummary;

	public bool m_CanClearTargets = true;

	public int m_CappedMinHealh;

	public List<CAttackSummary.TargetSummary> TargetsDamagedSummaries = new List<CAttackSummary.TargetSummary>();

	public CAttackSummary DamageSummary => m_DamageSummary;

	public CAttackSummary.TargetSummary CurrentDamagingTargetSummary { get; set; }

	public static CAbility CreateDefaultDamage(int strength, int range, int numberOfTargets, bool isMonster, string animationOverload = "", bool isMultipass = true, CAbilityFilterContainer abilityfilter = null, string name = "DefaultDamage")
	{
		CAbilityFilterContainer abilityFilter = abilityfilter ?? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
		List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
		bool multiPassAttack = isMultipass;
		string empty = string.Empty;
		bool isMonster2 = isMonster;
		string empty2 = string.Empty;
		List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
		CAbilityRequirements startAbilityConditions = new CAbilityRequirements();
		AbilityData.MiscAbilityData miscAbilityData = new AbilityData.MiscAbilityData();
		return CAbility.CreateAbility(parentName: string.Empty, miscAbilityData: miscAbilityData, skipAnim: false, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: new List<AbilityData.StatIsBasedOnXData>(), moveObstacleRange: 0, moveObstacleAnimOverload: string.Empty, slotsToRefresh: new List<CItem.EItemSlot>(), slotStatesToRefresh: new List<CItem.EItemSlotState>(), addActiveBonusAbility: null, overrideAugmentAttackType: EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: new List<EAbilityType>(), cardAction: ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), abilityType: EAbilityType.Damage, strength: strength, useSpecialBaseStat: false, range: range, numberTargets: numberOfTargets, tokens: 0, flags: EAbilityFlag.None, abilityFilter: abilityFilter, animationOverload: animationOverload, animationOverload2: null, subAbilities: null, areaEffect: null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, aiFocusOverride: null, moveRestrictionType: CAbilityMove.EMoveRestrictionType.None, activeBonusData: null, activeBonusYML: null, areaEffectYMLString: "", areaEffectLayoutOverrideYMLString: "", areaEffectFlipAxis: "", name: name, abilityEnhancements: abilityEnhancements, positiveConditions: null, negativeConditions: null, multiPassAttack: multiPassAttack, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: EAbilityTargeting.Range, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: isMonster2, propName: empty2, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 1, isTargetedAbility: true, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: conditionalOverrides, startAbilityConditions: startAbilityConditions, abilityXP: 0, consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		CAbilityDamage obj = destAbility as CAbilityDamage;
		obj.m_State = m_State;
		obj.m_ActorBeingDamaged = m_ActorBeingDamaged;
		obj.m_DamageSummary = m_DamageSummary;
		obj.TargetsDamagedSummaries = TargetsDamagedSummaries.ToList();
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		return m_ValidActorsInRange.Count > 0;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		m_CappedMinHealh = GetMiscVar("MinHealth").GetInt();
		base.Start(targetActor, filterActor, controllingActor);
		bool flag = false;
		if (ParentAbility?.ActorsTargeted != null && AbilityFilter.Equals(ParentAbility.AbilityFilter) && IsInlineAndUsesInlineTargeting && (InlineSubAbilityTiles == null || InlineSubAbilityTiles.Count == 0))
		{
			InlineSubAbilityTiles = new List<CTile>();
			foreach (CActor item in ParentAbility.ActorsTargeted.ToList())
			{
				InlineSubAbilityTiles.Add(ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y]);
			}
		}
		if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = EDamageState.PreDamageBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(TargetingActor);
		}
		else if (UseSubAbilityTargeting || TargetingActor.Type != CActor.EType.Player || (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value))
		{
			m_State = EDamageState.ActorHasDamaged;
			if (UseSubAbilityTargeting)
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.UseParentTargets == true)
				{
					if (ParentAbility?.ActorsTargeted != null)
					{
						if (AbilityFilter.Equals(ParentAbility.AbilityFilter))
						{
							ValidActorsInRange = ParentAbility.ActorsTargeted.ToList();
						}
						else
						{
							ValidActorsInRange = new List<CActor>();
							foreach (CActor item2 in ParentAbility.ActorsTargeted)
							{
								if (AbilityFilter.IsValidTarget(item2, filterActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
								{
									ValidActorsInRange.Add(item2);
								}
							}
						}
					}
					RemoveImmuneActorsFromList(ref m_ValidActorsInRange, onlyCheck: false);
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
					NumberTargets = ValidActorsInRange.Count;
					flag = true;
				}
			}
		}
		else
		{
			m_State = EDamageState.SelectDamageFocus;
		}
		InitDamageSummary();
		m_ActorBeingDamaged = null;
		if (Targeting == EAbilityTargeting.Custom)
		{
			AreaEffect = null;
		}
		if (AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], AreaEffect, 0f, flipped: false, AreaEffectFlipAxis, GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true), ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks);
		}
		if (!AbilityHasBeenCancelled)
		{
			if (NumberTargets == -1 || AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || AllTargets)
			{
				m_AllTargets = true;
				if (UseSubAbilityTargeting || IsInlineAndUsesInlineTargeting)
				{
					m_ValidActorsInRange = new List<CActor>();
					TilesInRange.Clear();
					if (IsInlineAndUsesInlineTargeting)
					{
						if (InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
						{
							List<CTile> list = new List<CTile>();
							if (TargetThisActorAutomatically != null)
							{
								list.Add(TargetThisActorAutomatically.Tile);
							}
							else
							{
								list.AddRange(InlineSubAbilityTiles);
							}
							AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
							if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.HasValue)
							{
								AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
								if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentTargets.Value)
								{
									foreach (CTile item3 in list)
									{
										CActor cActor = ScenarioManager.Scenario.FindActorAt(item3.m_ArrayIndex);
										if (cActor != null)
										{
											m_ActorsToIgnore.Add(cActor);
										}
										TilesInRange.AddRange(GameState.GetTilesInRange(item3.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
										m_ValidActorsInRange.AddRange(GameState.GetActorsInRange(item3.m_ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible));
									}
									TilesInRange = TilesInRange.Distinct().ToList();
									m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
									goto IL_0ae3;
								}
							}
							TilesInRange = list.ToList();
							foreach (CTile item4 in list)
							{
								CActor cActor2 = ScenarioManager.Scenario.FindActorAt(item4.m_ArrayIndex);
								if (cActor2 != null && !m_ValidActorsInRange.Contains(cActor2) && AbilityFilter.IsValidTarget(cActor2, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, false))
								{
									m_ValidActorsInRange.Add(cActor2);
								}
							}
						}
						goto IL_0ae3;
					}
					AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentMovePath.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
						if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentMovePath.Value && ParentAbility is CAbilityMove)
						{
							List<CTile> list2 = new List<CTile>();
							for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
							{
								if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
								{
									Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
									CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
									list2.Add(cTile);
									if (cTile != null)
									{
										TilesInRange.AddRange(GameState.GetTilesInRange(cTile.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
									}
								}
							}
							TilesInRange = TilesInRange.Distinct().ToList();
							foreach (CTile item5 in TilesInRange)
							{
								if (item5 != null)
								{
									CActor actorOnTile = GameState.GetActorOnTile(item5, TargetingActor, AbilityFilter, m_ActorsToIgnore, IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
									if (actorOnTile != null && !m_ValidActorsInRange.Contains(actorOnTile))
									{
										m_ValidActorsInRange.Add(actorOnTile);
									}
								}
							}
							goto IL_0ae3;
						}
					}
					if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
					{
						if (ParentAbility.AreaEffect != null)
						{
							TilesInRange.AddRange(ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
						}
						else
						{
							AbilityData.MiscAbilityData miscAbilityData6 = MiscAbilityData;
							if (miscAbilityData6 != null && miscAbilityData6.AllTargetsAdjacentToParentTargets.HasValue)
							{
								AbilityData.MiscAbilityData miscAbilityData7 = MiscAbilityData;
								if (miscAbilityData7 != null && miscAbilityData7.AllTargetsAdjacentToParentTargets.Value)
								{
									List<CTile> list3 = new List<CTile>();
									if (ParentAbility.ActorsTargeted.Count > 0)
									{
										list3.AddRange(ParentAbility.ActorsTargeted.Select((CActor x) => x.Tile));
									}
									else
									{
										list3.AddRange(ParentAbility.TilesSelected);
									}
									foreach (CTile item6 in list3)
									{
										CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item6.m_ArrayIndex);
										if (cActor3 != null)
										{
											m_ActorsToIgnore.Add(cActor3);
										}
										TilesInRange.AddRange(GameState.GetTilesInRange(item6.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: true));
									}
									TilesInRange = TilesInRange.Distinct().ToList();
									goto IL_0969;
								}
							}
							TilesInRange.AddRange(ParentAbility.TilesSelected);
						}
						goto IL_0969;
					}
					goto IL_0acd;
				}
				if (AllTargetsInRangeAlongPath)
				{
					SharedAbilityTargeting.GetValidActorsInRangeAlongPath(this, CAbilityMove.s_AllArrayIndicesAlongPath, ref m_ValidActorsInRange);
				}
				else
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				NumberTargets = m_ValidActorsInRange.Count;
				m_ActorsToTarget = m_ValidActorsInRange.ToList();
			}
			else if (!flag)
			{
				if (AllTargetsInRangeAlongPath)
				{
					SharedAbilityTargeting.GetValidActorsInRangeAlongPath(this, CAbilityMove.s_AllArrayIndicesAlongPath, ref m_ValidActorsInRange);
					NumberTargets = m_ValidActorsInRange.Count;
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
				}
				else
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
			}
		}
		goto IL_0bd7;
		IL_0bd7:
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (TargetingActor.Type == CActor.EType.Player && !IsPositive() && (MiscAbilityData == null || !MiscAbilityData.NegativeAbilityIsOptional.HasValue || !MiscAbilityData.NegativeAbilityIsOptional.Value) && m_ValidActorsInRange.Count > 0 && (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally) || AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) || AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion)))
		{
			foreach (CActor item7 in m_ValidActorsInRange)
			{
				if (CActor.AreActorsAllied(TargetingActor.Type, item7.Type, TargetingActor, item7) || item7 == TargetingActor)
				{
					m_CanSkip = false;
					break;
				}
			}
		}
		if (Strength <= 0 && !StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => CAbility.TargetSpecificStatIsBasedOnXTypes.Contains(x.BasedOn) && x.AbilityStatType == EAbilityStatType.Strength))
		{
			SetCancelAbilityFlag(cancel: true);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
		return;
		IL_0969:
		AbilityData.MiscAbilityData miscAbilityData8 = MiscAbilityData;
		if (miscAbilityData8 != null && miscAbilityData8.UseParentTiles.HasValue)
		{
			AbilityData.MiscAbilityData miscAbilityData9 = MiscAbilityData;
			if (miscAbilityData9 != null && miscAbilityData9.UseParentTiles.Value)
			{
				foreach (CTile item8 in TilesInRange)
				{
					CActor cActor4 = ScenarioManager.Scenario.FindActorAt(item8.m_ArrayIndex);
					if (cActor4 != null && AbilityFilter.IsValidTarget(cActor4, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						m_ValidActorsInRange.Add(cActor4);
					}
				}
				goto IL_0acd;
			}
		}
		foreach (CTile item9 in TilesInRange.ToList())
		{
			CActor cActor5 = ScenarioManager.Scenario.FindActorAt(item9.m_ArrayIndex);
			if (cActor5 != null && AbilityFilter.IsValidTarget(cActor5, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
			{
				m_ValidActorsInRange.Add(cActor5);
			}
		}
		goto IL_0acd;
		IL_0acd:
		m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
		goto IL_0ae3;
		IL_0ae3:
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) && !m_ValidActorsInRange.Contains(TargetingActor))
		{
			m_ValidActorsInRange.Add(TargetingActor);
		}
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange, onlyCheck: false);
		NumberTargets = m_ValidActorsInRange.Count;
		m_ActorsToTarget = m_ValidActorsInRange.ToList();
		goto IL_0bd7;
	}

	private void UpdateDamageSummary(bool useCurrentTargets)
	{
		int targetIndex = 0;
		DamageSummary.UpdateTargetData(this, ActiveSingleTargetItems, ActiveSingleTargetActiveBonuses, useCurrentTargets);
		CurrentDamagingTargetSummary = (useCurrentTargets ? DamageSummary.FindTarget(m_ActorBeingDamaged, ref targetIndex) : null);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0089;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0089;
				}
			}
		}
		switch (m_State)
		{
		case EDamageState.PreDamageBuffTargeting:
		{
			if (TargetingActor.Type != CActor.EType.Player || (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value))
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EDamageState.ApplyDamageBuff:
		{
			ScenarioRuleClient.FirstAbilityStarted();
			AbilityHasHappened = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				ApplyToActor(item);
			}
			CDamageBuff_MessageData message = new CDamageBuff_MessageData(AnimOverload, TargetingActor)
			{
				m_DamageAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.NextStep();
			return true;
		}
		case EDamageState.SelectDamageFocus:
		{
			if (UseSubAbilityTargeting && m_ValidActorsInRange.Count == 0 && AreaEffect == null)
			{
				if (TargetingActor.Type == CActor.EType.Player)
				{
					TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				PhaseManager.NextStep();
				return true;
			}
			UpdateDamageSummary(useCurrentTargets: false);
			CActorIsSelectingDamageFocus_MessageData cActorIsSelectingDamageFocus_MessageData = new CActorIsSelectingDamageFocus_MessageData(AnimOverload, TargetingActor);
			cActorIsSelectingDamageFocus_MessageData.m_DamageSelf = AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true);
			cActorIsSelectingDamageFocus_MessageData.m_DamageAbility = this;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingDamageFocus_MessageData);
			break;
		}
		case EDamageState.ActorHasDamaged:
			m_CanUndo = false;
			if (Targeting == EAbilityTargeting.Custom && (TargetingActor.Type != CActor.EType.Player || (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)))
			{
				m_ActorsToTarget = m_ValidActorsInRange;
			}
			else
			{
				if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.UseParentTiles.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
						if (miscAbilityData3 != null && miscAbilityData3.UseParentTiles.Value)
						{
							m_ActorsToTarget = m_ValidActorsInRange;
							goto IL_03cd;
						}
					}
				}
				if (AreaEffect != null)
				{
					m_ActorsToTarget = GetValidActorsInArea(m_ValidTilesInAreaEffectIncludingBlocked);
				}
				else if (m_OriginalTargetCount == -1 || TargetingActor.Type != CActor.EType.Player)
				{
					List<CActor> list2 = m_ValidActorsInRange.ToList();
					list2.Sort((CActor x, CActor y) =>
					{
						int num = SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(x, TargetingActor);
						int num2 = SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(y, TargetingActor);
						if (num == -1)
						{
							num = int.MaxValue;
						}
						if (num2 == -1)
						{
							num2 = int.MaxValue;
						}
						return (num != num2) ? num.CompareTo(num2) : GameState.s_ActorInitiativeComparer.Compare(x, y);
					});
					m_ActorsToTarget = list2.Take(NumberTargets).ToList();
					m_NumberTargetsRemaining = 0;
				}
			}
			goto IL_03cd;
		case EDamageState.SelectActorToDamage:
			if (m_ActorsToTarget.Count == 0)
			{
				AbilityHasHappened = true;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				PhaseManager.NextStep();
				return true;
			}
			m_ActorBeingDamaged = m_ActorsToTarget[0];
			UpdateDamageSummary(useCurrentTargets: true);
			m_State = EDamageState.ActorBeenDamaged;
			Perform();
			break;
		case EDamageState.ActorBeenDamaged:
			if (m_ActorBeingDamaged != null)
			{
				PlayActorBeenDamagedVFX();
				if (!ActorsTargeted.Contains(m_ActorBeingDamaged))
				{
					if (TargetingActor.Type == CActor.EType.Player)
					{
						TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
						if (!m_AbilityHasHappened)
						{
							ScenarioRuleClient.FirstAbilityStarted();
						}
					}
					AbilityHasHappened = true;
					ActorsTargeted.Add(m_ActorBeingDamaged);
					if (ResourcesToTakeFromTargets != null && ResourcesToTakeFromTargets.Count > 0)
					{
						foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in ResourcesToTakeFromTargets)
						{
							if (m_ActorBeingDamaged.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
							{
								m_ActorBeingDamaged.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
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
								m_ActorBeingDamaged.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
							}
						}
					}
					_ = m_ActorBeingDamaged.Health;
					bool actorWasAsleep = m_ActorBeingDamaged.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					int strength = CurrentDamagingTargetSummary.AttackAbilityWithOverrides.Strength;
					if (XpPerTargetData != null && TargetingActor != null)
					{
						TargetingActor.GainXP(XpPerTargetData.AttackTarget(TargetingActor, m_ActorBeingDamaged), XPSourceKey);
					}
					TargetsDamagedSummaries.Add(CurrentDamagingTargetSummary);
					foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
					{
						scenarioScript.TickBeingDamaged(this, m_ActorBeingDamaged, strength, null);
					}
					GameState.ActorBeenDamaged(m_ActorBeingDamaged, strength, checkIfPlayerCanAvoidDamage: true, TargetingActor, this, AbilityType, 0, isTrapDamage: false, isTerrainDamage: false, cannotPreventDamageWithActiveBonus: false, pierceInvulnerable: false, 0, triggerOnDamageListeners: true, m_CappedMinHealh);
					if (ScenarioManager.Scenario.HasActor(m_ActorBeingDamaged))
					{
						if (GameState.ActorHealthCheck(TargetingActor, m_ActorBeingDamaged, out var onDeathAbility, isTrap: false, isTerrain: false, actorWasAsleep))
						{
							m_ActorsToIgnore.Add(m_ActorBeingDamaged);
						}
						if (onDeathAbility)
						{
							if (ScenarioRuleClient.MessageQueueCopy.Any((ScenarioRuleClient.CSRLMessage x) => x.m_MessageType == ScenarioRuleClient.EMessageType.STEP_COMPLETE))
							{
								return false;
							}
							if (AbilityHasBeenCancelled && AbilityInterruptCopy != null)
							{
								PhaseManager.NextStep();
							}
							else
							{
								PhaseManager.StepComplete();
							}
							return true;
						}
					}
				}
				m_State = EDamageState.NextActorCheck;
				Perform();
			}
			else
			{
				m_State = EDamageState.SelectActorToDamage;
				Perform();
			}
			break;
		case EDamageState.NextActorCheck:
			if (m_ActorBeingDamaged != null)
			{
				m_ActorsToTarget.Remove(m_ActorBeingDamaged);
				if (m_NegativeConditions.Count > 0)
				{
					ProcessNegativeStatusEffects(m_ActorBeingDamaged);
				}
				if (m_PositiveConditions.Count > 0)
				{
					ProcessPositiveStatusEffects(m_ActorBeingDamaged);
				}
			}
			m_NumberTargetsRemaining = m_ActorsToTarget.Count;
			if (m_NumberTargetsRemaining == 0)
			{
				if (m_ActorBeingDamaged != null)
				{
					CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = new CPlayerWaitForIdle_MessageData(TargetingActor);
					cPlayerWaitForIdle_MessageData.m_Actor = m_ActorBeingDamaged;
					ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData);
					return true;
				}
				PhaseManager.StepComplete();
				return true;
			}
			m_State = EDamageState.SelectActorToDamage;
			Perform();
			break;
		case EDamageState.InlineSubAbilityCheck:
			if (SubAbilities.Count > 0 && SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
			{
				List<CAbility> list = new List<CAbility>();
				foreach (CAbility item2 in SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					foreach (CActor item3 in ActorsTargeted)
					{
						if (item2.MiscAbilityData == null || !item2.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item2.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || item3.IsDead)
						{
							CAbility cAbility = CAbility.CopyAbility(item2, generateNewID: true, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
							cAbility.ParentAbility = this;
							cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[item3.ArrayIndex.X, item3.ArrayIndex.Y]);
							cAbility.TargetThisActorAutomatically = item3;
							list.Add(cAbility);
						}
					}
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: false, TargetingActor);
				m_State = EDamageState.DamageDone;
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case EDamageState.DamageDone:
			{
				PhaseManager.NextStep();
				return true;
			}
			IL_03cd:
			ExcludeMirroredHealthClones(ActorsToTarget);
			if (AnimOverload.Length == 0 && (UseSubAbilityTargeting || TargetingActor.Type != CActor.EType.Player || (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)))
			{
				m_State = EDamageState.SelectActorToDamage;
				Perform();
			}
			else
			{
				CActorHasDamaged_MessageData message2 = new CActorHasDamaged_MessageData(AnimOverload, TargetingActor)
				{
					m_DamageAbility = this
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			break;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player && (ProcessIfDead || !TargetingActor.IsDead))
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsStunned_MessageData message4 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	private void PlayActorBeenDamagedVFX()
	{
		List<(string, TValue)> list = MiscVars?.Where(((string, TValue) v) => v.Item1.StartsWith("PlayActorBeenDamagedVFX")).ToList();
		if (list == null)
		{
			return;
		}
		foreach (var item in list)
		{
			item.Item2.Calc(new CVarsContext(this));
		}
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (m_State == EDamageState.SelectDamageFocus && !AllTargets)
		{
			if (AreaEffect != null)
			{
				bool flag2 = false;
				List<CActor> validActorsInArea = GetValidActorsInArea(ValidTilesInAreaEffectedIncludingBlocked);
				if (validActorsInArea.Count > 0)
				{
					flag2 = true;
					m_AreaEffectLocked = true;
					ValidActorsInRange = validActorsInArea;
				}
				if (!flag2)
				{
					base.TileSelected(selectedTile, optionalTileList);
					LogEvent(ESESubTypeAbility.AbilityTileSelected);
					return;
				}
			}
			SharedAbilityTargeting.TileSelected(this, selectedTile, AreaEffect, TargetingActor, FilterActor, Range, ref m_ValidActorsInRange, m_ActorsToIgnore, AbilityFilter, NumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, IsTargetedAbility);
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		base.TileDeselected(selectedTile, optionalTileList);
		bool flag = false;
		if (!AllTargets)
		{
			if (AreaEffect != null && m_AreaEffectLocked)
			{
				m_AreaEffectLocked = false;
			}
			else if (SharedAbilityTargeting.TileDeselected(this, selectedTile, m_NumberTargetsRemaining, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
			{
				flag = true;
			}
		}
		if (flag)
		{
			Perform();
		}
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void Restart()
	{
		Start(TargetingActor, FilterActor, ControllingActor);
	}

	public override bool CanClearTargets()
	{
		if (m_AllTargets)
		{
			return false;
		}
		if (m_CanClearTargets)
		{
			return m_State == EDamageState.SelectDamageFocus;
		}
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		if (!base.CanReceiveTileSelection() || m_State != EDamageState.SelectActorToDamage)
		{
			return m_State == EDamageState.SelectDamageFocus;
		}
		return true;
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

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == EDamageState.SelectDamageFocus)
		{
			Perform();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EDamageState.DamageDone;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(GetLogEvent(subTypeAbility));
	}

	public SEventAbilityDamage GetLogEvent(ESESubTypeAbility subTypeAbility)
	{
		return new SEventAbilityDamage(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State);
	}

	public override string GetDescription()
	{
		if (Range > 0 || NumberTargets > 0)
		{
			return "Damage(" + Strength + ") R: " + Range + " N: " + NumberTargets;
		}
		return "Damage(" + Strength + ")";
	}

	private void InitDamageSummary()
	{
		if (GameState.CurrentSelectAndPlayCardAbility != null && GetAbilityBaseCard().CurrentCardPile == CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase)
		{
			int num = GameState.CurrentSelectAndPlayCardAbility.GetMiscVar("ReduceNextDamage").GetInt();
			Strength = Math.Max(0, Strength - num);
		}
		m_DamageSummary = new CAttackSummary(this);
		m_ModifiedStrength = m_DamageSummary.ModifiedAttackStrength;
		m_NumberTargetsRemaining = NumberTargets + m_DamageSummary.ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		Range += m_DamageSummary.ActiveBonusAddRangeBuff;
		if (m_DamageSummary.ActiveBonusAddRangeBuff != 0)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
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
}
