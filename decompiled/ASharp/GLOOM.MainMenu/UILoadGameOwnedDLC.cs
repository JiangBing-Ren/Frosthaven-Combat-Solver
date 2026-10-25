using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UILoadGameOwnedDLC : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextLocalizedListener nameText;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	public void SetDLC(DLCRegistry.EDLCKey dlc)
	{
		icon.sprite = UIInfoTools.Instance.GetDLCShieldIcon(dlc);
		nameText.SetTextKey(dlc.ToString());
		tooltip.SetText(LocalizationManager.GetTranslation($"GUI_{dlc}_OWNED_TOOLTIP"));
	}
}
