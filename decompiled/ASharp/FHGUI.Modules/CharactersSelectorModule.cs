using System.Collections.Generic;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Modules;

public class CharactersSelectorModule : FHMapViewModule
{
	public bool HideEmptySlots;

	[Header("Button")]
	protected List<UIPartySlot> _slots = new List<UIPartySlot>();

	private UIPartySlot.SlotEventHandler _onSlotClicked;

	protected UIPartySlot.SlotToHintedActions _hintedActions;

	protected UIPartySlotInteractionEvents _interactionEvents;

	protected override bool PlaySoundOnEnable => false;

	public IEnumerable<UIPartySlot> Slots()
	{
		foreach (UIPartySlot slot in _slots)
		{
			yield return slot;
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		ClearEvents();
	}

	private void InitCache()
	{
		if (_slots.Count <= 0)
		{
			_slots.Clear();
			_slots.AddRange(GetComponentsInChildren<UIPartySlot>(includeInactive: true));
		}
	}

	public void Init(IReadOnlyList<PartySlotDTO> dataList, UIPartySlot.SlotEventHandler onSlotClicked, UIPartySlot.SlotToHintedActions hintedActions = null, UIPartySlotInteractionEvents interactionEvents = null)
	{
		_onSlotClicked = onSlotClicked;
		_hintedActions = hintedActions;
		_interactionEvents = interactionEvents;
		SetCharacterSlotData(dataList);
	}

	private void SetCharacterSlotData(IReadOnlyList<PartySlotDTO> dataList)
	{
		InitCache();
		HelperTools.NormalizePool(ref _slots, _slots[0].gameObject, _slots[0].transform.parent, dataList.Count);
		foreach (PartySlotDTO data in dataList)
		{
			SetCharacterSlotData(data);
		}
	}

	protected virtual void SetCharacterSlotData(PartySlotDTO data)
	{
		UIPartySlot uIPartySlot = _slots[data.SlotId - 1];
		if (!uIPartySlot.gameObject.activeSelf)
		{
			Debug.LogError($"Setting data for {data.Character} on slot {data.SlotId} that is not active!", this);
		}
		uIPartySlot.Init(data, _onSlotClicked, _hintedActions?.Invoke(data), _interactionEvents);
		if (HideEmptySlots && data.Character == ECharacter.None)
		{
			uIPartySlot.gameObject.SetActive(value: false);
		}
	}

	public virtual void ClearEvents()
	{
		_onSlotClicked = null;
	}

	protected virtual void OnSlotClicked(UIPartySlot slot)
	{
		_onSlotClicked?.Invoke(slot);
	}
}
