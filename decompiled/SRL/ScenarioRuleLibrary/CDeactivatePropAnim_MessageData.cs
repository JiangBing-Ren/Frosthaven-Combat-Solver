namespace ScenarioRuleLibrary;

public class CDeactivatePropAnim_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public bool m_InitialLoad;

	public CDeactivatePropAnim_MessageData(CActor actorSpawningMessage)
		: base(MessageType.DeactivatePropAnim, actorSpawningMessage)
	{
	}
}
