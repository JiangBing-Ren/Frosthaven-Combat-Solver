using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class ItemToken : IProtocolToken
{
	private int chosenElementsCount;

	private int infusionElementsCount;

	public uint ItemNetworkID { get; set; }

	public int ItemSlotID { get; set; }

	public int ItemSlotIndex { get; set; }

	public int[] ChosenElement { get; set; }

	public int[] InfusionElements { get; set; }

	public ItemToken(uint itemNetworkID, int itemSlotID = -1, int itemSlotIndex = int.MaxValue, List<ElementInfusionBoardManager.EElement> chosenElement = null, List<ElementInfusionBoardManager.EElement> infusions = null)
	{
		Log.NetworkTrace("Creating a new ItemToken. Item NetworkID: " + itemNetworkID);
		ItemNetworkID = itemNetworkID;
		ItemSlotID = ((itemSlotID >= 0) ? itemSlotID : 0);
		ItemSlotIndex = itemSlotIndex;
		chosenElementsCount = chosenElement?.Count ?? 0;
		ChosenElement = new int[chosenElementsCount];
		for (int i = 0; i < chosenElementsCount; i++)
		{
			ChosenElement[i] = (int)chosenElement[i];
		}
		infusionElementsCount = infusions?.Count ?? 0;
		InfusionElements = new int[infusionElementsCount];
		for (int j = 0; j < infusionElementsCount; j++)
		{
			InfusionElements[j] = (int)infusions[j];
		}
	}

	public ItemToken()
	{
		ItemNetworkID = 0u;
		ItemSlotID = 0;
		ItemSlotIndex = int.MaxValue;
		chosenElementsCount = 0;
		ChosenElement = new int[chosenElementsCount];
		infusionElementsCount = 0;
		InfusionElements = new int[infusionElementsCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteUInt(ItemNetworkID);
		packet.WriteInt(ItemSlotID);
		packet.WriteInt(ItemSlotIndex);
		packet.WriteInt(chosenElementsCount);
		for (int i = 0; i < chosenElementsCount; i++)
		{
			packet.WriteInt(ChosenElement[i]);
		}
		packet.WriteInt(infusionElementsCount);
		for (int j = 0; j < infusionElementsCount; j++)
		{
			packet.WriteInt(InfusionElements[j]);
		}
	}

	public void Read(UdpPacket packet)
	{
		ItemNetworkID = packet.ReadUInt();
		ItemSlotID = packet.ReadInt();
		ItemSlotIndex = packet.ReadInt();
		chosenElementsCount = packet.ReadInt();
		ChosenElement = new int[chosenElementsCount];
		for (int i = 0; i < chosenElementsCount; i++)
		{
			ChosenElement[i] = packet.ReadInt();
		}
		infusionElementsCount = packet.ReadInt();
		InfusionElements = new int[infusionElementsCount];
		for (int j = 0; j < infusionElementsCount; j++)
		{
			InfusionElements[j] = packet.ReadInt();
		}
	}
}
