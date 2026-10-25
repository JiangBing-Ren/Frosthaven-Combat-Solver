using UnityEngine;

[ExecuteInEditMode]
public class WallActorIntersectionTest : MonoBehaviour
{
	public Transform Target;

	public Transform Camera;

	public Transform WallA;

	public Transform WallB;

	private void Update()
	{
		Debug.Log(IntersectionDetection.CheckIntersection(Target.position, Camera.position, WallA.position, WallB.position));
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(Target.position, Camera.position);
		Gizmos.DrawLine(WallA.position, WallB.position);
	}
}
