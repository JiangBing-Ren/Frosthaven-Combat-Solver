using System;
using FHRL;
using GLOOM;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.PersonalQuests;

namespace FHGUI.DTO;

[Serializable]
public class GoalDTO
{
	public string Id;

	public bool IsChecked;

	public bool Highlihed;

	public string NameLocKey;

	public string DescriptionLocKey;

	public string GoalDescriptionLocKey;

	public string RewardLiteral;

	public string RewardValue;

	public string[] Params;

	public GoalDTO()
	{
	}

	public GoalDTO(BattleGoalDef battleGoalDef, int partySize)
	{
		Id = battleGoalDef.ID.ID;
		NameLocKey = battleGoalDef.Name;
		DescriptionLocKey = battleGoalDef.Description;
		GoalDescriptionLocKey = battleGoalDef.Description;
		uint num = battleGoalDef.PerkChecksReward;
		if (partySize == 4 && battleGoalDef.PerkChecksRewardFullyParty != 0)
		{
			num = battleGoalDef.PerkChecksRewardFullyParty;
		}
		RewardLiteral = FHUILocFormatting.PopulateParams("GUI_QUEST_REWARD_PerkCheck", num);
		RewardValue = num.ToString();
	}

	public GoalDTO(string masteryDescription)
	{
		DescriptionLocKey = masteryDescription;
	}

	public GoalDTO(MasteryDef masteryDef, bool isChecked = false)
	{
		Id = masteryDef.ID.ID;
		NameLocKey = masteryDef.Name;
		DescriptionLocKey = masteryDef.Description;
		GoalDescriptionLocKey = masteryDef.Description;
		RewardLiteral = FHUILocFormatting.PopulateParams("GUI_QUEST_REWARD_PerkPoints", masteryDef.PerkMarksReward);
		RewardValue = masteryDef.PerkMarksReward.ToString();
		IsChecked = isChecked;
	}

	public GoalDTO(PersonalQuestDef personalQuestDef, params string[] locParams)
	{
		Id = personalQuestDef.ID.ID;
		NameLocKey = personalQuestDef.Name;
		DescriptionLocKey = personalQuestDef.Description;
		GoalDescriptionLocKey = personalQuestDef.GoalDescription;
		Params = locParams;
	}

	public string GetName(bool ignoreLocalization)
	{
		if (ignoreLocalization)
		{
			return NameLocKey;
		}
		if (!string.IsNullOrEmpty(NameLocKey))
		{
			return LocalizationManager.GetTranslation(NameLocKey);
		}
		return string.Empty;
	}

	public string GetDescription(bool ignoreLocalization)
	{
		if (ignoreLocalization)
		{
			return DescriptionLocKey;
		}
		if (!string.IsNullOrEmpty(DescriptionLocKey))
		{
			return LocalizationManager.GetTranslation(DescriptionLocKey);
		}
		return string.Empty;
	}

	public string GetGoal(bool ignoreLocalization)
	{
		if (ignoreLocalization)
		{
			return GoalDescriptionLocKey;
		}
		if (string.IsNullOrEmpty(GoalDescriptionLocKey))
		{
			return string.Empty;
		}
		string goalDescriptionLocKey = GoalDescriptionLocKey;
		object[] paramsBody = Params;
		return FHUILocFormatting.PopulateParams(goalDescriptionLocKey, paramsBody);
	}

	public GoalDTO(CPersonalQuestState personalQuestState)
		: this(personalQuestState.CurrentPersonalQuestStepData)
	{
		IsChecked = personalQuestState.IsFinished;
	}

	public GoalDTO(PersonalQuestYMLData personalQuest)
	{
		Id = personalQuest.ID;
		NameLocKey = personalQuest.LocalisedName;
		DescriptionLocKey = personalQuest.LocalisedDescription;
		GoalDescriptionLocKey = personalQuest.LocalisedObjectiveDescription;
		IsChecked = false;
		RewardLiteral = string.Empty;
		RewardValue = string.Empty;
	}

	public GoalDTO(CBattleGoalState battleGoalState, int partySize)
		: this(battleGoalState.BattleGoal, partySize)
	{
		IsChecked = !battleGoalState.BattleGoalConditionState.Failed && battleGoalState.BattleGoalConditionState.TotalConditionsAndTargets == battleGoalState.BattleGoalConditionState.CurrentProgress;
	}

	public GoalDTO(BattleGoalYMLData battleGoal, int partySize)
	{
		Id = battleGoal.ID;
		NameLocKey = battleGoal.LocalisedName;
		DescriptionLocKey = battleGoal.LocalisedDescription;
		GoalDescriptionLocKey = battleGoal.LocalisedDescription;
		int? num = battleGoal.PerkChecks;
		if (partySize == 4 && battleGoal.PerkChecksFullParty > 0)
		{
			num = battleGoal.PerkChecksFullParty;
		}
		RewardLiteral = FHUILocFormatting.PopulateParams("GUI_QUEST_REWARD_PerkCheck", num);
		RewardValue = num.ToString();
	}
}
