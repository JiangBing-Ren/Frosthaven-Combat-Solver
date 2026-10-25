using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityTargetingCombatLog_MessageData : CMessageData
{
	public CAbility m_Ability;

	public List<CActor> m_ActorsAppliedTo;

	public CAbilityTargetingCombatLog_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.AbilityTargetingCombatLog, actorSpawningMessage, animOverload)
	{
	}
}
