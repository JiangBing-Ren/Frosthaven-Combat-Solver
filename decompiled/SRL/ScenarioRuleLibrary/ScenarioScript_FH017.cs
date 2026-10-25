using System;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH017 : ScenarioScript
{
	[Serializable]
	public class Storage17
	{
		public int spawnedRound;

		public int roomsRevealed;

		public int monstersToSpawn;
	}

	private CSpawner _alliesSpawner;

	private CSpawner _eliteLivingSpiritIDSpawner;

	private CSpawner _eliteIceWraithSpawner;

	private CSpawner _livingDoomSpawner;

	private CSpawner _iceWraithSpawner;

	private CScenarioModifier ThreePlayerModifier;

	private CScenarioModifier TwoPlayerModifier;

	private Storage17 serializedProperties;

	private int monstersToSpawn
	{
		get
		{
			return serializedProperties.monstersToSpawn;
		}
		set
		{
			serializedProperties.monstersToSpawn = value;
		}
	}

	private int spawnedRound
	{
		get
		{
			return serializedProperties.spawnedRound;
		}
		set
		{
			serializedProperties.spawnedRound = value;
		}
	}

	private int roomsRevealed
	{
		get
		{
			return serializedProperties.roomsRevealed;
		}
		set
		{
			serializedProperties.roomsRevealed = value;
		}
	}

	private int StartScenarioAlliesCount => 4 - ScenarioManager.CurrentScenarioState.Players.Count;

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		ThreePlayerModifier?.SetDeactivated(ScenarioManager.Scenario.AllPlayers.Count != 3);
		TwoPlayerModifier?.SetDeactivated(ScenarioManager.Scenario.AllPlayers.Count != 2);
		spawnedRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		roomsRevealed = ScenarioManager.CurrentScenarioState.Maps.Count((CMap x) => x.Revealed);
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (Storage17)JsonConvert.DeserializeObject(storage, typeof(Storage17));
		}
	}

	private void SpawnMonster(CActor atPlayer, CSpawner spawner)
	{
		spawner.SetLocation(new TileIndex(atPlayer.ArrayIndex));
		spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
	}

	private void SpawnMonsters(PlayerState atPlayer)
	{
		switch (atPlayer.Actor.Tile.m_HexMap.RoomName)
		{
		case "Room_1":
			SpawnMonster(atPlayer.Actor, _eliteLivingSpiritIDSpawner);
			break;
		case "Room_2":
			SpawnMonster(atPlayer.Actor, _eliteIceWraithSpawner);
			break;
		case "Room_4":
			SpawnMonster(atPlayer.Actor, _livingDoomSpawner);
			break;
		case "Room_5":
			SpawnMonster(atPlayer.Actor, _iceWraithSpawner);
			break;
		}
	}

	private void SpawnMonsters()
	{
		for (int i = 0; i < monstersToSpawn; i++)
		{
			foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
			{
				SpawnMonsters(player);
			}
		}
		monstersToSpawn = 0;
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage17();
		_alliesSpawner = ScenarioManager.CurrentScenarioState.Spawners[0];
		_eliteLivingSpiritIDSpawner = ScenarioManager.CurrentScenarioState.Spawners[1];
		_eliteIceWraithSpawner = ScenarioManager.CurrentScenarioState.Spawners[2];
		_livingDoomSpawner = ScenarioManager.CurrentScenarioState.Spawners[3];
		_iceWraithSpawner = ScenarioManager.CurrentScenarioState.Spawners[4];
		ThreePlayerModifier = GetScenarioModifier("ThreePlayerHint");
		TwoPlayerModifier = GetScenarioModifier("TwoPlayerHint");
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (ScenarioScript.RoundNumber == 1 && OncePerRound())
			{
				SpawnStartScenarioAlliesOnStartingTiles(_alliesSpawner, StartScenarioAlliesCount);
			}
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			if (spawnedRound != ScenarioScript.RoundNumber)
			{
				spawnedRound = ScenarioScript.RoundNumber;
				SpawnMonsters();
			}
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 138);
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			break;
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		int num = 0;
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			if (map.Revealed)
			{
				num++;
			}
		}
		if (num == roomsRevealed)
		{
			return;
		}
		roomsRevealed = num;
		if (roomsRevealed <= 5)
		{
			monstersToSpawn++;
			return;
		}
		CObjectDoor cObjectDoor = GetProp(13, 18, "ThickDoor") as CObjectDoor;
		if (!cObjectDoor.DoorIsOpen)
		{
			cObjectDoor.ForceActivate(ScenarioManager.CurrentScenarioState.Players[0].Actor);
		}
		for (int num2 = cObjectDoor.LinkedPressurePlates.Count - 1; num2 >= 0; num2--)
		{
			cObjectDoor.UnlinkPressurePlate(cObjectDoor.LinkedPressurePlates[num2]);
		}
	}
}
