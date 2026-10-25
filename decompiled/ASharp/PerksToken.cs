using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary.YML;
using UdpKit;

public sealed class PerksToken : StatePropertyToken
{
	private int activePerkCount;

	public int[] ActivePerkIndexes { get; set; }

	public PerksToken(List<CharacterPerk> perks)
	{
		if (perks != null)
		{
			ActivePerkIndexes = (from x in perks.Select((CharacterPerk perk, int index) => new { perk, index })
				where x.perk.IsActive
				select x.index).ToArray();
			activePerkCount = ActivePerkIndexes.Length;
		}
		else
		{
			activePerkCount = 0;
			ActivePerkIndexes = new int[activePerkCount];
		}
	}

	public PerksToken()
	{
		activePerkCount = 0;
		ActivePerkIndexes = new int[activePerkCount];
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(activePerkCount);
		for (int i = 0; i < activePerkCount; i++)
		{
			packet.WriteInt(ActivePerkIndexes[i]);
		}
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		activePerkCount = packet.ReadInt();
		ActivePerkIndexes = new int[activePerkCount];
		for (int i = 0; i < activePerkCount; i++)
		{
			ActivePerkIndexes[i] = packet.ReadInt();
		}
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Active Perks: " + ActivePerkIndexes.ToStringPretty());
	}
}
