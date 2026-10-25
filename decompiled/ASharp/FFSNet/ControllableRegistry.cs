using System.Collections.Generic;
using System.Linq;
using FHRL;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FFSNet;

public static class ControllableRegistry
{
	public static List<NetworkControllable> AllControllables { get; private set; } = new List<NetworkControllable>();

	public static List<NetworkControllable> ParticipatingControllables => AllControllables.FindAll((NetworkControllable x) => x.IsParticipant);

	public static ControllableObjectChangedEvent OnControllableObjectChanged { get; set; }

	public static ControllerChangedEvent OnControllerChanged { get; set; }

	public static NetworkControllable CreateControllable(int id, IControllable controllableObject, PartySlotID initialSlot, bool changeObjectIfControllableExists = true, bool releaseOldControllableObject = true)
	{
		if (id == 0)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00019", "Error creating a new NetworkControllable. The ID #0 is reserved as the state default.");
			return null;
		}
		if (controllableObject == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00018", "Error creating a new NetworkControllable. ControllableObject returns null.");
			return null;
		}
		if (!PlayerRegistry.IsValidSlot(initialSlot))
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00018", $"Error creating a new NetworkControllable. Invalid party slot '{initialSlot}'!");
			return null;
		}
		NetworkControllable networkControllable = GetControllable(id);
		if (networkControllable == null && PlayerRegistry.SlotHasControllable(initialSlot))
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00018", $"Error creating a new NetworkControllable. Party slot '{initialSlot}' is already taken!");
			return null;
		}
		if (networkControllable == null)
		{
			networkControllable = new NetworkControllable(id, controllableObject);
			AllControllables.Add(networkControllable);
			Log.NetworkInfo("Controllable (ID: " + networkControllable.ID + ") CREATED");
		}
		else if (changeObjectIfControllableExists && controllableObject != networkControllable.ControllableObject)
		{
			Log.NetworkInfo("Changing the controllable object for the controllable with ID: " + id);
			IControllable controllableObject2 = networkControllable.ControllableObject;
			if (releaseOldControllableObject)
			{
				networkControllable.ControllableObject?.OnControlReleased();
			}
			networkControllable.ControllableObject = controllableObject;
			OnControllableObjectChanged?.Invoke(networkControllable, controllableObject2, controllableObject);
		}
		if (PlayerRegistry.GetSlotForControllable(networkControllable) != initialSlot)
		{
			PlayerRegistry.AssignControllableToSlot(networkControllable, initialSlot);
		}
		return networkControllable;
	}

	public static void DestroyControllable(int id, bool syncToClientsIfServer = false)
	{
		DestroyControllable(GetControllable(id), syncToClientsIfServer);
	}

	public static void DestroyControllable(NetworkControllable controllable, bool syncToClientsIfServer = false)
	{
		if (controllable != null)
		{
			if (AllControllables.Contains(controllable))
			{
				PartySlotID slot = controllable.Slot;
				if (slot.IsValid)
				{
					PlayerRegistry.AssignControllableToSlot(null, slot);
				}
				AllControllables.Remove(controllable);
				Log.NetworkInfo("Controllable (ID: " + controllable.ID + ") DESTROYED");
				if (FFSNetwork.IsHost && syncToClientsIfServer)
				{
					FFSNetwork.SendTokenToClients(new ControllableDestructionEventToken
					{
						ControllableID = controllable.ID
					});
				}
			}
			else
			{
				Debug.LogError("Tried to remove a controllable from the registry but no such controllable resides within the list of controllables.");
			}
		}
		else
		{
			Debug.Log("Tried to remove a controllable from the registry but the controllable returns null.");
		}
	}

	public static int GetControllableID(CPlayerActor playerActor)
	{
		return playerActor.CharacterClass.ModelInstanceID;
	}

	public static int GetControllableID(CMapCharacter mapCharacter)
	{
		return CharacterClassManager.GetModelInstanceIDFromCharacterID(mapCharacter.CharacterID);
	}

	public static int GetControllableID(string characterID)
	{
		return CharacterClassManager.GetModelInstanceIDFromCharacterID(characterID);
	}

	public static CMapCharacter GetMapCharacterByControllerID(int controllerID, bool errorIfNotFound = true)
	{
		CMapParty mapParty = AdventureState.MapState.MapParty;
		foreach (CMapCharacter selectedCharacter in mapParty.SelectedCharacters)
		{
			if (GetControllableID(selectedCharacter) == controllerID)
			{
				return selectedCharacter;
			}
		}
		foreach (CMapCharacter checkCharacter in mapParty.CheckCharacters)
		{
			if (GetControllableID(checkCharacter) == controllerID)
			{
				return checkCharacter;
			}
		}
		foreach (CMapCharacter item in mapParty.CharactersRetiredThisSession)
		{
			if (GetControllableID(item) == controllerID)
			{
				return item;
			}
		}
		if (errorIfNotFound)
		{
			Log.ErrorWithReport("Unable to find character name for hash code");
		}
		return null;
	}

	public static string GetCharacterIDByControllerID(int controllerID, bool errorIfNotFound = true)
	{
		return GetMapCharacterByControllerID(controllerID, errorIfNotFound)?.CharacterID;
	}

	public static NetworkControllable GetControllable(int controllableID)
	{
		return AllControllables.FirstOrDefault((NetworkControllable x) => x.ID == controllableID);
	}

	public static NetworkPlayer GetController(int controllableID)
	{
		return GetControllable(controllableID)?.Controller;
	}

	internal static void ProcessControllerChanged(NetworkControllable controllable, NetworkPlayer oldPlayer)
	{
		if (oldPlayer != controllable.Controller)
		{
			controllable.ControllableObject?.OnControlReleased();
			controllable.ControllableObject?.OnControlAssigned(controllable.Controller);
			if (oldPlayer == PlayerRegistry.MyPlayer || controllable.Controller == PlayerRegistry.MyPlayer)
			{
				PlayerRegistry.MyControllablesChanged?.Invoke();
			}
			OnControllerChanged?.Invoke(controllable, oldPlayer, controllable.Controller);
			DesyncDetector.RequestDelayedCheckOnHost();
		}
	}

	public static void Reset()
	{
		OnControllableObjectChanged = null;
		OnControllerChanged = null;
		AllControllables.Clear();
		Debug.Log("Cleared Controllable Registry!");
	}
}
