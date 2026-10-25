using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScenarioRuleLibrary;

[Serializable]
public class GameStateState : ISerializable
{
	public int RoundCount;

	public string CurrentActorGUID;

	public string ShortRestingPlayerGUID;

	public int ShortRestCardToBurnID;

	public string TurnActorGUID;

	public string LastActorGUID;

	public bool SkipMonsterTurns;

	public bool AutoRestorePlayerCards;

	public bool AutoSelectPlayerCards;

	public bool AutoTakeDamage;

	public float MulPlayerHealth;

	public List<(string, string)> TurnOrderSoFar;

	public List<string> InitiativeSortedActors = new List<string>();

	public List<string> PreInitiativeAdjustedPlayerActors = new List<string>();

	public int CheckAdjustInitiativeIndex;

	public GameState.EActionFlag[] ActionFlagsByActionSequenceType = new GameState.EActionFlag[Enum.GetNames(typeof(GameState.EActionSequenceType)).Length];

	public Dictionary<string, List<Tuple<int, GameState.EActionFlag>>[]> PlayedCards = new Dictionary<string, List<Tuple<int, GameState.EActionFlag>>[]>();

	public GameState.EActionInitiator CurrentActionInitiator;

	public CBaseCard.ActionType RoundAbilityCardActionType;

	public int RoundAbilityCardSelected;

	public CBaseCard.ActionType LastAbilityCardActionType;

	public int DamageInflictedThisTurn;

	public int DamagePreventedThisTurn;

	public List<string> ActorsMovedThisTurn;

	public List<TileIndex> HexesMovedThisTurn = new List<TileIndex>();

	public int ObstaclesDestroyedThisTurn;

	public List<string> ActorsKilledThisTurn;

	public List<string> ActorsKilledThisRound;

	public int TrophyTokensGainedThisRound;

	public List<(string victimGUID, string killerGUID)> EnemyKillTracker = new List<(string, string)>();

	public int LastDummyCardID;

	public List<(string, List<ElementInfusionBoardManager.EElement>)> MonsterConsumes = new List<(string, List<ElementInfusionBoardManager.EElement>)>();

	public List<(string, List<ElementInfusionBoardManager.EElement>)> MonsterInfusions = new List<(string, List<ElementInfusionBoardManager.EElement>)>();

	public int LastSpawnerStartRound;

