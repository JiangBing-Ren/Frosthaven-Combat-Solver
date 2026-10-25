namespace ScenarioRuleLibrary;

public class CPlayerActorChangedForm_MessageData : CMessageData
{
	public CActor m_PlayerActor;

	public CPlayerActor.EForm m_OldForm;

	public CPlayerActor.EForm m_NewForm;

	public bool Processed;

	public bool m_Instant;

	public CPlayerActorChangedForm_MessageData(CActor playerActor, CPlayerActor.EForm oldForm, CPlayerActor.EForm newForm, bool instant = false)
		: base(MessageType.PlayerActorChangedForm, playerActor)
	{
		m_PlayerActor = playerActor;
		m_OldForm = oldForm;
		m_NewForm = newForm;
		m_Instant = instant;
	}
}
