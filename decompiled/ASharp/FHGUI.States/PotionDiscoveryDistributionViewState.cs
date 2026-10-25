using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class PotionDiscoveryDistributionViewState : FHMapViewState
{
	private ItemID m_PotionToDistribute;

	private OutpostResources m_Herbs;

	private CharacterID m_TargetCharacter;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private PotionDistributionModule PotionDistributionModule => View.Modules.Characters.PotionDistributionModule;

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	private ItemViewService ItemService => View.GetService<ItemViewService>();

	private AlchemistViewService AlchemistService => View.GetService<AlchemistViewService>();

	private CharacterDetailsViewService CharacterDetailsService => View.GetService<CharacterDetailsViewService>();

	public PotionDiscoveryDistributionViewState(ItemID potionId, OutpostResources sourceHerbs, CharacterID characterID)
	{
		m_PotionToDistribute = potionId;
		m_Herbs = sourceHerbs;
		m_TargetCharacter = characterID;
	}

	public override void OnShow()
	{
		if (!View.GameState.Outpost.Resources.Has(m_Herbs))
		{
			PopThisState();
			return;
		}
		CharacterState characterState = View.GameState.PartyCharacters.FirstOrDefault((CharacterState c) => c.ID == m_TargetCharacter);
		if (characterState == null)
		{
			characterState = View.GameState.PartyCharacters.FirstOrDefault((CharacterState state) => AlchemistService.CanBrew(state, m_Herbs));
		}
		if (characterState == null)
		{
			ShowNotification();
			PopThisState();
			return;
		}
		m_TargetCharacter = characterState.ID;
		List<PartySlotDTO> party = PartyService.GetPartySlotsData(View.SelectedCharacter);
		int i;
		for (i = 0; i < party.Count; i++)
		{
			CharacterState characterState2 = View.GameState.AllCharacters.FirstOrDefault((CharacterState c) => c.ID == party[i].CharacterID);
			if (characterState2 == null || !AlchemistService.CanBrew(characterState2, m_Herbs))
			{
				PartySlotDTO partySlotDTO = party[i];
				PartySlotDTO partySlotDTO2 = PartySlotDTO.Empty();
				partySlotDTO2.Owned = partySlotDTO.Owned;
				partySlotDTO2.SlotId = partySlotDTO.SlotId;
				party[i] = partySlotDTO2;
			}
		}
		if (party.Count == 0)
		{
			PopThisState();
			return;
		}
		party.ForEach((PartySlotDTO dto) =>
		{
			dto.IsSelected = dto.CharacterID == m_TargetCharacter;
		});
		CharacterDataDTO selectedCharacter = CharacterDetailsService.PopulateCharacterDTO(characterState);
		ItemDataDTO potion = ItemService.ItemDTOFromID(m_PotionToDistribute);
		bool flag = AlchemistService.CanResearch(m_PotionToDistribute, m_Herbs);
		bool flag2 = AlchemistService.CanBrew(characterState, m_Herbs);
		if (!flag && !flag2)
		{
			ShowNotification();
			PopThisState();
			return;
		}
		PotionDistributionModule.Params obj = new PotionDistributionModule.Params
		{
			Potion = potion,
			HeroSlots = party,
			OnCharacterSelected = (UIPartySlot slot) =>
			{
				m_TargetCharacter = (slot.HasCharacter ? slot.CurrentData.CharacterID : null);
				View.RefreshView();
			},
			SelectedCharacter = selectedCharacter,
			OnConfirm = OnPotionDistribution
		};
		PotionDistributionModule.Init(obj);
	}

	private void ShowNotification()
	{
		Singleton<UINotificationManager>.Instance.ShowNotification("BUILDING_Name_35", LocalizationManager.GetTranslation("GUI_CRAFTSMAN_ITEM_UNAVAILABLE"), UIInfoTools.Instance.defaultNotificationDuration, "ITEM_UNAVAILABLE", "GAMEPLAY_NOTIFICATIONS");
	}

	private void OnPotionDistribution()
	{
		PopThisState();
		CharacterState targetState = View.GameState.AllCharacters.FirstOrDefault((CharacterState state) => state.ID == m_TargetCharacter);
		AlchemistService.DiscoverPotion(View.SelectedCharacter, targetState, m_Herbs, null, ShowNotification);
	}
}
