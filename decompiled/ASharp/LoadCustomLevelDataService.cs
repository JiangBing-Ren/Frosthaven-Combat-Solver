using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FH.GraphicalUserInterface.MainMenu;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;

internal class LoadCustomLevelDataService : ILoadGameService
{
	private CCustomLevelData m_LevelData;

	private ILoadGameService m_LoadGameService;

	public LoadCustomLevelDataService(CCustomLevelData levelData)
	{
		m_LevelData = levelData;
		if (LevelEditorController.s_Instance.LastLoadedRuleset == ScenarioManager.EDLLMode.Campaign)
		{
			m_LoadGameService = new CampaignLoadService();
			return;
		}
		throw new NotImplementedException("LoadCustomLevelDataService not available for this ruleset");
	}

	public async Task<List<IGameSaveData>> GetSaves()
	{
		return await m_LoadGameService.GetSaves();
	}

	public void DeleteGame(IGameSaveData data)
	{
		m_LoadGameService.DeleteGame(data);
	}

	public async Task LoadGame(IGameSaveData data)
	{
		MainMenuUIAutomation.SaveFile = data.GameName;
		await MainMenuUIManager.Instance.CustomPartySetupScreen.ShowForCustomLevel(m_LevelData, ((PartyGameSaveData)data).PartyAdventureData);
	}

	public async Task CreateCustomGame(CustomPartySetup.Configuration settings)
	{
		await m_LoadGameService.CreateCustomGame(settings);
	}

	public async Task DeleteCampaign(string campaignName)
	{
		await SaveData.Instance.DeleteCampaign(campaignName);
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		m_LoadGameService.EnableDLC(data, dlcToAdd);
	}
}
