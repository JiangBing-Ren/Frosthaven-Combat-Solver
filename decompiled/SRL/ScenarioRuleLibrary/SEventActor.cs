using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActor : SEvent
{
	[Serializable]
	public class SEventActorBaseData : ISerializable
	{
		public ESESubTypeActor ActorSubType { get; private set; }

		public bool MainActorResting { get; private set; }

		public CActor.EType MainActorType { get; private set; }

		public string MainActorGuid { get; private set; }

		public string MainActorClassID { get; private set; }

		public string MainActorSummonerID { get; private set; }

		public int Gold { get; private set; }

		public int XP { get; private set; }

		public LootState Loot { get; private set; }

		public int Level { get; private set; }

		public List<PositiveConditionPair> MainActorPositiveConditions { get; private set; }

		public List<NegativeConditionPair> MainActorNegativeConditions { get; private set; }

		public bool PlayedThisRound { get; private set; }

		public bool IsDead { get; private set; }

		public CActor.ECauseOfDeath CauseOfDeath { get; private set; }

		public bool MainActorEnemySummon { get; private set; }

		public string SecondaryActorGUID { get; private set; }

		public string SecondaryActorClassID { get; private set; }

		public CActor.EType? SecondaryActorType { get; private set; }

		public string SecondaryActorSummonerID { get; private set; }

		public int CardID { get; private set; }

		public CBaseCard.ECardType CardType { get; private set; }

		public CAbility.EAbilityType AbilityType { get; private set; }

		public string ActingAbilityName { get; private set; }

		public int AbilityStrength { get; private set; }

		public bool SecondaryActorEnemySummon { get; private set; }

		public bool Doom { get; private set; }

		public string SecondaryActorMonsterType { get; private set; }

		public string MainActorMonsterType { get; private set; }

		public List<PositiveConditionPair> SecondaryActorPositiveConditions { get; private set; }

		public List<NegativeConditionPair> SecondaryActorNegativeConditions { get; private set; }

		public int MainActorHealth { get; private set; }

		public int MainActorMaxHealth { get; private set; }

		public int SecondaryActorHealth { get; private set; }

		public int SecondaryActorMaxHealth { get; private set; }

		public int LastDamageAmount { get; private set; }

		public int PreDamageHealth { get; private set; }

		public DateTime TimeStamp { get; private set; }

		public List<string> Items { get; private set; }

		public bool AttackerAtDisadvantage { get; private set; }

		public bool TargetAdjacent { get; private set; }

		public int MainActorAllyAdjacent { get; private set; }

		public int MainActorEnemyAdjacent { get; private set; }

		public int MainActorObstacleAdjacent { get; private set; }

		public int MainActorPlayerAdjacent { get; private set; }

		public bool MainActorWallAdjacent { get; private set; }

		public int SecondaryActorAllyAdjacent { get; private set; }

		public int SecondaryActorEnemyAdjacent { get; private set; }

		public int SecondaryActorObstacleAdjacent { get; private set; }

		public bool SecondaryActorWallAdjacent { get; private set; }

		public int SecondaryActorPlayerAdjacent { get; private set; }

		public bool HasFavorite { get; private set; }

		public string TurnActorGUID { get; private set; }

		public Point Position { get; private set; }

		public string ControlledByClassID { get; private set; }

		public string ControlledByActorGuid { get; private set; }

		public uint ResonanceTokens { get; private set; }

		public CPlayerActor.EPressure Pressure { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Pressure", Pressure);
			info.AddValue("ResonanceTokens", ResonanceTokens);
			info.AddValue("ControlledByActorGuid", ControlledByActorGuid);
			info.AddValue("ActorSubType", ActorSubType);
			info.AddValue("MainActorType", MainActorType);
			info.AddValue("MainActorGuid", MainActorGuid);
			info.AddValue("MainActorClassID", MainActorClassID);
			info.AddValue("MainActorSummonerID", MainActorSummonerID);
			info.AddValue("Gold", Gold);
			info.AddValue("XP", XP);
			info.AddValue("Loot", Loot);
			info.AddValue("Level", Level);
			info.AddValue("MainActorPositiveConditions", MainActorPositiveConditions);
			info.AddValue("MainActorNegativeConditions", MainActorNegativeConditions);
			info.AddValue("PlayedThisRound", PlayedThisRound);
			info.AddValue("IsDead", IsDead);
			info.AddValue("CauseOfDeath", CauseOfDeath);
			info.AddValue("MainActorEnemySummon", MainActorEnemySummon);
			info.AddValue("SecondaryActorGUID", SecondaryActorGUID);
			info.AddValue("SecondaryActorClassID", SecondaryActorClassID);
			info.AddValue("SecondaryActorType", SecondaryActorType);
			info.AddValue("SecondaryActorSummonerID", SecondaryActorSummonerID);
			info.AddValue("CardID", CardID);
			info.AddValue("CardType", CardType);
			info.AddValue("AbilityType", AbilityType);
			info.AddValue("ActingAbilityName", ActingAbilityName);
			info.AddValue("AbilityStrength", AbilityStrength);
			info.AddValue("Doom", Doom);
			info.AddValue("SecondaryActorMonsterType", SecondaryActorMonsterType);
			info.AddValue("SecondaryActorPositiveConditions", SecondaryActorPositiveConditions);
			info.AddValue("SecondaryActorNegativeConditions", SecondaryActorNegativeConditions);
			info.AddValue("MainActorHealth", MainActorHealth);
			info.AddValue("MainActorMaxHealth", MainActorMaxHealth);
			info.AddValue("SecondaryActorHealth", SecondaryActorHealth);
			info.AddValue("SecondaryActorMaxHealth", SecondaryActorMaxHealth);
			info.AddValue("LastDamageAmount", LastDamageAmount);
			info.AddValue("PreDamageHealth", PreDamageHealth);
			info.AddValue("TimeStamp", TimeStamp);
			info.AddValue("AttackerAtDisadvantage", AttackerAtDisadvantage);
			info.AddValue("TargetAdjacent", TargetAdjacent);
			info.AddValue("MainActorPlayerAdjacent", MainActorPlayerAdjacent);
			info.AddValue("MainActorAllyAdjacent", MainActorAllyAdjacent);
			info.AddValue("MainActorEnemyAdjacent", MainActorEnemyAdjacent);
			info.AddValue("MainActorObstacleAdjacent", MainActorObstacleAdjacent);
			info.AddValue("MainActorWallAdjacent", MainActorWallAdjacent);
			info.AddValue("SecondaryActorAllyAdjacent", SecondaryActorAllyAdjacent);
			info.AddValue("SecondaryActorEnemyAdjacent", SecondaryActorEnemyAdjacent);
			info.AddValue("SecondaryActorObstacleAdjacent", SecondaryActorObstacleAdjacent);
			info.AddValue("SecondaryActorWallAdjacent", SecondaryActorWallAdjacent);
			info.AddValue("SecondaryActorPlayerAdjacent", SecondaryActorPlayerAdjacent);
			info.AddValue("HasFavorite", HasFavorite);
			info.AddValue("MainActorMonsterType", MainActorMonsterType);
			info.AddValue("TurnActorGUID", TurnActorGUID);
			info.AddValue("Position", Position);
			info.AddValue("ControlledByClassID", ControlledByClassID);
			info.AddValue("MainActorResting", MainActorResting);
		}

		public SEventActorBaseData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "Pressure":
						Pressure = (CPlayerActor.EPressure)info.GetValue("Pressure", typeof(CPlayerActor.EPressure));
						break;
					case "ResonanceTokens":
						ResonanceTokens = info.GetUInt32("ResonanceTokens");
						break;
					case "MainActorResting":
						MainActorResting = info.GetBoolean("MainActorResting");
						break;
					case "ControlledByClassID":
						ControlledByClassID = info.GetString("ControlledByClassID");
						break;
					case "ControlledByActorGuid":
						ControlledByActorGuid = info.GetString("ControlledByActorGuid");
						break;
					case "Position":
						Position = (Point)info.GetValue("Position", typeof(Point));
						break;
					case "ActorSubType":
						ActorSubType = (ESESubTypeActor)info.GetValue("ActorSubType", typeof(ESESubTypeActor));
						break;
					case "MainActorType":
						MainActorType = (CActor.EType)info.GetValue("MainActorType", typeof(CActor.EType));
						break;
					case "MainActorGuid":
						MainActorGuid = info.GetString("MainActorGuid");
						break;
					case "MainActorClassID":
						MainActorClassID = info.GetString("MainActorClassID");
						break;
					case "MainActorSummonerID":
						MainActorSummonerID = info.GetString("MainActorSummonerID");
						break;
					case "MainActorEnemySummon":
						MainActorEnemySummon = info.GetBoolean("MainActorEnemySummon");
						break;
					case "MainActorMonsterType":
						MainActorMonsterType = info.GetString("MainActorMonsterType");
						break;
					case "TurnActorGUID":
						TurnActorGUID = info.GetString("TurnActorGUID");
						break;
					case "Gold":
						Gold = info.GetInt32("Gold");
						break;
					case "XP":
						XP = info.GetInt32("XP");
						break;
					case "Loot":
						Loot = (LootState)info.GetValue("Loot", typeof(LootState));
						break;
					case "Level":
						Level = info.GetInt32("Level");
						break;
					case "MainActorPositiveConditions":
						MainActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("MainActorPositiveConditions", typeof(List<PositiveConditionPair>));
						break;
					case "MainActorNegativeConditions":
						MainActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("MainActorNegativeConditions", typeof(List<NegativeConditionPair>));
						break;
					case "PlayedThisRound":
						PlayedThisRound = info.GetBoolean("PlayedThisRound");
						break;
					case "IsDead":
						IsDead = info.GetBoolean("IsDead");
						break;
					case "CauseOfDeath":
						CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
						break;
					case "SecondaryActorGUID":
						SecondaryActorGUID = info.GetString("SecondaryActorGUID");
						break;
					case "SecondaryActorClassID":
						SecondaryActorClassID = info.GetString("SecondaryActorClassID");
						break;
					case "SecondaryActorType":
						SecondaryActorType = (CActor.EType?)info.GetValue("SecondaryActorType", typeof(CActor.EType?));
						break;
					case "SecondaryActorSummonerID":
						SecondaryActorSummonerID = info.GetString("SecondaryActorSummonerID");
						break;
					case "CardID":
						CardID = info.GetInt32("CardID");
						break;
					case "CardType":
						CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
						break;
					case "AbilityType":
						AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
						break;
					case "ActingAbilityName":
						ActingAbilityName = info.GetString("ActingAbilityName");
						break;
					case "AbilityStrength":
						AbilityStrength = info.GetInt32("AbilityStrength");
						break;
					case "SecondaryActorEnemySummon":
						SecondaryActorEnemySummon = info.GetBoolean("SecondaryActorEnemySummon");
						break;
					case "Doom":
						Doom = info.GetBoolean("Doom");
						break;
					case "SecondaryActorMonsterType":
						SecondaryActorMonsterType = info.GetString("SecondaryActorMonsterType");
						break;
					case "SecondaryActorPositiveConditions":
						SecondaryActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("SecondaryActorPositiveConditions", typeof(List<PositiveConditionPair>));
						break;
					case "SecondaryActorNegativeConditions":
						SecondaryActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("SecondaryActorNegativeConditions", typeof(List<NegativeConditionPair>));
						break;
					case "MainActorHealth":
						MainActorHealth = info.GetInt32("MainActorHealth");
						break;
					case "MainActorMaxHealth":
						MainActorMaxHealth = info.GetInt32("MainActorMaxHealth");
						break;
					case "SecondaryActorHealth":
						SecondaryActorHealth = info.GetInt32("SecondaryActorHealth");
						break;
					case "SecondaryActorMaxHealth":
						SecondaryActorMaxHealth = info.GetInt32("SecondaryActorMaxHealth");
						break;
					case "LastDamageAmount":
						LastDamageAmount = info.GetInt32("LastDamageAmount");
						break;
					case "PreDamageHealth":
						PreDamageHealth = info.GetInt32("PreDamageHealth");
						break;
					case "TimeStamp":
						TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
						break;
					case "AttackerAtDisadvantage":
						AttackerAtDisadvantage = info.GetBoolean("AttackerAtDisadvantage");
						break;
					case "TargetAdjacent":
						TargetAdjacent = info.GetBoolean("TargetAdjacent");
						break;
					case "MainActorAllyAdjacent":
						MainActorAllyAdjacent = info.GetInt32("MainActorAllyAdjacent");
						break;
					case "MainActorEnemyAdjacent":
						MainActorEnemyAdjacent = info.GetInt32("MainActorEnemyAdjacent");
						break;
					case "MainActorObstacleAdjacent":
						MainActorObstacleAdjacent = info.GetInt32("MainActorObstacleAdjacent");
						break;
					case "MainActorWallAdjacent":
						MainActorWallAdjacent = info.GetBoolean("MainActorWallAdjacent");
						break;
					case "MainActorPlayerAdjacent":
						MainActorPlayerAdjacent = info.GetInt32("MainActorPlayerAdjacent");
						break;
					case "SecondaryActorAllyAdjacent":
						SecondaryActorAllyAdjacent = info.GetInt32("SecondaryActorAllyAdjacent");
						break;
					case "SecondaryActorEnemyAdjacent":
						SecondaryActorEnemyAdjacent = info.GetInt32("SecondaryActorEnemyAdjacent");
						break;
					case "SecondaryActorObstacleAdjacent":
						SecondaryActorObstacleAdjacent = info.GetInt32("SecondaryActorObstacleAdjacent");
						break;
					case "SecondaryActorWallAdjacent":
						SecondaryActorWallAdjacent = info.GetBoolean("SecondaryActorWallAdjacent");
						break;
					case "SecondaryActorPlayerAdjacent":
						SecondaryActorPlayerAdjacent = info.GetInt32("SecondaryActorPlayerAdjacent");
						break;
					case "HasFavorite":
						HasFavorite = info.GetBoolean("HasFavorite");
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventActor entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SEventActorBaseData(ESESubTypeActor actorEventSubType, CActor.EType mainActorType, string mainActorGuid, string mainActorClassID, int health, int gold, int xp, LootState loot, int level, List<PositiveConditionPair> mainActorPositiveConditions, List<NegativeConditionPair> mainActorNegativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool mainActorEnemySummon, string secondaryActorGUID = "", string secondaryActorClassID = "", CActor.EType? secondaryActorType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool secondaryActorEnemySummon = false, List<PositiveConditionPair> secondaryActorPositiveConditions = null, List<NegativeConditionPair> secondaryActorNegativeConditions = null, string text = "", bool doNotSerialize = false, int maxHealth = 0, int lastDamageAmount = 0, int preDamageHealth = 0, List<string> items = null, bool doom = false, string secondaryActorMonsterType = "", string mainActorMonsterType = "", bool attackerDisadvantage = false, bool targetAdjacent = false, int mainActorAllyAdjacent = 0, int mainActorEnemyAdjacent = 0, int mainActorObstacleAdjacent = 0, bool mainActorWallAdjacent = false, bool hasFavorite = false, Point position = default(Point), string controlledByClassID = null, string controlledByActorGuid = null, int mainActorPlayerAdjacent = 0, int secondaryActorHealth = 0, int secondaryActorMaxHealth = 0, int secondaryActorAllyAdjacent = 0, int secondaryActorEnemyAdjacent = 0, int secondaryActorObstacleAdjacent = 0, bool secondaryActorWallAdjacent = false, int secondaryActorPlayerAdjacent = 0, bool mainActorResting = false, uint resonance = 0u, CPlayerActor.EPressure pressure = CPlayerActor.EPressure.None)
		{
			SEventActorBaseData sEventActorBaseData = this;
			Pressure = pressure;
			ResonanceTokens = resonance;
			MainActorResting = mainActorResting;
			ControlledByClassID = controlledByClassID;
			ControlledByActorGuid = controlledByActorGuid;
			Position = position;
			TurnActorGUID = GameState.TurnActor?.ActorGuid;
			ActorSubType = actorEventSubType;
			MainActorType = mainActorType;
			MainActorGuid = mainActorGuid;
			MainActorClassID = mainActorClassID;
			Gold = gold;
			XP = xp;
			if (loot != null)
			{
				Loot = new LootState();
				Loot.AccumulateLootFrom(loot);
			}
			Level = level;
			MainActorPositiveConditions = mainActorPositiveConditions?.ToList();
			MainActorNegativeConditions = mainActorNegativeConditions?.ToList();
			PlayedThisRound = playedThisRound;
			IsDead = isDead;
			CauseOfDeath = causeOfDeath;
			MainActorEnemySummon = mainActorEnemySummon;
			SecondaryActorGUID = secondaryActorGUID;
			SecondaryActorClassID = secondaryActorClassID;
			SecondaryActorType = secondaryActorType;
			CardID = cardID;
			CardType = cardType;
			AbilityType = abilityType;
			ActingAbilityName = actingAbilityName;
			AbilityStrength = abilityStrength;
			SecondaryActorEnemySummon = secondaryActorEnemySummon;
			SecondaryActorPositiveConditions = secondaryActorPositiveConditions?.ToList();
			SecondaryActorNegativeConditions = secondaryActorNegativeConditions?.ToList();
			MainActorHealth = health;
			MainActorMaxHealth = maxHealth;
			SecondaryActorHealth = secondaryActorHealth;
			SecondaryActorMaxHealth = secondaryActorMaxHealth;
			LastDamageAmount = lastDamageAmount;
			PreDamageHealth = preDamageHealth;
			TimeStamp = DateTime.Now;
			Items = items;
			Doom = doom;
			SecondaryActorMonsterType = secondaryActorMonsterType;
			MainActorMonsterType = mainActorMonsterType;
			AttackerAtDisadvantage = attackerDisadvantage;
			HasFavorite = hasFavorite;
			TargetAdjacent = targetAdjacent;
			MainActorAllyAdjacent = mainActorAllyAdjacent;
			MainActorEnemyAdjacent = mainActorEnemyAdjacent;
			MainActorObstacleAdjacent = mainActorObstacleAdjacent;
			MainActorWallAdjacent = mainActorWallAdjacent;
			MainActorPlayerAdjacent = mainActorPlayerAdjacent;
			SecondaryActorAllyAdjacent = secondaryActorAllyAdjacent;
			SecondaryActorEnemyAdjacent = secondaryActorEnemyAdjacent;
			SecondaryActorObstacleAdjacent = secondaryActorObstacleAdjacent;
			SecondaryActorWallAdjacent = secondaryActorWallAdjacent;
			SecondaryActorPlayerAdjacent = secondaryActorPlayerAdjacent;
			if (MainActorType == CActor.EType.HeroSummon)
			{
				CHeroSummonActor summons = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == mainActorGuid);
				if (summons != null)
				{
					CActor cActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == summons.Summoner.ActorGuid);
					if (cActor != null)
					{
						MainActorSummonerID = cActor.Class.ID;
					}
				}
			}
			if (SecondaryActorType != CActor.EType.HeroSummon)
			{
				return;
			}
			CHeroSummonActor summons2 = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == sEventActorBaseData.SecondaryActorGUID);
			if (summons2 != null)
			{
				CActor cActor2 = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == summons2.Summoner.ActorGuid);
				if (cActor2 != null)
				{
					SecondaryActorSummonerID = cActor2.Class.ID;
				}
			}
		}

		public SEventActorBaseData(ESESubTypeActor actorEventSubType, CActor mainActor, string secondaryActorGUID = "", string secondaryActorClassID = "", CActor.EType? secondaryActorType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool secondaryActorEnemySummon = false, List<PositiveConditionPair> secondaryActorPositiveConditions = null, List<NegativeConditionPair> secondaryActorNegativeConditions = null, string text = "", bool doNotSerialize = false, int lastDamageAmount = 0, int preDamageHealth = 0, List<string> items = null, string secondaryActorMonsterType = "", bool attackerDisadvantage = false, bool targetAdjacent = false, Point position = default(Point), int secondaryActorHealth = 0, int secondaryActorMaxHealth = 0)
			: this(actorEventSubType, mainActor.OriginalType, mainActor.ActorGuid, mainActor.Class.ID, mainActor.Health, mainActor.Gold, mainActor.XP, mainActor.TotalLoot, mainActor.Level, mainActor.Tokens.CheckPositiveTokens, mainActor.Tokens.CheckNegativeTokens, mainActor.PlayedThisRound, mainActor.IsDead, mainActor.CauseOfDeath, mainActorEnemySummon: false, secondaryActorGUID, secondaryActorClassID, secondaryActorType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, secondaryActorEnemySummon, secondaryActorPositiveConditions, secondaryActorNegativeConditions, text, doNotSerialize, mainActor.MaxHealth, lastDamageAmount, preDamageHealth, items, mainActor.IsDoomed, secondaryActorMonsterType, attackerDisadvantage: attackerDisadvantage, targetAdjacent: targetAdjacent, hasFavorite: mainActor.CharacterHasResourceAny("Favorite", 1), mainActorMonsterType: (mainActor?.MonsterClass == null) ? "" : mainActor.MonsterClass.MonsterType.ToString(), mainActorAllyAdjacent: 0, mainActorEnemyAdjacent: 0, mainActorObstacleAdjacent: 0, mainActorWallAdjacent: false, position: (position != default(Point)) ? position : (mainActor?.ArrayIndex ?? default(Point)), controlledByClassID: mainActor?.MindControlAbility?.TargetingActor?.Class?.ID, controlledByActorGuid: mainActor?.MindControlAbility?.TargetingActor?.ActorGuid, mainActorPlayerAdjacent: 0, secondaryActorHealth: secondaryActorHealth, secondaryActorMaxHealth: secondaryActorMaxHealth, secondaryActorAllyAdjacent: 0, secondaryActorEnemyAdjacent: 0, secondaryActorObstacleAdjacent: 0, secondaryActorWallAdjacent: false, secondaryActorPlayerAdjacent: 0, mainActorResting: mainActor.CharacterClass?.LongRest ?? false, resonance: (uint)((mainActor as CPlayerActor)?.GetResonanceTokens() ?? 0), pressure: (mainActor as CPlayerActor)?.Pressure ?? CPlayerActor.EPressure.None)
		{
			MainActorEnemySummon = false;
			if (mainActor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == MainActorGuid);
				if (cEnemyActor != null)
				{
					MainActorEnemySummon = cEnemyActor.IsSummon;
				}
			}
			GameState.GetAdjacency(mainActor, out var wall, out var obstacles, out var allies, out var enemies, out var players);
			MainActorWallAdjacent = wall;
			MainActorObstacleAdjacent = obstacles;
			MainActorAllyAdjacent = allies;
			MainActorEnemyAdjacent = enemies;
			MainActorPlayerAdjacent = players;
		}

		public SEventActorBaseData(ESESubTypeActor actorEventSubType, CActor mainActor, CActor secondaryActor, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool actedOnEnemySummon = false, string text = "", bool doNotSerialize = false, int lastDamageAmount = 0, int preDamageHealth = 0, List<string> items = null, bool attackerDisadvantage = false, bool targetAdjacent = false, Point position = default(Point))
			: this(actorEventSubType, mainActor, secondaryActor?.ActorGuid, secondaryActor?.Class?.ID, secondaryActor?.OriginalType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnEnemySummon, secondaryActor?.Tokens?.CheckPositiveTokens, secondaryActor?.Tokens?.CheckNegativeTokens, text, doNotSerialize, lastDamageAmount, preDamageHealth, items, (secondaryActor?.MonsterClass == null) ? "" : secondaryActor.MonsterClass.MonsterType.ToString(), attackerDisadvantage, targetAdjacent, position, secondaryActor?.Health ?? 0, secondaryActor?.MaxHealth ?? 0)
		{
			SecondaryActorEnemySummon = false;
			if (secondaryActor != null && secondaryActor.OriginalType == CActor.EType.Enemy && secondaryActor is CEnemyActor cEnemyActor)
			{
				SecondaryActorEnemySummon = cEnemyActor.IsSummon;
			}
			if (secondaryActor != null)
			{
				GameState.GetAdjacency(secondaryActor, out var wall, out var obstacles, out var allies, out var enemies, out var players);
				SecondaryActorWallAdjacent = wall;
				SecondaryActorObstacleAdjacent = obstacles;
				SecondaryActorAllyAdjacent = allies;
				SecondaryActorEnemyAdjacent = enemies;
				SecondaryActorPlayerAdjacent = players;
			}
		}

		public SEventActorBaseData(ESESubTypeActor actorEventSubType, CActor mainActor, CActor secondaryActor, CAbility actedOnByAbility, string text = "", bool doNotSerialize = false, int lastDamageAmount = 0, int preDamageHealth = 0, List<string> items = null, bool attackerDisadvantage = false, bool targetAdjacent = false, Point position = default(Point))
			: this(actorEventSubType, mainActor, secondaryActor, (actedOnByAbility?.GetAbilityBaseCard()?.ID).GetValueOrDefault(), (actedOnByAbility?.GetAbilityBaseCard()?.CardType).GetValueOrDefault(), actedOnByAbility?.AbilityType ?? CAbility.EAbilityType.None, actedOnByAbility?.Name, actedOnByAbility?.Strength ?? 0, actedOnEnemySummon: false, text, doNotSerialize, lastDamageAmount, preDamageHealth, items, attackerDisadvantage, targetAdjacent, position)
		{
		}
	}

	public SEventActorBaseData BaseData;

	public ESESubTypeActor ActorSubType => BaseData.ActorSubType;

	public CActor.EType MainActorType => BaseData.MainActorType;

	public string MainActorGuid => BaseData.MainActorGuid;

	public string MainActorClassID => BaseData.MainActorClassID;

	public string MainActorSummonerID => BaseData.MainActorSummonerID;

	public int Gold => BaseData.Gold;

	public int XP => BaseData.XP;

	public LootState Loot => BaseData.Loot;

	public int Level => BaseData.Level;

	public List<PositiveConditionPair> MainActorPositiveConditions => BaseData.MainActorPositiveConditions;

	public List<NegativeConditionPair> MainActorNegativeConditions => BaseData.MainActorNegativeConditions;

	public bool PlayedThisRound => BaseData.PlayedThisRound;

	public bool IsDead => BaseData.IsDead;

	public CActor.ECauseOfDeath CauseOfDeath => BaseData.CauseOfDeath;

	public string SecondaryActorGUID => BaseData.SecondaryActorGUID;

	public string SecondaryActorClassID => BaseData.SecondaryActorClassID;

	public CActor.EType? SecondaryActorType => BaseData.SecondaryActorType;

	public string SecondaryActorSummonerID => BaseData.SecondaryActorSummonerID;

	public int CardID => BaseData.CardID;

	public CBaseCard.ECardType CardType => BaseData.CardType;

	public CAbility.EAbilityType AbilityType => BaseData.AbilityType;

	public string ActingAbilityName => BaseData.ActingAbilityName;

	public int AbilityStrength => BaseData.AbilityStrength;

	public bool MainActorEnemySummon => BaseData.MainActorEnemySummon;

	public bool SecondaryActorEnemySummon => BaseData.SecondaryActorEnemySummon;

	public bool Doom => BaseData.Doom;

	public string SecondaryActorMonsterType => BaseData.SecondaryActorMonsterType;

	public string MainActorMonsterType => BaseData.MainActorMonsterType;

	public List<PositiveConditionPair> SecondaryActorPositiveConditions => BaseData.SecondaryActorPositiveConditions;

	public List<NegativeConditionPair> SecondaryActorNegativeConditions => BaseData.SecondaryActorNegativeConditions;

	public int MainActorHealth => BaseData.MainActorHealth;

	public int MainActorMaxHealth => BaseData.MainActorMaxHealth;

	public int SecondaryActorHealth => BaseData.SecondaryActorHealth;

	public int SecondaryActorMaxHealth => BaseData.SecondaryActorMaxHealth;

	public int LastDamageAmount => BaseData.LastDamageAmount;

	public int PreDamageHealth => BaseData.PreDamageHealth;

	public DateTime TimeStamp => BaseData.TimeStamp;

	public List<string> Items => BaseData.Items;

	public bool AttackerAtDisadvantage => BaseData.AttackerAtDisadvantage;

	public bool TargetAdjacent => BaseData.TargetAdjacent;

	public int MainActorAllyAdjacent => BaseData.MainActorAllyAdjacent;

	public int MainActorEnemyAdjacent => BaseData.MainActorEnemyAdjacent;

	public int MainActorObstacleAdjacent => BaseData.MainActorObstacleAdjacent;

	public bool MainActorWallAdjacent => BaseData.MainActorWallAdjacent;

	public int MainActorPlayerAdjacent => BaseData.MainActorPlayerAdjacent;

	public int SecondaryActorAllyAdjacent => BaseData.SecondaryActorAllyAdjacent;

	public int SecondaryActorEnemyAdjacent => BaseData.SecondaryActorEnemyAdjacent;

	public int SecondaryActorObstacleAdjacent => BaseData.SecondaryActorObstacleAdjacent;

	public bool SecondaryActorWallAdjacent => BaseData.SecondaryActorWallAdjacent;

	public int SecondaryActorPlayerAdjacent => BaseData.SecondaryActorPlayerAdjacent;

	public bool HasFavorite => BaseData.HasFavorite;

	public string TurnActorGUID => BaseData.TurnActorGUID;

	public Point Position => BaseData.Position;

	public string ControlledByClassID => BaseData.ControlledByClassID;

	public string ControlledByActorGuid => BaseData.ControlledByActorGuid;

	public bool MainActorResting => BaseData.MainActorResting;

	public uint ResonanceTokens => BaseData.ResonanceTokens;

	public CPlayerActor.EPressure Pressure => BaseData.Pressure;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("BaseData", BaseData);
	}

	public SEventActor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "BaseData")
				{
					BaseData = (SEventActorBaseData)info.GetValue("BaseData", typeof(SEventActorBaseData));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActor entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActor(SEventActorBaseData baseData, string text = "", bool doNotSerialize = false)
		: base(ESEType.Actor, text, doNotSerialize)
	{
		BaseData = baseData;
	}
}
