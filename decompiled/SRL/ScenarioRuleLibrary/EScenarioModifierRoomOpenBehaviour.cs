using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
[Flags]
public enum EScenarioModifierRoomOpenBehaviour
{
	None = 0,
	TriggerOnOpen = 1,
	ActivateOnOpen = 2
}
