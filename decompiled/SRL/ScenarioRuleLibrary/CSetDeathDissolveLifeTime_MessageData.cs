namespace ScenarioRuleLibrary;

public class CSetDeathDissolveLifeTime_MessageData : CMessageData
{
	public float m_DissolveLifeTime;

	public CSetDeathDissolveLifeTime_MessageData(CActor actorSpawningMessage)
		: base(MessageType.SetDeathDissolveLifeTime, actorSpawningMessage)
	{
	}
}
