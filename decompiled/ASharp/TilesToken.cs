using System.Collections.Generic;
using AStar;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public class TilesToken : IProtocolToken
{
	private int tileCount;

	public int[,] Tiles { get; set; }

	public bool HasClickedTile { get; private set; }

	public int ClickedTileX { get; private set; }

	public int ClickedTileY { get; private set; }

	public TilesToken(CTile clickedTile, List<CTile> optionalTiles = null)
	{
		if (clickedTile == null)
		{
			tileCount = 0;
			Tiles = null;
			return;
		}
		if (optionalTiles != null)
		{
			tileCount = optionalTiles.Count + 1;
			Tiles = new int[tileCount, 2];
			for (int i = 1; i < tileCount; i++)
			{
				Tiles[i, 0] = optionalTiles[i - 1].m_ArrayIndex.X;
				Tiles[i, 1] = optionalTiles[i - 1].m_ArrayIndex.Y;
			}
		}
		else
		{
			tileCount = 1;
			Tiles = new int[tileCount, 2];
		}
		Tiles[0, 0] = clickedTile.m_ArrayIndex.X;
		Tiles[0, 1] = clickedTile.m_ArrayIndex.Y;
	}

	public TilesToken(List<Point> points, CTile clickedTile = null)
	{
		if (points == null || points.Count == 0)
		{
			tileCount = 0;
			Tiles = null;
		}
		else
		{
			tileCount = points.Count;
			Tiles = new int[tileCount, 2];
			for (int i = 0; i < tileCount; i++)
			{
				Tiles[i, 0] = points[i].X;
				Tiles[i, 1] = points[i].Y;
			}
		}
		if (clickedTile != null)
		{
			HasClickedTile = true;
			ClickedTileX = clickedTile.m_ArrayIndex.X;
			ClickedTileY = clickedTile.m_ArrayIndex.Y;
		}
	}

	public TilesToken()
	{
		tileCount = 0;
		Tiles = null;
	}

	public virtual void Write(UdpPacket packet)
	{
		packet.WriteInt(tileCount);
		for (int i = 0; i < tileCount; i++)
		{
			packet.WriteInt(Tiles[i, 0]);
			packet.WriteInt(Tiles[i, 1]);
		}
		packet.WriteBool(HasClickedTile);
		if (HasClickedTile)
		{
			packet.WriteInt(ClickedTileX);
			packet.WriteInt(ClickedTileY);
		}
	}

	public virtual void Read(UdpPacket packet)
	{
		tileCount = packet.ReadInt();
		if (tileCount == 0)
		{
			Tiles = null;
		}
		else
		{
			Tiles = new int[tileCount, 2];
			for (int i = 0; i < tileCount; i++)
			{
				Tiles[i, 0] = packet.ReadInt();
				Tiles[i, 1] = packet.ReadInt();
			}
		}
		HasClickedTile = packet.ReadBool();
		if (HasClickedTile)
		{
			ClickedTileX = packet.ReadInt();
			ClickedTileY = packet.ReadInt();
		}
	}
}
