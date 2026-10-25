using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.DTO;

public record CharacterPersonalQuestDTO
{
	public string Id;

	public readonly string Name;

	public readonly string Description;

	public readonly string GoalDescription;

	public readonly PersonalQuestDef Def;

	public readonly IEnumerable<PersonalQuestStepDTO> Steps;

	private static readonly ItemEquipType[] ItemCategoriesOrder = new ItemEquipType[6]
	{
		ItemEquipType.Head,
		ItemEquipType.Body,
		ItemEquipType.Feet,
		ItemEquipType.OneHandWeapon,
		ItemEquipType.TwoHandWeapon,
		ItemEquipType.Small
	};

	public CharacterPersonalQuestDTO()
	{
	}

	public CharacterPersonalQuestDTO(PersonalQuestDef personalQuestDef)
	{
		if (personalQuestDef == null)
		{
			Id = string.Empty;
			Name = string.Empty;
			Description = string.Empty;
			GoalDescription = string.Empty;
			Id = string.Empty;
			Steps = null;
		}
		else
		{
			Id = personalQuestDef.ID.ID;
			Name = (personalQuestDef.Name.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(personalQuestDef.Name));
			Description = (personalQuestDef.Description.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(personalQuestDef.Description));
			GoalDescription = (personalQuestDef.GoalDescription.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(personalQuestDef.GoalDescription));
			Def = personalQuestDef;
			Steps = null;
		}
	}

	public CharacterPersonalQuestDTO(PersonalQuestDef def, CampaignController campaignController)
		: this(def)
	{
		GoalDescription = GetPersonalGoalLocalized(def, campaignController);
	}

	public CharacterPersonalQuestDTO(CharacterState character, CampaignController campaignController)
		: this(character.PersonalQuestDef)
	{
		Steps = GetPersonalQuestSteps(character, campaignController);
		GoalDescription = GetPersonalGoalLocalized(character, campaignController);
	}

