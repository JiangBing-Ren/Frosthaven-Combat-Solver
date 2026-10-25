namespace ScenarioRuleLibrary;

public class CPreventDamageTriggered_MessageData(string animOverload, CActor actorSpawningMessage) : CMessageData(MessageType.PreventDamageTriggered, actorSpawningMessage, animOverload)
{
	public CBaseCard m_PreventDamageBaseCard;

	public int m_OverridePreventedDamage = -1;

	public CAbility m_DamageAbility;
}
