using FFSNet;
using UdpKit;

public class CampaignPersonalQuestData : IProtocolToken
{
	public string CharacterID;

	public string CharacterName;

	public string PersonalQuestID;

	public int PlayerID;

	public int SlotIndex;

	public bool SelectCharacter;

	public CampaignPersonalQuestData(string characterID, string personalQuestID, int playerID, int slotIndex, bool selectCharacter, string characterName)
	{
		CharacterID = characterID;
		PersonalQuestID = personalQuestID;
		PlayerID = playerID;
		SlotIndex = slotIndex;
		SelectCharacter = selectCharacter;
		CharacterName = characterName;
	}

	public CampaignPersonalQuestData()
	{
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(CharacterID);
		packet.WriteString(PersonalQuestID);
		packet.WriteInt(PlayerID);
		packet.WriteInt(SlotIndex);
		packet.WriteBool(SelectCharacter);
		packet.WriteString(CharacterName);
	}

	public void Read(UdpPacket packet)
	{
		CharacterID = packet.ReadString();
		PersonalQuestID = packet.ReadString();
		PlayerID = packet.ReadInt();
		SlotIndex = packet.ReadInt();
		SelectCharacter = packet.ReadBool();
		CharacterName = packet.ReadString();
	}
}
