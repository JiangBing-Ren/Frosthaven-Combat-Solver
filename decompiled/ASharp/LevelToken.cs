using FFSNet;
using UdpKit;

public sealed class LevelToken : StatePropertyToken
{
	public int Level { get; private set; }

	public LevelToken(int level)
	{
		Level = level;
	}

	public LevelToken()
	{
		Level = 0;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(Level);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		Level = packet.ReadInt();
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Character level: " + Level);
	}
}
