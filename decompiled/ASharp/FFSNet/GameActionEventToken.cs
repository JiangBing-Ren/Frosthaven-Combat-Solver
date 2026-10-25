using UdpKit;

namespace FFSNet;

public class GameActionEventToken : IProtocolToken
{
	public int ActionID { get; set; }

	public int ActionTypeID { get; set; }

	public int PlayerID { get; set; }

	public int ActorID { get; set; }

	public int TargetPhaseID { get; set; }

	public int SupplementaryDataIDMin { get; set; }

	public int SupplementaryDataIDMed { get; set; }

	public int SupplementaryDataIDMax { get; set; }

	public bool SupplementaryDataBoolean { get; set; }

	public string SupplementaryDataString { get; set; }

	public IProtocolToken SupplementaryDataToken { get; set; }

	public IProtocolToken SupplementaryDataToken2 { get; set; }

	public IProtocolToken SupplementaryDataToken3 { get; set; }

	public IProtocolToken SupplementaryDataToken4 { get; set; }

	public bool ValidateAction { get; set; }

	public bool DoNotForwardAction { get; set; }

	public void Read(UdpPacket packet)
	{
		ActionID = packet.ReadInt();
		ActionTypeID = packet.ReadInt();
		PlayerID = packet.ReadInt();
		ActorID = packet.ReadInt();
		TargetPhaseID = packet.ReadInt();
		SupplementaryDataIDMin = packet.ReadInt();
		SupplementaryDataIDMed = packet.ReadInt();
		SupplementaryDataIDMax = packet.ReadInt();
		SupplementaryDataBoolean = packet.ReadBool();
		SupplementaryDataString = packet.ReadString();
		SupplementaryDataToken = packet.ReadProtocolToken();
		SupplementaryDataToken2 = packet.ReadProtocolToken();
		SupplementaryDataToken3 = packet.ReadProtocolToken();
		SupplementaryDataToken4 = packet.ReadProtocolToken();
		ValidateAction = packet.ReadBool();
		DoNotForwardAction = packet.ReadBool();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ActionID);
		packet.WriteInt(ActionTypeID);
		packet.WriteInt(PlayerID);
		packet.WriteInt(ActorID);
		packet.WriteInt(TargetPhaseID);
		packet.WriteInt(SupplementaryDataIDMin);
		packet.WriteInt(SupplementaryDataIDMed);
		packet.WriteInt(SupplementaryDataIDMax);
		packet.WriteBool(SupplementaryDataBoolean);
		packet.WriteString(SupplementaryDataString);
		packet.WriteProtocolToken(SupplementaryDataToken);
		packet.WriteProtocolToken(SupplementaryDataToken2);
		packet.WriteProtocolToken(SupplementaryDataToken3);
		packet.WriteProtocolToken(SupplementaryDataToken4);
		packet.WriteBool(ValidateAction);
		packet.WriteBool(DoNotForwardAction);
	}
}
