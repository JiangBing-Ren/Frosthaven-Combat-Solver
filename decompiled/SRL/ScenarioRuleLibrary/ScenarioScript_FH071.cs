using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH071 : ScenarioScript
{
	public const string ArtificerClass = "Aritficer71ID";

	private CActor _artificer;

	private Point[] _artificerTargetPos = new Point[2]
	{
		new Point(23, 11),
		new Point(24, 10)
	};

	private Point[] _jumpingPos = new Point[6]
	{
		new Point(10, 3),
		new Point(11, 3),
		new Point(12, 4),
		new Point(11, 5),
		new Point(10, 5),
		new Point(10, 4)
	};

	private Point[] _jumpDstPos = new Point[3]
	{
		new Point(18, 6),
		new Point(18, 5),
		new Point(18, 4)
	};

	private List<TileIndex> _artificerTargetTiles;

	private CObjectDoor DoorToShip;

	private CObjectDoor DoorBetweenShipAndRoom3;

	private CObjectDoor FinalDoorA;

	private CObjectDoor FinalDoorB;

	private CMap Room_2;

	private CMap Room_3;

	private CMap Room_4;

	private bool IsArtificerReached => !GetObjective("ReachArtificer").IsActive;

	protected override bool Initialize()
	{
		_artificerTargetTiles = _artificerTargetPos.Select((Point p) => new TileIndex(p)).ToList();
		DoorToShip = ScenarioScript.FindDoor(8, 7);
		DoorBetweenShipAndRoom3 = ScenarioScript.FindDoor(13, 7);
		Room_2 = ScenarioScript.FindRoom("Room_2");
		Room_3 = ScenarioScript.FindRoom("Room_3");
		Room_4 = ScenarioScript.FindRoom("Room_4");
		FinalDoorA = ScenarioScript.FindDoor(20, 9);
		FinalDoorB = ScenarioScript.FindDoor(21, 9);
		FinalDoorA.TwinDoors = new CObjectDoor[1] { FinalDoorB };
		FinalDoorB.TwinDoors = new CObjectDoor[1] { FinalDoorA };
		UpdateArtificerRef();
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateNavLinks(Room_2.Revealed, Room_2.Revealed);
		CObjective objective = GetObjective("ProtectArtificer");
		ScenarioManager.ToggleTileHighlights(_artificerTargetPos, objective?.IsActive ?? false);
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		UpdateNavLinks(Room_2.Revealed && _tickAbility.TargetingActor is CPlayerActor, Room_2.Revealed);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		UpdateNavLinks(Room_2.Revealed, Room_2.Revealed);
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		UpdateNavLinks(Room_2.Revealed, Room_2.Revealed);
	}

	private void UpdateArtificerRef()
	{
		if (_artificer == null)
		{
			_artificer = ScenarioManager.CurrentScenarioState.AllyMonsters.FirstOrDefault((EnemyState x) => x.ClassID == "Aritficer71ID").Actor;
		}
	}

	protected override void TickPhaseInternal()
	{
		UpdateArtificerRef();
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (_artificer == null)
		{
			return;
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndTurn && !IsArtificerReached)
		{
			foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
			{
				if (ScenarioManager.IsTileAdjacent(player.Actor.Tile, _artificer.Tile))
				{
					OnArtificerReached();
					break;
				}
			}
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound && IsArtificerReached && _artificerTargetPos.Contains(_artificer.Tile.m_ArrayIndex))
		{
			OnArtificerReachedTarget();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartTurn && IsArtificerReached && GameState.CurrentActor == _artificer)
		{
			UpdateArtificerTarget();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound)
		{
			OverrideArtificerRoundCard(_tickActor);
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == Room_2)
		{
			GetScenarioModifier("RoomJump").Show();
			UpdateNavLinks(enabled: true, enabledVisuals: true);
		}
		else if (_tickRoomRevealed == Room_4)
		{
			UpdateArtificerRef();
			OverrideArtificerRoundCard(_artificer);
			GetScenarioModifier("PreventDamageArtificer").Show();
		}
	}

	private void OnArtificerReachedTarget()
	{
		GetObjective<CObjective_Scripted>("ProtectArtificer").ScriptedIsComplete = true;
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void OnArtificerReached()
	{
		GetObjective("ReachArtificer").SetActivation(active: false);
		GetObjective("ProtectArtificer").SetActivation(active: true);
		ScenarioManager.ToggleTileHighlights(_artificerTargetPos, enabled: true);
		UpdateArtificerTarget();
	}

	private void UpdateArtificerTarget()
	{
		TileIndex tileIndex = _artificerTargetTiles[0];
		int num = ScenarioManager.GetTileDistance(tileIndex.X, tileIndex.Y, _artificer.Tile.m_ArrayIndex.X, _artificer.Tile.m_ArrayIndex.Y);
		if (!IsFreeTile(tileIndex.X, tileIndex.Y))
		{
			num = int.MaxValue;
		}
		if (num > ScenarioManager.GetTileDistance(_artificerTargetTiles[1].X, _artificerTargetTiles[1].Y, _artificer.Tile.m_ArrayIndex.X, _artificer.Tile.m_ArrayIndex.Y))
		{
			tileIndex = _artificerTargetTiles[1];
		}
		if (!IsFreeTile(tileIndex.X, tileIndex.Y))
		{
			CTile closestEmptyTile = ScenarioScript.GetClosestEmptyTile(tileIndex.ToPoint(), _artificer);
			if (closestEmptyTile != null)
			{
				tileIndex = new TileIndex(closestEmptyTile.m_ArrayIndex);
			}
		}
		CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(_artificer, ScenarioManager.GetTile(tileIndex.X, tileIndex.Y));
	}

	private void ToggleJumpTilesHighlights(bool enabled)
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		Point[] jumpingPos = _jumpingPos;
		foreach (Point jumpPos in jumpingPos)
		{
			list.AddRange(_jumpDstPos.Select((Point dstPos) => (jumpPos: jumpPos, dstPos: dstPos)));
		}
		ScenarioManager.ToggleTileHighlights(_jumpingPos.Concat(_jumpDstPos), enabled);
		ScenarioManager.ToggleTileLinks(list, enabled);
	}

	private void UpdateNavLinks(bool enabled, bool enabledVisuals)
	{
		Point[] jumpingPos = _jumpingPos;
		foreach (Point arrayIndex in jumpingPos)
		{
			Point[] jumpDstPos = _jumpDstPos;
			foreach (Point arrayIndex2 in jumpDstPos)
			{
				CNode node = ScenarioManager.PathFinder.GetNode(arrayIndex);
				CNode toNode = ScenarioManager.PathFinder.GetNode(arrayIndex2);
				CNavLink cNavLink = node.NavLinks.SingleOrDefault((CNavLink x) => x.DestinationNode == toNode);
				if (enabled)
				{
					if (cNavLink == null)
					{
						CNavLink item = new CNavLink(2, toNode);
						node.NavLinks.Add(item);
					}
				}
				else if (cNavLink != null)
				{
					node.NavLinks.Remove(cNavLink);
				}
			}
		}
		ToggleJumpTilesHighlights(enabled | enabledVisuals);
	}

	public static bool CanTeleportToNextRoom(CActor actor)
	{
		ScenarioScript_FH071 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH071;
		if (!scenarioScript_FH._jumpingPos.Contains(actor.ArrayIndex))
		{
			return false;
		}
		return !scenarioScript_FH.Room_4.Revealed;
	}

	public static void TeleportToNextRoom(CActor actor)
	{
		ScenarioScript_FH071 obj = ScenarioScript.current as ScenarioScript_FH071;
		CTile tile = ScenarioManager.GetTile(obj._jumpDstPos[0]);
		obj.DoorBetweenShipAndRoom3.SetExtraLockState(lockedStateToSet: false);
		obj.RevealRoom(obj.Room_4);
		obj.RevealRoom(obj.Room_3);
		GameState.TeleportActor(actor, tile);
		GameState.SortIntoInitiativeAndIDOrder();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			playerActor.FindApplicableActiveBonuses().SingleOrDefault((CActiveBonus x) => x.Ability.Name == "SkipMove2ToTeleportToNextRoom")?.Finish();
		}
	}

	private void OverrideArtificerRoundCard(CActor actor)
	{
		if (actor == null || actor != _artificer || actor.IsDead)
		{
			return;
		}
		List<CMonsterAbilityCard> abilityCardsPool = _artificer.MonsterClass.CardsPoolClass.AbilityCardsPool;
		if (!IsArtificerReached)
		{
			_artificer.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 11534);
		}
		else
		{
			_artificer.MonsterClass.CardsPoolClass.RoundAbilityCard = abilityCardsPool.Find((CMonsterAbilityCard c) => c.ID == 13006);
		}
	}

	public override bool TreatTileAsFeatureless(CTile tile)
	{
		if (tile == ScenarioManager.GetTile(20, 10) || tile == ScenarioManager.GetTile(20, 11))
		{
			return PhaseManager.CurrentPhase is CPhaseStartRoundEffects;
		}
		return base.TreatTileAsFeatureless(tile);
	}
}
