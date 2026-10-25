using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

[Serializable]
public class ActorState : ISerializable, IDumpState
{
	public string ClassID { get; set; }

	public string ActorGuid { get; set; }

	public TileIndex Location { get; set; }

	public int Health { get; set; }

	public int MinInitialMaxHealth { get; set; } = -1;

	public int MaxHealth { get; set; }

	public int Level { get; set; }

	public List<PositiveConditionPair> PositiveConditions { get; protected set; }

	public List<NegativeConditionPair> NegativeConditions { get; protected set; }

	public bool PlayedThisRound { get; protected set; }

	public bool PlayedAtLeastOnceThisRound { get; protected set; }

	public bool TurnStarted { get; protected set; }

	public bool ActorActionHasHappened { get; protected set; }

	public bool LastActionHasHappened { get; set; }

	public CActor.ECauseOfDeath CauseOfDeath { get; set; }

	public string KilledByActorGuid { get; protected set; }

	public bool IsRevealed { get; set; }

	public string StartingMapGuid { get; set; }

	public int AugmentSlots { get; protected set; }

	public CAIFocusOverrideDetails AIFocusOverride { get; protected set; }

	public bool PhasedOut { get; protected set; }

	public bool Deactivated { get; protected set; }

	public bool SkipTurnWhenDeactivated { get; protected set; }

	public int ChosenModelIndex { get; protected set; }

	public int RoundSpawned { get; protected set; }

	public bool HasBaneThisTurn { get; protected set; }

	public string OverrideCharacterModel { get; set; }

	public CActor.EType OverrideCharacterModelActorType { get; set; } = CActor.EType.Unknown;

	public string CustomLocKey { get; set; }

	public bool NoGoldDrop { get; set; }

	public List<CObjectProp> CarriedQuestProps { get; protected set; }

	public Dictionary<string, int> CharacterResources { get; set; }

	public bool IsDead => CauseOfDeath != CActor.ECauseOfDeath.StillAlive;

	public bool IsDeadForObjectives
	{
		get
		{
			if (CauseOfDeath != CActor.ECauseOfDeath.StillAlive)
			{
				return CauseOfDeath != CActor.ECauseOfDeath.ActorRemovedFromMap;
			}
			return false;
		}
	}

	public bool IsDowned { get; set; }

	public bool CanBeDowned { get; set; }

	public bool IsShadowMarked { get; set; }

	public CActor.TargetFromAnywhereFlags CanTargetFromAnywhereFlags { get; set; }

	public bool CanBeTargetedFromAnywhere { get; set; }

	public bool HasLootCardAdvantage { get; set; }

	public List<CAbility.EAbilityType> AbilityTypesPerformedThisTurn { get; set; }

	public List<string> ActorsAttackedThisRound { get; set; }

	public string WoundedBy { get; set; }

	public List<Point> AIMoveFocusWaypoints { get; set; }

	public int TilesMovedThisRound { get; set; }

	public int? InitiativeLockedDuringRound { get; set; }

