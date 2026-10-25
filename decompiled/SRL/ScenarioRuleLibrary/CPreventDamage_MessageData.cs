namespace ScenarioRuleLibrary;

public class CPreventDamage_MessageData : CMessageData
{
	public CAbilityPreventDamage m_PreventDamageAbility;

	public CPreventDamage_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PreventDamage, actorSpawningMessage, animOverload)
	{
	}
}
