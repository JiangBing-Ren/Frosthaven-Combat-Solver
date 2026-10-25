using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

[StateDump(UseToString = true)]
public class CTile
{
	[StateDump(Skip = true)]
	public CMap m_HexMap;

	[StateDump(Skip = true)]
	public CMapTile m_Hex;

	[StateDump(Skip = true)]
	public CMap m_Hex2Map;

	[StateDump(Skip = true)]
	public CMapTile m_Hex2;

	[StateDump(Skip = true)]
	public Point m_ArrayIndex;

	public List<CObjectProp> m_Props = new List<CObjectProp>();

	public List<CSpawner> m_Spawners = new List<CSpawner>();

	public bool TreatAsWater;

	public bool TreatAsHazardous;

	public bool TreatAsDifficult;

	public bool TreatAsIce;

	public IEnumerable<CActor> Actors => ScenarioManager.Scenario.AllActors.Where((CActor x) => x.ArrayIndex == m_ArrayIndex || (x is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.GetTiles().Contains(m_ArrayIndex))).ToList();

	public IEnumerable<CActor> LivingActors => Actors.Where((CActor x) => !x.IsDead);

	public CActor LivingActor => LivingActors.FirstOrDefault();

	public CObjectTrap Trap => m_Props.Find((CObjectProp x) => x is CObjectTrap && !x.Activated && x.RuntimeAttachedActor == null) as CObjectTrap;

	public CObjectObstacle Obstacle => m_Props.Find((CObjectProp x) => x is CObjectObstacle) as CObjectObstacle;

	public bool IsEmpty => CAbilityFilter.IsValidTile(this, CAbilityFilter.EFilterTile.EmptyHex);

	public bool IsEmptyNoProps => CAbilityFilter.IsValidTile(this, CAbilityFilter.EFilterTile.EmptyHexNoProps);

	public bool IsEmptyIgnoreTraps => CAbilityFilter.IsValidTile(this, CAbilityFilter.EFilterTile.EmptyHexIgnoreTraps);

	public bool IsFeatureless => CAbilityFilter.IsValidTile(this, CAbilityFilter.EFilterTile.FeaturelessHex);

	public bool IsWalkable => ScenarioManager.PathFinder.GetNode(m_ArrayIndex).Walkable;

	public bool IsBlocked => ScenarioManager.PathFinder.GetNode(m_ArrayIndex).Blocked;

