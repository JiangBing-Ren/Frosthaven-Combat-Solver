using System.Collections.Generic;
using MapRuleLibrary.Adventure;

namespace GLOOM.MainMenu;

public class GameData : IGameData
{
	private readonly Dictionary<string, object> extraData;

	public const string CampaignIDKey = "GameModeID";

	public string GameName { get; set; }

	public EAdventureDifficulty Difficulty { get; set; }

	public ERulesFlag Rules { get; set; }

	public GameData()
	{
		extraData = new Dictionary<string, object>();
		AddParam("GameModeID", "Frosthaven");
	}

	public T GetParam<T>(string key)
	{
		if (HasParam(key))
		{
			return (T)extraData[key];
		}
		return default;
	}

	public bool HasParam(string key)
	{
		return extraData.ContainsKey(key);
	}

	public void AddParam(string key, object value)
	{
		extraData[key] = value;
	}
}
