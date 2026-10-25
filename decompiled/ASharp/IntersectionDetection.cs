using UnityEngine;

public static class IntersectionDetection
{
	public static bool CheckIntersection(Vector3 target, Vector3 cameraPosition, Vector3 wallEdgeA, Vector3 wallEdgeB)
	{
		float num = (wallEdgeB.z - wallEdgeA.z) * (cameraPosition.x - target.x) - (wallEdgeB.x - wallEdgeA.x) * (cameraPosition.z - target.z);
		if (Mathf.Approximately(num, 0f))
		{
			return false;
		}
		float value = ((wallEdgeB.x - wallEdgeA.x) * (target.z - wallEdgeA.z) - (wallEdgeB.z - wallEdgeA.z) * (target.x - wallEdgeA.x)) / num;
		float value2 = ((cameraPosition.x - target.x) * (target.z - wallEdgeA.z) - (cameraPosition.z - target.z) * (target.x - wallEdgeA.x)) / num;
		if (IsInRange(value))
		{
			return IsInRange(value2);
		}
		return false;
		static bool IsInRange(float num2)
		{
			if (num2 > 0f)
			{
				return num2 < 1f;
			}
			return false;
		}
	}

	public static bool CheckIntersection(Vector3 target, Vector3 cameraPosition, Vector3 wallEdgeA, Vector3 wallEdgeB, out Vector3 intersectionPoint)
	{
		float num = (wallEdgeB.z - wallEdgeA.z) * (cameraPosition.x - target.x) - (wallEdgeB.x - wallEdgeA.x) * (cameraPosition.z - target.z);
		if (Mathf.Approximately(num, 0f))
		{
			intersectionPoint = Vector3.positiveInfinity;
			return false;
		}
		float num2 = ((wallEdgeB.x - wallEdgeA.x) * (target.z - wallEdgeA.z) - (wallEdgeB.z - wallEdgeA.z) * (target.x - wallEdgeA.x)) / num;
		float value = ((cameraPosition.x - target.x) * (target.z - wallEdgeA.z) - (cameraPosition.z - target.z) * (target.x - wallEdgeA.x)) / num;
		intersectionPoint = wallEdgeA + num2 * (wallEdgeB - wallEdgeA);
		if (IsInRange(num2))
		{
			return IsInRange(value);
		}
		return false;
		static bool IsInRange(float num3)
		{
			if (num3 > 0f)
			{
				return num3 < 1f;
			}
			return false;
		}
	}

	public static bool CheckIntersection(Vector2 target, Vector2 cameraPosition, Vector2 wallEdgeA, Vector2 wallEdgeB)
	{
		float num = (wallEdgeB.y - wallEdgeA.y) * (cameraPosition.x - target.x) - (wallEdgeB.x - wallEdgeA.x) * (cameraPosition.y - target.y);
		if (Mathf.Approximately(num, 0f))
		{
			return false;
		}
		float value = ((wallEdgeB.x - wallEdgeA.x) * (target.y - wallEdgeA.y) - (wallEdgeB.y - wallEdgeA.y) * (target.x - wallEdgeA.x)) / num;
		float value2 = ((cameraPosition.x - target.x) * (target.y - wallEdgeA.y) - (cameraPosition.y - target.y) * (target.x - wallEdgeA.x)) / num;
		if (IsInRange(value))
		{
			return IsInRange(value2);
		}
		return false;
		static bool IsInRange(float num2)
		{
			if (num2 > 0f)
			{
				return num2 < 1f;
			}
			return false;
		}
	}

	public static bool CheckIntersection(Vector2 target, Vector2 cameraPosition, Vector2 wallEdgeA, Vector2 wallEdgeB, out Vector2 intersectionPoint)
	{
		float num = (wallEdgeB.y - wallEdgeA.y) * (cameraPosition.x - target.x) - (wallEdgeB.x - wallEdgeA.x) * (cameraPosition.y - target.y);
		if (Mathf.Approximately(num, 0f))
		{
			intersectionPoint = Vector2.positiveInfinity;
			return false;
		}
		float num2 = ((wallEdgeB.x - wallEdgeA.x) * (target.y - wallEdgeA.y) - (wallEdgeB.y - wallEdgeA.y) * (target.x - wallEdgeA.x)) / num;
		float value = ((cameraPosition.x - target.x) * (target.y - wallEdgeA.y) - (cameraPosition.y - target.y) * (target.x - wallEdgeA.x)) / num;
		intersectionPoint = wallEdgeA + num2 * (wallEdgeB - wallEdgeA);
		if (IsInRange(num2))
		{
			return IsInRange(value);
		}
		return false;
		static bool IsInRange(float num3)
		{
			if (num3 > 0f)
			{
				return num3 < 1f;
			}
			return false;
		}
	}
}
