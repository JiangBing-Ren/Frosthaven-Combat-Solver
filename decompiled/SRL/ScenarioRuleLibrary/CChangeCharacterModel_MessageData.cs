namespace ScenarioRuleLibrary;

public class CChangeCharacterModel_MessageData : CMessageData
{
	public CActor m_ActorToChange;

	public CAbility m_ChangeCharacterAbility;

	public CChangeCharacterModel_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ChangeCharacterModel, actorSpawningMessage)
	{
		m_ActorToChange = actorSpawningMessage;
	}
}
