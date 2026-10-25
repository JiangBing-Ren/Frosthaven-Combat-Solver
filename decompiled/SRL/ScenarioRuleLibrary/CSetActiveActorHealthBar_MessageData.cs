namespace ScenarioRuleLibrary;

public class CSetActiveActorHealthBar_MessageData : CMessageData
{
	public CActor Actor { get; private set; }

	public bool Active { get; private set; }

	public bool HideOnlyHealthbar { get; private set; }

	public CSetActiveActorHealthBar_MessageData(CActor actor, bool active, bool onlyHealthBar = false)
		: base(MessageType.SetActiveActorHealthBar, actor)
	{
		Actor = actor;
		Active = active;
		HideOnlyHealthbar = onlyHealthBar;
	}
}
