using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsAbilities : CPlayerStatsAction, ISerializable
{
	public int Amount { get; private set; }

	public bool SpecialAbility { get; private set; }

	public bool? AreaEffect { get; private set; }

	public int Targets { get; private set; }

	public bool DefaultAction { get; private set; }

	public bool HasHappened { get; private set; }

	public List<string> TargetGUIDs { get; private set; }

	public List<CActor.EType> TargetTypes { get; set; }

	public int CardsInHandCount { get; private set; }

	public int CardsInDiscardCount { get; private set; }

	public DiscardType DiscardType { get; set; }

	public List<bool> Advantage { get; set; }

	public Guid AbilityID { get; private set; }

	public bool IsBuff { get; private set; }

	public CAbility.EAbilityFlag Flags { get; private set; }

	public int Tokens { get; private set; }

	public List<Point> TilesMovedTo { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TilesMoved", TilesMovedTo);
		info.AddValue("Tokens", Tokens);
		info.AddValue("Flags", Flags);
		info.AddValue("AbilityID", AbilityID);
		info.AddValue("Advantage", Advantage);
		info.AddValue("Amount", Amount);
		info.AddValue("SpecialAbility", SpecialAbility);
		info.AddValue("AreaEffect", AreaEffect);
		info.AddValue("Targets", Targets);
		info.AddValue("DefaultAction", DefaultAction);
		info.AddValue("HasHappened", HasHappened);
		info.AddValue("TargetGUIDs", TargetGUIDs);
		info.AddValue("TargetTypes", TargetTypes);
		info.AddValue("CardsInHandCount", CardsInHandCount);
		info.AddValue("CardsInDiscardCount", CardsInDiscardCount);
		info.AddValue("DiscardType", DiscardType);
		info.AddValue("IsBuff", IsBuff);
	}

	public CPlayerStatsAbilities(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Tokens":
					Tokens = info.GetInt32("Tokens");
					break;
				case "Flags":
					Flags = (CAbility.EAbilityFlag)info.GetValue("Flags", typeof(CAbility.EAbilityFlag));
					break;
				case "IsBuff":
					IsBuff = info.GetBoolean("IsBuff");
					break;
				case "AbilityID":
					AbilityID = (Guid)info.GetValue("AbilityID", typeof(Guid));
					break;
				case "TargetGUIDs":
					TargetGUIDs = (List<string>)info.GetValue("TargetGUIDs", typeof(List<string>));
					break;
				case "TargetTypes":
					TargetTypes = (List<CActor.EType>)info.GetValue("TargetTypes", typeof(List<CActor.EType>));
					break;
				case "TilesMoved":
					TilesMovedTo = (List<Point>)info.GetValue("TilesMoved", typeof(List<Point>));
					break;
				case "Amount":
					Amount = info.GetInt32("Amount");
					break;
				case "SpecialAbility":
					SpecialAbility = info.GetBoolean("SpecialAbility");
					break;
				case "Advantage":
					Advantage = (List<bool>)info.GetValue("Advantage", typeof(List<bool>));
					break;
				case "AreaEffect":
					AreaEffect = (bool?)info.GetValue("AreaEffect", typeof(bool?));
					break;
				case "Targets":
					Targets = info.GetInt32("Targets");
					break;
				case "CardsInHandCount":
					CardsInHandCount = info.GetInt32("CardsInHandCount");
					break;
				case "CardsInDiscardCount":
					CardsInDiscardCount = info.GetInt32("CardsInDiscardCount");
					break;
				case "DefaultAction":
					DefaultAction = info.GetBoolean("DefaultAction");
					break;
				case "HasHappened":
					HasHappened = info.GetBoolean("HasHappened");
					break;
				case "DiscardType":
					DiscardType = (DiscardType)info.GetValue("DiscardType", typeof(DiscardType));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsAbilities entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
			if (TilesMovedTo == null)
			{
				TilesMovedTo = new List<Point>();
			}
		}
	}

	public CPlayerStatsAbilities(CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData, int amount, bool specialAbility, bool? areaEffect, int targets, bool defaultAction, bool hasHappened, DiscardType discardType, Guid abilityID, int cardsInHandCount = 0, int cardsInDiscardCount = 0, List<string> targetGUIDs = null, List<CActor.EType> targetTypes = null, List<bool> advantage = null, bool isBuff = false, CAbility.EAbilityFlag flags = CAbility.EAbilityFlag.None, int tokens = 0, List<Point> tilesMoved = null)
		: base(baseData, actionData)
	{
		AbilityID = abilityID;
		Amount = amount;
		SpecialAbility = specialAbility;
		AreaEffect = areaEffect;
		Targets = targets;
		DefaultAction = defaultAction;
		HasHappened = hasHappened;
		DiscardType = discardType;
		TargetGUIDs = targetGUIDs?.ToList();
		TargetTypes = targetTypes?.ToList();
		CardsInHandCount = cardsInHandCount;
		CardsInDiscardCount = cardsInDiscardCount;
		Advantage = advantage;
		IsBuff = isBuff;
		Flags = flags;
		Tokens = tokens;
		TilesMovedTo = tilesMoved ?? new List<Point>();
	}

	public static CPlayerStatsAbilities CreateAbilityStat(SEventAbility sevent, string advGuid, string sceGuid, string currentQuestType, string mainActorType, string secondaryActorType, int amount, bool specialAbility, bool? areaEffect, int targets, string mainActorClassID = null, string secondaryActorClassID = null, bool performedBySummons = false, bool rolledIntoSummoner = false, List<bool> advantage = null, List<Point> tilesMoved = null)
	{
		CPlayerSatsBaseData baseData = new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder);
		string mainActorClassID2 = ((mainActorClassID != null) ? mainActorClassID : sevent.MainActorClassID);
		List<PositiveConditionPair> mainActorPositiveConditions = sevent.MainActorPositiveConditions;
		List<NegativeConditionPair> mainActorNegativeConditions = sevent.MainActorNegativeConditions;
		string mainActorGUID = sevent.MainActorGUID;
		string mainActorMonsterType = sevent.MainActorMonsterType;
		string secondaryActorMonsterType = sevent.SecondaryActorMonsterType;
		string secondaryActorClassID2 = ((secondaryActorClassID != null) ? secondaryActorClassID : sevent.SecondaryActorClassID);
		List<PositiveConditionPair> secondaryActorPositiveConditions = sevent.SecondaryActorPositiveConditions;
		List<NegativeConditionPair> secondaryActorNegativeConditions = sevent.SecondaryActorNegativeConditions;
		string turnActorGUID = sevent.TurnActorGUID;
		List<ElementInfusionBoardManager.EElement> elements = sevent.Elements;
		int cardID = sevent.CardID;
		CBaseCard.ECardType cardType = sevent.CardType;
		CAbility.EAbilityType abilityType = sevent.AbilityType;
		string cardName = sevent.CardName;
		int strength = sevent.Strength;
		int mainActorHealth = sevent.MainActorHealth;
		int mainActorMaxHealth = sevent.MainActorMaxHealth;
		return new CPlayerStatsAbilities(actionData: new CPlayerStatsActionData(mainActorClassID2, mainActorType, mainActorPositiveConditions, mainActorNegativeConditions, mainActorGUID, mainActorMonsterType, secondaryActorClassID2, secondaryActorType, secondaryActorPositiveConditions, secondaryActorNegativeConditions, null, secondaryActorMonsterType, turnActorGUID, elements, cardID, cardType, abilityType, cardName, strength, performedBySummons, rolledIntoSummoner, mainActorHealth, mainActorMaxHealth, 0, 0, 0, mainActorWallAdjacent: false, sevent.ControlledByClassID, sevent.ControlledByActorGuid, sevent.Position, 0, sevent.SecondaryActorHealth, sevent.SecondaryActorMaxHealth), abilityID: sevent.AbilityID, baseData: baseData, amount: amount, specialAbility: specialAbility, areaEffect: areaEffect, targets: targets, defaultAction: sevent.DefaultAction, hasHappened: sevent.HasHappened, discardType: sevent.DiscardType, cardsInHandCount: sevent.CardsInHandCount, cardsInDiscardCount: sevent.CardsInDiscardCount, targetGUIDs: sevent.TargetGUIDs, targetTypes: sevent.TargetTypes, advantage: advantage, isBuff: sevent.IsBuff, flags: sevent.Flags, tokens: sevent.Tokens, tilesMoved: tilesMoved);
	}
}
