using System.Threading.Tasks;

namespace GLOOM.MainMenu;

public interface ICreateGameService
{
	void CreateGame(IGameData data);

	Task<bool> ValidateExistsGame(string partyName);
}
