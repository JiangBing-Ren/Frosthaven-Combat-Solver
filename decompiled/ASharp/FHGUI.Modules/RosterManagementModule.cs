using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class RosterManagementModule : FHMapViewModule
{
	public delegate IEnumerable<UIPartySlotHints.HintedActionParams> ToHintedActions(PartySlotDTO characterData);

	public delegate UIPartySlotHints.HintedActionParams ToDeleteAction(PartySlotDTO characterData);

	public delegate UIPartySlotHints.HintedActionParams ToLevelUpAction(PartySlotDTO characterData);

	public class RosterParams
	{
		public List<PartySlotDTO> CharacterData;

		public ToHintedActions HintedActions;

		public ToDeleteAction DeleteAction;

		public ToLevelUpAction LevelUpAction;

		public bool FillEmptySlots;
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private RectTransform m_Viewport;

	[SerializeField]
	private UIPartySlot m_CardPrototype;

	private List<UIPartySlot> m_CharacterSlots = new List<UIPartySlot>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if (m_CardPrototype != null && m_CardPrototype.transform.parent == m_Container)
			{
				m_CharacterSlots.Add(m_CardPrototype);
			}
		}
	}

	public void Init(RosterParams rosterParams)
	{
		InitCache();
		UIHelperUtil.UpdateUIList(GetSlotCount(rosterParams), m_CharacterSlots, m_CardPrototype, m_Container, initElement);
		void initElement(int index, UIPartySlot card)
		{
			bool flag = rosterParams.CharacterData.Count > index;
			PartySlotDTO partySlotDTO = (flag ? rosterParams.CharacterData[index] : PartySlotDTO.Empty());
			card.Init(partySlotDTO, null, flag ? rosterParams.HintedActions(partySlotDTO) : null, null, flag ? rosterParams.DeleteAction(partySlotDTO) : null, (flag && partySlotDTO.HasLevelToBeGainedFromProsperity && !partySlotDTO.Retired && !partySlotDTO.CanRetire) ? rosterParams.LevelUpAction(partySlotDTO) : null);
		}
	}

	private int GetSlotCount(RosterParams rosterParams)
	{
		if (!rosterParams.FillEmptySlots)
		{
			return rosterParams.CharacterData.Count;
		}
		GridLayoutGroup component = m_Container.GetComponent<GridLayoutGroup>();
		if ((object)component == null)
		{
			return rosterParams.CharacterData.Count;
		}
		Rect rect = m_Viewport.rect;
		float num = rect.width - (float)component.padding.horizontal;
		float num2 = rect.height - (float)component.padding.vertical;
		Vector2 cellSize = component.cellSize;
		int num3 = (int)((num + component.spacing.x) / (cellSize.x + component.spacing.x));
		int num4 = (int)((num2 + component.spacing.y) / (cellSize.y + component.spacing.y));
		return Mathf.Max(num3 * num4, rosterParams.CharacterData.Count);
	}
}
