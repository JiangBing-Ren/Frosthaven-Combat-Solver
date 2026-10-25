using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using ScenarioRuleLibrary;

namespace FHGUI.Testing;

public class StartingGearState : TestState
{
	private CharacterState _characterState;

	private readonly UIItemElementInteractionEvents _heroInventoryElementInteractions = new UIItemElementInteractionEvents();

	private readonly UIItemInteractionEvents _equipmentElementInteractions = new UIItemInteractionEvents();

	private CharacterDetailsScreenModule CharacterDetailsScreenModule => Controller.GetModule<CharacterDetailsScreenModule>();

	private EquipmentViewModule EquipmentViewModule => Controller.GetModule<EquipmentViewModule>();

	private OutpostInventoryViewModule InventoryModule => Controller.GetModule<OutpostInventoryViewModule>();

	private ShopViewModule ShopViewModule => Controller.GetModule<ShopViewModule>();

	private CharacterPreviewModule CharacterPreviewModule => Controller.GetModule<CharacterPreviewModule>();

	private FHItemTooltip.ItemTooltipParams ShopTooltipParams(ItemDataDTO item)
	{
		return new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = Controller.GetFlipItem(item.ID.ID),
			Data = new FHItemTooltip.ItemTooltipData
			{
				Hints = GetTooltipHints(UIItemScreenFunctionality.Shop, item, _characterState).ToArray()
			}
		};
	}

	public StartingGearState(TestingCharacterCreationFlowViewData controller, CharacterState character)
		: base(controller)
	{
		_characterState = character;
		_heroInventoryElementInteractions.OnDoubleClick = (UIOutpostItemElement el) =>
		{
			DoubleClickItemInInventory(el.ItemID);
		};
		_equipmentElementInteractions.OnDoubleClick = (ItemDataDTO el) =>
		{
			DoubleClickItemInInventory(el.ID);
		};
	}

	public override void OnShow()
	{
		IOrderedEnumerable<ItemDataDTO> items = from dto in Controller.GetOwnedItems(_characterState)
			orderby dto.ID
			select dto;
		ECharacter character = CrossReference.ClassIdToECharacterModel(_characterState.ClassID.ID);
		OutpostInventoryViewModule.Params obj = new OutpostInventoryViewModule.Params
		{
			Items = items,
			ElementInteractionEvents = _heroInventoryElementInteractions,
			EnsureMinimumVisibleSlots = 0,
			FillFreeSpace = false,
			TintColor = UIInfoTools.Instance.GetCharacterColor(character),
			Icon = UIInfoTools.Instance.GetCharacterConfigUI(character.ToString()).iconClassGold
		};
		InventoryModule.Init(obj);
		ShopViewModule.Params obj2 = new ShopViewModule.Params
		{
			Items = Controller.GetPurchasableItems(_characterState),
			CaptionKey = "GUI_MERCHANT",
			TooltipData = ShopTooltipParams,
			HeaderColor = FHUIColors.MERCHANT_HEADER
		};
		ShopViewModule.Init(obj2);
		CharacterDataDTO characterDataDTO = TestingCharacterCreationFlowUtils.BuildCharacterDTO(Controller.DefRepo, _characterState);
		CharacterPreviewModule.Init(characterDataDTO, null, showValues: false);
		EquipmentViewModule.Init(Controller.GetEquippedItems(_characterState), _characterState.Level, Controller.GetMaxUnlockedSmallItems(_characterState), Controller.GetMaxSmallItems(_characterState), Controller.GetMaxOneHandSlots(_characterState), null, _equipmentElementInteractions);
		ButtonsBarParam[] buttonsBar = new ButtonsBarParam[2]
		{
			CharacterDetailsScreenModule.Button_Back(OnBack),
			CharacterDetailsScreenModule.Button_Complete(OnContinue)
		};
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(buttonsBar);
		CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(), 3);
		CharacterDetailsScreenModule.SetCharacterClass(characterDataDTO.Class);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.Equipment);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnBack()
	{
		Controller.PopState();
	}

	private void OnContinue()
	{
		Controller.PushState(new PartyCreationState(Controller));
	}

	private bool CharacterHasItems()
	{
		return false;
	}

	private void OnReset()
	{
		OnShow();
	}

	private void DoubleClickItemInInventory(ItemID itemID)
	{
	}

	public IEnumerable<FHItemTooltip.ItemTooltipHint> GetTooltipHints(UIItemScreenFunctionality screenFunctionality, ItemDataDTO itemData, CharacterState character)
	{
		yield return new FHItemTooltip.ItemTooltipHint
		{
			Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick) + " Buy"
		};
		yield return new FHItemTooltip.ItemTooltipHint
		{
			Text = "2x" + FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick) + " Buy & Equip"
		};
	}
}
