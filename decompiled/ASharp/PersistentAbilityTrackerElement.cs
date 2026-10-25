using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentAbilityTrackerElement : MonoBehaviour
{
	[SerializeField]
	private Image circleImage;

	[SerializeField]
	private TextMeshProUGUI experienceText;

	[SerializeField]
	private bool changeColors = true;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[SerializeField]
	private Sprite activeIcon;

	[SerializeField]
	private Sprite pastRoundIcon;

	[SerializeField]
	private Sprite leftRoundIcon;

	public void SetElementXP(int experience, bool isActive, bool isCompleted = false)
	{
		Color color;
		if (isActive)
		{
			color = UIInfoTools.Instance.currentRoundColor;
		}
		else
		{
			color = (isCompleted ? UIInfoTools.Instance.pastRoundColor : UIInfoTools.Instance.leftRoundColor);
		}
		Color color2;
		if (isActive)
		{
			color2 = UIInfoTools.Instance.currentRoundTextColor;
		}
		else
		{
			color2 = (isCompleted ? UIInfoTools.Instance.pastRoundTextColor : UIInfoTools.Instance.leftRoundTextColor);
		}
		experienceText.text = ((experience == 0) ? "" : new string('I', experience));
		experienceText.color = color2;
		if (changeColors)
		{
			circleImage.color = color;
		}
		if (_tooltip != null)
		{
			_tooltip.enabled = experience > 0;
			_tooltip.SetText(string.Format("{0} {1}", experience, LocalizationManager.GetTranslation("XP")));
		}
		Image image = circleImage;
		Sprite sprite;
		if (isActive)
		{
			sprite = activeIcon;
		}
		else
		{
			sprite = (isCompleted ? pastRoundIcon : leftRoundIcon);
		}
		image.sprite = sprite;
	}

	public void SetElementShield(bool isActive, bool isCompleted = false)
	{
	}
}
