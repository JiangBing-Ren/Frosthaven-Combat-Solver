using System.Collections.Generic;
using FHGUI.Elements;
using FHRL;
using TMPro;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingCharacterCreationFlowDebugControls : MonoBehaviour
{
	[Header("Components")]
	[Space(10f)]
	[SerializeField]
	private TestingCharacterCreationFlowViewData m_Controller;

	[SerializeField]
	private TMP_Dropdown m_SlotSelector;

	[Header("Containers")]
	[Space(10f)]
	[SerializeField]
	private RectTransform m_StateButtonsContainer;

	private void Start()
	{
		PopulateControls();
	}

	private void PopulateControls()
	{
		ButtonsBarParam[] buttonParams = new ButtonsBarParam[5]
		{
			new ButtonsBarParam("@Party Creation", null, PreparePartyCreation),
			new ButtonsBarParam("@New Character", null, PrepareNewCharacter),
			new ButtonsBarParam("@Rename Character", null, PrepareRenameCharacter),
			new ButtonsBarParam("@Quest Selection", null, PrepareQuestSelection),
			new ButtonsBarParam("@Starting Gear", null, PrepareStaringGear)
		};
		ButtonsBarElement.SetButtons(m_StateButtonsContainer, buttonParams);
		if (m_SlotSelector != null)
		{
			m_SlotSelector.ClearOptions();
			List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>(4);
			for (int i = 0; i < 4; i++)
			{
				list.Add(new TMP_Dropdown.OptionData("Slot " + (i + 1)));
			}
			m_SlotSelector.AddOptions(list);
			m_SlotSelector.onValueChanged.AddListener(HandleOnSlotSelect);
		}
	}

	private void PreparePartyCreation()
	{
		m_Controller.SetState(new PartyCreationState(m_Controller));
	}

	private void PrepareNewCharacter()
	{
		m_Controller.SetState(new NewCharacterState(m_Controller, m_Controller.GetEmptyPartySlotId()));
	}

	private void PrepareRenameCharacter()
	{
		CharacterState characterState = m_Controller.EnsureCharacter();
		m_Controller.SetState(new RenameCharacterState(m_Controller, characterState));
	}

	private void PrepareQuestSelection()
	{
		CharacterState characterState = m_Controller.EnsureCharacter();
		List<PersonalQuestID> drawnPersonalQuests = m_Controller.GetPlayer(characterState).DrawnPersonalQuests;
		if (characterState.PersonalQuestDef != null && !drawnPersonalQuests.Contains(characterState.PersonalQuestDef.ID))
		{
			drawnPersonalQuests.Insert(0, characterState.PersonalQuestDef.ID);
		}
		PersonalQuestDef leftQuestDef = ((drawnPersonalQuests.Count > 0) ? m_Controller.GetQuestDef(drawnPersonalQuests[0]) : null);
		PersonalQuestDef rightQuestDef = ((drawnPersonalQuests.Count > 1) ? m_Controller.GetQuestDef(drawnPersonalQuests[1]) : null);
		m_Controller.SetState(new QuestSelectionState(m_Controller, characterState, leftQuestDef, rightQuestDef, creationProcess: false));
	}

	private void PrepareStaringGear()
	{
		CharacterState character = m_Controller.EnsureCharacter();
		int partySlotIndex = m_Controller.GetPartySlotIndex(character);
		m_Controller.SetSelectedCharacter(character, partySlotIndex + 1);
		m_Controller.SetState(new StartingGearState(m_Controller, character));
	}

	private void HandleOnSlotSelect(int optionIndex)
	{
	}
}
