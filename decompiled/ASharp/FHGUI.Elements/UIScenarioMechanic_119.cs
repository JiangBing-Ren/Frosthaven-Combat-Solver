using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioMechanic_119 : UIScenarioMechanicsEntry
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text m_DustCount;

	public override void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		base.Init(newEntry, data, localizationParams, simpleTextLocKey);
		if (ScenarioScript.current is ScenarioScript_FH119 scenarioScript)
		{
			Populate(scenarioScript, localizationParams);
		}
	}

	private void Populate(ScenarioScript_FH119 scenarioScript, LocalizationParam[] localizationParams)
	{
		if (!(m_DustCount == null))
		{
			m_DustCount.text = scenarioScript.GetNumDusts().ToString();
		}
	}
}
