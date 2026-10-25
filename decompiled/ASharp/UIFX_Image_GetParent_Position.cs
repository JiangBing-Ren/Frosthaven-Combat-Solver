using UnityEngine;
using UnityEngine.UI;

public class UIFX_Image_GetParent_Position : MonoBehaviour
{
	public string shaderProperty = "_Position";

	private Vector3 parentPosition;

	private void Start()
	{
		parentPosition = GetComponentInParent<Transform>().position;
		Image component = GetComponent<Image>();
		component.material = new Material(component.material);
		component.material.SetVector(shaderProperty, parentPosition);
	}

	private void Update()
	{
	}
}
