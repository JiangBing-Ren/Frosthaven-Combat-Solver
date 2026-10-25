using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventObjectPropChest : SEventObjectProp
{
	public int Damage { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Damage", Damage);
	}

	public SEventObjectPropChest(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "Damage")
				{
					Damage = info.GetInt32("Damage");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventObjectPropChest entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventObjectPropChest(int damage, string characterID, string action, ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectGuid, string objectName, string ownerGuid, string text, Point position)
		: base(objectPropSubType, objectType, objectGuid, objectName, characterID, action, ownerGuid, text, null, position)
	{
		Damage = damage;
	}
}
