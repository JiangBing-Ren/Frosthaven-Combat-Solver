using UdpKit;

namespace FFSNet;

public sealed class ControllableToken : IProtocolToken
{
	public int ControllableID { get; set; }

	public ControllableToken(int controllableID)
	{
		ControllableID = controllableID;
	}

	public ControllableToken()
	{
		ControllableID = 0;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ControllableID);
	}

	public void Read(UdpPacket packet)
	{
		ControllableID = packet.ReadInt();
	}
}
