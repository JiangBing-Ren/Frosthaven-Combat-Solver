using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsPropActivated : CPlayerStatsAction, ISerializable
{
	public ESESubTypeObjectProp ObjectPropSubType { get; private set; }

	public ScenarioManager.ObjectImportType ObjectType { get; private set; }

	public string ObjectGuid { get; private set; }

	public string ObjectName { get; private set; }

	public string OwnerGuid { get; private set; }

	public string ObjectCharacterID { get; private set; }

	public string ObjectAction { get; private set; }

	public Guid ActivatedDuringAbilityID { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ObjectPropSubType", ObjectPropSubType);
		info.AddValue("ObjectType", ObjectType);
		info.AddValue("ObjectGuid", ObjectGuid);
		info.AddValue("ObjectName", ObjectName);
		info.AddValue("OwnerGuid", OwnerGuid);
		info.AddValue("ObjectCharacterID", ObjectCharacterID);
		info.AddValue("ObjectAction", ObjectAction);
		info.AddValue("ActivatedDuringAbilityID", ActivatedDuringAbilityID);
	}

	public CPlayerStatsPropActivated(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ActivatedDuringAbilityID":
					ActivatedDuringAbilityID = (Guid)info.GetValue("ActivatedDuringAbilityID", typeof(Guid));
					break;
				case "ObjectPropSubType":
					ObjectPropSubType = (ESESubTypeObjectProp)info.GetValue("ObjectPropSubType", typeof(ESESubTypeObjectProp));
					break;
				case "ObjectType":
					ObjectType = (ScenarioManager.ObjectImportType)info.GetValue("ObjectType", typeof(ScenarioManager.ObjectImportType));
					break;
				case "ObjectGuid":
					ObjectGuid = info.GetString("ObjectGuid");
					break;
				case "ObjectName":
					ObjectName = info.GetString("ObjectName");
					break;
				case "OwnerGuid":
					OwnerGuid = info.GetString("OwnerGuid");
					break;
				case "ObjectCharacterID":
					ObjectCharacterID = info.GetString("ObjectCharacterID");
					break;
				case "ObjectAction":
					ObjectAction = info.GetString("ObjectAction");
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

	public CPlayerStatsPropActivated(ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectGuid, string objectName, string objectActor, string objectAction, string ownerGuid, List<CMap> maps, Guid abilityID, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		ObjectPropSubType = objectPropSubType;
		ObjectType = objectType;
		ObjectGuid = objectGuid;
		ObjectName = objectName;
		OwnerGuid = ownerGuid;
		ObjectCharacterID = objectActor;
		ObjectAction = objectAction;
		ActivatedDuringAbilityID = abilityID;
	}

	public static CPlayerStatsPropActivated CreatePropActivatedStat(SEventObjectProp sevent, string advGuid, string sceGuid, string currentQuestType, string ActorType, string ActedOnType, string actingClassID = null, string actedOnClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		return new CPlayerStatsPropActivated(sevent.ObjectPropSubType, sevent.ObjectType, sevent.ObjectGuid, sevent.ObjectName, sevent.ObjectCharacterID, sevent.ObjectAction, sevent.OwnerGuid, sevent.ObjectMaps?.ToList(), sevent.ActivatedDuringAbilityID, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder), new CPlayerStatsActionData(actingClassID, ActorType, null, null, null, null, actedOnClassID, ActedOnType, null, null, null, null, null, sevent.Elements, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", int.MaxValue, performedBySummons, rolledIntoSummoner, 0, 0, 0, 0, 0, mainActorWallAdjacent: false, null, null, sevent.Position));
	}
}
