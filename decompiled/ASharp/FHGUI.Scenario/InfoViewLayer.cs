using ScenarioRuleLibrary;

namespace FHGUI.Scenario;

public class InfoViewLayer : FHScenarioViewLayer<FHScenarioInfoViewState>
{
	public override bool ProcessMessage(CMessageData messageData)
	{
		return false;
	}
}
