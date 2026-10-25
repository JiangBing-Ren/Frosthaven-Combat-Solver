using UdpKit;

namespace FFSNet;

public class ControllableDestructionEventToken : IProtocolToken
{
	public int ControllableID { get; set; }

	public void Read(UdpPacket packet)
	{
		ControllableID = packet.ReadInt();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ControllableID);
	}
}
