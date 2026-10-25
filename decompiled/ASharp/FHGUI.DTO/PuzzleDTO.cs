using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.DTO;

public sealed record PuzzleDTO
{
	public string Id;

	public string Name;

	public string Description;

	public string RewardText;

	public string RewardSectionID;

	public PuzzleDef Def;

	public bool IsCompleted;

	public bool IsLocked;

	public bool IsDisabled;

	public bool CanSolve;

	public PersonalQuestStepDTO[] Steps;

	public bool IsSelectable
	{
		get
		{
			if (!IsLocked)
			{
				return !IsDisabled;
			}
			return false;
		}
	}

	public IEnumerable<PersonalQuestStepDTO> VisibleSteps => Steps?.Where((PersonalQuestStepDTO step) => !step.IsComplete || step.ShowIfCompleted);

	private static readonly ItemEquipType[] ItemCategoriesOrder = new ItemEquipType[6]
	{
		ItemEquipType.Head,
		ItemEquipType.Body,
		ItemEquipType.Feet,
		ItemEquipType.OneHandWeapon,
		ItemEquipType.TwoHandWeapon,
		ItemEquipType.Small
	};

	public PuzzleDTO()
	{
	}

	public PuzzleDTO(PuzzleDef puzzleDef)
	{
		if (puzzleDef == null)
		{
			Id = string.Empty;
			Name = string.Empty;
			Description = string.Empty;
			RewardText = string.Empty;
			RewardSectionID = null;
			Id = string.Empty;
			Steps = Array.Empty<PersonalQuestStepDTO>();
		}
		else
		{
			Id = puzzleDef.ID.ID;
			Name = (puzzleDef.Name.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(puzzleDef.Name));
			Description = (puzzleDef.Description.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(puzzleDef.Description));
			RewardText = (puzzleDef.GoalDescription.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(puzzleDef.GoalDescription));
			RewardSectionID = string.Empty;
			Def = puzzleDef;
			Steps = Array.Empty<PersonalQuestStepDTO>();
		}
	}

	public PuzzleDTO(PuzzleDef def, CampaignController campaignController)
		: this(def)
	{
		Steps = GetPuzzleSteps(campaignController).ToArray();
		RewardText = GetPuzzleRewardText(campaignController);
		RewardSectionID = def.GetRewardSection(campaignController);
		PuzzleState puzzleState = campaignController.PuzzleController.GetPuzzleState(def);
		if (puzzleState != null)
		{
			IsCompleted = puzzleState.IsCompleted;
			CanSolve = !puzzleState.IsCompleted && campaignController.PuzzleController.CanSolvePuzzle(puzzleState);
			IsLocked = !campaignController.PuzzleController.UnlockedPuzzles.Contains(puzzleState);
			IsDisabled = campaignController.Settings.DisabledPuzzles.Contains(puzzleState.Def.ID.ID);
		}
	}

	public PuzzleDTO(PuzzleState puzzleState, CampaignController campaignController)
		: this(puzzleState.Def, campaignController)
	{
		IsCompleted = puzzleState.IsCompleted;
		CanSolve = !puzzleState.IsCompleted && campaignController.PuzzleController.CanSolvePuzzle(puzzleState);
		IsLocked = !campaignController.PuzzleController.UnlockedPuzzles.Contains(puzzleState);
		IsDisabled = campaignController.Settings.DisabledPuzzles.Contains(puzzleState.Def.ID.ID);
	}