	private IEnumerable<PersonalQuestStepDTO> GetPersonalQuestSteps(CharacterState character, CampaignController campaignController)
	{
		PersonalQuestDef personalQuestDef = character.PersonalQuestDef;
		PersonalQuestState state = character.PersonalQuest;
		if (state == null)
		{
			yield break;
		}
		bool isComplete = personalQuestDef.IsComplete(state, character, campaignController);
		personalQuestDef.CalcProgress(state, out var max, character, campaignController);
		if (!(personalQuestDef is PQ_1))
		{
			uint lumberStep;
			uint lumberStepMax;
			uint scenario69;
			uint waitStepMax;
			uint wait3weeks;
			uint scenario70;
			if (!(personalQuestDef is PQ_2))
			{
				if (!(personalQuestDef is PQ_3 pq3))
				{
					if (!(personalQuestDef is PQ_4))
					{
						uint max2;
						if (!(personalQuestDef is PQ_7 pQ_))
						{
							if (!(personalQuestDef is PQ_8))
							{
								int cnt;
								if (!(personalQuestDef is PQ_11))
								{
									if (!(personalQuestDef is PQ_12 pQ_2))
									{
										if (!(personalQuestDef is PQ_14))
										{
											if (personalQuestDef is PQ_5 || personalQuestDef is PQ_6 || personalQuestDef is PQ_9 || personalQuestDef is PQ_10 || personalQuestDef is PQ_13 || personalQuestDef is PQ_15 || personalQuestDef is PQ_16 || personalQuestDef is PQ_17 || personalQuestDef is PQ_18 || personalQuestDef is PQ_19 || personalQuestDef is PQ_20 || personalQuestDef is PQ_21 || personalQuestDef is PQ_22 || personalQuestDef is PQ_23)
											{
												yield return CreateProgressStep(state.ProgressTrackers[0], max);
												yield break;
											}
											throw new ArgumentOutOfRangeException("def");
										}
										for (cnt = 0; cnt < state.ProgressTrackers.Length; cnt++)
										{
											uint current = state.ProgressTrackers[cnt];
											ScenarioLocation scenarioLocation = PQ_14.RequiredAreas[cnt];
											yield return CreateCheckboxStep(current, LocalizationManager.GetTranslation("GUI_Biome_" + scenarioLocation));
										}
										yield break;
									}
									pQ_2.CalcProgress(state, out max2, character, campaignController);
									bool scenario71Complete = GetScenarioCompleted(71u, campaignController);
									bool scenario72Unlocked = GetScenarioUnlocked(72u, campaignController, out var state2);
									bool scenario72Complete = GetScenarioCompleted(72u, campaignController);
									bool scenario73Unlocked = GetScenarioUnlocked(73u, campaignController, out state2);
									bool scenario73Complete = GetScenarioCompleted(73u, campaignController);
									bool scenario74Unlocked = GetScenarioUnlocked(74u, campaignController, out state2);
									bool scenario74Complete = GetScenarioCompleted(74u, campaignController);
									bool scenario75Unlocked = GetScenarioUnlocked(75u, campaignController, out state2);
									bool scenario75Complete = GetScenarioCompleted(75u, campaignController);
									bool scenario76Unlocked = GetScenarioUnlocked(76u, campaignController, out state2);
									bool scenario76Complete = GetScenarioCompleted(76u, campaignController);
									bool scenario77Unlocked = GetScenarioUnlocked(77u, campaignController, out state2);
									bool scenario77Complete = GetScenarioCompleted(77u, campaignController);
									yield return CreateStepDto(scenarioName(71u), scenario71Complete ? 1u : 0u, 1u, scenario71Complete, isCheckbox: true);
									if (scenario71Complete)
									{
										yield return CreateStepDto(LocalizationManager.GetTranslation("GUI_PQ12_GOAL_WAIT_FOR_149.5"), scenario72Unlocked ? 1u : 0u, 1u, scenario72Unlocked, isCheckbox: true);
									}
									if (scenario72Unlocked)
									{
										yield return CreateStepDto(scenarioName(72u), scenario72Complete ? 1u : 0u, 1u, scenario72Complete, isCheckbox: true);
									}
									if (scenario73Unlocked)
									{
										yield return CreateStepDto(scenarioName(73u), scenario73Complete ? 1u : 0u, 1u, scenario73Complete, isCheckbox: true);
									}
									if (scenario73Complete)
									{
										yield return CreateStepDto(LocalizationManager.GetTranslation("GUI_PQ12_GOAL_WAIT_FOR_146.1"), scenario74Unlocked ? 1u : 0u, 1u, scenario74Unlocked, isCheckbox: true);
									}
									if (scenario74Unlocked)
									{
										yield return CreateStepDto(scenarioName(74u), scenario74Complete ? 1u : 0u, 1u, scenario74Complete, isCheckbox: true);
									}
									if (scenario75Unlocked)
									{
										yield return CreateStepDto(scenarioName(75u), scenario75Complete ? 1u : 0u, 1u, scenario75Complete, isCheckbox: true);
									}
									if (scenario76Unlocked)
									{
										yield return CreateStepDto(scenarioName(76u), scenario76Complete ? 1u : 0u, 1u, scenario76Complete, isCheckbox: true);
									}
									if (scenario77Unlocked)
									{
										yield return CreateStepDto(scenarioName(77u), scenario77Complete ? 1u : 0u, 1u, scenario77Complete, isCheckbox: true);
									}
									yield break;
								}
								yield return CreateProgressStep(state.ProgressTrackers[0], max);
								List<ItemDef> list = (from d in campaignController.CharacterController.GetOwnedItems(character)
									where d.RequiredCraftingItemIDs.Count > 0
									orderby d.ID
									where ItemCategoriesOrder.Contains(d.Type)
									orderby Array.IndexOf(ItemCategoriesOrder, d.Type)
									select d).ToList();
								cnt = 0;
								foreach (ItemDef item in list)
								{
									yield return CreateIconStepDto(UIInfoTools.Instance.GetItemConfig(item.Icon).miniIcon, 1u, FHUILocFormatting.PopulateParams("GUI_PQ_11_STEP_DESC", "<b><color=#BBB0A3FF>" + LocalizationManager.GetTranslation(item.NameLocKey) + "</color></b>", $"<b><color=#BBB0A3FF>{item.RequiredCraftingItemIDs.Count}</color></b>"));
									cnt += item.RequiredCraftingItemIDs.Count;
									if (cnt >= max)
									{
										break;
									}
								}
							}
							else
							{
								yield return CreateProgressStep(state.ProgressTrackers[0], 2u, "GUI_PQ8_GOAL_1", overrideNameLoc: true);
								yield return CreateProgressStep(state.ProgressTrackers[1], 2u, "GUI_PQ8_GOAL_2", overrideNameLoc: true);
								yield return CreateProgressStep(state.ProgressTrackers[2], 2u, "GUI_PQ8_GOAL_3", overrideNameLoc: true);
							}
							yield break;
						}
						pQ_.CalcProgress(state, out max2, character, campaignController);
						scenario69 = state.ProgressTrackers[0];
						waitStepMax = 6u;
						wait3weeks = state.ProgressTrackers[1];
						scenario70 = state.ProgressTrackers[2];
						lumberStepMax = 6u;
						lumberStep = state.ProgressTrackers[3];
						uint scenario71 = state.ProgressTrackers[4];
						uint scenario72 = state.ProgressTrackers[5];
						yield return CreateCheckboxStep(scenario69, scenarioName(PQ_7.Scenarios[0]));
						if (!stepCompleted(scenario69, 1u))
						{
							yield break;
						}
						yield return CreateProgressStep(wait3weeks, waitStepMax, LocalizationManager.GetTranslation("GUI_PQ7_GOAL_WAIT_FOR_95.4"));
						if (!stepCompleted(wait3weeks, waitStepMax))
						{
							yield break;
						}
						yield return CreateCheckboxStep(scenario70, scenarioName(PQ_7.Scenarios[1]));
						if (!stepCompleted(scenario70, 1u))
						{
							yield break;
						}
						yield return CreateProgressStep(lumberStep, lumberStepMax, LocalizationManager.GetTranslation("GUI_PQ7_GOAL_WAIT_FOR_29.3"));
						if (stepCompleted(lumberStep, lumberStepMax))
						{
							yield return CreateCheckboxStep(scenario71, scenarioName(PQ_7.Scenarios[2]));
							if (stepCompleted(scenario71, 1u))
							{
								yield return CreateCheckboxStep(scenario72, scenarioName(PQ_7.Scenarios[3]));
							}
						}
					}
					else
					{
						uint current2 = state.ProgressTrackers[0];
						yield return new PersonalQuestStepDTO
						{
							Name = FHUILocFormatting.PersonalQuestProgressGold(current2, max),
							Current = current2,
							Max = max,
							IsComplete = isComplete
						};
					}
				}
				else
				{
					uint current3 = state.ProgressTrackers[0];
					scenario70 = state.ProgressTrackers[1];
					wait3weeks = state.ProgressTrackers[2];
					waitStepMax = state.ProgressTrackers[3];
					scenario69 = state.ProgressTrackers[4];
					yield return CreateProgressStep(current3, pq3.maxValues[0], FHUILocFormatting.PersonalQuestProgress("GUI_INVENTORY_SLOT_Head", current3, pq3.maxValues[0]));
					yield return CreateProgressStep(scenario70, pq3.maxValues[1], FHUILocFormatting.PersonalQuestProgress("GUI_INVENTORY_SLOT_Body", scenario70, pq3.maxValues[1]));
					yield return CreateProgressStep(wait3weeks, pq3.maxValues[2], FHUILocFormatting.PersonalQuestProgress("GUI_INVENTORY_SLOT_Legs", wait3weeks, pq3.maxValues[2]));
					yield return CreateProgressStep(waitStepMax, pq3.maxValues[3], FHUILocFormatting.PersonalQuestProgress("GUI_PQ3_GOAL_WEAPONS", waitStepMax, pq3.maxValues[3]));
					yield return CreateProgressStep(scenario69, pq3.maxValues[4], FHUILocFormatting.PersonalQuestProgress("GUI_INVENTORY_SLOT_SmallItems", scenario69, pq3.maxValues[4]));
				}
				yield break;
			}
			lumberStep = state.ProgressTrackers[0];
			lumberStepMax = 8u;
			scenario69 = state.ProgressTrackers[1];
			waitStepMax = 3u;
			wait3weeks = state.ProgressTrackers[2];
			scenario70 = state.ProgressTrackers[3];
			yield return CreateProgressStepTimes(lumberStep, lumberStepMax);
			if (!stepCompleted(lumberStep, lumberStepMax))
			{
				yield break;
			}
			yield return CreateCheckboxStep(scenario69, scenarioName(PQ_2.Scenarios[0]));
			if (stepCompleted(scenario69, 1u))
			{
				yield return CreateProgressStep(wait3weeks, waitStepMax, LocalizationManager.GetTranslation("GUI_PQ2_GOAL_WAIT_FOR_15.4"));
				if (stepCompleted(wait3weeks, waitStepMax))
				{
					yield return CreateCheckboxStep(scenario70, scenarioName(PQ_2.Scenarios[1]));
				}
			}
		}
		else
		{
			max = 5u;
			uint current4 = (uint)state.ProgressTrackers.Sum((uint u) => Mathf.Min(u, 1f));
			new string[6]
			{
				stepCompleted(state.ProgressTrackers[0], 1u) ? null : LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Arrowvine)),
				stepCompleted(state.ProgressTrackers[1], 1u) ? null : LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Axenut)),
				stepCompleted(state.ProgressTrackers[2], 1u) ? null : LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Corpsecap)),
				stepCompleted(state.ProgressTrackers[3], 1u) ? null : LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Flamefruit)),
				stepCompleted(state.ProgressTrackers[4], 1u) ? null : LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Rockroot)),
				stepCompleted(state.ProgressTrackers[5], 1u) ? null : LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Snowthistle))
			}.Where((string herb) => herb.IsNOTNullOrEmpty());
			yield return new PersonalQuestStepDTO
			{
				Name = FHUILocFormatting.PersonalQuestProgressTypes(current4, max),
				Current = current4,
				Max = max,
				IsComplete = isComplete
			};
			yield return CreateCheckboxStep(state.ProgressTrackers[0], LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Arrowvine)));
			yield return CreateCheckboxStep(state.ProgressTrackers[1], LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Axenut)));
			yield return CreateCheckboxStep(state.ProgressTrackers[2], LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Corpsecap)));
			yield return CreateCheckboxStep(state.ProgressTrackers[3], LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Flamefruit)));
			yield return CreateCheckboxStep(state.ProgressTrackers[4], LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Rockroot)));
			yield return CreateCheckboxStep(state.ProgressTrackers[5], LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(OutpostHerbType.Snowthistle)));
		}
		static string scenarioName(uint id)
		{
			return LocalizationManager.GetTranslation($"FS_{id}_NAME");
		}
		static bool stepCompleted(uint num, uint num2)
		{
			return num >= num2;
		}
	}

	private string GetPersonalGoalLocalized(CharacterState character, CampaignController campaignController)
	{
		PersonalQuestDef personalQuestDef = character.PersonalQuestDef;
		PersonalQuestState personalQuest = character.PersonalQuest;
		if (personalQuestDef is PQ_4)
		{
			personalQuestDef.CalcProgress(personalQuest, out var max, character, campaignController);
			if (!personalQuestDef.GoalDescription.IsNullOrEmpty())
			{
				return FHUILocFormatting.PopulateParams(personalQuestDef.GoalDescription, max);
			}
			return string.Empty;
		}
		if (!personalQuestDef.GoalDescription.IsNullOrEmpty())
		{
			return LocalizationManager.GetTranslation(personalQuestDef.GoalDescription);
		}
		return string.Empty;
	}

	private string GetPersonalGoalLocalized(PersonalQuestDef def, CampaignController campaignController)
	{
		if (def is PQ_4)
		{
			def.CalcProgress(null, out var max, null, campaignController);
			if (!def.GoalDescription.IsNullOrEmpty())
			{
				return FHUILocFormatting.PopulateParams(def.GoalDescription, max);
			}
			return string.Empty;
		}
		if (!def.GoalDescription.IsNullOrEmpty())
		{
			return LocalizationManager.GetTranslation(def.GoalDescription);
		}
		return string.Empty;
	}

	private bool GetScenarioCompleted(uint scenarioId, CampaignController campaignController)
	{
		return campaignController.State.UnlockedScenarios.SingleOrDefault((ScenarioState x) => x.Def.ID.ID == scenarioId)?.Completed ?? false;
	}

	private bool GetScenarioUnlocked(uint scenarioId, CampaignController campaignController, out ScenarioState state)
	{
		state = campaignController.State.UnlockedScenarios.SingleOrDefault((ScenarioState x) => x.Def.ID.ID == scenarioId);
		return state != null;
	}

	[DebuggerStepThrough]
	private PersonalQuestStepDTO CreateProgressStepTimes(uint current, uint max, string tooltip = null)
	{
		current = (uint)Mathf.Clamp(current, 0f, max);
		string name = FHUILocFormatting.PersonalQuestProgressTimes(current, max);
		return CreateProgressStep(current, max, name, overrideNameLoc: false, tooltip);
	}

	[DebuggerStepThrough]
	private PersonalQuestStepDTO CreateProgressStep(uint current, uint max, string name = null, bool overrideNameLoc = false, string tooltip = null)
	{
		bool isComplete = current >= max;
		current = (uint)Mathf.Clamp(current, 0f, max);
		if (name == null)
		{
			name = FHUILocFormatting.PersonalQuestProgress(current, max);
		}
		else if (overrideNameLoc)
		{
			name = FHUILocFormatting.PersonalQuestProgress(name, current, max);
		}
		return CreateStepDto(name, current, max, isComplete, isCheckbox: false, tooltip);
	}

	[DebuggerStepThrough]
	private PersonalQuestStepDTO CreateCheckboxStep(uint current, string name, uint max = 1u, string tooltip = null)
	{
		bool isComplete = current >= max;
		current = (uint)Mathf.Clamp(current, 0f, max);
		return CreateStepDto(name, current, max, isComplete, isCheckbox: true, tooltip);
	}

	private PersonalQuestStepDTO CreateIconStepDto(Sprite icon, uint current, string name, uint max = 1u, string tooltip = null)
	{
		bool isComplete = current >= max;
		current = (uint)Mathf.Clamp(current, 0f, max);
		return new PersonalQuestStepDTO
		{
			Name = name,
			Icon = icon,
			Current = current,
			Max = max,
			IsComplete = isComplete,
			Tooltip = tooltip
		};
	}

	private PersonalQuestStepDTO CreateStepDto(string name, uint current, uint max, bool isComplete, bool isCheckbox, string tooltip = null)
	{
		return new PersonalQuestStepDTO
		{
			Name = name,
			Current = current,
			Max = max,
			IsComplete = isComplete,
			Tooltip = tooltip,
			IsCheckbox = isCheckbox
		};
	}

	private IEnumerable<PersonalQuestStepDTO> GetQuestChainSteps(uint[] scenarios, Predicate<int> getScenarioCompleted)
	{
		bool flag = true;
		for (int i = 0; i < scenarios.Length; i++)
		{
			bool completed = getScenarioCompleted(i);
			if (!flag && !completed)
			{
				break;
			}
			yield return CreateStepDto(LocalizationManager.GetTranslation($"FS_{scenarios[i]}_NAME"), completed ? 1u : 0u, 1u, completed, isCheckbox: true);
			flag = completed;
		}
	}
}
