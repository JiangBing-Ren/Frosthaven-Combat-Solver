namespace ScenarioRuleLibrary;

public class CMovingTerrain_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CTile m_From;

	public CTile m_To;

	public CMovingTerrain_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.MovingTerrain, actorSpawningMessage, animOverload)
	{
	}
}
