using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH046 : ScenarioScript
{
	[ConsoleVariable(Alias = "ss046_debug_snows")]
	public static int DebugSnows_cvar = 0;

	private static string SNOW_PREFAB_NAME = "HarvestSnow";

	public Dictionary<Point, int> TilesSnows = new Dictionary<Point, int>
	{
		{
			new Point(10, 5),
			0
		},
		{
			new Point(11, 7),
			0
		},
		{
			new Point(13, 6),
			0
		},
		{
			new Point(13, 4),
			0
		},
		{
			new Point(11, 3),
			0
		}
	};

	private CObjective_Scripted _harvest20SnowsWinObjective;

	private CObjectDoor _snowDoor;

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			TickStartScenario();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			TickAfterAbility();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickRoomRevealed();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 56);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
			break;
		}
	}

	protected override bool Initialize()
	{
		_snowDoor = GetProp(9, 5, "ThinDoor") as CObjectDoor;
		_harvest20SnowsWinObjective = GetObjective("Harvest20SnowsWinObjective") as CObjective_Scripted;
		ScenarioManager.ToggleTileHighlights(TilesSnows.Keys, enabled: true);
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(TilesSnows.Values.ToList());
		Trace("[ffa000]Serialize: " + text + "[-]", null, 72);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			int idx = 0;
			List<int> tilesSnowsList = JsonConvert.DeserializeObject<List<int>>(s);
			TilesSnows = TilesSnows.ToDictionary((KeyValuePair<Point, int> p) => p.Key, (KeyValuePair<Point, int> p) => tilesSnowsList[idx++]);
		}
	}

	private void TickStartScenario()
	{
		_harvest20SnowsWinObjective.Total = 20;
		_harvest20SnowsWinObjective.Current = 0;
		_harvest20SnowsWinObjective.ScriptedIsComplete = false;
	}

	private void TickStartRound()
	{
		if (!_snowDoor.DoorIsOpen || !OncePerRound())
		{
			return;
		}
		foreach (Point item in TilesSnows.Keys.ToList())
		{
			CTile tile = ScenarioManager.GetTile(item);
			if (IsFreeTile(tile) && ScenarioManager.GetAllAdjacentTiles(tile).All((CTile t) => IsTileUnooccupied(t)))
			{
				PlaceSnow(tile);
			}
		}
	}

	private bool IsTileUnooccupied(CTile tile)
	{
		return ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex)?.IsDead ?? true;
	}

	private void TickAfterAbility()
	{
		CAbility cAbility = _tickActor?.LastAbilityPerformed;
		if (cAbility == null || (cAbility.Flags & CAbility.EAbilityFlag.FromScenarioModifier) == 0 || !(cAbility is CAbilityDestroyObstacle))
		{
			return;
		}
		int num = RemoveSnow(cAbility.TilesSelected[0], destroyProp: false);
		if (num > 0)
		{
			_harvest20SnowsWinObjective.Current = Math.Min(_harvest20SnowsWinObjective.Current + num, _harvest20SnowsWinObjective.Total);
			if (_harvest20SnowsWinObjective.Current >= _harvest20SnowsWinObjective.Total)
			{
				_harvest20SnowsWinObjective.ScriptedIsComplete = true;
			}
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickRepositionInternal()
	{
		if (TilesSnows.ContainsKey(_tickActor.ArrayIndex))
		{
			RemoveSnow(ScenarioManager.GetTile(_tickActor.ArrayIndex), destroyProp: true);
		}
	}

	protected void TickRoomRevealed()
	{
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			if (!spawner.IsActive)
			{
				spawner.SpawnerData.SpawnStartRound = (spawner.SpawnerData.LoopSpawnPattern ? (ScenarioScript.RoundNumber + 8) : ScenarioScript.RoundNumber);
				spawner.SetActive(active: true, forceDontSpawn: true);
				spawner.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			}
		}
		if (_tickActor.Type == CActor.EType.Player)
		{
			ApplyHarvestAbility(_tickActor);
		}
	}

	private void PlaceSnow(CTile tile)
	{
		Point arrayIndex = tile.m_ArrayIndex;
		if (TilesSnows[arrayIndex] <= 0)
		{
			TilesSnows[arrayIndex] = 1;
			SpawnSnowProp(tile, TilesSnows[arrayIndex]);
			return;
		}
		TilesSnows[arrayIndex]++;
		tile.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == SNOW_PREFAB_NAME).CustomLocKeyArgs = new List<object> { TilesSnows[arrayIndex] };
	}

	private int RemoveSnow(CTile tile, bool destroyProp)
	{
		int num = TilesSnows[tile.m_ArrayIndex];
		if (destroyProp && num > 0)
		{
			tile.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == SNOW_PREFAB_NAME).DestroyProp();
		}
		TilesSnows[tile.m_ArrayIndex] = 0;
		return num;
	}

	private void SpawnSnowProp(CTile tile, int snowsNumber)
	{
		TileIndex arrayIndex = new TileIndex(tile.m_ArrayIndex);
		CObjectProp cObjectProp = new CObjectProp(SNOW_PREFAB_NAME, ScenarioManager.ObjectImportType.VFXProp, arrayIndex, null, null, null, tile.m_HexMap.MapGuid);
		cObjectProp.PropCustomLocKey = "FH_46_Pure_Snow_T";
		cObjectProp.PropCustomDescriptionLocKey = "FH_46_Pure_Snow_D";
		cObjectProp.CustomLocKeyArgs = new List<object> { snowsNumber };
		tile.SpawnProp(cObjectProp);
	}

	private void ApplyHarvestAbility(CActor playerActor)
	{
		CAbility cAbility = GetScenarioModifier("HarvestAbility").TriggerAbility();
		cAbility.Start(playerActor, playerActor);
		cAbility.ApplyToActor(playerActor);
	}
}
