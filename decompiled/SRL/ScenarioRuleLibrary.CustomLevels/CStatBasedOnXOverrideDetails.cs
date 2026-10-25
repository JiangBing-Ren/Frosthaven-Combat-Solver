using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CStatBasedOnXOverrideDetails : ISerializable
{
	public string AssociatedClassID;

	public AbilityData.StatIsBasedOnXData OverrideData;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AssociatedClassID", AssociatedClassID);
		info.AddValue("OverrideData", OverrideData);
	}

	public CStatBasedOnXOverrideDetails(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "AssociatedClassID"))
				{
					if (name == "OverrideData")
					{
						OverrideData = (AbilityData.StatIsBasedOnXData)info.GetValue("OverrideData", typeof(AbilityData.StatIsBasedOnXData));
					}
				}
				else
				{
					AssociatedClassID = info.GetString("AssociatedClassID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CStatBasedOnXOverrideDetails entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CStatBasedOnXOverrideDetails(string classId)
	{
		AssociatedClassID = classId;
		OverrideData = new AbilityData.StatIsBasedOnXData();
	}
}
