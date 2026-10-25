using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESESubTypeInternal
{
	None,
	Start,
	Pause,
	Resume,
	Lose,
	Win
}
