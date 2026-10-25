using UnityEngine;

public class VerticalPointerUI : MonoBehaviour
{
	public void PointAt(RectTransform rectTransform)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		transform.position = new Vector3(transform.position.x, array[1].y + (array[0].y - array[1].y) / 2f, transform.position.z);
	}
}
