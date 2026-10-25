using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH032 : ScenarioScript
{
	private readonly string corpseClassID = "FrozenCorpseID";

	private readonly string fiendClassID = "ShrikeFiendEliteID";

	private CSpawner _Spawner;

	private CScenarioModifier _specialRule1;

	[StateDump(Skip = true)]
	private bool roomRevealed;

	private CScenarioModifier _ModifierHiddenLocalizaion => ScenarioScript._css.ScenarioModifiers.FirstOrDefault((CScenarioModifier sm) => sm.EventIdentifier == "DoorLocalizaion");

	protected override bool Initialize()
	{
		_Spawner = ScenarioManager.CurrentScenarioState.Spawners[0];
		_specialRule1 = GetScenarioModifier("specialRule1");
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickPhaseRoomRevealed();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 46);
			break;
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (!(_tickActor.MonsterClass?.ID != corpseClassID) && actualDamage > 0)
		{
			FindMutateBonus(_tickActor)?.Finish();
		}
	}

	private void TickStartTurn()
	{
	}

	private void TickEndTurn()
	{
	}

	private void TickPhaseRoomRevealed()
	{
		if (!roomRevealed)
		{
			roomRevealed = true;
			_ModifierHiddenLocalizaion.SetHiddenState(shouldSetHidden: false);
			_Spawner.SetActive(active: true);
			_Spawner.OnStartRound(ScenarioScript.PartySize, ScenarioScript.RoundNumber);
			_Spawner.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		}
	}

	public static CActiveBonus FindMutateBonus(CActor actor)
	{
		return actor.FindApplicableActiveBonuses(CAbility.EAbilityType.NullTargeting).FirstOrDefault((CActiveBonus x) => x.Ability.GetAbilityBaseCard() is CScenarioModifier cScenarioModifier && cScenarioModifier.ScenarioAbilityID == "ScenarioAbility_FH32_Mutate");
	}

	private void TickEndRound()
	{
		if (!_tickActor.IsDead && FindMutateBonus(_tickActor) != null)
		{
			GameState.KillActor(_tickActor, _tickActor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _);
			CSpawner.SpawnUnitAtTile(ScenarioManager.CurrentScenarioState.Players.Count, CActor.EType.Enemy, fiendClassID, _tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y);
			UpdateRules();
		}
	}

	protected override void TickDeathInternal()
	{
		UpdateRules();
	}

	private void UpdateRules()
	{
		if (_specialRule1 != null && !_specialRule1.IsHidden && ScenarioManager.Scenario.Enemies.Count((CEnemyActor e) => !e.IsDead && e.MonsterClass.ID.Contains("FrozenCorpse")) == 0)
		{
			_specialRule1.Hide();
		}
	}
}
