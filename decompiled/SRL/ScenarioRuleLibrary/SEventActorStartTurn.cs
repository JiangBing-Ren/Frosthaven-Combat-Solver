using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorStartTurn : SEventActor
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	public List<int> RoundCardsInitiative { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
		info.AddValue("RoundCardsInitiative", RoundCardsInitiative);
	}

	public SEventActorStartTurn(SerializationInfo info, StreamingContext context)
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
				case "RoundCardsInitiative":
					RoundCardsInitiative = (List<int>)info.GetValue("RoundCardsInitiative", typeof(List<int>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorEndTurn entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorStartTurn(int x, int y, List<int> roundCardsInitiative, SEventActorBaseData baseData)
		: base(baseData)
	{
		TileX = x;
		TileY = y;
		RoundCardsInitiative = roundCardsInitiative;
	}
}
