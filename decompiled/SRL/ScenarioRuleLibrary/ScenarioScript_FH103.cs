using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH103 : ScenarioScript
{
	[Serializable]
	public class Storage103
	{
		public bool _pillarDamagedThisRound;

		public List<KeyValuePair<string, int>> playerStress;
	}

	private bool _pillarDamagedThisRound;

	private List<Point> _icePillars = new List<Point>
	{
		new Point(4, 5),
		new Point(4, 9),
		new Point(7, 9),
		new Point(7, 13),
		new Point(7, 17),
		new Point(4, 1)
	};

	private static Dictionary<string, int> _playerStress = new Dictionary<string, int>();

	private CScenarioModifier _stressIndicatorModifier;

	private const string LivingSpiritNormal = "LivingSpiritID";

	private const string LivingSpiritElite = "LivingSpiritEliteID";

	private const string ShrikeFiendNormal = "ShrikeFiendID";

	private const string LivingDoomNormal = "LivingDoomID";

	private const string LurkerClawcrusherNormal = "LurkerClawcrusherID";

	protected override bool Initialize()
	{
		_stressIndicatorModifier = GetScenarioModifier("StressIndicator");
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			_playerStress[allPlayer.CharacterClass.ID] = 0;
		}
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(new Storage103
		{
			_pillarDamagedThisRound = _pillarDamagedThisRound,
			playerStress = _playerStress.ToList()
		});
		Trace("[ffa000]Serialize: " + text + "[-]", null, 56);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 62);
		if (!string.IsNullOrEmpty(s))
		{
			Storage103 storage = JsonConvert.DeserializeObject<Storage103>(s);
			_playerStress = new Dictionary<string, int>(storage.playerStress);
			_pillarDamagedThisRound = storage._pillarDamagedThisRound;
		}
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			if (_tickActor is CPlayerActor cPlayerActor && ScenarioManager.GetAllAdjacentTiles(cPlayerActor.ArrayIndex.X, cPlayerActor.ArrayIndex.Y).Any((CTile tile) => tile.LivingActors.Any((CActor actor) => actor is CObjectActor && _icePillars.Contains(actor.ArrayIndex))))
			{
				_playerStress[cPlayerActor.CharacterClass.ID]++;
			}
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			if (!_pillarDamagedThisRound)
			{
				break;
			}
			foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
			{
				SpawnEnemies(allPlayer);
			}
			_pillarDamagedThisRound = false;
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 114);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor is CObjectActor cObjectActor && actualDamage > 0 && cObjectActor.LastAbilityDamagedBy != null)
		{
			_pillarDamagedThisRound = true;
			if (cObjectActor.LastAbilityDamagedBy.TargetingActor is CPlayerActor cPlayerActor)
			{
				_playerStress[cPlayerActor.CharacterClass.ID]++;
			}
		}
		if (_tickActor is CPlayerActor cPlayerActor2)
		{
			_playerStress[cPlayerActor2.CharacterClass.ID]++;
		}
	}

	private void SpawnEnemies(CPlayerActor player)
	{
		int num = _playerStress[player.CharacterClass.ID];
		CActor actor;
		if (0 <= num && num <= 5)
		{
			TrySpawnActorAtNearestTile(player.Tile, "LivingSpiritID", out actor);
		}
		if (6 <= num && num <= 8)
		{
			TrySpawnActorAtNearestTile(player.Tile, "ShrikeFiendID", out actor);
		}
		if (9 <= num && num <= 11)
		{
			TrySpawnActorAtNearestTile(player.Tile, "LivingDoomID", out actor);
		}
		if (12 <= num)
		{
			TrySpawnActorAtNearestTile(player.Tile, "LivingSpiritEliteID", out actor);
			TrySpawnActorAtNearestTile(player.Tile, "LurkerClawcrusherID", out var _);
		}
	}

	public static int GetPlayerStress(CPlayerActor player)
	{
		return _playerStress[player.CharacterClass.ID];
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("ShrikeFiendID", 0, null, CActor.EType.Enemy);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState;
		EnemyState enemyState2 = new EnemyState("LivingDoomID", 0, null, CActor.EType.Enemy);
		enemyState2.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState2.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState2.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState2.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState2.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState2;
		EnemyState enemyState3 = new EnemyState("LurkerClawcrusherID", 0, null, CActor.EType.Enemy);
		enemyState3.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState3.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState3.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState3.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState3.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState3;
	}
}
