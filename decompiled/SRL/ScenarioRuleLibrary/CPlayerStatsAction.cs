using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsAction : CPlayerStatsBase, ISerializable
{
	[Serializable]
	public class CPlayerStatsActionData : ISerializable
	{
		public string MainActorClassID { get; private set; }

		public string MainActorMonsterType { get; private set; }

		public string MainActorType { get; set; }

		public List<PositiveConditionPair> MainActorPositiveConditions { get; private set; }

		public List<NegativeConditionPair> MainActorNegativeConditions { get; private set; }

		public string MainActorGUID { get; set; }

		public string SecondaryActorClassID { get; set; }

		public string SecondaryActorType { get; set; }

		public List<ElementInfusionBoardManager.EElement> Infused { get; private set; }

		public List<PositiveConditionPair> SecondaryActorPositiveConditions { get; private set; }

		public List<NegativeConditionPair> SecondaryActorNegativeConditions { get; private set; }

		public string SecondaryActorMonsterType { get; private set; }

		public string SecondaryActorGUID { get; set; }

		public int CardID { get; private set; }

		public CBaseCard.ECardType CardType { get; private set; }

		public string ActingAbilityName { get; private set; }

		public CAbility.EAbilityType AbilityType { get; private set; }

		public int AbilityStrength { get; set; }

		public bool PerformedBySummons { get; private set; }

		public bool RolledIntoSummoner { get; private set; }

		public int MainActorHealth { get; private set; }

		public int MainActorMaxHealth { get; private set; }

		public int SecondaryActorHealth { get; private set; }

		public int SecondaryActorMaxHealth { get; private set; }

		public int MainActorAllyAdjacent { get; private set; }

		public int MainActorEnemyAdjacent { get; private set; }

		public int MainActorObstacleAdjacent { get; private set; }

		public bool MainActorWallAdjacent { get; private set; }

		public int MainActorPlayerAdjacent { get; private set; }

		public int SecondaryActorAllyAdjacent { get; private set; }

		public int SecondaryActorEnemyAdjacent { get; private set; }

		public int SecondaryActorObstacleAdjacent { get; private set; }

		public bool SecondaryActorWallAdjacent { get; private set; }

		public int SecondaryActorPlayerAdjacent { get; private set; }

		public string ControlledByClassID { get; private set; }

		public string ControlledByActorGuid { get; private set; }

		public string TurnActorGUID { get; private set; }

		public Point Position { get; private set; }

		public bool MainActorResting { get; private set; }

		public uint ResonanceTokens { get; private set; }

		public CPlayerActor.EPressure Pressure { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (Pressure != CPlayerActor.EPressure.None)
			{
				info.AddValue("Pressure", Pressure);
			}
			if (ResonanceTokens != 0)
			{
				info.AddValue("ResonanceTokens", ResonanceTokens);
			}
			if (ControlledByActorGuid != null)
			{
				info.AddValue("ControlledByClassGuid", ControlledByActorGuid);
			}
			if (MainActorClassID != null)
			{
				info.AddValue("MainActorClassID", MainActorClassID);
			}
			if (SecondaryActorClassID != null)
			{
				info.AddValue("SecondaryActorClassID", SecondaryActorClassID);
			}
			if (MainActorType != null)
			{
				info.AddValue("MainActorType", MainActorType);
			}
			if (SecondaryActorType != null)
			{
				info.AddValue("SecondaryActorType", SecondaryActorType);
			}
			if (Infused != null)
			{
				info.AddValue("Infused", Infused);
			}
			if (MainActorPositiveConditions != null)
			{
				info.AddValue("MainActorPositiveConditions", MainActorPositiveConditions);
			}
			if (MainActorNegativeConditions != null)
			{
				info.AddValue("MainActorNegativeConditions", MainActorNegativeConditions);
			}
			if (SecondaryActorPositiveConditions != null)
			{
				info.AddValue("SecondaryActorPositiveConditions", SecondaryActorPositiveConditions);
			}
			if (SecondaryActorNegativeConditions != null)
			{
				info.AddValue("SecondaryActorNegativeConditions", SecondaryActorNegativeConditions);
			}
			if (MainActorGUID != null)
			{
				info.AddValue("MainActorGUID", MainActorGUID);
			}
			if (SecondaryActorGUID != null)
			{
				info.AddValue("SecondaryActorGUID", SecondaryActorGUID);
			}
			if (CardID != 0)
			{
				info.AddValue("CardID", CardID);
			}
			if (CardType != CBaseCard.ECardType.None)
			{
				info.AddValue("CardType", CardType);
			}
			if (ActingAbilityName != null)
			{
				info.AddValue("ActingAbilityName", ActingAbilityName);
			}
			if (AbilityType != CAbility.EAbilityType.None)
			{
				info.AddValue("AbilityType", AbilityType);
			}
			if (AbilityStrength != 0)
			{
				info.AddValue("AbilityStrength", AbilityStrength);
			}
			if (PerformedBySummons)
			{
				info.AddValue("PerformedBySummons", PerformedBySummons);
			}
			if (RolledIntoSummoner)
			{
				info.AddValue("RolledIntoSummoner", RolledIntoSummoner);
			}
			if (MainActorHealth != 0)
			{
				info.AddValue("MainActorHealth", MainActorHealth);
			}
			if (MainActorMaxHealth != 0)
			{
				info.AddValue("MainActorMaxHealth", MainActorMaxHealth);
			}
			if (SecondaryActorHealth != 0)
			{
				info.AddValue("SecondaryActorHealth", SecondaryActorHealth);
			}
			if (SecondaryActorMaxHealth != 0)
			{
				info.AddValue("SecondaryActorMaxHealth", SecondaryActorMaxHealth);
			}
			if (MainActorAllyAdjacent != 0)
			{
				info.AddValue("MainActorAllyAdjacent", MainActorAllyAdjacent);
			}
			if (MainActorEnemyAdjacent != 0)
			{
				info.AddValue("MainActorEnemyAdjacent", MainActorEnemyAdjacent);
			}
			if (MainActorObstacleAdjacent != 0)
			{
				info.AddValue("MainActorObstacleAdjacent", MainActorObstacleAdjacent);
			}
			if (MainActorWallAdjacent)
			{
				info.AddValue("MainActorWallAdjacent", MainActorWallAdjacent);
			}
			if (MainActorPlayerAdjacent != 0)
			{
				info.AddValue("MainActorPlayerAdjacent", MainActorPlayerAdjacent);
			}
			if (SecondaryActorAllyAdjacent != 0)
			{
				info.AddValue("SecondaryActorAllyAdjacent", SecondaryActorAllyAdjacent);
			}
			if (SecondaryActorAllyAdjacent != 0)
			{
				info.AddValue("SecondaryActorEnemyAdjacent", SecondaryActorEnemyAdjacent);
			}
			if (SecondaryActorObstacleAdjacent != 0)
			{
				info.AddValue("SecondaryActorObstacleAdjacent", SecondaryActorObstacleAdjacent);
			}
			if (SecondaryActorWallAdjacent)
			{
				info.AddValue("SecondaryActorWallAdjacent", SecondaryActorWallAdjacent);
			}
			if (SecondaryActorPlayerAdjacent != 0)
			{
				info.AddValue("SecondaryActorPlayerAdjacent", SecondaryActorPlayerAdjacent);
			}
			if (ControlledByClassID != null)
			{
				info.AddValue("ControlledByClassID", ControlledByClassID);
			}
			if (MainActorMonsterType != null)
			{
				info.AddValue("MainActorMonsterType", MainActorMonsterType);
			}
			if (SecondaryActorMonsterType != null)
			{
				info.AddValue("SecondaryActorMonsterType", SecondaryActorMonsterType);
			}
			if (TurnActorGUID != null)
			{
				info.AddValue("TurnActorGUID", TurnActorGUID);
			}
			if (Position != default(Point))
			{
				info.AddValue("Position", Position);
			}
			if (MainActorResting)
			{
				info.AddValue("MainActorResting", MainActorResting);
			}
		}

		public CPlayerStatsActionData(SerializationInfo info, StreamingContext context)
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
					case "Position":
						Position = (Point)info.GetValue("Position", typeof(Point));
						break;
					case "MainActorMonsterType":
						MainActorMonsterType = info.GetString("MainActorMonsterType");
						break;
					case "ControlledByActorGuid":
						ControlledByActorGuid = info.GetString("ControlledByActorGuid");
						break;
					case "SecondaryActorMonsterType":
						SecondaryActorMonsterType = info.GetString("SecondaryActorMonsterType");
						break;
					case "MainActorClassID":
						MainActorClassID = info.GetString("MainActorClassID");
						break;
					case "SecondaryActorClassID":
						SecondaryActorClassID = info.GetString("SecondaryActorClassID");
						break;
					case "MainActorType":
						MainActorType = info.GetString("MainActorType");
						break;
					case "SecondaryActorType":
						SecondaryActorType = info.GetString("SecondaryActorType");
						break;
					case "TurnActorGUID":
						TurnActorGUID = info.GetString("TurnActorGUID");
						break;
					case "Infused":
						Infused = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Infused", typeof(List<ElementInfusionBoardManager.EElement>));
						break;
					case "MainActorPositiveConditions":
						MainActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("MainActorPositiveConditions", typeof(List<PositiveConditionPair>));
						break;
					case "MainActorNegativeConditions":
						MainActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("MainActorNegativeConditions", typeof(List<NegativeConditionPair>));
						break;
					case "SecondaryActorPositiveConditions":
						SecondaryActorPositiveConditions = (List<PositiveConditionPair>)info.GetValue("SecondaryActorPositiveConditions", typeof(List<PositiveConditionPair>));
						break;
					case "SecondaryActorNegativeConditions":
						SecondaryActorNegativeConditions = (List<NegativeConditionPair>)info.GetValue("SecondaryActorNegativeConditions", typeof(List<NegativeConditionPair>));
						break;
					case "MainActorGUID":
						MainActorGUID = info.GetString("MainActorGUID");
						break;
					case "SecondaryActorGUID":
						SecondaryActorGUID = info.GetString("SecondaryActorGUID");
						break;
					case "CardID":
						CardID = info.GetInt32("CardID");
						break;
					case "CardType":
						CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
						break;
					case "ActingAbilityName":
						ActingAbilityName = info.GetString("ActingAbilityName");
						break;
					case "AbilityType":
						AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
						break;
					case "AbilityStrength":
						AbilityStrength = info.GetInt32("AbilityStrength");
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
					case "PerformedBySummons":
						PerformedBySummons = info.GetBoolean("PerformedBySummons");
						break;
					case "RolledIntoSummoner":
						PerformedBySummons = info.GetBoolean("RolledIntoSummoner");
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
					case "ControlledByClassID":
						ControlledByClassID = info.GetString("ControlledByClassID");
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsAction entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public CPlayerStatsActionData(string mainActorClassID, string mainActorType, List<PositiveConditionPair> mainActorPositiveConditions, List<NegativeConditionPair> mainActorNegativeConditions, string mainActorGUID, string mainActorMonsterType, string secondaryActorClassID, string secondaryActorType, List<PositiveConditionPair> secondaryActorPositiveConditions, List<NegativeConditionPair> secondaryActorNegativeConditions, string secondaryActorGUID, string secondaryActorMonsterType, string turnActorGUID, List<ElementInfusionBoardManager.EElement> infused, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false, int health = 0, int maxHealth = 0, int mainActorAllyAdjacent = 0, int mainActorEnemyAdjacent = 0, int mainActorObstacleAdjacent = 0, bool mainActorWallAdjacent = false, string controlledByClassID = null, string controlledByActorGuid = null, Point position = default(Point), int mainActorPlayerAdjacent = 0, int secondaryActorHealth = 0, int secondaryActorMaxHealth = 0, int secondaryActorAllyAdjacent = 0, int secondaryActorEnemyAdjacent = 0, int secondaryActorObstacleAdjacent = 0, bool secondaryActorWallAdjacent = false, int secondaryActorPlayerAdjacent = 0, bool mainActorResting = false, uint resonance = 0u, CPlayerActor.EPressure pressure = CPlayerActor.EPressure.None)
		{
			Pressure = pressure;
			ResonanceTokens = resonance;
			MainActorResting = mainActorResting;
			MainActorClassID = mainActorClassID;
			MainActorType = mainActorType;
			MainActorPositiveConditions = mainActorPositiveConditions;
			MainActorNegativeConditions = mainActorNegativeConditions;
			MainActorGUID = mainActorGUID;
			MainActorMonsterType = mainActorMonsterType;
			SecondaryActorMonsterType = secondaryActorMonsterType;
			SecondaryActorClassID = secondaryActorClassID;
			SecondaryActorType = secondaryActorType;
			SecondaryActorPositiveConditions = secondaryActorPositiveConditions;
			SecondaryActorNegativeConditions = secondaryActorNegativeConditions;
			SecondaryActorGUID = secondaryActorGUID;
			TurnActorGUID = turnActorGUID;
			Infused = infused;
			CardID = cardID;
			CardType = cardType;
			AbilityType = abilityType;
			ActingAbilityName = actingAbilityName;
			AbilityStrength = abilityStrength;
			PerformedBySummons = performedBySummons;
			RolledIntoSummoner = rolledIntoSummoner;
			MainActorHealth = health;
			MainActorMaxHealth = maxHealth;
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
			ControlledByClassID = controlledByClassID;
			ControlledByActorGuid = controlledByActorGuid;
			Position = position;
			SecondaryActorHealth = secondaryActorHealth;
			SecondaryActorMaxHealth = secondaryActorMaxHealth;
		}
	}

	public CPlayerStatsActionData ActionData { get; private set; }

	public bool MainActorResting => ActionData.MainActorResting;

	public string MainActorClassID => ActionData.MainActorClassID;

	public string MainActorMonsterType => ActionData.MainActorMonsterType;

	public string SecondaryActorClassID => ActionData.SecondaryActorClassID;

	public string MainActorType => ActionData.MainActorType;

	public string SecondaryActorType => ActionData.SecondaryActorType;

	public List<ElementInfusionBoardManager.EElement> Infused => ActionData.Infused;

	public List<PositiveConditionPair> MainActorPositiveConditions => ActionData.MainActorPositiveConditions;

	public List<NegativeConditionPair> MainActorNegativeConditions => ActionData.MainActorNegativeConditions;

	public List<PositiveConditionPair> SecondaryActorPositiveConditions => ActionData.SecondaryActorPositiveConditions;

	public List<NegativeConditionPair> SecondaryActorNegativeConditions => ActionData.SecondaryActorNegativeConditions;

	public string MainActorGUID => ActionData.MainActorGUID;

	public string SecondaryActorGUID => ActionData.SecondaryActorGUID;

	public int CardID => ActionData.CardID;

	public CBaseCard.ECardType CardType => ActionData.CardType;

	public string ActingAbilityName => ActionData.ActingAbilityName;

	public CAbility.EAbilityType AbilityType => ActionData.AbilityType;

	public int AbilityStrength
	{
		get
		{
			return ActionData.AbilityStrength;
		}
		set
		{
			ActionData.AbilityStrength = value;
		}
	}

	public bool PerformedBySummons => ActionData.PerformedBySummons;

	public bool RolledIntoSummoner => ActionData.RolledIntoSummoner;

	public int MainActorHealth => ActionData.MainActorHealth;

	public int MainActorMaxHealth => ActionData.MainActorMaxHealth;

	public int SecondaryActorHealth => ActionData.SecondaryActorHealth;

	public int SecondaryActorMaxHealth => ActionData.SecondaryActorMaxHealth;

	public int MainActorAllyAdjacent => ActionData.MainActorAllyAdjacent;

	public int MainActorEnemyAdjacent => ActionData.MainActorEnemyAdjacent;

	public int MainActorObstacleAdjacent => ActionData.MainActorObstacleAdjacent;

	public bool MainActorWallAdjacent => ActionData.MainActorWallAdjacent;

	public int MainActorPlayerAdjacent => ActionData.MainActorPlayerAdjacent;

	public int SecondaryActorAllyAdjacent => ActionData.SecondaryActorAllyAdjacent;

	public int SecondaryActorEnemyAdjacent => ActionData.SecondaryActorEnemyAdjacent;

	public int SecondaryActorObstacleAdjacent => ActionData.SecondaryActorObstacleAdjacent;

	public bool SecondaryActorWallAdjacent => ActionData.SecondaryActorWallAdjacent;

	public int SecondaryActorPlayerAdjacent => ActionData.SecondaryActorPlayerAdjacent;

	public string ControlledByClassID => ActionData.ControlledByClassID;

	public string ControlledByActorGuid => ActionData.ControlledByActorGuid;

	public string TurnActorGUID => ActionData.TurnActorGUID;

	public Point Position => ActionData.Position;

	public uint ResonanceTokens => ActionData.ResonanceTokens;

	public CPlayerActor.EPressure Pressure => ActionData.Pressure;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActionData", ActionData);
	}

	public CPlayerStatsAction(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ActionData")
				{
					ActionData = (CPlayerStatsActionData)info.GetValue("ActionData", typeof(CPlayerStatsActionData));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsAction entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsAction(CPlayerSatsBaseData baseData, CPlayerStatsActionData actionData)
		: base(baseData)
	{
		ActionData = actionData;
	}
}
