using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class AspectRatioFitterTransition : UIBehaviour, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private float m_AspectRatio = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_TransitionPercent;

	[SerializeField]
	private Vector2 offsetFrom = Vector2.zero;

	[SerializeField]
	private Vector2 offsetTo = Vector2.one;

	[SerializeField]
	private Vector2 anchoredPositionFrom = Vector2.zero;

	[SerializeField]
	private Vector2 anchoredPositionTo = Vector2.one;

	[NonSerialized]
	private RectTransform m_Rect;

	private DrivenRectTransformTracker m_Tracker;

	public float aspectRatio
	{
		get
		{
			return m_AspectRatio;
		}
		set
		{
			m_AspectRatio = value;
			SetDirty();
		}
	}

	public float transitionPercent
	{
		get
		{
			return m_TransitionPercent;
		}
		set
		{
			m_TransitionPercent = Mathf.Clamp01(value);
			SetDirty();
		}
	}

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	protected AspectRatioFitterTransition()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		m_Tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		UpdateRect();
	}

	private void UpdateRect()
	{
		if (IsActive())
		{
			m_Tracker.Clear();
			m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.Lerp(anchoredPositionFrom, anchoredPositionTo, transitionPercent);
			Vector2 size = GetSize(AspectRatioFitter.AspectMode.FitInParent);
			Vector2 size2 = GetSize(AspectRatioFitter.AspectMode.EnvelopeParent);
			rectTransform.sizeDelta = Vector2.Lerp(size, size2, transitionPercent) + Vector2.Lerp(offsetFrom, offsetTo, transitionPercent);
		}
	}

	private Vector2 GetSize(AspectRatioFitter.AspectMode m_AspectMode)
	{
		Vector2 zero = Vector2.zero;
		Vector2 parentSize = GetParentSize();
		if ((parentSize.y * aspectRatio < parentSize.x) ^ (m_AspectMode == AspectRatioFitter.AspectMode.FitInParent))
		{
			zero.y = GetSizeDeltaToProduceSize(parentSize.x / aspectRatio, 1);
		}
		else
		{
			zero.x = GetSizeDeltaToProduceSize(parentSize.y * aspectRatio, 0);
		}
		return zero;
	}

	private float GetSizeDeltaToProduceSize(float size, int axis)
	{
		return size - GetParentSize()[axis] * (rectTransform.anchorMax[axis] - rectTransform.anchorMin[axis]);
	}

	private Vector2 GetParentSize()
	{
		RectTransform rectTransform = this.rectTransform.parent as RectTransform;
		if (!rectTransform)
		{
			return Vector2.zero + Vector2.Lerp(offsetFrom, offsetTo, transitionPercent);
		}
		return rectTransform.rect.size + Vector2.Lerp(offsetFrom, offsetTo, transitionPercent);
	}

	public virtual void SetLayoutHorizontal()
	{
	}

	public virtual void SetLayoutVertical()
	{
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			UpdateRect();
		}
	}
}
