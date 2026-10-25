using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum EConditionDecTrigger
{
	None,
	Abilities,
	Actions,
	Turns,
	Rounds,
	Never,
	ConditionalCondition,
	WhenDamaged,
	First,
	WhenFrozenFistMechanicActiveBonusEnds,
	_Wound_Unremovable,
	WhenAnimatedClaymoreEnds
}
