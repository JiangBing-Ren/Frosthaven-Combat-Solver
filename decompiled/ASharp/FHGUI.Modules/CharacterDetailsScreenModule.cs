using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class CharacterDetailsScreenModule : FHMapViewModule
{
	[Flags]
	public enum Functionality
	{
		None = 0,
		ClassSelect = 1,
		Rename = 2,
		PersonalQuest = 4,
		Perks = 8,
		Equipment = 0x10,
		LevelUp = 0x20,
		Info = 0x40,
		HandManagement = 0x80,
		RosterSelect = 0x100,
		ImportParty = 0x200,
		CreationProcess = Rename | PersonalQuest | Equipment,
		PartyCharacter = LevelUp,
		QuestPreparation = Equipment | HandManagement
	}

	[Serializable]
	public struct DetailsFunctionalityButton
	{
		public Functionality Functionality;

		public ButtonsBarElement ButtonElement;
	}

	[Serializable]
	public struct DetailsTabButton
	{
		public FHTabButton TabButton;

		public Functionality ActiveIn;

		public Functionality InteractableIn;

		public Functionality FocusedIn;
	}

	[Serializable]
	public struct SettingsGameObjects
	{
		public GameObject Object;

		public Functionality ActiveIn;

		public bool ExactMatch;
	}

	[Header("Components")]
	[Space(8f)]
	[SerializeField]
	private Transform _buttonsBar;

	[SerializeField]
	private Button _proceedButton;

	[SerializeField]
	private GameObject _proceedButtonAlert;

	[SerializeField]
	private GameObject _proceedHintBox;

	[SerializeField]
	private TMP_Text _proceedHint;

	[SerializeField]
	private UIBreadcrumbs _breadcrumbs;

	[SerializeField]
	private TMP_Text _textTitle;

	[SerializeField]
	private TMP_Text _textDescription;

	[SerializeField]
	public Image _heroClassIcon;

	[SerializeField]
	public Image[] _heroClassColor;

	[SerializeField]
	public FHButton _heroIntroductionVideoButton;

	[Header("Settings")]
	[Space(8f)]
	[SerializeField]
	private DetailsFunctionalityButton[] _presetButtons;

	[SerializeField]
	private DetailsTabButton[] _presetTabButtons;

	[SerializeField]
	private SettingsGameObjects[] _presetGameObjects;

	[SerializeField]
	public GameObject _forwardButton;

	[SerializeField]
	public GameObject _backButton;

	[SerializeField]
	public FHButton _closeButton;

	[SerializeField]
	public FHTabsGroup _infoCategories;

	protected override bool PlaySoundOnEnable => false;

	public FHTabButton InfoCategoriesActiveTab => _infoCategories.ActiveTab;

	public void SetButtonsBar(params ButtonsBarParam[] buttonParams)
	{
		_buttonsBar.gameObject.SetActive(value: true);
		ButtonsBarElement.SetButtons(_buttonsBar, buttonParams);
	}

	public void ClearButtonsBar(bool hideContainer = true)
	{
		_buttonsBar.gameObject.SetActive(!hideContainer);
		ButtonsBarElement.ClearButtons(_buttonsBar);
	}

	public void ShowProceedButton(ButtonsBarParam buttonParams, bool showAlert = false)
	{
		_proceedButtonAlert.SetActive(showAlert);
		ShowProceedButton(buttonParams.LabelKey, buttonParams.Callback, buttonParams.HintKey, buttonParams.Interactable);
	}

	public void ShowProceedButton(string labelKey, Action proceedButtonCallback, string hintKey = null, bool interactable = true)
	{
		_proceedButton.gameObject.SetActive(value: true);
		_proceedButton.onClick.RemoveAllListeners();
		_proceedButton.SetTextKey(labelKey);
		_proceedButton.interactable = interactable;
		ShowProceedHint(hintKey);
		_proceedButton.onClick.AddListener(proceedButtonCallback.Invoke);
	}

	public void ShowProceedHint(string hintKey)
	{
		ShowProceedHintLiteral(string.IsNullOrEmpty(hintKey) ? "" : LocalizationManager.GetTranslation(hintKey));
	}

	public void ShowProceedHintLiteral(string hintText)
	{
		if (string.IsNullOrWhiteSpace(hintText))
		{
			_proceedHint.text = "";
			_proceedHintBox.SetActive(value: false);
		}
		else
		{
			_proceedHint.text = hintText;
			_proceedHintBox.SetActive(value: true);
		}
	}

	public void HideProceedButton()
	{
		_proceedButton.gameObject.SetActive(value: false);
		_proceedButton.onClick.RemoveAllListeners();
		_proceedHint.text = "";
		_proceedHintBox.SetActive(value: false);
	}

	public void ShowCloseButton(Action onCloseAction)
	{
		_closeButton.RemoveAllEventListeners();
		_closeButton.onClick.AddListener(() =>
		{
			HideCloseButton();
			onCloseAction();
		});
		_closeButton.gameObject.SetActive(value: true);
	}

	public void HideCloseButton()
	{
		_closeButton.RemoveAllEventListeners();
		_closeButton.gameObject.SetActive(value: false);
	}

	public void SetDescription(string titleLocKey, string descriptionLocKey)
	{
		SetDescriptionLiteral(titleLocKey.IsNOTNullOrEmpty() ? LocalizationManager.GetTranslation(titleLocKey) : string.Empty, descriptionLocKey.IsNOTNullOrEmpty() ? LocalizationManager.GetTranslation(descriptionLocKey) : string.Empty);
	}

	public void SetDescriptionLiteral(string titleLiteral, string descriptionLiteral)
	{
		if ((bool)_textTitle)
		{
			_textTitle.text = titleLiteral;
		}
		if ((bool)_textDescription)
		{
			_textDescription.text = descriptionLiteral;
		}
	}

	public void SetCharacterClass(CharacterClassDef characterDtoClass)
	{
		ECharacter character = CrossReference.ClassIdToECharacterModel(characterDtoClass.ID.ID);
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(character);
		Sprite iconClassGold = UIInfoTools.Instance.GetCharacterConfigUI(character.ToString()).iconClassGold;
		if ((bool)_heroClassIcon)
		{
			_heroClassIcon.sprite = iconClassGold;
			_heroClassIcon.enabled = iconClassGold != null;
		}
		if (_heroClassColor != null && _heroClassColor.Length != 0)
		{
			Image[] heroClassColor = _heroClassColor;
			for (int i = 0; i < heroClassColor.Length; i++)
			{
				heroClassColor[i].color = characterColor;
			}
		}
	}

	public void ShowHeroIntroButton(Action onClick)
	{
		if (!(_heroIntroductionVideoButton == null))
		{
			_heroIntroductionVideoButton.gameObject.SetActive(value: true);
			_heroIntroductionVideoButton.RemoveAllEventListeners();
			_heroIntroductionVideoButton.onClick.AddListener(() =>
			{
				onClick?.Invoke();
			});
		}
	}

	private void ResetClassIcon()
	{
		if ((bool)_heroClassIcon)
		{
			_heroClassIcon.enabled = false;
		}
		if (_heroClassColor != null && _heroClassColor.Length != 0)
		{
			Image[] heroClassColor = _heroClassColor;
			for (int i = 0; i < heroClassColor.Length; i++)
			{
				heroClassColor[i].color = Color.white;
			}
		}
	}

	public void ClearDescription()
	{
		if ((bool)_textTitle)
		{
			_textTitle.text = string.Empty;
		}
		if ((bool)_textDescription)
		{
			_textDescription.text = string.Empty;
		}
	}

	public void SetPresetButton(Functionality functionality, Action action, string labelKey, bool intractable = true)
	{
		DetailsFunctionalityButton detailsFunctionalityButton = _presetButtons.FirstOrDefault((DetailsFunctionalityButton pb) => pb.Functionality == functionality);
		if (detailsFunctionalityButton.ButtonElement == null)
		{
			Log.GUIInfo($"Tried to set preset button functionality [{functionality}] but button could not be found");
		}
		else if (action != null)
		{
			detailsFunctionalityButton.ButtonElement.gameObject.SetActive(value: true);
			detailsFunctionalityButton.ButtonElement.SetParam(new ButtonsBarParam
			{
				Callback = action,
				LabelKey = labelKey
			});
			if (detailsFunctionalityButton.ButtonElement.TryGetComponent<UITextTooltipTarget>(out var component))
			{
				component.tooltipText = labelKey;
			}
		}
		else
		{
			detailsFunctionalityButton.ButtonElement.ClearParam();
			detailsFunctionalityButton.ButtonElement.gameObject.SetActive(value: false);
		}
	}

	public void ClearPresetButtons()
	{
		DetailsFunctionalityButton[] presetButtons = _presetButtons;
		for (int i = 0; i < presetButtons.Length; i++)
		{
			DetailsFunctionalityButton detailsFunctionalityButton = presetButtons[i];
			SetPresetButton(detailsFunctionalityButton.Functionality, null, string.Empty);
		}
	}

	public void SetFunctionality(Functionality functionality)
	{
		UpdateGameObjectsState(functionality);
		SetTabState(functionality);
	}

	private void UpdateGameObjectsState(Functionality functionality)
	{
		if (_presetGameObjects != null && _presetButtons.Length != 0)
		{
			SettingsGameObjects[] presetGameObjects = _presetGameObjects;
			for (int i = 0; i < presetGameObjects.Length; i++)
			{
				SettingsGameObjects settingsGameObjects = presetGameObjects[i];
				bool active = (settingsGameObjects.ExactMatch ? (settingsGameObjects.ActiveIn == functionality) : HasAnyFlag(functionality, settingsGameObjects.ActiveIn));
				UIManager.RegisterLateActiveState(settingsGameObjects.Object, active);
			}
		}
	}

	private void SetTabState(Functionality functionality)
	{
		for (int i = 0; i < _presetTabButtons.Length; i++)
		{
			DetailsTabButton detailsTabButton = _presetTabButtons[i];
			detailsTabButton.TabButton.interactable = HasAnyFlag(functionality, detailsTabButton.InteractableIn);
			detailsTabButton.TabButton.gameObject.SetActive(HasAnyFlag(functionality, detailsTabButton.ActiveIn));
			if (detailsTabButton.FocusedIn.HasFlag(functionality) && functionality != Functionality.None)
			{
				FHTabsGroup componentInParent = detailsTabButton.TabButton.GetComponentInParent<FHTabsGroup>(includeInactive: true);
				if (componentInParent != null)
				{
					componentInParent.SwitchToTab(detailsTabButton.TabButton);
				}
				else
				{
					Log.GUIError($"Tab Button {detailsTabButton.TabButton} is missing a FHTabsGroup parent");
				}
			}
		}
	}

	public bool TrySetInfoCategoriesActiveTab(FHTabButton tabButton)
	{
		if (tabButton == null || !tabButton.interactable || _infoCategories == null)
		{
			return false;
		}
		if (_infoCategories.ActiveTab == tabButton)
		{
			return true;
		}
		_infoCategories.SwitchToTab(tabButton);
		return _infoCategories.ActiveTab == tabButton;
	}

	public static bool HasAnyFlag(Functionality value, Functionality flags)
	{
		return (Convert.ToInt32(value) & Convert.ToInt32(flags)) != 0;
	}

	public void ShowBreadcrumbs(List<UIBreadcrumbs.Params> crumbs, byte selectedTag)
	{
		if ((object)_breadcrumbs != null)
		{
			_breadcrumbs.Init(crumbs, selectedTag);
			_breadcrumbs.gameObject.SetActive(value: true);
		}
	}

	public void HideBreadcrumbs()
	{
		if ((object)_breadcrumbs != null)
		{
			_breadcrumbs.gameObject.SetActive(value: false);
		}
	}

	public void ResetCharacterCategories()
	{
		if (!(_infoCategories == null))
		{
			_infoCategories.SwitchToTab(_infoCategories.StartTab);
		}
	}

	public void ClearAll(bool hideBarContainer = true)
	{
		UpdateGameObjectsState(Functionality.None);
		ClearButtonsBar(hideBarContainer);
		ClearPresetButtons();
		HideProceedButton();
		HideBreadcrumbs();
		ClearDescription();
		HideCloseButton();
		ResetClassIcon();
		SetPadding(Vector4.zero);
		_heroIntroductionVideoButton.gameObject.SetActive(value: false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		ClearAll();
	}

	public void SetPadding(Vector4 padding)
	{
		RectTransform obj = transform as RectTransform;
		obj.offsetMin = new Vector2(padding.x, padding.y);
		obj.offsetMax = new Vector2(padding.z, padding.w);
	}

	public ButtonsBarParam Button_Back(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_BACK", null, callback);
		result.NonInteractable = !interactable;
		result.Prototype = _backButton;
		return result;
	}

	public ButtonsBarParam Button_Continue(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_CONTINUE", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		result.Prototype = _forwardButton;
		return result;
	}

	public ButtonsBarParam Button_Rename(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_RENAME", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		result.Prototype = _forwardButton;
		return result;
	}

	public ButtonsBarParam Button_Complete(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_COMPLETE", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		result.Prototype = _forwardButton;
		return result;
	}

	public ButtonsBarParam Button_Create(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_CREATE", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		result.Prototype = _forwardButton;
		return result;
	}
}
