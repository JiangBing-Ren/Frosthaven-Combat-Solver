using System.Threading.Tasks;

namespace GLOOM.MainMenu;

public interface IGameModeService : ICreateGameService, ILoadGameService
{
	Task<bool> CanResume();

	bool CanLoadGames();

	bool HasInvalidDLCs(PartyAdventureData partyAdventureData);

	void ResumeLastGame();

	void RegisterToDeletedGame(BasicEventHandler action);
}
