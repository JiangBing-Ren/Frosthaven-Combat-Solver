using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class FHRadialElement : MonoBehaviour
{
	[SerializeField]
	private RectTransform _startPipPivot;

	[SerializeField]
	private RectTransform _endPipPivot;

	[SerializeField]
	private Image _fill;

	[SerializeField]
	[Range(0f, 1f)]
	private float _start;

	[SerializeField]
	[Range(0f, 1f)]
	private float _current;

	public void SetProgress(float progress)
	{
		_current = Mathf.Clamp01(progress);
		if (_startPipPivot != null)
		{
			_startPipPivot.rotation = Quaternion.Euler(0f, 0f, -1f * _start * 360f);
		}
		if (_endPipPivot != null)
		{
			_endPipPivot.rotation = Quaternion.Euler(0f, 0f, -1f * (_start + _current) * 360f);
		}
		if (_fill != null)
		{
			_fill.fillAmount = progress;
			_fill.transform.rotation = Quaternion.Euler(0f, 0f, -1f * _start * 360f);
		}
	}
}
