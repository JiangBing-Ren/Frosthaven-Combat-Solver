using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsLoot : CPlayerStatsAction, ISerializable
{
	public string LootType { get; private set; }

	public bool EndOfTurn { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LootType", LootType);
		info.AddValue("EndOfTurn", EndOfTurn);
	}

	public CPlayerStatsLoot(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "LootType"))
				{
					if (name == "EndOfTurn")
					{
						EndOfTurn = info.GetBoolean("EndOfTurn");
					}
				}
				else
				{
					LootType = info.GetString("LootType");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsLoot entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsLoot(string lootType, bool endOfTurn, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		LootType = lootType;
		EndOfTurn = endOfTurn;
	}

	public static CPlayerStatsLoot CreateLootStat(SEventActorLooted sevent, string advGuid, string sceGuid, string currentQuestType, string ActorType, string ActedOnType, string actingClassID = null, string actedOnClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		string lootType = sevent.LootedType.ToString();
		bool endOfTurn = sevent.EndOfTurn;
		CPlayerSatsBaseData baseData = new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder);
		string mainActorClassID = ((actingClassID != null) ? actingClassID : sevent.MainActorClassID);
		string mainActorGuid = sevent.MainActorGuid;
		string mainActorMonsterType = sevent.MainActorMonsterType;
		List<PositiveConditionPair> mainActorPositiveConditions = sevent.MainActorPositiveConditions;
		List<NegativeConditionPair> mainActorNegativeConditions = sevent.MainActorNegativeConditions;
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
		int mainActorEnemyAdjacent = sevent.MainActorEnemyAdjacent;
		bool mainActorWallAdjacent = sevent.MainActorWallAdjacent;
		int mainActorAllyAdjacent = sevent.MainActorAllyAdjacent;
		int mainActorObstacleAdjacent = sevent.MainActorObstacleAdjacent;
		bool mainActorWallAdjacent2 = mainActorWallAdjacent;
		int mainActorPlayerAdjacent = sevent.MainActorPlayerAdjacent;
		Point position = sevent.Position;
		return new CPlayerStatsLoot(lootType, endOfTurn, baseData, new CPlayerStatsActionData(controlledByClassID: sevent.ControlledByClassID, controlledByActorGuid: sevent.ControlledByActorGuid, position: position, mainActorPlayerAdjacent: mainActorPlayerAdjacent, secondaryActorHealth: sevent.SecondaryActorHealth, secondaryActorMaxHealth: sevent.SecondaryActorMaxHealth, secondaryActorEnemyAdjacent: sevent.SecondaryActorEnemyAdjacent, secondaryActorWallAdjacent: sevent.SecondaryActorWallAdjacent, mainActorClassID: mainActorClassID, mainActorType: ActorType, mainActorPositiveConditions: mainActorPositiveConditions, mainActorNegativeConditions: mainActorNegativeConditions, mainActorGUID: mainActorGuid, mainActorMonsterType: mainActorMonsterType, secondaryActorClassID: secondaryActorClassID, secondaryActorType: ActedOnType, secondaryActorPositiveConditions: secondaryActorPositiveConditions, secondaryActorNegativeConditions: secondaryActorNegativeConditions, secondaryActorGUID: secondaryActorGUID, secondaryActorMonsterType: secondaryActorMonsterType, turnActorGUID: turnActorGUID, infused: elements, cardID: cardID, cardType: cardType, abilityType: abilityType, actingAbilityName: actingAbilityName, abilityStrength: abilityStrength, performedBySummons: performedBySummons2, rolledIntoSummoner: rolledIntoSummoner2, health: mainActorHealth, maxHealth: mainActorMaxHealth, mainActorAllyAdjacent: mainActorAllyAdjacent, mainActorEnemyAdjacent: mainActorEnemyAdjacent, mainActorObstacleAdjacent: mainActorObstacleAdjacent, mainActorWallAdjacent: mainActorWallAdjacent2, secondaryActorAllyAdjacent: sevent.SecondaryActorAllyAdjacent, secondaryActorObstacleAdjacent: sevent.SecondaryActorObstacleAdjacent, secondaryActorPlayerAdjacent: sevent.SecondaryActorPlayerAdjacent));
	}
}
