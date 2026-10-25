namespace ScenarioRuleLibrary;

public class CRemoveCasterVFX_MessageData : CMessageData
{
	public string m_VFXName;

	public CRemoveCasterVFX_MessageData(CActor actor)
		: base(MessageType.RemoveCasterVFX, actor, string.Empty)
	{
	}
}
