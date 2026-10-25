namespace ScenarioRuleLibrary;

internal class CSRLChestMessage : ScenarioRuleClient.CSRLMessage
{
	public CObjectChest m_ChestProp;

	public CActor m_Actor;

	public CSRLChestMessage(CObjectChest chestProp, CActor actor)
		: base(ScenarioRuleClient.EMessageType.ACTIVATE_CHEST)
	{
		m_ChestProp = chestProp;
		m_Actor = actor;
	}
}
