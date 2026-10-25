using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH070 : ScenarioScript
{
	private Point oakPartOneLocation = new Point(5, 10);

	private Point oakPartTwoLocation = new Point(5, 6);

	private CObjectActor oakPartOne;

	private CObjectActor oakPartTwo;

	private CScenarioModifier mirrorDamageModifier;

	private CObjective_Scripted loseConditionOakDead;

	private CScenarioModifier scenarioAbilityOakInitiativeBuff;

	private CScenarioModifier scenarioAbilityOakAttackModifierBuff;

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
		{
			OnOakRevealed();
		}
		else
		{
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 29);
		}
	}

	protected override bool Initialize()
	{
		oakPartOne = ScenarioManager.Scenario.FindObjectActorAt(oakPartOneLocation);
		oakPartTwo = ScenarioManager.Scenario.FindObjectActorAt(oakPartTwoLocation);
		scenarioAbilityOakInitiativeBuff = GetScenarioModifier("scenarioAbilityOakInitiativeBuff");
		scenarioAbilityOakAttackModifierBuff = GetScenarioModifier("scenarioAbilityOakAttackModifierBuff");
		mirrorDamageModifier = GetScenarioModifier("mirrorDamageModifier");
		loseConditionOakDead = (CObjective_Scripted)GetObjective("loseCondition");
		TwinDoors();
		if (scenarioAbilityOakInitiativeBuff.IsHidden)
		{
			ScenarioManager.CurrentScenarioState.Spawners[0].SetActive(active: false);
			ScenarioManager.CurrentScenarioState.Spawners[0].HideSpawnerProp(forced: true);
		}
		return true;
	}

	private void TwinDoors()
	{
		CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.First((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.ArrayIndex.ToPoint() == new Point(6, 12));
		CObjectDoor cObjectDoor2 = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.First((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.ArrayIndex.ToPoint() == new Point(7, 12));
		CObjectDoor cObjectDoor3 = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.First((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.ArrayIndex.ToPoint() == new Point(2, 8));
		CObjectDoor cObjectDoor4 = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.First((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.ArrayIndex.ToPoint() == new Point(3, 8));
		CObjectDoor cObjectDoor5 = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.First((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.ArrayIndex.ToPoint() == new Point(7, 8));
		CObjectDoor cObjectDoor6 = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.First((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Door && p.ArrayIndex.ToPoint() == new Point(8, 8));
		cObjectDoor.TwinDoors = new CObjectDoor[1] { cObjectDoor2 };
		cObjectDoor2.TwinDoors = new CObjectDoor[1] { cObjectDoor };
		cObjectDoor3.TwinDoors = new CObjectDoor[3] { cObjectDoor4, cObjectDoor5, cObjectDoor6 };
		cObjectDoor4.TwinDoors = new CObjectDoor[3] { cObjectDoor3, cObjectDoor5, cObjectDoor6 };
		cObjectDoor5.TwinDoors = new CObjectDoor[3] { cObjectDoor3, cObjectDoor4, cObjectDoor6 };
		cObjectDoor6.TwinDoors = new CObjectDoor[3] { cObjectDoor3, cObjectDoor5, cObjectDoor5 };
	}

	private void OnOakRevealed()
	{
		if (_tickActor is CObjectActor cObjectActor)
		{
			if (cObjectActor.ArrayIndex == oakPartOneLocation)
			{
				oakPartOne = cObjectActor;
				ApplyScenarioActiveBonusToTickActor(scenarioAbilityOakInitiativeBuff, oakPartOne);
				ApplyScenarioActiveBonusToTickActor(scenarioAbilityOakAttackModifierBuff, oakPartOne);
				scenarioAbilityOakAttackModifierBuff.SetHiddenState(shouldSetHidden: false);
			}
			if (cObjectActor.ArrayIndex == oakPartTwoLocation)
			{
				oakPartTwo = cObjectActor;
				UpdateOakHealth(oakPartTwo, oakPartOne.Health);
				ApplyScenarioActiveBonusToTickActor(scenarioAbilityOakInitiativeBuff, oakPartTwo);
				ApplyScenarioActiveBonusToTickActor(scenarioAbilityOakAttackModifierBuff, oakPartTwo);
				ApplyScenarioActiveBonusToTickActor(mirrorDamageModifier, oakPartOne, oakPartTwo);
				ApplyScenarioActiveBonusToTickActor(mirrorDamageModifier, oakPartTwo, oakPartOne);
				scenarioAbilityOakInitiativeBuff.SetHiddenState(shouldSetHidden: false);
				ScenarioManager.CurrentScenarioState.Spawners[0].SetActive(active: true);
				ScenarioManager.CurrentScenarioState.Spawners[0].OnStartRound(ScenarioScript.PartySize, ScenarioScript.RoundNumber);
				ScenarioManager.CurrentScenarioState.Spawners[0].UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			}
		}
	}

	private new void ApplyScenarioActiveBonusToTickActor(CScenarioModifier holder, CActor actor, CActor targetActor = null)
	{
		CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
		cAbility.IsScenarioModifierAbility = true;
		if (targetActor != null)
		{
			cAbility.TargetThisActorAutomatically = targetActor;
		}
		cAbility.Start(actor, actor);
		cAbility.ApplyToActor(targetActor ?? actor);
	}

	private void UpdateOakHealth(CActor actor, int health)
	{
		if (actor != null)
		{
			actor.Health = health;
			CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(actor, actor, actor.Health);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor?.MonsterClass?.ID == "CityGuardSwordOfJusticeID")
		{
			ScenarioManager.CurrentScenarioState.Spawners[0].SetActive(active: false);
			ScenarioManager.CurrentScenarioState.Spawners[0].HideSpawnerProp(forced: true);
			scenarioAbilityOakInitiativeBuff.SetHiddenState(shouldSetHidden: true);
		}
		if (_tickActor == oakPartOne || _tickActor == oakPartTwo)
		{
			loseConditionOakDead.ScriptedIsComplete = true;
			CheckObjectivesComplete();
		}
	}
}
