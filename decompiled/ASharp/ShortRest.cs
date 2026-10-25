using System;
using System.Collections.Generic;
using FFSNet;
using I2.Loc;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShortRest : MonoBehaviour
{
	[SerializeField]
	private GameObject dialogPrefab;

	[SerializeField]
	private RectTransform dialogHolder;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private Sprite regularSprite;

	[SerializeField]
	private Sprite regularHighlightedSprite;

	[SerializeField]
	private Sprite selectSprite;

	[SerializeField]
	private Sprite selectHighlightedSprite;

	[SerializeField]
	private RectTransform selectedInitiativeContainer;

	[SerializeField]
	private FHButton button;

	[SerializeField]
	private GUIAnimator warningAnimator;

	[SerializeField]
	private GameObject improvedHighlight;

	[SerializeField]
	private InteractabilityIsolatedUIControl m_UIControlLimiting;

	[SerializeField]
	private GameObject unfocusedMask;

	[SerializeField]
	public UIHoverMoveEffects _hoverMove;

	[SerializeField]
	public UIHoverScaleEffects _hoverScale;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactability;

	private UnityAction<CPlayerActor> shortRestAction;

	private string playerActorGUID;

	private bool isMouseIn;

	private bool isValid = true;

	private Action onClickInvalid;

	private InitiativeTrack.InitiativeEffects _effect;

	private YesNoDialog yesNoDialog;

	private bool isEnabled;

	public static bool Instant
	{
		get
		{
			if (!GameState.s_DelayedShortRestInSinglePlayer)
			{
				return !FFSNetwork.IsOnline;
			}
			return false;
		}
	}

	public FHButton Button => button;

	public bool IsSelected => isSelected;

	private CPlayerActor playerActor => ScenarioManager.FindActor(playerActorGUID) as CPlayerActor;

	private bool isSelected
	{
		get
		{
			if (ScenarioManager.Scenario != null)
			{
				return playerActor.CharacterClass.ShortRest;
			}
			return false;
		}
	}

	public bool IsValid => isValid;

	public void Toggle(bool enabled)
	{
		isEnabled = enabled;
		RefreshInteractable();
	}

	public void Init(CPlayerActor pActor, UnityAction<CPlayerActor> srAction, Action onNoAction = null)
	{
		playerActorGUID = pActor.ActorGuid;
		shortRestAction = srAction;
		if (yesNoDialog == null)
		{
			yesNoDialog = UnityEngine.Object.Instantiate(dialogPrefab, dialogHolder.GetComponentInParent<Canvas>().transform).GetComponent<YesNoDialog>();
		}
		yesNoDialog.name = pActor.CharacterClass.DefaultModel + "_" + dialogPrefab.name;
		yesNoDialog.PrepareInteractabilityForPlayer(playerActorGUID);
		yesNoDialog.Init("GUI_SHORT_REST_CONFIRMATION", autoClose: false, () =>
		{
			ShowPopup(active: false);
			Select(active: true, fromCardSelected: false, fromPopup: true);
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestConfirmed));
		}, () =>
		{
			onNoAction?.Invoke();
			MouseExit();
			Select(active: false, fromCardSelected: false, fromPopup: true);
			ShowPopup(active: false);
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestCancelled));
		});
		yesNoDialog.Hide(instant: true);
		warningAnimator.Stop(goToEnd: true);
		SetValid(isValid: true);
		SetUnfocused(FFSNetwork.IsOnline && !pActor.IsUnderMyControl());
	}

	public void SetupInteractability(InteractabilityIsolatedUIControl control = null)
	{
		m_UIControlLimiting = control;
		if (m_UIControlLimiting == null)
		{
			m_UIControlLimiting = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (m_UIControlLimiting != null)
		{
			m_UIControlLimiting.ControlSecondIdentifier = playerActorGUID;
			interactability = new InteractabilityIsolatedUIControl.InteractabilityReference
			{
				control = m_UIControlLimiting,
				onInteractabilityChanged = () =>
				{
					RefreshInteractable();
				}
			};
		}
	}

	public void Clear()
	{
		playerActorGUID = null;
		shortRestAction = null;
		warningAnimator.Stop(goToEnd: true);
		SetValid(isValid: false);
		SetUnfocused(unfocused: true);
		if (m_UIControlLimiting != null)
		{
			m_UIControlLimiting.ControlSecondIdentifier = string.Empty;
		}
	}

	public void Show(CPlayerActor actor)
	{
		CPlayerActor actor2 = playerActor;
		UpdateVisuals();
		if (!FFSNetwork.IsOnline || (actor2.IsUnderMyControl() && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar))
		{
			SetValid(isValid: true);
		}
		else if (actor2.IsUnderMyControl())
		{
			SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
		}
		else
		{
			SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
		}
		SetUnfocused(FFSNetwork.IsOnline && !actor2.IsUnderMyControl());
		Toggle(enabled: true);
		SetImproved(actor.CharacterClass.ImprovedShortRest);
	}

	private void SetUnfocused(bool unfocused)
	{
		unfocusedMask.SetActive(unfocused);
	}

	public void Hide()
	{
		Toggle(enabled: false);
	}

	private void SetImproved(bool isImproved)
	{
		improvedHighlight.SetActive(isImproved);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
		RefreshInteractable();
	}

	public void RefreshInteractable()
	{
		if (gameObject == null)
		{
			return;
		}
		bool flag = isEnabled && (interactability == null || interactability.cachedEnabled);
		if (gameObject.scene.isLoaded)
		{
			gameObject.SetActive(flag);
		}
		if (flag)
		{
			bool flag2 = button.interactable && (interactability == null || interactability.cachedInteractable);
			button.SetAsFaded(!flag2);
			string text = LocalizationManager.GetTranslation("GUI_TOOLTIP_SHORT_REST");
			if (!flag2)
			{
				text = text + "\n<color=#C53F3FFF>" + LocalizationManager.GetTranslation("GUI_TOOLTIP_SHORT_REST_WARNING") + "</color>";
			}
			GetComponent<UITextTooltipTarget>().SetText(text);
		}
	}

	private void UpdateVisuals()
	{
		Image image = backgroundImage;
		Sprite sprite;
		if (isSelected)
		{
			sprite = (isMouseIn ? selectHighlightedSprite : selectSprite);
		}
		else
		{
			sprite = (isMouseIn ? regularHighlightedSprite : regularSprite);
		}
		image.sprite = sprite;
		if ((bool)selectedInitiativeContainer)
		{
			selectedInitiativeContainer.gameObject.SetActive(isSelected);
		}
		SetHoverLock(isSelected);
	}

	private void SetHoverLock(bool isLocked)
	{
		if (_hoverScale != null)
		{
			UIHoverScaleEffects.LockType lockType = (isLocked ? UIHoverScaleEffects.LockType.ScaleFactor : UIHoverScaleEffects.LockType.None);
			_hoverScale.SetLock(lockType);
		}
		if (_hoverMove != null)
		{
			UIHoverMoveEffects.LockType lockType2 = (isLocked ? UIHoverMoveEffects.LockType.MoveOffset : UIHoverMoveEffects.LockType.None);
			_hoverMove.SetLock(lockType2);
		}
	}

	public void MouseClick()
	{
		Log.GUIInfo("Clicked on the short rest button.");
		Select(!isSelected);
	}

	public void Select(bool active, bool fromCardSelected = false, bool fromPopup = false)
	{
		if (active == isSelected)
		{
			return;
		}
		if (FFSNetwork.IsOnline && Singleton<UIReadyToggle>.Instance != null && (Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) || Singleton<UIReadyToggle>.Instance.IsProgressingBar))
		{
			Log.Info("Discarding short rest button click because this player is already marked as ready");
			return;
		}
		if (!isValid)
		{
			if (!fromCardSelected)
			{
				warningAnimator.Play();
				onClickInvalid?.Invoke();
			}
			return;
		}
		if (active && !fromPopup && !fromCardSelected && Instant)
		{
			ShowPopup(active: true);
			return;
		}
		if (Instant & active)
		{
			FastReload.AutoSaveOnInstantShortRest(playerActor);
		}
		playerActor.CharacterClass.ShortRest = active;
		UpdateVisuals();
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(playerActor);
		if (isSelected)
		{
			hand.DeselectAllCards();
		}
		else
		{
			hand.ToggleHighlight(new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			}, active: false);
		}
		if (!fromCardSelected)
		{
			hand.NetworkSelectedRoundCards();
			hand.UpdateInitiative();
		}
		hand.OnSelectedCardsNumberChanged();
		InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestPressed));
		if (Instant & active)
		{
			GameState.s_ShortRestingPlayer = playerActor;
			PhaseManager.SetNextPhase(CPhase.PhaseType.ShortRest);
		}
	}

	private void ShowPopup(bool active)
	{
		if (active)
		{
			Log.GUIInfo("Activating Short Rest popup");
			yesNoDialog.SetDescriptionKey(FFSNetwork.IsOnline ? "GUI_SHORT_REST_CONFIRMATION_MULTIPLAYER" : "GUI_SHORT_REST_CONFIRMATION");
			yesNoDialog.Show(dialogHolder);
		}
		else
		{
			Log.GUIInfo("Deactivating Short Rest popup");
			yesNoDialog.Hide();
		}
	}

	public void MouseEnter()
	{
		isMouseIn = true;
		UpdateVisuals();
		Singleton<CardsHandManager>.Instance.GetHand(playerActor).ToggleHighlight(new List<CardPileType> { CardPileType.Discarded }, active: true, playerActor.CharacterClass.LongRest, keepShortRest: true);
	}

	public void MouseExit()
	{
		isMouseIn = false;
		Singleton<CardsHandManager>.Instance.GetHand(playerActor).ToggleHighlight(new List<CardPileType>
		{
			CardPileType.Hand,
			CardPileType.Round
		}, active: false, playerActor.CharacterClass.LongRest, keepShortRest: true);
		UpdateVisuals();
	}

	public void SetValid(bool isValid, Action onClickInvalid = null)
	{
		this.isValid = isValid;
		this.onClickInvalid = onClickInvalid;
	}

	private void OnDisable()
	{
		warningAnimator.Stop(goToEnd: true);
	}

	private void OnDestroy()
	{
	}
}
