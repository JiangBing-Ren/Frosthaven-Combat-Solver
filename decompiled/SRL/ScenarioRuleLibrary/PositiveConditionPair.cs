using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PositiveCondition}")]
public class PositiveConditionPair : ISerializable
{
	public CCondition.EPositiveCondition PositiveCondition;

	public RewardCondition.EConditionMapDuration MapDuration;

	public EConditionDecTrigger ConditionDecTrigger;

	public int RoundDuration;

	public string CasterClassGUID;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PositiveCondition", PositiveCondition);
		info.AddValue("MapDuration", MapDuration);
		info.AddValue("ConditionDecrementTrigger", ConditionDecTrigger);
		info.AddValue("RoundDuration", RoundDuration);
		info.AddValue("CasterClassGUID", CasterClassGUID);
	}

	private PositiveConditionPair(SerializationInfo info, StreamingContext context)
	{
		try
		{
			foreach (SerializationEntry item in info)
			{
				switch (item.Name)
				{
				case "CasterClassGUID":
					CasterClassGUID = info.GetString("CasterClassGUID");
					break;
				case "PositiveCondition":
					PositiveCondition = (CCondition.EPositiveCondition)info.GetValue("PositiveCondition", typeof(CCondition.EPositiveCondition));
					break;
				case "MapDuration":
					MapDuration = (RewardCondition.EConditionMapDuration)info.GetValue("MapDuration", typeof(RewardCondition.EConditionMapDuration));
					break;
				case "ConditionDecrementTrigger":
					ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
					break;
				case "RoundDuration":
					RoundDuration = info.GetInt32("RoundDuration");
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
			Log.ErrorWithReport("Unable to deserialize PositiveConditionPair.cs.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public PositiveConditionPair(CCondition.EPositiveCondition posCondition, RewardCondition.EConditionMapDuration mapDuration, int roundDuration, EConditionDecTrigger decTrigger, CActor caster)
		: this(posCondition, mapDuration, roundDuration, decTrigger, caster?.Class?.ID)
	{
	}

	public PositiveConditionPair(CCondition.EPositiveCondition posCondition, RewardCondition.EConditionMapDuration mapDuration, int roundDuration, EConditionDecTrigger decTrigger, string casterClassGuid)
	{
		PositiveCondition = posCondition;
		MapDuration = mapDuration;
		RoundDuration = roundDuration;
		ConditionDecTrigger = decTrigger;
		CasterClassGUID = casterClassGuid;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is PositiveConditionPair positiveConditionPair)
		{
			if (PositiveCondition == positiveConditionPair.PositiveCondition && ConditionDecTrigger == positiveConditionPair.ConditionDecTrigger && MapDuration == positiveConditionPair.MapDuration && RoundDuration == positiveConditionPair.RoundDuration)
			{
				return CasterClassGUID == positiveConditionPair.CasterClassGUID;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return "PositiveCondition: " + PositiveCondition.ToString() + ", ConditionDecTrigger: " + ConditionDecTrigger.ToString() + ", MapDuration: " + MapDuration.ToString() + ", RoundDuration: " + RoundDuration;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
