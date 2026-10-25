using UdpKit;

namespace FFSNet;

public class CustomDataToken : IProtocolToken
{
	private bool compressedData;

	public byte[] CustomData { get; private set; }

	public CustomDataToken(byte[] customData, bool compressData = false)
	{
		CustomData = customData;
		compressedData = compressData;
	}

	public CustomDataToken()
	{
		CustomData = null;
	}

	public virtual void Write(UdpPacket packet)
	{
		packet.WriteBool(compressedData);
		byte[] array = (compressedData ? Utility.CompressData(CustomData) : CustomData);
		packet.WriteByteArrayWithPrefix(array);
	}

	public virtual void Read(UdpPacket packet)
	{
		compressedData = packet.ReadBool();
		byte[] array = packet.ReadByteArrayWithPrefix();
		if (array != null)
		{
			CustomData = (compressedData ? Utility.DecompressData(array) : array);
		}
	}
}
