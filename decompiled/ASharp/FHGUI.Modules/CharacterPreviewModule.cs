using FHGUI.DTO;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Modules;

public class CharacterPreviewModule : FHMapViewModule
{
	public enum DisplayAlignment
	{
		Center,
		Left,
		Right
	}

	[SerializeField]
	private UIPartySlot _characterSlot;

	[SerializeField]
	private UICharacterSummary m_ShortStats;

	protected override bool PlaySoundOnEnable => false;

	public UIPartySlot CharacterSlot => _characterSlot;

	public void Init(CharacterDataDTO characterData, UIPartySlotInteractionEvents interactionEvents = null, bool showValues = true, bool showName = true)
	{
		PartySlotDTO partySlotDTO = new PartySlotDTO(characterData);
		partySlotDTO.IsAvailable = partySlotDTO.Character != ECharacter.None;
		partySlotDTO.Name = (showName ? characterData.Name : string.Empty);
		_characterSlot.Init(partySlotDTO, null, null, interactionEvents);
		if (m_ShortStats != null)
		{
			m_ShortStats.Init(characterData);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		_characterSlot.Init(new PartySlotDTO());
	}
}
