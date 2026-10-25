using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH135 : ScenarioScript
{
	private CScenarioModifier _mirrorDamageModifier;

	private List<(Point, Point)> ConvergancePoints = new List<(Point, Point)>
	{
		(new Point(14, 2), new Point(15, 9)),
		(new Point(14, 4), new Point(17, 13)),
		(new Point(10, 7), new Point(12, 12)),
		(new Point(11, 10), new Point(11, 15))
	};

	private List<CMap> shieldRooms = new List<CMap>();

	private List<CMap> attackRooms = new List<CMap>();

	private CScenarioModifier _shieldModifier;

	private CScenarioModifier _attackModifier;

	private CObjective_Scripted LoseIfNotEnoughCharactersInEachPlane;

	protected override bool Initialize()
	{
		_mirrorDamageModifier = GetScenarioModifier("MirrorDamage");
		_shieldModifier = GetScenarioModifier("ShieldModifier");
		_attackModifier = GetScenarioModifier("AttackModifier");
		shieldRooms.Clear();
		shieldRooms.Add(ScenarioScript.FindRoom("Room_1"));
		shieldRooms.Add(ScenarioScript.FindRoom("Room_3"));
		attackRooms.Clear();
		attackRooms.Add(ScenarioScript.FindRoom("Room_2"));
		attackRooms.Add(ScenarioScript.FindRoom("Room_4"));
		LoseIfNotEnoughCharactersInEachPlane = GetObjective<CObjective_Scripted>("LoseIfNotEnoughCharactersInEachPlane");
		LoseIfNotEnoughCharactersInEachPlane.Total = 1;
		UpdateTileHighlights();
		CAbility mirrorAbility = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _mirrorDamageModifier.ScenarioAbilityID)?.ScenarioAbilities[0];
		if (mirrorAbility != null)
		{
			foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => !(x is CPlayerActor)))
			{
				if (item.FindApplicableActiveBonuses().Any((CActiveBonus x) => x.Ability.HasID(mirrorAbility.ID)))
				{
					UpdateDamageRedirectionVFX(item);
				}
			}
		}
		return true;
	}

	private void UpdateTileHighlights()
	{
		ScenarioManager.ToggleTileHighlights(from x in ConvergancePoints.Select(((Point, Point) x) => x.Item1).Concat(ConvergancePoints.Select(((Point, Point) x) => x.Item2))
			where ScenarioManager.GetTile(x).m_HexMap.Revealed
			select x, enabled: true, new CVector4(1f, 1f, 0f, 1f));
		ScenarioManager.ToggleTileLinks(ConvergancePoints.Where(((Point, Point) x) => ScenarioManager.GetTile(x.Item1).m_HexMap.Revealed), enabled: true);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			CheckConvergance(allAliveActor, init: true);
		}
		CheckPlanePlayerCount();
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			UpdateTileHighlights();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			UpdateShieldAttackBonuses(_tickActor);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			if (LoseIfNotEnoughCharactersInEachPlane.Current >= LoseIfNotEnoughCharactersInEachPlane.Total)
			{
				LoseIfNotEnoughCharactersInEachPlane.ScriptedIsComplete = true;
			}
			break;
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		UpdateTileHighlights();
		if (_tickRoomRevealed.RoomName == "Room_3")
		{
			CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = new Point(8, 9),
				m_Zoom = 0.3f
			};
			ScenarioRuleClient.MessageHandler(message);
			GameState.TriggerCampaignSection("FS_135_Belaras_Keep_Special_1");
		}
		else if (_tickRoomRevealed.RoomName == "Room_5")
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemies.FirstOrDefault((CEnemyActor x) => x.Class.ID == "BelaraID");
			CFocusCamera_MessageData message2 = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = cEnemyActor.ArrayIndex,
				m_Zoom = 0.3f
			};
			ScenarioRuleClient.MessageHandler(message2);
			GameState.TriggerCampaignSection("FS_135_Belaras_Keep_Special_2");
		}
	}

	public static bool IsStandingOnConvergance(CActor actor)
	{
		(Point, Point) tuple = (ScenarioScript.current as ScenarioScript_FH135).ConvergancePoints.SingleOrDefault(((Point, Point) x) => x.Item1 == actor.ArrayIndex || x.Item2 == actor.ArrayIndex);
		if (!(tuple.Item1 != default(Point)))
		{
			return tuple.Item2 != default(Point);
		}
		return true;
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility.Flags.HasFlag(CAbility.EAbilityFlag.FromScenarioModifier) && !_tickAbility.AbilityHasBeenCancelled && _tickAbility.AbilityHasHappened)
		{
			CPlayerActor cPlayerActor = _tickAbility.TargetingActor as CPlayerActor;
			CActor mirrorHealthActor = cPlayerActor.GetMirrorHealthActor();
			CTile tile = mirrorHealthActor.Tile;
			TeleportActor(mirrorHealthActor, cPlayerActor.Tile);
			TeleportActor(cPlayerActor, tile);
			UpdateShieldAttackBonuses(mirrorHealthActor);
			UpdateShieldAttackBonuses(cPlayerActor);
		}
	}

	private bool IsInShieldRoom(CActor actor)
	{
		if (!shieldRooms.Contains(actor.Tile.m_HexMap) && (actor.Tile.m_Hex2Map == null || !shieldRooms.Contains(actor.Tile.m_Hex2Map)))
		{
			if (IsStandingOnConvergance(actor))
			{
				if (!(actor is CPlayerActor))
				{
					return actor.OverrideType == CActor.EType.Player;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private bool IsInAttackRoom(CActor actor)
	{
		if (!attackRooms.Contains(actor.Tile.m_HexMap) && (actor.Tile.m_Hex2Map == null || !attackRooms.Contains(actor.Tile.m_Hex2Map)))
		{
			if (IsStandingOnConvergance(actor))
			{
				if (!(actor is CPlayerActor))
				{
					return actor.OverrideType == CActor.EType.Player;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private void UpdateShieldAttackBonuses(CActor actor)
	{
		if (IsInShieldRoom(actor))
		{
			ApplyScenarioActiveBonusToTickActor(_shieldModifier, actor);
		}
		else
		{
			RemoveScenarioActiveBonusFromActor(_shieldModifier, actor);
		}
		if (IsInAttackRoom(actor))
		{
			ApplyScenarioActiveBonusToTickActor(_attackModifier, actor);
		}
		else
		{
			RemoveScenarioActiveBonusFromActor(_attackModifier, actor);
		}
	}

	private void RemoveScenarioActiveBonusFromActor(CScenarioModifier holder, CActor actor)
	{
		CAbility ability = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0];
		IEnumerable<CActiveBonus> enumerable = from x in actor.FindApplicableActiveBonuses()
			where x.Ability.HasID(ability.ID)
			select x;
		if (!enumerable.Any())
		{
			return;
		}
		foreach (CActiveBonus item in enumerable)
		{
			item.Finish();
		}
		actor.CheckForCachedValuesAfterActiveBonusesUpdate();
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		CActor tickActor = _tickActor;
		CheckConvergance(tickActor);
		CheckPlanePlayerCount();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	protected override void TickRepositionInCharacterPlacementInternal()
	{
		base.TickRepositionInCharacterPlacementInternal();
		CActor tickActor = _tickActor;
		CheckConvergance(tickActor);
		CheckPlanePlayerCount();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void CheckPlanePlayerCount()
	{
		LoseIfNotEnoughCharactersInEachPlane.Current = ((!ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => IsInShieldRoom(x)) || !ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => IsInAttackRoom(x))) ? 1 : 0);
	}

	private void CheckConvergance(CActor actor, bool init = false)
	{
		(Point, Point) tuple = ConvergancePoints.SingleOrDefault(((Point, Point) x) => x.Item1 == actor.ArrayIndex || x.Item2 == actor.ArrayIndex);
		(Point, Point) tuple2 = ConvergancePoints.SingleOrDefault(((Point, Point) x) => x.Item1 == actor.PrevArrayIndex || x.Item2 == actor.PrevArrayIndex);
		(Point, Point) tuple3 = tuple;
		if (tuple3.Item1 != default(Point) || tuple3.Item2 != default(Point))
		{
			CTile tile = ScenarioManager.GetTile(actor.ArrayIndex);
			CTile cTile = ((tuple.Item1 == actor.ArrayIndex) ? ScenarioManager.GetTile(tuple.Item2) : ScenarioManager.GetTile(tuple.Item1));
			if (actor is CPlayerActor actor2 && !init)
			{
				SpawnDamageRedirection(actor2, cTile);
			}
			ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Blocked = tile.LivingActor != null;
			ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex).Blocked = cTile.LivingActor != null;
		}
		else
		{
			tuple3 = tuple2;
			if (tuple3.Item1 != default(Point) || tuple3.Item2 != default(Point))
			{
				CTile tile2 = ScenarioManager.GetTile(actor.PrevArrayIndex);
				CTile cTile2 = ((tuple2.Item1 == actor.ArrayIndex) ? ScenarioManager.GetTile(tuple2.Item2) : ScenarioManager.GetTile(tuple2.Item1));
				if (actor is CPlayerActor cPlayerActor)
				{
					CActor mirrorHealthActor = cPlayerActor.GetMirrorHealthActor();
					if (mirrorHealthActor != null)
					{
						GameState.KillActor(mirrorHealthActor, mirrorHealthActor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _, null, actorWasAsleep: false, null, instantlyDeletePrefab: true, dontLog: true);
						mirrorHealthActor.CacheMirrorProperties();
					}
					actor.ActiveBonuses.FirstOrDefault((CActiveBonus x) => x is CMirrorDamageActiveBonus)?.Finish();
					cPlayerActor.CacheMirrorProperties();
				}
				ScenarioManager.PathFinder.GetNode(tile2.m_ArrayIndex).Blocked = tile2.LivingActor != null;
				ScenarioManager.PathFinder.GetNode(cTile2.m_ArrayIndex).Blocked = cTile2.LivingActor != null;
			}
		}
		UpdateShieldAttackBonuses(actor);
	}

	private void SpawnDamageRedirection(CActor actor, CTile tile)
	{
		CActor cActor = SpawnDamageRedirectionBase(actor, tile);
		ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, actor, cActor);
		ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, cActor, actor);
		UpdateShieldAttackBonuses(cActor);
		cActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		UpdateDamageRedirectionVFX(cActor);
	}

	private void UpdateDamageRedirectionVFX(CActor actor)
	{
		string vFXName = "S135CopyIndicationVFX";
		ScenarioRuleClient.MessageHandler(new CRemoveCasterVFX_MessageData(actor)
		{
			m_VFXName = vFXName
		});
		ScenarioRuleClient.MessageHandler(new CAddCasterVFX_MessageData(actor)
		{
			m_VFXName = vFXName
		});
	}
}
