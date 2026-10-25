namespace UnityEngine.UI;

[RequireComponent(typeof(UITextTooltipTarget))]
public class UITextTooltipGlossaryBuilder : MonoBehaviour
{
	private UITextTooltipTarget _tooltip;

	private void OnEnable()
	{
		if ((object)_tooltip == null)
		{
			_tooltip = GetComponent<UITextTooltipTarget>();
		}
		if (!(_tooltip == null) && LocalizationGlossaryUtils.TryParseGlossaryTexts(gameObject, out var parsedText))
		{
			_tooltip.SetText(parsedText);
		}
	}
}
