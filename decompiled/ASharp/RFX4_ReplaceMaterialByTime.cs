using UnityEngine;

public class RFX4_ReplaceMaterialByTime : MonoBehaviour
{
	public Material ReplacementMaterial;

	public float TimeDelay = 1f;

	public bool ChangeShadow = true;

	private bool isInitialized;

	private Material mat;

	private MeshRenderer mshRend;

	private void Start()
	{
		isInitialized = true;
		mshRend = GetComponent<MeshRenderer>();
		mat = mshRend.sharedMaterial;
		Invoke("ReplaceObject", TimeDelay);
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			mshRend.sharedMaterial = mat;
			Invoke("ReplaceObject", TimeDelay);
		}
	}

	private void ReplaceObject()
	{
		mshRend.sharedMaterial = ReplacementMaterial;
	}
}
