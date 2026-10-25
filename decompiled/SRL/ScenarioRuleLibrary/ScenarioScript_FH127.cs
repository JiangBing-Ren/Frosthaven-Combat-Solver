using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH127 : ScenarioScript
{
	[StateDump(Skip = true)]
	public Action OnStairStateChanged;

	private CMap room_7;

	public CMap room_8;

	private CTile tile_A;

	private CTile tile_B;

	private CTile tile_C;

	private CTile tile_D;

	private CObjective_LootX lootObjective;

	private List<CObjectResource> lootCrates;

	private CScenarioModifier SpecialRule3;

	protected override bool Initialize()
	{
		room_7 = ScenarioScript.FindRoom("Room_7");
		room_8 = ScenarioScript.FindRoom("Room_8");
		tile_A = ScenarioManager.GetTile(5, 10);
		tile_B = ScenarioManager.GetTile(13, 10);
		tile_C = ScenarioManager.GetTile(13, 12);
		tile_D = ScenarioManager.GetTile(11, 3);
		lootObjective = GetObjective<CObjective_LootX>("LootX");
		SpecialRule3 = GetScenarioModifier("SpecialRule3");
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateStairs();
		UpdateLootCrates();
	}

	private void UpdateLootCrates()
	{
		lootCrates = ScenarioManager.CurrentScenarioState.ResourceProps.Select((CObjectProp x) => x as CObjectResource).ToList();
		foreach (CObjectResource lootCrate in lootCrates)
		{
			if (lootCrate.PropTile.m_HexMap.Revealed)
			{
				ScenarioManager.PathFinder.Nodes[lootCrate.ArrayIndex.X, lootCrate.ArrayIndex.Y].Blocked = true;
				ScenarioManager.ToggleTileHighlights(new List<Point> { lootCrate.PropTile.m_ArrayIndex }, lootCrate.HasRemainingLoot, new CVector4(1f, 1f, 0f, 1f));
			}
		}
	}

	private void HandleAfterLootCrate()
	{
		CAbilityLoot ability = _tickActor.LastAbilityPerformed as CAbilityLoot;
		if (ability == null || ability.TilesSelected.Count == 0)
		{
			return;
		}
		foreach (CObjectResource item in lootCrates.Where((CObjectResource x) => ability.TilesSelected.Contains(x.PropTile)).ToList())
		{
			ScenarioManager.PathFinder.Nodes[item.ArrayIndex.X, item.ArrayIndex.Y].Blocked = false;
			ScenarioManager.ToggleTileHighlights(new List<Point> { item.PropTile.m_ArrayIndex }, enabled: false, new CVector4(1f, 1f, 0f, 1f));
			lootCrates.Remove(item);
		}
	}

	private void UpdateStairs()
	{
		CNode node = ScenarioManager.PathFinder.GetNode(5, 10);
		CNode node2 = ScenarioManager.PathFinder.GetNode(13, 12);
		node.IsDifficultTerrain = true;
		node2.IsDifficultTerrain = true;
		if (OnStairStateChanged != null)
		{
			OnStairStateChanged();
		}
		if (room_7.Revealed)
		{
			LinkTeleportTiles(tile_A, tile_B);
		}
		if (room_8.Revealed)
		{
			LinkTeleportTiles(tile_C, tile_D);
		}
		ScenarioManager.GetTile(5, 10).TreatAsDifficult = true;
		ScenarioManager.GetTile(13, 12).TreatAsDifficult = true;
	}

	private void LinkTeleportTiles(CTile tileA, CTile tileB)
	{
		CNode node = ScenarioManager.PathFinder.GetNode(tileA.m_ArrayIndex);
		CNode node2 = ScenarioManager.PathFinder.GetNode(tileB.m_ArrayIndex);
		CNavLink item = new CNavLink(1, node2);
		if (!node.NavLinks.Contains(item))
		{
			node.NavLinks.Add(item);
		}
		item = new CNavLink(1, node);
		if (!node2.NavLinks.Contains(item))
		{
			node2.NavLinks.Add(item);
		}
		ToggleTeleportTilesHighlights();
	}

	private void ToggleTeleportTilesHighlights()
	{
		ToggleTeleportTilesHighlights(new List<CTile> { tile_A, tile_B, tile_C, tile_D });
	}

	private void ToggleTeleportTilesHighlights(List<CTile> tiles)
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		List<Point> list2 = new List<Point>();
		foreach (CTile tile in tiles)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
			if (node.NavLinks == null || node.NavLinks.Count <= 0)
			{
				continue;
			}
			list2.Add(tile.m_ArrayIndex);
			foreach (CNavLink navLink in node.NavLinks)
			{
				list2.Add(navLink.DestinationNode.Location);
				list.Add((tile.m_ArrayIndex, navLink.DestinationNode.Location));
			}
		}
		ScenarioManager.ToggleTileHighlights(list2, enabled: true);
		ScenarioManager.ToggleTileLinks(list, enabled: true);
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UpdateLootCrates();
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			ToggleTeleportTilesHighlights();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
		{
			CActor tickActor = _tickActor;
			TryEscapeMap(tickActor);
			if (ScenarioManager.Scenario.PlayerActors.Count == 0 && !lootObjective.IsComplete)
			{
				lootObjective.UnableToComplete = true;
			}
			break;
		}
		case EScenarioModifierTriggerPhase.AfterAbility:
			HandleAfterLootCrate();
			break;
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		UpdateStairs();
		UpdateLootCrates();
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		return new List<object> { lootObjective.CurrentLootedAmount };
	}

	public static bool CanTeleportToNextDeck(CActor actor)
	{
		CTile tile;
		if (actor.ArrayIndex.X == 5 && actor.ArrayIndex.Y == 10)
		{
			tile = ScenarioManager.GetTile(13, 10);
		}
		else
		{
			if (actor.ArrayIndex.X != 13 || actor.ArrayIndex.Y != 12)
			{
				return false;
			}
			tile = ScenarioManager.GetTile(11, 3);
		}
		return !tile.m_HexMap.Revealed;
	}

	public static void TeleportToBottomDeck(CActor actor)
	{
		CTile destTile = ((actor.ArrayIndex.X != 5 || actor.ArrayIndex.Y != 10) ? ScenarioManager.GetTile(11, 3) : ScenarioManager.GetTile(13, 10));
		GameState.TeleportActor(actor, destTile, tryReveal: true);
		ScenarioScript_FH127 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH127;
		if (!scenarioScript_FH.room_8.Revealed)
		{
			return;
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			playerActor.FindApplicableActiveBonuses().SingleOrDefault((CActiveBonus x) => x.Ability.Name == "SkipMove2ToRevealLowerDeck")?.Finish();
		}
		scenarioScript_FH.SpecialRule3.SetDeactivated(deactivate: true);
	}

	public override bool CanActorLootResources(CActor actor)
	{
		if (actor is CHeroSummonActor)
		{
			return true;
		}
		return base.CanActorLootResources(actor);
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		if (tile == tile_A || tile == tile_C)
		{
			return true;
		}
		return base.TreatTileAsNonFeatureless(tile);
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("looted_crates_count", (lootCrates?.Count ?? 0).ToString());
	}
}
