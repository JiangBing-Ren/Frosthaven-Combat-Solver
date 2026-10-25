namespace ScenarioRuleLibrary;

public class CImmunity_MessageData : CMessageData
{
	public CActiveBonus m_ImmunityAbility;

	public CCondition.ENegativeCondition negativeCondition;

	public CCondition.EPositiveCondition positiveCondition;

	public CImmunity_MessageData(CActor actorSpawningMessage)
		: base(MessageType.Immunity, actorSpawningMessage)
	{
	}
}
