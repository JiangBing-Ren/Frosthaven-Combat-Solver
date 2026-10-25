using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDoor : CPlayerStatsAction, ISerializable
{
	public int Doors { get; private set; }

	public List<string> RevealedMapIDs { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Doors", Doors);
		info.AddValue("RevealedMapIDs", RevealedMapIDs);
	}

	public CPlayerStatsDoor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Doors":
					Doors = info.GetInt32("Doors");
					break;
				case "RevealedMapIDs":
					RevealedMapIDs = (List<string>)info.GetValue("RevealedMapIDs", typeof(List<string>));
					break;
				case "RevealedMaps":
					RevealedMapIDs = ((List<CMap>)info.GetValue("RevealedMaps", typeof(List<CMap>)))?.Select((CMap map) => map.MapGuid).ToList();
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsDoor entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDoor(int doors, List<CMap> revealedMaps, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		Doors = doors;
		RevealedMapIDs = revealedMaps?.Select((CMap map) => map.MapGuid).ToList();
	}

	public static CPlayerStatsDoor CreateDoorStat(SEventObjectProp sevent, string advGuid, string sceGuid, string currentQuestType, string ActorType, string ActedOnType, string actingClassID = null, string actedOnClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsDoor(1, sevent.ObjectMaps, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData(actingClassID, ActorType, null, null, null, null, actedOnClassID, ActedOnType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner));
	}
}
