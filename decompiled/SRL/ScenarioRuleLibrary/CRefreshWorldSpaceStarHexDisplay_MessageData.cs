namespace ScenarioRuleLibrary;

public class CRefreshWorldSpaceStarHexDisplay_MessageData : CMessageData
{
	public bool m_IsRestart;

	public CRefreshWorldSpaceStarHexDisplay_MessageData(bool isRestart = false)
		: base(MessageType.RefreshWorldSpaceStarHexDisplay, null)
	{
		m_IsRestart = isRestart;
	}
}
