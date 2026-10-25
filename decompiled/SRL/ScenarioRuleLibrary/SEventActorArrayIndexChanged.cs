using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorArrayIndexChanged : SEventActor
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	public bool TileWasOccupiedByObstacle { get; private set; }

	public bool TileWasOccupiedByActor { get; private set; }

	public int LootTokensInPreviousRoom { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
		info.AddValue("TileWasOccupiedByObstacle", TileWasOccupiedByObstacle);
		info.AddValue("TileWasOccupiedByActor", TileWasOccupiedByActor);
		info.AddValue("LootTokensInPreviousRoom", LootTokensInPreviousRoom);
	}

	public SEventActorArrayIndexChanged(SerializationInfo info, StreamingContext context)
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
				case "TileWasOccupiedByObstacle":
					TileWasOccupiedByObstacle = info.GetBoolean("TileWasOccupiedByObstacle");
					break;
				case "TileWasOccupiedByActor":
					TileWasOccupiedByActor = info.GetBoolean("TileWasOccupiedByActor");
					break;
				case "LootTokensInPreviousRoom":
					LootTokensInPreviousRoom = info.GetInt32("LootTokensInPreviousRoom");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorArrayIndexChanged entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorArrayIndexChanged(CTile tile, CMap prevRoom, SEventActorBaseData baseData)
		: base(baseData)
	{
		TileX = tile.m_ArrayIndex.X;
		TileY = tile.m_ArrayIndex.Y;
		string currentActorGUID = BaseData.MainActorGuid;
		TileWasOccupiedByActor = tile.LivingActors.Any((CActor x) => x.ActorGuid != currentActorGUID);
		TileWasOccupiedByObstacle = tile.FindObstacleProp() != null;
		if (prevRoom != null)
		{
			LootTokensInPreviousRoom = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.MoneyToken && prop.PropTile?.m_HexMap == prevRoom && prop.PropTile?.m_Hex2Map == null).Count();
		}
		else
		{
			LootTokensInPreviousRoom = -1;
		}
	}
}
