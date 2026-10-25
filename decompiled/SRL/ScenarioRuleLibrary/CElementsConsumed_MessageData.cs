using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CElementsConsumed_MessageData : CMessageData
{
	public List<ElementInfusionBoardManager.EElement> m_ChangedElements;

	public CAbilityConsumeElement m_ConsumeAbility;

	public CAbilityConsume m_Consume;

	public CBaseCard m_Card;

	public CElementsConsumed_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ElementsConsumed, actorSpawningMessage, animOverload)
	{
	}
}
