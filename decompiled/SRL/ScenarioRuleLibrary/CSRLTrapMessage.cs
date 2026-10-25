namespace ScenarioRuleLibrary;

internal class CSRLTrapMessage : ScenarioRuleClient.CSRLMessage
{
	public CObjectTrap m_TrapProp;

	public CActor m_Actor;

	public CSRLTrapMessage(CObjectTrap trapProp, CActor actor)
		: base(ScenarioRuleClient.EMessageType.ACTIVATE_TRAP)
	{
		m_TrapProp = trapProp;
		m_Actor = actor;
	}
}
