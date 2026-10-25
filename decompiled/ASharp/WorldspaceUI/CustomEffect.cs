using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class CustomEffect : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private IEffectBlink blinkEffect;

	public void ShowEffect(Sprite icon, string titleTooltip = null, string descriptionTooltip = null, bool blink = false)
	{
		image.sprite = icon;
		if (titleTooltip.IsNOTNullOrEmpty() || descriptionTooltip.IsNOTNullOrEmpty())
		{
			tooltip.enabled = true;
			tooltip.SetText(titleTooltip, refreshTooltip: false, descriptionTooltip);
		}
		else
		{
			tooltip.enabled = false;
		}
		EnableBlink(blink);
		gameObject.SetActive(value: true);
	}

	public void EnableBlink(bool blink)
	{
		blinkEffect.enabled = blink;
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
