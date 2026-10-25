using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIReadyTrackerBar : Singleton<UIReadyTrackerBar>
{
	private enum EReadyMode
	{
		Character,
		Player
	}

	[SerializeField]
	private List<UIReadyTracker> trackers;

	private Dictionary<string, UIReadyTracker> assignedTrackers = new Dictionary<string, UIReadyTracker>();

	private EReadyMode mode;

	private Func<string, bool> isAlive;

	public void ShowCharactersTrackers(List<string> characterIDs, Func<string, bool> isAlive = null)
	{
		this.isAlive = isAlive;
		mode = EReadyMode.Character;
		foreach (KeyValuePair<string, UIReadyTracker> item in assignedTrackers.Where((KeyValuePair<string, UIReadyTracker> it) => !characterIDs.Contains(it.Key)).ToList())
		{
			RemoveAssignedTracker(item.Key);
		}
		for (int num = 0; num < characterIDs.Count; num++)
		{
			string text = characterIDs[num];
			if (isAlive == null || isAlive(text))
			{
				if (!assignedTrackers.ContainsKey(text))
				{
					AddTracker(text);
				}
				else
				{
					assignedTrackers[text].ShowReady(isReady: false);
				}
			}
			else
			{
				RemoveAssignedTracker(text);
			}
		}
		for (int num2 = 0; num2 < trackers.Count; num2++)
		{
			trackers[num2].gameObject.SetActive(value: false);
		}
		gameObject.SetActive(value: true);
	}

	public void ShowPlayersTrackers(List<string> players, Func<string, bool> isAlive = null)
	{
		this.isAlive = isAlive;
		mode = EReadyMode.Player;
		foreach (KeyValuePair<string, UIReadyTracker> item in assignedTrackers.Where((KeyValuePair<string, UIReadyTracker> it) => !players.Contains(it.Key)).ToList())
		{
			RemoveAssignedTracker(item.Key);
		}
		for (int num = 0; num < players.Count; num++)
		{
			string text = players[num];
			if (!assignedTrackers.ContainsKey(text))
			{
				AddTracker(text);
			}
			else
			{
				assignedTrackers[text].ShowReady(isReady: false);
			}
		}
		for (int num2 = 0; num2 < trackers.Count; num2++)
		{
			trackers[num2].gameObject.SetActive(value: false);
		}
		gameObject.SetActive(value: true);
	}

	public void RemoveTracker(string character)
	{
		if (gameObject.activeSelf)
		{
			RemoveAssignedTracker(character);
		}
	}

	private void RemoveAssignedTracker(string character)
	{
		if (assignedTrackers.ContainsKey(character))
		{
			assignedTrackers[character].gameObject.SetActive(value: false);
			trackers.Add(assignedTrackers[character]);
			assignedTrackers.Remove(character);
			Log.GUIInfo(string.Format("Removed " + character + " ready tracker"));
		}
	}

	private void AddTracker(string character)
	{
		if (!assignedTrackers.ContainsKey(character) && (isAlive == null || isAlive(character)))
		{
			AddTrackerInternal(character);
		}
		else
		{
			RefreshReady(character);
		}
	}

	private IEnumerator AddTrackerCoroutine(string character, bool ready)
	{
		Log.GUIInfo("Ready Tracker: Started AddTrackerCoroutine for " + character);
		while (trackers.Count == 0)
		{
			yield return null;
		}
		AddTrackerInternal(character);
		assignedTrackers[character].ShowReady(ready);
		Log.GUIInfo("Ready Tracker: Ended AddTrackerCoroutine for " + character);
	}

	private void AddTrackerInternal(string id)
	{
		UIReadyTracker uIReadyTracker = trackers[0];
		if (mode == EReadyMode.Character)
		{
			uIReadyTracker.Init(UIInfoTools.Instance.GetCharacterMarker(CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == id).CharacterModel, CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == id).CharacterYML.CustomCharacterConfig));
		}
		else
		{
			uIReadyTracker.Init(PlayerRegistry.AllPlayers.First((NetworkPlayer it) => it.PlayerID.ToString() == id).Avatar);
		}
		uIReadyTracker.gameObject.SetActive(value: true);
		assignedTrackers[id] = uIReadyTracker;
		trackers.RemoveAt(0);
		Log.GUIInfo(string.Format("Added " + id + " ready tracker"));
	}

	public void RefreshReady(string id)
	{
		if (!gameObject.activeSelf || (isAlive != null && !isAlive(id)))
		{
			return;
		}
		switch (mode)
		{
		case EReadyMode.Character:
		{
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == id);
			if (cMapCharacter != null)
			{
				NetworkPlayer controller = ControllableRegistry.GetController(ControllableRegistry.GetControllableID(cMapCharacter));
				SetReady(id, Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(controller));
			}
			break;
		}
		case EReadyMode.Player:
			SetReady(id, Singleton<UIReadyToggle>.Instance.PlayersReady.Exists((NetworkPlayer it) => it.PlayerID.ToString() == id));
			break;
		}
	}

	public void RefreshReady()
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		foreach (KeyValuePair<string, UIReadyTracker> assignedTracker in assignedTrackers)
		{
			RefreshReady(assignedTracker.Key);
		}
	}

	public void RefreshReady(NetworkPlayer player, bool isReady)
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		if (mode == EReadyMode.Character)
		{
			foreach (NetworkControllable myControllable in player.MyControllables)
			{
				string characterIDByControllerID = ControllableRegistry.GetCharacterIDByControllerID(myControllable.ID);
				SetReady(characterIDByControllerID, isReady);
			}
			return;
		}
		RefreshReady(player.PlayerID.ToString());
	}

	private void SetReady(string characterID, bool ready)
	{
		if (isAlive != null && !isAlive(characterID))
		{
			return;
		}
		if (!assignedTrackers.ContainsKey(characterID))
		{
			if (trackers.Count == 0)
			{
				StartCoroutine(AddTrackerCoroutine(characterID, ready));
				return;
			}
			AddTracker(characterID);
			assignedTrackers[characterID].ShowReady(ready);
		}
		else
		{
			assignedTrackers[characterID].ShowReady(ready);
		}
	}

	public void HideTrackers()
	{
		gameObject.SetActive(value: false);
	}
}
