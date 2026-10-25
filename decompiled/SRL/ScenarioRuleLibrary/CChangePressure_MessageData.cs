namespace ScenarioRuleLibrary;

public class CChangePressure_MessageData : CMessageData
{
	public int m_PressureChange;

	public CChangePressure_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PressureChange, actorSpawningMessage, animOverload)
	{
	}
}
