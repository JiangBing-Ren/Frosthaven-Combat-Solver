using ScenarioRuleLibrary;

namespace FHGUI.Scenario;

public abstract class FHScenarioPhaseViewState : FHScenarioViewState
{
	public abstract CPhase.PhaseType PhaseType { get; }
}
