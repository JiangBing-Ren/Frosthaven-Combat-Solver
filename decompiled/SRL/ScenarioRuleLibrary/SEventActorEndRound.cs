using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorEndRound : SEventActor
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	public bool OccupiedDoor { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
		info.AddValue("OccupiedDoor", OccupiedDoor);
	}

	public SEventActorEndRound(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TileX":
					TileX = info.GetInt32("TileX");
					break;
				case "TileY":
					TileY = info.GetInt32("TileY");
					break;
				case "OccupiedDoor":
					OccupiedDoor = info.GetBoolean("OccupiedDoor");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorEndRound entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorEndRound(int x, int y, bool occupiedDoor, SEventActorBaseData baseData)
		: base(baseData)
	{
		TileX = x;
		TileY = y;
		OccupiedDoor = occupiedDoor;
	}

	public SEventActorEndRound(CActor actor)
		: this(actor.ArrayIndex.X, actor.ArrayIndex.Y, actor.Tile.FindDoor() != null, new SEventActorBaseData(ESESubTypeActor.None, actor))
	{
	}
}
