namespace ScenarioRuleLibrary;

public class CWaitForSingleTargetActiveBonus_MessageData : CMessageData
{
	public CActiveBonus m_ActiveBonus;

	public CWaitForSingleTargetActiveBonus_MessageData(CActor actorSpawningMessage)
		: base(MessageType.WaitForSingleTargetActiveBonus, actorSpawningMessage)
	{
	}
}
