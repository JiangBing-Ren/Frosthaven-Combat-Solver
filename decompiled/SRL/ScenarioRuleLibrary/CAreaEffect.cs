using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public class CAreaEffect
{
	[Flags]
	public enum EHexFlag
	{
		None = 0,
		Enabled = 1,
		Ally = 2,
		Enemy = 4,
		Space = 8,
		All = -1
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class CAreaEffectHex
	{
		[JsonProperty]
		public Point ArrayIndex { get; }

		[JsonProperty]
		public EHexFlag Flags { get; private set; }

		[JsonConstructor]
		public CAreaEffectHex(int x, int y, EHexFlag flags)
		{
			ArrayIndex = new Point(x, y);
			Flags = flags;
		}

		public CAreaEffectHex Copy()
		{
			return new CAreaEffectHex(ArrayIndex.X, ArrayIndex.Y, Flags);
		}

		public void SetFlags(EHexFlag flags)
		{
			Flags |= flags;
		}

		public void ClearFlags(EHexFlag flags)
		{
			Flags &= ~flags;
		}
	}

	[JsonProperty]
	public string Name { get; }

	[JsonProperty]
	public bool Melee { get; }

	[JsonProperty]
	public List<CAreaEffectHex> Hexes { get; }

	[JsonProperty]
	public bool CanFlip { get; set; }

	[JsonProperty]
	public bool IgnoreBlocked { get; set; }

	public IEnumerable<CAreaEffectHex> EnabledHexes => Hexes.Where((CAreaEffectHex h) => (h.Flags & EHexFlag.Enabled) != 0);

	public IEnumerable<CAreaEffectHex> DisabledHexes => Hexes.Where((CAreaEffectHex h) => (h.Flags & EHexFlag.Enabled) == 0);

	[JsonConstructor]
	public CAreaEffect(string name, bool melee, List<CAreaEffectHex> hexes, bool canFlip = true, bool ignoreBlocked = false)
	{
		Name = name;
		Melee = melee;
		Hexes = hexes;
		CanFlip = canFlip;
		IgnoreBlocked = ignoreBlocked;
	}

	public CAreaEffect Copy()
	{
		return new CAreaEffect(Name, Melee, Hexes.Select((CAreaEffectHex s) => s.Copy()).ToList(), CanFlip, IgnoreBlocked);
	}

	public static List<CTile> GetAllPossibleTiles(CActor actor, CAreaEffect areaEffect, int range, List<CTile> innerRange, ref List<CTile> validTilesIncludingBlockedOut, ref Dictionary<CTile, EHexFlag> flagsOfValidTiles, bool ignoreLos, bool ignoreDoorLocks)
	{
		List<CTile> tilesInRange = GameState.GetTilesInRange(actor, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true);
		if (areaEffect.Melee)
		{
			return GetTilesAtAllRotations(actor, null, areaEffect, tilesInRange, ref validTilesIncludingBlockedOut, ref flagsOfValidTiles, ignoreLos, ignoreDoorLocks);
		}
		CTile tile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
		List<CTile> list = new List<CTile>();
		list.AddRange(tilesInRange);
		foreach (CTile item in GameState.GetTilesInRange(actor, range + areaEffect.EnabledHexes.Count(), CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, innerRange, ignorePathLength: true))
		{
			List<CTile> areaTiles = GetRotatedHexTilesAtAllRotations(tile, item, areaEffect);
			if (tilesInRange.Any((CTile x) => areaTiles.Contains(x)))
			{
				list.AddRange(areaTiles);
			}
		}
		return GetValidTilesFromList(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y], tilesInRange, list.Distinct().ToList(), ref validTilesIncludingBlockedOut);
	}

	public static List<CTile> GetAllPossibleTiles(CTile inTile, CAreaEffect areaEffect, int range, List<CTile> innerRange, ref List<CTile> validTilesIncludingBlockedOut, ref Dictionary<CTile, EHexFlag> flagsOfValidTiles, bool ignoreLos, bool ignoreDoorLocks)
	{
		List<CTile> tilesInRange = GameState.GetTilesInRange(inTile.m_ArrayIndex, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true);
		if (areaEffect.Melee)
		{
			return GetTilesAtAllRotations(inTile, null, areaEffect, tilesInRange, ref validTilesIncludingBlockedOut, ref flagsOfValidTiles, ignoreLos, ignoreDoorLocks);
		}
		List<CTile> list = new List<CTile>();
		list.AddRange(tilesInRange);
		foreach (CTile item in GameState.GetTilesInRange(inTile.m_ArrayIndex, range + areaEffect.EnabledHexes.Count(), CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, innerRange, ignorePathLength: true))
		{
			List<CTile> areaTiles = GetRotatedHexTilesAtAllRotations(inTile, item, areaEffect);
			if (tilesInRange.Any((CTile x) => areaTiles.Contains(x)))
			{
				list.AddRange(areaTiles);
			}
		}
		return GetValidTilesFromList(inTile, tilesInRange, list.Distinct().ToList(), ref validTilesIncludingBlockedOut);
	}

	private static List<CTile> GetTilesAtAllRotations(CActor actor, CTile positionTile, CAreaEffect areaEffect, List<CTile> tilesInRange, ref List<CTile> validTilesIncludingBlockedOut, ref Dictionary<CTile, EHexFlag> flagsOfValidTiles, bool ignoreLos, bool ignoreDoorLocks)
	{
		return GetTilesAtAllRotations(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y], positionTile, areaEffect, tilesInRange, ref validTilesIncludingBlockedOut, ref flagsOfValidTiles, ignoreLos, ignoreDoorLocks);
	}

	private static List<CTile> GetTilesAtAllRotations(CTile tile, CTile positionTile, CAreaEffect areaEffect, List<CTile> tilesInRange, ref List<CTile> validTilesIncludingBlockedOut, ref Dictionary<CTile, EHexFlag> flagsOfValidTiles, bool ignoreLos, bool ignoreDoorLocks)
	{
		List<CTile> validTilesIncludingBlocked = new List<CTile>();
		Dictionary<CTile, EHexFlag> flagsOfValidTiles2 = new Dictionary<CTile, EHexFlag>();
		bool flipped = false;
		string flipAxis = null;
		List<CTile> validTiles = GetValidTiles(tile, positionTile, areaEffect, 0f, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles2, ignoreLos, ignoreDoorLocks);
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlocked);
		TryAddRange(flagsOfValidTiles, flagsOfValidTiles2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 60f, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles2, ignoreLos, ignoreDoorLocks));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlocked);
		TryAddRange(flagsOfValidTiles, flagsOfValidTiles2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 120f, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles2, ignoreLos, ignoreDoorLocks));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlocked);
		TryAddRange(flagsOfValidTiles, flagsOfValidTiles2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 180f, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles2, ignoreLos, ignoreDoorLocks));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlocked);
		TryAddRange(flagsOfValidTiles, flagsOfValidTiles2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 240f, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles2, ignoreLos, ignoreDoorLocks));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlocked);
		TryAddRange(flagsOfValidTiles, flagsOfValidTiles2);
		validTiles.AddRange(GetValidTiles(tile, positionTile, areaEffect, 300f, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles2, ignoreLos, ignoreDoorLocks));
		validTilesIncludingBlockedOut.AddRange(validTilesIncludingBlocked);
		TryAddRange(flagsOfValidTiles, flagsOfValidTiles2);
		HashSet<CTile> hashSet = new HashSet<CTile>(validTilesIncludingBlockedOut.Count);
		List<CTile> list = new List<CTile>(validTilesIncludingBlockedOut.Count);
		for (int i = 0; i < validTilesIncludingBlockedOut.Count; i++)
		{
			if (hashSet.Add(validTilesIncludingBlockedOut[i]))
			{
				list.Add(validTilesIncludingBlockedOut[i]);
			}
		}
		validTilesIncludingBlockedOut = list.ToList();
		return validTiles.Distinct().ToList();
		static void TryAddRange(Dictionary<CTile, EHexFlag> destination, Dictionary<CTile, EHexFlag> source)
		{
			foreach (KeyValuePair<CTile, EHexFlag> item in source)
			{
				destination.TryAdd(item.Key, item.Value);
			}
		}
	}

	private static List<CTile> GetRotatedHexTilesAtAllRotations(CTile tile, CTile positionTile, CAreaEffect areaEffect)
	{
		List<CAreaEffectHex> list = areaEffect.EnabledHexes.ToList();
		List<CTile> list2 = new List<CTile>(list.Count * 6);
		list2.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, list, 0f));
		list2.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, list, 60f));
		list2.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, list, 120f));
		list2.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, list, 180f));
		list2.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, list, 240f));
		list2.AddRange(GetRotatedHexTiles(tile, positionTile, areaEffect.Melee, list, 300f));
		return list2.Distinct().ToList();
	}

	public static List<CTile> GetValidTiles(CActor actor, CTile positionTile, CAreaEffect areaEffect, float rotation, bool flipped, string flipAxis, List<CTile> tilesInRange, ref List<CTile> validTilesIncludingBlocked, ref Dictionary<CTile, EHexFlag> flagsOfValidTiles, bool ignoreLos, bool ignoreDoorLocks, int range = 0, int minRange = -1)
	{
		return GetValidTiles(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y], positionTile, areaEffect, rotation, flipped, flipAxis, tilesInRange, ref validTilesIncludingBlocked, ref flagsOfValidTiles, ignoreLos, ignoreDoorLocks, range, minRange);
	}

	public static List<CTile> GetValidTiles(CTile actorOriginTile, CTile positionTile, CAreaEffect areaEffect, float rotation, bool flipped, string flipAxis, List<CTile> tilesInRange, ref List<CTile> validTilesIncludingBlocked, ref Dictionary<CTile, EHexFlag> flagsOfValidTiles, bool ignoreLos, bool ignoreDoorLocks, int range = 0, int minRange = -1)
	{
		List<CTile> list = new List<CTile>();
		validTilesIncludingBlocked?.Clear();
		flagsOfValidTiles?.Clear();
		bool flag = (actorOriginTile?.Actors?.FirstOrDefault((CActor actor) => actor != null && !actor.IsDead))?.CanTargetFromAnywhere(range, hasOtherTargets: false) ?? false;
		CTile cTile = (areaEffect.Melee ? actorOriginTile : positionTile);
		float num = rotation;
		float num2 = 0f;
		if (cTile != null)
		{
			List<Point> list2 = new List<Point>();
			List<EHexFlag> list3 = new List<EHexFlag>();
			foreach (CAreaEffectHex enabledHex in areaEffect.EnabledHexes)
			{
				Point arrayIndex = enabledHex.ArrayIndex;
				if (flipped)
				{
					Point p = Point.OddRToAxial(enabledHex.ArrayIndex);
					arrayIndex = Point.AxialToOddR((flipAxis == "Y") ? Point.AxialReflection_AlongAxisR(p) : Point.AxialSymmetrical_OverAxisR(p));
				}
				rotation = num;
				MF.ArrayIndexToCartesianCoord(arrayIndex, 1f, 1f, out var x, out var y);
				if (Math.Abs(arrayIndex.X) > 3)
				{
					if (rotation == 300f)
					{
						rotation -= 5f;
						if (arrayIndex.Y < 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 60f)
					{
						rotation += 5f;
						if (arrayIndex.Y > 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 120f)
					{
						rotation -= 5f;
						if (arrayIndex.Y < 0)
						{
							num2 = -0.1f;
						}
					}
					if (rotation == 240f)
					{
						rotation += 5f;
						if (arrayIndex.Y > 0)
						{
							num2 = -0.1f;
						}
					}
				}
				float num3 = (float)Math.Cos((double)rotation * (Math.PI / 180.0));
				float num4 = (float)Math.Sin((double)rotation * (Math.PI / 180.0));
				float num5 = x * num3 - y * num4;
				float num6 = x * num4 + y * num3;
				MF.ArrayIndexToCartesianCoord(cTile.m_ArrayIndex, 1f, 1f, out var x2, out var y2);
				float num7 = x2 + num5 + num2;
				float val = y2 + num6;
				int num8 = (int)MF.SnapFloat(1f, val);
				int x3 = (((num8 & 1) != 1) ? ((int)MF.SnapFloat(1f, num7)) : ((int)(MF.SnapFloat(1f, num7 - 0.5f) + 0.5f)));
				Point item = new Point(x3, num8);
				if (!list2.Contains(item))
				{
					list2.Add(item);
					list3.Add(enabledHex.Flags);
				}
			}
			for (int num9 = 0; num9 < list2.Count; num9++)
			{
				Point point = list2[num9];
				if (point.X < 0 || point.X >= ScenarioManager.Tiles.GetLength(0) || point.Y < 0 || point.Y >= ScenarioManager.Tiles.GetLength(1))
				{
					continue;
				}
				CTile cTile2 = ScenarioManager.Tiles[point.X, point.Y];
				if (cTile2 == null)
				{
					continue;
				}
				List<Point> list4 = ScenarioManager.PathFinder.FindPath(null, actorOriginTile.m_ArrayIndex, cTile2.m_ArrayIndex, areaEffect.IgnoreBlocked, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks);
				if (foundPath && minRange != -1 && (list4.Count < minRange || list4.Count > range))
				{
					foundPath = false;
				}
				if ((foundPath | flag) || actorOriginTile.m_ArrayIndex == cTile2.m_ArrayIndex)
				{
					if ((ignoreLos | flag) || CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], cTile2))
					{
						list.Add(cTile2);
						validTilesIncludingBlocked?.Add(cTile2);
						flagsOfValidTiles?.TryAdd(cTile2, list3[num9]);
					}
					continue;
				}
				list4 = ScenarioManager.PathFinder.FindPath(null, actorOriginTile.m_ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks);
				if (foundPath && minRange != -1 && (list4.Count < minRange || list4.Count > range))
				{
					foundPath = false;
				}
				if (((foundPath | flag) || actorOriginTile.m_ArrayIndex == cTile2.m_ArrayIndex) && ((ignoreLos | flag) || CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], cTile2)))
				{
					validTilesIncludingBlocked?.Add(cTile2);
					flagsOfValidTiles?.TryAdd(cTile2, list3[num9]);
				}
			}
		}
		if (!flag && tilesInRange != null)
		{
			List<CTile> obj = validTilesIncludingBlocked;
			if (obj != null && !obj.Any((CTile item2) => tilesInRange.Contains(item2)))
			{
				list.Clear();
				validTilesIncludingBlocked?.Clear();
				flagsOfValidTiles?.Clear();
			}
		}
		return list;
	}

	public static List<CTile> GetRotatedHexTiles(CTile actorOriginTile, CTile positionTile, bool melee, List<CAreaEffectHex> hexList, float rotation)
	{
		CTile cTile = (melee ? actorOriginTile : positionTile);
		float num = rotation;
		float num2 = 0f;
		List<CTile> list = new List<CTile>(hexList.Count);
		if (cTile != null)
		{
			foreach (CAreaEffectHex hex in hexList)
			{
				rotation = num;
				MF.ArrayIndexToCartesianCoord(hex.ArrayIndex, 1f, 1f, out var x, out var y);
				if (Math.Abs(hex.ArrayIndex.X) > 3)
				{
					if (rotation == 300f)
					{
						rotation -= 5f;
						if (hex.ArrayIndex.Y < 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 60f)
					{
						rotation += 5f;
						if (hex.ArrayIndex.Y > 0)
						{
							num2 = 0.1f;
						}
					}
					if (rotation == 120f)
					{
						rotation -= 5f;
						if (hex.ArrayIndex.Y < 0)
						{
							num2 = -0.1f;
						}
					}
					if (rotation == 240f)
					{
						rotation += 5f;
						if (hex.ArrayIndex.Y > 0)
						{
							num2 = -0.1f;
						}
					}
				}
				float num3 = (float)Math.Cos((double)rotation * (Math.PI / 180.0));
				float num4 = (float)Math.Sin((double)rotation * (Math.PI / 180.0));
				float num5 = x * num3 - y * num4;
				float num6 = x * num4 + y * num3;
				MF.ArrayIndexToCartesianCoord(cTile.m_ArrayIndex, 1f, 1f, out var x2, out var y2);
				float num7 = x2 + num5 + num2;
				float val = y2 + num6;
				int num8 = (int)MF.SnapFloat(1f, val);
				int num9 = (((num8 & 1) != 1) ? ((int)MF.SnapFloat(1f, num7)) : ((int)(MF.SnapFloat(1f, num7 - 0.5f) + 0.5f)));
				if (num9 >= 0 && num9 < ScenarioManager.Width && num8 >= 0 && num8 < ScenarioManager.Height)
				{
					CTile cTile2 = ScenarioManager.Tiles[num9, num8];
					if (cTile2 != null)
					{
						list.Add(cTile2);
					}
				}
			}
		}
		return list.Distinct().ToList();
	}

	public static void GetTransformedEnemyHexesPositions(CAreaEffect areaEffect, Point originPoint, float rotation, bool flipped, List<Point> transformedPoints)
	{
		transformedPoints.Clear();
		float num = rotation;
		float num2 = 0f;
		MF.ArrayIndexToCartesianCoord(originPoint, 1f, 1f, out var x, out var y);
		foreach (CAreaEffectHex enabledHex in areaEffect.EnabledHexes)
		{
			if (!enabledHex.Flags.HasFlag(EHexFlag.Enemy))
			{
				continue;
			}
			Point arrayIndex = enabledHex.ArrayIndex;
			if (flipped)
			{
				arrayIndex = Point.AxialToOddR(Point.AxialSymmetrical_OverAxisR(Point.OddRToAxial(enabledHex.ArrayIndex)));
			}
			rotation = num;
			MF.ArrayIndexToCartesianCoord(arrayIndex, 1f, 1f, out var x2, out var y2);
			if (Math.Abs(arrayIndex.X) > 3)
			{
				if (rotation == 300f)
				{
					rotation -= 5f;
					if (arrayIndex.Y < 0)
					{
						num2 = 0.1f;
					}
				}
				if (rotation == 60f)
				{
					rotation += 5f;
					if (arrayIndex.Y > 0)
					{
						num2 = 0.1f;
					}
				}
				if (rotation == 120f)
				{
					rotation -= 5f;
					if (arrayIndex.Y < 0)
					{
						num2 = -0.1f;
					}
				}
				if (rotation == 240f)
				{
					rotation += 5f;
					if (arrayIndex.Y > 0)
					{
						num2 = -0.1f;
					}
				}
			}
			float num3 = (float)Math.Cos((double)rotation * (Math.PI / 180.0));
			float num4 = (float)Math.Sin((double)rotation * (Math.PI / 180.0));
			float num5 = x2 * num3 - y2 * num4;
			float num6 = x2 * num4 + y2 * num3;
			float num7 = x + num5 + num2;
			float val = y + num6;
			int num8 = (int)MF.SnapFloat(1f, val);
			int x3 = (((num8 & 1) != 1) ? ((int)MF.SnapFloat(1f, num7)) : ((int)(MF.SnapFloat(1f, num7 - 0.5f) + 0.5f)));
			Point item = new Point(x3, num8);
			transformedPoints.Add(item);
		}
	}

	public static List<CTile> GetValidTilesFromList(CTile actorOriginTile, List<CTile> tilesInRange, List<CTile> tiles, ref List<CTile> validTilesIncludingBlockedOut)
	{
		List<CTile> list = new List<CTile>();
		List<CTile> list2 = new List<CTile>();
		foreach (CTile tile in tiles)
		{
			ScenarioManager.PathFinder.FindPath(null, actorOriginTile.m_ArrayIndex, tile.m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
			if (foundPath)
			{
				if (CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], tile))
				{
					list.Add(tile);
					list2.Add(tile);
				}
			}
			else
			{
				ScenarioManager.PathFinder.FindPath(null, actorOriginTile.m_ArrayIndex, tile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false);
				if (foundPath && CActor.HaveLOS(ScenarioManager.Tiles[actorOriginTile.m_ArrayIndex.X, actorOriginTile.m_ArrayIndex.Y], tile))
				{
					list2.Add(tile);
				}
			}
		}
		if (tilesInRange != null && !list2.Any((CTile x) => tilesInRange.Contains(x)))
		{
			list.Clear();
			list2.Clear();
		}
		list = list.Distinct().ToList();
		list2 = list2.Distinct().ToList();
		validTilesIncludingBlockedOut = list2;
		return list;
	}
}
