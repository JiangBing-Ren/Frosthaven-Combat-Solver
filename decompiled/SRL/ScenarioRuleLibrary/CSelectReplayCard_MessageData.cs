using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CSelectReplayCard_MessageData : CMessageData
{
	public CActor m_ActorReplayingCard;

	public CAbility m_Ability;

	public CAbility.EAbilityType m_ReplayAbilityType;

	public List<int> m_ValidCards;

	public int m_DamageReduction;

	public bool m_useLostCards;

	public bool m_UseParentTargeting;

	public CAbility.ECardAction m_CardAction;

	public CSelectReplayCard_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PlayDiscardedCard, actorSpawningMessage, animOverload)
	{
	}
}
