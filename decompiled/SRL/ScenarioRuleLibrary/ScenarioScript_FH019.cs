using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH019 : ScenarioScript
{
	[ConsoleVariable(Alias = "ss19_altars_immune_rounds", Persistent = false)]
	public static int AltarsImmuneStartRound = 10;

	private static int DARK_ALTARS_COUNT = 6;

	private static int shake_interval = 3;

	private static int[] _abRounds = new int[2] { 6, 9 };

	private static int[] _cdRounds = new int[4] { 4, 5, 7, 8 };

	private CSpawner[] _aSpawners;

	private CSpawner[] _bSpawners;

	private CSpawner[] _cSpawners;

	private CSpawner[] _dSpawners;

	private List<CObjectProp> _altars;

	private CScenarioModifier _specialRule1;

	private CScenarioModifier _specialRule2;

	private int _FallingIceEndRound = -1;

	public const ESpecificPropType DarkCrystalPropType = ESpecificPropType.DarkCrystalExposed;

	public static readonly Point[] iceTiles = new Point[6]
	{
		new Point(5, 7),
		new Point(6, 9),
		new Point(6, 5),
		new Point(8, 9),
		new Point(8, 5),
		new Point(9, 7)
	};

	protected override bool Initialize()
	{
		List<CObjectProp> props = ScenarioManager.CurrentScenarioState.Props;
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_aSpawners = new CSpawner[2]
		{
			spawners[4],
			spawners[8]
		};
		_bSpawners = new CSpawner[1] { spawners[5] };
		_cSpawners = new CSpawner[1] { spawners[6] };
		_dSpawners = new CSpawner[1] { spawners[7] };
		_altars = props.FindAll((CObjectProp p) => p.PropHealthDetails != null && !p.PrefabName.Equals("DarkCrystalExposed"));
		_altars = _altars.OrderBy((CObjectProp a) => a.PropTile.m_ArrayIndex.ToString()).ToList();
		_specialRule1 = GetScenarioModifier("SpecialRule1");
		_specialRule2 = GetScenarioModifier("SpecialRule2");
		ToggleAltarsImmuneToDamage(ScenarioScript.RoundNumber < AltarsImmuneStartRound);
		InitializeDarkFogObjective();
		UpdateDarkFogsObjective();
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_FallingIceEndRound);
		Trace("[fs019]Serialize: " + text + "[-]", null, 59);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[fs019]Deserialize " + s + "[-]", null, 64);
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		_FallingIceEndRound = JsonConvert.DeserializeObject<int>(s);
		CheckObjectivesComplete();
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PrefabName == "DarkCrystalExposed"))
		{
			if (item.RuntimeAttachedActor != null && !item.RuntimeAttachedActor.IsDead)
			{
				ApplyInitiativeFocusBuff(item.RuntimeAttachedActor);
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 98);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		UpdateDarkFogsObjective();
	}

	protected override void TickDeathInternal()
	{
		DestroyKilledAltarsProps();
	}

	private void UpdateFallingIce()
	{
		if (ScenarioScript.RoundNumber % shake_interval != 0 || ScenarioScript.RoundNumber == _FallingIceEndRound)
		{
			return;
		}
		_FallingIceEndRound = ScenarioScript.RoundNumber;
		if (ScenarioScript.RoundNumber == 9)
		{
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
			{
				duration = 6f
			});
		}
		else
		{
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
			{
				duration = 4f,
				magnitude = 0.1f,
				speed = 50f,
				distanceForce = 50f
			});
		}
		int num = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, iceTiles.Length);
		for (int i = 0; i < iceTiles.Length; i++)
		{
			Point point = iceTiles[(i + num) % iceTiles.Length];
			CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
			bool flag = false;
			if (cTile.m_Props.Count > 0)
			{
				foreach (CObjectProp item in cTile.m_Props.ToList())
				{
					if (item.PrefabName == "IceFall")
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				CObjectProp cObjectProp = new CObjectProp("IceFall", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(point), null, null, null, cTile.m_HexMap.MapGuid);
				cTile.SpawnProp(cObjectProp);
				Trace("Spawned prop " + cObjectProp.PrefabName, null, 148);
				break;
			}
		}
	}

	private void TickPhaseEndRound()
	{
		UpdateDarkFogsObjective();
		UpdateFallingIce();
	}

	private void TickPhaseStartRound()
	{
		if (!OncePerRound())
		{
			return;
		}
		HideAllSpawnersIndicators();
		ProcessSpawnerForRound(ScenarioScript.RoundNumber, (CSpawner[] spawners) =>
		{
			SpawnSpawners(spawners);
		});
		ProcessSpawnerForRound(ScenarioScript.RoundNumber + 1, (CSpawner[] spawners) =>
		{
			ShowSpawnersIndicators(spawners);
		});
		ToggleAltarsImmuneToDamage(ScenarioScript.RoundNumber < AltarsImmuneStartRound);
		if (ScenarioScript.RoundNumber == 1)
		{
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
			{
				duration = 5f
			});
		}
		else if (ScenarioScript.RoundNumber == AltarsImmuneStartRound)
		{
			_specialRule1.Hide();
			_specialRule2.Show();
			if (_altars.Count == DARK_ALTARS_COUNT)
			{
				OfferSpawnAltar(GameState.HostCharacter);
			}
		}
	}

	private void OfferSpawnAltar(CPlayerActor actor)
	{
		List<CTile> validTiles = (from x in _altars
			where x.RuntimeAttachedActor != null && !x.RuntimeAttachedActor.IsDead
			select x.PropTile).ToList();
		ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_019_SpawnDarkFog_HelpText$", actor, validTiles, SpawnAltarCallback, 1, animateActor: false, null, canSkip: false, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: true, ignoreDoorLocks: false, stackAsSeparateAbilities: false, autoTrigger: false, null, interruptCurrentAbility: false, tryInject: false, null, ignoreStun: true);
	}

	private void SpawnAltarCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		CObjectProp cObjectProp = _altars.SingleOrDefault((CObjectProp x) => selectedTiles.Contains(x.PropTile));
		if (cObjectProp != null)
		{
			cObjectProp.RuntimeAttachedActor.Damaged(cObjectProp.RuntimeAttachedActor.Health, fromAttackAbility: false, null, null);
			GameState.KillActor(cObjectProp.RuntimeAttachedActor, cObjectProp.RuntimeAttachedActor, CActor.ECauseOfDeath.Suicide, out var _);
			DestroyKilledAltarsProps();
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateDarkFogsObjective();
	}

	private void InitializeDarkFogObjective()
	{
		CObjective_Scripted obj = ScenarioManager.CurrentScenarioState.AllObjectives[0] as CObjective_Scripted;
		obj.Total = DARK_ALTARS_COUNT;
		obj.Current = 0;
		obj.ScriptedIsComplete = false;
	}

	private CObjectProp SpawnFogForAltar(CObjectProp oldProp)
	{
		CTile propTile = oldProp.PropTile;
		TileIndex tileIndex = new TileIndex(propTile.m_ArrayIndex);
		CObjectObstacle cObjectObstacle = new CObjectObstacle(ESpecificPropType.DarkCrystalExposed.ToString(), ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, oldProp.Rotation, new List<TileIndex> { tileIndex }, null, propTile.m_HexMap.MapGuid, ignoresFlyAndJump: false);
		cObjectObstacle.PropHealthDetails = new PropHealthDetails();
		cObjectObstacle.PropHealthDetails.DeathAction = PropHealthDetails.EDeathAction.Destruct;
		cObjectObstacle.PropCustomLocKey = "FH/FH_Scenario_Campaign_19_SCENARIO_OBJECT_1";
		cObjectObstacle.PropHealthDetails.CustomLocKey = "FH/FH_Scenario_Campaign_19_SCENARIO_OBJECT_1";
		cObjectObstacle.PropCustomDescriptionLocKey = "FH_19_Dark_Crystal_D";
		cObjectObstacle.PropHealthDetails.HealthBasedOnXType = CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX;
		cObjectObstacle.PropHealthDetails.HealthBasedOnXVariable = 2f;
		cObjectObstacle.PropHealthDetails.HasHealth = true;
		propTile.SpawnProp(cObjectObstacle);
		SpawnDummyActorOnProp(tileIndex.X, tileIndex.Y, cObjectObstacle, cObjectObstacle.PropHealthDetails.GetPropStartingHealth(), active: true);
		CObjectActor runtimeAttachedActor = cObjectObstacle.RuntimeAttachedActor;
		CActor.EType type = (runtimeAttachedActor.OriginalType = CActor.EType.Enemy2);
		runtimeAttachedActor.Type = type;
		ApplyInitiativeFocusBuff(runtimeAttachedActor);
		CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
		{
			m_SpawnTile = propTile,
			m_SpawnEnemy = runtimeAttachedActor,
			m_ActorSpawningMessage = runtimeAttachedActor
		};
		ScenarioRuleClient.MessageHandler(message);
		return cObjectObstacle;
	}

	private void ApplyInitiativeFocusBuff(CActor actor)
	{
		if (!actor.FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.FocusInitiative).Any())
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.AdjustInitiative, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: false, 1);
			cAbility.ActiveBonusData.Hidden = true;
			cAbility.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.FocusInitiative;
			cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private void UpdateDarkFogsObjective()
	{
		CObjective_Scripted cObjective_Scripted = ScenarioManager.CurrentScenarioState.AllObjectives[0] as CObjective_Scripted;
		IEnumerable<ObjectState> source = ScenarioManager.CurrentScenarioState.Objects.Where((ObjectState df) =>
		{
			CObjectActor cObjectActor = df.Object;
			return cObjectActor != null && cObjectActor.AttachedProp?.SpecificPropType == ESpecificPropType.DarkCrystalExposed && df.Actor.IsDead;
		});
		cObjective_Scripted.Current = source.Count();
		cObjective_Scripted.ScriptedIsComplete = cObjective_Scripted.Current >= cObjective_Scripted.Total;
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void DestroyKilledAltarsProps()
	{
		List<CObjectProp> list = _altars.ToList();
		foreach (CObjectProp item in list)
		{
			if (item.PropHealthDetails.HasHealth && item.PropHealthDetails.CurrentHealth <= 0)
			{
				ReplaceAltarWithDarkFog(item);
			}
		}
		list.Clear();
	}

	private void ReplaceAltarWithDarkFog(CObjectProp altar)
	{
		_altars.Remove(altar);
		SpawnFogForAltar(altar);
	}

	private void SpawnSpawners(CSpawner[] spawners)
	{
		for (int i = 0; i < spawners.Length; i++)
		{
			spawners[i].SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
		}
	}

	private void ShowSpawnersIndicators(CSpawner[] spawners)
	{
		for (int i = 0; i < spawners.Length; i++)
		{
			spawners[i].UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		}
	}

	private void HideAllSpawnersIndicators()
	{
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			spawner.HideSpawnerProp();
		}
	}

	private void ProcessSpawnerForRound(int roundNumber, Action<CSpawner[]> spawnersAction)
	{
		if (_abRounds.ToList().Contains(roundNumber))
		{
			spawnersAction(_aSpawners);
			spawnersAction(_bSpawners);
		}
		else if (_cdRounds.ToList().Contains(roundNumber))
		{
			spawnersAction(_cSpawners);
			spawnersAction(_dSpawners);
		}
	}

	private void ToggleAltarsImmuneToDamage(bool isImmune)
	{
		foreach (CObjectProp altar in _altars)
		{
			altar.IsImmuneToDamage = isImmune;
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Phase)
		{
			return false;
		}
		if (_phase == EScenarioModifierTriggerPhase.EndRound && ev.Round == 9)
		{
			return true;
		}
		return false;
	}
}
