using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorElementStatesElementGroup : MonoBehaviour
{
	public TextMeshProUGUI m_elementName;

	public Toggle strong;

	public Toggle waning;

	public Toggle inert;

	public void Setup(string elementName)
	{
		m_elementName.text = elementName;
		SetColumn(null);
	}

	public void SetColumn(ElementInfusionBoardManager.EColumn? column)
	{
		strong.isOn = column == ElementInfusionBoardManager.EColumn.Strong;
		waning.isOn = column == ElementInfusionBoardManager.EColumn.Waning;
		inert.isOn = column == ElementInfusionBoardManager.EColumn.Inert;
	}

	public ElementInfusionBoardManager.EColumn? GetColumn()
	{
		if (strong.isOn)
		{
			return ElementInfusionBoardManager.EColumn.Strong;
		}
		if (waning.isOn)
		{
			return ElementInfusionBoardManager.EColumn.Waning;
		}
		if (inert.isOn)
		{
			return ElementInfusionBoardManager.EColumn.Inert;
		}
		return null;
	}
}
