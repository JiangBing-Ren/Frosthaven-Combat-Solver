using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsEndTurn : CPlayerStatsAction, ISerializable
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
	}

	public CPlayerStatsEndTurn(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "TileX"))
				{
					if (name == "TileY")
					{
						TileY = info.GetInt32("TileY");
					}
				}
				else
				{
					TileX = info.GetInt32("TileX");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsEndTurn entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsEndTurn(int x, int y, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		TileX = x;
		TileY = y;
	}

	public static CPlayerStatsEndTurn CreateEndTurnStat(SEventActorEndTurn sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		int tileX = sevent.TileX;
		int tileY = sevent.TileY;
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
		return new CPlayerStatsEndTurn(tileX, tileY, baseData, new CPlayerStatsActionData(position: sevent.Position, mainActorPlayerAdjacent: mainActorPlayerAdjacent, secondaryActorHealth: sevent.SecondaryActorHealth, secondaryActorMaxHealth: sevent.SecondaryActorMaxHealth, secondaryActorEnemyAdjacent: sevent.SecondaryActorEnemyAdjacent, secondaryActorWallAdjacent: sevent.SecondaryActorWallAdjacent, mainActorClassID: mainActorClassID2, mainActorType: mainActorType, mainActorPositiveConditions: mainActorPositiveConditions, mainActorNegativeConditions: mainActorNegativeConditions, mainActorGUID: mainActorGuid, mainActorMonsterType: mainActorMonsterType, secondaryActorClassID: secondaryActorClassID2, secondaryActorType: secondaryActorType, secondaryActorPositiveConditions: secondaryActorPositiveConditions, secondaryActorNegativeConditions: secondaryActorNegativeConditions, secondaryActorGUID: secondaryActorGUID, secondaryActorMonsterType: secondaryActorMonsterType, turnActorGUID: turnActorGUID, infused: elements, cardID: cardID, cardType: cardType, abilityType: abilityType, actingAbilityName: actingAbilityName, abilityStrength: abilityStrength, performedBySummons: performedBySummons2, rolledIntoSummoner: rolledIntoSummoner2, health: mainActorHealth, maxHealth: mainActorMaxHealth, mainActorAllyAdjacent: mainActorAllyAdjacent, mainActorEnemyAdjacent: mainActorEnemyAdjacent, mainActorObstacleAdjacent: mainActorObstacleAdjacent, mainActorWallAdjacent: mainActorWallAdjacent2, controlledByClassID: null, controlledByActorGuid: null, secondaryActorAllyAdjacent: sevent.SecondaryActorAllyAdjacent, secondaryActorObstacleAdjacent: sevent.SecondaryActorObstacleAdjacent, secondaryActorPlayerAdjacent: sevent.SecondaryActorPlayerAdjacent, mainActorResting: false, resonance: sevent.ResonanceTokens, pressure: sevent.Pressure));
	}
}
