using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestingPartyListSelector : MonoBehaviour
{
	public UI3DCharacterDisplayEventsChannel MainCharacterDisplayEventsChannel;

	public TMP_Dropdown IndexDropdown;

	public TMP_Dropdown CharacterDropdown;

	public Button InspectHeroButton;

	public Button BackButton;

	public CharactersSelectorModule PartyViewContainer;

	public CharactersSelectorModule PartyViewSmallContainer;

	public CharactersSelectorModule CharacterDetailsViewContainer;

	public List<ECharacter> TestCharacters = new List<ECharacter>();

	private void Awake()
	{
		List<string> options = CCharacterClass.CharacterNames.Select((ECharacter ec) => ec.ToString()).ToList();
		IndexDropdown.ClearOptions();
		IndexDropdown.AddOptions(new List<string> { "1", "2", "3", "4" });
		CharacterDropdown.ClearOptions();
		CharacterDropdown.AddOptions(options);
		CharacterDropdown.onValueChanged.AddListener(OnCharacterDropdownChanged);
		InspectHeroButton.onClick.AddListener(GoToCharacterDetails);
		BackButton.onClick.AddListener(GoToPartyView);
	}

	private void Start()
	{
		CharacterDropdown.SetValueWithoutNotify(GetCharacterClassIndex(TestCharacters[IndexDropdown.value]));
		GoToPartyView();
	}

	private List<PartySlotDTO> GetDisplayedSlotData()
	{
		return TestCharacters.Select((ECharacter character, int index) => PartyViewService.FromClassType(index, character.ToString(), character, index == IndexDropdown.value)).ToList();
	}

	public void RefreshView()
	{
		if (PartyViewContainer.gameObject.activeInHierarchy)
		{
			GoToPartyView();
		}
		else
		{
			GoToCharacterDetails();
		}
	}

	private void GoToCharacterDetails()
	{
		PartyViewContainer.gameObject.SetActive(value: false);
		PartyViewSmallContainer.Init(GetDisplayedSlotData(), OnPartySlotClicked);
		PartyViewSmallContainer.gameObject.SetActive(value: true);
		List<PartySlotDTO> dataList = CCharacterClass.CharacterNames.Select((ECharacter character, int index) => PartyViewService.FromClassType(index, character.ToString(), character, CharacterDropdown.value == GetCharacterClassIndex(character))).ToList();
		CharacterDetailsViewContainer.Init(dataList, OnClassSlotClicked);
		CharacterDetailsViewContainer.gameObject.SetActive(value: true);
		ECharacter selectedCharacter = GetSelectedCharacter();
		MainCharacterDisplayEventsChannel.Display(this, selectedCharacter);
	}

	private void GoToPartyView()
	{
		PartyViewContainer.Init(GetDisplayedSlotData(), OnPartySlotClicked);
		PartyViewContainer.gameObject.SetActive(value: true);
		PartyViewSmallContainer.gameObject.SetActive(value: false);
		CharacterDetailsViewContainer.gameObject.SetActive(value: false);
		MainCharacterDisplayEventsChannel.Hide(this);
	}

	private void OnPartySlotClicked(UIPartySlot clickedSlot)
	{
		bool num = clickedSlot.CurrentData.SlotId == IndexDropdown.value || CharacterDetailsViewContainer.gameObject.activeInHierarchy;
		UpdateFromSlot(clickedSlot);
		if (num)
		{
			GoToCharacterDetails();
		}
		else
		{
			RefreshView();
		}
	}

	private void OnClassSlotClicked(UIPartySlot classSlot)
	{
		TestCharacters[IndexDropdown.value] = classSlot.CurrentData.Character;
		CharacterDropdown.SetValueWithoutNotify(GetCharacterClassIndex(classSlot.CurrentData.Character));
		RefreshView();
	}

	private void OnCharacterDropdownChanged(int value)
	{
		ECharacter value2 = CCharacterClass.CharacterNames[CharacterDropdown.value];
		TestCharacters[IndexDropdown.value] = value2;
		RefreshView();
	}

	private void UpdateFromSlot(UIPartySlot slot)
	{
		IndexDropdown.SetValueWithoutNotify(slot.CurrentData.SlotId);
		CharacterDropdown.SetValueWithoutNotify(GetCharacterClassIndex(slot.CurrentData.Character));
		TestCharacters[IndexDropdown.value] = slot.CurrentData.Character;
	}

	private ECharacter GetSelectedCharacter()
	{
		return CCharacterClass.CharacterNames[CharacterDropdown.value];
	}

	private int GetCharacterClassIndex(ECharacter character)
	{
		return CCharacterClass.CharacterNames.FindIndex((ECharacter charName) => charName == character);
	}
}
