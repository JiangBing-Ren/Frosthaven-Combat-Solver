using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	private float initialValue;

	private Vector3 initialPosition;

	private Vector3 initialScale;

	private float initialTime;

	private Light lightRef;

	public float amount = 0.01f;

	public float speed = 8f;

	public bool adjustLocation;

	public float locationAdjustAmount = 1f;

	public bool adjustScale;

	public float scaleAdjustAmount = 1f;

	public Transform scaleObject;

	private void Start()
	{
		initialTime = Random.value * 100f;
		lightRef = gameObject.GetComponent<Light>();
		if ((bool)lightRef)
		{
			initialValue = lightRef.intensity;
		}
		if (!scaleObject)
		{
			scaleObject = transform;
		}
		initialPosition = transform.position;
		initialScale = scaleObject.localScale;
	}

	private void Update()
	{
		float num = Mathf.PerlinNoise(Time.time * speed, initialTime);
		if ((bool)lightRef)
		{
			lightRef.intensity = initialValue + (num - 0.5f) * 2f * amount;
		}
		if (adjustLocation)
		{
			float x = Time.time * speed;
			Vector3 vector = new Vector3(Mathf.PerlinNoise(x, initialTime + 5f) - 0.5f, num - 0.5f, Mathf.PerlinNoise(x, initialTime + 10f) - 0.5f);
			transform.position = initialPosition + vector * locationAdjustAmount * 2f;
		}
		if (adjustScale)
		{
			scaleObject.localScale = initialScale * ((num - 0.5f) * scaleAdjustAmount + 1f);
		}
	}
}
