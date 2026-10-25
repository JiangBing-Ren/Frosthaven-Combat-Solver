using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronos;
using FH.ApparanceBaking;
using FH.GraphicalUserInterface.MainMenu;
using GLOOM;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorStandardDataPanel : MonoBehaviour
{
	private enum EMapLocationKeyType
	{
		None,
		MapIconMesh
	}

	[Header("BaseData")]
	public TMP_InputField LevelNameField;

	public TMP_Dropdown PartyChoiceTypeDropDown;

	public Toggle PreventUnspecfiedInteractionToggle;

	public Toggle UseRealtime;

	[Header("Randomise and Shuffle Data")]
	public Toggle ShuffleAttackModsEnabledForPlayersToggle;

	public Toggle ShuffleAbilityDecksEnabledForMonstersToggle;

	public Toggle ShuffleAttackModsEnabledForMonstersToggle;

	public Toggle RandomiseOnLoadToggle;

	public Toggle SplitStartingTileIndexesByIslands;

	public Toggle ExplicitlyDontSplitStartingTileIndexesByIslands;

	public TextMeshProUGUI SaveStatusText;

	public LayoutElement MonsterLevelBasedOnPartyToggleElement;

	public Toggle MonsterLevelBasedOnPartyToggle;

	public LayoutElement MonsterMaxHealthBasedOnPartyToggleElement;

	public Toggle MonsterMaxHealthBasedOnPartyToggle;

	public LayoutElement SetMonsterHealthToMaxOnPlayToggleElement;

	public Toggle SetMonsterHealthToMaxOnPlayToggle;

	[Header("Party Spawn Type Options")]
	public TextMeshProUGUI PartySpawnTypeLabel;

	public TextMeshProUGUI PartySpawnDescriptionLabel;

	public GameObject InlineRemovalListItemPrefab;

	public LayoutElement AllowAllButtonGroup;

	public Toggle FixStartingRotationToggle;

	[Header("Map Location Meta Data")]
	public TextMeshProUGUI EditedKeyTypeText;

	public TMP_Dropdown EditedItemDropdown;

	public RawImage ResolvedImage;

	public LayoutElement KeyDropdownElement;

	public LayoutElement ResolvedImageElement;

	public GameObject ImageLookupFailedObject;

	public LayoutGroup MapIconItemParent;

	public LayoutGroup StartingTileIndexItemParent;

	private List<LevelEditorListItemInlineButtons> m_MapIconKeyItems;

	private List<LevelEditorListItemInlineButtons> m_StartingTileIndexItems;

	private int m_CurrentlyEditedMapKeyIndex;

	private EMapLocationKeyType m_CurrentlyEditedMapKeyType;

	[Header("Allowed Items Panel")]
	public GameObject ItemsAllowedPanel;

	public TMP_Dropdown ItemsDropDown;

	public RectTransform ItemsParent;

	private List<LevelEditorListItemInlineButtons> m_ItemItems;

	[Header("Allowed Characters Panel")]
	public GameObject CharactersAllowedPanel;

	public TMP_Dropdown CharactersDropDown;

	public RectTransform CharactersParent;

	private List<LevelEditorListItemInlineButtons> m_CharacterItems;

	private string m_CurrentlySelectedCharacter;

	[Header("Allowed Abilites Panel")]
	public GameObject AbilitiesAllowedPanel;

	public TextMeshProUGUI AbilitiesAllowedPanelTitle;

	public TMP_Dropdown CharAbilitiesDropDown;

	public RectTransform AbilitiesParent;

	private List<LevelEditorListItemInlineButtons> m_CharacterAbilityItems;

	[Header("Character Rotation Panel")]
	public GameObject FixedMercStartingRotationPanel;

	public TextMeshProUGUI RotationStatusText;

	public Button RotationStatusButton;

	public TextMeshProUGUI FixedRotationStartingTileText;

	public TextMeshProUGUI FixedRotationEndingTileText;

	private TileIndex m_FacingDirectionStartIndex;

	private TileIndex m_FacingDirectionEndIndex;

	private IEnumerator m_ShowRotationTilesRoutine;

	[Header("Camera Profile")]
	public LevelEditorCameraProfilePanel CameraProfilePanel;

	public void InitUIForLoadedLevel()
	{
		LevelNameField.text = SaveData.Instance.Global.CurrentEditorLevelData.Name;
		PartyChoiceTypeDropDown.options.Clear();
		PartyChoiceTypeDropDown.AddOptions(CCustomLevelData.LevelPartyChoiceTypes.Select((ELevelPartyChoiceType t) => t.ToString()).ToList());
		PartyChoiceTypeDropDown.value = (int)SaveData.Instance.Global.CurrentEditorLevelData.PartySpawnType;
		PreventUnspecfiedInteractionToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.ShouldPreventUnspecifiedInteraction);
		UseRealtime.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.UseRealtime);
		MonsterLevelBasedOnPartyToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.EnemyLevelsScaleToPartyLevel);
		MonsterMaxHealthBasedOnPartyToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.EnemyMaxHealthBasedOnPartyLevel);
		SetMonsterHealthToMaxOnPlayToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.SetEnemyHealthToMaxOnPlay);
		ShuffleAttackModsEnabledForPlayersToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.ShuffleAttackModsEnabledForPlayers);
		ShuffleAbilityDecksEnabledForMonstersToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.ShuffleAbilityDecksEnabledForMonsters);
		ShuffleAttackModsEnabledForMonstersToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.ShuffleAttackModsEnabledForMonsters);
		RandomiseOnLoadToggle.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.RandomiseOnLoad);
		SplitStartingTileIndexesByIslands.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.SplitStartingTileIndexesByIslands);
		ExplicitlyDontSplitStartingTileIndexesByIslands.SetValue(SaveData.Instance.Global.CurrentEditorLevelData.ExplicitlyDontSplitStartingTileIndexesByIslands);
		OnPartySpawnTypeChanged();
		UpdateMapDataPanelForLevel();
		UpdateStartTileIndexPanelForLevel();
		UpdateFixedStartingRotationPanelForLevel();
		UpdateStartingCameraSettings();
		EditedItemDropdown.AddOptions(GlobalSettings.Instance.m_AdventureLocationMaterialSettings.Select((GlobalSettings.AdventureLocationMaterialSettings t) => t.LocationMaterial.name).ToList());
		EditedItemDropdown.onValueChanged.AddListener(OnFinishedEditionItemDropdownField);
	}

	public async void OnSaveDataPressed()
	{
		await AttemptToSaveLevel();
	}

	public async void OnDiscardAndPlayPressed()
	{
		MainMenuUIAutomation.LevelEditorAutoSelectMode = true;
		SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
		GlobalData global = SaveData.Instance.Global;
		global.CurrentCustomLevelData = await SaveData.Instance.Global.CurrentEditorLevelData.DeepCopySerializableObject<CCustomLevelData>();
		EnvironmentLoader.Instance.DisableApparance();
		if (SaveData.Instance.Global.CurrentEditorLevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty)
		{
			LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty);
			MainMenuUIManager.SetLoadingCompleteCallback(OnMainMenuLoadedToLoadPartyCallback);
			SceneController.Instance.LoadMainMenu();
		}
		else
		{
			LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel);
			SceneController.Instance.LoadCustomLevel(SaveData.Instance.Global.CurrentCustomLevelData);
		}
	}

	public async void OnSaveAndPlayPressed()
	{
		if (await AttemptToSaveLevel())
		{
			OnDiscardAndPlayPressed();
		}
	}

	public void OnPartySpawnTypeChanged()
	{
		InitUIForCurrentPartySpawnType();
	}

	public void OnMonstersLevelBasedOnPartyToggled(bool value)
	{
		MonsterMaxHealthBasedOnPartyToggleElement.gameObject.SetActive(value);
	}

	private void OnMainMenuLoadedToLoadPartyCallback()
	{
		MainMenuUIManager.Instance.ModeSelectionScreen.SetModeSuccessfullyLoadedCallback(OnModeSuccessfullyLoadedCallback);
		MainMenuUIManager.Instance.ModeSelectionScreen.ShowForLevelEditor();
	}

	private async void OnModeSuccessfullyLoadedCallback()
	{
		if (!string.IsNullOrWhiteSpace(MainMenuUIAutomation.SaveFile))
		{
			LoadCustomLevelDataService service = new LoadCustomLevelDataService(SaveData.Instance.Global.CurrentEditorLevelData);
			IGameSaveData gameSaveData = (await service.GetSaves()).FirstOrDefault((IGameSaveData s) => s.GameName == MainMenuUIAutomation.SaveFile);
			if (gameSaveData != null)
			{
				await service.LoadGame(gameSaveData);
				return;
			}
		}
		MainMenuUIManager.Instance.LevelEditorMainMenuScreen.OpenLoadPartyDialogForLevelData(SaveData.Instance.Global.CurrentEditorLevelData);
	}

	private async Task<bool> AttemptToSaveLevel()
	{
		ELevelPartyChoiceType eLevelPartyChoiceType = CCustomLevelData.LevelPartyChoiceTypes[PartyChoiceTypeDropDown.value];
		if (LevelEditorController.s_Instance.CurrentLevelEditingState != LevelEditorController.ELevelEditingState.PropActorPlacement)
		{
			SaveStatusText.text = "<color=red>Cannot save until all rooms and doors are placed";
			return false;
		}
		if (eLevelPartyChoiceType == ELevelPartyChoiceType.None)
		{
			SaveStatusText.text = "<color=red>Cannot save Party Choice type \"None\"";
			return false;
		}
		if (string.IsNullOrEmpty(LevelNameField.text.Trim()))
		{
			SaveStatusText.text = "<color=red>Cannot save without Level Name";
			return false;
		}
		if (!ScenarioManager.CurrentScenarioState.Maps.Any((CMap x) => x.DungeonEntranceDoor != null) && (SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes == null || SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.Count <= 0))
		{
			SaveStatusText.text = "<color=red>Cannot save without a dungeon entrance door set or starting tile indexes set - set one of these";
			return false;
		}
		LevelEditorController.s_Instance.m_LevelEditorUIInstance.ApparancePanel.EnsureAllStylesHaveAnOverride();
		SaveData.Instance.Global.CurrentEditorLevelData.Name = LevelNameField.text.Trim();
		SaveData.Instance.Global.CurrentEditorLevelData.PartySpawnType = eLevelPartyChoiceType;
		SaveData.Instance.Global.CurrentEditorLevelData.ShouldPreventUnspecifiedInteraction = PreventUnspecfiedInteractionToggle.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.UseRealtime = UseRealtime.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.EnemyLevelsScaleToPartyLevel = MonsterLevelBasedOnPartyToggle.isOn;
		if (MonsterLevelBasedOnPartyToggle.isOn)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.EnemyMaxHealthBasedOnPartyLevel = MonsterMaxHealthBasedOnPartyToggle.isOn;
		}
		else
		{
			SaveData.Instance.Global.CurrentEditorLevelData.EnemyMaxHealthBasedOnPartyLevel = false;
		}
		SaveData.Instance.Global.CurrentEditorLevelData.SetEnemyHealthToMaxOnPlay = SetMonsterHealthToMaxOnPlayToggle.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.ShuffleAttackModsEnabledForPlayers = ShuffleAttackModsEnabledForPlayersToggle.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.ShuffleAbilityDecksEnabledForMonsters = ShuffleAbilityDecksEnabledForMonstersToggle.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.ShuffleAttackModsEnabledForMonsters = ShuffleAttackModsEnabledForMonstersToggle.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.RandomiseOnLoad = RandomiseOnLoadToggle.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.SplitStartingTileIndexesByIslands = SplitStartingTileIndexesByIslands.isOn;
		SaveData.Instance.Global.CurrentEditorLevelData.ExplicitlyDontSplitStartingTileIndexesByIslands = ExplicitlyDontSplitStartingTileIndexesByIslands.isOn;
		ScenarioManager.CurrentScenarioState.Update(saveHiddenUnits: true);
		ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Clear();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			prop.IsBaked = false;
		}
		CCustomLevelData currentEditorLevelData = SaveData.Instance.Global.CurrentEditorLevelData;
		currentEditorLevelData.ScenarioState = await ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>();
		SaveData.Instance.Global.CurrentEditorLevelData.DLCUsed = DLCRegistry.GetDLCUsedInLevel(SaveData.Instance.Global.CurrentEditorLevelData);
		SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState.ScenarioEventLog?.Events?.Clear();
		(bool, string) tuple = await SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(SaveData.Instance.Global.CurrentEditorLevelData, LevelNameField.text.Trim());
		if (tuple.Item1)
		{
			SaveStatusText.text = "<color=green> - Saved to " + tuple.Item2 + "- </color>";
			InitUIForLoadedLevel();
			return true;
		}
		SaveStatusText.text = "<color=red> - Save Failed - </color>";
		return false;
	}

	public void InitUIForCurrentPartySpawnType()
	{
		ELevelPartyChoiceType eLevelPartyChoiceType = CCustomLevelData.LevelPartyChoiceTypes[PartyChoiceTypeDropDown.value];
		PartySpawnTypeLabel.text = eLevelPartyChoiceType.ToString();
		switch (eLevelPartyChoiceType)
		{
		case ELevelPartyChoiceType.None:
		case ELevelPartyChoiceType.PresetSpawnAtEntrance:
		case ELevelPartyChoiceType.PresetSpawnSpecificLocations:
			PartySpawnDescriptionLabel.text = "Please ensure that you have at least one Hero spawned in the level." + ((eLevelPartyChoiceType == ELevelPartyChoiceType.PresetSpawnAtEntrance) ? "\n\nNote: The player will still choose the starting locations of Heroes around the level entrance" : "");
			ItemsAllowedPanel.SetActive(value: false);
			CharactersAllowedPanel.SetActive(value: false);
			AbilitiesAllowedPanel.SetActive(value: false);
			AllowAllButtonGroup.gameObject.SetActive(value: false);
			MonsterLevelBasedOnPartyToggleElement.gameObject.SetActive(value: false);
			MonsterMaxHealthBasedOnPartyToggleElement.gameObject.SetActive(value: false);
			break;
		case ELevelPartyChoiceType.LoadAdventureParty:
			PartySpawnDescriptionLabel.text = "Please ensure NO heroes are spawned in the level - They will be chosen upfront by the player on entering the level";
			ItemsAllowedPanel.SetActive(value: false);
			CharactersAllowedPanel.SetActive(value: false);
			AbilitiesAllowedPanel.SetActive(value: false);
			AllowAllButtonGroup.gameObject.SetActive(value: false);
			MonsterLevelBasedOnPartyToggleElement.gameObject.SetActive(value: true);
			MonsterMaxHealthBasedOnPartyToggleElement.gameObject.SetActive(SaveData.Instance.Global.CurrentEditorLevelData.EnemyLevelsScaleToPartyLevel);
			break;
		case ELevelPartyChoiceType.ChooseOwnParty:
			PartySpawnDescriptionLabel.text = "Please ensure NO heroes are spawned in the level - They will be chosen upfront by the player on entering the level";
			ItemsAllowedPanel.SetActive(value: false);
			CharactersAllowedPanel.SetActive(value: false);
			AbilitiesAllowedPanel.SetActive(value: false);
			AllowAllButtonGroup.gameObject.SetActive(value: false);
			MonsterLevelBasedOnPartyToggleElement.gameObject.SetActive(value: true);
			MonsterMaxHealthBasedOnPartyToggleElement.gameObject.SetActive(SaveData.Instance.Global.CurrentEditorLevelData.EnemyLevelsScaleToPartyLevel);
			break;
		case ELevelPartyChoiceType.ChooseOwnPartyRestricted:
			PartySpawnDescriptionLabel.text = "Please ensure NO heroes are spawned in the level - They will be chosen upfront by the player on entering the level. \n\nAdditionally, please set up the restriction for the party choice in the right panels.";
			ItemsAllowedPanel.SetActive(value: true);
			CharactersAllowedPanel.SetActive(value: true);
			AbilitiesAllowedPanel.SetActive(value: true);
			AllowAllButtonGroup.gameObject.SetActive(value: true);
			MonsterLevelBasedOnPartyToggleElement.gameObject.SetActive(value: true);
			MonsterMaxHealthBasedOnPartyToggleElement.gameObject.SetActive(SaveData.Instance.Global.CurrentEditorLevelData.EnemyLevelsScaleToPartyLevel);
			FillAllowedItemsListFromCurrentData();
			FillAllowedCharacterListFromCurrentData();
			FillAllowedCharacterAbilityListFromCurrentData();
			break;
		}
	}

	public void FillAllowedItemsListFromCurrentData()
	{
		if (m_ItemItems != null)
		{
			foreach (LevelEditorListItemInlineButtons itemItem in m_ItemItems)
			{
				Object.Destroy(itemItem.gameObject);
			}
			m_ItemItems.Clear();
		}
		else
		{
			m_ItemItems = new List<LevelEditorListItemInlineButtons>();
		}
		List<ItemCardYMLData> allItems = ScenarioRuleClient.SRLYML.ItemCards.ToList();
		List<string> currentlyAllowedItems = SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyItems.Where((string s) => allItems.Any((ItemCardYMLData i) => i.Name == s)).ToList();
		for (int num = 0; num < currentlyAllowedItems.Count; num++)
		{
			LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, ItemsParent).GetComponent<LevelEditorListItemInlineButtons>();
			component.SetupListItem(currentlyAllowedItems[num], num, OnRemoveAllowedItemPressed);
			m_ItemItems.Add(component);
		}
		ItemsDropDown.ClearOptions();
		ItemsDropDown.AddOptions((from i in allItems
			where !currentlyAllowedItems.Contains(i.Name)
			select i.Name).ToList());
		ItemsDropDown.value = 0;
	}

	public void OnAddAllowedItemPressed()
	{
		if (ItemsDropDown.options.Count > 0)
		{
			string text = ItemsDropDown.options[ItemsDropDown.value].text;
			if (!string.IsNullOrEmpty(text) && !SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyItems.Contains(text))
			{
				SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyItems.Add(text);
			}
		}
		FillAllowedItemsListFromCurrentData();
	}

	public void OnRemoveAllowedItemPressed(LevelEditorListItemInlineButtons itemItem)
	{
		if (itemItem.ItemIndex >= SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyItems.Count)
		{
			Log.Error("Cant remove selected item - unexpected index on item list item");
		}
		else
		{
			SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyItems.RemoveAt(itemItem.ItemIndex);
		}
		FillAllowedItemsListFromCurrentData();
	}

	public void OnAddAllItemsToAllowedListPressed()
	{
		SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyItems = ScenarioRuleClient.SRLYML.ItemCards.Select((ItemCardYMLData i) => i.Name).ToList();
		FillAllowedItemsListFromCurrentData();
	}

	public void FillAllowedCharacterListFromCurrentData()
	{
		if (m_CharacterItems != null)
		{
			foreach (LevelEditorListItemInlineButtons characterItem in m_CharacterItems)
			{
				Object.Destroy(characterItem.gameObject);
			}
			m_CharacterItems.Clear();
		}
		else
		{
			m_CharacterItems = new List<LevelEditorListItemInlineButtons>();
		}
		List<string> allChars = CharacterClassManager.Classes.Select((CCharacterClass s) => s.ID).ToList();
		List<string> currentlyAllowedChars = SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.Where((string w) => allChars.Any((string a) => a == w)).ToList();
		for (int num = 0; num < currentlyAllowedChars.Count; num++)
		{
			LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, CharactersParent).GetComponent<LevelEditorListItemInlineButtons>();
			component.SetupListItem(currentlyAllowedChars[num], num, OnRemoveAllowedCharacterPressed, OnAllowedCharacterPressed);
			m_CharacterItems.Add(component);
		}
		CharactersDropDown.ClearOptions();
		CharactersDropDown.AddOptions(allChars.Where((string c) => !currentlyAllowedChars.Contains(c)).ToList());
		CharactersDropDown.value = 0;
	}

	public void OnAddAllowedCharacterPressed()
	{
		if (CharactersDropDown.options.Count > 0)
		{
			string text = CharactersDropDown.options[CharactersDropDown.value].text;
			if (!string.IsNullOrEmpty(text) && !SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.Contains(text))
			{
				SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.Add(text);
			}
		}
		FillAllowedCharacterListFromCurrentData();
	}

	public void OnRemoveAllowedCharacterPressed(LevelEditorListItemInlineButtons charItem)
	{
		if (charItem.ItemIndex >= SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.Count)
		{
			Log.Error("Cant remove selected character - unexpected index on character list item");
		}
		else
		{
			SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.RemoveAt(charItem.ItemIndex);
		}
		FillAllowedCharacterListFromCurrentData();
	}

	public void OnAllowedCharacterPressed(LevelEditorListItemInlineButtons charItem)
	{
		if (charItem.ItemIndex >= SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.Count)
		{
			Log.Error("Cant show details for selected character - unexpected index on character list item");
			m_CurrentlySelectedCharacter = null;
		}
		else
		{
			m_CurrentlySelectedCharacter = SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs[charItem.ItemIndex];
		}
		FillAllowedCharacterAbilityListFromCurrentData();
	}

	public void OnAddAllCharactersToAllowedListPressed()
	{
		List<string> source = CharacterClassManager.Classes.Select((CCharacterClass s) => s.ID).ToList();
		SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs = source.ToList();
		FillAllowedCharacterListFromCurrentData();
	}

	public void FillAllowedCharacterAbilityListFromCurrentData()
	{
		if (m_CharacterAbilityItems != null)
		{
			foreach (LevelEditorListItemInlineButtons characterAbilityItem in m_CharacterAbilityItems)
			{
				Object.Destroy(characterAbilityItem.gameObject);
			}
			m_CharacterAbilityItems.Clear();
		}
		else
		{
			m_CharacterAbilityItems = new List<LevelEditorListItemInlineButtons>();
		}
		AbilitiesAllowedPanelTitle.text = "Select a character to see allowed abilities";
		CharAbilitiesDropDown.ClearOptions();
		CharAbilitiesDropDown.value = 0;
		if (string.IsNullOrEmpty(m_CurrentlySelectedCharacter) || !SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs.Any((string c) => c == m_CurrentlySelectedCharacter))
		{
			return;
		}
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.ID == m_CurrentlySelectedCharacter);
		if (cCharacterClass == null)
		{
			Log.Error("Current ruleset is missing Playable character with the ID \"" + m_CurrentlySelectedCharacter + "\"");
			return;
		}
		AbilitiesAllowedPanelTitle.text = "Showing allowed abilities for \"" + LocalizationManager.GetTranslation(cCharacterClass.LocKey) + "\"";
		if (!SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Select((CAllowedAbilitiesPerCharacter s) => s.CharacterID).Contains(m_CurrentlySelectedCharacter))
		{
			SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Add(new CAllowedAbilitiesPerCharacter(m_CurrentlySelectedCharacter, new List<string>()));
		}
		List<string> currentAllowedStrings = SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Single((CAllowedAbilitiesPerCharacter s) => s.CharacterID == m_CurrentlySelectedCharacter).Abilities;
		List<CAbilityCard> currentAllowedCards = cCharacterClass.AbilityCardsPool.Where((CAbilityCard c) => currentAllowedStrings.Contains(c.Name)).ToList();
		for (int num = 0; num < currentAllowedCards.Count; num++)
		{
			LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, AbilitiesParent).GetComponent<LevelEditorListItemInlineButtons>();
			component.SetupListItem(currentAllowedCards[num].Name + "[I:" + currentAllowedCards[num].Initiative + "]", num, OnRemoveAllowedCharacterAbilityPressed);
			m_CharacterAbilityItems.Add(component);
		}
		CharAbilitiesDropDown.AddOptions((from a in cCharacterClass.AbilityCardsPool
			where !currentAllowedCards.Contains(a)
			select a.Name).ToList());
	}

	public void OnAddAllowedCharacterAbilityPressed()
	{
		if (CharAbilitiesDropDown.options.Count > 0 && !string.IsNullOrEmpty(m_CurrentlySelectedCharacter))
		{
			string text = CharAbilitiesDropDown.options[CharAbilitiesDropDown.value].text;
			if (!string.IsNullOrEmpty(text))
			{
				if (!SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Select((CAllowedAbilitiesPerCharacter s) => s.CharacterID).Contains(m_CurrentlySelectedCharacter))
				{
					SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Add(new CAllowedAbilitiesPerCharacter(m_CurrentlySelectedCharacter, new List<string>()));
				}
				List<string> abilities = SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Single((CAllowedAbilitiesPerCharacter s) => s.CharacterID == m_CurrentlySelectedCharacter).Abilities;
				if (!abilities.Contains(text))
				{
					abilities.Add(text);
				}
			}
		}
		FillAllowedCharacterAbilityListFromCurrentData();
	}

	public void OnRemoveAllowedCharacterAbilityPressed(LevelEditorListItemInlineButtons charAbilityItem)
	{
		if (string.IsNullOrEmpty(m_CurrentlySelectedCharacter))
		{
			FillAllowedCharacterAbilityListFromCurrentData();
		}
		List<string> list = SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.SingleOrDefault((CAllowedAbilitiesPerCharacter s) => s.CharacterID == m_CurrentlySelectedCharacter)?.Abilities;
		if (list == null || charAbilityItem.ItemIndex >= list.Count)
		{
			Log.Error("Cant remove selected character ability - unexpected index on character ability list item");
		}
		else
		{
			list.RemoveAt(charAbilityItem.ItemIndex);
		}
		FillAllowedCharacterAbilityListFromCurrentData();
	}

	public void OnAddAllCharacterAbilitiesToAllowedListPressed()
	{
		foreach (string character in SaveData.Instance.Global.CurrentEditorLevelData.AllowedPartyCharacterIDs)
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.ID == character);
			if (cCharacterClass == null)
			{
				Log.Error("Current ruleset is missing Playable character with the name \"" + cCharacterClass?.ToString() + "\"");
			}
			else if (!SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Select((CAllowedAbilitiesPerCharacter s) => s.CharacterID).Contains(character))
			{
				SaveData.Instance.Global.CurrentEditorLevelData.AllowedAbilitiesPerCharacterList.Add(new CAllowedAbilitiesPerCharacter(character, cCharacterClass.AbilityCardsPool.Select((CAbilityCard c) => c.Name).ToList()));
			}
		}
		FillAllowedCharacterAbilityListFromCurrentData();
	}

	public void UpdateMapDataPanelForLevel()
	{
		m_CurrentlyEditedMapKeyType = EMapLocationKeyType.None;
		m_CurrentlyEditedMapKeyIndex = -1;
		KeyDropdownElement.gameObject.SetActive(value: false);
		ResolvedImageElement.gameObject.SetActive(value: false);
		EditedKeyTypeText.text = "Add or select key to lists on the right to edit.";
		if (m_MapIconKeyItems == null)
		{
			m_MapIconKeyItems = new List<LevelEditorListItemInlineButtons>();
		}
		foreach (LevelEditorListItemInlineButtons mapIconKeyItem in m_MapIconKeyItems)
		{
			Object.Destroy(mapIconKeyItem.gameObject);
		}
		m_MapIconKeyItems.Clear();
		if (SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames != null)
		{
			for (int i = 0; i < SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames.Count; i++)
			{
				LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, MapIconItemParent.transform).GetComponent<LevelEditorListItemInlineButtons>();
				m_MapIconKeyItems.Add(component);
				component.SetupListItem(SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames[i], i, OnDeleteMapIconKey, OnEditMapIconKey);
			}
		}
	}

	public void OnAddMapIconKeyPressed()
	{
		if (SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames == null)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames = new List<string>();
		}
		SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames.Add(string.Empty);
		UpdateMapDataPanelForLevel();
		OnEditMapIconKey(m_MapIconKeyItems.Last());
	}

	private void OnDeleteMapIconKey(LevelEditorListItemInlineButtons item)
	{
		SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames.RemoveAt(item.ItemIndex);
		UpdateMapDataPanelForLevel();
	}

	private void OnEditMapIconKey(LevelEditorListItemInlineButtons item)
	{
		EditedKeyTypeText.text = "Map location icon";
		m_CurrentlyEditedMapKeyType = EMapLocationKeyType.MapIconMesh;
		m_CurrentlyEditedMapKeyIndex = item.ItemIndex;
		KeyDropdownElement.gameObject.SetActive(value: true);
		ResolvedImageElement.gameObject.SetActive(value: true);
		string mapMaterialName = SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames[m_CurrentlyEditedMapKeyIndex];
		EditedItemDropdown.value = EditedItemDropdown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == mapMaterialName);
		ResolveValuesFromKeyInput();
	}

	public void OnFinishedEditionItemDropdownField(int value)
	{
		if (m_CurrentlyEditedMapKeyType == EMapLocationKeyType.MapIconMesh)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.MapIconMaterialNames[m_CurrentlyEditedMapKeyIndex] = EditedItemDropdown.options[EditedItemDropdown.value].text;
			m_MapIconKeyItems[m_CurrentlyEditedMapKeyIndex].DescriptionLabel.text = EditedItemDropdown.options[EditedItemDropdown.value].text;
		}
		ResolveValuesFromKeyInput();
	}

	private void ResolveValuesFromKeyInput()
	{
		EMapLocationKeyType currentlyEditedMapKeyType = m_CurrentlyEditedMapKeyType;
		if (currentlyEditedMapKeyType == EMapLocationKeyType.None || currentlyEditedMapKeyType != EMapLocationKeyType.MapIconMesh)
		{
			return;
		}
		Material material = GlobalSettings.Instance.m_AdventureLocationMaterialSettings.FirstOrDefault((GlobalSettings.AdventureLocationMaterialSettings m) => m.LocationMaterial.name == EditedItemDropdown.options[EditedItemDropdown.value].text)?.LocationMaterial;
		if (material != null)
		{
			Texture texture = material.GetTexture("_MainTex");
			if (texture != null)
			{
				ResolvedImage.texture = texture;
				ImageLookupFailedObject.SetActive(value: false);
			}
			else
			{
				ResolvedImage.texture = null;
				ImageLookupFailedObject.SetActive(value: true);
			}
		}
		else
		{
			ResolvedImage.texture = null;
			ImageLookupFailedObject.SetActive(value: true);
		}
	}

	public void UpdateStartTileIndexPanelForLevel()
	{
		if (m_StartingTileIndexItems == null)
		{
			m_StartingTileIndexItems = new List<LevelEditorListItemInlineButtons>();
		}
		foreach (LevelEditorListItemInlineButtons startingTileIndexItem in m_StartingTileIndexItems)
		{
			Object.Destroy(startingTileIndexItem.gameObject);
		}
		m_StartingTileIndexItems.Clear();
		if (SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes != null)
		{
			for (int i = 0; i < SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.Count; i++)
			{
				LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, StartingTileIndexItemParent.transform).GetComponent<LevelEditorListItemInlineButtons>();
				m_StartingTileIndexItems.Add(component);
				TileIndex tileIndex = SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes[i];
				component.SetupListItem("Tile: " + tileIndex.X + "," + tileIndex.Y, i, OnDeleteStartTileIndexKey, TileItemSelected);
			}
		}
	}

	public void OnButtonSelectTilePressed()
	{
		LevelEditorController.SelectTile(TileSelected);
	}

	public void TileItemSelected(LevelEditorListItemInlineButtons tileItemSelected)
	{
		TileIndex tileIndex = SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes[tileItemSelected.ItemIndex];
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
		LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
	}

	public void TileSelected(CClientTile tileSelected)
	{
		TileIndex newTileIndex = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		if (!SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.Any((TileIndex x) => TileIndex.Compare(x, newTileIndex)))
		{
			SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.Add(new TileIndex(tileSelected.m_Tile.m_ArrayIndex));
		}
		UpdateStartTileIndexPanelForLevel();
	}

	private void OnDeleteStartTileIndexKey(LevelEditorListItemInlineButtons item)
	{
		SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.RemoveAt(item.ItemIndex);
		UpdateStartTileIndexPanelForLevel();
	}

	public void OnFixStartingRotationToggled(bool value)
	{
		SaveData.Instance.Global.CurrentEditorLevelData.UsesFixedMercStartingRotation = value;
		UpdateFixedStartingRotationPanelForLevel();
	}

	public void UpdateFixedStartingRotationPanelForLevel()
	{
		FixStartingRotationToggle.SetIsOnWithoutNotify(SaveData.Instance.Global.CurrentEditorLevelData.UsesFixedMercStartingRotation);
		FixedMercStartingRotationPanel.SetActive(SaveData.Instance.Global.CurrentEditorLevelData.UsesFixedMercStartingRotation);
		if (SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices == null || SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices.Count != 2)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices = new List<TileIndex> { m_FacingDirectionStartIndex, m_FacingDirectionEndIndex };
		}
		m_FacingDirectionStartIndex = SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices[0];
		m_FacingDirectionEndIndex = SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices[1];
		FixedRotationStartingTileText.SetText((m_FacingDirectionStartIndex == null) ? "<b>NEED SELECTION</b>" : $"Tile: {m_FacingDirectionStartIndex.X},{m_FacingDirectionStartIndex.Y}");
		FixedRotationEndingTileText.SetText((m_FacingDirectionEndIndex == null) ? "<b>NEED SELECTION</b>" : $"Tile: {m_FacingDirectionEndIndex.X},{m_FacingDirectionEndIndex.Y}");
		RotationStatusButton.interactable = m_FacingDirectionEndIndex != null && m_FacingDirectionStartIndex != null;
		RotationStatusText.SetText((m_FacingDirectionEndIndex != null && m_FacingDirectionStartIndex != null) ? "Show rotation tiles" : "<i>Rotation tiles not configured</i>");
	}

	public void OnSetFixedFacingDirectionButtonPressed(bool startingIndex)
	{
		if (startingIndex)
		{
			LevelEditorController.SelectTile(OnFixedRotationStartingTileSelected);
		}
		else
		{
			LevelEditorController.SelectTile(OnFixedRotationEndingTileSelected);
		}
	}

	private void OnFixedRotationStartingTileSelected(CClientTile tileSelected)
	{
		TileIndex facingDirectionStartIndex = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		m_FacingDirectionStartIndex = facingDirectionStartIndex;
		SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices = new List<TileIndex> { m_FacingDirectionStartIndex, m_FacingDirectionEndIndex };
		UpdateFixedStartingRotationPanelForLevel();
	}

	private void OnFixedRotationEndingTileSelected(CClientTile tileSelected)
	{
		TileIndex facingDirectionEndIndex = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		m_FacingDirectionEndIndex = facingDirectionEndIndex;
		SaveData.Instance.Global.CurrentEditorLevelData.FixedFacingDirectionIndices = new List<TileIndex> { m_FacingDirectionStartIndex, m_FacingDirectionEndIndex };
		UpdateFixedStartingRotationPanelForLevel();
	}

	public void OnShowRotationTilesPressed()
	{
		if (m_ShowRotationTilesRoutine != null)
		{
			StopCoroutine(m_ShowRotationTilesRoutine);
			m_ShowRotationTilesRoutine = null;
		}
		m_ShowRotationTilesRoutine = ShowRotationTilesInOrder();
		StartCoroutine(m_ShowRotationTilesRoutine);
	}

	private IEnumerator ShowRotationTilesInOrder()
	{
		if (m_FacingDirectionStartIndex != null)
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[m_FacingDirectionStartIndex.X, m_FacingDirectionStartIndex.Y];
			LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
			yield return Timekeeper.instance.WaitForSeconds(1f);
		}
		if (m_FacingDirectionEndIndex != null)
		{
			CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[m_FacingDirectionEndIndex.X, m_FacingDirectionEndIndex.Y];
			LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile2.m_GameObject.transform.position);
		}
	}

	public void OnFixPlayerStatesPressed()
	{
		for (int num = ScenarioManager.Scenario.PlayerActors.Count - 1; num >= 0; num--)
		{
			LevelEditorController.RemovePlayer(ScenarioManager.Scenario.PlayerActors[num]);
		}
		ScenarioManager.Scenario.ExhaustedPlayers.Clear();
		ScenarioManager.CurrentScenarioState.Players.Clear();
		ScenarioManager.CurrentScenarioState.RoundNumber = 1;
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			actorState.ResetCauseOfDeath();
		}
		LevelEditorController.s_Instance.m_LevelEditorUIInstance.LevelDataPanel.OnSaveDataPressed();
	}

	private void UpdateStartingCameraSettings()
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState.StartingCameraProfile == null)
		{
			currentScenarioState.StartingCameraProfile = new CLevelCameraProfile();
		}
		CameraProfilePanel.InitForProfile(currentScenarioState.StartingCameraProfile);
	}
}
