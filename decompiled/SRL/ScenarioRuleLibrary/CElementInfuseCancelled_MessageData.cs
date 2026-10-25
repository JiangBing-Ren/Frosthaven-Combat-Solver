using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CElementInfuseCancelled_MessageData : CMessageData
{
	public List<ElementInfusionBoardManager.EElement> m_InfusedElements;

	public CAbilityInfuse m_InfuseAbility;

	public CAbilityConsumeElement m_ConsumeAbility;

	public CAbilityConsume m_Consume;

	public bool m_IsInfuse;

	public CElementInfuseCancelled_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ElementInfuseCancelled, actorSpawningMessage, animOverload)
	{
	}
}
