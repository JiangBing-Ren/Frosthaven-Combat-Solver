using MapRuleLibrary.Adventure;

namespace GLOOM.MainMenu;

public interface IGameData
{
	string GameName { get; }

	EAdventureDifficulty Difficulty { get; }

	ERulesFlag Rules { get; }

	T GetParam<T>(string key);
}
