namespace ScenarioRuleLibrary;

public class CRedirectedAttack_MessageData : CMessageData
{
	public CActor m_Target;

	public CActor m_OriginalTarget;

	public string m_ProjectileName;

	public CRedirectedAttack_MessageData(CActor actorSpawningMessage)
		: base(MessageType.RedirectAttack, actorSpawningMessage)
	{
	}
}