	public bool IsWater
	{
		get
		{
			if (TreatAsWater)
			{
				return true;
			}
			if (m_HexMap != null && m_HexMap.TreatAsWater)
			{
				return true;
			}
			if (m_Hex2Map != null && m_Hex2Map.TreatAsWater)
			{
				return true;
			}
			return m_Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater);
		}
	}

	public bool IsIce
	{
		get
		{
			if (TreatAsIce)
			{
				return true;
			}
			if (CAbilityFilter.IsTileFeatureless(this))
			{
				if (m_HexMap != null && m_HexMap.TreatAsIce)
				{
					return true;
				}
				if (m_Hex2Map != null && m_Hex2Map.TreatAsIce)
				{
					return true;
				}
			}
			return m_Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainIce);
		}
	}

	public bool MapTreatAsIce
	{
		get
		{
			if (m_HexMap != null && m_HexMap.TreatAsIce)
			{
				return true;
			}
			if (m_Hex2Map != null && m_Hex2Map.TreatAsIce)
			{
				return true;
			}
			return false;
		}
	}

	public CObjectProp FindProp(ScenarioManager.ObjectImportType importObjectType, bool excludeHealth = false)
	{
		lock (m_Props)
		{
			return m_Props.FirstOrDefault((CObjectProp x) => x.ObjectType == importObjectType && (!excludeHealth || x.RuntimeAttachedActor == null));
		}
	}

	public CObjectProp FindProp(ESpecificPropType specificPropType)
	{
		return m_Props.Find((CObjectProp x) => x.SpecificPropType == specificPropType);
	}

	public CObjectDoor FindDoor()
	{
		return FindPropOfType<CObjectDoor>();
	}

	public CObjectProp FindTerrainProp(bool forDestructionFromPlayer = false)
	{
		return m_Props.FirstOrDefault((CObjectProp x) => (!forDestructionFromPlayer || x.CanBeRemovedByPlayers()) && (x is CObjectDifficultTerrain || x is CObjectHazardousTerrain || x is CObjectIceTerrain));
	}

	public CObjectProp FindHazardousProp(bool forDestructionFromPlayer = false)
	{
		return m_Props.FirstOrDefault((CObjectProp x) => (!forDestructionFromPlayer || x.CanBeRemovedByPlayers()) && x is CObjectHazardousTerrain);
	}

	public CObjectProp GetIceTerrain()
	{
		return m_Props.FirstOrDefault((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.TerrainIce);
	}

	public CObjectProp FindWaterProp()
	{
		return m_Props.FirstOrDefault((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.TerrainWater);
	}

	public void FindPropsOfType<TProp>(List<TProp> outProps)
	{
		foreach (CObjectProp prop in m_Props)
		{
			if (prop is TProp item)
			{
				outProps.Add(item);
			}
		}
	}

	public CObjectProp FindPropOfTypes(List<EPropType> types, bool forDestructionFromPlayer = false)
	{
		foreach (CObjectProp prop in m_Props)
		{
			if ((!forDestructionFromPlayer || prop.CanBeRemovedByPlayers()) && types.Contains(prop.PropType))
			{
				return prop;
			}
		}
		return null;
	}

	public CObjectTrap FindTrapProp(bool forDestructionFromPlayer = false)
	{
		return FindPropOfTypes(EPropType.Trap, forDestructionFromPlayer) as CObjectTrap;
	}

	public CObjectProp FindPropOfTypes(EPropType type, bool forDestructionFromPlayer = false)
	{
		return FindPropOfTypes(new List<EPropType> { type }, forDestructionFromPlayer);
	}

	public TProp FindPropOfType<TProp>() where TProp : CObjectProp
	{
		foreach (CObjectProp prop in m_Props)
		{
			if (prop is TProp result)
			{
				return result;
			}
		}
		return null;
	}

	public CObjectObstacle FindObstacleProp(bool forDestructionFromPlayer = false, bool checkMultiHexProps = false)
	{
		foreach (CObjectProp prop in m_Props)
		{
			if ((!forDestructionFromPlayer || prop.CanBeRemovedByPlayers()) && prop is CObjectObstacle cObjectObstacle && (cObjectObstacle.PropHealthDetails == null || !cObjectObstacle.PropHealthDetails.HasHealth))
			{
				return cObjectObstacle;
			}
		}
		if (checkMultiHexProps)
		{
			CTile propTile = this;
			CObjectObstacle cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(m_ArrayIndex, ref propTile);
			if (cObjectObstacle2 != null)
			{
				if (forDestructionFromPlayer && !cObjectObstacle2.CanBeRemovedByPlayers())
				{
					return null;
				}
				return cObjectObstacle2;
			}
		}
		return null;
	}

	public List<CObjectProp> FindProps(ScenarioManager.ObjectImportType importObjectType)
	{
		return m_Props.Where((CObjectProp x) => x.ObjectType == importObjectType).ToList();
	}

	public bool CheckForPropTypes(List<ScenarioManager.ObjectImportType> typesToCheckFor, bool ignoreNonBlocking = false)
	{
		if (typesToCheckFor == null || typesToCheckFor.Count == 0)
		{
			return false;
		}
		bool flag = m_Props.Where((CObjectProp p) => typesToCheckFor.Contains(p.ObjectType)).Any();
		if ((ignoreNonBlocking & flag) && !ScenarioManager.PathFinder.GetNode(m_ArrayIndex).Blocked)
		{
			return false;
		}
		return flag;
	}

	public CSpawner FindSpawner(string guid)
	{
		return m_Spawners.Find((CSpawner x) => x.SpawnerGuid == guid);
	}

	public void AddProp(CObjectProp prop, bool recursiveTiles = false)
	{
		if (prop.OnlyVisuals)
		{
			return;
		}
		int num = m_Props.FindIndex((CObjectProp x) => x.PropGuid == prop.PropGuid);
		if (num >= 0)
		{
			m_Props[num] = prop;
		}
		else
		{
			m_Props.AddOrdered(prop, GameState.ComparePropsByGUID);
		}
		if (prop is CObjectShadow)
		{
			ScenarioManager.PathFinder.GetNode(m_ArrayIndex).HasShadow = true;
		}
		if (!recursiveTiles)
		{
			return;
		}
		foreach (Point tile in prop.GetTiles())
		{
			if (!(tile == m_ArrayIndex))
			{
				ScenarioManager.GetTile(tile).AddProp(prop);
			}
		}
	}

	public void DelProp(CObjectProp prop, bool recursiveTiles = false)
	{
		lock (m_Props)
		{
			m_Props.Remove(prop);
			if (prop is CObjectShadow && !m_Props.Any((CObjectProp x) => x is CObjectShadow))
			{
				ScenarioManager.PathFinder.GetNode(m_ArrayIndex).HasShadow = false;
			}
			if (!recursiveTiles)
			{
				return;
			}
			foreach (Point tile in prop.GetTiles())
			{
				if (!(tile == m_ArrayIndex))
				{
					ScenarioManager.GetTile(tile)?.DelProp(prop);
				}
			}
		}
	}

	public void DelProp(string guid, bool recursiveTiles = false)
	{
		for (int num = m_Props.Count - 1; num >= 0; num--)
		{
			CObjectProp cObjectProp = m_Props[num];
			if (!(cObjectProp.PropGuid != guid))
			{
				DelProp(cObjectProp, recursiveTiles);
			}
		}
	}

	public void SpawnProp(CObjectProp prop, bool notifyClient = true, float spawnDelay = 0f, CAbility fromAbility = null)
	{
		if (prop == null)
		{
			throw new ArgumentNullException("prop");
		}
		bool recursiveTiles = false;
		AddProp(prop, recursiveTiles);
		ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_HexMap.MapGuid);
		if (!prop.OnlyVisuals)
		{
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				if (!ScenarioManager.CurrentScenarioState.Props.Any((CObjectProp a) => a.PropGuid == prop.PropGuid))
				{
					lock (ScenarioManager.CurrentScenarioState.DestroyedProps)
					{
						ScenarioManager.CurrentScenarioState.DestroyedProps.Remove(prop);
					}
					ScenarioManager.CurrentScenarioState.Props.Add(prop);
				}
			}
		}
		if (notifyClient)
		{
			CSpawn_MessageData message = new CSpawn_MessageData(null)
			{
				m_SpawnDelay = spawnDelay,
				m_Prop = prop
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		if (fromAbility != null)
		{
			fromAbility.TargetingActor.m_OnPropCreatedListeners?.Invoke(fromAbility.TargetingActor, fromAbility, prop);
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCreateProp(prop.PropType, prop.SpecificPropType, new SEventAbility.SEventAbilityBaseData(fromAbility.TargetingActor, ESESubTypeAbility.AbilityEnded, fromAbility)));
		}
	}

	public void SpawnSpawner(CSpawner spawner)
	{
		if (spawner != null)
		{
			m_Spawners.AddOrdered(spawner, ScenarioManager.CurrentScenarioState.Spawners);
			CMap cMap = ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_HexMap.MapGuid);
			CMap cMap2 = ((m_Hex2Map != null) ? ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_Hex2Map.MapGuid) : null);
			if (!cMap.Spawners.Any((CSpawner a) => a.SpawnerGuid == spawner.SpawnerGuid) && (cMap2 == null || !cMap2.Spawners.Any((CSpawner a2) => a2.SpawnerGuid == spawner.SpawnerGuid)))
			{
				ScenarioManager.CurrentScenarioState.Spawners.Add(spawner);
			}
		}
	}

	public bool IsMapShared(CTile tile, bool hexMapOnly = false)
	{
		if (tile.m_HexMap != null)
		{
			if (m_HexMap != null && tile.m_HexMap == m_HexMap)
			{
				return true;
			}
			if (m_Hex2Map != null && tile.m_HexMap == m_Hex2Map)
			{
				return true;
			}
		}
		if (!hexMapOnly && tile.m_Hex2Map != null)
		{
			if (m_HexMap != null && tile.m_Hex2Map == m_HexMap)
			{
				return true;
			}
			if (m_Hex2Map != null && tile.m_Hex2Map == m_Hex2Map)
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return $"{GetType().Name}({m_ArrayIndex})";
	}
}
