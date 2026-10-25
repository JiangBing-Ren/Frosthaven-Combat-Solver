using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

[RequireComponent(typeof(Image))]
public class TestingReferenceScreenshot : MonoBehaviour
{
	public Image Image => GetComponent<Image>();

	private void Awake()
	{
		SetAlpha(0f);
	}

	public void SetAlpha(float alpha)
	{
		Image image = Image;
		image.raycastTarget = false;
		Color color = image.color;
		color.a = Mathf.Clamp(alpha, 0f, 1f);
		image.color = color;
	}
}
