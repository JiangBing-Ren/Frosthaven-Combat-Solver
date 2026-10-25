namespace ScenarioRuleLibrary;

public class CChoosePlayerActorToBurnCardToPreventDamage_MessageData : CMessageData
{
	public CAttackSummary.TargetSummary m_Summary;

	public CChoosePlayerActorToBurnCardToPreventDamage_MessageData(CAttackSummary.TargetSummary m_Summary)
		: base(MessageType.ChoosePlayerActorToBurnCardToPreventDamage, null)
	{
		this.m_Summary = m_Summary;
	}
}
