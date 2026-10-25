using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

public class ObjectHoverFX : MonoBehaviour
{
	[Range(0f, 20f)]
	public float Amplitude = 1f;

	[Range(0f, 10f)]
	public float Speed = 1f;

	private Vector3 _initialPosition = Vector3.zero;

	private float _initialRandom;

	private void Start()
	{
		_initialPosition = transform.localPosition;
		_initialRandom = Random.Range(0f, 100f);
	}

	private void Update()
	{
		transform.localPosition = _initialPosition + new Vector3(0f, Mathf.Sin(Time.time * Speed + _initialRandom) * Amplitude, 0f);
	}
}
