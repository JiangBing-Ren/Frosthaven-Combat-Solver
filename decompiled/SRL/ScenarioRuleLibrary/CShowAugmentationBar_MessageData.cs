namespace ScenarioRuleLibrary;

public class CShowAugmentationBar_MessageData : CMessageData
{
	public CAction m_Action;

	public CAbility m_Ability;

	public CShowAugmentationBar_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ShowAugmentationBar, actorSpawningMessage)
	{
	}
}
