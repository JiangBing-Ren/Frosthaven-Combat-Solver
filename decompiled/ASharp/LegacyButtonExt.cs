using TMPro;

public static class LegacyButtonExt
{
	public static void SetTextAndTooltip(this ILegacyButton legacyButton, string text, ref string textField, TextMeshProUGUI textComponent, UITextTooltipTarget tooltipComponent)
	{
		if (!(legacyButton.Text == text))
		{
			textField = text;
			if (textComponent != null)
			{
				textComponent.text = text;
			}
			if (tooltipComponent != null)
			{
				tooltipComponent.SetText(text);
			}
		}
	}
}
