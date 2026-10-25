using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class ItemsToken : IProtocolToken
{
	private int itemCount;

	public uint[] ItemNetworkIDs { get; set; }

	public ItemsToken(List<CItem> items)
	{
		Log.NetworkTrace("Creating a new ItemsToken.");
		if (items != null)
		{
			itemCount = items.Count;
			ItemNetworkIDs = new uint[itemCount];
			for (int i = 0; i < itemCount; i++)
			{
				ItemNetworkIDs[i] = items[i].NetworkID;
				Log.NetworkTrace("Item NetworkID: " + ItemNetworkIDs[i] + ", SlotState: " + items[i].SlotState);
			}
		}
		else
		{
			itemCount = 0;
			ItemNetworkIDs = new uint[itemCount];
		}
	}

	public ItemsToken()
	{
		itemCount = 0;
		ItemNetworkIDs = new uint[itemCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(itemCount);
		for (int i = 0; i < itemCount; i++)
		{
			packet.WriteUInt(ItemNetworkIDs[i]);
		}
	}

	public void Read(UdpPacket packet)
	{
		itemCount = packet.ReadInt();
		ItemNetworkIDs = new uint[itemCount];
		for (int i = 0; i < itemCount; i++)
		{
			ItemNetworkIDs[i] = packet.ReadUInt();
		}
	}
}
