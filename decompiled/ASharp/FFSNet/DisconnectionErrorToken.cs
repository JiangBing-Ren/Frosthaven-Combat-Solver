using UdpKit;

namespace FFSNet;

public sealed class DisconnectionErrorToken : IProtocolToken
{
	public int ErrorCode { get; private set; }

	public DisconnectionErrorToken(DisconnectionErrorCode errorCode)
	{
		ErrorCode = (int)errorCode;
	}

	public DisconnectionErrorToken()
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
