using System.Collections.Generic;
using FHGUI;
using FHGUI.DTO;
using FHGUI.Elements;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIMultiplayerPartyAssignments : MonoBehaviour
{
	public class PartySlotEvent : UnityEvent<UIPartySlot>
	{
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private UIPartySlot m_CardPrototype;

	public UnityEvent OnDeselectedSlot = new UnityEvent();

	public PartySlotEvent OnSelectedSlot = new PartySlotEvent();

	private List<UIPartySlot> m_PartySlots = new List<UIPartySlot>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_PartySlots.AddRange(m_Container.GetComponentsInChildren<UIPartySlot>(includeInactive: true));
		}
	}

	public void Init(CMapCharacter[] mapPartySelectedCharactersArray)
	{
		InitCache();
		List<PartySlotDTO> slots = UIPartySlot.GetList(mapPartySelectedCharactersArray);
		UIHelperUtil.UpdateUIList(mapPartySelectedCharactersArray.Length, m_PartySlots, m_CardPrototype, m_Container, initElement);
		IEnumerable<UIPartySlotHints.HintedActionParams> GetInteractions(UIPartySlot slot, PartySlotDTO partyDTO, bool isLimited)
		{
			if (!isLimited)
			{
				yield return new UIPartySlotHints.HintedActionParams
				{
					ActionIcon = null,
					HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_MULTIPLAYER_SESSION_ASSIGN_PLAYER"),
					InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
					ActionCallback = () =>
					{
						OnSelectedSlot.Invoke(slot);
					}
				};
			}
		}
		void initElement(int index, UIPartySlot slot)
		{
			PartySlotDTO partySlotDTO = ((slots.Count > index) ? slots[index] : PartySlotDTO.Empty());
			bool flag = ScenarioManager.IsInTactical && partySlotDTO.Character == ECharacter.None;
			if (flag)
			{
				partySlotDTO.MultiplayerUserData = null;
			}
			slot.Init(partySlotDTO, null, GetInteractions(slot, partySlotDTO, flag));
		}
	}

	private void OnSlotClicked(UIPartySlot slot)
	{
		OnSelectedSlot.Invoke(slot);
	}
}
