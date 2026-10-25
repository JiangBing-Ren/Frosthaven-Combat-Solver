using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ValueHorizontalPickerUI : MonoBehaviour
{
	[SerializeField]
	private string[] localizationStrings;

	[SerializeField]
	private bool useValueInLocalization;

	[SerializeField]
	private TextMeshProUGUI partySizeText;

	[SerializeField]
	private int minValue = 2;

	[SerializeField]
	private int maxValue = 4;

	private int currentValue;

	private UnityAction<int> onValueChanged;

	public void Init(UnityAction<int> onPartySizeChanged)
	{
		onValueChanged = onPartySizeChanged;
	}

	public void OnChange(bool isLeftArrow)
	{
		int value = minValue + (currentValue - 2 * minValue + maxValue + ((!isLeftArrow) ? 2 : 0)) % (maxValue - minValue + 1);
		SetValue(value);
	}

	public void SetValue(int newValue)
	{
		currentValue = Mathf.Clamp(newValue, minValue, maxValue);
		if (useValueInLocalization)
		{
			try
			{
				partySizeText.text = string.Format(LocalizationManager.GetTranslation(localizationStrings[0]), currentValue);
			}
			catch
			{
				partySizeText.text = "Error";
				string translation = LocalizationManager.GetTranslation(localizationStrings[0]);
				Log.GUIError("Unable to lookup loc value.  \nLoc Key: " + localizationStrings[0] + "\nLookup Text: " + translation);
			}
		}
		else
		{
			partySizeText.text = LocalizationManager.GetTranslation(localizationStrings[currentValue - minValue]);
		}
		if (onValueChanged != null)
		{
			onValueChanged(currentValue);
		}
	}
}
