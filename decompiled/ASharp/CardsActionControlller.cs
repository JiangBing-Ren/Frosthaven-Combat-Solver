using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;

public class CardsActionControlller : MonoBehaviour
{
	public enum Phase
	{
		None,
		Select1stCard,
		Pick1stTarget,
		Select2ndCard,
		Pick2ndTarget,
		Select3rdCard,
		Pick3rdTarget
	}

	private struct CardActionPair(FullAbilityCard card, CBaseCard.ActionType action)
	{
		public FullAbilityCard Card = card;

		public CBaseCard.ActionType Action = action;

		public override string ToString()
		{
			return $"[{Card?.AbilityCard}, {Action}]";
		}
	}

	public static CardsActionControlller s_Instance;

	private const int FIRST_CARD = 0;

	private const int SECOND_CARD = 1;

	private const int THIRD_CARD = 2;

	private static Phase _currentPhase;

	private static Phase _cachedPhase;

	private FullAbilityCard _firstCard;

	private FullAbilityCard _secondCard;

	private FullAbilityCard _thirdCard;

	private List<CardActionPair> _playedCards = new List<CardActionPair>();

	private List<FullAbilityCard> _nonPlayedCards = new List<FullAbilityCard>();

	private CAbilityExtraTurn.EExtraTurnType _extraTurnType;

	private CActor _actor;

	private CBaseCard.ActionType _mandatoryFirstAction;

	public bool IsActionAvailable
	{
		get
		{
			Phase currentPhase = _currentPhase;
			return currentPhase == Phase.Select1stCard || currentPhase == Phase.Select2ndCard || currentPhase == Phase.Select3rdCard;
		}
	}

