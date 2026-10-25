using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH016 : ScenarioScript
{
	[ConsoleVariable(Alias = "ss016_debug_descent")]
	public static int DebugDescent_cvar = 0;

	[ConsoleVariable(Alias = "ss016_debug_reach_round")]
	public static int DebugReachRound_cvar = 0;

	[ConsoleVariable(Alias = "ss016_debug_party_size")]
	public static int DebugPartySize_cvar = 0;

	[ConsoleVariable(Alias = "ss016_skip_artillery_spawn")]
	public static bool SkipArtillerySpawn_cvar = false;

	[ConsoleVariable(Alias = "ss016_debug_pebbles")]
	public static int DebugPebbles_cvar = 0;

	public const int PLATFORM_FLOOR_0 = 2;

	public const int PLATFORM_FLOOR_1 = 5;

	public const int PLATFORM_FLOOR_2 = 8;

	private const string clsNormArtillery = "AncientArtilleryID";

	private const string clsEliteArtillery = "AncientArtilleryEliteID";

	private const string clsNormalBladespinner = "FlamingBladespinnerID";

	private const string clsEliteBladespinner = "FlamingBladespinnerEliteID";

	public List<CTile> tilesP0 = new List<CTile>();

	public List<CTile> tilesP1 = new List<CTile>();

	private CScenarioModifier _specialRule2;

	private int[,] _entry0 = new int[3, 2]
	{
		{ 4, 12 },
		{ 5, 12 },
		{ 6, 12 }
	};

	private int[,] _entry1 = new int[3, 2]
	{
		{ 4, 4 },
		{ 5, 4 },
		{ 6, 4 }
	};

	private List<Point> RoomAHighlightTiles = new List<Point>
	{
		new Point(4, 12),
		new Point(5, 12),
		new Point(6, 12)
	};

	private List<Point> RoomBHighlightTiles = new List<Point>
	{
		new Point(4, 4),
		new Point(5, 4),
		new Point(6, 4)
	};

	[StateDump(Skip = true)]
	private int _roundTrackPebbles = -1;

	[StateDump(Skip = true)]
	private int _roundTrackLights = -1;

	private CObjective_ReachRound _reachRoundObjective;

	private List<int> _alreadySpawnedXY = new List<int>();

	private CObjectChest _chest;

	public static readonly Point[][] MarkedTiles = new Point[8][]
	{
		new Point[2]
		{
			new Point(4, 10),
			new Point(7, 8)
		},
		new Point[1]
		{
			new Point(3, 7)
		},
		new Point[2]
		{
			new Point(4, 8),
			new Point(6, 6)
		},
		new Point[1]
		{
			new Point(5, 11)
		},
		new Point[1]
		{
			new Point(6, 10)
		},
		new Point[1]
		{
			new Point(5, 5)
		},
		new Point[2]
		{
			new Point(6, 11),
			new Point(6, 5)
		},
		new Point[2]
		{
			new Point(3, 11),
			new Point(3, 5)
		}
	};

	[StateDump(Skip = true)]
	private int _onceOnStartRound = -1;

	[StateDump(Skip = true)]
	private int _onceOnEndRound = -1;

	private int _partySize
	{
		get
		{
			if (DebugPartySize_cvar <= 1)
			{
				return ScenarioManager.CurrentScenarioState.Players.Count;
			}
			return DebugPartySize_cvar;
		}
	}

	public int elevatorFloor => Math.Clamp((DebugDescent_cvar > 0) ? (DebugDescent_cvar - 1) : (ScenarioScript._css.RoundNumber - 1), 0, 13);

	protected override bool Initialize()
	{
		foreach (CObjective winObjective in ScenarioScript._css.WinObjectives)
		{
			if (winObjective is CObjective_ReachRound reachRoundObjective)
			{
				_reachRoundObjective = reachRoundObjective;
			}
		}
		if (_reachRoundObjective == null)
		{
			Error("Needs reach round objective.", null, 101);
			return false;
		}
		Trace("Found reach round objective.", null, 105);
		_reachRoundObjective.CustomRoundCounter = 0;
		int reachRoundNumber = ((DebugReachRound_cvar > 0) ? DebugReachRound_cvar : 12);
		_reachRoundObjective.ReachRoundNumber = reachRoundNumber;
		tilesP0.Clear();
		tilesP1.Clear();
		for (int i = 12; i <= 15; i++)
		{
			for (int j = 4; j <= 8; j++)
			{
				CTile cTile = ScenarioManager.Tiles[j, i];
				if (cTile != null && ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex).Walkable)
				{
					tilesP0.Add(cTile);
				}
			}
		}
		for (int k = 1; k <= 4; k++)
		{
			for (int l = 2; l <= 6; l++)
			{
				CTile cTile2 = ScenarioManager.Tiles[l, k];
				if (cTile2 != null && ScenarioManager.PathFinder.GetNode(cTile2.m_ArrayIndex).Walkable)
				{
					tilesP1.Add(cTile2);
				}
			}
		}
		_chest = ((ScenarioScript._css.ChestProps.Count > 0) ? (ScenarioScript._css.ChestProps[0] as CObjectChest) : null);
		if (_chest == null)
		{
			List<CObjectProp> list = ScenarioScript._css.DestroyedProps.Where((CObjectProp d) => d.ObjectType == ScenarioManager.ObjectImportType.Chest || d.ObjectType == ScenarioManager.ObjectImportType.GoalChest).ToList();
			_chest = ((list.Count > 0) ? (list[0] as CObjectChest) : null);
		}
		else
		{
			Trace("Found chest " + _chest.PropGuid, null, 148);
		}
		_specialRule2 = GetScenarioModifier("specialRule2");
		return true;
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		int num = ((DebugDescent_cvar > 0) ? DebugDescent_cvar : ScenarioManager.CurrentScenarioState.RoundNumber);
		if (message.MessageName.EndsWith($"_{num}"))
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 159);
			return true;
		}
		return false;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			TickPhaseStartScenario();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 183);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
			break;
		}
	}

	private void TickPhaseStartScenario()
	{
		Trace("Destroying the chest.", null, 188);
		if (_chest != null)
		{
			if (!_chest.IsDestroyed)
			{
				_chest.DestroyProp();
			}
		}
		else
		{
			Error("Needs chest on scenario start.", null, 195);
		}
	}

	private void TickPhaseStartRound()
	{
		int num = ((DebugDescent_cvar > 0) ? DebugDescent_cvar : ScenarioScript._css.RoundNumber);
		if (_onceOnStartRound == ScenarioScript._css.RoundNumber)
		{
			return;
		}
		_onceOnStartRound = ScenarioScript._css.RoundNumber;
		UpdatePebblesAndSpawns();
		UpdateRoomLights();
		if (num > 5 && num < 8)
		{
			Trace("Can spawn artillery again on platform 0..", null, 210);
			if (AlreadyHaveSpawned(4, 14, out var i))
			{
				_alreadySpawnedXY.RemoveRange(i, 2);
			}
			if (AlreadyHaveSpawned(6, 14, out i))
			{
				_alreadySpawnedXY.RemoveRange(i, 2);
			}
		}
		UpdateEntranceBlockers(skipActors: true);
		_specialRule2.SetHiddenState(ScenarioManager.CurrentScenarioState.RoundNumber < 4);
	}

	private void TickPhaseEndRound()
	{
		int roundNumber = ScenarioManager.CurrentScenarioState.RoundNumber;
		if (_onceOnEndRound != roundNumber)
		{
			_onceOnEndRound = roundNumber;
			UpdateEntranceBlockers();
			_reachRoundObjective.CustomRoundCounter++;
			Trace($"Reach Round Objective round counter: {_reachRoundObjective.CustomRoundCounter}", null, 244);
			Trace($"Round end, current round: {ScenarioScript._css.RoundNumber}", null, 245);
			CheckObjectivesComplete();
			if (DebugDescent_cvar > 0)
			{
				DebugDescent_cvar++;
			}
			if (DebugPebbles_cvar > 0)
			{
				DebugPebbles_cvar++;
			}
		}
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_reachRoundObjective.CustomRoundCounter);
		string text2 = JsonConvert.SerializeObject(_alreadySpawnedXY);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 262);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 267);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_reachRoundObjective.CustomRoundCounter = JsonConvert.DeserializeObject<int>(array[0]);
			_alreadySpawnedXY = JsonConvert.DeserializeObject<List<int>>(array[1]);
			CheckObjectivesComplete();
		}
		UpdateEntranceBlockers(skipActors: true);
	}

	private void UnblockEntrance(int[,] xy)
	{
		Trace("Unblock entrance...", null, 280);
		for (int i = 0; i < xy.GetLength(0); i++)
		{
			int x = xy[i, 0];
			int y = xy[i, 1];
			CNode node = ScenarioManager.PathFinder.GetNode(x, y);
			CTile tile = ScenarioManager.GetTile(x, y);
			if (node != null)
			{
				node.Blocked = tile.Obstacle != null;
				node.Walkable = true;
			}
		}
	}

	private void BlockEntrance(int[,] xy)
	{
		Trace("Block entrance...", null, 294);
		for (int i = 0; i < xy.GetLength(0); i++)
		{
			int x = xy[i, 0];
			int y = xy[i, 1];
			CNode node = ScenarioManager.PathFinder.GetNode(x, y);
			if (node != null)
			{
				node.Blocked = true;
				node.Walkable = false;
			}
		}
	}

	public bool PickElevatorTileToTeleport(CActor actor, out CTile result)
	{
		int x = actor.ArrayIndex.X;
		int y = actor.ArrayIndex.Y;
		return PickElevatorTileToTeleport(x, y, out result);
	}

	public bool PickElevatorTileToTeleport(int x, int y, out CTile result)
	{
		List<CTile> list = new List<CTile>();
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile item in tiles)
		{
			if (!tilesP0.Contains(item) && !tilesP1.Contains(item))
			{
				list.Add(item);
			}
		}
		return GetNearestFreeTile(x, y, out result, list);
	}

	private void TeleportFromPlatforms(CActor actor)
	{
		if (PickElevatorTileToTeleport(actor, out var result))
		{
			TeleportActor(actor, result);
		}
		else
		{
			Error("Can't teleport " + actor.Class.ID, null, 327);
		}
	}

	private void KillMonster(CActor actor)
	{
		actor.Health = 0;
		GameState.KillActor(actor, actor, CActor.ECauseOfDeath.Damage, out var _);
	}

	private void DealTrapDamage(CActor actor, string sourceLocKey)
	{
		DealScenarioDamage(actor, ScenarioManager.TrapDamage, isTrapDamage: true, isTerrainDamage: false, sourceLocKey);
	}

	private void TryRemovePlatformProps(List<CTile> tiles)
	{
		foreach (CTile tile in tiles)
		{
			foreach (CObjectProp item in tile.m_Props.ToList())
			{
				if (!item.IsDestroyed && !item.SkipVisuals)
				{
					Trace("Removed prop " + item.PrefabName + ".", null, 346);
					item.DestroyProp();
				}
			}
		}
	}

	private void TryRemovePlatformArtillery(int platform)
	{
		switch (platform)
		{
		case 0:
			TryRemovePlatformProps(tilesP0);
			break;
		case 1:
			TryRemovePlatformProps(tilesP1);
			break;
		case 2:
			TryRemovePlatformProps(tilesP0);
			break;
		}
	}

	private bool AlreadyHaveSpawned(int x, int y, out int i)
	{
		for (i = 0; i < _alreadySpawnedXY.Count; i += 2)
		{
			if (x == _alreadySpawnedXY[i] && y == _alreadySpawnedXY[i + 1])
			{
				return true;
			}
		}
		return false;
	}

	private void TrySpawnPlatformArtillery(int platform)
	{
		string clsNorm;
		string clsElite;
		if (!SkipArtillerySpawn_cvar)
		{
			Trace($"Placing platform {platform} artillery...", null, 380);
			clsNorm = "AncientArtilleryID";
			clsElite = "AncientArtilleryEliteID";
			switch (platform)
			{
			case 0:
				placeOnPlatform2();
				break;
			case 1:
				placeOnPlatform3();
				break;
			default:
				placeOnPlatform();
				break;
			}
		}
		void place(int x, int y, string cls, out CActor artillery)
		{
			if (AlreadyHaveSpawned(x, y, out var _))
			{
				Trace($"Artillery was already spawned at {x}:{y}", null, 386);
				artillery = null;
			}
			else
			{
				artillery = ScenarioManager.Scenario.FindActorAt(x, y);
				if (artillery == null)
				{
					TrySpawnActorAtTile(x, y, cls, out artillery, addIntoInitiativeOrder: false, startRound: true);
				}
				if (artillery != null && artillery.Class.ID != clsElite && artillery.Class.ID != clsNorm)
				{
					artillery = null;
					Error($"There is something at the artillery tile at {x}:{y}", null, 401);
				}
				if (artillery != null)
				{
					Trace($"Placed {cls} at {x}:{y}", null, 405);
					_alreadySpawnedXY.Add(x);
					_alreadySpawnedXY.Add(y);
				}
			}
		}
		void placeOnPlatform()
		{
			placeOnPlatform2();
			if (_chest != null && !_chest.Activated && _chest.IsDestroyed && (ScenarioManager.CurrentScenarioState.ScenarioSetup == null || !ScenarioManager.CurrentScenarioState.ScenarioSetup.AlreadyLootedChests.Contains(_chest.Type.ToString())))
			{
				Trace("Turning on the chest...", null, 433);
				ScenarioManager.Tiles[_chest.ArrayIndex.X, _chest.ArrayIndex.Y].SpawnProp(_chest);
			}
		}
		void placeOnPlatform2()
		{
			place(4, 14, (_partySize > 3) ? clsElite : clsNorm, out var artillery);
			place(6, 14, (_partySize > 2) ? clsElite : clsNorm, out artillery);
		}
		void placeOnPlatform3()
		{
			place(5, 2, clsElite, out var artillery);
			if (_partySize > 2)
			{
				place(3, 3, (_partySize > 3) ? clsElite : clsNorm, out artillery);
			}
		}
	}

	public void UpdateEntranceBlockers(bool skipActors = false)
	{
		if (elevatorFloor - 2 <= 2 && updatePlatform(2, _entry0, 0, tilesP0) && !skipActors)
		{
			dealDamageToActorsOnPlatforms(tilesP0);
			removeActorsFromPlatform(tilesP0);
		}
		if (updatePlatform(5, _entry1, 1, tilesP1) && !skipActors)
		{
			dealDamageToActorsOnPlatforms(tilesP1);
			removeActorsFromPlatform(tilesP1);
		}
		if (elevatorFloor - 2 > 2 && updatePlatform(8, _entry0, 2, tilesP0) && !skipActors)
		{
			dealDamageToActorsOnPlatforms(tilesP0);
			removeActorsFromPlatform(tilesP0);
		}
		void dealDamageToActorsOnPlatforms(List<CTile> tiles)
		{
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				CTile item = ScenarioManager.Tiles[allAliveActor.ArrayIndex.X, allAliveActor.ArrayIndex.Y];
				if (tiles.Contains(item))
				{
					if (allAliveActor is CPlayerActor)
					{
						Trace("[fff000]Hurt actor " + allAliveActor.Class.ID + " on platform tiles[-]", null, 505);
						DealTrapDamage(allAliveActor, "FH_Scenario_Campaign_16_DAMAGE_SOURCE_PLATFORM");
					}
					else if (!(allAliveActor is CObjectActor))
					{
						Trace("[fff000]Kill actor " + allAliveActor.Class.ID + " on platform tiles[-]", null, 509);
						allAliveActor.NoGoldDrop = true;
						KillMonster(allAliveActor);
					}
				}
			}
		}
		void removeActorsFromPlatform(List<CTile> tiles)
		{
			foreach (CActor allAliveActor2 in ScenarioManager.Scenario.AllAliveActors)
			{
				CTile item = ScenarioManager.Tiles[allAliveActor2.ArrayIndex.X, allAliveActor2.ArrayIndex.Y];
				if (tiles.Contains(item))
				{
					if (allAliveActor2 is CPlayerActor)
					{
						Trace("[fff000]Teleport actor " + allAliveActor2.Class.ID + " from platform[-]", null, 486);
						TeleportFromPlatforms(allAliveActor2);
					}
					else if (!(allAliveActor2 is CObjectActor))
					{
						Trace("[fff000]Kill actor " + allAliveActor2.Class.ID + " on platform[-]", null, 490);
						allAliveActor2.NoGoldDrop = true;
						KillMonster(allAliveActor2);
					}
				}
			}
		}
		bool updatePlatform(int floor, int[,] entrance, int artillery, List<CTile> tiles)
		{
			Trace("[fff000]Update platform entrance blocking tiles[-]", null, 453);
			bool flag = false;
			floor = (skipActors ? floor : (floor - 1));
			int num = Math.Abs(floor - elevatorFloor);
			Trace($"Dist in update platform: {num}", null, 458);
			if (num > 2)
			{
				foreach (CTile tile in tiles)
				{
					ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Walkable = false;
				}
				BlockEntrance(entrance);
				TryRemovePlatformArtillery(artillery);
				return true;
			}
			foreach (CTile tile2 in tiles)
			{
				ScenarioManager.PathFinder.GetNode(tile2.m_ArrayIndex).Walkable = true;
			}
			UnblockEntrance(entrance);
			TrySpawnPlatformArtillery(artillery);
			return false;
		}
	}

	private static bool GetSelf(out ScenarioScript_FH016 script)
	{
		script = null;
		if (!ScenarioManager.CurrentScenarioState.FindScenarioScript("ScenarioScript_FH016", out var script2))
		{
			return false;
		}
		if (!(script2 is ScenarioScript_FH016 scenarioScript_FH))
		{
			return false;
		}
		script = scenarioScript_FH;
		return true;
	}

	[ConsoleCommand(Command = "ss016_update_pebbles")]
	public static void SS016UpdatePebbles(IConsole console)
	{
		if (GetSelf(out var script))
		{
			script.UpdatePebblesAndSpawns();
		}
	}

	[ConsoleCommand(Command = "ss016_update_entrance_blockers")]
	public static void SS016UpdateEntranceBlockers(IConsole console)
	{
		if (GetSelf(out var script))
		{
			script.UpdateEntranceBlockers(skipActors: true);
		}
	}

	private static void SS016UpdateEntranceBlockers_kmd(string[] argv)
	{
		SS016UpdateEntranceBlockers(null);
	}

	private static void SS016UpdatePebbles_kmd(string[] argv)
	{
		SS016UpdatePebbles(null);
	}

	private void UpdatePebblesAndSpawns()
	{
		int num = ((DebugPebbles_cvar > 0) ? DebugPebbles_cvar : ScenarioScript._css.RoundNumber);
		if (_roundTrackPebbles == num)
		{
			return;
		}
		_roundTrackPebbles = num;
		Trace($"[00f0ff]Updating pebbles; round number {num}...[-]", null, 712);
		if (num == 2)
		{
			placePebbles(97);
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
			{
				duration = 4f,
				magnitude = 0.1f,
				speed = 50f,
				distanceForce = 50f
			});
		}
		switch (num)
		{
		case 4:
		{
			removePebbles(97);
			placePebbles(98);
			placeDebris(97);
			string text = "RuinedMachineID";
			string text2 = "RuinedMachineEliteID";
			Point[] array = getMarkedTiles(104);
			CActor actor2;
			for (int i = 0; i < array.Length; i++)
			{
				Point point3 = array[i];
				string cls2 = ((_partySize >= 4) ? text2 : text);
				TrySpawnActorAtNearestTile(point3.X, point3.Y, cls2, out actor2, addIntoInitiativeOrder: false, startRound: true);
				TrySpawnActorAtNearestTile(point3.X, point3.Y, cls2, out actor2, addIntoInitiativeOrder: false, startRound: true);
			}
			array = getMarkedTiles(103);
			for (int i = 0; i < array.Length; i++)
			{
				Point point4 = array[i];
				string cls3 = ((_partySize >= 3) ? text2 : text);
				TrySpawnActorAtNearestTile(point4.X, point4.Y, cls3, out actor2, addIntoInitiativeOrder: false, startRound: true);
			}
			break;
		}
		case 7:
		{
			removePebbles(98);
			placePebbles(99);
			spawnFlamingBladespinner(98, _partySize >= 3);
			Point[] array = getMarkedTiles(103);
			CActor actor;
			for (int i = 0; i < array.Length; i++)
			{
				Point point = array[i];
				TrySpawnActorAtNearestTile(point.X, point.Y, "RuinedMachineEliteID", out actor, addIntoInitiativeOrder: false, startRound: true);
			}
			if (_partySize >= 3)
			{
				array = getMarkedTiles(104);
				for (int i = 0; i < array.Length; i++)
				{
					Point point2 = array[i];
					string cls = ((_partySize >= 4) ? "RuinedMachineEliteID" : "RuinedMachineID");
					TrySpawnActorAtNearestTile(point2.X, point2.Y, cls, out actor, addIntoInitiativeOrder: false, startRound: true);
				}
			}
			break;
		}
		case 9:
			removePebbles(99);
			placePebbles(100);
			placePebbles(101);
			placePebbles(102);
			placeDebris(99);
			break;
		case 12:
			removePebbles(100);
			removePebbles(101);
			removePebbles(102);
			placeDebris(100);
			spawnFlamingBladespinner(101, _partySize >= 4);
			spawnFlamingBladespinner(102, _partySize >= 3);
			break;
		}
		static Point[] getMarkedTiles(int ch)
		{
			int num2 = ch - 97;
			return MarkedTiles[num2];
		}
		void placeDebris(int ch)
		{
			Trace($"Trying to place debris on tiles {(char)ch}...", null, 619);
			Point[] array2 = getMarkedTiles(ch);
			List<Point> list = new List<Point>();
			Point[] array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				Point point5 = array3[j];
				CTile cTile = ScenarioManager.Tiles[point5.X, point5.Y];
				if (cTile.m_Props.Count > 0)
				{
					foreach (CObjectProp item in cTile.m_Props.ToList())
					{
						if (!(item is CObjectShadow))
						{
							item.DestroyProp();
							Trace("Destroyed prop " + item.PrefabName, null, 630);
						}
					}
				}
				TileIndex tileIndex = new TileIndex(point5);
				CObjectObstacle cObjectObstacle = new CObjectObstacle("DebrisPile", ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, null, new List<TileIndex> { tileIndex }, null, cTile.m_HexMap.MapGuid, ignoresFlyAndJump: true);
				cTile.SpawnProp(cObjectObstacle);
				Trace("Spawned prop " + cObjectObstacle.PrefabName, null, 644);
				ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
				{
					duration = 4f,
					magnitude = 0.1f,
					speed = 50f,
					distanceForce = 50f
				});
				if (ScenarioManager.Scenario.FindActorsAt(cTile.m_ArrayIndex).Count > 0)
				{
					list.Add(point5);
				}
			}
			if (list.Count > 0)
			{
				CWaitingForProlongedSpawns_MessageData message = new CWaitingForProlongedSpawns_MessageData();
				ScenarioRuleClient.MessageHandler(message);
				foreach (Point item2 in list)
				{
					pushTileActors(item2.X, item2.Y);
				}
			}
		}
		void placePebbles(int ch)
		{
			Trace($"Trying to place debris on tiles {(char)ch}...", null, 588);
			Point[] array2 = getMarkedTiles(ch);
			for (int j = 0; j < array2.Length; j++)
			{
				Point point5 = array2[j];
				CTile cTile = ScenarioManager.Tiles[point5.X, point5.Y];
				CObjectProp cObjectProp = new CObjectProp("ShowerOfPebbles", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(point5), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(cObjectProp);
				Trace("Spawned prop " + cObjectProp.PrefabName, null, 597);
			}
		}
		void pushTileActors(int x, int y, CActor self = null)
		{
			Trace($"Pushing aside actors at {x},{y}...", null, 661);
			CTile cTile = ScenarioManager.Tiles[x, y];
			if (cTile == null)
			{
				Error($"Bad tile {x}:{y}", null, 665);
				return;
			}
			if (self == null)
			{
				Trace("Self not supplied.", null, 670);
			}
			else
			{
				Trace("Self supplied: " + self.Class.ID, null, 673);
			}
			foreach (CActor item3 in ScenarioManager.Scenario.FindActorsAt(cTile.m_ArrayIndex))
			{
				Trace("Testing " + item3.Class.ID, null, 678);
				if (item3 != self)
				{
					int x2 = item3.ArrayIndex.X;
					int y2 = item3.ArrayIndex.Y;
					if (GetNearestFreeTile(x2, y2, out var nearestTile))
					{
						TeleportActor(item3, nearestTile);
						DealTrapDamage(item3, "FH_Scenario_Campaign_16_DAMAGE_SOURCE_PEBBLES_RAIN");
						Trace($"Pushed and hurt {item3.Class.ID} at {x2}:{y2}", null, 687);
					}
				}
			}
		}
		void removePebbles(int ch)
		{
			Trace($"Trying to place debris on tiles {(char)ch}...", null, 602);
			Point[] array2 = getMarkedTiles(ch);
			for (int j = 0; j < array2.Length; j++)
			{
				Point point5 = array2[j];
				CTile cTile = ScenarioManager.Tiles[point5.X, point5.Y];
				if (cTile.m_Props.Count > 0)
				{
					foreach (CObjectProp item4 in cTile.m_Props.ToList())
					{
						if (item4.PrefabName.Equals("ShowerOfPebbles", StringComparison.InvariantCultureIgnoreCase))
						{
							item4.DestroyProp();
							Trace("Destroyed prop " + item4.PrefabName, null, 612);
						}
					}
				}
			}
		}
		void spawnFlamingBladespinner(int ch, bool elite)
		{
			Trace("Spawning Flaming Blade Spinner, elite: {elite}", null, 693);
			Point[] array2 = getMarkedTiles(ch);
			foreach (Point point5 in array2)
			{
				string cls4 = (elite ? "FlamingBladespinnerEliteID" : "FlamingBladespinnerID");
				Point point6 = ScenarioScript.GetClosestEmptyTile(point5)?.m_ArrayIndex ?? point5;
				if (SpawnActorAtTile(point6.X, point6.Y, cls4, out var actor3, addIntoInitiativeOrder: false, startRound: true))
				{
					pushTileActors(point6.X, point6.Y, actor3);
				}
			}
		}
	}

	private void UpdateRoomLights()
	{
		string blueLight = "RoomBlueLight";
		string redLight = "RoomRedLight";
		List<Point> positions = new List<Point>
		{
			new Point(5, 13),
			new Point(8, 14)
		};
		List<Point> positions2 = new List<Point>
		{
			new Point(4, 3),
			new Point(2, 2)
		};
		new CVector4(1f, 0.5f, 0f, 1f);
		int num = ((DebugPebbles_cvar > 0) ? DebugPebbles_cvar : ScenarioScript._css.RoundNumber);
		if (_roundTrackLights != num)
		{
			_roundTrackLights = num;
			Trace($"[00f0ff]Updating lights; round number {num}...[-]", null, 883);
			if (num == 1)
			{
				placeLights(positions, isBlueLight: true);
			}
			if (num == 3)
			{
				removeLight(positions);
			}
			if (num == 4)
			{
				placeLights(positions, isBlueLight: false);
				placeLights(positions2, isBlueLight: true);
			}
			if (num == 6)
			{
				removeLight(positions);
				removeLight(positions2);
			}
			if (num == 7)
			{
				placeLights(positions, isBlueLight: true);
				placeLights(positions2, isBlueLight: false);
			}
			if (num == 9)
			{
				removeLight(positions);
				removeLight(positions2);
			}
			if (num == 10)
			{
				placeLights(positions, isBlueLight: false);
			}
			if (num == 12)
			{
				removeLight(positions);
			}
		}
		void placeLight(Point p, bool isBlueLight)
		{
			CTile cTile = ScenarioManager.Tiles[p.X, p.Y];
			CObjectProp cObjectProp = new CObjectProp(isBlueLight ? blueLight : redLight, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(p), null, null, null, cTile.m_HexMap.MapGuid);
			cTile.SpawnProp(cObjectProp);
			Trace("Spawned prop " + cObjectProp.PrefabName, null, 841);
		}
		void placeLights(List<Point> list, bool isBlueLight)
		{
			foreach (Point item in list)
			{
				placeLight(item, isBlueLight);
			}
		}
		void removeLight(List<Point> list)
		{
			foreach (Point item2 in list)
			{
				removeLight2(item2);
			}
		}
		void removeLight2(Point p)
		{
			CTile cTile = ScenarioManager.Tiles[p.X, p.Y];
			if (cTile.m_Props.Count > 0)
			{
				foreach (CObjectProp item3 in cTile.m_Props.ToList())
				{
					if (item3.PrefabName == redLight || item3.PrefabName == blueLight)
					{
						item3.DestroyProp();
						Trace("Destroyed prop " + item3.PrefabName, null, 863);
					}
				}
			}
		}
	}

	public override IEnumerable<CTile> TilesCannotStopOnTopOf()
	{
		yield return ScenarioManager.GetTile(4, 12);
		yield return ScenarioManager.GetTile(5, 12);
		yield return ScenarioManager.GetTile(6, 12);
		yield return ScenarioManager.GetTile(4, 4);
		yield return ScenarioManager.GetTile(5, 4);
		yield return ScenarioManager.GetTile(6, 4);
	}

	public override bool IsTileEleveated(CTile tile)
	{
		bool flag = RoomAHighlightTiles.Contains(tile.m_ArrayIndex);
		bool flag2 = RoomBHighlightTiles.Contains(tile.m_ArrayIndex);
		return flag | flag2;
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("AncientArtilleryID", 0, null, CActor.EType.Enemy);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Hidden;
		yield return enemyState;
		EnemyState enemyState2 = new EnemyState("AncientArtilleryID", 0, null, CActor.EType.Enemy);
		enemyState2.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState2.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState2.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState2.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState2.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.ToElite;
		yield return enemyState2;
		EnemyState enemyState3 = new EnemyState("FlamingBladespinnerID", 0, null, CActor.EType.Enemy);
		enemyState3.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState3.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState3.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState3.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState3.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Hidden;
		yield return enemyState3;
		EnemyState enemyState4 = new EnemyState("FlamingBladespinnerID", 0, null, CActor.EType.Enemy);
		enemyState4.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState4.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState4.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState4.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState4.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.ToElite;
		yield return enemyState4;
	}
}
