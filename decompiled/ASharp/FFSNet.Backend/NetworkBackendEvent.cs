namespace FFSNet.Backend;

public record NetworkBackendEvent
{
	public enum Type
	{
		None,
		ConnectedToServer,
		DisconnectedFromServer,
		ConnectToServerFailed,
		CancelConnectingToServer,
		PlayerJoined,
		PlayerLeft,
		ProtocolTokenRecieved,
		ConnectionAccepted,
		ChangeConnectionState,
		GameConnectionEstablished,
		GameConnectionFailed,
		ShutdownCommand
	}

	public Type EventType;

	public object Data;

	public object Data2;

	public string GetString(int pos = 0)
	{
		return null;
	}

	public T Get<T>()
	{
		return (T)Data;
	}

	public T Get2<T>()
	{
		return (T)Data2;
	}

	public NetworkBackendEvent(Type type, object data = null, object data2 = null)
	{
		EventType = type;
		Data = data;
		Data2 = data2;
	}
}
