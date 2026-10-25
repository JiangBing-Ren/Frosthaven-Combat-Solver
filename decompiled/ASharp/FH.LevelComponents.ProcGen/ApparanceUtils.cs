using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public static class ApparanceUtils
{
	public static bool PropNeedsPlaceholder(CObjectProp prop, GameObject propGO, out List<Vector3> placeholderPositions)
	{
		placeholderPositions = new List<Vector3>();
		if (new string[3] { "Tile", "Terrain", "PressurePlate" }.Any((string x) => prop.ObjectType.GetName().Contains(x, StringComparison.InvariantCultureIgnoreCase)))
		{
			placeholderPositions.Add(propGO.transform.position);
			return true;
		}
		ProceduralTile[] componentsInChildren = propGO.GetComponentsInChildren<ProceduralTile>();
		ProceduralTile[] array = componentsInChildren;
		foreach (ProceduralTile proceduralTile in array)
		{
			if (((IProceduralTile)proceduralTile).IsActive)
			{
				placeholderPositions.Add(proceduralTile.transform.position);
			}
		}
		return componentsInChildren.Length != 0;
	}
}
