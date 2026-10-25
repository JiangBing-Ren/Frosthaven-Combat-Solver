using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FHRL;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public interface IGameSaveData
{
	string GameName { get; }

	string CampaignName { get; }

	string SaveName { get; }

	SaveOwner Owner { get; }

	DateTime LastSavedTimeStamp { get; }

	EGoldMode GoldMode { get; }

	int? PartyGold { get; }

	int? Wealth { get; }

	int? Reputation { get; }

	string CurrentQuest { get; }

	int Week { get; }

	int CurrentScenarioRound { get; }

	ScenarioID? ScenarioID { get; }

	Task<string> ScreenShotFilePath { get; }

	List<IGameSaveCharacterData> SelectedCharacters { get; }

	PartyAdventureData PartyAdventureData { get; }

	EGameMode GameMode { get; }

	List<DLCRegistry.EDLCKey> GetDLCAvailables();

	bool IsDLCActive(DLCRegistry.EDLCKey dlcKey);

	bool HasInvalidDLCs(out List<DLCRegistry.EDLCKey> invalidDLCs);
}
