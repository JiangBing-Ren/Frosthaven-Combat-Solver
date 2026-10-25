using System;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FH.SeasonTech;
using SETUtil;
using ScenarioRuleLibrary;
using UnityEngine;

public static class ScenarioMaptileEditHelpers
{
	public static EnvironmentLoader.Mapping GenerateMappings(GameObject root, ScenarioState state)
	{
		EnvironmentLoader.Mapping mapping = new EnvironmentLoader.Mapping();
		ProceduralMapTile[] componentsInChildren = root.GetComponentsInChildren<ProceduralMapTile>(includeInactive: true);
		foreach (CMap map in state.Maps)
		{
			GameObject gameObject = componentsInChildren.Single((ProceduralMapTile a) => a.name.Contains(map.MapGuid)).gameObject;
			mapping.Maps.Add(new EnvironmentLoader.Mapping.Tuple<string, GameObject>(map.MapGuid, gameObject));
			GameObject gameObject2 = gameObject.FindInImmediateChildren("Generated Content");
			if (gameObject2 == null)
			{
				throw new Exception("No Generated Content found in " + map.RoomName + " at " + state.ScenarioFileName);
			}
			GameObject gameObject3 = gameObject2.FindInImmediateChildren("Full");
			if (gameObject3 == null)
			{
				if (!ProceduralMapTile.IsInLevelEditorMode)
				{
					throw new Exception("No Full found in " + map.RoomName + " at " + state.ScenarioFileName);
				}
				continue;
			}
			GameObject[] array = (from a in gameObject3.FindInImmediateChildren<Transform>()
				where IsFullFloorTile(a)
				select a.gameObject).ToArray();
			if (array.Length == 0)
			{
				throw new Exception("No renderTiles collected from " + map.RoomName + " at " + state.ScenarioFileName + ". Make sure you include FShape in the name of full hex tiles.");
			}
			Vector3[] collection = array.Select((GameObject a) => GetCenter(a.transform)).ToArray();
			foreach (CMapTile tile in map.MapTiles)
			{
				GameObject gameObject4 = gameObject.FindInChildren(tile.TileGuid, includeInactive: true);
				if (gameObject4 == null)
				{
					mapping.MissingTiles.Add(tile.TileGuid);
					Debug.LogWarning("Tile " + tile.TileGuid + " not found in " + map.RoomName + " at " + state.ScenarioFileName);
					continue;
				}
				GameObject gameObject5 = array[collection.Closest(gameObject4.transform.position)];
				GameObject gameObject6 = CollectTerrainVisuals(root, state, tile.TileGuid, tile.ArrayIndex);
				if (Vector3.SqrMagnitude(gameObject5.transform.position - gameObject4.transform.position) > 1f)
				{
					if (gameObject6 == null)
					{
						continue;
					}
					gameObject5 = gameObject6;
				}
				Vector3 worldPos = new Vector3(tile.Position.X, tile.Position.Y, tile.Position.Z);
				if (mapping.Tiles.Any((EnvironmentLoader.Mapping.Tuple<string, EnvironmentLoader.Mapping.TileData> a) => a.Key == tile.TileGuid))
				{
					throw new Exception("Tile GUID collision");
				}
				EnvironmentLoader.Mapping.TileData tileData = new EnvironmentLoader.Mapping.TileData(gameObject4, gameObject5, tile.ArrayIndex, worldPos);
				tileData.LinkTerrainVisuals(gameObject6);
				mapping.Tiles.Add(new EnvironmentLoader.Mapping.Tuple<string, EnvironmentLoader.Mapping.TileData>(tile.TileGuid, tileData));
			}
			foreach (CObjectProp item in map.Props.Concat(map.DoorProps))
			{
				foreach (Transform item2 in gameObject.transform)
				{
					if (item2.name.Contains(item.PropGuid))
					{
						mapping.Props.Add(new EnvironmentLoader.Mapping.Tuple<string, GameObject>(item.PropGuid, item2.gameObject));
					}
				}
			}
		}
		return mapping;
		static Vector3 GetCenter(Transform t)
		{
			BoxCollider component = t.GetComponent<BoxCollider>();
			if ((bool)component)
			{
				return t.localToWorldMatrix.MultiplyPoint3x4(component.center);
			}
			return t.position;
		}
		static bool IsFullFloorTile(Transform a)
		{
			if (!a.name.ToLower().Contains("fshape") && !(a.name == "Simple Tile") && !a.name.Contains("CR_FloorHex_"))
			{
				return a.name.ToLower().Contains("_floor_");
			}
			return true;
		}
	}

	public static bool IsTerrainVisualEffectProp(EPropType propType)
	{
		if (propType == EPropType.TerrainVisualEffect)
		{
			return true;
		}
		return false;
	}

