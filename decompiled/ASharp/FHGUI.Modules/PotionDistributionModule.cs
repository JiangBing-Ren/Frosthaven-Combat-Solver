using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules.Inventory;
using UnityEngine;

namespace FHGUI.Modules;

public class PotionDistributionModule : FHMapViewModule
{
	public struct Params
	{
		public ItemDataDTO Potion;

		public List<PartySlotDTO> HeroSlots;

		public CharacterDataDTO SelectedCharacter;

		public UIPartySlot.SlotEventHandler OnCharacterSelected;

		public Action OnConfirm;
	}

	[Header("Components")]
	[SerializeField]
	private ItemTooltipCommonData m_ItemTooltip;

	[SerializeField]
	private TextLocalizedListener m_Description;

	[SerializeField]
	private CharactersSelectorModule m_CharactersSelectorModule;

	[SerializeField]
	private FHButton m_ButtonConfirm;

	[Header("Hero Equipment")]
	[SerializeField]
	private RectTransform m_EquippedPotionsContainer;

	[SerializeField]
	private RectTransform m_OwnedPotionsContainer;

	[SerializeField]
	private UIOutpostItemElement m_ItemPrototype;

	[SerializeField]
	private GameObject m_Separator;

	[Header("Events")]
	[SerializeField]
	private RectTransform m_TooltipPos;

	[SerializeField]
	private TextAnchor m_TooltipAnchor;

	private List<UIOutpostItemElement> m_InventoryItems = new List<UIOutpostItemElement>();

	private List<UIOutpostItemElement> m_OwnedItems = new List<UIOutpostItemElement>();

	private bool m_Initialized;

	public Params Data { get; set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if ((bool)m_ButtonConfirm)
			{
				m_ButtonConfirm.onClick.AddListener(HandleOnConfirm);
			}
			m_InventoryItems.AddRange(m_EquippedPotionsContainer.GetComponentsInChildren<UIOutpostItemElement>());
			m_OwnedItems.AddRange(m_OwnedPotionsContainer.GetComponentsInChildren<UIOutpostItemElement>());
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		if (m_ItemTooltip != null)
		{
			FHItemTooltip.ItemTooltipParams itemTooltipParams = new FHItemTooltip.ItemTooltipParams(Data.Potion);
			m_ItemTooltip.ShowImmediate(itemTooltipParams);
		}
		if (m_CharactersSelectorModule != null)
		{
			m_CharactersSelectorModule.Init(Data.HeroSlots, Data.OnCharacterSelected);
			m_CharactersSelectorModule.gameObject.SetActive(value: true);
		}
		PopulateInventory();
	}

	private void PopulateInventory()
	{
		if (Data.SelectedCharacter == null)
		{
			UIHelperUtil.UpdateUIList(0, m_InventoryItems, m_ItemPrototype, m_EquippedPotionsContainer);
			UIHelperUtil.UpdateUIList(0, m_OwnedItems, m_ItemPrototype, m_OwnedPotionsContainer);
			return;
		}
		List<ItemDataDTO> equippedPotions = Data.SelectedCharacter.Items?.Where((ItemDataDTO dto) => dto.IsPotion && dto.IsEquipped).ToList();
		int num = (int)(Data.SelectedCharacter.UnlockedSmallItemCount - equippedPotions?.Count).GetValueOrDefault();
		List<ItemDataDTO> ownedPotions = Data.SelectedCharacter.Items?.Where((ItemDataDTO dto) => dto.IsPotion && !dto.IsEquipped).ToList();
		TooltipPositionParams ttParams = new TooltipPositionParams
		{
			TransformAnchor = m_TooltipPos,
			Anchor = m_TooltipAnchor
		};
		UIHelperUtil.UpdateUIList(equippedPotions.Count + num, m_InventoryItems, m_ItemPrototype, m_EquippedPotionsContainer, setItemData);
		UIHelperUtil.UpdateUIList(ownedPotions.Count, m_OwnedItems, m_ItemPrototype, m_OwnedPotionsContainer, setOwnedItemData);
		if ((bool)m_Separator)
		{
			m_Separator.SetActive(equippedPotions.Count + num > 0 && ownedPotions.Count > 0);
		}
		void setItemData(int i, UIOutpostItemElement item)
		{
			if (equippedPotions.Count > i)
			{
				item.SetItem(equippedPotions[i], null, new FHItemTooltip.ItemTooltipParams(equippedPotions[i]), ttParams);
			}
			else
			{
				item.SetEmpty();
			}
		}
		void setOwnedItemData(int i, UIOutpostItemElement item)
		{
			item.SetItem(ownedPotions[i], null, new FHItemTooltip.ItemTooltipParams(ownedPotions[i]), ttParams);
		}
	}

	private void HandleOnConfirm()
	{
		Data.OnConfirm?.Invoke();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
