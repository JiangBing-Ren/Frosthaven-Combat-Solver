using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsApplyingControlActor : CMessageData
{
	public List<CActor> m_ActorsAppliedTo;

	public CAbilityControlActor m_ControlActorAbility;

	public CActorIsApplyingControlActor(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsApplyingControlActor, actorSpawningMessage, animOverload)
	{
	}
}
