using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverMoveEffects : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public enum LockType
	{
		None,
		InitialPosition,
		MoveOffset,
		MoveOffsetOnClick
	}

	[Header("Reference Point")]
	[Tooltip("If set use the ReferencePoint as the target movement's starting position instead of the transform's original position.")]
	public Transform ReferencePoint;

	public bool UseReferenceX;

	public bool UseReferenceY;

	[Header("Offset")]
	public Vector2 MoveOffset = new Vector2(10f, 0f);

	public Vector2 MoveOffsetOnClick = new Vector2(10f, 0f);

	[Header("General Settings")]
	public float Duration = 0.3f;

	public bool MoveOnSelected = true;

	public bool MoveNonInteractable = true;

	public bool MoveOnClick = true;

	public bool ResetPositionOnDisable = true;

	[Tooltip("Select target transform to scale, or leave empty for this object.")]
	public RectTransform OverrideTransform;

	private Selectable _selectable;

	private Tween _tween;

	private Vector2? _iPos;

	private Vector2 _disabledPosition;

	private LockType _lockType;

	private bool _isPointerOver;

	private Vector2 _initialPosition
	{
		get
		{
			if (_iPos.HasValue)
			{
				return _iPos.Value;
			}
			if (OverrideTransform == null)
			{
				OverrideTransform = (RectTransform)transform;
			}
			_iPos = OverrideTransform.anchoredPosition;
			return _iPos.Value;
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
		if (OverrideTransform == null)
		{
			OverrideTransform = (RectTransform)transform;
		}
		_isPointerOver = false;
		if (_lockType != LockType.None)
		{
			SetLock(_lockType, forceUpdate: true);
		}
	}

	private void OnDisable()
	{
		_tween?.Kill();
		if (ResetPositionOnDisable)
		{
			OverrideTransform.anchoredPosition = _initialPosition;
		}
		_isPointerOver = false;
	}

	private void MoveTo(Vector2 position)
	{
		if (_lockType == LockType.None)
		{
			_tween?.Kill();
			_tween = OverrideTransform.DOAnchorPos(position, Duration).SetEase(Ease.OutExpo).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (MoveOnClick && _isInteractable)
		{
			MoveTo(GetTargetPosition(onClick: true));
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (MoveOnClick)
		{
			if (_isInteractable)
			{
				MoveTo(GetTargetPosition(onClick: false));
			}
			else
			{
				MoveTo(_initialPosition);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isPointerOver = true;
		if (_isInteractable)
		{
			MoveTo(GetTargetPosition(onClick: false));
		}
	}

	private Vector2 GetTargetPosition(bool onClick)
	{
		if ((!UseReferenceX && !UseReferenceY) || ReferencePoint == null)
		{
			if (!onClick)
			{
				return _initialPosition + MoveOffset;
			}
			return _initialPosition + MoveOffsetOnClick;
		}
		Vector2 vector = transform.InverseTransformPoint(ReferencePoint.position);
		Vector2 vector2 = new Vector2
		{
			x = (UseReferenceX ? vector.x : _initialPosition.x),
			y = (UseReferenceY ? vector.y : _initialPosition.y)
		} + _initialPosition;
		if (!onClick)
		{
			return vector2;
		}
		return vector2 - (MoveOffset - MoveOffsetOnClick);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isPointerOver = false;
		if (!MoveOnSelected || !(EventSystem.current?.currentSelectedGameObject == gameObject))
		{
			MoveTo(_initialPosition);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_isInteractable && MoveOnSelected)
		{
			MoveTo(GetTargetPosition(onClick: false));
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (MoveOnSelected)
		{
			MoveTo(_initialPosition);
		}
	}

	public void SetLock(LockType lockType, bool forceUpdate = false)
	{
		if (_lockType == lockType && !forceUpdate)
		{
			return;
		}
		Vector2 vector;
		switch (lockType)
		{
		case LockType.None:
			if (_isPointerOver)
			{
				vector = GetTargetPosition(onClick: false);
				break;
			}
			if (!_isPointerOver)
			{
				vector = _initialPosition;
				break;
			}
			goto default;
		case LockType.InitialPosition:
			vector = _initialPosition;
			break;
		case LockType.MoveOffset:
			vector = GetTargetPosition(onClick: false);
			break;
		case LockType.MoveOffsetOnClick:
			vector = GetTargetPosition(onClick: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("lockType", lockType, null);
		}
		Vector2 position = vector;
		_lockType = LockType.None;
		MoveTo(position);
		_lockType = lockType;
	}
}
