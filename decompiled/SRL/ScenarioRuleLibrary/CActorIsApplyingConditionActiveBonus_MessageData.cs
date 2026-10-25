using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsApplyingConditionActiveBonus_MessageData : CMessageData
{
	public List<CActor> m_ActorsAppliedTo;

	public CAbility m_Ability;

	public CActorIsApplyingConditionActiveBonus_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsApplyingConditionActiveBonus, actorSpawningMessage, animOverload)
	{
	}
}
