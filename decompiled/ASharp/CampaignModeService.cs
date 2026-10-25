using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLOOM.MainMenu;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using UnityEngine;

public class CampaignModeService : MonoBehaviour, IGameModeService, ICreateGameService, ILoadGameService
{
	private CampaignLoadService loadService = new CampaignLoadService();

	private BasicEventHandler onDeleted;

	public void CreateGame(IGameData data)
	{
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		SceneController.Instance.NewCampaignStart(data.GameName, data.Difficulty, data.Rules, data.GetParam<string>("GameModeID"));
	}

	public async Task<bool> ValidateExistsGame(string partyName)
	{
		return (await SaveData.Instance.Global.LoadSavesFromDisk()).Any((PartyAdventureData c) => c.PartyName == partyName);
	}

	public bool CanLoadGames()
	{
		return SaveData.Instance?.CanLoad() ?? false;
	}

	public async Task<bool> CanResume()
	{
		PartyAdventureData partyAdventureData = await SaveData.Instance.Global.GetResumeCampaign();
		return partyAdventureData != null && !HasInvalidDLCs(partyAdventureData);
	}

	public bool HasInvalidDLCs(PartyAdventureData partyAdventureData)
	{
		return false;
	}

	public async void ResumeLastGame()
	{
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		PartyAdventureData resume = await SaveData.Instance.Global.GetResumeCampaign();
		await SaveData.Instance.Global.DeserializeGameStateAndValidateOnLoad(EGameMode.Campaign, resume);
		SaveData.Instance.LoadCampaignMode(resume, isJoiningMPClient: false);
	}

	public void RegisterToDeletedGame(BasicEventHandler action)
	{
		onDeleted = (BasicEventHandler)Delegate.Combine(onDeleted, action);
	}

	public async Task<List<IGameSaveData>> GetSaves()
	{
		return (await loadService.GetSaves()).ToList();
	}

	public void DeleteGame(IGameSaveData data)
	{
		loadService.DeleteGame(data);
		onDeleted?.Invoke();
	}

	public Task LoadGame(IGameSaveData data)
	{
		return loadService.LoadGame(data);
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		loadService.EnableDLC(data, dlcToAdd);
	}

	public static async Task NewCustomCampaign(CustomPartySetup.Configuration settings)
	{
		SceneController.Instance.YML.LoadCampaign();
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), Guid.NewGuid().ToString(), EAdventureDifficulty.Normal, new SaveOwner(), EGameMode.Campaign, ERulesFlag.None);
		CustomPartySetup.InitCustomParty(partyAdventureData, settings);
		await partyAdventureData.AutoSave();
	}

	public async Task CreateCustomGame(CustomPartySetup.Configuration settings)
	{
		await NewCustomCampaign(settings);
	}

	public async Task DeleteCampaign(string campaignName)
	{
		try
		{
			MainMenuUIManager.Instance?.RequestDisableInteraction(disable: true, this);
			await SaveData.Instance.DeleteCampaign(campaignName);
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "Unable to delete game");
		}
		MainMenuUIManager.Instance?.RequestDisableInteraction(disable: false, this);
		onDeleted?.Invoke();
	}
}
