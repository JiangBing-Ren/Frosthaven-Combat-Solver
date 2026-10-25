using System.Linq;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.DTO;

public static class ItemDTOUtilities
{
	public static Sprite GetItemImage(this ItemDef itemDef)
	{
		string text = ((!itemDef.Icon.IsNullOrEmpty()) ? itemDef.Icon : GetSpriteKeyFromName(itemDef.Name));
		if (!text.IsNullOrEmpty())
		{
			return UIInfoTools.Instance.GetItemMiniSprite(text);
		}
		return UIInfoTools.Instance.GetItemMiniSprite("_item_placeholder");
	}

	public static Sprite GetItemImage(this ItemCardYMLData itemYml)
	{
		string text = ((!itemYml.Art.IsNullOrEmpty()) ? itemYml.Art : GetSpriteKeyFromName(itemYml.Name));
		if (!text.IsNullOrEmpty())
		{
			return UIInfoTools.Instance.GetItemMiniSprite(text);
		}
		return UIInfoTools.Instance.GetItemMiniSprite("_item_placeholder");
	}

	private static string GetSpriteKeyFromName(string name)
	{
		return name.Replace("'", "").Replace("-", " ");
	}

	public static Sprite GetItemTypeImage(this ItemDef itemDef)
	{
		if (itemDef.Icon.IsNullOrEmpty())
		{
			return UIInfoTools.Instance.GetItemMiniSprite("_item_placeholder");
		}
		return UIInfoTools.Instance.GetItemSlotIcon(CrossReference.EItemSlotToItemEquipType(itemDef.Type).ToString());
	}

	public static Sprite GetItemTypeImage(this ItemCardYMLData itemYml)
	{
		if (itemYml.Art.IsNullOrEmpty())
		{
			return UIInfoTools.Instance.GetItemMiniSprite("_item_placeholder");
		}
		return UIInfoTools.Instance.GetItemSlotIcon(itemYml.Slot.ToString());
	}

	public static string GetLocalizedCost(this ItemDataDTO dto)
	{
		return $"<sprite name=Gold> {dto.Cost}";
	}

	public static string GetLocalizedSellCost(this ItemDataDTO dto)
	{
		if (dto.SellGoldValue == 0)
		{
			return string.Empty;
		}
		return $"<sprite name=Gold> {dto.SellGoldValue}";
	}

	public static Sprite GetItemTypeImage(ItemEquipType type)
	{
		return UIInfoTools.Instance.GetItemSlotConfig(CrossReference.EItemSlotToItemEquipType(type).ToString())?.IconStylized;
	}

	public static Sprite GetItemTypeImageTintable(ItemEquipType type)
	{
		return UIInfoTools.Instance.GetItemSlotConfig(CrossReference.EItemSlotToItemEquipType(type).ToString())?.IconTintable;
	}

	public static string GetNameLocalized(ItemDef itemDef)
	{
		if (string.IsNullOrEmpty(itemDef.NameLocKey))
		{
			if (itemDef.Name.IsNullOrEmpty())
			{
				return $"[Item({itemDef.ID.ID})]";
			}
			return LocalizationManager.GetTranslation(itemDef.Name);
		}
		return LocalizationManager.GetTranslation(itemDef.NameLocKey);
	}

	public static string GetNameLocalized(ItemCardYMLData itemYml)
	{
		if (string.IsNullOrEmpty(itemYml.Name))
		{
			return $"[Item({itemYml.ID})]";
		}
		return LocalizationManager.GetTranslation(itemYml.Name);
	}

	public static string GetDescriptionKey(ItemCardYMLData itemYml)
	{
		if (!itemYml.Name.IsNullOrEmpty())
		{
			return itemYml.Name.Substring(0, itemYml.Name.Length - 1) + "2";
		}
		return string.Empty;
	}

	public static string GetDescriptionLocalized(ItemDef itemDef)
	{
		string text = itemDef.DescriptionLocKey ?? $"Empty DescKey [Item({itemDef.ID.ID})]";
		if (string.IsNullOrEmpty(text))
		{
			return $"Empty DescKey [Item({itemDef.ID.ID})]";
		}
		return LocalizationManager.GetTranslation(text, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
	}

	public static string GetDescriptionLocalized(ItemCardYMLData itemYml)
	{
		string descriptionKey = GetDescriptionKey(itemYml);
		if (string.IsNullOrEmpty(descriptionKey))
		{
			return $"Empty DescKey [Item({itemYml.ID})]";
		}
		return LocalizationManager.GetTranslation(descriptionKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
	}

	public static void GetFullItemInfo(CItem item, out ItemDataDTO itemData, out ItemDataDTO flipItem)
	{
		itemData = new ItemDataDTO(item);
		flipItem = GetFlipItem(itemData);
	}

	public static ItemDataDTO GetFlipItem(ItemDataDTO item)
	{
		if (!item.FlipScenarioItemID.HasValue)
		{
			return null;
		}
		int id = item.FlipScenarioItemID.Value;
		return new ItemDataDTO(ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData i) => i.ID == id));
	}
}
