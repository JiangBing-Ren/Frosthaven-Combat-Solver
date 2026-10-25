using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHRL;

namespace FHGUI.Services;

public class UIElementTagsViewService : FHMapViewService
{
	private const char TAG_VALUE_SEPARATOR = '=';

	public UIElementTagsViewService(FHMapView view)
		: base(view)
	{
	}

	public string FormatTag(string key, string value)
	{
		return $"{key}{'='}{value}";
	}

	public static string GetTagValue(string tagKeyValuePair)
	{
		string[] array = tagKeyValuePair.Split('=');
		if (array.Length <= 1)
		{
			return string.Empty;
		}
		return array[1];
	}

	public static string GetTagKey(string tagKeyValuePair)
	{
		string[] array = tagKeyValuePair.Split('=');
		if (array.Length <= 1)
		{
			return string.Empty;
		}
		return array[0];
	}

	public void AddPlayerTag(string key, string value, int maxCountForKey = 0, PlayerID? playerID = null)
	{
		string text = FormatTag(key, value);
		PlayerID playerID2 = playerID ?? _view.PlayerID;
		if (!_view.MultiplayerGameController.PlayerHasUITag(playerID2, text) && (maxCountForKey < 1 || GetPlayerTagsByKey(key).Count() < maxCountForKey))
		{
			Choreographer.ExecuteCommand("AddPlayerUITag", playerID2, text);
		}
	}

	public void RemovePlayerTag(string key, string value, PlayerID? playerID = null)
	{
		string text = FormatTag(key, value);
		PlayerID playerID2 = playerID ?? _view.PlayerID;
		if (!_view.MultiplayerGameController.PlayerHasUITag(playerID2, text))
		{
			Log.GUIWarning("Trying to remove UI element tag " + text + ", but the player does not have such a tag!");
			return;
		}
		Choreographer.ExecuteCommand("RemovePlayerUITag", playerID2, text);
	}

	public void ReplacePlayerTagsByKey(string key, string value, PlayerID? playerID = null)
	{
		PlayerID value2 = playerID ?? _view.PlayerID;
		if (HasPlayerTag(key, value2))
		{
			ClearPlayerTagsByKey(key, value2);
		}
		AddPlayerTag(key, value, 0, value2);
	}

	public IEnumerable<string> GetPlayerTagsByKey(string key, PlayerID? playerID = null)
	{
		PlayerID pid = playerID ?? _view.PlayerID;
		MultiplayerGameState.Player player = _view.MultiplayerGameState.Players.First((MultiplayerGameState.Player p) => p.ID == pid);
		foreach (string item in player.UITags.Where((string uiTag) => uiTag.StartsWith(key)))
		{
			yield return item;
		}
	}

	public void ClearPlayerTagGroup(string prefix)
	{
		foreach (MultiplayerGameState.Player player in _view.MultiplayerGameState.Players)
		{
			string[] array = player.UITags.Where((string s) => s.Contains(prefix)).ToArray();
			foreach (string text in array)
			{
				Choreographer.ExecuteCommand("RemovePlayerUITag", player.ID, text);
			}
		}
	}

	public void ClearPlayerTagsByKey(string key, PlayerID? playerID = null)
	{
		foreach (string item in GetPlayerTagsByKey(key, playerID))
		{
			Choreographer.ExecuteCommand("RemovePlayerUITag", playerID, item);
		}
	}

	public IEnumerable<UIMultiplayerUser.Data> GetMultiplayerUsersWithTag(string tag)
	{
		foreach (MultiplayerGameState.Player player2 in _view.MultiplayerGameState.Players)
		{
			if (MultiplayerGameController.PlayerHasUITag(player2.ID, tag))
			{
				NetworkPlayer player = PlayerRegistry.GetPlayer(player2.ID);
				yield return new UIMultiplayerUser.Data
				{
					Id = player2.ID.ID,
					Username = (player?.Username ?? string.Empty),
					Avatar = player?.Avatar,
					IsLeader = MultiplayerGameController.IsPartyLeader(player2.ID)
				};
			}
		}
	}

	public void AddGameTag(string key, string value, int maxCountForKey = 0)
	{
		string text = FormatTag(key, value);
		if (!_view.MultiplayerGameController.HasUITag(text) && (maxCountForKey < 1 || GetGameTagsByKey(key).Count() < maxCountForKey))
		{
			Choreographer.ExecuteCommand("AddUITag", text);
		}
	}

	public void RemoveGameTag(string key, string value)
	{
		string text = FormatTag(key, value);
		if (!_view.MultiplayerGameController.HasUITag(text))
		{
			Log.GUIWarning("Trying to remove UI element tag " + text + ", but the tag does not exist!");
			return;
		}
		Choreographer.ExecuteCommand("RemoveUITag", text);
	}

	public IEnumerable<string> GetGameTagsByKey(string key, string value = null)
	{
		string tag = (value.IsNullOrEmpty() ? key : string.Format(key, value));
		if (!value.IsNullOrEmpty())
		{
			return _view.MultiplayerGameState.UITags.Where((string t) => t.StartsWith(tag));
		}
		return _view.MultiplayerGameState.UITags.Where((string t) => t == tag);
	}

	public IEnumerable<string> GetGameTagsValuesByKey(string key)
	{
		return _view.MultiplayerGameState.UITags.Where((string t) => t.StartsWith(key)).Select(GetTagValue);
	}

	public void ReplaceGameTagByKey(string key, string value)
	{
		if (HasGameTag(key))
		{
			ClearGameTagsByKey(key);
		}
		AddGameTag(key, value);
	}

	public bool HasPlayerTag(string key, PlayerID? playerID = null)
	{
		return _view.MultiplayerGameController.PlayerHasUITag(playerID ?? _view.PlayerID, key);
	}

	public bool HasGameTag(string key)
	{
		return _view.MultiplayerGameController.HasUITag(key);
	}

	public void ClearGameTagsByKey(string key)
	{
		foreach (string item in _view.MultiplayerGameState.UITags.Where((string tag) => tag.StartsWith(key)))
		{
			Choreographer.ExecuteCommand("RemoveUITag", item);
		}
	}
}
