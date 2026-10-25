using System.Collections.Generic;
using System.Linq;
using Libs.GameConsole;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH029 : ScenarioScript
{
	private CSpawner _alliesSpawner;

	private CSpawner _algoxPriestSpawner;

	private CSpawner _algoxIcespeakerSpawner;

	private CObjectProp[] _corePillars;

	private CObjectProp[] _allPillars;

	private int _corePillarsCount;

	private int _allPillarsCount;

	[ConsoleVariable(Alias = "ss_029_adjacent_pillars_count", Persistent = false)]
	public static int AdjacentPillarsCount = 3;

	private int CorePillarsAliveCount => _corePillars.ToList().FindAll((CObjectProp p) => IsPillarAlive(p)).Count;

	private int AllPillarsAliveCount => _allPillars.ToList().FindAll((CObjectProp p) => IsPillarAlive(p)).Count;

	private int StartScenarioAlliesCount => 6 - ScenarioManager.CurrentScenarioState.Players.Count;

	private bool IsPillarAlive(CObjectProp pillar)
	{
		if (pillar != null)
		{
			return pillar.PropHealthDetails.CurrentHealth > 0;
		}
		return false;
	}

	protected override bool Initialize()
	{
		_ = ScenarioManager.CurrentScenarioState.Props;
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		string prefabName = "OneHexObstacle";
		_corePillars = new CObjectProp[3]
		{
			GetProp(9, 9, prefabName, checkDestroyed: true),
			GetProp(10, 9, prefabName, checkDestroyed: true),
			GetProp(10, 8, prefabName, checkDestroyed: true)
		};
		_allPillars = new CObjectProp[12]
		{
			GetProp(9, 7, prefabName, checkDestroyed: true),
			GetProp(10, 7, prefabName, checkDestroyed: true),
			GetProp(11, 8, prefabName, checkDestroyed: true),
			GetProp(9, 8, prefabName, checkDestroyed: true),
			GetProp(8, 9, prefabName, checkDestroyed: true),
			GetProp(9, 10, prefabName, checkDestroyed: true),
			GetProp(10, 10, prefabName, checkDestroyed: true),
			GetProp(11, 10, prefabName, checkDestroyed: true),
			GetProp(11, 9, prefabName, checkDestroyed: true),
			GetProp(9, 9, prefabName, checkDestroyed: true),
			GetProp(10, 9, prefabName, checkDestroyed: true),
			GetProp(10, 8, prefabName, checkDestroyed: true)
		};
		UpdateCurrentPillarsCount();
		_alliesSpawner = spawners[2];
		_algoxPriestSpawner = spawners[0];
		_algoxIcespeakerSpawner = spawners[1];
		UpdateSpecialRules();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (ScenarioScript.RoundNumber == 1 && OncePerRound())
			{
				SpawnStartScenarioAlliesOnStartingTiles(_alliesSpawner, StartScenarioAlliesCount);
			}
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 77);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			break;
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateSpecialRules();
	}

	protected override void TickRepositionInternal()
	{
		UpdateSpecialRules();
	}

	protected override void TickDeathInternal()
	{
		UpdateSpecialRules();
	}

	private void UpdateSpecialRules()
	{
		UpdatePillarsImmunity();
		CheckSpawnAlgoxAfterKillingPillar();
	}

	private void CheckSpawnAlgoxAfterKillingPillar()
	{
		if (_allPillarsCount != AllPillarsAliveCount)
		{
			((_corePillarsCount != CorePillarsAliveCount) ? _algoxIcespeakerSpawner : _algoxPriestSpawner).SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			UpdateCurrentPillarsCount();
		}
	}

	private void UpdateCurrentPillarsCount()
	{
		_corePillarsCount = CorePillarsAliveCount;
		_allPillarsCount = AllPillarsAliveCount;
	}

	private void UpdatePillarsImmunity()
	{
		CObjectProp[] corePillars = _corePillars;
		foreach (CObjectProp core in corePillars)
		{
			if (IsPillarAlive(core))
			{
				List<CObjectProp> list = _allPillars.ToList().FindAll((CObjectProp p) => IsPillarAlive(p) && ScenarioManager.IsTileAdjacent(core.PropTile, p.PropTile));
				bool isImmuneToDamage = core.IsImmuneToDamage;
				core.IsImmuneToDamage = list.Count > AdjacentPillarsCount;
				ScenarioRuleClient.MessageHandler(new CSetPropChildActive_MessageData
				{
					m_prop = core,
					m_childName = "VFX_P_IceSpire_Shield",
					m_active = core.IsImmuneToDamage
				});
				if (isImmuneToDamage && !core.IsImmuneToDamage)
				{
					ScenarioRuleClient.MessageHandler(new CSetPropChildActive_MessageData
					{
						m_prop = core,
						m_childName = "VFX_P_IceSpire_Shield_Destruction",
						m_active = true
					});
				}
			}
		}
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		return actor.GetPrefabName() switch
		{
			"AlgoxGuard" => "AlgoxGuardSnowspeaker", 
			"AlgoxGuard Elite" => "AlgoxGuardSnowspeaker Elite", 
			"AlgoxArcher" => "AlgoxArcherSnowspeaker", 
			"AlgoxArcher Elite" => "AlgoxArcherSnowspeaker Elite", 
			_ => null, 
		};
	}
}
