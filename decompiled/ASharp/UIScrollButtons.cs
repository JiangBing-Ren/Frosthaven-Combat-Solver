using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIScrollButtons : MonoBehaviour
{
	[SerializeField]
	private FHButton _startButton;

	[SerializeField]
	private FHButton _endButton;

	[SerializeField]
	private ScrollRect _scroll;

	private void OnEnable()
	{
		if (_scroll != null)
		{
			_scroll.onValueChanged.AddListener(OnScroll);
			AdjustVisuals();
		}
		_startButton.onClick.AddListener(ScrollUpOrLeft);
		_endButton.onClick.AddListener(ScrollDownOrRight);
	}

	private void ScrollDownOrRight()
	{
		if (!(_scroll == null))
		{
			Vector2 normalizedPosition = _scroll.normalizedPosition;
			_scroll.normalizedPosition = (_scroll.horizontal ? new Vector2(Mathf.Clamp01(normalizedPosition.x - 0.1f), normalizedPosition.y) : new Vector2(normalizedPosition.x, Mathf.Clamp01(normalizedPosition.y - 0.1f)));
		}
	}

	private void ScrollUpOrLeft()
	{
		if (!(_scroll == null))
		{
			Vector2 normalizedPosition = _scroll.normalizedPosition;
			_scroll.normalizedPosition = (_scroll.horizontal ? new Vector2(Mathf.Clamp01(normalizedPosition.x + 0.1f), normalizedPosition.y) : new Vector2(normalizedPosition.x, Mathf.Clamp01(normalizedPosition.y + 0.1f)));
		}
	}

	private void OnDisable()
	{
		if (_scroll != null)
		{
			_scroll.onValueChanged.RemoveListener(OnScroll);
		}
		_startButton.onClick.RemoveListener(ScrollUpOrLeft);
		_endButton.onClick.RemoveListener(ScrollDownOrRight);
	}

	private void OnScroll(Vector2 arg0)
	{
		AdjustVisuals();
	}

	private void AdjustVisuals()
	{
		if (!(_scroll == null))
		{
			bool horizontal = _scroll.horizontal;
			float num = (horizontal ? _scroll.content.sizeDelta.x : _scroll.content.sizeDelta.y);
			float num2 = (horizontal ? _scroll.viewport.sizeDelta.x : _scroll.viewport.sizeDelta.y);
			bool flag = num > num2;
			float num3 = (horizontal ? _scroll.normalizedPosition.x : _scroll.normalizedPosition.y);
			_startButton.interactable = flag && num3 < 1f;
			_endButton.interactable = flag && num3 > 0f;
		}
	}

	private void OnValidate()
	{
		AdjustVisuals();
	}
}
