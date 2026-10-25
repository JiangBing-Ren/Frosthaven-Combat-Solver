using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum EPropType
{
	None,
	LootToken,
	Chest,
	GoalChest,
	Trap,
	OneHexObstacle,
	TwoHexObstacle,
	Spawner,
	PressurePlate,
	TerrainHotCoals,
	TerrainWater,
	TerrainRubble,
	TerrainThorns,
	ThreeHexObstacle,
	DarkPitObstacle,
	Portal,
	TerrainVisualEffect,
	MonsterGrave,
	ThreeHexCurvedObstacle,
	ThreeHexStraightObstacle,
	QuestItem,
	CharacterResource,
	TerrainIce,
	WallObstacle,
	Snowdrift,
	Shadow,
	TerrainLampVisualEffect
}
