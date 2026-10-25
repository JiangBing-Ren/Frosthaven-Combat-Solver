using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.Scenario.Modules;
using GLOOM;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Scenario.Services;

public class ReactionViewService : FHScenarioViewService
{
	[ConsoleVariable(Alias = "reaction_use_experimental", Persistent = true)]
	public static bool UseExperimental;

	public ReactionViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	[ConsoleCommand(Command = "reaction_drifter")]
	public static void CmdDriftest(IConsole console, int moveStrength = -1)
	{
		ReactionViewService service = Singleton<FHScenarioViewController>.Instance.GetService<ReactionViewService>();
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.First((CPlayerActor s) => s.CharacterClass.CharacterModel == ECharacter.Drifter);
		List<CAbilityCard> cards = (from ab in CAbilityMoveActiveBonusTracker.GetActiveBonusesWithMovableTrackers(cPlayerActor, moveStrength)
			select ab.BaseCard as CAbilityCard into c
			where c != null
			select c).ToList();
		service.SelectCardsWithActiveBonusTracker(cPlayerActor, cards, 1, isLoseBonusType: false);
	}

	public void SelectCardsWithActiveBonusTracker(CPlayerActor playerActor, List<CAbilityCard> cards, int maxCardsToSelect, bool isLoseBonusType)
	{
		UIReactionActiveBonusTrackerCards reactionActiveBonusTracker = _view.Modules.ReactionActiveBonusTracker;
		string titleKey;
		if (isLoseBonusType)
		{
			titleKey = ((maxCardsToSelect < 2) ? "GUI_LOSE_ACTIVE_BONUS_TRACKER" : "GUI_LOSE_ACTIVE_BONUS_TRACKERS");
		}
		else
		{
			titleKey = ((maxCardsToSelect < 2) ? "GUI_MOVE_ACTIVE_BONUS_TRACKER" : "GUI_MOVE_ACTIVE_BONUS_TRACKERS");
		}
		string term = ((maxCardsToSelect < 2) ? "GUI_TOOLTIP_SELECT_ACTIVE_BONUS_CARD" : "GUI_TOOLTIP_SELECT_ACTIVE_BONUS_CARDS");
		UIReactionActiveBonusTrackerCards.Params obj = new UIReactionActiveBonusTrackerCards.Params
		{
			PlayerActor = playerActor,
			Ability = null,
			ChoiceCards = cards,
			TitleKey = titleKey,
			Description = LocalizationManager.GetTranslation(term),
			ConfirmKey = "GUI_CONFIRM",
			SourceIllustrations = GetSourceIllustration(playerActor, null),
			UndoButtonReference = Choreographer.s_Choreographer.m_UndoButton,
			SkipButtonReference = Choreographer.s_Choreographer.m_SkipButton,
			ConfirmCondition = (List<CAbilityCard> selectedCards) => selectedCards.Count > 0 && selectedCards.Count <= maxCardsToSelect,
			OnConfirm = OnMoveTrackerOfActiveBonus_Confirmed,
			OnSelectedCardsUpdated = OnMoveTrackerOfActiveBonusCard_SelectedCardsUpdated
		};
		reactionActiveBonusTracker.gameObject.SetActive(value: true);
		reactionActiveBonusTracker.Init(obj);
	}

