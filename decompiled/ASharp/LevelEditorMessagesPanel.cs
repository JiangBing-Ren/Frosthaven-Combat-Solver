using System;
using System.Collections.Generic;
using System.Linq;
using ClockStone;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMessagesPanel : MonoBehaviour
{
	[Header("Top Level containers")]
	public GameObject MessageSubMenuGroup;

	public Toggle PagesSubMenuToggle;

	public Toggle InteractabilitySubMenuToggle;

	public Toggle DisplayTriggerSubMenuToggle;

	public Toggle CameraProfileSubMenuToggle;

	[Header("Main Messages Panel")]
	public GameObject MessageDisplayPanel;

	public GameObject MessageItemPrefab;

	public Transform MessageListItemParent;

	public InputField NameInput;

	public LayoutElement LayoutDropDownElement;

	public TMP_Dropdown LayoutTypeDropDown;

	public LayoutElement TitleKeyElement;

	public InputField TitleKeyInput;

	public InputField DisplayDelayInput;

	public Toggle ShouldPauseToggle;

	public Toggle CanRepeat;

	public Toggle ShouldDisplayBGToggle;

	public Button ApplyButton;

	public TextMeshProUGUI ApplyButtonText;

	public Button DeleteButton;

	public Button DuplicateButton;

	public TextMeshProUGUI StatusText;

	public InputField MessageIndexInput;

	[Header("Message Pages Panel")]
	public GameObject PagesPanel;

	public GameObject PageDisplayPanel;

	public GameObject MessagePageItemPrefab;

	public Transform MessagePagesListItemParent;

	public TextMeshProUGUI TitleLabelText;

	public InputField PageTextKeyInput;

	public InputField PageImagePathInput;

	public TextMeshProUGUI LocaleCheckText;

	public GameObject[] ObjectsActiveForDialogTypeLayout;

	public TMP_Dropdown DialogCharacterGUIDDropDown;

	public TMP_Dropdown DialogCharacterExpressionDropDown;

	public Image CharacterPortraitCheck;

	public TextMeshProUGUI CharacterPortraitCheckText;

	public TMP_Dropdown DialogBackgroundsDropDown;

	public TMP_Dropdown DialogAudioDropDown;

	[Header("Interactability Limiting")]
	public GameObject InteractabilityItemDisplayPanel;

	public GameObject InteractabilityItemListPrefab;

	public Transform InteractabilityItemParent;

	[Space]
	public TMP_Dropdown InteractabilityTypeDropDown;

	[Space]
	public TextMeshProUGUI ControlTypeLabel;

	public TMP_Dropdown ControlBehaviourDropDown;

	[Space]
	public LayoutElement ControlContextualIDElement;

	public InputField ControlContextualIDInput;

	[Space]
	public LayoutElement ControlContextualIndexElement;

	public InputField ControlContextualIndexInput;

	[Space]
	public LayoutElement ControlContextualDropDownElement;

	public TMP_Dropdown ControlContextualDropdown;

	[Space]
	public LayoutElement ControlContextualDropDownElement2;

	public TMP_Dropdown ControlContextualDropdown2;

	[Space]
	public LayoutElement TileIndexElement;

	public TextMeshProUGUI TileIndexParameterLabel;

	[Space]
	public LayoutElement ControlHighlightPrefabKeyElement;

	public InputField ControlHighlightPrefabKeyInput;

	[Header("Display Triggers")]
	public LevelEditorEventTriggerPanel DisplayTrigger;

	public LevelEditorEventTriggerPanel DismissTrigger;

	[Header("Camera Profile")]
	public LevelEditorCameraProfilePanel CameraProfilePanel;

	private List<LevelEditorMessageItem> m_MessageItems = new List<LevelEditorMessageItem>();

	private LevelEditorMessageItem m_CurrentMessageItem;

	private List<LevelEditorMessagePageItem> m_PageItems = new List<LevelEditorMessagePageItem>();

	private LevelEditorMessagePageItem m_CurrentPageItem;

	private List<string> m_DialogCharacterGUIDOptions = new List<string>();

	private List<EExpression> m_AvailableExpressions = ((EExpression[])Enum.GetValues(typeof(EExpression))).ToList();

	private List<string> m_DialogBackgroundOptions = new List<string>();

	private List<string> m_DialogAudioOptions = new List<string>();

	private List<LevelEditorLimitedInteractionItem> m_InteractabilityControlItems = new List<LevelEditorLimitedInteractionItem>();

	private LevelEditorLimitedInteractionItem m_CurrentInteractabilityItem;

	private TileIndex m_CurrentInteractabilityItemTileIndex;

	private List<CActor> m_ActorOptions = new List<CActor>();

	private bool m_ButtonModeAdd = true;

	private bool m_IgnoreMessageLayoutDropDownEvents;

	private void Awake()
	{
		LayoutTypeDropDown.options.Clear();
		LayoutTypeDropDown.AddOptions(CLevelMessage.LevelMessageLayouts.Select((CLevelMessage.ELevelMessageLayoutType s) => s.ToString()).ToList());
		InteractabilityTypeDropDown.options.Clear();
		InteractabilityTypeDropDown.AddOptions((from s in CLevelUIInteractionProfile.ControlTypes
			where s != CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu && s != CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI && s != CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow
			select s.ToString()).ToList());
		MessageDisplayPanel.SetActive(value: false);
	}

	public void RefreshUIWithLoadedLevel()
	{
		foreach (LevelEditorMessageItem messageItem in m_MessageItems)
		{
			UnityEngine.Object.Destroy(messageItem.gameObject);
		}
		m_MessageItems.Clear();
		if (SaveData.Instance.Global.CurrentEditorLevelData?.LevelMessages != null)
		{
			for (int i = 0; i < SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Count; i++)
			{
				CLevelMessage messageToAddFor = SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages[i];
				AddMessageItemToUI(messageToAddFor, i);
			}
		}
	}

	private LevelEditorMessageItem AddMessageItemToUI(CLevelMessage messageToAddFor, int messageIndex)
	{
		LevelEditorMessageItem component = UnityEngine.Object.Instantiate(MessageItemPrefab, MessageListItemParent).GetComponent<LevelEditorMessageItem>();
		component.Init(EditMessageItem, ReorderMessageItem, messageIndex);
		m_MessageItems.Add(component);
		return component;
	}

	public void EditMessageItem(LevelEditorMessageItem viewItem)
	{
		m_CurrentMessageItem = viewItem;
		CLevelMessage message = m_CurrentMessageItem.Message;
		m_IgnoreMessageLayoutDropDownEvents = true;
		NameInput.text = message.MessageName;
		LayoutTypeDropDown.value = (int)message.LayoutType;
		TitleKeyInput.text = message.TitleKey;
		DisplayDelayInput.text = message.DisplayDelay.ToString();
		ShouldPauseToggle.isOn = message.ShouldPauseGame;
		CanRepeat.isOn = message.CanRepeat;
		ShouldDisplayBGToggle.isOn = message.ShowScreenBG;
		MessageIndexInput.text = viewItem.MessageIndex.ToString();
		m_IgnoreMessageLayoutDropDownEvents = false;
		if (message.CameraProfile == null)
		{
			message.CameraProfile = new CLevelCameraProfile();
		}
		ShowMessageEditDisplay(addMode: false, message.LayoutType);
	}

	public void ReorderMessageItem(LevelEditorMessageItem reorderItem, bool moveUp)
	{
		int messageIndex = reorderItem.MessageIndex;
		int num = (moveUp ? Mathf.Max(0, messageIndex - 1) : Mathf.Min(messageIndex + 1, SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Count - 1));
		if (num != messageIndex)
		{
			CLevelMessage message = reorderItem.Message;
			ReorderMessage(messageIndex, num, message);
			RefreshUIWithLoadedLevel();
		}
	}

	public void ReorderMessage(int currentIndex, int newIndex, CLevelMessage messageToMove)
	{
		SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.RemoveAt(currentIndex);
		SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Insert(newIndex, messageToMove);
	}

	public void OnButtonAddNewClicked()
	{
		PagesSubMenuToggle.isOn = false;
		InteractabilitySubMenuToggle.isOn = false;
		DisplayTriggerSubMenuToggle.isOn = false;
		CameraProfileSubMenuToggle.isOn = false;
		m_CurrentMessageItem = null;
		NameInput.text = "";
		m_IgnoreMessageLayoutDropDownEvents = true;
		LayoutTypeDropDown.value = 0;
		m_IgnoreMessageLayoutDropDownEvents = false;
		TitleKeyInput.text = "";
		ShouldPauseToggle.isOn = false;
		CanRepeat.isOn = false;
		ShouldDisplayBGToggle.isOn = false;
		ShowMessageEditDisplay(addMode: true);
	}

	public void ResetPanel()
	{
		MessageSubMenuGroup.SetActive(value: false);
		PagesSubMenuToggle.gameObject.SetActive(value: true);
		InteractabilitySubMenuToggle.gameObject.SetActive(value: true);
		DisplayTriggerSubMenuToggle.gameObject.SetActive(value: true);
		CameraProfileSubMenuToggle.gameObject.SetActive(value: true);
		PagesSubMenuToggle.isOn = false;
		InteractabilitySubMenuToggle.isOn = false;
		DisplayTriggerSubMenuToggle.isOn = false;
		CameraProfileSubMenuToggle.isOn = false;
		PageDisplayPanel.SetActive(value: false);
		InteractabilityItemDisplayPanel.SetActive(value: false);
	}

	public void ShowMessageEditDisplay(bool addMode, CLevelMessage.ELevelMessageLayoutType type = CLevelMessage.ELevelMessageLayoutType.MiddleOfScreenBox)
	{
		m_ButtonModeAdd = addMode;
		StatusText.text = "-";
		PageDisplayPanel.SetActive(value: false);
		InteractabilityItemDisplayPanel.SetActive(value: false);
		if (addMode)
		{
			ApplyButtonText.text = "ADD NEW";
			DeleteButton.gameObject.SetActive(value: false);
			DuplicateButton.gameObject.SetActive(value: false);
			LayoutDropDownElement.gameObject.SetActive(value: false);
			TitleKeyElement.gameObject.SetActive(value: false);
			MessageSubMenuGroup.SetActive(value: false);
			PagesSubMenuToggle.gameObject.SetActive(value: true);
			InteractabilitySubMenuToggle.gameObject.SetActive(value: true);
			DisplayTriggerSubMenuToggle.gameObject.SetActive(value: true);
			CameraProfileSubMenuToggle.gameObject.SetActive(value: true);
		}
		else
		{
			ApplyButtonText.text = "APPLY";
			DeleteButton.gameObject.SetActive(value: true);
			DuplicateButton.gameObject.SetActive(value: true);
			LayoutDropDownElement.gameObject.SetActive(value: true);
			TitleKeyElement.gameObject.SetActive(value: true);
			MessageSubMenuGroup.SetActive(value: true);
			RefreshInteractabilityItemList();
			RefreshEventTriggerDisplays();
			CameraProfilePanel.InitForProfile(m_CurrentMessageItem.Message.CameraProfile);
			switch (type)
			{
			case CLevelMessage.ELevelMessageLayoutType.EmptyMessageFlag:
				PagesSubMenuToggle.gameObject.SetActive(value: false);
				InteractabilitySubMenuToggle.gameObject.SetActive(value: false);
				DisplayTriggerSubMenuToggle.gameObject.SetActive(value: true);
				DismissTrigger.gameObject.SetActive(value: false);
				CameraProfileSubMenuToggle.gameObject.SetActive(value: false);
				break;
			case CLevelMessage.ELevelMessageLayoutType.HelpText:
				if (PagesSubMenuToggle.isOn)
				{
					PagesSubMenuToggle.isOn = false;
					PagesPanel.SetActive(value: false);
				}
				PagesSubMenuToggle.gameObject.SetActive(value: false);
				InteractabilitySubMenuToggle.gameObject.SetActive(value: true);
				DisplayTriggerSubMenuToggle.gameObject.SetActive(value: true);
				CameraProfileSubMenuToggle.gameObject.SetActive(value: true);
				DismissTrigger.gameObject.SetActive(value: true);
				break;
			default:
				PagesSubMenuToggle.gameObject.SetActive(value: true);
				InteractabilitySubMenuToggle.gameObject.SetActive(value: true);
				DisplayTriggerSubMenuToggle.gameObject.SetActive(value: true);
				CameraProfileSubMenuToggle.gameObject.SetActive(value: true);
				DismissTrigger.gameObject.SetActive(value: true);
				RefreshPageList();
				break;
			}
		}
		MessageDisplayPanel.SetActive(value: true);
	}

	public void OnButtonDuplicateClicked()
	{
		DuplicateCurrentMessage();
	}

	private async void DuplicateCurrentMessage()
	{
		CLevelMessage newMessage = await m_CurrentMessageItem.Message.DeepCopySerializableObject<CLevelMessage>();
		int num = SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Count((CLevelMessage x) => x.MessageName == newMessage.MessageName);
		newMessage.MessageName += $" ({num})";
		SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Add(newMessage);
		m_CurrentMessageItem = AddMessageItemToUI(newMessage, SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Count - 1);
		EditMessageItem(m_CurrentMessageItem);
		StatusText.text = "<color=green> - Successfully added message - </color>";
	}

	public void OnButtonApplyClicked()
	{
		if (ValidateCurrentUIForMessage(out var completionMessage))
		{
			if (m_ButtonModeAdd)
			{
				CLevelMessage.ELevelMessageLayoutType messageLayoutType = CLevelMessage.LevelMessageLayouts.Single((CLevelMessage.ELevelMessageLayoutType s) => s.ToString() == LayoutTypeDropDown.options[LayoutTypeDropDown.value].text);
				CLevelMessage cLevelMessage = new CLevelMessage(NameInput.text, messageLayoutType, TitleKeyInput.text, string.IsNullOrEmpty(DisplayDelayInput.text) ? 0f : float.Parse(DisplayDelayInput.text), new CLevelTrigger(), new CLevelTrigger(), new CLevelUIInteractionProfile(), new List<CLevelMessagePage>(), ShouldPauseToggle.isOn, ShouldDisplayBGToggle.isOn, new CLevelCameraProfile(), null, CanRepeat.isOn);
				SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Add(cLevelMessage);
				m_CurrentMessageItem = AddMessageItemToUI(cLevelMessage, SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Count - 1);
				EditMessageItem(m_CurrentMessageItem);
				StatusText.text = "<color=green> - Successfully added message - </color>";
			}
			else if (m_CurrentMessageItem != null)
			{
				CLevelMessage message = m_CurrentMessageItem.Message;
				message.MessageName = NameInput.text;
				message.TitleKey = TitleKeyInput.text;
				message.DisplayDelay = (string.IsNullOrEmpty(DisplayDelayInput.text) ? 0f : float.Parse(DisplayDelayInput.text));
				message.LayoutType = CLevelMessage.LevelMessageLayouts.SingleOrDefault((CLevelMessage.ELevelMessageLayoutType s) => s.ToString() == LayoutTypeDropDown.options[LayoutTypeDropDown.value].text);
				message.ShouldPauseGame = ShouldPauseToggle.isOn;
				message.CanRepeat = CanRepeat.isOn;
				message.ShowScreenBG = ShouldDisplayBGToggle.isOn;
				m_CurrentMessageItem.UpdateMessageName();
				StatusText.text = "<color=green> - Successfully edited message - </color>";
			}
		}
		else
		{
			StatusText.text = "<color=red> - " + completionMessage + " - </color>";
			Log.Error("Message validation failed: " + completionMessage);
		}
	}

	public void OnMessageTypeDropDownChanged(int value)
	{
		if (!m_IgnoreMessageLayoutDropDownEvents)
		{
			CLevelMessage.ELevelMessageLayoutType type = CLevelMessage.LevelMessageLayouts.Single((CLevelMessage.ELevelMessageLayoutType s) => s.ToString() == LayoutTypeDropDown.options[LayoutTypeDropDown.value].text);
			ShowMessageEditDisplay(addMode: false, type);
		}
	}

	public void OnButtonDeleteClicked()
	{
		if (m_CurrentMessageItem != null)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Remove(m_CurrentMessageItem.Message);
			m_MessageItems.Remove(m_CurrentMessageItem);
			UnityEngine.Object.Destroy(m_CurrentMessageItem.gameObject);
			m_CurrentMessageItem = null;
			RefreshUIWithLoadedLevel();
			ResetPanel();
		}
	}

	public void OnMessageIndexSetPressed()
	{
		if (m_CurrentMessageItem != null && !string.IsNullOrEmpty(MessageIndexInput.text))
		{
			int value = int.Parse(MessageIndexInput.text);
			value = Mathf.Clamp(value, 0, SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Count - 1);
			ReorderMessage(m_CurrentMessageItem.MessageIndex, value, m_CurrentMessageItem.Message);
			RefreshUIWithLoadedLevel();
			ResetPanel();
		}
	}

	public bool ValidateCurrentUIForMessage(out string completionMessage)
	{
		if (NameInput.text == string.Empty)
		{
			completionMessage = "Unique message name must not be blank";
			return false;
		}
		if (m_ButtonModeAdd)
		{
			if (SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Any((CLevelMessage x) => x.MessageName == NameInput.text))
			{
				completionMessage = "Message name is not Unique";
				return false;
			}
		}
		else if (NameInput.text != m_CurrentMessageItem.Message.MessageName && SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Any((CLevelMessage x) => x.MessageName == NameInput.text))
		{
			completionMessage = "Message name is not Unique";
			return false;
		}
		completionMessage = "Validation succeeded";
		return true;
	}

	public void RefreshTriggerPanelsAfterUnitListChange()
	{
		DisplayTrigger.FillActorNameDropDown();
		DismissTrigger.FillActorNameDropDown();
	}

	private void RefreshPageList()
	{
		foreach (LevelEditorMessagePageItem pageItem in m_PageItems)
		{
			UnityEngine.Object.Destroy(pageItem.gameObject);
		}
		m_PageItems.Clear();
		CLevelMessage message = m_CurrentMessageItem.Message;
		if (message.Pages == null)
		{
			return;
		}
		int num = 0;
		foreach (CLevelMessagePage page in message.Pages)
		{
			_ = page;
			LevelEditorMessagePageItem component = UnityEngine.Object.Instantiate(MessagePageItemPrefab, MessagePagesListItemParent).GetComponent<LevelEditorMessagePageItem>();
			component.Init(message.MessageName, num, PageListItemPressed);
			m_PageItems.Add(component);
			num++;
		}
	}

	public void AddNewPageItemPressed()
	{
		CLevelMessage message = m_CurrentMessageItem.Message;
		if (message.Pages == null)
		{
			message.Pages = new List<CLevelMessagePage>();
		}
		message.Pages.Add(new CLevelMessagePage());
		RefreshPageList();
	}

	public void PageListItemPressed(LevelEditorMessagePageItem itemToEdit)
	{
		m_CurrentPageItem = itemToEdit;
		CLevelMessage message = m_CurrentMessageItem.Message;
		CLevelMessagePage currentPage = message.Pages[itemToEdit.PageIndex];
		PageDisplayPanel.SetActive(value: true);
		TitleLabelText.text = "Page " + itemToEdit.PageIndex;
		PageTextKeyInput.text = currentPage.PageTextKey;
		if (PageTextKeyInput.text.IsNullOrEmpty())
		{
			PageTextKeyInput.text = "Default_Key";
		}
		PageImagePathInput.text = currentPage.ImagePath;
		CheckTextLocale();
		if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.StoryDialog)
		{
			GameObject[] objectsActiveForDialogTypeLayout = ObjectsActiveForDialogTypeLayout;
			for (int i = 0; i < objectsActiveForDialogTypeLayout.Length; i++)
			{
				objectsActiveForDialogTypeLayout[i].SetActive(value: true);
			}
			m_DialogCharacterGUIDOptions = UIInfoTools.Instance.StoryCharacters.Select((StoryCharacterConfigUI c) => c.CharacterGUID).ToList();
			DialogCharacterGUIDDropDown.ClearOptions();
			DialogCharacterGUIDDropDown.AddOptions(m_DialogCharacterGUIDOptions);
			if (m_DialogCharacterGUIDOptions.Any((string c) => c == currentPage.SpeakingCharacterGUID))
			{
				DialogCharacterGUIDDropDown.value = m_DialogCharacterGUIDOptions.IndexOf(currentPage.SpeakingCharacterGUID);
			}
			else
			{
				DialogCharacterGUIDDropDown.value = 0;
			}
			DialogCharacterExpressionDropDown.ClearOptions();
			DialogCharacterExpressionDropDown.AddOptions(m_AvailableExpressions.Select((EExpression e) => e.ToString()).ToList());
			DialogCharacterExpressionDropDown.value = m_AvailableExpressions.IndexOf((EExpression)currentPage.SpeakingCharacterExpression);
			m_DialogBackgroundOptions = new List<string> { string.Empty };
			m_DialogBackgroundOptions.AddRange(UIInfoTools.Instance.narrativeImages.Select((NarrativeConfigUI it) => it.id));
			DialogBackgroundsDropDown.ClearOptions();
			DialogBackgroundsDropDown.AddOptions(m_DialogBackgroundOptions);
			DialogBackgroundsDropDown.value = ((!currentPage.SpeakingCharacterBackgroundId.IsNullOrEmpty()) ? m_DialogBackgroundOptions.IndexOf(currentPage.SpeakingCharacterBackgroundId) : 0);
			m_DialogAudioOptions = new List<string> { string.Empty };
			m_DialogAudioOptions.AddRange(from it in SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Where((AudioCategory it) => it.Name.StartsWith("VONarration")).SelectMany((AudioCategory it) => it.AudioItems)
				select it.Name into it
				orderby it
				select it);
			DialogAudioDropDown.ClearOptions();
			DialogAudioDropDown.AddOptions(m_DialogAudioOptions);
			DialogAudioDropDown.value = ((!currentPage.SpeakingCharacterAudioId.IsNullOrEmpty()) ? m_DialogAudioOptions.IndexOf(currentPage.SpeakingCharacterAudioId) : 0);
			CheckPortraitImagePath();
		}
		else
		{
			GameObject[] objectsActiveForDialogTypeLayout = ObjectsActiveForDialogTypeLayout;
			for (int i = 0; i < objectsActiveForDialogTypeLayout.Length; i++)
			{
				objectsActiveForDialogTypeLayout[i].SetActive(value: false);
			}
		}
	}

	public void ApplyPageChangesPressed()
	{
		CLevelMessage message = m_CurrentMessageItem.Message;
		CLevelMessagePage cLevelMessagePage = message.Pages[m_CurrentPageItem.PageIndex];
		cLevelMessagePage.PageTextKey = PageTextKeyInput.text;
		cLevelMessagePage.ImagePath = PageImagePathInput.text;
		if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.StoryDialog)
		{
			cLevelMessagePage.SpeakingCharacterGUID = m_DialogCharacterGUIDOptions[DialogCharacterGUIDDropDown.value];
			cLevelMessagePage.SpeakingCharacterExpression = (int)m_AvailableExpressions[DialogCharacterExpressionDropDown.value];
			cLevelMessagePage.SpeakingCharacterBackgroundId = m_DialogBackgroundOptions[DialogBackgroundsDropDown.value];
			cLevelMessagePage.SpeakingCharacterAudioId = m_DialogAudioOptions[DialogAudioDropDown.value];
		}
	}

	public void DeletePagePressed()
	{
		m_CurrentMessageItem.Message.Pages.RemoveAt(m_CurrentPageItem.PageIndex);
		RefreshPageList();
		PageDisplayPanel.SetActive(value: false);
	}

	public void CheckPortraitImagePath()
	{
		string characterGUID = m_DialogCharacterGUIDOptions[DialogCharacterGUIDDropDown.value];
		EExpression expression = m_AvailableExpressions[DialogCharacterExpressionDropDown.value];
		Sprite storyCharacterExpression = UIInfoTools.Instance.GetStoryCharacterExpression(characterGUID, expression);
		if (storyCharacterExpression != null)
		{
			CharacterPortraitCheck.sprite = storyCharacterExpression;
			CharacterPortraitCheckText.gameObject.SetActive(value: false);
		}
		else
		{
			CharacterPortraitCheck.sprite = null;
			CharacterPortraitCheckText.gameObject.SetActive(value: true);
		}
	}

	public void CheckTextLocale()
	{
		string translation = LocalizationManager.GetTranslation(PageTextKeyInput.text);
		LocaleCheckText.text = translation;
	}

	public void OnPortraitDropDownEdited()
	{
		CheckPortraitImagePath();
	}

	private void RefreshInteractabilityItemList()
	{
		foreach (LevelEditorLimitedInteractionItem interactabilityControlItem in m_InteractabilityControlItems)
		{
			UnityEngine.Object.Destroy(interactabilityControlItem.gameObject);
		}
		m_InteractabilityControlItems.Clear();
		if (m_CurrentMessageItem == null)
		{
			return;
		}
		CLevelMessage message = m_CurrentMessageItem.Message;
		if (message.InteractabilityProfileForMessage == null)
		{
			return;
		}
		int num = 0;
		foreach (CLevelUIInteractionProfile.CLevelUIInteractionSpecific item in message.InteractabilityProfileForMessage.ControlsToAllow)
		{
			LevelEditorLimitedInteractionItem component = UnityEngine.Object.Instantiate(InteractabilityItemListPrefab, InteractabilityItemParent).GetComponent<LevelEditorLimitedInteractionItem>();
			component.Init(item, InteractabilityItemPressed, num);
			m_InteractabilityControlItems.Add(component);
			num++;
		}
	}

	public void OnAddNewInteractabilityItemPressed()
	{
		CLevelMessage message = m_CurrentMessageItem.Message;
		if (message.InteractabilityProfileForMessage == null)
		{
			message.InteractabilityProfileForMessage = new CLevelUIInteractionProfile();
		}
		CLevelUIInteractionProfile.EIsolatedControlType controlType = CLevelUIInteractionProfile.ControlTypes.Single((CLevelUIInteractionProfile.EIsolatedControlType s) => s.ToString() == InteractabilityTypeDropDown.options[InteractabilityTypeDropDown.value].text);
		message.InteractabilityProfileForMessage.ControlsToAllow.Add(new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(controlType));
		RefreshInteractabilityItemList();
		if (m_InteractabilityControlItems.Count > 0)
		{
			InteractabilityItemPressed(m_InteractabilityControlItems.Last());
		}
	}

	private void InteractabilityItemPressed(LevelEditorLimitedInteractionItem itemPressed)
	{
		m_CurrentInteractabilityItem = itemPressed;
		CLevelUIInteractionProfile.CLevelUIInteractionSpecific cLevelUIInteractionSpecific = m_CurrentMessageItem.Message.InteractabilityProfileForMessage.ControlsToAllow[m_CurrentInteractabilityItem.ControlIndex];
		ControlBehaviourDropDown.options.Clear();
		ControlBehaviourDropDown.AddOptions(CLevelUIInteractionProfile.CLevelUIInteractionSpecific.ControlBehaviourTypes.Select((CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType t) => t.ToString()).ToList());
		ControlTypeLabel.text = "Control Type:[" + cLevelUIInteractionSpecific.ControlType.ToString() + "]";
		ControlContextualIDInput.text = cLevelUIInteractionSpecific.ControlIdentifier;
		ControlHighlightPrefabKeyInput.text = cLevelUIInteractionSpecific.HighlightPrefab;
		ControlContextualIndexInput.text = cLevelUIInteractionSpecific.ControlIndex.ToString();
		ControlBehaviourDropDown.value = (int)cLevelUIInteractionSpecific.ControlBehaviour;
		m_CurrentInteractabilityItemTileIndex = cLevelUIInteractionSpecific.ControlTileIndex;
		TileIndexParameterLabel.text = ((m_CurrentInteractabilityItemTileIndex == null) ? "Not Assigned" : ("X:" + m_CurrentInteractabilityItemTileIndex.X + "\nY:" + m_CurrentInteractabilityItemTileIndex.Y));
		ControlHighlightPrefabKeyElement.gameObject.SetActive(value: true);
		switch (cLevelUIInteractionSpecific.ControlType)
		{
		case CLevelUIInteractionProfile.EIsolatedControlType.CardSelection:
		case CLevelUIInteractionProfile.EIsolatedControlType.CardTopHalfSelection:
		case CLevelUIInteractionProfile.EIsolatedControlType.CardBottomHalfSelection:
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestDialogOption:
		case CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility:
		case CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility:
		case CLevelUIInteractionProfile.EIsolatedControlType.CharacterPortrait:
		case CLevelUIInteractionProfile.EIsolatedControlType.CardSelectionInitiative:
		case CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton:
			ControlContextualIDElement.gameObject.SetActive(value: true);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: true);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: false);
			ControlContextualIndexElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.BurnAvailableButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.BurnDiscardedButton:
			ControlContextualIDElement.gameObject.SetActive(value: false);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: true);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: false);
			ControlContextualIndexElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.TileSelection:
			ControlContextualIDElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: false);
			ControlContextualIndexElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: true);
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardTop:
		case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardBottom:
			ControlContextualIDElement.gameObject.SetActive(value: true);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: true);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: false);
			ControlContextualIndexElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.UseItemSlot:
			ControlContextualDropdown.options.Clear();
			ControlContextualDropdown.AddOptions(CItem.ItemSlots.Select((CItem.EItemSlot t) => t.ToString()).ToList());
			ControlContextualDropdown.value = cLevelUIInteractionSpecific.ControlContextTypeInt;
			FillActorDropDown(cLevelUIInteractionSpecific);
			ControlContextualIDElement.gameObject.SetActive(value: false);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement.gameObject.SetActive(value: true);
			ControlContextualDropDownElement2.gameObject.SetActive(value: true);
			ControlContextualIndexElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.CharacterTabIcon:
			FillActorDropDown(cLevelUIInteractionSpecific);
			ControlContextualIDElement.gameObject.SetActive(value: false);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: true);
			ControlContextualIndexElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestConfirmButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestCancelButton:
			FillActorDropDown(cLevelUIInteractionSpecific);
			ControlContextualIDElement.gameObject.SetActive(value: false);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: true);
			ControlContextualIndexElement.gameObject.SetActive(value: false);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		default:
			ControlContextualIDElement.gameObject.SetActive(value: false);
			ControlHighlightPrefabKeyElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement.gameObject.SetActive(value: false);
			ControlContextualDropDownElement2.gameObject.SetActive(value: false);
			ControlContextualIndexElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			break;
		}
		InteractabilityItemDisplayPanel.SetActive(value: true);
	}

	public void DeleteInteractabilityItemPressed()
	{
		m_CurrentMessageItem.Message.InteractabilityProfileForMessage.ControlsToAllow.RemoveAt(m_CurrentInteractabilityItem.ControlIndex);
		RefreshInteractabilityItemList();
		InteractabilityItemDisplayPanel.SetActive(value: false);
	}

	public void ApplyInteractabilityItemPressed()
	{
		CLevelUIInteractionProfile.CLevelUIInteractionSpecific cLevelUIInteractionSpecific = m_CurrentMessageItem.Message.InteractabilityProfileForMessage.ControlsToAllow[m_CurrentInteractabilityItem.ControlIndex];
		cLevelUIInteractionSpecific.ControlIdentifier = ControlContextualIDInput.text;
		cLevelUIInteractionSpecific.ControlIndex = (string.IsNullOrEmpty(ControlContextualIndexInput.text) ? (-1) : int.Parse(ControlContextualIndexInput.text));
		cLevelUIInteractionSpecific.ControlTileIndex = m_CurrentInteractabilityItemTileIndex;
		cLevelUIInteractionSpecific.ControlBehaviour = (CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType)ControlBehaviourDropDown.value;
		cLevelUIInteractionSpecific.HighlightPrefab = ControlHighlightPrefabKeyInput.text;
		if (cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton || cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.UseItemSlot)
		{
			cLevelUIInteractionSpecific.ControlContextTypeInt = ControlContextualDropdown.value;
		}
		if (cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.CharacterTabIcon || cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ShortRestButton || cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton || cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ShortRestConfirmButton || cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ShortRestCancelButton || cLevelUIInteractionSpecific.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.UseItemSlot)
		{
			cLevelUIInteractionSpecific.ControlIdentifier2 = ((ControlContextualDropdown2.value == 0) ? string.Empty : m_ActorOptions[ControlContextualDropdown2.value - 1].ActorGuid);
		}
	}

	public void FillActorDropDown(CLevelUIInteractionProfile.CLevelUIInteractionSpecific currentControl)
	{
		ControlContextualDropdown2.options.Clear();
		m_ActorOptions.Clear();
		m_ActorOptions.AddRange(ScenarioManager.Scenario.PlayerActors);
		List<string> list = new List<string> { "NONE" };
		list.AddRange(m_ActorOptions.Select((CActor a) => a.GetPrefabName() + " - [" + a.ActorGuid + "]"));
		ControlContextualDropdown2.AddOptions(list);
		int b = m_ActorOptions.FindIndex((CActor a) => a.ActorGuid == currentControl.ControlIdentifier2) + 1;
		ControlContextualDropdown2.value = Mathf.Max(0, b);
	}

	public void SelectTileForInteractabilityItemPressed()
	{
		LevelEditorController.SelectTile(TileSelected);
	}

	public void TileSelected(CClientTile tileSelected)
	{
		m_CurrentInteractabilityItemTileIndex = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		TileIndexParameterLabel.text = ((m_CurrentInteractabilityItemTileIndex == null) ? "Not Assigned" : ("X:" + m_CurrentInteractabilityItemTileIndex.X + "\nY:" + m_CurrentInteractabilityItemTileIndex.Y));
	}

	private void RefreshEventTriggerDisplays()
	{
		if (!(m_CurrentMessageItem == null))
		{
			CLevelMessage message = m_CurrentMessageItem.Message;
			DisplayTrigger.InitForMessageTrigger(message.DisplayTrigger);
			DismissTrigger.InitForMessageTrigger(message.DismissTrigger);
		}
	}

	public void ApplyTriggersPressed()
	{
		DisplayTrigger.SaveValuesToTrigger();
		DismissTrigger.SaveValuesToTrigger();
	}
}
