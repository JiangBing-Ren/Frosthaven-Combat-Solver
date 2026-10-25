using System.Text;
using ScenarioRuleLibrary;
using UdpKit;

namespace FFSNet;

public sealed class PlayerToken : IProtocolToken, IDumpState
{
	public int PlayerID { get; set; }

	public string Username { get; set; }

	public bool IsHost { get; set; }

	public bool IsLocalPlayer { get; set; }

	public string EosUserID { get; set; }

	public PlayerToken(int playerID, string userName, bool isHost, bool isLocalPlayer)
	{
		PlayerID = playerID;
		Username = userName;
		IsHost = isHost;
		IsLocalPlayer = isLocalPlayer;
		EosUserID = "";
	}

	public PlayerToken()
	{
		PlayerID = 0;
		Username = "NONE";
		IsHost = true;
		IsLocalPlayer = true;
		EosUserID = "";
	}

	public override string ToString()
	{
		string text = (IsHost ? "Host " : "Client ");
		text += PlayerID;
		if (IsLocalPlayer)
		{
			text += ", Local player";
		}
		text = text + ": " + Username;
		return text + ", EosUserID: " + EosUserID;
	}

	public string ToStateDumpString(StateDump dump)
	{
		return string.Concat((IsHost ? "Host " : "Client ") + PlayerID, " (", Username, ")");
	}

	public void DumpMembers(StateDump dump)
	{
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(PlayerID);
		packet.WriteBool(IsHost);
		packet.WriteBool(IsLocalPlayer);
		packet.WriteString(Username, Encoding.UTF8);
		packet.WriteString(EosUserID, Encoding.UTF8);
	}

	public void Read(UdpPacket packet)
	{
		PlayerID = packet.ReadInt();
		IsHost = packet.ReadBool();
		IsLocalPlayer = packet.ReadBool();
		Username = packet.ReadString(Encoding.UTF8);
		EosUserID = packet.ReadString(Encoding.UTF8);
	}
}
