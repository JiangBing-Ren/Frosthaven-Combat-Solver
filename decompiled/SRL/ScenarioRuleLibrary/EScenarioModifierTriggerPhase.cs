using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(1)]
public enum EScenarioModifierTriggerPhase
{
	None,
	StartRound,
	EndRound,
	StartTurn,
	EndTurn,
	StartScenario,
	AfterAbility,
	OnActorSpawnedOrRevealed,
	OnRoomRevealed,
	AfterSelectMonsterClassAbilityCards,
	AfterAction,
	EndTurnLoot,
	AfterShortRest,
	StartRoundCardSelection,
	BeforeSelectMonsterClassAbilityCards
}
