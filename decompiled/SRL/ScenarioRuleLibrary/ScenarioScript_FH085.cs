using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH085 : ScenarioScript
{
	[Serializable]
	public class Storage85
	{
		public bool Tiles5HTeleportEnabled;

		public bool Tiles6ITeleportEnabled;

		public bool Tiles7JTeleportEnabled;

		public bool Tiles8KTeleportEnabled;

		public bool TileFLooted;

		public bool TileBPropHealthAdded;

		public bool TileBPropKilled;

		public bool TileATriggered;

		public bool TileGTriggered;

		public bool TilesEMTeleportEnabled;

		public bool TileDTurnedEscape;

		public bool Goal3Spawned;

		public List<string> PlayersWhoHaveActivatedTileG = new List<string>();
	}

	private CScenarioModifier TileAModifier;

	private CTile Tile1;

	private CTile Tile2;

	private CMap Room5A;

	private CTile Tile10;

	private CObjectDoor Tile10Door;

	private List<Point> BlockedTilesRoom5A = new List<Point>
	{
		new Point(9, 5),
		new Point(8, 5),
		new Point(9, 6)
	};

	private CTile Goal1Tile;

	private CTile Tile9;

	private CObjectDoor Tile9Door;

	private CMap Room2J;

	private CTile Tile12;

	private CObjectDoor Tile12Door;

	private CMap Room7C;

	private CTile TileB;

	[StateDump(Skip = true)]
	private CObjectObstacle TileBProp;

	private CTile Goal3Tile;

	private CTile Tile11;

	private CObjectDoor Tile11Door;

	private CTile TileH;

	private CTile Tile5;

	private List<Point> BlockedTilesRoom7C = new List<Point>
	{
		new Point(9, 8),
		new Point(8, 8),
		new Point(9, 9),
		new Point(8, 9),
		new Point(9, 10),
		new Point(8, 10)
	};

	private CMap Room2L;

	private CTile Tile3;

	private CTile Tile7;

	private CTile TileJ;

	private CMap Room7A;

	private CTile TileC;

	private CObjectDoor TileCDoor;

	private CTile TileM;

	private CTile TileD;

	private CMap Room2E;

	private CTile Tile6;

	private CTile TileI;

	private CTile TileF;

	private CTile Tile4;

	private CMap Room1F;

	private CTile Goal4Tile;

	private CScenarioModifier Room4ADiscardModifier;

	private CMap Room4A;

	private CTile Tile8;

	private CTile TileK;

	private CTile Goal2Tile;

	private CScenarioModifier TileGModifier;

	private CMap Room1G;

	private CTile TileE;

	private CObjective_ActorsEscaped EscapeObjective;

	private Storage85 serializedProperties;

	public CTile TileA { get; private set; }

	public bool TileATriggered
	{
		get
		{
			return serializedProperties.TileATriggered;
		}
		private set
		{
			serializedProperties.TileATriggered = value;
		}
	}

	private bool Goal3Spawned
	{
		get
		{
			return serializedProperties.Goal3Spawned;
		}
		set
		{
			serializedProperties.Goal3Spawned = value;
		}
	}

	private bool Tiles5HTeleportEnabled
	{
		get
		{
			return serializedProperties.Tiles5HTeleportEnabled;
		}
		set
		{
			serializedProperties.Tiles5HTeleportEnabled = value;
		}
	}

	private bool TileBPropHealthAdded
	{
		get
		{
			return serializedProperties.TileBPropHealthAdded;
		}
		set
		{
			serializedProperties.TileBPropHealthAdded = value;
		}
	}

	private bool TileBPropKilled
	{
		get
		{
			return serializedProperties.TileBPropKilled;
		}
		set
		{
			serializedProperties.TileBPropKilled = value;
		}
	}

	private bool Tiles7JTeleportEnabled
	{
		get
		{
			return serializedProperties.Tiles7JTeleportEnabled;
		}
		set
		{
			serializedProperties.Tiles7JTeleportEnabled = value;
		}
	}

	private bool TileDTurnedEscape
	{
		get
		{
			return serializedProperties.TileDTurnedEscape;
		}
		set
		{
			serializedProperties.TileDTurnedEscape = value;
		}
	}

	private bool TilesEMTeleportEnabled
	{
		get
		{
			return serializedProperties.TilesEMTeleportEnabled;
		}
		set
		{
			serializedProperties.TilesEMTeleportEnabled = value;
		}
	}

	private bool Tiles6ITeleportEnabled
	{
		get
		{
			return serializedProperties.Tiles6ITeleportEnabled;
		}
		set
		{
			serializedProperties.Tiles6ITeleportEnabled = value;
		}
	}

	private bool TileFLooted
	{
		get
		{
			return serializedProperties.TileFLooted;
		}
		set
		{
			serializedProperties.TileFLooted = value;
		}
	}

	private bool Tiles8KTeleportEnabled
	{
		get
		{
			return serializedProperties.Tiles8KTeleportEnabled;
		}
		set
		{
			serializedProperties.Tiles8KTeleportEnabled = value;
		}
	}

	public CTile TileG { get; private set; }

	public bool TileGTriggered
	{
		get
		{
			return serializedProperties.TileGTriggered;
		}
		private set
		{
			serializedProperties.TileGTriggered = value;
		}
	}

	private List<string> PlayersWhoHaveActivatedTileG => serializedProperties.PlayersWhoHaveActivatedTileG;

	private bool Tile9IsValidToOpenFromRoom2J
	{
		get
		{
			if (Room2J != null && Room2J.Revealed && Tile9 != null && Tile9Door != null)
			{
				return !Tile9Door.DoorIsOpen;
			}
			return false;
		}
	}

	private bool Tile9IsValidToOpenFromRoom2L
	{
		get
		{
			if (Room2L != null && Room2L.Revealed && Tile9 != null && Tile9Door != null)
			{
				return !Tile9Door.DoorIsOpen;
			}
			return false;
		}
	}

	private bool Tile11IsValidToOpenFromRoom7C
	{
		get
		{
			if (Room7C != null && Room7C.Revealed && Tile11 != null && Tile11Door != null)
			{
				return !Tile11Door.DoorIsOpen;
			}
			return false;
		}
	}

	private bool Tile11IsValidToOpenFromRoom7A
	{
		get
		{
			if (Room7A != null && Room7A.Revealed && Tile11 != null && Tile11Door != null)
			{
				return !Tile11Door.DoorIsOpen;
			}
			return false;
		}
	}

	private bool Tile5IsValidToOpenTeleport
	{
		get
		{
			if (Room7C != null)
			{
				return Room7C.Revealed;
			}
			return false;
		}
	}

	private bool Tile7IsValidToOpenTeleport
	{
		get
		{
			if (Room2L != null)
			{
				return Room2L.Revealed;
			}
			return false;
		}
	}

	private bool Tile6IsValidToOpenTeleport
	{
		get
		{
			if (Room2E != null)
			{
				return Room2E.Revealed;
			}
			return false;
		}
	}

	private bool Tile8IsValidToOpenTeleport
	{
		get
		{
			if (Room4A != null)
			{
				return Room4A.Revealed;
			}
			return false;
		}
	}

	private bool TileEIsValidToOpenTeleport
	{
		get
		{
			if (Room1G != null)
			{
				return Room1G.Revealed;
			}
			return false;
		}
	}

	private bool TileBIsValidToGiveHealth
	{
		get
		{
			if (Room7C != null && Room7C.Revealed)
			{
				return !TileBPropHealthAdded;
			}
			return false;
		}
	}

	private bool TileFIsValidToLoot
	{
		get
		{
			if (Room2E != null && Room2E.Revealed)
			{
				return !TileFLooted;
			}
			return false;
		}
	}

	public bool TileGStillUsable
	{
		get
		{
			if (Room4A != null && Room4A.Revealed)
			{
				return PlayersWhoHaveActivatedTileG.Count < ScenarioManager.Scenario.AllPlayers.Count;
			}
			return false;
		}
	}

	public bool TileGStillUsableForCurrentCharacter
	{
		get
		{
			if (TileGStillUsable)
			{
				if (GameState.CurrentActor != null)
				{
					return !PlayersWhoHaveActivatedTileG.Contains(GameState.CurrentActor.ActorGuid);
				}
				return true;
			}
			return false;
		}
	}

	private bool TileCIsValidToOpenFromRoom1F
	{
		get
		{
			if (Room1F != null && Room1F.Revealed && TileC != null && TileCDoor != null)
			{
				return !TileCDoor.DoorIsOpen;
			}
			return false;
		}
	}

	private bool Tile10IsValidToOpen
	{
		get
		{
			if (Tiles8KTeleportEnabled)
			{
				return !Tile10Door.DoorIsOpen;
			}
			return false;
		}
	}

	private bool Tile12IsValidToOpen
	{
		get
		{
			if (!Tile12Door.DoorIsOpen)
			{
				if (!TileBPropKilled)
				{
					if (Tile10Door != null)
					{
						return Tile10Door.DoorIsOpen;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	private CPlayerActor playerActor => _tickActor as CPlayerActor;

	protected override bool Initialize()
	{
		EscapeObjective = ScenarioManager.CurrentScenarioState.AllObjectives.SingleOrDefault((CObjective x) => x.EventIdentifier == "escape_objective") as CObjective_ActorsEscaped;
		TileA = ScenarioManager.GetTile(10, 4);
		Tile1 = ScenarioManager.GetTile(8, 2);
		Tile2 = ScenarioManager.GetTile(11, 7);
		Room5A = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_5_A");
		Tile10 = ScenarioManager.GetTile(8, 7);
		Tile10Door = Tile10?.FindDoor();
		TileAModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.SingleOrDefault((CScenarioModifier x) => x.EventIdentifier == "tile_a_modifier");
		Tile12 = ScenarioManager.GetTile(8, 11);
		Tile12Door = Tile12?.FindDoor();
		TileB = ScenarioManager.GetTile(10, 9);
		TileBProp = TileB?.FindPropOfType<CObjectObstacle>();
		Goal3Tile = ScenarioManager.GetTile(8, 9);
		Goal1Tile = ScenarioManager.GetTile(5, 2);
		Tile9 = ScenarioManager.GetTile(5, 6);
		Tile9Door = Tile9?.FindDoor();
		Room2J = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_2_J");
		Room2L = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_2_L");
		Tile11 = ScenarioManager.GetTile(11, 11);
		Tile11Door = Tile11?.FindDoor();
		Room7C = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_7_C");
		Room7A = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_7_A");
		TileM = ScenarioManager.GetTile(7, 13);
		TileD = ScenarioManager.GetTile(5, 14);
		TileC = ScenarioManager.GetTile(12, 13);
		TileCDoor = TileC?.FindDoor();
		Tile3 = ScenarioManager.GetTile(5, 11);
		Tile5 = ScenarioManager.GetTile(13, 10);
		Room2E = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_2_E");
		TileH = ScenarioManager.GetTile(8, 19);
		Tile6 = ScenarioManager.GetTile(10, 22);
		Room1F = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_1_F");
		TileI = ScenarioManager.GetTile(16, 14);
		Tile4 = ScenarioManager.GetTile(6, 21);
		Room1G = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_1_G");
		TileE = ScenarioManager.GetTile(2, 21);
		Goal4Tile = ScenarioManager.GetTile(13, 14);
		TileF = ScenarioManager.GetTile(10, 20);
		Tile7 = ScenarioManager.GetTile(6, 7);
		Room4A = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "Room_4_A");
		Room4ADiscardModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.SingleOrDefault((CScenarioModifier x) => x.EventIdentifier == "room_4A_discard");
		TileJ = ScenarioManager.GetTile(15, 22);
		if (Room4A != null)
		{
			Room4A.TreatAsWater = true;
			foreach (CMapTile mapTile in Room4A.MapTiles)
			{
				TileIndex arrayIndex = mapTile.ArrayIndex;
				ScenarioManager.PathFinder.Nodes[arrayIndex.X, arrayIndex.Y].IsDifficultTerrain = true;
				ScenarioManager.PathFinder.Nodes[arrayIndex.X, arrayIndex.Y].IsWaterTerrain = true;
			}
		}
		Tile8 = ScenarioManager.GetTile(18, 20);
		TileK = ScenarioManager.GetTile(9, 5);
		Goal2Tile = ScenarioManager.GetTile(8, 5);
		TileG = ScenarioManager.GetTile(16, 21);
		TileGModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.SingleOrDefault((CScenarioModifier x) => x.EventIdentifier == "tile_g_modifier");
		serializedProperties = new Storage85();
		RefreshRoom4AModifier();
		RefreshTileAModifier();
		RefreshTileGModifier();
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PropType == EPropType.GoalChest))
		{
			item.PropCustomLocKey = "FH/LootableCorpse_DESCR_TOOLTIP";
		}
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			RefreshRoom5AVisibility();
			RefreshRoom7CVisibility();
			RecalcTileHighlights();
			return;
		}
		serializedProperties = (Storage85)JsonConvert.DeserializeObject(storage, typeof(Storage85));
		if (Tiles5HTeleportEnabled)
		{
			LinkTeleportTiles(Tile5, TileH);
		}
		if (Tiles6ITeleportEnabled)
		{
			LinkTeleportTiles(Tile6, TileI);
		}
		if (Tiles7JTeleportEnabled)
		{
			LinkTeleportTiles(Tile7, TileJ);
		}
		if (Tile8.m_HexMap.Revealed)
		{
			LinkTeleportTiles(Tile8, TileK);
		}
		RefreshEscapeHex();
		RefreshRoom4AModifier();
		RefreshTileAModifier();
		RefreshTileGModifier();
		RefreshRoom5AVisibility();
		RefreshRoom7CVisibility();
		RecalcTileHighlights();
		RefreshTileEVisuals();
	}

	private void RefreshRoom5AVisibility()
	{
		bool tiles8KTeleportEnabled = Tiles8KTeleportEnabled;
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp x) => x.ArrayIndex.ToPoint() == TileK.m_ArrayIndex && x.PropType == EPropType.TerrainWater);
		if (tiles8KTeleportEnabled)
		{
			if (cObjectProp == null)
			{
				cObjectProp = CAbilityCreateTerrain.SpawnTerrain(ScenarioManager.ObjectImportType.TerrainWater, TileK);
				cObjectProp.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
			}
			ToggleTilesVisible(val: true, BlockedTilesRoom5A);
		}
		else
		{
			cObjectProp?.DestroyProp();
			ToggleTilesVisible(val: false, BlockedTilesRoom5A);
		}
		RecalcTileHighlights();
	}

	private void RefreshRoom7CVisibility()
	{
		bool goal3Spawned = Goal3Spawned;
		ToggleTilesVisible(goal3Spawned, BlockedTilesRoom7C);
	}

	private void ToggleTilesVisible(bool val, List<Point> points)
	{
		foreach (Point point in points)
		{
			CTile tile = ScenarioManager.GetTile(point);
			CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
			node.Walkable = val;
			node.Blocked = !val;
		}
	}

	private IEnumerable<CTile> GetTilesInRange(CTile origin, int range, bool includeUnrevealed = true)
	{
		if (!origin.IsBlocked && origin.IsWalkable)
		{
			yield return origin;
		}
		foreach (CTile item in GameState.GetTilesInRange(origin.m_ArrayIndex, range))
		{
			if (!item.IsBlocked && item.IsWalkable && (includeUnrevealed || item.m_HexMap.Revealed))
			{
				yield return item;
			}
		}
	}

	private void CalcVisibleTiles(out List<(Point, CVector4)> toShow, out List<(Point, CVector4)> toHide)
	{
		List<(Point, CVector4)> list = new List<(Point, CVector4)>();
		List<(Point, CVector4)> list2 = new List<(Point, CVector4)>();
		AddVisibilityTiles(list, list2, TileA, 1, visible: true);
		AddVisibilityTiles(list, list2, Tile9, 1, Tile9IsValidToOpenFromRoom2J);
		AddVisibilityTiles(list, list2, Tile11, 1, Tile11IsValidToOpenFromRoom7C);
		AddVisibilityTiles(list, list2, Tile11, 1, Tile11IsValidToOpenFromRoom7A);
		AddVisibilityTiles(list, list2, TileB, 1, TileBIsValidToGiveHealth);
		AddVisibilityTiles(list, list2, Tile5, 0, Tile5IsValidToOpenTeleport);
		AddVisibilityTiles(list, list2, Tile7, 0, Tile7IsValidToOpenTeleport);
		AddVisibilityTiles(list, list2, Tile9, 1, Tile9IsValidToOpenFromRoom2L);
		AddVisibilityTiles(list, list2, Tile6, 0, Tile6IsValidToOpenTeleport);
		AddVisibilityTiles(list, list2, TileF, 1, TileFIsValidToLoot, null, new CVector4(1f, 0.5f, 0.2f, 1f));
		AddVisibilityTiles(list, list2, TileC, 1, TileCIsValidToOpenFromRoom1F, Room1F);
		AddVisibilityTiles(list, list2, Tile8, 0, Tile8IsValidToOpenTeleport);
		AddVisibilityTiles(list, list2, TileG, 1, TileGStillUsableForCurrentCharacter, null, new CVector4(1f, 0.5f, 0.2f, 1f));
		AddVisibilityTiles(list, list2, TileE, 1, TileEIsValidToOpenTeleport && !TilesEMTeleportEnabled);
		AddVisibilityTiles(list, list2, TileE, 0, TileEIsValidToOpenTeleport);
		AddVisibilityTiles(list, list2, TileD, 1, TilesEMTeleportEnabled && !TileDTurnedEscape);
		AddVisibilityTiles(list, list2, TileD, 0, TileDTurnedEscape);
		AddVisibilityTiles(list, list2, Tile10, 1, Tile10IsValidToOpen);
		AddVisibilityTiles(list, list2, Tile12, 1, Tile12IsValidToOpen);
		toShow = list2.Distinct().ToList();
		toHide = list.Distinct().Except(toShow).ToList();
	}

	private void AddVisibilityTiles(List<(Point, CVector4)> allPoints, List<(Point, CVector4)> visibleTiles, CTile tile, int range, bool visible, CMap roomFilter = null, CVector4 color = null)
	{
		if (tile == null)
		{
			return;
		}
		allPoints.AddRange(from t in GetTilesInRange(tile, range)
			select (pos: t.m_ArrayIndex, color: color));
		if (!visible)
		{
			return;
		}
		foreach (CTile item in GetTilesInRange(tile, range, includeUnrevealed: false))
		{
			if (roomFilter == null || item.m_HexMap == roomFilter || item.m_Hex2Map == roomFilter)
			{
				visibleTiles.Add((item.m_ArrayIndex, color));
			}
		}
	}

	private void RecalcTileHighlights()
	{
		CalcVisibleTiles(out var toShow, out var toHide);
		IEnumerable<IGrouping<CVector4, (Point, CVector4)>> enumerable = from x in toHide
			group x by x.Item2;
		IEnumerable<IGrouping<CVector4, (Point, CVector4)>> enumerable2 = from x in toShow
			group x by x.Item2;
		foreach (IGrouping<CVector4, (Point, CVector4)> item in enumerable)
		{
			ScenarioManager.ToggleTileHighlights(item.Select(((Point, CVector4) x) => x.Item1), enabled: false, item.Key);
		}
		foreach (IGrouping<CVector4, (Point, CVector4)> item2 in enumerable2)
		{
			ScenarioManager.ToggleTileHighlights(item2.Select(((Point, CVector4) x) => x.Item1), enabled: true, item2.Key);
		}
	}

	private void RefreshTileEVisuals()
	{
		if (TilesEMTeleportEnabled)
		{
			CObjectProp prop = TileE.m_Props.Find((CObjectProp p) => p.PropGuid == "b3fdd2d4-566e-356c-6b40-740f18141102");
			CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
			cChangePropVisualState_MessageData.m_prop = prop;
			cChangePropVisualState_MessageData.m_stateName = "State1";
			cChangePropVisualState_MessageData.m_delay = 0.001f;
			ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
		}
	}

	private void LinkTeleportTiles(CTile tileA, CTile tileB)
	{
		CNode node = ScenarioManager.PathFinder.GetNode(tileA.m_ArrayIndex);
		CNode node2 = ScenarioManager.PathFinder.GetNode(tileB.m_ArrayIndex);
		CNavLink item = new CNavLink(1, node2);
		if (!node.NavLinks.Contains(item))
		{
			node.NavLinks.Add(item);
		}
		item = new CNavLink(1, node);
		if (!node2.NavLinks.Contains(item))
		{
			node2.NavLinks.Add(item);
		}
		ToggleTeleportTilesHighlights();
	}

	private void ToggleTeleportTilesHighlights()
	{
		ToggleTeleportTilesHighlights(new List<CTile> { Tile5, Tile6, Tile7, Tile8 });
	}

	private void ToggleTeleportTilesHighlights(List<CTile> tiles)
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		List<Point> list2 = new List<Point>();
		foreach (CTile tile in tiles)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
			if (node.NavLinks == null || node.NavLinks.Count <= 0)
			{
				continue;
			}
			list2.Add(tile.m_ArrayIndex);
			foreach (CNavLink navLink in node.NavLinks)
			{
				list2.Add(navLink.DestinationNode.Location);
				list.Add((tile.m_ArrayIndex, navLink.DestinationNode.Location));
			}
		}
		ScenarioManager.ToggleTileHighlights(list2, enabled: true);
		ScenarioManager.ToggleTileLinks(list, enabled: true);
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			RecalcTileHighlights();
			break;
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor is CObjectActor cObjectActor && cObjectActor.AttachedProp == TileBProp)
		{
			if (!Goal3Spawned)
			{
				GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_7C_Tile_B_PropCleared");
				SpawnGoal3();
			}
			TileBPropKilled = true;
		}
		RecalcTileHighlights();
	}

	private void SpawnGoal3()
	{
		if (!Goal3Spawned)
		{
			Goal3Spawned = true;
			SpawnGoalProp(Goal3Tile);
			RefreshRoom7CVisibility();
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		CPlayerActor actor = playerActor;
		CAbility tickAbility = _tickAbility;
		CheckTile5(actor);
		CheckTile6(actor);
		CheckTile7(actor);
		CheckTile8(actor);
		CheckTileE(actor);
		if (TickAbilityIsAForcedMoveAbility(tickAbility))
		{
			OnActorFinishedMovedOrWasMoved(actor);
		}
		RecalcTileHighlights();
	}

	private void OnActorFinishedMovedOrWasMoved(CPlayerActor actor)
	{
		CheckTileAAdjacency(actor);
		CheckTileBAdjacency(actor);
		CheckTile9Adjacency(actor);
		CheckTile11Adjacency(actor);
		CheckTileCAdjacency(actor);
		CheckTileFAdjacencyOrOccupancy(actor);
		CheckTileGAdjacencyOrOccupancy(actor);
		CheckTileDAdjacencyOrOccupancy(actor);
		CheckTile10Adjacency(actor);
		CheckTile12Adjacency(actor);
	}

	private void SpawnGoalProp(CTile tile)
	{
		if (tile != null)
		{
			CObjectChest cObjectChest = new CObjectChest(ESpecificPropType.LootableCorpse.ToString(), ScenarioManager.ObjectImportType.GoalChest, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
			cObjectChest.PropCustomLocKey = "FH/LootableCorpse_DESCR_TOOLTIP";
			cObjectChest.PropCustomDescriptionLocKey = "FH_85_Lootable_Corpse_D";
			tile.SpawnProp(cObjectChest);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		CPlayerActor actor = playerActor;
		CAbility tickAbility = _tickAbility;
		CheckLootedGoalTilesFromAbility(tickAbility);
		if (TickAbilityIsAMoveTypeAbility(tickAbility))
		{
			OnActorFinishedMovedOrWasMoved(actor);
		}
		RecalcTileHighlights();
	}

	private bool CheckIfTeleportLinkShouldActivate(CPlayerActor actor, CTile tileA, CTile tileB, CMap revealRoom, string campaignSection, bool alreadyDone)
	{
		if (actor != null && !alreadyDone && tileA != null && tileB != null && actor.GetDistanceTo(tileA) == 0)
		{
			if (revealRoom != null)
			{
				revealRoom.Reveal(initial: false);
				CRevealRooms_MessageData message = new CRevealRooms_MessageData();
				ScenarioRuleClient.MessageHandler(message);
			}
			LinkTeleportTiles(tileA, tileB);
			GameState.TriggerCampaignSection(campaignSection);
			return true;
		}
		return false;
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		CheckDoor1(door.PropTile);
		CheckDoor2(door.PropTile);
		CheckDoor3(door.PropTile);
		CheckDoor4(door.PropTile);
	}

	private void CheckTile5(CPlayerActor actor)
	{
		if (CheckIfTeleportLinkShouldActivate(actor, Tile5, TileH, Room2E, "FS_085_Deadly_Pastimes_Special_7C_Tile_5", Tiles5HTeleportEnabled))
		{
			Tiles5HTeleportEnabled = true;
		}
	}

	private void CheckTile6(CPlayerActor actor)
	{
		if (CheckIfTeleportLinkShouldActivate(actor, Tile6, TileI, Room1F, "FS_085_Deadly_Pastimes_Special_2E_Tile_6", Tiles6ITeleportEnabled))
		{
			Tiles6ITeleportEnabled = true;
		}
	}

	private void CheckTile7(CPlayerActor actor)
	{
		if (CheckIfTeleportLinkShouldActivate(actor, Tile7, TileJ, Room4A, "FS_085_Deadly_Pastimes_Special_2L_Tile_7", Tiles7JTeleportEnabled))
		{
			Tiles7JTeleportEnabled = true;
			RefreshRoom4AModifier();
		}
	}

	private void CheckTile8(CPlayerActor actor)
	{
		if (CheckIfTeleportLinkShouldActivate(actor, Tile8, TileK, null, "FS_085_Deadly_Pastimes_Special_4A_Tile_8", Tiles8KTeleportEnabled))
		{
			Tiles8KTeleportEnabled = true;
			SpawnGoalProp(Goal2Tile);
			RefreshRoom5AVisibility();
		}
	}

	private void CheckTileE(CPlayerActor actor)
	{
		if (actor == null || actor.Tile != TileE)
		{
			return;
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if ((allAliveActor.Type != CActor.EType.Player && allAliveActor != actor) || allAliveActor.GetDistanceTo(TileE) > 1)
			{
				continue;
			}
			if (!TilesEMTeleportEnabled)
			{
				if (Room7A != null && !Room7A.Revealed)
				{
					Room7A.Reveal(initial: false);
					CRevealRooms_MessageData message = new CRevealRooms_MessageData();
					ScenarioRuleClient.MessageHandler(message);
				}
				GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_1G_Tile_E");
				TilesEMTeleportEnabled = true;
				CObjectObstacle cObjectObstacle = TileM.FindObstacleProp();
				if (cObjectObstacle != null)
				{
					cObjectObstacle.ReleasePathfinding();
					cObjectObstacle.DestroyProp();
				}
				CAbilityCreateTerrain.SpawnTerrain(ScenarioManager.ObjectImportType.TerrainRubble, TileM);
			}
			RefreshTileEVisuals();
			CTile destTile = GameState.FindClosestEmptyTile(TileM);
			GameState.TeleportActor(allAliveActor, destTile, tryReveal: false, _tickAbility as CAbilityMove);
		}
	}

	private bool TickAbilityIsAMoveTypeAbility(CAbility ability)
	{
		if (ability == null)
		{
			return false;
		}
		if (ability.AbilityType != CAbility.EAbilityType.Move)
		{
			return ability.AbilityType == CAbility.EAbilityType.Teleport2;
		}
		return true;
	}

	private bool TickAbilityIsAForcedMoveAbility(CAbility ability)
	{
		if (ability == null)
		{
			return false;
		}
		if (ability.AbilityType != CAbility.EAbilityType.Push)
		{
			return ability.AbilityType == CAbility.EAbilityType.Pull;
		}
		return true;
	}

	private bool CheckTileAdjacencyRevealFromEitherSide(CPlayerActor actor, CTile tile, CObjectDoor door, CMap room1, CMap room2, string section)
	{
		return CheckTileAdjacencyRevealFromEitherSide(actor, tile, door, room1, room2, section, section);
	}

	private bool CheckTileAdjacencyRevealFromEitherSide(CPlayerActor actor, CTile tile, CObjectDoor door, CMap room1, CMap room2, string section1, string section2)
	{
		if (door == null || door.DoorIsOpen)
		{
			return false;
		}
		if (actor == null)
		{
			return false;
		}
		if (actor.GetDistanceTo(tile) == 1)
		{
			if (room1 != null && room2 != null)
			{
				if (actor.Tile.m_HexMap == room1 && !room2.Revealed)
				{
					GameState.TriggerCampaignSection(section1);
					CRevealRooms_MessageData message = new CRevealRooms_MessageData();
					ScenarioRuleClient.MessageHandler(message);
				}
				else if (actor.Tile.m_HexMap == room2 && !room1.Revealed)
				{
					GameState.TriggerCampaignSection(section2);
					CRevealRooms_MessageData message2 = new CRevealRooms_MessageData();
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
			else
			{
				GameState.TriggerCampaignSection(section1);
			}
			door.Activate(actor);
			return true;
		}
		return false;
	}

	private void CheckTile9Adjacency(CPlayerActor actor)
	{
		CheckTileAdjacencyRevealFromEitherSide(actor, Tile9, Tile9Door, Room2J, Room2L, "FS_085_Deadly_Pastimes_Special_2J_Tile_9", "FS_085_Deadly_Pastimes_Special_2L_Tile_9");
	}

	private void CheckTile11Adjacency(CPlayerActor actor)
	{
		CheckTileAdjacencyRevealFromEitherSide(actor, Tile11, Tile11Door, Room7C, Room7A, "FS_085_Deadly_Pastimes_Special_7C_Tile_11", "FS_085_Deadly_Pastimes_Special_7A_Tile_11");
	}

	private void CheckTileCAdjacency(CPlayerActor actor)
	{
		if (actor != null && actor.Tile.m_HexMap == Room1F && !TileCDoor.DoorIsOpen && actor.GetDistanceTo(TileC) == 1)
		{
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_7A_Or_1F_Tile_C");
			CRevealRooms_MessageData message = new CRevealRooms_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			TileCDoor.Activate(actor);
		}
	}

	private void TickPhaseEndTurn()
	{
		CPlayerActor actor = playerActor;
		TryEscapeMap();
		DiscardCardIfInRoom4A(actor);
		CheckLootedGoalTilesFromEndTurn(actor);
		RecalcTileHighlights();
	}

	private void CheckLootedGoalTilesFromAbility(CAbility ability)
	{
		if (ability is CAbilityLoot { AbilityHasHappened: not false } cAbilityLoot)
		{
			if (cAbilityLoot.TilesSelected.Contains(Goal1Tile))
			{
				OnLootGoalTileRecoverCard(ability.TargetingActor, Goal1Tile);
			}
			if (cAbilityLoot.TilesSelected.Contains(Goal2Tile))
			{
				OnLootGoalTileRecoverCard(ability.TargetingActor, Goal2Tile);
			}
			if (cAbilityLoot.TilesSelected.Contains(Goal3Tile))
			{
				OnLootGoalTileRecoverCard(ability.TargetingActor, Goal3Tile);
			}
			if (cAbilityLoot.TilesSelected.Contains(Goal4Tile))
			{
				OnLootGoalTileRecoverCard(ability.TargetingActor, Goal4Tile);
			}
		}
	}

	private void DiscardCardIfInRoom4A(CPlayerActor actor)
	{
		if (actor != null && actor.Tile.m_HexMap == Room4A && actor.CharacterClass.HandAbilityCards.Any())
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.DiscardCards, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, new AbilityData.MiscAbilityData(), skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
			cAbility.SetCanSkip(canSkip: false);
			cAbility.SetCanUndo(canUndo: false);
			PhaseManager.StartAbilities(new List<(CAbility, CActor)> { (cAbility, actor) });
		}
	}

	private void CheckLootedGoalTilesFromEndTurn(CActor actor)
	{
		if (actor != null && (actor.Tile == Goal3Tile || actor.Tile == Goal1Tile || actor.Tile == Goal4Tile || actor.Tile == Goal2Tile))
		{
			OnLootGoalTileRecoverCard(actor, actor.Tile);
		}
	}

	private void OnLootGoalTileRecoverCard(CActor actor, CTile tile)
	{
		if (tile == Goal1Tile && !ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioState.TriggeredLevelMessageInfo x) => x.MessageName.Contains("FS_085_Deadly_Pastimes_Special_Loot_Goal_1", StringComparison.InvariantCultureIgnoreCase)))
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				RestoreLostCards(1, playerActor);
			}
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_Loot_Goal_1");
		}
		else if (tile == Goal2Tile && !ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioState.TriggeredLevelMessageInfo x) => x.MessageName.Contains("FS_085_Deadly_Pastimes_Special_Loot_Goal_2", StringComparison.InvariantCultureIgnoreCase)))
		{
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				RestoreLostCards(1, playerActor2);
			}
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_Loot_Goal_2");
		}
		else if (tile == Goal3Tile && !ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioState.TriggeredLevelMessageInfo x) => x.MessageName.Contains("FS_085_Deadly_Pastimes_Special_Loot_Goal_3", StringComparison.InvariantCultureIgnoreCase)))
		{
			foreach (CPlayerActor playerActor3 in ScenarioManager.Scenario.PlayerActors)
			{
				RestoreLostCards(1, playerActor3);
			}
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_Loot_Goal_3");
		}
		else
		{
			if (tile != Goal4Tile || ScenarioManager.CurrentScenarioState.TriggeredLevelMessages.Any((ScenarioState.TriggeredLevelMessageInfo x) => x.MessageName.Contains("FS_085_Deadly_Pastimes_Special_Loot_Goal_4", StringComparison.InvariantCultureIgnoreCase)))
			{
				return;
			}
			foreach (CPlayerActor playerActor4 in ScenarioManager.Scenario.PlayerActors)
			{
				RestoreLostCards(1, playerActor4);
			}
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_Loot_Goal_4");
		}
	}

	private void RestoreLostCards(int strength, CActor targetActor)
	{
		if (targetActor?.CharacterClass != null && targetActor.CharacterClass.LostAbilityCards.Any())
		{
			CAbility item = CAbility.CreateAbility(CAbility.EAbilityType.RecoverLostCards, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: false, strength);
			PhaseManager.StartAbilities(new List<(CAbility, CActor)> { (item, targetActor) });
		}
	}

	private void CheckTileBAdjacency(CPlayerActor actor)
	{
		if (TileBIsValidToGiveHealth && actor != null && actor.GetDistanceTo(TileB) <= 1 && TileBProp != null && !TileBPropHealthAdded)
		{
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_7C_Tile_B_Adjacency");
			TileBPropHealthAdded = true;
			if (SpawnDummyActorOnProp(TileB.m_ArrayIndex.X, TileB.m_ArrayIndex.Y, TileBProp, 1, active: true))
			{
				CObjectActor runtimeAttachedActor = TileBProp.RuntimeAttachedActor;
				TileBProp.PropHealthDetails.DeathAction = PropHealthDetails.EDeathAction.Destruct;
				TileBProp.SetBlockLOS(blockLOS: false);
				CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
				{
					m_SpawnTile = TileB,
					m_SpawnEnemy = runtimeAttachedActor,
					m_ActorSpawningMessage = runtimeAttachedActor
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	private void CheckDoor1(CTile doorTile)
	{
		if (doorTile == Tile1 && _tickRoomRevealed == Room2J)
		{
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_5A_Door_1");
		}
	}

	private void CheckDoor2(CTile doorTile)
	{
		if (doorTile == Tile2 && _tickRoomRevealed == Room7C)
		{
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_5A_Door_2");
		}
	}

	private void CheckDoor3(CTile doorTile)
	{
		if (doorTile == Tile3)
		{
			if (_tickRoomRevealed == Room7A)
			{
				GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_2L_Door_3");
			}
			else if (_tickRoomRevealed == Room2L)
			{
				GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_7A_Door_3");
			}
		}
	}

	private void CheckDoor4(CTile doorTile)
	{
		if (doorTile == Tile4 && _tickRoomRevealed == Room1G)
		{
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_2E_Tile_4");
		}
	}

	private void CheckTile10Adjacency(CPlayerActor actor)
	{
		if (actor != null && Tile10IsValidToOpen)
		{
			if (Goal3Spawned)
			{
				CheckTileAdjacencyRevealFromEitherSide(actor, Tile10, Tile10Door, null, null, null);
			}
			else if (CheckTileAdjacencyRevealFromEitherSide(actor, Tile10, Tile10Door, Room5A, Room7C, "FS_085_Deadly_Pastimes_Special_5A_Tile_10_Adjacency"))
			{
				SpawnGoal3();
			}
		}
	}

	private void CheckTile12Adjacency(CPlayerActor actor)
	{
		if (actor != null && Tile12IsValidToOpen)
		{
			CheckTileAdjacencyRevealFromEitherSide(actor, Tile12, Tile12Door, null, null, "FS_085_Deadly_Pastimes_Special_7C_Tile_12_Adjacency");
		}
	}

	private void CheckTileAAdjacency(CPlayerActor actor)
	{
		if (actor != null && actor.GetDistanceTo(TileA) == 1)
		{
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_5A_Tile_A");
			actor.ApplyCondition(actor, CCondition.EPositiveCondition.Regenerate, 0, EConditionDecTrigger.WhenDamaged);
			TileATriggered = true;
			RefreshTileAModifier();
		}
	}

	private void CheckTileFAdjacencyOrOccupancy(CPlayerActor actor)
	{
		if (!TileFLooted && actor != null && actor.GetDistanceTo(TileF) <= 1)
		{
			TileFLooted = true;
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_2E_Tile_F");
			for (int i = 0; i < 2; i++)
			{
				actor.GainLoot(endOfScenario: false, incLootTokenCount: false, i == 0);
			}
			actor.ShowLastLootReward("", "MoneyToken", null, focusCamera: true);
		}
	}

	private void CheckTileDAdjacencyOrOccupancy(CPlayerActor actor)
	{
		if (!TileDTurnedEscape && TilesEMTeleportEnabled && actor != null && actor.GetDistanceTo(TileD) <= 1)
		{
			TileDTurnedEscape = true;
			GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_7A_Tile_D");
			RefreshEscapeHex();
		}
	}

	private void RefreshEscapeHex()
	{
		if (TileDTurnedEscape)
		{
			EscapeObjective.EscapePositions.Clear();
			EscapeObjective.EscapePositions.Add(new TileIndex(TileD.m_ArrayIndex));
			ScenarioRuleClient.MessageHandler(new CRefreshExitTiles_MessageData());
		}
	}

	private void RefreshRoom4AModifier()
	{
		Room4ADiscardModifier?.SetDeactivated(!Room4A.Revealed);
	}

	private void RefreshTileAModifier()
	{
		TileAModifier?.SetDeactivated(!TileATriggered);
	}

	private void RefreshTileGModifier()
	{
		TileGModifier?.SetDeactivated(!TileGStillUsable || !TileGTriggered);
	}

	private void CheckTileGAdjacencyOrOccupancy(CPlayerActor actor)
	{
		if (actor != null && !PlayersWhoHaveActivatedTileG.Contains(actor.ActorGuid) && actor.GetDistanceTo(TileG) <= 1)
		{
			if (PlayersWhoHaveActivatedTileG.Count == 0)
			{
				TileGTriggered = true;
				GameState.TriggerCampaignSection("FS_085_Deadly_Pastimes_Special_4A_Tile_G");
			}
			PlayersWhoHaveActivatedTileG.Add(actor.ActorGuid);
			RestoreLostCards(2, actor);
			RefreshTileGModifier();
		}
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		return new List<object> { TileFLooted ? 1 : 0 };
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == Tile8.m_HexMap)
		{
			LinkTeleportTiles(Tile8, TileK);
		}
	}
}
