using FFSNet;
using UdpKit;

public class ReadyUpToken : IProtocolToken
{
	public string ToggleState;

	public ReadyUpToken(string toggleState)
	{
		ToggleState = toggleState;
	}

	public ReadyUpToken()
	{
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(ToggleState);
	}

	public void Read(UdpPacket packet)
	{
		ToggleState = packet.ReadString();
	}
}
