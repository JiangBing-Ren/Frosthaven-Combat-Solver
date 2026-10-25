using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CObjectClass : CMonsterClass
{
	public CObjectClass(MonsterYMLData objectYML, List<CMonsterAbilityCard> abilityCards)
		: base(objectYML, abilityCards)
	{
	}
}
