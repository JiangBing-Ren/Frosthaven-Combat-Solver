using FFSNet;
using UdpKit;

public sealed class ConsoleCommandToken : StatePropertyToken
{
	public string cmd;

	public string selectedActorGUID;

	public int pickedX;

	public int pickedY;

	public ConsoleCommandToken(string cmd, string selectedActorGUID, int pickedX, int pickedY)
	{
		this.cmd = cmd;
		this.selectedActorGUID = selectedActorGUID;
		this.pickedX = pickedX;
		this.pickedY = pickedY;
	}

	public ConsoleCommandToken()
	{
		cmd = "";
		selectedActorGUID = "";
		pickedX = -1;
		pickedY = -1;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteString(cmd);
		packet.WriteString(selectedActorGUID);
		packet.WriteInt(pickedX);
		packet.WriteInt(pickedY);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		cmd = packet.ReadString();
		selectedActorGUID = packet.ReadString();
		pickedX = packet.ReadInt();
		pickedY = packet.ReadInt();
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo($"{customTitle}{GetRevisionString()} cmd: \"{cmd}\", selected: {selectedActorGUID}, picked: {pickedX}, {pickedY}");
	}
}
