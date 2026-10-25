using UdpKit;

namespace FFSNet;

public class SlotStateToken : IProtocolToken
{
	public int[] SlotOwnership = new int[4];

	public int[] SlotHeroAssignment = new int[4];

	public void Read(UdpPacket packet)
	{
		for (int i = 0; i < SlotOwnership.Length; i++)
		{
			SlotOwnership[i] = packet.ReadInt();
		}
		for (int j = 0; j < SlotHeroAssignment.Length; j++)
		{
			SlotHeroAssignment[j] = packet.ReadInt();
		}
	}

	public void Write(UdpPacket packet)
	{
		for (int i = 0; i < SlotOwnership.Length; i++)
		{
			packet.WriteInt(SlotOwnership[i]);
		}
		for (int j = 0; j < SlotHeroAssignment.Length; j++)
		{
			packet.WriteInt(SlotHeroAssignment[j]);
		}
	}
}
