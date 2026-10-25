using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorUsedItem : SEventActor
{
	public string ItemName;

	public string Slot;

	public bool FirstTimeUse;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ItemName", ItemName);
		info.AddValue("Slot", Slot);
		info.AddValue("FirstTimeUse", FirstTimeUse);
	}

	public SEventActorUsedItem(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ItemName":
					ItemName = info.GetString("ItemName");
					break;
				case "Slot":
					Slot = info.GetString("Slot");
					break;
				case "FirstTimeUse":
					FirstTimeUse = info.GetBoolean("FirstTimeUse");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorUsedItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorUsedItem(string itemName, string slot, bool firstTimeUse, SEventActorBaseData baseData)
		: base(baseData)
	{
		ItemName = itemName;
		Slot = slot;
		FirstTimeUse = firstTimeUse;
	}
}
