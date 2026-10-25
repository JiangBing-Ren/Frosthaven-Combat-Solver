using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventItem : SEvent
{
	public ESESubTypeItem ItemSubType { get; private set; }

	public CItem.EItemType ItemType { get; private set; }

	public CItem.EItemSlot ItemSlot { get; private set; }

	public string ItemStringID { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ItemSubType", ItemSubType);
		info.AddValue("ItemType", ItemType);
		info.AddValue("ItemSlot", ItemSlot);
		info.AddValue("ItemStringID", ItemStringID);
	}

	public SEventItem(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ItemSubType":
					ItemSubType = (ESESubTypeItem)info.GetValue("ItemSubType", typeof(ESESubTypeItem));
					break;
				case "ItemType":
					ItemType = (CItem.EItemType)info.GetValue("ItemType", typeof(CItem.EItemType));
					break;
				case "ItemSlot":
					ItemSlot = (CItem.EItemSlot)info.GetValue("ItemSlot", typeof(CItem.EItemSlot));
					break;
				case "ItemStringID":
					ItemStringID = info.GetString("ItemStringID");
					break;
				case "ItemName":
					ItemStringID = info.GetString("ItemName");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventItem(ESESubTypeItem itemSubType, CItem.EItemType itemType, CItem.EItemSlot itemSlot, string itemStringID, string text = "")
		: base(ESEType.Item, text)
	{
		ItemSubType = itemSubType;
		ItemType = itemType;
		ItemSlot = itemSlot;
		ItemStringID = itemStringID;
	}
}
