using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH034 : ScenarioScript
{
	public static bool ShowChaoticBeams_cvar;

	public static bool SetupNegativeHex_cvar;

	private const float sparkToCharacterRatio = 2f;

	private const string bossClassID = "LordOfChaosID";

	private const string chaosSparkClassID = "ChaosSparkID";

	private CSpawner _aSpawner;

	private CSpawner _bSpawner;

	public bool GetChaoticBeams(out IEnumerable<TileIndex> beams)
	{
		beams = new List<TileIndex>
		{
			new TileIndex(3, 3),
			new TileIndex(3, 4),
			new TileIndex(4, 4),
			new TileIndex(3, 5),
			new TileIndex(3, 6),
			new TileIndex(4, 6),
			new TileIndex(3, 7),
			new TileIndex(3, 8),
			new TileIndex(4, 8)
		};
		return true;
	}

	protected override bool Initialize()
	{
		if (!ChaoticBeamInit())
		{
			return false;
		}
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_aSpawner = spawners[0];
		_aSpawner.SetActive(active: false);
		_bSpawner = spawners[1];
		_bSpawner.SetActive(active: false);
		DisableEnemyLoot();
		return true;
	}

	private int CountChaosSparks()
	{
		List<CEnemyActor> allEnemyMonsters = ScenarioManager.Scenario.AllEnemyMonsters;
		int num = 0;
		for (int i = 0; i < allEnemyMonsters.Count; i++)
		{
			if (allEnemyMonsters[i]?.MonsterClass?.ID == "ChaosSparkID")
			{
				num++;
			}
		}
		return num;
	}

	private void ActivateSpawner(CSpawner spawner)
	{
		if (!((float)CountChaosSparks() >= (float)ScenarioScript.PartySize * 2f))
		{
			spawner.SetActive(active: true, forceDontSpawn: true);
			spawner.SpawnUnit(ScenarioScript.PartySize);
			spawner.SetActive(active: false);
			GameState.SortIntoInitiativeAndIDOrder();
		}
	}

	private void HandleBossSpecial1()
	{
		ActivateSpawner(_bSpawner);
		if (ScenarioScript.PartySize > 3)
		{
			ActivateSpawner(_aSpawner);
		}
	}

	private List<Point> FindBestPathsToChaoticBeams(CActor actor)
	{
		if (!GetChaoticBeams(out var beams))
		{
			return null;
		}
		List<Point> list = new List<Point>();
		IEnumerable<Point> collection = ScenarioManager.Scenario.AllAliveActors.Select((CActor a) => a.ArrayIndex);
		list.AddRange(collection);
		List<(Point, int)> list2 = new List<(Point, int)>();
		foreach (TileIndex item in beams)
		{
			Point point = item.ToPoint();
			if (ScenarioManager.GetTile(point).LivingActor == null)
			{
				int count = CActor.FindCharacterPath(actor, actor.ArrayIndex, point, ignoreBlocked: false, ignoreMoveCost: false, out var _).Count;
				if (count != 0)
				{
					list2.Add((point, count));
				}
			}
		}
		if (list2.Count == 0)
		{
			return new List<Point> { actor.ArrayIndex };
		}
		List<(Point, int)> list3 = list2.OrderBy(((Point, int) x) => x.Item2).ToList();
		int best = list3.First().Item2;
		list3.RemoveAll(((Point, int) x) => x.Item2 != best);
		List<Point> list4 = new List<Point>();
		foreach (var item2 in list3)
		{
			List<Point> collection2 = ScenarioManager.PathFinder.FindAllBestDestinations(actor, item2.Item1, list, addWithFloodedIfEmpty: true);
			list4.AddRange(collection2);
		}
		return list4;
	}

	private CPlayerActor GetClosestPlayerActorToTickActor()
	{
		CPlayerActor result = ScenarioManager.Scenario.PlayerActors[0];
		List<CPlayerActor> list = new List<CPlayerActor>();
		int num = int.MaxValue;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			bool foundPath = false;
			List<Point> list2 = ScenarioManager.PathFinder.FindPath(_tickActor, _tickActor.ArrayIndex, playerActor.ArrayIndex, ignoreBlocked: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false, (from summon in ScenarioManager.Scenario.AllHeroSummons
				where !summon.IsDead
				select summon.ArrayIndex).ToList());
			if (foundPath)
			{
				if (num > list2.Count)
				{
					list.Clear();
					num = list2.Count;
				}
				if (num <= list2.Count)
				{
					list.Add(playerActor);
				}
			}
		}
		if (list.Count == 0)
		{
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				bool foundPath2 = false;
				List<Point> list3 = ScenarioManager.PathFinder.FindPath(_tickActor, _tickActor.ArrayIndex, playerActor2.ArrayIndex, ignoreBlocked: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath2, ignoreDoorLocks: false);
				if (foundPath2)
				{
					if (num > list3.Count)
					{
						list.Clear();
						num = list3.Count;
					}
					if (num <= list3.Count)
					{
						list.Add(playerActor2);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			list.Sort(GameState.s_ActorAdjustedInitiativeComparer);
			result = list.FirstOrDefault();
		}
		return result;
	}

	private void SetSparkAttackFocus()
	{
		if (ScenarioManager.Scenario.PlayerActors.Count != 0 && _tickActor.MonsterClass?.ID == "ChaosSparkID")
		{
			CPlayerActor cPlayerActor = GetClosestPlayerActorToTickActor();
			if (cPlayerActor == null)
			{
				cPlayerActor = ScenarioManager.Scenario.PlayerActors[0];
			}
			ScenarioManager.CurrentScenarioState.Monsters.Find((EnemyState actor) => actor.ActorGuid == _tickActor.ActorGuid).SetAIFocusOverride(new CAIFocusOverrideDetails
			{
				OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor,
				OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus,
				TargetGUID = cPlayerActor.ActorGuid
			});
		}
	}

	private void HandleBossSpecial2()
	{
		if (_tickActor.LastAbilityPerformed.GetVar("First").GetBool())
		{
			ActivateSpawner(_aSpawner);
			if (ScenarioScript.PartySize > 2)
			{
				ActivateSpawner(_bSpawner);
			}
		}
	}

	private void HandleBossAbilities()
	{
		if (!(_tickActor.MonsterClass?.ID != "LordOfChaosID"))
		{
			CAbility lastAbilityPerformed = _tickActor.LastAbilityPerformed;
			if ((lastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special1) != 0)
			{
				HandleBossSpecial1();
			}
			else if ((lastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special2) != 0)
			{
				HandleBossSpecial2();
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAbility)
		{
			HandleBossAbilities();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartTurn)
		{
			SetSparkAttackFocus();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
		{
			DisableEnemyLoot(_tickActor as CEnemyActor);
		}
	}

	private void DisableEnemyLoot()
	{
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			DisableEnemyLoot(enemy);
		}
	}

	private void DisableEnemyLoot(CEnemyActor actor)
	{
		if (actor != null)
		{
			actor.NoGoldDrop = true;
		}
	}

	private void AddVulnerability()
	{
		int num = 1;
		ScenarioManager.Scenario.AllAliveActors.Find((CActor a) => a.ActorGuid == _tickActor.ActorGuid);
		List<CActiveBonus> list = _tickActor.FindApplicableActiveBonuses(CAbility.EAbilityType.ExploitableVulnerability);
		if (list.Count > 0)
		{
			CActiveBonus cActiveBonus = list[0];
			num += cActiveBonus.Ability.Strength;
			cActiveBonus.Finish();
		}
		CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.ExploitableVulnerability, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: true);
		cAbility.Strength = num;
		cAbility.ActiveBonusData.AbilityData = cAbility;
		cAbility.ActiveBonusData.ListLayoutOverride = ("$FH/Scenario_034_ChaosCharge_Desc$", "Negative_ChaosCharge_Campaign");
		cAbility.ActiveBonusData.MiscVars = new List<(string, TValue)> { ("OverrideLayoutName", "$FH/Scenario_034_ChaosCharge_Title$ {0}") };
		cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
		cAbility.Start(_tickActor, _tickActor);
		cAbility.ApplyToActor(_tickActor);
	}

	protected override void TickRepositionInternal()
	{
		if (HasChaoticBeam(_tickActorTile.m_ArrayIndex.X, _tickActorTile.m_ArrayIndex.Y) && (_tickActor.Type == CActor.EType.Player || _tickActor.Type == CActor.EType.HeroSummon))
		{
			DealScenarioDamage(_tickActor, CObjectHazardousTerrain.GetTerrainDamage(), isTrapDamage: false, isTerrainDamage: true);
			Gain<CAbilityMuddle>(_tickActor, CAbility.EAbilityType.Muddle);
			Gain<CAbilityCurse>(_tickActor, CAbility.EAbilityType.Curse);
			AddVulnerability();
			CCombatLogMessage_MessageData cCombatLogMessage_MessageData = new CCombatLogMessage_MessageData();
			cCombatLogMessage_MessageData.m_MessageKey = "FH_Scenario_Campaign_034_Chaos_Beam_Event_Log";
			cCombatLogMessage_MessageData.m_Params = new List<string>
			{
				"Actor_0",
				CObjectHazardousTerrain.GetTerrainDamage().ToString()
			};
			cCombatLogMessage_MessageData.m_Actors = new List<CActor> { _tickActor };
			ScenarioRuleClient.MessageHandler(cCombatLogMessage_MessageData);
		}
	}

	private bool ChaoticBeamInit()
	{
		if (!GetChaoticBeams(out var beams))
		{
			return false;
		}
		foreach (TileIndex item in beams)
		{
			CObjectProp cObjectProp = ScenarioManager.Tiles[item.X, item.Y].FindPropOfTypes(EPropType.TerrainVisualEffect);
			if (cObjectProp?.PropHealthDetails != null)
			{
				cObjectProp.PropHealthDetails.CustomLocKey = "FH_Scenario_Campaign_024_SCENARIO_OBJECT_1";
			}
		}
		return true;
	}

	private bool HasChaoticBeam(int x, int y)
	{
		if (GetChaoticBeams(out var beams))
		{
			return beams.FirstOrDefault((TileIndex b) => b.X == x && b.Y == y) != null;
		}
		return false;
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		if (_tickAbility.AbilityType != CAbility.EAbilityType.Move || !_tickAbility.Flags.HasFlag(CAbility.EAbilityFlag.Special2))
		{
			return;
		}
		CAbilityMove cAbilityMove = _tickAbility as CAbilityMove;
		List<CTile> list = (from x in FindBestPathsToChaoticBeams(cAbilityMove.TargetingActor)
			select ScenarioManager.GetTile(x)).ToList();
		if (list.Count > 0)
		{
			cAbilityMove.MoveRestrictionType = CAbilityMove.EMoveRestrictionType.MustEndOnSpecificTiles;
			cAbilityMove.SetMiscVar("MustEndOnSpecificTiles", new TValue(list));
			bool flag = false;
			if (list.Contains(cAbilityMove.TargetingActor.Tile))
			{
				flag = true;
			}
			cAbilityMove.MiscAbilityData.CanSkip = flag;
			cAbilityMove.SetCanSkip(flag);
		}
		else
		{
			cAbilityMove.MiscAbilityData.CanSkip = true;
			cAbilityMove.SetCanSkip(canSkip: true);
		}
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("max_chaos_sparks_count", ((float)ScenarioScript.PartySize * 2f).ToString());
	}
}
