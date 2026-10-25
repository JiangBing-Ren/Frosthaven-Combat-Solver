using UnityEngine;

public class VFX_GetSkinnedMesh_Script : MonoBehaviour
{
	public bool getSkinnedMeshfromParent;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
		if (getSkinnedMeshfromParent)
		{
			SkinnedMeshRenderer componentInParent = GetComponentInParent<SkinnedMeshRenderer>();
			ParticleSystem.ShapeModule shape = GetComponent<ParticleSystem>().shape;
			shape.skinnedMeshRenderer = componentInParent;
		}
		else
		{
			Log.Info("This script doesn't serve any function atm. Enable bool or extend script");
		}
	}
}