	public CActor Actor
	{
		get
		{
			if (ActorGuid != null && ActorGuid != string.Empty)
			{
				return ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor s) => s.ActorGuid == ActorGuid);
			}
			return null;
		}
	}

	public int ActorID
	{
		get
		{
			if (this is PlayerState)
			{
				return 0;
			}
			if (this is EnemyState enemyState)
			{
				return enemyState.ID;
			}
			if (this is HeroSummonState heroSummonState)
			{
				return heroSummonState.ID;
			}
			throw new Exception("Invalid ActorState type. Unable to get Actor ID.");
		}
	}

	public override string ToString()
	{
		return "[" + MF.RefStr(this) + "] " + GetType().Name + "(" + ClassID + ", GUID:" + ActorGuid + ")";
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return GetType().Name + "(" + ClassID + ", GUID:" + ActorGuid + ")";
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ClassID", ClassID);
		info.AddValue("ActorGuid", ActorGuid);
		info.AddValue("Location", Location);
		info.AddValue("Health", Health);
		info.AddValue("RoundSpawned", RoundSpawned);
		info.AddValue("MinInitialMaxHealth", MinInitialMaxHealth);
		info.AddValue("MaxHealth", MaxHealth);
		info.AddValue("Level", Level);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("PlayedThisRound", PlayedThisRound);
		info.AddValue("PlayedAtLeastOnceThisRound", PlayedAtLeastOnceThisRound);
		info.AddValue("TurnStarted", TurnStarted);
		info.AddValue("ActorActionHasHappened", ActorActionHasHappened);
		info.AddValue("LastActionHasHappened", LastActionHasHappened);
		info.AddValue("CauseOfDeath", CauseOfDeath);
		info.AddValue("KilledByActorGuid", KilledByActorGuid);
		info.AddValue("IsRevealed", IsRevealed);
		info.AddValue("StartingMapGuid", StartingMapGuid);
		info.AddValue("AugmentSlots", AugmentSlots);
		info.AddValue("AIFocusOverride", AIFocusOverride);
		info.AddValue("PhasedOut", PhasedOut);
		info.AddValue("Deactivated", Deactivated);
		info.AddValue("SkipTurnWhenDeactivated", SkipTurnWhenDeactivated);
		info.AddValue("ChosenModelIndex", ChosenModelIndex);
		info.AddValue("CarriedQuestProps", CarriedQuestProps);
		info.AddValue("CharacterResources", CharacterResources);
		info.AddValue("CanBeDowned", CanBeDowned);
		info.AddValue("IsDowned", IsDowned);
		info.AddValue("HasLootCardAdvantage", HasLootCardAdvantage);
		info.AddValue("IsShadowMarked", IsShadowMarked);
		info.AddValue("CanTargetFromAnywhereFlags", CanTargetFromAnywhereFlags);
		info.AddValue("CanBeTargetedFromAnywhere", CanBeTargetedFromAnywhere);
		info.AddValue("AbilityTypesPerformedThisTurn", AbilityTypesPerformedThisTurn);
		info.AddValue("ActorsAttackedThisRound", ActorsAttackedThisRound);
		info.AddValue("WoundedBy", WoundedBy);
		info.AddValue("TilesMovedThisRound", TilesMovedThisRound);
		info.AddValue("AIMoveFocusWaypoints", AIMoveFocusWaypoints);
		info.AddValue("InitiativeLockedDuringRound", InitiativeLockedDuringRound);
		info.AddValue("OverrideCharacterModel", OverrideCharacterModel);
		info.AddValue("OverrideCharacterModelActorType", OverrideCharacterModelActorType);
		info.AddValue("CustomLocKey", CustomLocKey);
		info.AddValue("NoGoldDrop", NoGoldDrop);
		info.AddValue("HasBaneThisTurn", HasBaneThisTurn);
	}

	public ActorState(SerializationInfo info, StreamingContext context)
	{
		bool flag = false;
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "InitiativeLockedDuringRound":
					InitiativeLockedDuringRound = (int?)info.GetValue("InitiativeLockedDuringRound", typeof(int?));
					break;
				case "ClassID":
					ClassID = info.GetString("ClassID");
					if (ClassID == "Elementalist3DemonSummonAltarID")
					{
						ClassID = "DemonSummonAltarID";
					}
					break;
				case "ActorGuid":
					ActorGuid = info.GetString("ActorGuid");
					break;
				case "Location":
					Location = (TileIndex)info.GetValue("Location", typeof(TileIndex));
					break;
				case "Health":
					Health = info.GetInt32("Health");
					break;
				case "RoundSpawned":
					RoundSpawned = info.GetInt32("RoundSpawned");
					break;
				case "MaxHealth":
					MaxHealth = info.GetInt32("MaxHealth");
					break;
				case "MinInitialMaxHealth":
					MinInitialMaxHealth = info.GetInt32("MinInitialMaxHealth");
					break;
				case "Level":
					Level = info.GetInt32("Level");
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "PlayedThisRound":
					PlayedThisRound = info.GetBoolean("PlayedThisRound");
					break;
				case "PlayedAtLeastOnceThisRound":
					PlayedAtLeastOnceThisRound = info.GetBoolean("PlayedAtLeastOnceThisRound");
					break;
				case "TurnStarted":
					TurnStarted = info.GetBoolean("TurnStarted");
					break;
				case "ActorActionHasHappened":
					ActorActionHasHappened = info.GetBoolean("ActorActionHasHappened");
					break;
				case "LastActionHasHappened":
					LastActionHasHappened = info.GetBoolean("LastActionHasHappened");
					break;
				case "CauseOfDeath":
					CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
					break;
				case "KilledByActorGuid":
					KilledByActorGuid = info.GetString("KilledByActorGuid");
					break;
				case "IsRevealed":
					IsRevealed = info.GetBoolean("IsRevealed");
					break;
				case "StartingMapGuid":
					StartingMapGuid = info.GetString("StartingMapGuid");
					break;
				case "AugmentSlots":
					AugmentSlots = info.GetInt32("AugmentSlots");
					break;
				case "ClassName":
				{
					string text = info.GetString("ClassName").Replace(" ", string.Empty);
					ClassID = text + "ID";
					if (ClassID == "Elementalist3DemonSummonAltarID")
					{
						ClassID = "DemonSummonAltarID";
					}
					break;
				}
				case "AIFocusOverride":
					AIFocusOverride = (CAIFocusOverrideDetails)info.GetValue("AIFocusOverride", typeof(CAIFocusOverrideDetails));
					break;
				case "PhasedOut":
					PhasedOut = info.GetBoolean("PhasedOut");
					break;
				case "Deactivated":
					Deactivated = info.GetBoolean("Deactivated");
					break;
				case "SkipTurnWhenDeactivated":
					SkipTurnWhenDeactivated = info.GetBoolean("SkipTurnWhenDeactivated");
					break;
				case "ChosenModelIndex":
					ChosenModelIndex = info.GetInt32("ChosenModelIndex");
					break;
				case "OverrideCharacterModel":
					OverrideCharacterModel = info.GetString("OverrideCharacterModel");
					break;
				case "OverrideCharacterModelActorType":
					OverrideCharacterModelActorType = (CActor.EType)info.GetValue("OverrideCharacterModelActorType", typeof(CActor.EType));
					break;
				case "CustomLocKey":
					CustomLocKey = info.GetString("CustomLocKey");
					break;
				case "CarriedQuestProps":
					CarriedQuestProps = (List<CObjectProp>)info.GetValue("CarriedQuestProps", typeof(List<CObjectProp>));
					break;
				case "CharacterResources":
					CharacterResources = (Dictionary<string, int>)info.GetValue("CharacterResources", typeof(Dictionary<string, int>));
					break;
				case "CanBeDowned":
					CanBeDowned = info.GetBoolean("CanBeDowned");
					break;
				case "IsDowned":
					IsDowned = info.GetBoolean("IsDowned");
					break;
				case "HasLootCardAdvantage":
					HasLootCardAdvantage = info.GetBoolean("HasLootCardAdvantage");
					break;
				case "IsShadowMarked":
					IsShadowMarked = info.GetBoolean("IsShadowMarked");
					break;
				case "CanTargetFromAnywhereFlags":
					CanTargetFromAnywhereFlags = (CActor.TargetFromAnywhereFlags)info.GetValue("CanTargetFromAnywhereFlags", typeof(CActor.TargetFromAnywhereFlags));
					break;
				case "CanBeTargetedFromAnywhere":
					CanBeTargetedFromAnywhere = info.GetBoolean("CanBeTargetedFromAnywhere");
					break;
				case "AbilityTypesPerformedThisTurn":
					AbilityTypesPerformedThisTurn = (List<CAbility.EAbilityType>)info.GetValue("AbilityTypesPerformedThisTurn", typeof(List<CAbility.EAbilityType>));
					break;
				case "ActorsAttackedThisRound":
					ActorsAttackedThisRound = (List<string>)info.GetValue("ActorsAttackedThisRound", typeof(List<string>));
					break;
				case "WoundedBy":
					WoundedBy = info.GetString("WoundedBy");
					break;
				case "TilesMovedThisRound":
					TilesMovedThisRound = info.GetInt32("TilesMovedThisRound");
					break;
				case "AIMoveFocusWaypoints":
					AIMoveFocusWaypoints = (List<Point>)info.GetValue("AIMoveFocusWaypoints", typeof(List<Point>));
					break;
				case "NoGoldDrop":
					NoGoldDrop = info.GetBoolean("NoGoldDrop");
					break;
				case "HasBaneThisTurn":
					HasBaneThisTurn = info.GetBoolean("HasBaneThisTurn");
					break;
				case "IsDead":
					flag = info.GetBoolean("IsDead");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize ActorState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (AugmentSlots == 0)
		{
			AugmentSlots = 1;
		}
		if (flag)
		{
			CauseOfDeath = CActor.ECauseOfDeath.Undetermined;
		}
		else if (CauseOfDeath == CActor.ECauseOfDeath.None)
		{
			CauseOfDeath = CActor.ECauseOfDeath.StillAlive;
		}
	}

	public virtual void SaveFields(CActor actor)
	{
		RoundSpawned = actor.RoundSpawned;
		Location = new TileIndex(actor.ArrayIndex.X, actor.ArrayIndex.Y);
		Health = actor.Health;
		MinInitialMaxHealth = actor.MinInitialMaxHealth;
		MaxHealth = actor.OriginalMaxHealth;
		Level = actor.Level;
		PositiveConditions = actor.Tokens.CheckPositiveTokens.ToList();
		NegativeConditions = actor.Tokens.CheckNegativeTokens.ToList();
		PlayedThisRound = actor.PlayedThisRound;
		PlayedAtLeastOnceThisRound = actor.PlayedAtLeastOnceThisRound;
		TurnStarted = actor.TurnStarted;
		ActorActionHasHappened = actor.ActorActionHasHappened;
		LastActionHasHappened = actor.LastActionHasHappened;
		CauseOfDeath = actor.CauseOfDeath;
		KilledByActorGuid = actor.KilledByActorGuid;
		AugmentSlots = Math.Max(1, actor.AugmentSlots);
		PhasedOut = actor.PhasedOut;
		Deactivated = actor.Deactivated;
		SkipTurnWhenDeactivated = actor.SkipTurnWhenDeactivated;
		IsDowned = actor.IsDowned;
		CanBeDowned = actor.CanBeDowned;
		IsShadowMarked = actor.IsShadowMarked;
		CanTargetFromAnywhereFlags = actor.GetCanTargetFromAnywhere();
		CanBeTargetedFromAnywhere = actor.CanBeTargetedFromAnywhere();
		AbilityTypesPerformedThisTurn = actor.AbilityTypesPerformedThisTurn?.ToList();
		ActorsAttackedThisRound = GameState.SaveActorsAsGUIDs(actor.ActorsAttackedThisRound);
		TilesMovedThisRound = actor.TilesMovedThisRound;
		WoundedBy = GameState.SaveActorAsGUID(actor.WoundedBy);
		AIMoveFocusWaypoints = actor.AIMoveFocusWaypoints?.Select((CTile x) => new Point(x)).ToList();
		InitiativeLockedDuringRound = actor.InitiativeLockedDuringRound;
		OverrideCharacterModel = actor.OverrideCharacterModel;
		OverrideCharacterModelActorType = (actor.OverrideCharacterModelActorType.HasValue ? actor.OverrideCharacterModelActorType.Value : CActor.EType.Unknown);
		CustomLocKey = actor.CustomLocKey;
		NoGoldDrop = actor.NoGoldDrop;
		HasBaneThisTurn = actor.HasBaneThisTurn;
		CarriedQuestProps = actor.CarriedQuestItems.ToList();
		CharacterResources.Clear();
		foreach (CCharacterResource characterResource in actor.CharacterResources)
		{
			CharacterResources.Add(characterResource.ID, characterResource.Amount);
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		onDeserialized(context);
	}

	protected virtual void onDeserialized(StreamingContext context)
	{
		if (CarriedQuestProps == null)
		{
			CarriedQuestProps = new List<CObjectProp>();
		}
		if (PositiveConditions == null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
		}
		if (NegativeConditions == null)
		{
			NegativeConditions = new List<NegativeConditionPair>();
		}
		if (CharacterResources == null)
		{
			CharacterResources = new Dictionary<string, int>();
		}
	}

	public ActorState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex tileIndex, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, int augmentSlots)
	{
		ClassID = classID;
		ChosenModelIndex = chosenModelIndex;
		ActorGuid = actorGuid;
		StartingMapGuid = mapGuid;
		Location = tileIndex;
		Health = health;
		MaxHealth = maxHealth;
		Level = level;
		PositiveConditions = posCons;
		NegativeConditions = negCons;
		PlayedThisRound = (PlayedAtLeastOnceThisRound = playedThisRound);
		CauseOfDeath = causeOfDeath;
		IsRevealed = false;
		AugmentSlots = Math.Max(1, augmentSlots);
		PhasedOut = false;
		Deactivated = false;
		SkipTurnWhenDeactivated = false;
		CarriedQuestProps = new List<CObjectProp>();
		CharacterResources = new Dictionary<string, int>();
		HasLootCardAdvantage = false;
		IsShadowMarked = false;
		CanTargetFromAnywhereFlags = CActor.TargetFromAnywhereFlags.None;
		CanBeTargetedFromAnywhere = false;
		CustomLocKey = string.Empty;
		NoGoldDrop = false;
		HasBaneThisTurn = false;
	}

	public void ResetCauseOfDeath()
	{
		CauseOfDeath = CActor.ECauseOfDeath.StillAlive;
	}

	public void SetAIFocusOverride(CAIFocusOverrideDetails aiFocusOverride)
	{
		AIFocusOverride = aiFocusOverride;
	}

	public void SetLootCardAdvantage(bool val)
	{
		HasLootCardAdvantage = val;
	}

	public void ResetAIFocusOverride()
	{
		if (AIFocusOverride != null)
		{
			AIFocusOverride = new CAIFocusOverrideDetails();
		}
	}

	public static List<Tuple<int, string>> Compare(ActorState state1, ActorState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		string text = "Actor State";
		string text2 = null;
		string text3 = null;
		if (state1 is PlayerState)
		{
			text = "Player State";
		}
		else if (state1 is EnemyState enemyState && state2 is EnemyState enemyState2)
		{
			text = "Enemy State";
			text2 = enemyState.ID.ToString();
			text3 = enemyState2.ID.ToString();
		}
		else if (state1 is HeroSummonState heroSummonState && state2 is HeroSummonState heroSummonState2)
		{
			text = "Hero Summon State";
			text2 = heroSummonState.ID.ToString();
			text3 = heroSummonState2.ID.ToString();
		}
		try
		{
			if (state1.ActorGuid != state2.ActorGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 400, "Enemy State ActorGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (state1.ClassID != state2.ClassID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 401, text + " ClassID does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (!TileIndex.Compare(state1.Location, state2.Location))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 402, text + " Location does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (state1.Health != state2.Health)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 403, text + " Health does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"Health",
						state1.Health.ToString(),
						state2.Health.ToString()
					}
				});
			}
			if (state1.MaxHealth != state2.MaxHealth)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 404, text + " MaxHealth does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"MaxHealth",
						state1.MaxHealth.ToString(),
						state2.MaxHealth.ToString()
					}
				});
			}
			if (state1.Level != state2.Level)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 405, text + " Level does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"Level",
						state1.Level.ToString(),
						state2.Level.ToString()
					}
				});
			}
			if (state1.PlayedThisRound != state2.PlayedThisRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 406, text + " PlayedThisRound does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PlayedThisRound",
						state1.PlayedThisRound.ToString(),
						state2.PlayedThisRound.ToString()
					}
				});
			}
			if (state1.CauseOfDeath != state2.CauseOfDeath)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 407, text + " CauseOfDeath does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"CauseOfDeath",
						state1.CauseOfDeath.ToString(),
						state2.CauseOfDeath.ToString()
					}
				});
			}
			if (state1.IsRevealed != state2.IsRevealed)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 408, text + " IsRevealed does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsRevealed",
						state1.IsRevealed.ToString(),
						state2.IsRevealed.ToString()
					}
				});
			}
			if (state1.StartingMapGuid != state2.StartingMapGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 409, text + " StartingMapGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3] { "StartingMapGuid", state1.StartingMapGuid, state2.StartingMapGuid }
				});
			}
			if (state1.AugmentSlots != state2.AugmentSlots)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 410, text + " AugmentSlots does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"AugmentSlots",
						state1.AugmentSlots.ToString(),
						state2.AugmentSlots.ToString()
					}
				});
			}
			if (state1.IsDowned != state2.IsDowned)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 410, text + " IsDowned does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsDowned",
						state1.IsDowned.ToString(),
						state2.IsDowned.ToString()
					}
				});
			}
			if (state1.CanBeDowned != state2.CanBeDowned)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 410, text + " CanBeDowned does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"CanBeDowned",
						state1.CanBeDowned.ToString(),
						state2.CanBeDowned.ToString()
					}
				});
			}
			if (state1.HasLootCardAdvantage != state2.HasLootCardAdvantage)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 410, text + " HasLootCardAdvantage does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsDowned",
						state1.IsDowned.ToString(),
						state2.IsDowned.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(state1.PositiveConditions, state2.PositiveConditions))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 411, text + " PositiveConditions null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PositiveConditions",
						(state1.PositiveConditions == null) ? "is null" : "is not null",
						(state2.PositiveConditions == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.PositiveConditions.Count != state2.PositiveConditions.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 412, text + " total PositiveConditions Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Enemy ID",
							(text2 != null) ? text2 : "NA",
							(text3 != null) ? text3 : "NA"
						},
						new string[3]
						{
							"Location",
							state1.Location.ToString(),
							state2.Location.ToString()
						},
						new string[3]
						{
							"PositiveConditions Count",
							state1.PositiveConditions.Count.ToString(),
							state2.PositiveConditions.Count.ToString()
						}
					});
					break;
				}
				foreach (PositiveConditionPair posCon1 in state1.PositiveConditions)
				{
					List<PositiveConditionPair> list2 = state1.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon1)).ToList();
					List<PositiveConditionPair> list3 = state2.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon1)).ToList();
					if (list2.Count != list3.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 413, text + " PositiveCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"PositiveCondition",
								posCon1.ToString(),
								(list3.Count > 0) ? list3.First().ToString() : "None"
							},
							new string[3]
							{
								"PositiveCondition Count",
								list2.Count.ToString(),
								list3.Count.ToString()
							}
						});
					}
				}
				foreach (PositiveConditionPair posCon2 in state2.PositiveConditions)
				{
					List<PositiveConditionPair> list4 = state1.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon2)).ToList();
					List<PositiveConditionPair> list5 = state2.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon2)).ToList();
					if (list4.Count != list5.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 413, text + " PositiveCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"PositiveCondition",
								(list4.Count > 0) ? list4.First().ToString() : "None",
								posCon2.ToString()
							},
							new string[3]
							{
								"PositiveCondition Count",
								list4.Count.ToString(),
								list5.Count.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.NegativeConditions, state2.NegativeConditions))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 414, text + " NegativeConditions null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"NegativeConditions",
						(state1.NegativeConditions == null) ? "is null" : "is not null",
						(state2.NegativeConditions == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.NegativeConditions.Count != state2.NegativeConditions.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 415, text + " total NegativeConditions Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Enemy ID",
							(text2 != null) ? text2 : "NA",
							(text3 != null) ? text3 : "NA"
						},
						new string[3]
						{
							"Location",
							state1.Location.ToString(),
							state2.Location.ToString()
						},
						new string[3]
						{
							"NegativeConditions Count",
							state1.NegativeConditions.Count.ToString(),
							state2.NegativeConditions.Count.ToString()
						}
					});
					break;
				}
				foreach (NegativeConditionPair negCon1 in state1.NegativeConditions)
				{
					List<NegativeConditionPair> list6 = state1.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon1)).ToList();
					List<NegativeConditionPair> list7 = state2.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon1)).ToList();
					if (list6.Count != list7.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 416, text + " NegativeCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"NegativeCondition",
								negCon1.ToString(),
								(list7.Count > 0) ? list7.First().ToString() : "None"
							},
							new string[3]
							{
								"NegativeCondition Count",
								list6.Count.ToString(),
								list7.Count.ToString()
							}
						});
					}
				}
				foreach (NegativeConditionPair negCon2 in state2.NegativeConditions)
				{
					List<NegativeConditionPair> list8 = state1.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon2)).ToList();
					List<NegativeConditionPair> list9 = state2.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon2)).ToList();
					if (list8.Count != list9.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 416, text + " NegativeCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"NegativeCondition",
								(list8.Count > 0) ? list8.First().ToString() : "None",
								negCon2.ToString()
							},
							new string[3]
							{
								"NegativeCondition Count",
								list8.Count.ToString(),
								list9.Count.ToString()
							}
						});
					}
				}
				break;
			}
			StateShared.ENullStatus eNullStatus = StateShared.CheckNullsMatch(state1.AIFocusOverride, state2.AIFocusOverride);
			if (eNullStatus == StateShared.ENullStatus.Mismatch)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 417, text + " AIFocusOverride null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"AIFocusOverride",
						(state1.AIFocusOverride == null) ? "is null" : "is not null",
						(state2.AIFocusOverride == null) ? "is null" : "is not null"
					}
				});
			}
			if (state1.PlayedAtLeastOnceThisRound != state2.PlayedAtLeastOnceThisRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 418, text + " PlayedAtLeastOnceThisRound does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PlayedAtLeastOnceThisRound",
						state1.PlayedThisRound.ToString(),
						state2.PlayedThisRound.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(499, "Exception during " + text + " compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
