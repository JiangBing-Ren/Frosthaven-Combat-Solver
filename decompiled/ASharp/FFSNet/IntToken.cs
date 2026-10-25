using UdpKit;

namespace FFSNet;

public class IntToken : IProtocolToken
{
	public int Data { get; private set; }

	public IntToken()
	{
	}

	public IntToken(int data)
	{
		Data = data;
	}

	public void Read(UdpPacket packet)
	{
		Data = packet.ReadInt();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(Data);
	}
}
