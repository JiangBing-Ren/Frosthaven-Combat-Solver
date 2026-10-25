namespace ScenarioRuleLibrary;

public class CPlayerFinishedLongRest_MessageData : CMessageData
{
	public CAbilityCard m_LostCard;

	public bool m_ImprovedShortRestUsed;

	public bool m_FromStateUpdate;

	public CPlayerFinishedLongRest_MessageData(CActor actor)
		: base(MessageType.PlayerFinishedLongRest, actor)
	{
	}
}
