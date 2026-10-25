using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FH.YML;
using GLOOM;
using GLOOM.MainMenu;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;

public class CustomPartySetup : MonoBehaviour
{
	public class Configuration
	{
		public bool UnlockAllItems;

		public bool UnlockAllCharacters;

		public bool MaximizeCharacters;

		public int StartingCharactersCount = -1;

		public int StartingGold;
	}

	public static CustomPartySetup Instance;

	[SerializeField]
	private NewPartyDisplayUI NewPartyDisplayUI;

	[SerializeField]
	private GameObject LevelEditorMenu;

	[SerializeField]
	private CanvasGroup warningPanel;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private GameObject startButton;

	[SerializeField]
	private GameObject backButton;

	[NonSerialized]
	private CCustomLevelData m_CurrentCustomLevel;

	[NonSerialized]
	private PartyAdventureData m_CurrentCustomParty;

	public const string SandBoxScenarioID = "SandboxScenario";

	private bool showingForSandbox;

	private bool showingForCreateNew;

	private bool showingForCustomCreated;

	private ScenarioDefinition scenarioDefinition;

	public PartyAdventureData CurrentCustomParty => m_CurrentCustomParty;

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Start()
	{
		NewPartyDisplayUI.OnOpenedPanel += OnOpenedPanel;
	}

