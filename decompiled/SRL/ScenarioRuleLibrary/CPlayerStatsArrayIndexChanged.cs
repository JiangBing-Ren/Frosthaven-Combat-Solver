using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsArrayIndexChanged : CPlayerStatsAction, ISerializable
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	public bool TileWasOccupiedByObstacle { get; private set; }

	public bool TileWasOccupiedByActor { get; private set; }

	public int LootTokensInPreviousRoom { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
		info.AddValue("TileWasOccupiedByObstacle", TileWasOccupiedByObstacle);
		info.AddValue("TileWasOccupiedByActor", TileWasOccupiedByActor);
		info.AddValue("LootTokensInPreviousRoom", LootTokensInPreviousRoom);
	}

	public CPlayerStatsArrayIndexChanged(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TileX":
					TileX = info.GetInt32("TileX");
					break;
				case "TileY":
					TileY = info.GetInt32("TileY");
					break;
				case "TileWasOccupiedByObstacle":
					TileWasOccupiedByObstacle = info.GetBoolean("TileWasOccupiedByObstacle");
					break;
				case "TileWasOccupiedByActor":
					TileWasOccupiedByActor = info.GetBoolean("TileWasOccupiedByActor");
					break;
				case "LootTokensInPreviousRoom":
					LootTokensInPreviousRoom = info.GetInt32("LootTokensInPreviousRoom");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsEndTurn entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsArrayIndexChanged(int x, int y, bool tileWasOccupiedByObstacle, bool tileWasOccupiedByActor, int lootTokensInPreviousRoom, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		TileX = x;
		TileY = y;
		TileWasOccupiedByObstacle = tileWasOccupiedByObstacle;
		TileWasOccupiedByActor = tileWasOccupiedByActor;
		LootTokensInPreviousRoom = lootTokensInPreviousRoom;
	}

	public static CPlayerStatsArrayIndexChanged CreateArrayIndexChangedStat(SEventActorArrayIndexChanged sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		int tileX = sevent.TileX;
		int tileY = sevent.TileY;
		bool tileWasOccupiedByObstacle = sevent.TileWasOccupiedByObstacle;
		bool tileWasOccupiedByActor = sevent.TileWasOccupiedByActor;
		int lootTokensInPreviousRoom = sevent.LootTokensInPreviousRoom;
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
		int mainActorAllyAdjacent = sevent.MainActorAllyAdjacent;
		int mainActorObstacleAdjacent = sevent.MainActorObstacleAdjacent;
		bool mainActorWallAdjacent2 = mainActorWallAdjacent;
		int mainActorPlayerAdjacent = sevent.MainActorPlayerAdjacent;
		return new CPlayerStatsArrayIndexChanged(tileX, tileY, tileWasOccupiedByObstacle, tileWasOccupiedByActor, lootTokensInPreviousRoom, baseData, new CPlayerStatsActionData(position: sevent.Position, mainActorPlayerAdjacent: mainActorPlayerAdjacent, secondaryActorHealth: sevent.SecondaryActorHealth, secondaryActorMaxHealth: sevent.SecondaryActorMaxHealth, secondaryActorEnemyAdjacent: sevent.SecondaryActorEnemyAdjacent, secondaryActorWallAdjacent: sevent.SecondaryActorWallAdjacent, mainActorClassID: mainActorClassID2, mainActorType: mainActorType, mainActorPositiveConditions: mainActorPositiveConditions, mainActorNegativeConditions: mainActorNegativeConditions, mainActorGUID: mainActorGuid, mainActorMonsterType: mainActorMonsterType, secondaryActorClassID: secondaryActorClassID2, secondaryActorType: secondaryActorType, secondaryActorPositiveConditions: secondaryActorPositiveConditions, secondaryActorNegativeConditions: secondaryActorNegativeConditions, secondaryActorGUID: secondaryActorGUID, secondaryActorMonsterType: secondaryActorMonsterType, turnActorGUID: turnActorGUID, infused: elements, cardID: cardID, cardType: cardType, abilityType: abilityType, actingAbilityName: actingAbilityName, abilityStrength: abilityStrength, performedBySummons: performedBySummons2, rolledIntoSummoner: rolledIntoSummoner2, health: mainActorHealth, maxHealth: mainActorMaxHealth, mainActorAllyAdjacent: mainActorAllyAdjacent, mainActorEnemyAdjacent: mainActorEnemyAdjacent, mainActorObstacleAdjacent: mainActorObstacleAdjacent, mainActorWallAdjacent: mainActorWallAdjacent2, controlledByClassID: null, controlledByActorGuid: null, secondaryActorAllyAdjacent: sevent.SecondaryActorAllyAdjacent, secondaryActorObstacleAdjacent: sevent.SecondaryActorObstacleAdjacent, secondaryActorPlayerAdjacent: sevent.SecondaryActorPlayerAdjacent));
	}
}
