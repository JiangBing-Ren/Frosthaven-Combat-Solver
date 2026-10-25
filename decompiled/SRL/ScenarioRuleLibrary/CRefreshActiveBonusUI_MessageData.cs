namespace ScenarioRuleLibrary;

public class CRefreshActiveBonusUI_MessageData : CMessageData
{
	public CActor m_Actor;

	public CRefreshActiveBonusUI_MessageData(CActor actorSpawningMessage)
		: base(MessageType.RefreshActiveBonusUI, actorSpawningMessage)
	{
	}
}
