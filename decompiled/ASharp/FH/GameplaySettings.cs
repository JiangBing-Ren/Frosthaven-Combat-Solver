using System;
using FHGUI.Common;
using SharedLibrary.Achievments;
using UnityEngine;

namespace FH;

public class GameplaySettings : MonoBehaviour
{
	public ExtendedToggle ToggleAutomaticTacticalCameraRotation;

	public ExtendedToggle ToggleAutomaticTacticalCameraMovement;

	public ExtendedToggle ShowSpeedUpToggle;

	public ExtendedToggle FastReloadToggle;

	public ExtendedToggle ToggleContextualHints;

	public ExtendedButton ResetContextualHints;

	public FHButton UseAlternativeCardsButton;

	public ExtendedToggle UseAlternativeCards;

	public ExtendedToggle ToggleChooseAdvantageModifiers;

	public ExtendedToggle ToggleConfirmAttackAbilityBeforeFinishing;

	public ExtendedToggle DoubleClickToConfirmActionToggle;

	public ExtendedToggle UseBoardgameIconsToggle;

	private ButtonSwitch FastReloadButtonSwitch;

	private void Awake()
	{
		ToggleAutomaticTacticalCameraRotation.onValueChanged.AddListener(OnToggleAutomaticTacticalCameraRotation);
		ToggleAutomaticTacticalCameraMovement.onValueChanged.AddListener(OnToggleAutomaticTacticalCameraMovement);
		ShowSpeedUpToggle.onValueChanged.AddListener(OnShowSpeedUpChange);
		FastReloadToggle.onValueChanged.AddListener(OnFastReloadChange);
		FastReload.OnFastUndoEnabledChanged = (Action)Delegate.Remove(FastReload.OnFastUndoEnabledChanged, new Action(UpdateFastUndo));
		FastReload.OnFastUndoEnabledChanged = (Action)Delegate.Combine(FastReload.OnFastUndoEnabledChanged, new Action(UpdateFastUndo));
		FastReloadButtonSwitch = FastReloadToggle.GetComponent<ButtonSwitch>();
		ToggleContextualHints.onValueChanged.AddListener(OnToggleContextualHintsActionChange);
		ResetContextualHints.onClick.AddListener(OnResetContextualHintsAction);
		DoubleClickToConfirmActionToggle.onValueChanged.AddListener(OnDoubleClickToConfirmActionChange);
		UseBoardgameIconsToggle.onValueChanged.AddListener(OnUseBoardgameIconsToggleChange);
		Choreographer.OnChangeModifierOptions = (Action)Delegate.Remove(Choreographer.OnChangeModifierOptions, new Action(UpdateConfirmModifierOptions));
		Choreographer.OnChangeModifierOptions = (Action)Delegate.Combine(Choreographer.OnChangeModifierOptions, new Action(UpdateConfirmModifierOptions));
		ToggleChooseAdvantageModifiers.onValueChanged.AddListener(OnChooseAdvantageModifiersChange);
		ToggleConfirmAttackAbilityBeforeFinishing.onValueChanged.AddListener(OnConfirmAttackAbilityBeforeFinishingChange);
		UseAlternativeCards.onValueChanged.AddListener(OnUseAlternativeCards);
	}

	private void OnDestroy()
	{
		FastReload.OnFastUndoEnabledChanged = (Action)Delegate.Remove(FastReload.OnFastUndoEnabledChanged, new Action(UpdateFastUndo));
		Choreographer.OnChangeModifierOptions = (Action)Delegate.Remove(Choreographer.OnChangeModifierOptions, new Action(UpdateConfirmModifierOptions));
	}

