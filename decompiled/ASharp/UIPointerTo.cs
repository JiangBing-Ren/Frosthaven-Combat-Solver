using UnityEngine;

public class UIPointerTo : MonoBehaviour
{
	[SerializeField]
	private RectTransform pointer;

	[SerializeField]
	private float offsetAngle;

	public void PointToWorldPosition(Vector3 position)
	{
		Vector3 vector = position - pointer.position;
		float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f + offsetAngle;
		pointer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}
}
