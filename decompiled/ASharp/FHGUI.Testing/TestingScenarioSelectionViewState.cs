using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class TestingScenarioSelectionViewState : MonoBehaviour
{
	[Range(2f, 4f)]
	public int PartyMembers = 2;

	public uint ScenarioId = 1u;

	[Space]
	public Toggle ToggleHasBoat;

	public Toggle ToggleHasSled;

	public Toggle ToggleHasClimbingGear;

	public Toggle ToggleIsCompleted;

	[Space]
	public Toggle ToggleIncludeStandard;

	public Toggle ToggleIncludeSecondFaction;

	public Toggle ToggleIncludeAllies;

	[Space]
	public ScenarioLauncherModule ScenarioLauncherModule;

	public ScenarioListModule ScenarioListModule;

	private ScenarioInfoDTO _selectedScenarioDto = new ScenarioInfoDTO();

	private List<ScenarioInfoDTO> _availableScenarios = new List<ScenarioInfoDTO>();

	private ScenarioRequirementsDto _requirementsDto = new ScenarioRequirementsDto();

	private void Awake()
	{
		ToggleHasBoat.onValueChanged.AddListener(OnToggleChanged);
		ToggleHasSled.onValueChanged.AddListener(OnToggleChanged);
		ToggleHasClimbingGear.onValueChanged.AddListener(OnToggleChanged);
		ToggleIsCompleted.onValueChanged.AddListener(OnToggleChanged);
		ToggleIncludeStandard.onValueChanged.AddListener(OnToggleChanged);
		ToggleIncludeSecondFaction.onValueChanged.AddListener(OnToggleChanged);
		ToggleIncludeAllies.onValueChanged.AddListener(OnToggleChanged);
	}

	private void OnToggleChanged(bool _)
	{
		OnShow();
	}

	private void Start()
	{
		ScenarioDef scenarioDef = YMLDefRepository.Repo.DefRepository.Scenarios[new ScenarioID(ScenarioId)];
		_selectedScenarioDto = new ScenarioInfoDTO(scenarioDef, PartyMembers, 1, isCompleted: false, isPlayable: false, isMandatory: false, scenarioDef.Levels[0]);
		_availableScenarios = YMLDefRepository.Repo.DefRepository.Scenarios.Values.Select((ScenarioDef def) => new ScenarioInfoDTO(def, PartyMembers, 1, isCompleted: false, isPlayable: false, isMandatory: false, def.Levels[0])).ToList();
		OnShow();
	}

	private bool ScenarioFilter(ScenarioInfoDTO scenarioDto)
	{
		bool flag = scenarioDto.MonsterDtos.Any((MonsterDataDto m) => m.IsSecondFaction);
		bool flag2 = scenarioDto.MonsterDtos.Any((MonsterDataDto m) => m.IsAlly);
		bool flag3 = !flag && !flag2;
		if (flag && !ToggleIncludeSecondFaction.isOn)
		{
			return false;
		}
		if (flag2 && !ToggleIncludeAllies.isOn)
		{
			return false;
		}
		if (flag3 && !ToggleIncludeStandard.isOn)
		{
			return false;
		}
		return true;
	}

	private void OnShow()
	{
		ReadToggles();
		List<ScenarioInfoDTO> availableScenarios = _availableScenarios.Where(ScenarioFilter).ToList();
		ButtonsBarParam startButtonParam = new ButtonsBarParam
		{
			LabelKey = "GUI_START"
		};
		ScenarioLauncherModule.Init(_selectedScenarioDto, _requirementsDto, startButtonParam, hasStartQuestAuthority: true);
		ScenarioListModule.Init(new ScenarioListModule.Params
		{
			selectedScenario = _selectedScenarioDto,
			availableScenarios = availableScenarios,
			onScenarioClicked = OnScenarioSelected
		});
	}

	private void ReadToggles()
	{
		_requirementsDto.MissingBoat = !ToggleHasBoat.isOn;
		_requirementsDto.MissingSled = !ToggleHasSled.isOn;
		_requirementsDto.MissingClimbingGear = !ToggleHasClimbingGear.isOn;
		_selectedScenarioDto.IsCompleted = ToggleIsCompleted.isOn;
	}

	private void OnScenarioSelected(ScenarioInfoDTO scenarioInfoDto)
	{
		_selectedScenarioDto = scenarioInfoDto;
		OnShow();
	}
}
