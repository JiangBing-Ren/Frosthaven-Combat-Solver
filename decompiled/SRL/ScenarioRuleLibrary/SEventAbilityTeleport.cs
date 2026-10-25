using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTeleport : SEventAbility
{
	public CAbilityTeleport.ETeleportState TeleportState { get; private set; }

	public TileIndex MovedFromPoint { get; private set; }

	public TileIndex MovedToPoint { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TeleportState", TeleportState);
		info.AddValue("MovedFromPoint", MovedFromPoint);
		info.AddValue("MovedToPoint", MovedToPoint);
	}

	public SEventAbilityTeleport(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TeleportState":
					TeleportState = (CAbilityTeleport.ETeleportState)info.GetValue("TeleportState", typeof(CAbilityTeleport.ETeleportState));
					break;
				case "MovedFromPoint":
					MovedFromPoint = (TileIndex)info.GetValue("MovedFromPoint", typeof(TileIndex));
					break;
				case "MovedToPoint":
					MovedToPoint = (TileIndex)info.GetValue("MovedToPoint", typeof(TileIndex));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityTeleport entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTeleport(SEventAbilityBaseData baseData, CAbilityTeleport.ETeleportState teleportState, TileIndex movedFromPoint, TileIndex movedToPoint)
		: base(baseData)
	{
		TeleportState = teleportState;
		MovedFromPoint = movedFromPoint;
		MovedToPoint = movedToPoint;
	}
}
