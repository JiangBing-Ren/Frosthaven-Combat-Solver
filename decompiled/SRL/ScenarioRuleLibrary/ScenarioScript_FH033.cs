using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.Utils;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH033 : ScenarioScript
{
	private readonly string _dowsingRunePropname = "DowsingRune";

	private readonly int _connectionsToGetStone = 3;

	private CObjective_Scripted _winObjective;

	private CScenarioModifier _scenarioAbilityHolderGetStone;

	private CScenarioModifier _scenarioAbilityHolderMoveRune;

	private CScenarioModifier _scenarioAbilityHolderStoneBuff;

	private CScenarioModifier hint2;

	private CScenarioModifier hint3;

	private CScenarioModifier spawner2;

	private CScenarioModifier spawner3;

	private CScenarioModifier spawner4;

	private List<int> _remainingRuneReaches = new List<int>();

	private Dictionary<Point, int> _runes = new Dictionary<Point, int>
	{
		{
			new Point(6, 5),
			-1
		},
		{
			new Point(7, 3),
			-1
		},
		{
			new Point(9, 2),
			-1
		},
		{
			new Point(10, 1),
			-1
		},
		{
			new Point(7, 7),
			-1
		},
		{
			new Point(9, 6),
			-1
		},
		{
			new Point(10, 4),
			-1
		},
		{
			new Point(11, 3),
			-1
		},
		{
			new Point(8, 9),
			-1
		},
		{
			new Point(10, 8),
			-1
		},
		{
			new Point(11, 7),
			-1
		},
		{
			new Point(12, 5),
			-1
		}
	};

	private Dictionary<Point, int> _tileRuneConnections;

	public CMap _lastRoom;

	private CSpawner _earthDemonSpawner;

	private CSpawner _flameDemonSpawner;

	[StateDump(Skip = true)]
	private CAbility _lastEndedAbility;

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_runes.Keys.Select((Point k) => (X: k.X, Y: k.Y)));
		string text2 = JsonConvert.SerializeObject(_runes.Values.ToList());
		string text3 = JsonConvert.SerializeObject(_remainingRuneReaches);
		string text4 = JsonConvert.SerializeObject(_lastRoom.Revealed);
		string text5 = JsonConvert.SerializeObject(new string[4] { text, text2, text3, text4 });
		Trace("[ffa000]Serialize: " + text5 + "[-]", null, 54);
		return text5;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 60);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			List<(int, int)> source = JsonConvert.DeserializeObject<List<(int, int)>>(array[0]);
			List<int> values = JsonConvert.DeserializeObject<List<int>>(array[1]);
			int idx = 0;
			_runes = source.ToDictionary(((int, int) p) => new Point(p.Item1, p.Item2), ((int, int) p) => values[idx++]);
			_remainingRuneReaches = JsonConvert.DeserializeObject<List<int>>(array[2]);
			if (JsonConvert.DeserializeObject<bool>(array[3]))
			{
				_tileRuneConnections = GetTileRuneConnections();
			}
			RefreshDowsingRunesVisualState();
			CheckObjectivesComplete();
		}
		UpdateModifierHints();
	}

	protected override bool Initialize()
	{
		_flameDemonSpawner = ScenarioManager.CurrentScenarioState.Spawners[1];
		_earthDemonSpawner = ScenarioManager.CurrentScenarioState.Spawners[2];
		_scenarioAbilityHolderGetStone = GetScenarioModifier("ScenarioAbilityHolderGetStone");
		_scenarioAbilityHolderMoveRune = GetScenarioModifier("ScenarioAbilityHolderMoveRune");
		_scenarioAbilityHolderStoneBuff = GetScenarioModifier("ScenarioAbilityHolderStoneBuff");
		_winObjective = (CObjective_Scripted)GetObjective("WinObjective");
		_lastRoom = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_15");
		hint3 = GetScenarioModifier("Hint3");
		hint2 = GetScenarioModifier("Hint2");
		UpdateWinObjective();
		return true;
	}

	private void UpdateModifierHints()
	{
		bool revealed = _lastRoom.Revealed;
		hint3.SetDeactivated(!revealed);
		_scenarioAbilityHolderMoveRune.SetDeactivated(!revealed);
		hint2.SetDeactivated(!revealed);
		_scenarioAbilityHolderStoneBuff.SetDeactivated(!revealed);
	}

	private void RefreshDowisngRuneVisualState(Point runePosition, int runeReach)
	{
		CObjectProp cObjectProp = ScenarioManager.Tiles[runePosition.X, runePosition.Y].m_Props.Find((CObjectProp p) => p.PrefabName == "DowsingRune");
		if (cObjectProp != null)
		{
			if (runeReach >= 0)
			{
				cObjectProp.PropCustomLocKey = "FH_Scenario_Campaign_033_DowsingRune_Active";
				cObjectProp.CustomLocKeyArgs = new List<object> { runeReach };
			}
			else
			{
				cObjectProp.PropCustomLocKey = "FH_Scenario_Campaign_033_DowsingRune";
			}
			string stateName = ((runeReach >= 0) ? "StateOn" : "StateOff");
			CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
			cChangePropVisualState_MessageData.m_prop = cObjectProp;
			cChangePropVisualState_MessageData.m_stateName = stateName;
			cChangePropVisualState_MessageData.m_delay = 0.001f;
			ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
		}
	}

	private void RefreshDowsingRunesVisualState()
	{
		foreach (KeyValuePair<Point, int> rune in _runes)
		{
			RefreshDowisngRuneVisualState(rune.Key, rune.Value);
		}
	}

	private void AddScenarioAbilityToTickActor(CScenarioModifier holder)
	{
		if (_tickActor.Type == CActor.EType.Player)
		{
			CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(_tickActor, _tickActor);
			cAbility.ApplyToActor(_tickActor);
		}
	}

	private void RemoveScenarioAbilityFromTickActor(CScenarioModifier holder)
	{
		if (_tickActor.Type != CActor.EType.Player)
		{
			return;
		}
		for (int num = holder.ActiveBonuses.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = holder.ActiveBonuses[num];
			if (cActiveBonus.Actor == _tickActor)
			{
				cActiveBonus.Finish();
			}
		}
	}

	private void TryAddGetStoneAbility()
	{
		if (_tickActor.Type == CActor.EType.Player && _tileRuneConnections != null && _tileRuneConnections.TryGetValue(_tickActor.ArrayIndex, out var value) && value >= _connectionsToGetStone && !_runes.TryGetValue(_tickActor.ArrayIndex, out var _) && !_scenarioAbilityHolderGetStone.ActiveBonuses.Any((CActiveBonus b) => b.Actor == _tickActor))
		{
			AddScenarioAbilityToTickActor(_scenarioAbilityHolderGetStone);
		}
	}

	private void TryAddMoveRuneAbility()
	{
		if (_tickActor.Type == CActor.EType.Player && _runes.TryGetValue(_tickActor.ArrayIndex, out var _) && !_scenarioAbilityHolderMoveRune.ActiveBonuses.Any((CActiveBonus b) => b.Actor == _tickActor))
		{
			AddScenarioAbilityToTickActor(_scenarioAbilityHolderMoveRune);
		}
	}

	private List<CTile> GetConnectedRunesTiles(Point tilePos, bool limit = false)
	{
		Dictionary<Point, int> connectedRunes = GetConnectedRunes(tilePos);
		List<CTile> list = new List<CTile>();
		foreach (KeyValuePair<Point, int> item in connectedRunes)
		{
			list.Add(ScenarioManager.Tiles[item.Key.X, item.Key.Y]);
		}
		if (limit)
		{
			while (list.Count > _connectionsToGetStone)
			{
				list.RemoveAt(0);
			}
		}
		return list;
	}

	private Dictionary<Point, int> GetConnectedRunes(Point tilePos)
	{
		Dictionary<Point, int> dictionary = new Dictionary<Point, int>();
		foreach (KeyValuePair<Point, int> rune in _runes)
		{
			if (ScenarioManager.GetTileDistance(tilePos.X, tilePos.Y, rune.Key.X, rune.Key.Y) <= rune.Value)
			{
				dictionary[rune.Key] = rune.Value;
			}
		}
		return dictionary;
	}

	private int GetNumRuneConnections(Point tilePos)
	{
		if (_runes.TryGetValue(tilePos, out var _))
		{
			return -1;
		}
		return GetConnectedRunes(tilePos).Count;
	}

	private Dictionary<Point, int> GetTileRuneConnections()
	{
		Dictionary<Point, int> dictionary = new Dictionary<Point, int>();
		foreach (CMapTile mapTile in _lastRoom.MapTiles)
		{
			if ((mapTile.Flags & EFlags.Edge) == 0)
			{
				Point point = new Point(mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y);
				dictionary[point] = GetNumRuneConnections(point);
			}
		}
		return dictionary;
	}

	private void RandomizeRuneReaches()
	{
		_remainingRuneReaches.Clear();
		for (int i = 1; i <= _runes.Count; i++)
		{
			_remainingRuneReaches.Add(i / 2 + i % 2);
		}
		_remainingRuneReaches.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
	}

	private void InitRunes()
	{
		if (OncePerRound())
		{
			RandomizeRuneReaches();
			RefreshRunes(init: true);
		}
	}

	private string GetPropPrefabName(Point pos, int numConnections)
	{
		return numConnections switch
		{
			-1 => _dowsingRunePropname, 
			0 => null, 
			1 => "RuneInfluence", 
			2 => "RuneInfluenceStrong", 
			_ => "RuneInfluenceActivated", 
		};
	}

	private void PlaceProp(Point pos, string prefabName)
	{
		if (prefabName != null)
		{
			CTile cTile = ScenarioManager.Tiles[pos.X, pos.Y];
			CObjectProp cObjectProp = new CObjectProp(prefabName, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(pos), null, null, null, cTile.m_HexMap.MapGuid);
			if (_runes.TryGetValue(pos, out var _))
			{
				cObjectProp.PropCustomLocKey = "FH_Scenario_Campaign_033_DowsingRune";
				cObjectProp.PropCustomDescriptionLocKey = "FH_33_Dowsing_Rune_D";
			}
			cTile.SpawnProp(cObjectProp);
		}
	}

	private void RemoveProp(Point pos, string prefabName)
	{
		if (prefabName == null)
		{
			return;
		}
		CTile cTile = ScenarioManager.Tiles[pos.X, pos.Y];
		if (cTile.m_Props.Count <= 0)
		{
			return;
		}
		foreach (CObjectProp item in cTile.m_Props.ToList())
		{
			if (item.PrefabName == prefabName)
			{
				item.DestroyProp();
				Trace("Destroyed prop " + item.PrefabName, null, 301);
				break;
			}
		}
	}

	private void RefreshRunes(bool init = false)
	{
		if (!init && !_lastRoom.Revealed)
		{
			return;
		}
		Dictionary<Point, int> tileRuneConnections = _tileRuneConnections;
		_tileRuneConnections = GetTileRuneConnections();
		foreach (KeyValuePair<Point, int> tileRuneConnection in _tileRuneConnections)
		{
			int value = 0;
			tileRuneConnections?.TryGetValue(tileRuneConnection.Key, out value);
			string propPrefabName = GetPropPrefabName(tileRuneConnection.Key, value);
			string propPrefabName2 = GetPropPrefabName(tileRuneConnection.Key, tileRuneConnection.Value);
			if (propPrefabName != propPrefabName2)
			{
				RemoveProp(tileRuneConnection.Key, propPrefabName);
				PlaceProp(tileRuneConnection.Key, propPrefabName2);
			}
		}
	}

	private void SpawnRuneDemon(int runeReach)
	{
		CSpawner cSpawner = ((runeReach % 2 == 0) ? _earthDemonSpawner : _flameDemonSpawner);
		cSpawner.SetLocation(new TileIndex(_tickActor.ArrayIndex));
		cSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor.Type != CActor.EType.Player)
		{
			return;
		}
		RemoveScenarioAbilityFromTickActor(_scenarioAbilityHolderMoveRune);
		RemoveScenarioAbilityFromTickActor(_scenarioAbilityHolderGetStone);
		TryAddGetStoneAbility();
		if (_runes.TryGetValue(_tickActor.ArrayIndex, out var value) && value <= 0)
		{
			value = _remainingRuneReaches.PopLast();
			_runes[_tickActor.ArrayIndex] = value;
			RefreshDowisngRuneVisualState(_tickActor.ArrayIndex, value);
			RefreshRunes();
			SpawnRuneDemon(value);
			if (_tickAbility is CAbilityMove { RemainingMoves: >0 } cAbilityMove)
			{
				cAbilityMove.ForceInterrupt();
			}
		}
	}

	private void RemoveRunesForRadiantStone(List<CTile> runeTiles)
	{
		for (int i = 0; i < runeTiles.Count; i++)
		{
			_runes.Remove(runeTiles[i].m_ArrayIndex);
		}
		RefreshRunes();
	}

	private void UpdateWinObjective()
	{
		_winObjective.Total = 3;
		_winObjective.Current = _scenarioAbilityHolderStoneBuff.ActiveBonuses.Count;
		_winObjective.ScriptedIsComplete = _winObjective.Current == _winObjective.Total;
		_winObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		CheckObjectivesComplete();
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickActor.Type != CActor.EType.Player)
		{
			return;
		}
		CAbility cAbility = _tickActor?.LastAbilityPerformed;
		if (cAbility != null && _lastEndedAbility != cAbility)
		{
			_lastEndedAbility = cAbility;
			if ((cAbility.Flags & CAbility.EAbilityFlag.Special1) != 0)
			{
				_tickActor.FindApplicableActiveBonuses();
				RemoveScenarioAbilityFromTickActor(_scenarioAbilityHolderGetStone);
				RemoveRunesForRadiantStone(cAbility.TilesSelected);
				AddScenarioAbilityToTickActor(_scenarioAbilityHolderStoneBuff);
				UpdateWinObjective();
			}
			else if ((cAbility.Flags & CAbility.EAbilityFlag.Special2) != 0)
			{
				RemoveScenarioAbilityFromTickActor(_scenarioAbilityHolderMoveRune);
				int num = _runes[_tickActor.ArrayIndex];
				_runes.Remove(_tickActor.ArrayIndex);
				_runes[cAbility.TilesSelected[0].m_ArrayIndex] = num;
				RefreshRunes();
				RefreshDowisngRuneVisualState(cAbility.TilesSelected[0].m_ArrayIndex, num);
				TryAddGetStoneAbility();
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			RemoveScenarioAbilityFromTickActor(_scenarioAbilityHolderGetStone);
			RemoveScenarioAbilityFromTickActor(_scenarioAbilityHolderMoveRune);
			TryAddGetStoneAbility();
			TryAddMoveRuneAbility();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			InitRunes();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 433);
			break;
		}
	}

	public bool IsValidMoveRuneTile(TValue tTile)
	{
		CTile cTile = tTile.GetObject<CTile>();
		if (_runes.ContainsKey(cTile.m_ArrayIndex))
		{
			return false;
		}
		return true;
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		UpdateModifierHints();
	}
}
