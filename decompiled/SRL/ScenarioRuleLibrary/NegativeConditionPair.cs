using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{NegativeCondition}")]
public class NegativeConditionPair : ISerializable
{
	public CCondition.ENegativeCondition NegativeCondition;

	[Obsolete]
	public RewardCondition.EConditionMapDuration MapDuration;

	public EConditionDecTrigger ConditionDecTrigger;

	public int RoundDuration;

	public int Strength = 1;

	public bool TempDisabled;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("NegativeCondition", NegativeCondition);
		info.AddValue("MapDuration", MapDuration);
		info.AddValue("ConditionDecTrigger", ConditionDecTrigger);
		info.AddValue("RoundDuration", RoundDuration);
		info.AddValue("Strength", Strength);
		info.AddValue("TempDisabled", TempDisabled);
	}

	private NegativeConditionPair(SerializationInfo info, StreamingContext context)
	{
		try
		{
			foreach (SerializationEntry item in info)
			{
				switch (item.Name)
				{
				case "Strength":
					Strength = info.GetInt32("Strength");
					break;
				case "NegativeCondition":
					NegativeCondition = (CCondition.ENegativeCondition)info.GetValue("NegativeCondition", typeof(CCondition.ENegativeCondition));
					break;
				case "MapDuration":
					MapDuration = (RewardCondition.EConditionMapDuration)info.GetValue("MapDuration", typeof(int));
					break;
				case "ConditionDecrementTrigger":
					ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
					break;
				case "RoundDuration":
					RoundDuration = info.GetInt32("RoundDuration");
					break;
				case "TempDisabled":
					TempDisabled = info.GetBoolean("TempDisabled");
					break;
				case "ConditionDecTrigger":
					ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecTrigger", typeof(EConditionDecTrigger));
					if (ConditionDecTrigger == EConditionDecTrigger.Rounds)
					{
						ConditionDecTrigger = EConditionDecTrigger.Turns;
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("Unable to deserialize NegativeConditionPair.cs.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public NegativeConditionPair(CCondition.ENegativeCondition negCondition, RewardCondition.EConditionMapDuration mapDuration, int roundDuration, EConditionDecTrigger decTrigger, int strength, bool tempDisabled = false)
	{
		NegativeCondition = negCondition;
		MapDuration = mapDuration;
		RoundDuration = roundDuration;
		ConditionDecTrigger = decTrigger;
		Strength = strength;
		TempDisabled = tempDisabled;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is NegativeConditionPair negativeConditionPair)
		{
			if (NegativeCondition == negativeConditionPair.NegativeCondition && ConditionDecTrigger == negativeConditionPair.ConditionDecTrigger && MapDuration == negativeConditionPair.MapDuration && RoundDuration == negativeConditionPair.RoundDuration)
			{
				return Strength == negativeConditionPair.Strength;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return "NegativeCondition: " + NegativeCondition.ToString() + ", ConditionDecTrigger: " + ConditionDecTrigger.ToString() + ", MapDuration: " + MapDuration.ToString() + ", RoundDuration: " + RoundDuration + ", Strength: " + Strength;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
