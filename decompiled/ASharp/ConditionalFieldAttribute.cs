using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
	public string PropertyToCheck;

	public object CompareValue;

	public bool EqualComparision;

	public ConditionalFieldAttribute(string propertyToCheck, object compareValue = null, bool equal = true)
	{
		PropertyToCheck = propertyToCheck;
		CompareValue = compareValue;
		EqualComparision = equal;
	}
}
