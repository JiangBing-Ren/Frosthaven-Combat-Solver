namespace ScenarioRuleLibrary;

public class CActorWantsGenericConfirmation_MessageData : CMessageData
{
	public string m_TitleTextLocKey;

	public string m_HelpBoxTextLocKey;

	public string m_CancelButtonLocKey;

	public string m_ConfirmButtonLocKey;

	public bool m_ShowHeader;

	public object[] m_HelpParams;

	public CActorWantsGenericConfirmation_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorWantsGenericConfirmation, actorSpawningMessage)
	{
	}
}
