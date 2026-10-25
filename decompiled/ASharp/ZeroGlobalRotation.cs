using UnityEngine;

public class ZeroGlobalRotation : MonoBehaviour
{
	public bool onlyYAxis = true;

	[Header("X and Z offsets only work if 'Only Y Axis' is turned off")]
	public Vector3 Offset;

	private void LateUpdate()
	{
		enabled = false;
		enabled = true;
		Vector3 eulerAngles = ((!onlyYAxis) ? (new Vector3(0f, 0f, 0f) + Offset) : new Vector3(transform.eulerAngles.x, Offset.y, transform.eulerAngles.z));
		transform.eulerAngles = eulerAngles;
	}
}
