using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FFSNet;
using FHGUI.Scenario;
using FHRL;
using GLOOM;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardsHandManager : Singleton<CardsHandManager>
{
	public CPlayerActor m_PushPopCardHandManagerPlayer;

	public CardHandMode m_PushPopCardHandMode;

	public CardPileType m_PushPopCardPileType;

	[SerializeField]
	private GameObject cardsHandPrefab;

	[SerializeField]
	private Transform holderTransform;

	[SerializeField]
	private Transform highlightTransform;

	[SerializeField]
	private CardsHandTabs handTabs;

	[SerializeField]
	private Button viewAllDecksButton;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactability;

	[SerializeField]
	private UIWindow window;

	public CardsActionControlller cardsActionController;

	[SerializeField]
	private GameObject headerGO;

	[SerializeField]
	[StateDump(Skip = true)]
	private CardsHandPreviewWindow previewDecksWindow;

	[SerializeField]
	public LongRestConfirmationButton longRestConfirmationButton;

	[NonSerialized]
	public List<CardsHandUI> cardHandsUI = new List<CardsHandUI>();

	private CardsHandUI currentHand;

	private bool isFullCardPreviewAllowed;

	private CPlayerActor activePlayer;

	private CPlayerActor previewPlayer;

	private bool enableCancelActiveAbilities = true;

	private List<CardPileType> selectableCardTypes;

	private int maxCardsSelected;

	private bool fadeUnselectableCards;

	private bool highlightSelectableCards;

	private CardsHandUI.CardActionsCommand resetCardActionsPhase;

	private bool isFullDeckPreviewAllowed;

	private bool isShown;

	public CardsHandUI CurrentHand => currentHand;

	public CardHandViewMode ViewMode { get; private set; }

	public bool IsFullCardPreviewShowing => cardHandsUI.Any((CardsHandUI a) => a.IsFullCardPreviewShowing);

	public bool isPreviewing { get; private set; }

	[ConsoleVariable(Alias = "auto_select_player_cards", Persistent = false)]
	public static bool AutoSelectPlayerCardsCVar
	{
		get
		{
			return GameState.s_AutoSelectPlayerCards;
		}
		set
		{
			GameState.s_AutoSelectPlayerCards = value;
			Singleton<CardsHandManager>.InstanceFast?.TryAutoSelectPlayerCards();
		}
	}

	public bool IsShown => isShown;

	public bool EnableCancelActiveAbilities
	{
		get
		{
			return enableCancelActiveAbilities;
		}
		set
		{
			enableCancelActiveAbilities = value;
			InitiativeTrack.Instance.SetActiveBonusInteractable(enableCancelActiveAbilities);
		}
	}

	public bool IsFullCardPreviewAllowed
	{
		get
		{
			return isFullCardPreviewAllowed;
		}
		set
		{
			isFullCardPreviewAllowed = value;
			if (!isFullCardPreviewAllowed)
			{
				CloseViewHeroCardsAll();
			}
		}
	}

	public bool IsFullDeckPreviewAllowed
	{
		get
		{
			return isFullCardPreviewAllowed;
		}
		set
		{
			isFullDeckPreviewAllowed = value;
			viewAllDecksButton.gameObject.SetActive(value);
			if (!isFullDeckPreviewAllowed)
			{
				previewDecksWindow.Cancel();
			}
		}
	}

	public void SetupInteractability(InteractabilityIsolatedUIControl control = null)
	{
		if (control == null)
		{
			control = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (control != null)
		{
			interactability = new InteractabilityIsolatedUIControl.InteractabilityReference
			{
				control = control,
				onInteractabilityChanged = () =>
				{
					RefreshActive();
				}
			};
		}
	}

	private void RefreshActive()
	{
		if (!(this == null) && !destroyCancellationToken.IsCancellationRequested)
		{
			if (isShown && (interactability == null || interactability.cachedEnabled))
			{
				ShowHands();
			}
			else
			{
				HideHands();
			}
		}
	}

	private void Start()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			viewAllDecksButton.onClick.AddListener(() =>
			{
				ShowAllDecks(openByKey: false);
			});
		}
		SubscribeToControllableChanges();
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			longRestConfirmationButton.Init();
		}
	}

	protected override void AwakenedDestroy()
	{
		if (viewAllDecksButton != null)
		{
			viewAllDecksButton.onClick.RemoveAllListeners();
		}
		cardHandsUI.Clear();
		handTabs = null;
		previewDecksWindow = null;
		window = null;
		UnsubscribeFromControllableChanges();
	}

	public bool ShowAllDecks(bool openByKey)
	{
		if (!isFullCardPreviewAllowed)
		{
			return false;
		}
		CloseViewHeroCardsAll();
		window.Hide();
		previewDecksWindow.Show(m_PushPopCardHandMode == CardHandMode.CardsSelection, () =>
		{
			if (isShown)
			{
				window.Show();
			}
		}, openByKey);
		return true;
	}

	public bool IsShowingPlayerHand(CPlayerActor playerActor)
	{
		if (window.IsOpen && currentHand != null && currentHand.IsShown)
		{
			return currentHand.PlayerActor == playerActor;
		}
		return false;
	}

	public void AddPlayer(CPlayerActor playerData)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		CardsHandUI cardsHand = null;
		bool flag = false;
		if (!IsHandAvailable(playerData))
		{
			flag = true;
			cardsHand = UnityEngine.Object.Instantiate(cardsHandPrefab, holderTransform).GetComponent<CardsHandUI>();
			cardsHand.gameObject.name = cardsHandPrefab.name + " - " + playerData.Class.AbilityCardArt;
			cardsHand.OnHide.AddListener(() =>
			{
				if (CurrentHand == cardsHand)
				{
					ToggleShown(val: false);
				}
			});
			cardsHand.OnShow.AddListener(() =>
			{
				window.Show();
				Singleton<ScenarioInfoModule>.Instance.ShowBattleGoals(playerData);
			});
			cardHandsUI.Add(cardsHand);
		}
		else
		{
			cardsHand = GetHand(playerData);
		}
		cardsHand.Init(playerData, highlightTransform, cardsActionController);
		cardsHand.ToggleFullCardsPreview(active: false);
		if (flag)
		{
			previewDecksWindow.CreatePreview(cardsHand);
			Singleton<FanHandManager>.Instance.CreateHandFan(cardsHand);
		}
		List<CPlayerActor> players = cardHandsUI.Select((CardsHandUI hand) => hand.PlayerActor).ToList();
		handTabs.Init(players);
		Log.Trace($"CardsHandManager.AddPlayer took {stopwatch.ElapsedMilliseconds}ms");
	}

	public void UpdateElements(CPlayerActor playerActor)
	{
		CardsHandUI cardsHandUI = cardHandsUI.SingleOrDefault((CardsHandUI s) => s.PlayerActor == playerActor);
		if (!(cardsHandUI == null))
		{
			cardsHandUI.UpdateElements();
		}
	}

	public void Reset()
	{
		bool keepRecycledCardsActiveState = ObjectPool.KeepRecycledCardsActiveState;
		ObjectPool.KeepRecycledCardsActiveState = true;
		foreach (CardsHandUI item in cardHandsUI)
		{
			Singleton<FanHandManager>.Instance.RemoveHandFan(item);
			item.RecycleCards();
			UnityEngine.Object.Destroy(item.gameObject);
		}
		cardHandsUI.Clear();
		Singleton<CardsHandPreviewWindow>.Instance.Reset();
		Singleton<FanHandManager>.Instance.Reset();
		ObjectPool.KeepRecycledCardsActiveState = keepRecycledCardsActiveState;
	}

	public void OnAfterLoad()
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.OnAfterLoad();
		}
	}

	public CardsHandUI GetHand(int actorID, bool throwError = true)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (item.PlayerActor.ID == actorID)
			{
				return item;
			}
		}
		if (throwError)
		{
			Log.GUIWarning("Requested card hand could not be found. Actor ID: " + actorID);
		}
		return null;
	}

	public bool IsHandAvailable(CPlayerActor playerActor)
	{
		return cardHandsUI.Any((CardsHandUI a) => a.PlayerActor == playerActor || a.PlayerActor.CharacterClass == playerActor.CharacterClass);
	}

	public CardsHandUI GetHand(CPlayerActor cPlayer, bool throwError = true)
	{
		if (cPlayer != null)
		{
			foreach (CardsHandUI item in cardHandsUI)
			{
				if (item.PlayerActor == cPlayer || item.PlayerActor.CharacterClass == cPlayer.CharacterClass)
				{
					return item;
				}
			}
		}
		if (!throwError)
		{
			return null;
		}
		throw new Exception(string.Format("Hand not found for player {0} in {1} hands", cPlayer?.CharacterName ?? "NULL", cardHandsUI?.Count));
	}

	public void SwitchHand(CPlayerActor cPlayer)
	{
		StopPreview();
		activePlayer = cPlayer;
		foreach (CardsHandUI item in cardHandsUI.OrderBy((CardsHandUI it) => (it.PlayerActor != cPlayer) ? 1 : 0))
		{
			if (item.PlayerActor == cPlayer)
			{
				Log.GUIInfoFormat("Switch hand {0} from {1}", cPlayer.CharacterClass.DefaultModel, currentHand?.PlayerActor?.CharacterClass?.DefaultModel);
				currentHand = item;
				item.Show();
			}
			else
			{
				item.Hide();
			}
		}
		handTabs.SelectTab(cPlayer);
		Choreographer.s_Choreographer.OnSwitchHand(cPlayer);
		longRestConfirmationButton.Hide();
		ToggleShown(val: true);
		Singleton<FHScenarioViewController>.Instance.RefreshView();
	}

	private void ToggleShown(bool val)
	{
		isShown = val;
		RefreshActive();
	}

	private void ShowHands()
	{
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: true);
		previewDecksWindow.Cancel();
		window.Show();
		if (activePlayer != null)
		{
			Singleton<ScenarioInfoModule>.Instance.ShowBattleGoals(activePlayer);
		}
		UpdateCurrentHandView();
		TryAutoSelectPlayerCards();
	}

	public bool ShouldAutoSelectPlayerCards()
	{
		if (!GameState.s_AutoSelectPlayerCards)
		{
			return false;
		}
		if (m_PushPopCardHandMode != CardHandMode.CardsSelection)
		{
			return false;
		}
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			return false;
		}
		return true;
	}

	public bool TryAutoSelectPlayerCards()
	{
		if (!ShouldAutoSelectPlayerCards())
		{
			return false;
		}
		foreach (CardsHandUI item in cardHandsUI)
		{
			AutoSelectPlayerCards(item);
		}
		return true;
	}

	private void AutoSelectPlayerCards(CardsHandUI hand)
	{
		FHRandom fHRandom = null;
		int count;
		do
		{
			count = hand.SelectedCards.Count;
			if (count < hand.MaxCardsSelected)
			{
				if (fHRandom == null)
				{
					fHRandom = new FHRandom(ScenarioManager.CurrentScenarioState.Seed + GameState.RoundCount * 10000 + hand.PlayerActor.ID * 100 + hand.SelectedCards.Count);
				}
				AutoSelectPlayerCard(hand, fHRandom);
				continue;
			}
			break;
		}
		while (hand.SelectedCards.Count > count || TryAutoShortRest(hand, fHRandom));
	}

	private void AutoSelectPlayerCard(CardsHandUI hand, FHRandom rnd)
	{
		List<AbilityCardUI> list = (from x in hand.AllCards.Except(hand.SelectedCards)
			where x.CardType == CardPileType.Hand && x.IsSelectable && x.AbilityCard != null
			select x).ToList();
		if (list.Count != 0)
		{
			int index = rnd.Next(list.Count);
			list[index].ToggleSelect(active: true, highlight: false, networkAction: false);
		}
	}

	private bool TryAutoShortRest(CardsHandUI hand, FHRandom rnd)
	{
		CPlayerActor playerActor = hand.PlayerActor;
		if (playerActor == null)
		{
			return false;
		}
		CCharacterClass characterClass = playerActor.CharacterClass;
		if (characterClass == null)
		{
			return false;
		}
		if (characterClass.DiscardedAbilityCards.Count < 2)
		{
			return false;
		}
		int index = rnd.Next(characterClass.DiscardedAbilityCards.Count);
		CAbilityCard abilityCard = characterClass.DiscardedAbilityCards[index];
		characterClass.MoveAbilityCard(abilityCard, characterClass.DiscardedAbilityCards, characterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
		while (characterClass.DiscardedAbilityCards.Count > 0)
		{
			CAbilityCard abilityCard2 = characterClass.DiscardedAbilityCards[0];
			characterClass.MoveAbilityCard(abilityCard2, characterClass.DiscardedAbilityCards, characterClass.HandAbilityCards, "DiscardedAbilityCards", "HandAbilityCards");
		}
		hand.ResetViewToCachedSettings();
		return true;
	}

	public void Undo(CPlayerActor cPlayer)
	{
		GetHand(cPlayer).Undo();
		cardsActionController.OnActionUndo();
	}

	public void Reset(CPlayerActor cPlayer)
	{
		cardsActionController.Reset();
	}

	public void RefreshCardsActionControllerPhase()
	{
		cardsActionController.OnFastReload();
	}

	public void ToggleViewHeroCards(CPlayerActor player)
	{
		if (isFullCardPreviewAllowed)
		{
			StopPreview();
		}
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.ToggleFullCardsPreview(item.PlayerActor == player && isFullCardPreviewAllowed);
		}
	}

	public void ToggleViewHeroCards(int handIndex)
	{
		if (handIndex < 0 || handIndex >= cardHandsUI.Count)
		{
			ToggleViewHeroCards(null);
		}
		else
		{
			ToggleViewHeroCards(cardHandsUI[handIndex].PlayerActor);
		}
	}

	public void CloseViewHeroCards(int handIndex)
	{
		if (handIndex >= 0 && handIndex < cardHandsUI.Count)
		{
			cardHandsUI[handIndex].ToggleFullCardsPreview(active: false);
		}
	}

	private void CloseViewHeroCardsAll()
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.ToggleFullCardsPreview(active: false);
		}
	}

	public void ShowLongRestConfirmation(CPlayerActor playerActor, Action<bool> onToggledConfirmLongRest)
	{
		Show(playerActor, CardHandMode.ActionSelection, CardPileType.None, CardPileType.None);
		longRestConfirmationButton.ShowConfirm(playerActor, onToggledConfirmLongRest);
		handTabs.UpdateTabsInteraction(interactable: false);
	}

	public void ShowTabs(bool show)
	{
		bool flag = !FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant);
		handTabs.gameObject.SetActive((show && PhaseManager.IsInCardsSelectionPhase) & flag);
		if (handTabs.gameObject.activeSelf)
		{
			handTabs.UpdateTabsInteraction();
		}
	}

	public void ShowLongRested(CPlayerActor playerActor)
	{
		Show(playerActor, CardHandMode.Preview, CardPileType.None, CardPileType.None);
		longRestConfirmationButton.ShowUsed(playerActor);
	}

	public void Show(CPlayerActor playerActor, CardHandMode mode, CardPileType filterType, CardPileType selectableCardType = CardPileType.Any, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, bool allowFullCardPreview = true, CardsHandUI.CardActionsCommand resetCardActions = CardsHandUI.CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, bool allowFullDeckPreview = true, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardSelectableFilter = null, Func<CAbilityCard, bool> cardVisibleFilter = null)
	{
		Show(playerActor, mode, filterType, new List<CardPileType> { selectableCardType }, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, allowFullCardPreview, resetCardActions, forceUseCurrentRoundCards, allowFullDeckPreview, overrideCardCallback, cardSelectableFilter, cardVisibleFilter);
	}

	public void Show(CPlayerActor playerActor, CardHandMode mode, CardPileType filterType, List<CardPileType> selectableCardTypes, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, bool allowFullCardPreview = true, CardsHandUI.CardActionsCommand resetCardActionsPhase = CardsHandUI.CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, bool allowFullDeckPreview = true, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardSelectableFilter = null, Func<CAbilityCard, bool> cardVisibleFilter = null)
	{
		m_PushPopCardHandManagerPlayer = playerActor;
		m_PushPopCardHandMode = mode;
		m_PushPopCardPileType = filterType;
		this.selectableCardTypes = selectableCardTypes;
		this.maxCardsSelected = maxCardsSelected;
		this.fadeUnselectableCards = fadeUnselectableCards;
		this.highlightSelectableCards = highlightSelectableCards;
		this.resetCardActionsPhase = resetCardActionsPhase;
		IsFullCardPreviewAllowed = allowFullCardPreview;
		IsFullDeckPreviewAllowed = allowFullDeckPreview;
		ShowSelectionElements(show: true);
		previewDecksWindow.SetCardsSelectable(mode == CardHandMode.CardsSelection);
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: true);
		longRestConfirmationButton.Hide();
		SwitchHand(playerActor);
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				item.UpdateView(mode, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActionsPhase, forceUseCurrentRoundCards, overrideCardCallback, cardSelectableFilter, cardVisibleFilter);
				item.SetInteractable(!item.PlayerActor.IsDead, !item.PlayerActor.IsDead);
				return;
			}
		}
		ToggleShown(val: true);
		if (mode != CardHandMode.CardsSelection)
		{
			Choreographer.s_Choreographer.PlayerToSelectAbilityCardsOrLongRest = false;
		}
	}

	public void UpdateOriginalExtraTurnCards(CPlayerActor playerActor)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				item.UpdateOriginalExtraTurnCards(playerActor);
			}
		}
	}

	private void ShowSelectionElements(bool show)
	{
		bool flag = !FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant);
		if (handTabs != null)
		{
			handTabs.gameObject.SetActive((show && PhaseManager.IsInCardsSelectionPhase) & flag);
			if (handTabs.gameObject.activeSelf)
			{
				handTabs.UpdateTabsInteraction();
			}
		}
		if (headerGO != null)
		{
			headerGO.SetActive((show & flag) && (PhaseManager.IsInCardsSelectionPhase || (m_PushPopCardHandMode == CardHandMode.CardsSelection && m_PushPopCardHandManagerPlayer != null && m_PushPopCardHandManagerPlayer.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None) || m_PushPopCardHandMode == CardHandMode.LoseCard || m_PushPopCardHandMode == CardHandMode.DiscardCard || m_PushPopCardHandMode == CardHandMode.RecoverDiscardedCard || m_PushPopCardHandMode == CardHandMode.RecoverDiscardedCard || m_PushPopCardHandMode == CardHandMode.SelectCardsWithActiveBonusTracker || m_PushPopCardHandMode == CardHandMode.SelectCardsWithTideActiveBonusesToKeep || m_PushPopCardHandMode == CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate));
		}
	}

	public void Show(CardHandMode mode, CardPileType filterType, List<CardPileType> selectableCardTypes, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool allowFullCardPreview = true, bool allowFullDeckPreview = true)
	{
		m_PushPopCardHandMode = mode;
		m_PushPopCardPileType = filterType;
		IsFullCardPreviewAllowed = allowFullCardPreview;
		IsFullDeckPreviewAllowed = allowFullDeckPreview;
		this.selectableCardTypes = selectableCardTypes;
		this.maxCardsSelected = maxCardsSelected;
		this.fadeUnselectableCards = fadeUnselectableCards;
		longRestConfirmationButton.Hide();
		ShowSelectionElements(show: true);
		Log.GUIInfo("ShowHandManager");
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (mode == CardHandMode.CardsSelection && maxCardsSelected == -12345)
			{
				this.maxCardsSelected = item.PlayerActor.MaxCardsClampedToHand;
			}
			item.UpdateView(mode, filterType, selectableCardTypes, this.maxCardsSelected, fadeUnselectableCards);
			item.SetInteractable(!item.PlayerActor.IsDeadForObjectives, !item.PlayerActor.IsDeadForObjectives);
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			handTabs.Reset();
		}
		previewDecksWindow.SetCardsSelectable(mode == CardHandMode.CardsSelection);
		ToggleShown(val: true);
		if (mode != CardHandMode.CardsSelection)
		{
			Choreographer.s_Choreographer.PlayerToSelectAbilityCardsOrLongRest = false;
		}
	}

	public static CPlayerActor DecidePlayerToShowCardsFor()
	{
		if (IsPlayerValidForNormalSelection(Singleton<CardsHandManager>.Instance?.currentHand?.PlayerActor) && PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest && Singleton<CardsHandManager>.Instance.currentHand.PlayerActor.CharacterClass.HasShortRested)
		{
			return Singleton<CardsHandManager>.Instance.currentHand.PlayerActor;
		}
		CPlayerActor cPlayerActor = null;
		CPlayerActor cPlayerActor2 = null;
		List<CPlayerActor> list = ScenarioManager.Scenario?.PlayerActors;
		if (list == null)
		{
			return null;
		}
		foreach (CPlayerActor item in list)
		{
			if (item.IsDead)
			{
				continue;
			}
			if (cPlayerActor == null)
			{
				cPlayerActor = item;
			}
			if (item.IsUnderMyControl())
			{
				if (cPlayerActor2 == null)
				{
					cPlayerActor2 = item;
				}
				if (item.CharacterClass.RoundAbilityCards.Count < item.MaxRoundCardsCount && !item.CharacterClass.LongRest)
				{
					return item;
				}
			}
		}
		if (cPlayerActor2 != null)
		{
			return cPlayerActor2;
		}
		return cPlayerActor;
	}

	public static bool IsPlayerValidForNormalSelection(CPlayerActor player)
	{
		if (player == null)
		{
			return false;
		}
		if (player.IsDead)
		{
			return false;
		}
		if (!player.IsUnderMyControl())
		{
			return false;
		}
		return true;
	}

	public static void ShowCardsSelection(CPlayerActor playerActor = null, bool refreshValid = false)
	{
		CardsHandManager instance = Singleton<CardsHandManager>.Instance;
		if (instance == null)
		{
			return;
		}
		if (playerActor == null)
		{
			SaveData instance2 = SaveData.Instance;
			playerActor = (((object)instance2 == null || instance2.Global?.GameMode != EGameMode.FrontEndTutorial) ? DecidePlayerToShowCardsFor() : instance.currentHand?.PlayerActor);
		}
		Choreographer.s_Choreographer.PlayerToSelectAbilityCardsOrLongRest = true;
		Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.WaitingForCardSelection, 0, null);
		Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		instance.EnableCancelActiveAbilities = true;
		instance.Show(CardHandMode.CardsSelection, CardPileType.Any, new List<CardPileType>
		{
			CardPileType.Hand,
			CardPileType.Active,
			CardPileType.Round
		}, -12345);
		if (playerActor != null)
		{
			instance.SwitchHand(playerActor);
			if (refreshValid)
			{
				instance.GetHand(playerActor).RefreshValidCards();
			}
		}
	}

	public void UseFanViewMode(bool useFan)
	{
		CardHandViewMode viewMode = (useFan ? CardHandViewMode.Fan : CardHandViewMode.Column);
		SetViewMode(viewMode);
	}

	private void SetViewMode(CardHandViewMode viewMode)
	{
		ViewMode = viewMode;
		UpdateCurrentHandView();
	}

	private void UpdateCurrentHandView()
	{
		CardHandViewMode cardHandViewMode = (PhaseManager.IsInCardsSelectionPhase ? ViewMode : CardHandViewMode.Column);
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.abilityCardsHolder.gameObject.SetActive(cardHandViewMode == CardHandViewMode.Column);
		}
		Singleton<FanHandManager>.Instance.UpdateHandViewMode();
	}

	public void SetHandInteractable(bool interactable)
	{
		previewDecksWindow.SetCardsSelectable(interactable && m_PushPopCardHandMode == CardHandMode.CardsSelection);
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable);
		RefreshValidCards();
	}

	public void RefreshValidCards()
	{
		cardHandsUI.ForEach((CardsHandUI x) =>
		{
			x.RefreshValidCards();
		});
	}

	public void DisableHands()
	{
		previewDecksWindow.SetCardsSelectable(selectable: false);
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: false);
		cardHandsUI.ForEach((CardsHandUI x) =>
		{
			x.SetInteractable(interactable: false, allowHover: false);
		});
	}

	public void ResetShortRestFlags()
	{
		cardHandsUI.ForEach((CardsHandUI x) =>
		{
			x.ResetShortRestUIFlag();
		});
	}

	public void UnselectShortRest()
	{
		cardHandsUI.ForEach((CardsHandUI x) =>
		{
			x.UnselectShortRest();
		});
	}

	public bool IsActive()
	{
		return handTabs.gameObject.activeSelf;
	}

	public void Hide(CPlayerActor playerActor = null)
	{
		if (playerActor == null)
		{
			ShowSelectionElements(show: false);
		}
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor != null && item.PlayerActor == playerActor)
			{
				item.Hide();
			}
		}
		ToggleShown(val: false);
	}

	private void HideHands()
	{
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: false);
		previewDecksWindow?.Hide();
		window.Hide();
		Singleton<ScenarioInfoModule>.Instance.HideBattleGoals(currentHand?.PlayerActor);
		Singleton<FanHandManager>.Instance.UpdateHandViewMode();
	}

	public void Preview(CPlayerActor playerActor, Transform cardsHighlightHolder)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				if (m_PushPopCardHandMode != CardHandMode.IncreaseCardLimit)
				{
					isPreviewing = true;
					previewPlayer = playerActor;
					item.PreviewActionCards(cardsHighlightHolder);
				}
				break;
			}
		}
	}

	public void StopPreview()
	{
		if (!isPreviewing)
		{
			return;
		}
		isPreviewing = false;
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (previewPlayer == item.PlayerActor)
			{
				if (m_PushPopCardHandMode != CardHandMode.IncreaseCardLimit)
				{
					item.HidePreviewActionCards(m_PushPopCardPileType, m_PushPopCardHandMode);
				}
				break;
			}
		}
	}

	public List<AbilityCardUI> GetSelectedCards(int actorID)
	{
		return GetHand(actorID)?.SelectedCards;
	}

	public void OnSelectedCardsNumberChanged(CPlayerActor playerActor, int cardsNumber)
	{
		if (PhaseManager.IsInCardsSelectionPhase)
		{
			handTabs.UpdateSelectedCardsNum(playerActor, cardsNumber);
		}
	}

	public void UpdateDeadPlayer(CActor player)
	{
		bool flag = false;
		handTabs.UpdateTabsInteraction();
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (player == item.PlayerActor)
			{
				flag = item.gameObject.activeSelf;
				item.SetInteractable(interactable: false, allowHover: false);
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		flag = isShown;
		foreach (CardsHandUI item2 in cardHandsUI)
		{
			if (!item2.PlayerActor.IsDead)
			{
				SwitchHand(item2.PlayerActor);
				break;
			}
		}
		if (!flag && isShown)
		{
			ToggleShown(val: false);
		}
	}

	public void DiscardActiveAbilityCard(CPlayerActor playerActor, CAbilityCard card, UnityAction onFinished)
	{
		if (EnableCancelActiveAbilities && card.CanBeRemovedFromActiveBonuses())
		{
			LocalizationManager.TryGetTranslation("GUI_CONFIRMATION_CANCEL_" + card.Name + "_DESCR", out var translation);
			Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation(card.Name), card.SelectedAction.CardPile != CBaseCard.ECardPile.Discarded, () =>
			{
				CancelActiveAbility(playerActor, card);
				NetworkCancelingActiveBonuses(playerActor, card);
				onFinished?.Invoke();
			}, translation, onFinished);
		}
	}

	public void CancelTideActiveBonuses(CPlayerActor playerActor, IList<int> tideActiveBonusCardsToKeep)
	{
		bool flag = false;
		foreach (CActiveBonus cancellableTideActiveBonuse in CustomAbility.GetCancellableTideActiveBonuses(playerActor))
		{
			if (tideActiveBonusCardsToKeep == null || !tideActiveBonusCardsToKeep.Contains(cancellableTideActiveBonuse.BaseCard.ID))
			{
				flag = true;
				CancelActiveAbility(playerActor, (CAbilityCard)cancellableTideActiveBonuse.BaseCard, animateCards: false, processImmediately: false);
			}
		}
		ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
		{
			CAbility cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: false, "", new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, "CrashingTidePreventMoreTideCancelsAfterFirst");
			cAbility.ActiveBonusData.Flags = AbilityData.EActiveBonusFlag.DontCancelTidesOnRest;
			cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
			cAbility.ActiveBonusData.Hidden = true;
			cAbility.Start(playerActor, playerActor);
			cAbility.ApplyToActor(playerActor);
		}, "DONT_CANCEL_TIDE_BONUSES_ANYMORE");
		if (flag)
		{
			string arg = LocalizationManager.GetTranslation(playerActor.ActorLocKey()) + Choreographer.GetActorIDForCombatLogIfNeeded(playerActor);
			Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation("COMBAT_LOG_RESET_TIDES"), arg), CombatLogFilter.ABILITIES);
		}
	}

	public void CancelActiveAbility(CPlayerActor playerActor, CAbilityCard card, bool animateCards = true, bool processImmediately = true)
	{
		List<CPlayerActor> list = new List<CPlayerActor>();
		bool flag = false;
		bool allowFromMainThread = GameState.ThreadIsSleeping | processImmediately;
		foreach (CActiveBonus activeBonus in card.ActiveBonuses.ToList())
		{
			if (activeBonus.IsAura)
			{
				flag = true;
			}
			if (activeBonus.Actor.Type == CActor.EType.Player)
			{
				list.Add(activeBonus.Actor as CPlayerActor);
			}
			if (activeBonus.Caster.Type == CActor.EType.Player)
			{
				list.Add(activeBonus.Caster as CPlayerActor);
			}
			else if (activeBonus.Actor.Type == CActor.EType.HeroSummon)
			{
				list.Add((activeBonus.Actor as CHeroSummonActor).Summoner);
			}
			if (activeBonus.Actor is CPlayerActor playerActor2 && activeBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && cAbilityAddActiveBonus.AddAbility is CAbilityImprovedShortRest)
			{
				CancelImprovedShortRest(playerActor2);
			}
			ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
			{
				CClass.CancelActiveBonus(activeBonus, allowFromMainThread);
			}, "CANCEL_ACTIVE_ABILITY", allowFromMainThread);
		}
		Log.SimpleInfo(LocalizationManager.GetTranslation(playerActor.ActorLocKey()) + " removed active ability " + LocalizationManager.GetTranslation(card.Name));
		if (flag)
		{
			ScenarioManager.Scenario.PlayerActors.ForEach((CPlayerActor f) =>
			{
				RefreshCancelledActiveCardUI(f, card, animateCards);
			});
		}
		else
		{
			list.ForEach((CPlayerActor f) =>
			{
				RefreshCancelledActiveCardUI(f, card, animateCards);
			});
		}
		Choreographer.s_Choreographer?.RefreshViewController();
	}

	private void RefreshCancelledActiveCardUI(CPlayerActor playerActor, CAbilityCard card, bool animateCards = true)
	{
		InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = InitiativeTrack.Instance.FindInitiativeTrackActor(playerActor);
		if (!(initiativeTrackActorBehaviourBase != null))
		{
			return;
		}
		initiativeTrackActorBehaviourBase.Avatar.OnRefreshAbilities();
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				bool flag = IsShowingPlayerHand(playerActor) && currentHand == item && m_PushPopCardHandMode == CardHandMode.CardsSelection;
				item.RefreshCancelActiveAbility(m_PushPopCardPileType, card, flag & animateCards);
				break;
			}
		}
	}

	public void RefreshPendingActiveBonus(CPlayerActor playerActor)
	{
		CardsHandUI hand = GetHand(playerActor);
		if (!(hand == null))
		{
			AbilityCardUI abilityCardUI = hand.GetActiveAbilityCards().FirstOrDefault((AbilityCardUI it) => !playerActor.CharacterClass.ActivatedAbilityCards.Contains(it.AbilityCard));
			if (abilityCardUI != null)
			{
				RefreshCancelledActiveCardUI(playerActor, abilityCardUI.AbilityCard);
			}
		}
	}

	public void CancelImprovedShortRest(CPlayerActor playerActor)
	{
		playerActor.CharacterClass.ImprovedShortRest = false;
		playerActor.CharacterClass.ImprovedShortRestDrawCount = 0u;
		InitiativeTrack.Instance.FindInitiativeTrackActor(playerActor).RefreshAbilities();
	}

	public void OnAbilityStart(CActor actor, CAbility ability)
	{
		if (ability.GetAbilityBaseCard() == null || actor == null || !(actor is CPlayerActor))
		{
			return;
		}
		List<AbilityCardUI> selectedCards = GetSelectedCards(actor.ID);
		if (selectedCards != null)
		{
			AbilityCardUI abilityCardUI = selectedCards.FirstOrDefault((AbilityCardUI c) => c.CardID == ability.GetAbilityBaseCard().ID);
			if (abilityCardUI != null && abilityCardUI.fullAbilityCard != null)
			{
				abilityCardUI.fullAbilityCard.OnAbilityStart(ability.Name);
			}
		}
	}

	public void OnAbilityEnd(CActor actor, CAbility ability)
	{
		if (ability == null || ability.GetAbilityBaseCard() == null || actor == null || !(actor is CPlayerActor))
		{
			return;
		}
		List<AbilityCardUI> selectedCards = GetSelectedCards(actor.ID);
		if (selectedCards != null)
		{
			AbilityCardUI abilityCardUI = selectedCards.FirstOrDefault((AbilityCardUI c) => c.CardID == ability.GetAbilityBaseCard().ID);
			if (abilityCardUI != null && abilityCardUI.fullAbilityCard != null)
			{
				abilityCardUI.fullAbilityCard.OnAbilityEnd(ability.Name);
			}
		}
	}

	private void SubscribeToControllableChanges()
	{
		Log.NetworkInfo("Card Manager: Subscribing to controllable changes.");
		PlayerRegistry.MyControllablesChanged = (Action)Delegate.Combine(PlayerRegistry.MyControllablesChanged, new Action(OnMyControllableOwnershipChanged));
	}

	private void UnsubscribeFromControllableChanges()
	{
		Log.NetworkInfo("Card Manager: Unsubscribing from controllable changes.");
		PlayerRegistry.MyControllablesChanged = (Action)Delegate.Remove(PlayerRegistry.MyControllablesChanged, new Action(OnMyControllableOwnershipChanged));
	}

	private void OnMyControllableOwnershipChanged()
	{
		if (FFSNetwork.IsStartingUp || FFSNetwork.IsShuttingDown)
		{
			return;
		}
		RefreshConnectionState();
		if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
		{
			return;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			CardsHandUI cardsHandUI = currentHand;
			Log.NetworkInfo("OnControllableOwnershipChanged");
			handTabs?.Reset();
			ShowSelectionElements(PlayerRegistry.MyPlayer.IsParticipant);
			InitiativeTrack.Instance.SelectedActor()?.Deselect();
			Choreographer.s_Choreographer.ClearHilightedActors();
			if (!PlayerRegistry.MyPlayer.MyControllables.Any())
			{
				WorldspaceStarHexDisplay.Instance.ClearStars();
			}
			if (cardsHandUI?.PlayerActor != null && cardsHandUI.PlayerActor.IsUnderMyControl())
			{
				SwitchHand(cardsHandUI.PlayerActor);
			}
			else
			{
				foreach (CardsHandUI item in cardHandsUI)
				{
					if (!item.PlayerActor.IsDead && InitiativeTrack.Instance.Select(item.PlayerActor))
					{
						SwitchHand(item.PlayerActor);
						break;
					}
				}
			}
			{
				foreach (CardsHandUI item2 in cardHandsUI)
				{
					Log.NetworkInfo("Updating hand view for " + item2.PlayerActor.CharacterClass.ID);
					item2.ResetViewToCachedSettings();
					item2.SetInteractable(!item2.PlayerActor.IsDeadForObjectives, !item2.PlayerActor.IsDeadForObjectives);
				}
				return;
			}
		}
		Log.GUIInfo("OnMyControllableOwnershipChanged.UpdateView");
		foreach (CardsHandUI item3 in cardHandsUI)
		{
			if (m_PushPopCardHandManagerPlayer == item3.PlayerActor)
			{
				item3.UpdateView(m_PushPopCardHandMode, m_PushPopCardPileType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, CardsHandUI.CardActionsCommand.NONE);
				break;
			}
		}
	}

	public void ProxySelectRoundCards(GameAction action)
	{
		StartRoundCardsToken startRoundCardsToken = (StartRoundCardsToken)action.SupplementaryDataToken;
		if (startRoundCardsToken.FastReloadID != FastReload.CurrentHistoryID && startRoundCardsToken.FastReloadID != null && FastReload.CurrentHistoryID != null)
		{
			Log.NetworkWarning("[PROXY SELECT ROUND CARDS]: Ignoring command because fast reload checkpoints aren't synced. Expected " + FastReload.CurrentHistoryID + ", received " + startRoundCardsToken.FastReloadID);
		}
		else if (startRoundCardsToken.ShortRestSelected)
		{
			ProxySelectRoundCards(action.ActorID, new int[0], shortRestSelected: true);
		}
		else if (startRoundCardsToken.LongRestSelected)
		{
			ProxySelectRoundCards(action.ActorID, new int[1], shortRestSelected: false);
		}
		else
		{
			ProxySelectRoundCards(action.ActorID, ((StartRoundCardsToken)action.SupplementaryDataToken).RoundCardIDs, shortRestSelected: false);
		}
	}

	private void ProxySelectRoundCards(int controllableID, int[] selectedCardIDs, bool shortRestSelected)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			CardsHandUI hand = GetHand(cActor.ID);
			if (hand != null)
			{
				CPlayerActor cPlayerActor = (CPlayerActor)cActor;
				Log.NetworkTrace("[PROXY SELECT ROUND CARDS]: About to select round cards for " + cPlayerActor.CharacterClass.ID);
				hand.DeselectAllCards(networkAction: false);
				Log.NetworkTrace("[PROXY SELECT ROUND CARDS]: Deselected all previously selected cards.");
				foreach (int cardInstanceID in selectedCardIDs)
				{
					hand.ProxySelectCard(cardInstanceID);
				}
				if (cPlayerActor.CharacterClass.ShortRest != shortRestSelected)
				{
					cPlayerActor.CharacterClass.ShortRest = shortRestSelected;
					hand.UpdateInitiative();
					hand.OnSelectedCardsNumberChanged();
				}
				hand.UpdateShortRest();
				return;
			}
			throw new Exception("Error selecting round cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + cActor.ID + ").");
		}
		throw new Exception("Error selecting round cards for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxySelectExtraTurnCards(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.DeselectAllCards(networkAction: false);
			CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
			for (int num = cardsToken.CardIDs.Length - 1; num >= 0; num--)
			{
				hand.ProxySelectCard(cardsToken.CardIDs[num]);
			}
			return;
		}
		throw new Exception("Error selecting extra turn cards proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxySelectOutOfPhaseCards(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.DeselectAllCards(networkAction: false);
			CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
			for (int num = cardsToken.CardIDs.Length - 1; num >= 0; num--)
			{
				hand.ProxySelectCard(cardsToken.CardIDs[num]);
			}
			return;
		}
		throw new Exception("Error selecting extra turn cards proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxySelectDiscardedCardToPlay(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxySelectDiscardedCardToPlay(action.SupplementaryDataIDMax);
			return;
		}
		throw new Exception("Error selecting discarded card to play proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxySelectCardAbility(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
			hand.ProxySelectCardAction(action.SupplementaryDataIDMax, (CBaseCard.ActionType)action.SupplementaryDataIDMin);
			return;
		}
		throw new Exception("Error selecting card action for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyToggleAugment(GameAction action)
	{
		if (Choreographer.s_Choreographer == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00002", "Cannot toggle the enhancement. Choreographer not yet initialized.");
			return;
		}
		if (Choreographer.s_Choreographer.m_CurrentActor == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00003", "Cannot toggle the enhancement. Current actor not yet initialized.");
			return;
		}
		CActor currentActor = Choreographer.s_Choreographer.CurrentActor;
		if (currentActor != null)
		{
			CPlayerActor cPlayerActor = null;
			if (GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().ControllingActor is CPlayerActor cPlayerActor2)
			{
				cPlayerActor = cPlayerActor2;
			}
			else if (currentActor is CPlayerActor cPlayerActor3)
			{
				cPlayerActor = cPlayerActor3;
			}
			if (cPlayerActor != null)
			{
				AbilityAugmentToken abilityAugmentToken = (AbilityAugmentToken)action.SupplementaryDataToken;
				AbilityCardUI abilityCardUI = GetHand(cPlayerActor.ID)?.GetCard(abilityAugmentToken.CardID);
				if (abilityCardUI != null)
				{
					abilityCardUI.fullAbilityCard.ProxyToggleAugment(action.ActionTypeID == 34, abilityAugmentToken);
					return;
				}
				throw new Exception("Error trying to toggle an ability augment for proxy character. Card returns null (CardID: " + abilityAugmentToken.CardID + ").");
			}
			throw new Exception("Error trying to toggle an ability augment for proxy character. CPlayerActor returns null (ActorID: " + action.ActorID + ").");
		}
		throw new Exception("Error trying to toggle an ability augment for proxy character. CPlayerActor returns null (ActorID: " + action.ActorID + ").");
	}

	public void ProxyShortRest(GameAction action)
	{
		if (action.SupplementaryDataToken is StartRoundCardsToken startRoundCardsToken)
		{
			if (!(startRoundCardsToken.FastReloadID != FastReload.CurrentHistoryID))
			{
				ProxyShortRest(action.ActorID, startRoundCardsToken);
			}
		}
		else
		{
			UIManager.Instance.dialogPopup.ChooseOption(action.SupplementaryDataIDMin);
		}
	}

	private void ProxyShortRest(int controllableID, StartRoundCardsToken startRoundCardToken)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			CardsHandUI hand = GetHand(cActor.ID);
			if (hand != null)
			{
				hand.ProxyShortRest(startRoundCardToken);
				return;
			}
			throw new Exception("Error using short rest for proxy playerActor. Card Hand returns null for the actor (ActorID: " + cActor.ID + ").");
		}
		throw new Exception("Error using short rest for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxyLongRest(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyLongRest(action.SupplementaryDataIDMax);
			return;
		}
		throw new Exception("Error using long rest for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyRecoverCards(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyRecoverCards((CardsToken)action.SupplementaryDataToken, action.SupplementaryDataBoolean);
			Singleton<FHScenarioViewController>.Instance?.Modules?.ReactionFrozenFistRecoverFromSelection?.Close();
			return;
		}
		throw new Exception("Error recovering cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void NetworkCancelingActiveBonuses(CPlayerActor playerActor, CBaseCard baseCard)
	{
		if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl())
		{
			if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 57, FastReload.CurrentHistoryID);
			}
			else if (FFSNetwork.IsOnline)
			{
				int controllableID = ControllableRegistry.GetControllableID(playerActor);
				Synchronizer.SendGameAction(GameActionType.CancelActiveBonus, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(baseCard));
			}
		}
	}

	public void ProxyCancelActiveBonuses(GameAction action)
	{
		if (action.SupplementaryDataToken is StartRoundCardsToken startRoundCardsToken)
		{
			if (startRoundCardsToken.FastReloadID != FastReload.CurrentHistoryID || !(((CharacterManager)(ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject))?.CharacterActor is CPlayerActor cPlayerActor))
			{
				return;
			}
			for (int num = cPlayerActor.CharacterClass.ActivatedCards.Count - 1; num >= 0; num--)
			{
				CBaseCard cBaseCard = cPlayerActor.CharacterClass.ActivatedCards[num];
				if (!startRoundCardsToken.ActivatedCardIDs.Contains(cBaseCard.ID))
				{
					ProxyCancelActiveBonus(action.ActorID, cBaseCard.ID, cBaseCard.CardType);
				}
			}
		}
		else if (action.SupplementaryDataToken is CardsToken cardsToken && ((CharacterManager)(ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject))?.CharacterActor is CPlayerActor)
		{
			int[] cardIDs = cardsToken.CardIDs;
			foreach (int cardID in cardIDs)
			{
				ProxyCancelActiveBonus(action.ActorID, cardID, cardsToken.CardTypeEnum);
			}
		}
	}

	private void ProxyCancelActiveBonus(int controllableID, int cardID, CBaseCard.ECardType cardType)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			List<CActiveBonus> activeBonuses = cActor.ActiveBonuses;
			if (cActor is CPlayerActor cPlayerActor)
			{
				activeBonuses.AddRange(cPlayerActor.CharacterClass.ActivatedCards.SelectMany((CBaseCard x) => x.ActiveBonuses));
			}
			Log.NetworkTrace("About to cancel an active bonus (Character: " + (cActor as CPlayerActor).CharacterClass.CharacterID + ", Base Card ID: " + cardID + " CardType: " + cardType.ToString() + "). Listing all active bonuses on the character:");
			foreach (CActiveBonus item in activeBonuses)
			{
				Log.NetworkTrace("Active Bonus. Base Card Name: " + item.BaseCard.Name + ", Base Card ID: " + item.BaseCard.ID + ", Ability Name:" + item.Ability.Name);
			}
			List<CActiveBonus> list = activeBonuses.Where((CActiveBonus x) => x.BaseCard.ID == cardID && x.BaseCard.CardType == cardType).Distinct().ToList();
			if (list.Count > 0)
			{
				foreach (CActiveBonus item2 in list)
				{
					switch (item2.BaseCard.CardType)
					{
					case CBaseCard.ECardType.Item:
					case CBaseCard.ECardType.Pet:
						CClass.CancelActiveBonus(item2);
						InitiativeTrack.Instance.FindInitiativeTrackActor(item2.Actor)?.Avatar.OnRefreshAbilities();
						break;
					case CBaseCard.ECardType.CharacterAbility:
					{
						CardsHandUI hand = GetHand(item2.Caster.ID);
						AbilityCardUI abilityCardUI = hand?.GetCard(cardID, throwError: false);
						if (abilityCardUI == null)
						{
							foreach (CardsHandUI item3 in cardHandsUI)
							{
								if (!(item3 == hand))
								{
									abilityCardUI = item3.GetCard(cardID, throwError: false);
									if (abilityCardUI != null)
									{
										break;
									}
								}
							}
						}
						if (!(abilityCardUI != null))
						{
							throw new Exception("Error canceling an ability card base active bonus for proxy playerActor. No such card exists (ActorID: " + cActor.ID + ", CardID: " + cardID + ").");
						}
						CancelActiveAbility((CPlayerActor)cActor, abilityCardUI.AbilityCard);
						break;
					}
					case CBaseCard.ECardType.HeroSummonAbility:
					case CBaseCard.ECardType.MonsterAbility:
					case CBaseCard.ECardType.AttackModifier:
					case CBaseCard.ECardType.ScenarioModifier:
						throw new Exception("Error canceling an active ability bonus for proxy playerActor. Missing implementation for the card type: " + item2.BaseCard.CardType);
					default:
						throw new Exception("Error canceling an active ability bonus for proxy playerActor. Unidentified card type: " + item2.BaseCard.CardType);
					}
					Log.NetworkInfo("Successfully canceled the active bonus (Base Card Name: " + item2.BaseCard.Name + ", Base Card ID: " + item2.BaseCard.ID + ", Ability Name: " + item2.Ability.Name + ") from " + ((CPlayerActor)cActor).CharacterClass.ID);
				}
				return;
			}
			Log.NetworkInfo("No active bonuses were found to cancel");
			return;
		}
		throw new Exception("Error canceling an active ability bonus for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxyCancelImprovedShortRest(GameAction action)
	{
		ProxyCancelImprovedShortRest(action.ActorID);
	}

	private void ProxyCancelImprovedShortRest(int controllableID)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			CancelImprovedShortRest((CPlayerActor)cActor);
			return;
		}
		throw new Exception("Error canceling an active ability bonus for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxyToggleSelectLongRestAbility(GameAction gameAction)
	{
		longRestConfirmationButton.ProxyToggleSelectLongRestAbility(gameAction);
	}

	public void ClearStartRoundCardsTokenCache()
	{
		cardHandsUI.ForEach((CardsHandUI x) =>
		{
			x.CachedStartRoundToken = null;
		});
	}

	public void RefreshConnectionState()
	{
		if (handTabs != null)
		{
			handTabs.RefreshConnectionState();
		}
		if (longRestConfirmationButton != null)
		{
			longRestConfirmationButton.RefreshConnectionState();
		}
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (item != null)
			{
				item.RefreshConnectionState();
			}
		}
	}

	public void ProxyAbilityDiscardCard(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyAbilityDiscardCard((CardsToken)action.SupplementaryDataToken);
			return;
		}
		throw new Exception("Error discarding cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyAbilityLoseCard(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyAbilityLoseCard((CardsToken)action.SupplementaryDataToken);
			return;
		}
		throw new Exception("Error losing cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyAbilityMoveActiveBonusTracker(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyAbilityMoveActiveBonusTracker((CardsToken)action.SupplementaryDataToken, (IntToken)action.SupplementaryDataToken2);
			return;
		}
		throw new Exception("Error moving active bonus trackers for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyChangeAgility(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyChangeAgility((CPlayerActor.EAgility)action.SupplementaryDataIDMin);
			return;
		}
		throw new Exception("Error changing agility for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyChangePendingForm(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyChangePendingForm((CPlayerActor.EForm)action.SupplementaryDataIDMin);
			return;
		}
		throw new Exception("Error changing pending form for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void HandViewUpdated(CardsHandUI cardsHandUI)
	{
		Singleton<FanHandManager>.Instance.SyncHandView(cardsHandUI);
	}
}
