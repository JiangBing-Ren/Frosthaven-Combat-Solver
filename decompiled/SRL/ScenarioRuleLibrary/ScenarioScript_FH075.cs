using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH075 : ScenarioScript
{
	public class Shaft
	{
		public List<Point> points = new List<Point>();

		public List<Point> mainPoints = new List<Point>();

		public CObjectProp prop;

		public bool Closed { get; private set; }

		public void SetClosed(bool val)
		{
			Closed = val;
			UpdateProp();
			ScenarioManager.ToggleTileHighlights(mainPoints, !Closed);
		}

		public void Close(CPlayerActor closingActor)
		{
			SetClosed(val: true);
			foreach (Point mainPoint in mainPoints)
			{
				foreach (CActor item in GameState.GetActorsOnTile(ScenarioManager.GetTile(mainPoint), closingActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), null, isTargetedAbility: false, true))
				{
					GameState.KillActor(item, item, CActor.ECauseOfDeath.Undetermined, out var _);
				}
			}
		}

		public void UpdateProp()
		{
			if (prop != null)
			{
				string stateName = (Closed ? "State1" : "State0");
				CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
				cChangePropVisualState_MessageData.m_prop = prop;
				cChangePropVisualState_MessageData.m_stateName = stateName;
				cChangePropVisualState_MessageData.m_delay = 0.001f;
				ref string destructionVFXName = ref cChangePropVisualState_MessageData.m_DestructionVFXName;
				string text;
				if (Closed)
				{
					text = ((mainPoints.Count > 2) ? "VFX_P_HallwayDestruction_3hex" : "VFX_P_HallwayDestruction_2hex");
				}
				else
				{
					text = string.Empty;
				}
				destructionVFXName = text;
				cChangePropVisualState_MessageData.m_DestructionVFXDelay = (Closed ? 0.3167f : 0f);
				ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
			}
		}
	}

	public const string demonID = "ChaosDemonID";

	public const string demonEliteID = "ChaosDemonEliteID";

	public Shaft[] Shafts;

	public CObjective_Scripted ShaftObjective;

	public CScenarioModifier ShaftModifier;

	protected override bool Initialize()
	{
		InitShafts();
		ShaftObjective = GetObjective("ShaftsClosed") as CObjective_Scripted;
		if (ShaftObjective != null)
		{
			ShaftObjective.Total = Shafts.Length;
			ShaftObjective.Current = 0;
		}
		ShaftModifier = GetScenarioModifier("ShaftRevealModifier");
		GetScenarioModifier("WaterDamageModifier").CustomLocKeyArgs = new List<object> { Math.Ceiling((float)ScenarioManager.CurrentScenarioState.Level * 0.5f) };
		UpdateRulesAndObjectivesVisibility();
		return true;
	}

	private void InitShafts()
	{
		Shafts = new Shaft[2]
		{
			new Shaft
			{
				points = new List<Point>
				{
					new Point(1, 15),
					new Point(2, 15),
					new Point(2, 14),
					new Point(3, 14)
				},
				mainPoints = new List<Point>
				{
					new Point(1, 15),
					new Point(2, 14)
				},
				prop = ScenarioManager.GetTile(2, 14)?.FindPropOfType<CObjectTerrainVisual>()
			},
			new Shaft
			{
				points = new List<Point>
				{
					new Point(14, 9),
					new Point(14, 8),
					new Point(14, 7),
					new Point(13, 9),
					new Point(13, 8),
					new Point(13, 7)
				},
				mainPoints = new List<Point>
				{
					new Point(14, 9),
					new Point(14, 8),
					new Point(14, 7)
				},
				prop = ScenarioManager.GetTile(14, 8)?.FindPropOfType<CObjectTerrainVisual>()
			}
		};
		Shaft[] shafts = Shafts;
		for (int i = 0; i < shafts.Length; i++)
		{
			shafts[i].SetClosed(val: false);
		}
	}

	private void UpdateRulesAndObjectivesVisibility()
	{
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_L");
		CMap cMap2 = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_G");
		bool flag = cMap.DoorProps.Any((CObjectProp x) => x is CObjectDoor cObjectDoor && cObjectDoor.DoorIsOpen) || cMap2.DoorProps.Any((CObjectProp x) => x is CObjectDoor cObjectDoor && cObjectDoor.DoorIsOpen);
		ShaftModifier.SetHiddenState(!flag);
		ShaftObjective.SetActivation(flag);
	}

	protected override string SerializeInternal()
	{
		string[] array = new string[Shafts.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = JsonConvert.SerializeObject(Shafts[i].Closed);
		}
		return JsonConvert.SerializeObject(array);
	}

	protected override void DeserializeInternal(string storage)
	{
		string[] array = JsonConvert.DeserializeObject<string[]>(storage);
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Shafts[i].SetClosed(JsonConvert.DeserializeObject<bool>(array[i]));
			}
		}
		UpdateShaftObjective();
		UpdateRulesAndObjectivesVisibility();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndTurn && GameState.InternalCurrentActor is CPlayerActor player)
		{
			TryCloseShafts(player);
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnRoomRevealed)
		{
			UpdateRulesAndObjectivesVisibility();
		}
	}

	private void UpdateShaftObjective()
	{
		if (ShaftObjective != null)
		{
			ShaftObjective.Current = Shafts.Count((Shaft x) => x.Closed);
			ShaftObjective.ScriptedIsComplete = ShaftObjective.Current == ShaftObjective.Total;
			ShaftObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		}
	}

	private void TryCloseShafts(CPlayerActor player)
	{
		bool flag = false;
		bool flag2 = false;
		Shaft[] shafts = Shafts;
		for (int i = 0; i < shafts.Length; i++)
		{
			if (shafts[i].Closed)
			{
				flag = true;
				break;
			}
		}
		shafts = Shafts;
		foreach (Shaft shaft in shafts)
		{
			if (!shaft.Closed && shaft.points.Contains(player.RealArrayIndex))
			{
				shaft.Close(player);
				flag2 = true;
			}
		}
		if (flag2)
		{
			UpdateShaftObjective();
			player.ShowLootAnimations();
			if (!flag)
			{
				CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
				{
					m_Point = player.ArrayIndex,
					m_Zoom = 0.3f
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.TriggerCampaignSection("FS_075_Infiltrating_the_Lair_Special_1_1");
				SpawnSpawner(new List<string> { "ChaosDemonID", "ChaosDemonID", "ChaosDemonID", "ChaosDemonEliteID" });
				SpawnSpawner(new List<string> { "ChaosDemonID", "ChaosDemonID", "ChaosDemonEliteID", "ChaosDemonEliteID" });
			}
		}
	}

	private void SpawnSpawner(List<string> classes)
	{
		Point point = new Point(8, 18);
		CTile tile = ScenarioManager.GetTile(point);
		CSpawner cSpawner = new CSpawner(startingMapGuid: tile.m_HexMap.MapGuid, spawnerData: new SpawnerData(spawnRoundEntryDictionary: new Dictionary<string, List<SpawnRoundEntry>> { ["Default"] = new List<SpawnRoundEntry>
		{
			new SpawnRoundEntry(classes)
		} }, spawnerTriggerType: SpawnerData.ESpawnerTriggerType.StartRound, spawnerActivationType: SpawnerData.ESpawnerActivationType.ScenarioStart, spawnStartRound: ScenarioManager.CurrentScenarioState.RoundNumber + 1, loopSpawnPattern: false, shouldCountTowardsKillAllEnemies: false, spawnRoundInterval: new List<int> { 1, 1, 1, 1 }, spawnerHoverNameLoc: null), arrayIndex: new TileIndex(point));
		tile.SpawnSpawner(cSpawner);
		cSpawner.SetActive(active: true);
		cSpawner.OnStartRound(ScenarioScript.PartySize, ScenarioScript.RoundNumber);
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("ChaosDemonID", 0, null, CActor.EType.Enemy);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.ToElite;
		yield return enemyState;
		EnemyState enemyState2 = new EnemyState("ChaosDemonID", 0, null, CActor.EType.Enemy);
		enemyState2.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState2.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState2.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState2.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState2.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.ToElite;
		yield return enemyState2;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("damage_from_water_hex", Math.Ceiling((float)ScenarioManager.CurrentScenarioState.Level * 0.5f).ToString());
	}
}
