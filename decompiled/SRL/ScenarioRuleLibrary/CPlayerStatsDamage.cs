using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDamage : CPlayerStatsAction, ISerializable
{
	public int Avoided;

	public int Shielded;

	public int ItemShielded;

	public bool IsMelee;

	public int PoisonDamage;

	public int Shield;

	public bool HasFavorite;

	public List<PositiveConditionPair> PositiveConditionsBeforeDamage;

	public int HealthBeforeDamage;

	public CActor.ECauseOfDamage CauseOfDamage { get; private set; }

	public int FinalDamageAmount { get; private set; }

	public bool ActorWhoReceivedDamagedIsResting => ActionData.MainActorResting;

	public string ActorWhoReceivedDamageGUID => ActionData.MainActorGUID;

	public string ActorWhoDealtDamageGUID => ActionData.SecondaryActorGUID;

	public string ActorWhoReceivedDamageType => ActionData.MainActorType;

	public string ActorWhoDealtDamageType => ActionData.SecondaryActorType;

	public string ActorWhoReceivedDamageMonsterType => ActionData.MainActorMonsterType;

	public string ActorWhoDealtDamageMonsterType => ActionData.SecondaryActorMonsterType;

	public string ActorWhoReceivedDamageClassID => ActionData.MainActorClassID;

	public string ActorWhoDealtDamageClassID => ActionData.SecondaryActorClassID;

	public int ActorWhoReceivedDamageHealth => ActionData.MainActorHealth;

	public int ActorWhoReceivedDamageMaxHealth => ActionData.MainActorMaxHealth;

	public List<NegativeConditionPair> ActorWhoReceivedDamageNegativeConditions => ActionData.MainActorNegativeConditions;

	public int ActorWhoDealtDamageAllyAdjacency => ActionData.SecondaryActorAllyAdjacent;

	public int ActorWhoDealtDamageEnemyAdjacency => ActionData.SecondaryActorEnemyAdjacent;

	public int ActorWhoDealtDamageObstacleAdjacency => ActionData.SecondaryActorObstacleAdjacent;

	public int ActorWhoDealtDamagePlayerAdjacency => ActionData.SecondaryActorPlayerAdjacent;

	public bool ActorWhoDealtDamageWallAdjacency => ActionData.SecondaryActorWallAdjacent;

	public int ActorWhoReceivedDamageAllyAdjacency => ActionData.MainActorAllyAdjacent;

	public int ActorWhoReceivedDamageEnemyAdjacency => ActionData.MainActorEnemyAdjacent;

	public int ActorWhoReceivedDamageObstacleAdjacency => ActionData.MainActorObstacleAdjacent;

	public int ActorWhoReceivedDamagePlayerAdjacency => ActionData.MainActorPlayerAdjacent;

	public bool ActorWhoReceivedDamageWallAdjacency => ActionData.MainActorWallAdjacent;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CauseOfDamage", CauseOfDamage);
		info.AddValue("FinalDamageAmount", FinalDamageAmount);
		info.AddValue("Avoided", Avoided);
		info.AddValue("Shielded", Shielded);
		info.AddValue("ItemShielded", Shielded);
		info.AddValue("IsMelee", IsMelee);
		info.AddValue("PoisonDamage", PoisonDamage);
		info.AddValue("Shield", Shield);
		info.AddValue("HasFavorite", HasFavorite);
		info.AddValue("PositiveConditionsBeforeDamage", PositiveConditionsBeforeDamage);
		info.AddValue("HealthBeforeDamage", HealthBeforeDamage);
	}

	public CPlayerStatsDamage(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "HealthBeforeDamage":
					HealthBeforeDamage = info.GetInt32("HealthBeforeDamage");
					break;
				case "PositiveConditionsBeforeDamage":
					PositiveConditionsBeforeDamage = (List<PositiveConditionPair>)info.GetValue("PositiveConditionsBeforeDamage", typeof(List<PositiveConditionPair>));
					break;
				case "CauseOfDamage":
					CauseOfDamage = (CActor.ECauseOfDamage)info.GetValue("CauseOfDamage", typeof(CActor.ECauseOfDamage));
					break;
				case "FinalDamageAmount":
					FinalDamageAmount = info.GetInt32("FinalDamageAmount");
					break;
				case "Avoided":
					Avoided = info.GetInt32("Avoided");
					break;
				case "Shielded":
					Shielded = info.GetInt32("Shielded");
					break;
				case "ItemShielded":
					ItemShielded = info.GetInt32("ItemShielded");
					break;
				case "IsMelee":
					IsMelee = info.GetBoolean("IsMelee");
					break;
				case "PoisonDamage":
					PoisonDamage = info.GetInt32("PoisonDamage");
					break;
				case "Shield":
					Shield = info.GetInt32("Shield");
					break;
				case "HasFavorite":
					HasFavorite = info.GetBoolean("HasFavorite");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsDamage entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDamage(CActor.ECauseOfDamage causeOfDamage, int finalDamage, int avoided, int shielded, int itemShielded, bool isMelee, int poison, int shield, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData, bool hasFavorite = false, List<PositiveConditionPair> positiveConditionsBeforeDamage = null, int healthBeforeDamage = 0)
		: base(baseData, actionData)
	{
		CauseOfDamage = causeOfDamage;
		FinalDamageAmount = finalDamage;
		Avoided = avoided;
		Shielded = shielded;
		ItemShielded = itemShielded;
		IsMelee = isMelee;
		PoisonDamage = poison;
		Shield = shield;
		HasFavorite = hasFavorite;
		PositiveConditionsBeforeDamage = positiveConditionsBeforeDamage;
		HealthBeforeDamage = healthBeforeDamage;
	}

	public static CPlayerStatsDamage CreateDamageStat(SEventActorDamaged sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		CActor.ECauseOfDamage causeOfDamage = sevent.CauseOfDamage;
		int damageTaken = sevent.DamageTaken;
		int avoided = sevent.Avoided;
		int shielded = sevent.Shielded;
		int itemShielded = sevent.ItemShielded;
		bool isMelee = sevent.IsMelee;
		int poisonDamage = sevent.PoisonDamage;
		int shield = sevent.Shield;
		int healthBeforeDamage = sevent.HealthBeforeDamage;
		CPlayerSatsBaseData baseData = new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder);
		string mainActorClassID2 = ((mainActorClassID != null) ? mainActorClassID : sevent.MainActorClassID);
		List<PositiveConditionPair> mainActorPositiveConditions = sevent.MainActorPositiveConditions;
		List<NegativeConditionPair> mainActorNegativeConditions = sevent.MainActorNegativeConditions;
		string mainActorGuid = sevent.MainActorGuid;
		string mainActorMonsterType = sevent.MainActorMonsterType;
		string secondaryActorMonsterType = sevent.SecondaryActorMonsterType;
		string secondaryActorClassID2 = ((secondaryActorClassID != null) ? secondaryActorClassID : sevent.SecondaryActorClassID);
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
		int mainActorEnemyAdjacent = sevent.MainActorEnemyAdjacent;
		bool mainActorWallAdjacent = sevent.MainActorWallAdjacent;
		return new CPlayerStatsDamage(causeOfDamage, damageTaken, avoided, shielded, itemShielded, isMelee, poisonDamage, shield, baseData, new CPlayerStatsActionData(mainActorAllyAdjacent: sevent.MainActorAllyAdjacent, mainActorEnemyAdjacent: mainActorEnemyAdjacent, mainActorObstacleAdjacent: sevent.MainActorObstacleAdjacent, mainActorWallAdjacent: mainActorWallAdjacent, controlledByClassID: null, controlledByActorGuid: null, mainActorPlayerAdjacent: sevent.MainActorPlayerAdjacent, secondaryActorEnemyAdjacent: sevent.SecondaryActorEnemyAdjacent, secondaryActorWallAdjacent: sevent.SecondaryActorWallAdjacent, secondaryActorAllyAdjacent: sevent.SecondaryActorAllyAdjacent, secondaryActorObstacleAdjacent: sevent.SecondaryActorObstacleAdjacent, secondaryActorPlayerAdjacent: sevent.SecondaryActorPlayerAdjacent, mainActorClassID: mainActorClassID2, mainActorType: mainActorType, mainActorPositiveConditions: mainActorPositiveConditions, mainActorNegativeConditions: mainActorNegativeConditions, mainActorGUID: mainActorGuid, mainActorMonsterType: mainActorMonsterType, secondaryActorClassID: secondaryActorClassID2, secondaryActorType: secondaryActorType, secondaryActorPositiveConditions: secondaryActorPositiveConditions, secondaryActorNegativeConditions: secondaryActorNegativeConditions, secondaryActorGUID: secondaryActorGUID, secondaryActorMonsterType: secondaryActorMonsterType, turnActorGUID: turnActorGUID, infused: elements, cardID: cardID, cardType: cardType, abilityType: abilityType, actingAbilityName: actingAbilityName, abilityStrength: abilityStrength, performedBySummons: performedBySummons2, rolledIntoSummoner: rolledIntoSummoner2, health: mainActorHealth, maxHealth: mainActorMaxHealth, position: sevent.Position, secondaryActorHealth: sevent.SecondaryActorHealth, secondaryActorMaxHealth: sevent.SecondaryActorMaxHealth, mainActorResting: sevent.MainActorResting), sevent.HasFavorite, sevent.PositiveConditionsBeforeDamage, healthBeforeDamage);
	}
}
