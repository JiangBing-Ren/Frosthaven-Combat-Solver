using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectiveFilter : ISerializable
{
	public CAbilityFilter.EFilterActorType FilterActorType { get; set; }

	public CAbilityFilter.EFilterEnemy FilterEnemyType { get; set; }

	public CAbilityFilter.ELootType FilterLootType { get; set; }

	public CAbility.EAbilityType FilterAbilityType { get; set; }

	public CAbilityFilter.EFilterRevealedType FilterRevealedType { get; set; }

	public List<string> FilterPlayerClassIDs { get; set; }

	public List<string> FilterEnemyClassIDs { get; set; }

	public List<string> FilterHeroSummonClassIDs { get; set; }

	public List<string> FilterObjectClassIDs { get; set; }

	public List<string> FilterActorGUIDs { get; set; }

	public List<string> FilterPropGUIDs { get; set; }

	public List<string> FilterItemIDs { get; set; }

	public List<CCondition.ENegativeCondition> FilterNegativeConditions { get; set; }

	public List<CCondition.EPositiveCondition> FilterPositiveConditions { get; set; }

	public List<string> FilterActiveBonusTypes { get; set; }

	public List<string> FilterPersonalQuestIDs { get; set; }

	public bool FilterCarryingQuestItem { get; set; }

	public bool FilterLootedGoalChest { get; set; }

	public bool Invert { get; set; }

	public bool FilterHasValues
	{
		get
		{
			if (FilterActorType == CAbilityFilter.EFilterActorType.None && FilterEnemyType == CAbilityFilter.EFilterEnemy.None && FilterLootType == CAbilityFilter.ELootType.None && FilterAbilityType == CAbility.EAbilityType.None && FilterRevealedType == CAbilityFilter.EFilterRevealedType.None && (FilterPlayerClassIDs == null || FilterPlayerClassIDs.Count <= 0) && (FilterEnemyClassIDs == null || FilterEnemyClassIDs.Count <= 0) && (FilterHeroSummonClassIDs == null || FilterHeroSummonClassIDs.Count <= 0) && (FilterObjectClassIDs == null || FilterObjectClassIDs.Count <= 0) && (FilterActorGUIDs == null || FilterActorGUIDs.Count <= 0) && (FilterPropGUIDs == null || FilterPropGUIDs.Count <= 0) && (FilterItemIDs == null || FilterItemIDs.Count <= 0) && (FilterNegativeConditions == null || FilterNegativeConditions.Count <= 0) && (FilterPositiveConditions == null || FilterPositiveConditions.Count <= 0) && (FilterActiveBonusTypes == null || FilterActiveBonusTypes.Count <= 0))
			{
				if (FilterPersonalQuestIDs != null)
				{
					return FilterPersonalQuestIDs.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	public string ActorLocKey
	{
		get
		{
			if (FilterActorType != CAbilityFilter.EFilterActorType.None)
			{
				switch (FilterActorType)
				{
				case CAbilityFilter.EFilterActorType.Player:
					return "GUI_OBJECTIVE_FILTER_ACTOR_PLAYER";
				case CAbilityFilter.EFilterActorType.Enemy:
					return "GUI_OBJECTIVE_FILTER_ACTOR_ENEMY";
				case CAbilityFilter.EFilterActorType.Summon:
					return "GUI_OBJECTIVE_FILTER_ACTOR_SUMMON";
				}
			}
			return string.Empty;
		}
	}

	public bool Compare(CObjectiveFilter compareFilter)
	{
		if (FilterActorType != compareFilter.FilterActorType)
		{
			return false;
		}
		if (FilterEnemyType != compareFilter.FilterEnemyType)
		{
			return false;
		}
		if (FilterLootType != compareFilter.FilterLootType)
		{
			return false;
		}
		if (FilterAbilityType != compareFilter.FilterAbilityType)
		{
			return false;
		}
		if (FilterRevealedType != compareFilter.FilterRevealedType)
		{
			return false;
		}
		if (FilterPlayerClassIDs != null)
		{
			foreach (string filterPlayerClassID in FilterPlayerClassIDs)
			{
				if (!compareFilter.FilterPlayerClassIDs.Contains(filterPlayerClassID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterPlayerClassIDs != null)
		{
			return false;
		}
		if (FilterEnemyClassIDs != null)
		{
			foreach (string filterEnemyClassID in FilterEnemyClassIDs)
			{
				if (!compareFilter.FilterEnemyClassIDs.Contains(filterEnemyClassID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterEnemyClassIDs != null)
		{
			return false;
		}
		if (FilterHeroSummonClassIDs != null)
		{
			foreach (string filterHeroSummonClassID in FilterHeroSummonClassIDs)
			{
				if (!compareFilter.FilterHeroSummonClassIDs.Contains(filterHeroSummonClassID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterHeroSummonClassIDs != null)
		{
			return false;
		}
		if (FilterObjectClassIDs != null)
		{
			foreach (string filterObjectClassID in FilterObjectClassIDs)
			{
				if (!compareFilter.FilterObjectClassIDs.Contains(filterObjectClassID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterObjectClassIDs != null)
		{
			return false;
		}
		if (FilterActorGUIDs != null)
		{
			foreach (string filterActorGUID in FilterActorGUIDs)
			{
				if (!compareFilter.FilterActorGUIDs.Contains(filterActorGUID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterActorGUIDs != null)
		{
			return false;
		}
		if (FilterPropGUIDs != null)
		{
			foreach (string filterPropGUID in FilterPropGUIDs)
			{
				if (!compareFilter.FilterPropGUIDs.Contains(filterPropGUID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterPropGUIDs != null)
		{
			return false;
		}
		if (FilterItemIDs != null)
		{
			foreach (string filterItemID in FilterItemIDs)
			{
				if (!compareFilter.FilterItemIDs.Contains(filterItemID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterItemIDs != null)
		{
			return false;
		}
		if (FilterNegativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition filterNegativeCondition in FilterNegativeConditions)
			{
				if (!compareFilter.FilterNegativeConditions.Contains(filterNegativeCondition))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterNegativeConditions != null)
		{
			return false;
		}
		if (FilterPositiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition filterPositiveCondition in FilterPositiveConditions)
			{
				if (!compareFilter.FilterPositiveConditions.Contains(filterPositiveCondition))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterPositiveConditions != null)
		{
			return false;
		}
		if (FilterActiveBonusTypes != null)
		{
			foreach (string filterActiveBonusType in FilterActiveBonusTypes)
			{
				if (!compareFilter.FilterActiveBonusTypes.Contains(filterActiveBonusType))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterActiveBonusTypes != null)
		{
			return false;
		}
		if (FilterPersonalQuestIDs != null)
		{
			foreach (string filterPersonalQuestID in FilterPersonalQuestIDs)
			{
				if (!compareFilter.FilterPersonalQuestIDs.Contains(filterPersonalQuestID))
				{
					return false;
				}
			}
		}
		else if (compareFilter.FilterPersonalQuestIDs != null)
		{
			return false;
		}
		if (FilterCarryingQuestItem != compareFilter.FilterCarryingQuestItem)
		{
			return false;
		}
		if (FilterLootedGoalChest != compareFilter.FilterLootedGoalChest)
		{
			return false;
		}
		if (Invert != compareFilter.Invert)
		{
			return false;
		}
		return true;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("FilterActor", FilterActorType);
		info.AddValue("FilterEnemyType", FilterEnemyType);
		info.AddValue("FilterLootType", FilterLootType);
		info.AddValue("FilterAbilityType", FilterAbilityType);
		info.AddValue("FilterRevealedType", FilterRevealedType);
		info.AddValue("FilterPlayerClassIDs", FilterPlayerClassIDs);
		info.AddValue("FilterEnemyClassIDs", FilterEnemyClassIDs);
		info.AddValue("FilterHeroSummonClassIDs", FilterHeroSummonClassIDs);
		info.AddValue("FilterObjectClassIDs", FilterObjectClassIDs);
		info.AddValue("FilterActorGUIDs", FilterActorGUIDs);
		info.AddValue("FilterPropGUIDs", FilterPropGUIDs);
		info.AddValue("FilterItemIDs", FilterItemIDs);
		info.AddValue("FilterNegativeConditions", FilterNegativeConditions);
		info.AddValue("FilterPositiveConditions", FilterPositiveConditions);
		info.AddValue("FilterActiveBonusTypes", FilterActiveBonusTypes);
		info.AddValue("FilterPersonalQuestIDs", FilterPersonalQuestIDs);
		info.AddValue("FilterCarryingQuestItem", FilterCarryingQuestItem);
		info.AddValue("FilterLootedGoalChest", FilterLootedGoalChest);
		info.AddValue("Invert", Invert);
	}

	public CObjectiveFilter(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "FilterActor":
					FilterActorType = (CAbilityFilter.EFilterActorType)info.GetValue("FilterActor", typeof(CAbilityFilter.EFilterActorType));
					break;
				case "FilterActorType":
				{
					int num = info.GetInt32("FilterActorType");
					if (num == 22)
					{
						num = 16;
					}
					FilterActorType = (CAbilityFilter.EFilterActorType)num;
					break;
				}
				case "FilterEnemyType":
					FilterEnemyType = (CAbilityFilter.EFilterEnemy)info.GetValue("FilterEnemyType", typeof(CAbilityFilter.EFilterEnemy));
					break;
				case "FilterLootType":
					FilterLootType = (CAbilityFilter.ELootType)info.GetValue("FilterLootType", typeof(CAbilityFilter.ELootType));
					break;
				case "FilterAbilityType":
					FilterAbilityType = (CAbility.EAbilityType)info.GetValue("FilterAbilityType", typeof(CAbility.EAbilityType));
					break;
				case "FilterRevealedType":
					FilterRevealedType = (CAbilityFilter.EFilterRevealedType)info.GetValue("FilterRevealedType", typeof(CAbilityFilter.EFilterRevealedType));
					break;
				case "FilterPlayerClassIDs":
					FilterPlayerClassIDs = (List<string>)info.GetValue("FilterPlayerClassIDs", typeof(List<string>));
					break;
				case "FilterEnemyClassIDs":
					FilterEnemyClassIDs = (List<string>)info.GetValue("FilterEnemyClassIDs", typeof(List<string>));
					break;
				case "FilterHeroSummonClassIDs":
					FilterHeroSummonClassIDs = (List<string>)info.GetValue("FilterHeroSummonClassIDs", typeof(List<string>));
					break;
				case "FilterObjectClassIDs":
					FilterObjectClassIDs = (List<string>)info.GetValue("FilterObjectClassIDs", typeof(List<string>));
					break;
				case "FilterActorGUIDs":
					FilterActorGUIDs = (List<string>)info.GetValue("FilterActorGUIDs", typeof(List<string>));
					break;
				case "FilterPropGUIDs":
					FilterPropGUIDs = (List<string>)info.GetValue("FilterPropGUIDs", typeof(List<string>));
					break;
				case "FilterItemIDs":
					FilterItemIDs = (List<string>)info.GetValue("FilterItemIDs", typeof(List<string>));
					break;
				case "FilterNegativeConditions":
					FilterNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("FilterNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "FilterPositiveConditions":
					FilterPositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("FilterPositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "FilterActiveBonusTypes":
					FilterActiveBonusTypes = (List<string>)info.GetValue("FilterActiveBonusTypes", typeof(List<string>));
					break;
				case "FilterPersonalQuestIDs":
					FilterPersonalQuestIDs = (List<string>)info.GetValue("FilterPersonalQuestIDs", typeof(List<string>));
					break;
				case "FilterCarryingQuestItem":
					FilterCarryingQuestItem = info.GetBoolean("FilterCarryingQuestItem");
					break;
				case "FilterLootedGoalChest":
					FilterLootedGoalChest = info.GetBoolean("FilterLootedGoalChest");
					break;
				case "Invert":
					Invert = info.GetBoolean("Invert");
					break;
				case "FilterPlayerClasses":
				{
					List<string> list4 = (List<string>)info.GetValue("FilterPlayerClasses", typeof(List<string>));
					if (list4 == null)
					{
						break;
					}
					FilterPlayerClassIDs = new List<string>();
					foreach (string item2 in list4)
					{
						if (!item2.EndsWith("ID"))
						{
							FilterPlayerClassIDs.Add(item2.Replace(" ", string.Empty) + "ID");
						}
						else
						{
							FilterPlayerClassIDs.Add(item2);
						}
					}
					break;
				}
				case "FilterEnemyClasses":
				{
					List<string> list3 = (List<string>)info.GetValue("FilterEnemyClasses", typeof(List<string>));
					if (list3 == null)
					{
						break;
					}
					FilterEnemyClassIDs = new List<string>();
					foreach (string item3 in list3)
					{
						if (!item3.EndsWith("ID"))
						{
							FilterEnemyClassIDs.Add(item3.Replace(" ", string.Empty) + "ID");
						}
						else
						{
							FilterEnemyClassIDs.Add(item3);
						}
					}
					break;
				}
				case "FilterHeroSummonClasses":
				{
					List<string> list2 = (List<string>)info.GetValue("FilterHeroSummonClasses", typeof(List<string>));
					if (list2 == null)
					{
						break;
					}
					FilterHeroSummonClassIDs = new List<string>();
					foreach (string item4 in list2)
					{
						if (!item4.EndsWith("ID"))
						{
							FilterHeroSummonClassIDs.Add(item4.Replace(" ", string.Empty) + "ID");
						}
						else
						{
							FilterHeroSummonClassIDs.Add(item4);
						}
					}
					break;
				}
				case "FilterObjectClasses":
				{
					List<string> list = (List<string>)info.GetValue("FilterObjectClasses", typeof(List<string>));
					if (list == null)
					{
						break;
					}
					FilterObjectClassIDs = new List<string>();
					foreach (string item5 in list)
					{
						if (!item5.EndsWith("ID"))
						{
							FilterObjectClassIDs.Add(item5.Replace(" ", string.Empty) + "ID");
						}
						else
						{
							FilterObjectClassIDs.Add(item5);
						}
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectiveFilter entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (FilterObjectClassIDs == null)
		{
			return;
		}
		for (int i = 0; i < FilterObjectClassIDs.Count; i++)
		{
			if (FilterObjectClassIDs[i] == "Elementalist3DemonSummonAltarID")
			{
				FilterObjectClassIDs[i] = "DemonSummonAltarID";
			}
		}
	}

	public CObjectiveFilter(CAbilityFilter.EFilterActorType filterActorType = CAbilityFilter.EFilterActorType.None, CAbilityFilter.EFilterEnemy filterEnemyType = CAbilityFilter.EFilterEnemy.None, CAbilityFilter.ELootType filterLootType = CAbilityFilter.ELootType.None, CAbility.EAbilityType filterAbilityType = CAbility.EAbilityType.None, CAbilityFilter.EFilterRevealedType filterRevealedType = CAbilityFilter.EFilterRevealedType.None, List<string> filterPlayerClassIDs = null, List<string> filterEnemyClassIDs = null, List<string> filterHeroSummonClassIDs = null, List<string> filterObjectClassIDs = null, List<string> filterActorGUIDs = null, List<string> filterPropGUIDs = null, List<string> filterItemIDs = null, List<CCondition.ENegativeCondition> filterNegativeConditions = null, List<CCondition.EPositiveCondition> filterPositiveConditions = null, List<string> filterActiveBonusTypes = null, List<string> filterPersonalQuestIDs = null, bool filterCarryingQuestItem = false, bool filterLootedGoalChest = false, bool invert = false)
	{
		FilterActorType = filterActorType;
		FilterEnemyType = filterEnemyType;
		FilterLootType = filterLootType;
		FilterAbilityType = filterAbilityType;
		FilterRevealedType = filterRevealedType;
		FilterPlayerClassIDs = filterPlayerClassIDs;
		FilterEnemyClassIDs = filterEnemyClassIDs;
		FilterHeroSummonClassIDs = filterHeroSummonClassIDs;
		FilterObjectClassIDs = filterObjectClassIDs;
		FilterActorGUIDs = filterActorGUIDs;
		FilterPropGUIDs = filterPropGUIDs;
		FilterItemIDs = filterItemIDs;
		FilterNegativeConditions = filterNegativeConditions;
		FilterPositiveConditions = filterPositiveConditions;
		FilterActiveBonusTypes = filterActiveBonusTypes;
		FilterPersonalQuestIDs = filterPersonalQuestIDs;
		FilterCarryingQuestItem = filterCarryingQuestItem;
		FilterLootedGoalChest = filterLootedGoalChest;
		Invert = invert;
	}

	public CObjectiveFilter Copy()
	{
		return new CObjectiveFilter(FilterActorType, FilterEnemyType, FilterLootType, FilterAbilityType, FilterRevealedType, (FilterPlayerClassIDs != null) ? FilterPlayerClassIDs.ToList() : null, (FilterEnemyClassIDs != null) ? FilterEnemyClassIDs.ToList() : null, (FilterHeroSummonClassIDs != null) ? FilterHeroSummonClassIDs.ToList() : null, (FilterObjectClassIDs != null) ? FilterObjectClassIDs.ToList() : null, (FilterActorGUIDs != null) ? FilterActorGUIDs.ToList() : null, (FilterPropGUIDs != null) ? FilterPropGUIDs.ToList() : null, (FilterItemIDs != null) ? FilterItemIDs.ToList() : null, (FilterNegativeConditions != null) ? FilterNegativeConditions.ToList() : null, (FilterPositiveConditions != null) ? FilterPositiveConditions.ToList() : null, (FilterActiveBonusTypes != null) ? FilterActiveBonusTypes.ToList() : null, (FilterPersonalQuestIDs != null) ? FilterPersonalQuestIDs.ToList() : null, FilterCarryingQuestItem, FilterLootedGoalChest, Invert);
	}

	public override string ToString()
	{
		return "FilterActorType: " + FilterActorType.ToString() + " FilterEnemyType: " + FilterEnemyType.ToString() + " FilterLootType: " + FilterLootType.ToString() + " FilterAbilityType: " + FilterAbilityType.ToString() + " FilterRevealedType: " + FilterRevealedType.ToString() + " FilterPlayerClassIDs: (" + string.Join(", ", FilterPlayerClassIDs) + ") FilterEnemyClassIDs: (" + string.Join(", ", FilterEnemyClassIDs) + ") FilterHeroSummonClassIDs: (" + string.Join(", ", FilterHeroSummonClassIDs) + ") FilterObjectClassIDs: (" + string.Join(", ", FilterObjectClassIDs) + ") FilterItemIDs: (" + string.Join(", ", FilterItemIDs) + ") FilterPositiveConditions: (" + string.Join(", ", FilterPositiveConditions) + ") FilterNegativeConditions: (" + string.Join(", ", FilterNegativeConditions) + ") FilterActiveBonusTypes: (" + string.Join(", ", FilterActiveBonusTypes) + ") FilterPersonalQuestIDs: (" + string.Join(", ", FilterPersonalQuestIDs) + ") FilterCarryingQuestItem: " + FilterCarryingQuestItem + " FilterCarryingQuestItem: " + FilterLootedGoalChest + " Invert: " + Invert + ")";
	}

	public bool IsValidTarget(ActorState actorState)
	{
		bool flag = CheckActorType(actorState) && CheckEnemyType(actorState) && CheckClass(actorState) && CheckActorGUID(actorState) && CheckRevealedType(actorState) && CheckConditions(actorState) && CheckItems(actorState) && CheckActiveBonusTypes(actorState) && CheckPersonalQuests(actorState) && CheckCarryingQuestItem(actorState) && CheckLootedGoalChest(actorState);
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	public bool IsValidProp(CObjectProp prop)
	{
		bool flag = CheckPropGUID(prop);
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	public bool IsValidLootTarget(ScenarioManager.ObjectImportType objectType)
	{
		bool flag = false;
		if (objectType == ScenarioManager.ObjectImportType.GoalChest || objectType == ScenarioManager.ObjectImportType.Chest || objectType == ScenarioManager.ObjectImportType.MoneyToken || objectType == ScenarioManager.ObjectImportType.CarryableQuestItem || objectType == ScenarioManager.ObjectImportType.Resource)
		{
			flag = CheckLootType(objectType);
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	public bool IsValidAbilityType(CAbility.EAbilityType abilityType)
	{
		bool flag = true;
		if (FilterAbilityType != CAbility.EAbilityType.None)
		{
			flag = FilterAbilityType.HasFlag(abilityType);
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckLootType(ScenarioManager.ObjectImportType objectType)
	{
		if (FilterLootType == CAbilityFilter.ELootType.None)
		{
			return true;
		}
		CAbilityFilter.ELootType eLootType = CAbilityFilter.ELootType.None;
		switch (objectType)
		{
		case ScenarioManager.ObjectImportType.MoneyToken:
			eLootType = CAbilityFilter.ELootType.Gold;
			break;
		case ScenarioManager.ObjectImportType.Chest:
			eLootType = CAbilityFilter.ELootType.Chest;
			break;
		case ScenarioManager.ObjectImportType.GoalChest:
			eLootType = CAbilityFilter.ELootType.GoalChest;
			break;
		case ScenarioManager.ObjectImportType.CarryableQuestItem:
			eLootType = CAbilityFilter.ELootType.QuestItem;
			break;
		case ScenarioManager.ObjectImportType.Resource:
			eLootType = CAbilityFilter.ELootType.Resource;
			break;
		}
		return FilterLootType.HasFlag(eLootType);
	}

	private bool CheckActorType(ActorState actorState)
	{
		if (FilterActorType == CAbilityFilter.EFilterActorType.None)
		{
			return true;
		}
		CAbilityFilter.EFilterActorType eFilterActorType = CAbilityFilter.EFilterActorType.None;
		if (actorState is PlayerState)
		{
			eFilterActorType = CAbilityFilter.EFilterActorType.Player;
		}
		else if (actorState is EnemyState enemyState)
		{
			if (enemyState.Enemy != null)
			{
				switch (enemyState.Enemy.OriginalType)
				{
				case CActor.EType.Enemy:
					eFilterActorType = CAbilityFilter.EFilterActorType.Enemy;
					break;
				case CActor.EType.Ally:
					eFilterActorType = CAbilityFilter.EFilterActorType.Ally;
					break;
				case CActor.EType.Enemy2:
					eFilterActorType = CAbilityFilter.EFilterActorType.Enemy2;
					break;
				case CActor.EType.Neutral:
					eFilterActorType = CAbilityFilter.EFilterActorType.Neutral;
					break;
				}
			}
			else
			{
				eFilterActorType = CAbilityFilter.EFilterActorType.Enemy;
			}
		}
		else if (actorState is HeroSummonState)
		{
			eFilterActorType = CAbilityFilter.EFilterActorType.Summon;
		}
		return FilterActorType.HasFlag(eFilterActorType);
	}

	private bool CheckEnemyType(ActorState enemyState)
	{
		if (FilterEnemyType == CAbilityFilter.EFilterEnemy.None)
		{
			return true;
		}
		if (enemyState is ObjectState)
		{
			return FilterEnemyType.HasFlag(CAbilityFilter.EFilterEnemy.Object);
		}
		if (enemyState is EnemyState)
		{
			CMonsterClass cMonsterClass = MonsterClassManager.Find(enemyState.ClassID);
			CAbilityFilter.EFilterEnemy eFilterEnemy = CAbilityFilter.EFilterEnemy.None;
			eFilterEnemy = (cMonsterClass.Boss ? CAbilityFilter.EFilterEnemy.Boss : ((cMonsterClass.NonEliteVariant == null) ? CAbilityFilter.EFilterEnemy.Normal : CAbilityFilter.EFilterEnemy.Elite));
			return FilterEnemyType.HasFlag(eFilterEnemy);
		}
		if (enemyState is PlayerState)
		{
			return false;
		}
		if (enemyState is HeroSummonState)
		{
			return false;
		}
		return true;
	}

	private bool CheckClass(ActorState actorState)
	{
		if (actorState is PlayerState && FilterPlayerClassIDs != null && FilterPlayerClassIDs.Count > 0)
		{
			return FilterPlayerClassIDs.Contains(actorState.ClassID);
		}
		if (actorState is ObjectState && FilterObjectClassIDs != null && FilterObjectClassIDs.Count > 0)
		{
			return FilterObjectClassIDs.Contains(actorState.ClassID);
		}
		if (actorState is EnemyState && FilterEnemyClassIDs != null && FilterEnemyClassIDs.Count > 0)
		{
			CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == actorState.ClassID);
			if (cMonsterClass != null)
			{
				if (!FilterEnemyClassIDs.Contains(cMonsterClass.ID))
				{
					return FilterEnemyClassIDs.Contains(actorState.ClassID);
				}
				return true;
			}
			return FilterEnemyClassIDs.Contains(actorState.ClassID);
		}
		if (actorState is ObjectState && FilterEnemyClassIDs != null && FilterEnemyClassIDs.Count > 0)
		{
			CMonsterClass cMonsterClass2 = MonsterClassManager.MonsterAndObjectClassesWithInitiative.SingleOrDefault((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == actorState.ClassID);
			if (cMonsterClass2 != null)
			{
				if (!FilterEnemyClassIDs.Contains(cMonsterClass2.ID))
				{
					return FilterEnemyClassIDs.Contains(actorState.ClassID);
				}
				return true;
			}
			return FilterEnemyClassIDs.Contains(actorState.ClassID);
		}
		if (actorState is HeroSummonState && FilterHeroSummonClassIDs != null && FilterHeroSummonClassIDs.Count > 0)
		{
			return FilterHeroSummonClassIDs.Contains(actorState.ClassID);
		}
		if ((FilterPlayerClassIDs != null && FilterPlayerClassIDs.Count > 0) || (FilterEnemyClassIDs != null && FilterEnemyClassIDs.Count > 0) || (FilterHeroSummonClassIDs != null && FilterHeroSummonClassIDs.Count > 0) || (FilterObjectClassIDs != null && FilterObjectClassIDs.Count > 0))
		{
			return false;
		}
		return true;
	}

	private bool CheckActorGUID(ActorState actorState)
	{
		if (actorState is ObjectState { IsAttachedToProp: not false } objectState)
		{
			if (FilterPropGUIDs != null && FilterPropGUIDs.Count > 0)
			{
				return FilterPropGUIDs.Contains(objectState.PropGuidAttachedTo);
			}
		}
		else if (FilterActorGUIDs != null && FilterActorGUIDs.Count > 0)
		{
			return FilterActorGUIDs.Contains(actorState.ActorGuid);
		}
		return true;
	}

	private bool CheckRevealedType(ActorState actorState)
	{
		if (FilterRevealedType == CAbilityFilter.EFilterRevealedType.None)
		{
			return true;
		}
		CAbilityFilter.EFilterRevealedType eFilterRevealedType = (actorState.IsRevealed ? CAbilityFilter.EFilterRevealedType.IsRevealed : CAbilityFilter.EFilterRevealedType.NotRevealed);
		return FilterRevealedType.HasFlag(eFilterRevealedType);
	}

	private bool CheckItems(ActorState actorState)
	{
		if (FilterItemIDs != null && FilterItemIDs.Count > 0)
		{
			if (actorState is PlayerState playerState)
			{
				return playerState.Items.Any((CItem i) => FilterItemIDs.Contains(i.YMLData.StringID));
			}
			return false;
		}
		return true;
	}

	private bool CheckConditions(ActorState actorState)
	{
		bool flag = FilterPositiveConditions == null;
		bool flag2 = FilterNegativeConditions == null;
		CActor actorInScene = ScenarioManager.FindActor(actorState.ActorGuid);
		if (actorInScene != null)
		{
			if (FilterPositiveConditions != null)
			{
				flag = FilterPositiveConditions.All((CCondition.EPositiveCondition a) => actorInScene.Tokens.HasKey(a));
			}
			if (FilterNegativeConditions != null)
			{
				flag2 = FilterNegativeConditions.All((CCondition.ENegativeCondition a) => actorInScene.Tokens.HasKey(a));
			}
		}
		else
		{
			if (FilterPositiveConditions != null)
			{
				flag = FilterPositiveConditions.All((CCondition.EPositiveCondition a) => actorState.PositiveConditions.Any((PositiveConditionPair b) => b.PositiveCondition == a));
			}
			if (FilterNegativeConditions != null)
			{
				flag2 = FilterNegativeConditions.All((CCondition.ENegativeCondition a) => actorState.NegativeConditions.Any((NegativeConditionPair b) => b.NegativeCondition == a));
			}
		}
		return flag2 & flag;
	}

	private bool CheckActiveBonusTypes(ActorState actorState)
	{
		if (FilterActiveBonusTypes != null && FilterActiveBonusTypes.Count > 0)
		{
			CActor cActor = ScenarioManager.FindActor(actorState.ActorGuid);
			if (cActor == null)
			{
				return false;
			}
			if (cActor.ActiveBonuses.Any((CActiveBonus b) => FilterActiveBonusTypes.Contains(b.Type().ToString())))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private bool CheckPersonalQuests(ActorState actorState)
	{
		if (FilterPersonalQuestIDs != null && FilterPersonalQuestIDs.Count > 0)
		{
			if (actorState is PlayerState)
			{
				CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor p) => p.ActorGuid == actorState.ActorGuid);
				if (cPlayerActor == null || string.IsNullOrEmpty(cPlayerActor.PersonalQuestID))
				{
					return false;
				}
				return FilterPersonalQuestIDs.Contains(cPlayerActor.PersonalQuestID);
			}
			return false;
		}
		return true;
	}

	private bool CheckCarryingQuestItem(ActorState actorState)
	{
		if (FilterCarryingQuestItem)
		{
			CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.ActorGuid == actorState.ActorGuid);
			if (cActor != null && !cActor.IsDeadForObjectives)
			{
				return cActor.CarriedQuestItems.Count > 0;
			}
			return false;
		}
		return true;
	}

	private bool CheckLootedGoalChest(ActorState actorState)
	{
		if (FilterLootedGoalChest)
		{
			return ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.IsLootable && p.ObjectType == ScenarioManager.ObjectImportType.GoalChest && IsValidLootTarget(p.ObjectType)).ToList()?.Any((CObjectProp x) => x.ActorActivated == actorState.ActorGuid) ?? false;
		}
		return true;
	}

	private bool CheckPropGUID(CObjectProp prop)
	{
		if (FilterPropGUIDs != null && FilterPropGUIDs.Count > 0)
		{
			return FilterPropGUIDs.Contains(prop.PropGuid);
		}
		return true;
	}
}
