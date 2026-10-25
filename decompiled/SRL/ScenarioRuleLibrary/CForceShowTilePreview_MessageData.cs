namespace ScenarioRuleLibrary;

public class CForceShowTilePreview_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CForceShowTilePreview_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ForceShowTilePreview, actorSpawningMessage, string.Empty)
	{
	}
}
