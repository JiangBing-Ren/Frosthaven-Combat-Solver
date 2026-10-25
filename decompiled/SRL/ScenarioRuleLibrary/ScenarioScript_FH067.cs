using System;
using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH067 : ScenarioScript
{
	private const string _elementPrefabName = "ElementalCore";

	private ElementInfusionBoardManager.EElement[] _elements = new ElementInfusionBoardManager.EElement[4]
	{
		ElementInfusionBoardManager.EElement.Earth,
		ElementInfusionBoardManager.EElement.Air,
		ElementInfusionBoardManager.EElement.Ice,
		ElementInfusionBoardManager.EElement.Light
	};

	private List<Point> _tileFPoints = new List<Point>
	{
		new Point(1, 6),
		new Point(15, 6),
		new Point(1, 7),
		new Point(14, 5)
	};

	private List<Point> _elementTiles = new List<Point>
	{
		new Point(9, 3),
		new Point(6, 3),
		new Point(6, 9),
		new Point(9, 9)
	};

	private Dictionary<ElementInfusionBoardManager.EElement, string> _demonClasses = new Dictionary<ElementInfusionBoardManager.EElement, string>
	{
		{
			ElementInfusionBoardManager.EElement.Fire,
			"FlameDemon"
		},
		{
			ElementInfusionBoardManager.EElement.Ice,
			"FrostDemon"
		},
		{
			ElementInfusionBoardManager.EElement.Air,
			"WindDemon"
		},
		{
			ElementInfusionBoardManager.EElement.Earth,
			"EarthDemon"
		},
		{
			ElementInfusionBoardManager.EElement.Light,
			"SunDemon"
		},
		{
			ElementInfusionBoardManager.EElement.Dark,
			"NightDemon"
		}
	};

	private Dictionary<TileIndex, ElementInfusionBoardManager.EElement> _standees = new Dictionary<TileIndex, ElementInfusionBoardManager.EElement>();

	private CScenarioModifier _healCoreRule;

	protected override bool Initialize()
	{
		Trace("Conclusion Params JSON: " + ScenarioScript._css.ScenarioSetup.ConclusionParametersJSON, null, 40);
		FHScenarioSetup scenarioSetup = ScenarioScript._css.ScenarioSetup;
		if (scenarioSetup.ParseScenariosConclusionParameters(out var all))
		{
			if (scenarioSetup.GetConclusionParameters(66u, all, out var result))
			{
				if (_elementTiles.Count < result.Count)
				{
					Error($"Too much elements coming from scenario 66 - {result.Count}. Taking the first ones.", null, 47);
				}
				else if (_elementTiles.Count > result.Count)
				{
					Error($"Too little elements coming from scenario 66 - {result.Count}. Use default ones.", null, 49);
					result.Clear();
				}
				List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
				foreach (long item in result)
				{
					if (!Enum.IsDefined(typeof(ElementInfusionBoardManager.EElement), (int)item))
					{
						Error($"Invalid element coming from scenario 66 - {item}. Use default ones.", null, 58);
						list.Clear();
						break;
					}
					ElementInfusionBoardManager.EElement eElement = (ElementInfusionBoardManager.EElement)item;
					list.Add(eElement);
					Trace($"Acquired element {eElement}", null, 65);
					if (list.Count >= _elementTiles.Count)
					{
						break;
					}
				}
				if (list.Count > 0 && !list.Contains(ElementInfusionBoardManager.EElement.Any))
				{
					_elements = list.ToArray();
				}
				else
				{
					Error("Scenario 66 finished with cheats, no elements set. Will use defaults....", null, 75);
				}
			}
			else
			{
				Error("Need demon elements to be passed down from scenario 66 as Conclusion Parameters. Will use defaults....", null, 80);
			}
		}
		else
		{
			Log.Warning("Need demon elements to be passed down from scenario 66 as Conclusion Parameters. Will use defaults....");
		}
		RefreshElementsVisuals();
		int num2 = 0;
		foreach (CSpawner spawner in ScenarioScript._css.Spawners)
		{
			if (!_tileFPoints.Contains(spawner.ArrayIndex.ToPoint()))
			{
				if (!_standees.TryGetValue(spawner.ArrayIndex, out var value))
				{
					value = _elements[num2 % _elements.Length];
					num2++;
				}
				_standees[spawner.ArrayIndex] = value;
			}
		}
		foreach (CSpawner spawner2 in ScenarioScript._css.Spawners)
		{
			if (_tileFPoints.Contains(spawner2.ArrayIndex.ToPoint()))
			{
				continue;
			}
			string text = spawner2.SpawnerGuid.Substring(0, 8);
			if (spawner2.SpawnerData.LoopSpawnPattern)
			{
				Error("Spanwer " + text + " loops, shouldn't loop, forcing to not loop", null, 110);
				spawner2.SpawnerData.LoopSpawnPattern = false;
			}
			foreach (KeyValuePair<string, List<SpawnRoundEntry>> item2 in spawner2.SpawnerData.SpawnRoundEntryDictionary)
			{
				ElementInfusionBoardManager.EElement key = _standees[spawner2.ArrayIndex];
				string text2 = "\n" + text + ": " + spawner2.ArrayIndex?.ToString() + " " + key.ToString() + "\n";
				foreach (SpawnRoundEntry item3 in item2.Value)
				{
					text2 += "  ";
					for (int i = 0; i < item3.SpawnClass.Count; i++)
					{
						string newValue = _demonClasses[key];
						string value2 = item3.SpawnClass[i].Replace("ChaosDemon", newValue);
						item3.SpawnClass[i] = value2;
					}
					foreach (string item4 in item3.SpawnClass)
					{
						text2 = text2 + item4 + ", ";
					}
					text2 += "\n";
				}
				Trace(text2, null, 136);
			}
		}
		_healCoreRule = GetScenarioModifier("healCore");
		_healCoreRule.CustomLocKeyArgs = new List<object> { ScenarioManager.CurrentScenarioState.Level + 1 };
		return true;
	}

	private void RefreshElementsVisuals()
	{
		for (int i = 0; i < _elements.Length; i++)
		{
			ElementInfusionBoardManager.EElement eElement = _elements[i];
			Point point = _elementTiles[i];
			CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
			CObjectProp cObjectProp = cTile.m_Props.Find((CObjectProp p) => p.PrefabName == "ElementalCore");
			if (cObjectProp == null)
			{
				if (cTile.m_Props.Find((CObjectProp p) => p.RuntimeAttachedActor != null) == null)
				{
					continue;
				}
				cObjectProp = new CObjectProp("ElementalCore", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(point), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(cObjectProp);
			}
			CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
			cChangePropVisualState_MessageData.m_prop = cObjectProp;
			cChangePropVisualState_MessageData.m_stateName = "State" + eElement;
			cChangePropVisualState_MessageData.m_delay = 0.001f;
			ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor is CObjectActor cObjectActor)
		{
			cObjectActor.Tile.m_Props.Find((CObjectProp p) => p.PrefabName == "ElementalCore")?.DestroyProp();
			base.TickDeathInternal();
		}
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		yield break;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("heal_amount", (1 + ScenarioManager.CurrentScenarioState.Level).ToString());
	}
}
