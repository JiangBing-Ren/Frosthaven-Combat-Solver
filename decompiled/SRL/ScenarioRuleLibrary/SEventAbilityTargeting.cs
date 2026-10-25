using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTargeting : SEventAbility
{
	public CAbilityTargeting.TargetingState TargetingState { get; private set; }

	public int Amount { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TargetingState", TargetingState);
		info.AddValue("Amount", Amount);
	}

	public SEventAbilityTargeting(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "TargetingState"))
				{
					if (name == "Amount")
					{
						Amount = info.GetInt32("Amount");
					}
				}
				else
				{
					TargetingState = (CAbilityTargeting.TargetingState)info.GetValue("TargetingState", typeof(CAbilityTargeting.TargetingState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityTargeting entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTargeting(SEventAbilityBaseData baseData, CAbilityTargeting.TargetingState targetingState, int amount = 0)
		: base(baseData)
	{
		TargetingState = targetingState;
		Amount = amount;
	}
}
