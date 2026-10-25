namespace ScenarioRuleLibrary;

public class CActorIsInfusing_MessageData : CMessageData
{
	public ElementInfusionBoardManager.EElement m_Element;

	public bool m_ForceLog;

	public CActorIsInfusing_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsInfusing, actorSpawningMessage)
	{
	}
}
