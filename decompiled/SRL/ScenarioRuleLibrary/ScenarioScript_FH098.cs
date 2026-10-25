using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH098 : ScenarioScript
{
	private const string _FallingDebrisPropName = "S98FallingDebris";

	private const string _RedLightPropName = "RoomRedLight";

	private const int _DebrisMinSpacing = 3;

	private const float _DebrisSpawnDelay = 0.15f;

	[StateDump(Skip = true)]
	public Action OnOpeningRoom;

	private static readonly Point[][] _RoomRedLightTiles = new Point[3][]
	{
		new Point[2]
		{
			new Point(3, 16),
			new Point(7, 14)
		},
		new Point[1]
		{
			new Point(11, 6)
		},
		new Point[3]
		{
			new Point(13, 10),
			new Point(16, 4),
			new Point(20, 4)
		}
	};

	private List<CMap> _RoomsInOrder;

	private List<byte> _visitedRooms = new List<byte>();

	private List<byte> _collapsingRooms = new List<byte>();

	public IReadOnlyList<byte> CollapsingRooms => _collapsingRooms;

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickPhaseStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 28);
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_visitedRooms);
		Trace("[ffa000]Serialize: " + text + "[-]", null, 34);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 39);
		if (!string.IsNullOrEmpty(s))
		{
			_visitedRooms.Clear();
			_visitedRooms.AddRange(JsonConvert.DeserializeObject<List<byte>>(s));
		}
	}

	protected override bool Initialize()
	{
		_RoomsInOrder = new List<CMap>
		{
			ScenarioScript.FindRoom("Room_1"),
			ScenarioScript.FindRoom("Room_2"),
			ScenarioScript.FindRoom("Room_3"),
			ScenarioScript.FindRoom("Room_4")
		};
		OnOpeningRoom?.Invoke();
		return base.Initialize();
	}

	private bool IsEscapeZoneProp(CTile tile)
	{
		foreach (CObjectProp prop in tile.m_Props)
		{
			if (_scenarioModifier.ScenarioModifierFilter.IsValidProp(prop))
			{
				return true;
			}
		}
		return false;
	}

	private void ScenarioEffectSufferDamage(int damage, bool allowBurnCard)
	{
		Trace($"Suffer Scenario Effect damage {damage}", null, 118);
		GameState.ActorBeenDamaged(_tickActor, damage, allowBurnCard, null, null, CAbility.EAbilityType.None, 0, isTrapDamage: true);
		GameState.ActorHealthCheck(_tickActor, _tickActor);
	}

	private void TickPhaseStartRound()
	{
	}

	private void TickPhaseEndRound()
	{
		for (int i = 0; i < _visitedRooms.Count; i++)
		{
			CMap cMap = _RoomsInOrder[_visitedRooms[i]];
			if (_tickActorTile.m_HexMap == cMap || _tickActorTile.m_Hex2Map == cMap)
			{
				if (OncePerRound())
				{
					PlayStructureShake();
				}
				ScenarioEffectSufferDamage(10, allowBurnCard: true);
				break;
			}
		}
	}

	private void TickPhaseStartTurn()
	{
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		_ = _tickActor;
		for (int num = _RoomsInOrder.Count - 1; num >= 1; num--)
		{
			CMap room = _RoomsInOrder[num];
			if (ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => x.Tile.m_HexMap == room && x.Tile.m_Hex2Map == null))
			{
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					if (!_visitedRooms.Contains((byte)num2))
					{
						_visitedRooms.Add((byte)num2);
					}
				}
				break;
			}
		}
	}

	private void PlayStructureShake()
	{
		ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null));
		ScenarioRuleClient.MessageHandler(new CPlaySound_MessageData("Earthquake98"));
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		CMap previousRoom = GetPreviousRoom(actor);
		SpawnFallingDebris(actor);
		PlayStructureShake();
		if (previousRoom != null)
		{
			int num = _RoomsInOrder.IndexOf(previousRoom);
			SpawnRedLights(num);
			if (!_collapsingRooms.Contains((byte)num))
			{
				_collapsingRooms.Add((byte)num);
			}
		}
		OnOpeningRoom?.Invoke();
	}

	private CMap GetPreviousRoom(CActor actor)
	{
		foreach (CMap item in _RoomsInOrder)
		{
			if (actor.Tile.m_HexMap == item || actor.Tile.m_Hex2Map == item)
			{
				return item;
			}
		}
		return null;
	}

	private List<CTile> PickSpreadTiles(CMap room)
	{
		List<CTile> list = (from t in GetRoomTiles(room.RoomName)
			where t.IsWalkable && t.FindDoor() == null
			select t).ToList();
		list.Shuffle(ScenarioScript._css.ScenarioRNG);
		List<CTile> list2 = new List<CTile>();
		foreach (CTile tile in list)
		{
			if (!list2.Any((CTile t) => ScenarioManager.GetTileDistance(t.m_ArrayIndex.X, t.m_ArrayIndex.Y, tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y) < 3))
			{
				list2.Add(tile);
			}
		}
		return list2;
	}

	private void SpawnRedLights(int roomIndex)
	{
		if (roomIndex >= _RoomRedLightTiles.Length)
		{
			return;
		}
		Point[] array = _RoomRedLightTiles[roomIndex];
		for (int i = 0; i < array.Length; i++)
		{
			Point point = array[i];
			CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
			if (cTile != null)
			{
				CObjectProp prop = new CObjectProp("RoomRedLight", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(point), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(prop);
				Trace($"Spawned red light at {point.X},{point.Y}", null, 218);
			}
		}
	}

	private void SpawnFallingDebris(CActor actor)
	{
		CMap previousRoom = GetPreviousRoom(actor);
		if (previousRoom == null)
		{
			return;
		}
		foreach (CTile item in PickSpreadTiles(previousRoom))
		{
			SpawnVFXProp(item, "S98FallingDebris");
		}
	}
}
