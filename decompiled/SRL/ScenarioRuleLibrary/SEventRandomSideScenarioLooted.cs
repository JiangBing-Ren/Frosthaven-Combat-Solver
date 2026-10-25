using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
[Obsolete]
public class SEventRandomSideScenarioLooted : SEvent
{
	public uint ScenarioID { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ScenarioID", ScenarioID);
	}

	public SEventRandomSideScenarioLooted(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ItemStringID")
				{
					ScenarioID = info.GetUInt32("ScenarioID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventItem entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventRandomSideScenarioLooted(uint scenarioID, string text = "")
		: base(ESEType.RandomSideScenarioLooted, text)
	{
		ScenarioID = scenarioID;
	}
}
