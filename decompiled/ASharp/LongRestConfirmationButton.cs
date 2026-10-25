using System;
using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class LongRestConfirmationButton : MonoBehaviour
{
	[SerializeField]
	private RectTransform holder;

	[SerializeField]
	private ClickTrackerScopeElement cancelTracker;

	private AbilityCardUI longRestCard;

	private Action<bool> onToggled;

	private bool isSelected;

	private CPlayerActor playerActor;

	public Selectable Selectable => longRestCard?.fullAbilityCard.topActionButton.actionButton;

	public bool IsVisible
	{
		get
		{
			if (holder != null && holder.gameObject != null)
			{
				return holder.gameObject.activeSelf;
			}
			return false;
		}
	}

	private void Awake()
	{
		cancelTracker.onClick.AddListener(CancelSelection);
	}

	public void Init()
	{
		if (longRestCard != null)
		{
			return;
		}
		Hide();
		if (CharacterClassManager.LongRestLayout == null)
		{
			return;
		}
		try
		{
			longRestCard = ObjectPool.SpawnCard(-1, ObjectPool.ECardType.Ability, holder).GetComponent<AbilityCardUI>();
			longRestCard.transform.localScale = Vector3.one;
			longRestCard.fullAbilityCard.EnableSelectCard(enabled: true);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "Unable to initialise Long Rest card");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void Show(CPlayerActor playerActor, CardPileType cardPile, Action<bool> onToggled = null)
	{
		if (longRestCard == null)
		{
			Init();
			if (longRestCard == null)
			{
				return;
			}
		}
		this.playerActor = playerActor;
		this.onToggled = onToggled;
		if (playerActor.CanUseLongRest)
		{
			longRestCard.Init(null, playerActor, cardPile, (AbilityCardUI _, bool T) =>
			{
				Toggle(selected: true);
			}, (AbilityCardUI _, bool T) =>
			{
				Toggle(selected: false);
			}, null, null, null, null, null, isLongRest: true);
			longRestCard.SetMode(CardHandMode.ActionSelection, (onToggled == null) ? null : new List<CardPileType> { CardPileType.Any }, fadeUnselectableCards: false, highlightSelectableCard: false, longRestAvailable: true);
			longRestCard.IsInteractable = onToggled != null;
		}
		holder.gameObject.SetActive(value: true);
		RefreshOnlineInteraction();
		SetSelected(selected: false);
	}

	private void RefreshOnlineInteraction()
	{
		if (!FFSNetwork.IsOnline || playerActor == null || playerActor.IsUnderMyControl())
		{
			longRestCard.SetValid(isValid: true);
			cancelTracker.enabled = true;
			longRestCard.SetUnfocused(unfocused: false);
		}
		else
		{
			longRestCard.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			cancelTracker.enabled = false;
			longRestCard.SetUnfocused(unfocused: true);
		}
	}

	public void ShowConfirm(CPlayerActor playerActor, Action<bool> onToggled)
	{
		Show(playerActor, CardPileType.Hand, onToggled);
	}

	public void ShowUsed(CPlayerActor playerActor)
	{
		Show(playerActor, CardPileType.Discarded);
	}

	public void Hide()
	{
		holder.gameObject.SetActive(value: false);
	}

	private void Toggle(bool selected)
	{
		SetSelected(selected);
		onToggled?.Invoke(isSelected);
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl() && !(PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest))
		{
			Synchronizer.SendGameAction(GameActionType.ToggleLongRestAbility, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, selected);
		}
	}

	private void CancelSelection()
	{
		if (isSelected)
		{
			longRestCard.fullAbilityCard.OnAbilityClick(isTopAbility: true);
		}
	}

	private void SetSelected(bool selected)
	{
		isSelected = selected;
		longRestCard.fullAbilityCard.ToggleSelect(selected, CBaseCard.ActionType.TopAction);
		longRestCard.fullAbilityCard.DisplaySelected(selected);
	}

	private void OnDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			ObjectPool.RecycleCard(longRestCard.CardID, ObjectPool.ECardType.Ability, longRestCard.gameObject);
		}
	}

	public void RefreshConnectionState()
	{
		if (IsVisible)
		{
			RefreshOnlineInteraction();
		}
	}

	public void ProxyToggleSelectLongRestAbility(GameAction gameAction)
	{
		longRestCard.ToggleSelect(gameAction.SupplementaryDataBoolean, highlight: false, networkAction: false);
	}
}
