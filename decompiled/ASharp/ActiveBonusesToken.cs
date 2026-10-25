using System.Collections.Generic;
using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class ActiveBonusesToken : IProtocolToken
{
	public struct ActiveBonusData
	{
		public string AbilityName;

		public int BaseCardID;
	}

	private int activeBonusesCount;

	public ActiveBonusData[] ActiveBonuses { get; set; }

	public ActiveBonusesToken(List<CActiveBonus> activeBonuses)
	{
		Log.NetworkInfo("Creating an active bonus token. Active bonus count: " + activeBonuses.Count);
		activeBonusesCount = activeBonuses.Count;
		ActiveBonuses = new ActiveBonusData[activeBonusesCount];
		for (int i = 0; i < activeBonusesCount; i++)
		{
			ActiveBonuses[i].AbilityName = activeBonuses[i].Ability.Name;
			ActiveBonuses[i].BaseCardID = activeBonuses[i].BaseCard.ID;
			Log.NetworkInfo("Active bonus data added. AbilityName: " + ActiveBonuses[i].AbilityName + ", BaseCardID: " + ActiveBonuses[i].BaseCardID + ".");
		}
	}

	public ActiveBonusesToken()
	{
		activeBonusesCount = 0;
		ActiveBonuses = new ActiveBonusData[activeBonusesCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(activeBonusesCount);
		for (int i = 0; i < activeBonusesCount; i++)
		{
			packet.WriteString(ActiveBonuses[i].AbilityName, Encoding.UTF8);
			packet.WriteInt(ActiveBonuses[i].BaseCardID);
		}
	}

	public void Read(UdpPacket packet)
	{
		activeBonusesCount = packet.ReadInt();
		ActiveBonuses = new ActiveBonusData[activeBonusesCount];
		for (int i = 0; i < activeBonusesCount; i++)
		{
			ActiveBonuses[i].AbilityName = packet.ReadString(Encoding.UTF8);
			ActiveBonuses[i].BaseCardID = packet.ReadInt();
		}
	}
}
