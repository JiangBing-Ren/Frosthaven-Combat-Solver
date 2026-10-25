using UdpKit;

namespace FFSNet;

public class NetworkActionEventToken : IProtocolToken
{
	public IProtocolToken Token { get; set; }

	public void Read(UdpPacket packet)
	{
		Token = packet.ReadProtocolToken();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteProtocolToken(Token);
	}
}
