using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CMonsterClassesToSelectAbilityCards_MessageData : CMessageData
{
	public List<CMonsterClass> m_MonsterClasses;

	public CMonsterClassesToSelectAbilityCards_MessageData(CActor actorSpawningMessage)
		: base(MessageType.MonsterClassesToSelectAbilityCards, actorSpawningMessage)
	{
	}
}
