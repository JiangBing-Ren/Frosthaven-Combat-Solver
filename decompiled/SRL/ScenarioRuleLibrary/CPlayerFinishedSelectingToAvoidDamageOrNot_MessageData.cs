namespace ScenarioRuleLibrary;

public class CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerFinishedSelectingToAvoidDamageOrNot, actorSpawningMessage)
	{
	}
}
