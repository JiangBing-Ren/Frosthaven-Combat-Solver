using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityControlActor : CAbilityTargeting
{
	public enum EControlDurationType
	{
		None,
		ControlForOneAction,
		ControlForOneTurn
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class ControlActorAbilityData
	{
		[JsonProperty]
		public List<CAbility> ControlAbilities;

		[JsonProperty]
		public EControlDurationType ControlDurationType;

		[JsonProperty]
		public CActor.EType? ControlType;

		[JsonProperty]
		public bool TransferAugments;

		[JsonProperty]
		public bool TreatControlAbilityParentAsParent;

		[JsonProperty]
		public bool TreatPreviousAbilityToControlAbilityAsParent;

		[JsonProperty]
		public CAbilityFilter UseControllingActorModifierDeckFilter;

		[JsonProperty]
		public bool UseControllingActorModifierDeck;

		[JsonProperty]
		public bool UseControllingActorPlayerControl;

		[JsonProperty]
		public bool ChooseClosestActor;

		[JsonProperty]
		public bool UseControllingActorType;

		public ControlActorAbilityData()
		{
			ControlAbilities = new List<CAbility>();
			ControlDurationType = EControlDurationType.ControlForOneAction;
			ControlType = CActor.EType.Player;
			UseControllingActorModifierDeckFilter = CAbilityFilter.CreateDefaultFilter();
			UseControllingActorType = false;
		}

		public ControlActorAbilityData Copy()
		{
			List<CAbility> list = new List<CAbility>();
			foreach (CAbility controlAbility in ControlAbilities)
			{
				list.Add(CAbility.CopyAbility(controlAbility, generateNewID: false));
			}
			return new ControlActorAbilityData
			{
				ControlAbilities = list,
				ControlDurationType = ControlDurationType,
				ControlType = ControlType,
				TransferAugments = TransferAugments,
				TreatControlAbilityParentAsParent = TreatControlAbilityParentAsParent,
				TreatPreviousAbilityToControlAbilityAsParent = TreatPreviousAbilityToControlAbilityAsParent,
				UseControllingActorModifierDeckFilter = UseControllingActorModifierDeckFilter.Copy(),
				UseControllingActorModifierDeck = UseControllingActorModifierDeck,
				UseControllingActorPlayerControl = UseControllingActorPlayerControl,
				ChooseClosestActor = ChooseClosestActor,
				UseControllingActorType = UseControllingActorType
			};
		}

		public ControlActorAbilityData Merge(ControlActorAbilityData dataToMerge)
		{
			List<CAbility> list = new List<CAbility>();
			foreach (CAbility controlAbility in ControlAbilities)
			{
				list.Add(CAbility.CopyAbility(controlAbility, generateNewID: false));
			}
			return new ControlActorAbilityData
			{
				ControlAbilities = list.Concat(dataToMerge.ControlAbilities).ToList(),
				ControlDurationType = ControlDurationType,
				ControlType = ControlType,
				TransferAugments = (TransferAugments || dataToMerge.TransferAugments),
				TreatControlAbilityParentAsParent = (TreatControlAbilityParentAsParent || dataToMerge.TreatControlAbilityParentAsParent),
				TreatPreviousAbilityToControlAbilityAsParent = (TreatPreviousAbilityToControlAbilityAsParent || dataToMerge.TreatPreviousAbilityToControlAbilityAsParent),
				UseControllingActorModifierDeckFilter = dataToMerge.UseControllingActorModifierDeckFilter.Copy(),
				UseControllingActorModifierDeck = (UseControllingActorModifierDeck || dataToMerge.UseControllingActorModifierDeck),
				UseControllingActorPlayerControl = (UseControllingActorPlayerControl || dataToMerge.UseControllingActorPlayerControl),
				ChooseClosestActor = (ChooseClosestActor || dataToMerge.ChooseClosestActor),
				UseControllingActorType = (UseControllingActorType || dataToMerge.UseControllingActorType)
			};
		}
	}

	public static readonly EControlDurationType[] ControlDurationTypes = (EControlDurationType[])Enum.GetValues(typeof(EControlDurationType));

	[JsonProperty]
	public ControlActorAbilityData ControlActorData { get; set; }

	public CAbilityControlActor(ControlActorAbilityData controlActorAbilityData)
		: base(EAbilityType.ControlActor)
	{
		ControlActorData = controlActorAbilityData;
		m_OneTargetAtATime = true;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (AbilityFilter.AbilityFilters.All((CAbilityFilter x) => x.FilterEnemy == CAbilityFilter.EFilterEnemy.None))
		{
			foreach (CAbilityFilter abilityFilter in AbilityFilter.AbilityFilters)
			{
				abilityFilter.FilterEnemy = CAbilityFilter.EFilterEnemy.Normal | CAbilityFilter.EFilterEnemy.Elite | CAbilityFilter.EFilterEnemy.Boss;
			}
		}
		base.Start(targetActor, filterActor, controllingActor);
		SetHasStarted(val: true);
	}

	public static CAbility CreateDefaultControl(int range, int numberOfTargets, bool isMonster, string animationOverload = "", CAbilityFilterContainer abilityFilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.ControlActor, 0, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, abilityFilter ?? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), animationOverload, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultControl", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		if (actorsAppliedTo.Count == 0 || actorsAppliedTo.Contains(actorApplying))
		{
			return true;
		}
		actorsAppliedTo = ((!OneTargetAtATime || m_ActorsToIgnore.Count != 0) ? actorsAppliedTo.Where((CActor w) => !m_ActorsToIgnore.Contains(w)).ToList() : new List<CActor> { actorsAppliedTo[0] });
		CActorIsApplyingControlActor message = new CActorIsApplyingControlActor(AnimOverload, actorApplying)
		{
			m_ActorsAppliedTo = actorsAppliedTo,
			m_ControlActorAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		bool flag = true;
		if (GetAbilityBaseCard() is CAbilityCard { CardType: CBaseCard.ECardType.CharacterAbility })
		{
			flag = false;
		}
		if (ControlActorData.ControlDurationType.Equals(EControlDurationType.ControlForOneAction))
		{
			if (ControlActorData.ControlAbilities != null && ControlActorData.ControlAbilities.Count > 0 && base.ApplyToActor(actor))
			{
				AbilityHasHappened = true;
				actor.MindControlDuration = EControlDurationType.ControlForOneAction;
				actor.MindControlIsFromEnemy = !CActor.AreActorsAllied(actor.Type, TargetingActor.Type, actor, TargetingActor);
				actor.MindControlAbility = this;
				List<CAbility> list = new List<CAbility>();
				if (actor is CHeroSummonActor cHeroSummonActor)
				{
					foreach (CAbility controlAbility in ControlActorData.ControlAbilities)
					{
						CAbility cAbility = cHeroSummonActor.ApplyBaseStats(controlAbility, copyCurrentOverrides: false, overrideAttackZero: false, flag);
						if (cAbility != null)
						{
							cAbility.IsControlAbility = true;
							cAbility.ControlledByClassID = TargetingActor.Class.ID;
							if (GetMiscVar("TreatThisAsParent").GetBool())
							{
								cAbility.ParentAbility = this;
								cAbility.IsSubAbility = true;
							}
							else if (ControlActorData.TreatControlAbilityParentAsParent)
							{
								cAbility.ParentAbility = ParentAbility;
								cAbility.IsSubAbility = true;
							}
							else if (ControlActorData.TreatPreviousAbilityToControlAbilityAsParent && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.PreviousPhaseAbilities.Count > 0)
							{
								cAbility.ParentAbility = cPhaseAction.PreviousPhaseAbilities.Last().m_Ability;
								cAbility.IsSubAbility = true;
							}
							list.Add(cAbility);
						}
					}
				}
				else if (actor is CPlayerActor)
				{
					foreach (CAbility controlAbility2 in ControlActorData.ControlAbilities)
					{
						CAbility cAbility2 = CAbility.CopyAbility(controlAbility2, GetMiscVar("CreateNewId").GetBool(null, flag));
						cAbility2.IsControlAbility = true;
						cAbility2.ControlledByClassID = TargetingActor.Class.ID;
						if (GetMiscVar("TreatThisAsParent").GetBool())
						{
							cAbility2.ParentAbility = this;
							cAbility2.IsSubAbility = true;
						}
						else if (ControlActorData.TreatControlAbilityParentAsParent)
						{
							cAbility2.ParentAbility = ParentAbility;
							cAbility2.IsSubAbility = true;
						}
						else if (ControlActorData.TreatPreviousAbilityToControlAbilityAsParent && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2 && cPhaseAction2.PreviousPhaseAbilities.Count > 0)
						{
							cAbility2.ParentAbility = cPhaseAction2.PreviousPhaseAbilities.Last().m_Ability;
							cAbility2.IsSubAbility = true;
						}
						list.Add(cAbility2);
					}
				}
				else if (actor is CEnemyActor cEnemyActor)
				{
					foreach (CAbility controlAbility3 in ControlActorData.ControlAbilities)
					{
						CAbility cAbility3 = cEnemyActor.CloneAbilityAndApplyBaseStats(controlAbility3, flag);
						cAbility3.IsControlAbility = true;
						cAbility3.ControlledByClassID = TargetingActor.Class.ID;
						if (GetMiscVar("TreatThisAsParent").GetBool())
						{
							cAbility3.ParentAbility = this;
							cAbility3.IsSubAbility = true;
						}
						else if (ControlActorData.TreatControlAbilityParentAsParent)
						{
							cAbility3.ParentAbility = ParentAbility;
							cAbility3.IsSubAbility = true;
						}
						else if (ControlActorData.TreatPreviousAbilityToControlAbilityAsParent && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction3 && cPhaseAction3.PreviousPhaseAbilities.Count > 0)
						{
							cAbility3.ParentAbility = cPhaseAction3.PreviousPhaseAbilities.Last().m_Ability;
							cAbility3.IsSubAbility = true;
						}
						list.Add(cAbility3);
					}
				}
				foreach (CAbility item in list)
				{
					item.ParentAbilityBaseCard = GetAbilityBaseCard();
					item.AddParentContext(VarsContext);
				}
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction4 && cPhaseAction4.ActionAugmentationsConsumed.Count > 0)
				{
					foreach (CAbility clonedAbility in list)
					{
						foreach (CActionAugmentationOp item2 in cPhaseAction4.ActionAugmentationsConsumed.SelectMany((CActionAugmentation x) => x.AugmentationOps.Where((CActionAugmentationOp y) => y.ParentAbilityName == clonedAbility.Name && y.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)).ToList())
						{
							clonedAbility.OverrideAbilityValues(item2.AbilityOverride, perform: false);
						}
					}
				}
				bool useControllingActorModifierDeck = ControlActorData.UseControllingActorModifierDeck && ControlActorData.UseControllingActorModifierDeckFilter.IsValidTarget(actor, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false);
				GameState.OverrideCurrentActorForOneAction(actor, ControlActorData.UseControllingActorType ? new CActor.EType?(TargetingActor.Type) : ControlActorData.ControlType, killActorAfterAction: false, ControlActorData.TransferAugments ? TargetingActor.Augments : null, TargetingActor, useControllingActorModifierDeck, ControlActorData.UseControllingActorPlayerControl, immediatelySetCurrentActor: true, GetMiscVar("TreatAsNonControl").GetBool());
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, TargetingActor, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: true, actor);
				CActorIsControlled_MessageData message = new CActorIsControlled_MessageData(TargetingActor)
				{
					m_ControlActorAbility = this,
					m_ControlledActor = actor
				};
				ScenarioRuleClient.MessageHandler(message);
				return true;
			}
		}
		else if (ControlActorData.ControlDurationType.Equals(EControlDurationType.ControlForOneTurn) && (ControlActorData.UseControllingActorType || ControlActorData.ControlType.HasValue) && base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			actor.MindControlDuration = EControlDurationType.ControlForOneTurn;
			actor.MindControlIsFromEnemy = !CActor.AreActorsAllied(actor.Type, TargetingActor.Type, actor, TargetingActor);
			actor.MindControlAbility = this;
			GameState.QueueOverrideActorForOneTurn(actor, ControlActorData.UseControllingActorType ? TargetingActor.Type : ControlActorData.ControlType.Value, ControlActorData.ControlAbilities, GetAbilityBaseCard(), TargetingActor, ControlActorData.UseControllingActorPlayerControl);
			if (actor is CPlayerActor cPlayerActor)
			{
				foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
				{
					if (heroSummon.Summoner == cPlayerActor)
					{
						GameState.QueueOverrideActorForOneTurn(heroSummon, ControlActorData.UseControllingActorType ? TargetingActor.Type : ControlActorData.ControlType.Value, null, null, TargetingActor, ControlActorData.UseControllingActorPlayerControl);
					}
				}
			}
			return true;
		}
		return false;
	}

	public override void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (actorApplying.Type == CActor.EType.Player)
		{
			return;
		}
		if (ControlActorData.ChooseClosestActor)
		{
			validActorsInRange.Sort((CActor x, CActor y) =>
			{
				int tileDistance = ScenarioManager.GetTileDistance(actorApplying.ArrayIndex.X, actorApplying.ArrayIndex.Y, x.ArrayIndex.X, x.ArrayIndex.Y);
				int tileDistance2 = ScenarioManager.GetTileDistance(actorApplying.ArrayIndex.X, actorApplying.ArrayIndex.Y, y.ArrayIndex.X, y.ArrayIndex.Y);
				return tileDistance.CompareTo(tileDistance2);
			});
		}
		else if (CachedAIMoveFocusActors != null && CachedAIMoveFocusActors.Count > 0)
		{
			validActorsInRange.Sort((CActor x, CActor y) =>
			{
				int num = CachedAIMoveFocusActors.IndexOf(x);
				int num2 = CachedAIMoveFocusActors.IndexOf(y);
				if (num != num2)
				{
					if (num == -1)
					{
						return 1;
					}
					if (num2 == -1)
					{
						return -1;
					}
					return num.CompareTo(num2);
				}
				return GameState.s_ActorInitiativeComparer.Compare(x, y);
			});
		}
		else
		{
			validActorsInRange.Sort(GameState.s_ActorInitiativeComparer);
		}
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList, bool onlyCheck = true)
	{
		actorList.RemoveAll((CActor x) => x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
		for (int num = actorList.Count - 1; num >= 0; num--)
		{
			CActor actor = actorList[num];
			bool flag = true;
			foreach (CAbility controlAbility in ControlActorData.ControlAbilities)
			{
				if (controlAbility.AbilityType == EAbilityType.Attack || controlAbility.AbilityType == EAbilityType.Damage || !CAbility.ImmuneToAbility(actor, controlAbility, targeting: true))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				actorList.RemoveAt(num);
			}
		}
		base.RemoveImmuneActorsFromList(ref actorList, onlyCheck);
	}

	public override void ClearTargets()
	{
		if (m_TilesSelected == null)
		{
			return;
		}
		List<CTile> optionalTileList = new List<CTile>();
		foreach (CTile item in m_TilesSelected.ToList())
		{
			TileDeselected(item, optionalTileList);
		}
	}

	public override bool IsPositive()
	{
		if (!AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally))
		{
			return AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion);
		}
		return true;
	}

	public override void GetSubAbilities(List<CAbility> outAbilities)
	{
		outAbilities.AddRange(ControlActorData.ControlAbilities);
	}
}
