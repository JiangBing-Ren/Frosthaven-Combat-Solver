using AStar;
using FFSNet;
using UdpKit;

public sealed class TileToken : StatePropertyToken
{
	private bool useCompression = true;

	public int TileX { get; set; }

	public int TileY { get; set; }

	public TileToken(Point point, bool useCompression = false)
	{
		TileX = point.X;
		TileY = point.Y;
		this.useCompression = useCompression;
	}

	public TileToken(int x, int y, bool useCompression = false)
	{
		TileX = x;
		TileY = y;
		this.useCompression = useCompression;
	}

	public TileToken()
	{
		TileX = 0;
		TileY = 0;
		useCompression = false;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteBool(useCompression);
		if (useCompression)
		{
			packet.WriteInt(TileX);
			packet.WriteInt(TileY);
		}
		else
		{
			packet.WriteInt(TileX);
			packet.WriteInt(TileY);
		}
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		useCompression = packet.ReadBool();
		if (useCompression)
		{
			TileX = packet.ReadInt();
			TileY = packet.ReadInt();
		}
		else
		{
			TileX = packet.ReadInt();
			TileY = packet.ReadInt();
		}
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Starting Tile X: " + TileX + ", Y: " + TileY);
	}
}