	private int CardsCount
	{
		get
		{
			int num = 0;
			if (_firstCard != null)
			{
				num++;
			}
			if (_secondCard != null)
			{
				num++;
			}
			if (_thirdCard != null)
			{
				num++;
			}
			return num;
		}
	}

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
			_currentPhase = Phase.None;
		}
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
			_firstCard = null;
			_secondCard = null;
			_thirdCard = null;
			_playedCards.Clear();
		}
	}

	private void LogState(Phase newPhase)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"CardsActionControlller New Phase: {newPhase}");
		stringBuilder.AppendLine($"Current Phase: {_currentPhase}");
		stringBuilder.AppendLine($"Extra Turn Type: {_extraTurnType}");
		stringBuilder.AppendLine($"First Card: {_firstCard?.AbilityCard}");
		stringBuilder.AppendLine($"Second Card: {_secondCard?.AbilityCard}");
		stringBuilder.AppendLine($"Third Card: {_thirdCard?.AbilityCard}");
		stringBuilder.AppendLine($"First Played Card: {GetPlayedCard(0)}");
		stringBuilder.AppendLine($"Second Played Card: {GetPlayedCard(1)}");
		stringBuilder.AppendLine($"Third Played Card: {GetPlayedCard(2)}");
		Log.Info(stringBuilder.ToString());
	}

	public void Init(FullAbilityCard firstCard, FullAbilityCard secondCard, FullAbilityCard thirdCard, bool reset, CAbilityExtraTurn.EExtraTurnType extraTurnType, CActor actor)
	{
		_firstCard = firstCard;
		_secondCard = secondCard;
		_thirdCard = thirdCard;
		_extraTurnType = extraTurnType;
		_actor = actor;
		if (_actor is CPlayerActor { PlayBottomActionFirst: not false })
		{
			_mandatoryFirstAction = CBaseCard.ActionType.BottomAction;
		}
		else
		{
			_mandatoryFirstAction = CBaseCard.ActionType.NA;
		}
		if ((reset || !ReloadState()) && (!ScenarioManager.IsFastReloading || !OnFastReload()))
		{
			ResetCards();
			SetPhase(Phase.Select1stCard);
		}
	}

	private void ResetCards()
	{
		_playedCards.Clear();
		_nonPlayedCards.Clear();
		MarkCardNonPlayed(_firstCard);
		MarkCardNonPlayed(_secondCard);
		MarkCardNonPlayed(_thirdCard);
	}

	public Phase GetPhase()
	{
		return _currentPhase;
	}

	private void SetPhase(Phase newPhase, bool reloadStateToPickTargetPhase = false)
	{
		if (this == null)
		{
			Log.Error("SetPhase() called for destroyed CardsActionController!");
			return;
		}
		switch (newPhase)
		{
		case Phase.Select1stCard:
		{
			ResetHighlight();
			Singleton<CardsHandManager>.Instance.CurrentHand.SetInteractable(interactable: true, allowHover: true);
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			if (!reloadStateToPickTargetPhase && _currentPhase == Phase.Pick1stTarget && !GameState.IsPerformingCardOutOfPhase)
			{
				CardActionPair playedCard4 = GetPlayedCard(0);
				if (playedCard4.Card != null)
				{
					playedCard4.Card.Deselect();
					playedCard4.Card.ResetConsumes();
					MarkCardNonPlayed(playedCard4.Card);
				}
			}
			bool isValid = !FFSNetwork.IsOnline || (Singleton<CardsHandManager>.Instance.CurrentHand.PlayerActor?.IsUnderMyControl() ?? false);
			_firstCard?.SetInteractable(active: true);
			_secondCard?.SetInteractable(active: true);
			_thirdCard?.SetInteractable(active: true);
			_firstCard?.SetValid(isValid, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			_secondCard?.SetValid(isValid, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			_thirdCard?.SetValid(isValid, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			_firstCard?.Deselect();
			_secondCard?.Deselect();
			_thirdCard?.Deselect();
			_firstCard?.ResetConsumes();
			_secondCard?.ResetConsumes();
			_thirdCard?.ResetConsumes();
			_firstCard?.ResetHighlightInfusions();
			_secondCard?.ResetHighlightInfusions();
			_thirdCard?.ResetHighlightInfusions();
			_firstCard?.HighlightAvailableConsumes(InfusionBoardUI.Instance?.GetAvailableElements());
			_secondCard?.HighlightAvailableConsumes(InfusionBoardUI.Instance?.GetAvailableElements());
			_thirdCard?.HighlightAvailableConsumes(InfusionBoardUI.Instance?.GetAvailableElements());
			if (_extraTurnType == CAbilityExtraTurn.EExtraTurnType.BottomAction)
			{
				_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			}
			else if (_extraTurnType == CAbilityExtraTurn.EExtraTurnType.TopAction)
			{
				_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			}
			if (_mandatoryFirstAction == CBaseCard.ActionType.BottomAction)
			{
				_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
				_secondCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
				_thirdCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			}
			else if (_mandatoryFirstAction == CBaseCard.ActionType.TopAction)
			{
				_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
				_secondCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
				_thirdCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			}
			DisableForgoneActions();
			TryUpdateGameState();
			LogState(newPhase);
			break;
		}
		case Phase.Pick1stTarget:
		{
			ResetHighlight();
			Singleton<CardsHandManager>.Instance.CurrentHand.SetInteractable(interactable: true, allowHover: false);
			Choreographer.s_Choreographer.ShowAugmentationBar = true;
			CardActionPair playedCard5 = GetPlayedCard(0);
			playedCard5.Card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			playedCard5.Card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			playedCard5.Card.ToggleSelect(active: true, playedCard5.Action);
			playedCard5.Card.HighlightInfusions(highlight: true, playedCard5.Action);
			playedCard5.Card.ResetConsumes(clearSelection: false);
			if (!reloadStateToPickTargetPhase && CardsCount == 2 && GameState.CurrentSelectAndPlayCardAbility == null)
			{
				GetNonPlayedCard(0).ToggleSideInteractivity(active: false, playedCard5.Action);
			}
			TryUpdateGameState();
			LogState(newPhase);
			break;
		}
		case Phase.Select2ndCard:
		{
			Singleton<CardsHandManager>.Instance.CurrentHand.SetInteractable(interactable: true, allowHover: true);
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			CardActionPair playedCard = GetPlayedCard(0);
			playedCard.Card.HighlightInfusions(highlight: false, playedCard.Action);
			playedCard.Card.SetInteractable(active: false);
			playedCard.Card.ResetConsumes(clearSelection: false);
			foreach (FullAbilityCard nonPlayedCard in _nonPlayedCards)
			{
				nonPlayedCard.ToggleSideInteractivity(active: true, CBaseCard.ActionType.TopAction);
				nonPlayedCard.ToggleSideInteractivity(active: true, CBaseCard.ActionType.BottomAction);
				if (CardsCount - 1 == _playedCards.Count && !GameState.IsPerformingCardOutOfPhase)
				{
					nonPlayedCard.ToggleSideInteractivity(active: false, playedCard.Action);
				}
			}
			_mandatoryFirstAction = CBaseCard.ActionType.NA;
			if (!reloadStateToPickTargetPhase && _currentPhase == Phase.Pick2ndTarget && !GameState.IsPerformingCardOutOfPhase)
			{
				CardActionPair playedCard2 = GetPlayedCard(1);
				playedCard2.Card.Deselect();
				playedCard2.Card.ResetConsumes();
				MarkCardNonPlayed(playedCard2.Card);
				foreach (FullAbilityCard nonPlayedCard2 in _nonPlayedCards)
				{
					nonPlayedCard2.ToggleSideInteractivity(active: true, CBaseCard.ActionType.TopAction);
					nonPlayedCard2.ToggleSideInteractivity(active: true, CBaseCard.ActionType.BottomAction);
				}
				if (CardsCount == 2)
				{
					playedCard2.Card.ToggleSideInteractivity(active: false, playedCard.Action);
				}
			}
			else
			{
				playedCard.Card.TryPlayBurnAnimation(playedCard.Action);
			}
			DisableForgoneActions();
			TryUpdateGameState();
			LogState(newPhase);
			break;
		}
		case Phase.Pick2ndTarget:
		{
			ResetHighlight();
			Singleton<CardsHandManager>.Instance.CurrentHand.SetInteractable(interactable: true, allowHover: false);
			Choreographer.s_Choreographer.ShowAugmentationBar = true;
			CardActionPair playedCard3 = GetPlayedCard(1);
			playedCard3.Card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			playedCard3.Card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			playedCard3.Card.ToggleSelect(active: true, playedCard3.Action);
			playedCard3.Card.HighlightInfusions(highlight: true, playedCard3.Action);
			playedCard3.Card.ResetConsumes(clearSelection: false);
			if (!reloadStateToPickTargetPhase && CardsCount > 2)
			{
				int num = _playedCards.Count((CardActionPair x) => x.Action == CBaseCard.ActionType.TopAction || x.Action == CBaseCard.ActionType.DefaultAttackAction);
				int num2 = _playedCards.Count((CardActionPair x) => x.Action == CBaseCard.ActionType.BottomAction || x.Action == CBaseCard.ActionType.DefaultMoveAction);
				if (num == 2 || num2 == 2)
				{
					GetNonPlayedCard(0).ToggleSideInteractivity(active: false, playedCard3.Action);
				}
			}
			TryUpdateGameState();
			LogState(newPhase);
			break;
		}
		case Phase.Select3rdCard:
		{
			Singleton<CardsHandManager>.Instance.CurrentHand.SetInteractable(interactable: true, allowHover: true);
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			CardActionPair playedCard7 = GetPlayedCard(0);
			CardActionPair playedCard8 = GetPlayedCard(1);
			playedCard8.Card.HighlightInfusions(highlight: false, playedCard8.Action);
			playedCard8.Card.SetInteractable(active: false);
			playedCard8.Card.ResetConsumes(clearSelection: false);
			foreach (FullAbilityCard nonPlayedCard3 in _nonPlayedCards)
			{
				nonPlayedCard3.ToggleSideInteractivity(active: true, CBaseCard.ActionType.TopAction);
				nonPlayedCard3.ToggleSideInteractivity(active: true, CBaseCard.ActionType.BottomAction);
				if (CardsCount - 1 == _playedCards.Count && !GameState.IsPerformingCardOutOfPhase && playedCard7.Action == playedCard8.Action)
				{
					nonPlayedCard3.ToggleSideInteractivity(active: false, playedCard8.Action);
				}
			}
			if (!reloadStateToPickTargetPhase && _currentPhase == Phase.Pick3rdTarget && !GameState.IsPerformingCardOutOfPhase)
			{
				CardActionPair playedCard9 = GetPlayedCard(2);
				playedCard9.Card.Deselect();
				playedCard9.Card.ResetConsumes();
				MarkCardNonPlayed(playedCard9.Card);
				foreach (FullAbilityCard nonPlayedCard4 in _nonPlayedCards)
				{
					nonPlayedCard4.ToggleSideInteractivity(active: true, CBaseCard.ActionType.TopAction);
					nonPlayedCard4.ToggleSideInteractivity(active: true, CBaseCard.ActionType.BottomAction);
				}
				int num3 = _playedCards.Count((CardActionPair x) => x.Action == CBaseCard.ActionType.TopAction || x.Action == CBaseCard.ActionType.DefaultAttackAction);
				int num4 = _playedCards.Count((CardActionPair x) => x.Action == CBaseCard.ActionType.BottomAction || x.Action == CBaseCard.ActionType.DefaultMoveAction);
				if (num3 == 2 || num4 == 2)
				{
					playedCard9.Card.ToggleSideInteractivity(active: false, playedCard8.Action);
				}
			}
			else
			{
				playedCard8.Card.TryPlayBurnAnimation(playedCard8.Action);
			}
			DisableForgoneActions();
			TryUpdateGameState();
			LogState(newPhase);
			break;
		}
		case Phase.Pick3rdTarget:
		{
			ResetHighlight();
			Singleton<CardsHandManager>.Instance.CurrentHand.SetInteractable(interactable: true, allowHover: false);
			Choreographer.s_Choreographer.ShowAugmentationBar = true;
			CardActionPair playedCard6 = GetPlayedCard(2);
			playedCard6.Card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			playedCard6.Card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			playedCard6.Card.ToggleSelect(active: true, playedCard6.Action);
			playedCard6.Card.HighlightInfusions(highlight: true, playedCard6.Action);
			playedCard6.Card.ResetConsumes(clearSelection: false);
			TryUpdateGameState();
			LogState(newPhase);
			break;
		}
		default:
			Singleton<CardsHandManager>.Instance?.CurrentHand?.SetInteractable(interactable: true, allowHover: true);
			Singleton<UIUseAugmentationsBar>.Instance?.Hide();
			break;
		}
		if (GameState.CurrentOutOfPhaseCardSelectionAbility != null && (newPhase == Phase.Select1stCard || newPhase == Phase.Select2ndCard || newPhase == Phase.Select3rdCard))
		{
			foreach (AbilityCardUI item in Singleton<CardsHandManager>.Instance.CurrentHand.AllCards.Where((AbilityCardUI x) => x.Mode == CardHandMode.ActionSelection && x.fullAbilityCard.isActiveAndEnabled))
			{
				if (!(item.fullAbilityCard == _firstCard) && !(item.fullAbilityCard == _secondCard) && !(item.fullAbilityCard == _thirdCard))
				{
					bool isValid2 = !FFSNetwork.IsOnline || (Singleton<CardsHandManager>.Instance.CurrentHand.PlayerActor?.IsUnderMyControl() ?? false);
					item.fullAbilityCard.SetInteractable(active: true);
					item.fullAbilityCard.SetValid(isValid2, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
					item.fullAbilityCard.Deselect();
					item.fullAbilityCard.ResetConsumes();
					item.fullAbilityCard.ResetHighlightInfusions();
					item.fullAbilityCard.HighlightAvailableConsumes(InfusionBoardUI.Instance?.GetAvailableElements());
				}
			}
		}
		_currentPhase = newPhase;
	}

	private bool CheckDeckPresence(FullAbilityCard card)
	{
		if (card?.AbilityCard == null)
		{
			return false;
		}
		if (_actor?.CharacterClass == null)
		{
			return false;
		}
		if (_extraTurnType == CAbilityExtraTurn.EExtraTurnType.None && !_actor.CharacterClass.RoundAbilityCards.Any((CAbilityCard c) => c.ID == card.AbilityCard.ID) && !_actor.CharacterClass.SelectedAbilityCardsOutOfPhase.Any((CAbilityCard c) => c.ID == card.AbilityCard.ID))
		{
			card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			card.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			return true;
		}
		return false;
	}

	private void DisableForgoneActions()
	{
		if (!GameState.IsPerformingCardOutOfPhase)
		{
			int remainingActions = GameState.GetRemainingActions(_actor as CPlayerActor, top: true);
			int remainingActions2 = GameState.GetRemainingActions(_actor as CPlayerActor, top: false);
			if (remainingActions == 0)
			{
				_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
				_secondCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
				_thirdCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
			}
			if (remainingActions2 == 0)
			{
				_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
				_secondCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
				_thirdCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
			}
		}
		CheckDeckPresence(_firstCard);
		CheckDeckPresence(_secondCard);
		CheckDeckPresence(_thirdCard);
	}

	private GameState.EActionFlag ActionTypeToActionFlags(CBaseCard.ActionType actionType)
	{
		switch (actionType)
		{
		case CBaseCard.ActionType.TopAction:
		case CBaseCard.ActionType.DefaultAttackAction:
			return GameState.EActionFlag.TopPlayed;
		case CBaseCard.ActionType.DefaultMoveAction:
		case CBaseCard.ActionType.BottomAction:
			return GameState.EActionFlag.BottomPlayed;
		default:
			return GameState.EActionFlag.None;
		}
	}

	private CBaseCard.ActionType ActionFlagsToActionType(GameState.EActionFlag flags)
	{
		if (flags.HasFlag(GameState.EActionFlag.TopPlayed) || flags.HasFlag(GameState.EActionFlag.TopForgone))
		{
			return CBaseCard.ActionType.TopAction;
		}
		if (flags.HasFlag(GameState.EActionFlag.BottomPlayed) || flags.HasFlag(GameState.EActionFlag.BottomForgone))
		{
			return CBaseCard.ActionType.BottomAction;
		}
		return CBaseCard.ActionType.NA;
	}

	private void MarkCardPlayed(FullAbilityCard card, CBaseCard.ActionType action)
	{
		if (!(card == null))
		{
			int num = _playedCards.FindIndex((CardActionPair x) => x.Card == card);
			if (num >= 0)
			{
				_playedCards[num] = new CardActionPair(card, action);
				return;
			}
			_nonPlayedCards.Remove(card);
			_playedCards.Add(new CardActionPair(card, action));
		}
	}

	private void MarkCardNonPlayed(FullAbilityCard card)
	{
		if (!(card == null) && _nonPlayedCards.IndexOf(card) < 0)
		{
			int num = _playedCards.FindIndex((CardActionPair x) => x.Card == card);
			if (num >= 0)
			{
				_playedCards.RemoveAt(num);
			}
			_nonPlayedCards.Add(card);
		}
	}

	private CardActionPair GetPlayedCard(int index)
	{
		if (index >= 0 && index < _playedCards.Count)
		{
			return _playedCards[index];
		}
		return new CardActionPair(null, CBaseCard.ActionType.NA);
	}

	private FullAbilityCard GetNonPlayedCard(int index)
	{
		if (index >= 0 && index < _nonPlayedCards.Count)
		{
			return _nonPlayedCards[index];
		}
		return null;
	}

	public bool HasPlayedCards()
	{
		return _playedCards.Count > 0;
	}

	private bool TryUpdateGameState()
	{
		if (ScenarioManager.IsFastReloading)
		{
			return false;
		}
		if (GameState.IsPerformingCardOutOfPhase)
		{
			return false;
		}
		GameState.ClearPlayedCards(_extraTurnType, _actor.ActorGuid);
		foreach (CardActionPair playedCard in _playedCards)
		{
			int iD = playedCard.Card.AbilityCard.ID;
			GameState.EActionFlag flags = ActionTypeToActionFlags(playedCard.Action);
			GameState.AddPlayedCard(iD, flags, _extraTurnType, _actor.ActorGuid);
		}
		return true;
	}

	private bool ReloadState()
	{
		ResetCards();
		List<Tuple<int, GameState.EActionFlag>> playedCards = GameState.GetPlayedCards(_extraTurnType, _actor.ActorGuid);
		foreach (Tuple<int, GameState.EActionFlag> playedCard in playedCards)
		{
			FullAbilityCard fullAbilityCard = _nonPlayedCards.FirstOrDefault((FullAbilityCard x) => x.AbilityCard.ID == playedCard.Item1);
			if (fullAbilityCard == null)
			{
				break;
			}
			CBaseCard.ActionType action = ActionFlagsToActionType(playedCard.Item2);
			MarkCardPlayed(fullAbilityCard, action);
		}
		bool reloadStateToPickTargetPhase = false;
		if (_currentPhase == Phase.Pick1stTarget || _currentPhase == Phase.Pick2ndTarget || _currentPhase == Phase.Pick3rdTarget)
		{
			reloadStateToPickTargetPhase = true;
		}
		else if (playedCards.Count == 0)
		{
			SetPhase(Phase.Select1stCard);
		}
		else if (playedCards.Count == 1 && _secondCard != null)
		{
			SetPhase(Phase.Select2ndCard);
		}
		else
		{
			if (playedCards.Count != 2 || !(_thirdCard != null))
			{
				Finish();
				return true;
			}
			SetPhase(Phase.Select3rdCard);
		}
		Phase currentPhase = _currentPhase;
		SetPhase(Phase.Select1stCard, reloadStateToPickTargetPhase);
		if (currentPhase == Phase.Select1stCard)
		{
			return true;
		}
		CardActionPair playedCard2 = GetPlayedCard(0);
		OnCardClicked(playedCard2.Card, playedCard2.Action, reloadStateToPickTargetPhase, isReloadingState: true);
		if (currentPhase == Phase.Pick1stTarget)
		{
			return true;
		}
		OnActionFinished(reloadStateToPickTargetPhase);
		if (currentPhase == Phase.Select2ndCard)
		{
			return true;
		}
		CardActionPair playedCard3 = GetPlayedCard(1);
		OnCardClicked(playedCard3.Card, playedCard3.Action, reloadStateToPickTargetPhase, isReloadingState: true);
		if (currentPhase == Phase.Pick2ndTarget)
		{
			return true;
		}
		OnActionFinished(reloadStateToPickTargetPhase);
		if (currentPhase == Phase.Select3rdCard)
		{
			return true;
		}
		CardActionPair playedCard4 = GetPlayedCard(2);
		OnCardClicked(playedCard4.Card, playedCard4.Action, reloadStateToPickTargetPhase, isReloadingState: true);
		return true;
	}

	public bool OnFastReload()
	{
		if (PhaseManager.PhaseType != CPhase.PhaseType.ActionSelection)
		{
			return false;
		}
		if (!(_actor?.Class is CCharacterClass cCharacterClass))
		{
			return false;
		}
		if (_actor.IsTakingExtraTurn)
		{
			return false;
		}
		if (GameState.IsPerformingCardOutOfPhase)
		{
			return false;
		}
		if (cCharacterClass.RoundAbilityCardsSelectedInCardSelection.Count == 0)
		{
			return false;
		}
		if (cCharacterClass.RoundAbilityCardsSelectedInCardSelection.Count == cCharacterClass.RoundAbilityCards.Count)
		{
			return false;
		}
		if (cCharacterClass.RoundAbilityCards.Count == 0)
		{
			Finish();
			return true;
		}
		if (ReloadState())
		{
			return true;
		}
		SetPhase(Phase.Select1stCard);
		return false;
	}

	public void OnCardHighlight(bool isActive, FullAbilityCard abilityCard, bool isTopSide, bool isDefaultAbility)
	{
		if ((_currentPhase == Phase.Select1stCard || _currentPhase == Phase.Select2ndCard || _currentPhase == Phase.Select3rdCard) && (_firstCard == abilityCard || _secondCard == abilityCard || _thirdCard == abilityCard))
		{
			abilityCard.ToggleHighlightHover(isActive, isTopSide, isDefaultAbility);
		}
	}

	private void ResetHighlight()
	{
		_firstCard.ToggleHover(active: false, isTopSide: true);
		_firstCard.ToggleHover(active: false, isTopSide: false);
		_secondCard?.ToggleHover(active: false, isTopSide: true);
		_secondCard?.ToggleHover(active: false, isTopSide: false);
		_thirdCard?.ToggleHover(active: false, isTopSide: true);
		_thirdCard?.ToggleHover(active: false, isTopSide: false);
		_firstCard.UntoggleHighlightHover(isTopSide: true);
		_firstCard.UntoggleHighlightHover(isTopSide: false);
		_secondCard?.UntoggleHighlightHover(isTopSide: true);
		_secondCard?.UntoggleHighlightHover(isTopSide: false);
		_thirdCard?.UntoggleHighlightHover(isTopSide: true);
		_thirdCard?.UntoggleHighlightHover(isTopSide: false);
	}

	public CBaseCard.ActionType? GetAction(CAbilityCard card)
	{
		int num = _playedCards.FindIndex((CardActionPair x) => x.Card.AbilityCard == card);
		if (num >= 0)
		{
			return _playedCards[num].Action;
		}
		return null;
	}

	private static void RefreshConsumeBar(CActor actor, CAbility ability, CBaseCard.ActionType actionType, FullAbilityCard card, bool skipAugmentGroups)
	{
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability?.GetMiscVar("DontShowConsumesWhileChoosing").GetBool() == true)
		{
			Singleton<UIUseAugmentationsBar>.Instance.Hide(clearSelection: true);
			return;
		}
		Choreographer.s_Choreographer.ShowAugmentationBar = false;
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.TopAction)
		{
			bool flag = actionType == CBaseCard.ActionType.BottomAction;
			CAction cAction = (flag ? card.AbilityCard.BottomAction : card.AbilityCard.TopAction);
			if (cAction == null)
			{
				Singleton<UIUseAugmentationsBar>.Instance.Hide(clearSelection: true);
				return;
			}
			List<CActionAugmentation> list = cAction.Augmentations.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				CActionAugmentation aug = list[num];
				if (PhaseManager.CurrentPhaseAction != null && PhaseManager.CurrentPhaseAction.ActionAugmentationsConsumed.Any((CActionAugmentation x) => x.Name == aug.Name))
				{
					list.RemoveAt(num);
				}
			}
			ConsumeButton[] cardActionButtons = (flag ? card.bottomActionButton.ConsumeButtons : card.topActionButton.ConsumeButtons);
			List<UIUseAugmentationsBar.ConsumeGroupParam> augmentationGroupParams = (skipAugmentGroups ? new List<UIUseAugmentationsBar.ConsumeGroupParam>() : (GetAugmentationGroups(cAction.Augmentations) ?? new List<UIUseAugmentationsBar.ConsumeGroupParam>()));
			List<UIUseAugmentationsBar.ConsumeAugmentationParam> list2 = (from augmentParams in GetSingleAugmentations(actor, list, cardActionButtons)
				where augmentationGroupParams == null || !augmentationGroupParams.Any((UIUseAugmentationsBar.ConsumeGroupParam @group) => @group.Augmentations.Contains(augmentParams.Augmentation))
				select augmentParams).ToList();
			if ((list2 != null && list2.Count > 0) || (augmentationGroupParams != null && augmentationGroupParams.Count > 0))
			{
				Singleton<UIUseAugmentationsBar>.Instance.Show(actor, ability, list2, augmentationGroupParams);
				return;
			}
		}
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
	}

	private static IEnumerable<UIUseAugmentationsBar.ConsumeAugmentationParam> GetSingleAugmentations(CActor actor, IEnumerable<CActionAugmentation> augmentations, IReadOnlyCollection<ConsumeButton> cardActionButtons)
	{
		List<CActionAugmentation> actionAugmentationsAvailable = ((PhaseManager.CurrentPhase is CPhaseAction cPhaseAction) ? cPhaseAction.ActionAugmentationsAvailableForCurrentAbility() : null);
		foreach (CActionAugmentation aug in augmentations)
		{
			if ((aug.CostAbility == null || !aug.CostAbility.GetMiscVar("CheckCanBePerformedWhenAugment").GetBool() || aug.CostAbility.CanBePerformed(actor)) && !(actionAugmentationsAvailable?.All((CActionAugmentation a) => a.Name != aug.Name) ?? false))
			{
				ConsumeButton consumeButton = cardActionButtons.FirstOrDefault((ConsumeButton b) => b.AbilityConsume != null && b.AbilityConsume.ConsumeData != null && b.AbilityConsume.ConsumeData.Name == aug.Name);
				yield return new UIUseAugmentationsBar.ConsumeAugmentationParam
				{
					Augmentation = aug,
					ConsumeButton = consumeButton
				};
			}
		}
	}

	private static List<UIUseAugmentationsBar.ConsumeGroupParam> GetAugmentationGroups(List<CActionAugmentation> augmentations)
	{
		CPhase currentPhase = PhaseManager.CurrentPhase;
		CPhaseAction phaseAction = currentPhase as CPhaseAction;
		if (phaseAction != null)
		{
			List<CActionAugmentation> actionAugmentationsAvailable = phaseAction.ActionAugmentationsAvailableForCurrentAbility();
			return (from it in augmentations
				where actionAugmentationsAvailable.Any((CActionAugmentation x) => x.Name == it.Name) && it.ConsumeGroup.IsNOTNullOrEmpty()
				group it by it.ConsumeGroup into it
				where !it.Any(phaseAction.HasConsumedActionAugmentation)
				select new UIUseAugmentationsBar.ConsumeGroupParam
				{
					ConsumeGroupKey = it.Key,
					Augmentations = it.ToList()
				}).ToList();
		}
		return null;
	}

	public void RefreshConsumeBar(CActor actor, CAbility ability, bool skipAugmentGroups = false)
	{
		if (ability.AutoTrigger && (ability is CAbilityHeal || ability is CAbilityRetaliate || ability is CAbilityShield))
		{
			return;
		}
		CBaseCard abilityBaseCard = ability.GetAbilityBaseCard();
		CAbilityCard abilityCard = abilityBaseCard as CAbilityCard;
		if (abilityCard != null)
		{
			CAction action = ability.GetAction();
			if (action != null)
			{
				CBaseCard.ActionType actionType = CBaseCard.ActionType.NA;
				if (abilityCard.TopAction == action)
				{
					actionType = CBaseCard.ActionType.TopAction;
				}
				else if (abilityCard.BottomAction == action)
				{
					actionType = CBaseCard.ActionType.BottomAction;
				}
				else if (abilityCard.DefaultAttackAction == action)
				{
					actionType = CBaseCard.ActionType.DefaultAttackAction;
				}
				else if (abilityCard.DefaultMoveAction == action)
				{
					actionType = CBaseCard.ActionType.DefaultMoveAction;
				}
				if (actionType != CBaseCard.ActionType.NA)
				{
					AbilityCardUI abilityCardUI = Singleton<CardsHandManager>.InstanceFast.GetHand(actor.ID, throwError: false)?.GetCard(abilityCard.ID, throwError: false);
					if (abilityCardUI == null)
					{
						abilityCardUI = Singleton<CardsHandManager>.InstanceFast.cardHandsUI.Select((CardsHandUI hand) => hand.GetCard(abilityCard.ID, throwError: false)).FirstOrDefault((AbilityCardUI card) => card != null);
					}
					if (abilityCardUI != null)
					{
						RefreshConsumeBar(actor, ability, actionType, abilityCardUI.fullAbilityCard, skipAugmentGroups);
						return;
					}
				}
			}
		}
		int num = _currentPhase switch
		{
			Phase.Pick1stTarget => 0, 
			Phase.Pick2ndTarget => 1, 
			Phase.Pick3rdTarget => 2, 
			_ => -1, 
		};
		if (num > -1)
		{
			CardActionPair playedCard = GetPlayedCard(num);
			RefreshConsumeBar(actor, ability, playedCard.Action, playedCard.Card, skipAugmentGroups);
		}
		else
		{
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
		}
	}

	private void Finish()
	{
		foreach (CardActionPair playedCard in _playedCards)
		{
			playedCard.Card.TryPlayBurnAnimation(playedCard.Action);
			playedCard.Card.HighlightInfusions(highlight: false, playedCard.Action);
		}
		_firstCard.Deselect();
		_secondCard?.Deselect();
		_thirdCard?.Deselect();
		_firstCard.SetInteractable(active: false);
		_secondCard?.SetInteractable(active: false);
		_thirdCard?.SetInteractable(active: false);
		_firstCard.ResetConsumes(clearSelection: false);
		_secondCard?.ResetConsumes(clearSelection: false);
		_thirdCard?.ResetConsumes(clearSelection: false);
		_firstCard.HighlightAvailableConsumes(null);
		_secondCard?.HighlightAvailableConsumes(null);
		_thirdCard?.HighlightAvailableConsumes(null);
		SetPhase(Phase.None);
	}

	private void AfterItemUseAtEndOfTurn()
	{
		_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
		_firstCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
		_secondCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
		_secondCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
		_thirdCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
		_thirdCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
		foreach (CardActionPair playedCard in _playedCards)
		{
			playedCard.Card.TryPlayBurnAnimation(playedCard.Action);
			playedCard.Card.HighlightInfusions(highlight: false, playedCard.Action);
		}
		_firstCard?.Deselect();
		_secondCard?.Deselect();
		_thirdCard?.Deselect();
		_firstCard?.SetInteractable(active: false);
		_secondCard?.SetInteractable(active: false);
		_thirdCard?.SetInteractable(active: false);
		_firstCard?.ResetConsumes(clearSelection: false);
		_secondCard?.ResetConsumes(clearSelection: false);
		_thirdCard?.ResetConsumes(clearSelection: false);
		_firstCard?.HighlightAvailableConsumes(null);
		_secondCard?.HighlightAvailableConsumes(null);
		_thirdCard?.HighlightAvailableConsumes(null);
		SetPhase(Phase.None);
	}

	public void OnCardClicked(FullAbilityCard card, CBaseCard.ActionType actionType, bool reloadStateToPickTargetPhase = false, bool isReloadingState = false)
	{
		if (!(card == null))
		{
			if (!isReloadingState)
			{
				Singleton<UIUseItemsBar>.Instance.Hide();
			}
			if (_currentPhase == Phase.Select1stCard)
			{
				MarkCardPlayed(card, actionType);
				SetPhase(Phase.Pick1stTarget, reloadStateToPickTargetPhase);
			}
			else if (_currentPhase == Phase.Select2ndCard)
			{
				MarkCardPlayed(card, actionType);
				SetPhase(Phase.Pick2ndTarget, reloadStateToPickTargetPhase);
			}
			else if (_currentPhase == Phase.Select3rdCard)
			{
				MarkCardPlayed(card, actionType);
				SetPhase(Phase.Pick3rdTarget, reloadStateToPickTargetPhase);
			}
			else
			{
				Log.GUIError("Unexpected phase - " + _currentPhase);
			}
		}
	}

	public void OnActionFinished(bool reloadStateToPickTargetPhase = false)
	{
		if (_currentPhase == Phase.Pick1stTarget)
		{
			if (_secondCard != null)
			{
				SetPhase(Phase.Select2ndCard, reloadStateToPickTargetPhase);
			}
			else
			{
				Finish();
			}
		}
		else if (_currentPhase == Phase.Pick2ndTarget)
		{
			if (_thirdCard != null)
			{
				SetPhase(Phase.Select3rdCard, reloadStateToPickTargetPhase);
			}
			else
			{
				Finish();
			}
		}
		else if (_currentPhase == Phase.Pick3rdTarget)
		{
			Finish();
		}
		else if (_currentPhase == Phase.None)
		{
			AfterItemUseAtEndOfTurn();
		}
		else if (_currentPhase != Phase.Select1stCard && _currentPhase != Phase.Select2ndCard && _currentPhase != Phase.Select3rdCard)
		{
			Log.GUIError("Unexpected phase - " + _currentPhase);
		}
	}

	public void Reset()
	{
		SetPhase(Phase.None);
	}

	public void RefreshPhase()
	{
		SetPhase(_currentPhase);
	}

	public void OnActionUndo()
	{
		InfusionBoardUI.Instance?.UnreserveElements();
		if (_currentPhase == Phase.Pick1stTarget)
		{
			Choreographer.s_Choreographer.onCharacterAbilityComplete = null;
			SetPhase(Phase.Select1stCard);
		}
		else if (_currentPhase == Phase.Pick2ndTarget)
		{
			Choreographer.s_Choreographer.onCharacterAbilityComplete = null;
			SetPhase(Phase.Select2ndCard);
		}
		else if (_currentPhase == Phase.Pick3rdTarget)
		{
			Choreographer.s_Choreographer.onCharacterAbilityComplete = null;
			SetPhase(Phase.Select3rdCard);
		}
		else
		{
			Log.GUIError("Unexpected phase - " + _currentPhase);
		}
	}

	public void PickAnyInfusionElements(CActor actorPicking, Action<bool> onPicked = null)
	{
		if (_currentPhase == Phase.Pick1stTarget)
		{
			PickUnselectedInfusions(GetPlayedCard(0).Card, GetPlayedCard(0).Action, onPicked, actorPicking);
		}
		else if (_currentPhase == Phase.Pick2ndTarget)
		{
			PickUnselectedInfusions(GetPlayedCard(1).Card, GetPlayedCard(1).Action, onPicked, actorPicking);
		}
		else if (_currentPhase == Phase.Pick3rdTarget)
		{
			PickUnselectedInfusions(GetPlayedCard(2).Card, GetPlayedCard(2).Action, onPicked, actorPicking);
		}
		else
		{
			onPicked?.Invoke(obj: false);
		}
	}

	private void PickUnselectedInfusions(FullAbilityCard card, CBaseCard.ActionType abilityType, Action<bool> onFinish, CActor actorPicking)
	{
		if (abilityType == CBaseCard.ActionType.DefaultAttackAction || abilityType == CBaseCard.ActionType.DefaultMoveAction)
		{
			onFinish?.Invoke(obj: false);
			return;
		}
		List<InfuseElement> infusions = card.UnselectedInfusions(abilityType);
		if (infusions.Count == 0)
		{
			onFinish?.Invoke(obj: false);
			return;
		}
		if (FFSNetwork.IsOnline)
		{
			if (!actorPicking.IsUnderMyControl())
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ElementPicking, savePreviousState: true);
			}
			else
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ElementPicking, savePreviousState: true);
			}
		}
		Singleton<UIUseAbilitiesBar>.Instance.ShowInfusionsAction(actorPicking as CPlayerActor, card, (abilityType == CBaseCard.ActionType.BottomAction) ? card.AbilityCard.BottomAction : card.AbilityCard.TopAction, infusions, () =>
		{
			Singleton<UIUseAbilitiesBar>.Instance.Hide();
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				foreach (InfuseElement item in infusions)
				{
					_ = item;
					((CPhaseAction)PhaseManager.CurrentPhase).ElementsToInfuse.Remove(ElementInfusionBoardManager.EElement.Any);
				}
			}
			onFinish?.Invoke(obj: true);
		});
	}
}
