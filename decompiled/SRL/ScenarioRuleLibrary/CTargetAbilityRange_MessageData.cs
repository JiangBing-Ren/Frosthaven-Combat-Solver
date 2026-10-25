using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CTargetAbilityRange_MessageData : CMessageData
{
	public CActor TargetingActor;

	public List<CTile> m_TargetAbilityRange;

	public CTargetAbilityRange_MessageData(CActor actorSpawningMessage)
		: base(MessageType.TargetAbilityRange, actorSpawningMessage)
	{
	}
}
