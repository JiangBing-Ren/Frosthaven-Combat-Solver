using Epic.OnlineServices;

namespace FFSNet.Backend;

public class NetworkBackendPlayer
{
	public int PlayerID;

	public readonly string Username;

	public readonly string PlatformID;

	internal object AdditionalData;

	internal ProductUserId EosUserId
	{
		get
		{
			return AdditionalData as ProductUserId;
		}
		set
		{
			AdditionalData = value;
		}
	}

	public bool Disconnected { get; internal set; }

	public NetworkBackendPlayer(string username, string platformID)
	{
		Username = username;
		PlatformID = platformID;
	}

	public NetworkBackendPlayer(UserToken userToken)
		: this(userToken.Username, userToken.PlatformPlayerID)
	{
	}

	public override string ToString()
	{
		return $"{PlayerID}: {Username}";
	}

	public PlayerToken GeneratePlayerToken(bool isPlayerHost, bool isLocalPlayer)
	{
		return new PlayerToken(PlayerID, Username, isPlayerHost, isLocalPlayer)
		{
			EosUserID = (EosUserId?.ToString() ?? string.Empty)
		};
	}

	public bool SameUserToken(UserToken userToken)
	{
		return userToken.Username == Username;
	}
}
