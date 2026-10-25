namespace ScenarioRuleLibrary;

public class CAddCasterVFX_MessageData : CMessageData
{
	public string m_VFXName;

	public float m_delay;

	public CAddCasterVFX_MessageData(CActor actor)
		: base(MessageType.AddCasterVFX, actor, string.Empty)
	{
	}
}
