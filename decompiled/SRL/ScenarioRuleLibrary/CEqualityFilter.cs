using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public class CEqualityFilter : ISerializable
{
	[JsonProperty]
	public string EqualityString { get; private set; }

	[JsonProperty]
	public int Value { get; private set; }

	[JsonProperty]
	public bool ValueIsPercentage { get; internal set; }

	[JsonProperty]
	public bool Level { get; private set; }

	[JsonProperty]
	public bool RoundDown { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("EqualityString", EqualityString);
		info.AddValue("Value", Value);
		info.AddValue("ValueIsPercentage", ValueIsPercentage);
		info.AddValue("Level", Level);
		info.AddValue("RoundDown", RoundDown);
	}

	public CEqualityFilter(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "EqualityString":
					EqualityString = info.GetString("EqualityString");
					break;
				case "Value":
					Value = info.GetInt32("Value");
					break;
				case "ValueIsPercentage":
					ValueIsPercentage = info.GetBoolean("ValueIsPercentage");
					break;
				case "Level":
					Level = info.GetBoolean("Level");
					break;
				case "RoundDown":
					RoundDown = info.GetBoolean("RoundDown");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CEqualityFilter entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public static bool IsValidEqualityString(string equalityString)
	{
		switch (equalityString)
		{
		default:
			return equalityString == "!=";
		case "==":
		case ">":
		case "<":
		case ">=":
		case "<=":
			return true;
		}
	}

	[JsonConstructor]
	public CEqualityFilter(string equalityString, int value, bool valueIsPercentage, bool level = false, bool roundDown = false)
	{
		if (!IsValidEqualityString(equalityString))
		{
			throw new Exception("Invalid equality string '" + equalityString + "' provided as comparer!");
		}
		EqualityString = equalityString;
		Value = value;
		ValueIsPercentage = valueIsPercentage;
		Level = level;
		RoundDown = roundDown;
	}

	public CEqualityFilter Copy()
	{
		return new CEqualityFilter(EqualityString, Value, ValueIsPercentage, level: false, RoundDown);
	}

	public bool Equals(CEqualityFilter compare)
	{
		if (EqualityString == compare?.EqualityString && Value == compare?.Value)
		{
			return RoundDown == compare.RoundDown;
		}
		return false;
	}

	public bool Compare(int compareValue, int filterValueIsPercentageOperand = -1, int level = 0)
	{
		int num = Value;
		if (Level)
		{
			num += level;
		}
		if (filterValueIsPercentageOperand >= 0 && Value >= 0 && Value <= 100)
		{
			num = ((!RoundDown) ? ((int)Math.Ceiling((float)filterValueIsPercentageOperand * ((float)Value / 100f))) : ((int)Math.Floor((float)filterValueIsPercentageOperand * ((float)Value / 100f))));
		}
		switch (EqualityString)
		{
		case "==":
			return compareValue == num;
		case ">":
			return compareValue > num;
		case "<":
			return compareValue < num;
		case ">=":
			return compareValue >= num;
		case "<=":
			return compareValue <= num;
		case "!=":
			return compareValue != num;
		default:
			Log.ErrorWithReport(" Invalid equality comparison " + EqualityString);
			return false;
		}
	}
}
