using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class EnhancerViewState : FHMapViewState
{
	private struct SelectionData
	{
		public int CardId;

		public string AbilityName;

		public uint SlotIndex;

		public uint EnhancementLine;

		public string EnhancementID;

		public void Parse(UICardEnhancementElement.Params p)
		{
			CardId = p.card.Id;
			SlotIndex = p.slot.SlotIndex;
			EnhancementID = p.enhancement.ID;
			AbilityName = p.slot.AbilityName;
			EnhancementLine = p.slot.EnhancementLine;
		}

		public void Reset()
		{
			CardId = 0;
			SlotIndex = 0u;
			EnhancementLine = 0u;
			EnhancementID = null;
			AbilityName = null;
		}

		public bool Match(CardDTO card, List<CardEnhancementSlotDTO> data, out CardEnhancementSlotDTO slot, out string enhancementID)
		{
			slot = null;
			enhancementID = null;
			if (card.Id != CardId)
			{
				return false;
			}
			foreach (CardEnhancementSlotDTO datum in data)
			{
				if (datum.SlotIndex == SlotIndex && !(datum.AbilityName != AbilityName) && datum.EnhancementLine == EnhancementLine)
				{
					slot = datum;
					enhancementID = EnhancementID;
					return true;
				}
			}
			return false;
		}

		public void ParseDefault(CardDTO card, List<CardEnhancementSlotDTO> slotData, out CardEnhancementSlotDTO slot, out string enhancementID)
		{
			slot = slotData[0];
			CardEnhancementDTO cardEnhancementDTO = (slot.InUse ? slot.Enhancements.FirstOrDefault((CardEnhancementDTO dto) => dto.Sloted) : slot.Enhancements.First());
			enhancementID = cardEnhancementDTO?.ID;
			CardId = card.Id;
			SlotIndex = slot.SlotIndex;
			EnhancementID = cardEnhancementDTO?.ID;
			AbilityName = slot.AbilityName;
			EnhancementLine = slot.EnhancementLine;
		}
	}

	private readonly UICardInteractionEvents _deckCardInteractions = new UICardInteractionEvents();

	private readonly UIEnhancementInteractionEvents _enhancementInteractions = new UIEnhancementInteractionEvents();

	private readonly CharacterDetailsViewService _characterDetailsService;

	private CharacterDataDTO _characterDto;

	private SelectionData _selectionData;

	public override StateCategory Category => StateCategory.Shop;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private HandCardListModule CardListModule => View.Modules.Characters.EnchancableCardList;

	[Preserve]
	private EnhancementInventoryModule EnhancementsInventory => View.Modules.Characters.EnhancementsInventoryModule;

	[Preserve]
	private EnhancementPurchaseModule EnhancerPurchase => View.Modules.Characters.EnhancementPurchaseModule;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	private EnhancerViewService EnhancerViewService => View.GetService<EnhancerViewService>();

	private CardViewService CardViewService => View.GetService<CardViewService>();

	private BuildingViewService BuildingService => View.GetService<BuildingViewService>();

	public EnhancerViewState()
	{
		_deckCardInteractions.OnClick = OnDeckCardClicked;
		_enhancementInteractions.OnHover = EnhancementsInventory.HighlightEnhancement;
		_enhancementInteractions.OnClick = HandleOnSelectEnhancement;
	}

	public override void OnShow()
	{
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		EnsureAbilityCards();
		_characterDto = View.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(View.SelectedCharacter);
		HeaderService.SetupHeader(HeaderModule);
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, MainViewSubCategories.Enhancer);
		PartyService.SetupPartyColumn(PartyModule);
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		List<CardDTO> list = (from dto in _characterDto.Cards
			where EnhancerViewService.HasEnhancementSlots(dto)
			orderby !dto.IsInHand, dto.Initiative
			select dto).ToList();
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(CrossReference.ClassDefToECharacterModel(_characterDto.Class));
		Color characterAltColor = UIInfoTools.Instance.GetCharacterAltColor(CrossReference.ClassDefToECharacterModel(_characterDto.Class));
		bool isDualDeck = list.Any((CardDTO c) => c.IsAltColor);
		CardListModule.Init(new CardListModuleBase.Params
		{
			Cards = list,
			InteractionEvents = _deckCardInteractions,
			MinCount = list.Count
		}, characterColor, characterAltColor, isDualDeck);
		TrySelectPreviousOrDefault(list);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true, UIInfoTools.Instance.GetOutpostPanelIllustration("FH_UI_View_Enhancer_Background"));
	}

	private void TrySelectPreviousOrDefault(List<CardDTO> cards)
	{
		if (cards != null && cards.Count != 0)
		{
			int num = cards.FindIndex((CardDTO dto) => dto.Id == _selectionData.CardId);
			SelectCard(cards[(num != -1) ? num : 0]);
		}
	}

	private void SelectCard(CardDTO card)
	{
		List<CardEnhancementSlotDTO> cardEnhancements = EnhancerViewService.GetCardEnhancements(View.SelectedCharacter, card);
		UICardEnhancementElement.Params data;
		if (_selectionData.Match(card, cardEnhancements, out var slot, out var e))
		{
			EnhancementsInventory.SelectEnhancement(slot, e);
			data = new UICardEnhancementElement.Params
			{
				card = card,
				enhancement = slot.Enhancements.FirstOrDefault((CardEnhancementDTO dto) => dto.ID == e),
				slot = slot
			};
		}
		else
		{
			_selectionData.ParseDefault(card, cardEnhancements, out var slot2, out var enhancementID);
			EnhancementsInventory.SelectEnhancement(slot2, enhancementID);
			data = new UICardEnhancementElement.Params
			{
				card = card,
				enhancement = null,
				slot = null
			};
		}
		EnhancerPurchase.Init(data, HandleOnEnhancementPurchase);
		EnhancementsInventory.Init(card, BuildingService.GetBuildingLevelById(new BuildingTypeID(44u)), cardEnhancements, _enhancementInteractions);
		CardListModule.SelectCard(card);
	}

	private void HandleOnSelectEnhancement(UICardEnhancementElement.Params eParams)
	{
		EnhancerPurchase.Init(eParams, HandleOnEnhancementPurchase);
		_selectionData.Parse(eParams);
	}

	private void HandleOnEnhancementPurchase(UICardEnhancementElement.Params data)
	{
		if (!data.enhancement.Sloted && !data.slot.InUse && data.enhancement.Affordable)
		{
			EnhancerViewService.Enhance(View.SelectedCharacter, data.card, data.slot, data.enhancement);
		}
	}

	public override void OnHide()
	{
		_selectionData.Reset();
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnDeckCardClicked(CardDTO cardDto)
	{
		SelectCard(cardDto);
	}

	private void EnsureAbilityCards()
	{
	}
}
