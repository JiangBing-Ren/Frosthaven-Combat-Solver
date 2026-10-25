using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISliderBar : MonoBehaviour
{
	[SerializeField]
	private List<TextMeshProUGUI> amountTexts;

	[SerializeField]
	protected Slider progress;

	[Tooltip("{0} will be replaced by current amount and {1} will be replaced by the total amount")]
	[SerializeField]
	private string amountFormat = "{0}/{1}";

	public virtual void SetAmount(int total, int currentAmount)
	{
		progress.maxValue = total;
		SetAmount(currentAmount);
	}

	public virtual void SetAmount(int amount)
	{
		SetAmountText(amount);
		UpdateProgressBar(amount);
	}

	protected void SetAmountText(int amount)
	{
		string text = string.Format(amountFormat, amount, progress.maxValue);
		for (int i = 0; i < amountTexts.Count; i++)
		{
			amountTexts[i].text = text;
		}
	}

	protected virtual void UpdateProgressBar(float amount)
	{
		progress.SetValueWithoutNotify(amount);
	}
}
