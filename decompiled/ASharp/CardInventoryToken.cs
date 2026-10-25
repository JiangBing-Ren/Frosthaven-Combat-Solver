using System.Collections.Generic;
using FFSNet;
using UdpKit;

public sealed class CardInventoryToken : StatePropertyToken
{
	private int selectedCardCount;

	private int unselectedCardCount;

	public int[] SelectedCardIDs { get; set; }

	public int[] UnselectedCardIDs { get; set; }

	public CardInventoryToken(List<int> selectedCardIDs, List<int> ownedCardIDs)
	{
		if (ownedCardIDs.IsNullOrEmpty())
		{
			selectedCardCount = 0;
			unselectedCardCount = 0;
			SelectedCardIDs = new int[selectedCardCount];
			UnselectedCardIDs = new int[unselectedCardCount];
			return;
		}
		List<int> list = new List<int>(ownedCardIDs);
		list.RemoveAll((int x) => selectedCardIDs.Contains(x));
		if (list.IsNullOrEmpty())
		{
			UnselectedCardIDs = new int[0];
		}
		else
		{
			UnselectedCardIDs = list.ToArray();
		}
		if (selectedCardIDs.IsNullOrEmpty())
		{
			SelectedCardIDs = new int[0];
		}
		else
		{
			SelectedCardIDs = selectedCardIDs.ToArray();
		}
		selectedCardCount = SelectedCardIDs.Length;
		unselectedCardCount = UnselectedCardIDs.Length;
	}

	public CardInventoryToken()
	{
		selectedCardCount = 0;
		unselectedCardCount = 0;
		SelectedCardIDs = new int[selectedCardCount];
		UnselectedCardIDs = new int[unselectedCardCount];
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(selectedCardCount);
		packet.WriteInt(unselectedCardCount);
		for (int i = 0; i < selectedCardCount; i++)
		{
			packet.WriteInt(SelectedCardIDs[i]);
		}
		for (int j = 0; j < unselectedCardCount; j++)
		{
			packet.WriteInt(UnselectedCardIDs[j]);
		}
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		selectedCardCount = packet.ReadInt();
		unselectedCardCount = packet.ReadInt();
		SelectedCardIDs = new int[selectedCardCount];
		UnselectedCardIDs = new int[unselectedCardCount];
		for (int i = 0; i < selectedCardCount; i++)
		{
			SelectedCardIDs[i] = packet.ReadInt();
		}
		for (int j = 0; j < unselectedCardCount; j++)
		{
			UnselectedCardIDs[j] = packet.ReadInt();
		}
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Selected Card IDs: " + SelectedCardIDs.ToStringPretty() + ". Unselected Card IDs: " + UnselectedCardIDs.ToStringPretty());
	}
}
