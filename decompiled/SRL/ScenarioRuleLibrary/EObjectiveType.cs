using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum EObjectiveType
{
	None,
	KillAllEnemies,
	KillAllBosses,
	ReachRound,
	ActorReachPosition,
	XCharactersDie,
	LootX,
	CustomTrigger,
	DestroyXObjects,
	ActivateXPressurePlates,
	RevealAllRooms,
	AllCharactersMustLoot,
	AnyActorReachPosition,
	DeactivateXSpawners,
	ActorsEscaped,
	DealXDamage,
	ActivateXSpawners,
	ActorsNotInAllRooms,
	XActorsHealToMax,
	DamageQuota,
	ActivateXProps,
	Scripted,
	BringItemsToArea
}
