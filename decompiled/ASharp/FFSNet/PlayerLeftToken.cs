using UdpKit;

namespace FFSNet;

public class PlayerLeftToken : IProtocolToken
{
	public int PlayerID;

	public void Read(UdpPacket packet)
	{
		PlayerID = packet.ReadInt();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(PlayerID);
	}
}
