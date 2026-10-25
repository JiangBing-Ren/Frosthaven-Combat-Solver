using System;
using System.Collections.Generic;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH055 : ScenarioScript
{
	private static Point BossPosition = new Point(12, 5);

	[ConsoleVariable(Alias = "fh055_frozenfist_unlocked")]
	private static bool FrozenfistUnlocked;

	private CObjectDoor _door;

	private bool _bossSpawned;

	private uint scenario004ConclusionIndex;

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.AfterAbility:
			TickAfterAbility();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 50);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override bool Initialize()
	{
		_door = GetProp(9, 4, "ThinDoor") as CObjectDoor;
		List<string> unlockedClassIDs = ScenarioManager.CurrentScenarioState.ScenarioSetup.UnlockedClassIDs;
		if (unlockedClassIDs != null)
		{
			FrozenfistUnlocked = unlockedClassIDs.Contains("FrozenFistID");
		}
		FHScenarioSetup scenarioSetup = ScenarioScript._css.ScenarioSetup;
		if (scenarioSetup.ParseScenariosConclusionParameters(out var all))
		{
			scenarioSetup.GetConclusionIndex(4u, all, out scenario004ConclusionIndex);
		}
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_bossSpawned);
		return JsonConvert.SerializeObject(new string[1] { text });
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_bossSpawned = JsonConvert.DeserializeObject<bool>(array[0]);
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_door.DoorIsOpen && !_bossSpawned && OncePerRound())
		{
			string cls = (FrozenfistUnlocked ? "AlgoxSnowDancer55ID" : "AlgoxFrozenFist55ID");
			SpawnActorAtTile(BossPosition.X, BossPosition.Y, cls, out var _);
			_bossSpawned = true;
		}
	}

	private void TickAfterAbility()
	{
		if (_tickActor.Type == CActor.EType.Enemy)
		{
			ConsumeElementIfAvailable("Ice");
		}
	}

	public void ConsumeElementIfAvailable(string elementName)
	{
		if (Enum.TryParse<ElementInfusionBoardManager.EElement>(elementName, out var result) && ElementInfusionBoardManager.GetAvailableElements().Contains(result))
		{
			ElementInfusionBoardManager.Consume(result, GameState.CurrentActor);
		}
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		if (scenario004ConclusionIndex != 0)
		{
			return null;
		}
		return actor.GetPrefabName() switch
		{
			"AlgoxGuard" => "AlgoxGuardSnowspeaker", 
			"AlgoxGuard Elite" => "AlgoxGuardSnowspeaker Elite", 
			"AlgoxPriest" => "AlgoxPriestSnowspeaker", 
			"AlgoxPriest Elite" => "AlgoxPriestSnowspeaker Elite", 
			"AlgoxArcher" => "AlgoxArcherSnowspeaker", 
			"AlgoxArcher Elite" => "AlgoxArcherSnowspeaker Elite", 
			_ => null, 
		};
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		return new List<object> { ScenarioManager.Scenario.Enemies.Count + ScenarioManager.Scenario.Enemy2Monsters.Count };
	}
}
