namespace ScenarioRuleLibrary;

public class CChangeModifier_MessageData : CMessageData
{
	public string m_OriginalModifier;

	public CActiveBonus m_ActiveBonus;

	public CChangeModifier_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ChangeModifier, actorSpawningMessage)
	{
	}
}
