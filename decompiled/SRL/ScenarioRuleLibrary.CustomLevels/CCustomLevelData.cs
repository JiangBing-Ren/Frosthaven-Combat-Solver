using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CCustomLevelData : ISerializable
{
	public static readonly ELevelPartyChoiceType[] LevelPartyChoiceTypes = ((ELevelPartyChoiceType[])Enum.GetValues(typeof(ELevelPartyChoiceType))).Where((ELevelPartyChoiceType t) => t != ELevelPartyChoiceType.ChooseOwnParty && t != ELevelPartyChoiceType.ChooseOwnPartyRestricted).ToArray();

	public string Name;

	public string YMLFile;

	public DLCRegistry.EDLCKey DLCUsed;

	public ScenarioState ScenarioState;

	public List<CLevelMessage> LevelMessages { get; private set; }

	public List<CLevelEvent> LevelEvents { get; set; }

	public bool EnemyLevelsScaleToPartyLevel { get; set; }

	public bool SetEnemyHealthToMaxOnPlay { get; set; }

	public bool EnemyMaxHealthBasedOnPartyLevel { get; set; }

	public bool ShouldPreventUnspecifiedInteraction { get; set; }

	public bool UseRealtime { get; set; }

	public bool ShuffleAttackModsEnabledForPlayers { get; set; }

	public bool ShuffleAbilityDecksEnabledForMonsters { get; set; }

	public bool ShuffleAttackModsEnabledForMonsters { get; set; }

	public bool RandomiseOnLoad { get; set; }

	public ELevelPartyChoiceType PartySpawnType { get; set; }

	public int PartySizeLimit { get; set; }

	public List<string> AllowedPartyItems { get; set; }

	public List<string> AllowedPartyCharacterIDs { get; set; }

	public List<CAllowedAbilitiesPerCharacter> AllowedAbilitiesPerCharacterList { get; set; }

	public bool UsesFixedMercStartingRotation { get; set; }

	public List<TileIndex> FixedFacingDirectionIndices { get; set; }

	public List<TileIndex> StartingTileIndexes { get; set; }

	public bool SplitStartingTileIndexesByIslands { get; set; }

	public bool ExplicitlyDontSplitStartingTileIndexesByIslands { get; set; }

	public List<CApparanceOverrideDetails> ApparanceOverrideList { get; set; }

	public List<CStatBasedOnXOverrideDetails> StatBasedOnXOverrideList { get; set; }

	public List<string> MapIconMaterialNames { get; set; }

	public bool HasScriptedEvents
	{
		get
		{
			if ((LevelMessages == null || LevelMessages.Count <= 0) && (LevelEvents == null || LevelEvents.Count <= 0))
			{
				if (ScenarioState != null)
				{
					if (!ScenarioState.WinObjectives.Any((CObjective o) => o.ObjectiveType == EObjectiveType.CustomTrigger))
					{
						return ScenarioState.LoseObjectives.Any((CObjective o) => o.ObjectiveType == EObjectiveType.CustomTrigger);
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasApparanceOverrides => ApparanceOverrideList.Count > 0;

	public bool HasFixedPlayerFacingRotation
	{
		get
		{
			if (UsesFixedMercStartingRotation && FixedFacingDirectionIndices != null && FixedFacingDirectionIndices.Count > 1)
			{
				return !FixedFacingDirectionIndices.Any((TileIndex t) => t == null);
			}
			return false;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("YMLFile", YMLFile);
		info.AddValue("DLCUsed", DLCUsed);
		info.AddValue("ScenarioState", ScenarioState);
		info.AddValue("LevelMessages", LevelMessages);
		info.AddValue("LevelEvents", LevelEvents);
		info.AddValue("EnemyLevelsScaleToPartyLevel", EnemyLevelsScaleToPartyLevel);
		info.AddValue("SetEnemyHealthToMaxOnPlay", SetEnemyHealthToMaxOnPlay);
		info.AddValue("EnemyMaxHealthBasedOnPartyLevel", EnemyMaxHealthBasedOnPartyLevel);
		info.AddValue("ShouldPreventUnspecifiedInteraction", ShouldPreventUnspecifiedInteraction);
		info.AddValue("UseRealtime", UseRealtime);
		info.AddValue("ShuffleAttackModsEnabledForPlayers", ShuffleAttackModsEnabledForPlayers);
		info.AddValue("ShuffleAbilityDecksEnabledForMonsters", ShuffleAbilityDecksEnabledForMonsters);
		info.AddValue("ShuffleAttackModsEnabledForMonsters", ShuffleAttackModsEnabledForMonsters);
		info.AddValue("RandomiseOnLoad", RandomiseOnLoad);
		if (PartySpawnType == ELevelPartyChoiceType.ChooseOwnParty || PartySpawnType == ELevelPartyChoiceType.ChooseOwnPartyRestricted)
		{
			PartySpawnType = ELevelPartyChoiceType.LoadAdventureParty;
		}
		info.AddValue("PartySpawnType", PartySpawnType);
		info.AddValue("PartySizeLimit", PartySizeLimit);
		info.AddValue("AllowedPartyItems", AllowedPartyItems);
		info.AddValue("AllowedPartyCharacterIDs", AllowedPartyCharacterIDs);
		info.AddValue("AllowedAbilitiesPerCharacterList", AllowedAbilitiesPerCharacterList);
		info.AddValue("MapIconMaterialNames", MapIconMaterialNames);
		info.AddValue("UsesFixedMercStartingRotation", UsesFixedMercStartingRotation);
		info.AddValue("FixedFacingDirectionIndices", FixedFacingDirectionIndices);
		info.AddValue("StartingTileIndexes", StartingTileIndexes);
		info.AddValue("ApparanceOverrideList", ApparanceOverrideList);
		info.AddValue("StatBasedOnXOverrideList", StatBasedOnXOverrideList);
		info.AddValue("SplitStartingTileIndexesByIslands", SplitStartingTileIndexesByIslands);
		info.AddValue("ExplicitlyDontSplitStartingTileIndexesByIslands", ExplicitlyDontSplitStartingTileIndexesByIslands);
	}

	public CCustomLevelData(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "YMLFile":
					YMLFile = info.GetString("YMLFile");
					break;
				case "DLCUsed":
					DLCUsed = (DLCRegistry.EDLCKey)info.GetValue("DLCUsed", typeof(DLCRegistry.EDLCKey));
					break;
				case "ScenarioState":
					ScenarioState = (ScenarioState)info.GetValue("ScenarioState", typeof(ScenarioState));
					break;
				case "LevelMessages":
					LevelMessages = (List<CLevelMessage>)info.GetValue("LevelMessages", typeof(List<CLevelMessage>));
					break;
				case "LevelEvents":
					LevelEvents = (List<CLevelEvent>)info.GetValue("LevelEvents", typeof(List<CLevelEvent>));
					break;
				case "EnemyLevelsScaleToPartyLevel":
					EnemyLevelsScaleToPartyLevel = info.GetBoolean("EnemyLevelsScaleToPartyLevel");
					break;
				case "SetEnemyHealthToMaxOnPlay":
					SetEnemyHealthToMaxOnPlay = info.GetBoolean("SetEnemyHealthToMaxOnPlay");
					break;
				case "EnemyMaxHealthBasedOnPartyLevel":
					EnemyMaxHealthBasedOnPartyLevel = info.GetBoolean("EnemyMaxHealthBasedOnPartyLevel");
					break;
				case "ShouldPreventUnspecifiedInteraction":
					ShouldPreventUnspecifiedInteraction = info.GetBoolean("ShouldPreventUnspecifiedInteraction");
					break;
				case "UseRealtime":
					UseRealtime = info.GetBoolean("UseRealtime");
					break;
				case "ShuffleAttackModsEnabledForPlayers":
					ShuffleAttackModsEnabledForPlayers = info.GetBoolean("ShuffleAttackModsEnabledForPlayers");
					break;
				case "ShuffleAbilityDecksEnabledForMonsters":
					ShuffleAbilityDecksEnabledForMonsters = info.GetBoolean("ShuffleAbilityDecksEnabledForMonsters");
					break;
				case "ShuffleAttackModsEnabledForMonsters":
					ShuffleAttackModsEnabledForMonsters = info.GetBoolean("ShuffleAttackModsEnabledForMonsters");
					break;
				case "RandomiseOnLoad":
					RandomiseOnLoad = info.GetBoolean("RandomiseOnLoad");
					break;
				case "PartySpawnType":
				{
					ELevelPartyChoiceType eLevelPartyChoiceType = (ELevelPartyChoiceType)info.GetValue("PartySpawnType", typeof(ELevelPartyChoiceType));
					if (eLevelPartyChoiceType == ELevelPartyChoiceType.ChooseOwnParty || eLevelPartyChoiceType == ELevelPartyChoiceType.ChooseOwnPartyRestricted)
					{
						PartySpawnType = ELevelPartyChoiceType.LoadAdventureParty;
					}
					else
					{
						PartySpawnType = eLevelPartyChoiceType;
					}
					break;
				}
				case "PartySizeLimit":
					PartySizeLimit = info.GetInt32("PartySizeLimit");
					break;
				case "AllowedPartyItems":
					AllowedPartyItems = (List<string>)info.GetValue("AllowedPartyItems", typeof(List<string>));
					break;
				case "AllowedPartyCharacterIDs":
					AllowedPartyCharacterIDs = (List<string>)info.GetValue("AllowedPartyCharacterIDs", typeof(List<string>));
					break;
				case "AllowedAbilitiesPerCharacterList":
					AllowedAbilitiesPerCharacterList = (List<CAllowedAbilitiesPerCharacter>)info.GetValue("AllowedAbilitiesPerCharacterList", typeof(List<CAllowedAbilitiesPerCharacter>));
					break;
				case "MapIconMaterialNames":
					MapIconMaterialNames = (List<string>)info.GetValue("MapIconMaterialNames", typeof(List<string>));
					break;
				case "UsesFixedMercStartingRotation":
					UsesFixedMercStartingRotation = info.GetBoolean("UsesFixedMercStartingRotation");
					break;
				case "FixedFacingDirectionIndices":
					FixedFacingDirectionIndices = (List<TileIndex>)info.GetValue("FixedFacingDirectionIndices", typeof(List<TileIndex>));
					break;
				case "StartingTileIndexes":
					StartingTileIndexes = (List<TileIndex>)info.GetValue("StartingTileIndexes", typeof(List<TileIndex>));
					break;
				case "SplitStartingTileIndexesByIslands":
					SplitStartingTileIndexesByIslands = info.GetBoolean("SplitStartingTileIndexesByIslands");
					break;
				case "ExplicitlyDontSplitStartingTileIndexesByIslands":
					ExplicitlyDontSplitStartingTileIndexesByIslands = info.GetBoolean("ExplicitlyDontSplitStartingTileIndexesByIslands");
					break;
				case "EnemyHealthBasedOnPartyLevel":
					SetEnemyHealthToMaxOnPlay = info.GetBoolean("EnemyHealthBasedOnPartyLevel");
					break;
				case "AllowedPartyCharacters":
				{
					List<string> list = (List<string>)info.GetValue("AllowedPartyCharacters", typeof(List<string>));
					if (AllowedPartyCharacterIDs == null)
					{
						AllowedPartyCharacterIDs = new List<string>();
					}
					foreach (string item2 in list)
					{
						AllowedPartyCharacterIDs.Add(item2.Replace(" ", string.Empty) + "ID");
					}
					break;
				}
				case "ApparanceOverrideList":
					ApparanceOverrideList = (List<CApparanceOverrideDetails>)info.GetValue("ApparanceOverrideList", typeof(List<CApparanceOverrideDetails>));
					break;
				case "StatBasedOnXOverrideList":
					StatBasedOnXOverrideList = (List<CStatBasedOnXOverrideDetails>)info.GetValue("StatBasedOnXOverrideList", typeof(List<CStatBasedOnXOverrideDetails>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CCustomLevelData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (PartySizeLimit == 0)
		{
			PartySizeLimit = 4;
		}
	}

	public void UpdateScenarioStateID()
	{
		ScenarioState.ID = Name;
		ScenarioState.ScenarioFileName = YMLFile;
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (AllowedPartyItems == null)
		{
			AllowedPartyItems = new List<string>();
		}
		if (AllowedPartyCharacterIDs == null)
		{
			AllowedPartyCharacterIDs = new List<string>();
		}
		if (AllowedAbilitiesPerCharacterList == null)
		{
			AllowedAbilitiesPerCharacterList = new List<CAllowedAbilitiesPerCharacter>();
		}
		if (StartingTileIndexes == null)
		{
			StartingTileIndexes = new List<TileIndex>();
		}
		if (ApparanceOverrideList == null)
		{
			ApparanceOverrideList = new List<CApparanceOverrideDetails>();
		}
		if (StatBasedOnXOverrideList == null)
		{
			StatBasedOnXOverrideList = new List<CStatBasedOnXOverrideDetails>();
		}
	}

	public CCustomLevelData()
	{
		Name = string.Empty;
		SetDefaults();
	}

	public CCustomLevelData(string name, ScenarioState state)
	{
		Name = name;
		ScenarioState = state;
		SetDefaults();
	}

	public CCustomLevelData(string name, ScenarioDefinition scenarioDefinition)
	{
		Name = name;
		YMLFile = scenarioDefinition.FileName;
		ScenarioState = null;
		SetDefaults();
		PartySpawnType = ELevelPartyChoiceType.ChooseOwnParty;
	}

	public CCustomLevelData(CCustomLevelData data)
	{
		Name = data.Name;
		ScenarioState = data.ScenarioState;
		LevelMessages = data.LevelMessages;
		LevelEvents = data.LevelEvents;
		EnemyLevelsScaleToPartyLevel = data.EnemyLevelsScaleToPartyLevel;
		SetEnemyHealthToMaxOnPlay = data.SetEnemyHealthToMaxOnPlay;
		EnemyMaxHealthBasedOnPartyLevel = data.EnemyMaxHealthBasedOnPartyLevel;
		ShouldPreventUnspecifiedInteraction = data.ShouldPreventUnspecifiedInteraction;
		ShuffleAttackModsEnabledForPlayers = data.ShuffleAttackModsEnabledForPlayers;
		ShuffleAbilityDecksEnabledForMonsters = data.ShuffleAbilityDecksEnabledForMonsters;
		ShuffleAttackModsEnabledForMonsters = data.ShuffleAttackModsEnabledForMonsters;
		RandomiseOnLoad = data.RandomiseOnLoad;
		PartySpawnType = data.PartySpawnType;
		PartySizeLimit = data.PartySizeLimit;
		AllowedPartyItems = data.AllowedPartyItems;
		AllowedPartyCharacterIDs = data.AllowedPartyCharacterIDs;
		AllowedAbilitiesPerCharacterList = data.AllowedAbilitiesPerCharacterList;
		MapIconMaterialNames = data.MapIconMaterialNames;
		StartingTileIndexes = data.StartingTileIndexes;
		SplitStartingTileIndexesByIslands = data.SplitStartingTileIndexesByIslands;
		ExplicitlyDontSplitStartingTileIndexesByIslands = data.ExplicitlyDontSplitStartingTileIndexesByIslands;
		ApparanceOverrideList = data.ApparanceOverrideList;
		StatBasedOnXOverrideList = data.StatBasedOnXOverrideList;
	}

	private void SetDefaults()
	{
		LevelMessages = new List<CLevelMessage>();
		LevelEvents = new List<CLevelEvent>();
		PartySpawnType = ELevelPartyChoiceType.LoadAdventureParty;
		PartySizeLimit = 4;
		EnemyLevelsScaleToPartyLevel = true;
		SetEnemyHealthToMaxOnPlay = true;
		EnemyMaxHealthBasedOnPartyLevel = true;
		ShouldPreventUnspecifiedInteraction = false;
		ShuffleAttackModsEnabledForPlayers = true;
		ShuffleAbilityDecksEnabledForMonsters = true;
		ShuffleAttackModsEnabledForMonsters = true;
		RandomiseOnLoad = true;
		AllowedPartyItems = new List<string>();
		AllowedPartyCharacterIDs = new List<string>();
		AllowedAbilitiesPerCharacterList = new List<CAllowedAbilitiesPerCharacter>();
		MapIconMaterialNames = new List<string>();
		StartingTileIndexes = new List<TileIndex>();
		SplitStartingTileIndexesByIslands = false;
		ExplicitlyDontSplitStartingTileIndexesByIslands = false;
		ApparanceOverrideList = new List<CApparanceOverrideDetails>();
		StatBasedOnXOverrideList = new List<CStatBasedOnXOverrideDetails>();
	}

	public CApparanceOverrideDetails GetApparanceOverrideDetailsForGUID(string GUID)
	{
		return ApparanceOverrideList.FirstOrDefault((CApparanceOverrideDetails o) => o.GUID == GUID);
	}

	public List<CStatBasedOnXOverrideDetails> GetStatBasedOnXEntriedForClass(string classID)
	{
		return StatBasedOnXOverrideList.Where((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == classID).ToList();
	}
}
