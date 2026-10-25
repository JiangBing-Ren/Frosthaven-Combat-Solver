using UdpKit;

namespace FFSNet.Backend;

public class ClientToClientToken : IProtocolToken
{
	public int Sender;

	public int Target;

	public IProtocolToken Token;

	public void Read(UdpPacket packet)
	{
		Sender = packet.ReadInt();
		Target = packet.ReadInt();
		Token = packet.ReadProtocolToken();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(Sender);
		packet.WriteInt(Target);
		packet.WriteProtocolToken(Token);
	}
}
