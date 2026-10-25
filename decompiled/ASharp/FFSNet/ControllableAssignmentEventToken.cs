using UdpKit;

namespace FFSNet;

public class ControllableAssignmentEventToken : IProtocolToken
{
	public int PlayerID { get; set; }

	public int ControllableID { get; set; }

	public bool ReleaseFirst { get; set; }

	public void Read(UdpPacket packet)
	{
		PlayerID = packet.ReadInt();
		ControllableID = packet.ReadInt();
		ReleaseFirst = packet.ReadBool();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(PlayerID);
		packet.WriteInt(ControllableID);
		packet.WriteBool(ReleaseFirst);
	}
}
