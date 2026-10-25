using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UICharacterSummary : MonoBehaviour
{
	[Header("Base Elements")]
	[SerializeField]
	private Image m_HealthForeground;

	[SerializeField]
	private Image m_XPForeground;

	[SerializeField]
	private TextLocalizedListener m_TextXP;

	[SerializeField]
	private TextLocalizedListener m_TextHealth;

	public void Init(CharacterDataDTO data)
	{
		if (m_HealthForeground != null)
		{
			m_HealthForeground.fillAmount = (float)data.Health.Value / (float)data.Health.Max;
		}
		if (m_XPForeground != null)
		{
			m_XPForeground.fillAmount = (float)data.Experience.Value / (float)data.Experience.Max;
		}
		if (m_TextXP != null)
		{
			m_TextXP.SetParameters(data.Experience.ToLocalizedParams());
		}
		if (m_TextHealth != null)
		{
			m_TextHealth.GetComponent<TextMeshProUGUI>().text = $"{data.Health.Value}/{data.Health.Max}";
		}
	}
}