	private void OnEnable()
	{
		ToggleAutomaticTacticalCameraRotation.SetIsOnWithoutNotify(SaveData.Instance.Global.AllowAutomaticTacticalCameraRotation);
		ToggleAutomaticTacticalCameraRotation.SetIsOnWithoutNotify(SaveData.Instance.Global.AllowAutomaticTacticalCameraMovement);
		ShowSpeedUpToggle.SetIsOnWithoutNotify(SaveData.Instance.Global.ShowSpeedUpButton);
		ToggleContextualHints.SetIsOnWithoutNotify(UIContextualTutorialModule.AreTutorialsEnabled);
		UpdateConfirmModifierOptions();
		UseAlternativeCards.SetIsOnWithoutNotify(PersistentData.HasAlternativeCards);
		bool flag = SaveData.Instance?.Global != null && SaveData.Instance.Global.GameMode == EGameMode.MainMenu;
		UseAlternativeCards.interactable = flag;
		UITextTooltipTarget component = UseAlternativeCards.GetComponent<UITextTooltipTarget>();
		if (component != null)
		{
			component.enabled = !flag;
		}
		UseAlternativeCardsButton.interactable = flag;
		UpdateFastUndo();
	}

	private void OnToggleAutomaticTacticalCameraRotation(bool isOn)
	{
		SaveData.Instance.Global.AllowAutomaticTacticalCameraRotation = isOn;
		SaveData.Instance.SaveGlobalData();
	}

	private void OnToggleAutomaticTacticalCameraMovement(bool isOn)
	{
		SaveData.Instance.Global.AllowAutomaticTacticalCameraMovement = isOn;
		SaveData.Instance.SaveGlobalData();
	}

	private void OnShowSpeedUpChange(bool isOn)
	{
		SaveData.Instance.Global.ShowSpeedUpButton = isOn;
		SaveData.Instance.SaveGlobalData();
	}

	private void OnFastReloadChange(bool isOn)
	{
		FastReload.setting_FastUndoEnabled = isOn;
	}

	private void OnToggleContextualHintsActionChange(bool isOn)
	{
		UIContextualTutorialModule.SetTutorialsEnabled(!UIContextualTutorialModule.AreTutorialsEnabled);
	}

	private void OnResetContextualHintsAction()
	{
		GlobalStatRepository.Instance.ResetTutorials();
	}

	private void OnDoubleClickToConfirmActionChange(bool isOn)
	{
		throw new NotImplementedException();
	}

	private void OnUseBoardgameIconsToggleChange(bool isOn)
	{
		throw new NotImplementedException();
	}

	private void OnUseAlternativeCards(bool isOn)
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation("Use Alternative Ability Cards", "This options requires restart", () =>
		{
			PersistentData.HasAlternativeCards = isOn;
			StartCoroutine(SceneController.Instance.ReloadGlobalCardAssets());
		}, () =>
		{
			UseAlternativeCards.SetIsOnWithoutNotify(!isOn);
		});
	}

	private void UpdateFastUndo()
	{
		FastReloadToggle.interactable = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
		FastReloadToggle.SetIsOnWithoutNotify(FastReload.setting_FastUndoEnabled);
		FastReloadButtonSwitch?.Refresh(FastReload.setting_FastUndoEnabled);
	}

	private void UpdateConfirmModifierOptions()
	{
		ToggleChooseAdvantageModifiers.SetIsOnWithoutNotify(SaveData.Instance.Global.ChooseAdvantageModifiers);
		ToggleChooseAdvantageModifiers.interactable = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
		ToggleConfirmAttackAbilityBeforeFinishing.SetIsOnWithoutNotify(SaveData.Instance.Global.ConfirmAttackAbilityBeforeFinishing);
		ToggleConfirmAttackAbilityBeforeFinishing.interactable = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
	}

	private void OnChooseAdvantageModifiersChange(bool isOn)
	{
		Choreographer.SetChooseAdvantageModifiers(isOn);
	}

	private void OnConfirmAttackAbilityBeforeFinishingChange(bool isOn)
	{
		Choreographer.SetConfirmAttackAbilityFinished(isOn);
	}
}
