using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum EEnhancementLine
{
	None,
	Mainline,
	Range,
	Targets,
	Pierce,
	AreaHex,
	SummonAttack,
	SummonMove,
	SummonRange,
	SummonHealth,
	RetaliateRange,
	Push,
	Pull
}
