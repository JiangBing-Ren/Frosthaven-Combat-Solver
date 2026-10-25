using UdpKit;

namespace FFSNet.Backend;

public class EosClientToClientToken : IProtocolToken
{
	public string Sender;

	public string Target;

	public IProtocolToken Token;

	public void Read(UdpPacket packet)
	{
		Sender = packet.ReadString();
		Target = packet.ReadString();
		Token = packet.ReadProtocolToken();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(Sender);
		packet.WriteString(Target);
		packet.WriteProtocolToken(Token);
	}
}
