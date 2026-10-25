using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH090 : ScenarioScript
{
	private const int FrozenCorpseActivationTotalDamage = 5;

	private const int GoldPerTreasurePiece = 10;

	private const string IceCorridorPrefabNameA = "IceCorridorAS70";

	private const string IceCorridorPrefabNameB = "IceCorridorBS70";

	private const string TreasurePiecePrefabName = "TreasurePiece";

	private const string ResourceID = "TreasurePiece";

	private static readonly List<Tuple<Point, Point>> CorridorsPoints = new List<Tuple<Point, Point>>
	{
		new Tuple<Point, Point>(new Point(9, 4), new Point(8, 5)),
		new Tuple<Point, Point>(new Point(5, 7), new Point(4, 7)),
		new Tuple<Point, Point>(new Point(6, 11), new Point(6, 12)),
		new Tuple<Point, Point>(new Point(11, 10), new Point(10, 10))
	};

	private List<string> _inactiveFrozenCorpsesGuids;

	private List<int> _revealedCorridors;

	private CObjective_Scripted _killMonstersWinObjective;

	private CObjective_Scripted _treasurePiecesObjective;

	private int chosenConclusion;

	private bool goneThroughTreasureNarrative;

	private bool AreAllCorridorsRevealed => _revealedCorridors.Count == CorridorsPoints.Count;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private bool IsFrozenCorpse(CActor actor)
	{
		if (actor is CEnemyActor)
		{
			return actor.MonsterClass.ID.StartsWith("FrozenCorpse");
		}
		return false;
	}

	private bool IsActive(CActor actor)
	{
		return !_inactiveFrozenCorpsesGuids.Contains(actor.ActorGuid);
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			TickStartScenario();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			TickAfterAbility();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 52);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	public static bool ValidateIsFrozenCorpseActive(CActor actor)
	{
		if (!(ScenarioScript.current is ScenarioScript_FH090 scenarioScript_FH) || scenarioScript_FH.IsActive(actor))
		{
			return true;
		}
		scenarioScript_FH.Trace("Frozen Corpse $" + actor.ActorGuid + " is not active and will not act now.", null, 61);
		return false;
	}

	protected override bool Initialize()
	{
		_killMonstersWinObjective = (CObjective_Scripted)GetObjective("KillActiveEnemies");
		_treasurePiecesObjective = (CObjective_Scripted)GetObjective("TreasurePiecesCashed");
		CObjective_Scripted objective = GetObjective<CObjective_Scripted>("BlockVictory");
		if (objective != null)
		{
			objective.Total = 1;
		}
		if (_inactiveFrozenCorpsesGuids == null)
		{
			_inactiveFrozenCorpsesGuids = new List<string>();
			foreach (EnemyState monster in ScenarioScript._css.Monsters)
			{
				if (IsFrozenCorpse(monster.Actor))
				{
					_inactiveFrozenCorpsesGuids.Add(monster.ActorGuid);
				}
			}
		}
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_inactiveFrozenCorpsesGuids);
		string text2 = JsonConvert.SerializeObject(_revealedCorridors);
		return JsonConvert.SerializeObject(new string[2] { text, text2 });
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_inactiveFrozenCorpsesGuids = JsonConvert.DeserializeObject<List<string>>(array[0]);
			_revealedCorridors = JsonConvert.DeserializeObject<List<int>>(array[1]);
		}
		CheckSpecialRules();
		if (_revealedCorridors == null)
		{
			return;
		}
		for (int i = 0; i < CorridorsPoints.Count; i++)
		{
			bool flag = _revealedCorridors.Contains(i);
			Tuple<Point, Point> tuple = CorridorsPoints[i];
			CTile tile = ScenarioManager.GetTile(tuple.Item1);
			CTile tile2 = ScenarioManager.GetTile(tuple.Item2);
			foreach (CObjectProp item in from x in tile.m_Props.Concat(tile2.m_Props)
				where x.PrefabName == "IceCorridorAS70" || x.PrefabName == "IceCorridorBS70"
				select x)
			{
				UpdateIceCorridorVisualState(item, !flag);
			}
		}
	}

	private void TickStartScenario()
	{
		if (_revealedCorridors == null)
		{
			InitAllCorridors();
		}
	}

	private void TickStartRound()
	{
		if (OncePerRound())
		{
			CheckCorridors();
			CheckSpecialRules();
		}
	}

	protected override void TickDeathInternal()
	{
		if (IsFrozenCorpse(_tickActor))
		{
			_tickActor.NoGoldDrop = true;
			SpawnResourceProp("TreasurePiece", "TreasurePiece", _tickActor.Tile);
		}
		CheckSpecialRules();
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		CheckSpecialRules();
	}

	private void TickAfterAbility()
	{
		CheckSpecialRules();
	}

	private void UpdateActivatedFrozenCorpses()
	{
		EnemyState[] array = ScenarioScript._css.Monsters.Where((EnemyState m) => !m.IsHiddenForCurrentPartySize && !IsActive(m.Actor)).ToArray();
		foreach (EnemyState enemyState in array)
		{
			if (enemyState.Actor.Health <= enemyState.Actor.MaxHealth - 5)
			{
				Activate(enemyState.Actor);
			}
		}
	}

	private void Activate(CActor actor)
	{
		_inactiveFrozenCorpsesGuids.Remove(actor.ActorGuid);
		ScenarioScript.HealTo(actor, actor.MaxHealth);
		CActiveBonus cActiveBonus = FindIceBonus(actor);
		if (cActiveBonus != null)
		{
			actor.PlayVFX("IceDestructionGeneric");
			cActiveBonus.Finish();
		}
	}

	public static CActiveBonus FindIceBonus(CActor actor)
	{
		return actor.FindApplicableActiveBonuses(CAbility.EAbilityType.NullTargeting).FirstOrDefault((CActiveBonus x) => x.Ability.GetAbilityBaseCard() is CScenarioModifier cScenarioModifier && cScenarioModifier.ScenarioAbilityID == "ScenarioAbility_FH90_FrozenCorpseIce");
	}

	private void CheckCorridors()
	{
		if (AreAllCorridorsRevealed)
		{
			RemoveAllCorridors();
		}
		else
		{
			RevealCorridor(GetCorridorNoToReveal());
		}
	}

	private void RevealCorridor(int corridorNo)
	{
		Tuple<Point, Point> tuple = CorridorsPoints[corridorNo];
		RemoveProps(ScenarioManager.Tiles[tuple.Item1.X, tuple.Item1.Y], "IceCorridorAS70");
		RemoveProps(ScenarioManager.Tiles[tuple.Item2.X, tuple.Item2.Y], "IceCorridorBS70");
		_revealedCorridors.Add(corridorNo);
	}

	private void RemoveCorridor(int corridorNo)
	{
		Tuple<Point, Point> tuple = CorridorsPoints[corridorNo];
		RemoveCorridorTile(ScenarioManager.Tiles[tuple.Item1.X, tuple.Item1.Y], "IceCorridorAS70");
		RemoveCorridorTile(ScenarioManager.Tiles[tuple.Item2.X, tuple.Item2.Y], "IceCorridorBS70");
		_revealedCorridors.Remove(corridorNo);
	}

	private void RemoveCorridorTile(CTile tile, string iceCorridorPrefabName)
	{
		RemoveProps(tile, iceCorridorPrefabName, updateCorridorVisualState: false);
		KillAllNonPlayerActors(tile);
		MovePlayerActors(tile);
		SpawnCorridorProp(tile, iceCorridorPrefabName);
	}

	private void RemoveProps(CTile tile, string iceCorridorPrefabName, bool updateCorridorVisualState = true)
	{
		CObjectProp[] array = tile.m_Props.ToArray();
		foreach (CObjectProp cObjectProp in array)
		{
			if (cObjectProp.PrefabName == iceCorridorPrefabName)
			{
				if (updateCorridorVisualState)
				{
					UpdateIceCorridorVisualState(cObjectProp, active: false);
				}
			}
			else
			{
				cObjectProp.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			}
		}
	}

	private void UpdateIceCorridorVisualState(CObjectProp prop, bool active)
	{
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = prop;
		cChangePropVisualState_MessageData.m_stateName = (active ? "State1" : "State0");
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		prop.PropCustomLocKey = (active ? "FH_90_Deep_Water_T" : "FH_90_Icy_Bridge_T");
		prop.PropCustomDescriptionLocKey = (active ? "FH_90_Deep_Water_D" : "FH_90_Icy_Bridge_D");
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	private void KillAllNonPlayerActors(CTile tile)
	{
		foreach (CActor item in tile.LivingActors.Where((CActor a) => !(a is CPlayerActor)))
		{
			item.NoGoldDrop = true;
			GameState.KillActor(item, item, CActor.ECauseOfDeath.Damage, out var _);
		}
	}

	private void MovePlayerActors(CTile tile)
	{
		foreach (CActor item in tile.LivingActors.Where((CActor a) => a is CPlayerActor && a.RealArrayIndex == tile.m_ArrayIndex))
		{
			if (GetNearestFreeTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, out var nearestTile))
			{
				GameState.TeleportActor(item, nearestTile);
			}
			else
			{
				Trace("Can't find nearest free tile to move player from the corridor!", null, 232);
			}
			DealScenarioDamage(item, ScenarioManager.TrapDamage, isTrapDamage: true);
		}
	}

	private void SpawnCorridorProp(CTile tile, string iceCorridorPrefabName)
	{
		TileIndex tileIndex = new TileIndex(tile.m_ArrayIndex);
		List<TileIndex> list = new List<TileIndex>();
		list.Add(tileIndex);
		CObjectObstacle prop = new CObjectObstacle("SkipVisuals", ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, null, list, null, tile.m_HexMap.MapGuid, ignoresFlyAndJump: true, setPositionToCenterOfAllPathBlockers: false, blockLOS: true);
		tile.SpawnProp(prop);
		CObjectProp cObjectProp = tile.m_Props.Find((CObjectProp p) => p.PrefabName == iceCorridorPrefabName);
		if (cObjectProp == null)
		{
			cObjectProp = SpawnVFXProp(tile, iceCorridorPrefabName);
		}
		UpdateIceCorridorVisualState(cObjectProp, active: true);
	}

	private void InitAllCorridors()
	{
		_revealedCorridors = new List<int>();
		RemoveAllCorridors();
	}

	private void RemoveAllCorridors()
	{
		for (int i = 0; i < CorridorsPoints.Count; i++)
		{
			RemoveCorridor(i);
		}
	}

	private int GetCorridorNoToReveal()
	{
		List<int> list = new List<int> { 0, 1, 2, 3 }.Except(_revealedCorridors).ToList();
		return list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
	}

	public override bool IsTileEleveated(CTile tile)
	{
		foreach (Tuple<Point, Point> corridorsPoint in CorridorsPoints)
		{
			if ((tile.m_ArrayIndex.X == corridorsPoint.Item1.X && tile.m_ArrayIndex.Y == corridorsPoint.Item1.Y) || (tile.m_ArrayIndex.X == corridorsPoint.Item2.X && tile.m_ArrayIndex.Y == corridorsPoint.Item2.Y))
			{
				return true;
			}
		}
		return false;
	}

	private void TickEndRound()
	{
		if (_killMonstersWinObjective.ScriptedIsComplete && _tickActor is CPlayerActor actor)
		{
			OfferCashTreasurePieces(actor);
		}
	}

	private void OfferCashTreasurePieces(CPlayerActor actor)
	{
		CPlayerActor hostCharacter = GameState.HostCharacter;
		if (actor == hostCharacter && !GameState.ShowGenericConfirmButton(GameState.HostCharacter, "FH_Scenario_Campaign_090_ChooseCompleteScenario", "GUI_CONTINUE", null, "GUI_COMPLETE", showHeader: false))
		{
			CompleteTreasurePiecesObjective();
		}
	}

	private void CheckSpecialRules()
	{
		UpdateActivatedFrozenCorpses();
		UpdateWinObjective();
	}

	private void UpdateWinObjective()
	{
		EnemyState[] source = ScenarioScript._css.Monsters.Where((EnemyState m) => !m.IsHiddenForCurrentPartySize && IsActive(m.Actor)).ToArray();
		_killMonstersWinObjective.Total = source.Count();
		_killMonstersWinObjective.Current = source.Count((EnemyState m) => m.Actor.Health <= 0 || m.Actor.IsDead);
		_killMonstersWinObjective.ScriptedIsComplete = _killMonstersWinObjective.Current >= _killMonstersWinObjective.Total;
		_killMonstersWinObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void CompleteTreasurePiecesObjective()
	{
		_treasurePiecesObjective.ScriptedIsComplete = true;
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	public override void OnLevelMessageWasDismissed(CLevelMessage message)
	{
		base.OnLevelMessageWasDismissed(message);
		if (message.MessageName == "ShareTheLoot")
		{
			goneThroughTreasureNarrative = true;
			if (GameState.ThreadIsSleeping && GameState.ThreadSleepReason == "WaitForTreasureNarrative")
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
			if (!goneThroughTreasureNarrative)
			{
				GameState.WaitForConfirmation("WaitForTreasureNarrative");
			}
			if (GameState.ShowGenericConfirmButton(GameState.HostCharacter, "Scenario_90_Reward_Choice", "Scenario_90_Reward_Choice_A", null, "Scenario_90_Reward_Choice_B", showHeader: false))
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

	protected override List<object> GetConclusionParametersInternal()
	{
		List<object> list = new List<object>();
		foreach (PlayerState item in ScenarioScript._css.Players.OrderBy((PlayerState x) => x.ClassID))
		{
			int characterResourceAmount = item.Actor.GetCharacterResourceAmount("TreasurePiece");
			list.Add(characterResourceAmount);
		}
		return list;
	}
}
