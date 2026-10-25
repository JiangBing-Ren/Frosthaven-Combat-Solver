using FFSNet;
using UdpKit;

namespace Assets.Script.Networking.Tokens;

internal class RoadEventToken : IProtocolToken
{
	public string EventID;

	public string CurrentScreenName;

	public RoadEventToken()
	{
	}

	public RoadEventToken(string roadEventID, string currentScreenName)
	{
		EventID = roadEventID;
		CurrentScreenName = currentScreenName;
	}

	public void Read(UdpPacket packet)
	{
		EventID = packet.ReadString();
		CurrentScreenName = packet.ReadString();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(EventID);
		packet.WriteString(CurrentScreenName);
	}
}
