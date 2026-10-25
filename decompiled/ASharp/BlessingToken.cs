using FFSNet;
using UdpKit;

public class BlessingToken : IProtocolToken
{
	public string CharacterID;

	public BlessingToken(string characterID)
	{
		CharacterID = characterID;
	}

	public BlessingToken()
	{
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(CharacterID);
	}

	public void Read(UdpPacket packet)
	{
		CharacterID = packet.ReadString();
	}
}
