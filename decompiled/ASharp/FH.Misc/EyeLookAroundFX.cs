using System.Collections;
using SETUtil.Types;
using UnityEngine;

namespace FH.Misc;

public class EyeLookAroundFX : MonoBehaviour
{
	[SerializeField]
	private float _lookAroundSpeed = 1f;

	[Range(0.01f, 1f)]
	[SerializeField]
	private float _lookAroundRange = 0.5f;

	[SerializeField]
	private float _lookAroundRate = 1f;

	private void Start()
	{
		StartCoroutine(LookAround());
	}

	private IEnumerator LookAround()
	{
		TransformData initialOrientation = new TransformData(transform);
		while (Application.isPlaying)
		{
			float time = 0f;
			float duration = Random.Range(0.5f, 1.5f) * _lookAroundRate;
			float f = Random.Range(0, 360);
			Vector3 vector = (initialOrientation.right * Mathf.Sin(f) + initialOrientation.up * Mathf.Cos(f)) * _lookAroundRange + initialOrientation.forward;
			Vector3 target = vector;
			TransformData startingRotation = new TransformData(transform);
			while (time < duration)
			{
				transform.rotation = Quaternion.Slerp(startingRotation.rotation, Quaternion.LookRotation(target), time / duration);
				time += Time.deltaTime * _lookAroundSpeed;
				yield return null;
			}
			yield return new WaitForSeconds(Random.Range(0.5f, 1.5f) * _lookAroundRate);
		}
	}
}
