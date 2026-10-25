using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventTrapActor : SEvent
{
	public int HealAmount { get; private set; }

	public string ObjectGuid { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("HealAmount", HealAmount);
		info.AddValue("ObjectGuid", ObjectGuid);
	}

	public SEventTrapActor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "HealAmount"))
				{
					if (name == "ObjectGuid")
					{
						ObjectGuid = info.GetString("ObjectGuid");
					}
				}
				else
				{
					HealAmount = info.GetInt32("HealAmount");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventTrapWithHealXExists entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventTrapActor(int healAmount, string objectGuid, string text = "")
		: base(ESEType.None, text)
	{
		HealAmount = healAmount;
		ObjectGuid = objectGuid;
	}
}
