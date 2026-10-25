using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class HorizontalOrVerticalLayoutGroup : UnityEngine.UI.HorizontalOrVerticalLayoutGroup
{
	[SerializeField]
	private bool _vertical;

	[SerializeField]
	private bool _skipPaddingIfEmpty;

	protected HorizontalOrVerticalLayoutGroup()
	{
	}

	public override void CalculateLayoutInputHorizontal()
	{
		RectOffset rectOffset = m_Padding;
		base.CalculateLayoutInputHorizontal();
		if (_skipPaddingIfEmpty && rectChildren.Count == 0)
		{
			m_Padding = new RectOffset(0, 0, 0, 0);
		}
		CalcAlongAxis(0, _vertical);
		m_Padding = rectOffset;
	}

	public override void CalculateLayoutInputVertical()
	{
		RectOffset rectOffset = m_Padding;
		if (_skipPaddingIfEmpty && rectChildren.Count == 0)
		{
			m_Padding = new RectOffset(0, 0, 0, 0);
		}
		CalcAlongAxis(1, _vertical);
		m_Padding = rectOffset;
	}

	public override void SetLayoutHorizontal()
	{
		RectOffset rectOffset = m_Padding;
		if (_skipPaddingIfEmpty && rectChildren.Count == 0)
		{
			m_Padding = new RectOffset(0, 0, 0, 0);
		}
		SetChildrenAlongAxis(0, _vertical);
		m_Padding = rectOffset;
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1, _vertical);
	}
}
