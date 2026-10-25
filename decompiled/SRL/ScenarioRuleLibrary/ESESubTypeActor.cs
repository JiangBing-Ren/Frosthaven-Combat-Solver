using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESESubTypeActor
{
	None,
	ActorInitialized,
	ActorOnDeath,
	ActorDamaged,
	ActorCalculateShield,
	ActorApplyRedirector,
	ActorApplyRetaliate,
	ActorHealed,
	ActorGainXP,
	ActorApplyImmediateDamage,
	ActorApplyNegativeCondition,
	ActorApplyPositiveCondition,
	ActorLooted,
	ActorUsedItem,
	ActorFinishedScenario,
	ActorEndTurn,
	ActorStartTurn,
	ActorChangeForm,
	ActorHeroSummonRangeChange,
	ActorActiveBonusTracker,
	ActorAgility,
	ActorArrayIndexChanged
}
