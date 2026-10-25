using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityLoot : CAbility
{
	public enum EAdditionalLootEffect
	{
		None
	}

	public enum ELootState
	{
		None,
		PreLootBuffTargeting,
		ApplyLootBuff,
		CheckForDelayedDrops,
		CheckToAllowTileSelection,
		SelectLootTiles,
		LootTiles,
		CheckForLootQuestItems,
		LootQuestItems,
		LootDone
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class LootData
	{
		[JsonProperty]
		public List<ScenarioManager.ObjectImportType> LootableObjectImportTypes;

		[JsonProperty]
		public EAdditionalLootEffect AdditionalLootEffect;

		public LootData()
		{
			LootableObjectImportTypes = new List<ScenarioManager.ObjectImportType>();
			AdditionalLootEffect = EAdditionalLootEffect.None;
		}

		public LootData Copy()
		{
			return new LootData
			{
				LootableObjectImportTypes = LootableObjectImportTypes.ToList(),
				AdditionalLootEffect = AdditionalLootEffect
			};
		}

		public static LootData CreateDefaultLootData()
		{
			return new LootData
			{
				LootableObjectImportTypes = new List<ScenarioManager.ObjectImportType>
				{
					ScenarioManager.ObjectImportType.MoneyToken,
					ScenarioManager.ObjectImportType.Chest,
					ScenarioManager.ObjectImportType.GoalChest,
					ScenarioManager.ObjectImportType.CarryableQuestItem,
					ScenarioManager.ObjectImportType.Resource
				}
			};
		}
	}

	public static readonly EAdditionalLootEffect[] AdditionalLootEffects = (EAdditionalLootEffect[])Enum.GetValues(typeof(EAdditionalLootEffect));

	private ELootState m_State;

	private List<CObjectProp> m_PropsLooted;

	private int m_ExtraGoldFromAdditionalEffects;

	private List<CLootActiveBonus_LimitLoot> m_LimitLootActiveBonuses = new List<CLootActiveBonus_LimitLoot>();

	private List<CTile> m_ValidTilesInRange = new List<CTile>();

	[JsonProperty]
	public LootData AbilityLootData { get; set; }

	public int LootCount { get; private set; }

	public int LootTokenCount { get; private set; }

	public List<CObjectProp> PropsLooted => m_PropsLooted;

	public CAbilityLoot(LootData lootData)
	{
		AbilityLootData = lootData;
	}

	public static CAbility CreateDefaultLoot(int range)
	{
		return CAbility.CreateAbility(EAbilityType.Loot, 0, useSpecialBaseStat: false, range, -1, 0, EAbilityFlag.None, new CAbilityFilterContainer(), "Loot", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultLoot", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster: false, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, LootData.CreateDefaultLootData(), new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (TilesInRange == null || TilesInRange.Count == 0)
		{
			return false;
		}
		foreach (CTile item in TilesInRange)
		{
			if (item.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource).Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ELootState.CheckForDelayedDrops;
		m_ExtraGoldFromAdditionalEffects = 0;
		CalcValidTiles();
		m_PropsLooted = new List<CObjectProp>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	private void CalcValidTiles()
	{
		TilesInRange.Clear();
		List<CTile> list = new List<CTile>();
		if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = ELootState.PreLootBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(TargetingActor);
		}
		else if (AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
			{
				if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						list.Add(cTile);
					}
				}
			}
		}
		else
		{
			GetBonuses();
			if (AreaEffect == null)
			{
				list = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
			if (!list.Contains(adjacentTile))
			{
				list.Add(adjacentTile);
			}
		}
		m_ValidTilesInRange = new List<CTile>();
		IList list2 = GetMiscVar("AutoLootTiles").GetObject<IList>();
		if (list2 != null)
		{
			list.Clear();
			foreach (object item2 in list2)
			{
				if (item2 is CTile item)
				{
					list.Add(item);
				}
			}
		}
		foreach (CTile item3 in list)
		{
			if (item3 == TargetingActor.Tile && GetMiscVar("DontIncludeSelfTile").GetBool())
			{
				continue;
			}
			List<CObjectProp> list3 = item3.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource);
			list3.RemoveAll((CObjectProp x) => !AbilityLootData.LootableObjectImportTypes.Contains(x.ObjectType) || !x.CanActorLoot(TargetingActor) || !m_LimitLootActiveBonuses.All((CLootActiveBonus_LimitLoot y) => y.CanLootObject(TargetingActor, x.ObjectType)));
			list3.RemoveAll((CObjectProp x) => x is CObjectResource cObjectResource && !cObjectResource.HasRemainingLoot);
			TilesInRange.Add(item3);
			if (list3.Count > 0)
			{
				m_ValidTilesInRange.Add(item3);
			}
		}
		if (OriginalTargetCount < 0)
		{
			m_AllTargets = OriginalTargetCount == -1;
			NumberTargets = m_ValidTilesInRange.Count;
		}
		else
		{
			m_AllTargets = false;
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
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
		case ELootState.PreLootBuffTargeting:
		{
			if (TargetingActor.Type != CActor.EType.Player || AutoTrigger)
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData2 = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData2.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData2.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData2);
			break;
		}
		case ELootState.ApplyLootBuff:
			ScenarioRuleClient.FirstAbilityStarted();
			AbilityHasHappened = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				ApplyToActor(item);
			}
			PhaseManager.NextStep();
			return true;
		case ELootState.CheckForDelayedDrops:
		{
			CWaitForDelayedDrops_MessageData message2 = new CWaitForDelayedDrops_MessageData();
			ScenarioRuleClient.MessageHandler(message2);
			return true;
		}
		case ELootState.CheckToAllowTileSelection:
			if (TargetingActor.Type == CActor.EType.Player && !AutoTrigger)
			{
				TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				if (m_AllTargets || m_ValidTilesInRange.Count <= NumberTargets)
				{
					m_TilesSelected.AddRange(m_ValidTilesInRange);
				}
				if (m_ValidTilesInRange.Count <= 0 && TargetingActor.Type == CActor.EType.Player)
				{
					GetLootAlly();
					if (ValidActorsInRange.Count > 0)
					{
						m_State = ELootState.CheckForLootQuestItems;
					}
					else
					{
						m_State = ELootState.SelectLootTiles;
					}
				}
				else
				{
					m_State = ELootState.SelectLootTiles;
				}
			}
			else
			{
				m_TilesSelected = m_ValidTilesInRange.ToList();
				m_State = ELootState.LootTiles;
			}
			m_NumberTargetsRemaining = NumberTargets - m_TilesSelected.Count;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			Perform();
			return true;
		case ELootState.SelectLootTiles:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
					m_IsPositive = IsPositive(),
					m_ShowActiveBonusBar = true
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			return true;
		case ELootState.LootTiles:
		{
			CActor targetingActor = TargetingActor;
			CActor cActor = targetingActor;
			CActor cActor2 = GetMiscVar("ForcedFinalLooter").GetObject<CActor>();
			if (cActor2 != null)
			{
				cActor = cActor2;
			}
			if (TargetingActor is CHeroSummonActor cHeroSummonActor)
			{
				cActor = cHeroSummonActor.Summoner;
			}
			CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
			if (mirrorAbilityBehaviour != null && mirrorAbilityBehaviour.MirrorTarget == targetingActor)
			{
				cActor = mirrorAbilityBehaviour.ActiveBonus.Actor;
			}
			if (cActor.Type == CActor.EType.Player)
			{
				cActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			int num = 0;
			LootCount = 0;
			LootTokenCount = 0;
			AbilityHasHappened = true;
			for (int i = 0; i < m_TilesSelected.Count; i++)
			{
				if (num != 0)
				{
					break;
				}
				CTile cTile = m_TilesSelected[i];
				List<CObjectProp> list = ((!(cActor.Class is CCharacterClass)) ? cTile.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken) : cTile.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource));
				LootCount += list.Count;
				LootTokenCount += list.Count((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken);
				if (!m_AllTargets && LootCount >= NumberTargets)
				{
					num = i;
				}
				if (targetingActor.LootTile(cTile, asPartOfAbility: true, endOfTurn: false, ScenarioManager.ObjectImportType.None, this, cActor2))
				{
					m_PropsLooted.AddRange(list);
				}
			}
			if (ScenarioManager.Scenario.HasActor(cActor) && m_PropsLooted != null && m_PropsLooted.Count > 0)
			{
				TargetingActor.ShowLootAnimations(AnimOverload, m_PropsLooted, this, targetingActor);
			}
			else
			{
				PhaseManager.StepComplete();
			}
			return true;
		}
		case ELootState.CheckForLootQuestItems:
			GetLootAlly();
			ActorsToTarget.AddRange(ValidActorsInRange);
			if (ActorsToTarget.Count > 0 && !AutoTrigger)
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_ObjectiveRelated = true;
				cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		case ELootState.LootQuestItems:
			foreach (CActor item2 in ValidActorsInRange)
			{
				foreach (CObjectProp carriedQuestItem in item2.CarriedQuestItems)
				{
					TargetingActor.CarryQuestItem(carriedQuestItem);
					CActivateProp_MessageData cActivateProp_MessageData = new CActivateProp_MessageData(TargetingActor);
					cActivateProp_MessageData.m_Prop = carriedQuestItem;
					cActivateProp_MessageData.m_CarriedByActor = true;
					ScenarioRuleClient.MessageHandler(cActivateProp_MessageData);
				}
				item2.CarriedQuestItems.Clear();
			}
			PhaseManager.NextStep();
			return true;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
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

	public void GetLootAlly()
	{
		ValidActorsInRange.Clear();
		CAbilityFilterContainer cAbilityFilterContainer = CAbilityFilterContainer.CreateDefaultFilter();
		cAbilityFilterContainer.AbilityFilters[0].FilterTargetType = CAbilityFilter.EFilterTargetType.Ally;
		cAbilityFilterContainer.AbilityFilters[0].FilterActorType = CAbilityFilter.EFilterActorType.Player;
		cAbilityFilterContainer.AbilityFilters[0].FilterFlags = CAbilityFilter.EFilterFlags.CarryingQuestItem;
		ValidActorsInRange = GameState.GetActorsInRange(TargetingActor.ArrayIndex, TargetingActor, 1, null, cAbilityFilterContainer, null, null, isTargetedAbility: false, null, false);
		m_ActorsToTarget.Clear();
	}

	public void GetBonuses()
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Loot);
		if (list == null)
		{
			return;
		}
		foreach (CActiveBonus item in list.Where((CActiveBonus x) => !x.HasBehaviour<CLootActiveBonus_LimitLoot>()))
		{
			Range += item.ReferenceStrength(this, TargetingActor);
		}
		foreach (CActiveBonus item2 in list.Where((CActiveBonus x) => !x.HasBehaviour<CLootActiveBonus_LimitLoot>()))
		{
			m_ModifiedStrength *= item2.ReferenceStrengthScalar(this, TargetingActor);
		}
		foreach (CActiveBonus item3 in list)
		{
			if (item3.BespokeBehaviour != null && item3.TryGetBehaviour(out CLootActiveBonus_LimitLoot behaviour))
			{
				m_LimitLootActiveBonuses.Add(behaviour);
			}
		}
	}

	private bool ValidToSelect(CTile tile)
	{
		IList lootTypesCanSelect = GetVar("LootTypesCanSelect").GetObject<IList>();
		if (lootTypesCanSelect != null && tile.m_Props.Where((CObjectProp x) => x is CObjectResource || x is CObjectChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x is CObjectQuestItem).Any((CObjectProp x) => !lootTypesCanSelect.Contains(x.ObjectType.ToString())))
		{
			return false;
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (ValidToSelect(selectedTile) && !TilesSelected.Contains(selectedTile) && m_State == ELootState.SelectLootTiles && m_NumberTargetsRemaining > 0 && m_ValidTilesInRange.Contains(selectedTile))
		{
			List<CObjectProp> list = selectedTile.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource);
			list.RemoveAll((CObjectProp x) => !AbilityLootData.LootableObjectImportTypes.Contains(x.ObjectType));
			if (list != null && list.Count > 0)
			{
				m_TilesSelected.Add(selectedTile);
				m_NumberTargetsRemaining--;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
			}
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!AllTargets && ValidToSelect(selectedTile) && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
			}
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public bool HasPassedState(ELootState lootState)
	{
		return m_State > lootState;
	}

	public override bool CanClearTargets()
	{
		return m_State == ELootState.SelectLootTiles;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ELootState.SelectLootTiles;
		}
		return false;
	}

	public override string GetDescription()
	{
		if (Range > 0)
		{
			return "Loot R: " + Range;
		}
		return "Loot";
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
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ELootState.LootDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = ELootState.CheckToAllowTileSelection;
		m_ExtraGoldFromAdditionalEffects = 0;
		CalcValidTiles();
		m_TilesSelected.Clear();
		m_PropsLooted = new List<CObjectProp>();
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityLoot(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, m_PropsLooted.Select((CObjectProp x) => x.PrefabName).ToList()));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		if (AllowNoTargets())
		{
			return true;
		}
		return m_TilesSelected.Count > 0;
	}
}
