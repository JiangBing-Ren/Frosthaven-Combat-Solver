using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH076 : ScenarioScript
{
	private CActor _fishKing;

	private CObjective_Scripted _fishKingDiedObjective;

	private const int RevealRoom2Round = 7;

	private int chosenConclusion;

	private bool goneThroughFishKingNarrative;

	private const string piranhaPigID = "PiranhaPigID";

	private CMap Room2 => ScenarioManager.Scenario.Maps.First((CMap m) => m.RoomName == "Room_2");

	private CObjectDoor _door => GetProp(11, 8, "ThinDoor") as CObjectDoor;

	protected override bool Initialize()
	{
		_fishKing = ScenarioManager.CurrentScenarioState.AllyMonsters.First((EnemyState a) => a.ActorGuid == "947693b1-f3a5-7299-aada-0a15e085cc70").Actor;
		_fishKingDiedObjective = GetObjective<CObjective_Scripted>("FishKingDied");
		return true;
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor == _fishKing)
		{
			_fishKingDiedObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.BeforeSelectMonsterClassAbilityCards && OncePerRound() && ScenarioScript.RoundNumber == 1)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				SpawnPirannaPigAroundActor(playerActor);
			}
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound && ScenarioScript.RoundNumber == 7 && !Room2.Revealed)
		{
			RevealRoom(Room2);
			_door.SetExtraLockState(lockedStateToSet: false);
			SpawnDifficultTerrain(11, 8);
		}
	}

	private void SpawnPirannaPigAroundActor(CPlayerActor player)
	{
		TrySpawnActorAtNearestTile(player.Tile, "PiranhaPigID", CActor.EType.Ally, out var _);
	}

	private void SpawnDifficultTerrain(int x, int y)
	{
		CTile cTile = ScenarioManager.Tiles[x, y];
		CObjectDifficultTerrain prop = new CObjectDifficultTerrain("TerrainRubble", ScenarioManager.ObjectImportType.TerrainRubble, new TileIndex(cTile.m_ArrayIndex), null, null, null, cTile.m_HexMap.MapGuid, treatAsTrap: false, new CObjectiveFilter());
		cTile.SpawnProp(prop, notifyClient: true, 0.5f);
		Trace($"Created difficult terrain prop at {x},{y}", null, 63);
	}

	public override bool TreatTileAsFeatureless(CTile tile)
	{
		if (tile?.m_Props?.Find((CObjectProp p) => p.PropType == EPropType.TerrainWater) == null)
		{
			return false;
		}
		if (PhaseManager.PhaseType != CPhase.PhaseType.StartRoundEffects)
		{
			return false;
		}
		return true;
	}

	public override void OnLevelMessageWasDismissed(CLevelMessage message)
	{
		base.OnLevelMessageWasDismissed(message);
		if (message.MessageName == "Round12End")
		{
			goneThroughFishKingNarrative = true;
			if (GameState.ThreadIsSleeping && GameState.ThreadSleepReason == "WaitForFishKingNarrative")
			{
				GameState.SelectedContinueAbility();
			}
		}
	}

	public override void ApplyReward(bool won)
	{
		base.ApplyReward(won);
		if (won)
		{
			if (!goneThroughFishKingNarrative)
			{
				GameState.WaitForConfirmation("WaitForFishKingNarrative");
			}
			if (GameState.ShowGenericConfirmButton(GameState.HostCharacter, "Scenario_76_Reward_Choice", "Scenario_76_Reward_Choice_A", null, "Scenario_76_Reward_Choice_B", showHeader: false))
			{
				chosenConclusion = 0;
			}
			else
			{
				chosenConclusion = 1;
			}
		}
	}

	protected override int GetConclusionIndexInternal()
	{
		return chosenConclusion;
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("PiranhaPigID", 0, null, CActor.EType.Ally);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState;
	}
}
