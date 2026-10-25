using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsKilling_MessageData : CMessageData
{
	public List<CActor> m_ActorsAppliedTo;

	public CAbilityKill m_KillAbility;

	public CActorIsKilling_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsKilling, actorSpawningMessage, animOverload)
	{
	}
}
