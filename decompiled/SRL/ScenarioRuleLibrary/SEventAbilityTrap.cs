using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTrap : SEventAbility
{
	public CAbilityTrap.TrapState TrapState { get; private set; }

	public string TrapName { get; private set; }

	public int TrapTriggeredXP { get; private set; }

	public int TrapHeal { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TrapState", TrapState);
		info.AddValue("TrapName", TrapName);
		info.AddValue("TrapTriggeredXP", TrapTriggeredXP);
		info.AddValue("TrapHeal", TrapHeal);
	}

	public SEventAbilityTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TrapState":
					TrapState = (CAbilityTrap.TrapState)info.GetValue("TrapState", typeof(CAbilityTrap.TrapState));
					break;
				case "TrapName":
					TrapName = info.GetString("TrapName");
					break;
				case "TrapTriggeredXP":
					TrapTriggeredXP = info.GetInt32("TrapTriggeredXP");
					break;
				case "TrapHeal":
					TrapHeal = info.GetInt32("TrapHeal");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityTrap entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTrap(SEventAbilityBaseData baseData, CAbilityTrap.TrapState trapState, string trapName, int trapTriggeredXP, int trapHeal = 0, List<CCondition.ENegativeCondition> negativeConditions = null, List<CCondition.EPositiveCondition> positiveConditions = null)
		: base(baseData)
	{
		TrapState = trapState;
		TrapName = trapName;
		TrapTriggeredXP = trapTriggeredXP;
		TrapHeal = trapHeal;
		if (negativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition negativeCondition in negativeConditions)
			{
				AddedNegativeConditionData.Add(new SEventAbilityAddedConditionData(CCondition.AbilityTypeFromNegativeCondition(negativeCondition), 1, EConditionDecTrigger.None));
			}
		}
		if (positiveConditions == null)
		{
			return;
		}
		foreach (CCondition.EPositiveCondition positiveCondition in positiveConditions)
		{
			AddedPositiveConditionData.Add(new SEventAbilityAddedConditionData(CCondition.AbilityTypeFromPositiveCondition(positiveCondition), 1, EConditionDecTrigger.None));
		}
	}
}
