using Libs.Utils;
using UnityEngine;

namespace Util;

public class ParableTesselator : MonoBehaviour
{
	public float Thickness = 0.025f;

	public float Curve = 0.1f;

	public int MeshDetail = 20;

	public float DashSize = 20f;

	public float GroundOffsetAtStartPosition;

	public float GroundOffsetAtEndPosition = 1f;

	public void Tesselate(Vector3 startPos, Vector3 endPos)
	{
		Vector3 vector = startPos + new Vector3(0f, GroundOffsetAtStartPosition, 0f);
		gameObject.transform.position = vector;
		Vector3 vector2 = endPos + new Vector3(0f, GroundOffsetAtEndPosition, 0f);
		Vector3 vector3 = vector2 - vector;
		Tesselator.ExtrudeBoxAlongParableInMesh(sectionBox: new Vector2(Thickness, Thickness), mesh: gameObject.GetComponent<MeshFilter>().mesh, height: vector3.magnitude * Curve, parableStart: vector, parableEnd: vector2, numSplinePoints: MeshDetail, uScale: DashSize, spline: out var _);
	}
}
