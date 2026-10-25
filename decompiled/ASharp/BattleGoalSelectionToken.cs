using System.Text;
using FFSNet;
using UdpKit;

public sealed class BattleGoalSelectionToken : IProtocolToken
{
	public string CharacterID { get; private set; }

	public string BattleGoalID { get; set; }

	public bool Selected { get; private set; }

	public string QuestID { get; private set; }

	public BattleGoalSelectionToken(string battleGoalId, string characterId, bool selected, string questId)
	{
		CharacterID = characterId;
		Selected = selected;
		BattleGoalID = battleGoalId;
		QuestID = questId;
	}

	public BattleGoalSelectionToken()
	{
		CharacterID = string.Empty;
		BattleGoalID = string.Empty;
		Selected = true;
		QuestID = string.Empty;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(CharacterID, Encoding.UTF8);
		packet.WriteString(BattleGoalID, Encoding.UTF8);
		packet.WriteString(QuestID, Encoding.UTF8);
		packet.WriteBool(Selected);
	}

	public void Read(UdpPacket packet)
	{
		CharacterID = packet.ReadString(Encoding.UTF8);
		BattleGoalID = packet.ReadString(Encoding.UTF8);
		QuestID = packet.ReadString(Encoding.UTF8);
		Selected = packet.ReadBool();
	}
}
