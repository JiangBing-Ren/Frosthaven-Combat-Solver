namespace FFSNet.Backend;

public class NetworkBackendConnectRequest
{
	public readonly UserToken UserToken;

	public ConnectionErrorToken ErrorToken { get; private set; }

	public bool Accepted { get; set; }

	public NetworkBackendConnectRequest(UserToken user)
	{
		UserToken = user;
	}

	public void Accept()
	{
		Accepted = true;
		ErrorToken = null;
	}

	public void Refuse(ConnectionErrorToken errorToken)
	{
		ErrorToken = errorToken;
		Accepted = false;
	}
}
