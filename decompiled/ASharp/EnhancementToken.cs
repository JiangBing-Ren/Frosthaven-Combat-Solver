using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class EnhancementToken : IProtocolToken
{
	public int CardID { get; private set; }

	public string AbilityName { get; private set; }

	public int EnhancementTypeID { get; private set; }

	public int EnhancementLineID { get; private set; }

	public int EnhancementSlot { get; private set; }

	public EnhancementToken(EnhancementButtonBase enhancementButton, EEnhancement enhancementType)
	{
		CardID = enhancementButton.AbilityCardID;
		AbilityName = enhancementButton.Ability.Name;
		EnhancementTypeID = (int)enhancementType;
		EnhancementLineID = (int)enhancementButton.EnhancementLine;
		EnhancementSlot = enhancementButton.EnhancementSlot;
	}

	public EnhancementToken()
	{
		CardID = 0;
		AbilityName = string.Empty;
		EnhancementTypeID = 0;
		EnhancementLineID = 0;
		EnhancementSlot = 0;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(CardID);
		packet.WriteString(AbilityName, Encoding.UTF8);
		packet.WriteInt(EnhancementTypeID);
		packet.WriteInt(EnhancementLineID);
		packet.WriteInt(EnhancementSlot);
	}

	public void Read(UdpPacket packet)
	{
		CardID = packet.ReadInt();
		AbilityName = packet.ReadString(Encoding.UTF8);
		EnhancementTypeID = packet.ReadInt();
		EnhancementLineID = packet.ReadInt();
		EnhancementSlot = packet.ReadInt();
	}

	public string GetDataString()
	{
		return "(CardID: " + CardID + ", AbilityName: " + AbilityName + ", EnhancementType: " + ((EEnhancement)EnhancementTypeID/*cast due to constrained. prefix*/).ToString() + ", EnhancementLine: " + ((EEnhancementLine)EnhancementLineID/*cast due to constrained. prefix*/).ToString() + ", EnhancementSlot: " + EnhancementSlot + ").";
	}
}
