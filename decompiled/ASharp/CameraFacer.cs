using UnityEngine;

public class CameraFacer : MonoBehaviour
{
	private void Update()
	{
		transform.LookAt(Camera.main.transform.position, Vector3.up);
	}
}
