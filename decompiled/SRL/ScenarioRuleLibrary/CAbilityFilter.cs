using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using FH.Tools;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class CAbilityFilter : ISerializable
{
	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterTargetType
	{
		None = 0,
		Self = 1,
		Enemy = 2,
		Ally = 4,
		Companion = 8,
		OwnSummon = 0x10,
		OwnBannerSummon = 0x20,
		All = -1
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterEnemy
	{
		None = 0,
		Normal = 1,
		Elite = 2,
		Boss = 4,
		Object = 8
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterActorType
	{
		None = 0,
		Player = 1,
		Enemy = 2,
		Summon = 4,
		Ally = 8,
		Enemy2 = 0x10,
		Neutral = 0x20
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterAllyType
	{
		None = 0,
		Ally = 2,
		OwnSummon = 4,
		All = -1
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterRevealedType
	{
		None = 0,
		IsRevealed = 1,
		NotRevealed = 2
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterTile
	{
		None = 0,
		EmptyHex = 1,
		Trap = 2,
		Obstacle = 4,
		Loot = 8,
		SingleHexObstacle = 0x10,
		ActiveSpawner = 0x20,
		DeactiveSpawner = 0x40,
		AdjacentToWall = 0x80,
		Actor = 0x100,
		ObjectActor = 0x200,
		Wall = 0x400,
		Shadow = 0x800,
		Terrain = 0x1000,
		ObstacleWithoutHealth = 0x2000,
		EmptyHexNoProps = 0x4000,
		Custom = 0x8000,
		FeaturelessHex = 0x10000,
		NotBlocked = 0x20000,
		EmptyHexIgnoreTraps = 0x40000
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum ELootType
	{
		None = 0,
		Gold = 1,
		Chest = 2,
		GoalChest = 4,
		QuestItem = 8,
		Resource = 0x10
	}

	[Serializable]
	[KronosEnumPolice(0)]
	[Flags]
	public enum EFilterFlags
	{
		None = 0,
		IsDoomed = 1,
		CarryingQuestItem = 2,
		LootedGoalChest = 4,
		TargetAttackedByCasterThisRound = 8,
		TargetTargetedByCasterPreviousAbility = 0x10,
		TargetTargetedByAllPreviousAbilitiesInAction = 0x20,
		CasterPreviousMovementEachHexCloserToTarget = 0x40
	}

	public static readonly EFilterTargetType[] FilterTargetTypes = (EFilterTargetType[])Enum.GetValues(typeof(EFilterTargetType));

	public static readonly EFilterEnemy[] FilterEnemyTypes = (EFilterEnemy[])Enum.GetValues(typeof(EFilterEnemy));

	public static readonly EFilterActorType[] FilterActorTypes = (EFilterActorType[])Enum.GetValues(typeof(EFilterActorType));

	public static readonly EFilterAllyType[] EFilterAllyTypes = (EFilterAllyType[])Enum.GetValues(typeof(EFilterAllyType));

	public static readonly EFilterRevealedType[] FilterRevealedTypes = (EFilterRevealedType[])Enum.GetValues(typeof(EFilterRevealedType));

	public static readonly EFilterTile[] FilterTiles = (EFilterTile[])Enum.GetValues(typeof(EFilterTile));

	public static readonly ELootType[] FilterLootTypes = (ELootType[])Enum.GetValues(typeof(ELootType));

	public static readonly EFilterFlags[] FilterFlagTypes = (EFilterFlags[])Enum.GetValues(typeof(EFilterFlags));

	private List<CActor> m_LastCheckedTargetAdjacentActors = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentEnemies = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentAllies = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentSummonsOfCaster = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentBannerSummonsOfCaster = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentAlliesOfTarget = new List<CActor>();

	private List<CActor> m_LastCheckedCasterAdjacentEnemies = new List<CActor>();

	private List<CActor> m_LastCheckedCasterAdjacentAllies = new List<CActor>();

	[JsonProperty]
	public EFilterTargetType FilterTargetType { get; set; }

	[JsonProperty]
	public EFilterEnemy FilterEnemy { get; set; }

	[JsonProperty]
	public EFilterActorType FilterActorType { get; set; }

	[JsonProperty]
	public EFilterAllyType FilterAllyType { get; set; }

	[JsonProperty]
	public List<(string, TValue)> FilterVars { get; set; }

	[JsonProperty]
	public List<string> FilterPlayerClasses { get; set; }

	[JsonProperty]
	public List<string> FilterEnemyClasses { get; set; }

	[JsonProperty]
	public List<string> FilterHeroSummonClasses { get; set; }

	[JsonProperty]
	public List<string> FilterObjectClasses { get; set; }

	[JsonProperty]
	public List<string> FilterSummonerClasses { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterHealth { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterHealthSelf { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentActors { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentEnemies { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentAllies { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentBannerSummonsOfCaster { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentSummonsOfCaster { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentAlliesOfTarget { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterAdjacentEnemies { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterAdjacentAllies { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentToWalls { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterAdjacentToWalls { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetInWater { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetAdjacentValidTiles { get; set; }

	[JsonProperty]
	public List<EFilterTile> FilterTargetAdjacentValidTilesFilterList { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterAdjacentValidTiles { get; set; }

	[JsonProperty]
	public List<EFilterTile> FilterCasterAdjacentValidTilesFilterList { get; set; }

	[JsonProperty]
	public List<CCondition.ENegativeCondition> FilterTargetHasNegativeConditions { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetNegativeConditionCount { get; set; }

	[JsonProperty]
	public List<CCondition.EPositiveCondition> FilterTargetHasPositiveConditions { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetPositiveConditionCount { get; set; }

	[JsonProperty]
	public List<CCondition.ENegativeCondition> FilterCasterHasNegativeConditions { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterNegativeConditionCount { get; set; }

	[JsonProperty]
	public List<CCondition.EPositiveCondition> FilterCasterHasPositiveConditions { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterPositiveConditionCount { get; set; }

	[JsonProperty]
	public List<CAbility.EAbilityType> FilterTargetHasImmunities { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetImmunitiesCount { get; set; }

	[JsonProperty]
	public List<CAbility.EAbilityType> FilterCasterHasImmunities { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCasterImmunitiesCount { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterCompareTargetHPToYourMissingHP { get; set; }

	[JsonProperty]
	public CEqualityFilter FilterTargetMissingHP { get; set; }

	[JsonProperty]
	public EFilterFlags FilterFlags { get; set; }

	[JsonProperty]
	public List<string> FilterTargetHasCharacterResource { get; set; }

	[JsonProperty]
	public bool Invert { get; set; }

	[JsonProperty]
	public int CheckAdjacentRange { get; set; }

	[JsonProperty]
	public bool UseTargetOriginalType { get; set; }

	[JsonProperty]
	public List<string> SpecificAbilityNames { get; set; }

	[JsonProperty]
	public bool CanTargetUntargetable { get; set; }

	public List<CActor> LastCheckedTargetAdjacentActors => m_LastCheckedTargetAdjacentActors;

	public List<CActor> LastCheckedTargetAdjacentEnemies => m_LastCheckedTargetAdjacentEnemies;

	public List<CActor> LastCheckedTargetAdjacentAllies => m_LastCheckedTargetAdjacentAllies;

	public List<CActor> LastCheckedTargetAdjacentAlliesOfTarget => m_LastCheckedTargetAdjacentAlliesOfTarget;

	public List<CActor> LastCheckedCasterAdjacentEnemies => m_LastCheckedCasterAdjacentEnemies;

	public List<CActor> LastCheckedCasterAdjacentAllies => m_LastCheckedCasterAdjacentAllies;

	public int LastCheckedTargetAdjacentWalls { get; private set; }

	public int LastCheckedCasterAdjacentWalls { get; private set; }

	public bool LastCheckedTargetInWater { get; private set; }

	public int LastCheckedTargetAdjacentTiles { get; private set; }

	public int LastCheckedCasterAdjacentValidTiles { get; private set; }

	public static CVarsContext CurrentVarsContext { get; set; }

	public static CAbilityFilter CreateDefaultFilter()
	{
		return new CAbilityFilter(EFilterTargetType.Self | EFilterTargetType.Enemy | EFilterTargetType.Ally | EFilterTargetType.Companion, EFilterEnemy.None, EFilterActorType.None, EFilterAllyType.All);
	}

	public static CAbilityFilter CreateDefaultFriendlyFilter()
	{
		return new CAbilityFilter(EFilterTargetType.Self | EFilterTargetType.Ally | EFilterTargetType.Companion | EFilterTargetType.OwnSummon, EFilterEnemy.None, EFilterActorType.None, EFilterAllyType.All);
	}

	public static CAbilityFilter CreateDefaultEnemyFilter()
	{
		return new CAbilityFilter(EFilterTargetType.Enemy, EFilterEnemy.None, EFilterActorType.None, EFilterAllyType.All);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("FilterTargetType", FilterTargetType);
		info.AddValue("FilterEnemy", FilterEnemy);
		info.AddValue("FilterActor", FilterActorType);
		info.AddValue("FilterAllyType", FilterAllyType);
		info.AddValue("FilterVars", FilterVars);
		info.AddValue("FilterPlayerClasses", FilterPlayerClasses);
		info.AddValue("FilterEnemyClasses", FilterEnemyClasses);
		info.AddValue("FilterHeroSummonClasses", FilterHeroSummonClasses);
		info.AddValue("FilterObjectClasses", FilterObjectClasses);
		info.AddValue("FilterTargetAdjacentActors", FilterTargetAdjacentActors);
		info.AddValue("FilterSummonerClasses", FilterSummonerClasses);
		info.AddValue("FilterHealth", FilterHealth);
		info.AddValue("FilterHealthSelf", FilterHealthSelf);
		info.AddValue("FilterTargetAdjacentEnemies", FilterTargetAdjacentEnemies);
		info.AddValue("FilterTargetAdjacentAllies", FilterTargetAdjacentAllies);
		info.AddValue("FilterTargetAdjacentAlliesOfTarget", FilterTargetAdjacentAlliesOfTarget);
		info.AddValue("FilterCasterAdjacentEnemies", FilterCasterAdjacentEnemies);
		info.AddValue("FilterCasterAdjacentAllies", FilterCasterAdjacentAllies);
		info.AddValue("FilterTargetAdjacentToWalls", FilterTargetAdjacentToWalls);
		info.AddValue("FilterCasterAdjacentToWalls", FilterCasterAdjacentToWalls);
		info.AddValue("FilterTargetInWater", FilterTargetInWater);
		info.AddValue("FilterTargetAdjacentValidTiles", FilterTargetAdjacentValidTiles);
		info.AddValue("FilterTargetAdjacentValidTilesFilterList", FilterTargetAdjacentValidTilesFilterList);
		info.AddValue("FilterCasterAdjacentValidTiles", FilterCasterAdjacentValidTiles);
		info.AddValue("FilterCasterAdjacentValidTilesFilterList", FilterCasterAdjacentValidTilesFilterList);
		info.AddValue("FilterTargetHasNegativeConditions", FilterTargetHasNegativeConditions);
		info.AddValue("FilterTargetNegativeConditionCount", FilterTargetNegativeConditionCount);
		info.AddValue("FilterTargetHasPositiveConditions", FilterTargetHasPositiveConditions);
		info.AddValue("FilterTargetPositiveConditionCount", FilterTargetPositiveConditionCount);
		info.AddValue("FilterCasterHasNegativeConditions", FilterCasterHasNegativeConditions);
		info.AddValue("FilterCasterNegativeConditionCount", FilterCasterNegativeConditionCount);
		info.AddValue("FilterCasterHasPositiveConditions", FilterCasterHasPositiveConditions);
		info.AddValue("FilterCasterPositiveConditionCount", FilterCasterPositiveConditionCount);
		info.AddValue("FilterTargetHasImmunities", FilterTargetHasImmunities);
		info.AddValue("FilterTargetImmunitiesCount", FilterTargetImmunitiesCount);
		info.AddValue("FilterCasterHasImmunities", FilterCasterHasImmunities);
		info.AddValue("FilterCasterImmunitiesCount", FilterCasterImmunitiesCount);
		info.AddValue("Invert", Invert);
		info.AddValue("CheckAdjacentRange", CheckAdjacentRange);
		info.AddValue("FilterCompareTargetHPToYourMissingHP", FilterCompareTargetHPToYourMissingHP);
		info.AddValue("FilterTargetMissingHP", FilterTargetMissingHP);
		info.AddValue("FilterFlags", FilterFlags);
		info.AddValue("FilterTargetHasCharacterResource", FilterTargetHasCharacterResource);
		info.AddValue("UseTargetOriginalType", UseTargetOriginalType);
		info.AddValue("SpecificAbilityNames", SpecificAbilityNames);
	}

	public CAbilityFilter(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item3 in info)
		{
			try
			{
				switch (item3.Name)
				{
				case "FilterTargetType":
					FilterTargetType = (EFilterTargetType)info.GetValue("FilterTargetType", typeof(EFilterTargetType));
					break;
				case "FilterEnemy":
					FilterEnemy = (EFilterEnemy)info.GetValue("FilterEnemy", typeof(EFilterEnemy));
					break;
				case "FilterActor":
					FilterActorType = (EFilterActorType)info.GetValue("FilterActor", typeof(EFilterActorType));
					break;
				case "FilterAllyType":
					FilterAllyType = (EFilterAllyType)info.GetValue("FilterAllyType", typeof(EFilterAllyType));
					break;
				case "FilterVars":
					FilterVars = (List<(string, TValue)>)info.GetValue("FilterVars", typeof(List<(string, TValue)>));
					break;
				case "FilterActorType":
				{
					int num = info.GetInt32("FilterActorType");
					if (num == 22)
					{
						num = 16;
					}
					FilterActorType = (EFilterActorType)num;
					break;
				}
				case "FilterPlayerClasses":
					FilterPlayerClasses = (List<string>)info.GetValue("FilterPlayerClasses", typeof(List<string>));
					break;
				case "FilterEnemyClasses":
					FilterEnemyClasses = (List<string>)info.GetValue("FilterEnemyClasses", typeof(List<string>));
					break;
				case "FilterHeroSummonClasses":
					FilterHeroSummonClasses = (List<string>)info.GetValue("FilterHeroSummonClasses", typeof(List<string>));
					break;
				case "FilterObjectClasses":
					FilterObjectClasses = (List<string>)info.GetValue("FilterObjectClasses", typeof(List<string>));
					break;
				case "FilterSummonerClasses":
					FilterSummonerClasses = (List<string>)info.GetValue("FilterSummonerClasses", typeof(List<string>));
					break;
				case "FilterHealth":
					FilterHealth = (CEqualityFilter)info.GetValue("FilterHealth", typeof(CEqualityFilter));
					break;
				case "FilterHealthSelf":
					FilterHealthSelf = (CEqualityFilter)info.GetValue("FilterHealthSelf", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentActors":
					FilterTargetAdjacentActors = (CEqualityFilter)info.GetValue("FilterTargetAdjacentActors", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentEnemies":
					FilterTargetAdjacentEnemies = (CEqualityFilter)info.GetValue("FilterTargetAdjacentEnemies", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentAllies":
					FilterTargetAdjacentAllies = (CEqualityFilter)info.GetValue("FilterTargetAdjacentAllies", typeof(CEqualityFilter));
					break;
				case "TargetAdjacentSummonsOfCaster":
					FilterTargetAdjacentSummonsOfCaster = (CEqualityFilter)info.GetValue("FilterTargetAdjacentSummonsOfCaster", typeof(CEqualityFilter));
					break;
				case "TargetAdjacentBannerSummonsOfCaster":
					FilterTargetAdjacentBannerSummonsOfCaster = (CEqualityFilter)info.GetValue("FilterTargetAdjacentBannerSummonsOfCaster", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentAlliesOfTarget":
					FilterTargetAdjacentAlliesOfTarget = (CEqualityFilter)info.GetValue("FilterTargetAdjacentAlliesOfTarget", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentEnemies":
					FilterCasterAdjacentEnemies = (CEqualityFilter)info.GetValue("FilterCasterAdjacentEnemies", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentAllies":
					FilterCasterAdjacentAllies = (CEqualityFilter)info.GetValue("FilterCasterAdjacentAllies", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentToWalls":
					FilterTargetAdjacentToWalls = (CEqualityFilter)info.GetValue("FilterTargetAdjacentToWalls", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentToWalls":
					FilterCasterAdjacentToWalls = (CEqualityFilter)info.GetValue("FilterCasterAdjacentToWalls", typeof(CEqualityFilter));
					break;
				case "FilterTargetInWater":
					FilterTargetInWater = (CEqualityFilter)info.GetValue("FilterTargetInWater", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentValidTiles":
					FilterTargetAdjacentValidTiles = (CEqualityFilter)info.GetValue("FilterTargetAdjacentValidTiles", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentValidTilesFilterList":
					FilterTargetAdjacentValidTilesFilterList = (List<EFilterTile>)info.GetValue("FilterTargetAdjacentValidTilesFilterList", typeof(List<EFilterTile>));
					break;
				case "FilterCasterAdjacentValidTiles":
					FilterCasterAdjacentValidTiles = (CEqualityFilter)info.GetValue("FilterCasterAdjacentValidTiles", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentValidTilesFilterList":
					FilterCasterAdjacentValidTilesFilterList = (List<EFilterTile>)info.GetValue("FilterCasterAdjacentValidTilesFilterList", typeof(List<EFilterTile>));
					break;
				case "FilterTargetHasNegativeConditions":
					FilterTargetHasNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("FilterTargetHasNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "FilterTargetNegativeConditionCount":
					FilterTargetNegativeConditionCount = (CEqualityFilter)info.GetValue("FilterTargetNegativeConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterTargetHasPositiveConditions":
					FilterTargetHasPositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("FilterTargetHasPositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "FilterTargetPositiveConditionCount":
					FilterTargetPositiveConditionCount = (CEqualityFilter)info.GetValue("FilterTargetPositiveConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterCasterHasNegativeConditions":
					FilterCasterHasNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("FilterCasterHasNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "FilterCasterNegativeConditionCount":
					FilterCasterNegativeConditionCount = (CEqualityFilter)info.GetValue("FilterCasterNegativeConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterCasterHasPositiveConditions":
					FilterCasterHasPositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("FilterCasterHasPositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "FilterCasterPositiveConditionCount":
					FilterCasterPositiveConditionCount = (CEqualityFilter)info.GetValue("FilterCasterPositiveConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterTargetHasImmunities":
					FilterTargetHasImmunities = (List<CAbility.EAbilityType>)info.GetValue("FilterTargetHasImmunities", typeof(List<CAbility.EAbilityType>));
					break;
				case "FilterTargetImmunitiesCount":
					FilterTargetImmunitiesCount = (CEqualityFilter)info.GetValue("FilterTargetImmunitiesCount", typeof(CEqualityFilter));
					break;
				case "FilterCasterHasImmunities":
					FilterCasterHasImmunities = (List<CAbility.EAbilityType>)info.GetValue("FilterCasterHasImmunities", typeof(List<CAbility.EAbilityType>));
					break;
				case "FilterCasterImmunitiesCount":
					FilterCasterImmunitiesCount = (CEqualityFilter)info.GetValue("FilterCasterImmunitiesCount", typeof(CEqualityFilter));
					break;
				case "Invert":
					Invert = info.GetBoolean("Invert");
					break;
				case "CheckAdjacentRange":
					CheckAdjacentRange = info.GetInt32("CheckAdjacentRange");
					break;
				case "FilterCompareTargetHPToYourMissingHP":
					FilterCompareTargetHPToYourMissingHP = (CEqualityFilter)info.GetValue("FilterCompareTargetHPToYourMissingHP", typeof(CEqualityFilter));
					break;
				case "FilterTargetMissingHP":
					FilterTargetMissingHP = (CEqualityFilter)info.GetValue("FilterTargetMissingHP", typeof(CEqualityFilter));
					break;
				case "FilterFlags":
					FilterFlags = (EFilterFlags)info.GetValue("FilterFlags", typeof(EFilterFlags));
					break;
				case "FilterTargetHasCharacterResource":
					FilterTargetHasCharacterResource = (List<string>)info.GetValue("FilterTargetHasCharacterResource", typeof(List<string>));
					break;
				case "UseTargetOriginalType":
					UseTargetOriginalType = info.GetBoolean("UseTargetOriginalType");
					break;
				case "SpecificAbilityNames":
					SpecificAbilityNames = (List<string>)info.GetValue("SpecificAbilityNames", typeof(List<string>));
					break;
				case "FilterTargetAdjacentValidTilesFilter":
				{
					EFilterTile item2 = (EFilterTile)info.GetValue("FilterTargetAdjacentValidTilesFilter", typeof(EFilterTile));
					FilterTargetAdjacentValidTilesFilterList = new List<EFilterTile> { item2 };
					break;
				}
				case "FilterCasterAdjacentValidTilesFilter":
				{
					EFilterTile item = (EFilterTile)info.GetValue("FilterCasterAdjacentValidTilesFilter", typeof(EFilterTile));
					FilterCasterAdjacentValidTilesFilterList = new List<EFilterTile> { item };
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CAbilityFilter entry " + item3.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[JsonConstructor]
	public CAbilityFilter(EFilterTargetType filterTargetType = EFilterTargetType.None, EFilterEnemy filterEnemy = EFilterEnemy.None, EFilterActorType filterActorType = EFilterActorType.None, EFilterAllyType filterAllyType = EFilterAllyType.Ally, List<(string, TValue)> filterVars = null, List<string> filterPlayerClasses = null, List<string> filterEnemyClasses = null, List<string> filterHeroSummonClasses = null, List<string> filterObjectClasses = null, List<string> filterSummonerClasses = null, CEqualityFilter filterHealth = null, CEqualityFilter filterHealthSelf = null, CEqualityFilter filterTargetAdjacentActors = null, CEqualityFilter filterTargetAdjacentEnemies = null, CEqualityFilter filterTargetAdjacentAllies = null, CEqualityFilter filterTargetAdjacentOwnSummons = null, CEqualityFilter filterTargetAdjacentOwnBannerSummons = null, CEqualityFilter filterTargetAdjacentAlliesOfTarget = null, CEqualityFilter filterCasterAdjacentEnemies = null, CEqualityFilter filterCasterAdjacentAllies = null, CEqualityFilter filterTargetAdjacentToWalls = null, CEqualityFilter filterCasterAdjacentToWalls = null, CEqualityFilter filterTargetInWater = null, CEqualityFilter filterTargetAdjacentTiles = null, List<EFilterTile> filterTargetAdjacentTilesFilter = null, CEqualityFilter filterCasterAdjacentTiles = null, List<EFilterTile> filterCasterAdjacentTilesFilter = null, List<CCondition.ENegativeCondition> filterTargetHasNegativeConditions = null, CEqualityFilter filterTargetNegativeConditionCount = null, List<CCondition.EPositiveCondition> filterTargetHasPositiveConditions = null, CEqualityFilter filterTargetPositiveConditionCount = null, List<CCondition.ENegativeCondition> filterCasterHasNegativeConditions = null, CEqualityFilter filterCasterNegativeConditionCount = null, List<CCondition.EPositiveCondition> filterCasterHasPositiveConditions = null, CEqualityFilter filterCasterPositiveConditionCount = null, List<CAbility.EAbilityType> filterTargetHasImmunities = null, CEqualityFilter filterTargetImmunitiesCount = null, List<CAbility.EAbilityType> filterCasterHasImmunities = null, CEqualityFilter filterCasterImmunitiesCount = null, CEqualityFilter filterCompareTargetHPToYourMissingHP = null, CEqualityFilter filterTargetMissingHP = null, EFilterFlags filterFlags = EFilterFlags.None, List<string> filterTargetHasCharacterResource = null, bool invert = false, bool useTargetOriginalType = false, int checkAdjacentRange = 1, List<string> specificAbilityNames = null, bool canTargetUntargetable = false)
	{
		FilterTargetType = filterTargetType;
		FilterEnemy = filterEnemy;
		FilterActorType = filterActorType;
		FilterAllyType = filterAllyType;
		FilterVars = filterVars;
		FilterPlayerClasses = filterPlayerClasses;
		FilterEnemyClasses = filterEnemyClasses;
		FilterHeroSummonClasses = filterHeroSummonClasses;
		FilterObjectClasses = filterObjectClasses;
		FilterSummonerClasses = filterSummonerClasses;
		FilterHealth = filterHealth;
		FilterHealthSelf = filterHealthSelf;
		FilterTargetAdjacentActors = filterTargetAdjacentActors;
		FilterTargetAdjacentEnemies = filterTargetAdjacentEnemies;
		FilterTargetAdjacentAllies = filterTargetAdjacentAllies;
		FilterTargetAdjacentSummonsOfCaster = filterTargetAdjacentOwnSummons;
		FilterTargetAdjacentBannerSummonsOfCaster = filterTargetAdjacentOwnBannerSummons;
		FilterTargetAdjacentAlliesOfTarget = filterTargetAdjacentAlliesOfTarget;
		FilterCasterAdjacentEnemies = filterCasterAdjacentEnemies;
		FilterCasterAdjacentAllies = filterCasterAdjacentAllies;
		FilterTargetAdjacentToWalls = filterTargetAdjacentToWalls;
		FilterCasterAdjacentToWalls = filterCasterAdjacentToWalls;
		FilterTargetInWater = filterTargetInWater;
		FilterTargetAdjacentValidTiles = filterTargetAdjacentTiles;
		FilterTargetAdjacentValidTilesFilterList = filterTargetAdjacentTilesFilter;
		FilterCasterAdjacentValidTiles = filterCasterAdjacentTiles;
		FilterCasterAdjacentValidTilesFilterList = filterCasterAdjacentTilesFilter;
		FilterTargetHasNegativeConditions = filterTargetHasNegativeConditions;
		FilterTargetNegativeConditionCount = filterTargetNegativeConditionCount;
		FilterTargetHasPositiveConditions = filterTargetHasPositiveConditions;
		FilterTargetPositiveConditionCount = filterTargetPositiveConditionCount;
		FilterCasterHasNegativeConditions = filterCasterHasNegativeConditions;
		FilterCasterNegativeConditionCount = filterCasterNegativeConditionCount;
		FilterCasterHasPositiveConditions = filterCasterHasPositiveConditions;
		FilterCasterPositiveConditionCount = filterCasterPositiveConditionCount;
		FilterTargetHasImmunities = filterTargetHasImmunities;
		FilterTargetImmunitiesCount = filterTargetImmunitiesCount;
		FilterCasterHasImmunities = filterCasterHasImmunities;
		FilterCasterImmunitiesCount = filterCasterImmunitiesCount;
		FilterCompareTargetHPToYourMissingHP = filterCompareTargetHPToYourMissingHP;
		FilterTargetMissingHP = filterTargetMissingHP;
		FilterFlags = filterFlags;
		FilterTargetHasCharacterResource = filterTargetHasCharacterResource;
		Invert = invert;
		UseTargetOriginalType = useTargetOriginalType;
		CheckAdjacentRange = checkAdjacentRange;
		SpecificAbilityNames = specificAbilityNames;
		CanTargetUntargetable = canTargetUntargetable;
	}

	public CAbilityFilter Copy()
	{
		EFilterTargetType filterTargetType = FilterTargetType;
		EFilterEnemy filterEnemy = FilterEnemy;
		EFilterActorType filterActorType = FilterActorType;
		EFilterAllyType filterAllyType = FilterAllyType;
		List<(string, TValue)> filterVars = FilterVars;
		List<string> filterPlayerClasses = FilterPlayerClasses?.ToList();
		List<string> filterEnemyClasses = FilterEnemyClasses?.ToList();
		List<string> filterHeroSummonClasses = FilterHeroSummonClasses?.ToList();
		List<string> filterObjectClasses = FilterObjectClasses?.ToList();
		List<string> filterSummonerClasses = FilterSummonerClasses?.ToList();
		CEqualityFilter filterHealth = FilterHealth?.Copy();
		CEqualityFilter filterHealthSelf = FilterHealthSelf?.Copy();
		CEqualityFilter filterTargetAdjacentActors = FilterTargetAdjacentActors?.Copy();
		CEqualityFilter filterTargetAdjacentEnemies = FilterTargetAdjacentEnemies?.Copy();
		CEqualityFilter filterTargetAdjacentAllies = FilterTargetAdjacentAllies?.Copy();
		CEqualityFilter filterTargetAdjacentOwnSummons = FilterTargetAdjacentSummonsOfCaster?.Copy();
		CEqualityFilter filterTargetAdjacentOwnBannerSummons = FilterTargetAdjacentBannerSummonsOfCaster?.Copy();
		CEqualityFilter filterTargetAdjacentAlliesOfTarget = FilterTargetAdjacentAlliesOfTarget?.Copy();
		CEqualityFilter filterCasterAdjacentEnemies = FilterCasterAdjacentEnemies?.Copy();
		CEqualityFilter filterCasterAdjacentAllies = FilterCasterAdjacentAllies?.Copy();
		CEqualityFilter filterTargetAdjacentToWalls = FilterTargetAdjacentToWalls?.Copy();
		CEqualityFilter filterCasterAdjacentToWalls = FilterCasterAdjacentToWalls?.Copy();
		CEqualityFilter filterTargetInWater = FilterTargetInWater?.Copy();
		CEqualityFilter filterTargetAdjacentTiles = FilterTargetAdjacentValidTiles?.Copy();
		List<EFilterTile> filterTargetAdjacentValidTilesFilterList = FilterTargetAdjacentValidTilesFilterList;
		CEqualityFilter filterCasterAdjacentTiles = FilterCasterAdjacentValidTiles?.Copy();
		List<EFilterTile> filterCasterAdjacentValidTilesFilterList = FilterCasterAdjacentValidTilesFilterList;
		List<CCondition.ENegativeCondition> filterTargetHasNegativeConditions = FilterTargetHasNegativeConditions?.ToList();
		CEqualityFilter filterTargetNegativeConditionCount = FilterTargetNegativeConditionCount?.Copy();
		List<CCondition.EPositiveCondition> filterTargetHasPositiveConditions = FilterTargetHasPositiveConditions?.ToList();
		CEqualityFilter filterTargetPositiveConditionCount = FilterTargetPositiveConditionCount?.Copy();
		List<CCondition.ENegativeCondition> filterCasterHasNegativeConditions = FilterCasterHasNegativeConditions?.ToList();
		CEqualityFilter filterCasterNegativeConditionCount = FilterCasterNegativeConditionCount?.Copy();
		List<CCondition.EPositiveCondition> filterCasterHasPositiveConditions = FilterCasterHasPositiveConditions?.ToList();
		CEqualityFilter filterCasterPositiveConditionCount = FilterCasterPositiveConditionCount?.Copy();
		List<CAbility.EAbilityType> filterTargetHasImmunities = FilterTargetHasImmunities?.ToList();
		CEqualityFilter filterTargetImmunitiesCount = FilterTargetImmunitiesCount?.Copy();
		List<CAbility.EAbilityType> filterCasterHasImmunities = FilterCasterHasImmunities?.ToList();
		CEqualityFilter filterCasterImmunitiesCount = FilterCasterImmunitiesCount?.Copy();
		CEqualityFilter filterCompareTargetHPToYourMissingHP = FilterCompareTargetHPToYourMissingHP?.Copy();
		CEqualityFilter filterTargetMissingHP = FilterTargetMissingHP?.Copy();
		EFilterFlags filterFlags = FilterFlags;
		List<string> filterTargetHasCharacterResource = (FilterTargetHasCharacterResource = FilterTargetHasCharacterResource?.ToList());
		return new CAbilityFilter(filterTargetType, filterEnemy, filterActorType, filterAllyType, filterVars, filterPlayerClasses, filterEnemyClasses, filterHeroSummonClasses, filterObjectClasses, filterSummonerClasses, filterHealth, filterHealthSelf, filterTargetAdjacentActors, filterTargetAdjacentEnemies, filterTargetAdjacentAllies, filterTargetAdjacentOwnSummons, filterTargetAdjacentOwnBannerSummons, filterTargetAdjacentAlliesOfTarget, filterCasterAdjacentEnemies, filterCasterAdjacentAllies, filterTargetAdjacentToWalls, filterCasterAdjacentToWalls, filterTargetInWater, filterTargetAdjacentTiles, filterTargetAdjacentValidTilesFilterList, filterCasterAdjacentTiles, filterCasterAdjacentValidTilesFilterList, filterTargetHasNegativeConditions, filterTargetNegativeConditionCount, filterTargetHasPositiveConditions, filterTargetPositiveConditionCount, filterCasterHasNegativeConditions, filterCasterNegativeConditionCount, filterCasterHasPositiveConditions, filterCasterPositiveConditionCount, filterTargetHasImmunities, filterTargetImmunitiesCount, filterCasterHasImmunities, filterCasterImmunitiesCount, filterCompareTargetHPToYourMissingHP, filterTargetMissingHP, filterFlags, filterTargetHasCharacterResource, Invert, UseTargetOriginalType, CheckAdjacentRange, SpecificAbilityNames, CanTargetUntargetable);
	}

	public bool Equals(CAbilityFilter compare)
	{
		if (compare == null)
		{
			return false;
		}
		bool flag = FilterTargetType == compare.FilterTargetType;
		bool flag2 = FilterEnemy == compare.FilterEnemy;
		bool flag3 = FilterActorType == compare.FilterActorType;
		bool flag4 = ((FilterPlayerClasses != null && compare.FilterPlayerClasses != null) ? FilterPlayerClasses.SequenceEqual(compare.FilterPlayerClasses) : StateShared.CheckNullsMatchBoolean(FilterPlayerClasses, compare.FilterPlayerClasses));
		bool flag5 = ((FilterEnemyClasses != null && compare.FilterEnemyClasses != null) ? FilterEnemyClasses.SequenceEqual(compare.FilterEnemyClasses) : StateShared.CheckNullsMatchBoolean(FilterEnemyClasses, compare.FilterEnemyClasses));
		bool flag6 = ((FilterHeroSummonClasses != null && compare.FilterHeroSummonClasses != null) ? FilterHeroSummonClasses.SequenceEqual(compare.FilterHeroSummonClasses) : StateShared.CheckNullsMatchBoolean(FilterHeroSummonClasses, compare.FilterHeroSummonClasses));
		bool flag7 = ((FilterObjectClasses != null && compare.FilterObjectClasses != null) ? FilterObjectClasses.SequenceEqual(compare.FilterObjectClasses) : StateShared.CheckNullsMatchBoolean(FilterObjectClasses, compare.FilterObjectClasses));
		bool flag8 = ((FilterSummonerClasses != null && compare.FilterSummonerClasses != null) ? FilterSummonerClasses.SequenceEqual(compare.FilterSummonerClasses) : StateShared.CheckNullsMatchBoolean(FilterSummonerClasses, compare.FilterSummonerClasses));
		bool flag9 = ((FilterHealth != null) ? FilterHealth.Equals(compare.FilterHealth) : (compare.FilterHealth == null));
		bool flag10 = ((FilterHealthSelf != null) ? FilterHealthSelf.Equals(compare.FilterHealthSelf) : (compare.FilterHealthSelf == null));
		bool flag11 = ((FilterTargetAdjacentActors != null) ? FilterTargetAdjacentActors.Equals(compare.FilterTargetAdjacentActors) : (compare.FilterTargetAdjacentActors == null));
		bool flag12 = ((FilterTargetAdjacentEnemies != null) ? FilterTargetAdjacentEnemies.Equals(compare.FilterTargetAdjacentEnemies) : (compare.FilterTargetAdjacentEnemies == null));
		bool flag13 = ((FilterTargetAdjacentAllies != null) ? FilterTargetAdjacentAllies.Equals(compare.FilterTargetAdjacentAllies) : (compare.FilterTargetAdjacentAllies == null));
		bool flag14 = ((FilterTargetAdjacentSummonsOfCaster != null) ? FilterTargetAdjacentSummonsOfCaster.Equals(compare.FilterTargetAdjacentSummonsOfCaster) : (compare.FilterTargetAdjacentSummonsOfCaster == null));
		if (FilterTargetAdjacentBannerSummonsOfCaster == null)
		{
			_ = compare.FilterTargetAdjacentBannerSummonsOfCaster;
		}
		else
		{
			FilterTargetAdjacentBannerSummonsOfCaster.Equals(compare.FilterTargetAdjacentBannerSummonsOfCaster);
		}
		bool flag15 = ((FilterTargetAdjacentAlliesOfTarget != null) ? FilterTargetAdjacentAlliesOfTarget.Equals(compare.FilterTargetAdjacentAlliesOfTarget) : (compare.FilterTargetAdjacentAlliesOfTarget == null));
		bool flag16 = ((FilterCasterAdjacentEnemies != null) ? FilterCasterAdjacentEnemies.Equals(compare.FilterCasterAdjacentEnemies) : (compare.FilterCasterAdjacentEnemies == null));
		bool flag17 = ((FilterCasterAdjacentAllies != null) ? FilterCasterAdjacentAllies.Equals(compare.FilterCasterAdjacentAllies) : (compare.FilterCasterAdjacentAllies == null));
		bool flag18 = ((FilterTargetAdjacentToWalls != null) ? FilterTargetAdjacentToWalls.Equals(compare.FilterTargetAdjacentToWalls) : (compare.FilterTargetAdjacentToWalls == null));
		bool flag19 = ((FilterCasterAdjacentToWalls != null) ? FilterCasterAdjacentToWalls.Equals(compare.FilterCasterAdjacentToWalls) : (compare.FilterCasterAdjacentToWalls == null));
		bool flag20 = ((FilterTargetInWater != null) ? FilterTargetInWater.Equals(compare.FilterTargetInWater) : (compare.FilterTargetInWater == null));
		bool flag21 = ((FilterTargetAdjacentValidTiles != null) ? FilterTargetAdjacentValidTiles.Equals(compare.FilterTargetAdjacentValidTiles) : (compare.FilterTargetAdjacentValidTiles == null));
		bool flag22 = FilterTargetAdjacentValidTilesFilterList == compare.FilterTargetAdjacentValidTilesFilterList;
		bool flag23 = ((FilterCasterAdjacentValidTiles != null) ? FilterCasterAdjacentValidTiles.Equals(compare.FilterCasterAdjacentValidTiles) : (compare.FilterCasterAdjacentValidTiles == null));
		bool flag24 = FilterCasterAdjacentValidTilesFilterList == compare.FilterCasterAdjacentValidTilesFilterList;
		bool flag25 = ((FilterTargetHasNegativeConditions != null && compare.FilterTargetHasNegativeConditions != null) ? FilterTargetHasNegativeConditions.SequenceEqual(compare.FilterTargetHasNegativeConditions) : StateShared.CheckNullsMatchBoolean(FilterTargetHasNegativeConditions, compare.FilterTargetHasNegativeConditions));
		bool flag26 = ((FilterTargetNegativeConditionCount != null) ? FilterTargetNegativeConditionCount.Equals(compare.FilterTargetNegativeConditionCount) : (compare.FilterTargetNegativeConditionCount == null));
		bool flag27 = ((FilterTargetHasPositiveConditions != null && compare.FilterTargetHasPositiveConditions != null) ? FilterTargetHasPositiveConditions.SequenceEqual(compare.FilterTargetHasPositiveConditions) : StateShared.CheckNullsMatchBoolean(FilterTargetHasPositiveConditions, compare.FilterTargetHasPositiveConditions));
		bool flag28 = ((FilterTargetPositiveConditionCount != null) ? FilterTargetPositiveConditionCount.Equals(compare.FilterTargetPositiveConditionCount) : (compare.FilterTargetPositiveConditionCount == null));
		bool flag29 = ((FilterCasterHasNegativeConditions != null && compare.FilterCasterHasNegativeConditions != null) ? FilterCasterHasNegativeConditions.SequenceEqual(compare.FilterCasterHasNegativeConditions) : StateShared.CheckNullsMatchBoolean(FilterCasterHasNegativeConditions, compare.FilterCasterHasNegativeConditions));
		bool flag30 = ((FilterCasterNegativeConditionCount != null) ? FilterCasterNegativeConditionCount.Equals(compare.FilterCasterNegativeConditionCount) : (compare.FilterCasterNegativeConditionCount == null));
		bool flag31 = ((FilterCasterHasPositiveConditions != null && compare.FilterCasterHasPositiveConditions != null) ? FilterCasterHasPositiveConditions.SequenceEqual(compare.FilterCasterHasPositiveConditions) : StateShared.CheckNullsMatchBoolean(FilterCasterHasPositiveConditions, compare.FilterCasterHasPositiveConditions));
		bool flag32 = ((FilterCasterPositiveConditionCount != null) ? FilterCasterPositiveConditionCount.Equals(compare.FilterCasterPositiveConditionCount) : (compare.FilterCasterPositiveConditionCount == null));
		bool flag33 = ((FilterTargetHasImmunities != null && compare.FilterTargetHasImmunities != null) ? FilterTargetHasImmunities.SequenceEqual(compare.FilterTargetHasImmunities) : StateShared.CheckNullsMatchBoolean(FilterTargetHasImmunities, compare.FilterTargetHasImmunities));
		bool flag34 = ((FilterTargetImmunitiesCount != null) ? FilterTargetImmunitiesCount.Equals(compare.FilterTargetImmunitiesCount) : (compare.FilterTargetImmunitiesCount == null));
		bool flag35 = ((FilterCasterHasImmunities != null && compare.FilterCasterHasImmunities != null) ? FilterCasterHasImmunities.SequenceEqual(compare.FilterCasterHasImmunities) : StateShared.CheckNullsMatchBoolean(FilterCasterHasImmunities, compare.FilterCasterHasImmunities));
		bool flag36 = ((FilterCasterImmunitiesCount != null) ? FilterCasterImmunitiesCount.Equals(compare.FilterCasterImmunitiesCount) : (compare.FilterCasterImmunitiesCount == null));
		bool flag37 = ((FilterCompareTargetHPToYourMissingHP != null) ? FilterCompareTargetHPToYourMissingHP.Equals(compare.FilterCompareTargetHPToYourMissingHP) : (compare.FilterCompareTargetHPToYourMissingHP == null));
		bool flag38 = ((FilterTargetMissingHP != null) ? FilterTargetMissingHP.Equals(compare.FilterTargetMissingHP) : (compare.FilterTargetMissingHP == null));
		bool flag39 = FilterFlags == compare.FilterFlags;
		bool flag40 = ((FilterTargetHasCharacterResource != null && compare.FilterTargetHasCharacterResource != null) ? FilterTargetHasCharacterResource.SequenceEqual(compare.FilterTargetHasCharacterResource) : StateShared.CheckNullsMatchBoolean(FilterTargetHasCharacterResource, compare.FilterTargetHasCharacterResource));
		bool flag41 = Invert == compare.Invert;
		bool flag42 = UseTargetOriginalType == compare.UseTargetOriginalType;
		bool flag43 = CheckAdjacentRange == compare.CheckAdjacentRange;
		bool flag44 = ((SpecificAbilityNames != null && compare.SpecificAbilityNames != null) ? SpecificAbilityNames.SequenceEqual(compare.SpecificAbilityNames) : StateShared.CheckNullsMatchBoolean(SpecificAbilityNames, compare.SpecificAbilityNames));
		bool flag45 = FilterVars == compare.FilterVars;
		bool flag46 = CanTargetUntargetable == compare.CanTargetUntargetable;
		return flag & flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8 & flag9 & flag10 & flag11 & flag12 & flag13 & flag14 & flag15 & flag16 & flag17 & flag18 & flag19 & flag20 & flag21 & flag22 & flag23 & flag24 & flag25 & flag26 & flag27 & flag28 & flag29 & flag30 & flag31 & flag32 & flag33 & flag34 & flag35 & flag36 & flag37 & flag38 & flag39 & flag40 & flag41 & flag42 & flag43 & flag44 & flag45 & flag46;
	}

	public bool IsValidTarget(CActor target, CActor self, bool isTargetedAbility, bool useTargetOriginalType = false, bool? canTargetInvisible = false, bool skipUntargetableCheck = false, CActiveBonus activeBonus = null, bool doesntWorkOnDeactivated = true)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			bool? flag = GameState.CanTargetInvisible(self, canTargetInvisible);
			if (target == null)
			{
				return false;
			}
			if (self == null)
			{
				Log.ErrorWithReport(" Unable to check valid target as self is null!");
				return false;
			}
			if (doesntWorkOnDeactivated && (target.Deactivated || target.PhasedOut))
			{
				return false;
			}
			if (!CanTargetUntargetable && target.Untargetable && target != self && !skipUntargetableCheck)
			{
				return false;
			}
			bool flag2 = CheckTargetType(target, self, useTargetOriginalType);
			bool flag3 = (target is CPlayerActor && target.OverrideType == CActor.EType.Enemy) || (target is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner.OverrideType == CActor.EType.Enemy) || CheckEnemy(target);
			bool flag4 = CheckActorType(target, useTargetOriginalType);
			bool flag5 = CheckHealthFilter(target);
			bool flag6 = CheckHealthSelfFilter(self);
			bool flag7 = CheckAdjacencyFilter(target, self, flag);
			bool flag8 = CheckWallAdjacencyFilter(target, self, flag);
			bool flag9 = CheckTileAdjacencyFilter(target, self, false);
			bool flag10 = CheckInWaterFilter(target, self, flag);
			bool flag11 = CheckConditionsFilters(target, self);
			bool flag12 = CheckConditionImmunitiesFilters(target, self);
			bool flag13 = CheckTargetHPtoMissingHPFilters(target, self);
			bool flag14 = CheckTargetMissingHPFilters(target);
			bool flag15 = CheckClass(target);
			bool flag16 = CheckSummoner(target);
			bool flag17 = CheckFlags(target, self);
			bool flag18 = CheckCharacterResources(target);
			bool flag19 = true;
			if (CurrentVarsContext != null && FilterVars != null && FilterVars.Count > 0)
			{
				flag19 = CheckVars(target, self, activeBonus);
			}
			if (flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8 & flag10 & flag9 & flag11 & flag12 & flag13 & flag14 & flag15 & flag16 & flag17 & flag18 & flag19)
			{
				if ((!CActor.AreActorsAllied(target.Type, self.Type, target, self) & isTargetedAbility) && target.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) && flag != true)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsValidTarget_ActorState(ActorState targetState, ActorState selfState, bool isTargetedAbility, bool useTargetOriginalType = false, bool? canTargetInvisible = false)
	{
		CActor cActor = ScenarioManager.FindActor(targetState.ActorGuid);
		CActor cActor2 = ScenarioManager.FindActor(selfState.ActorGuid);
		if (cActor != null && cActor2 != null)
		{
			return IsValidTarget(cActor, cActor2, isTargetedAbility, useTargetOriginalType, canTargetInvisible);
		}
		bool? flag = ((cActor2 != null) ? GameState.CanTargetInvisible(cActor2, canTargetInvisible) : new bool?(false));
		if (targetState == null)
		{
			return false;
		}
		if (selfState == null)
		{
			Log.ErrorWithReport(" Unable to check valid target state as self is null!");
			return false;
		}
		bool flag2 = CheckTargetType_ActorState(targetState, selfState);
		bool flag3 = CheckEnemy_ActorState(targetState);
		bool flag4 = CheckActorType_ActorState(targetState);
		bool flag5 = CheckHealthFilter_ActorState(targetState);
		bool flag6 = CheckHealthSelfFilter_ActorState(selfState);
		bool flag7 = CheckAdjacencyFilter_ActorState(targetState, selfState, flag);
		bool flag8 = CheckConditionsFilters_ActorState(targetState, selfState);
		bool flag9 = CheckConditionsImmunitiesFilters_ActorState(targetState, selfState);
		bool flag10 = CheckTargetHPtoMissingHPFilters_ActorState(targetState, selfState);
		bool flag11 = CheckTargetMissingHPFilters_ActorState(targetState);
		bool flag12 = CheckClass_ActorState(targetState);
		bool flag13 = CheckSummoner_ActorState(targetState);
		bool flag14 = CheckFlags_ActorState(targetState);
		if (flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8 & flag9 & flag10 & flag11 & flag12 & flag13 & flag14)
		{
			CActor.EType actorType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
			CActor.EType actorType2 = ScenarioManager.CurrentScenarioState.GetActorType(selfState);
			if ((!CActor.AreActorsAllied(actorType, actorType2, targetState.Actor, selfState.Actor) & isTargetedAbility) && targetState.PositiveConditions.Any((PositiveConditionPair c) => c.PositiveCondition == CCondition.EPositiveCondition.Invisible) && flag != true)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CheckTargetType(CActor target, CActor self, bool useTargetOriginalType = false)
	{
		CActor.EType eType = ((target.OverrideType != CActor.EType.Unknown) ? target.OverrideType : target.Type);
		eType = ((UseTargetOriginalType | useTargetOriginalType) ? target.OriginalType : eType);
		CActor.EType eType2 = ((self.OverrideType != CActor.EType.Unknown) ? self.OverrideType : self.Type);
		if (self is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner.OverrideType != CActor.EType.Unknown)
		{
			eType2 = cHeroSummonActor.Summoner.OverrideType;
		}
		if (target is CHeroSummonActor cHeroSummonActor2 && cHeroSummonActor2.Summoner.OverrideType != CActor.EType.Unknown)
		{
			eType = cHeroSummonActor2.Summoner.OverrideType;
		}
		if (!useTargetOriginalType && target.MindControlAbility != null && GameState.OverridenActionActorStack.Count > 0)
		{
			foreach (GameState.OverrideActorForActionStackData item in GameState.OverridenActionActorStack)
			{
				if (item.ActorBeingControlled == target)
				{
					eType = item.OriginalType;
					break;
				}
			}
		}
		bool flag = true;
		if (FilterTargetType == EFilterTargetType.None)
		{
			return true;
		}
		if (target == self)
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Self);
		}
		else if ((((eType2 == CActor.EType.Player || eType2 == CActor.EType.HeroSummon || eType2 == CActor.EType.Ally) && eType == CActor.EType.Neutral) || ((eType == CActor.EType.Player || eType == CActor.EType.HeroSummon || eType == CActor.EType.Ally) && eType2 == CActor.EType.Neutral)) && !FilterActorType.HasFlag(EFilterActorType.Neutral))
		{
			flag = false;
		}
		else if (CActor.AreActorsAllied(eType, eType2, target, self))
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Ally);
			if (self is CPlayerActor { CompanionSummon: not null } cPlayerActor)
			{
				if (cPlayerActor.CompanionSummon == target)
				{
					flag |= FilterTargetType.HasFlag(EFilterTargetType.Companion);
				}
			}
			else if (self is CPlayerActor && target is CHeroSummonActor cHeroSummonActor3 && cHeroSummonActor3.Summoner == self)
			{
				flag |= FilterTargetType.HasFlag(EFilterTargetType.OwnSummon);
				if (cHeroSummonActor3.IsBanner)
				{
					flag |= FilterTargetType.HasFlag(EFilterTargetType.OwnBannerSummon);
				}
			}
			else if (self is CHeroSummonActor cHeroSummonActor4 && target is CHeroSummonActor cHeroSummonActor5 && cHeroSummonActor5.Summoner == cHeroSummonActor4.Summoner)
			{
				flag |= FilterTargetType.HasFlag(EFilterTargetType.OwnSummon);
			}
			else if (self is CEnemyActor && target is CHeroSummonActor cHeroSummonActor6)
			{
				CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
				if (mirrorAbilityBehaviour != null && mirrorAbilityBehaviour.MirrorTarget == self && mirrorAbilityBehaviour.ActiveBonus.Actor == cHeroSummonActor6.Summoner)
				{
					flag |= FilterTargetType.HasFlag(EFilterTargetType.OwnSummon);
				}
			}
		}
		else
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Enemy);
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetType_ActorState(ActorState targetState, ActorState selfState)
	{
		CActor.EType actorType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
		CActor.EType actorType2 = ScenarioManager.CurrentScenarioState.GetActorType(selfState);
		bool flag = true;
		if (FilterTargetType == EFilterTargetType.None)
		{
			return true;
		}
		if (targetState.ActorGuid == selfState.ActorGuid)
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Self);
		}
		else if (CActor.AreActorsAllied(actorType, actorType2, targetState.Actor, selfState.Actor))
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Ally);
			if (selfState is PlayerState playerState && !string.IsNullOrEmpty(playerState.CompanionSummonGuid) && playerState.CompanionSummonGuid == targetState.ActorGuid)
			{
				flag |= FilterTargetType.HasFlag(EFilterTargetType.Companion);
			}
		}
		else
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Enemy);
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckEnemy(CActor target)
	{
		if (FilterEnemy == EFilterEnemy.None)
		{
			return true;
		}
		if (target.IsMonsterType)
		{
			CEnemyActor cEnemyActor = target as CEnemyActor;
			EFilterEnemy eFilterEnemy = EFilterEnemy.None;
			eFilterEnemy = ((cEnemyActor == null) ? EFilterEnemy.Normal : ((target is CObjectActor) ? EFilterEnemy.Object : (cEnemyActor.MonsterClass.Boss ? EFilterEnemy.Boss : ((cEnemyActor.MonsterClass.NonEliteVariant == null) ? EFilterEnemy.Normal : EFilterEnemy.Elite))));
			bool flag = FilterEnemy.HasFlag(eFilterEnemy);
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	private bool CheckEnemy_ActorState(ActorState targetState)
	{
		if (FilterEnemy == EFilterEnemy.None)
		{
			return true;
		}
		if (targetState is EnemyState enemyState)
		{
			EFilterEnemy eFilterEnemy = EFilterEnemy.None;
			if (enemyState is ObjectState)
			{
				eFilterEnemy = EFilterEnemy.Object;
			}
			else
			{
				eFilterEnemy = (enemyState.IsBoss ? EFilterEnemy.Boss : ((!enemyState.IsElite) ? EFilterEnemy.Normal : EFilterEnemy.Elite));
			}
			bool flag = FilterEnemy.HasFlag(eFilterEnemy);
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	private bool CheckActorType(CActor target, bool useTargetOriginalType = false)
	{
		if (FilterActorType == EFilterActorType.None)
		{
			return true;
		}
		CActor.EType eType = (useTargetOriginalType ? target.OriginalType : target.Type);
		if (!useTargetOriginalType && target.MindControlAbility != null && GameState.OverridenActionActorStack.Count > 0)
		{
			GameState.OverrideActorForActionStackData overrideActorForActionStackData = GameState.OverridenActionActorStack.Peek();
			if (overrideActorForActionStackData.PreviousActor == target)
			{
				eType = overrideActorForActionStackData.OriginalType;
			}
		}
		if (target.Type != target.OriginalType && GameState.CurrentOverridenActorForTurnData != null && GameState.CurrentOverridenActorForTurnData.OverridenActor == target)
		{
			eType = GameState.CurrentOverridenActorForTurnData.OriginalType;
		}
		EFilterActorType eFilterActorType = EFilterActorType.None;
		switch (eType)
		{
		case CActor.EType.Player:
			eFilterActorType = ((!(target is CPlayerActor { OverrideType: CActor.EType.Enemy })) ? EFilterActorType.Player : EFilterActorType.Enemy);
			break;
		case CActor.EType.Enemy:
			eFilterActorType = EFilterActorType.Enemy;
			break;
		case CActor.EType.HeroSummon:
			eFilterActorType = ((!(target is CHeroSummonActor cHeroSummonActor) || cHeroSummonActor.Summoner.OverrideType != CActor.EType.Enemy) ? EFilterActorType.Summon : EFilterActorType.Enemy);
			break;
		case CActor.EType.Ally:
			eFilterActorType = EFilterActorType.Ally;
			break;
		case CActor.EType.Enemy2:
			eFilterActorType = EFilterActorType.Enemy2;
			break;
		case CActor.EType.Neutral:
			eFilterActorType = EFilterActorType.Neutral;
			break;
		}
		bool flag = FilterActorType.HasFlag(eFilterActorType);
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckActorType_ActorState(ActorState targetState)
	{
		if (FilterActorType == EFilterActorType.None)
		{
			return true;
		}
		CActor.EType actorType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
		EFilterActorType eFilterActorType = EFilterActorType.None;
		switch (actorType)
		{
		case CActor.EType.Player:
			eFilterActorType = EFilterActorType.Player;
			break;
		case CActor.EType.Enemy:
			eFilterActorType = EFilterActorType.Enemy;
			break;
		case CActor.EType.HeroSummon:
			eFilterActorType = EFilterActorType.Summon;
			break;
		case CActor.EType.Ally:
			eFilterActorType = EFilterActorType.Ally;
			break;
		case CActor.EType.Enemy2:
			eFilterActorType = EFilterActorType.Enemy2;
			break;
		case CActor.EType.Neutral:
			eFilterActorType = EFilterActorType.Neutral;
			break;
		}
		bool flag = FilterActorType.HasFlag(eFilterActorType);
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckClass(CActor target)
	{
		if (FilterPlayerClasses == null && FilterObjectClasses == null && FilterEnemyClasses == null && FilterHeroSummonClasses == null)
		{
			return true;
		}
		bool flag = false;
		if (target is CPlayerActor && FilterPlayerClasses != null && FilterPlayerClasses.Count > 0)
		{
			flag = FilterPlayerClasses.Contains(target.Class.ID);
		}
		else if (target is CObjectActor && FilterObjectClasses != null && FilterObjectClasses.Count > 0)
		{
			flag = FilterObjectClasses.Contains(target.Class.ID);
		}
		else if (target is CEnemyActor && FilterEnemyClasses != null && FilterEnemyClasses.Count > 0)
		{
			flag = FilterEnemyClasses.Contains(target.Class.ID);
		}
		else
		{
			flag = ((target is CHeroSummonActor && FilterHeroSummonClasses != null && FilterHeroSummonClasses.Count > 0) ? FilterHeroSummonClasses.Contains(target.Class.ID) : (((FilterPlayerClasses == null || FilterPlayerClasses.Count <= 0) && (FilterEnemyClasses == null || FilterEnemyClasses.Count <= 0) && (FilterHeroSummonClasses == null || FilterHeroSummonClasses.Count <= 0) && (FilterObjectClasses == null || FilterObjectClasses.Count <= 0)) ? true : false));
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckClass_ActorState(ActorState targetState)
	{
		if (FilterPlayerClasses == null && FilterObjectClasses == null && FilterEnemyClasses == null && FilterHeroSummonClasses == null)
		{
			return true;
		}
		bool flag = false;
		if (targetState is PlayerState && FilterPlayerClasses != null && FilterPlayerClasses.Count > 0)
		{
			flag = FilterPlayerClasses.Contains(targetState.ClassID);
		}
		else if (targetState is ObjectState && FilterObjectClasses != null && FilterObjectClasses.Count > 0)
		{
			flag = FilterObjectClasses.Contains(targetState.ClassID);
		}
		else if (targetState is EnemyState && FilterEnemyClasses != null && FilterEnemyClasses.Count > 0)
		{
			flag = FilterEnemyClasses.Contains(targetState.ClassID);
		}
		else
		{
			flag = ((targetState is HeroSummonState && FilterHeroSummonClasses != null && FilterHeroSummonClasses.Count > 0) ? FilterHeroSummonClasses.Contains(targetState.ClassID) : (((FilterPlayerClasses == null || FilterPlayerClasses.Count <= 0) && (FilterEnemyClasses == null || FilterEnemyClasses.Count <= 0) && (FilterHeroSummonClasses == null || FilterHeroSummonClasses.Count <= 0) && (FilterObjectClasses == null || FilterObjectClasses.Count <= 0)) ? true : false));
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckSummoner(CActor target)
	{
		if (FilterSummonerClasses == null)
		{
			return true;
		}
		if (target is CHeroSummonActor cHeroSummonActor && FilterSummonerClasses.Contains(cHeroSummonActor.Summoner.Class.ID))
		{
			return true;
		}
		return false;
	}

	private bool CheckSummoner_ActorState(ActorState targetState)
	{
		if (FilterSummonerClasses == null)
		{
			return true;
		}
		HeroSummonState summonState = targetState as HeroSummonState;
		if (summonState != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == summonState.Summoner);
			if (actorState != null && FilterSummonerClasses.Contains(actorState.ClassID))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckCharacterResources(CActor target)
	{
		bool flag = true;
		if (FilterTargetHasCharacterResource != null && FilterTargetHasCharacterResource.Count > 0)
		{
			foreach (string item in FilterTargetHasCharacterResource)
			{
				if (!target.CharacterHasResource(item, 1))
				{
					flag = false;
					break;
				}
			}
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	public bool CheckVars(CActor target, CActor self, CActiveBonus activeBonus = null)
	{
		string failReason;
		return CheckVars(target, self, activeBonus, out failReason);
	}

	public bool CheckVars(CActor target, CActor self, CActiveBonus activeBonus, out string failReason)
	{
		failReason = null;
		if (FilterVars == null)
		{
			return true;
		}
		CVarsContext cVarsContext = CurrentVarsContext ?? CExpression.Globals.CurrentAbility?.VarsContext;
		if (cVarsContext == null)
		{
			Log.Error("CAbilityFilter.CurrentVarsContext is not set");
		}
		CVars cVars = new CVars();
		CVarsContext context = new CVarsContext(cVars, cVarsContext);
		cVars.Set("target", new TValue(target));
		cVars.Set("self", new TValue(self));
		cVars.Set("activeBonus", new TValue(activeBonus));
		foreach (var filterVar in FilterVars)
		{
			string item = filterVar.Item1;
			TValue item2 = filterVar.Item2;
			TValue value = item2.Calc(context);
			if (item.StartsWith("Validate", StringComparison.Ordinal) && !value.GetBool())
			{
				failReason = item;
				return false;
			}
			cVars.Set(item, value);
		}
		return true;
	}

	private bool CheckFlags(CActor target, CActor self)
	{
		if (FilterFlags == EFilterFlags.None)
		{
			return true;
		}
		bool flag = true;
		if (FilterFlags.HasFlag(EFilterFlags.IsDoomed))
		{
			flag = target.IsDoomed;
		}
		bool flag2 = true;
		if (FilterFlags.HasFlag(EFilterFlags.CarryingQuestItem))
		{
			flag2 = target.CarriedQuestItems.Count > 0;
		}
		bool flag3 = true;
		if (FilterFlags.HasFlag(EFilterFlags.LootedGoalChest))
		{
			List<CObjectProp> list = ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.IsLootable && p.ObjectType == ScenarioManager.ObjectImportType.GoalChest).ToList();
			if (list != null)
			{
				flag3 = list.Any((CObjectProp x) => x.ActorActivated == target.ActorGuid);
			}
		}
		bool flag4 = true;
		if (FilterFlags.HasFlag(EFilterFlags.TargetAttackedByCasterThisRound))
		{
			flag4 = self.ActorsAttackedThisRound.Contains(target);
		}
		bool flag5 = true;
		if (FilterFlags.HasFlag(EFilterFlags.TargetTargetedByCasterPreviousAbility))
		{
			flag5 = false;
			if (SpecificAbilityNames != null)
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					foreach (CPhaseAction.CPhaseAbility previousPhaseAbility in ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities)
					{
						if (SpecificAbilityNames.Contains(previousPhaseAbility.m_Ability.Name))
						{
							flag5 = previousPhaseAbility.m_Ability.ActorsTargeted.Contains(target);
						}
					}
				}
			}
			else if (self.LastAbilityPerformed != null && self.LastAbilityPerformed.ActorsTargeted != null)
			{
				flag5 = self.LastAbilityPerformed.ActorsTargeted.Contains(target);
			}
		}
		bool flag6 = true;
		if (FilterFlags.HasFlag(EFilterFlags.TargetTargetedByAllPreviousAbilitiesInAction) && PhaseManager.PhaseType == CPhase.PhaseType.Action)
		{
			foreach (CPhaseAction.CPhaseAbility previousPhaseAbility2 in ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities)
			{
				if (!previousPhaseAbility2.m_Ability.IsModifierAbility && (previousPhaseAbility2.m_Ability.ActorsTargeted == null || !previousPhaseAbility2.m_Ability.ActorsTargeted.Contains(target)))
				{
					flag6 = false;
					break;
				}
			}
		}
		bool flag7 = true;
		if (FilterFlags.HasFlag(EFilterFlags.CasterPreviousMovementEachHexCloserToTarget))
		{
			int num = int.MaxValue;
			foreach (Point allPlayerArrayIndexOnPathIncludingRepeat in CAbilityMove.AllPlayerArrayIndexOnPathIncludingRepeats)
			{
				int tileDistance = ScenarioManager.GetTileDistance(allPlayerArrayIndexOnPathIncludingRepeat.X, allPlayerArrayIndexOnPathIncludingRepeat.Y, target.ArrayIndex.X, target.ArrayIndex.Y);
				if (tileDistance >= num)
				{
					flag7 = false;
					break;
				}
				num = tileDistance;
			}
		}
		bool flag8 = flag & flag2 & flag3 & flag4 & flag5 & flag6 & flag7;
		if (!Invert)
		{
			return flag8;
		}
		return !flag8;
	}

	private bool CheckFlags_ActorState(ActorState targetState)
	{
		if (FilterFlags == EFilterFlags.None)
		{
			return true;
		}
		bool flag = false;
		if (FilterFlags.HasFlag(EFilterFlags.IsDoomed))
		{
			flag = false;
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthFilter(CActor target)
	{
		if (FilterHealth == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealth.ValueIsPercentage) ? FilterHealth.Compare(target.Health) : FilterHealth.Compare(target.Health, target.OriginalMaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthFilter_ActorState(ActorState targetState)
	{
		if (FilterHealth == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealth.ValueIsPercentage) ? FilterHealth.Compare(targetState.Health) : FilterHealth.Compare(targetState.Health, targetState.MaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthSelfFilter(CActor self)
	{
		if (FilterHealthSelf == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealthSelf.ValueIsPercentage) ? FilterHealthSelf.Compare(self.Health) : FilterHealthSelf.Compare(self.Health, self.OriginalMaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthSelfFilter_ActorState(ActorState selfState)
	{
		if (FilterHealthSelf == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealthSelf.ValueIsPercentage) ? FilterHealthSelf.Compare(selfState.Health) : FilterHealthSelf.Compare(selfState.Health, selfState.MaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckAdjacencyFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentActors == null && FilterTargetAdjacentAllies == null && FilterTargetAdjacentSummonsOfCaster == null && FilterTargetAdjacentBannerSummonsOfCaster == null && FilterTargetAdjacentAlliesOfTarget == null && FilterTargetAdjacentEnemies == null && FilterCasterAdjacentAllies == null && FilterCasterAdjacentEnemies == null)
		{
			return true;
		}
		List<CActor> actorsInRange = GameState.GetActorsInRange(target, target, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible);
		List<CActor> actorsInRange2 = GameState.GetActorsInRange(self, self, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible);
		if (self is CEnemyActor)
		{
			actorsInRange.RemoveAll((CActor x) => x is CObjectActor);
			actorsInRange2.RemoveAll((CActor x) => x is CObjectActor);
		}
		List<CActor> list = actorsInRange.Where((CActor x) => !CActor.AreActorsAllied(x.Type, self.Type, x, self)).ToList();
		List<CActor> list2 = actorsInRange.Where((CActor x) => CActor.AreActorsAllied(x.Type, self.Type, x, self) && x.ActorGuid != self.ActorGuid).ToList();
		List<CActor> list3 = actorsInRange.Where((CActor x) => CActor.AreActorsAllied(x.Type, target.Type, x, target) && x.ActorGuid != self.ActorGuid).ToList();
		List<CActor> list4 = actorsInRange2.Where((CActor x) => !CActor.AreActorsAllied(x.Type, self.Type, x, self)).ToList();
		List<CActor> list5 = actorsInRange2.Where((CActor x) => CActor.AreActorsAllied(x.Type, self.Type, x, self) && x.ActorGuid != self.ActorGuid).ToList();
		List<CActor> list6 = actorsInRange.Where((CActor x) => CActor.AreActorsAllied(x.Type, self.Type, x, self) && x.ActorGuid != self.ActorGuid && x is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == self).ToList();
		List<CActor> list7 = actorsInRange.Where((CActor x) => CActor.AreActorsAllied(x.Type, self.Type, x, self) && x.ActorGuid != self.ActorGuid && x is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == self && cHeroSummonActor.IsBanner).ToList();
		bool flag = FilterTargetAdjacentActors == null;
		if (FilterTargetAdjacentActors != null)
		{
			flag = FilterTargetAdjacentActors.Compare(actorsInRange.Count);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterTargetAdjacentEnemies == null;
		if (FilterTargetAdjacentEnemies != null)
		{
			flag2 = FilterTargetAdjacentEnemies.Compare(list.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		bool flag3 = FilterTargetAdjacentAllies == null;
		if (FilterTargetAdjacentAllies != null)
		{
			flag3 = FilterTargetAdjacentAllies.Compare(list2.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		bool flag4 = FilterTargetAdjacentSummonsOfCaster == null;
		if (FilterTargetAdjacentSummonsOfCaster != null)
		{
			flag4 = FilterTargetAdjacentSummonsOfCaster.Compare(list6.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		bool flag5 = FilterTargetAdjacentBannerSummonsOfCaster == null;
		if (FilterTargetAdjacentBannerSummonsOfCaster != null)
		{
			flag5 = FilterTargetAdjacentBannerSummonsOfCaster.Compare(list7.Count);
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		bool flag6 = FilterTargetAdjacentAlliesOfTarget == null;
		if (FilterTargetAdjacentAlliesOfTarget != null)
		{
			flag6 = FilterTargetAdjacentAlliesOfTarget.Compare(list3.Count);
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		bool flag7 = FilterCasterAdjacentEnemies == null;
		if (FilterCasterAdjacentEnemies != null)
		{
			flag7 = FilterCasterAdjacentEnemies.Compare(list4.Count);
			if (Invert)
			{
				flag7 = !flag7;
			}
		}
		bool flag8 = FilterCasterAdjacentAllies == null;
		if (FilterCasterAdjacentAllies != null)
		{
			flag8 = FilterCasterAdjacentAllies.Compare(list5.Count);
			if (Invert)
			{
				flag8 = !flag8;
			}
		}
		m_LastCheckedTargetAdjacentActors = actorsInRange;
		m_LastCheckedTargetAdjacentEnemies = list;
		m_LastCheckedTargetAdjacentAllies = list2;
		m_LastCheckedTargetAdjacentSummonsOfCaster = list6;
		m_LastCheckedTargetAdjacentBannerSummonsOfCaster = list7;
		m_LastCheckedTargetAdjacentAlliesOfTarget = list3;
		m_LastCheckedCasterAdjacentEnemies = list4;
		m_LastCheckedCasterAdjacentAllies = list5;
		return flag & flag2 & flag3 & flag6 & flag7 & flag8 & flag4;
	}

	private bool CheckAdjacencyFilter_ActorState(ActorState targetState, ActorState selfState, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentActors == null && FilterTargetAdjacentAllies == null && FilterTargetAdjacentSummonsOfCaster == null && FilterTargetAdjacentBannerSummonsOfCaster == null && FilterTargetAdjacentAlliesOfTarget == null && FilterTargetAdjacentEnemies == null && FilterCasterAdjacentAllies == null && FilterCasterAdjacentEnemies == null)
		{
			return true;
		}
		CActor cActor = ScenarioManager.FindActor(targetState.ActorGuid);
		CActor selfActor = ScenarioManager.FindActor(selfState.ActorGuid);
		List<CActor> list = ((cActor == null) ? new List<CActor>() : GameState.GetActorsInRange(cActor, cActor, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible));
		List<CActor> list2 = ((selfActor == null) ? new List<CActor>() : GameState.GetActorsInRange(selfActor, selfActor, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible));
		list.RemoveAll((CActor x) => x is CObjectActor);
		list2.RemoveAll((CActor x) => x is CObjectActor);
		CActor.EType targetType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
		CActor.EType selfType = ScenarioManager.CurrentScenarioState.GetActorType(selfState);
		List<CActor> list3 = list.Where((CActor x) => !CActor.AreActorsAllied(x.Type, selfType, x, selfState.Actor)).ToList();
		List<CActor> list4 = list.Where((CActor x) => CActor.AreActorsAllied(x.Type, selfType, x, selfState.Actor) && x.ActorGuid != selfState.ActorGuid).ToList();
		List<CActor> list5 = list.Where((CActor x) => CActor.AreActorsAllied(x.Type, targetType, x, targetState.Actor) && x.ActorGuid != selfState.ActorGuid).ToList();
		List<CActor> list6 = list2.Where((CActor x) => !CActor.AreActorsAllied(x.Type, selfType, x, selfState.Actor)).ToList();
		List<CActor> list7 = list2.Where((CActor x) => CActor.AreActorsAllied(x.Type, selfType, x, selfState.Actor) && x.ActorGuid != selfState.ActorGuid).ToList();
		List<CActor> list8 = list.Where((CActor x) => CActor.AreActorsAllied(x.Type, selfType, x, selfState.Actor) && x.ActorGuid != selfState.ActorGuid && x is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == selfActor).ToList();
		List<CActor> list9 = list.Where((CActor x) => CActor.AreActorsAllied(x.Type, selfType, x, selfState.Actor) && x.ActorGuid != selfState.ActorGuid && x is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == selfActor && cHeroSummonActor.IsBanner).ToList();
		bool flag = FilterTargetAdjacentActors == null;
		if (FilterTargetAdjacentActors != null)
		{
			flag = FilterTargetAdjacentActors.Compare(list.Count);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterTargetAdjacentEnemies == null;
		if (FilterTargetAdjacentEnemies != null)
		{
			flag2 = FilterTargetAdjacentEnemies.Compare(list3.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		bool flag3 = FilterTargetAdjacentAllies == null;
		if (FilterTargetAdjacentAllies != null)
		{
			flag3 = FilterTargetAdjacentAllies.Compare(list4.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		bool flag4 = FilterTargetAdjacentSummonsOfCaster == null;
		if (FilterTargetAdjacentSummonsOfCaster != null)
		{
			flag4 = FilterTargetAdjacentSummonsOfCaster.Compare(list8.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		bool flag5 = FilterTargetAdjacentBannerSummonsOfCaster == null;
		if (FilterTargetAdjacentBannerSummonsOfCaster != null)
		{
			flag5 = FilterTargetAdjacentBannerSummonsOfCaster.Compare(list9.Count);
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		bool flag6 = FilterTargetAdjacentAlliesOfTarget == null;
		if (FilterTargetAdjacentAlliesOfTarget != null)
		{
			flag6 = FilterTargetAdjacentAlliesOfTarget.Compare(list5.Count);
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		bool flag7 = FilterCasterAdjacentEnemies == null;
		if (FilterCasterAdjacentEnemies != null)
		{
			flag7 = FilterCasterAdjacentEnemies.Compare(list6.Count);
			if (Invert)
			{
				flag7 = !flag7;
			}
		}
		bool flag8 = FilterCasterAdjacentAllies == null;
		if (FilterCasterAdjacentAllies != null)
		{
			flag8 = FilterCasterAdjacentAllies.Compare(list7.Count);
			if (Invert)
			{
				flag8 = !flag8;
			}
		}
		m_LastCheckedTargetAdjacentActors = list;
		m_LastCheckedTargetAdjacentEnemies = list3;
		m_LastCheckedTargetAdjacentAllies = list4;
		m_LastCheckedTargetAdjacentSummonsOfCaster = list8;
		m_LastCheckedTargetAdjacentBannerSummonsOfCaster = list9;
		m_LastCheckedTargetAdjacentAlliesOfTarget = list5;
		m_LastCheckedCasterAdjacentEnemies = list6;
		m_LastCheckedCasterAdjacentAllies = list7;
		return flag & flag2 & flag3 & flag6 & flag7 & flag8 & flag4;
	}

	private bool CheckWallAdjacencyFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentToWalls == null && FilterCasterAdjacentToWalls == null)
		{
			return true;
		}
		CTile tile = ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y];
		int num = 0;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(target.ArrayIndex.X, target.ArrayIndex.Y, eposition);
			if (adjacentTile != null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if ((!cNode.Walkable && !cNode.IsBridge) || !adjacentTile.IsMapShared(tile, hexMapOnly: true))
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		CTile tile2 = ScenarioManager.Tiles[self.ArrayIndex.X, self.ArrayIndex.Y];
		int num2 = 0;
		for (int j = 1; j < 7; j++)
		{
			ScenarioManager.EAdjacentPosition eposition2 = (ScenarioManager.EAdjacentPosition)j;
			CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(self.ArrayIndex.X, self.ArrayIndex.Y, eposition2);
			if (adjacentTile2 != null)
			{
				CNode cNode2 = ScenarioManager.PathFinder.Nodes[adjacentTile2.m_ArrayIndex.X, adjacentTile2.m_ArrayIndex.Y];
				if ((!cNode2.Walkable && !cNode2.IsBridge) || !adjacentTile2.IsMapShared(tile2, hexMapOnly: true))
				{
					num2++;
				}
			}
			else
			{
				num2++;
			}
		}
		bool flag = FilterTargetAdjacentToWalls == null;
		if (FilterTargetAdjacentToWalls != null)
		{
			flag = FilterTargetAdjacentToWalls.Compare(num);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterCasterAdjacentToWalls == null;
		if (FilterCasterAdjacentToWalls != null)
		{
			flag2 = FilterCasterAdjacentToWalls.Compare(num2);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		LastCheckedTargetAdjacentWalls = num;
		LastCheckedCasterAdjacentWalls = num2;
		return flag & flag2;
	}

	private bool CheckInWaterFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetInWater == null)
		{
			return true;
		}
		_ = ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y];
		bool flag = false;
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(target.ArrayIndex.X, target.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
		if (adjacentTile != null)
		{
			flag = adjacentTile.m_Props.Find((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater) != null;
		}
		bool flag2 = FilterTargetInWater == null;
		if (FilterTargetInWater != null)
		{
			flag2 = FilterTargetInWater.Compare(flag ? 1 : 0);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		LastCheckedTargetInWater = flag;
		return flag2;
	}

	private bool CheckTileAdjacencyFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentValidTiles == null && FilterCasterAdjacentValidTiles == null)
		{
			return true;
		}
		List<CTile> list = new List<CTile>();
		if (FilterTargetAdjacentValidTilesFilterList != null)
		{
			for (int i = 1; i < 7; i++)
			{
				ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(target.ArrayIndex.X, target.ArrayIndex.Y, eposition);
				foreach (EFilterTile filterTargetAdjacentValidTilesFilter in FilterTargetAdjacentValidTilesFilterList)
				{
					if (adjacentTile != null && IsValidTile(adjacentTile, filterTargetAdjacentValidTilesFilter))
					{
						list.Add(adjacentTile);
						break;
					}
				}
			}
		}
		List<CTile> list2 = new List<CTile>();
		if (FilterCasterAdjacentValidTilesFilterList != null)
		{
			for (int j = 1; j < 7; j++)
			{
				ScenarioManager.EAdjacentPosition eposition2 = (ScenarioManager.EAdjacentPosition)j;
				CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(self.ArrayIndex.X, self.ArrayIndex.Y, eposition2);
				foreach (EFilterTile filterCasterAdjacentValidTilesFilter in FilterCasterAdjacentValidTilesFilterList)
				{
					if (adjacentTile2 != null && IsValidTile(adjacentTile2, filterCasterAdjacentValidTilesFilter))
					{
						list2.Add(adjacentTile2);
						break;
					}
				}
			}
		}
		bool flag = FilterTargetAdjacentValidTiles == null;
		if (FilterTargetAdjacentValidTiles != null)
		{
			flag = FilterTargetAdjacentValidTiles.Compare(list.Count);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterCasterAdjacentValidTiles == null;
		if (FilterCasterAdjacentValidTiles != null)
		{
			flag2 = FilterCasterAdjacentValidTiles.Compare(list2.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		LastCheckedTargetAdjacentTiles = list.Count;
		LastCheckedCasterAdjacentValidTiles = list2.Count;
		return flag & flag2;
	}

	private bool CheckConditionsFilters(CActor target, CActor caster)
	{
		bool flag = FilterTargetHasNegativeConditions == null;
		bool flag2 = FilterTargetHasPositiveConditions == null;
		bool flag3 = FilterTargetNegativeConditionCount == null;
		bool flag4 = FilterTargetPositiveConditionCount == null;
		bool flag5 = FilterCasterHasNegativeConditions == null;
		bool flag6 = FilterCasterHasPositiveConditions == null;
		bool flag7 = FilterCasterNegativeConditionCount == null;
		bool flag8 = FilterCasterPositiveConditionCount == null;
		if (FilterTargetHasNegativeConditions != null)
		{
			flag = FilterTargetHasNegativeConditions.Any((CCondition.ENegativeCondition a) => target.Tokens.HasKey(a));
			if (Invert)
			{
				flag = !flag;
			}
		}
		if (FilterTargetHasPositiveConditions != null)
		{
			flag2 = FilterTargetHasPositiveConditions.Any((CCondition.EPositiveCondition a) => target.Tokens.HasKey(a));
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		if (FilterTargetNegativeConditionCount != null)
		{
			flag3 = FilterTargetNegativeConditionCount.Compare(target.Tokens.CheckNegativeTokens.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		if (FilterTargetPositiveConditionCount != null)
		{
			flag4 = FilterTargetPositiveConditionCount.Compare(target.Tokens.CheckPositiveTokens.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		if (FilterCasterHasNegativeConditions != null)
		{
			flag5 = FilterCasterHasNegativeConditions.Any((CCondition.ENegativeCondition a) => caster.Tokens.HasKey(a));
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		if (FilterCasterHasPositiveConditions != null)
		{
			flag6 = FilterCasterHasPositiveConditions.Any((CCondition.EPositiveCondition a) => caster.Tokens.HasKey(a));
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		if (FilterCasterNegativeConditionCount != null)
		{
			flag7 = FilterCasterNegativeConditionCount.Compare(caster.Tokens.CheckNegativeTokens.Count);
			if (Invert)
			{
				flag7 = !flag7;
			}
		}
		if (FilterCasterPositiveConditionCount != null)
		{
			flag8 = FilterCasterPositiveConditionCount.Compare(caster.Tokens.CheckPositiveTokens.Count);
			if (Invert)
			{
				flag8 = !flag8;
			}
		}
		return flag & flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8;
	}

	private bool CheckConditionsFilters_ActorState(ActorState targetState, ActorState casterState)
	{
		bool flag = FilterTargetHasNegativeConditions == null;
		bool flag2 = FilterTargetHasPositiveConditions == null;
		bool flag3 = FilterTargetNegativeConditionCount == null;
		bool flag4 = FilterTargetPositiveConditionCount == null;
		bool flag5 = FilterCasterHasNegativeConditions == null;
		bool flag6 = FilterCasterHasPositiveConditions == null;
		bool flag7 = FilterCasterNegativeConditionCount == null;
		bool flag8 = FilterCasterPositiveConditionCount == null;
		if (FilterTargetHasNegativeConditions != null)
		{
			flag = FilterTargetHasNegativeConditions.Any((CCondition.ENegativeCondition a) => targetState.NegativeConditions.Any((NegativeConditionPair n) => n.NegativeCondition == a));
			if (Invert)
			{
				flag = !flag;
			}
		}
		if (FilterTargetHasPositiveConditions != null)
		{
			flag2 = FilterTargetHasPositiveConditions.Any((CCondition.EPositiveCondition a) => targetState.PositiveConditions.Any((PositiveConditionPair n) => n.PositiveCondition == a));
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		if (FilterTargetNegativeConditionCount != null)
		{
			flag3 = FilterTargetNegativeConditionCount.Compare(targetState.NegativeConditions.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		if (FilterTargetPositiveConditionCount != null)
		{
			flag4 = FilterTargetPositiveConditionCount.Compare(targetState.PositiveConditions.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		if (FilterCasterHasNegativeConditions != null)
		{
			flag5 = FilterCasterHasNegativeConditions.Any((CCondition.ENegativeCondition a) => casterState.NegativeConditions.Any((NegativeConditionPair n) => n.NegativeCondition == a));
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		if (FilterCasterHasPositiveConditions != null)
		{
			flag6 = FilterCasterHasPositiveConditions.Any((CCondition.EPositiveCondition a) => casterState.PositiveConditions.Any((PositiveConditionPair n) => n.PositiveCondition == a));
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		if (FilterCasterNegativeConditionCount != null)
		{
			flag7 = FilterCasterNegativeConditionCount.Compare(casterState.NegativeConditions.Count);
			if (Invert)
			{
				flag7 = !flag7;
			}
		}
		if (FilterCasterPositiveConditionCount != null)
		{
			flag8 = FilterCasterPositiveConditionCount.Compare(casterState.PositiveConditions.Count);
			if (Invert)
			{
				flag8 = !flag8;
			}
		}
		return flag & flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8;
	}

	private bool CheckConditionImmunitiesFilters(CActor target, CActor caster)
	{
		bool flag = FilterTargetHasImmunities == null;
		bool flag2 = FilterTargetImmunitiesCount == null;
		bool flag3 = FilterCasterHasImmunities == null;
		bool flag4 = FilterCasterImmunitiesCount == null;
		if (FilterTargetHasImmunities != null)
		{
			flag = FilterTargetHasImmunities.Any((CAbility.EAbilityType a) => target.Immunities.Contains(a));
			if (Invert)
			{
				flag = !flag;
			}
		}
		if (FilterTargetImmunitiesCount != null)
		{
			flag2 = FilterTargetImmunitiesCount.Compare(target.Immunities.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		if (FilterCasterHasImmunities != null)
		{
			flag3 = FilterCasterHasImmunities.Any((CAbility.EAbilityType a) => caster.Immunities.Contains(a));
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		if (FilterCasterImmunitiesCount != null)
		{
			flag4 = FilterCasterImmunitiesCount.Compare(caster.Immunities.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		return flag & flag2 & flag3 & flag4;
	}

	private bool CheckConditionsImmunitiesFilters_ActorState(ActorState targetState, ActorState casterState)
	{
		bool flag = FilterTargetHasImmunities == null;
		bool flag2 = FilterTargetImmunitiesCount == null;
		bool flag3 = FilterCasterHasImmunities == null;
		bool flag4 = FilterCasterImmunitiesCount == null;
		return flag & flag2 & flag3 & flag4;
	}

	private bool CheckTargetHPtoMissingHPFilters(CActor target, CActor self)
	{
		if (FilterCompareTargetHPToYourMissingHP == null)
		{
			return true;
		}
		bool flag = FilterCompareTargetHPToYourMissingHP.Compare(target.Health, Math.Abs(self.OriginalMaxHealth - self.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetHPtoMissingHPFilters_ActorState(ActorState targetState, ActorState selfState)
	{
		if (FilterCompareTargetHPToYourMissingHP == null)
		{
			return true;
		}
		bool flag = FilterCompareTargetHPToYourMissingHP.Compare(targetState.Health, Math.Abs(selfState.MaxHealth - selfState.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetMissingHPFilters(CActor target)
	{
		if (FilterTargetMissingHP == null)
		{
			return true;
		}
		bool flag = FilterTargetMissingHP.Compare(Math.Abs(target.OriginalMaxHealth - target.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetMissingHPFilters_ActorState(ActorState targetState)
	{
		if (FilterTargetMissingHP == null)
		{
			return true;
		}
		bool flag = FilterTargetMissingHP.Compare(Math.Abs(targetState.MaxHealth - targetState.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	public static bool WallIsAdjacent(CTile tile)
	{
		bool result = false;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile != null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
				{
					result = true;
					break;
				}
				continue;
			}
			result = true;
			break;
		}
		return result;
	}

	public static int GetAdjacentWallCount(CActor actor)
	{
		if (actor.MultiHexArrayIndexes != null)
		{
			int num = 0;
			{
				foreach (Point multiHexArrayIndex in actor.MultiHexArrayIndexes)
				{
					int adjacentWallCount = GetAdjacentWallCount(ScenarioManager.Tiles[multiHexArrayIndex.X, multiHexArrayIndex.Y]);
					num += adjacentWallCount;
				}
				return num;
			}
		}
		return GetAdjacentWallCount(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
	}

	public static int GetAdjacentWallCount(CTile tile)
	{
		int num = 0;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile != null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		return num;
	}

	public static int ObstacleIsAdjacent(CActor actor)
	{
		if (actor.MultiHexArrayIndexes != null)
		{
			int num = 0;
			{
				foreach (Point multiHexArrayIndex in actor.MultiHexArrayIndexes)
				{
					int num2 = ObstacleIsAdjacent(ScenarioManager.Tiles[multiHexArrayIndex.X, multiHexArrayIndex.Y]);
					num += num2;
				}
				return num;
			}
		}
		return ObstacleIsAdjacent(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
	}

	public static int ObstacleIsAdjacent(CTile tile)
	{
		int num = 0;
		List<CObjectObstacle> list;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile == null)
			{
				continue;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
			{
				continue;
			}
			foreach (CObjectObstacle item in list)
			{
				if (item.RuntimeAttachedActor != null && !item.RuntimeAttachedActor.IsDead && !item.RuntimeAttachedActor.TreatAsObstacle)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in item.PathingBlockers)
				{
					if (adjacentTile == ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y])
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public static int ActorIsAdjacent(CTile tile, CActor actor, CActor.EType type)
	{
		int num = 0;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile == null)
			{
				continue;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
			{
				continue;
			}
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(adjacentTile.m_ArrayIndex))
			{
				bool flag = CActor.AreActorsAllied(actor.Type, item.Type, actor, item);
				if (type == CActor.EType.Player && item.Type == CActor.EType.Player)
				{
					num++;
				}
				else if ((type == CActor.EType.Ally) & flag)
				{
					num++;
				}
				else if (type == CActor.EType.Enemy && !flag)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static int ActorIsAdjacent(CTile tile, CActor actor, CActor.EType type, out List<CActor> adjacent)
	{
		int num = 0;
		adjacent = new List<CActor>();
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile == null)
			{
				continue;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
			{
				continue;
			}
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(adjacentTile.m_ArrayIndex))
			{
				bool flag = CActor.AreActorsAllied(actor.Type, item.Type, actor, item);
				if ((type == CActor.EType.Ally) & flag)
				{
					num++;
					adjacent.Add(item);
				}
				else if (type == CActor.EType.Enemy && !flag)
				{
					num++;
					adjacent.Add(item);
				}
			}
		}
		return num;
	}

	public static bool IsTileFeatureless(CTile tile, bool includeInitial = false, string monsterClassID = null, bool ignoreTraps = false)
	{
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			if (scenarioScript.TreatTileAsFeatureless(tile))
			{
				return true;
			}
			if (scenarioScript.TreatTileAsNonFeatureless(tile))
			{
				return false;
			}
		}
		CMonsterClass cMonsterClass = ((monsterClassID == null) ? null : MonsterClassManager.Classes.Find((CMonsterClass c) => c.ID == monsterClassID));
		bool flag = monsterClassID != null && cMonsterClass != null && cMonsterClass.IsWaterMonster;
		lock (tile.m_Props)
		{
			if (tile.m_Props.Count > 0)
			{
				foreach (CObjectProp prop in tile.m_Props)
				{
					if (prop.TreatAsNotFeatureless())
					{
						return false;
					}
					if (prop.TreatAsFeatureless())
					{
						continue;
					}
					if (prop.ObjectType == ScenarioManager.ObjectImportType.Door)
					{
						if (prop is CObjectDoor { Activated: false })
						{
							return false;
						}
					}
					else if (prop is CObjectObstacle cObjectObstacle)
					{
						if (cObjectObstacle.PathingBlockers != null && cObjectObstacle.PathingBlockers.Count > 0)
						{
							return false;
						}
					}
					else if (prop.ObjectType == ScenarioManager.ObjectImportType.TerrainWater)
					{
						if (!flag)
						{
							return false;
						}
					}
					else if (prop.ObjectType != ScenarioManager.ObjectImportType.GenericProp && prop.ObjectType != ScenarioManager.ObjectImportType.Portal && prop.ObjectType != ScenarioManager.ObjectImportType.MoneyToken && prop.ObjectType != ScenarioManager.ObjectImportType.CarryableQuestItem && prop.ObjectType != ScenarioManager.ObjectImportType.TerrainVisualEffect && prop.ObjectType != ScenarioManager.ObjectImportType.TerrainLampVisualEffect && prop.ObjectType != ScenarioManager.ObjectImportType.Shadow && prop.ObjectType != ScenarioManager.ObjectImportType.VFXProp && prop.ObjectType != ScenarioManager.ObjectImportType.MonsterGrave && prop.ObjectType != ScenarioManager.ObjectImportType.Spawner && (prop.ObjectType != ScenarioManager.ObjectImportType.Trap || (prop.ObjectType == ScenarioManager.ObjectImportType.Trap && !ignoreTraps && !prop.Activated)))
					{
						return false;
					}
				}
			}
		}
		if (flag && !tile.IsWater && (ScenarioScript.current == null || !ScenarioScript.current.IsUnderwater(tile)))
		{
			return false;
		}
		if (ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial) is CObjectActor { IsDead: false })
		{
			return false;
		}
		if (CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile) != null)
		{
			return false;
		}
		return true;
	}

	public static bool IsTileEmpty(CTile tile, bool includeInitial = false, string monsterClassID = null, bool ignoreTraps = false)
	{
		if (!tile.IsWalkable)
		{
			return false;
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial);
		if (cActor != null && !cActor.IsDead)
		{
			return false;
		}
		return IsTileFeatureless(tile, includeInitial, monsterClassID, ignoreTraps);
	}

	public static bool IsValidTile(CTile tile, EFilterTile tileFilter, bool includeInitial = false, string monsterClassID = null, List<(string, TValue)> vars = null, CVarsContext parentContext = null, bool canTargetUnrevealed = false)
	{
		bool flag = true;
		if (!canTargetUnrevealed && !GameState.IsInLevelEditor && tile.m_Hex2Map == null && tile.m_HexMap != null && !tile.m_HexMap.Revealed && tile.FindDoor() == null)
		{
			return false;
		}
		if (tileFilter.HasFlag(EFilterTile.EmptyHex))
		{
			flag = IsTileEmpty(tile, includeInitial, monsterClassID);
		}
		if (tileFilter.HasFlag(EFilterTile.EmptyHexIgnoreTraps))
		{
			if (!tile.IsWalkable)
			{
				return false;
			}
			CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial);
			if (cActor != null && !cActor.IsDead)
			{
				flag = false;
			}
			else if (!IsTileFeatureless(tile, includeInitial, monsterClassID, ignoreTraps: true))
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.FeaturelessHex))
		{
			flag = IsTileFeatureless(tile, includeInitial, monsterClassID);
		}
		if (tileFilter.HasFlag(EFilterTile.EmptyHexNoProps))
		{
			CActor cActor2 = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial);
			if (cActor2 != null && !cActor2.IsDead)
			{
				flag = false;
			}
			else if (tile.m_Props.Any((CObjectProp p) => p.ObjectType != ScenarioManager.ObjectImportType.Spawner))
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Obstacle))
		{
			CObjectObstacle cObjectObstacle = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile);
			}
			if (cObjectObstacle == null && !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Blocked)
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.ObstacleWithoutHealth))
		{
			CObjectObstacle cObjectObstacle2 = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle2 == null)
			{
				cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile);
			}
			if (cObjectObstacle2 == null && !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Blocked)
			{
				flag = false;
			}
			else if (cObjectObstacle2?.PropHealthDetails?.HasHealth == true)
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Terrain) && tile.FindTerrainProp() == null && !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Blocked)
		{
			flag = false;
		}
		if (tileFilter.HasFlag(EFilterTile.Trap) && tile.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) == null)
		{
			flag = false;
		}
		if (tileFilter.HasFlag(EFilterTile.Shadow) && tile.FindProp(ScenarioManager.ObjectImportType.Shadow) == null)
		{
			flag = false;
		}
		if (tileFilter.HasFlag(EFilterTile.Loot) && tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null && tile.FindProp(ScenarioManager.ObjectImportType.GoalChest) == null && tile.FindProp(ScenarioManager.ObjectImportType.MoneyToken) == null && tile.FindProp(ScenarioManager.ObjectImportType.CarryableQuestItem) == null && tile.FindProp(ScenarioManager.ObjectImportType.Resource) == null)
		{
			flag = false;
		}
		if (tileFilter.HasFlag(EFilterTile.SingleHexObstacle))
		{
			CObjectProp cObjectProp = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle);
			if (cObjectProp == null || (cObjectProp is CObjectObstacle cObjectObstacle3 && cObjectObstacle3.PathingBlockers.Count > 1))
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.ActiveSpawner) && !tile.m_Spawners.Any((CSpawner x) => x is CInteractableSpawner && x.IsActive))
		{
			CTile spawnerTile = null;
			CInteractableSpawner cInteractableSpawner = CObjectProp.FindSpawnerWithPathingBlocker(tile.m_ArrayIndex, ref spawnerTile);
			if (cInteractableSpawner == null || !cInteractableSpawner.IsActive)
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.AdjacentToWall) && !WallIsAdjacent(tile))
		{
			flag = false;
		}
		if (tileFilter.HasFlag(EFilterTile.DeactiveSpawner) && !tile.m_Spawners.Any((CSpawner x) => x is CInteractableSpawner && !x.IsActive))
		{
			CTile spawnerTile2 = null;
			CInteractableSpawner cInteractableSpawner2 = CObjectProp.FindSpawnerWithPathingBlocker(tile.m_ArrayIndex, ref spawnerTile2);
			if (cInteractableSpawner2 == null || cInteractableSpawner2.IsActive)
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Actor) && ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial) == null)
		{
			flag = false;
		}
		if (tileFilter.HasFlag(EFilterTile.ObjectActor))
		{
			CActor cActor3 = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial);
			if (cActor3 == null || !(cActor3 is CObjectActor))
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Wall))
		{
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
			if (cNode.Walkable || cNode.IsBridge)
			{
				flag = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Custom) && vars != null)
		{
			(string, TValue) tuple = vars.SingleOrDefault(((string, TValue) x) => x.Item1 == "IsValidTile");
			(string, TValue) tuple2 = tuple;
			if ((tuple2.Item1 != null || tuple2.Item2 != default(TValue)) && tuple.Item2.objVal is CExpression cExpression)
			{
				CVars cVars = new CVars();
				CVarsContext context = new CVarsContext(cVars)
				{
					parentContext = parentContext
				};
				cVars.Set("obj", new TValue(tile));
				flag &= cExpression.Calc(context).GetBool();
			}
		}
		if (tileFilter.HasFlag(EFilterTile.NotBlocked))
		{
			CNode cNode2 = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
			if (cNode2.Blocked || cNode2.SuperBlocked)
			{
				flag = false;
			}
		}
		return flag;
	}
}
