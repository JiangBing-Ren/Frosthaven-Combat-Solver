using System.Collections.Generic;
using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class InfuseToken : IProtocolToken
{
	private int elementsCount;

	public string AbilityID { get; set; }

	public bool IsConsume { get; set; }

	public bool IsDeselect { get; set; }

	public int[] Elements { get; set; }

	public InfuseToken(string abilityID, bool isConsume, bool isDeselect, List<ElementInfusionBoardManager.EElement> elements)
	{
		AbilityID = abilityID;
		IsConsume = isConsume;
		IsDeselect = isDeselect;
		elementsCount = elements?.Count ?? 0;
		Elements = new int[elementsCount];
		for (int i = 0; i < elementsCount; i++)
		{
			Elements[i] = (int)elements[i];
		}
	}

	public InfuseToken()
	{
		AbilityID = string.Empty;
		IsConsume = false;
		IsDeselect = false;
		elementsCount = 0;
		Elements = new int[elementsCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(AbilityID, Encoding.ASCII);
		packet.WriteBool(IsConsume);
		packet.WriteBool(IsDeselect);
		packet.WriteInt(elementsCount);
		for (int i = 0; i < elementsCount; i++)
		{
			packet.WriteInt(Elements[i]);
		}
	}

	public void Read(UdpPacket packet)
	{
		AbilityID = packet.ReadString(Encoding.ASCII);
		IsConsume = packet.ReadBool();
		IsDeselect = packet.ReadBool();
		elementsCount = packet.ReadInt();
		Elements = new int[elementsCount];
		for (int i = 0; i < elementsCount; i++)
		{
			Elements[i] = packet.ReadInt();
		}
	}
}
