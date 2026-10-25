using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

public class ChandelierSway : MonoBehaviour
{
	public Vector3 LocalSwayAxisA = Vector3.right;

	public Vector3 LocalSwayAxisB = Vector3.forward;

	[Range(0f, 20f)]
	public float Sway = 1f;

	[Range(0f, 10f)]
	public float Speed = 1f;

	private Quaternion _initialRotation = Quaternion.identity;

	private void Start()
	{
		_initialRotation = transform.localRotation;
	}

	private void Update()
	{
		transform.localRotation = _initialRotation;
		transform.Rotate(LocalSwayAxisA, Mathf.Sin(Time.timeSinceLevelLoad * Speed) * Sway, Space.Self);
		transform.Rotate(LocalSwayAxisB, Mathf.Sin(Time.timeSinceLevelLoad * 2f * Speed) * Sway / 2f, Space.Self);
	}
}
