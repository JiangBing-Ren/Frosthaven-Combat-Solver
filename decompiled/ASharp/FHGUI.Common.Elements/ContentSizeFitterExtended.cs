using System;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class ContentSizeFitterExtended : ContentSizeFitter
{
	[SerializeField]
	private float _maxX = -1f;

	[SerializeField]
	private float _maxY = -1f;

	[NonSerialized]
	private RectTransform m_Rect;

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

	public override void SetLayoutHorizontal()
	{
		base.SetLayoutHorizontal();
		if (_maxX >= 0f)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(rectTransform.sizeDelta.x, _maxX));
		}
	}

	public override void SetLayoutVertical()
	{
		base.SetLayoutVertical();
		if (_maxY >= 0f)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(rectTransform.sizeDelta.y, _maxY));
		}
	}
}
