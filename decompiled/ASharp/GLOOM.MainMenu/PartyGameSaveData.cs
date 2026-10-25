using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FHRL;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public abstract class PartyGameSaveData : IGameSaveData
{
	public PartyAdventureData PartyAdventureData { get; }

	public string GameName => (PartyAdventureData.IsOldSaveFileFormat ? "[RECOVERED] " : string.Empty) + PartyAdventureData.PartyName + " - " + PartyAdventureData.SaveFileName;

	public string CampaignName => PartyAdventureData.PartyName;

	public string SaveName => PartyAdventureData.SaveFileName;

	public SaveOwner Owner => PartyAdventureData.Owner;

	public DateTime LastSavedTimeStamp => PartyAdventureData.LastSavedTimeStamp;

	public string CurrentQuest => string.Empty;

	public abstract EGoldMode GoldMode { get; }

	public abstract int? PartyGold { get; }

	public abstract int? Wealth { get; }

	public abstract int? Reputation { get; }

	public Task<string> ScreenShotFilePath => PartyAdventureData.ScreenShotFile;

	public int Week => PartyAdventureData.LastSavedWeek;

	public int CurrentScenarioRound => PartyAdventureData.CurrentScenarioRound;

	public ScenarioID? ScenarioID => PartyAdventureData.CurrentScenarioID;

	public abstract List<IGameSaveCharacterData> SelectedCharacters { get; }

	public EGameMode GameMode => PartyAdventureData.GameMode;

	protected PartyGameSaveData(PartyAdventureData data)
	{
		PartyAdventureData = data;
	}

	public List<DLCRegistry.EDLCKey> GetDLCAvailables()
	{
		return new List<DLCRegistry.EDLCKey>();
	}

	public bool IsDLCActive(DLCRegistry.EDLCKey dlcKey)
	{
		return false;
	}

	public bool HasInvalidDLCs(out List<DLCRegistry.EDLCKey> dlcInvalid)
	{
		dlcInvalid = new List<DLCRegistry.EDLCKey>();
		return false;
	}
}
