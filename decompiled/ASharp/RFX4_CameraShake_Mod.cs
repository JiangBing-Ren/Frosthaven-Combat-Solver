using System.Collections;
using UnityEngine;

public class RFX4_CameraShake_Mod : MonoBehaviour
{
	public AnimationCurve ShakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public float Duration = 2f;

	public float Speed = 22f;

	public float Magnitude = 1f;

	public float DistanceForce = 100f;

	public float RotationDamper = 2f;

	public bool IsEnabled = true;

	private bool isPlaying;

	[HideInInspector]
	public bool canUpdate;

	private void PlayShake()
	{
		StopAllCoroutines();
		CoroutineHelper.RunCoroutine(Shake(transform.position, DistanceForce, Duration, ShakeCurve, Speed, Magnitude, RotationDamper, canUpdate));
	}

	private void Update()
	{
		if (!CameraController.s_CameraController.m_EnableCameraShake && isPlaying && IsEnabled)
		{
			isPlaying = false;
			PlayShake();
		}
	}

	private void OnEnable()
	{
		isPlaying = true;
		Object.FindObjectsOfType(typeof(RFX4_CameraShake));
		canUpdate = true;
	}

	private static IEnumerator Shake(Vector3 position, float DistanceForce, float Duration, AnimationCurve ShakeCurve, float Speed, float Magnitude, float RotationDamper, bool canUpdate)
	{
		CameraController.s_CameraController.m_EnableCameraShake = true;
		CameraController.s_CameraController.m_CameraShakePosition = Vector3.zero;
		float elapsed = 0f;
		Vector3 originalCamRotation = CameraController.s_CameraController.m_CameraShakeRotation.eulerAngles;
		Vector3 direction = (position - CameraController.s_CameraController.m_CameraShakePosition).normalized;
		float time = 0f;
		float randomStart = Random.Range(-1000f, 1000f);
		float distanceDamper = 1f - Mathf.Clamp01((CameraController.s_CameraController.m_CameraShakePosition - position).magnitude / DistanceForce);
		Vector3 oldRotation = Vector3.zero;
		while ((elapsed < Duration) & canUpdate)
		{
			elapsed += Time.deltaTime;
			float num = elapsed / Duration;
			float num2 = ShakeCurve.Evaluate(num) * distanceDamper;
			time += Time.deltaTime * num2;
			CameraController.s_CameraController.m_CameraShakePosition -= direction * Time.deltaTime * Mathf.Sin(time * Speed) * num2 * Magnitude / 2f;
			float num3 = randomStart + Speed * num / 10f;
			float num4 = Mathf.PerlinNoise(num3, 0f) * 2f - 1f;
			float num5 = Mathf.PerlinNoise(1000f + num3, num3 + 1000f) * 2f - 1f;
			float num6 = Mathf.PerlinNoise(0f, num3) * 2f - 1f;
			if (Quaternion.Euler(originalCamRotation + oldRotation) != CameraController.s_CameraController.m_CameraShakeRotation)
			{
				originalCamRotation = CameraController.s_CameraController.m_CameraShakeRotation.eulerAngles;
			}
			oldRotation = Mathf.Sin(time * Speed) * num2 * Magnitude * new Vector3(0.5f + num5, 0.3f + num4, 0.3f + num6) * RotationDamper;
			CameraController.s_CameraController.m_CameraShakeRotation = Quaternion.Euler(originalCamRotation + oldRotation);
			yield return null;
		}
		CameraController.s_CameraController.m_EnableCameraShake = false;
		CameraController.s_CameraController.ResetCameraRotation();
	}
}
