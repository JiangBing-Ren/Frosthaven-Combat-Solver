using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsCreateProp : CPlayerStatsAction, ISerializable
{
	public EPropType PropType;

	public ESpecificPropType SpecificPropType;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PropType", PropType);
		info.AddValue("SpecificPropType", SpecificPropType);
	}

	public CPlayerStatsCreateProp(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PropType"))
				{
					if (name == "SpecificPropType")
					{
						SpecificPropType = (ESpecificPropType)info.GetValue("SpecificPropType", typeof(ESpecificPropType));
					}
				}
				else
				{
					PropType = (EPropType)info.GetValue("PropType", typeof(EPropType));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsDestroyObstacle entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsCreateProp(EPropType PropType, ESpecificPropType SpecificPropType, CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData, actionData)
	{
		this.PropType = PropType;
		this.SpecificPropType = SpecificPropType;
	}

	public static CPlayerStatsCreateProp CreateCreatePropStat(SEventAbilityCreateProp sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false)
	{
		EPropType propType = sevent.PropType;
		ESpecificPropType specificPropType = sevent.SpecificPropType;
		CPlayerSatsBaseData baseData = new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder);
		string mainActorClassID2 = ((mainActorClassID != null) ? mainActorClassID : sevent.MainActorClassID);
		List<PositiveConditionPair> mainActorPositiveConditions = sevent.MainActorPositiveConditions;
		List<NegativeConditionPair> mainActorNegativeConditions = sevent.MainActorNegativeConditions;
		string mainActorMonsterType = sevent.MainActorMonsterType;
		string secondaryActorMonsterType = sevent.SecondaryActorMonsterType;
		string secondaryActorClassID2 = ((secondaryActorClassID != null) ? secondaryActorClassID : sevent.SecondaryActorClassID);
		List<PositiveConditionPair> secondaryActorPositiveConditions = sevent.SecondaryActorPositiveConditions;
		List<NegativeConditionPair> secondaryActorNegativeConditions = sevent.SecondaryActorNegativeConditions;
		string mainActorGUID = sevent.MainActorGUID;
		string turnActorGUID = sevent.TurnActorGUID;
		List<ElementInfusionBoardManager.EElement> elements = sevent.Elements;
		int cardID = sevent.CardID;
		CBaseCard.ECardType cardType = sevent.CardType;
		CAbility.EAbilityType abilityType = sevent.AbilityType;
		string cardName = sevent.CardName;
		int strength = sevent.Strength;
		int mainActorHealth = sevent.MainActorHealth;
		int mainActorMaxHealth = sevent.MainActorMaxHealth;
		bool performedBySummons2 = performedBySummons;
		bool rolledIntoSummoner2 = rolledIntoSummoner;
		int secondaryActorHealth = sevent.SecondaryActorHealth;
		int secondaryActorMaxHealth = sevent.SecondaryActorMaxHealth;
		return new CPlayerStatsCreateProp(propType, specificPropType, baseData, new CPlayerStatsActionData(mainActorClassID2, mainActorType, mainActorPositiveConditions, mainActorNegativeConditions, mainActorGUID, mainActorMonsterType, secondaryActorClassID2, secondaryActorType, secondaryActorPositiveConditions, secondaryActorNegativeConditions, null, secondaryActorMonsterType, turnActorGUID, elements, cardID, cardType, abilityType, cardName, strength, performedBySummons2, rolledIntoSummoner2, mainActorHealth, mainActorMaxHealth, 0, 0, 0, mainActorWallAdjacent: false, null, null, default, 0, secondaryActorHealth, secondaryActorMaxHealth));
	}
}
