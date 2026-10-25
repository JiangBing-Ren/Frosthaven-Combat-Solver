using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class FHImageFillElement : MonoBehaviour
{
	[SerializeField]
	private Image _fill;

	[SerializeField]
	private RectTransform _pip;

	[SerializeField]
	private RectTransform _pipContainer;

	[SerializeField]
	[Range(0f, 1f)]
	private float _current;

	public void SetProgress(float progress)
	{
		_current = Mathf.Clamp01(progress);
		if (_pipContainer != null)
		{
			if (_fill.fillMethod == Image.FillMethod.Horizontal)
			{
				_pipContainer.anchorMax = new Vector2(progress, _pipContainer.anchorMax.y);
			}
			else if (_fill.fillMethod == Image.FillMethod.Vertical)
			{
				_pipContainer.anchorMax = new Vector2(_pipContainer.anchorMax.x, progress);
			}
		}
		_fill.fillAmount = progress;
	}

	public void Set(float min, float max)
	{
		SetProgress((max <= 0f) ? 0f : (min / max));
	}
}
