using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Modules;

public class PartyManagementModule : CharactersSelectorModule
{
	public class PartyManagementParams
	{
		public List<PartySlotDTO> slotData;

		public UIPartySlot.SlotToHintedActions HintedActions;

		public bool isOffset;

		public UIPartySlotInteractionEvents SlotInteractionEvents;
	}

	public Vector2 Offset = new Vector2(0f, 60f);

	public RectTransform SlotsContainer;

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
		}
	}

	public void Init(PartyManagementParams partyParams)
	{
		InitCache();
		Init(partyParams.slotData, null, partyParams?.HintedActions, partyParams?.SlotInteractionEvents);
		SlotsContainer.anchoredPosition = (partyParams.isOffset ? Offset : Vector2.zero);
	}
}
