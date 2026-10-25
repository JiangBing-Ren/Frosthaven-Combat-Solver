using Unity.Mathematics;
using UnityEngine;

namespace MeshSlicing;

public static class Math2DUtils
{
	public static bool IsVertexConvex(float2 prevP0, float2 P0, float2 nextP0, bool isCCW)
	{
		float num = math.determinant(new float2x2
		{
			c0 = P0 - prevP0,
			c1 = nextP0 - P0
		});
		if (isCCW)
		{
			return num > 0f;
		}
		return num < 0f;
	}

	public static bool IsVertexReflex(float2 prevP0, float2 P0, float2 nextP0, bool isCCW)
	{
		float num = math.determinant(new float2x2
		{
			c0 = P0 - prevP0,
			c1 = nextP0 - P0
		});
		if (isCCW)
		{
			return num < 0f;
		}
		return num > 0f;
	}

	public static bool IsInsideTriangle(float2 p, float2 a, float2 b, float2 c)
	{
		float num = (0f - b.y) * c.x + a.y * (0f - b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y;
		float num2 = 1f / num * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
		float num3 = 1f / num * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
		if (num2 >= 0f && num3 >= 0f)
		{
			return num2 + num3 <= 1f;
		}
		return false;
	}

	public static bool SegmentsIntersection(float2 a0, float2 a1, float2 b0, float2 b1, out float2 intersection)
	{
		intersection = default;
		float2 float5 = a1 - a0;
		float2 b2 = b1 - b0;
		float num = Cross2D(float5, b2);
		float num2 = Cross2D(b0 - a0, float5);
		float num3 = Cross2D(b0 - a0, b2) / num;
		float num4 = num2 / num;
		bool flag = math.abs(num) < float.Epsilon;
		bool flag2 = math.abs(num2) < float.Epsilon;
		if (!(flag & flag2))
		{
			if (flag && !flag2)
			{
				return false;
			}
			if (!flag && num3 >= 0f && num3 <= 1f && num4 >= 0f && num4 <= 1f)
			{
				intersection = a0 + num3 * float5;
				return true;
			}
		}
		return false;
	}

	public static float Cross2D(float2 a, float2 b)
	{
		return math.mul(a.x, b.y) - math.mul(a.y, b.x);
	}

	public static float Cross2D(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	public static bool SamePoints(float2 a, float2 b)
	{
		return math.distance(a, b) <= float.Epsilon;
	}

	public static float LineSide(float2 p, float2 a, float2 b)
	{
		return math.mul(b.x - a.x, p.y - a.y) - math.mul(b.y - a.y, p.x - a.x);
	}
}
