using UdpKit;

namespace FFSNet;

public interface IProtocolToken
{
	void Read(UdpPacket packet);

	void Write(UdpPacket packet);
}
