namespace ScenarioRuleLibrary;

public class COnItemAbilityUnstacked_MessageData : CMessageData
{
	public CAbility m_ItemAbility;

	public CItem m_Item;

	public COnItemAbilityUnstacked_MessageData(CActor actorSpawningMessage)
		: base(MessageType.OnItemAbilityUnstacked, actorSpawningMessage)
	{
	}
}
