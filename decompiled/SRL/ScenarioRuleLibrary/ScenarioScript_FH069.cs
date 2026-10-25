using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH069 : ScenarioScript
{
	private static readonly List<Tuple<Tuple<Point, Point>, Tuple<Point, Point>>> PassagesPoints = new List<Tuple<Tuple<Point, Point>, Tuple<Point, Point>>>
	{
		new Tuple<Tuple<Point, Point>, Tuple<Point, Point>>(new Tuple<Point, Point>(new Point(10, 7), new Point(10, 6)), new Tuple<Point, Point>(new Point(4, 3), new Point(5, 3))),
		new Tuple<Tuple<Point, Point>, Tuple<Point, Point>>(new Tuple<Point, Point>(new Point(13, 8), new Point(14, 8)), new Tuple<Point, Point>(new Point(20, 8), new Point(21, 8))),
		new Tuple<Tuple<Point, Point>, Tuple<Point, Point>>(new Tuple<Point, Point>(new Point(4, 11), new Point(5, 11)), new Tuple<Point, Point>(new Point(17, 11), new Point(18, 12)))
	};

	private static readonly int SummonsTravelPassageRange = 2;

	private static readonly int CollectSoilObjectiveCount = 3;

	private static readonly string SoilSamplePrefabName = "SoilSample";

	private static readonly string DF_RoomName = "Room_3";

	private static readonly string CE_RoomName = "Room_5";

	private static readonly string resourceID = "SoilSample";

	private readonly CVector4 _yellowColor = new CVector4(0.76f, 0.67f, 0.13f, 0.5f);

	private readonly CVector4 _purpleColor = new CVector4(0.56f, 0.18f, 0.68f, 0.5f);

	private readonly CVector4 _blueColor = new CVector4(0f, 0.8f, 1f, 0.5f);

	private bool _showDFRoomNarration;

	private bool _showCERoomNarration;

	private bool _showedDFRoomNarration;

	private bool _showedCERoomNarration;

	private int _removedShadows;

	private CObjective_Scripted _collectSoilSamples;

	private bool IsEarthDemon(CActor actor)
	{
		if (actor is CEnemyActor)
		{
			return actor.MonsterClass.ID.StartsWith("EarthDemon");
		}
		return false;
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override bool Initialize()
	{
		_collectSoilSamples = (CObjective_Scripted)GetObjective("CollectSoilSamplesObjective");
		_collectSoilSamples.Total = CollectSoilObjectiveCount;
		ToggleTileHighlights(PassagesPoints[0], enabled: true, _purpleColor);
		ToggleTileHighlights(PassagesPoints[1], enabled: true, _yellowColor);
		ToggleTileHighlights(PassagesPoints[2], enabled: true, _blueColor);
		ToggleTileLinks();
		SetNoGoldDropForEarthDemons();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			map.CanTeleportToFromAnotherMap = false;
		}
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_showDFRoomNarration);
		string text2 = JsonConvert.SerializeObject(_showCERoomNarration);
		string text3 = JsonConvert.SerializeObject(_showedDFRoomNarration);
		string text4 = JsonConvert.SerializeObject(_showedCERoomNarration);
		string text5 = JsonConvert.SerializeObject(_removedShadows);
		return JsonConvert.SerializeObject(new string[5] { text, text2, text3, text4, text5 });
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_showDFRoomNarration = JsonConvert.DeserializeObject<bool>(array[0]);
			_showCERoomNarration = JsonConvert.DeserializeObject<bool>(array[1]);
			_showedDFRoomNarration = JsonConvert.DeserializeObject<bool>(array[2]);
			_showedCERoomNarration = JsonConvert.DeserializeObject<bool>(array[3]);
			if (array.Length > 4)
			{
				_removedShadows = JsonConvert.DeserializeObject<int>(array[4]);
			}
		}
	}

	private void ToggleTileHighlights(Tuple<Tuple<Point, Point>, Tuple<Point, Point>> passagePoints, bool enabled, CVector4 color)
	{
		ScenarioManager.ToggleTileHighlights(new Point[4]
		{
			passagePoints.Item1.Item1,
			passagePoints.Item1.Item2,
			passagePoints.Item2.Item1,
			passagePoints.Item2.Item2
		}, enabled, color);
	}

	private void ToggleTileLinks()
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		foreach (Tuple<Tuple<Point, Point>, Tuple<Point, Point>> passagesPoint in PassagesPoints)
		{
			list.Add((passagesPoint.Item1.Item1, passagesPoint.Item2.Item1));
			list.Add((passagesPoint.Item1.Item2, passagesPoint.Item2.Item2));
		}
		ScenarioManager.ToggleTileLinks(list, enabled: true);
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			TickAfterAbility();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 130);
			break;
		}
	}

	private void TickStartTurn()
	{
		UpdateWinObjective();
	}

	private void TickEndTurn()
	{
		if (_tickActor is CPlayerActor { PhasedOut: false } cPlayerActor)
		{
			UpdateWinObjective();
			CheckIfCanOfferTravelPassage(cPlayerActor);
		}
	}

	private void TickStartRound()
	{
		UpdateWinObjective();
	}

	private void TickEndRound()
	{
		if (_tickActor.PhasedOut && _tickActor is CPlayerActor cPlayerActor)
		{
			Point point = new Point(cPlayerActor.Tile);
			Point secondPassagePoint = FindDestinationPassagePoint(point);
			if (PhaseInActorAndSummons(cPlayerActor, secondPassagePoint))
			{
				TriggerDiscard2Ability(cPlayerActor);
				CheckIsShouldShowNarrativeEvents(secondPassagePoint);
			}
		}
		UpdateWinObjective();
	}

	private void TickAfterAbility()
	{
		UpdateWinObjective();
	}

	protected override void OnFinishedRevealingRoom()
	{
		SetNoGoldDropForEarthDemons();
	}

	private Point FindDestinationPassagePoint(Point point)
	{
		Tuple<Tuple<Point, Point>, Tuple<Point, Point>> tuple = PassagesPoints.Find((Tuple<Tuple<Point, Point>, Tuple<Point, Point>> pp) => new Point[4]
		{
			pp.Item1.Item1,
			pp.Item1.Item2,
			pp.Item2.Item1,
			pp.Item2.Item2
		}.Contains(point));
		if (tuple == null)
		{
			Error($"Can't find second passage point for entry point: {point}", null, 173);
			return default;
		}
		if (!(tuple.Item1.Item1 == point) && !(tuple.Item1.Item2 == point))
		{
			if (!(point == tuple.Item2.Item1))
			{
				return tuple.Item1.Item2;
			}
			return tuple.Item1.Item1;
		}
		if (!(point == tuple.Item1.Item1))
		{
			return tuple.Item2.Item2;
		}
		return tuple.Item2.Item1;
	}

	private bool PhaseInActorAndSummons(CPlayerActor playerActor, Point secondPassagePoint)
	{
		playerActor.ArrayIndex = FindNearestFreePointInPointRoom(secondPassagePoint);
		RevealRoom(ScenarioManager.GetTile(secondPassagePoint).m_HexMap);
		bool flag = GameState.PhaseInActor(playerActor);
		if (flag)
		{
			OnPhaseIn(playerActor, ref _removedShadows);
		}
		return flag;
	}

	private void TriggerDiscard2Ability(CPlayerActor playerActor)
	{
		GetScenarioModifier("Discard2ScenarioAbility").PerformScenarioModifier(ScenarioScript.RoundNumber, playerActor);
	}

	private void CheckIsShouldShowNarrativeEvents(Point secondPassagePoint)
	{
		if (ScenarioManager.GetTile(secondPassagePoint).m_HexMap.RoomName == CE_RoomName && !_showedCERoomNarration)
		{
			_showCERoomNarration = true;
		}
		else if (ScenarioManager.GetTile(secondPassagePoint).m_HexMap.RoomName == DF_RoomName && !_showedDFRoomNarration)
		{
			_showDFRoomNarration = true;
		}
	}

	protected override void TickDeathInternal()
	{
		if (IsEarthDemon(_tickActor))
		{
			SpawnResourceProp(resourceID, SoilSamplePrefabName, _tickActor.Tile);
		}
	}

	private void CheckIfCanOfferTravelPassage(CPlayerActor tickPlayerActor)
	{
		Point tap = new Point(tickPlayerActor.Tile);
		if (PassagesPoints.Any((Tuple<Tuple<Point, Point>, Tuple<Point, Point>> pp) => pp.Item1.Item1 == tap || pp.Item1.Item2 == tap || pp.Item2.Item1 == tap || pp.Item2.Item2 == tap))
		{
			OfferTravelPassage(tickPlayerActor);
		}
	}

	private void UpdateWinObjective()
	{
		_collectSoilSamples.Current = ScenarioScript._css.Players.Sum((PlayerState ps) => ps.Actor.GetCharacterResourceAmount(resourceID));
		_collectSoilSamples.ScriptedIsComplete = _collectSoilSamples.Current == _collectSoilSamples.Total;
		_collectSoilSamples.CheckObjectiveComplete(ScenarioScript.PartySize);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void OfferTravelPassage(CPlayerActor playerActor)
	{
		List<CTile> list = new List<CTile> { ScenarioManager.GetTile(playerActor.ArrayIndex) };
		foreach (CHeroSummonActor item in ScenarioManager.Scenario.AllHeroSummons.FindAll((CHeroSummonActor hs) => hs.Summoner == playerActor))
		{
			if (!item.IsDead && ScenarioManager.GetTileDistance(playerActor.Tile, item.Tile) <= SummonsTravelPassageRange)
			{
				list.Add(item.Tile);
			}
		}
		if (IsDeathwalker(playerActor))
		{
			foreach (CTile item2 in GameState.GetTilesInRange(playerActor.ArrayIndex, 2).ToList())
			{
				if (item2.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow) != null)
				{
					list.Add(item2);
				}
			}
		}
		ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_069_Travel_Passage_1$", playerActor, list, TravelPassageCallback, 99, animateActor: false, null, canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: false, ignoreDoorLocks: false, stackAsSeparateAbilities: false, autoTrigger: false, new List<(string, TValue)>
		{
			("CantClearTargets", true),
			("CantReceiveTileSelection", true)
		});
	}

	private void TravelPassageCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles != null && selectedTiles.Count != 0 && selectedTiles.Contains(ScenarioManager.GetTile(_tickActor.ArrayIndex)))
		{
			TravelPassage(_tickActor as CPlayerActor, selectedTiles);
		}
	}

	private void TravelPassage(CPlayerActor playerActor, List<CTile> selectedTiles)
	{
		foreach (CTile item in selectedTiles.Distinct())
		{
			foreach (CActor actor in ScenarioManager.Scenario.AllActors)
			{
				if (actor.IsDead)
				{
					continue;
				}
				if (actor.Tile == item && (actor == playerActor || (actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == playerActor)))
				{
					ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
					{
						if (actor != null)
						{
							GameState.PhaseOutActor(actor);
						}
					}, "FH069_PHASE_OUT");
				}
				if (!IsDeathwalker(playerActor))
				{
					continue;
				}
				CObjectProp cObjectProp = item.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow);
				if (cObjectProp != null)
				{
					List<CObjectProp> list = new List<CObjectProp>();
					cObjectProp.DestroyProp(0f, sendMessageToClient: false, forceDestroy: true);
					list.Add(cObjectProp);
					CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(null, playerActor)
					{
						m_ActorDestroyingObstacle = playerActor,
						m_Tiles = list.Select((CObjectProp prop) => prop.PropTile).ToList(),
						m_DestroyedProps = list,
						m_DestroyDelay = 0f,
						m_DestroyObstacleAbility = null
					};
					ScenarioRuleClient.MessageHandler(message);
					_removedShadows++;
				}
			}
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (GameState.PendingScenarioModifierAbilities.Any())
		{
			return false;
		}
		if (message.MessageName == "FS_069_Sacred_Soil_Special_1_1")
		{
			if (!_showedCERoomNarration && _showCERoomNarration)
			{
				_showedCERoomNarration = true;
				_showCERoomNarration = false;
				return true;
			}
		}
		else if (message.MessageName == "FS_069_Sacred_Soil_Special_1_2" && !_showedDFRoomNarration && _showDFRoomNarration)
		{
			_showedDFRoomNarration = true;
			_showDFRoomNarration = false;
			return true;
		}
		return false;
	}

	private void SetNoGoldDropForEarthDemons()
	{
		foreach (CEnemyActor allEnemyMonster in ScenarioManager.Scenario.AllEnemyMonsters)
		{
			if (IsEarthDemon(allEnemyMonster))
			{
				allEnemyMonster.NoGoldDrop = true;
			}
		}
	}

	public override bool CanActorLootResources(CActor actor)
	{
		if (actor is CHeroSummonActor)
		{
			return true;
		}
		return base.CanActorLootResources(actor);
	}
}
