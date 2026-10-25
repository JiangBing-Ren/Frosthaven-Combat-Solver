namespace ScenarioRuleLibrary;

public class CSetActiveActorVisuals_MessageData : CMessageData
{
	public CActor Actor { get; private set; }

	public bool Active { get; private set; }

	public CSetActiveActorVisuals_MessageData(CActor actor, bool active)
		: base(MessageType.SetActiveActorVisuals, actor)
	{
		Actor = actor;
		Active = active;
	}
}
