using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

public class AbilityCardUIClassElementsBlinkblade : MonoBehaviour, IAbilityCardClassUIElements
{
	[SerializeField]
	private CanvasGroup[] m_CanvasGroupsFast;

	[SerializeField]
	private CanvasGroup[] m_CanvasGroupsSlow;

	[SerializeField]
	private float m_AlphaActive = 1f;

	[SerializeField]
	private float m_AlphaInactive = 0.33f;

	[SerializeField]
	private GameObject m_InitiativeFastContainer;

	[SerializeField]
	private GameObject m_InitiativeSlowContainer;

	[SerializeField]
	private TextMeshProUGUI initiativeFastText;

	[SerializeField]
	private TextMeshProUGUI initiativeSlowText;

	public void SetupCharacterClassElements(string cardName, AbilityCardYMLData cardData)
	{
		initiativeFastText.text = cardData.Initiative.ToString();
		initiativeSlowText.text = (cardData.Initiative + 30).ToString();
	}

	public void UpdateCharacterClassElements(CPlayerActor cPlayerActor)
	{
		bool flag = cPlayerActor == null || cPlayerActor.Agility != CPlayerActor.EAgility.Slow;
		bool flag2 = cPlayerActor == null || cPlayerActor.Agility != CPlayerActor.EAgility.Fast;
		m_InitiativeFastContainer.SetActive(flag);
		m_InitiativeSlowContainer.SetActive(flag2);
		float alpha = (flag ? m_AlphaActive : m_AlphaInactive);
		float alpha2 = (flag2 ? m_AlphaActive : m_AlphaInactive);
		SetGroupsAlpha(m_CanvasGroupsFast, alpha);
		SetGroupsAlpha(m_CanvasGroupsSlow, alpha2);
	}

	private static void SetGroupsAlpha(IEnumerable<CanvasGroup> groups, float alpha)
	{
		foreach (CanvasGroup group in groups)
		{
			if (!(group == null))
			{
				group.alpha = alpha;
			}
		}
	}
}
