using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsCapturing_MessageData : CMessageData
{
	public List<CActor> m_ActorsAppliedTo;

	public CAbilityCapture m_KillAbility;

	public CActorIsCapturing_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsCapturing, actorSpawningMessage, animOverload)
	{
	}
}
