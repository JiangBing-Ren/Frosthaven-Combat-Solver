using UnityEngine;

internal class InitiativeOption : IOption
{
	private int initiativeIncrease;

	private int newInitiative;

	public string ID => (initiativeIncrease > 0).ToString();

	public InitiativeOption(int initiativeIncrease, int newInitiative)
	{
		this.initiativeIncrease = initiativeIncrease;
		this.newInitiative = newInitiative;
	}

	public Sprite GetPickerIcon()
	{
		return null;
	}

	public string GetPickerText()
	{
		return ((initiativeIncrease < 0) ? initiativeIncrease.ToString() : $"+{initiativeIncrease}") ?? "";
	}

	public string GetSelectedText()
	{
		return newInitiative.ToString();
	}

	public Color GetSelectedTextColor()
	{
		return UIInfoTools.DefInstance.basicTextColor;
	}
}
