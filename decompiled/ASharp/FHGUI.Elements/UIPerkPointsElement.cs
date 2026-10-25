using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPerkPointsElement : MonoBehaviour
{
	private const uint CHECKS_PER_MARK = 3u;

	[Header("Components")]
	[SerializeField]
	private TMP_Text m_PerkMarks;

	[SerializeField]
	private Image m_ImageChecks;

	[SerializeField]
	private Sprite m_Single;

	[SerializeField]
	private Sprite m_Double;

	[SerializeField]
	private Sprite m_Full;

	[Header("Tooltip")]
	[SerializeField]
	private UITextTooltipTarget m_TooltipTarget;

	public void Init(uint perkMarks, uint perkChecks, string tooltipKey)
	{
		if ((bool)m_PerkMarks)
		{
			m_PerkMarks.text = perkMarks.ToString();
		}
		if ((bool)m_ImageChecks)
		{
			Image imageChecks = m_ImageChecks;
			imageChecks.sprite = perkChecks switch
			{
				1u => m_Single, 
				2u => m_Double, 
				_ => m_Full, 
			};
			m_ImageChecks.enabled = m_ImageChecks.sprite != null;
		}
		if (m_TooltipTarget != null)
		{
			m_TooltipTarget.SetText(FHUILocFormatting.PopulateParams(tooltipKey, perkMarks, perkChecks, 3u));
		}
	}
}
