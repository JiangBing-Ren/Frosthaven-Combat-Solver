using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierDestroyRoom : CScenarioModifier
{
	public string MapGuid { get; private set; }

	public int DestructionDamage { get; private set; }

	public bool InstaKillActors { get; private set; }

	public CMap DestroyMap => ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.MapGuid == MapGuid);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MapGuid", MapGuid);
		info.AddValue("DestructionDamage", DestructionDamage);
		info.AddValue("InstaKillActors", InstaKillActors);
	}

	public CScenarioModifierDestroyRoom(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "MapGuid":
					MapGuid = info.GetString("MapGuid");
					break;
				case "DestructionDamage":
					DestructionDamage = info.GetInt32("DestructionDamage");
					break;
				case "InstaKillActors":
					InstaKillActors = info.GetBoolean("InstaKillActors");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierDestroyRoom entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierDestroyRoom(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string mapGUID, int destructionDamage, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null, bool instaKillActors = false)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.DestroyRoom, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		MapGuid = mapGUID;
		DestructionDamage = destructionDamage;
		InstaKillActors = instaKillActors;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (IsActiveInRound(roundCount, forceActivate) && (!ApplyOnceTotal || !HasBeenAppliedOnce))
		{
			PerformScenarioModifier(roundCount, (CActor)null, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (IsActiveInRound(roundCount, forceActivate) && (IsPositive || !IgnoreNegativeScenarioEffects(currentActor)))
		{
			HasBeenAppliedOnce = true;
			DestroyMapDefault(DestroyMap);
		}
	}

	public static void DestroyMapDefault(CMap mapToDestroy, bool InstaKillActors = false, int DestructionDamage = 10)
	{
		mapToDestroy.Destroyed = true;
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			CTile cTile = ScenarioManager.Tiles[allAliveActor.ArrayIndex.X, allAliveActor.ArrayIndex.Y];
			if (cTile.m_HexMap != mapToDestroy && cTile.m_Hex2Map != mapToDestroy)
			{
				continue;
			}
			if (!InstaKillActors && allAliveActor.OriginalType == CActor.EType.Player)
			{
				GameState.ActorBeenDamaged(allAliveActor, DestructionDamage, checkIfPlayerCanAvoidDamage: true);
				if (!GameState.ActorHealthCheck(allAliveActor, allAliveActor))
				{
					continue;
				}
				CTile cTile2 = FindClosest(cTile, out var doors);
				if (cTile2 != null)
				{
					foreach (CObjectDoor item in doors)
					{
						item.SetDoorOpenedByMovingActor(allAliveActor);
					}
					if (!cTile2.m_HexMap.Revealed)
					{
						cTile2.m_HexMap.Reveal(initial: false, noIDRegen: false, forLevelEditor: false, fromActorOpeningDoor: true);
					}
					if (cTile2.m_Hex2Map != null && !cTile2.m_Hex2Map.Revealed)
					{
						cTile2.m_Hex2Map.Reveal(initial: false, noIDRegen: false, forLevelEditor: false, fromActorOpeningDoor: true);
					}
					CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(allAliveActor)
					{
						m_EndLocation = cTile2.m_ArrayIndex,
						m_StartLocation = allAliveActor.ArrayIndex,
						m_ActorTeleported = allAliveActor,
						m_TeleportAbility = null
					};
					allAliveActor.ArrayIndex = cTile2.m_ArrayIndex;
					ScenarioRuleClient.MessageHandler(message);
				}
				else
				{
					GameState.KillActor(allAliveActor, allAliveActor, CActor.ECauseOfDeath.Suicide, out var _);
				}
			}
			else
			{
				GameState.KillActor(allAliveActor, allAliveActor, CActor.ECauseOfDeath.Suicide, out var _);
			}
		}
		foreach (CObjectDoor doorProp in ScenarioManager.CurrentScenarioState.DoorProps)
		{
			if (doorProp.IsConnectedToDestroyedMap() && doorProp.DoorIsOpen)
			{
				doorProp.CloseOpenedDoor();
			}
		}
		foreach (CObjectProp prop in mapToDestroy.Props)
		{
			if (prop.ObjectType != ScenarioManager.ObjectImportType.Door)
			{
				prop.DestroyProp();
			}
		}
		CDestroyRoom_MessageData cDestroyRoom_MessageData = new CDestroyRoom_MessageData();
		cDestroyRoom_MessageData.m_MapToDestroy = mapToDestroy;
		ScenarioRuleClient.MessageHandler(cDestroyRoom_MessageData);
	}

	private CTile FindClosestEmptyTileInUndestroyedRoom(CTile originTile, int maxDepth = 100)
	{
		List<CTile> searchedTiles = new List<CTile> { originTile };
		return FindClosestEmptyTileInUndestroyedRoomRecursive(originTile, ref searchedTiles, 0, maxDepth);
	}

	private CTile FindClosestEmptyTileInUndestroyedRoomRecursive(CTile currentTile, ref List<CTile> searchedTiles, int currentDepth, int maxDepth)
	{
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			if (!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Walkable || searchedTiles.Contains(allAdjacentTile))
			{
				continue;
			}
			if (allAdjacentTile.m_HexMap.Destroyed || (allAdjacentTile.m_Hex2Map != null && allAdjacentTile.m_Hex2Map.Destroyed) || !CAbilityFilter.IsValidTile(allAdjacentTile, CAbilityFilter.EFilterTile.EmptyHex))
			{
				searchedTiles.Add(allAdjacentTile);
				if (currentDepth < maxDepth)
				{
					CTile cTile = FindClosestEmptyTileInUndestroyedRoomRecursive(allAdjacentTile, ref searchedTiles, currentDepth + 1, maxDepth);
					if (cTile != null)
					{
						return cTile;
					}
				}
				continue;
			}
			return allAdjacentTile;
		}
		return null;
	}

	private static CTile FindClosest(CTile currentTile, out List<CObjectDoor> doors)
	{
		List<List<Point>> list = new List<List<Point>>();
		List<CTile> tilesInRange = GameState.GetTilesInRange(currentTile.m_ArrayIndex, 1000, CAbility.EAbilityTargeting.All, emptyTilesOnly: true, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: true, includeTargetPosition: false, noActorsAllowed: false, skipPathingCheck: true);
		tilesInRange.RemoveAll((CTile x) => x.m_HexMap.Destroyed || (x.m_Hex2Map != null && x.m_Hex2Map.Destroyed));
		foreach (CTile item3 in tilesInRange)
		{
			List<Point> item = ScenarioManager.PathFinder.FindPath(null, currentTile.m_ArrayIndex, item3.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath, ignoreDoorLocks: false);
			if (foundPath)
			{
				list.Add(item);
			}
		}
		CTile bestPath = GetBestPath(list, out doors);
		if (bestPath != null)
		{
			return bestPath;
		}
		list.Clear();
		foreach (CTile item4 in tilesInRange)
		{
			List<Point> item2 = ScenarioManager.PathFinder.FindPath(null, currentTile.m_ArrayIndex, item4.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath2, ignoreDoorLocks: false);
			if (foundPath2)
			{
				list.Add(item2);
			}
		}
		return GetBestPath(list, out doors);
	}

	private static CTile GetBestPath(List<List<Point>> paths, out List<CObjectDoor> doors)
	{
		CTile cTile = null;
		List<CObjectDoor> list = new List<CObjectDoor>();
		doors = new List<CObjectDoor>();
		if (paths.Count > 0)
		{
			paths.Sort((List<Point> x, List<Point> y) => x.Count.CompareTo(y.Count));
			foreach (List<Point> path in paths)
			{
				bool flag = false;
				CTile cTile2 = ScenarioManager.Tiles[path.First().X, path.First().Y];
				foreach (Point item in path)
				{
					CTile cTile3 = ScenarioManager.Tiles[item.X, item.Y];
					if ((cTile3.m_HexMap != cTile2.m_HexMap && cTile3.m_HexMap.Destroyed) || (cTile3.m_Hex2Map != null && (cTile2.m_Hex2Map == null || cTile3.m_Hex2Map != cTile2.m_Hex2Map) && cTile3.m_Hex2Map.Destroyed))
					{
						flag = true;
						if (cTile == null)
						{
							cTile = ScenarioManager.Tiles[path.Last().X, path.Last().Y];
							list = GetClosedDoors(path);
						}
					}
				}
				if (!flag)
				{
					doors = GetClosedDoors(path);
					return ScenarioManager.Tiles[path.Last().X, path.Last().Y];
				}
			}
			if (list.Count > 0)
			{
				doors.Add(list[0]);
			}
			return cTile;
		}
		return null;
	}

	private static List<CObjectDoor> GetClosedDoors(List<Point> path)
	{
		List<CObjectDoor> list = new List<CObjectDoor>();
		foreach (Point item in path)
		{
			CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.Tiles[item.X, item.Y].FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor != null && !cObjectDoor.Activated)
			{
				list.Add(cObjectDoor);
			}
		}
		return list;
	}
}
