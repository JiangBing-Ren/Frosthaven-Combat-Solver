namespace ScenarioRuleLibrary;

public class CRestartRound_MessageData : CMessageData
{
	public int Mode;

	public bool FromNetwork;

	public CRestartRound_MessageData(int mode = 0, bool fromNetwork = false)
		: base(MessageType.RestartRound, null)
	{
		Mode = mode;
		FromNetwork = fromNetwork;
	}
}
