using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ILayoutElement))]
[ExecuteAlways]
public class UILayoutElementMaxSizeConstraint : UIBehaviour, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private float _maxSize;

	[SerializeField]
	private bool _horizontal = true;

	[SerializeField]
	private bool _vertical = true;

	[NonSerialized]
	private RectTransform m_Rect;

	private ILayoutElement _layoutElement;

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

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	public void SetLayoutHorizontal()
	{
		if (_horizontal)
		{
			LayoutUtility.GetMinSize(m_Rect, 0);
			float preferredSize = LayoutUtility.GetPreferredSize(m_Rect, 0);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(preferredSize, _maxSize));
		}
	}

	public void SetLayoutVertical()
	{
		if (_vertical)
		{
			LayoutUtility.GetMinSize(m_Rect, 1);
			float preferredSize = LayoutUtility.GetPreferredSize(m_Rect, 1);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(preferredSize, _maxSize));
		}
	}
}
