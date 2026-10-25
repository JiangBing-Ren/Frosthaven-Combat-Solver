using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH054 : ScenarioScript
{
	private const int TokensCount = 6;

	private List<int> _tokens = new List<int>();

	private List<int> _tokensSpent = new List<int>();

	private CObjectProp _largeCaveRock;

	private CObjectProp[] _objectiveProps;

	private CObjectDoor _door1;

	private CObjectDoor _door2;

	private CSpawner[] _tokensSpawners;

	private CSpawner _bossSpawner;

	private CObjective _destroyAltarObjective;

	private CObjective _killBossObjective;

	private CObjective_Scripted _destroy5Obstacles;

	private CObjective_Scripted _destroy10Obstacles;

	private CScenarioModifier _enemyPresentRoundScenarioModifier;

	private CScenarioModifier _destroyPropSpecialRule;

	private CScenarioModifier _door1SpecialRule;

	private CScenarioModifier _door2SpecialRule;

	[StateDump(Skip = true)]
	private bool _anyEnemyPresentDuringRound;

	private int NoEnemyDuringRoundDamage => ScenarioScript._css.Players.Count - 1;

	private int TokenDamage
	{
		get
		{
			if (ScenarioScript.PartySize <= 2)
			{
				return ScenarioScript._css.Level + 2;
			}
			if (ScenarioScript.PartySize <= 3)
			{
				return ScenarioScript._css.Level + 4;
			}
			return 2 * ScenarioScript._css.Level + 4;
		}
	}

	public List<int> Tokens => _tokens;

	public List<int> TokensSpent => _tokensSpent;

	public CSpawner[] TokensSpawners => _tokensSpawners;

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UnderwaterStart();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickRoomRevealed();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			PutSnorkelsOn(_tickActor);
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 79);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
			break;
		}
	}

	protected override bool Initialize()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.Concat(ScenarioManager.CurrentScenarioState.DestroyedProps).ToList();
		list.Sort(GameState.ComparePropsByGUID);
		_largeCaveRock = GetProp(3, 17, "TwoHexObstacle", checkDestroyed: true);
		_door1 = GetProp(3, 5, "ThickDoor") as CObjectDoor;
		_door2 = GetProp(3, 13, "ThickDoor") as CObjectDoor;
		_tokensSpawners = new CSpawner[5]
		{
			spawners[0],
			spawners[1],
			spawners[2],
			spawners[3],
			spawners[4]
		};
		_bossSpawner = spawners[5];
		_objectiveProps = list.Where((CObjectProp p) => p.PropHealthDetails != null && p.PropHealthDetails.HasHealth && p != _largeCaveRock).ToArray();
		_enemyPresentRoundScenarioModifier = GetScenarioModifier("EnemyPresentRound");
		_enemyPresentRoundScenarioModifier.CustomLocKeyArgs = new List<object> { NoEnemyDuringRoundDamage };
		_door1SpecialRule = GetScenarioModifier("Door1SpecialRule");
		_door2SpecialRule = GetScenarioModifier("Door2SpecialRule");
		_destroyPropSpecialRule = GetScenarioModifier("DestroyPropSpecialRule");
		_destroyAltarObjective = GetObjective("DestroyAltarObjective");
		_killBossObjective = GetObjective("KillBossObjective");
		_destroy5Obstacles = GetObjective("Destroy5Obstacles") as CObjective_Scripted;
		_destroy5Obstacles.Total = 5;
		_destroy10Obstacles = GetObjective("Destroy10Obstacles") as CObjective_Scripted;
		_destroy10Obstacles.Total = 10;
		PutSnorkelsOn();
		if (!IsLoading)
		{
			TryRefillDeck();
		}
		CheckIfAnyEnemyPresentDuringRound();
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_tokens);
		string text2 = JsonConvert.SerializeObject(_tokensSpent);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 122);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 128);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_tokens = JsonConvert.DeserializeObject<List<int>>(array[0]);
			_tokensSpent = JsonConvert.DeserializeObject<List<int>>(array[1]);
		}
	}

	protected override void TickRepositionInternal()
	{
		CheckIfAnyEnemyPresentDuringRound();
	}

	private void TickPhaseStartRound()
	{
		_anyEnemyPresentDuringRound = false;
		CheckIfAnyEnemyPresentDuringRound();
	}

	private void TickPhaseEndRound()
	{
		if (OncePerRound() && !_anyEnemyPresentDuringRound)
		{
			List<CActor> list = ScenarioManager.Scenario.AllAliveActors.FindAll((CActor a) => a.Type == CActor.EType.Player && !a.IsDead);
			CAbility cAbility = CAbilityDamage.CreateDefaultDamage(NoEnemyDuringRoundDamage, 1, 1, isMonster: false, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self | CAbilityFilter.EFilterTargetType.Ally, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player));
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.IgnoreLOS = true;
			cAbility.MiscAbilityData.IgnoreStun = true;
			cAbility.MiscAbilityData.CanSkip = false;
			cAbility.MiscAbilityData.CanUndo = false;
			cAbility.Targeting = CAbility.EAbilityTargeting.Custom;
			cAbility.TargetingVars = new List<(string, TValue)>
			{
				("AddValidActorsInRange", new TValue(list)),
				("AddTilesInRange", new TValue(list.Select((CActor x) => x.Tile).ToList()))
			};
			cAbility.MiscVars = new List<(string, TValue)> { ("includeCasterTile", true) };
			cAbility.SetTreatAsScenarioRule(val: true);
			PhaseManager.StartAbilities(cAbility, GameState.HostCharacter);
		}
	}

	private void CheckIfAnyEnemyPresentDuringRound()
	{
		_ = ScenarioManager.Scenario.AllAliveMonsters;
		if (ScenarioManager.Scenario.AllAliveMonsters.Count > 0)
		{
			_anyEnemyPresentDuringRound = true;
		}
	}

	protected override void TickDeathInternal()
	{
		if (_objectiveProps.Any((CObjectProp p) => p.RuntimeAttachedActor == _tickActor) && _door2.DoorIsClosed)
		{
			TriggerTokenEffect(_tickActor);
			UpdatePropsObjectives();
		}
		else if (_largeCaveRock?.RuntimeAttachedActor == _tickActor)
		{
			TriggerSpawnerAtTile(_bossSpawner, _largeCaveRock.ArrayIndex.ToPoint());
			CActor actor = ScenarioScript._css.Monsters.Find((EnemyState m) => m.IsBoss).Actor;
			actor.Damaged(actor.Health / 2, fromAttackAbility: false, null, null);
			_killBossObjective.SetActivation(active: true);
		}
	}

	private void TickRoomRevealed()
	{
		if (_door1.DoorIsOpen)
		{
			_door2SpecialRule.Show();
		}
		if (_door2.DoorIsOpen)
		{
			_destroyPropSpecialRule.Hide();
		}
	}

	public override bool IsUnderwater(CTile tile = null)
	{
		return true;
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (_door2.DoorIsOpen && _largeCaveRock.IsDestroyed)
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 230);
			return true;
		}
		return false;
	}

	private void UpdatePropsObjectives()
	{
		if (!_destroy5Obstacles.IsComplete)
		{
			_destroy5Obstacles.HideOnComplete = true;
			UpdatePropsObjective(_destroy5Obstacles, _door1SpecialRule, _door1);
			if (_destroy5Obstacles.IsComplete)
			{
				_door1.ForceActivate(null);
				_destroy5Obstacles.SetActivation(active: false);
				_destroy10Obstacles.Current = _destroy5Obstacles.Total;
				_destroy10Obstacles.SetActivation(active: true);
			}
		}
		else if (!_destroy10Obstacles.IsComplete)
		{
			UpdatePropsObjective(_destroy10Obstacles, _door2SpecialRule, _door2);
			if (_destroy10Obstacles.IsComplete)
			{
				_door2.ForceActivate(null);
				_door2SpecialRule.Hide();
				_destroy10Obstacles.SetActivation(active: false);
				_destroyAltarObjective.SetActivation(active: true);
			}
		}
	}

	private void UpdatePropsObjective(CObjective_Scripted objective, CScenarioModifier doorSpecialRule, CObjectDoor door)
	{
		objective.Current++;
		if (objective.Current >= objective.Total)
		{
			objective.ScriptedIsComplete = true;
			doorSpecialRule.Hide();
			door.SetHasExtraLock(hasExtraLock: false);
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void TriggerTokenEffect(CActor actor)
	{
		int nextRandomToken = GetNextRandomToken();
		if (nextRandomToken == 6)
		{
			if (_tickAbility?.TargetingActor != null)
			{
				DealScenarioDamage(_tickAbility.TargetingActor, TokenDamage);
			}
		}
		else
		{
			CSpawner spawner = _tokensSpawners[nextRandomToken - 1];
			TriggerSpawnerAtTile(spawner, actor.ArrayIndex);
		}
		CheckIfAnyEnemyPresentDuringRound();
	}

	private void TryRefillDeck()
	{
		if (_tokens.Count == 0)
		{
			_tokensSpent.Clear();
			for (int i = 1; i <= 6; i++)
			{
				_tokens.Add(i);
			}
			_tokens.Shuffle(ScenarioScript._css.ScenarioRNG);
			Trace("Tokens refilled.", null, 300);
		}
	}

	private int GetNextRandomToken()
	{
		TryRefillDeck();
		int num = _tokens[0];
		_tokens.RemoveAt(0);
		_tokensSpent.Add(num);
		return num;
	}

	private void TriggerSpawnerAtTile(CSpawner spawner, Point arrayIndex)
	{
		spawner.SetLocation(new TileIndex(arrayIndex));
		for (int i = 0; i < spawner.SpawnRoundEntries.Count; i++)
		{
			spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		}
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("damage_from_no_enemies_this_round", NoEnemyDuringRoundDamage.ToString());
		yield return ("damage_from_token", TokenDamage.ToString());
	}
}
