using System;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHRL;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class TestingScenarioBriefingFlow : MonoBehaviour
{
	[Serializable]
	public struct TestCharacterControl
	{
		public ECharacter StartingCharacter;

		public GameObject SlotControlsContainer;

		public TMP_Dropdown Dropdown => SlotControlsContainer.GetComponentInChildren<TMP_Dropdown>();

		public Toggle Toggle => SlotControlsContainer.GetComponentInChildren<Toggle>();
	}

	private const string TEST_GOAL_INFO_SELECTING = "SELECTING";

	private const string TEST_GOAL_INFO_READY = "READY";

	public ScenarioBriefingPartyModule ScenarioBriefingPartyModule;

	public TestCharacterControl Slot1;

	public TestCharacterControl Slot2;

	public TestCharacterControl Slot3;

	public TestCharacterControl Slot4;

	private List<TestCharacterControl> _testingControls = new List<TestCharacterControl>();

	private List<GoalDTO> _goalDtos = new List<GoalDTO>();

	private List<ScenarioBriefingPartyModule.BriefingSlotData> _slotData = new List<ScenarioBriefingPartyModule.BriefingSlotData>();

	private void Start()
	{
		InitTestControls();
		InitCache();
		Refresh();
	}

	private void InitTestControls()
	{
		_testingControls = new List<TestCharacterControl> { Slot1, Slot2, Slot3, Slot4 };
		foreach (TestCharacterControl tc in _testingControls)
		{
			List<string> list = Enum.GetNames(typeof(ECharacter)).ToList();
			TMP_Dropdown dropdown = tc.Dropdown;
			Toggle toggle = tc.Toggle;
			dropdown.ClearOptions();
			dropdown.AddOptions(list);
			dropdown.value = list.FindIndex((string s) => s.Equals(tc.StartingCharacter.ToString()));
			dropdown.onValueChanged.AddListener((int _) =>
			{
				toggle.isOn = false;
				Refresh();
			});
			toggle.onValueChanged.AddListener((bool _) =>
			{
				Refresh();
			});
		}
	}

	private void InitCache()
	{
		_slotData.Clear();
		_goalDtos = YMLDefRepository.Repo.DefRepository.BattleGoals.Values.Select((BattleGoalDef def) => new GoalDTO(def, 1)).ToList();
		for (int num = 0; num < _testingControls.Count; num++)
		{
			ECharacter testingCharacter = GetTestingCharacter(num);
			PartySlotDTO partySlotDto = new PartySlotDTO
			{
				SlotId = num,
				Character = testingCharacter,
				ClassID = CrossReference.ECharacterModelToClassId(testingCharacter),
				Name = num.ToString(),
				Portrait = UIInfoTools.Instance.GetCharacterConfigUI(testingCharacter).illustration,
				Illustration = UIInfoTools.Instance.GetCharacterConfigUI(testingCharacter).illustration
			};
			List<GoalDTO> goalDtos = _goalDtos.Skip(3 * num).Take(3).ToList();
			_slotData.Add(new ScenarioBriefingPartyModule.BriefingSlotData
			{
				PartySlotDto = partySlotDto,
				SelectedGoalDto = null,
				GoalDtos = goalDtos,
				InfoLiteral = string.Empty
			});
		}
	}

	[ContextMenu("Refresh")]
	private void Refresh()
	{
		TryRefreshSlots();
		ScenarioBriefingPartyModule.ListParams listParams = new ScenarioBriefingPartyModule.ListParams
		{
			ShowGoalsList = true,
			ShowSelectedGoal = true,
			OnSlotClicked = null,
			SlotHintedActions = GetSlotInteractions,
			OnSelectedGoalClicked = OnSelectedGoalClicked,
			OnGoalListElementClicked = OnGoalListElementClicked
		};
		List<ScenarioBriefingPartyModule.BriefingSlotData> list = _slotData.Select((ScenarioBriefingPartyModule.BriefingSlotData sd) => sd.Clone()).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			if (_testingControls[num].Toggle.isOn)
			{
				ScenarioBriefingPartyModule.BriefingSlotData briefingSlotData = list[num];
				briefingSlotData.InfoLiteral = ((briefingSlotData.SelectedGoalDto != null) ? "READY" : "SELECTING");
				briefingSlotData.GoalDtos.Clear();
				briefingSlotData.SelectedGoalDto = null;
			}
		}
		ScenarioBriefingPartyModule.Init(listParams, list);
	}

	private void TryRefreshSlots()
	{
		IEnumerable<ECharacter> testCharacters = _testingControls.Select((TestCharacterControl tc, int i) => GetTestingCharacter(i));
		if (_slotData.Any((ScenarioBriefingPartyModule.BriefingSlotData s) => testCharacters.All((ECharacter tc) => tc != s.PartySlotDto.Character)) || testCharacters.Any((ECharacter tc) => _slotData.All((ScenarioBriefingPartyModule.BriefingSlotData s) => s.PartySlotDto.Character != tc)))
		{
			InitCache();
		}
	}

	private ECharacter GetTestingCharacter(int testingControlsIndex)
	{
		TMP_Dropdown dropdown = _testingControls[testingControlsIndex].Dropdown;
		if (!Enum.TryParse<ECharacter>(dropdown.options[dropdown.value].text, out var result))
		{
			return ECharacter.None;
		}
		return result;
	}

	private ScenarioBriefingPartyModule.BriefingSlotData GetSlotData(PartySlotDTO partySlotDto)
	{
		return _slotData.First((ScenarioBriefingPartyModule.BriefingSlotData sd) => sd.PartySlotDto == partySlotDto);
	}

	private IEnumerable<UIPartySlotHints.HintedActionParams> GetSlotInteractions(PartySlotDTO partySlot)
	{
		yield return new UIPartySlotHints.HintedActionParams
		{
			HintKey = null,
			HitLocalizedOverride = null,
			TooltipKey = null,
			TooltipLocalizedOverride = null,
			ActionIcon = null,
			ColorSchemeID = null,
			ActionCallback = () =>
			{
				OnSlotClicked(partySlot);
			},
			InteractionType = FHInputEventElement.UserInteractionType.LeftClick
		};
	}

	private void OnSlotClicked(PartySlotDTO partySlot)
	{
		partySlot.Name += 1;
		Refresh();
	}

	private void OnGoalListElementClicked(PartySlotDTO partySlotDto, GoalDTO goalDto)
	{
		ScenarioBriefingPartyModule.BriefingSlotData slotData = GetSlotData(partySlotDto);
		foreach (GoalDTO goalDto2 in slotData.GoalDtos)
		{
			bool isChecked = goalDto2 == goalDto;
			goalDto2.IsChecked = isChecked;
		}
		slotData.SelectedGoalDto = goalDto;
		Refresh();
	}

	private void OnSelectedGoalClicked(PartySlotDTO partySlotDto, GoalDTO goalDto)
	{
		ScenarioBriefingPartyModule.BriefingSlotData slotData = GetSlotData(partySlotDto);
		slotData.SelectedGoalDto.IsChecked = false;
		slotData.SelectedGoalDto = null;
		Refresh();
	}
}
