using System;
using System.Collections;
using System.Collections.Generic;
using DevLocker.GFrame.Input.UIScope;
using FFSNet;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class CardsHandPreviewUI : MonoBehaviour
{
	[SerializeField]
	private UICharacterStats characterStats;

	[SerializeField]
	private Transform abilityCardsHolder;

	[SerializeField]
	private CardHandHeader activeHeader;

	[SerializeField]
	private CardHandHeader burntHeader;

	[SerializeField]
	private CardHandHeader takenHeader;

	[SerializeField]
	private CardHandHeader discardedHeader;

	[SerializeField]
	private ItemsUI itemsUI;

	[SerializeField]
	private ModifiersDisplayController modifiersController;

	[SerializeField]
	private int fullCardCanvasOrder = 2;

	[SerializeField]
	private int fullCardPositionX = 270;

	[SerializeField]
	private int fullCardMirrorPositionX = -690;

	[SerializeField]
	private Vector2 preferredMiniCardSize;

	private List<AbilityCardUI> cardsUI = new List<AbilityCardUI>();

	private bool cardsSpawned;

	private CardsHandUI cardHand;

	private bool cardsSelectable;

	public bool invalidateCardSize;

	private AbilityCardUI hoveredCard;

	[field: SerializeField]
	public UINavigationGroup CardsNavigationGroup { get; private set; }

	[field: SerializeField]
	public UINavigationGroup ExtraCardsNavigationGroup { get; private set; }

	[field: SerializeField]
	public UINavigationGroup InventoryNavigationGroup { get; private set; }

	public CPlayerActor PlayerActor => cardHand.PlayerActor;

	public void ToggleAttackModifiers(bool active)
	{
		try
		{
			if (active)
			{
				modifiersController.Display(PlayerActor.CharacterClass, !FFSNetwork.IsOnline || PlayerActor.IsUnderMyControl());
			}
			else
			{
				modifiersController.Hide();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.ToggleAttackModifiers().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RecycleCards()
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		cardsUI.Clear();
	}

	private void OnDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			RecycleCards();
			cardsUI.Clear();
			cardHand = null;
			itemsUI = null;
			CardsNavigationGroup?.Exclude.Clear();
			CardsNavigationGroup = null;
			ExtraCardsNavigationGroup = null;
			InventoryNavigationGroup = null;
			abilityCardsHolder = null;
			activeHeader = null;
			burntHeader = null;
			discardedHeader = null;
			takenHeader = null;
		}
	}

	public void Init(CardsHandUI cardHand, bool showFullPreviewOnLeftSize = false)
	{
		try
		{
			this.cardHand = cardHand;
			if (!cardsSpawned)
			{
				SpawnCards(PlayerActor.CharacterClass.OwnedAbilityCards, showFullPreviewOnLeftSize);
			}
			characterStats.Init(PlayerActor);
			characterStats.Show(show: true);
			itemsUI.SetActor(PlayerActor, subscribeToEvents: false, force: true);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Init().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Hide()
	{
		try
		{
			gameObject.SetActive(value: false);
			ResetView();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Hide().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetView()
	{
		modifiersController.Hide(immendiately: true);
		if (hoveredCard != null)
		{
			hoveredCard.OnPointerExit();
			hoveredCard = null;
		}
	}

	public void Show(bool cardsSelectable)
	{
		try
		{
			this.cardsSelectable = cardsSelectable && cardHand.IsInteractable;
			UpdateView();
			itemsUI?.SetActor(PlayerActor, subscribeToEvents: false, force: true);
			gameObject.SetActive(value: true);
			UpdateTakenHeader();
			invalidateCardSize = true;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Show().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void UpdateView()
	{
		try
		{
			bool flag = !FFSNetwork.IsOnline || PlayerActor == null || PlayerActor.IsUnderMyControl();
			if (flag && GameState.s_ShortRestingPlayer != null)
			{
				flag = PlayerActor == GameState.s_ShortRestingPlayer;
			}
			bool flag2 = flag && cardsSelectable;
			if (flag2 && FFSNetwork.IsOnline)
			{
				flag2 = !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar;
			}
			bool flag3 = ScenarioScript.current != null && ScenarioScript.current.ShowTakenPile();
			foreach (AbilityCardUI item in cardsUI)
			{
				CopyState(item);
				item.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl());
				item.SetMode(item.Mode, new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Round
				});
				if (flag2)
				{
					item.SetValid(isValid: true);
					if (!cardsSelectable)
					{
						item.fullAbilityCard.SetValid(isValid: false);
					}
				}
				else if (!cardsSelectable)
				{
					item.SetValid(isValid: false);
				}
				else if (flag)
				{
					item.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
				}
				else
				{
					item.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
				}
				CPlayerActor playerActor = PlayerActor;
				bool flag4 = playerActor == null || !playerActor.CharacterClass.OwnedUnusedCards.Contains(item.AbilityCard);
				if (flag4 && !flag3)
				{
					CPlayerActor playerActor2 = PlayerActor;
					flag4 = playerActor2 == null || !playerActor2.CharacterClass.TakenCards.Contains(item.AbilityCard);
				}
				item.Toggle(flag4);
				item.gameObject.SetActive(flag4);
			}
			SortCards();
			UpdateEffects();
			UpdateCharacterSpecificMechanic();
			if (PlayerActor != null)
			{
				characterStats.Init(PlayerActor);
			}
			itemsUI.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl());
			modifiersController.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl());
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateView().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateCharacterSpecificMechanic()
	{
		UpdateGeminateModule();
	}

	private void LateUpdate()
	{
		if (invalidateCardSize)
		{
			invalidateCardSize = false;
			StartCoroutine(ExecuteDelayedRebuild());
		}
	}

	private IEnumerator ExecuteDelayedRebuild()
	{
		yield return null;
		abilityCardsHolder.GetComponent<GridLayoutGroup>()?.FitInParent(preferredMiniCardSize);
	}

	private void UpdateGeminateModule()
	{
		if (!(PlayerActor.Class.ID != "GeminateID"))
		{
			CPlayerActor.EForm form = PlayerActor.Form;
			CPlayerActor.EForm prevForm = PlayerActor.PrevForm;
			CPlayerActor.EForm eForm = PlayerActor.PendingForm;
			if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
			{
				eForm = form;
			}
			if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
			{
				eForm = prevForm;
			}
			if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
			{
				CPlayerActor.EForm eForm2 = (PlayerActor.PendingForm = CPlayerActor.EForm.Melee);
				eForm = eForm2;
			}
			RestrictGeminateCardsSelection(eForm);
		}
	}

	private void RestrictGeminateCardsSelection(CPlayerActor.EForm form)
	{
		if (cardHand.currentMode == CardHandMode.CardsSelection)
		{
			RestrictToForm(form);
		}
	}

	private void RestrictToForm(CPlayerActor.EForm form)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (!item.IsLongRest && item.CardType != CardPileType.Active && item.CardType != CardPileType.Discarded && item.CardType != CardPileType.Lost && item.CardType != CardPileType.Permalost && item.CardType != CardPileType.Taken)
			{
				bool flag = item.CardRequiredForm == form;
				item.SetSelectable(flag);
				item.SetFade(flag ? 1f : 0.3f);
			}
		}
		SortCards();
	}

	public void SetCardsSelectable(bool selectable)
	{
		bool flag = selectable && cardHand.IsInteractable;
		if (!gameObject.activeSelf || cardsSelectable == flag)
		{
			return;
		}
		cardsSelectable = flag;
		bool flag2 = !FFSNetwork.IsOnline || PlayerActor == null || (PlayerActor.IsUnderMyControl() && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
		foreach (AbilityCardUI item in cardsUI)
		{
			item.SetMode(CardHandMode.CardsSelection, new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			});
			if (flag2)
			{
				item.SetValid(isValid: true);
				if (!cardsSelectable)
				{
					item.fullAbilityCard.SetValid(isValid: false);
				}
			}
			else if (PlayerActor.IsUnderMyControl())
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
			}
			else
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			}
			item.SetUnfocused(FFSNetwork.IsOnline && PlayerActor != null && !PlayerActor.IsUnderMyControl());
		}
	}

	private void CopyState(AbilityCardUI cardUI)
	{
		AbilityCardUI card = cardHand.GetCard(cardUI.AbilityCard.ID);
		if (PlayerActor.CharacterClass.DiscardedAbilityCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Discarded);
		}
		else if (PlayerActor.CharacterClass.LostAbilityCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Lost);
		}
		else if (PlayerActor.CharacterClass.PermanentlyLostAbilityCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Permalost);
		}
		else if (PlayerActor.CharacterClass.TakenCards.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(CardPileType.Taken);
		}
		else
		{
			cardUI.SetType(card.CardType);
		}
		cardUI.UpdateCard();
	}

	private void UpdateEffects()
	{
		try
		{
			bool flag = !FFSNetwork.IsOnline || PlayerActor.IsUnderMyControl() || !PhaseManager.IsInCardsSelectionPhase;
			if ((PhaseManager.IsInCardsSelectionPhase && PlayerActor.CharacterClass.RoundAbilityCards.Count >= 1) & flag)
			{
				foreach (AbilityCardUI item in cardsUI)
				{
					item.PlayEffect((PlayerActor.CharacterClass.RoundAbilityCards[0] == item.AbilityCard) ? InitiativeTrack.InitiativeEffects.Active : InitiativeTrack.InitiativeEffects.None);
				}
				return;
			}
			if ((PlayerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater) & flag)
			{
				foreach (AbilityCardUI item2 in cardsUI)
				{
					item2.PlayEffect((PlayerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count > 0 && PlayerActor.CharacterClass.ExtraTurnInitiativeAbilityCard == item2.AbilityCard) ? InitiativeTrack.InitiativeEffects.Active : InitiativeTrack.InitiativeEffects.None);
				}
				return;
			}
			foreach (AbilityCardUI item3 in cardsUI)
			{
				item3.PlayEffect(InitiativeTrack.InitiativeEffects.None);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateEffects().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SortCards()
	{
		try
		{
			cardsUI.Sort();
			activeHeader.Hide();
			discardedHeader.Hide();
			burntHeader.Hide();
			takenHeader.Hide();
			List<CardHandHeader> list = new List<CardHandHeader>();
			foreach (AbilityCardUI item in cardsUI)
			{
				if (item.CardType == CardPileType.Unselected && !list.Contains(burntHeader))
				{
					list.Add(burntHeader);
					burntHeader.transform.SetAsLastSibling();
				}
				CardHandHeader header = GetHeader(item.CardType);
				if (header != null)
				{
					if (!list.Contains(header))
					{
						list.Add(header);
						header.transform.SetAsLastSibling();
					}
					if (item.gameObject.activeSelf)
					{
						header.Show();
					}
				}
				item.transform.SetAsLastSibling();
			}
			if (!list.Contains(burntHeader))
			{
				burntHeader.transform.SetAsLastSibling();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.SortCards().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private CardHandHeader GetHeader(CardPileType cardType)
	{
		switch (cardType)
		{
		case CardPileType.Hand:
		case CardPileType.Active:
			return null;
		case CardPileType.Discarded:
			return discardedHeader;
		case CardPileType.Lost:
		case CardPileType.Permalost:
			return burntHeader;
		case CardPileType.Taken:
			return takenHeader;
		default:
			return null;
		}
	}

	private void SpawnCards(List<CAbilityCard> abilityCards, bool showFullPreviewOnLeftSize = false)
	{
		cardsSpawned = true;
		cardsUI.Clear();
		foreach (CAbilityCard abilityCard in abilityCards)
		{
			if (abilityCard.GetAbilityCardYML == null)
			{
				Log.GUIError("Ability card data is null! Check YML file for " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
				component.Init(abilityCard, PlayerActor, CardPileType.Hand, OnSelectedCard, OnDeselectedCard, null, OnHovered, null, null, null, isLongRest: false, (RectTransform rect) => new Vector2(showFullPreviewOnLeftSize ? fullCardMirrorPositionX : fullCardPositionX, rect.anchoredPosition.y));
				component.transform.localPosition = Vector3.zero;
				UIRectTransformUtils.StretchToParent(component.miniAbilityCard.transform as RectTransform);
				cardsUI.Add(component);
				CardsNavigationGroup.Exclude.AddRange(component.fullAbilityCard.GetComponentsInChildren<Selectable>(includeInactive: true));
			}
			catch (Exception ex)
			{
				Log.GUIException(ex, "Unable to initialise ability card " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		invalidateCardSize = true;
	}

	private void UpdateTakenHeader()
	{
		if (ScenarioScript.current != null)
		{
			takenHeader.SetKeys(ScenarioScript.current.TakenPileNameKey(), ScenarioScript.current.TakenPileNameKey(), ScenarioScript.current.TakenPileTooltipKey());
		}
	}

	private void OnDeselectedCard(AbilityCardUI card, bool arg2)
	{
		if (PhaseManager.IsInCardsSelectionPhase && cardsSelectable)
		{
			cardHand.UnselectCard(card.AbilityCard);
			UpdateEffects();
		}
	}

	private void OnSelectedCard(AbilityCardUI card, bool arg2)
	{
		if (!PhaseManager.IsInCardsSelectionPhase || !cardsSelectable)
		{
			return;
		}
		cardHand.SelectCard(card.AbilityCard);
		foreach (AbilityCardUI item in cardsUI)
		{
			if (item.CardType == CardPileType.Round)
			{
				CopyState(item);
			}
		}
		UpdateEffects();
	}

	private void OnHovered(bool hovered, AbilityCardUI cardUI)
	{
		if (hovered)
		{
			hoveredCard = cardUI;
			cardUI.fullAbilityCard.HighlightInCanvas(fullCardCanvasOrder);
		}
		else if (hoveredCard == cardUI)
		{
			hoveredCard = null;
		}
	}
}
