using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH129 : ScenarioScript
{
	private const int maxAgates = 12;

	private const int maxAgatesLost = 5;

	[StateDump(Skip = true)]
	private int _playersDiscarded;

	[StateDump(Skip = true)]
	private bool _apPhaseStarted;

	private CScenarioModifier _spendAPModifier;

	private CObjectDoor _door1;

	private CObjectDoor _door2;

	private CObjectDoor _door3;

	private Point _endTile = new Point(3, 13);

	private CActor _gemBearer;

	private CObjectQuestItem _gemToCarry;

	private CObjective_Scripted _sunAgateLoseObjective;

	private CObjective_Scripted _gemBearerLoseObjective;

	private CObjective_Scripted _activityPointsObjective;

	private CObjective _killAllWinObjective;

	private const string CityGuardArcherNormal = "CityGuardArcherID";

	private const string CityGuardArcherElite = "CityGuardArcherEliteID";

	[StateDump(Skip = true)]
	public List<CPlayerActor> actorsWhoHavePlayedAPersistentAction = new List<CPlayerActor>();

	public int ActivityPoints
	{
		get
		{
			return _activityPointsObjective.Total - _activityPointsObjective.Current;
		}
		set
		{
			if (ActivityPoints != value)
			{
				_activityPointsObjective.Current = _activityPointsObjective.Total - value;
				_activityPointsObjective.CustomLocKeyArgs = new List<object> { ActivityPoints };
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
		}
	}

	public int LostAgates
	{
		get
		{
			return _sunAgateLoseObjective.Current;
		}
		set
		{
			if (LostAgates != value)
			{
				_sunAgateLoseObjective.Current = value;
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
		}
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = 12 - LostAgates;
		return new List<object> { num };
	}

	protected override bool Initialize()
	{
		_spendAPModifier = GetScenarioModifier("SpendAP");
		_playersDiscarded = 0;
		_door1 = GetProp(13, 9, "ThinDoor") as CObjectDoor;
		_door2 = GetProp(13, 6, "ThickDoor") as CObjectDoor;
		_door3 = GetProp(10, 5, "ThickDoor") as CObjectDoor;
		_sunAgateLoseObjective = (CObjective_Scripted)GetObjective("SunAgateLoseObjective");
		_gemBearerLoseObjective = (CObjective_Scripted)GetObjective("GemBearerLoseObjective");
		_gemBearerLoseObjective.ScriptedLocKey = "FH_Scenario_Campaign_129_SCENARIO_OBJECTIVE_3";
		_sunAgateLoseObjective.ScriptedLocKey = "FH_Scenario_Campaign_129_SCENARIO_OBJECTIVE_4";
		_activityPointsObjective = (CObjective_Scripted)GetObjective("PrepareObjective");
		_killAllWinObjective = GetObjective("KillAllObjective");
		return base.Initialize();
	}

	private void SetGemBearer(CEnemyActor actor)
	{
		_gemBearer = actor;
		if (_gemBearer != null)
		{
			if (_gemBearer.CarriedQuestItems.Count == 0)
			{
				if (_gemToCarry == null)
				{
					CObjectQuestItem gemToCarry = new CObjectQuestItem("SunAgate", ScenarioManager.ObjectImportType.CarryableQuestItem, new TileIndex(_gemBearer.Tile.m_ArrayIndex), null, null, null, _gemBearer.Tile.m_HexMap.MapGuid);
					_gemToCarry = gemToCarry;
				}
				_gemBearer.CarryQuestItem(_gemToCarry);
			}
			CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(_gemBearer, ScenarioManager.Tiles[_endTile.X, _endTile.Y]);
			if (_gemBearer.ActiveBonuses.FirstOrDefault((CActiveBonus x) => x.Ability.Name == "GemCarrierVFXBonus") == null)
			{
				CAbility cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: true, "", new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, "GemCarrierVFXBonus");
				cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				cAbility.ActiveBonusData.Hidden = true;
				cAbility.ActiveBonusData.SetVar("PersistentCasterVFX", "AmuletReanimationVFX");
				cAbility.Start(_gemBearer, _gemBearer);
				cAbility.ApplyToActor(_gemBearer);
			}
			ScenarioManager.ToggleTileHighlights(new List<Point> { _endTile }, enabled: true);
		}
		else
		{
			ScenarioManager.ToggleTileHighlights(new List<Point> { _endTile }, enabled: false);
		}
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		SetGemBearer(ScenarioManager.Scenario.Enemies.FirstOrDefault((CEnemyActor x) => x.CarriedQuestItems.Count > 0));
		_gemToCarry = _gemBearer?.CarriedQuestItems?.FirstOrDefault() as CObjectQuestItem;
		_activityPointsObjective.CustomLocKeyArgs = new List<object> { ActivityPoints };
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			ScenarioManager.ToggleTileHighlights(new List<Point> { _endTile }, enabled: true);
			if (!_apPhaseStarted)
			{
				if (_activityPointsObjective.Total == 0)
				{
					_activityPointsObjective.Total = 15;
					_activityPointsObjective.Current = 15;
					ActivityPoints = 15;
					_activityPointsObjective.SetActivation(active: false);
				}
				if (_tickActor is CPlayerActor actor)
				{
					Trace("AP: " + ActivityPoints, null, 127);
					DiscardCards(actor);
				}
				CPlayerActor hostCharacter = GameState.HostCharacter;
				if (_playersDiscarded == ScenarioManager.Scenario.AllPlayers.Count())
				{
					_activityPointsObjective.SetActivation(active: true);
					_apPhaseStarted = true;
					CAbility cAbility = CAbility.CopyAbility(_spendAPModifier.TriggerAbility(hostCharacter, ScenarioScript.PartySize), generateNewID: true, fullCopy: true);
					cAbility.TargetingActor = hostCharacter;
					GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, hostCharacter));
					ToggleStartingHexCustomHighlights(active: true);
				}
			}
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			if (ScenarioScript.RoundNumber == 1)
			{
				foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
				{
					if (enemy != _gemBearer)
					{
						CAIFocusOverrideDetails.ClearAIFocusOverride(enemy);
					}
				}
			}
			if (ScenarioScript.RoundNumber == 4)
			{
				_door3.ForceActivate(GameState.HostCharacter);
			}
			break;
		}
	}

	private void SetupGemBearer()
	{
		if (_gemBearer == null)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemies.Find((CEnemyActor enemy) => enemy.ArrayIndex.X == 15 && enemy.ArrayIndex.Y == 4);
			if (cEnemyActor != null)
			{
				SetGemBearer(cEnemyActor);
			}
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility.GetMiscVar("APCost").isValid && _tickAbility.AbilityHasHappened)
		{
			int num = _tickAbility.GetMiscVar("APCost").GetInt();
			ActivityPoints -= num;
			Trace("AP: " + ActivityPoints, null, 176);
			if (_playersDiscarded == ScenarioManager.Scenario.AllPlayers.Count())
			{
				CheckFinishedAPPhase(GameState.HostCharacter);
			}
		}
		else if (_tickAbility.GetMiscVar("CheckRevealSecondDoorLater").GetBool() && !GameState.PendingScenarioModifierAbilities.Any((Tuple<CAbility, CActor> x) => x.Item1.GetMiscVar("CheckRevealSecondDoorLater").GetBool()))
		{
			ContinueSecondDoorReveal();
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor != _gemBearer)
		{
			return;
		}
		CEnemyActor cEnemyActor = null;
		_tickActor.CarriedQuestItems.Clear();
		if (ScenarioManager.Scenario.AllEnemies.Any((CEnemyActor enemy) => !enemy.IsDead))
		{
			cEnemyActor = (from x in ScenarioManager.Scenario.AllEnemies
				where !x.IsDead
				orderby x.GetDistanceTo(_gemBearer)
				select x).FirstOrDefault();
			SetGemBearer(cEnemyActor);
			LostAgates++;
		}
		if (_sunAgateLoseObjective.Current >= _sunAgateLoseObjective.Total)
		{
			_sunAgateLoseObjective.ScriptedIsComplete = true;
			CheckObjectivesComplete();
		}
	}

	private void DiscardCards(CPlayerActor actor)
	{
		if (!actor.IsDead && !actor.PhasedOut)
		{
			CAbilityDiscardCards cAbilityDiscardCards = CAbility.CreateAbility(CAbility.EAbilityType.DiscardCards, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 4) as CAbilityDiscardCards;
			cAbilityDiscardCards.TargetingActor = actor;
			cAbilityDiscardCards.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbilityDiscardCards.MiscAbilityData.AutotriggerAbility = true;
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbilityDiscardCards, actor));
			_playersDiscarded++;
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor == _gemBearer && _tickActor.Tile.m_ArrayIndex == _endTile)
		{
			_gemBearerLoseObjective.ScriptedIsComplete = true;
			_gemBearerLoseObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
			CheckObjectivesComplete();
		}
	}

	private void ToggleStartingHexCustomHighlights(bool active)
	{
		ScenarioManager.ToggleTileHighlights(from x in ScenarioManager.StartingTiles.ToList()
			select x.m_ArrayIndex, active, new CVector4(1f, 1f, 1f, 1f));
	}

	private void CheckFinishedAPPhase(CPlayerActor actor)
	{
		if (!actor.IsDead && !actor.PhasedOut && ActivityPoints > 0)
		{
			ToggleStartingHexCustomHighlights(active: true);
		}
		else if (ActivityPoints <= 0)
		{
			ToggleStartingHexCustomHighlights(active: false);
			RevealFirstDoor();
		}
	}

	private void RevealFirstDoor()
	{
		if (PhaseManager.CurrentPhaseAction != null)
		{
			List<CPhaseAction.CPhaseAbility> list = PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities?.Where((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == "ChooseAPAbility")?.ToList();
			if (list != null)
			{
				foreach (CPhaseAction.CPhaseAbility item3 in list)
				{
					item3.m_Ability.AbilityHasHappened = true;
					item3.m_Ability.SetCancelAbilityFlag(cancel: true);
				}
			}
		}
		_door1.ForceActivate(GameState.HostCharacter);
		CAbility cAbility = CAbilityMove.CreateDefaultMove(9, isMonster: true);
		cAbility.SetMiscVar("CheckRevealSecondDoorLater", true);
		List<CEnemyActor> list2 = ScenarioManager.Scenario.AllEnemies.Where((CEnemyActor enemy) => enemy.Tile.m_HexMap.RoomName == "Room_2" && (enemy.Class.ID == "AlgoxScoutID" || enemy.Class.ID == "AlgoxScoutEliteID")).ToList();
		List<CEnemyActor> list3 = ScenarioManager.Scenario.AllEnemies.Where((CEnemyActor enemy) => enemy.Tile.m_HexMap.RoomName == "Room_2" && (enemy.Class.ID == "AlgoxGuardID" || enemy.Class.ID == "AlgoxGuardEliteID")).ToList();
		foreach (CEnemyActor item4 in list2)
		{
			CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(item4, ScenarioManager.Tiles[_endTile.X, _endTile.Y]);
			CAbility item = CAbility.CopyAbility(cAbility, generateNewID: true, fullCopy: true);
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(item, item4));
		}
		foreach (CEnemyActor item5 in list3)
		{
			CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(item5, ScenarioManager.Tiles[_endTile.X, _endTile.Y]);
			CAbility item2 = CAbility.CopyAbility(cAbility, generateNewID: true, fullCopy: true);
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(item2, item5));
		}
		GameState.TriggerCampaignSection("FS_129_How_to_Lay_an_Ambush_Special_1");
	}

	private void ContinueSecondDoorReveal()
	{
		_door2.ForceActivate(GameState.HostCharacter);
		for (int i = 0; i < 6 - ScenarioScript.PartySize; i++)
		{
			List<CTile> list = ScenarioManager.StartingTiles.Where((CTile tile) => tile.LivingActor == null).ToList();
			if (list.Count == 0)
			{
				list = ScenarioManager.StartingTiles;
			}
			TrySpawnActorAtNearestTile(list[0], "CityGuardArcherEliteID", CActor.EType.Ally, out var _);
		}
		foreach (CEnemyActor allEnemy in ScenarioManager.Scenario.AllEnemies)
		{
			CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(allEnemy, ScenarioManager.Tiles[_endTile.X, _endTile.Y]);
		}
		SetupGemBearer();
		_killAllWinObjective.SetActivation(active: true);
		_sunAgateLoseObjective.SetActivation(active: true);
		_sunAgateLoseObjective.Total = 5;
		_sunAgateLoseObjective.Current = 0;
		_sunAgateLoseObjective.ScriptedIsComplete = false;
		_gemBearerLoseObjective.SetActivation(active: true);
		_activityPointsObjective.SetActivation(active: false);
		_spendAPModifier.SetHiddenState(shouldSetHidden: true);
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			if (scenarioModifier.EventIdentifier == "APSpecial")
			{
				scenarioModifier.SetHiddenState(shouldSetHidden: true);
			}
			else if (scenarioModifier.EventIdentifier == "GemSpecial")
			{
				scenarioModifier.SetHiddenState(shouldSetHidden: false);
			}
		}
		GameState.TriggerCampaignSection("FS_129_How_to_Lay_an_Ambush_Special_2");
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("CityGuardArcherID", 0, null, CActor.EType.Ally);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.ToElite;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.ToElite;
		yield return enemyState;
	}

	public static bool HasEnoughAP(int amount)
	{
		if (!(ScenarioScript.current is ScenarioScript_FH129 scenarioScript_FH))
		{
			return true;
		}
		return scenarioScript_FH.ActivityPoints >= amount;
	}

	public static bool HasTwoHexDifficultTerrain(CTile tile)
	{
		return FindTwoHexDifficultTerrain(tile) != null;
	}

	public static CObjectProp FindTwoHexDifficultTerrain(CTile tile)
	{
		return tile.m_Props.FirstOrDefault((CObjectProp x) => x is CObjectDifficultTerrain && (x.GetLinkedPropsTotal()?.Count ?? 1) == 2);
	}

	public static int CountTwoHexDifficultTerrains()
	{
		List<CObjectProp> props = new List<CObjectProp>();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (prop is CObjectDifficultTerrain)
			{
				List<CObjectProp> linkedPropsTotal = prop.GetLinkedPropsTotal();
				if (linkedPropsTotal != null && linkedPropsTotal.Count == 2 && linkedPropsTotal.All((CObjectProp x) => !props.Contains(x)))
				{
					props.Add(prop);
				}
			}
		}
		return props.Count;
	}

	public static int CountTraps()
	{
		return ScenarioManager.CurrentScenarioState.Props.Count((CObjectProp x) => x is CObjectTrap);
	}

	public static int CountIce()
	{
		return ScenarioManager.CurrentScenarioState.Props.Count((CObjectProp x) => x is CObjectIceTerrain);
	}

	public static int CountHazardous()
	{
		return ScenarioManager.CurrentScenarioState.Props.Count((CObjectProp x) => x is CObjectHazardousTerrain);
	}

	public static int CountSnowdrift()
	{
		List<CObjectProp> list = new List<CObjectProp>();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (prop is CObjectDifficultTerrain && !prop.HasLinkedProps)
			{
				list.Add(prop);
			}
		}
		return list.Count;
	}

	public static string ChooseAPKey(CAbility ability, bool isHost)
	{
		if (isHost)
		{
			return "FH_Scenario_Campaign_129_SCENARIO_MODIFIER_9";
		}
		return "FH_Scenario_Campaign_129_SCENARIO_MODIFIER_9_ClientSide";
	}

	public static List<CActiveBonus> GetPersistentBonuses(CActor actor)
	{
		return (from x in actor.FindApplicableActiveBonuses()
			where x.Duration == CActiveBonus.EActiveBonusDurationType.Persistent || x.Duration == CActiveBonus.EActiveBonusDurationType.Summon
			select x).ToList();
	}

	public static void CachePersistentBonuses(CActor actor)
	{
		actor.SetTempVar("PersistentBonuses", new TValue(GetPersistentBonuses(actor)));
	}

	public static bool HasNewPersistents(CActor actor)
	{
		List<CActiveBonus> list = actor.GetVar("PersistentBonuses").GetObject<List<CActiveBonus>>();
		foreach (CActiveBonus persistentBonuse in GetPersistentBonuses(actor))
		{
			if (!list.Contains(persistentBonuse))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasNotPlayedPersistent(CActor actor)
	{
		return !(ScenarioScript.current as ScenarioScript_FH129).actorsWhoHavePlayedAPersistentAction.Contains(actor);
	}

	public static void CacheHasPlayedPersistent(CActor actor)
	{
		(ScenarioScript.current as ScenarioScript_FH129).actorsWhoHavePlayedAPersistentAction.Add(actor as CPlayerActor);
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("archer_count", (6 - ScenarioScript.PartySize).ToString());
	}
}
