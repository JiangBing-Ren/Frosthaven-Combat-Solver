using System.Text;
using FFSNet;
using UdpKit;

public sealed class AbilityAugmentGroupToken : IProtocolToken
{
	public string AugmentName { get; private set; }

	public string AugmentGroupID { get; private set; }

	public AbilityAugmentGroupToken(string augmentName, string augmentGroupID)
	{
		AugmentName = augmentName;
		AugmentGroupID = augmentGroupID;
	}

	public AbilityAugmentGroupToken()
	{
		AugmentName = string.Empty;
		AugmentGroupID = string.Empty;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(AugmentName, Encoding.UTF8);
		packet.WriteString(AugmentGroupID, Encoding.ASCII);
	}

	public void Read(UdpPacket packet)
	{
		AugmentName = packet.ReadString(Encoding.UTF8);
		AugmentGroupID = packet.ReadString(Encoding.ASCII);
	}
}
