using FFSNet;
using UdpKit;

public sealed class ShortRestToken : StatePropertyToken
{
	public int CardID { get; set; }

	public bool CardRedrawn { get; set; }

	public bool ImprovedVersion { get; set; }

	public ShortRestToken(int cardID, bool cardRedrawn = false, bool improvedVersion = false)
	{
		CardID = cardID;
		CardRedrawn = cardRedrawn;
		ImprovedVersion = improvedVersion;
	}

	public ShortRestToken()
	{
		CardID = 0;
		CardRedrawn = false;
		ImprovedVersion = false;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(CardID);
		packet.WriteBool(CardRedrawn);
		packet.WriteBool(ImprovedVersion);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		CardID = packet.ReadInt();
		CardRedrawn = packet.ReadBool();
		ImprovedVersion = packet.ReadBool();
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Short Rest used. Discarded card ID: " + CardID + ". Card redrawn: " + CardRedrawn + ". Improved version: " + ImprovedVersion);
	}
}
