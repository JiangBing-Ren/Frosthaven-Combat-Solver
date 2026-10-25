using UdpKit;

namespace FFSNet;

public class NetworkAction : IProtocolToken
{
	public int ActionTypeID { get; private set; }

	public int PlayerID { get; private set; }

	public IProtocolToken DataToken { get; private set; }

	public int TargetPlayerID { get; private set; }

	public int DataInt { get; private set; }

	public int DataInt2 { get; private set; }

	public bool DataBoolean { get; private set; }

	public string DataString { get; private set; }

	public NetworkAction(GameActionType actionType, NetworkPlayer actingPlayer, IProtocolToken actionDataToken, int targetPlayerID = 0, int dataInt = 0, int dataInt2 = 0, bool dataBoolean = false, string dataString = null)
	{
		ActionTypeID = (int)actionType;
		PlayerID = actingPlayer.PlayerID.ID;
		DataToken = actionDataToken;
		TargetPlayerID = targetPlayerID;
		DataInt = dataInt;
		DataInt2 = dataInt2;
		DataBoolean = dataBoolean;
		DataString = dataString;
	}

	public NetworkAction()
	{
		ActionTypeID = 0;
		PlayerID = 0;
		DataToken = null;
		TargetPlayerID = 0;
		DataInt = 0;
		DataInt2 = 0;
		DataBoolean = false;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(ActionTypeID);
		packet.WriteInt(PlayerID);
		packet.WriteInt(TargetPlayerID);
		packet.WriteInt(DataInt);
		packet.WriteInt(DataInt2);
		packet.WriteBool(DataBoolean);
		packet.WriteString(DataString);
		packet.WriteProtocolToken(DataToken);
	}

	public void Read(UdpPacket packet)
	{
		ActionTypeID = packet.ReadInt();
		PlayerID = packet.ReadInt();
		TargetPlayerID = packet.ReadInt();
		DataInt = packet.ReadInt();
		DataInt2 = packet.ReadInt();
		DataBoolean = packet.ReadBool();
		DataString = packet.ReadString();
		DataToken = packet.ReadProtocolToken();
	}
}
