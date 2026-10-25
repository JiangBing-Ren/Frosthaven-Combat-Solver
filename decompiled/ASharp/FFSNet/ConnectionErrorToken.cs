using UdpKit;

namespace FFSNet;

public sealed class ConnectionErrorToken : IProtocolToken
{
	public int ErrorCode { get; private set; }

	public ConnectionErrorToken(ConnectionErrorCode errorCode)
	{
		ErrorCode = (int)errorCode;
	}

	public ConnectionErrorToken()
	{
		ErrorCode = 0;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ErrorCode);
	}

	public void Read(UdpPacket packet)
	{
		ErrorCode = packet.ReadInt();
	}
}
