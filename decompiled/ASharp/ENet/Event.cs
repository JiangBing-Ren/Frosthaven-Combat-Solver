namespace ENet;

public struct Event
{
	private ENetEvent nativeEvent;

	internal ENetEvent NativeData
	{
		get
		{
			return nativeEvent;
		}
		set
		{
			nativeEvent = value;
		}
	}

	public EventType Type => nativeEvent.type;

	public Peer Peer => new Peer(nativeEvent.peer);

	public byte ChannelID => nativeEvent.channelID;

	public uint Data => nativeEvent.data;

	public Packet Packet => new Packet(nativeEvent.packet);

	internal Event(ENetEvent @event)
	{
		nativeEvent = @event;
	}
}
