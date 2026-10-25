using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

[Serializable]
public class FreeLevelResetTicket : ISerializable
{
	public string Id;

	public int Order;

	public bool IsUsed;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Id", Id);
		info.AddValue("Order", Order);
		info.AddValue("IsUsed", IsUsed);
	}

	protected FreeLevelResetTicket(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Id":
					Id = info.GetString("Id");
					break;
				case "IsUsed":
					IsUsed = info.GetBoolean("IsUsed");
					break;
				case "Order":
					Order = info.GetInt32("Order");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize FreeLevelResetTicket entry " + item.Name);
				throw ex;
			}
		}
	}

	public FreeLevelResetTicket(string id, int order)
	{
		Id = id;
		Order = order;
		IsUsed = false;
	}
}
