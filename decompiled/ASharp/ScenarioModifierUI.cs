using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class ScenarioModifierUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI scenarioModifierText;

	[HideInInspector]
	public CScenarioModifier m_ScenarioModifier;

	public void Init(CScenarioModifier scenarioModifier, string locString)
	{
		m_ScenarioModifier = scenarioModifier;
		SetText(locString);
	}

	public void SetText(string locString)
	{
		scenarioModifierText.text = locString;
	}
}
