using FFSNet;
using UdpKit;

public sealed class CampaignCharacterData : IProtocolToken
{
	public string CharacterID;

	public string CharacterName;

	public CampaignCharacterData(string characterID, string characterName)
	{
		CharacterID = characterID;
		CharacterName = characterName;
	}

	public CampaignCharacterData()
	{
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(CharacterID);
		packet.WriteString(CharacterName);
	}

	public void Read(UdpPacket packet)
	{
		CharacterID = packet.ReadString();
		CharacterName = packet.ReadString();
	}
}
