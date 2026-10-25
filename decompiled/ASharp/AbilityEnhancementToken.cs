using FFSNet;
using UdpKit;

public sealed class AbilityEnhancementToken : IProtocolToken
{
	public int CardID { get; set; }

	public int ActionTypeID { get; set; }

	public int ConsumeIndex { get; set; }

	public bool UsesBothElements { get; set; }

	public int FirstElementTypeID { get; set; }

	public int SecondElementTypeID { get; set; }

	public AbilityEnhancementToken(int cardID, int actionTypeID, int consumeIndex, bool usesBothElements, int firstElementTypeID, int secondElementTypeID)
	{
		CardID = cardID;
		ActionTypeID = actionTypeID;
		ConsumeIndex = consumeIndex;
		UsesBothElements = usesBothElements;
		FirstElementTypeID = firstElementTypeID;
		SecondElementTypeID = secondElementTypeID;
	}

	public AbilityEnhancementToken()
	{
		CardID = 0;
		ActionTypeID = 0;
		ConsumeIndex = 0;
		UsesBothElements = true;
		FirstElementTypeID = 0;
		SecondElementTypeID = 0;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(CardID);
		packet.WriteInt(ActionTypeID);
		packet.WriteInt(ConsumeIndex);
		packet.WriteBool(UsesBothElements);
		packet.WriteInt(FirstElementTypeID);
		packet.WriteInt(SecondElementTypeID);
	}

	public void Read(UdpPacket packet)
	{
		CardID = packet.ReadInt();
		ActionTypeID = packet.ReadInt();
		ConsumeIndex = packet.ReadInt();
		UsesBothElements = packet.ReadBool();
		FirstElementTypeID = packet.ReadInt();
		SecondElementTypeID = packet.ReadInt();
	}
}
