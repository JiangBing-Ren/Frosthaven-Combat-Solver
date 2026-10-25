using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH092 : ScenarioScript
{
	private const string heroicsID = "HEROICS92ID";

	private string resourceID = "LootableCrate";

	private List<CObjectResource> lootCrates;

	private Point[] Tile2 = new Point[4]
	{
		new Point(11, 9),
		new Point(11, 8),
		new Point(11, 7),
		new Point(12, 8)
	};

	private Point[] Tile1 = new Point[4]
	{
		new Point(3, 7),
		new Point(4, 8),
		new Point(3, 9),
		new Point(3, 8)
	};

	private Point[] Tile3 = new Point[3]
	{
		new Point(11, 3),
		new Point(11, 2),
		new Point(12, 2)
	};

	private CMap TopDeck;

	private CMap MidDeck;

	private CMap BottomDeck;

	private CObjective LootXLootCratesObjective;

	private CObjective_ActorsEscaped EscapeObjective;

	private CScenarioModifier SkipMove3ToLootModifier;

	private CScenarioModifier DestroyBottomDeckModifier;

	private CScenarioModifier DestroyMidDeckModifier;

	private CScenarioModifier DestroyTopDeckModifier;

	protected override bool Initialize()
	{
		TopDeck = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "TopDeck");
		MidDeck = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "MidDeck");
		BottomDeck = ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == "BottomDeck");
		LootXLootCratesObjective = GetObjective("LootXLootCrates");
		EscapeObjective = GetObjective("EscapeObjective") as CObjective_ActorsEscaped;
		SkipMove3ToLootModifier = GetScenarioModifier("SkipMove3ToLoot");
		DestroyBottomDeckModifier = GetScenarioModifier("DestroyBottomDeck");
		DestroyMidDeckModifier = GetScenarioModifier("DestroyMidDeck");
		DestroyTopDeckModifier = GetScenarioModifier("DestroyTopDeck");
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		InitLootCrates();
		LinkTeleportTiles();
		SkipMove3ToLootModifier.CustomLocKeyArgs = new List<object> { ScenarioManager.CurrentScenarioState.Level + 2 };
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			InitLootCrates();
			{
				foreach (CObjectResource lootCrate in lootCrates)
				{
					lootCrate.RemainingLoot = 4;
					if (lootCrate.ArrayIndex.X == 13 && lootCrate.ArrayIndex.Y == 4)
					{
						lootCrate.RemainingLoot = -1;
					}
				}
				break;
			}
		case EScenarioModifierTriggerPhase.AfterAbility:
			HandleAfterLootCrate();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			if (_tickActor is CPlayerActor)
			{
				CActiveBonus cActiveBonus = _tickActor.FindApplicableActiveBonuses().FirstOrDefault((CActiveBonus x) => x.Ability.Name == "SkipMove3ToLoot" && x.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.EndTurnAbility));
				if (cActiveBonus == null || !cActiveBonus.RequirementsMet(_tickActor))
				{
					TryEscapeMap();
				}
			}
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		{
			if (_scenarioModifier == DestroyBottomDeckModifier)
			{
				DestroyMap(BottomDeck, _scenarioModifier);
				DestroyMidDeckModifier.SetDeactivated(deactivate: false);
			}
			else if (_scenarioModifier == DestroyMidDeckModifier)
			{
				DestroyMap(MidDeck, _scenarioModifier);
				DestroyTopDeckModifier.SetDeactivated(deactivate: false);
			}
			else if (_scenarioModifier == DestroyTopDeckModifier)
			{
				DestroyMap(TopDeck, _scenarioModifier);
			}
			CActor cActor = ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.Class.ID == "HEROICS92ID");
			if (cActor != null && cActor.CauseOfDeath == CActor.ECauseOfDeath.StillAlive)
			{
				cActor.AllowEscape = true;
				TryEscapeMap(cActor);
			}
			break;
		}
		case EScenarioModifierTriggerPhase.StartTurn:
			break;
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		if (_tickActor?.Class?.ID == "HEROICS92ID")
		{
			_tickActor.AllowEscape = true;
			TryEscapeMap();
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
			ScenarioManager.ToggleTileHighlights(new List<Point> { item.PropTile.m_ArrayIndex }, item.HasRemainingLoot, new CVector4(1f, 1f, 0f, 1f));
		}
		if (LootXLootCratesObjective.IsComplete)
		{
			EscapeObjective.SetActivation(active: true);
		}
	}

	private void InitLootCrates()
	{
		lootCrates = ScenarioManager.CurrentScenarioState.ResourceProps.Select((CObjectProp x) => x as CObjectResource).ToList();
		lootCrates.Sort(GameState.ComparePropsByGUID);
		foreach (CObjectResource lootCrate in lootCrates)
		{
			ScenarioManager.PathFinder.Nodes[lootCrate.ArrayIndex.X, lootCrate.ArrayIndex.Y].Blocked = true;
			ScenarioManager.ToggleTileHighlights(new List<Point> { lootCrate.PropTile.m_ArrayIndex }, lootCrate.HasRemainingLoot, new CVector4(1f, 1f, 0f, 1f));
		}
	}

	public override bool CanActorLootResources(CActor actor)
	{
		if (actor.Class.ID == "HEROICS92ID")
		{
			return true;
		}
		return base.CanActorLootResources(actor);
	}

	private void LinkTeleportTiles()
	{
		List<CTile> list = new List<CTile>();
		if (MidDeck.Revealed && !MidDeck.Destroyed)
		{
			Point[] tile = Tile2;
			for (int i = 0; i < tile.Length; i++)
			{
				CTile tile2 = ScenarioManager.GetTile(tile[i]);
				list.Add(tile2);
				Point[] tile3;
				if (BottomDeck.Revealed && !BottomDeck.Destroyed)
				{
					tile3 = Tile3;
					for (int j = 0; j < tile3.Length; j++)
					{
						CTile tile4 = ScenarioManager.GetTile(tile3[j]);
						list.Add(tile4);
						LinkTeleportTiles(tile2, tile4);
					}
				}
				tile3 = Tile1;
				for (int j = 0; j < tile3.Length; j++)
				{
					CTile tile5 = ScenarioManager.GetTile(tile3[j]);
					list.Add(tile5);
					LinkTeleportTiles(tile2, tile5);
				}
			}
		}
		ToggleTeleportTilesHighlights(list);
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

	public override void ApplyReward(bool won)
	{
		base.ApplyReward(won);
		if (!won)
		{
			return;
		}
		List<CActor> list = ScenarioManager.CurrentScenarioState.Players.Select((PlayerState x) => x.Actor).ToList();
		int num = (int)Math.Floor((float)(5 * list.Count) / 2f);
		CActor item = ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.Class.ID == "HEROICS92ID");
		List<CActor> list2 = new List<CActor>();
		list2.Add(item);
		list2.AddRange(list);
		int num2 = 0;
		foreach (CActor item2 in list2)
		{
			int characterResourceAmount = item2.GetCharacterResourceAmount(resourceID);
			int num3 = Math.Min(characterResourceAmount, num);
			num2 += characterResourceAmount - num3;
			num -= num3;
		}
		int num4 = (int)Math.Ceiling((float)num2 / (float)list.Count);
		foreach (CActor item3 in list)
		{
			if (num4 > 0)
			{
				for (int num5 = 0; num5 < num4; num5++)
				{
					item3.GainLoot(endOfScenario: true, incLootTokenCount: false, num5 == 0);
				}
				item3.ShowLastLootReward("", "MoneyToken", null, focusCamera: true);
			}
			num2 -= num4;
			num4 = Math.Min(num4, num2);
		}
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("heroics_damage", (ScenarioManager.CurrentScenarioState.Level + 2).ToString());
	}
}
