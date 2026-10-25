namespace ScenarioRuleLibrary;

public class CReplicateStartRoundCardState_MessageData : CMessageData
{
	public CPlayerActor m_PlayerActor;

	public int m_GameActionID;

	public StartRoundCardState m_StartRoundCardState;

	public CReplicateStartRoundCardState_MessageData(CPlayerActor actor, int gameActionID, StartRoundCardState startRoundCardState)
		: base(MessageType.ReplicateStartRoundCardState, actor)
	{
		m_PlayerActor = actor;
		m_GameActionID = gameActionID;
		m_StartRoundCardState = startRoundCardState;
	}
}
