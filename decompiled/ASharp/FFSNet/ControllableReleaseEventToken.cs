using UdpKit;

namespace FFSNet;

public class ControllableReleaseEventToken : IProtocolToken
{
	public int PlayerID { get; set; }

	public int ControllableID { get; set; }

	public void Read(UdpPacket packet)
	{
		PlayerID = packet.ReadInt();
		ControllableID = packet.ReadInt();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(PlayerID);
		packet.WriteInt(ControllableID);
	}
}