	private void OnMoveTrackerOfActiveBonus_Confirmed(CPlayerActor playerActor, List<CAbilityCard> selectedCards)
	{
		_view.Modules.ReactionActiveBonusTracker.Close();
		if (selectedCards.Any() && playerActor.IsUnderMyControl() && PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMoveActiveBonusTracker cAbilityMoveActiveBonusTracker)
		{
			int num = Choreographer.s_Choreographer.m_CurrentAbility?.Strength ?? (-1);
			cAbilityMoveActiveBonusTracker.OnTrackerUpdated(selectedCards, num);
			if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
			{
				Synchronizer.SendGameAction(GameActionType.AbilityMoveActiveBonusTracker, ActionPhaseType.MoveActiveBonusTracker, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCards), new IntToken(num));
			}
		}
	}

	private void OnMoveTrackerOfActiveBonusCard_SelectedCardsUpdated(CPlayerActor playerActor, List<CAbilityCard> selectedCards)
	{
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction(GameActionType.MoveTrackerSelectedCardsUpdated, ActionPhaseType.MoveActiveBonusTracker, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCards));
		}
		_view.RefreshView();
	}

	public void Proxy_TrackerOfActiveBonus_UpdateSelectedCards(GameAction action)
	{
		CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.First((CPlayerActor a) => a.PlayerActor.ID == action.ActorID);
		List<CAbilityCard> list = new List<CAbilityCard>();
		int[] cardIDs = cardsToken.CardIDs;
		for (int num = 0; num < cardIDs.Length; num++)
		{
			int cardID = cardIDs[num];
			CAbilityCard cAbilityCard = cPlayerActor.CharacterClass.AbilityCardsPool.FirstOrDefault((CAbilityCard x) => x.ID == cardID);
			if (cAbilityCard == null)
			{
				throw new Exception("Error with proxy selecting ability tracker. Could not find a card with card ID " + cardID + " from actor with ID: " + cPlayerActor.ID);
			}
			list.Add(cAbilityCard);
		}
		_view.Modules.ReactionActiveBonusTracker.SetSelectedCards(list);
	}

	[ConsoleCommand(Command = "reaction_forzenfist_selected_card_to_recover")]
	public static void CmdFrozenFistOneWithTheMountainTest(IConsole console, int moveStrength = -1)
	{
		ReactionViewService service = Singleton<FHScenarioViewController>.Instance.GetService<ReactionViewService>();
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor s) => s.CharacterClass.CharacterModel == ECharacter.Frozenfist);
		if (cPlayerActor != null)
		{
			service.FrozenFistOneWithTheMountain_SelectCardToRecover(cPlayerActor, null, cPlayerActor.CharacterClass.DiscardedAbilityCards, cPlayerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection);
		}
	}

	public void FrozenFistOneWithTheMountain_SelectCardToRecover(CPlayerActor playerActor, CAbility ability, List<CAbilityCard> validDiscarded, List<CAbilityCard> validRoundBonuses)
	{
		Singleton<CardsHandManager>.Instance.Hide();
		UIReactionFrozenFistCardRecoverFromSelection reactionFrozenFistRecoverFromSelection = _view.Modules.ReactionFrozenFistRecoverFromSelection;
		string titleKey = "RecoverDiscardedCards";
		string descriptionKey = "GUI_REACTION_ONE_WITH_THE_MOUNTAIN_DESC";
		string buffWarningKey = "GUI_WARNING_CANCEL_ROUND_ACTIVE_BONUS_WARNING";
		UIReactionFrozenFistCardRecoverFromSelection.Params obj = new UIReactionFrozenFistCardRecoverFromSelection.Params
		{
			PlayerActor = playerActor,
			Ability = ability,
			ChoiceCards = validDiscarded.Concat(validRoundBonuses).ToList(),
			TitleKey = titleKey,
			DescriptionKey = descriptionKey,
			ConfirmKey = "GUI_CONFIRM",
			BuffWarningKey = buffWarningKey,
			SourceIllustrations = GetSourceIllustration(playerActor, ability),
			UndoButtonReference = Choreographer.s_Choreographer.m_UndoButton,
			SkipButtonReference = Choreographer.s_Choreographer.m_SkipButton,
			ConfirmCondition = (List<CAbilityCard> selectedCards) => selectedCards.Count == 1,
			CancelRoundBonusCondition = (List<CAbilityCard> selectedCards) => selectedCards.Count == 1 && CharacterClassManager.FindAllActiveBonuses().Any((CActiveBonus ab) => ab.Duration == CActiveBonus.EActiveBonusDurationType.Round && ab.BaseCard == selectedCards[0]),
			OnConfirm = OnOneWithTheMountain_SelectCardToRecover_Confirmed,
			OnSelectedCardsUpdated = OnSelectCardToRecover_SelectedCardsUpdated,
			SelfDamageValue = (List<CAbilityCard> selectedCards) =>
			{
				if (selectedCards.Count != 1)
				{
					return 0;
				}
				CAbilityCard cAbilityCard = selectedCards[0];
				int num = 2 + cAbilityCard.Level / 2;
				if (playerActor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
				{
					num *= 2;
				}
				if (playerActor.Tokens.HasKey(CCondition.EPositiveCondition.Ward))
				{
					num /= 2;
				}
				return num;
			}
		};
		reactionFrozenFistRecoverFromSelection.gameObject.SetActive(value: true);
		reactionFrozenFistRecoverFromSelection.Init(obj);
	}

	private void OnOneWithTheMountain_SelectCardToRecover_Confirmed(CPlayerActor playerActor, List<CAbilityCard> selectedCards)
	{
		_view.Modules.ReactionFrozenFistRecoverFromSelection.Close();
		if (selectedCards.Any() && playerActor.IsUnderMyControl() && PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityRecoverDiscardedCards)
		{
			if (selectedCards.Count == 1 && CharacterClassManager.FindAllActiveBonuses().Any((CActiveBonus ab) => ab.Duration == CActiveBonus.EActiveBonusDurationType.Round && ab.BaseCard == selectedCards[0]))
			{
				Singleton<CardsHandManager>.Instance.CancelActiveAbility(playerActor, selectedCards[0]);
				Singleton<CardsHandManager>.Instance.NetworkCancelingActiveBonuses(playerActor, selectedCards[0]);
			}
			if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
			{
				Synchronizer.SendGameAction(GameActionType.RecoverCards, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCards));
			}
			GameState.PlayerSelectedCardsToRecover(playerActor, selectedCards, CBaseCard.ECardPile.Discarded);
			GameState.PlayerFinishedMovingCards(playerActor);
		}
	}

	private void OnSelectCardToRecover_SelectedCardsUpdated(CPlayerActor playerActor, List<CAbilityCard> selectedCards)
	{
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction(GameActionType.CardToRecoverSelectedCardsUpdated, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCards));
		}
		_view.RefreshView();
	}

	public void Proxy_OnSelectCardToRecover_UpdateSelectedCards(GameAction action)
	{
		CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.First((CPlayerActor a) => a.PlayerActor.ID == action.ActorID);
		List<CAbilityCard> list = new List<CAbilityCard>();
		int[] cardIDs = cardsToken.CardIDs;
		for (int num = 0; num < cardIDs.Length; num++)
		{
			int cardID = cardIDs[num];
			CAbilityCard cAbilityCard = cPlayerActor.CharacterClass.AbilityCardsPool.FirstOrDefault((CAbilityCard x) => x.ID == cardID);
			if (cAbilityCard == null)
			{
				throw new Exception("Error with proxy selecting ability tracker. Could not find a card with card ID " + cardID + " from actor with ID: " + cPlayerActor.ID);
			}
			list.Add(cAbilityCard);
		}
		_view.Modules.ReactionFrozenFistRecoverFromSelection.SetSelectedCards(list);
	}

	public void SelectAndPlayCard(CPlayerActor playerActor, CAbility ability, List<CAbilityCard> cards, int maxCardsToSelect)
	{
		UIReactionSelectAndPlayCards reactionSelectAndPlayCards = _view.Modules.ReactionSelectAndPlayCards;
		string titleKey = "GUI_SELECT_CARD";
		UIReactionSelectAndPlayCards.Params obj = new UIReactionSelectAndPlayCards.Params
		{
			PlayerActor = playerActor,
			Ability = ability,
			ChoiceCards = cards,
			SourceIllustrations = GetSourceIllustration(playerActor, ability),
			TitleKey = titleKey,
			Description = GetSelectAndPlayDescription(playerActor, ability, maxCardsToSelect),
			ConfirmKey = "GUI_CONFIRM",
			UndoButtonReference = Choreographer.s_Choreographer.m_UndoButton,
			SkipButtonReference = Choreographer.s_Choreographer.m_SkipButton,
			ConfirmCondition = (List<CAbilityCard> selectedCards) => selectedCards.Count > 0 && selectedCards.Count <= maxCardsToSelect,
			OnConfirm = SelectAndPlayCard_Confirmed,
			OnSelectedCardsUpdated = SelectAndPlayCard_SelectedCardsUpdated
		};
		reactionSelectAndPlayCards.gameObject.SetActive(value: true);
		reactionSelectAndPlayCards.Init(obj);
	}

	private void SelectAndPlayCard_Confirmed(CPlayerActor playerActor, List<CAbilityCard> selectedCards)
	{
		_view.Modules.ReactionSelectAndPlayCards.Close();
		if (selectedCards.Any() && playerActor.IsUnderMyControl())
		{
			Choreographer.s_Choreographer.readyButton.OnClick();
			Singleton<CardsHandManager>.Instance.cardsActionController.OnActionFinished();
			GameState.CurrentSelectAndPlayCardAbility.StartOutOfPhaseAbilities();
		}
	}

	private void SelectAndPlayCard_SelectedCardsUpdated(CPlayerActor playerActor, List<CAbilityCard> selectedCards)
	{
		foreach (CAbilityCard selectedCard in selectedCards)
		{
			Singleton<CardsHandManager>.Instance.CurrentHand.AllCards.FirstOrDefault((AbilityCardUI ui) => ui.AbilityCard.ID == selectedCard.ID)?.OnClick();
		}
		_view.RefreshView();
	}

	public void Proxy_SelectAndPlayCard_SelectedCardsUpdated_UpdateSelectedCards(GameAction action)
	{
	}

	private string GetSelectAndPlayDescription(CPlayerActor playerActor, CAbility ability, int maxCardsToSelect)
	{
		if (ability.HelpBoxTooltipLocKey.IsNOTNullOrEmpty())
		{
			return LocalizationManager.GetTranslation(ability.HelpBoxTooltipLocKey);
		}
		if (maxCardsToSelect == 1)
		{
			return "GUI_REACTION_SELECT_AND_PLAY_ONECARD_DESC";
		}
		return FHUILocFormatting.PopulateParams("GUI_REACTION_SELECT_AND_PLAY_CARDS_DESC", maxCardsToSelect);
	}

	public static string GetCardNameKey(CAbility ability, CBaseCard card)
	{
		PerksYMLData perksYMLData = ability?.perkData;
		if (perksYMLData != null)
		{
			return perksYMLData.Name;
		}
		if (card == null)
		{
			if (ability?.TargetingActor != null && (ability is CAbilityMove || ability is CAbilityAttack))
			{
				return ability.TargetingActor.ActorLocKey();
			}
			return null;
		}
		if (card.CardType == CBaseCard.ECardType.ScenarioModifier)
		{
			return null;
		}
		if (card.CardType == CBaseCard.ECardType.Dummy)
		{
			return null;
		}
		if (card.CardType == CBaseCard.ECardType.AttackModifier)
		{
			return null;
		}
		if (card.CardType == CBaseCard.ECardType.MonsterAbility)
		{
			MonsterCardYMLData monsterCardYMLData = ScenarioRuleClient.SRLYML.MonsterCards.Find((MonsterCardYMLData s) => s.ID == card.ID);
			if (!string.IsNullOrEmpty(monsterCardYMLData?.Name))
			{
				return monsterCardYMLData.Name;
			}
			return MonsterClassManager.MonsterAndObjectClassesWithInitiative.First((CMonsterClass c) => c.ID == ((CBaseAbilityCard)card).ClassID).LocKey;
		}
		return card.Name;
	}

	private Sprite GetSourceIllustration(CPlayerActor playerActor, CAbility ability)
	{
		if (playerActor != null)
		{
			return UIInfoTools.Instance.GetCharacterConfigUI(playerActor.CharacterClass.CharacterModel)?.InfoIllustration;
		}
		return null;
	}

	public void UpdateReactionModules()
	{
		foreach (IUIViewModule allModule in _view.Modules.AllModules)
		{
			if (allModule is ICharacterReactionModule characterReactionModule && allModule.GameObject.activeInHierarchy)
			{
				characterReactionModule.UpdateState();
			}
		}
	}
}
