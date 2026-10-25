namespace ScenarioRuleLibrary;

public class CRevealDoor_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public CRevealDoor_MessageData(CActor actorSpawningMessage)
		: base(MessageType.RevealProp, actorSpawningMessage)
	{
	}
}
