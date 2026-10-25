using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH114 : ScenarioScript
{
	public readonly Point _aLinkIndex = new Point(7, 6);

	public readonly Point _bLinkIndex = new Point(6, 6);

	private CObjective openDoorObjective;

	private CMap secondRoom;

	private int DetonationChargeDemo => ScenarioManager.CurrentScenarioState.Level * 2 + 4;

	private IEnumerable<CObjectActor> AllDetonationCharges => from o in ScenarioManager.Scenario.AllObjects
		where o.IsAttachedToProp
		where o.AttachedProp.SpecificPropType == ESpecificPropType.DetonationCharge
		select o;

	protected override bool Initialize()
	{
		secondRoom = ScenarioScript.FindRoom("Room_6");
		openDoorObjective = GetObjective("Ice Pillars Open Door");
		foreach (CObjectActor allDetonationCharge in AllDetonationCharges)
		{
			allDetonationCharge.SetIgnoreActorCollision(enable: true);
		}
		Point[] array = new Point[2]
		{
			new Point(2, 8),
			new Point(2, 2)
		};
		ScenarioManager.ToggleTileHighlights(array, enabled: true);
		Point[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Point point = array2[i];
			string demoChargeName = "DemoCharge";
			CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
			if (cTile.m_Props.Find((CObjectProp p) => p.PrefabName == demoChargeName) == null)
			{
				CObjectProp cObjectProp = new CObjectProp(demoChargeName, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(point), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(cObjectProp);
				cObjectProp.PropCustomLocKey = "FH_114_Detonation_Charge_Pile_T";
				cObjectProp.PropCustomDescriptionLocKey = "FH_114_Detonation_Charge_Pile_D";
			}
		}
		GetScenarioModifier("Hint - Charges Damage").CustomLocKeyArgs = new List<object> { DetonationChargeDemo };
		return true;
	}

	protected override void DeserializeInternal(string s)
	{
		CheckIfLinkRoomsTiles();
	}

	protected override void TickAbilityPerformStartedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.MoveTrap)
		{
			SetDetonationChargesPathFinderNodesBlocked(isBlocked: true);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		SetDetonationChargesPathFinderNodesBlocked(isBlocked: false);
	}

	private void SetDetonationChargesPathFinderNodesBlocked(bool isBlocked)
	{
		foreach (CObjectActor allDetonationCharge in AllDetonationCharges)
		{
			ScenarioManager.PathFinder.GetNode(allDetonationCharge.Tile.m_ArrayIndex).Blocked = isBlocked;
		}
	}

	public void TakeDetonationCharge(CPlayerActor actor)
	{
		CObjectQuestItem cObjectQuestItem = new CObjectQuestItem("DetonationCharge", ScenarioManager.ObjectImportType.CarryableQuestItem, new TileIndex(actor.Tile.m_ArrayIndex), null, null, null, actor.Tile.m_HexMap.MapGuid);
		actor.CarryQuestItem(cObjectQuestItem);
		cObjectQuestItem.Activate(actor);
	}

	public void PlaceDetonationCharge(CPlayerActor actor, CTile tile, bool spawnPropActor = true)
	{
		if (actor.CarriedQuestItems.Count == 0)
		{
			Log.GameplayError($"Player {actor} doesn't have a detonation charge. Abort!", actor);
			return;
		}
		actor.CarriedQuestItems.Clear();
		CObjectTrap cObjectTrap = new CObjectTrap(ESpecificPropType.DetonationCharge.ToString(), ScenarioManager.ObjectImportType.Trap, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, DetonationChargeDemo, 1, DetonationChargeDemo, new List<CCondition.ENegativeCondition>(), null, 0);
		cObjectTrap.PropCustomLocKey = "DetonationCharge_TOOLTIP";
		cObjectTrap.PropCustomDescriptionLocKey = "FH_114_Detonation_Charge_D";
		tile.SpawnProp(cObjectTrap);
		string text = ESpecificPropType.DetonationCharge.ToString();
		Point arrayIndex = tile.m_ArrayIndex;
		Log.SimpleInfo("Spawned Trap: " + text + " at Array Index: " + arrayIndex.ToString());
		if (spawnPropActor && SpawnDummyActorOnProp(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, cObjectTrap, 1, active: true))
		{
			CObjectActor runtimeAttachedActor = cObjectTrap.RuntimeAttachedActor;
			cObjectTrap.PropHealthDetails.DeathAction = PropHealthDetails.EDeathAction.Activate;
			runtimeAttachedActor.SetIgnoreActorCollision(enable: true);
			CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
			{
				m_SpawnTile = tile,
				m_SpawnEnemy = runtimeAttachedActor,
				m_ActorSpawningMessage = runtimeAttachedActor,
				m_SpawnSource = actor
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor is CPlayerActor cPlayerActor && cPlayerActor.CarriedQuestItems.Count != 0 && _tickAbility is CAbilityAttack)
		{
			PlaceDetonationCharge(cPlayerActor, cPlayerActor.Tile, spawnPropActor: false);
			cPlayerActor.Tile.m_Props.First((CObjectProp p) => p.SpecificPropType == ESpecificPropType.DetonationCharge).Activate(cPlayerActor, _tickAttacker);
		}
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		if (!OncePerRound())
		{
			return;
		}
		foreach (CSpawner item in ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner s) => s.ArrayIndex.X == 5 && s.ArrayIndex.Y == 2))
		{
			item.SetLocation(new TileIndex(11, 8));
			item.HideSpawnerProp();
			item.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		}
		foreach (CSpawner item2 in ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner s) => s.ArrayIndex.X == 4 && s.ArrayIndex.Y == 8))
		{
			item2.SetLocation(new TileIndex(8, 2));
			item2.HideSpawnerProp();
			item2.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		CheckIfLinkRoomsTiles();
	}

	private bool IsDetonationCharge(CActor actor)
	{
		if (!(actor is CObjectActor cObjectActor))
		{
			return false;
		}
		if (!cObjectActor.IsAttachedToProp)
		{
			return false;
		}
		if (!(cObjectActor.AttachedProp is CObjectTrap cObjectTrap))
		{
			return false;
		}
		if (cObjectTrap.SpecificPropType != ESpecificPropType.DetonationCharge)
		{
			return false;
		}
		return true;
	}

	public override bool UseCustomAreActorsAllied()
	{
		return true;
	}

	public override bool UseCustomAreActorsEnemies()
	{
		return true;
	}

	public override bool CustomAreActorsAllied(CActor.EType actor1Type, CActor.EType actor2Type, CActor actor1 = null, CActor actor2 = null)
	{
		if ((actor1Type != CActor.EType.Player && IsDetonationCharge(actor2)) || (actor2Type != CActor.EType.Player && IsDetonationCharge(actor1)))
		{
			return true;
		}
		return base.CustomAreActorsAllied(actor1Type, actor2Type, actor1, actor2);
	}

	public override bool CustomAreActorsEnemies(CActor.EType actor1Type, CActor.EType actor2Type, CActor actor1 = null, CActor actor2 = null)
	{
		if ((actor1Type != CActor.EType.Player && IsDetonationCharge(actor2)) || (actor2Type != CActor.EType.Player && IsDetonationCharge(actor1)))
		{
			return false;
		}
		return base.CustomAreActorsEnemies(actor1Type, actor2Type, actor1, actor2);
	}

	private void CheckIfLinkRoomsTiles()
	{
		if (secondRoom != null && secondRoom.Revealed)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(_aLinkIndex);
			CNode node2 = ScenarioManager.PathFinder.GetNode(_bLinkIndex);
			CNavLink item = new CNavLink(1, node2, blocked: false, allowLos: true);
			if (!node.NavLinks.Contains(item))
			{
				node.NavLinks.Add(item);
			}
			item = new CNavLink(1, node, blocked: false, allowLos: true);
			if (!node2.NavLinks.Contains(item))
			{
				node2.NavLinks.Add(item);
			}
		}
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("charge_damage", DetonationChargeDemo.ToString());
	}

	public override void OnObjectiveComplete(CObjective objective)
	{
		base.OnObjectiveComplete(objective);
		if (objective != openDoorObjective)
		{
			return;
		}
		foreach (CObjectDoor doorProp in ScenarioManager.CurrentScenarioState.DoorProps)
		{
			doorProp.SetExtraLockState(lockedStateToSet: false);
		}
	}
}
