using System;
using UnityEngine;

namespace FHGUI;

public abstract class MonsterCardConditionalElement : MonoBehaviour
{
	public enum ComparisonOp
	{
		Less,
		LessThanOrEqual,
		Equal,
		GreaterThanOrEqual,
		Greater
	}

	protected abstract bool CanShow(MonsterCardUIContext context);

	public void Init(MonsterCardUIContext context)
	{
		gameObject.SetActive(CanShow(context));
	}

	public static bool Compare(int value, int other, ComparisonOp op)
	{
		return op switch
		{
			ComparisonOp.Less => value < other, 
			ComparisonOp.LessThanOrEqual => value <= other, 
			ComparisonOp.Equal => value == other, 
			ComparisonOp.GreaterThanOrEqual => value >= other, 
			ComparisonOp.Greater => value > other, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
