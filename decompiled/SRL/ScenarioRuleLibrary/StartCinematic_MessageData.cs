namespace ScenarioRuleLibrary;

public class StartCinematic_MessageData : CMessageData
{
	public string m_PrefabName;

	public CPhase.PhaseType m_NextPhase;

	public StartCinematic_MessageData(string prefabName, CPhase.PhaseType nextPhase)
		: base(MessageType.StartCinematic, null)
	{
		m_PrefabName = prefabName;
		m_NextPhase = nextPhase;
	}
}
