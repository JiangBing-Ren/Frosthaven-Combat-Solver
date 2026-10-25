using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public static class CurveBlending
{
	public const int MAX_PROCESSED_KEYS = 1000;

	public static void BlendCurvesGeometrically(AnimationCurve A, AnimationCurve B, AnimationCurve Blend, float blendFactor, float zeroValue)
	{
		Blend.ClearKeys();
		EnsureMinimumKeys(A, zeroValue);
		EnsureMinimumKeys(B, zeroValue);
		int num = A.length - B.length;
		if (Mathf.Max(A.length, B.length) <= 1000)
		{
			if ((float)num > 0f)
			{
				AddIntermediateKeys(B, num);
			}
			else if ((float)num < 0f)
			{
				AddIntermediateKeys(A, Mathf.Abs(num));
			}
			for (int i = 0; i < A.length; i++)
			{
				Blend.AddKey(LerpKeyframe(A.keys[i], B.keys[i], blendFactor));
			}
		}
	}

	private static void EnsureMinimumKeys(AnimationCurve curve, float zeroValue)
	{
		if (curve.length == 0)
		{
			curve.AddKey(0f, zeroValue);
			curve.AddKey(1f, zeroValue);
		}
		else if (curve.length == 1)
		{
			float time = ((curve.keys[0].time > 0.5f) ? 0f : 1f);
			curve.AddKey(time, curve.Evaluate(time));
		}
	}

	private static void AddIntermediateKeys(AnimationCurve curve, int extraKeyCount)
	{
		List<(int, float)> source = CalculateTimeDeltaBetweenKeys(curve);
		source = source.OrderByDescending(((int index, float distance) x) => x.distance).ToList();
		List<Keyframe> list = new List<Keyframe>();
		for (int num = 0; num < extraKeyCount; num++)
		{
			(int, float) value = source[num];
			float t = curve.keys[value.Item1].time - value.Item2 * 0.5f;
			SplitCurve(curve, t);
			int item = value.Item1;
			float item2 = value.Item2 * 0.5f;
			value.Item1++;
			value.Item2 = item2;
			source[num] = value;
			source.Add((item, item2));
			source = source.OrderByDescending(((int index, float distance) x) => x.distance).ToList();
		}
		foreach (Keyframe item3 in list)
		{
			curve.AddKey(item3);
		}
	}

	private static List<(int index, float distance)> CalculateTimeDeltaBetweenKeys(AnimationCurve curve)
	{
		List<(int, float)> list = new List<(int, float)>();
		float num = 0f;
		for (int i = 0; i < curve.length; i++)
		{
			float time = curve.keys[i].time;
			list.Add((i, Mathf.Abs(time - num)));
			num = time;
		}
		return list;
	}

	public static Keyframe LerpKeyframe(Keyframe a, Keyframe b, float t)
	{
		return new Keyframe
		{
			time = Mathf.Lerp(a.time, b.time, t),
			value = Mathf.Lerp(a.value, b.value, t),
			inTangent = Mathf.Lerp(a.inTangent, b.inTangent, t),
			outTangent = Mathf.Lerp(a.outTangent, b.outTangent, t),
			inWeight = Mathf.Lerp(a.inWeight, b.inWeight, t),
			outWeight = Mathf.Lerp(a.outWeight, b.outWeight, t)
		};
	}

	public static void CalculateTangent(AnimationCurve curve, float t, out float inTangent, out float outTangent)
	{
		float num = 0.1f;
		float num2 = curve.Evaluate(t - num);
		float num3 = curve.Evaluate(t);
		float num4 = curve.Evaluate(t + num);
		inTangent = Mathf.Abs(num2 - num3) / num;
		outTangent = Mathf.Abs(num4 - num3) / num;
	}

	public static AnimationCurve ToAnimationCurve(this TextureCurve curve)
	{
		int length = curve.length;
		Keyframe[] array = new Keyframe[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = curve[i];
		}
		return new AnimationCurve(array);
	}

	public static TextureCurve ToTextureCurve(this AnimationCurve curve)
	{
		return new TextureCurve(curve.keys, 0f, loop: false, Vector2.one);
	}

	public static void Refresh(this TextureCurve curve)
	{
		curve.SetDirty();
		curve.Evaluate(0f);
	}

	public static void SplitCurve(AnimationCurve curve, float t)
	{
		float num = curve.Evaluate(t);
		float outTangent = (curve.Evaluate(t + 0.001f) - num) / 0.001f;
		float inTangent = (num - curve.Evaluate(t - 0.001f)) / 0.001f;
		Keyframe key = new Keyframe(t, curve.Evaluate(t), inTangent, outTangent);
		curve.AddKey(key);
	}
}
