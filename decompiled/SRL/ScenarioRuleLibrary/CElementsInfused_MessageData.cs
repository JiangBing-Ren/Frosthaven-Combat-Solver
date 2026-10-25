using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CElementsInfused_MessageData : CMessageData
{
	public List<ElementInfusionBoardManager.EElement> m_ChangedElements;

	public CAbilityInfuse m_InfuseAbility;

	public CAbilityConsumeElement m_ConsumeAbility;

	public CAbilityConsume m_Consume;

	public bool m_IsConsume;

	public CElementsInfused_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ElementsInfused, actorSpawningMessage, animOverload)
	{
	}
}
