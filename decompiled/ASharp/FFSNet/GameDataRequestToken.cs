using UdpKit;

namespace FFSNet;

public class GameDataRequestToken : IProtocolToken
{
	public int DataActionID { get; set; }

	public void Read(UdpPacket packet)
	{
		DataActionID = packet.ReadInt();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(DataActionID);
	}
}
