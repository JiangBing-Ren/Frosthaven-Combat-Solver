using UnityEngine;

public class FocalBounds : MonoBehaviour
{
	public Vector3 Center;

	public Vector3 Bounds;

	public Bounds GetUBounds()
	{
		return new Bounds(Center, Bounds);
	}
}
