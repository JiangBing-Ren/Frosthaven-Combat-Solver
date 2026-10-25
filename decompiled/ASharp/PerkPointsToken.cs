using FFSNet;
using UdpKit;

public sealed class PerkPointsToken : StatePropertyToken
{
	public int PerkPoints { get; private set; }

	public PerkPointsToken(int perkPoints)
	{
		PerkPoints = perkPoints;
	}

	public PerkPointsToken()
	{
		PerkPoints = 0;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(PerkPoints);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		PerkPoints = packet.ReadInt();
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Perk Points: " + PerkPoints);
	}
}
