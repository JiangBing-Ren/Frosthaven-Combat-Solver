namespace ENet;

public enum PeerState
{
	Uninitialized = -1,
	Disconnected,
	Connecting,
	AcknowledgingConnect,
	ConnectionPending,
	ConnectionSucceeded,
	Connected,
	DisconnectLater,
	Disconnecting,
	AcknowledgingDisconnect,
	Zombie
}