	private static GameObject CollectTerrainVisuals(GameObject root, ScenarioState state, string guid, TileIndex index)
	{
		foreach (CObjectProp item in state.Props.Where((CObjectProp a) => IsTerrainVisualEffectProp(a.PropType)))
		{
			if (TileIndex.Compare(item.PropTile.m_ArrayIndex.ToTileIndex(), index))
			{
				return root.FindInChildren(item.InstanceName);
			}
		}
		return null;
	}

	public static EnvironmentLoader.Mapping GeneratePartialMappings(GameObject root, ScenarioState state)
	{
		EnvironmentLoader.Mapping mapping = new EnvironmentLoader.Mapping();
		ProceduralMapTile[] componentsInChildren = root.GetComponentsInChildren<ProceduralMapTile>(includeInactive: true);
		foreach (CMap map in state.Maps)
		{
			GameObject gameObject = componentsInChildren.Single((ProceduralMapTile a) => a.name.Contains(map.MapGuid)).gameObject;
			mapping.Maps.Add(new EnvironmentLoader.Mapping.Tuple<string, GameObject>(map.MapGuid, gameObject));
			foreach (CMapTile tile in map.MapTiles)
			{
				GameObject gameObject2 = gameObject.FindInChildren(tile.TileGuid, includeInactive: true);
				if (gameObject2 == null)
				{
					mapping.MissingTiles.Add(tile.TileGuid);
					Debug.LogWarning("Tile " + tile.TileGuid + " not found in " + map.RoomName + " at " + state.ScenarioFileName);
				}
				else
				{
					Vector3 worldPos = new Vector3(tile.Position.X, tile.Position.Y, tile.Position.Z);
					if (mapping.Tiles.Any((EnvironmentLoader.Mapping.Tuple<string, EnvironmentLoader.Mapping.TileData> a) => a.Key == tile.TileGuid))
					{
						throw new Exception("Tile GUID collision");
					}
					mapping.Tiles.Add(new EnvironmentLoader.Mapping.Tuple<string, EnvironmentLoader.Mapping.TileData>(tile.TileGuid, new EnvironmentLoader.Mapping.TileData(gameObject2, null, tile.ArrayIndex, worldPos)));
				}
			}
			foreach (CObjectProp prop in map.Props.Concat(state.Props))
			{
				foreach (Transform item in gameObject.transform)
				{
					if (item.name.Contains(prop.PropGuid) && !mapping.Props.Any((EnvironmentLoader.Mapping.Tuple<string, GameObject> a) => a.Key == prop.PropGuid))
					{
						mapping.Props.Add(new EnvironmentLoader.Mapping.Tuple<string, GameObject>(prop.PropGuid, item.gameObject));
					}
				}
			}
			foreach (CMapTile mapTile in map.MapTiles)
			{
				GameObject gameObject3 = gameObject.FindInChildren(mapTile.TileGuid, includeInactive: true);
				if (gameObject3 == null)
				{
					mapping.MissingTiles.Add(mapTile.TileGuid);
					Debug.LogWarning("Tile " + mapTile.TileGuid + " not found in " + map.RoomName + " at " + state.ScenarioFileName);
				}
				else
				{
					Vector3 worldPos2 = new Vector3(mapTile.Position.X, mapTile.Position.Y, mapTile.Position.Z);
					mapping.Tiles.Add(new EnvironmentLoader.Mapping.Tuple<string, EnvironmentLoader.Mapping.TileData>(mapTile.TileGuid, new EnvironmentLoader.Mapping.TileData(gameObject3, null, mapTile.ArrayIndex, worldPos2)));
				}
			}
		}
		return mapping;
	}

	public static void MaptileRefreshPostProcessing()
	{
		ProceduralScenario[] array = UnityEngine.Object.FindObjectsOfType<ProceduralScenario>();
		foreach (ProceduralScenario proceduralScenario in array)
		{
			MappingRecord componentInChildren = proceduralScenario.GetComponentInChildren<MappingRecord>();
			IMapping mapping = ((!proceduralScenario || !componentInChildren) ? ((IMapping)new EnvironmentLoader.GymMapping(proceduralScenario.gameObject)) : ((IMapping)new EnvironmentLoader.BakedMapping
			{
				MappingRecord = componentInChildren,
				EnvironmentRoot = proceduralScenario.gameObject
			}));
			if (mapping != null && mapping.GetMaps().Count() > 0)
			{
				proceduralScenario.StartCoroutine(proceduralScenario.OnMaptileGenerationCompleteCrt(mapping));
			}
			WorldSeasonState.Set(WorldSeasonState.LastSeasonState);
		}
	}
}
