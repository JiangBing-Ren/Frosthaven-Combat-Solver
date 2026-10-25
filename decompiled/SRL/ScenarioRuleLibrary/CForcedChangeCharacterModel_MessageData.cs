namespace ScenarioRuleLibrary;

public class CForcedChangeCharacterModel_MessageData : CMessageData
{
	public CActor m_ActorToChange;

	public CForcedChangeCharacterModel_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ForcedChangeCharacterModel, actorSpawningMessage)
	{
		m_ActorToChange = actorSpawningMessage;
	}
}
