using System;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIPartySlotHints : FHInputEventContainer
{
	public class HintedActionParams : Params
	{
		public string ColorSchemeID;
	}

	[Serializable]
	public struct ColorScheme
	{
		public string ID;

		public Color Background;

		public Color Text;
	}

	[Header("Settings")]
	[SerializeField]
	private Sprite m_MultiOptions;

	[SerializeField]
	private Sprite m_SingleOption;

	[SerializeField]
	private ColorScheme[] m_ColorSchemes;

	protected override void InitCache()
	{
		if (!m_Initialized)
		{
			base.InitCache();
		}
	}

	protected override void PopulateElement(FHInputEventElement element, FHInputEventElement.IInputEventData elementData, int index, int totalCount)
	{
		base.PopulateElement(element, elementData, index, totalCount);
		HintedActionParams hintedActionParams = elementData as HintedActionParams;
		if (!TryGetColors(hintedActionParams.ColorSchemeID, out var colorScheme))
		{
			return;
		}
		if (element.Label != null)
		{
			element.Label.color = colorScheme.Text;
			if (totalCount > 1)
			{
				element.Label.alignment = ((index == 0) ? TextAlignmentOptions.Right : TextAlignmentOptions.Left);
			}
			else
			{
				element.Label.alignment = TextAlignmentOptions.Center;
			}
		}
		if (element.Background != null)
		{
			element.Background.color = colorScheme.Background;
			element.Background.transform.localScale = new Vector3((index != 0) ? 1 : (-1), 1f, 1f);
			element.Background.overrideSprite = ((totalCount == 1) ? m_SingleOption : m_MultiOptions);
		}
	}

	public void Clear()
	{
		ClearData();
		InitCache();
		UIHelperUtil.UpdateUIList(0, m_Elements, m_PrototypeHint, m_Container);
	}

	private bool TryGetColors(string scheme, out ColorScheme colorScheme)
	{
		colorScheme = default;
		if (m_ColorSchemes == null || m_ColorSchemes.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < m_ColorSchemes.Length; i++)
		{
			if (!(m_ColorSchemes[i].ID != scheme))
			{
				colorScheme = m_ColorSchemes[i];
				return true;
			}
		}
		return false;
	}
}
