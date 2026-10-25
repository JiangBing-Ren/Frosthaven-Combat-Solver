using Chronos;
using UnityEngine;

public class AnimatedProjectile : MonoBehaviour
{
	public Transform TargetTransform;

	public float RotationSpeed = 1000f;

	private bool _animating;

	private Vector3 _rotateAround;

	private Quaternion _initialRot;

	public void StartAnim(Vector3 rotAxis)
	{
		_rotateAround = rotAxis;
		_initialRot = TargetTransform.rotation;
		_animating = true;
	}

	public void StopAnim()
	{
		_animating = false;
		TargetTransform.rotation = _initialRot;
	}

	private void Update()
	{
		if (_animating)
		{
			TargetTransform.Rotate(_rotateAround, Timekeeper.instance.m_GlobalClock.deltaTime * RotationSpeed);
		}
	}
}
