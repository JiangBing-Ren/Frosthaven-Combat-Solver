using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class UISummonTooltip : MonoBehaviour
{
	public class TooltipParams
	{
		public HeroSummonGeneralDTO Summon;

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
	private TMP_Text m_Hints;

	[Header("Stats")]
	[SerializeField]
	private UIBaseStatsView _stats;

	[SerializeField]
	private UICompactAbilityCard m_SourceCard;

	public void Init(TooltipParams data)
	{
		HeroSummonGeneralDTO summon = data.Summon;
		if (m_Caption != null)
		{
			m_Caption.text = summon.Name;
		}
		if (m_Description != null)
		{
			m_Description.text = summon.Description;
		}
		if (m_Icon != null)
		{
			m_Icon.sprite = summon.Icon;
			m_Icon.gameObject.SetActive(summon.Icon != null);
		}
		if (m_Hints != null)
		{
			m_Hints.text = data.Hints;
		}
		_stats.Init(summon.Stats);
		bool hasValue = summon.SourceCard.HasValue;
		m_SourceCard.gameObject.SetActive(hasValue);
		if (hasValue)
		{
			m_SourceCard.Init(UIGeneralInfoUtils.GetAbilityCardInfo(summon.SourceCard.Value), null);
		}
	}
}
