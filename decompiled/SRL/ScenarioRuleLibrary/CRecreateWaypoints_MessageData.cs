namespace ScenarioRuleLibrary;

public class CRecreateWaypoints_MessageData : CMessageData
{
	public CAbilityMove m_MoveAbility;

	public CRecreateWaypoints_MessageData(CAbilityMove moveAbility)
		: base(MessageType.RecreateWaypoints, null)
	{
		m_MoveAbility = moveAbility;
	}
}
