using System;
using DevLocker.GFrame.Input;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class LoseCardsOptionsPanel : Singleton<LoseCardsOptionsPanel>
{
	private enum PreviewOptions
	{
		None,
		PreviewAvailableCards,
		PreviewDiscardedCards
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TMP_Text loseCardsText;

	[SerializeField]
	private FHToggle burnAvailableCardsToggle;

	[SerializeField]
	private FHToggle burnDiscardedCardsToggle;

	[SerializeField]
	private FHButton skipButton;

	[SerializeField]
	private GameObject buttonsContainer;

	private CPlayerActor actorToShowCardsFor;

	private WorldspacePanelUIController actorHUDController;

	private PreviewOptions currentlyPreviewing;

	private Toggle currentlyToggled;

	private bool hasEnoughAvailableCards;

	private bool hasEnoughDiscardedCards;

	public bool ThisPlayerHasTakeDamageControl
	{
		get
		{
			if (!FFSNetwork.IsOnline)
			{
				return true;
			}
			return actorToShowCardsFor.IsUnderMyControl();
		}
	}

	public bool IsOpen => window.IsOpen;

	protected override void AwakeInit()
	{
		burnAvailableCardsToggle.onValueChanged.AddListener(BurnAvailableCard);
		burnAvailableCardsToggle.PointerEntered += OnMouseEnterBurnOne;
		burnAvailableCardsToggle.PointerExited += OnMouseExitBurnOne;
		skipButton.onClick.AddListener(SkipLosingCards);
		burnDiscardedCardsToggle.onValueChanged.AddListener(BurnDiscardedCards);
		burnDiscardedCardsToggle.PointerEntered += OnMouseEnterBurnTwo;
		burnDiscardedCardsToggle.PointerExited += OnMouseExitBurnTwo;
	}

	public void Show(CPlayerActor actorToShowCardsFor)
	{
		try
		{
			this.actorToShowCardsFor = actorToShowCardsFor;
			buttonsContainer.SetActive(value: true);
			burnAvailableCardsToggle.gameObject.SetActive(value: true);
			burnDiscardedCardsToggle.gameObject.SetActive(value: true);
			skipButton.gameObject.SetActive(value: false);
			WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
			for (int i = 0; i < array?.Length; i++)
			{
				if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorToShowCardsFor))
				{
					actorHUDController = array[i];
					break;
				}
			}
			UpdateCardRemovalOptionVisuals();
			ClearPreviouslyPreviewed();
			ClearSelectedToggle();
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ChooseLoseCardsOptions));
			window.Show();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.Show().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ClearSelectedToggle()
	{
		currentlyToggled = null;
		RefreshPreviewing();
	}

	public void ClearPreviouslyPreviewed()
	{
		currentlyPreviewing = PreviewOptions.None;
	}

	private void UpdateCardRemovalOptionVisuals()
	{
		try
		{
			ResetToggles();
			bool thisPlayerHasTakeDamageControl = ThisPlayerHasTakeDamageControl;
			hasEnoughAvailableCards = actorToShowCardsFor != null && actorToShowCardsFor.CharacterClass.HandAbilityCards.Count > 0;
			burnAvailableCardsToggle.interactable = hasEnoughAvailableCards & thisPlayerHasTakeDamageControl;
			hasEnoughDiscardedCards = actorToShowCardsFor != null && actorToShowCardsFor.CharacterClass.DiscardedAbilityCards.Count > 1;
			burnDiscardedCardsToggle.interactable = hasEnoughDiscardedCards & thisPlayerHasTakeDamageControl;
			if (!hasEnoughAvailableCards && !hasEnoughDiscardedCards)
			{
				burnAvailableCardsToggle.gameObject.SetActive(value: false);
				burnDiscardedCardsToggle.gameObject.SetActive(value: false);
				skipButton.gameObject.SetActive(value: true);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.UpdateCardRemovalOptionVisuals().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00036", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ResetToggles()
	{
		burnAvailableCardsToggle.SetValue(value: false);
		burnDiscardedCardsToggle.SetValue(value: false);
		currentlyToggled = null;
		burnDiscardedCardsToggle.group.SetAllTogglesOff(sendCallback: false);
		Singleton<UIUseItemsBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.Hide();
	}

	public void OnMouseEnterBurnOne()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnAvailableCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Enter for button " + burnAvailableCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewAvailableCards();
		}
	}

	public void OnMouseExitBurnOne()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnAvailableCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Exit for button " + burnAvailableCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void OnMouseEnterBurnTwo()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnDiscardedCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Enter for button " + burnDiscardedCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewDiscardedCards();
		}
	}

	public void OnMouseExitBurnTwo()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnDiscardedCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Exit for button " + burnDiscardedCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void PreviewAvailableCards()
	{
		try
		{
			if (hasEnoughAvailableCards)
			{
				currentlyPreviewing = PreviewOptions.PreviewAvailableCards;
				Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = false;
				Singleton<CardsHandManager>.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Hand, 1, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_BURN_1_ABILITY_CARD"), Singleton<InputFHManager>.Instance.InputContext.GetDisplayTextFor(InputFHManager.Controls.Generic.Confirm)), LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE_BURN"));
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.PreviewAvailableCards().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00038", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PreviewDiscardedCards()
	{
		try
		{
			if (hasEnoughDiscardedCards)
			{
				currentlyPreviewing = PreviewOptions.PreviewDiscardedCards;
				Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = false;
				Singleton<CardsHandManager>.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 2, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_BURN_2_DISCARDED_ABILITY_CARDS"), Singleton<InputFHManager>.Instance.InputContext.GetDisplayTextFor(InputFHManager.Controls.Generic.Confirm)), LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE_DISCARD"));
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.PreviewDiscardedCards().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00039", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ResetPreviewing()
	{
		try
		{
			currentlyPreviewing = PreviewOptions.None;
			if (burnAvailableCardsToggle.Equals(currentlyToggled))
			{
				BurnAvailableCard();
			}
			else if (burnDiscardedCardsToggle.Equals(currentlyToggled))
			{
				BurnDiscardedCards();
			}
			else
			{
				HideCardPreview();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.ResetPreviewing().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00041", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RefreshPreviewing()
	{
		switch (currentlyPreviewing)
		{
		case PreviewOptions.None:
			HideCardPreview();
			break;
		case PreviewOptions.PreviewAvailableCards:
			PreviewAvailableCards();
			break;
		case PreviewOptions.PreviewDiscardedCards:
			PreviewDiscardedCards();
			break;
		default:
			Log.GUIError("Unknown preview option encountered for the Take Damage Panel.");
			break;
		}
	}

	public void BurnAvailableCard(bool toggledOn = true)
	{
		try
		{
			if (toggledOn)
			{
				UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseBurnAvailable));
				OnSelectedToggle(burnAvailableCardsToggle);
				PreviewAvailableCards();
			}
			else if (currentlyToggled == burnAvailableCardsToggle)
			{
				ClearSelectedToggle();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.BurnAvailableCard().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00042", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void HideCardPreview()
	{
		Singleton<CardsHandManager>.Instance.Hide();
	}

	public void SkipLosingCards()
	{
		ResetAndHide();
	}

	private void OnSelectedToggle(FHToggle toggle)
	{
		currentlyToggled = toggle;
		Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
	}

	public void BurnDiscardedCards(bool toggledOn = true)
	{
		try
		{
			if (toggledOn)
			{
				UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseBurnDiscarded));
				OnSelectedToggle(burnDiscardedCardsToggle);
				PreviewDiscardedCards();
			}
			else if (currentlyToggled == burnDiscardedCardsToggle)
			{
				ClearSelectedToggle();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the LoseCardsPanel.BurnDiscardedCards().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00046", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetAndHide(bool switchBackToSavedState = false)
	{
		Singleton<HelpBox>.Instance.Hide();
		ResetToggles();
		window.Hide();
		if ((FFSNetwork.IsOnline && ActionProcessor.HasSavedState) & switchBackToSavedState)
		{
			ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
		}
	}

	public void ToggleVisibility(bool visible)
	{
		if (window.IsOpen)
		{
			if (visible)
			{
				buttonsContainer.SetActive(value: true);
				return;
			}
			buttonsContainer.SetActive(value: false);
			Singleton<HelpBox>.Instance.Hide();
		}
	}

	public void ShowOtherPlayer(CPlayerActor actorToShowCardsFor)
	{
		this.actorToShowCardsFor = actorToShowCardsFor;
		WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
		for (int i = 0; i < array?.Length; i++)
		{
			if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorToShowCardsFor))
			{
				actorHUDController = array[i];
				break;
			}
		}
		ResetToggles();
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = ScenarioManager.Scenario.PlayerActors[0];
		}
		int controllableID = ControllableRegistry.GetControllableID(actorToShowCardsFor);
		InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_MP"), ControllableRegistry.GetController(controllableID).Username), LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_DEAL_DAMAGE"), HelpBox.FormatTarget.TITLE);
		window.Hide(instant: true);
	}

	public void ProxySkipLoseCards(GameAction gameAction)
	{
		SkipLosingCards();
	}

	public void ProxyBurnAvailableCard(GameAction action)
	{
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyBurnOneAvailableCard(((CardsToken)action.SupplementaryDataToken).CardIDs[0]);
			ResetAndHide(switchBackToSavedState: true);
			return;
		}
		throw new Exception("Error losing an available card. Card Hand returns null for the actor (ID: " + action.ActorID + ").");
	}

	public void ProxyBurnDiscardedCards(GameAction action)
	{
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(action.ActorID);
		if (hand != null)
		{
			CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
			hand.ProxyBurnTwoDiscardedCards(cardsToken.CardIDs[0], cardsToken.CardIDs[1]);
			ResetAndHide(switchBackToSavedState: true);
			return;
		}
		throw new Exception("Error losing an available card. Card Hand returns null for the actor (ID: " + action.ActorID + ").");
	}
}
