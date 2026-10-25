using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class Map2DShapeBrush : MonoBehaviour, IMapShapeBrush
{
	public float Scale = 1f;

	public List<Vector2> GetShape()
	{
		Vector3 vector = Vector3.one * 0.5f * Scale;
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		return new List<Vector3>
		{
			localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector.x, 0f, vector.z)),
			localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector.x, 0f, vector.z)),
			localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector.x, 0f, 0f - vector.z)),
			localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector.x, 0f, 0f - vector.z))
		}.Select((Vector3 a) => new Vector2(a.x, a.z)).ToList();
	}

	private void OnDrawGizmos()
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one * Scale);
		Gizmos.matrix = matrix;
	}
}
