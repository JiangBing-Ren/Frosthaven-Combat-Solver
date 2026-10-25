using UnityEngine;

namespace FHGUI.Elements;

[RequireComponent(typeof(RectTransform))]
public class UI3DPositionConstraint : MonoBehaviour
{
	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private bool _trackTransform;

	[SerializeField]
	private Camera _camera;

	private void Awake()
	{
		_camera = Camera.main;
	}

	private void OnEnable()
	{
		UpdatePos();
	}

	public void TrackTransformConstraint(Transform t)
	{
		_transform = t;
		_trackTransform = true;
	}

	public void SetTransformConstraint(Transform t)
	{
		_transform = t;
		_trackTransform = false;
		UpdatePos();
	}

	private void Update()
	{
		if (_trackTransform && !(_transform == null) && !(_camera == null))
		{
			UpdatePos();
		}
	}

	private void UpdatePos()
	{
		Vector3 vector = _camera.WorldToScreenPoint(_transform.position);
		vector.z = 0f;
		RectTransform obj = transform as RectTransform;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(obj.parent as RectTransform, vector, null, out var worldPoint);
		obj.anchoredPosition = worldPoint;
	}
}
