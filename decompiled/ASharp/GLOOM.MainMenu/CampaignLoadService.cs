using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public class CampaignLoadService : ILoadGameService
{
	private class CampaignSave : PartyGameSaveData
	{
		public override EGoldMode GoldMode => EGoldMode.CharacterGold;

		public override int? PartyGold => null;

		public override int? Wealth => PartyAdventureData.LastSavedProsperity;

		public override int? Reputation => 0;

		public override List<IGameSaveCharacterData> SelectedCharacters => ((IEnumerable<GameSaveCharacterData>)PartyAdventureData.LastSavedSelectedCharacterInfo.ConvertAll((PartyAdventureData.CharacterInfo it) =>
		{
			if (it == null)
			{
				return (GameSaveCharacterData)null;
			}
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == it.ID);
			return (cCharacterClass == null) ? new GameSaveCharacterData((ECharacter)Enum.Parse(typeof(ECharacter), it.ID.ReplaceLastOccurrence("ID", string.Empty)), null, it.Level, it.Gold) : new GameSaveCharacterData(cCharacterClass.CharacterModel, null, it.Level, it.Gold);
		})).Select((Func<GameSaveCharacterData, IGameSaveCharacterData>)((GameSaveCharacterData s) => s)).ToList();

		public CampaignSave(PartyAdventureData data)
			: base(data)
		{
		}
	}

	public PartyAdventureData GetAdventureData(IGameSaveData data)
	{
		return ((CampaignSave)data).PartyAdventureData;
	}

	public void DeleteGame(IGameSaveData data)
	{
		SaveData.Instance.DeletePartyData(GetAdventureData(data), EGameMode.Campaign);
	}

	public async Task<List<IGameSaveData>> GetSaves()
	{
		return ((IEnumerable<PartyAdventureData>)(await SaveData.Instance.Global.LoadSavesFromDisk()).OrderByDescending((PartyAdventureData it) => it.LastSavedTimeStamp)).Select((Func<PartyAdventureData, IGameSaveData>)((PartyAdventureData it) => new CampaignSave(it))).ToList();
	}

	public async Task LoadGame(IGameSaveData gameData)
	{
		PartyAdventureData data = GetAdventureData(gameData);
		await SaveData.Instance.Global.DeserializeGameStateAndValidateOnLoad(EGameMode.Campaign, data);
		try
		{
			MainMenuUIManager.Instance?.RequestDisableInteraction(disable: true, this);
			SaveData.Instance.LoadCampaignMode(data, isJoiningMPClient: false);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "Unable to resume Campaign game");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, MoveGameStateFileToCorruptedSaveFolder, ex.Message);
		}
		static async void MoveGameStateFileToCorruptedSaveFolder()
		{
			await SaveData.Instance.Global.CurrentAdventureData.MoveGameStateFileToCorruptedSaveFolder(EGameMode.Campaign);
			SceneController.Instance.LoadMainMenu();
		}
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		SaveData.Instance.CopySaveToBackupBeforeEnablingDLCOnIt(GetAdventureData(data), dlcToAdd);
	}

	public async Task CreateCustomGame(CustomPartySetup.Configuration settings)
	{
		await CampaignModeService.NewCustomCampaign(settings);
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
	}
}