	private void OnOpenedPanel(bool selected, NewPartyDisplayUI.DisplayType display)
	{
		if (selected)
		{
			startButton.SetActive(value: false);
			backButton.SetActive(value: false);
		}
		else
		{
			startButton.SetActive(value: true);
			backButton.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (m_CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.Count() < 2)
		{
			warningText.text = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROES"), 2);
			warningPanel.alpha = 1f;
		}
		else
		{
			warningPanel.alpha = 0f;
		}
	}

	public void ShowForSandbox()
	{
		showingForSandbox = true;
		PartyAdventureData partyData = CreateAllUnlockedParty("SandboxScenario", EAdventureDifficulty.Normal, EGameMode.SingleScenario, ERulesFlag.None);
		ShowForYML("SandboxScenario", partyData);
		SaveData.Instance.Global.SetResumeSingleScenario(null);
	}

	public void ShowForYML(string scenarioID, PartyAdventureData partyData)
	{
		scenarioDefinition = ScenarioRuleClient.SRLYML.Scenarios.SingleOrDefault((ScenarioDefinition s) => s.ID == scenarioID);
		if (scenarioDefinition != null)
		{
			AdventureState.UpdateMapState(partyData.AdventureMapState);
			m_CurrentCustomParty = partyData;
			gameObject.SetActive(value: true);
			NewPartyDisplayUI.Init(partyData.AdventureMapState.MapParty);
		}
		else
		{
			Log.GUIError("Unable to find Scenario with ID " + scenarioID);
		}
	}

	public void ShowForCustomLevel(CCustomLevelData customLevel)
	{
		if (customLevel.ScenarioState == null)
		{
			scenarioDefinition = ScenarioRuleClient.SRLYML.Scenarios.SingleOrDefault((ScenarioDefinition s) => s.FileName == customLevel.YMLFile);
		}
		showingForCreateNew = true;
		m_CurrentCustomLevel = customLevel;
		m_CurrentCustomParty = CreateEmptyParty();
		m_CurrentCustomParty.ResolveFHGameState(YMLDefRepository.Repo, recreateMapParty: true);
		AdventureState.UpdateMapState(m_CurrentCustomParty.AdventureMapState);
		gameObject.SetActive(value: true);
		NewPartyDisplayUI.Init(m_CurrentCustomParty.AdventureMapState.MapParty);
	}

	public async Task ShowForCustomLevel(CCustomLevelData customLevel, PartyAdventureData partyData)
	{
		if (customLevel.ScenarioState == null)
		{
			scenarioDefinition = ScenarioRuleClient.SRLYML.Scenarios.SingleOrDefault((ScenarioDefinition s) => s.FileName == customLevel.YMLFile);
		}
		showingForCustomCreated = true;
		m_CurrentCustomLevel = await customLevel.DeepCopySerializableObject<CCustomLevelData>();
		m_CurrentCustomParty = partyData;
		await partyData.Load(partyData.GameMode, isJoiningMPClient: false);
		partyData.ResolveFHGameState(YMLDefRepository.Repo, recreateMapParty: true);
		AdventureState.UpdateMapState(partyData.AdventureMapState);
		partyData.AdventureMapState.MapParty.OnMapStateAdventureStarted();
		gameObject.SetActive(value: true);
		NewPartyDisplayUI.Init(partyData.AdventureMapState.MapParty);
	}

	private PartyAdventureData CreateEmptyParty()
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), "Custom Party", EAdventureDifficulty.Normal, new SaveOwner(), EGameMode.SingleScenario, ERulesFlag.None);
		AdventureState.UpdateMapState(partyAdventureData.AdventureMapState);
		partyAdventureData.AdventureMapState.MapParty.ModifyPartyGold(1000, useGoldModifier: false);
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			if (GlobalData.CompletedCharacters.Contains(@class.CharacterID))
			{
				partyAdventureData.AdventureMapState.MapParty.UnlockedCharacterIDs.Add(@class.CharacterID);
				partyAdventureData.AdventureMapState.MapParty.AddCharacterToCharactersList(new CMapCharacter(@class.CharacterID, string.Empty, 0));
			}
		}
		foreach (CMapCharacter character in partyAdventureData.AdventureMapState.MapParty.CheckCharacters)
		{
			character.MaxLevel();
			character.OwnedAbilityCardIDs.AddRange(from s in CharacterClassManager.AllAbilityCards
				where s.ClassID == character.CharacterID && !character.OwnedAbilityCardIDs.Contains(s.ID)
				select s.ID);
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			partyAdventureData.AdventureMapState.MapParty.AddItemToUnboundItems(itemCard.GetItem);
		}
		return partyAdventureData;
	}

	public static void AddEachCharacter(PartyAdventureData partyData, int count = 999)
	{
		int num = 0;
		List<string> list = new List<string> { "CrashingTideID", "FrozenFistID", "DrifterID", "DeathwalkerID", "BannerSpearID", "BoneshaperID", "BlinkbladeID", "GeminateID" };
		foreach (CMapCharacter checkCharacter in partyData.AdventureMapState.MapParty.CheckCharacters)
		{
			if (!list.Contains(checkCharacter.CharacterID))
			{
				list.Add(checkCharacter.CharacterID);
			}
		}
		foreach (string characterId in list)
		{
			if (num >= count)
			{
				break;
			}
			CMapCharacter cMapCharacter = partyData.AdventureMapState.MapParty.CheckCharacters.FirstOrDefault((CMapCharacter f) => f.CharacterID == characterId);
			if (cMapCharacter != null)
			{
				AdventureState.MapState.MapParty.AddSelectedCharacterToNextOpenSlot(cMapCharacter);
				num++;
			}
		}
	}

	public static void InitCustomParty(PartyAdventureData partyData, Configuration settings)
	{
		AdventureState.UpdateMapState(partyData.AdventureMapState);
		if (settings.UnlockAllCharacters)
		{
			foreach (CCharacterClass @class in CharacterClassManager.Classes)
			{
				string characterName = @class.CharacterID.ReplaceLastOccurrence("ID", string.Empty);
				partyData.AdventureMapState.MapParty.UnlockedCharacterIDs.Add(@class.CharacterID);
				partyData.AdventureMapState.MapParty.AddCharacterToCharactersList(new CMapCharacter(@class.CharacterID, characterName, 0));
			}
		}
		if (settings.MaximizeCharacters)
		{
			foreach (CMapCharacter character in partyData.AdventureMapState.MapParty.CheckCharacters)
			{
				character.MaxLevel();
				character.OwnedAbilityCardIDs.AddRange(from s in CharacterClassManager.AllAbilityCards
					where s.ClassID == character.CharacterID && !character.OwnedAbilityCardIDs.Contains(s.ID)
					select s.ID);
			}
		}
		if (settings.UnlockAllItems)
		{
			foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
			{
				CItem cItem = itemCard.GetItem.Copy();
				cItem.NetworkID = 0u;
				partyData.AdventureMapState.MapParty.AddItemToUnboundItems(cItem);
			}
		}
		int count = ((settings.StartingCharactersCount < 0) ? 999 : settings.StartingCharactersCount);
		AddEachCharacter(partyData, count);
	}

	public static void LevelUp(CMapCharacter character)
	{
		int nextXPThreshold = character.GetNextXPThreshold();
		nextXPThreshold -= character.EXP;
		character.GainEXP(nextXPThreshold, 1f);
	}

	public static PartyAdventureData CreateAllUnlockedParty(string partyName, EAdventureDifficulty difficulty, EGameMode gameMode, ERulesFlag rules)
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), partyName, difficulty, new SaveOwner(), gameMode, rules);
		AdventureState.UpdateMapState(partyAdventureData.AdventureMapState);
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			if (@class.CharacterYML.OnlyValidInLevels == null || !partyName.ToLower().Contains("sandbox"))
			{
				string characterName = @class.CharacterID.ReplaceLastOccurrence("ID", string.Empty);
				partyAdventureData.AdventureMapState.MapParty.UnlockedCharacterIDs.Add(@class.CharacterID);
				partyAdventureData.AdventureMapState.MapParty.AddCharacterToCharactersList(new CMapCharacter(@class.CharacterID, characterName, 0));
			}
		}
		foreach (CMapCharacter character in partyAdventureData.AdventureMapState.MapParty.CheckCharacters)
		{
			character.MaxLevel();
			character.OwnedAbilityCardIDs.AddRange(from s in CharacterClassManager.AllAbilityCards
				where s.ClassID == character.CharacterID && !character.OwnedAbilityCardIDs.Contains(s.ID)
				select s.ID);
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			CItem cItem = itemCard.GetItem.Copy();
			cItem.NetworkID = 0u;
			partyAdventureData.AdventureMapState.MapParty.AddItemToUnboundItems(cItem);
		}
		return partyAdventureData;
	}

	public async void OnStartLevelPressed()
	{
		if (m_CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.Count() < 2)
		{
			return;
		}
		if (showingForCustomCreated)
		{
			LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty);
		}
		if (scenarioDefinition != null)
		{
			List<Tuple<ScenarioMessage, string>> roomRevealedMessageToMapGuid;
			if (m_CurrentCustomLevel == null)
			{
				m_CurrentCustomLevel = new CCustomLevelData(scenarioDefinition.ID, CMapScenarioState.CreateNewScenario(scenarioDefinition, SharedClient.GlobalRNG.Next(), m_CurrentCustomParty.AdventureMapState.MapParty.PartyLevel, scenarioDefinition.FileName, scenarioDefinition.ID, (scenarioDefinition.Description != string.Empty) ? LocalizationManager.GetTranslation(scenarioDefinition.Description) : string.Empty, 4, new SharedLibrary.Random(), out var debugOutput, out roomRevealedMessageToMapGuid, m_CurrentCustomParty.AdventureMapState.MapParty.ThreatLevel));
				Log.GUIInfo("Generating Custom YML scenario:\n" + debugOutput);
				scenarioDefinition = null;
			}
			else if (m_CurrentCustomLevel.ScenarioState == null)
			{
				m_CurrentCustomLevel.ScenarioState = CMapScenarioState.CreateNewScenario(scenarioDefinition, SharedClient.GlobalRNG.Next(), m_CurrentCustomParty.AdventureMapState.MapParty.PartyLevel, scenarioDefinition.FileName, LocalizationManager.GetTranslation(scenarioDefinition.ID), (scenarioDefinition.Description != string.Empty) ? LocalizationManager.GetTranslation(scenarioDefinition.Description) : string.Empty, 4, new SharedLibrary.Random(), out var _, out roomRevealedMessageToMapGuid, m_CurrentCustomParty.AdventureMapState.MapParty.ThreatLevel);
			}
		}
		await SaveData.Instance.StartNewCustomLevel(m_CurrentCustomLevel, m_CurrentCustomParty);
		SceneController.Instance.LoadSingleScenario(m_CurrentCustomParty);
	}

	public void OnBackButtonPressed()
	{
		if (showingForSandbox)
		{
			showingForSandbox = false;
			NewPartyDisplayUI.Hide(NewPartyDisplayUI, instant: false, () =>
			{
				gameObject.SetActive(value: false);
				SceneController.Instance.YML.ReturnToMainMenu();
				MainMenuUIManager.Instance.mainMenu.Show();
			});
		}
		else if (showingForCreateNew)
		{
			showingForCreateNew = false;
			NewPartyDisplayUI.Hide(NewPartyDisplayUI, instant: false, () =>
			{
				gameObject.SetActive(value: false);
				LevelEditorMenu.SetActive(value: true);
			});
		}
		else
		{
			if (showingForCustomCreated)
			{
				showingForCustomCreated = false;
			}
			NewPartyDisplayUI.Hide(NewPartyDisplayUI, instant: false, () =>
			{
				gameObject.SetActive(value: false);
			});
		}
	}
}
