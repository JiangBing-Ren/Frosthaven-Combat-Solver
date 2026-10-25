using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventInternal : SEvent
{
	public ESESubTypeInternal SubType { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SubType", SubType);
	}

	public SEventInternal(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "SubType")
				{
					SubType = (ESESubTypeInternal)info.GetValue("SubType", typeof(ESESubTypeInternal));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventInternal entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventInternal(ESESubTypeInternal subType, string text = "")
		: base(ESEType.Internal, text)
	{
		SubType = subType;
	}
}
