namespace FHGUI.Scenario;

public abstract class FHScenarioViewState : FHViewStateBase
{
	public FHScenarioViewController View => Singleton<FHScenarioViewController>.Instance;

	public abstract void Update();
}
