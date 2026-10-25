using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESESubTypePhase
{
	None,
	PhaseStart,
	PhaseNextStep,
	PhaseStepComplete,
	PhaseEnd,
	PhaseTileSelected,
	PhaseTileDeselected,
	PhaseApplySingleTarget
}
