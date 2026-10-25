using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[Flags]
[KronosEnumPolice(0)]
public enum EFlags
{
	Blocked = 1,
	Edge = 2
}
