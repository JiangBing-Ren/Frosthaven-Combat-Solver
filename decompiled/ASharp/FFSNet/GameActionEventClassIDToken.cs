using UdpKit;

namespace FFSNet;

public class GameActionEventClassIDToken : IProtocolToken
{
	public int ActionID { get; set; }

	public int ActionTypeID { get; set; }

	public int PlayerID { get; set; }

	public int ActorID { get; set; }

	public int TargetPhaseID { get; set; }

	public int SupplementaryDataIDMin { get; set; }

	public bool SupplementaryDataBoolean { get; set; }

	public string ClassID { get; set; }

	public void Read(UdpPacket packet)
	{
		ActionID = packet.ReadInt();
		ActionTypeID = packet.ReadInt();
		PlayerID = packet.ReadInt();
		ActorID = packet.ReadInt();
		TargetPhaseID = packet.ReadInt();
		SupplementaryDataIDMin = packet.ReadInt();
		SupplementaryDataBoolean = packet.ReadBool();
		ClassID = packet.ReadString();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ActionID);
		packet.WriteInt(ActionTypeID);
		packet.WriteInt(PlayerID);
		packet.WriteInt(ActorID);
		packet.WriteInt(TargetPhaseID);
		packet.WriteInt(SupplementaryDataIDMin);
		packet.WriteBool(SupplementaryDataBoolean);
		packet.WriteString(ClassID);
	}
}
