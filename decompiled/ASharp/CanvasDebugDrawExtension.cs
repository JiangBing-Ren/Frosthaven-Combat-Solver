using UnityEngine;

public static class CanvasDebugDrawExtension
{
	public static void DrawDebugLine(this Transform tx, Vector2 start, Vector2 end)
	{
		Matrix4x4 localToWorldMatrix = tx.localToWorldMatrix;
		Debug.DrawLine(localToWorldMatrix.MultiplyPoint(start), localToWorldMatrix.MultiplyPoint(end));
	}
}
