using System.Collections.Generic;
using System.Threading.Tasks;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public interface ILoadGameService
{
	Task<List<IGameSaveData>> GetSaves();

	void DeleteGame(IGameSaveData data);

	Task LoadGame(IGameSaveData data);

	void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd);

	Task CreateCustomGame(CustomPartySetup.Configuration settings);

	Task DeleteCampaign(string campaignName);
}
