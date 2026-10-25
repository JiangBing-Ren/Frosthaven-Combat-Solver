using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH124 : ScenarioScript
{
	private struct TileIceClusterSummary
	{
		public bool IsHazardous;

		public bool IsDifficult;

		public bool IsObstacle;
	}

	public const string SPIKES_PREFAB_NAME = "IcyRootsHazardous";

	public const string SPIKES_PREFAB_DESC = "FH_124_Crystalline_Roots_Hazardous_D";

	public const string SNOWDRIFT_PREFAB_NAME = "IcyRootsDifficultA";

	public const string SNOWDRIFT_PREFAB_DESC = "FH_124_Crystalline_Roots_Difficult_D";

	public const string MIX_PREFAB_NAME = "IcyRootsDifficultB";

	public const string MIX_PREFAB_DESC = "FH_124_Crystalline_Roots_Hazardous_and_Difficult_D";

	public const string SNOW_ROCK_PREFAB_NAME = "IcyRootsObstacle";

	public const string SNOW_ROCK_PREFAB_DESC = "";

	private static readonly Point TemportalPosition = new Point(5, 1);

	private static readonly List<string> RoomNames = new List<string> { "Room_1", "Room_2", "Room_3" };

	private static readonly Dictionary<string, List<Point[]>> ROOMS_ICE_CLUSTER_PATHS = new Dictionary<string, List<Point[]>>
	{
		{
			RoomNames[0],
			new List<Point[]>
			{
				new Point[2]
				{
					new Point(4, 21),
					new Point(4, 20)
				},
				new Point[3]
				{
					new Point(6, 18),
					new Point(5, 18),
					new Point(4, 18)
				},
				new Point[6]
				{
					new Point(4, 16),
					new Point(4, 17),
					new Point(5, 18),
					new Point(5, 19),
					new Point(6, 20),
					new Point(6, 21)
				}
			}
		},
		{
			RoomNames[1],
			new List<Point[]>
			{
				new Point[7]
				{
					new Point(4, 13),
					new Point(5, 12),
					new Point(5, 11),
					new Point(6, 10),
					new Point(6, 9),
					new Point(7, 8),
					new Point(7, 7)
				},
				new Point[4]
				{
					new Point(7, 12),
					new Point(6, 12),
					new Point(5, 12),
					new Point(4, 12)
				},
				new Point[5]
				{
					new Point(3, 9),
					new Point(4, 9),
					new Point(5, 9),
					new Point(6, 9),
					new Point(7, 9)
				},
				new Point[7]
				{
					new Point(6, 7),
					new Point(6, 8),
					new Point(5, 9),
					new Point(5, 10),
					new Point(4, 11),
					new Point(4, 12),
					new Point(3, 13)
				}
			}
		},
		{
			RoomNames[2],
			new List<Point[]>
			{
				new Point[5]
				{
					new Point(7, 4),
					new Point(6, 4),
					new Point(5, 4),
					new Point(4, 4),
					new Point(3, 4)
				},
				new Point[5]
				{
					new Point(3, 3),
					new Point(4, 3),
					new Point(5, 3),
					new Point(6, 3),
					new Point(7, 3)
				},
				new Point[5]
				{
					new Point(4, 1),
					new Point(5, 2),
					new Point(5, 3),
					new Point(6, 4),
					new Point(6, 5)
				},
				new Point[5]
				{
					new Point(6, 1),
					new Point(6, 2),
					new Point(5, 3),
					new Point(5, 4),
					new Point(4, 5)
				}
			}
		}
	};

	private int[] _roomsRevealRound = new int[3] { 1, -1, -1 };

	private CObjective_KillAllEnemies _killEnemiesObjective;

	private CObjective_Scripted _winObjective;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override bool Initialize()
	{
		_winObjective = (CObjective_Scripted)GetObjective("ReachTemportal");
		_killEnemiesObjective = (CObjective_KillAllEnemies)GetObjective("KillEnemies");
		ToggleTemportalPathfinding(enabled: false);
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			InitIceClusters(RoomNames[0]);
			UpdatePathfinding();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 91);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	private void TickEndRound()
	{
		if (!OncePerRound() || (_killEnemiesObjective.IsComplete && _winObjective.IsComplete))
		{
			return;
		}
		foreach (CMap map in ScenarioScript._css.Maps)
		{
			if (map.Revealed)
			{
				UpdateIceClusterProps(map);
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		bool flag = _tickAbility is CAbilityMove cAbilityMove && cAbilityMove.Jump;
		bool flyingAfterConditions = _tickActor.FlyingAfterConditions;
		CObjectProp cObjectProp = _tickActor.Tile.FindProp(ScenarioManager.ObjectImportType.VFXProp);
		if (cObjectProp != null && !flag && !flyingAfterConditions && (cObjectProp.PropCustomLocKey == "IcyRootsHazardous" || cObjectProp.PropCustomLocKey == "IcyRootsDifficultB"))
		{
			TryDealHazardousDamage(_tickActor, cObjectProp.PropCustomLocKey);
		}
		UpdateWinObjective();
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		UpdateWinObjective();
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		ToggleTemportalPathfinding(_tickAbility is CAbilityMove && _tickActor is CPlayerActor);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		ToggleTemportalPathfinding(enabled: false);
		if (_tickAbility is CAbilityMove cAbilityMove)
		{
			bool flag = cAbilityMove?.Jump ?? false;
			bool flyingAfterConditions = _tickActor.FlyingAfterConditions;
			CObjectProp cObjectProp = _tickActor.Tile.FindProp(ScenarioManager.ObjectImportType.VFXProp);
			if (((cObjectProp != null) & flag) && !flyingAfterConditions && (cObjectProp.PropCustomLocKey == "IcyRootsHazardous" || cObjectProp.PropCustomLocKey == "IcyRootsDifficultB"))
			{
				TryDealHazardousDamage(_tickActor, cObjectProp.PropCustomLocKey);
			}
		}
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		ToggleTemportalPathfinding(enabled: false);
	}

	private void ToggleTemportalPathfinding(bool enabled)
	{
		ScenarioManager.PathFinder.GetNode(TemportalPosition).Blocked = !enabled;
		ScenarioManager.PathFinder.GetNode(TemportalPosition).Walkable = enabled;
	}

	private void UpdateWinObjective()
	{
		_winObjective.ScriptedIsComplete = ScenarioScript._css.Players.Any((PlayerState p) => !p.IsDead && !p.Player.IsDead && p.Player.ArrayIndex == TemportalPosition);
		_winObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_roomsRevealRound[RoomNames.IndexOf(_tickRoomRevealed.RoomName)] == -1)
		{
			_roomsRevealRound[RoomNames.IndexOf(_tickRoomRevealed.RoomName)] = ScenarioScript.RoundNumber;
			InitIceClusters(_tickRoomRevealed.RoomName);
			UpdatePathfinding();
		}
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_roomsRevealRound);
		string text2 = JsonConvert.SerializeObject(new string[1] { text });
		Trace("[ffa000]Serialize: " + text2 + "[-]", null, 171);
		return text2;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 176);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_roomsRevealRound = JsonConvert.DeserializeObject<int[]>(array[0]);
		}
		UpdatePathfinding();
		RecalculateTileHighlights();
	}

	private void RecalculateTileHighlights()
	{
		List<Point> list = new List<Point>();
		foreach (List<Point[]> value in ROOMS_ICE_CLUSTER_PATHS.Values)
		{
			foreach (Point[] item in value)
			{
				list.AddRange(item);
			}
		}
		ScenarioManager.ToggleTileHighlights(list, enabled: false);
		List<Point> list2 = new List<Point>();
		for (int i = 0; i < RoomNames.Count; i++)
		{
			if (_roomsRevealRound[i] == -1)
			{
				continue;
			}
			int num = ScenarioScript.RoundNumber - _roomsRevealRound[i] + 1;
			foreach (Point[] item2 in ROOMS_ICE_CLUSTER_PATHS[RoomNames[i]])
			{
				if (num < item2.Length)
				{
					list2.Add(item2[num]);
				}
			}
		}
		if (list2.Count > 0)
		{
			ScenarioManager.ToggleTileHighlights(list2, enabled: true);
		}
	}

	private void InitIceClusters(string roomName)
	{
		List<Point> list = new List<Point>();
		foreach (Point[] item in ROOMS_ICE_CLUSTER_PATHS[roomName])
		{
			CTile tile = ScenarioManager.GetTile(item[0]);
			PlaceIceCluster(tile, "IcyRootsHazardous", "FH_124_Crystalline_Roots_Hazardous_D", replaceExisting: false);
			list.Add(item[1]);
			ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).IsNegativeHex = true;
		}
		ScenarioManager.ToggleTileHighlights(list, enabled: true);
	}

	private void UpdatePathfinding()
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile == null)
			{
				continue;
			}
			CObjectProp cObjectProp = cTile.FindProp(ScenarioManager.ObjectImportType.VFXProp);
			if (cObjectProp != null)
			{
				CNode node = ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex);
				switch (cObjectProp.PrefabName)
				{
				case "IcyRootsObstacle":
					node.IsDifficultTerrain = false;
					node.Blocked = true;
					node.IsNegativeHex = false;
					cTile.TreatAsHazardous = false;
					cTile.TreatAsDifficult = false;
					break;
				case "IcyRootsDifficultB":
					node.IsDifficultTerrain = true;
					node.Blocked = false;
					node.IsNegativeHex = true;
					cTile.TreatAsHazardous = true;
					cTile.TreatAsDifficult = true;
					break;
				case "IcyRootsHazardous":
					node.IsDifficultTerrain = false;
					node.Blocked = false;
					node.IsNegativeHex = true;
					cTile.TreatAsHazardous = true;
					cTile.TreatAsDifficult = false;
					break;
				case "IcyRootsDifficultA":
					node.IsDifficultTerrain = true;
					node.Blocked = false;
					node.IsNegativeHex = false;
					cTile.TreatAsHazardous = false;
					cTile.TreatAsDifficult = true;
					break;
				}
			}
		}
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		foreach (CObjectProp prop in tile.m_Props)
		{
			if (prop.PrefabName == "IcyRootsObstacle" || prop.PrefabName == "IcyRootsDifficultB" || prop.PrefabName == "IcyRootsHazardous" || prop.PrefabName == "IcyRootsDifficultA")
			{
				return true;
			}
		}
		return base.TreatTileAsNonFeatureless(tile);
	}

	private void UpdateIceClusterProps(CMap room)
	{
		List<Point[]> list = ROOMS_ICE_CLUSTER_PATHS[room.RoomName];
		Dictionary<CTile, TileIceClusterSummary> dictionary = new Dictionary<CTile, TileIceClusterSummary>();
		int num = _roomsRevealRound[RoomNames.IndexOf(room.RoomName)];
		List<Point> tilesToHighlight = new List<Point>();
		foreach (Point[] item2 in list)
		{
			for (int i = 0; i < item2.Length; i++)
			{
				CTile tile = ScenarioManager.GetTile(item2[i]);
				dictionary.TryGetValue(tile, out var value);
				int num2 = ScenarioScript.RoundNumber - num + 1;
				if (i < num2 - 1)
				{
					value.IsObstacle = true;
				}
				else if (i == num2 - 1)
				{
					value.IsDifficult = true;
				}
				else if (i == num2)
				{
					value.IsHazardous = true;
				}
				else if (i == num2 + 1)
				{
					tilesToHighlight.Add(item2[i]);
				}
				dictionary[tile] = value;
			}
		}
		List<CActor> list2 = new List<CActor>();
		List<CActor> list3 = new List<CActor>();
		foreach (KeyValuePair<CTile, TileIceClusterSummary> item3 in dictionary)
		{
			CTile key = item3.Key;
			TileIceClusterSummary value2 = item3.Value;
			ScenarioManager.PathFinder.GetNode(key.m_ArrayIndex);
			if (value2.IsObstacle)
			{
				PlaceIceCluster(key, "IcyRootsObstacle", "");
			}
			else if (value2.IsDifficult && value2.IsHazardous)
			{
				PlaceIceCluster(key, "IcyRootsDifficultB", "FH_124_Crystalline_Roots_Hazardous_and_Difficult_D");
			}
			else if (value2.IsHazardous)
			{
				PlaceIceCluster(key, "IcyRootsHazardous", "FH_124_Crystalline_Roots_Hazardous_D");
			}
			else if (value2.IsDifficult)
			{
				PlaceIceCluster(key, "IcyRootsDifficultA", "FH_124_Crystalline_Roots_Difficult_D");
			}
			foreach (CActor livingActor in key.LivingActors)
			{
				if (value2.IsObstacle)
				{
					list3.Add(livingActor);
				}
				else if (value2.IsHazardous)
				{
					list2.Add(livingActor);
				}
			}
		}
		UpdatePathfinding();
		foreach (CActor item4 in list2)
		{
			TryDealHazardousDamage(item4, "IcyRootsHazardous");
		}
		foreach (CActor item5 in list3)
		{
			TryDealHazardousDamage(item5, "IcyRootsObstacle");
			TryMoveAwayActor(item5);
		}
		List<Point> list4 = dictionary.Keys.ToList().ConvertAll((CTile t) => t.m_ArrayIndex);
		list4.RemoveAll((Point tp) => tilesToHighlight.Contains(tp));
		ScenarioManager.ToggleTileHighlights(list4, enabled: false);
		ScenarioManager.ToggleTileHighlights(tilesToHighlight, enabled: true);
	}

	private void PlaceIceCluster(CTile tile, string prefabName, string description, bool replaceExisting = true)
	{
		CObjectProp cObjectProp = tile.FindProp(ScenarioManager.ObjectImportType.VFXProp);
		if ((!replaceExisting && cObjectProp != null) || (cObjectProp != null && cObjectProp.PrefabName == prefabName))
		{
			return;
		}
		if (replaceExisting)
		{
			CObjectProp[] array = tile.m_Props.ToArray();
			foreach (CObjectProp cObjectProp2 in array)
			{
				if (!(cObjectProp2 is CObjectShadow) && cObjectProp2.ObjectType != ScenarioManager.ObjectImportType.MoneyToken)
				{
					cObjectProp2.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
				}
			}
		}
		cObjectProp?.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = "IceDestructionGeneric",
			m_ArrayIndex = tile.m_ArrayIndex
		});
		CVector3 rotation = new CVector3(0f, 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360), 0f);
		CObjectProp cObjectProp3 = new CObjectProp(prefabName, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(tile.m_ArrayIndex), null, rotation, null, tile.m_HexMap.MapGuid);
		cObjectProp3.PropCustomLocKey = prefabName;
		cObjectProp3.PropCustomDescriptionLocKey = description;
		tile.SpawnProp(cObjectProp3);
	}

	private void TryDealHazardousDamage(CActor actor, string damageSourceLocKey)
	{
		if (!actor.FlyingAfterConditions)
		{
			DealScenarioDamage(actor, ScenarioManager.HazardousTerrainDamage, isTrapDamage: false, isTerrainDamage: false, damageSourceLocKey);
		}
	}

	private void TryMoveAwayActor(CActor actor)
	{
		if (actor.FlyingAfterConditions)
		{
			return;
		}
		if (GetNearestFreeTile(actor.Tile.m_ArrayIndex.X, actor.Tile.m_ArrayIndex.Y, out var nearestTile))
		{
			if (ScenarioManager.IsTileAdjacent(nearestTile, actor.Tile))
			{
				CustomAbility.MoveActorInDirection(actor, nearestTile);
			}
			else
			{
				TeleportActor(actor, nearestTile);
			}
		}
		else
		{
			ScenarioScript.current.Trace("TryMoveAwayActor: Can't find  nearest free tile for actor $" + actor.ActorGuid + ".", null, 390);
		}
	}
}
