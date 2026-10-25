using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH056 : ScenarioScript
{
	private static Point Harbinger1RoomTeleportPosition = new Point(14, 4);

	private static int AltarMinimumHP = 1;

	private CObjectDoor _door1;

	private CObjectDoor _door2;

	private CObjectProp _altar1;

	private CObjectProp _altar2;

	private CObjectProp _altar3;

	private CScenarioModifier _firstDoorUnlockScenarioModifier;

	private CScenarioModifier _secondDoorUnlockScenarioModifier;

	private CScenarioModifier _immuneHarbingerScenarioModifier1;

	private CScenarioModifier _immuneHarbingerScenarioModifier2;

	private CScenarioModifier _activeHarbingerScenarioModifier;

	private CScenarioModifier _altarBelow1HpScenarioModifier;

	private CScenarioModifier _sharedHpHarbingersScenarioModifier;

	private CScenarioModifier _separateHpHarbingersScenarioModifier;

	private List<(CObjectProp, string)> _altarsMessages;

	private CObjective_Scripted EnoughPlayers;

	private static readonly float ONE_THIRD = 0.33334f;

	private static readonly float TWO_THIRD = 0.66667f;

	private bool _teleportedFirstTime;

	private bool _teleportedSecondTime;

	private bool _teleportedThirdTime;

	private bool _teleportedBackFirstTime;

	private bool _teleportedBackSecondTime;

	private CActor HarbingerInTeleportRoom => FindActorByGUID("fa8ea114-e8a1-c18c-3428-0bd82427a51f", errorIfNotFound: false);

	private CActor HarbingerInStartingRoom => FindActorByGUID("64d3e79c-f257-039a-41d7-135ab78d0c04", errorIfNotFound: false);

	private CMap HarbingerRoom => ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_9");

	private int GetHarbingerHealth(float ratio)
	{
		return (int)Math.Floor((float)HarbingerInTeleportRoom.MaxHealth * ratio);
	}

	protected override bool Initialize()
	{
		_ = ScenarioManager.CurrentScenarioState.Props;
		_ = ScenarioManager.CurrentScenarioState.Spawners;
		_door1 = GetProp(3, 9, "ThickDoor") as CObjectDoor;
		_altar1 = GetProp(2, 7, "OneHexObstacle", checkDestroyed: true);
		_door2 = GetProp(6, 14, "ThickDoor") as CObjectDoor;
		_altar2 = GetProp(2, 15, "OneHexObstacle", checkDestroyed: true);
		_altar3 = GetProp(10, 16, "OneHexObstacle", checkDestroyed: true);
		_altarsMessages = new List<(CObjectProp, string)>
		{
			(_altar1, "_1"),
			(_altar2, "_3"),
			(_altar3, "_5")
		};
		_firstDoorUnlockScenarioModifier = GetScenarioModifier("firstDoorUnlockScenarioModifier");
		_secondDoorUnlockScenarioModifier = GetScenarioModifier("secondDoorUnlockScenarioModifier");
		_immuneHarbingerScenarioModifier1 = GetScenarioModifier("immuneHarbingerScenarioModifier1");
		_immuneHarbingerScenarioModifier2 = GetScenarioModifier("immuneHarbingerScenarioModifier2");
		_activeHarbingerScenarioModifier = GetScenarioModifier("activeHarbingerScenarioModifier");
		_altarBelow1HpScenarioModifier = GetScenarioModifier("altarBelow1HpScenarioModifier");
		_sharedHpHarbingersScenarioModifier = GetScenarioModifier("sharedHpHarbingersScenarioModifier");
		_separateHpHarbingersScenarioModifier = GetScenarioModifier("separateHpHarbingersScenarioModifier");
		EnoughPlayers = GetObjective<CObjective_Scripted>("EnoughPlayers");
		EnoughPlayers.Total = 1;
		UpdateHarbringerIdleVFX();
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_teleportedFirstTime);
		string text2 = JsonConvert.SerializeObject(_teleportedSecondTime);
		string text3 = JsonConvert.SerializeObject(_teleportedThirdTime);
		string text4 = JsonConvert.SerializeObject(_teleportedBackFirstTime);
		string text5 = JsonConvert.SerializeObject(_teleportedBackSecondTime);
		string text6 = JsonConvert.SerializeObject((HarbingerInTeleportRoom as CEnemyActor)?.IsImmuneToDamage ?? false);
		string text7 = JsonConvert.SerializeObject((HarbingerInStartingRoom as CEnemyActor)?.IsImmuneToDamage ?? false);
		return JsonConvert.SerializeObject(new string[7] { text, text2, text3, text4, text5, text6, text7 });
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_teleportedFirstTime = JsonConvert.DeserializeObject<bool>(array[0]);
			_teleportedSecondTime = JsonConvert.DeserializeObject<bool>(array[1]);
			_teleportedThirdTime = JsonConvert.DeserializeObject<bool>(array[2]);
			_teleportedBackFirstTime = JsonConvert.DeserializeObject<bool>(array[3]);
			_teleportedBackSecondTime = JsonConvert.DeserializeObject<bool>(array[4]);
			bool isImmuneToDamage = JsonConvert.DeserializeObject<bool>(array[5]);
			bool isImmuneToDamage2 = JsonConvert.DeserializeObject<bool>(array[6]);
			if (HarbingerInTeleportRoom is CEnemyActor cEnemyActor)
			{
				cEnemyActor.IsImmuneToDamage = isImmuneToDamage;
			}
			if (HarbingerInStartingRoom is CEnemyActor cEnemyActor2)
			{
				cEnemyActor2.IsImmuneToDamage = isImmuneToDamage2;
			}
		}
		UpdateAltarsHas1MinimumHp();
	}

	private void CheckHasEnoughPlayersLeft()
	{
		IEnumerable<CPlayerActor> enumerable = ScenarioManager.Scenario.PlayerActors.Where((CPlayerActor x) => !x.IsDead);
		IEnumerable<CPlayerActor> enumerable2 = enumerable.Where((CPlayerActor x) => x.Tile.m_HexMap == HarbingerRoom);
		IEnumerable<CPlayerActor> source = enumerable.Except(enumerable2);
		int num = enumerable2.Count();
		int num2 = source.Count();
		CActor harbingerInTeleportRoom = HarbingerInTeleportRoom;
		CActor harbingerInStartingRoom = HarbingerInStartingRoom;
		bool flag = !_altar1.IsDestroyed || !_altar2.IsDestroyed || !_altar3.IsDestroyed;
		if ((harbingerInStartingRoom == null || !harbingerInStartingRoom.IsDead) && num2 < 1)
		{
			EnoughPlayers.Current = 1;
			EnoughPlayers.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
		else if ((harbingerInTeleportRoom == null || !harbingerInTeleportRoom.IsDead) && num < 1)
		{
			int num3 = num2 - 1;
			if (!flag || num3 < 1)
			{
				EnoughPlayers.Current = 1;
				EnoughPlayers.ScriptedIsComplete = true;
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			TickStartScenario();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickRoomRevealed();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 174);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.AfterAbility:
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			break;
		}
	}

	private void TickStartScenario()
	{
		if (OncePerRound())
		{
			ToggleImmunityAbility(HarbingerInStartingRoom, isActive: true);
		}
	}

	protected override void TickDeathInternal()
	{
		CheckSpecialRules();
		if (_tickActor == _altar1.RuntimeAttachedActor && !_teleportedFirstTime)
		{
			TeleportToHarbingerRoom();
			_teleportedFirstTime = true;
		}
		else if (_tickActor == _altar2.RuntimeAttachedActor && !_teleportedSecondTime)
		{
			_secondDoorUnlockScenarioModifier.Hide();
			TeleportToHarbingerRoom();
			_teleportedSecondTime = true;
		}
		else if (_tickActor == _altar3.RuntimeAttachedActor && !_teleportedThirdTime)
		{
			_teleportedThirdTime = true;
			TeleportToHarbingerRoom();
			int health = HarbingerInTeleportRoom.Health * (ScenarioScript._css.Players.Count - 1) / 2;
			ScenarioScript.SetHealth(HarbingerInStartingRoom, health);
			_separateHpHarbingersScenarioModifier.Show();
			_sharedHpHarbingersScenarioModifier.Hide();
			_altarBelow1HpScenarioModifier.Hide();
			_activeHarbingerScenarioModifier.Hide();
		}
		CheckHasEnoughPlayersLeft();
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor == HarbingerInTeleportRoom)
		{
			CheckIfTeleportBackFromHarbingersRoom();
		}
		if (!_teleportedThirdTime && (_tickActor == HarbingerInTeleportRoom || _tickActor == HarbingerInStartingRoom) && (_door2.DoorIsClosed || _altar3.PropHealthDetails.CurrentHealth > 0) && SyncHealthPoints(HarbingerInTeleportRoom, HarbingerInStartingRoom))
		{
			CheckIfTeleportBackFromHarbingersRoom();
		}
	}

	private void CheckIfTeleportBackFromHarbingersRoom()
	{
		if (!_teleportedBackFirstTime && TeleportedBackWhenHarbingerHealthBelow(TWO_THIRD))
		{
			_teleportedBackFirstTime = true;
		}
		else if (!_teleportedBackSecondTime && TeleportedBackWhenHarbingerHealthBelow(ONE_THIRD))
		{
			_teleportedBackSecondTime = true;
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		CheckSpecialRules();
	}

	protected override void TickRepositionInternal()
	{
		CheckSpecialRules();
		CheckHasEnoughPlayersLeft();
	}

	private void TickRoomRevealed()
	{
		if (_tickActor.ArrayIndex == _door1.ArrayIndex.ToPoint() && !_firstDoorUnlockScenarioModifier.IsHidden)
		{
			_firstDoorUnlockScenarioModifier.Hide();
			_secondDoorUnlockScenarioModifier.Show();
			_altarBelow1HpScenarioModifier.Show();
		}
		else if (_tickActor.ArrayIndex == _door2.ArrayIndex.ToPoint())
		{
			_ = _secondDoorUnlockScenarioModifier.IsHidden;
		}
	}

	private void CheckSpecialRules()
	{
		UnlockDoorIfAltarIsDestroyed(_door1, _altar1);
		UnlockDoorIfAltarIsDestroyed(_door2, _altar2);
		UpdateAltarsHas1MinimumHp();
		UpdateHarbringerIdleVFX();
	}

	private bool TeleportedBackWhenHarbingerHealthBelow(float ratio)
	{
		int harbingerHealth = GetHarbingerHealth(ratio);
		if (HarbingerInTeleportRoom.Health <= harbingerHealth)
		{
			CActor cActor = ScenarioManager.Scenario.AllAliveActors.FirstOrDefault((CActor a) => a.Type == CActor.EType.Player && a.Tile.m_HexMap == HarbingerRoom);
			CActor cActor2 = ScenarioManager.Scenario.AllAliveActors.FirstOrDefault((CActor a) => a.Tile.m_HexMap != HarbingerRoom);
			if (cActor != null && cActor2 != null)
			{
				TeleportActorAndSummons(cActor, cActor2.Tile);
				foreach (EnemyState item in HarbingerRoom.Monsters.Where((EnemyState m) => m.ClassID == "BlackImpID"))
				{
					GameState.KillActor(item.Actor, item.Actor, CActor.ECauseOfDeath.Damage, out var _);
				}
				ScenarioScript.HealTo(HarbingerInTeleportRoom, harbingerHealth);
				ToggleImmunityAbility(HarbingerInTeleportRoom, isActive: true);
				_activeHarbingerScenarioModifier.Hide();
				_immuneHarbingerScenarioModifier2.Show();
				HarbingerRoom.Destroyed = true;
				return true;
			}
		}
		return false;
	}

	private void RevealHarbingerRoom()
	{
		if (!HarbingerRoom.Revealed)
		{
			HarbingerRoom.Reveal(initial: false);
			CRevealRooms_MessageData message = new CRevealRooms_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			AddSharedHPBuff(HarbingerInTeleportRoom);
			AddSharedHPBuff(HarbingerInStartingRoom);
			GameState.SortIntoInitiativeAndIDOrder();
		}
		HarbingerRoom.Destroyed = false;
	}

	private void AddSharedHPBuff(CActor actor)
	{
		if (!actor.ActiveBonuses.Any((CActiveBonus x) => x.Ability.Name == "FH056_SharedHPBuff"))
		{
			CAbility cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: false, "", null, null, null, "FH056_SharedHPBuff");
			cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			cAbility.ActiveBonusData.Vars = new List<(string, TValue)> { ("FH056_SharedHPBuff", new TValue(val: true)) };
			cAbility.ActiveBonusData.ListLayoutOverride = ("$FH_Scenario_Campaign_056_SCENARIO_MODIFIER_1$", "Positive_Unique_Campaign");
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private void UpdateHarbringerIdleVFX()
	{
		if (HarbingerInStartingRoom != null && !HarbingerInStartingRoom.IsDead)
		{
			if (_altar1?.RuntimeAttachedActor != null && _altar1.PropHealthDetails.CurrentHealth > 0)
			{
				UpdateHarbringerIdleVFX("State1");
			}
			else if (_altar2 != null && (_altar2?.RuntimeAttachedActor == null || _altar2.PropHealthDetails.CurrentHealth > 0))
			{
				UpdateHarbringerIdleVFX("State2");
			}
			else if (_altar3 != null && (_altar3?.RuntimeAttachedActor == null || _altar3.PropHealthDetails.CurrentHealth > 0))
			{
				UpdateHarbringerIdleVFX("State3");
			}
			else
			{
				UpdateHarbringerIdleVFX("StateOff");
			}
		}
		void UpdateHarbringerIdleVFX(string state)
		{
			CChangeActorVisualState_MessageData message = new CChangeActorVisualState_MessageData
			{
				m_actor = HarbingerInStartingRoom,
				m_stateName = state,
				m_delay = 0.001f
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	private void UnlockDoorIfAltarIsDestroyed(CObjectDoor door, CObjectProp prop)
	{
		if (prop.RuntimeAttachedActor != null && prop.PropHealthDetails.CurrentHealth <= 0 && door.DoorHasExtraLock)
		{
			door.SetHasExtraLock(hasExtraLock: false);
		}
	}

	private void UpdateAltarsHas1MinimumHp()
	{
		CObjectProp[] array = new CObjectProp[2] { _altar2, _altar3 };
		foreach (CObjectProp altar in array)
		{
			if (altar.RuntimeAttachedActor != null)
			{
				int count = ScenarioManager.Scenario.PlayerActors.FindAll((CPlayerActor a) => a.Tile.m_HexMap == altar.PropTile.m_HexMap).Count;
				altar.RuntimeAttachedActor.MinimumHealth = ((count == ScenarioManager.Scenario.PlayerActors.Count) ? ((int?)null) : new int?(AltarMinimumHP));
			}
		}
	}

	private void TeleportToHarbingerRoom()
	{
		CActor actor = ((_tickAbility.TargetingActor is CHeroSummonActor cHeroSummonActor) ? cHeroSummonActor.Summoner : _tickAbility.TargetingActor);
		RevealHarbingerRoom();
		TeleportActorAndSummons(actor, ScenarioManager.GetTile(Harbinger1RoomTeleportPosition));
		ToggleImmunityAbility(HarbingerInTeleportRoom, isActive: false);
		ToggleImmunityAbility(HarbingerInStartingRoom, !_teleportedThirdTime);
		_immuneHarbingerScenarioModifier1.Hide();
		_immuneHarbingerScenarioModifier2.Hide();
		_activeHarbingerScenarioModifier.Show();
	}

	public bool IsHarbinger1Active()
	{
		return !HasImmunityAbility(HarbingerInTeleportRoom);
	}

	private bool HasImmunityAbility(CActor actor)
	{
		return actor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).Count > 0;
	}

	private void ToggleImmunityAbility(CActor actor, bool isActive)
	{
		if (actor != null)
		{
			(actor as CEnemyActor).IsImmuneToDamage = isActive;
			if (!HasImmunityAbility(actor) & isActive)
			{
				CAbility cAbility = GetScenarioModifier("ImmunityAbility").TriggerAbility();
				cAbility.Start(actor, actor);
				cAbility.ApplyToActor(actor);
			}
			else if (HasImmunityAbility(actor) && !isActive)
			{
				actor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).FirstOrDefault()?.Finish();
			}
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		foreach (var altarsMessage in _altarsMessages)
		{
			if (altarsMessage.Item1.PropTile.m_HexMap.Revealed && altarsMessage.Item1.PropHealthDetails.CurrentHealth <= 0 && message.MessageName.EndsWith(altarsMessage.Item2))
			{
				Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 413);
				return true;
			}
		}
		if (_teleportedBackFirstTime && message.MessageName.EndsWith("_2"))
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 419);
			return true;
		}
		if (_teleportedBackSecondTime && message.MessageName.EndsWith("_4"))
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 424);
			return true;
		}
		return false;
	}

	private void TeleportActorAndSummons(CActor actor, CTile tile)
	{
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if ((allAliveActor == actor || (allAliveActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == actor)) && GetNearestFreeTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, out var nearestTile))
			{
				TeleportActor(allAliveActor, nearestTile);
			}
		}
	}

	private bool SyncHealthPoints(CActor actor1, CActor actor2)
	{
		if (actor1 == null || actor2 == null || actor1.Health == actor2.Health)
		{
			return false;
		}
		CActor cActor = ((actor1.Health < actor2.Health) ? actor1 : actor2);
		ScenarioScript.ReduceHp((actor1.Health > actor2.Health) ? actor1 : actor2, cActor.Health);
		return true;
	}
}
