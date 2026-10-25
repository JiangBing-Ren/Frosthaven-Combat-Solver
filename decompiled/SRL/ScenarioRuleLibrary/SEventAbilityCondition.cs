using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityCondition : SEventAbility
{
	public EConditionDecTrigger ConditionDecrementTrigger { get; private set; }

	public int Duration { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ConditionDecrementTrigger", ConditionDecrementTrigger);
		info.AddValue("Duration", Duration);
	}

	public SEventAbilityCondition(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "ConditionDecrementTrigger"))
				{
					if (name == "Duration")
					{
						Duration = info.GetInt32("Duration");
					}
				}
				else
				{
					ConditionDecrementTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityCondition entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityCondition(SEventAbilityBaseData baseData, EConditionDecTrigger conditionDecrementTrigger, int duration)
		: base(baseData)
	{
		ConditionDecrementTrigger = conditionDecrementTrigger;
		Duration = duration;
	}
}
