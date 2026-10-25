using System.Collections.Generic;
using ScenarioRuleLibrary;
using UdpKit;

public class TargetSelectionToken : TilesToken
{
	public int TargetingAngle { get; private set; }

	public bool TargetingFlipped { get; private set; }

	public bool AoeLocked { get; private set; }

	public bool SecondClickToConfirmEnabled { get; set; }

	public TargetSelectionToken(CTile clickedTile, int targetingAngle = 0, bool targetingFlipped = false, bool aoeLocked = false, bool secondClickToConfirmEnabled = false, List<CTile> optionalTiles = null)
		: base(clickedTile, optionalTiles)
	{
		TargetingAngle = targetingAngle;
		TargetingFlipped = targetingFlipped;
		AoeLocked = aoeLocked;
		SecondClickToConfirmEnabled = secondClickToConfirmEnabled;
	}

	public TargetSelectionToken()
	{
		TargetingAngle = 0;
		TargetingFlipped = false;
		AoeLocked = false;
		SecondClickToConfirmEnabled = false;
	}

	public override void Write(UdpPacket packet)
	{
		packet.WriteInt(TargetingAngle);
		packet.WriteBool(TargetingFlipped);
		packet.WriteBool(AoeLocked);
		packet.WriteBool(SecondClickToConfirmEnabled);
		base.Write(packet);
	}

	public override void Read(UdpPacket packet)
	{
		TargetingAngle = packet.ReadInt();
		TargetingFlipped = packet.ReadBool();
		AoeLocked = packet.ReadBool();
		SecondClickToConfirmEnabled = packet.ReadBool();
		base.Read(packet);
	}
}
