using System.Text;
using UdpKit;

namespace FFSNet;

public sealed class UserToken : IProtocolToken
{
	public int BuildType { get; private set; }

	public string ServerPassword { get; private set; }

	public string Username { get; private set; }

	public string PlatformPlayerID { get; private set; }

	public string GameVersion { get; private set; }

	public string CurrentMod { get; private set; }

	public UserToken(int buildType, string serverPassword, string username, string platformPlayerID, string gameVersion, string currentMod)
	{
		BuildType = buildType;
		ServerPassword = serverPassword;
		Username = username;
		PlatformPlayerID = platformPlayerID;
		GameVersion = gameVersion;
		CurrentMod = currentMod;
	}

	public UserToken()
	{
		BuildType = 0;
		ServerPassword = string.Empty;
		Username = string.Empty;
		PlatformPlayerID = string.Empty;
		GameVersion = string.Empty;
		CurrentMod = string.Empty;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(BuildType);
		packet.WriteString(ServerPassword, Encoding.ASCII);
		packet.WriteString(Username, Encoding.UTF8);
		packet.WriteString(PlatformPlayerID, Encoding.UTF8);
		packet.WriteString(GameVersion, Encoding.ASCII);
		packet.WriteString(CurrentMod, Encoding.ASCII);
	}

	public void Read(UdpPacket packet)
	{
		BuildType = packet.ReadInt();
		ServerPassword = packet.ReadString(Encoding.ASCII);
		Username = packet.ReadString(Encoding.UTF8);
		PlatformPlayerID = packet.ReadString(Encoding.UTF8);
		GameVersion = packet.ReadString(Encoding.ASCII);
		CurrentMod = packet.ReadString(Encoding.ASCII);
	}
}
