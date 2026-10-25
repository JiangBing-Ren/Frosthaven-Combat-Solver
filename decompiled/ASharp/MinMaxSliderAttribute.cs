using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class MinMaxSliderAttribute : PropertyAttribute
{
	public float Min;

	public float Max;

	public MinMaxSliderAttribute(float min = 0f, float max = 1f)
	{
		Min = min;
		Max = max;
	}
}
