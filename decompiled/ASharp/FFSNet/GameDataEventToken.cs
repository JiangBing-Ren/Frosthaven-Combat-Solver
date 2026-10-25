using UdpKit;

namespace FFSNet;

public class GameDataEventToken : IProtocolToken
{
	public int DataActionID { get; set; }

	public int ChunkSize { get; set; }

	public int ChunkIndex { get; set; }

	public int TotalSize { get; set; }

	public bool Complete { get; set; }

	public void Read(UdpPacket packet)
	{
		DataActionID = packet.ReadInt();
		ChunkSize = packet.ReadInt();
		ChunkIndex = packet.ReadInt();
		TotalSize = packet.ReadInt();
		Complete = packet.ReadBool();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(DataActionID);
		packet.WriteInt(ChunkSize);
		packet.WriteInt(ChunkIndex);
		packet.WriteInt(TotalSize);
		packet.WriteBool(Complete);
	}
}
