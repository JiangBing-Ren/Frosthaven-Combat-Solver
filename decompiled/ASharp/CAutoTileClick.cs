using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("SelectedTile: {SelectedTile}")]
public class CAutoTileClick : CAuto, ISerializable
{
	public TileIndex SelectedTile;

	public List<TileIndex> OptionalTileList;

	public List<TileIndex> Waypoints;

	public TileIndex PlacementTile;

	public CInteractable.EInputType InputType;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SelectedTile", SelectedTile);
		info.AddValue("OptionalTileList", OptionalTileList);
		info.AddValue("Waypoints", Waypoints);
		info.AddValue("PlacementTile", PlacementTile);
		info.AddValue("InputType", InputType);
	}

	protected CAutoTileClick(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "SelectedTile":
					SelectedTile = (TileIndex)info.GetValue("SelectedTile", typeof(TileIndex));
					break;
				case "OptionalTileList":
					OptionalTileList = (List<TileIndex>)info.GetValue("OptionalTileList", typeof(List<TileIndex>));
					break;
				case "Waypoints":
					Waypoints = (List<TileIndex>)info.GetValue("Waypoints", typeof(List<TileIndex>));
					break;
				case "PlacementTile":
					PlacementTile = (TileIndex)info.GetValue("PlacementTile", typeof(TileIndex));
					break;
				case "InputType":
					InputType = (CInteractable.EInputType)info.GetValue("InputType", typeof(CInteractable.EInputType));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize CAutoTileClick entry " + item.Name);
				throw ex;
			}
		}
	}

	public CAutoTileClick(int id, TileIndex selectedTile, List<TileIndex> optionalTileList, List<TileIndex> waypoints, TileIndex placementTile, CInteractable.EInputType inputType)
		: base(EAutoType.TileClick, id)
	{
		SelectedTile = selectedTile;
		OptionalTileList = optionalTileList;
		Waypoints = waypoints;
		PlacementTile = placementTile;
		InputType = inputType;
	}

	public static List<TileIndex> CTileToTileIndexList(List<CTile> tileList)
	{
		if (tileList == null)
		{
			return null;
		}
		List<TileIndex> list = new List<TileIndex>();
		foreach (CTile tile in tileList)
		{
			list.Add(new TileIndex(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y));
		}
		return list;
	}

	public static List<CTile> TileIndexToCTileList(List<TileIndex> tileIndexList)
	{
		if (tileIndexList == null)
		{
			return null;
		}
		List<CTile> list = new List<CTile>();
		foreach (TileIndex tileIndex in tileIndexList)
		{
			list.Add(ScenarioManager.Tiles[tileIndex.X, tileIndex.Y]);
		}
		return list;
	}

	public static CClientTile TileIndexToClientTile(TileIndex tileIndex)
	{
		if (tileIndex == null)
		{
			return null;
		}
		return ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
	}

	public static TileIndex ClientTileToTileIndex(CClientTile clientTile)
	{
		if (clientTile == null)
		{
			return null;
		}
		return new TileIndex(clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y);
	}

	public static List<TileIndex> AStarToTileIndexList(List<Point> aStarList)
	{
		if (aStarList == null)
		{
			return null;
		}
		List<TileIndex> list = new List<TileIndex>();
		foreach (Point aStar in aStarList)
		{
			list.Add(new TileIndex(aStar.X, aStar.Y));
		}
		return list;
	}

	public static List<Point> TileIndexToAStarList(List<TileIndex> tileIndexList)
	{
		if (tileIndexList == null)
		{
			return null;
		}
		List<Point> list = new List<Point>();
		foreach (TileIndex tileIndex in tileIndexList)
		{
			list.Add(new Point(tileIndex.X, tileIndex.Y));
		}
		return list;
	}
}
