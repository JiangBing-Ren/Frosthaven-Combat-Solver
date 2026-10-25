using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniAbilityCardIndicator : MonoBehaviour
{
	public IndicatorType type;

	public GameObject indicator;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI numberText;

	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color highlightedColor;

	public int offset = 32;

	public void ShowNumber(int number)
	{
		numberText.text = number.ToString();
		numberText.gameObject.SetActive(number > 0);
	}

	public void HideNumber()
	{
		numberText.gameObject.SetActive(value: false);
	}

	public void Highlight(bool enabled)
	{
		icon.color = (enabled ? highlightedColor : normalColor);
	}

	public void Show()
	{
		Highlight(enabled: false);
		indicator.SetActive(value: true);
	}

	public void Hide()
	{
		indicator.SetActive(value: false);
	}
}
