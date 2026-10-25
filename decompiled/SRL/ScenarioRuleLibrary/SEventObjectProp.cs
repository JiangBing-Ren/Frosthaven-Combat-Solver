using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventObjectProp : SEvent
{
	public ESESubTypeObjectProp ObjectPropSubType { get; private set; }

	public ScenarioManager.ObjectImportType ObjectType { get; private set; }

	public string ObjectGuid { get; private set; }

	public string ObjectName { get; private set; }

	public string OwnerGuid { get; private set; }

	public string ObjectCharacterID { get; private set; }

	public string ObjectAction { get; private set; }

	public List<CMap> ObjectMaps { get; private set; }

	public Guid ActivatedDuringAbilityID { get; private set; }

	public Point Position { get; private set; }

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
		info.AddValue("ObjectMaps", ObjectMaps);
		info.AddValue("ActivatedDuringAbilityID", ActivatedDuringAbilityID);
		info.AddValue("Position", Position);
	}

	public SEventObjectProp(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Position":
					Position = (Point)info.GetValue("Position", typeof(Point));
					break;
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
				case "ObjectMaps":
					ObjectMaps = (List<CMap>)info.GetValue("ObjectMaps", typeof(List<CMap>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventObjectProp entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventObjectProp(ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectGuid, string objectName, string characterID, string objectAction, string ownerGuid, string text, List<CMap> maps, Point position)
		: base(ESEType.ObjectProp, text)
	{
		if (objectName.Equals(ESpecificPropType.PyroclastHazardTerrain.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			objectName = EPropType.TerrainHotCoals.ToString();
		}
		else if (objectName.Equals(ESpecificPropType.SnowdancerHazardTerrain.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			objectName = EPropType.TerrainThorns.ToString();
		}
		else if (objectName.Equals(ESpecificPropType.VestigeHazardTerrain.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			objectName = EPropType.TerrainThorns.ToString();
		}
		ObjectPropSubType = objectPropSubType;
		ObjectType = objectType;
		ObjectGuid = objectGuid;
		ObjectName = objectName;
		OwnerGuid = ownerGuid;
		ObjectCharacterID = characterID;
		ObjectAction = objectAction;
		ObjectMaps = maps;
		Position = position;
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability != null)
		{
			ActivatedDuringAbilityID = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability.ID;
		}
		else
		{
			ActivatedDuringAbilityID = default;
		}
	}
}
