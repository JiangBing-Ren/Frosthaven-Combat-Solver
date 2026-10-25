using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_RFS007 : ScenarioScript
{
	[Serializable]
	public class StorageRFH007
	{
		public int NextBrittleRound;

		public int RemainingReinforcements;

		public int TransferDamageTrackerIndex;
	}

	private CEnemyActor boss;

	private CSpawner spawner1;

	private CSpawner spawner2;

	private CObjective_Scripted reinforcementsObjective;

	private CScenarioModifier transferDamageHolder;

	private StorageRFH007 serializedProperties;

	private int RemainingReinforcements
	{
		get
		{
			return serializedProperties.RemainingReinforcements;
		}
		set
		{
			serializedProperties.RemainingReinforcements = value;
		}
	}

	private int NextBrittleRound
	{
		get
		{
			return serializedProperties.NextBrittleRound;
		}
		set
		{
			serializedProperties.NextBrittleRound = value;
		}
	}

	protected override void DeserializeInternal(string storage)
	{
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (StorageRFH007)JsonConvert.DeserializeObject(storage, typeof(StorageRFH007));
		}
		if (reinforcementsObjective != null)
		{
			reinforcementsObjective.CustomLocKeyArgs = new List<object> { RemainingReinforcements };
		}
		if (transferDamageHolder.ActiveBonuses != null)
		{
			foreach (CActiveBonus activeBonuse in transferDamageHolder.ActiveBonuses)
			{
				activeBonuse.Ability.ActiveBonusData.Tracker = Enumerable.Repeat(0, ScenarioScript.PartySize).ToList();
				if (serializedProperties != null)
				{
					activeBonuse.TrackerIndex = serializedProperties.TransferDamageTrackerIndex;
				}
				else
				{
					activeBonuse.TrackerIndex = 0;
				}
			}
		}
		ScenarioManager.CurrentScenarioState.UpdateScenarioModifiersDescriptions();
	}

	protected override string SerializeInternal()
	{
		if (transferDamageHolder.ActiveBonuses != null)
		{
			foreach (CActiveBonus activeBonuse in transferDamageHolder.ActiveBonuses)
			{
				serializedProperties.TransferDamageTrackerIndex = activeBonuse.TrackerIndex;
			}
		}
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override bool Initialize()
	{
		spawner1 = ScenarioScript.FindSpawner(2, 8);
		spawner2 = ScenarioScript.FindSpawner(9, 8);
		serializedProperties = new StorageRFH007();
		boss = ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList()[0];
		reinforcementsObjective = GetObjective<CObjective_Scripted>("ReinforcementsModifier");
		transferDamageHolder = GetScenarioModifier("TransferDamageHolder");
		return true;
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			StartScenario();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			StartRound();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			RefreshBossVFX();
			break;
		}
	}

	private void StartScenario()
	{
		RemainingReinforcements = ScenarioScript.PartySize * 2;
		NextBrittleRound = 2;
		FHScenarioSetup scenarioSetup = ScenarioScript._css.ScenarioSetup;
		if (scenarioSetup.ParseScenariosConclusionParameters(out var all) && scenarioSetup.GetConclusionParameters(206u, all, out var result) && result.Count == 1)
		{
			long num = (long)result[0];
			RemainingReinforcements += (int)Math.Floor((float)num / 4f);
		}
		UpdateScoutSpawners();
	}

	private void StartRound()
	{
		ApplyTransferDamageToBoss();
		ApplyBrittle();
		UpdateScoutSpawners();
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		UpdateScoutSpawners();
		RefreshBossVFX();
	}

	private void RefreshBossVFX()
	{
		if (boss == null || _tickActor == null)
		{
			return;
		}
		foreach (CTransferDamageActiveBonus item in boss.FindApplicableActiveBonuses(CAbility.EAbilityType.TransferDamage))
		{
			CAbilityFilter cAbilityFilter = item.Ability.GetMiscVar("TransferFilter").GetObject<CAbilityFilter>();
			if (cAbilityFilter != null)
			{
				CActor tickActor = _tickActor;
				CEnemyActor self = boss;
				CActiveBonus activeBonus = item;
				if (!cAbilityFilter.IsValidTarget(tickActor, self, isTargetedAbility: false, useTargetOriginalType: false, false, skipUntargetableCheck: false, activeBonus))
				{
					continue;
				}
			}
			item.RefreshPersistentVFX();
		}
	}

	private void ApplyBrittle()
	{
		if (ScenarioManager.CurrentScenarioState.RoundNumber == NextBrittleRound)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "ChillingWind",
				m_ArrayIndex = new Point(1, 9),
				m_faceTowards = new Point(1, 11)
			});
			NextBrittleRound = ScenarioManager.CurrentScenarioState.RoundNumber + 2;
			ApplyBrittle(ScenarioManager.Scenario.AllAliveActors);
		}
	}

	private void ApplyBrittle(List<CActor> actors)
	{
		if (actors.Count != 0)
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Brittle, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.All), isMonster: false, isTargetedAbility: true, 0, 1, 1, 0, CAbility.EAbilityFlag.None, 1, EConditionDecTrigger.Never);
			cAbility.IsScenarioModifierAbility = true;
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.SkipAnim = true;
			cAbility.SetMiscVar("IsMapCondition", true);
			cAbility.NumberTargets = -1;
			cAbility.Targeting = CAbility.EAbilityTargeting.Custom;
			List<object> list = new List<object>();
			list.AddRange(actors);
			cAbility.TargetingVars = new List<(string, TValue)> { ("AddValidActorsInRange", new TValue(list)) };
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, boss));
		}
	}

	private void UpdateScoutSpawners()
	{
		int count = ScenarioManager.Scenario.AllAliveMonsters.FindAll((CEnemyActor e) => IsVermlingScout(e) && !e.IsDead).Count;
		spawner1.SetActive(RemainingReinforcements >= 1 && boss != null && !boss.IsDead && count < ScenarioScript.PartySize * 2, forceDontSpawn: true);
		spawner2.SetActive(RemainingReinforcements >= 2 && boss != null && !boss.IsDead && count + 1 < ScenarioScript.PartySize * 2, forceDontSpawn: true);
		if (reinforcementsObjective != null)
		{
			reinforcementsObjective.CustomLocKeyArgs = new List<object> { RemainingReinforcements };
			ScenarioManager.CurrentScenarioState.UpdateScenarioModifiersDescriptions();
		}
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		base.OnSpawnerHasSpawned(spawner, actor);
		RemainingReinforcements--;
		if ((spawner1.HasSpawned && spawner2.HasSpawned) || (spawner == spawner1 && !spawner2.IsActive) || (spawner == spawner2 && !spawner1.IsActive))
		{
			UpdateScoutSpawners();
		}
		if (ScenarioManager.CurrentScenarioState.RoundNumber == NextBrittleRound - 2)
		{
			ApplyBrittle(new List<CActor> { actor });
		}
	}

	private bool IsVermlingScout(CActor actor)
	{
		if (actor == null || actor.MonsterClass?.ID == null)
		{
			return false;
		}
		if (actor.MonsterClass.ID != "VermlingScoutID" && actor.MonsterClass.ID != "VermlingScoutEliteID")
		{
			return false;
		}
		return true;
	}

	private void ApplyTransferDamageToBoss()
	{
		if (_tickActor != boss)
		{
			return;
		}
		RemoveScenarioAbilityFromActor(transferDamageHolder, boss);
		AddScenarioAbilityToActor(transferDamageHolder, boss);
		foreach (CActiveBonus activeBonuse in transferDamageHolder.ActiveBonuses)
		{
			activeBonuse.Ability.ActiveBonusData.Tracker = Enumerable.Repeat(0, ScenarioScript.PartySize).ToList();
			activeBonuse.TrackerIndex = 0;
		}
	}

	public void IncreaseReinforcements()
	{
		if (ScenarioScript.PartySize <= 2)
		{
			RemainingReinforcements++;
		}
		else
		{
			RemainingReinforcements += 2;
		}
		UpdateScoutSpawners();
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("damage_redirect_count", ScenarioScript.PartySize.ToString());
	}
}