	private IEnumerable<PersonalQuestStepDTO> GetPuzzleSteps(CampaignController campaignController)
	{
		PuzzleDef def = Def;
		if (campaignController?.State == null)
		{
			yield break;
		}
		PuzzleState puzzleState = campaignController?.PuzzleController.GetPuzzleState(def);
		OutpostEventsService outpostService = Singleton<FHMapChoreographer>.InstanceFast.View.GetService<OutpostEventsService>();
		bool anySteps = false;
		string puzzleName = LocalizationManager.GetTranslation(def.Name);
		foreach (PuzzleDef.PuzzleStep item in def.GetStepsProgress(campaignController))
		{
			PersonalQuestStepDTO personalQuestStepDTO = new PersonalQuestStepDTO
			{
				PQName = puzzleName,
				Name = LocalizationManager.GetTranslation(item.LocKey),
				ShowIfCompleted = item.ShowIfCompleted
			};
			if (item.LocArgs != null && item.LocArgs.Length != 0)
			{
				List<string> list = new List<string>();
				object[] locArgs = item.LocArgs;
				foreach (object obj in locArgs)
				{
					if (obj is ScenarioDef scenarioDef)
					{
						list.Add(outpostService.GetScenarioNameLocalized(scenarioDef.ID.ID, noStyle: true));
					}
					else if (obj is BuildingTypeDef buildingTypeDef)
					{
						list.Add(outpostService.GetBuildingNameLocalized(buildingTypeDef.ID.ID, noStyle: true));
					}
					else if (obj is PersonalQuestDef personalQuestDef)
					{
						list.Add(LocalizationManager.GetTranslation(personalQuestDef.Name));
					}
					else
					{
						list.Add(obj.ToString());
					}
				}
				string name = personalQuestStepDTO.Name;
				locArgs = list.ToArray();
				personalQuestStepDTO.Name = string.Format(name, locArgs);
			}
			if (!IsStepAvailable(def, item))
			{
				personalQuestStepDTO.Name = personalQuestStepDTO.Name + " (" + LocalizationManager.GetTranslation("EA_ComingSoon") + ")";
			}
			if (!string.IsNullOrEmpty(item.LocProgressKey))
			{
				uint num = item.Progress;
				if (item.ReverseProgress)
				{
					num = item.MaxProgress - num;
				}
				string progressDesc = string.Format(LocalizationManager.GetTranslation(item.LocProgressKey), num);
				personalQuestStepDTO.ProgressDesc = progressDesc;
			}
			else if (item.MaxProgress > 1 && item.ShowProgress)
			{
				uint num2 = item.Progress;
				if (item.ReverseProgress)
				{
					num2 = item.MaxProgress - num2;
				}
				personalQuestStepDTO.ProgressDesc = $"{num2}/{item.MaxProgress}";
			}
			personalQuestStepDTO.IsCheckbox = true;
			personalQuestStepDTO.Current = item.Progress;
			personalQuestStepDTO.Max = item.MaxProgress;
			personalQuestStepDTO.IsComplete = item.IsComplete;
			if (!item.IsComplete)
			{
				anySteps = true;
			}
			yield return personalQuestStepDTO;
		}
		if (!anySteps && !def.CanAutoSolve)
		{
			yield return new PersonalQuestStepDTO
			{
				PQName = LocalizationManager.GetTranslation(def.Name),
				Name = LocalizationManager.GetTranslation("GUI_PUZZLE_SOLVE_CURRENT_PUZZLE_GOAL"),
				IsCheckbox = true,
				IsComplete = (puzzleState?.IsCompleted ?? false)
			};
		}
	}

	private bool IsStepAvailable(PuzzleDef puzzle, PuzzleDef.PuzzleStep step)
	{
		if (ReleaseStage.IsEarlyAccess3OrAbove())
		{
			return true;
		}
		int num = PuzzleDef.PuzzleIDs.Order.IndexOf(puzzle.ID);
		int num2 = PuzzleDef.PuzzleIDs.Order.IndexOf(new PuzzleID("EnergySource"));
		if (num < num2)
		{
			return true;
		}
		if (step.LocArgs == null)
		{
			return true;
		}
		IEnumerable<ScenarioDef> source = step.LocArgs.OfType<ScenarioDef>();
		CampaignController campaignController = Singleton<FHMapChoreographer>.InstanceFast.GameController;
		return source.All((ScenarioDef sc) => campaignController.IsScenarioUnlockedAndValid(sc));
	}

	private string GetPuzzleRewardText(CampaignController campaignController)
	{
		PuzzleDef def = Def;
		PuzzleState puzzleState = campaignController.PuzzleController.GetPuzzleState(def);
		if (puzzleState == null || !puzzleState.IsCompleted)
		{
			return string.Empty;
		}
		string rewardSection = def.GetRewardSection(campaignController);
		if (!string.IsNullOrEmpty(rewardSection))
		{
			return LocalizationManager.GetTranslation("Event_" + rewardSection + "_Text");
		}
		return string.Empty;
	}

	private string GetPersonalGoalLocalized(PersonalQuestDef def, CampaignController campaignController)
	{
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

	public bool Equals(PuzzleDTO other)
	{
		if ((object)other != null && other.Def == Def && other.IsCompleted == IsCompleted)
		{
			return other.Id == Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, IsCompleted, IsLocked, CanSolve);
	}
}
