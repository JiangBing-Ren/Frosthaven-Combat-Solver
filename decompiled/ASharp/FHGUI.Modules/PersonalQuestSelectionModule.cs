using System;
using FHGUI.DTO;
using FHRL;
using UnityEngine;

namespace FHGUI.Modules;

public class PersonalQuestSelectionModule : FHMapViewModule
{
	public UIPersonalQuestCard QuestLeft;

	public UIPersonalQuestCard QuestRight;

	[Space]
	public FHToggle ToggleYes;

	private Action<PersonalQuestDef> _onQuestConfirmed;

	private Action<bool> _onVisibilityOptionChanged;

	private void Awake()
	{
		ToggleYes.onValueChanged.AddListener(OnQuestVisibilityEnabledValueChange);
	}

	public void Init(PersonalQuestDef selectedQuest, CharacterPersonalQuestDTO questLeft, CharacterPersonalQuestDTO questRight, Action<PersonalQuestDef> onQuestSelected, Action<bool> onVisibilityOptionChanged)
	{
		_onQuestConfirmed = null;
		_onVisibilityOptionChanged = null;
		InitCardDef(QuestLeft, questLeft, questLeft?.Def == selectedQuest);
		InitCardDef(QuestRight, questRight, questRight?.Def == selectedQuest);
		if (selectedQuest == null)
		{
			ToggleYes.isOn = false;
		}
		_onQuestConfirmed = onQuestSelected;
		_onVisibilityOptionChanged = onVisibilityOptionChanged;
	}

	private void InitCardDef(UIPersonalQuestCard questCard, CharacterPersonalQuestDTO personalQuest, bool isSelected)
	{
		if (personalQuest == null)
		{
			questCard.gameObject.SetActive(value: false);
			return;
		}
		questCard.gameObject.SetActive(value: true);
		questCard.Init(personalQuest, () =>
		{
			OnQuestSelected(personalQuest.Def);
		});
		questCard.ToggleHighlight(isSelected);
	}

	private void OnQuestVisibilityEnabledValueChange(bool isOn)
	{
		_onVisibilityOptionChanged?.Invoke(isOn);
	}

	private void OnQuestSelected(PersonalQuestDef def)
	{
		_onQuestConfirmed?.Invoke(def);
	}
}
