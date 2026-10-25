using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIActiveAbilityTooltip : MonoBehaviour
{
	public class TooltipParams
	{
		public ActiveAbilityDTO ActiveAbility;

		public string Hints;
	}

	[Header("Components")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Hints;

	[SerializeField]
	private UIActiveAbility abilityLayout;

	public void Init(TooltipParams data)
	{
		ActiveAbilityDTO activeAbility = data.ActiveAbility;
		if (m_Icon != null)
		{
			m_Icon.sprite = activeAbility.Icon;
			m_Icon.gameObject.SetActive(activeAbility.Icon != null);
		}
		if (m_Hints != null)
		{
			m_Hints.text = data.Hints;
		}
		abilityLayout.Initialize(activeAbility.Bonus);
	}
}
