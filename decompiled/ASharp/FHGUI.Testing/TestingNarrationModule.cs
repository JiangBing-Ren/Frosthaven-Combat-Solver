using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using FHGUI.Modules;
using TMPro;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingNarrationModule : MonoBehaviour
{
	[Header("Components")]
	public NarrationModalModule Module;

	public NarrativeDataSO DataSO;

	public TMP_Dropdown DropdownScenarios;

	private int _step;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		DropdownScenarios.onValueChanged.RemoveAllListeners();
		DropdownScenarios.ClearOptions();
		DropdownScenarios.AddOptions(DataSO.EventConfigs.Select((ScenarioNarrativeConfigUI c) => c.ScenarioId).ToList());
		DropdownScenarios.onValueChanged.AddListener(OnScenarioSelected);
		OnScenarioSelected(0);
	}

	private void OnScenarioSelected(int value)
	{
		_step = 0;
		UpdateElement();
	}

	private void UpdateElement()
	{
		NarrationModalModule.Params obj = new NarrationModalModule.Params
		{
			SuggestingUsers = null,
			Narration = GetNarrationSetting(),
			Step = _step,
			IsPartyLeader = true,
			IsOnline = false,
			Complete = false
		};
		Module.Init(obj);
	}

	private NarrationSetting GetNarrationSetting()
	{
		ScenarioNarrativeConfigUI scenarioNarrativeConfigUI = DataSO.EventConfigs[DropdownScenarios.value];
		IEnumerable<ScenarioNarrativeConfigUI.NarrativeConfig> source = scenarioNarrativeConfigUI.Intro.Concat(scenarioNarrativeConfigUI.Rooms).Concat(scenarioNarrativeConfigUI.Outro);
		return new NarrationSetting
		{
			Story = source.Select((ScenarioNarrativeConfigUI.NarrativeConfig nc) => new NarrationSetting.Setting
			{
				Image = nc.id,
				Text = nc.id,
				Voiceover = null,
				Options = new NarrationSetting.Setting.ChoiceOption[0]
			}).ToList()
		};
	}
}
