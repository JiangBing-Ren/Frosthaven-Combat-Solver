namespace ScenarioRuleLibrary;

public class CShowConsumes_MessageData : CMessageData
{
	public CAbilityConsume m_Ability;

	public bool m_IsFirstAbility;

	public CShowConsumes_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowConsumes, actorSpawningMessage)
	{
	}
}
