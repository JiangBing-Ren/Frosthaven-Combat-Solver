using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilitySelectCard : CAbility
{
	public enum SelectCardsState
	{
		SelectTarget,
		SelectCards,
		AnimateCharacter,
		Done
	}

	[JsonProperty]
	public List<CBaseCard.ECardPile> PilesToMoveFrom;

	[JsonProperty]
	public int MinRequiredCards;

	private List<CAbilityCard> selectedAbilityCardsOutOfPhase;

	public SelectCardsState m_State;

	private CPlayerActor playerActor => TargetingActor as CPlayerActor;

	public CAbilitySelectCard(List<CBaseCard.ECardPile> pilesToMoveFrom, int minRequiredCards)
	{
		PilesToMoveFrom = pilesToMoveFrom;
		MinRequiredCards = minRequiredCards;
		Range = 1;
		Targeting = EAbilityTargeting.Range;
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		m_State = SelectCardsState.SelectTarget;
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
		case SelectCardsState.SelectTarget:
		{
			if (MiscAbilityData == null)
			{
				MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value && ActorsToTarget.Count > 0)
			{
				m_State = SelectCardsState.SelectCards;
				Perform();
				return false;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			return false;
		}
		case SelectCardsState.SelectCards:
		{
			CPlayerActor cPlayerActor = (CPlayerActor)TargetingActor;
			if (cPlayerActor == null)
			{
				break;
			}
			List<CAbilityCard> list = PilesToMoveFrom.SelectMany(cPlayerActor.CharacterClass.GetCardPileList).Where(IsValidCard).ToList();
			if (list.Count > 0)
			{
				AbilityHasHappened = true;
				GameState.CurrentOutOfPhaseCardSelectionAbility = this;
				CSelectCard_MessageData cSelectCard_MessageData = new CSelectCard_MessageData(AnimOverload, TargetingActor);
				cSelectCard_MessageData.m_ActorReplayingCard = TargetingActor;
				cSelectCard_MessageData.m_Ability = this;
				cSelectCard_MessageData.m_ValidCards = list.Select((CAbilityCard c) => c.ID).ToList();
				cSelectCard_MessageData.m_PilesToChooseFrom = PilesToMoveFrom;
				ScenarioRuleClient.MessageHandler(cSelectCard_MessageData);
				return true;
			}
			CNoValidCardsForAbility_MessageData message2 = new CNoValidCardsForAbility_MessageData(TargetingActor, this);
			ScenarioRuleClient.MessageHandler(message2);
			return false;
		}
		case SelectCardsState.AnimateCharacter:
		{
			AbilityHasHappened = true;
			if (TargetingActor.Type == CActor.EType.Player && !m_AbilityHasHappened)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			CPlayCharacterAnimation_MessageData message = new CPlayCharacterAnimation_MessageData(AnimOverload, TargetingActor)
			{
				m_Ability = this,
				m_TargetedTilesForVFX = null,
				m_AreaTilesForVFX = null,
				m_IsFromItem = IsItemAbility
			};
			ScenarioRuleClient.MessageHandler(message);
			EndSelectCard();
			break;
		}
		case SelectCardsState.Done:
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

	public override bool IsPositive()
	{
		return true;
	}

	public void PerformSelection()
	{
		m_State = SelectCardsState.AnimateCharacter;
		Perform();
	}

	public void EndSelectCard()
	{
		m_State = SelectCardsState.Done;
		Perform();
		CActor cActor = ActorsToTarget[0];
		List<KeyValuePair<CAbilityCard, CBaseCard.ECardPile>> enumerable = GameState.__OutOfPhaseCardFromPiles.OrderBy((KeyValuePair<CAbilityCard, CBaseCard.ECardPile> x) => x.Key.ID).ToList();
		GameState.__OutOfPhaseCardFromPiles.Clear();
		GameState.__OutOfPhaseCardFromPiles.AddRange(enumerable);
		selectedAbilityCardsOutOfPhase = playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.ToList();
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
		GameState.__OutOfPhaseCardFromPiles.Clear();
		CEndedExtraTurn cEndedExtraTurn = new CEndedExtraTurn(playerActor);
		cEndedExtraTurn.m_ActorTurnEnded = playerActor;
		ScenarioRuleClient.MessageHandler(cEndedExtraTurn);
		PhaseManager.StepComplete();
		CEndSelectAndPlayCard_MessageData message = new CEndSelectAndPlayCard_MessageData(AnimOverload, playerActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool IsValidAction(CAction action, CBaseCard.ActionType actionType)
	{
		CVars cVars = new CVars();
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
		if (!dontMoveState && m_State == SelectCardsState.SelectTarget)
		{
			m_State = SelectCardsState.SelectCards;
		}
		return m_State == SelectCardsState.Done;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (m_State == SelectCardsState.SelectTarget && !AllTargets)
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
		return m_State == SelectCardsState.SelectTarget;
	}
}
