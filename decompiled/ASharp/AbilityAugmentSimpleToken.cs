using System.Collections.Generic;
using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class AbilityAugmentSimpleToken : IProtocolToken
{
	private int elementsCount;

	public string AugmentName { get; private set; }

	public int[] Elements { get; set; }

	public AbilityAugmentSimpleToken(string augmentName, List<ElementInfusionBoardManager.EElement> elements)
	{
		AugmentName = augmentName;
		elementsCount = elements?.Count ?? 0;
		Elements = new int[elementsCount];
		for (int i = 0; i < elementsCount; i++)
		{
			Elements[i] = (int)elements[i];
		}
	}

	public AbilityAugmentSimpleToken()
	{
		AugmentName = string.Empty;
		elementsCount = 0;
		Elements = new int[elementsCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(AugmentName, Encoding.UTF8);
		packet.WriteInt(elementsCount);
		for (int i = 0; i < elementsCount; i++)
		{
			packet.WriteInt(Elements[i]);
		}
	}

	public void Read(UdpPacket packet)
	{
		AugmentName = packet.ReadString(Encoding.UTF8);
		elementsCount = packet.ReadInt();
		Elements = new int[elementsCount];
		for (int i = 0; i < elementsCount; i++)
		{
			Elements[i] = packet.ReadInt();
		}
	}
}
