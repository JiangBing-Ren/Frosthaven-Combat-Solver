using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Notifications;

public class ToastNotification : MonoBehaviour, IPooleable
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Image _background;

	private RectTransform _rectTransform;

	public float Height => RectTransform.sizeDelta.y;

	public float Width => RectTransform.sizeDelta.x;

	public CanvasGroup CanvasGroup => _canvasGroup;

	public RectTransform RectTransform
	{
		get
		{
			if (!_rectTransform)
			{
				_rectTransform = transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	private void Awake()
	{
		_rectTransform = transform as RectTransform;
	}

	public void Init(string t)
	{
		gameObject.SetActive(value: true);
		_text.text = t;
		_canvasGroup.alpha = 0f;
	}

	public void OnReturnedToPool()
	{
		gameObject.SetActive(value: false);
	}

	public void OnRemovedFromPool()
	{
		gameObject.SetActive(value: true);
		_canvasGroup.alpha = 0f;
	}
}
