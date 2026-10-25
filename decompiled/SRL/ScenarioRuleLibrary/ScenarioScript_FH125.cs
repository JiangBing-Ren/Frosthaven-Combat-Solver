using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH125 : ScenarioScript
{
	private string explosionPrefabName = "S125FireVFX";

	private CObjective exploreObjective;

	private CObjective activateExplosionObjective;

	private CObjective_ActorsEscaped escapeObjective;

	private CScenarioModifier textModifier1;

	private CScenarioModifier textModifier2;

	private CScenarioModifier textModifier3;

	private CMap explosionRoom;

	private CObjectDoor finalDoor;

	private Point explosionCenter = new Point(15, 5);

	private int explosionExpandRate = 3;

	private bool explosionStarted;

	private int currentExplosionRadius = -3;

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(explosionStarted);
		string text2 = JsonConvert.SerializeObject(currentExplosionRadius);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 36);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 42);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			explosionStarted = JsonConvert.DeserializeObject<bool>(array[0]);
			currentExplosionRadius = JsonConvert.DeserializeObject<int>(array[1]);
			UpdateExplosionTileHighlights();
		}
	}

	protected override bool Initialize()
	{
		finalDoor = GetProp(9, 5, "ThickDoor") as CObjectDoor;
		explosionRoom = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_63");
		exploreObjective = GetObjective("exploreObjective");
		activateExplosionObjective = GetObjective("activateExplosionObjective");
		escapeObjective = GetObjective("escapeObjective") as CObjective_ActorsEscaped;
		textModifier1 = GetScenarioModifier("RuleText1");
		textModifier2 = GetScenarioModifier("RuleText2");
		textModifier3 = GetScenarioModifier("RuleText3");
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TryExpandExplosion();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			OnRoomRevealed();
			break;
		}
	}

	private void TryExpandExplosion()
	{
		if (!explosionStarted || !OncePerRound())
		{
			return;
		}
		currentExplosionRadius += explosionExpandRate;
		UpdateExplosionVFX();
		foreach (CActor item in new List<CActor>(ScenarioManager.Scenario.AllAliveActors))
		{
			if (GetDistanceToExplosionCenter(item) <= currentExplosionRadius)
			{
				MoveToClosestNonExplosionEmptyTile(item, out var tilesMoved);
				DealScenarioDamage(item, ScenarioManager.TrapDamage * Math.Max(1, tilesMoved), isTrapDamage: true);
			}
		}
		if (currentExplosionRadius > 0)
		{
			textModifier1.SetHiddenState(shouldSetHidden: true);
		}
		textModifier3.SetHiddenState(shouldSetHidden: false);
		textModifier2.SetHiddenState(shouldSetHidden: false);
	}

	private void UpdateExplosionVFX()
	{
		bool flag = false;
		List<CTile> list = new List<CTile>();
		foreach (CTile item in GameState.GetTilesInRange(explosionCenter, currentExplosionRadius, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: false, skipPathingCheck: false, includeStartingTile: true))
		{
			if (item != null && item.IsWalkable && item.m_Props.Find((CObjectProp p) => p.PrefabName == explosionPrefabName) == null)
			{
				list.Add(item);
			}
		}
		foreach (CTile item2 in list.OrderBy((CTile x) => Point.HexDist(x.m_ArrayIndex, explosionCenter)))
		{
			SpawnVFXProp(item2, explosionPrefabName);
			flag = true;
		}
		if (flag)
		{
			UpdateExplosionTileHighlights();
		}
	}

	private void UpdateExplosionTileHighlights()
	{
		List<Point> list = new List<Point>();
		List<Point> list2 = new List<Point>();
		List<Point> list3 = new List<Point>();
		List<Point> list4 = new List<Point>();
		List<CTile> tilesInRange = GameState.GetTilesInRange(explosionCenter, currentExplosionRadius + explosionExpandRate, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: false, skipPathingCheck: false, includeStartingTile: true);
		List<CTile> list5 = new List<CTile>();
		foreach (CTile item in tilesInRange)
		{
			if (item != null && item.IsWalkable)
			{
				list5.Add(item);
			}
		}
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile2 in tiles)
		{
			if (cTile2 == null)
			{
				continue;
			}
			if (cTile2.m_Props.Any((CObjectProp x) => x.PrefabName == explosionPrefabName))
			{
				list2.Add(cTile2.m_ArrayIndex);
				if (list5.Contains(cTile2))
				{
					list3.Add(cTile2.m_ArrayIndex);
				}
			}
			else
			{
				list.Add(cTile2.m_ArrayIndex);
				if (list5.Contains(cTile2))
				{
					list4.Add(cTile2.m_ArrayIndex);
				}
			}
		}
		CVector4 color = new CVector4(1f, 0f, 0f, 0.5f);
		ScenarioManager.ToggleTileHighlights(list, enabled: false, color);
		ScenarioManager.ToggleTileHighlights(list2, enabled: true, color);
		CVector4 color2 = new CVector4(1f, 1f, 0f, 0.25f);
		ScenarioManager.ToggleTileHighlights(list3, enabled: false, color2);
		ScenarioManager.ToggleTileHighlights(list4, enabled: true, color2);
	}

	private int GetDistanceToExplosionCenter(CActor actor)
	{
		return ScenarioManager.GetTileDistance(explosionCenter.X, explosionCenter.Y, actor.ArrayIndex.X, actor.ArrayIndex.Y);
	}

	private int GetDistanceToExplosionCenter(CTile tile)
	{
		return ScenarioManager.GetTileDistance(explosionCenter.X, explosionCenter.Y, tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y);
	}

	private void MoveToClosestNonExplosionEmptyTile(CActor actor, out int tilesMoved)
	{
		tilesMoved = 0;
		CTile cTile = FindClosestNonExplosionEmptyTile(actor);
		if (cTile != null)
		{
			Point arrayIndex = actor.ArrayIndex;
			CustomAbility.PushActorTo(actor, cTile, actor.Tile, 0, choreographerWait: false);
			tilesMoved = actor.GetDistanceTo(arrayIndex, viaPathfinding: true);
			CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
			{
				m_Actors = new List<CActor> { actor },
				m_Params = new List<string>
				{
					"Actor_0",
					tilesMoved.ToString()
				},
				m_MessageKey = "FH_Scenario_Campaign_125_EXPLOSION_MOVED_DISTANCE"
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	private CTile FindClosestNonExplosionEmptyTile(CActor actor)
	{
		if (IsTileNonExplosionEmpty(actor.Tile))
		{
			return actor.Tile;
		}
		CTile tile = actor.Tile;
		Queue<CTile> queue = new Queue<CTile>();
		HashSet<CTile> hashSet = new HashSet<CTile>();
		queue.Enqueue(tile);
		hashSet.Add(tile);
		while (queue.Count > 0)
		{
			foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(queue.Dequeue()))
			{
				if (!hashSet.Contains(allAdjacentTile))
				{
					if (IsTileNonExplosionEmpty(allAdjacentTile))
					{
						return allAdjacentTile;
					}
					hashSet.Add(allAdjacentTile);
					queue.Enqueue(allAdjacentTile);
				}
			}
		}
		return null;
	}

	private bool IsTileNonExplosionEmpty(CTile tile)
	{
		if (GetDistanceToExplosionCenter(tile) <= currentExplosionRadius)
		{
			return false;
		}
		if (!CAbilityFilter.IsTileEmpty(tile))
		{
			return false;
		}
		return true;
	}

	private void TickPhaseEndTurn()
	{
		TryStartExplosion();
		TryEscapeTickActor();
	}

	private void TryStartExplosion()
	{
		if (explosionStarted || !finalDoor.DoorIsOpen || _tickActor.Type != CActor.EType.Player || GetDistanceToExplosionCenter(_tickActor) > 1)
		{
			return;
		}
		bool flag = false;
		foreach (PlayerState player in ScenarioScript._css.Players)
		{
			if (player.Actor.Tile.m_HexMap != explosionRoom)
			{
				return;
			}
			flag |= GetDistanceToExplosionCenter(player.Actor) <= 1;
		}
		explosionStarted = flag;
		if (flag)
		{
			CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = explosionCenter,
				m_Zoom = 0.5f
			};
			ScenarioRuleClient.MessageHandler(message);
			GameState.TriggerCampaignSection("FS_125_The_Longest_Second_Special_1");
			activateExplosionObjective.SetActivation(active: false);
			escapeObjective.SetActivation(active: true);
			textModifier1.SetHiddenState(shouldSetHidden: false);
			CustomAbility.PlayVFX("S125ExplosionVFX", explosionCenter);
			RefreshExitTiles();
			SpawnFinalMonsters();
		}
	}

	private void RefreshExitTiles(CActor lastDeadSpeaker = null)
	{
		if (finalDoor.DoorIsOpen)
		{
			escapeObjective.EscapePositions.Clear();
			escapeObjective.EscapePositions.AddRange(ScenarioManager.StartingTiles.Select((CTile t) => new TileIndex(t.m_ArrayIndex)));
			ScenarioRuleClient.MessageHandler(new CRefreshExitTiles_MessageData());
		}
	}

	private void SpawnFinalMonsters()
	{
		if (!finalDoor.DoorIsOpen)
		{
			return;
		}
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			foreach (SpawnRoundEntry spawnRoundEntry in spawner.SpawnRoundEntries)
			{
				_ = spawnRoundEntry;
				spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			}
			spawner.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		}
	}

	private void TryEscapeTickActor()
	{
		if ((_tickActor.Type == CActor.EType.Ally || _tickActor.Type == CActor.EType.Player) && (_tickActor.Type != CActor.EType.Player || _tickActor.AllowEscape) && ScenarioManager.StartingTiles.Contains(_tickActor.Tile))
		{
			EscapeMap();
		}
	}

	private void OnRoomRevealed()
	{
		if (finalDoor.DoorIsOpen)
		{
			activateExplosionObjective.SetActivation(active: true);
			exploreObjective.SetActivation(active: false);
		}
	}

	protected override void TickRepositionInternal()
	{
		if (IsExplosionTile(_tickActorTile.m_ArrayIndex.X, _tickActorTile.m_ArrayIndex.Y))
		{
			DealScenarioDamage(_tickActor, CObjectHazardousTerrain.GetTerrainDamage(), isTrapDamage: false, isTerrainDamage: true);
		}
	}

	private bool IsExplosionTile(int x, int y)
	{
		CTile cTile = ScenarioManager.Tiles[x, y];
		if (cTile.m_Props != null && cTile.m_Props.Any((CObjectProp p) => p.PrefabName == explosionPrefabName))
		{
			return true;
		}
		return false;
	}
}
