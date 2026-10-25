using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverReparentEffects : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Tooltip("Select target transform to be the hover parent.")]
	public RectTransform ReparentTargetTransform;

	[Header("General Settings")]
	public bool ReparentOnSelected = true;

	public bool ReparentNonInteractable = true;

	[Tooltip("Select target transform to follow when reparented (usually the initial parent, but it can be something else if you want to anchor this somewhere).")]
	public RectTransform FollowTransform;

	[Tooltip("Select target transform to be reparented, or leave empty for this object.")]
	public RectTransform OverrideTransform;

	private Selectable _selectable;

	private PositionConstraint _positionConstraint;

	private RectTransform _initialParent;

	private Vector2 _initialAnchoredPosition;

	private bool _isInteractable
	{
		get
		{
			if (!(_selectable == null) && !_selectable.IsInteractable())
			{
				return ReparentNonInteractable;
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
		if (FollowTransform != null && _positionConstraint == null)
		{
			if (!TryGetComponent<PositionConstraint>(out _positionConstraint))
			{
				_positionConstraint = gameObject.AddComponent<PositionConstraint>();
			}
			while (_positionConstraint.sourceCount > 0)
			{
				_positionConstraint.RemoveSource(0);
			}
			_positionConstraint.AddSource(new ConstraintSource
			{
				sourceTransform = FollowTransform,
				weight = 1f
			});
			_positionConstraint.weight = 1f;
		}
		if (_initialParent == null)
		{
			_initialParent = (RectTransform)OverrideTransform.parent;
			_initialAnchoredPosition = OverrideTransform.anchoredPosition;
		}
		else
		{
			Reparent(_initialParent);
		}
	}

	private void Reparent(Transform newParent)
	{
		OverrideTransform.SetParent(newParent, worldPositionStays: true);
		if (newParent == _initialParent)
		{
			OverrideTransform.anchoredPosition = _initialAnchoredPosition;
			if (_positionConstraint != null)
			{
				_positionConstraint.constraintActive = false;
			}
		}
		else if (FollowTransform != null && _positionConstraint != null)
		{
			_positionConstraint.constraintActive = true;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isInteractable)
		{
			Reparent(ReparentTargetTransform);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Reparent(_initialParent);
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_isInteractable && ReparentOnSelected)
		{
			Reparent(ReparentTargetTransform);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (ReparentOnSelected)
		{
			Reparent(_initialParent);
		}
	}
}
