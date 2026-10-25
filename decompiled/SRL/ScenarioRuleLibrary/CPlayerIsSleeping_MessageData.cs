namespace ScenarioRuleLibrary;

public class CPlayerIsSleeping_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CPlayerIsSleeping_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerIsSleeping, actorSpawningMessage)
	{
	}
}
