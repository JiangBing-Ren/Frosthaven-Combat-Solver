using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIGenericTooltipRenderer : MonoBehaviour
{
	public struct Params
	{
		public string CaptionLiteral;

		public string DescriptionLiteral;

		public Sprite Icon;

		public string Hints;
	}

	[Header("Components")]
	[SerializeField]
	private TMP_Text m_Caption;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_IconContainer;

	[SerializeField]
	private TMP_Text m_Hints;

	[SerializeField]
	private GameObject m_HintsContainer;

	public void Init(Params @params)
	{
		if (m_Caption != null)
		{
			m_Caption.text = @params.CaptionLiteral;
			m_Caption.gameObject.SetActive(@params.CaptionLiteral.IsNOTNullOrEmpty());
		}
		if (m_Description != null)
		{
			m_Description.text = @params.DescriptionLiteral;
		}
		if (m_Icon != null)
		{
			m_Icon.sprite = @params.Icon;
			m_Icon.gameObject.SetActive(@params.Icon != null);
		}
		if (m_IconContainer != null)
		{
			m_IconContainer.SetActive(@params.Icon != null);
		}
		if (m_Hints != null)
		{
			m_Hints.text = @params.Hints;
		}
		if ((bool)m_HintsContainer)
		{
			m_HintsContainer.SetActive(@params.Hints.IsNOTNullOrEmpty());
		}
	}
}
