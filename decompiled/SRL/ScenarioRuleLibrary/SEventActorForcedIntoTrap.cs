using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorForcedIntoTrap : SEvent
{
	public string ControllingClassID;

	public int AbilityCardID { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("AbilityCardID", AbilityCardID);
		info.AddValue("ControllingClassID", ControllingClassID);
	}

	public SEventActorForcedIntoTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "AbilityCardID"))
				{
					if (name == "ControllingClassID")
					{
						ControllingClassID = info.GetString("ControllingClassID");
					}
				}
				else
				{
					AbilityCardID = info.GetInt32("AbilityCardID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventTrapWithHealXExists entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorForcedIntoTrap(int AbilityCardID, string ControllingClassID, string text = "")
		: base(ESEType.Actor, text)
	{
		this.AbilityCardID = AbilityCardID;
		this.ControllingClassID = ControllingClassID;
	}
}
