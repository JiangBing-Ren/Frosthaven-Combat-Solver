using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsKill : CPlayerStatsAction, ISerializable
{
	public CActor.ECauseOfDeath CauseOfDeath { get; private set; }

	public int MaxHealth { get; private set; }

	public int LastDamageAmount { get; private set; }

	public int PreDamageHealth { get; private set; }

	public List<string> Items { get; private set; }

	public DateTime TimeStamp { get; private set; }

	public bool Doom { get; private set; }

	public bool Disadvantaged { get; private set; }

	public bool TargetAdjacent { get; private set; }

	public bool HasFavorite { get; private set; }

	public string ActorWhoKilledSummonerID { get; private set; }

	public string ActorWhoDiedGUID
	{
		get
		{
			return ActionData.MainActorGUID;
		}
		set
		{
			ActionData.MainActorGUID = value;
		}
	}

	public string ActorWhoKilledGUID
	{
		get
		{
			return ActionData.SecondaryActorGUID;
		}
		set
		{
			ActionData.SecondaryActorGUID = value;
		}
	}

	public string ActorWhoDiedMonsterType => ActionData.MainActorMonsterType;

	public string ActorWhoKilledMonsterType => ActionData.SecondaryActorMonsterType;

	public string ActorWhoDiedClassID => ActionData.MainActorClassID;

	public string ActorWhoDiedType => ActionData.MainActorType;

	public string ActorWhoKilledClassID => ActionData.SecondaryActorClassID;

	public string ActorWhoKilledType => ActionData.SecondaryActorType;

	public List<NegativeConditionPair> ActorWhoDiedNegativeConditions => ActionData.MainActorNegativeConditions;

	public int ActorWhoKilledAllyAdjacency => ActionData.SecondaryActorAllyAdjacent;

	public int ActorWhoKilledEnemyAdjacency => ActionData.SecondaryActorEnemyAdjacent;

	public int ActorWhoKilledObstacleAdjacency => ActionData.SecondaryActorObstacleAdjacent;

	public int ActorWhoKilledPlayerAdjacency => ActionData.SecondaryActorPlayerAdjacent;

	public bool ActorWhoKilledWallAdjacency => ActionData.SecondaryActorWallAdjacent;

	public int ActorWhoDiedAllyAdjacency => ActionData.MainActorAllyAdjacent;

	public int ActorWhoDiedEnemyAdjacency => ActionData.MainActorEnemyAdjacent;

	public int ActorWhoDiedObstacleAdjacency => ActionData.MainActorObstacleAdjacent;

	public int ActorWhoDiedPlayerAdjacency => ActionData.MainActorPlayerAdjacent;

	public bool ActorWhoDiedWallAdjacency => ActionData.MainActorWallAdjacent;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CauseOfDeath", CauseOfDeath);
		info.AddValue("MaxHealth", MaxHealth);
		info.AddValue("LastDamageAmount", LastDamageAmount);
		info.AddValue("PreDamageHealth", PreDamageHealth);
		info.AddValue("Items", Items);
		info.AddValue("TimeStamp", TimeStamp);
		info.AddValue("Doom", Doom);
		info.AddValue("Disadvantaged", Disadvantaged);
		info.AddValue("TargetAdjacent", TargetAdjacent);
		info.AddValue("ActorWhoKilledSummonerID", ActorWhoKilledSummonerID);
	}

	public CPlayerStatsKill(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CauseOfDeath":
					CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
					break;
				case "MaxHealth":
					MaxHealth = info.GetInt32("MaxHealth");
					break;
				case "LastDamageAmount":
					LastDamageAmount = info.GetInt32("LastDamageAmount");
					break;
				case "PreDamageHealth":
					PreDamageHealth = info.GetInt32("PreDamageHealth");
					break;
				case "Items":
					Items = (List<string>)info.GetValue("Items", typeof(List<string>));
					break;
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				case "Doom":
					Doom = info.GetBoolean("Doom");
					break;
				case "Disadvantaged":
					Disadvantaged = info.GetBoolean("Disadvantaged");
					break;
				case "TargetAdjacent":
					TargetAdjacent = info.GetBoolean("TargetAdjacent");
					break;
				case "HasFavorite":
					HasFavorite = info.GetBoolean("HasFavorite");
					break;
				case "ActorWhoKilledSummonerID":
					ActorWhoKilledSummonerID = info.GetString("ActorWhoKilledSummonerID");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsKill entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsKill(CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData, CActor.ECauseOfDeath causeOfDeath, DateTime timestamp, int maxHealth, int lastDamageAmount, int preDamageHealth, List<string> items = null, bool doom = false, string monsterType = "", bool disadvantaged = false, bool targetAdjacent = false, bool hasFavorite = false, string actorWhoKilledSummonerID = null)
		: base(baseData, actionData)
	{
		CauseOfDeath = causeOfDeath;
		MaxHealth = maxHealth;
		LastDamageAmount = lastDamageAmount;
		PreDamageHealth = preDamageHealth;
		Items = items;
		TimeStamp = timestamp;
		Doom = doom;
		Disadvantaged = disadvantaged;
		TargetAdjacent = targetAdjacent;
		HasFavorite = hasFavorite;
		ActorWhoKilledSummonerID = actorWhoKilledSummonerID;
	}

	public static CPlayerStatsKill CreateKillStat(SEventActor sevent, string advGuid, string sceGuid, string currentQuestType, string ActorType, string ActedOnType, string actingClassID = null, string actedOnClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		CPlayerSatsBaseData baseData = new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder);
		string mainActorClassID = ((actingClassID != null) ? actingClassID : sevent.MainActorClassID);
		List<PositiveConditionPair> mainActorPositiveConditions = sevent.MainActorPositiveConditions;
		List<NegativeConditionPair> mainActorNegativeConditions = sevent.MainActorNegativeConditions;
		string mainActorMonsterType = sevent.MainActorMonsterType;
		string mainActorGuid = sevent.MainActorGuid;
		string secondaryActorClassID = ((actedOnClassID != null) ? actedOnClassID : sevent.SecondaryActorClassID);
		string secondaryActorMonsterType = sevent.SecondaryActorMonsterType;
		List<PositiveConditionPair> secondaryActorPositiveConditions = sevent.SecondaryActorPositiveConditions;
		List<NegativeConditionPair> secondaryActorNegativeConditions = sevent.SecondaryActorNegativeConditions;
		string secondaryActorGUID = sevent.SecondaryActorGUID;
		string turnActorGUID = sevent.TurnActorGUID;
		List<ElementInfusionBoardManager.EElement> elements = sevent.Elements;
		int cardID = sevent.CardID;
		CBaseCard.ECardType cardType = sevent.CardType;
		CAbility.EAbilityType abilityType = sevent.AbilityType;
		string actingAbilityName = sevent.ActingAbilityName;
		int abilityStrength = sevent.AbilityStrength;
		int mainActorHealth = sevent.MainActorHealth;
		int mainActorMaxHealth = sevent.MainActorMaxHealth;
		bool performedBySummons2 = performedBySummons;
		bool rolledIntoSummoner2 = rolledIntoSummoner;
		int mainActorAllyAdjacent = sevent.MainActorAllyAdjacent;
		int mainActorEnemyAdjacent = sevent.MainActorEnemyAdjacent;
		int mainActorObstacleAdjacent = sevent.MainActorObstacleAdjacent;
		bool mainActorWallAdjacent = sevent.MainActorWallAdjacent;
		int mainActorPlayerAdjacent = sevent.MainActorPlayerAdjacent;
		return new CPlayerStatsKill(baseData, new CPlayerStatsActionData(position: sevent.Position, mainActorPlayerAdjacent: mainActorPlayerAdjacent, secondaryActorHealth: sevent.SecondaryActorHealth, secondaryActorMaxHealth: sevent.SecondaryActorMaxHealth, secondaryActorEnemyAdjacent: sevent.SecondaryActorEnemyAdjacent, secondaryActorWallAdjacent: sevent.SecondaryActorWallAdjacent, mainActorClassID: mainActorClassID, mainActorType: ActorType, mainActorPositiveConditions: mainActorPositiveConditions, mainActorNegativeConditions: mainActorNegativeConditions, mainActorGUID: mainActorGuid, mainActorMonsterType: mainActorMonsterType, secondaryActorClassID: secondaryActorClassID, secondaryActorType: ActedOnType, secondaryActorPositiveConditions: secondaryActorPositiveConditions, secondaryActorNegativeConditions: secondaryActorNegativeConditions, secondaryActorGUID: secondaryActorGUID, secondaryActorMonsterType: secondaryActorMonsterType, turnActorGUID: turnActorGUID, infused: elements, cardID: cardID, cardType: cardType, abilityType: abilityType, actingAbilityName: actingAbilityName, abilityStrength: abilityStrength, performedBySummons: performedBySummons2, rolledIntoSummoner: rolledIntoSummoner2, health: mainActorHealth, maxHealth: mainActorMaxHealth, mainActorAllyAdjacent: mainActorAllyAdjacent, mainActorEnemyAdjacent: mainActorEnemyAdjacent, mainActorObstacleAdjacent: mainActorObstacleAdjacent, mainActorWallAdjacent: mainActorWallAdjacent, controlledByClassID: null, controlledByActorGuid: null, secondaryActorAllyAdjacent: sevent.SecondaryActorAllyAdjacent, secondaryActorObstacleAdjacent: sevent.SecondaryActorObstacleAdjacent, secondaryActorPlayerAdjacent: sevent.SecondaryActorPlayerAdjacent), sevent.CauseOfDeath, sevent.TimeStamp, sevent.MainActorMaxHealth, sevent.LastDamageAmount, sevent.PreDamageHealth, sevent.Items, sevent.Doom, sevent.SecondaryActorMonsterType, sevent.AttackerAtDisadvantage, sevent.TargetAdjacent, sevent.HasFavorite, sevent.SecondaryActorSummonerID)
		{
			ActorWhoDiedGUID = sevent.MainActorGuid,
			ActorWhoKilledGUID = sevent.SecondaryActorGUID
		};
	}
}
