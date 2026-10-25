using FFSNet;
using FHRL;
using UdpKit;

public sealed class FHMultiplayerCommandToken : IProtocolToken
{
	public MultiplayerCommand Command { get; private set; }

	public FHMultiplayerCommandToken(MultiplayerCommand command)
	{
		Command = command;
	}

	public FHMultiplayerCommandToken()
	{
	}

	public void Write(UdpPacket packet)
	{
		byte[] array = FFSNet.Utility.CompressData(new StateSerializer<MultiplayerCommand>().BinarySerialize(Command));
		packet.WriteByteArrayWithPrefix(array);
	}

	public void Read(UdpPacket packet)
	{
		byte[] data = packet.ReadByteArrayWithPrefix();
		data = FFSNet.Utility.DecompressData(data);
		StateSerializer<MultiplayerCommand> stateSerializer = new StateSerializer<MultiplayerCommand>();
		Command = stateSerializer.BinaryDeserialzie(data);
	}
}
