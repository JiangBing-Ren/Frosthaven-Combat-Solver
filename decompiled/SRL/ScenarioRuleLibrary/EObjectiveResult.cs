using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum EObjectiveResult
{
	None,
	Win,
	Lose
}
