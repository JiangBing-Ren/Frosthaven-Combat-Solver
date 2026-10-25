namespace ScenarioRuleLibrary;

public class CPlayerActorChangedPendingForm_MessageData : CMessageData
{
	public CPlayerActor m_PlayerActor;

	public CPlayerActor.EForm m_CurrentForm;

	public CPlayerActor.EForm m_PendingForm;

	public CPlayerActorChangedPendingForm_MessageData(CPlayerActor playerActor, CPlayerActor.EForm currentForm, CPlayerActor.EForm pendingForm)
		: base(MessageType.PlayerActorChangedPendingForm, playerActor)
	{
		m_PlayerActor = playerActor;
		m_CurrentForm = currentForm;
		m_PendingForm = pendingForm;
	}
}
