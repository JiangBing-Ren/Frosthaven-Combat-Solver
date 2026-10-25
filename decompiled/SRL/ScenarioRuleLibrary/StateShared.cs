using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ScenarioRuleLibrary;

public static class StateShared
{
	public enum ENullStatus
	{
		None,
		BothNull,
		BothNotNull,
		Mismatch
	}

	[Serializable]
	[Flags]
	public enum EHouseRulesFlag
	{
		None = 0,
		ReducedRandomness = 1,
		OptionalRetirement = ReducedRandomness
	}

	public static EHouseRulesFlag SetHouseRuleFlag(EHouseRulesFlag a, EHouseRulesFlag b)
	{
		return a | b;
	}

	public static EHouseRulesFlag UnsetHouseRuleFlag(EHouseRulesFlag a, EHouseRulesFlag b)
	{
		return a & ~b;
	}

	public static bool HasHouseRuleFlag(EHouseRulesFlag a, EHouseRulesFlag b)
	{
		return (a & b) == b;
	}

	public static EHouseRulesFlag ToggleHouseRuleFlag(EHouseRulesFlag a, EHouseRulesFlag b)
	{
		return a ^ b;
	}

	public static ENullStatus CheckNullsMatch(object obj1, object obj2)
	{
		if (obj1 == null || obj2 == null)
		{
			if (obj1 == null && obj2 == null)
			{
				return ENullStatus.BothNull;
			}
			return ENullStatus.Mismatch;
		}
		return ENullStatus.BothNotNull;
	}

	public static bool CheckNullsMatchBoolean(object obj1, object obj2)
	{
		if (obj1 == null || obj2 == null)
		{
			if (obj1 == null && obj2 == null)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static List<Tuple<int, string>> Compare(object state1, object state2, string guid, int code)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		if (state1 == null || state2 == null)
		{
			Log.ErrorWithReport("Null state sent to Compare function.");
			return list;
		}
		if (state1.GetType() != state2.GetType())
		{
			Log.ErrorWithReport("Invalid arguements sent to Compare function.  State1 Type: " + state1.GetType().ToString() + " State2 Type: " + state2.GetType().ToString());
			return list;
		}
		Type type = state1.GetType();
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!propertyInfo.PropertyType.IsSerializable)
			{
				continue;
			}
			object value = propertyInfo.GetValue(state1);
			object value2 = propertyInfo.GetValue(state2);
			if (value == null || value2 == null)
			{
				if (value != value2)
				{
					List<Tuple<int, string>> list2 = list;
					int item = code++;
					string item2;
					if ("Mismatch Found: " + propertyInfo.Name + " does not match.  State 1: " + propertyInfo.Name + " is " + value == null)
					{
						item2 = " null";
					}
					else
					{
						item2 = ((" not null  State 2: " + propertyInfo.Name + " = " + value2 == null) ? " null" : (" notnull  ParentGuid: " + guid));
					}
					list2.Add(new Tuple<int, string>(item, item2));
				}
				continue;
			}
			if (value is IEnumerable enumerable)
			{
				foreach (object item3 in enumerable)
				{
					Type.GetTypeCode(item3.GetType());
					_ = 1;
				}
			}
			if (Type.GetTypeCode(propertyInfo.PropertyType) != TypeCode.Object)
			{
				continue;
			}
			MethodInfo method = type.GetMethod("Compare");
			if (!(method != null))
			{
				continue;
			}
			object obj = method.Invoke(null, new object[2] { value, value2 });
			if (Type.GetTypeCode(obj.GetType()) == TypeCode.Boolean)
			{
				if (!(bool)obj)
				{
					list.Add(new Tuple<int, string>(code++, "Mismatch Found: " + propertyInfo.Name + " does not match.  State 1: " + propertyInfo.Name + " = " + value?.ToString() + "  State 2: " + propertyInfo.Name + " = " + value2?.ToString() + "  ParentGuid: " + guid));
				}
			}
			else
			{
				try
				{
					list.AddRange((List<Tuple<int, string>>)obj);
				}
				catch (InvalidCastException)
				{
				}
			}
		}
		return list;
	}
}
