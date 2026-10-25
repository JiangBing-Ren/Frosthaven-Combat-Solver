using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioMechanic_097 : UIScenarioMechanicsEntry
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_FlamingBladespinnerContainer;

	public override void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		base.Init(newEntry, data, localizationParams, simpleTextLocKey);
		if (ScenarioScript.current is ScenarioScript_FH097 scenarioScript)
		{
			Populate(scenarioScript, localizationParams);
		}
	}

	private void Populate(ScenarioScript_FH097 scenarioScript, LocalizationParam[] localizationParams)
	{
		if (!(m_FlamingBladespinnerContainer == null))
		{
			m_FlamingBladespinnerContainer.gameObject.SetActive(ScenarioScript.PartySize >= 3);
		}
	}
}
