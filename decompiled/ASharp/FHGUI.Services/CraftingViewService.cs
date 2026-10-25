using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.Services;

public class CraftingViewService : FHMapViewService
{
	private static StringBuilder sm_StringBuilder = new StringBuilder();

	public bool IsCraftingEnabled => _view.GameController.CraftingController.IsCraftingEnabled;

	public CraftingViewService(FHMapView view)
		: base(view)
	{
	}

	public List<ItemDataDTO> GetCraftableItems(CharacterState character, bool stacked = true)
	{
		List<ItemDef> craftables = GameController.CraftingController.CraftableItems.Where((ItemDef def) => def.CanBeCrafted).ToList();
		if (stacked)
		{
			return (from i in craftables
				group i by i.ID).Select((IGrouping<ItemID, ItemDef> i) =>
			{
				ItemDef itemDef = craftables.First((ItemDef ii) => ii.ID == i.Key);
				int quantity = i.Count();
				return new ItemDataDTO(itemDef, (int)itemDef.Quantity, showCost: true, isNew: false, itemDef.GoldCost <= character.Gold && !character.OwnedItems.Contains(itemDef), isEquipped: false, 0, CanCraftItem(character, itemDef.ID), character.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == itemDef.ID))
				{
					Quantity = (uint)quantity,
					OwnedInstance = character.OwnedItems.Contains(itemDef),
					CraftCost = GetItemCraftCost(itemDef, character),
					ResourceCost = new ResourceCostDTO(GetItemCraftCost(itemDef, character))
				};
			}).ToList();
		}
		return craftables.Select((ItemDef def) => FillItemCraftCost(def, character)).ToList();
	}

	public bool CanCraftItem(CharacterState character, ItemID itemID)
	{
		return CanCraftItem(character, DefRepo.Items[itemID]);
	}

	public bool CanCraftItem(CharacterState character, ItemDef itemDef)
	{
		if (_view.GameController.CharacterController.CanHaveItem(character, itemDef))
		{
			return GameController.CraftingController.CanCraftItem(character, itemDef);
		}
		return false;
	}

	public ItemDataDTO FillItemCraftCost(ItemID itemID, CharacterState character)
	{
		return FillItemCraftCost(DefRepo.Items[itemID], character);
	}

	public ItemDataDTO FillItemCraftCost(ItemDef itemDef, CharacterState character)
	{
		return new ItemDataDTO(itemDef)
		{
			CraftCost = GetItemCraftCost(itemDef, character)
		};
	}

	public ItemCraftCostDTO GetItemCraftCost(ItemID itemID, CharacterState character)
	{
		return GetItemCraftCost(DefRepo.Items[itemID], character);
	}

	public ItemCraftCostDTO GetItemCraftCost(ItemDef item, CharacterState character)
	{
		ItemCraftCostDTO itemCraftCostDTO = new ItemCraftCostDTO();
		itemCraftCostDTO.ResourceCost = item.CraftingCost;
		itemCraftCostDTO.CharacterResources = character.Resources;
		itemCraftCostDTO.OutpostResources = _view.GameState.Outpost.Resources;
		List<ItemDef> items = _view.GameController.CraftingController.GetOwnedRequiredItemsToCraft(character, item).ToList();
		itemCraftCostDTO.RequiredItems = ItemDataDTO.FromItemDef(items);
		itemCraftCostDTO.ResourceCost = _view.GameController.CraftingController.GetItemCraftingResourceCost(item, character);
		itemCraftCostDTO.GoldCost = (int)_view.GameController.CraftingController.GetItemCraftingGoldCost(item, character);
		itemCraftCostDTO.CharacterGold = (int)character.Gold;
		itemCraftCostDTO.CanAffordGoldCost = character.Gold >= (uint)itemCraftCostDTO.GoldCost;
		return itemCraftCostDTO;
	}

	public void CraftItem(CharacterState character, ItemDataDTO item)
	{
		CraftItem(character, DefRepo.Items[item.ID]);
	}

	public void CraftItem(CharacterState character, ItemDef item)
	{
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Crafting.CraftItem, character.ID, item, true, false);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.craftItemAudioItem);
	}

	public IEnumerable<string> GetCraftHintsLocalized(CharacterState character, ItemDataDTO itemDto)
	{
		ItemDef itemDef = DefRepo.Items[itemDto.ID];
		if (!_view.GameController.CharacterController.CanHaveItem(character, itemDef))
		{
			yield return FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_ITEM_OWNED"), FHUIColors.MISSING_RESOURCE);
		}
		if (!CanAffordCraftCost(character, itemDef))
		{
			yield return FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_CRAFTSMAN_MISSING_INGREDIENTS"), FHUIColors.MISSING_RESOURCE);
		}
	}

	public IEnumerable<FHItemTooltip.ItemTooltipHint> GetTooltipHints(ItemDataDTO itemData, CharacterState character)
	{
		yield return new FHItemTooltip.ItemTooltipHint
		{
			Text = FHUILocFormatting.GetActionIconFormatLiteral(FHInputEventElement.UserInteractionType.LeftClick, "GUI_SELECT_BLUEPRINT"),
			IsAction = true
		};
		yield return new FHItemTooltip.ItemTooltipHint
		{
			Text = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.MiddleClick, "GUI_INSPECT"),
			IsAction = true
		};
	}

	public FHItemTooltip.ItemTooltipCost GetTooltipCosts(ItemDataDTO itemData, CharacterState character)
	{
		return new FHItemTooltip.ItemTooltipCost
		{
			CostLiteral = CraftingCostToLocalizedString(character, DefRepo.Items[itemData.ID])
		};
	}

	public bool CanAffordCraftCost(CharacterState character, ItemDef itemDef)
	{
		return _view.GameController.CraftingController.CanAfford(character, itemDef);
	}

	public string CraftingCostToLocalizedString(CharacterState character, ItemDef itemDef)
	{
		CampaignController gameController = _view.GameController;
		List<ItemDef> list = gameController.CraftingController.GetOwnedRequiredItemsToCraft(character, itemDef).ToList();
		OutpostResources itemCraftingResourceCost = gameController.CraftingController.GetItemCraftingResourceCost(itemDef, character);
		int itemCraftingGoldCost = (int)_view.GameController.CraftingController.GetItemCraftingGoldCost(itemDef, character);
		string text = string.Empty;
		if (itemCraftingGoldCost > 0)
		{
			string text2 = FHUILocFormatting.Gold(itemCraftingGoldCost);
			if (character.Gold < itemCraftingGoldCost)
			{
				text2 = FHUILocFormatting.Color(text2, FHUIColors.MISSING_RESOURCE);
			}
			text += text2;
		}
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0 || text.Length > 0)
				{
					text += "\n";
				}
				text = text + "1 x " + ItemDTOUtilities.GetNameLocalized(list[i]);
			}
		}
		string text3 = LocalizeCost(itemCraftingResourceCost, character.Resources + _view.GameState.Outpost.Resources);
		if (text.Length > 0 && text3.Length > 0)
		{
			text += "\n";
		}
		return text + LocalizeCost(itemCraftingResourceCost, character.Resources + _view.GameState.Outpost.Resources);
	}

	public static string LocalizeCost(OutpostResources resources, OutpostResources available)
	{
		sm_StringBuilder.Clear();
		bool first = true;
		if (resources.Metal > 0)
		{
			AppendResource(resources.Metal, available.Metal, "Metal", null);
		}
		if (resources.Lumber > 0)
		{
			AppendResource(resources.Lumber, available.Lumber, "Lumber", null);
		}
		if (resources.Hide > 0)
		{
			AppendResource(resources.Hide, available.Hide, "Hide", null);
		}
		if (resources.Arrowvine > 0)
		{
			AppendResource(resources.Arrowvine, available.Arrowvine, "Arrowvine", null);
		}
		if (resources.Axenut > 0)
		{
			AppendResource(resources.Axenut, available.Axenut, "Axenut", null);
		}
		if (resources.Corpsecap > 0)
		{
			AppendResource(resources.Corpsecap, available.Corpsecap, "Corpsecap", null);
		}
		if (resources.Flamefruit > 0)
		{
			AppendResource(resources.Flamefruit, available.Flamefruit, "Flamefruit", null);
		}
		if (resources.Rockroot > 0)
		{
			AppendResource(resources.Rockroot, available.Rockroot, "Rockroot", null);
		}
		if (resources.Snowthistle > 0)
		{
			AppendResource(resources.Snowthistle, available.Snowthistle, "Snowthistle", null);
		}
		return sm_StringBuilder.ToString();
		void AppendResource(int count, int num, string sprite, string nameKey)
		{
			bool flag = num < count;
			if (!first)
			{
				sm_StringBuilder.Append(", ");
			}
			if (flag)
			{
				sm_StringBuilder.Append("<color=#" + ColorUtility.ToHtmlStringRGBA(FHUIColors.MISSING_RESOURCE) + ">");
			}
			sm_StringBuilder.Append(count);
			sm_StringBuilder.Append('x');
			if (!string.IsNullOrEmpty(sprite))
			{
				sm_StringBuilder.Append("<size=-2><sprite name=" + sprite + "></size>");
			}
			if (!string.IsNullOrEmpty(nameKey))
			{
				sm_StringBuilder.Append(LocalizationManager.GetTranslation(nameKey));
			}
			if (flag)
			{
				sm_StringBuilder.Append("</color>");
			}
			first = false;
		}
	}
}
