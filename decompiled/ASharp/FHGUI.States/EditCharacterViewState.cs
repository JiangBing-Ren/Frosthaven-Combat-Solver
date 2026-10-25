using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class EditCharacterViewState : FHMapViewState
{
	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly ItemViewService _itemService;

	private readonly HeaderViewService _headerService;

	private readonly QuickAccessBarViewService _quickAccessService;

	private readonly PartyViewService _partyService;

	private readonly UICardInteractionEvents _handCardInteractions = new UICardInteractionEvents();

	private readonly UIItemElementInteractionEvents _heroInventoryElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemInteractionEvents _equipmentElementInteractions = new UIItemInteractionEvents();

	private CharacterDataDTO _overrideCharacterDTO;

	private CharacterDataDTO _characterDTO;

	private readonly bool _readOnly;

	private readonly bool _dontRefresh;

	private FHTabButton _activeTab;

	private CategoryFilter _characterActiveFilters = CategoryFilter.All;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private MainHeaderModule MainHeader => View.Modules.MainHeaderModule;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private SimpleHeaderModule SimpleHeader => View.Modules.Characters.Header;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	[Preserve]
	private CharacterGoalsModule CharacterMasteries => View.Modules.Characters.CharacterMasteries;

	[Preserve]
	private CharacterPersonalQuestModule CharacterPersonalQuest => View.Modules.Characters.CharacterPersonalQuest;

	[Preserve]
	private HeroClassStatsModule HeroClassStatsModule => View.Modules.Characters.HeroClassStats;

	[Preserve]
	private CharacterBiographyModule CharacterBioModule => View.Modules.Characters.CharacterBio;

	[Preserve]
	private EquipmentViewModule EquipmentViewModule => View.Modules.Characters.EquipmentView;

	[Preserve]
	private HandCardListModule HandCardListModule => View.Modules.Characters.AbilitiesHandCard;

	[Preserve]
	private OutpostInventoryViewModule EquipmentInventoryView => View.Modules.Characters.EquipmentInventoryView;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	private FHItemTooltip.ItemTooltipParams InventoryTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = _itemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = _itemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, View.SelectedCharacter).ToArray(),
				Cost = _itemService.GetTooltipCost(UIItemScreenFunctionality.Inventory, item, View.SelectedCharacter)
			}
		};
	}

	private FHItemTooltip.ItemTooltipParams EquipmentTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = _itemService.GetFlipItem(item.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = _itemService.GetTooltipHints(item.IsEquipped ? UIItemScreenFunctionality.Unequip : UIItemScreenFunctionality.Equip, item, View.SelectedCharacter).ToArray(),
				Cost = _itemService.GetTooltipCost(UIItemScreenFunctionality.Inventory, item, View.SelectedCharacter)
			}
		};
	}

	public EditCharacterViewState(CharacterDataDTO overrideCharacterDto, bool readOnly = false, bool dontRefresh = false)
		: this(readOnly, dontRefresh)
	{
		_overrideCharacterDTO = overrideCharacterDto;
		_quickAccessService = View.GetService<QuickAccessBarViewService>();
	}

	public EditCharacterViewState(bool readOnly = false, bool dontRefresh = false)
	{
		_readOnly = readOnly;
		_dontRefresh = dontRefresh;
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_headerService = View.GetService<HeaderViewService>();
		_itemService = View.GetService<ItemViewService>();
		_partyService = View.GetService<PartyViewService>();
		_handCardInteractions.OnClick = OnHandCardClicked;
		_quickAccessService = View.GetService<QuickAccessBarViewService>();
		_heroInventoryElementInteractions.OnClick = (UIOutpostItemElement el) =>
		{
			ClickItemInInventory(el.ItemID);
		};
		_equipmentElementInteractions.OnClick = (ItemDataDTO el) =>
		{
			ClickItemInInventory(el.ID);
		};
	}

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		if (_overrideCharacterDTO == null)
		{
			if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
			{
				PopThisState();
				return;
			}
			if (!View.TryEnsureSelectedCharacter())
			{
				View.ResetView();
				return;
			}
		}
		if (_overrideCharacterDTO != null && !_dontRefresh && !_characterDetailsService.TryRefreshDTO(ref _overrideCharacterDTO))
		{
			PopThisState();
			return;
		}
		SavePayload();
		_characterDTO = _overrideCharacterDTO ?? _characterDetailsService.PopulateCharacterDTO(View.SelectedCharacter);
		bool flag = View.GameState.ActiveScenario != null;
		bool flag2 = !_readOnly && View.PlayerCharacters.Any((CharacterState state) => state.ID == _characterDTO.CharacterID);
		bool flag3 = _headerService.SetupCharacterDetailsHeader(MainHeader, SimpleHeader, "Hero Details");
		_partyService.SetupPartyColumn(PartyModule);
		CharacterDetailsScreenModule.ClearAll();
		if (flag)
		{
			PartyModule.gameObject.SetActive(value: false);
			_quickAccessService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.CharacterManagement);
			CharacterDetailsScreenModule.ShowCloseButton(PopThisState);
		}
		else if (flag3 || _readOnly)
		{
			CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(PopThisState));
			MainHeader.gameObject.SetActive(value: false);
			QuickAccessBar.gameObject.SetActive(value: false);
			PartyModule.gameObject.SetActive(value: false);
			SimpleHeader.SetFunctionality(CharacterDetailsScreenModule.Functionality.Info);
		}
		else
		{
			_quickAccessService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.CharacterManagement);
			_headerService.SetupHeader(MainHeader);
		}
		EnsureAbilityCards();
		_characterDetailsService.PopulateCharacterDetails(_characterDTO, false, CharacterPreviewModule, HeroClassStatsModule, CharacterBioModule, CharacterPersonalQuest);
		View.OutpostSceneController.OpenCharacterScreen(_characterDTO);
		CharacterMasteries.Init(_characterDTO.Masteries);
		if (flag2)
		{
			_characterDetailsService.SetFunctionality(View.SelectedCharacter, GetFunctionality(flag3, flag));
		}
		ECharacter character = CrossReference.ClassIdToECharacterModel(_characterDTO.Class.ID.ID);
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = _characterDTO.Items,
			TooltipParams = (flag2 ? new FHItemTooltip.ItemTooltipParamsFunc(InventoryTooltipParams) : null),
			ElementInteractionEvents = (flag2 ? _heroInventoryElementInteractions : null),
			OnFilterStateUpdated = OnCharacterFiltersUpdated,
			ActiveFilters = _characterActiveFilters,
			TintColor = UIInfoTools.Instance.GetCharacterColor(character),
			Icon = UIInfoTools.Instance.GetCharacterConfigUI(character.ToString()).iconClassGold
		};
		EquipmentInventoryView.Init(obj);
		EquipmentInventoryView.SetOffset(flag3 ? new Vector2(-112f, 0f) : Vector2.zero);
		EquipmentViewModule.Init(_characterDTO.GetEquippedItems(), _characterDTO.Level, _characterDTO.UnlockedSmallItemCount, _characterDTO.MaxSmallItems, _characterDTO.MaxOneHandSlots, EquipmentTooltipParams, flag2 ? _equipmentElementInteractions : null);
		List<CardDTO> cards = _characterDTO.Cards;
		List<CardDTO> cards2 = (_characterDTO.InCreation ? _characterDTO.Cards.ToList() : _characterDTO.Cards.Where((CardDTO dto) => dto.IsInHand).ToList());
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(CrossReference.ClassDefToECharacterModel(_characterDTO.Class));
		Color characterAltColor = UIInfoTools.Instance.GetCharacterAltColor(CrossReference.ClassDefToECharacterModel(_characterDTO.Class));
		bool isDualDeck = cards.Any((CardDTO c) => c.IsAltColor);
		HandCardListModule.Init(new CardListModuleBase.Params
		{
			Cards = cards2,
			InteractionEvents = _handCardInteractions,
			MinCount = (int)_characterDTO.Class.MaxCards
		}, characterColor, characterAltColor, isDualDeck);
		CharacterDetailsScreenModule.SetCharacterClass(_characterDTO.Class);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.Info);
		LoadPayload();
	}

	private void OnHandCardClicked(CardDTO card)
	{
	}

	public override void OnHide()
	{
		ClearPayload();
		CharacterDetailsScreenModule.ClearAll();
		_overrideCharacterDTO = null;
	}

	protected override void SavePayload()
	{
		_activeTab = CharacterDetailsScreenModule.InfoCategoriesActiveTab;
	}

	protected override void LoadPayload()
	{
		CharacterDetailsScreenModule.TrySetInfoCategoriesActiveTab(_activeTab);
	}

	protected override void ClearPayload()
	{
		_activeTab = null;
	}

	private CharacterDetailsScreenModule.Functionality GetFunctionality(bool isSimpleHeader, bool hasActiveScenario)
	{
		if (View.SelectedCharacter.PartyState == CharacterPartyStateType.CreationProcess)
		{
			return CharacterDetailsScreenModule.Functionality.CreationProcess;
		}
		if (hasActiveScenario)
		{
			return CharacterDetailsScreenModule.Functionality.Info;
		}
		CharacterDetailsScreenModule.Functionality functionality = CharacterDetailsScreenModule.Functionality.LevelUp;
		if (isSimpleHeader)
		{
			functionality |= CharacterDetailsScreenModule.Functionality.Equipment;
		}
		return functionality;
	}

	private void ClickItemInInventory(ItemID itemID)
	{
		if (!_readOnly && View.PlayerCharacters.Any((CharacterState state) => state.ID == _characterDTO.CharacterID))
		{
			if (View.SelectedCharacter.EquipedItemIDs.Contains(itemID))
			{
				_itemService.UnequipItem(itemID, View.SelectedCharacter);
			}
			else
			{
				_itemService.EquipItem(itemID, View.SelectedCharacter);
			}
		}
	}

	private void OnCharacterFiltersUpdated(CategoryFilter filter)
	{
		_characterActiveFilters = filter;
	}

	private void EnsureAbilityCards()
	{
	}
}
