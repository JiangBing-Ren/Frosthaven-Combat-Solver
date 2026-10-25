using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorEndTurn : SEventActor
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
	}

	public SEventActorEndTurn(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "TileX"))
				{
					if (name == "TileY")
					{
						TileY = info.GetInt32("TileY");
					}
				}
				else
				{
					TileX = info.GetInt32("TileX");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorEndTurn entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorEndTurn(int x, int y, SEventActorBaseData baseData)
		: base(baseData)
	{
		TileX = x;
		TileY = y;
	}
}
