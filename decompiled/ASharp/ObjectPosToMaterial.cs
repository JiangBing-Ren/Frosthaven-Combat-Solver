using SETUtil.Materials;
using UnityEngine;

public class ObjectPosToMaterial : MonoBehaviour
{
	public string materialProperty = "_ObjPos";

	private Vector3 moved;

	public bool rendererTypeProjector = true;

	private void Start()
	{
	}

	private void OnEnable()
	{
		if (rendererTypeProjector)
		{
			SetProjectorMaterialProperty();
		}
		else
		{
			SetMaterialProperty();
		}
	}

	private void SetProjectorMaterialProperty()
	{
		GetComponent<Projector>().material.SetVector(materialProperty, transform.position);
	}

	private void SetMaterialProperty()
	{
		GetComponent<SkinnedMeshRenderer>().GetMaterialClonesSlow()[0].SetVector(materialProperty, transform.position);
	}
}