	public bool PathfinderIceTerrain;

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PathfinderIceTerrain", PathfinderIceTerrain);
		info.AddValue("LastSpawnerStartRound", LastSpawnerStartRound);
		info.AddValue("ObstaclesDestroyedThisTurn", ObstaclesDestroyedThisTurn);
		info.AddValue("RoundCount", RoundCount);
		info.AddValue("CurrentActor", CurrentActorGUID);
		info.AddValue("ShortRestingPlayer", ShortRestingPlayerGUID);
		info.AddValue("ShortRestCardToBurn", ShortRestCardToBurnID);
		info.AddValue("TurnActor", TurnActorGUID);
		info.AddValue("LastActor", LastActorGUID);
		info.AddValue("SkipMonsterTurns", SkipMonsterTurns);
		info.AddValue("AutoRestorePlayerCards", AutoRestorePlayerCards);
		info.AddValue("AutoSelectPlayerCards", AutoSelectPlayerCards);
		info.AddValue("AutoTakeDamage", AutoTakeDamage);
		info.AddValue("MulPlayerHealth", MulPlayerHealth);
		info.AddValue("InitiativeSortedActors", InitiativeSortedActors);
		info.AddValue("TurnOrderSoFar", TurnOrderSoFar);
		info.AddValue("PreInitiativeAdjustedPlayerActors", PreInitiativeAdjustedPlayerActors);
		info.AddValue("CheckAdjustInitiativeIndex", CheckAdjustInitiativeIndex);
		info.AddValue("ActionFlagsByActionSequenceType", ActionFlagsByActionSequenceType);
		info.AddValue("PlayedCards", PlayedCards);
		info.AddValue("CurrentActionInitiator", CurrentActionInitiator);
		info.AddValue("RoundAbilityCardActionType", RoundAbilityCardActionType);
		info.AddValue("RoundAbilityCardSelected", RoundAbilityCardSelected);
		info.AddValue("LastAbilityCardActionType", LastAbilityCardActionType);
		info.AddValue("DamageInflictedThisTurn", DamageInflictedThisTurn);
		info.AddValue("DamagePreventedThisTurn", DamagePreventedThisTurn);
		info.AddValue("ActorsMovedThisTurn", ActorsMovedThisTurn);
		info.AddValue("HexesMovedThisTurn", HexesMovedThisTurn);
		info.AddValue("ActorsKilledThisTurn", ActorsKilledThisTurn);
		info.AddValue("ActorsKilledThisRound", ActorsKilledThisRound);
		info.AddValue("TrophyTokensGainedThisRound", TrophyTokensGainedThisRound);
		info.AddValue("EnemyKillTracker", EnemyKillTracker);
		info.AddValue("LastDummyCardID", LastDummyCardID);
		info.AddValue("MonsterConsumes", MonsterConsumes);
		info.AddValue("MonsterInfusions", MonsterInfusions);
	}

	public GameStateState(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "LastSpawnerStartRound":
					LastSpawnerStartRound = info.GetInt32("LastSpawnerStartRound");
					break;
				case "ObstaclesDestroyedThisTurn":
					ObstaclesDestroyedThisTurn = info.GetInt32("ObstaclesDestroyedThisTurn");
					break;
				case "RoundCount":
					RoundCount = info.GetInt32("RoundCount");
					break;
				case "CurrentActor":
					CurrentActorGUID = info.GetString("CurrentActor");
					break;
				case "ShortRestingPlayer":
					ShortRestingPlayerGUID = info.GetString("ShortRestingPlayer");
					break;
				case "ShortRestCardToBurn":
					ShortRestCardToBurnID = info.GetInt32("ShortRestCardToBurn");
					break;
				case "TurnActor":
					TurnActorGUID = info.GetString("TurnActor");
					break;
				case "LastActor":
					LastActorGUID = info.GetString("LastActor");
					break;
				case "SkipMonsterTurns":
					SkipMonsterTurns = info.GetBoolean("SkipMonsterTurns");
					break;
				case "AutoRestorePlayerCards":
					AutoRestorePlayerCards = info.GetBoolean("AutoRestorePlayerCards");
					break;
				case "PathfinderIceTerrain":
					PathfinderIceTerrain = info.GetBoolean("PathfinderIceTerrain");
					break;
				case "AutoSelectPlayerCards":
					AutoSelectPlayerCards = info.GetBoolean("AutoSelectPlayerCards");
					break;
				case "AutoTakeDamage":
					AutoTakeDamage = info.GetBoolean("AutoTakeDamage");
					break;
				case "MulPlayerHealth":
					MulPlayerHealth = info.GetSingle("MulPlayerHealth");
					break;
				case "MonsterConsumes":
					MonsterConsumes = (List<(string, List<ElementInfusionBoardManager.EElement>)>)info.GetValue("MonsterConsumes", typeof(List<(string, List<ElementInfusionBoardManager.EElement>)>));
					break;
				case "MonsterInfusions":
					MonsterInfusions = (List<(string, List<ElementInfusionBoardManager.EElement>)>)info.GetValue("MonsterInfusions", typeof(List<(string, List<ElementInfusionBoardManager.EElement>)>));
					break;
				case "InitiativeSortedActors":
					InitiativeSortedActors = (List<string>)info.GetValue("InitiativeSortedActors", typeof(List<string>));
					break;
				case "TurnOrderSoFar":
					TurnOrderSoFar = (List<(string, string)>)info.GetValue("TurnOrderSoFar", typeof(List<(string, string)>));
					break;
				case "PreInitiativeAdjustedPlayerActors":
					PreInitiativeAdjustedPlayerActors = (List<string>)info.GetValue("PreInitiativeAdjustedPlayerActors", typeof(List<string>));
					break;
				case "CheckAdjustInitiativeIndex":
					CheckAdjustInitiativeIndex = info.GetInt32("CheckAdjustInitiativeIndex");
					break;
				case "ActionFlagsByActionSequenceType":
					ActionFlagsByActionSequenceType = (GameState.EActionFlag[])info.GetValue("ActionFlagsByActionSequenceType", typeof(GameState.EActionFlag[]));
					break;
				case "PlayedCards":
				{
					object value = info.GetValue("PlayedCards", typeof(object));
					if (value.GetType() != typeof(Dictionary<string, List<Tuple<int, GameState.EActionFlag>>[]>))
					{
						PlayedCards = new Dictionary<string, List<Tuple<int, GameState.EActionFlag>>[]>();
					}
					else
					{
						PlayedCards = (Dictionary<string, List<Tuple<int, GameState.EActionFlag>>[]>)value;
					}
					break;
				}
				case "CurrentActionInitiator":
					CurrentActionInitiator = (GameState.EActionInitiator)info.GetValue("CurrentActionInitiator", typeof(GameState.EActionInitiator));
					break;
				case "RoundAbilityCardActionType":
					RoundAbilityCardActionType = (CBaseCard.ActionType)info.GetValue("RoundAbilityCardActionType", typeof(CBaseCard.ActionType));
					break;
				case "RoundAbilityCardSelected":
					RoundAbilityCardSelected = info.GetInt32("RoundAbilityCardSelected");
					break;
				case "LastAbilityCardActionType":
					LastAbilityCardActionType = (CBaseCard.ActionType)info.GetValue("LastAbilityCardActionType", typeof(CBaseCard.ActionType));
					break;
				case "DamageInflictedThisTurn":
					DamageInflictedThisTurn = info.GetInt32("DamageInflictedThisTurn");
					break;
				case "DamagePreventedThisTurn":
					DamagePreventedThisTurn = info.GetInt32("DamagePreventedThisTurn");
					break;
				case "ActorsMovedThisTurn":
					ActorsMovedThisTurn = (List<string>)info.GetValue("ActorsMovedThisTurn", typeof(List<string>));
					break;
				case "HexesMovedThisTurn":
					HexesMovedThisTurn = (List<TileIndex>)info.GetValue("HexesMovedThisTurn", typeof(List<TileIndex>));
					break;
				case "ActorsKilledThisTurn":
					ActorsKilledThisTurn = (List<string>)info.GetValue("ActorsKilledThisTurn", typeof(List<string>));
					break;
				case "ActorsKilledThisRound":
					ActorsKilledThisRound = (List<string>)info.GetValue("ActorsKilledThisRound", typeof(List<string>));
					break;
				case "EnemyKillTracker":
					EnemyKillTracker = (List<(string, string)>)info.GetValue("EnemyKillTracker", typeof(List<(string, string)>));
					break;
				case "TrophyTokensGainedThisRound":
					TrophyTokensGainedThisRound = info.GetInt32("TrophyTokensGainedThisRound");
					break;
				case "LastDummyCardID":
					LastDummyCardID = info.GetInt32("LastDummyCardID");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize GameStateState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw;
			}
		}
	}

	public GameStateState()
	{
	}
}
