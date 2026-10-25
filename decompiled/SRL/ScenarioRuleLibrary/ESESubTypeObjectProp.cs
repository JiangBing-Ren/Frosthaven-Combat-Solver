using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESESubTypeObjectProp
{
	None,
	Activated,
	Destroyed,
	Died,
	Spawned
}
