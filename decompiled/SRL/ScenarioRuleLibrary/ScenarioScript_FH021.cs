using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH021 : ScenarioScript
{
	private readonly Point _firstDoorTileIndex = new Point(10, 12);

	private readonly Point _secondDoorTileIndex = new Point(16, 8);

	private readonly Point _glowingOrbTileIndex = new Point(13, 10);

	private readonly Point _spawner0TileIndex = new Point(15, 11);

	private readonly Point _spawner1TileIndex = new Point(13, 7);

	private CMap room2;

	private readonly HashSet<Point> _firstRoomTileIndices = new HashSet<Point>
	{
		new Point(1, 15),
		new Point(2, 16),
		new Point(2, 17),
		new Point(3, 18),
		new Point(3, 19),
		new Point(2, 14),
		new Point(2, 15),
		new Point(3, 16),
		new Point(3, 17),
		new Point(4, 18),
		new Point(4, 19),
		new Point(3, 14),
		new Point(3, 15),
		new Point(4, 16),
		new Point(4, 17),
		new Point(5, 18),
		new Point(3, 13),
		new Point(4, 14),
		new Point(4, 15),
		new Point(5, 16),
		new Point(5, 17),
		new Point(6, 18),
		new Point(4, 13),
		new Point(5, 14),
		new Point(5, 15),
		new Point(6, 16),
		new Point(6, 17),
		new Point(5, 12),
		new Point(5, 13),
		new Point(6, 14),
		new Point(6, 15),
		new Point(7, 16),
		new Point(7, 17),
		new Point(6, 12),
		new Point(6, 13),
		new Point(7, 14),
		new Point(7, 15),
		new Point(8, 16),
		new Point(7, 12),
		new Point(7, 13),
		new Point(8, 14),
		new Point(8, 15),
		new Point(7, 11),
		new Point(8, 12),
		new Point(8, 13),
		new Point(9, 14),
		new Point(8, 11),
		new Point(9, 12),
		new Point(9, 13)
	};

	private readonly List<Point> _secondRoomTileIndices = new List<Point>
	{
		new Point(10, 11),
		new Point(11, 12),
		new Point(10, 10),
		new Point(11, 13),
		new Point(11, 11),
		new Point(11, 10),
		new Point(12, 12),
		new Point(10, 9),
		new Point(12, 13),
		new Point(12, 10),
		new Point(12, 11),
		new Point(11, 9),
		new Point(13, 12),
		new Point(11, 18),
		new Point(13, 13),
		new Point(12, 8),
		new Point(12, 9),
		new Point(13, 10),
		new Point(13, 11),
		new Point(14, 12),
		new Point(12, 7),
		new Point(13, 8),
		new Point(13, 9),
		new Point(14, 10),
		new Point(14, 11),
		new Point(15, 12),
		new Point(13, 7),
		new Point(14, 8),
		new Point(14, 9),
		new Point(15, 10),
		new Point(15, 11),
		new Point(14, 7),
		new Point(15, 8),
		new Point(15, 9),
		new Point(16, 10)
	};

	private bool _firstDoorOpened;

	private CObjectDoor _firstDoor;

	private CObjectDoor _secondDoor;

	private CObjectProp _glowingOrb;

	private CSpawner _spawner0;

	private CSpawner _spawner1;

	private CScenarioModifier _scenarioModifier_ScenarioEffects;

	private CScenarioModifier _scenarioModifier_FirstDoorUnlock;

	private CScenarioModifier _scenarioModifier_GlowingOrbDamage;

	private CScenarioModifier _scenarioModifier_LivingBonesSpawners;

	private CScenarioModifier _scenarioModifier_SecondDoorUnlock;

	protected override bool Initialize()
	{
		room2 = ScenarioScript.FindRoom("Room_2");
		_glowingOrb = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.ArrayIndex.ToPoint() == _glowingOrbTileIndex && p.PrefabName == "OrbOfBones");
		_spawner0 = ScenarioManager.CurrentScenarioState.Spawners.First((CSpawner s) => s.ArrayIndex.ToPoint() == _spawner0TileIndex);
		_spawner1 = ScenarioManager.CurrentScenarioState.Spawners.First((CSpawner s) => s.ArrayIndex.ToPoint() == _spawner1TileIndex);
		_firstDoor = ScenarioScript.FindDoor(_firstDoorTileIndex.X, _firstDoorTileIndex.Y);
		_secondDoor = ScenarioScript.FindDoor(_secondDoorTileIndex.X, _secondDoorTileIndex.Y);
		_scenarioModifier_ScenarioEffects = GetScenarioModifier("ScenarioEffects");
		_scenarioModifier_FirstDoorUnlock = GetScenarioModifier("FirstDoorUnlock");
		_scenarioModifier_GlowingOrbDamage = GetScenarioModifier("GlowingOrbDamage");
		_scenarioModifier_LivingBonesSpawners = GetScenarioModifier("LivingBonesSpawners");
		_scenarioModifier_SecondDoorUnlock = GetScenarioModifier("SecondDoorUnlock");
		return true;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(_firstDoorOpened);
	}

	protected override void DeserializeInternal(string s)
	{
		_secondDoor.SetHasExtraLock(_glowingOrb != null);
		if (string.IsNullOrEmpty(s))
		{
			_firstDoor.SetHasExtraLock(AnyAliveEnemyInFirstRoom() && !room2.Revealed);
			return;
		}
		_firstDoorOpened = JsonConvert.DeserializeObject<bool>(s);
		if (_firstDoorOpened)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(_firstDoorTileIndex);
			node.Walkable = false;
			node.Blocked = true;
			_firstDoor.SetHasExtraLock(hasExtraLock: true);
		}
		else
		{
			_firstDoor.SetHasExtraLock(AnyAliveEnemyInFirstRoom() && !room2.Revealed);
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickRoomRevealed();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 117);
			break;
		}
	}

	protected override void TickDeathInternal()
	{
		if (_firstDoor.DoorHasExtraLock && !room2.Revealed && !AnyAliveEnemyInFirstRoom())
		{
			_firstDoor.SetHasExtraLock(hasExtraLock: false);
			_scenarioModifier_FirstDoorUnlock.SetHiddenState(shouldSetHidden: true);
		}
		if (_secondDoor.DoorHasExtraLock && _glowingOrb != null && _tickActor == _glowingOrb.RuntimeAttachedActor)
		{
			_secondDoor.SetHasExtraLock(hasExtraLock: false);
			_spawner0.SetActive(active: false);
			_spawner1.SetActive(active: false);
			_glowingOrb = null;
			_scenarioModifier_GlowingOrbDamage.SetHiddenState(shouldSetHidden: true);
			_scenarioModifier_LivingBonesSpawners.SetHiddenState(shouldSetHidden: true);
			_scenarioModifier_SecondDoorUnlock.SetHiddenState(shouldSetHidden: true);
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_glowingOrb == null || _tickActor != _glowingOrb.RuntimeAttachedActor || damageTaken <= 0)
		{
			return;
		}
		CustomAbility.PlayVFX("S21OrbExplosion", _tickActor.ArrayIndex);
		foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor a) => a != _glowingOrb.RuntimeAttachedActor && (a.Class == null || !(a.Class is CHeroSummonClass))))
		{
			GameState.ActorBeenDamaged(item, 2, checkIfPlayerCanAvoidDamage: true);
			GameState.ActorHealthCheck(item, item);
		}
	}

	protected override void TickRepositionInternal()
	{
		CNode node = ScenarioManager.PathFinder.GetNode(_firstDoorTileIndex);
		if (_tickActor.Type != CActor.EType.Player || (_tickAbility.AbilityType != CAbility.EAbilityType.Move && _tickAbility.AbilityType != CAbility.EAbilityType.Teleport2) || !_firstDoor.DoorIsOpen || _firstDoorOpened)
		{
			return;
		}
		List<CActor> list = ScenarioManager.Scenario.AllAliveActors.Where((CActor a) => a != _tickActor && (a.Type == CActor.EType.Player || a.Type == CActor.EType.HeroSummon)).ToList();
		if (!_secondRoomTileIndices.Contains(_tickActor.ArrayIndex))
		{
			list.Add(_tickActor);
		}
		list.Sort((CActor x, CActor y) => x.Initiative() - y.Initiative());
		int num = -1;
		foreach (CActor item in list)
		{
			for (int num2 = num + 1; num2 < _secondRoomTileIndices.Count; num2++)
			{
				CTile tile = ScenarioManager.GetTile(_secondRoomTileIndices[num2]);
				if (CSpawner.IsEmptyTile(tile))
				{
					GameState.TeleportActor(item, tile);
					num++;
					break;
				}
			}
		}
		num = -1;
		foreach (CObjectProp item2 in ScenarioManager.CurrentScenarioState.ShadowProps.ToList())
		{
			int num3 = num + 1;
			if (num3 < _secondRoomTileIndices.Count)
			{
				CTile tile2 = ScenarioManager.GetTile(_secondRoomTileIndices[num3]);
				CObjectShadow prop = new CObjectShadow("Shadow", ScenarioManager.ObjectImportType.Shadow, new TileIndex(tile2.m_ArrayIndex), null, null, item2.Owner, tile2.m_HexMap.MapGuid);
				tile2.SpawnProp(prop);
				num++;
			}
			item2.DestroyProp();
		}
		_firstDoor.CloseOpenedDoor();
		_firstDoor.SetHasExtraLock(hasExtraLock: true);
		_firstDoorOpened = true;
		node.Walkable = false;
		node.Blocked = true;
		_secondDoor.SetHasExtraLock(hasExtraLock: true);
	}

	private void TickStartRound()
	{
	}

	private void TickRoomRevealed()
	{
		if (_tickActor.ArrayIndex == _firstDoorTileIndex && !_scenarioModifier_ScenarioEffects.IsHidden)
		{
			_scenarioModifier_ScenarioEffects.SetHiddenState(shouldSetHidden: true);
			_scenarioModifier_FirstDoorUnlock.SetHiddenState(shouldSetHidden: true);
			_scenarioModifier_GlowingOrbDamage.SetHiddenState(shouldSetHidden: false);
			_scenarioModifier_LivingBonesSpawners.SetHiddenState(shouldSetHidden: false);
			_scenarioModifier_SecondDoorUnlock.SetHiddenState(shouldSetHidden: false);
		}
		else if (_tickActor.ArrayIndex == _secondDoorTileIndex && !_scenarioModifier_GlowingOrbDamage.IsHidden)
		{
			_scenarioModifier_GlowingOrbDamage.SetHiddenState(shouldSetHidden: true);
			_scenarioModifier_LivingBonesSpawners.SetHiddenState(shouldSetHidden: true);
			_scenarioModifier_SecondDoorUnlock.SetHiddenState(shouldSetHidden: true);
		}
	}

	private bool AnyAliveEnemyInFirstRoom()
	{
		return GetAllActorsInsideFirstRoom().Any((CActor a) => !a.IsDead && a.Type == CActor.EType.Enemy);
	}

	private IEnumerable<CActor> GetAllActorsInsideFirstRoom()
	{
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			if (_firstRoomTileIndices.Contains(allActor.ArrayIndex))
			{
				yield return allActor;
			}
		}
	}
}
