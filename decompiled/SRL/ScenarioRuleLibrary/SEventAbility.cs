using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbility : SEvent
{
	[Serializable]
	public class SEventAbilityBaseData : ISerializable
	{
		public CActor.EType? MainActorType;

		public Guid AbilityID { get; private set; }

		public bool Advantage { get; set; }

		public string MainActorMonsterType { get; private set; }

		public string SecondaryActorMonsterType { get; private set; }

		public string MainActorGUID { get; set; }

		public List<string> TargetGUIDs { get; set; }

		public List<CActor.EType> TargetTypes { get; set; }

		public ESESubTypeAbility AbilitySubType { get; private set; }

		public CAbility.EAbilityType AbilityType { get; private set; }

		public string CardName { get; private set; }

		public int CardID { get; private set; }

		public CBaseCard.ECardType CardType { get; private set; }

		public string MainActorClassID { get; private set; }

		public int Strength { get; private set; }

		public int MainActorHealth { get; private set; }

		public int MainActorMaxHealth { get; private set; }

		public int SecondaryActorHealth { get; private set; }

		public int SecondaryActorMaxHealth { get; private set; }

		public DiscardType DiscardType { get; set; }

		public List<SEventAbilityAddedConditionData> AddedPositiveConditionData { get; private set; }

		public List<SEventAbilityAddedConditionData> AddedNegativeConditionData { get; private set; }

		public bool MainActorIsEnemySummon { get; private set; }

		public List<PositiveConditionPair> MainActorPositiveConditions { get; private set; }

		public List<NegativeConditionPair> MainActorNegativeConditions { get; private set; }

		public string SecondaryActorClassID { get; private set; }

		public CActor.EType? SecondaryActorType { get; private set; }

		public bool SecondaryActorIsSummon { get; private set; }

		public List<PositiveConditionPair> SecondaryActorPositiveConditions { get; private set; }

		public List<NegativeConditionPair> SecondaryActorNegativeConditions { get; private set; }

		public bool DefaultAction { get; private set; }

		public bool HasHappened { get; private set; }

		public bool IsBuff { get; private set; }

		public string ControlledByClassID { get; private set; }

		public string ControlledByActorGuid { get; private set; }

		public int CardsInHandCount { get; private set; }

		public int CardsInDiscardCount { get; private set; }

		public string TurnActorGUID { get; private set; }

		public Point Position { get; private set; }

		public CAbility.EAbilityFlag Flags { get; private set; }

		public int Tokens { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Tokens", Tokens);
			info.AddValue("Flags", Flags);
			info.AddValue("ControlledByActorGuid", ControlledByActorGuid);
			info.AddValue("AbilityID", AbilityID);
			info.AddValue("MainActorGUID", MainActorGUID);
			info.AddValue("TargetGUIDs", TargetGUIDs);
			info.AddValue("TargetTypes", TargetTypes);
			info.AddValue("AbilityType", AbilityType);
			info.AddValue("AbilitySubType", AbilitySubType);
			info.AddValue("CardName", CardName);
			info.AddValue("CardID", CardID);
			info.AddValue("CardType", CardType);
			info.AddValue("MainActorClassID", MainActorClassID);
			info.AddValue("Strength", Strength);
			info.AddValue("AddedPositiveConditionData", AddedPositiveConditionData);
			info.AddValue("AddedNegativeConditionData", AddedNegativeConditionData);
			info.AddValue("MainActorType", MainActorType);
			info.AddValue("MainActorIsEnemySummon", MainActorIsEnemySummon);
			info.AddValue("MainActorPositiveConditions", MainActorPositiveConditions);
			info.AddValue("MainActorNegativeConditions", MainActorNegativeConditions);
			info.AddValue("SecondaryActorClassID", SecondaryActorClassID);
			info.AddValue("SecondaryActorType", SecondaryActorType);
			info.AddValue("SecondaryActorIsSummon", SecondaryActorIsSummon);
			info.AddValue("SecondaryActorPositiveConditions", SecondaryActorPositiveConditions);
			info.AddValue("SecondaryActorNegativeConditions", SecondaryActorNegativeConditions);
			info.AddValue("DefaultAction", DefaultAction);
			info.AddValue("HasHappened", HasHappened);
			info.AddValue("IsBuff", IsBuff);
			info.AddValue("ControlledByClassID", ControlledByClassID);
			info.AddValue("CardsInHandCount", CardsInHandCount);
			info.AddValue("CardsInDiscardCount", CardsInDiscardCount);
			info.AddValue("MainActorHealth", MainActorHealth);
			info.AddValue("MainActorMaxHealth", MainActorMaxHealth);
			info.AddValue("SecondaryActorHealth", SecondaryActorHealth);
			info.AddValue("SecondaryActorMaxHealth", SecondaryActorMaxHealth);
			info.AddValue("MainActorMonsterType", MainActorMonsterType);
			info.AddValue("SecondaryActorMonsterType", SecondaryActorMonsterType);
			info.AddValue("DiscardType", DiscardType);
			info.AddValue("TurnActorGUID", TurnActorGUID);
			info.AddValue("Position", Position);
		}

		public SEventAbilityBaseData(SerializationInfo info, StreamingContext context)
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
					case "ControlledByActorGuid":
						ControlledByActorGuid = info.GetString("ControlledByActorGuid");
						break;
					case "AbilityID":
						AbilityID = (Guid)info.GetValue("AbilityID", typeof(Guid));
						break;
					case "Position":
						Position = (Point)info.GetValue("Position", typeof(Point));
						break;
					case "TargetGUIDs":
						TargetGUIDs = (List<string>)info.GetValue("TargetGUIDs", typeof(List<string>));
						break;
					case "TargetTypes":
						TargetTypes = (List<CActor.EType>)info.GetValue("TargetTypes", typeof(List<CActor.EType>));
						break;
					case "MainActorMonsterType":
						MainActorMonsterType = info.GetString("MainActorMonsterType");
						break;
					case "SecondaryActorMonsterType":
						SecondaryActorMonsterType = info.GetString("SecondaryActorMonsterType");
						break;
					case "MainActorGUID":
						MainActorGUID = info.GetString("MainActorGUID");
						break;
					case "ControlledByClassID":
						ControlledByClassID = info.GetString("ControlledByClassID");
						break;
					case "AbilityType":
						AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
						break;
					case "AbilitySubType":
						AbilitySubType = (ESESubTypeAbility)info.GetValue("AbilitySubType", typeof(ESESubTypeAbility));
						break;
					case "CardName":
						CardName = info.GetString("CardName");
						break;
					case "CardID":
						CardID = info.GetInt32("CardID");
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
					case "CardType":
						CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
						break;
					case "MainActorClassID":
						MainActorClassID = info.GetString("MainActorClassID");
						break;
					case "TurnActorGUID":
						TurnActorGUID = info.GetString("TurnActorGUID");
						break;
					case "Strength":
						Strength = info.GetInt32("Strength");
						break;
					case "CardsInHandCount":
						CardsInHandCount = info.GetInt32("CardsInHandCount");
						break;
					case "CardsInDiscardCount":
						CardsInDiscardCount = info.GetInt32("CardsInDiscardCount");
						break;
					case "AddedPositiveConditionData":
						AddedPositiveConditionData = (List<SEventAbilityAddedConditionData>)info.GetValue("AddedPositiveConditionData", typeof(List<SEventAbilityAddedConditionData>));
						break;
					case "AddedNegativeConditionData":
						AddedNegativeConditionData = (List<SEventAbilityAddedConditionData>)info.GetValue("AddedNegativeConditionData", typeof(List<SEventAbilityAddedConditionData>));
						break;
					case "MainActorType":
						MainActorType = (CActor.EType?)info.GetValue("MainActorType", typeof(CActor.EType?));
						break;
					case "MainActorIsEnemySummon":
						MainActorIsEnemySummon = info.GetBoolean("MainActorIsEnemySummon");
						break;
					case "DiscardType":
						DiscardType = (DiscardType)info.GetValue("DiscardType", typeof(DiscardType));
						break;
					case "MainActorPositiveConditions":
						MainActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("MainActorPositiveConditions", typeof(List<PositiveConditionPair>));
						break;
					case "MainActorNegativeConditions":
						MainActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("MainActorNegativeConditions", typeof(List<NegativeConditionPair>));
						break;
					case "SecondaryActorClassID":
						SecondaryActorClassID = info.GetString("SecondaryActorClassID");
						break;
					case "SecondaryActorType":
						SecondaryActorType = (CActor.EType?)info.GetValue("SecondaryActorType", typeof(CActor.EType?));
						break;
					case "SecondaryActorIsSummon":
						SecondaryActorIsSummon = info.GetBoolean("SecondaryActorIsSummon");
						break;
					case "SecondaryActorPositiveConditions":
						SecondaryActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("SecondaryActorPositiveConditions", typeof(List<PositiveConditionPair>));
						break;
					case "SecondaryActorNegativeConditions":
						SecondaryActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("SecondaryActorNegativeConditions", typeof(List<NegativeConditionPair>));
						break;
					case "DefaultAction":
						DefaultAction = info.GetBoolean("DefaultAction");
						break;
					case "HasHappened":
						HasHappened = info.GetBoolean("HasHappened");
						break;
					case "IsBuff":
						IsBuff = info.GetBoolean("IsBuff");
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventAbility entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
			if (AddedPositiveConditionData == null)
			{
				AddedPositiveConditionData = new List<SEventAbilityAddedConditionData>();
			}
			if (AddedNegativeConditionData == null)
			{
				AddedNegativeConditionData = new List<SEventAbilityAddedConditionData>();
			}
		}

		public SEventAbilityBaseData(CAbility.EAbilityType abilityType, ESESubTypeAbility abilitySubType, string cardName, int cardID, CBaseCard.ECardType cardType, string mainActorClassID, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? mainActorType, bool mainActorIsEnemySummon, List<PositiveConditionPair> mainActorPositiveConditions, List<NegativeConditionPair> mainActorNegativeConditions, string mainActorGUID, string mainActorMonsterType, string secondaryActorClassID, CActor.EType secondaryActorType, bool secondaryActorIsSummon, List<PositiveConditionPair> secondaryActorPositiveConditions, List<NegativeConditionPair> secondaryActorNegativeConditions, string secondaryActorMonsterType, List<string> targetGUIDs, List<CActor.EType> targetTypes, int mainActorHealth, int mainActorMaxHealth, Guid abilityGuid, CAbility.EAbilityFlag flags, DiscardType discardType = DiscardType.None, bool defaultAction = false, bool hasHappened = true, bool isBuff = false, string controlledByClassID = null, int cardsInHandCount = 0, int cardsInDiscardCount = 0, Point position = default(Point), int secondaryActorHealth = 0, int secondaryActorMaxHealth = 0, string controlledByActorGuid = null, int tokens = 0)
		{
			Tokens = tokens;
			Flags = flags;
			AbilityID = abilityGuid;
			Position = position;
			TurnActorGUID = GameState.TurnActor?.ActorGuid;
			AbilityType = abilityType;
			AbilitySubType = abilitySubType;
			CardName = cardName;
			CardID = cardID;
			CardType = cardType;
			MainActorClassID = mainActorClassID;
			Strength = strength;
			MainActorType = mainActorType;
			MainActorIsEnemySummon = mainActorIsEnemySummon;
			MainActorMonsterType = mainActorMonsterType;
			SecondaryActorClassID = secondaryActorClassID;
			SecondaryActorType = secondaryActorType;
			SecondaryActorIsSummon = secondaryActorIsSummon;
			SecondaryActorMonsterType = secondaryActorMonsterType;
			DefaultAction = defaultAction;
			HasHappened = hasHappened;
			IsBuff = isBuff;
			ControlledByClassID = controlledByClassID;
			ControlledByActorGuid = controlledByActorGuid;
			MainActorGUID = mainActorGUID;
			TargetGUIDs = targetGUIDs?.ToList();
			TargetTypes = targetTypes?.ToList();
			DiscardType = discardType;
			AddedPositiveConditionData = new List<SEventAbilityAddedConditionData>();
			if (addedPositiveConditions != null && addedPositiveConditions.Count > 0)
			{
				foreach (CAbility addedPositiveCondition in addedPositiveConditions)
				{
					CAbilityCondition cAbilityCondition = addedPositiveCondition as CAbilityCondition;
					AddedPositiveConditionData.Add(new SEventAbilityAddedConditionData(cAbilityCondition.AbilityType, cAbilityCondition.Duration, cAbilityCondition.DecrementTrigger));
				}
			}
			AddedNegativeConditionData = new List<SEventAbilityAddedConditionData>();
			if (addedNegativeConditions != null && addedNegativeConditions.Count > 0)
			{
				foreach (CAbility addedNegativeCondition in addedNegativeConditions)
				{
					CAbilityCondition cAbilityCondition2 = addedNegativeCondition as CAbilityCondition;
					AddedNegativeConditionData.Add(new SEventAbilityAddedConditionData(cAbilityCondition2.AbilityType, cAbilityCondition2.Duration, cAbilityCondition2.DecrementTrigger));
				}
			}
			MainActorNegativeConditions = new List<NegativeConditionPair>();
			if (mainActorNegativeConditions != null && mainActorNegativeConditions.Count > 0)
			{
				foreach (NegativeConditionPair mainActorNegativeCondition in mainActorNegativeConditions)
				{
					MainActorNegativeConditions.Add(new NegativeConditionPair(mainActorNegativeCondition.NegativeCondition, mainActorNegativeCondition.MapDuration, mainActorNegativeCondition.RoundDuration, mainActorNegativeCondition.ConditionDecTrigger, mainActorNegativeCondition.Strength));
				}
			}
			MainActorPositiveConditions = new List<PositiveConditionPair>();
			if (mainActorPositiveConditions != null && mainActorPositiveConditions.Count > 0)
			{
				foreach (PositiveConditionPair mainActorPositiveCondition in mainActorPositiveConditions)
				{
					MainActorPositiveConditions.Add(new PositiveConditionPair(mainActorPositiveCondition.PositiveCondition, mainActorPositiveCondition.MapDuration, mainActorPositiveCondition.RoundDuration, mainActorPositiveCondition.ConditionDecTrigger, mainActorPositiveCondition.CasterClassGUID));
				}
			}
			SecondaryActorNegativeConditions = new List<NegativeConditionPair>();
			if (secondaryActorNegativeConditions != null && secondaryActorNegativeConditions.Count > 0)
			{
				foreach (NegativeConditionPair secondaryActorNegativeCondition in secondaryActorNegativeConditions)
				{
					SecondaryActorNegativeConditions.Add(new NegativeConditionPair(secondaryActorNegativeCondition.NegativeCondition, secondaryActorNegativeCondition.MapDuration, secondaryActorNegativeCondition.RoundDuration, secondaryActorNegativeCondition.ConditionDecTrigger, secondaryActorNegativeCondition.Strength));
				}
			}
			SecondaryActorPositiveConditions = new List<PositiveConditionPair>();
			if (secondaryActorPositiveConditions != null && secondaryActorPositiveConditions.Count > 0)
			{
				foreach (PositiveConditionPair secondaryActorPositiveCondition in secondaryActorPositiveConditions)
				{
					SecondaryActorPositiveConditions.Add(new PositiveConditionPair(secondaryActorPositiveCondition.PositiveCondition, secondaryActorPositiveCondition.MapDuration, secondaryActorPositiveCondition.RoundDuration, secondaryActorPositiveCondition.ConditionDecTrigger, secondaryActorPositiveCondition.CasterClassGUID));
				}
			}
			CardsInHandCount = cardsInHandCount;
			CardsInDiscardCount = cardsInDiscardCount;
			MainActorHealth = mainActorHealth;
			MainActorMaxHealth = mainActorMaxHealth;
			SecondaryActorHealth = secondaryActorHealth;
			SecondaryActorMaxHealth = secondaryActorMaxHealth;
		}

		public SEventAbilityBaseData(CActor mainActor, CAbility.EAbilityType abilityType, ESESubTypeAbility abilitySubType, CBaseCard card, List<string> targetGUIDs, List<CActor.EType> targetTypes, bool hasHappened, CAbility.EAbilityFlag flags, Guid abilityGuid = default(Guid), DiscardType discardType = DiscardType.None, List<CAbility> addedPositiveConditions = null, List<CAbility> addedNegativeConditions = null, bool isBuff = false, int cardsInHand = -1, int cardsInDiscard = -1, int strength = 1, bool defaultAction = false, string secondaryActorClassID = null, CActor.EType secondaryActorType = CActor.EType.Unknown, bool secondaryActorIsSummon = false, List<PositiveConditionPair> secondaryActorPositiveConditions = null, List<NegativeConditionPair> secondaryActorNegativeConditions = null, string secondaryActorMonsterType = null, Point position = default(Point), int secondaryActorHealth = 0, int secondaryActorMaxHealth = 0, int tokens = 0, bool topAction = false)
		{
			string cardName = card?.Name;
			int cardID = card?.ID ?? int.MaxValue;
			CBaseCard.ECardType cardType = card?.CardType ?? CBaseCard.ECardType.None;
			string mainActorClassID = mainActor?.Class?.ID;
			List<CAbility> addedPositiveConditions2 = ((addedPositiveConditions == null) ? new List<CAbility>() : addedPositiveConditions);
			List<CAbility> addedNegativeConditions2 = ((addedNegativeConditions == null) ? new List<CAbility>() : addedNegativeConditions);
			CActor.EType? mainActorType = mainActor?.Type;
			List<PositiveConditionPair> mainActorPositiveConditions = mainActor?.Tokens?.CheckPositiveTokens;
			List<NegativeConditionPair> mainActorNegativeConditions = mainActor?.Tokens?.CheckNegativeTokens;
			string text = ((mainActor?.MonsterClass == null) ? null : mainActor.MonsterClass.MonsterType.ToString());
			bool flag = secondaryActorIsSummon;
			this._002Ector(mainActorGUID: mainActor?.ActorGuid, mainActorMonsterType: text, secondaryActorClassID: secondaryActorClassID, secondaryActorType: secondaryActorType, secondaryActorIsSummon: flag, secondaryActorPositiveConditions: secondaryActorPositiveConditions, secondaryActorNegativeConditions: secondaryActorNegativeConditions, secondaryActorMonsterType: secondaryActorMonsterType, flags: flags, abilityGuid: abilityGuid, discardType: discardType, abilityType: abilityType, abilitySubType: abilitySubType, cardName: cardName, cardID: cardID, cardType: cardType, mainActorClassID: mainActorClassID, strength: strength, addedPositiveConditions: addedPositiveConditions2, addedNegativeConditions: addedNegativeConditions2, mainActorType: mainActorType, mainActorIsEnemySummon: false, mainActorPositiveConditions: mainActorPositiveConditions, mainActorNegativeConditions: mainActorNegativeConditions, targetGUIDs: targetGUIDs, targetTypes: targetTypes, mainActorHealth: mainActor?.Health ?? 0, mainActorMaxHealth: mainActor?.MaxHealth ?? 0, defaultAction: defaultAction, hasHappened: hasHappened, isBuff: isBuff, controlledByClassID: mainActor?.MindControlAbility?.TargetingActor?.Class?.ID, cardsInHandCount: (cardsInHand != -1) ? cardsInHand : (mainActor?.CharacterClass?.HandAbilityCards?.Count).GetValueOrDefault(), cardsInDiscardCount: (cardsInDiscard != -1) ? cardsInDiscard : (mainActor?.CharacterClass?.DiscardedAbilityCards?.Count).GetValueOrDefault(), position: (position != default(Point)) ? position : (mainActor?.ArrayIndex ?? default(Point)), secondaryActorHealth: secondaryActorHealth, secondaryActorMaxHealth: secondaryActorMaxHealth, controlledByActorGuid: null, tokens: tokens);
			MainActorIsEnemySummon = false;
			CActor cActor = mainActor;
			if (cActor != null && cActor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == mainActor.ActorGuid);
				if (cEnemyActor != null)
				{
					MainActorIsEnemySummon = cEnemyActor.IsSummon;
				}
			}
		}

		public SEventAbilityBaseData(CActor mainActor, ESESubTypeAbility abilitySubType, CAbility ability, bool defaultAction = false, int strength = 1, bool isBuff = false, string secondaryActorClassID = null, CActor.EType secondaryActorType = CActor.EType.Unknown, bool secondaryActorIsSummon = false, List<PositiveConditionPair> secondaryActorPositiveConditions = null, List<NegativeConditionPair> secondaryActorNegativeConditions = null, string secondaryActorMonsterType = null, Point position = default(Point), int secondaryActorHealth = 0, int secondaryActorMaxHealth = 0)
			: this(mainActor, (ability.ActiveBonusData != null && ability.ActiveBonusData.OverrideAsSong) ? CAbility.EAbilityType.PlaySong : ability.AbilityType, abilitySubType, ability?.GetAbilityBaseCard(), ability.ActorsTargetedGUIDS, ability.ActorsTargetedTypes, ability.AbilityHasHappened, ability?.Flags ?? CAbility.EAbilityFlag.None, ability?.ID ?? default(Guid), (ability.GetAbilityBaseCard() as CAbilityCard)?.GetDiscardType(ability) ?? DiscardType.None, ability.PositiveConditions.Values.ToList(), ability.NegativeConditions.Values.ToList(), isBuff, -1, -1, strength, defaultAction, secondaryActorClassID, secondaryActorType, secondaryActorIsSummon, secondaryActorPositiveConditions, secondaryActorNegativeConditions, secondaryActorMonsterType, position, secondaryActorHealth, secondaryActorMaxHealth, ability?.Tokens ?? 0)
		{
		}

		public SEventAbilityBaseData(CActor mainActor, ESESubTypeAbility abilitySubType, CAbility ability, CActor secondaryActor, bool defaultAction = false, int strength = 1, bool isBuff = false, Point position = default(Point))
			: this(mainActor, abilitySubType, ability, defaultAction, strength, isBuff, null, CActor.EType.Unknown, secondaryActorIsSummon: false, null, null, (secondaryActor?.MonsterClass == null) ? null : secondaryActor.MonsterClass.MonsterType.ToString(), position, secondaryActor?.Health ?? 0, secondaryActor?.MaxHealth ?? 0)
		{
			if (secondaryActor == null)
			{
				return;
			}
			CActor.EType eType = CActor.EType.Unknown;
			bool secondaryActorIsSummon = false;
			CActor cActor = null;
			_ = secondaryActor.Class.ID;
			eType = secondaryActor.Type;
			if (cActor != null && cActor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == secondaryActor.ActorGuid);
				if (cEnemyActor != null)
				{
					secondaryActorIsSummon = cEnemyActor.IsSummon;
				}
			}
			if (secondaryActor == null)
			{
				return;
			}
			SecondaryActorClassID = secondaryActor.Class.ID;
			SecondaryActorType = eType;
			SecondaryActorIsSummon = secondaryActorIsSummon;
			List<PositiveConditionPair> checkPositiveTokens = secondaryActor.Tokens.CheckPositiveTokens;
			List<NegativeConditionPair> checkNegativeTokens = secondaryActor.Tokens.CheckNegativeTokens;
			SecondaryActorNegativeConditions = new List<NegativeConditionPair>();
			if (checkNegativeTokens != null && checkNegativeTokens.Count > 0)
			{
				foreach (NegativeConditionPair item in checkNegativeTokens)
				{
					SecondaryActorNegativeConditions.Add(new NegativeConditionPair(item.NegativeCondition, item.MapDuration, item.RoundDuration, item.ConditionDecTrigger, item.Strength));
				}
			}
			SecondaryActorPositiveConditions = new List<PositiveConditionPair>();
			if (checkPositiveTokens == null || checkPositiveTokens.Count <= 0)
			{
				return;
			}
			foreach (PositiveConditionPair item2 in checkPositiveTokens)
			{
				SecondaryActorPositiveConditions.Add(new PositiveConditionPair(item2.PositiveCondition, item2.MapDuration, item2.RoundDuration, item2.ConditionDecTrigger, item2.CasterClassGUID));
			}
		}
	}

	[Serializable]
	public class SEventAbilityAddedConditionData : ISerializable
	{
		public CAbility.EAbilityType AddedConditionType { get; private set; }

		public int Duration { get; private set; }

		public EConditionDecTrigger ConditionDecTrigger { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AddedConditionType", AddedConditionType);
			info.AddValue("Duration", Duration);
			info.AddValue("ConditionDecTrigger", ConditionDecTrigger);
		}

		public SEventAbilityAddedConditionData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "AddedConditionType":
						AddedConditionType = (CAbility.EAbilityType)info.GetValue("AddedConditionType", typeof(CAbility.EAbilityType));
						break;
					case "Duration":
						Duration = info.GetInt32("Duration");
						break;
					case "ConditionDecTrigger":
						ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecTrigger", typeof(EConditionDecTrigger));
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityAddedConditionData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SEventAbilityAddedConditionData(CAbility.EAbilityType conditionType, int duration, EConditionDecTrigger conditionDecTrigger)
		{
			AddedConditionType = conditionType;
			Duration = duration;
			ConditionDecTrigger = conditionDecTrigger;
		}
	}

	public SEventAbilityBaseData BaseData;

	public string MainActorGUID => BaseData.MainActorGUID;

	public string MainActorMonsterType => BaseData.MainActorMonsterType;

	public string SecondaryActorMonsterType => BaseData.SecondaryActorMonsterType;

	public List<string> TargetGUIDs => BaseData.TargetGUIDs;

	public List<CActor.EType> TargetTypes => BaseData.TargetTypes;

	public ESESubTypeAbility AbilitySubType => BaseData.AbilitySubType;

	public CAbility.EAbilityType AbilityType => BaseData.AbilityType;

	public string CardName => BaseData.CardName;

	public int CardID => BaseData.CardID;

	public Guid AbilityID => BaseData.AbilityID;

	public CBaseCard.ECardType CardType => BaseData.CardType;

	public string MainActorClassID => BaseData.MainActorClassID;

	public int Strength => BaseData.Strength;

	public int MainActorHealth => BaseData.MainActorHealth;

	public int MainActorMaxHealth => BaseData.MainActorMaxHealth;

	public int SecondaryActorHealth => BaseData.SecondaryActorHealth;

	public int SecondaryActorMaxHealth => BaseData.SecondaryActorMaxHealth;

	public DiscardType DiscardType => BaseData.DiscardType;

	public List<SEventAbilityAddedConditionData> AddedPositiveConditionData => BaseData.AddedPositiveConditionData;

	public List<SEventAbilityAddedConditionData> AddedNegativeConditionData => BaseData.AddedNegativeConditionData;

	public CActor.EType? MainActorType => BaseData.MainActorType;

	public bool MainActorIsEnemySummon => BaseData.MainActorIsEnemySummon;

	public List<PositiveConditionPair> MainActorPositiveConditions => BaseData.MainActorPositiveConditions;

	public List<NegativeConditionPair> MainActorNegativeConditions => BaseData.MainActorNegativeConditions;

	public string SecondaryActorClassID => BaseData.SecondaryActorClassID;

	public CActor.EType? SecondaryActorType => BaseData.SecondaryActorType;

	public bool SecondaryActorIsSummon => BaseData.SecondaryActorIsSummon;

	public List<PositiveConditionPair> SecondaryActorPositiveConditions => BaseData.SecondaryActorPositiveConditions;

	public List<NegativeConditionPair> SecondaryActorNegativeConditions => BaseData.SecondaryActorNegativeConditions;

	public bool DefaultAction => BaseData.DefaultAction;

	public bool HasHappened => BaseData.HasHappened;

	public bool IsBuff => BaseData.IsBuff;

	public string ControlledByClassID => BaseData.ControlledByClassID;

	public string ControlledByActorGuid => BaseData.ControlledByActorGuid;

	public int CardsInHandCount => BaseData.CardsInHandCount;

	public int CardsInDiscardCount => BaseData.CardsInDiscardCount;

	public string TurnActorGUID => BaseData.TurnActorGUID;

	public Point Position => BaseData.Position;

	public CAbility.EAbilityFlag Flags => BaseData.Flags;

	public int Tokens => BaseData.Tokens;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("BaseData", BaseData);
	}

	public SEventAbility(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "BaseData")
				{
					BaseData = (SEventAbilityBaseData)info.GetValue("BaseData", typeof(SEventAbilityBaseData));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbility entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbility(SEventAbilityBaseData baseData, string text = "")
		: base(ESEType.Ability, text)
	{
		BaseData = baseData;
	}
}
