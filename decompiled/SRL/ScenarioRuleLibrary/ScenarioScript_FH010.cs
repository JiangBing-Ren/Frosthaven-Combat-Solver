using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH010 : ScenarioScript
{
	private static int PILLARS_RANGE = 4;

	private static string ROOM_1_NAME = "Room_5";

	private static string ROOM_2_NAME = "Room_6";

	private static string[] SHIELD_ABILITY_NAMES = new string[5] { "AScenarioShield1", "AScenarioShield2", "AScenarioShield3", "AScenarioShield4", "AScenarioShield5" };

	private static string[] SHIELD_MODIFIERS_NAMES = new string[5] { "ShieldAbility1", "ShieldAbility2", "ShieldAbility3", "ShieldAbility4", "ShieldAbility5" };

	private CObjectDoor[] _doors;

	private CMap _room1;

	private CMap _room2;

	private CScenarioModifier _lockedDoorScenarioModifier;

	private List<EnemyState> RevealedMonstersAlive => ScenarioManager.CurrentScenarioState.Monsters.FindAll((EnemyState m) => m.IsRevealed && !m.IsDead && !m.Actor.IsDead);

	private List<CObjectProp> IcePillars => ScenarioManager.CurrentScenarioState.Props.FindAll((CObjectProp p) => p.PrefabName == "OneHexObstacle" && p.PropHealthDetails.CurrentHealth > 0);

	protected override bool Initialize()
	{
		_doors = new CObjectDoor[3]
		{
			GetProp(9, 2, "ThinDoor") as CObjectDoor,
			GetProp(9, 4, "ThinDoor") as CObjectDoor,
			GetProp(9, 6, "ThinDoor") as CObjectDoor
		};
		CObjectProp prop = GetProp(6, 4, "OneHexObstacle", checkDestroyed: true);
		CObjectProp prop2 = GetProp(2, 5, "OneHexObstacle", checkDestroyed: true);
		if (ScenarioScript.PartySize < 3)
		{
			EnsurePropIsRemoved(prop);
		}
		if (ScenarioScript.PartySize < 4)
		{
			EnsurePropIsRemoved(prop2);
		}
		_room1 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_5");
		_room2 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_6");
		_lockedDoorScenarioModifier = GetScenarioModifier("lockedDoor");
		UnblockIcePillarsForTargeting();
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (_room1.Destroyed)
		{
			DestroyRoom1Props();
		}
	}

	private void UnblockIcePillarsForTargeting()
	{
		foreach (CObjectProp icePillar in IcePillars)
		{
			(icePillar as CObjectObstacle)?.SetBlockLOS(blockLOS: false);
			if (icePillar.RuntimeAttachedActor != null)
			{
				icePillar.RuntimeAttachedActor.IsTransientBlocker = false;
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UpdateMonstersShieldBonuses();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			CheckSpecialRules(checkIfMoveToRoom2: true);
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 89);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
			break;
		}
	}

	protected override void TickRepositionInternal()
	{
		CheckSpecialRules(checkIfMoveToRoom2: true, _tickAbility as CAbilityMove);
	}

	protected override void TickDeathInternal()
	{
		CheckSpecialRules(checkIfMoveToRoom2: false);
	}

	private void CheckSpecialRules(bool checkIfMoveToRoom2, CAbilityMove moveAbility = null)
	{
		UnlockDoorsIfRevealedMonstersKilled();
		if (checkIfMoveToRoom2)
		{
			MoveToRoom2IfAnyDoorOpened(moveAbility);
		}
		UpdateMonstersShieldBonuses();
		CheckSpecialRuleDescription();
	}

	private void CheckSpecialRuleDescription()
	{
		_lockedDoorScenarioModifier.SetHiddenState(_room2.Revealed);
	}

	private void UpdateMonstersShieldBonuses()
	{
		foreach (EnemyState item in RevealedMonstersAlive)
		{
			UpdateActorShieldBonus(item.Actor);
		}
	}

	private void UnlockDoorsIfRevealedMonstersKilled()
	{
		if (!_room2.Revealed && RevealedMonstersAlive.Count == 0)
		{
			CObjectDoor[] doors = _doors;
			for (int i = 0; i < doors.Length; i++)
			{
				doors[i].UnlockLockedDoorWithoutOpening();
			}
		}
	}

	private void MoveToRoom2IfAnyDoorOpened(CAbilityMove moveAbility = null)
	{
		if (!_doors.Any((CObjectDoor d) => d.DoorIsOpen) || !OncePerRound())
		{
			return;
		}
		_room2.Reveal(initial: false);
		CRevealRooms_MessageData message = new CRevealRooms_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		List<CTile> doorsTiles = _doors.Select((CObjectDoor d) => ScenarioManager.GetTile(d.ArrayIndex)).ToList();
		List<CTile> room2Tiles = GetRoomTiles(ROOM_2_NAME);
		room2Tiles.RemoveAll((CTile t) => doorsTiles.Contains(t));
		if (moveAbility != null)
		{
			moveAbility.LocoStop();
			ProcessActor(moveAbility.TargetingActor);
		}
		List<CActor> list = new List<CActor>(ScenarioManager.Scenario.PlayerActors.Where((CPlayerActor pa) => pa != moveAbility?.TargetingActor));
		list.AddRange(ScenarioManager.Scenario.HeroSummons);
		foreach (CActor item in list)
		{
			ProcessActor(item);
		}
		DestroyRoom1Props();
		CObjectDoor[] doors = _doors;
		foreach (CObjectDoor obj in doors)
		{
			obj.CloseOpenedDoor(lockDoor: true);
			obj.SetExtraLockState(lockedStateToSet: true);
		}
		_room1.Destroyed = true;
		UnblockIcePillarsForTargeting();
		UpdateMonstersShieldBonuses();
		void ProcessActor(CActor cActor)
		{
			if (!GetNearestFreeTile(cActor.Tile.m_ArrayIndex.X, cActor.Tile.m_ArrayIndex.Y, out var nearestTile, room2Tiles))
			{
				Trace("Can't find nearest free tile for actor: " + cActor.ActorGuid, null, 149);
			}
			room2Tiles.Remove(nearestTile);
			TeleportActor(cActor, nearestTile, tryReveal: true, (moveAbility?.TargetingActor == cActor) ? moveAbility : null);
			if (cActor is CPlayerActor playerActor)
			{
				int _removedShadows = 0;
				if (RemoveAndCacheShadows(playerActor, ref _removedShadows))
				{
					OnPhaseIn(playerActor, ref _removedShadows);
				}
			}
		}
	}

	private void DestroyRoom1Props()
	{
		List<CTile> roomTiles = GetRoomTiles(ROOM_1_NAME);
		foreach (CObjectProp item in roomTiles.SelectMany((CTile x) => x.m_Props).ToList())
		{
			if (!(item is CObjectDoor))
			{
				if (item.RuntimeAttachedActor != null)
				{
					GameState.KillActor(item.RuntimeAttachedActor, item.RuntimeAttachedActor, CActor.ECauseOfDeath.Suicide, out var _);
				}
				item.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			}
		}
		foreach (CTile item2 in roomTiles)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(item2.m_ArrayIndex);
			node.Walkable = false;
			node.Blocked = true;
		}
	}

	private void UpdateActorShieldBonus(CActor actor)
	{
		if (actor.Type == CActor.EType.Enemy)
		{
			int count = IcePillars.FindAll((CObjectProp ip) => ScenarioManager.GetTileDistance(ip.PropTile, actor.Tile) <= PILLARS_RANGE && actor.Tile.m_HexMap.RoomName == ip.PropTile.m_HexMap.RoomName).Count;
			CActiveBonus cActiveBonus = actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Shield).FirstOrDefault((CActiveBonus sb) => SHIELD_ABILITY_NAMES.Contains(sb.Ability.Name));
			if ((cActiveBonus?.Ability.Strength ?? 0) != count)
			{
				ChangeActorShieldBonus(actor, cActiveBonus, count);
			}
		}
	}

	private void ChangeActorShieldBonus(CActor actor, CActiveBonus currentScenarioBonus, int shieldStrength)
	{
		currentScenarioBonus?.Finish();
		if (shieldStrength > 0)
		{
			CAbility cAbility = GetScenarioModifier(SHIELD_MODIFIERS_NAMES[shieldStrength - 1]).TriggerAbility();
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private void EnsurePropIsRemoved(CObjectProp prop)
	{
		if (prop != null)
		{
			CObjectObstacle lastDestroyedObstacle = GameState.LastDestroyedObstacle;
			prop.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			GameState.LastDestroyedObstacle = lastDestroyedObstacle;
		}
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		foreach (CObjectProp icePillar in IcePillars)
		{
			if (icePillar.RuntimeAttachedActor != null && !icePillar.RuntimeAttachedActor.IsDead && icePillar.ArrayIndex.ToPoint() == tile.m_ArrayIndex)
			{
				return true;
			}
		}
		return false;
	}
}
