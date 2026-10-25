namespace ScenarioRuleLibrary;

public class CPlayerIsStunned_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CPlayerIsStunned_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerIsStunned, actorSpawningMessage)
	{
	}
}
