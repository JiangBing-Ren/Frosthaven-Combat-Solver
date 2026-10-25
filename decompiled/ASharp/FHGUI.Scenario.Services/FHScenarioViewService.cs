using ScenarioRuleLibrary;

namespace FHGUI.Scenario.Services;

public abstract class FHScenarioViewService
{
	protected readonly FHScenarioViewController _view;

	public Choreographer Choreographer => Choreographer.s_Choreographer;

	public CScenario Scenario => ScenarioManager.Scenario;

	public FHScenarioViewService(FHScenarioViewController view)
	{
		_view = view;
	}

	public virtual void OnDestroy()
	{
	}
}
