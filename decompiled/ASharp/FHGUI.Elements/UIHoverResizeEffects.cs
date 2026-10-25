using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIHoverResizeEffects : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Header("Offset")]
	public Vector2 NewSize = new Vector2(0f, 0f);

	public bool SetWidth = true;

	public bool SetHeight = true;

	[Header("General Settings")]
	public float Duration = 0.3f;

	public bool MoveOnSelected = true;

	public bool MoveNonInteractable = true;

	public bool MoveOnClick = true;

	[Tooltip("Select target transform to resize, or leave empty for this object.")]
	public RectTransform OverrideTransform;

	private Selectable _selectable;

	private Tween _tween;

	private Vector2? _iSize;

	private bool _lock;

	public Ease Ease = Ease.OutExpo;

	private bool _isPointerOver;

	private Vector2 _initialSize
	{
		get
		{
			if (_iSize.HasValue)
			{
				return _iSize.Value;
			}
			if (OverrideTransform == null)
			{
				OverrideTransform = (RectTransform)transform;
			}
			_iSize = OverrideTransform.sizeDelta;
			return _iSize.Value;
		}
	}

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null) && !_selectable.IsInteractable())
			{
				return MoveNonInteractable;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (_selectable == null)
		{
			_selectable = GetComponent<Selectable>();
		}
		_isPointerOver = false;
	}

	private void OnDisable()
	{
		if (_tween != null && _tween.IsActive())
		{
			_tween.Kill();
		}
		OverrideTransform.sizeDelta = _initialSize;
		_isPointerOver = false;
	}

	private void Resize(Vector2 target, bool immediate = false)
	{
		if (!_lock)
		{
			float duration = (immediate ? 0f : Duration);
			if (_tween != null && _tween.IsActive())
			{
				duration = Duration * _tween.ElapsedPercentage();
				_tween.Kill();
			}
			_tween = OverrideTransform.DOSizeDelta(target, duration).SetEase(Ease).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (MoveOnClick && _isInteractable)
		{
			DoResize();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (MoveOnClick)
		{
			if (_isInteractable)
			{
				DoResize();
			}
			else
			{
				ResetSize();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isPointerOver = true;
		if (_isInteractable)
		{
			DoResize();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isPointerOver = false;
		if (!MoveOnSelected || !(EventSystem.current?.currentSelectedGameObject == gameObject))
		{
			ResetSize();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_isInteractable && MoveOnSelected)
		{
			DoResize();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (MoveOnSelected)
		{
			ResetSize();
		}
	}

	private void DoResize()
	{
		Vector2 target = new Vector2(SetWidth ? NewSize.x : _initialSize.x, SetHeight ? NewSize.y : _initialSize.y);
		Resize(target);
	}

	private void ResetSize()
	{
		Resize(_initialSize);
	}

	public void SetLock(bool locked)
	{
		_lock = false;
		if (EventSystem.current?.currentSelectedGameObject == gameObject)
		{
			DoResize();
		}
		_lock = locked;
		if (!_lock)
		{
			if (_isPointerOver)
			{
				DoResize();
			}
			else
			{
				ResetSize();
			}
		}
	}
}
