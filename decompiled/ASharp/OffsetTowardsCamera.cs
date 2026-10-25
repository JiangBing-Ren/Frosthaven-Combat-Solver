using UnityEngine;

public class OffsetTowardsCamera : MonoBehaviour
{
	public float offsetAmount;

	private void Start()
	{
		Vector3 position = transform.position;
		Vector3 position2 = position + Vector3.Normalize(Camera.main.transform.position - position) * offsetAmount;
		transform.position = position2;
	}

	private void Update()
	{
	}

	private void OnAwake()
	{
	}
}
