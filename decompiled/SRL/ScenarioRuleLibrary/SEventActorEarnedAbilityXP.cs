using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorEarnedAbilityXP : SEventActor
{
	public int XPEarned { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("XPEarned", XPEarned);
	}

	public SEventActorEarnedAbilityXP(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "XPEarned")
				{
					XPEarned = info.GetInt32("XPEarned");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorEarnedAbilityXP entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorEarnedAbilityXP(int xpEarned, SEventActorBaseData baseData)
		: base(baseData)
	{
		XPEarned = xpEarned;
	}
}
