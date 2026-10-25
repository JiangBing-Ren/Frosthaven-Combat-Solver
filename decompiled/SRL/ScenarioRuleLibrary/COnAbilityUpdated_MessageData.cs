namespace ScenarioRuleLibrary;

public class COnAbilityUpdated_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public COnAbilityUpdated_MessageData(CActor actorSpawningMessage)
		: base(MessageType.OnAbilityUpdated, actorSpawningMessage)
	{
	}
}
