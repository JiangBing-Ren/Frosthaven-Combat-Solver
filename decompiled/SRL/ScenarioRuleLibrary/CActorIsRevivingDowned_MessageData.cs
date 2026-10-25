using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsRevivingDowned_MessageData : CMessageData
{
	public List<CActor> m_ActorsRevived;

	public CAbilityReviveDowned m_ReviveDownedAbility;

	public CActorIsRevivingDowned_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsRevivingDowned, actorSpawningMessage, animOverload)
	{
	}
}
