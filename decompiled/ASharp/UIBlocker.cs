using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIBlocker : MonoBehaviour
{
	private Image _graphic;

	private Canvas _canvas;

	private void OnEnable()
	{
		_graphic = GetComponent<Image>();
		_graphic.raycastTarget = true;
		_canvas = GetComponentInParent<Canvas>().rootCanvas;
		_graphic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
		_graphic.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		_graphic.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		Place();
	}

	private void Place()
	{
		Vector2 sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, new Vector2(Screen.width, Screen.height) / 2f, _canvas.worldCamera, out var localPoint);
		_graphic.rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
		_graphic.rectTransform.sizeDelta = sizeDelta;
	}

	private void OnRectTransformDimensionsChange()
	{
		Place();
	}
}
