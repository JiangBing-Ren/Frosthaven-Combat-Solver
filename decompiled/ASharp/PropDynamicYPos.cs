using Chronos;
using UnityEngine;

public class PropDynamicYPos : MonoBehaviour
{
	[HideInInspector]
	public Vector2 HorizontalOffset;

	private bool _possibleFloorOffset;

	private float _targetYOffset;

	private void LateUpdate()
	{
		UpdateYOffset();
	}

	private void UpdateYOffset()
	{
		_targetYOffset = 0f;
		Vector3 localPosition = transform.localPosition;
		if (_possibleFloorOffset)
		{
			localPosition.y = Mathf.Lerp(localPosition.y, _targetYOffset, 7f * Timekeeper.instance.m_GlobalClock.deltaTime);
		}
		localPosition.x = HorizontalOffset.x;
		localPosition.z = HorizontalOffset.y;
		transform.localPosition = localPosition;
	}
}
