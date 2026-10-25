using UdpKit;

namespace FFSNet.Backend;

public class DebugMessageToken : IProtocolToken
{
	public string Message = string.Empty;

	public void Read(UdpPacket packet)
	{
		Message = packet.ReadString();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(Message);
	}
}
