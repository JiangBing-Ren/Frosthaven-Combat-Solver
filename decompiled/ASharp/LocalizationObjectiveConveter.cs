using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHRL;
using GLOOM;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public static class LocalizationObjectiveConveter
{
	private const string BRIEFING_PREFIX = " - ";

	private static readonly StringBuilder sb_objectives = new StringBuilder();

	public static string LocalizeText(this CObjective m_Objective, bool initialState = true)
	{
		if (m_Objective.LocKey != string.Empty)
		{
			int num = ((SaveData.Instance.Global.CurrentGameState == EGameState.Map) ? Mathf.Max(AdventureState.MapState.MapParty.SelectedCharacters.Count(), 1) : Mathf.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1));
			string text = LocalizationManager.GetTranslation(m_Objective.LocKey);
			switch (m_Objective.ObjectiveType)
			{
			case EObjectiveType.Scripted:
				if (m_Objective.CustomLocKeyArgs == null)
				{
					break;
				}
				try
				{
					object[] array = m_Objective.CustomLocKeyArgs?.Select((object arg) =>
					{
						object result = ((arg is string text3) ? ((!LocalizationManager.TryGetTranslation(text3, out var translation3)) ? text3 : translation3) : arg);
						return result;
					}).ToArray();
					text = string.Format(text, array ?? new object[0]);
				}
				catch (FormatException)
				{
					Log.Warning("Invalid CustomLocKeyArgs for objective.LocKey: " + m_Objective.LocKey);
					text = "INVALID CUSTOM_LOC_KEY_ARGS";
				}
				break;
			case EObjectiveType.ReachRound:
			{
				int num2 = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? ((m_Objective as CObjective_ReachRound).ReachRoundNumber - (ScenarioManager.CurrentScenarioState.RoundNumber - 1)) : (m_Objective as CObjective_ReachRound).ReachRoundNumber);
				text = string.Format(text, num2);
				break;
			}
			case EObjectiveType.XCharactersDie:
			{
				CObjective_XCharactersDie cObjective_XCharactersDie = m_Objective as CObjective_XCharactersDie;
				if (m_Objective.CustomLocKeyArgs != null && m_Objective.CustomLocKeyArgs.Count > 0)
				{
					try
					{
						object[] array2 = m_Objective.CustomLocKeyArgs?.Select((object arg) =>
						{
							object result = ((arg is string text3) ? ((!LocalizationManager.TryGetTranslation(text3, out var translation3)) ? text3 : translation3) : arg);
							return result;
						}).ToArray();
						return string.Format(text, array2 ?? new object[0]);
					}
					catch (FormatException)
					{
						Log.Warning("Invalid CustomLocKeyArgs for objective.LocKey: " + m_Objective.LocKey);
						return "INVALID CUSTOM_LOC_KEY_ARGS";
					}
				}
				if (m_Objective.ObjectiveFilter == null || !m_Objective.ObjectiveFilter.FilterHasValues)
				{
					text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XCharactersDie.KillAmount[num - 1] - cObjective_XCharactersDie.CurrentKills)) : string.Format(text, cObjective_XCharactersDie.KillAmount[num - 1]));
					break;
				}
				string actorStringsFromFilter2 = GetActorStringsFromFilter(m_Objective.ObjectiveFilter);
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XCharactersDie.KillAmount[num - 1] - cObjective_XCharactersDie.CurrentKills), actorStringsFromFilter2) : string.Format(text, cObjective_XCharactersDie.KillAmount[num - 1], actorStringsFromFilter2));
				break;
			}
			case EObjectiveType.LootX:
			{
				CObjective_LootX cObjective_LootX = m_Objective as CObjective_LootX;
				if (m_Objective.ObjectiveFilter != null && m_Objective.ObjectiveFilter.FilterHasValues && m_Objective.ObjectiveFilter.FilterLootType != CAbilityFilter.ELootType.None)
				{
					string translation2 = LocalizationManager.GetTranslation(m_Objective.ObjectiveFilter.FilterLootType.ToString());
					text = string.Format(text, cObjective_LootX.LootAmount[num - 1], translation2);
				}
				else
				{
					text = string.Format(text, cObjective_LootX.LootAmount[num - 1]);
				}
				break;
			}
			case EObjectiveType.DestroyXObjects:
			{
				CObjective_DestroyXObjects cObjective_DestroyXObjects = m_Objective as CObjective_DestroyXObjects;
				if (m_Objective.ObjectiveFilter != null && m_Objective.ObjectiveFilter.FilterHasValues)
				{
					string text2 = "";
					if (m_Objective.ObjectiveFilter.FilterObjectClassIDs != null)
					{
						for (int i = 0; i < m_Objective.ObjectiveFilter.FilterObjectClassIDs.Count; i++)
						{
							string objectClass = m_Objective.ObjectiveFilter.FilterObjectClassIDs[i];
							MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == objectClass);
							if (monsterYMLData != null)
							{
								string translation = LocalizationManager.GetTranslation(monsterYMLData.LocKey);
								text2 += translation;
								if (m_Objective.ObjectiveFilter.FilterObjectClassIDs.Count > 1 && i < m_Objective.ObjectiveFilter.FilterObjectClassIDs.Count - 1)
								{
									text2 += ", ";
								}
							}
							else
							{
								Log.GUIError("Unable to find Object class " + objectClass);
							}
						}
					}
					text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_DestroyXObjects.DestroyAmount[num - 1] - cObjective_DestroyXObjects.CurrentObjectsDestroyed), text2) : string.Format(text, cObjective_DestroyXObjects.DestroyAmount[num - 1], text2));
				}
				else
				{
					text = string.Format(text, cObjective_DestroyXObjects.DestroyAmount[num - 1]);
				}
				break;
			}
			case EObjectiveType.ActorReachPosition:
			{
				CObjective_ActorReachPosition cObjective_ActorReachPosition = m_Objective as CObjective_ActorReachPosition;
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, cObjective_ActorReachPosition.ReachPositionAmounts[num - 1] - cObjective_ActorReachPosition.CurrentReachedPosition) : string.Format(text, cObjective_ActorReachPosition.ReachPositionAmounts[num - 1]));
				break;
			}
			case EObjectiveType.AnyActorReachPosition:
			{
				CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition = m_Objective as CObjective_AnyActorReachPosition;
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, cObjective_AnyActorReachPosition.ReachPositionAmounts[num - 1] - cObjective_AnyActorReachPosition.CurrentReachedPosition) : string.Format(text, cObjective_AnyActorReachPosition.ReachPositionAmounts[num - 1]));
				break;
			}
			case EObjectiveType.ActivateXPressurePlates:
			{
				CObjective_ActivatePressurePlateX cObjective_ActivatePressurePlateX = m_Objective as CObjective_ActivatePressurePlateX;
				text = string.Format(text, cObjective_ActivatePressurePlateX.NumberToActivate[num - 1]);
				break;
			}
			case EObjectiveType.DeactivateXSpawners:
			{
				CObjective_DeactivateXSpawners cObjective_DeactivateXSpawners = m_Objective as CObjective_DeactivateXSpawners;
				text = string.Format(text, cObjective_DeactivateXSpawners.NumberToDeactivate[num - 1]);
				break;
			}
			case EObjectiveType.ActivateXSpawners:
			{
				CObjective_ActivateXSpawners cObjective_ActivateXSpawners = m_Objective as CObjective_ActivateXSpawners;
				text = string.Format(text, cObjective_ActivateXSpawners.NumberToActivate[num - 1]);
				break;
			}
			case EObjectiveType.ActorsEscaped:
			{
				CObjective_ActorsEscaped cObjective_ActorsEscaped = m_Objective as CObjective_ActorsEscaped;
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, cObjective_ActorsEscaped.TargetNumberOfEscapees[num - 1] - cObjective_ActorsEscaped.CurrentEscapedAmount) : string.Format(text, cObjective_ActorsEscaped.TargetNumberOfEscapees[num - 1]));
				break;
			}
			case EObjectiveType.BringItemsToArea:
			{
				CObjective_BringItemsToArea cObjective_BringItemsToArea = m_Objective as CObjective_BringItemsToArea;
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, cObjective_BringItemsToArea.TargetNumberOfItems[num - 1] - cObjective_BringItemsToArea.CurrentItemsAmount) : string.Format(text, cObjective_BringItemsToArea.TargetNumberOfItems[num - 1]));
				break;
			}
			case EObjectiveType.DealXDamage:
			{
				CObjective_DealXDamage cObjective_DealXDamage = m_Objective as CObjective_DealXDamage;
				if (m_Objective.ObjectiveFilter == null || !m_Objective.ObjectiveFilter.FilterHasValues)
				{
					text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_DealXDamage.TargetDamage[num - 1] - cObjective_DealXDamage.CurrentDamageDealt)) : string.Format(text, cObjective_DealXDamage.TargetDamage[num - 1]));
					break;
				}
				string actorStringsFromFilter3 = GetActorStringsFromFilter(m_Objective.ObjectiveFilter);
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_DealXDamage.TargetDamage[num - 1] - cObjective_DealXDamage.CurrentDamageDealt), actorStringsFromFilter3) : string.Format(text, cObjective_DealXDamage.TargetDamage[num - 1], actorStringsFromFilter3));
				break;
			}
			case EObjectiveType.XActorsHealToMax:
			{
				CObjective_XActorsHealToMax cObjective_XActorsHealToMax = m_Objective as CObjective_XActorsHealToMax;
				if (m_Objective.CustomLocKeyArgs != null)
				{
					text = string.Format(text, m_Objective.CustomLocKeyArgs.ToArray());
					break;
				}
				if (m_Objective.ObjectiveFilter == null || !m_Objective.ObjectiveFilter.FilterHasValues)
				{
					text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XActorsHealToMax.ActorAmount[num - 1] - cObjective_XActorsHealToMax.CurrentActorsFullHealth)) : string.Format(text, cObjective_XActorsHealToMax.ActorAmount[num - 1]));
					break;
				}
				string actorStringsFromFilter = GetActorStringsFromFilter(m_Objective.ObjectiveFilter);
				text = ((!((SaveData.Instance.Global.CurrentGameState == EGameState.Map) | initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XActorsHealToMax.ActorAmount[num - 1] - cObjective_XActorsHealToMax.CurrentActorsFullHealth), actorStringsFromFilter) : string.Format(text, cObjective_XActorsHealToMax.ActorAmount[num - 1], actorStringsFromFilter));
				break;
			}
			}
			return text;
		}
		return m_Objective.LocKey;
	}

	public static string GetAllObjectivesLocalizedText(ScenarioDef scenarioDef, int partySize)
	{
		sb_objectives.Clear();
		foreach (ScenarioDef.ScenarioObjective objective in scenarioDef.Levels[0].Objectives)
		{
			string localizedText = objective.GetLocalizedText(partySize);
			if (!localizedText.IsNullOrEmpty())
			{
				sb_objectives.Append(" - ");
				sb_objectives.AppendLine(localizedText);
			}
		}
		return sb_objectives.ToString();
	}

	public static string GetLocalizedText(this ScenarioDef.ScenarioObjective objective, int partySize)
	{
		if (objective.LocKey.IsNullOrEmpty())
		{
			return string.Empty;
		}
		string text = LocalizationManager.GetTranslation(objective.LocKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
		if (objective.CustomLocKeyArgs != null)
		{
			return string.Format(text, objective.CustomLocKeyArgs.ToArray());
		}
		string text2 = null;
		if (objective.LocFilterClassLocKeys != null)
		{
			text2 = "";
			for (int i = 0; i < objective.LocFilterClassLocKeys.Count; i++)
			{
				string translation = LocalizationManager.GetTranslation(objective.LocFilterClassLocKeys[i], fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
				if (!translation.IsNullOrEmpty())
				{
					text2 += translation;
					if (objective.LocFilterClassLocKeys.Count > 1 && i < objective.LocFilterClassLocKeys.Count - 1)
					{
						text2 += ", ";
					}
				}
			}
		}
		int num = objective.LocParamsByPartySize?.Count ?? 0;
		if (num > 0 && partySize > 0)
		{
			int index = Mathf.Min(num, partySize) - 1;
			int num2 = objective.LocParamsByPartySize[index];
			text = ((text2 != null) ? string.Format(text, num2, text2) : string.Format(text, num2));
		}
		return text;
	}

	public static ScenarioDef.ScenarioObjective ToBriefingScenarioObjective(CObjective objective)
	{
		List<string> locFilterClassLocKeys = null;
		List<int> locParamsByPartySize = null;
		switch (objective.ObjectiveType)
		{
		case EObjectiveType.ReachRound:
			locParamsByPartySize = new List<int> { ((CObjective_ReachRound)objective).ReachRoundNumber };
			break;
		case EObjectiveType.XCharactersDie:
			locParamsByPartySize = (objective as CObjective_XCharactersDie).KillAmount;
			locFilterClassLocKeys = GetActorLocKeysFromFilter(objective.ObjectiveFilter).ToList();
			break;
		case EObjectiveType.LootX:
			locParamsByPartySize = (objective as CObjective_LootX).LootAmount;
			if (objective.ObjectiveFilter != null && objective.ObjectiveFilter.FilterLootType != CAbilityFilter.ELootType.None)
			{
				locFilterClassLocKeys = new List<string> { objective.ObjectiveFilter.FilterLootType.ToString() };
			}
			break;
		case EObjectiveType.DestroyXObjects:
		{
			locParamsByPartySize = (objective as CObjective_DestroyXObjects).DestroyAmount;
			CObjectiveFilter objectiveFilter = objective.ObjectiveFilter;
			if (objectiveFilter == null || !objectiveFilter.FilterHasValues)
			{
				break;
			}
			locFilterClassLocKeys = LookupLocKeys(objective.ObjectiveFilter.FilterPlayerClassIDs, (string id) => ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == id)?.LocKey).ToList();
			break;
		}
		case EObjectiveType.ActorReachPosition:
			locParamsByPartySize = (objective as CObjective_ActorReachPosition).ReachPositionAmounts;
			break;
		case EObjectiveType.AnyActorReachPosition:
			locParamsByPartySize = (objective as CObjective_AnyActorReachPosition).ReachPositionAmounts;
			break;
		case EObjectiveType.ActivateXPressurePlates:
			locParamsByPartySize = (objective as CObjective_ActivatePressurePlateX).NumberToActivate;
			break;
		case EObjectiveType.DeactivateXSpawners:
			locParamsByPartySize = (objective as CObjective_DeactivateXSpawners).NumberToDeactivate;
			break;
		case EObjectiveType.ActivateXSpawners:
			locParamsByPartySize = (objective as CObjective_ActivateXSpawners).NumberToActivate;
			break;
		case EObjectiveType.ActorsEscaped:
			locParamsByPartySize = (objective as CObjective_ActorsEscaped).TargetNumberOfEscapees;
			break;
		case EObjectiveType.BringItemsToArea:
			locParamsByPartySize = (objective as CObjective_BringItemsToArea).TargetNumberOfItems;
			break;
		case EObjectiveType.DealXDamage:
			locParamsByPartySize = (objective as CObjective_DealXDamage).TargetDamage;
			locFilterClassLocKeys = GetActorLocKeysFromFilter(objective.ObjectiveFilter).ToList();
			break;
		case EObjectiveType.XActorsHealToMax:
			locParamsByPartySize = (objective as CObjective_XActorsHealToMax).ActorAmount;
			locFilterClassLocKeys = GetActorLocKeysFromFilter(objective.ObjectiveFilter).ToList();
			break;
		}
		return new ScenarioDef.ScenarioObjective
		{
			LocKey = objective.LocKey,
			LocParamsByPartySize = locParamsByPartySize,
			LocFilterClassLocKeys = locFilterClassLocKeys,
			CustomLocKeyArgs = objective.CustomLocKeyArgs
		};
	}

	private static IEnumerable<string> LookupLocKeys(List<string> ids, Func<string, string> locKeyFunc)
	{
		if (ids == null)
		{
			yield break;
		}
		foreach (string id in ids)
		{
			string text = locKeyFunc(id);
			if (!text.IsNullOrEmpty())
			{
				yield return text;
			}
			else
			{
				Log.GUIError("Unable to find locKey for item with id [" + id + "]");
			}
		}
	}

	public static IEnumerable<string> GetActorLocKeysFromFilter(CObjectiveFilter filter)
	{
		if (filter == null || !filter.FilterHasValues)
		{
			yield break;
		}
		List<string> filterPlayerClassIDs = filter.FilterPlayerClassIDs;
		if (filterPlayerClassIDs == null || filterPlayerClassIDs.Count <= 0)
		{
			filterPlayerClassIDs = filter.FilterEnemyClassIDs;
			if (filterPlayerClassIDs == null || filterPlayerClassIDs.Count <= 0)
			{
				filterPlayerClassIDs = filter.FilterHeroSummonClassIDs;
				if (filterPlayerClassIDs == null || filterPlayerClassIDs.Count <= 0)
				{
					if (filter.FilterEnemyType != CAbilityFilter.EFilterEnemy.None)
					{
						yield return filter.FilterEnemyType.ToString();
					}
					else if (filter.FilterActorType != CAbilityFilter.EFilterActorType.None)
					{
						yield return filter.ActorLocKey.IsNullOrEmpty() ? ("GUI_" + filter.FilterActorType) : filter.ActorLocKey;
					}
					yield break;
				}
			}
		}
		IEnumerable<string> first = LookupLocKeys(filter.FilterPlayerClassIDs, (string id) => ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == id)?.LocKey);
		IEnumerable<string> second = LookupLocKeys(filter.FilterEnemyClassIDs, (string id) => ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == id)?.LocKey);
		IEnumerable<string> second2 = LookupLocKeys(filter.FilterHeroSummonClassIDs, (string id) => ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == id)?.LocKey);
		foreach (string item in first.Concat(second).Concat(second2))
		{
			yield return item;
		}
	}

	public static string GetActorStringsFromFilter(CObjectiveFilter filter)
	{
		string text = "";
		if (filter.FilterActorGUIDs == null || filter.FilterActorGUIDs.Count <= 0)
		{
			if ((filter.FilterPlayerClassIDs != null && filter.FilterPlayerClassIDs.Count > 0) || (filter.FilterEnemyClassIDs != null && filter.FilterEnemyClassIDs.Count > 0) || (filter.FilterHeroSummonClassIDs != null && filter.FilterHeroSummonClassIDs.Count > 0))
			{
				if (filter.FilterPlayerClassIDs != null)
				{
					foreach (string playerClass in filter.FilterPlayerClassIDs)
					{
						CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == playerClass);
						if (characterYMLData != null)
						{
							string translation = LocalizationManager.GetTranslation(characterYMLData.LocKey);
							text += translation;
						}
						else
						{
							Log.GUIError("Unable to find player class " + playerClass);
						}
					}
				}
				if (filter.FilterEnemyClassIDs != null)
				{
					for (int num = 0; num < filter.FilterEnemyClassIDs.Count; num++)
					{
						string enemyClass = filter.FilterEnemyClassIDs[num];
						MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == enemyClass);
						if (monsterYMLData != null)
						{
							string translation2 = LocalizationManager.GetTranslation(monsterYMLData.LocKey);
							text += translation2;
							if (filter.FilterEnemyClassIDs.Count > 1 && num < filter.FilterEnemyClassIDs.Count - 1)
							{
								text += ", ";
							}
						}
						else
						{
							Log.GUIError("Unable to find monster class " + enemyClass);
						}
					}
				}
				if (filter.FilterHeroSummonClassIDs != null)
				{
					foreach (string summonClass in filter.FilterHeroSummonClassIDs)
					{
						HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonClass);
						if (heroSummonYMLData != null)
						{
							string translation3 = LocalizationManager.GetTranslation(heroSummonYMLData.LocKey);
							text += translation3;
						}
						else
						{
							Log.GUIError("Unable to find summon class " + summonClass);
						}
					}
				}
			}
			else if (filter.FilterEnemyType != CAbilityFilter.EFilterEnemy.None)
			{
				string translation4 = LocalizationManager.GetTranslation(filter.FilterEnemyType.ToString());
				text += translation4;
			}
			else if (filter.FilterActorType != CAbilityFilter.EFilterActorType.None)
			{
				string translation5 = LocalizationManager.GetTranslation(filter.ActorLocKey.IsNullOrEmpty() ? ("GUI_" + filter.FilterActorType) : filter.ActorLocKey);
				text += translation5;
			}
		}
		return text;
	}
}
