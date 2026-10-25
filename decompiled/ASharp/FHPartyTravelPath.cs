using UnityEngine;

public class FHPartyTravelPath : MonoBehaviour
{
	[SerializeField]
	private Vector3 offset;

	[SerializeField]
	private LineRenderer _lineRenderer;

	public void Init(Vector3[] points)
	{
		Show();
		for (int i = 0; i < points.Length; i++)
		{
			points[i] += offset;
		}
		_lineRenderer.SetPositions(points);
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide(bool instant = false)
	{
		gameObject.SetActive(value: false);
	}
}
