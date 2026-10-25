using UnityEngine;
using UnityEngine.UI;

public class UIFX_Image_RectFormat : MonoBehaviour
{
	public string shaderProperty = "_RectFormat";

	public bool adjust;

	public RectTransform rectToTrack;

	public Image image;

	public Vector2 offset = Vector2.zero;

	private void Awake()
	{
		if (rectToTrack == null)
		{
			rectToTrack = (RectTransform)transform.parent;
		}
		image.material = new Material(image.material);
		if (adjust)
		{
			doAdjust();
		}
	}

	public void doAdjust()
	{
		image.material.SetVector(shaderProperty, rectToTrack.rect.size + offset);
	}
}
