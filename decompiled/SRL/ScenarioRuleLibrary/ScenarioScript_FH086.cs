using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH086 : ScenarioScript
{
	private bool _ladyInWhiteSpawned;

	private CScenarioModifier _treeModifier;

	private CObjective _treesObjective;

	private CObjective_Scripted _ladyInWhiteObjective;

	private CMap _room2;

	private const string LadyInWhiteNormalID = "LadyInWhiteID";

	private const string LadyInWhiteEliteID = "LadyInWhiteEliteID";

	protected override bool Initialize()
	{
		_treesObjective = GetObjective("DestroyAllTrees");
		_ladyInWhiteObjective = (CObjective_Scripted)GetObjective("KillLadyInWhite");
		_ladyInWhiteObjective.Total = 1;
		_ladyInWhiteObjective.ScriptedEnoughToWinAlone = true;
		_treeModifier = GetScenarioModifier("KillTreeModifier");
		_room2 = ScenarioScript.FindRoom("Room_2");
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemies.FirstOrDefault((CEnemyActor x) => x.Class.ID.Equals("LadyInWhiteEliteID", StringComparison.InvariantCultureIgnoreCase) || x.Class.ID.Equals("LadyInWhiteID", StringComparison.InvariantCultureIgnoreCase));
		_ladyInWhiteSpawned = cEnemyActor != null;
		if (_ladyInWhiteSpawned)
		{
			_ladyInWhiteObjective.Current = (cEnemyActor.IsDead ? 1 : 0);
			_ladyInWhiteObjective.ScriptedIsComplete = _ladyInWhiteObjective.Current >= _ladyInWhiteObjective.Total;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete(isEndOfRound: false, EObjectiveType.Scripted);
		}
	}

	protected override void TickDeathInternal()
	{
		List<CObjectActor> list = ScenarioManager.Scenario.AllObjects.FindAll((CObjectActor tree) => !tree.IsDead).ToList();
		if (_tickActor is CObjectActor && list.Count == 0 && _room2.Revealed)
		{
			Trace(_tickActor, null, 53);
			TrySpawnActorAtNearestTile(_tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y, "LadyInWhiteEliteID", CActor.EType.Enemy, out var actor, addIntoInitiativeOrder: true);
			if (actor != null)
			{
				actor.PlayedThisRound = false;
				_ladyInWhiteSpawned = true;
				_treesObjective.SetActivation(active: false);
				_treeModifier.SetHiddenState(shouldSetHidden: true);
				_ladyInWhiteObjective.SetActivation(active: true);
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete(isEndOfRound: false, EObjectiveType.Scripted);
			}
		}
		if ((_tickActor.Class.ID.Equals("LadyInWhiteEliteID", StringComparison.InvariantCultureIgnoreCase) || _tickActor.Class.ID.Equals("LadyInWhiteID", StringComparison.InvariantCultureIgnoreCase)) && _tickActor.IsDead)
		{
			_ladyInWhiteObjective.Current = 1;
			_ladyInWhiteObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete(isEndOfRound: false, EObjectiveType.Scripted);
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (!(_tickActor is CObjectActor cObjectActor) || damageTaken <= 0)
		{
			return;
		}
		Trace(_tickActor, null, 79);
		if (_tickActor.Health <= 0)
		{
			return;
		}
		List<CTile> list = new List<CTile>();
		if (!(cObjectActor.AttachedProp is CObjectObstacle cObjectObstacle))
		{
			return;
		}
		foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
		{
			GameState.GetAdjacentTiles(pathingBlocker.ToPoint(), list, clear: false);
		}
		CTile cTile = list.Find((CTile tile) => !tile.IsBlocked && tile.IsEmpty);
		if (cTile != null)
		{
			SpawnActorAtTile(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, "BlackImpID", CActor.EType.Enemy, out var actor, addIntoInitiativeOrder: true, treatAsSummon: true);
			actor?.MarkWontPlayThisRound(wontPlay: true);
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Ability)
		{
			return false;
		}
		Trace("Checking message trigger for " + message.MessageName + " " + message.TitleKey + "...", null, 110);
		if (_ladyInWhiteSpawned && message.MessageName.Contains("FS086_1"))
		{
			Trace("...success", null, 114);
			return true;
		}
		Trace("...failed", null, 118);
		return false;
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("LadyInWhiteID", 0, null, CActor.EType.Enemy);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.ToElite;
		yield return enemyState;
	}
}
