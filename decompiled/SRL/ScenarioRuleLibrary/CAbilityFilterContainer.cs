using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class CAbilityFilterContainer : ISerializable
{
	[JsonProperty]
	public List<CAbilityFilter> AbilityFilters { get; set; }

	[JsonProperty]
	public bool AndLogic { get; set; }

	public List<CActor> LastCheckedTargetAdjacentActors
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedTargetAdjacentActors);
			}
			return list.Distinct().ToList();
		}
	}

	public List<CActor> LastCheckedTargetAdjacentEnemies
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedTargetAdjacentEnemies);
			}
			return list.Distinct().ToList();
		}
	}

	public List<CActor> LastCheckedTargetAdjacentAllies
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedTargetAdjacentAllies);
			}
			return list.Distinct().ToList();
		}
	}

	public List<CActor> LastCheckedTargetAdjacentSummons
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedTargetAdjacentAllies);
			}
			return list.Distinct().ToList();
		}
	}

	public List<CActor> LastCheckedTargetAdjacentAlliesOfTarget
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedTargetAdjacentAlliesOfTarget);
			}
			return list.Distinct().ToList();
		}
	}

	public List<CActor> LastCheckedCasterAdjacentEnemies
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedCasterAdjacentEnemies);
			}
			return list.Distinct().ToList();
		}
	}

	public List<CActor> LastCheckedCasterAdjacentAllies
	{
		get
		{
			List<CActor> list = new List<CActor>();
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				list.AddRange(abilityFilter.LastCheckedCasterAdjacentAllies);
			}
			return list.Distinct().ToList();
		}
	}

	public int LastCheckedTargetAdjacentWalls
	{
		get
		{
			int num = 0;
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				num += abilityFilter.LastCheckedTargetAdjacentWalls;
			}
			return num;
		}
	}

	public int LastCheckedCasterAdjacentWalls
	{
		get
		{
			int num = 0;
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				num += abilityFilter.LastCheckedCasterAdjacentWalls;
			}
			return num;
		}
	}

	public int LastCheckedTargetInWater
	{
		get
		{
			int num = 0;
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				num += (abilityFilter.LastCheckedTargetInWater ? 1 : 0);
			}
			return num;
		}
	}

	public int LastCheckedTargetAdjacentTiles
	{
		get
		{
			int num = 0;
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				num += abilityFilter.LastCheckedTargetAdjacentTiles;
			}
			return num;
		}
	}

	public int LastCheckedCasterAdjacentValidTiles
	{
		get
		{
			int num = 0;
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				num += abilityFilter.LastCheckedCasterAdjacentValidTiles;
			}
			return num;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AbilityFilters", AbilityFilters);
		info.AddValue("AndLogic", AndLogic);
	}

	public CAbilityFilterContainer(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "AbilityFilters"))
				{
					if (name == "AndLogic")
					{
						AndLogic = info.GetBoolean("AndLogic");
					}
				}
				else
				{
					AbilityFilters = (List<CAbilityFilter>)info.GetValue("AbilityFilters", typeof(List<CAbilityFilter>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CAbilityFilterContainer entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAbilityFilterContainer(List<CAbilityFilter> abilityFilters)
	{
		AbilityFilters = abilityFilters;
	}

	[JsonConstructor]
	public CAbilityFilterContainer(List<CAbilityFilter> abilityFilters, bool andLogic)
	{
		AbilityFilters = abilityFilters;
		AndLogic = andLogic;
	}

	public CAbilityFilterContainer(CAbilityFilter.EFilterTargetType filterTargetType = CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy filterEnemy = CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType filterActorType = CAbilityFilter.EFilterActorType.None, CAbilityFilter.EFilterAllyType filterAllyType = CAbilityFilter.EFilterAllyType.Ally, List<(string, TValue)> filterVars = null, List<string> filterPlayerClasses = null, List<string> filterEnemyClasses = null, List<string> filterHeroSummonClasses = null, List<string> filterObjectClasses = null, List<string> filterSummonerClasses = null, CEqualityFilter filterHealth = null, CEqualityFilter filterHealthSelf = null, CEqualityFilter filterTargetAdjacentActors = null, CEqualityFilter filterTargetAdjacentEnemies = null, CEqualityFilter filterTargetAdjacentAllies = null, CEqualityFilter filterTargetAdjacentOwnSummons = null, CEqualityFilter filterTargetAdjacentAlliesOfTarget = null, CEqualityFilter filterCasterAdjacentEnemies = null, CEqualityFilter filterCasterAdjacentAllies = null, CEqualityFilter filterTargetAdjacentToWall = null, CEqualityFilter filterCasterAdjacentToWall = null, CEqualityFilter filterTargetInWater = null, CEqualityFilter filterTargetAdjacentToValidTiles = null, List<CAbilityFilter.EFilterTile> filterTargetAdjacentToValidTilesFilter = null, CEqualityFilter filterCasterAdjacentToValidTiles = null, List<CAbilityFilter.EFilterTile> filterCasterAdjacentToValidTilesFilter = null, List<CCondition.ENegativeCondition> filterTargetHasNegativeConditions = null, CEqualityFilter filterTargetNegativeConditionCount = null, List<CCondition.EPositiveCondition> filterTargetHasPositiveConditions = null, CEqualityFilter filterTargetPositiveConditionCount = null, List<CCondition.ENegativeCondition> filterCasterHasNegativeConditions = null, CEqualityFilter filterCasterNegativeConditionCount = null, List<CCondition.EPositiveCondition> filterCasterHasPositiveConditions = null, CEqualityFilter filterCasterPositiveConditionCount = null, List<CAbility.EAbilityType> filterTargetHasImmunities = null, CEqualityFilter filterTargetImmunitiesCount = null, List<CAbility.EAbilityType> filterCasterHasImmunities = null, CEqualityFilter filterCasterImmunitiesCount = null, CEqualityFilter filterCompareTargetHPToYourMissingHP = null, CEqualityFilter filterTargetMissingHP = null, CAbilityFilter.EFilterFlags filterFlags = CAbilityFilter.EFilterFlags.None, List<string> filterTargetHasCharacterResource = null, bool invert = false, bool useTargetOriginalType = false, int checkAdjacentRange = 1)
	{
		AbilityFilters = new List<CAbilityFilter>();
		AbilityFilters.Add(new CAbilityFilter(filterTargetType, filterEnemy, filterActorType, filterAllyType, filterVars, filterPlayerClasses, filterEnemyClasses, filterHeroSummonClasses, filterObjectClasses, filterSummonerClasses, filterHealth, filterHealthSelf, filterTargetAdjacentActors, filterTargetAdjacentEnemies, filterTargetAdjacentAllies, filterTargetAdjacentOwnSummons, filterTargetAdjacentOwnSummons, filterTargetAdjacentAlliesOfTarget, filterCasterAdjacentEnemies, filterCasterAdjacentAllies, filterTargetAdjacentToWall, filterCasterAdjacentToWall, filterTargetInWater, filterTargetAdjacentToValidTiles, filterTargetAdjacentToValidTilesFilter, filterCasterAdjacentToValidTiles, filterCasterAdjacentToValidTilesFilter, filterTargetHasNegativeConditions, filterTargetNegativeConditionCount, filterTargetHasPositiveConditions, filterTargetPositiveConditionCount, filterCasterHasNegativeConditions, filterCasterNegativeConditionCount, filterCasterHasPositiveConditions, filterCasterPositiveConditionCount, filterTargetHasImmunities, filterTargetImmunitiesCount, filterCasterHasImmunities, filterCasterImmunitiesCount, filterCompareTargetHPToYourMissingHP, filterTargetMissingHP, filterFlags, filterTargetHasCharacterResource, invert, useTargetOriginalType, checkAdjacentRange));
	}

	public static CAbilityFilterContainer CreateDefaultFilter()
	{
		return new CAbilityFilterContainer(new List<CAbilityFilter> { CAbilityFilter.CreateDefaultFilter() });
	}

	public static CAbilityFilterContainer CreateDefaultFriendlyFilter()
	{
		return new CAbilityFilterContainer(new List<CAbilityFilter> { CAbilityFilter.CreateDefaultFriendlyFilter() });
	}

	public static CAbilityFilterContainer CreateDefaultEnemyFilter()
	{
		return new CAbilityFilterContainer(new List<CAbilityFilter> { CAbilityFilter.CreateDefaultEnemyFilter() });
	}

	public CAbilityFilterContainer Copy()
	{
		List<CAbilityFilter> list = new List<CAbilityFilter>();
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			list.Add(abilityFilter.Copy());
		}
		return new CAbilityFilterContainer(list)
		{
			AndLogic = AndLogic
		};
	}

	public bool Equals(CAbilityFilterContainer compare)
	{
		if (compare == null)
		{
			return false;
		}
		if (AndLogic != compare.AndLogic)
		{
			return false;
		}
		if (AbilityFilters.Count != compare.AbilityFilters.Count)
		{
			return false;
		}
		for (int i = 0; i < AbilityFilters.Count; i++)
		{
			if (!AbilityFilters[i].Equals(compare.AbilityFilters[i]))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsValidTarget(CActor target, CActor self, bool isTargetedAbility, bool useTargetOriginalType = false, bool? canTargetInvisible = false, bool skipUntargetableCheck = false, CActiveBonus activeBonus = null, bool doesntWorkOnDeactivated = true)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			bool result = AndLogic;
			foreach (CAbilityFilter abilityFilter in AbilityFilters)
			{
				bool flag = abilityFilter.IsValidTarget(target, self, isTargetedAbility, useTargetOriginalType, canTargetInvisible, skipUntargetableCheck, activeBonus, doesntWorkOnDeactivated);
				if (!AndLogic & flag)
				{
					result = true;
				}
				else if (AndLogic && !flag)
				{
					result = false;
				}
			}
			return result;
		}
	}

	public bool FilterAlly()
	{
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			if (abilityFilter.FilterTargetType.HasFlag(CAbilityFilter.EFilterTargetType.Ally))
			{
				return true;
			}
		}
		return false;
	}

	public bool FilterAllyType(CAbilityFilter.EFilterAllyType allyType)
	{
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			if (abilityFilter.FilterAllyType.HasFlag(allyType))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValidTarget_ActorState(ActorState targetState, ActorState selfState, bool isTargetedAbility, bool useTargetOriginalType = false, bool? canTargetInvisible = false)
	{
		bool result = AndLogic;
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			bool flag = abilityFilter.IsValidTarget_ActorState(targetState, selfState, isTargetedAbility, useTargetOriginalType, canTargetInvisible);
			if (!AndLogic & flag)
			{
				result = true;
			}
			else if (AndLogic && !flag)
			{
				result = false;
			}
		}
		return result;
	}

	public bool HasTargetTypeFlag(CAbilityFilter.EFilterTargetType targetType, bool exclusive = false)
	{
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			bool flag = (exclusive ? (abilityFilter.FilterTargetType == targetType) : abilityFilter.FilterTargetType.HasFlag(targetType));
			if (!AndLogic & flag)
			{
				return true;
			}
			if (AndLogic && !flag)
			{
				return false;
			}
		}
		return AndLogic;
	}

	public bool HasTargetTypeFilters()
	{
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			if (abilityFilter.FilterTargetType != CAbilityFilter.EFilterTargetType.None)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNonTargetTypeFilters()
	{
		foreach (CAbilityFilter abilityFilter in AbilityFilters)
		{
			if (abilityFilter.FilterVars != null && abilityFilter.FilterVars.Count > 0)
			{
				return true;
			}
			if (abilityFilter.FilterEnemy != CAbilityFilter.EFilterEnemy.None)
			{
				return true;
			}
			if (abilityFilter.FilterActorType != CAbilityFilter.EFilterActorType.None)
			{
				return true;
			}
			if (abilityFilter.FilterPlayerClasses != null && abilityFilter.FilterPlayerClasses.Count > 0)
			{
				return true;
			}
			if (abilityFilter.FilterEnemyClasses != null && abilityFilter.FilterEnemyClasses.Count > 0)
			{
				return true;
			}
			if (abilityFilter.FilterHeroSummonClasses != null && abilityFilter.FilterHeroSummonClasses.Count > 0)
			{
				return true;
			}
			if (abilityFilter.FilterObjectClasses != null && abilityFilter.FilterObjectClasses.Count > 0)
			{
				return true;
			}
			if (abilityFilter.FilterSummonerClasses != null && abilityFilter.FilterSummonerClasses.Count > 0)
			{
				return true;
			}
			if (abilityFilter.FilterHealth != null)
			{
				return true;
			}
			if (abilityFilter.FilterHealthSelf != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetAdjacentActors != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetAdjacentEnemies != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetAdjacentAllies != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetAdjacentAlliesOfTarget != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterAdjacentEnemies != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterAdjacentAllies != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetAdjacentToWalls != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterAdjacentToWalls != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetInWater != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetAdjacentValidTiles != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterAdjacentValidTiles != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetHasNegativeConditions != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetNegativeConditionCount != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetHasPositiveConditions != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetPositiveConditionCount != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterHasNegativeConditions != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterNegativeConditionCount != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterHasPositiveConditions != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterPositiveConditionCount != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetHasImmunities != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetImmunitiesCount != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterHasImmunities != null)
			{
				return true;
			}
			if (abilityFilter.FilterCasterImmunitiesCount != null)
			{
				return true;
			}
			if (abilityFilter.FilterCompareTargetHPToYourMissingHP != null)
			{
				return true;
			}
			if (abilityFilter.FilterTargetMissingHP != null)
			{
				return true;
			}
			if (abilityFilter.FilterFlags != CAbilityFilter.EFilterFlags.None)
			{
				return true;
			}
		}
		return false;
	}
}
