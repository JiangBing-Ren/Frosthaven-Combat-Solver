namespace ScenarioRuleLibrary;

public class CPlayerSelectedTile_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CPlayerSelectedTile_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectedTile, actorSpawningMessage)
	{
	}
}
