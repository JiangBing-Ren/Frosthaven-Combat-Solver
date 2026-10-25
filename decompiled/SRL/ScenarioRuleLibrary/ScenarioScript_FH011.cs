using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH011 : ScenarioScript
{
	private ScenarioManager.ObjectImportType _terrainType = ScenarioManager.ObjectImportType.TerrainThorns;

	private CObjective _winObjective;

	private bool _showedBossNarration;

	private IEnumerable<CActor> _allActors => ((IEnumerable<CPlayerActor>)ScenarioManager.Scenario.PlayerActors).Select((Func<CPlayerActor, CActor>)((CPlayerActor x) => x)).Concat(ScenarioManager.Scenario.AllyMonsters).Concat(ScenarioManager.Scenario.HeroSummons)
		.Concat(ScenarioManager.Scenario.NeutralMonsters)
		.Concat(ScenarioManager.Scenario.Enemy2Monsters);

	protected override bool Initialize()
	{
		_winObjective = ScenarioManager.CurrentScenarioState.WinObjectives[0];
		return true;
	}

	protected override void TickPhaseInternal()
	{
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAbility)
		{
			Trace("TickPhase " + _tickActor.Class.ID, null, 34);
			if (_tickActor.Class.ID == "AlgoxStormcallerID" && (_tickActor.LastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special2) != 0)
			{
				TickAlgoxStormcaller(_tickActor);
			}
		}
	}

	private void TickAlgoxStormcaller(CActor algoxStormcaller)
	{
		Trace("Algox Stormcaller Special 2: Spawn hazardous terrain", null, 48);
		SpawnHazardousTerrain(algoxStormcaller);
		Trace("Algox Stormcaller Special 2: Damage hazardous terrain adjacent enemies", null, 51);
		DamageEnemies();
	}

	private void SpawnHazardousTerrain(CActor algoxStormcaller)
	{
		if (GetClosestMonster(algoxStormcaller, out var nearestMonster) && GetNearestTileToSpawn(nearestMonster, out var nearestTile, excludeAllNonLootProps: true))
		{
			CObjectHazardousTerrain prop = new CObjectHazardousTerrain(_terrainType.ToString(), _terrainType, new TileIndex(nearestTile.m_ArrayIndex), null, null, algoxStormcaller, nearestTile.m_HexMap.MapGuid);
			nearestTile.SpawnProp(prop);
			Trace("Spawned prop " + _terrainType, null, 84);
		}
	}

	private List<CActor> GetActorsToDamage()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => prop is CObjectHazardousTerrain).ToList();
		List<CActor> list2 = new List<CActor>();
		List<CActor> source = _allActors.ToList();
		foreach (CObjectProp terrain in list)
		{
			list2.AddRange(source.Where((CActor actor) => ScenarioManager.GetTileDistance(terrain.PropTile.m_ArrayIndex.X, terrain.PropTile.m_ArrayIndex.Y, actor.ArrayIndex.X, actor.ArrayIndex.Y) <= 1).ToList());
		}
		return list2;
	}

	private void DamageEnemies()
	{
		foreach (CActor item in GetActorsToDamage().Distinct())
		{
			GameState.ActorBeenDamaged(item, ScenarioManager.HazardousTerrainDamage, checkIfPlayerCanAvoidDamage: true);
			GameState.ActorHealthCheck(item, item);
		}
	}

	private bool GetClosestMonster(CActor inActor, out CActor nearestMonster)
	{
		CTile cTile = ScenarioManager.Tiles[inActor.ArrayIndex.X, inActor.ArrayIndex.Y];
		nearestMonster = null;
		int num = 9999999;
		foreach (CActor allActor in _allActors)
		{
			CTile cTile2 = ScenarioManager.Tiles[allActor.ArrayIndex.X, allActor.ArrayIndex.Y];
			int tileDistance = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y);
			if (tileDistance < num)
			{
				num = tileDistance;
				nearestMonster = allActor;
			}
		}
		return nearestMonster != null;
	}

	private bool GetNearestTileToSpawn(CActor movedActor, out CTile nearestTile, bool excludeAllNonLootProps)
	{
		Point arrayIndex = movedActor.ArrayIndex;
		CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
		nearestTile = null;
		int num = 9999999;
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile t in tiles)
		{
			if (t != null)
			{
				int tileDistance = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, t.m_ArrayIndex.X, t.m_ArrayIndex.Y);
				if (CSpawner.IsEmptyTile(t) && ScenarioManager.PathFinder.Nodes[t.m_ArrayIndex.X, t.m_ArrayIndex.Y].Walkable && !ScenarioManager.PathFinder.Nodes[t.m_ArrayIndex.X, t.m_ArrayIndex.Y].Blocked && (!excludeAllNonLootProps || ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.PropTile == t && p.PropType != EPropType.LootToken) == null) && tileDistance < num)
				{
					num = tileDistance;
					nearestTile = t;
				}
			}
		}
		if (nearestTile == null)
		{
			return false;
		}
		return true;
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (!_showedBossNarration && _winObjective.IsComplete)
		{
			_showedBossNarration = true;
			return true;
		}
		return false;
	}

	public override List<CTile> NullAbilityTargetedTiles(CAbilityNull ability)
	{
		if ((ability.Flags & CAbility.EAbilityFlag.Special2) == 0)
		{
			return null;
		}
		if (!GetClosestMonster(ability.TargetingActor, out var nearestMonster))
		{
			return null;
		}
		if (!GetNearestTileToSpawn(nearestMonster, out var nearestTile, excludeAllNonLootProps: true))
		{
			return null;
		}
		return new List<CTile> { nearestTile };
	}

	public override List<CTile> NullAbilityAreaTiles(CAbilityNull ability)
	{
		if ((ability.Flags & CAbility.EAbilityFlag.Special2) == 0)
		{
			return null;
		}
		if (!GetClosestMonster(ability.TargetingActor, out var nearestMonster))
		{
			return null;
		}
		if (!GetNearestTileToSpawn(nearestMonster, out var nearestTile, excludeAllNonLootProps: true))
		{
			return null;
		}
		List<CTile> list = (from p in ScenarioManager.CurrentScenarioState.Props
			where p is CObjectHazardousTerrain
			select p.PropTile).ToList();
		list.Add(nearestTile);
		List<CTile> list2 = new List<CTile>();
		List<CActor> source = _allActors.ToList();
		foreach (CTile terrain in list)
		{
			list2.AddRange((from a in source
				where ScenarioManager.GetTileDistance(terrain.m_ArrayIndex.X, terrain.m_ArrayIndex.Y, a.ArrayIndex.X, a.ArrayIndex.Y) <= 1
				select a.Tile).ToList());
		}
		return list2;
	}
}
