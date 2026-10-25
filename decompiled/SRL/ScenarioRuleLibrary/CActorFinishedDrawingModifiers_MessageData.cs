namespace ScenarioRuleLibrary;

public class CActorFinishedDrawingModifiers_MessageData : CMessageData
{
	public CAttackSummary.TargetSummary m_TargetSummary;

	public CActorFinishedDrawingModifiers_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorFinishedDrawingModifiers, actorSpawningMessage)
	{
	}
}
