using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilitySelectAndPlayCard : CAbility
{
	public enum SelectAndPlayCardState
	{
		SelectTarget,
		SelectCards,
		PlayCards,
		Done
	}

	public class SelectedCardInfo
	{
		public CAbilityCard card;

		public CBaseCard.ECardPile cardPile;

		public CAction LastAction;

		public bool LastActionWasTop
		{
			get
			{
				Guid iD = card.TopAction.ID;
				Guid? guid = LastAction?.ID;
				if (!(iD == guid))
				{
					iD = card.DefaultAttackAction.ID;
					Guid? guid2 = LastAction?.ID;
					return iD == guid2;
				}
				return true;
			}
		}

		public bool LastActionWasLoss
		{
			get
			{
				if (LastAction != null)
				{
					if (LastAction.CardPile != CBaseCard.ECardPile.Lost)
					{
						return LastAction.CardPile == CBaseCard.ECardPile.PermanentlyLost;
					}
					return true;
				}
				return false;
			}
		}
	}

	[JsonProperty]
	public List<CBaseCard.ECardPile> PilesToChooseFrom;

	[JsonProperty]
	public bool BlockXP;

	[JsonProperty]
	public int MinRequiredCards;

	private CAbilityCard previousRoundCard;

	private int numCardsPlayed;

	private int numCardsSelected;

	private CActor previousActor;

	private GameState.EActionInitiator previousInitiator;

	private CActiveBonus currentTriggeredBonus;

	[JsonIgnore]
	[StateDump(Skip = true)]
	private List<CAbilityCard> initialOutOfPhaseCards;

	public SelectAndPlayCardState m_State;

	[StateDump(Skip = false)]
	private Dictionary<CAbilityCard, SelectedCardInfo> cachedCardPiles = new Dictionary<CAbilityCard, SelectedCardInfo>();

	private CPlayerActor playerActor => TargetingActor as CPlayerActor;

	public CPhase previousPhase { get; private set; }

	public List<CAbilityCard> selectedAbilityCardsOutOfPhase { get; private set; }

	private CAbilityCard AutoSelCard { get; set; }

	public bool IsSelectingOrPlayingCards
	{
		get
		{
			if (m_State != SelectAndPlayCardState.SelectCards)
			{
				return m_State == SelectAndPlayCardState.PlayCards;
			}
			return true;
		}
	}

	public CAbilitySelectAndPlayCard(List<CBaseCard.ECardPile> pilesToChooseFrom, bool blockXP, int minRequiredCards)
	{
		PilesToChooseFrom = pilesToChooseFrom;
		BlockXP = blockXP;
		MinRequiredCards = minRequiredCards;
		Range = 1;
		Targeting = EAbilityTargeting.Range;
	}

	public bool HasValidCards(CActor actor)
	{
		return PilesToChooseFrom.SelectMany(actor.CharacterClass.GetCardPileList).Where(IsValidCard).Any();
	}

	public static CAbility CreateDefaultSelectAndPlayCard(int numCardsToSelect, List<CBaseCard.ECardPile> pilesToChooseFrom, CAbilityFilterContainer abilityFilter = null)
	{
		CAbilitySelectAndPlayCard cAbilitySelectAndPlayCard = (CAbilitySelectAndPlayCard)CAbility.CreateAbility(EAbilityType.SelectAndPlayCard, numCardsToSelect, useSpecialBaseStat: false, 1, 1, 0, EAbilityFlag.None, abilityFilter ?? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), "", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultHeal", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster: false, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
		cAbilitySelectAndPlayCard.PilesToChooseFrom = pilesToChooseFrom;
		return cAbilitySelectAndPlayCard;
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		initialOutOfPhaseCards = TargetingActor.CharacterClass.SelectedAbilityCardsOutOfPhase.ToList();
		m_State = SelectAndPlayCardState.SelectTarget;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			TileSelected(targetingActor.Tile, null);
		}
		SetMiscVar("MinRequiredCards", MinRequiredCards);
		int autoSelCardID = GetVar("AutoSelCardID", VarsContext).GetInt();
		if (autoSelCardID != 0)
		{
			CCharacterClass characterClass = TargetingActor.CharacterClass;
			AutoSelCard = characterClass.AbilityCardsPool.SingleOrDefault((CAbilityCard x) => x.ID == autoSelCardID);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
		previousActor = PhaseManager.CurrentPhaseAction?.ActorInitiator ?? GameState.CurrentActor;
	}

	public override bool Perform()
	{
		base.Perform();
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0079;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0079;
				}
			}
		}
		switch (m_State)
		{
		case SelectAndPlayCardState.SelectTarget:
		{
			if (MiscAbilityData == null)
			{
				MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value && ActorsToTarget.Count > 0)
			{
				m_State = SelectAndPlayCardState.SelectCards;
				Perform();
				return false;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			return false;
		}
		case SelectAndPlayCardState.SelectCards:
		{
			CPlayerActor cPlayerActor = (CPlayerActor)ActorsToTarget[0];
			if (cPlayerActor == null)
			{
				break;
			}
			List<CAbilityCard> list = PilesToChooseFrom.SelectMany(cPlayerActor.CharacterClass.GetCardPileList).Where(IsValidCard).ToList();
			if (list.Count > 0 || AutoSelCard != null)
			{
				cachedCardPiles.Clear();
				foreach (CAbilityCard item in list)
				{
					SelectedCardInfo value = new SelectedCardInfo
					{
						card = item,
						cardPile = item.CurrentCardPile,
						LastAction = item.GetAction(item.LastPlayedActionType)
					};
					cachedCardPiles[item] = value;
				}
				previousInitiator = GameState.CurrentActionInitiator;
				previousPhase = PhaseManager.CurrentPhase;
				previousRoundCard = GetAbilityBaseCard() as CAbilityCard;
				if (PhaseManager.PreviousPhase != null && GameState.CurrentOutOfPhaseCardSelectionAbility == null && (PhaseManager.PreviousPhase.Type == CPhase.PhaseType.EndTurn || PhaseManager.PreviousPhase.Type == CPhase.PhaseType.ActionSelection))
				{
					previousRoundCard = null;
					previousPhase = PhaseManager.PreviousPhase;
				}
				GameState.CurrentOutOfPhaseCardSelectionAbility = this;
				if (AutoSelCard != null)
				{
					GameState.__OutOfPhaseCardFromPiles.Add(AutoSelCard, AutoSelCard.CurrentCardPile);
					CCharacterClass characterClass = cPlayerActor.CharacterClass;
					characterClass.MoveAbilityCard(AutoSelCard, characterClass.GetCardPileList(AutoSelCard.CurrentCardPile), characterClass.SelectedAbilityCardsOutOfPhase, CCharacterClass.GetCardPileListKey(AutoSelCard.CurrentCardPile), "SelectedAbilityCardsOutOfPhase");
					StartOutOfPhaseAbilities();
				}
				else
				{
					CSelectAndPlayCard_MessageData cSelectAndPlayCard_MessageData = new CSelectAndPlayCard_MessageData(AnimOverload, TargetingActor);
					cSelectAndPlayCard_MessageData.m_ActorReplayingCard = TargetingActor;
					cSelectAndPlayCard_MessageData.m_Ability = this;
					cSelectAndPlayCard_MessageData.m_ValidCards = list.Select((CAbilityCard c) => c.ID).ToList();
					cSelectAndPlayCard_MessageData.m_PilesToChooseFrom = PilesToChooseFrom;
					ScenarioRuleClient.MessageHandler(cSelectAndPlayCard_MessageData);
				}
				return true;
			}
			CNoValidCardsForAbility_MessageData message = new CNoValidCardsForAbility_MessageData(TargetingActor, this);
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		case SelectAndPlayCardState.PlayCards:
		case SelectAndPlayCardState.Done:
			return true;
		}
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == this)
		{
			SetCancelAbilityFlag(cancel: true);
			PhaseManager.NextStep();
		}
		return false;
		IL_0079:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public void StartOutOfPhaseAbilities()
	{
		AbilityHasHappened = true;
		currentTriggeredBonus = PhaseManager.CurrentPhaseAction?.CurrentTriggeredActiveBonus;
		CActor cActor = ActorsToTarget[0];
		TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
		GameState.OverrideCurrentActor(cActor);
		m_State = SelectAndPlayCardState.PlayCards;
		Perform();
		selectedAbilityCardsOutOfPhase = playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Except(initialOutOfPhaseCards).ToList();
		List<CAbilityCard> list = selectedAbilityCardsOutOfPhase.OrderBy((CAbilityCard x) => x.ID).ToList();
		List<KeyValuePair<CAbilityCard, CBaseCard.ECardPile>> enumerable = GameState.__OutOfPhaseCardFromPiles.OrderBy((KeyValuePair<CAbilityCard, CBaseCard.ECardPile> x) => x.Key.ID).ToList();
		foreach (CAbilityCard item in selectedAbilityCardsOutOfPhase)
		{
			playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Remove(item);
		}
		playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.AddRange(list);
		foreach (CAbilityCard item2 in list)
		{
			item2.PendingCardPile = CBaseCard.ECardPile.None;
		}
		GameState.__OutOfPhaseCardFromPiles.Clear();
		GameState.__OutOfPhaseCardFromPiles.AddRange(enumerable);
		numCardsSelected = selectedAbilityCardsOutOfPhase.Count;
		GameState.OutOfPhaseActionSelectionFlagStack.Push(GameState.EActionFlag.None);
		PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
		TValue miscVar = GetMiscVar("OverridePosition");
		if (!miscVar.isUnknown)
		{
			CPlayerActor cPlayerActor = cActor as CPlayerActor;
			CTile cTile = miscVar.GetObject<CTile>(VarsContext);
			if (cTile != null)
			{
				cPlayerActor.OverrideArrayIndex = cTile.m_ArrayIndex;
			}
		}
		if (!GetMiscVar("DontCancelTidesOnRest").GetBool())
		{
			return;
		}
		foreach (CAbilityCard item3 in selectedAbilityCardsOutOfPhase)
		{
			foreach (CAbility item4 in (from x in item3.GetAllAbilities()
				where x.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.Tide)
				select x).ToList())
			{
				item4.SetMiscVar("DontCancelTideOnRest", true);
			}
		}
	}

	public void EndOutOfPhaseAbilities()
	{
		GameState.OverrideCurrentActor(previousActor);
		m_State = SelectAndPlayCardState.Done;
		Perform();
		CPlayerActor cPlayerActor = ActorsToTarget[0] as CPlayerActor;
		if (cPlayerActor != null)
		{
			if (!GetMiscVar("OverridePosition").isUnknown)
			{
				cPlayerActor.OverrideArrayIndex = cPlayerActor.RealArrayIndex;
			}
			foreach (CAbilityCard item in cPlayerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Except(initialOutOfPhaseCards).ToList())
			{
				if (!GameState.__OutOfPhaseCardFromPiles.TryGetValue(item, out var value))
				{
					Log.GameplayError("Can't find the source pile for card " + item.Name);
					continue;
				}
				cPlayerActor.CharacterClass.MoveAbilityCard(item, cPlayerActor.CharacterClass.SelectedAbilityCardsOutOfPhase, cPlayerActor.CharacterClass.GetCardPileList(value), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase), CCharacterClass.GetCardPileListKey(value));
				GameState.__OutOfPhaseCardFromPiles.Remove(item);
			}
		}
		if (GameState.OutOfPhaseActionSelectionFlagStack.Count > 0 && GameState.OutOfPhaseActionSelectionFlagStack.Peek() != GameState.EActionFlag.None)
		{
			GameState.OutOfPhaseActionSelectionFlagStack.Pop();
		}
		GameState.CurrentOutOfPhaseCardSelectionAbility = null;
		GameState.__OutOfPhaseCardFromPiles.RemoveWhere((KeyValuePair<CAbilityCard, CBaseCard.ECardPile> x) => !initialOutOfPhaseCards.Contains(x.Key));
		if (GetMiscVar("LoseCardsIfTheyWereLost").GetBool())
		{
			if (selectedAbilityCardsOutOfPhase != null)
			{
				foreach (CAbilityCard item2 in selectedAbilityCardsOutOfPhase)
				{
					bool flag = item2.CurrentCardPile == CBaseCard.ECardPile.Lost || cachedCardPiles[item2].cardPile == CBaseCard.ECardPile.Lost;
					bool flag2 = item2.CurrentCardPile == CBaseCard.ECardPile.PermanentlyLost || cachedCardPiles[item2].cardPile == CBaseCard.ECardPile.PermanentlyLost;
					CBaseCard.ECardPile eCardPile = CBaseCard.ECardPile.Discarded;
					if (item2.CurrentCardPile != CBaseCard.ECardPile.Activated)
					{
						if (flag2)
						{
							eCardPile = CBaseCard.ECardPile.PermanentlyLost;
						}
						else if (flag)
						{
							eCardPile = CBaseCard.ECardPile.Lost;
						}
						if (eCardPile != item2.CurrentCardPile)
						{
							CCharacterClass characterClass = cPlayerActor.CharacterClass;
							characterClass.MoveAbilityCard(item2, characterClass.GetCardPileList(item2.CurrentCardPile), characterClass.GetCardPileList(eCardPile), CCharacterClass.GetCardPileListKey(item2.CurrentCardPile), CCharacterClass.GetCardPileListKey(eCardPile));
						}
					}
				}
			}
		}
		else if (GetMiscVar("MoveCardsToLostPile").GetBool())
		{
			if (selectedAbilityCardsOutOfPhase != null)
			{
				foreach (CAbilityCard item3 in selectedAbilityCardsOutOfPhase)
				{
					foreach (CAbility item4 in (from x in item3.GetAllAbilities()
						where x.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.Tide)
						select x).ToList())
					{
						item4.ActiveBonusData.SetMiscVar("MoveCardToLostPile", true);
					}
				}
			}
		}
		else
		{
			CBaseCard.ECardPile eCardPile2 = GetMiscVar("DiscardPile").GetEnum(null, CBaseCard.ECardPile.Discarded);
			if (eCardPile2 != CBaseCard.ECardPile.Discarded && cPlayerActor != null && selectedAbilityCardsOutOfPhase != null)
			{
				foreach (CAbilityCard item5 in selectedAbilityCardsOutOfPhase)
				{
					if (item5.CurrentCardPile == CBaseCard.ECardPile.Activated)
					{
						item5.PendingCardPile = eCardPile2;
						continue;
					}
					CCharacterClass characterClass2 = cPlayerActor.CharacterClass;
					characterClass2.MoveAbilityCard(item5, characterClass2.GetCardPileList(item5.CurrentCardPile), characterClass2.GetCardPileList(eCardPile2), CCharacterClass.GetCardPileListKey(item5.CurrentCardPile), CCharacterClass.GetCardPileListKey(eCardPile2));
				}
			}
		}
		CEndedExtraTurn cEndedExtraTurn = new CEndedExtraTurn(playerActor);
		cEndedExtraTurn.m_ActorTurnEnded = playerActor;
		ScenarioRuleClient.MessageHandler(cEndedExtraTurn);
		PhaseManager.ManuallySetCurrentPhase(previousPhase);
		if (previousRoundCard?.SelectedAction != null && GameState.InternalCurrentActor != null && GameState.InternalCurrentActor.OriginalType == CActor.EType.Player)
		{
			GameState.PlayerSelectedAbilityCardAction(previousRoundCard, (previousRoundCard.SelectedAction.ID == previousRoundCard.TopAction.ID) ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction);
		}
		else
		{
			GameState.CurrentAction = null;
		}
		GameState.CurrentActionInitiator = previousInitiator;
		CEndSelectAndPlayCard_MessageData message = new CEndSelectAndPlayCard_MessageData(AnimOverload, playerActor);
		ScenarioRuleClient.MessageHandler(message);
		cachedCardPiles.Clear();
		PhaseManager.StepComplete();
		if (!(PhaseManager.CurrentPhase is CPhaseEndTurn) && !(PhaseManager.CurrentPhase is CPhaseActionSelection))
		{
			return;
		}
		if (currentTriggeredBonus != null && currentTriggeredBonus.ToggleLocked && currentTriggeredBonus.Ability is CAbilityAddActiveBonus)
		{
			currentTriggeredBonus.RestrictActiveBonus(TargetingActor);
			currentTriggeredBonus?.BespokeBehaviour?.OnBehaviourTriggered();
			if (currentTriggeredBonus.HasTracker)
			{
				currentTriggeredBonus.UpdateXPTracker(processTracker: true, processXP: false);
				currentTriggeredBonus.FinishIfNoRemaining();
			}
		}
		if (PhaseManager.CurrentPhase is CPhaseActionSelection cPhaseActionSelection)
		{
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			cPhaseActionSelection.NextStep();
		}
		else
		{
			GameState.NextPhase();
		}
	}

	public bool IncNumCardsPlayed()
	{
		numCardsPlayed++;
		if (numCardsPlayed >= numCardsSelected)
		{
			EndOutOfPhaseAbilities();
			return true;
		}
		return false;
	}

	public bool IsValidAction(CAction action, CBaseCard.ActionType actionType, CAbilityCard card)
	{
		CVars cVars = new CVars();
		TValue tValue = new TValue(TargetingActor ?? GameState.InternalCurrentActor);
		if (tValue == null)
		{
			return false;
		}
		cVars.Set("TargetingActor", new TValue(tValue));
		cVars.Set("ActionType", actionType.ToString());
		cVars.Set("Action", new TValue(action));
		cVars.Set("AbilityCard", new TValue(card));
		cVars.Set("IsTop", actionType == CBaseCard.ActionType.TopAction || actionType == CBaseCard.ActionType.DefaultAttackAction);
		cVars.Set("IsLoss", action.CardPile == CBaseCard.ECardPile.Lost || action.CardPile == CBaseCard.ECardPile.PermanentlyLost);
		if (cachedCardPiles.TryGetValue(card, out var value))
		{
			cVars.Set("LastPlayedActionInfo", new TValue(value));
		}
		else
		{
			cVars.Set("LastPlayedActionInfo", null);
		}
		CVarsContext context = new CVarsContext(cVars, VarsContext);
		foreach (var miscVar in MiscVars)
		{
			var (text, _) = miscVar;
			if (!(text == "IsCardValid"))
			{
				TValue item = miscVar.Item2;
				TValue value2 = item.Calc(context);
				cVars.Set(text, value2);
				if (value2.isValid && text == "IsActionValid")
				{
					return item.GetBool(context);
				}
			}
		}
		return true;
	}

	public bool IsValidCard(CAbilityCard card)
	{
		CVars cVars = new CVars();
		TValue tValue = new TValue(TargetingActor ?? GameState.InternalCurrentActor);
		if (tValue == null)
		{
			return false;
		}
		cVars.Set("TargetingActor", new TValue(tValue));
		cVars.Set("AbilityCard", new TValue(card));
		CVarsContext context = new CVarsContext(cVars, VarsContext);
		foreach (var miscVar in MiscVars)
		{
			var (text, _) = miscVar;
			if (!(text == "IsActionValid"))
			{
				TValue item = miscVar.Item2;
				TValue value = item.Calc(context);
				cVars.Set(text, value);
				if (value.isValid && text == "IsCardValid" && !item.GetBool(context))
				{
					return false;
				}
			}
		}
		if (!IsValidAction(card.TopAction, CBaseCard.ActionType.TopAction, card) && !IsValidAction(card.BottomAction, CBaseCard.ActionType.BottomAction, card) && !IsValidAction(card.DefaultAttackAction, CBaseCard.ActionType.DefaultAttackAction, card))
		{
			return IsValidAction(card.DefaultMoveAction, CBaseCard.ActionType.DefaultMoveAction, card);
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		fullAbilityRestart = false;
		if (!dontMoveState && m_State == SelectAndPlayCardState.SelectTarget)
		{
			m_State = SelectAndPlayCardState.SelectCards;
		}
		return m_State == SelectAndPlayCardState.Done;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (m_State == SelectAndPlayCardState.SelectTarget && !AllTargets)
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
		base.TileSelected(selectedTile, optionalTileList);
		if (flag)
		{
			Perform();
		}
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

	public override bool CanClearTargets()
	{
		return m_State == SelectAndPlayCardState.SelectTarget;
	}
}
