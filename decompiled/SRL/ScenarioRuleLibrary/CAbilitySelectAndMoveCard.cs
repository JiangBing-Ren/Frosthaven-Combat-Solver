using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilitySelectAndMoveCard : CAbility
{
	public enum SelectAndSwapCardsState
	{
		SelectTarget,
		SelectCards,
		Swap,
		Done
	}

	[JsonProperty]
	public List<CBaseCard.ECardPile> PilesToMoveFrom;

	[JsonProperty]
	public CBaseCard.ECardPile PileToMoveTo;

	[JsonProperty]
	public int MinRequiredCards;

	private List<CAbilityCard> selectedAbilityCardsOutOfPhase = new List<CAbilityCard>();

	[JsonIgnore]
	[StateDump(Skip = true)]
	private List<CAbilityCard> initialOutOfPhaseCards;

	public SelectAndSwapCardsState m_State;

	private CPlayerActor playerActor => TargetingActor as CPlayerActor;

	public CAbilitySelectAndMoveCard(List<CBaseCard.ECardPile> pilesToMoveFrom, CBaseCard.ECardPile pileToMoveTo, int minRequiredCards)
	{
		PilesToMoveFrom = pilesToMoveFrom;
		PileToMoveTo = pileToMoveTo;
		MinRequiredCards = minRequiredCards;
		Range = 1;
		Targeting = EAbilityTargeting.Range;
	}

	public bool HasValidCards(CActor actor)
	{
		return PilesToMoveFrom.SelectMany(actor.CharacterClass.GetCardPileList).Where(IsValidCard).Any();
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		if (!(TargetingActor is CPlayerActor))
		{
			SetCancelAbilityFlag(cancel: true);
			return;
		}
		m_State = SelectAndSwapCardsState.SelectTarget;
		initialOutOfPhaseCards = TargetingActor.CharacterClass?.SelectedAbilityCardsOutOfPhase?.ToList();
		SharedAbilityTargeting.GetValidActorsInRange(this);
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			TileSelected(targetingActor.Tile, null);
		}
		SetMiscVar("MinRequiredCards", MinRequiredCards);
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
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
		case SelectAndSwapCardsState.SelectTarget:
		{
			if (MiscAbilityData == null)
			{
				MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value && ActorsToTarget.Count > 0)
			{
				m_State = SelectAndSwapCardsState.SelectCards;
				Perform();
				return false;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			return false;
		}
		case SelectAndSwapCardsState.SelectCards:
		{
			CPlayerActor cPlayerActor2 = (CPlayerActor)TargetingActor;
			if (cPlayerActor2 == null)
			{
				break;
			}
			List<CAbilityCard> list = PilesToMoveFrom.SelectMany(cPlayerActor2.CharacterClass.GetCardPileList).Where(IsValidCard).ToList();
			if (list.Count > 0)
			{
				GameState.CurrentOutOfPhaseCardSelectionAbility = this;
				CSelectAndMoveCard_MessageData cSelectAndMoveCard_MessageData = new CSelectAndMoveCard_MessageData(AnimOverload, TargetingActor);
				cSelectAndMoveCard_MessageData.m_ActorReplayingCard = TargetingActor;
				cSelectAndMoveCard_MessageData.m_Ability = this;
				cSelectAndMoveCard_MessageData.m_ValidCards = list.Select((CAbilityCard c) => c.ID).ToList();
				cSelectAndMoveCard_MessageData.m_PilesToChooseFrom = PilesToMoveFrom;
				ScenarioRuleClient.MessageHandler(cSelectAndMoveCard_MessageData);
				return true;
			}
			CNoValidCardsForAbility_MessageData message = new CNoValidCardsForAbility_MessageData(TargetingActor, this);
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		case SelectAndSwapCardsState.Swap:
		{
			TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			AbilityHasHappened = true;
			CPlayerActor cPlayerActor = (CPlayerActor)TargetingActor;
			List<CAbilityCard> cardPileList = cPlayerActor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase);
			string cardPileListKey = CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase);
			List<CAbilityCard> cardPileList2 = cPlayerActor.CharacterClass.GetCardPileList(PileToMoveTo);
			string cardPileListKey2 = CCharacterClass.GetCardPileListKey(PileToMoveTo);
			selectedAbilityCardsOutOfPhase.Clear();
			for (int num = cardPileList.Count - 1; num >= 0; num--)
			{
				CAbilityCard cAbilityCard = cardPileList[num];
				if (!initialOutOfPhaseCards.Contains(cAbilityCard))
				{
					GameState.__OutOfPhaseCardFromPiles.Remove(cAbilityCard);
					selectedAbilityCardsOutOfPhase.Add(cAbilityCard);
					cPlayerActor.CharacterClass.MoveAbilityCard(cAbilityCard, cardPileList, cardPileList2, cardPileListKey, cardPileListKey2);
				}
			}
			EndSwap(ScenarioRuleClient.IsWorkerThread);
			return true;
		}
		case SelectAndSwapCardsState.Done:
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

	public void PerformSwap()
	{
		m_State = SelectAndSwapCardsState.Swap;
		Perform();
	}

	public void EndSwap(bool fromLogicThread)
	{
		if (!fromLogicThread)
		{
			ScenarioRuleClient.FinishSelectAndMoveCardAbility(playerActor, this);
			return;
		}
		m_State = SelectAndSwapCardsState.Done;
		Perform();
		CActor cActor = ActorsToTarget[0];
		List<KeyValuePair<CAbilityCard, CBaseCard.ECardPile>> enumerable = GameState.__OutOfPhaseCardFromPiles.OrderBy((KeyValuePair<CAbilityCard, CBaseCard.ECardPile> x) => x.Key.ID).ToList();
		GameState.__OutOfPhaseCardFromPiles.Clear();
		GameState.__OutOfPhaseCardFromPiles.AddRange(enumerable);
		foreach (CAbilityCard item in cActor.CharacterClass.SelectedAbilityCardsOutOfPhase.ToList())
		{
			if (!GameState.__OutOfPhaseCardFromPiles.TryGetValue(item, out var value))
			{
				Log.GameplayError("Can't find the source pile for card " + item.Name);
			}
			else
			{
				cActor.CharacterClass.MoveAbilityCard(item, cActor.CharacterClass.SelectedAbilityCardsOutOfPhase, cActor.CharacterClass.GetCardPileList(value), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase), CCharacterClass.GetCardPileListKey(value));
			}
		}
		GameState.CurrentOutOfPhaseCardSelectionAbility = null;
		GameState.__OutOfPhaseCardFromPiles.RemoveWhere((KeyValuePair<CAbilityCard, CBaseCard.ECardPile> x) => !initialOutOfPhaseCards.Contains(x.Key));
		CEndedExtraTurn cEndedExtraTurn = new CEndedExtraTurn(playerActor);
		cEndedExtraTurn.m_ActorTurnEnded = playerActor;
		ScenarioRuleClient.MessageHandler(cEndedExtraTurn);
		CEndSelectAndPlayCard_MessageData message = new CEndSelectAndPlayCard_MessageData(AnimOverload, playerActor);
		ScenarioRuleClient.MessageHandler(message);
		if (PhaseManager.PreviousPhase != null && PhaseManager.PreviousPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			CPlayerActor cPlayerActor = playerActor;
			if (cPlayerActor != null && cPlayerActor.IsExhausted())
			{
				GameState.CheckExhaustAfterShortRest(playerActor, isShortRestingPlayer: true);
			}
		}
		PhaseManager.StepComplete();
	}

	public bool IsValidAction(CAction action, CBaseCard.ActionType actionType)
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
		CVarsContext context = new CVarsContext(cVars, VarsContext);
		foreach (var miscVar in MiscVars)
		{
			var (text, _) = miscVar;
			if (!(text == "IsCardValid"))
			{
				TValue item = miscVar.Item2;
				TValue value = item.Calc(context);
				cVars.Set(text, value);
				if (value.isValid && text == "IsActionValid")
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
		cVars.Set("AbilityCard", new TValue(card));
		TValue tValue = new TValue(TargetingActor ?? GameState.InternalCurrentActor);
		if (tValue == null)
		{
			return false;
		}
		cVars.Set("TargetingActor", new TValue(tValue));
		CVarsContext context = new CVarsContext(cVars, VarsContext);
		foreach (var miscVar in MiscVars)
		{
			var (text, _) = miscVar;
			if (!(text == "IsActionValid"))
			{
				TValue item = miscVar.Item2;
				TValue value = item.Calc(context);
				cVars.Set(text, value);
				if (value.isValid && text == "IsCardValid")
				{
					return item.GetBool(context);
				}
			}
		}
		if (!IsValidAction(card.TopAction, CBaseCard.ActionType.TopAction))
		{
			return IsValidAction(card.BottomAction, CBaseCard.ActionType.BottomAction);
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		fullAbilityRestart = false;
		if (!dontMoveState && m_State == SelectAndSwapCardsState.SelectTarget)
		{
			m_State = SelectAndSwapCardsState.SelectCards;
		}
		return m_State == SelectAndSwapCardsState.Done;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (m_State == SelectAndSwapCardsState.SelectTarget && !AllTargets)
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

	public override bool CanClearTargets()
	{
		return m_State == SelectAndSwapCardsState.SelectTarget;
	}
}
