using UnityEngine;

[ExecuteInEditMode]
public class DisableMeshRendererAfterDelay : MonoBehaviour
{
	public float delayTime = 15f;

	private float startTime;

	private void OnEnable()
	{
		startTime = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup - startTime > delayTime)
		{
			DisableMeshRenderer();
		}
	}

	private void DisableMeshRenderer()
	{
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}
}
