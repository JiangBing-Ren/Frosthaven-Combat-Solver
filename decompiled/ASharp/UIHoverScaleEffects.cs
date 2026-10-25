using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverScaleEffects : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public enum LockType
	{
		None,
		StartingScale,
		ScaleFactor,
		ScaleFactorOnClick
	}

	public float StartingScaleFactor = 1f;

	public float ScaleFactor = 1.1f;

	public float ScaleFactorOnClick = 1.1f;

	public float Duration = 0.3f;

	public bool ScaleOnSelected = true;

	public bool ScaleNonInteractable = true;

	public bool ScaleOnClick = true;

	[Tooltip("Select target transform to scale, or leave empty for this object.")]
	public RectTransform OverrideTransform;

	private Selectable _selectable;

	private Tween _tween;

	private LockType _lockType;

	private bool _isPointerOver;

	public bool IsInTransition
	{
		get
		{
			if (_tween != null)
			{
				return _tween.IsActive();
			}
			return false;
		}
	}

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null) && !_selectable.IsInteractable())
			{
				return ScaleNonInteractable;
			}
			return true;
		}
	}

	private Vector3 ScaleFactorVector => new Vector3(ScaleFactor, ScaleFactor, 1f);

	private Vector3 ScaleFactorOnClickVector => new Vector3(ScaleFactorOnClick, ScaleFactorOnClick, 1f);

	private Vector3 StartingScaleFactorVector => StartingScaleFactor * Vector3.one;

	private void OnEnable()
	{
		if (_selectable == null)
		{
			_selectable = GetComponent<Selectable>();
		}
		if (OverrideTransform == null)
		{
			OverrideTransform = (RectTransform)transform;
		}
		SetLock(_lockType, forceUpdate: true);
		_isPointerOver = false;
	}

	private void OnDisable()
	{
		if (_tween != null && _tween.IsActive())
		{
			_tween.Kill();
		}
		OverrideTransform.localScale = StartingScaleFactor * Vector3.one;
		_isPointerOver = false;
	}

	private void ScaleTo(Vector3 scale)
	{
		if (_lockType == LockType.None)
		{
			if (_tween != null && _tween.IsActive())
			{
				_tween.Kill();
			}
			_tween = OverrideTransform.DOScale(scale, Duration).SetEase(Ease.OutExpo).SetUpdate(isIndependentUpdate: true);
		}
	}

	private void DoScale()
	{
		ScaleTo(ScaleFactorVector);
	}

	private void DoScaleOnClick()
	{
		ScaleTo(ScaleFactorOnClickVector);
	}

	private void ResetScale()
	{
		ScaleTo(StartingScaleFactorVector);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (ScaleOnClick && _isInteractable)
		{
			_isPointerOver = true;
			DoScaleOnClick();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (ScaleOnClick)
		{
			if (_isInteractable)
			{
				DoScale();
			}
			else
			{
				ResetScale();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			DoScale();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isPointerOver = false;
		ResetScale();
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_isInteractable && ScaleOnSelected)
		{
			DoScale();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (ScaleOnSelected)
		{
			ResetScale();
		}
	}

	public void SetLock(LockType lockType, bool forceUpdate = false)
	{
		if (_lockType == lockType && !forceUpdate)
		{
			return;
		}
		Vector3 vector;
		switch (lockType)
		{
		case LockType.None:
			if (_isPointerOver)
			{
				vector = ScaleFactorVector;
				break;
			}
			if (!_isPointerOver)
			{
				vector = StartingScaleFactorVector;
				break;
			}
			goto default;
		case LockType.StartingScale:
			vector = StartingScaleFactorVector;
			break;
		case LockType.ScaleFactor:
			vector = ScaleFactorVector;
			break;
		case LockType.ScaleFactorOnClick:
			vector = ScaleFactorOnClickVector;
			break;
		default:
			throw new ArgumentOutOfRangeException("lockType", lockType, null);
		}
		Vector3 scale = vector;
		_lockType = LockType.None;
		ScaleTo(scale);
		_lockType = lockType;
	}
}
