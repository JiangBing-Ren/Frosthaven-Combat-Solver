using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Party;
using UdpKit;

public sealed class QuestCompletionToken : IProtocolToken
{
	public struct QuestCompletionData
	{
		public string QuestID;

		public int QuestCompletedScenarioLevel;

		public int QuestGainedXP;

		public int QuestGainedGold;
	}

	public string CharacterID;

	public string CharacterName;

	private int questCompletionCount;

	public QuestCompletionData[] QuestCompletions { get; private set; }

	public List<QuestCompletionData> QuestCompletionsList { get; private set; }

	public QuestCompletionToken(CMapCharacter localCharacter, CMapCharacter joinedGameCharacter)
	{
		Log.NetworkTrace("Creating a new QuestCompletionToken.");
		CharacterID = localCharacter.CharacterID;
		CharacterName = localCharacter.CharacterName;
		List<CCompletedSoloQuestData> list = new List<CCompletedSoloQuestData>();
		foreach (CCompletedSoloQuestData questData in localCharacter.CompletedSoloQuestData)
		{
			if (joinedGameCharacter.CompletedSoloQuestData.SingleOrDefault((CCompletedSoloQuestData x) => x.QuestID == questData.QuestID) == null)
			{
				list.Add(questData);
			}
		}
		questCompletionCount = list.Count;
		QuestCompletions = new QuestCompletionData[questCompletionCount];
		for (int num = 0; num < list.Count; num++)
		{
			CCompletedSoloQuestData cCompletedSoloQuestData = list[num];
			QuestCompletions[num].QuestID = cCompletedSoloQuestData.QuestID;
			QuestCompletions[num].QuestCompletedScenarioLevel = cCompletedSoloQuestData.QuestCompletionLevel;
			QuestCompletions[num].QuestGainedXP = cCompletedSoloQuestData.QuestGainedXP;
			QuestCompletions[num].QuestGainedGold = cCompletedSoloQuestData.QuestGainedGold;
		}
		QuestCompletionsList = QuestCompletions.ToList();
	}

	public QuestCompletionToken()
	{
		questCompletionCount = 0;
		QuestCompletions = new QuestCompletionData[questCompletionCount];
		QuestCompletionsList = QuestCompletions.ToList();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(CharacterID);
		packet.WriteString(CharacterName);
		packet.WriteInt(questCompletionCount);
		for (int i = 0; i < questCompletionCount; i++)
		{
			packet.WriteString(QuestCompletions[i].QuestID);
			packet.WriteInt(QuestCompletions[i].QuestCompletedScenarioLevel);
			packet.WriteInt(QuestCompletions[i].QuestGainedXP);
			packet.WriteInt(QuestCompletions[i].QuestGainedGold);
		}
	}

	public void Read(UdpPacket packet)
	{
		CharacterID = packet.ReadString();
		CharacterName = packet.ReadString();
		questCompletionCount = packet.ReadInt();
		QuestCompletions = new QuestCompletionData[questCompletionCount];
		for (int i = 0; i < questCompletionCount; i++)
		{
			QuestCompletions[i].QuestID = packet.ReadString();
			QuestCompletions[i].QuestCompletedScenarioLevel = packet.ReadInt();
			QuestCompletions[i].QuestGainedXP = packet.ReadInt();
			QuestCompletions[i].QuestGainedGold = packet.ReadInt();
		}
		QuestCompletionsList = QuestCompletions.ToList();
	}
}
