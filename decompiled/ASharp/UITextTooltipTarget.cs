using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UITextTooltipTarget : UITooltipTarget
{
	[TextArea(3, 5)]
	[SerializeField]
	public string tooltipText;

	[SerializeField]
	private bool textIsLocalizationKeyInstead;

	[SerializeField]
	private bool textIsLocalizationFormat;

	[SerializeField]
	private TMP_SpriteAsset spriteAsset;

	private string shownTooltipText;

	private string shownTooltipSubtext;

	public void SetText(string text, bool refreshTooltip = false, string subtext = null)
	{
		shownTooltipText = text;
		shownTooltipSubtext = subtext;
		if (refreshTooltip && tooltipShown)
		{
			HideTooltip(0f);
			PrepareTooltip(null);
			ShowTooltip(0f);
		}
	}

	protected override bool CanShowTooltip()
	{
		if (shownTooltipText.IsNOTNullOrEmpty() || shownTooltipSubtext.IsNOTNullOrEmpty() || tooltipText.IsNOTNullOrEmpty())
		{
			return base.CanShowTooltip();
		}
		return false;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		UITooltipManager.ResetContent();
		string title;
		if (shownTooltipText.IsNOTNullOrEmpty())
		{
			title = shownTooltipText;
		}
		else if (textIsLocalizationFormat)
		{
			title = CreateLayout.LocaliseText(tooltipText);
		}
		else
		{
			title = (textIsLocalizationKeyInstead ? LocalizationManager.GetTranslation(tooltipText) : tooltipText);
		}
		UITooltipManager.AddTitle(title, spriteAsset);
		if (shownTooltipSubtext.IsNOTNullOrEmpty())
		{
			UITooltipManager.AddDescription(shownTooltipSubtext, spriteAsset);
		}
		base.ShowTooltip(delay);
	}

	protected override void SetControls()
	{
		UITooltipManager.SetVerticalControls(autoAdjustHeight, tempIsPrefabTooltip: true);
	}
}
