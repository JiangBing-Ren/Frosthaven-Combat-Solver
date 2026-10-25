using SETUtil.Materials;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
	public int xFrames = 4;

	public int yFrames = 4;

	public float speed;

	public bool billboard = true;

	public Camera mainCamera;

	private int frame;

	private Renderer rendererReference;

	private int randomStart;

	private void Awake()
	{
		rendererReference = gameObject.GetComponent<Renderer>();
		rendererReference.GetMaterialClonesSlow()[0].mainTextureScale = new Vector2(1f / (float)xFrames, 1f / (float)yFrames);
		if (billboard && !mainCamera)
		{
			mainCamera = Camera.main;
		}
		randomStart = (int)(Random.value * (float)xFrames * (float)yFrames);
	}

	private void Update()
	{
		frame = (int)Mathf.Repeat(Mathf.FloorToInt(Time.time * speed) + randomStart, xFrames * yFrames);
		int num = frame % xFrames;
		int num2 = frame / xFrames;
		rendererReference.GetMaterialClonesSlow()[0].mainTextureOffset = new Vector2((float)num / ((float)xFrames * 1f), 1f - (float)(num2 + 1) / ((float)yFrames * 1f));
		if (billboard && mainCamera != null)
		{
			transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
		}
	}
}
