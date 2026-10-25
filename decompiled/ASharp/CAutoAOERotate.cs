using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

[Serializable]
public class CAutoAOERotate : CAuto, ISerializable
{
	public bool IsClockWise;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("IsClockWise", IsClockWise);
	}

	protected CAutoAOERotate(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "IsClockWise")
				{
					IsClockWise = info.GetBoolean("IsClockWise");
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize CAutoAOERotate entry " + item.Name);
				throw ex;
			}
		}
	}

	public CAutoAOERotate(int id, bool clockwise)
		: base(EAutoType.AOERotate, id)
	{
		IsClockWise = clockwise;
	}
}
