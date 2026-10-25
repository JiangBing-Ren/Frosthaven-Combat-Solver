using System.Text;
using FFSNet;
using UdpKit;

public sealed class LocationToken : IProtocolToken
{
	public string ID { get; private set; }

	public LocationToken(string nameID)
	{
		ID = nameID;
	}

	public LocationToken()
	{
		ID = string.Empty;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(ID, Encoding.ASCII);
	}

	public void Read(UdpPacket packet)
	{
		ID = packet.ReadString(Encoding.ASCII);
	}
}
