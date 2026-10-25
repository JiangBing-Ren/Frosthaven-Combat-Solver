using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum EScenarioModifierType
{
	None,
	SetElements,
	TriggerAbility,
	AddConditionsToAbilities,
	ApplyConditionToActor,
	ApplyActiveBonusToActor,
	AddModifierCards,
	PhaseOut,
	PhaseInAndTeleport,
	Teleport,
	ActivateClosestAI,
	ToggleActorDeactivated,
	MoveActorsInDirection,
	ForceSpawnerToSpawnIfActorsNotInRooms,
	MovePropsInSequence,
	MovePropsToNearestPlayer,
	DestroyRoom,
	ActorsCreateGraves,
	OverrideCompanionSummonTiles,
	MakeTrapsObstacles,
	ActorsDontDropCoins,
	DisableRest
}
