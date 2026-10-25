using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH031 : ScenarioScript
{
	private int _removedShadows;

	private CObjective_ActorsEscaped _escapeObjective;

	private CObjective_ActorsEscaped _escapeObjective2;

	private CObjective_ActorsEscaped _escapeObjective3;

	private CObjective_ActorsEscaped _escapeObjectiveDummy;

	private List<CTile> _escapeTiles;

	private List<CTile> _secondStartTiles;

	private List<CTile> _thirdStartTiles;

	private CMap _room1;

	private CMap _room2;

	private CMap _room3;

	[StateDump(Skip = true)]
	private List<CObjectProp> _conduits;

	private string _conduitLocKey;

	private bool _revealedNewRoom;

	[StateDump(Skip = true)]
	private List<CPlayerActor> _players = new List<CPlayerActor>();

	private CScenarioModifier _modifier3;

	private CScenarioModifier _modifier4;

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Round)
		{
			return false;
		}
		Dictionary<CMap, int> dictionary = new Dictionary<CMap, int>();
		foreach (CMap item in dictionary.Keys.ToList())
		{
			dictionary[item] = 0;
		}
		foreach (EnemyState monster in ScenarioManager.CurrentScenarioState.Monsters)
		{
			CTile cTile = ScenarioManager.Tiles[monster.Location.X, monster.Location.Y];
			CMap[] array = new CMap[2] { cTile.m_HexMap, cTile.m_Hex2Map };
			foreach (CMap cMap in array)
			{
				if (cMap != null)
				{
					if (!dictionary.TryGetValue(cMap, out var value))
					{
						value = (dictionary[cMap] = 0);
					}
					if (IsAlive(monster))
					{
						dictionary[cMap] = value + 1;
					}
				}
			}
		}
		Trace("Checking message trigger for " + message.MessageName + " " + message.TitleKey + "...", null, 57);
		foreach (KeyValuePair<CMap, int> item2 in dictionary)
		{
			Trace($"num alive on room '{item2.Key.RoomName}' = {item2.Value}", null, 61);
			if (item2.Value == 0)
			{
				if (item2.Key.RoomName == "First" && item2.Key.Destroyed && message.MessageName.Contains("31_1"))
				{
					Trace("...success", null, 66);
					return true;
				}
				if (item2.Key.RoomName == "Second" && item2.Key.Destroyed && message.MessageName.Contains("31_2"))
				{
					Trace("...success", null, 71);
					return true;
				}
			}
		}
		Trace("...failed", null, 77);
		return false;
	}

	protected override bool Initialize()
	{
		_escapeTiles = new List<CTile>
		{
			ScenarioManager.Tiles[7, 11],
			ScenarioManager.Tiles[7, 12],
			ScenarioManager.Tiles[6, 12],
			ScenarioManager.Tiles[17, 13],
			ScenarioManager.Tiles[17, 14],
			ScenarioManager.Tiles[18, 13]
		};
		_secondStartTiles = new List<CTile>
		{
			ScenarioManager.Tiles[11, 8],
			ScenarioManager.Tiles[12, 8],
			ScenarioManager.Tiles[11, 9],
			ScenarioManager.Tiles[11, 10],
			ScenarioManager.Tiles[12, 10]
		};
		_thirdStartTiles = new List<CTile>
		{
			ScenarioManager.Tiles[9, 4],
			ScenarioManager.Tiles[8, 4],
			ScenarioManager.Tiles[8, 3],
			ScenarioManager.Tiles[8, 2],
			ScenarioManager.Tiles[9, 2]
		};
		_room1 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "First");
		_room2 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Second");
		_room3 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Third");
		_conduits = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => prop is CObjectDifficultTerrain).ToList();
		_conduitLocKey = _conduits[0].PropCustomLocKey;
		_modifier3 = GetScenarioModifier("Modifier3");
		_modifier4 = GetScenarioModifier("Modifier4");
		_modifier4.CustomLocKeyArgs = new List<object> { ScenarioManager.HazardousTerrainDamage };
		_escapeObjective = GetObjective<CObjective_ActorsEscaped>("Room1");
		_escapeObjective2 = GetObjective<CObjective_ActorsEscaped>("Room2");
		_escapeObjective3 = GetObjective<CObjective_ActorsEscaped>("Escape");
		_escapeObjectiveDummy = GetObjective<CObjective_ActorsEscaped>("EscapeDummy");
		_players.AddRange(ScenarioManager.Scenario.AllPlayers);
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_removedShadows);
		return JsonConvert.SerializeObject(new string[1] { text });
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(storage);
			if (array.Length != 0)
			{
				_removedShadows = JsonConvert.DeserializeObject<int>(array[0]);
			}
		}
		if (!_escapeObjectiveDummy.IsActive)
		{
			return;
		}
		_escapeObjectiveDummy.EscapePositions.Clear();
		if (!_escapeObjective3.IsActive)
		{
			if (_escapeObjective2.IsActive)
			{
				_escapeObjectiveDummy.EscapePositions.AddRange(_escapeObjective2.EscapePositions);
			}
			else if (_escapeObjective.IsActive)
			{
				_escapeObjectiveDummy.EscapePositions.AddRange(_escapeObjective.EscapePositions);
			}
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 174);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected void TickPhaseStartRound()
	{
		if (_revealedNewRoom)
		{
			LoseCards();
			CRefreshStartingPositions_MessageData message = new CRefreshStartingPositions_MessageData();
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected void TickPhaseEndRound()
	{
		if (OncePerRound())
		{
			ApplyHazardousTerrainEffect();
			if (_players.All((CPlayerActor player) => player.PhasedOut))
			{
				MoveToNextRoom();
			}
		}
	}

	private bool GetNearestTileToSpawn(CActor actor, out CTile nearestTile)
	{
		Point arrayIndex = actor.ArrayIndex;
		CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
		nearestTile = null;
		int num = 9999999;
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile2 in tiles)
		{
			if (cTile2 != null && cTile.m_HexMap == cTile2.m_HexMap)
			{
				int tileDistance = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y);
				if (CSpawner.IsEmptyTile(cTile2) && ScenarioManager.PathFinder.Nodes[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y].Walkable && tileDistance < num)
				{
					num = tileDistance;
					nearestTile = cTile2;
				}
			}
		}
		if (nearestTile == null)
		{
			return false;
		}
		return true;
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor.Type == CActor.EType.Enemy && ScenarioManager.Scenario.Enemies.Count == 1 && ScenarioManager.Scenario.Enemies[0].ActorGuid == _tickActor.ActorGuid)
		{
			CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.Revealed && !map.Destroyed);
			if (cMap.RoomName == "First")
			{
				_escapeObjective.SetActivation(active: true);
				_escapeObjectiveDummy.EscapePositions.Clear();
				_escapeObjectiveDummy.EscapePositions.AddRange(_escapeObjective.EscapePositions);
			}
			if (cMap.RoomName == "Second")
			{
				_escapeObjective2.SetActivation(active: true);
				_escapeObjectiveDummy.EscapePositions.Clear();
				_escapeObjectiveDummy.EscapePositions.AddRange(_escapeObjective2.EscapePositions);
			}
			if (cMap.RoomName == "Third")
			{
				_escapeObjectiveDummy.SetActivation(active: false);
				_escapeObjective3.SetActivation(active: true);
			}
		}
	}

	protected void TickPhaseEndTurn()
	{
		if (_tickActor.Type != CActor.EType.Player || _tickActor.PhasedOut || ScenarioManager.Scenario.AllAliveMonsters.Count != 0)
		{
			return;
		}
		if (ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.Revealed && !map.Destroyed).RoomName == "Third")
		{
			TryEscapeMap();
		}
		else
		{
			if (!_escapeTiles.Contains(_tickActorTile) || !_tickActor.AllowEscape || !ScenarioManager.CurrentScenarioState.IsOnEscapeZone(_tickActor))
			{
				return;
			}
			GameState.PhaseOutActor(_tickActor);
			RemoveAndCacheShadows(_tickActor as CPlayerActor, ref _removedShadows);
			List<CHeroSummonActor> list = ScenarioManager.Scenario.HeroSummons.Where((CHeroSummonActor summon) => summon.Summoner.ActorGuid == _tickActor.ActorGuid).ToList();
			if (list.Count() > 0)
			{
				foreach (CHeroSummonActor item in list)
				{
					GameState.PhaseOutActor(item);
				}
			}
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	private void MoveToNextRoom()
	{
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.Revealed && !map.Destroyed);
		if (cMap.RoomName == "First")
		{
			_room2.Reveal(initial: false);
			_modifier3.SetHiddenState(shouldSetHidden: false);
			_modifier4.SetHiddenState(shouldSetHidden: false);
			ScenarioManager.CurrentScenarioState.IsSelectingPositions = true;
			int num = 0;
			foreach (CPlayerActor player in _players)
			{
				player.ArrayIndex = _secondStartTiles[num].m_ArrayIndex;
				GameState.PhaseInActor(player);
				num++;
			}
			foreach (CPlayerActor player2 in _players)
			{
				OnPhaseIn(player2, ref _removedShadows);
			}
			_room1.Destroyed = true;
		}
		else if (cMap.RoomName == "Second")
		{
			_room3.Reveal(initial: false);
			ScenarioManager.CurrentScenarioState.IsSelectingPositions = true;
			int num2 = 0;
			foreach (CPlayerActor player3 in _players)
			{
				player3.ArrayIndex = _thirdStartTiles[num2].m_ArrayIndex;
				GameState.PhaseInActor(player3);
				num2++;
			}
			foreach (CPlayerActor player4 in _players)
			{
				OnPhaseIn(player4, ref _removedShadows);
			}
			_room2.Destroyed = true;
		}
		CDestroyRoom_MessageData cDestroyRoom_MessageData = new CDestroyRoom_MessageData();
		cDestroyRoom_MessageData.m_MapToDestroy = cMap;
		ScenarioRuleClient.MessageHandler(cDestroyRoom_MessageData);
		_revealedNewRoom = true;
	}

	private void LoseCards()
	{
		foreach (CPlayerActor item in _players.Where((CPlayerActor player) => !player.PhasedOut).ToList())
		{
			CAbilityLoseCards cAbilityLoseCards = CAbility.CreateAbility(CAbility.EAbilityType.LoseCards, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 1) as CAbilityLoseCards;
			cAbilityLoseCards.TargetingActor = item;
			cAbilityLoseCards.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbilityLoseCards.MiscAbilityData.AutotriggerAbility = true;
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbilityLoseCards, item));
		}
		_revealedNewRoom = false;
	}

	private void PlayHazzardousTerrainVFX(CObjectProp hazzardTerrain)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = "ElectricityBurstRepair",
			m_ArrayIndex = hazzardTerrain.ArrayIndex.ToPoint()
		});
	}

	private void ApplyHazardousTerrainEffect()
	{
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			foreach (CObjectProp conduit in _conduits)
			{
				if (!conduit.IsDestroyed && ScenarioManager.GetTileDistance(conduit.PropTile.m_ArrayIndex.X, conduit.PropTile.m_ArrayIndex.Y, allAliveActor.ArrayIndex.X, allAliveActor.ArrayIndex.Y) <= 1 && !allAliveActor.PhasedOut)
				{
					if (allAliveActor is CPlayerActor || allAliveActor is CHeroSummonActor)
					{
						PlayHazzardousTerrainVFX(conduit);
						DealScenarioDamage(allAliveActor, ScenarioManager.HazardousTerrainDamage, isTrapDamage: false, isTerrainDamage: false, _conduitLocKey);
						break;
					}
					if (allAliveActor is CEnemyActor)
					{
						PlayHazzardousTerrainVFX(conduit);
						CAbilityHeal obj = CAbility.CreateAbility(CAbility.EAbilityType.Heal, CAbilityFilterContainer.CreateDefaultFilter(), allAliveActor.Type == CActor.EType.Enemy, isTargetedAbility: true, ScenarioManager.HazardousTerrainDamage) as CAbilityHeal;
						obj.TargetingActor = allAliveActor;
						obj.ApplyToActor(allAliveActor);
						break;
					}
				}
			}
		}
	}
}
