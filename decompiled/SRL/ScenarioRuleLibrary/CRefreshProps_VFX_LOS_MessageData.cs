namespace ScenarioRuleLibrary;

public class CRefreshProps_VFX_LOS_MessageData : CMessageData
{
	public CObjectProp[] m_Props;

	public CRefreshProps_VFX_LOS_MessageData()
		: base(MessageType.RefreshProps_VFX_LOS, null, string.Empty)
	{
	}
}
