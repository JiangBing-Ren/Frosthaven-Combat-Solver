using System;
using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Modules;

public class PartyColumnModule : CharactersSelectorModule
{
	public class PartyColumnParams
	{
		public List<PartySlotDTO> slotData;

		public UIPartySlot.SlotEventHandler OnSlotClick;

		public Action OnEditParty;

		public UIPartySlot.SlotToHintedActions HintedActions;
	}

	[Header("Buttons")]
	[SerializeField]
	private FHButton _editPartyButton;

	[SerializeField]
	private GameObject _hintsContainer;

	private Action _onEditPartyClick;

	private void Awake()
	{
		_editPartyButton.onClick.AddListener(EditPartyClick);
	}

	private void EditPartyClick()
	{
		_onEditPartyClick?.Invoke();
	}

	public void Init(PartyColumnParams @params)
	{
		Init(@params.slotData, @params.OnSlotClick, @params.HintedActions);
		_onEditPartyClick = @params.OnEditParty;
		_editPartyButton.gameObject.SetActive(_onEditPartyClick != null);
		_hintsContainer.SetActive(@params.OnSlotClick != null || @params.HintedActions != null);
	}
}
