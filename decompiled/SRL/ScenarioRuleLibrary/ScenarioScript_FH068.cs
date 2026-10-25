using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH068 : BossHasMonsterCardExtraTurnScenarioScript
{
	private List<CActor> _bossAttackActorsTargeted;

	private static Point _portalAPosition = new Point(2, 12);

	private static Point _portalBPosition = new Point(7, 12);

	private ElementInfusionBoardManager.EElement[] _elements = new ElementInfusionBoardManager.EElement[2]
	{
		ElementInfusionBoardManager.EElement.Earth,
		ElementInfusionBoardManager.EElement.Fire
	};

	private Dictionary<ElementInfusionBoardManager.EElement, string> _demonClasses = new Dictionary<ElementInfusionBoardManager.EElement, string>
	{
		{
			ElementInfusionBoardManager.EElement.Fire,
			"FlameDemonID"
		},
		{
			ElementInfusionBoardManager.EElement.Ice,
			"FrostDemonID"
		},
		{
			ElementInfusionBoardManager.EElement.Air,
			"WindDemonID"
		},
		{
			ElementInfusionBoardManager.EElement.Earth,
			"EarthDemonID"
		},
		{
			ElementInfusionBoardManager.EElement.Light,
			"SunDemonID"
		},
		{
			ElementInfusionBoardManager.EElement.Dark,
			"NightDemonID"
		}
	};

	protected override string BossClassID => "TheTormentorID";

	protected override string BossCardsClassID => "TheTormentorBossCardsID";

	protected override string MonsterClassID => "ChaosDemonID";

	protected override string MonsterEliteClassID => "ChaosDemonEliteID";

	private List<ElementInfusionBoardManager.EElement> AllElements => _demonClasses.Keys.ToList();

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
		{
			Trace("TickPhase " + _tickActor.Class.ID, null, 63);
			if (!(_tickActor.Class.ID == "TheTormentorID"))
			{
				break;
			}
			CAbility lastAbilityPerformed = _tickActor.LastAbilityPerformed;
			if (lastAbilityPerformed is CAbilityAttack cAbilityAttack)
			{
				_bossAttackActorsTargeted = cAbilityAttack.ActorsTargeted;
			}
			else if (_bossAttackActorsTargeted.Count > 0)
			{
				if ((lastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special2) != 0)
				{
					TickTormentorSpecial2(_tickActor);
				}
				else if ((lastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special1) != 0)
				{
					TickTormentorSpecial1(_tickActor);
				}
			}
			break;
		}
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			TickAfterSelectMonsterClassAbilityCards();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 84);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override bool Initialize()
	{
		Trace("Conclusion Params JSON: " + ScenarioScript._css.ScenarioSetup.ConclusionParametersJSON, null, 90);
		FHScenarioSetup scenarioSetup = ScenarioScript._css.ScenarioSetup;
		if (scenarioSetup.ParseScenariosConclusionParameters(out var all))
		{
			if (scenarioSetup.GetConclusionParameters(66u, all, out var result))
			{
				List<ElementInfusionBoardManager.EElement> elementsScenario66 = new List<ElementInfusionBoardManager.EElement>();
				foreach (long item in result)
				{
					ElementInfusionBoardManager.EElement eElement = (ElementInfusionBoardManager.EElement)item;
					elementsScenario66.Add(eElement);
					Trace($"Acquired element {eElement}", null, 100);
				}
				_elements = AllElements.FindAll((ElementInfusionBoardManager.EElement e) => !elementsScenario66.Contains(e)).ToArray();
			}
			else
			{
				Error("Need demon elements to be passed down from scenario 66 as Conclusion Parameters. Will use defaults....", null, 104);
			}
		}
		else
		{
			Log.Warning("Need demon elements to be passed down from scenario 66 as Conclusion Parameters. Will use defaults....");
		}
		List<CSpawner> spawners = ScenarioScript._css.Spawners;
		ActivateElementDemonSpawnerAt(_elements[0], _portalAPosition, spawners);
		ActivateElementDemonSpawnerAt(_elements[1], _portalBPosition, spawners);
		_bossAttackActorsTargeted = new List<CActor>();
		return base.Initialize();
	}

	protected override void TickPhaseStartRound()
	{
		base.TickPhaseStartRound();
		if (_tickActor.Class.ID == "TheTormentorID")
		{
			_bossAttackActorsTargeted = new List<CActor>();
		}
	}

	private void TickTormentorSpecial2(CActor tormentor)
	{
		foreach (CTile actorsAdjacentFeaturelessTile in GetActorsAdjacentFeaturelessTiles(_bossAttackActorsTargeted))
		{
			ScenarioManager.ObjectImportType type = ScenarioManager.ObjectImportType.TerrainIce;
			CObjectIceTerrain importObject = new CObjectIceTerrain(type.ToString(), type, new TileIndex(actorsAdjacentFeaturelessTile.m_ArrayIndex), null, null, tormentor, actorsAdjacentFeaturelessTile.m_HexMap.MapGuid);
			SpawnTerrain(tormentor, importObject, "Ice", actorsAdjacentFeaturelessTile);
		}
	}

	private void TickTormentorSpecial1(CActor tormentor)
	{
		List<CTile> targetsAdjacentFeaturelessTiles = GetActorsAdjacentFeaturelessTiles(_bossAttackActorsTargeted);
		foreach (CTile item in targetsAdjacentFeaturelessTiles)
		{
			ScenarioManager.ObjectImportType type = ScenarioManager.ObjectImportType.TerrainThorns;
			CObjectHazardousTerrain importObject = new CObjectHazardousTerrain(type.ToString(), type, new TileIndex(item.m_ArrayIndex), null, null, tormentor, item.m_HexMap.MapGuid);
			SpawnTerrain(tormentor, importObject, "Thorns", item);
		}
		List<CActor> list = ScenarioScript._css.Players.ConvertAll((PlayerState p) => p.Actor).FindAll((CActor p) => targetsAdjacentFeaturelessTiles.Contains(p.Tile));
		if (list.Count <= 0)
		{
			return;
		}
		bool flag = ElementInfusionBoardManager.GetAvailableElements().Contains(ElementInfusionBoardManager.EElement.Fire);
		foreach (CActor item2 in list)
		{
			int num = ScenarioScript._css.Players.Count - 1;
			if (flag)
			{
				num++;
			}
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "TormentorViolentTerrainDamage",
				m_ArrayIndex = item2.ArrayIndex
			});
			DealScenarioDamage(item2, num, isTrapDamage: false, isTerrainDamage: false, tormentor.MonsterClass.LocKey, null, null, null, showInflictMessage: false);
		}
		if (flag)
		{
			ElementInfusionBoardManager.Consume(ElementInfusionBoardManager.EElement.Fire, GameState.CurrentActor);
		}
	}

	protected override string SerializeInternal()
	{
		string[] array = JsonConvert.DeserializeObject<string[]>(base.SerializeInternal());
		string text = JsonConvert.SerializeObject(_bossAttackActorsTargeted?.Select((CActor a) => a.ActorGuid).ToArray());
		string text2 = JsonConvert.SerializeObject(new string[6]
		{
			array[0],
			array[1],
			array[2],
			array[3],
			array[4],
			text
		});
		Trace("[FH68]Serialize: " + text2 + "[-]", null, 185);
		return text2;
	}

	protected override void DeserializeInternal(string s)
	{
		base.DeserializeInternal(s);
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		string[] array = JsonConvert.DeserializeObject<string[]>(JsonConvert.DeserializeObject<string[]>(s)[5]);
		_bossAttackActorsTargeted = null;
		if (array == null)
		{
			return;
		}
		_bossAttackActorsTargeted = new List<CActor>();
		string[] array2 = array;
		foreach (string actorGuid in array2)
		{
			_bossAttackActorsTargeted.Add(ScenarioManager.Scenario.AllActors.Find((CActor a) => a.ActorGuid == actorGuid));
		}
	}

	private void SpawnTerrain(CActor targetingActor, CObjectProp importObject, string propName, CTile tile)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = "TormentorCreateTerrain",
			m_ArrayIndex = tile.m_ArrayIndex
		});
		tile.SpawnProp(importObject);
	}

	private List<CTile> GetActorsAdjacentFeaturelessTiles(List<CActor> actors)
	{
		List<CTile> list = new List<CTile>();
		foreach (CActor actor in actors)
		{
			foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(actor.Tile))
			{
				if (allAdjacentTile.IsWalkable && CAbilityFilter.IsTileFeatureless(allAdjacentTile) && !list.Contains(allAdjacentTile))
				{
					list.Add(allAdjacentTile);
				}
			}
		}
		return list;
	}

	private void ActivateElementDemonSpawnerAt(ElementInfusionBoardManager.EElement element, Point position, List<CSpawner> allSpawners)
	{
		List<CSpawner> list = allSpawners.FindAll((CSpawner s) => s.ArrayIndex.ToPoint() == position);
		string demonClass = _demonClasses[element];
		list.Find((CSpawner s) => s.SpawnRoundEntries[0].SpawnClass[1] == demonClass).SetActive(active: true, forceDontSpawn: true);
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		yield break;
	}
}
