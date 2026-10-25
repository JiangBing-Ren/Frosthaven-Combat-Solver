using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH079 : ScenarioScript
{
	private List<TileIndex> _exitTiles = new List<TileIndex>
	{
		new TileIndex(new Point(20, 11)),
		new TileIndex(new Point(20, 10)),
		new TileIndex(new Point(19, 9))
	};

	private TileIndex _pressurePlateTile = new TileIndex(new Point(5, 10));

	private CObjectDoor _door1;

	private List<CObjectDoor> _doorBreakales = new List<CObjectDoor>();

	private CObjectObstacle _sled;

	private CObjective_Scripted _winObjective;

	private CObjective_Scripted _iceWallObjective;

	private CScenarioModifier _scenarioAbilityHolder_ForgoTopAction;

	private CScenarioModifier _scenarioAbilityHolder_ForgoBottomAction;

	public static ScenarioScript_FH079 _script;

	private Point _startSledMovementFrom;

	private bool _showRoomRevealMessage;

	private CustomAbility.MoveActorInDirectionResult moveResult;

	private CObjectActor SledActor => _sled?.RuntimeAttachedActor;

	protected override bool Initialize()
	{
		_script = this;
		_door1 = GetProp(9, 10, "ThinDoor") as CObjectDoor;
		_doorBreakales.Add(GetProp(17, 12, "ThickDoor") as CObjectDoor);
		_doorBreakales.Add(GetProp(17, 13, "ThickDoor") as CObjectDoor);
		_sled = ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.PrefabName == "Sled") as CObjectObstacle;
		_winObjective = GetObjective<CObjective_Scripted>("SledToPosition");
		_iceWallObjective = GetObjective<CObjective_Scripted>("BreakBlocks");
		_scenarioAbilityHolder_ForgoTopAction = GetScenarioModifier("ScenarioAbilityHolder_ForgoTopAction");
		_scenarioAbilityHolder_ForgoBottomAction = GetScenarioModifier("ScenarioAbilityHolder_ForgoBottomAction");
		ScenarioManager.ToggleTileHighlights(_exitTiles.Select((TileIndex t) => t.ToPoint()), enabled: true);
		if (SledActor != null)
		{
			SledActor.IsImmuneToDamage = true;
			SledActor.ForcedUntargetable = true;
			SledActor.TreatAsObstacle = true;
			CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(SledActor, active: false);
			ScenarioRuleClient.MessageHandler(message);
		}
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		_showRoomRevealMessage = _door1.DoorIsOpen;
		CActiveBonus.RefreshAllAuraActiveBonuses();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound)
		{
			TickStartRound();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAbility)
		{
			HandleAfterMoveSled();
		}
	}

	private void TickStartRound()
	{
		if (_tickActor != ScenarioManager.Scenario.PlayerActors[0] || ScenarioManager.CurrentScenarioState.RoundNumber != 1)
		{
			return;
		}
		CAbility ability = ScenarioRuleClient.SRLYML.ScenarioAbilities.First((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _scenarioAbilityHolder_ForgoTopAction.ScenarioAbilityID).ScenarioAbilities[0];
		CAbility ability2 = ScenarioRuleClient.SRLYML.ScenarioAbilities.First((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _scenarioAbilityHolder_ForgoBottomAction.ScenarioAbilityID).ScenarioAbilities[0];
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(playerActor, playerActor);
			cAbility.ApplyToActor(playerActor);
			CAbility cAbility2 = CAbility.CopyAbility(ability2, generateNewID: false);
			cAbility2.IsScenarioModifierAbility = true;
			cAbility2.Start(playerActor, playerActor);
			cAbility2.ApplyToActor(playerActor);
		}
	}

	public void MoveSled(Point dst)
	{
		ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(SledActor.ArrayIndex.X, SledActor.ArrayIndex.Y, dst.X, dst.Y);
		moveResult = new CustomAbility.MoveActorInDirectionResult();
		SledActor.SetTempVar("LastTileBeforeMove", new TValue(SledActor.Tile));
		CustomAbility.MoveActorInDirection(SledActor, adjacentPosition, new List<CActor> { SledActor }, 100, moveResult, slideWithoutAnim: false, -1f, shouldWait: true, 10f);
		CAbilityNull cAbilityNull = CAbility.CreateAbility(CAbility.EAbilityType.Null, null, isMonster: true, isTargetedAbility: false) as CAbilityNull;
		cAbilityNull.TargetingActor = SledActor;
		cAbilityNull.customWaitMessage = new CWaitForMovementComplete_MessageData(SledActor);
		cAbilityNull.Flags = CAbility.EAbilityFlag.Special1;
		(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(new List<CAbility> { cAbilityNull }, SledActor, killAfter: false, stackToNextCurrent: true);
	}

	private void HandleAfterMoveSled()
	{
		CAbility cAbility = _tickActor?.LastAbilityPerformed;
		if (cAbility == null || cAbility.AbilityType != CAbility.EAbilityType.Null || (cAbility.Flags & CAbility.EAbilityFlag.Special1) == 0 || moveResult == null)
		{
			return;
		}
		if (moveResult.ActualMovedTiles > 0)
		{
			_tickActor.PlayVFX("IceDestructionGeneric");
			if (moveResult.BlockingDoor != null && moveResult.BlockingDoor != _door1 && !moveResult.BlockingDoor.DoorIsOpen)
			{
				moveResult.BlockingDoor.Activate(SledActor);
			}
		}
		if (!_winObjective.IsComplete && _exitTiles.Contains(_sled.ArrayIndex))
		{
			_winObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
		moveResult = null;
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		if (door == _door1)
		{
			_iceWallObjective.SetActivation(active: true);
			_showRoomRevealMessage = true;
		}
		else
		{
			_iceWallObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Phase)
		{
			return false;
		}
		return _showRoomRevealMessage;
	}

	public static List<CTile> ValidTilesForSledMovementChoice()
	{
		List<CTile> tilesInRange = GameState.GetTilesInRange((ScenarioScript.current as ScenarioScript_FH079).SledActor.Tile.m_ArrayIndex, 1);
		List<CTile> list = new List<CTile>();
		foreach (CTile item in tilesInRange)
		{
			if (!item.LivingActors.Any())
			{
				CNode node = ScenarioManager.PathFinder.GetNode(item.m_ArrayIndex);
				if (node != null && node.Walkable && !node.Blocked)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public static void RecalcValidTilesToFormPath(CAbility ability)
	{
		List<CTile> tilesSelected = ability.TilesSelected;
		int num = 1;
		if (tilesSelected.Count == 1)
		{
			CTile cTile = tilesSelected[0];
			tilesSelected.Remove(cTile);
			List<CTile> sledPreviewTiles = GetSledPreviewTiles(cTile);
			tilesSelected.AddRange(sledPreviewTiles);
		}
		ability.NumberTargetsRemaining = Math.Max(0, num - tilesSelected.Count);
		SharedAbilityTargeting.GetValidActorsInRange(ability);
		ability.TilesInRange.AddRange(tilesSelected);
	}

	public static void RemoveRemovedTilesToFormPath(CAbility ability)
	{
		List<CTile> tilesSelected = ability.TilesSelected;
		int num = 1;
		tilesSelected.Clear();
		ability.NumberTargetsRemaining = Math.Max(0, num - tilesSelected.Count);
	}

	public static List<CTile> GetSledPreviewTiles(CTile selectedTile, bool includeBlockingActorTile = false)
	{
		CObjectActor sledActor = (ScenarioScript.current as ScenarioScript_FH079).SledActor;
		return GetSledMoveTiles(sledActor.Tile, selectedTile, sledActor, includeBlockingActorTile);
	}

	private static List<CTile> GetSledMoveTiles(CTile prevTile, CTile selectedTile, CActor sledActor, bool includeBlockingActorTile = false)
	{
		ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(prevTile.m_ArrayIndex.X, prevTile.m_ArrayIndex.Y, selectedTile.m_ArrayIndex.X, selectedTile.m_ArrayIndex.Y);
		List<CTile> list = new List<CTile>();
		list.Add(selectedTile);
		if (adjacentPosition != ScenarioManager.EAdjacentPosition.ECenter)
		{
			while (true)
			{
				CTile cTile = list[list.Count - 1];
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, adjacentPosition);
				if (adjacentTile == null)
				{
					break;
				}
				CObjectDoor cObjectDoor = (CObjectDoor)adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door);
				CActor cActor = ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex);
				if (cActor != null && (cActor.DoesNotBlock || cActor == sledActor))
				{
					cActor = null;
				}
				CNode node = ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex);
				CNode node2 = ScenarioManager.PathFinder.GetNode(adjacentTile.m_ArrayIndex);
				bool flag = true;
				if (!node2.Walkable)
				{
					flag = false;
				}
				if (node2.Blocked && adjacentTile != sledActor?.Tile)
				{
					flag = false;
				}
				if (cObjectDoor != null && !cObjectDoor.DoorIsOpen)
				{
					flag = false;
				}
				if (cActor != null)
				{
					flag = false;
				}
				if (node2.IslandID != node.IslandID && !node2.IsBridgeOpen && !node.IsBridgeOpen && !node2.IsBridge && !node.IsBridge)
				{
					flag = false;
				}
				if (!flag)
				{
					if ((cActor != null) & includeBlockingActorTile)
					{
						list.Add(adjacentTile);
					}
					break;
				}
				list.Add(adjacentTile);
			}
		}
		return list;
	}

	public static List<CActor> GetActorsToDamageAfterSledMove()
	{
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
		if (cAbility is CAbilityDamage)
		{
			if (cAbility.ParentAbility?.TilesSelected != null && cAbility.ParentAbility.TilesSelected.Count > 0)
			{
				ScenarioScript_FH079 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH079;
				CTile cTile = GetSledMoveTiles(scenarioScript_FH.SledActor.GetVar("LastTileBeforeMove").GetObject<CTile>(), cAbility.ParentAbility.TilesSelected[0], scenarioScript_FH.SledActor, includeBlockingActorTile: true).LastOrDefault();
				if (cTile?.LivingActor != null)
				{
					return new List<CActor> { cTile.LivingActor };
				}
			}
		}
		else if (cAbility is CAbilityNullHex cAbilityNullHex && cAbilityNullHex.TilesSelected.Count > 0)
		{
			CTile cTile2 = GetSledPreviewTiles(cAbilityNullHex.TilesSelected[0], includeBlockingActorTile: true).LastOrDefault();
			if (cTile2?.LivingActor != null)
			{
				return new List<CActor> { cTile2.LivingActor };
			}
		}
		return new List<CActor>();
	}

	public static int GetAmountOfDamageAfterSledMove()
	{
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
		if (cAbility is CAbilityDamage)
		{
			if (cAbility.ParentAbility?.TilesSelected != null && cAbility.ParentAbility.TilesSelected.Count > 0)
			{
				return cAbility.ParentAbility.TilesSelected.Count;
			}
		}
		else if (cAbility is CAbilityNullHex cAbilityNullHex && cAbilityNullHex.TilesSelected.Count > 0)
		{
			return GetSledPreviewTiles(cAbilityNullHex.TilesSelected[0]).Count;
		}
		return 0;
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		if (!_door1.Activated && _tickActor is CPlayerActor cPlayerActor && cPlayerActor.Tile.m_ArrayIndex == _pressurePlateTile.ToPoint())
		{
			_door1.ForceActivate(cPlayerActor);
		}
	}
}
