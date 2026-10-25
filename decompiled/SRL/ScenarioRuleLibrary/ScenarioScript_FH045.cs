using System.Linq;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH045 : ScenarioScript
{
	private CObjective_Scripted _DownedHeroOutsideShipLoseObjective;

	private CObjective_Scripted _AllHeroesOnShipWinObjective;

	private CObjective_Scripted _NoEnemiesOnShipWinObjective;

	private CObjectProp heartOfTheGlacier;

	private bool _showNarrativeEvent;

	protected override bool Initialize()
	{
		_DownedHeroOutsideShipLoseObjective = (CObjective_Scripted)GetObjective("DownedHeroOutsideShip");
		_AllHeroesOnShipWinObjective = (CObjective_Scripted)GetObjective("AllHeroesOnShip");
		_NoEnemiesOnShipWinObjective = (CObjective_Scripted)GetObjective("NoEnemiesOnShip");
		heartOfTheGlacier = ScenarioScript.FindPropWithHealth(7, 20);
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			spawner.SetActive(active: false);
		}
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		TryActivateFinalObjectives();
	}

	private bool IsOnShip(CActor actor)
	{
		return actor.ArrayIndex.Y <= 9;
	}

	private void CheckWinConditions()
	{
		if (_AllHeroesOnShipWinObjective.IsActive)
		{
			_AllHeroesOnShipWinObjective.ScriptedIsComplete = !ScenarioManager.CurrentScenarioState.Players.Any((PlayerState p) => !p.IsDead && !IsOnShip(p.Actor));
		}
		if (_NoEnemiesOnShipWinObjective.IsActive)
		{
			_NoEnemiesOnShipWinObjective.ScriptedIsComplete = !ScenarioManager.CurrentScenarioState.AllEnemyStates.Any((EnemyState e) => IsAlive(e) && IsOnShip(e.Actor));
		}
		CheckObjectivesComplete();
	}

	protected override void TickPhaseInternal()
	{
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAbility)
		{
			CheckWinConditions();
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor.Type == CActor.EType.Player && !IsOnShip(_tickActor))
		{
			_DownedHeroOutsideShipLoseObjective.ScriptedIsComplete = true;
			CheckObjectivesComplete();
		}
		TryActivateFinalObjectives();
	}

	private void TryActivateFinalObjectives()
	{
		if (heartOfTheGlacier.IsDestroyed && !_AllHeroesOnShipWinObjective.IsActive)
		{
			_AllHeroesOnShipWinObjective.SetActivation(active: true);
			_NoEnemiesOnShipWinObjective.SetActivation(active: true);
			_showNarrativeEvent = true;
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				spawner.SetActive(active: true, forceDontSpawn: true);
				spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
				spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
				spawner.SetActive(active: false);
			}
		}
		CheckWinConditions();
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (_showNarrativeEvent && message.MessageName == "FS_045_NarrativeMessage")
		{
			_showNarrativeEvent = false;
			return true;
		}
		return base.AllowTriggerMessageInternal(ev, message);
	}
}
