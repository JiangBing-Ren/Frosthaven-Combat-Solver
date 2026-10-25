using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine.SceneManagement;

namespace FH.Platforms.Steam;

internal class SteamRichPresence
{
	private (string key, string value, string mission, string hero, int turn) _currentStatus;

	private (string key, string value, string mission, string hero, int turn) _previousStatus;

	public void SetRichPresenceValue(string key, string value)
	{
		SteamFriends.SetRichPresence(key, value);
		Log.Info("Rich presence set to: " + key + " = " + value);
	}

	public IEnumerator Tick()
	{
		List<string> list = (from a in RuntimeTools.GetCurrentActiveScenes()
			select a.name).ToList();
		if (list.Contains("MainMenu"))
		{
			_currentStatus = (key: "steam_display", value: "#MainMenu", mission: null, hero: null, turn: 0);
		}
		else if (list.Contains("FHCampaignMap"))
		{
			_currentStatus = (key: "steam_display", value: "#InOutpost", mission: null, hero: null, turn: 0);
		}
		else if (list.Contains("ProcGen"))
		{
			_currentStatus = (key: "steam_display", value: "#InMission", mission: null, hero: null, turn: 0);
		}
		(string, string, string, string, int) currentStatus = _currentStatus;
		(string, string, string, string, int) previousStatus = _previousStatus;
		if (currentStatus.Item1 != previousStatus.Item1 || currentStatus.Item2 != previousStatus.Item2 || currentStatus.Item3 != previousStatus.Item3 || currentStatus.Item4 != previousStatus.Item4 || currentStatus.Item5 != previousStatus.Item5)
		{
			SetRichPresenceValue(_currentStatus.key, _currentStatus.value);
			_previousStatus = _currentStatus;
		}
		yield break;
	}

	public void Dispose()
	{
	}
}
