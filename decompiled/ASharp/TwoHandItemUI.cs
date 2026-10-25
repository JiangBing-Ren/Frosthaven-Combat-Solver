using System;
using System.Collections;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class TwoHandItemUI : MonoBehaviour
{
	private const string ItemCardUIPrefab = "GUI/Items/ItemCard";

	public Image ItemImage;

	public Sprite StandardSprite;

	public Sprite DisabledSprite;

	public Sprite SelectedFirstSprite;

	public Sprite SelectedSecondSprite;

	public Sprite SelectedBothSprite;

	[NonSerialized]
	public ItemCardUI[] m_ItemCardsUI;

	private CItem.EItemSlot m_Slot;

	private bool m_Enabled;

	private bool m_IsFirstSelected;

	private bool m_IsSecondSelected;

	private bool m_IsBothSelected;

	private CInventory m_Inventory;

	private void Start()
	{
		ClearItems();
	}

	public void ClearItems()
	{
		m_Enabled = false;
		m_Slot = CItem.EItemSlot.None;
		m_IsFirstSelected = false;
		m_IsSecondSelected = false;
		m_IsBothSelected = false;
		ItemImage.sprite = DisabledSprite;
		m_Inventory = null;
		m_ItemCardsUI = new ItemCardUI[2];
	}

	public void SetItem(CItem item, CInventory inventory)
	{
		if (m_ItemCardsUI[0] == null || m_ItemCardsUI[1] == null)
		{
			m_Inventory = inventory;
			m_Slot = item.YMLData.Slot;
			ItemCardUI component = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, transform).GetComponent<ItemCardUI>();
			component.item = item;
			m_Enabled = true;
			ItemCardUI.SelectionChanged += OnSelectionChanged;
			component.gameObject.SetActive(value: false);
			if (m_ItemCardsUI[0] == null)
			{
				m_ItemCardsUI[0] = component;
				if (item.SlotState == CItem.EItemSlotState.Selected)
				{
					if (m_IsSecondSelected)
					{
						ItemImage.sprite = SelectedBothSprite;
					}
					else
					{
						ItemImage.sprite = SelectedFirstSprite;
					}
				}
				else if (m_IsSecondSelected)
				{
					ItemImage.sprite = SelectedSecondSprite;
				}
				else
				{
					ItemImage.sprite = StandardSprite;
				}
				return;
			}
			RectTransform rectTransform = component.transform as RectTransform;
			rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y - rectTransform.rect.height - 5f);
			m_ItemCardsUI[1] = component;
			if (item.SlotState == CItem.EItemSlotState.Selected)
			{
				if (m_IsFirstSelected)
				{
					ItemImage.sprite = SelectedBothSprite;
				}
				else
				{
					ItemImage.sprite = SelectedSecondSprite;
				}
			}
			else if (m_IsFirstSelected)
			{
				ItemImage.sprite = SelectedFirstSprite;
			}
			else
			{
				ItemImage.sprite = StandardSprite;
			}
		}
		else
		{
			Log.GUIError("Both item slots are already assigned.");
		}
	}

	public IEnumerator ItemSpent(CItem item)
	{
		if (m_ItemCardsUI[0].titleText.text == item.Name)
		{
			m_IsFirstSelected = false;
			UnityEngine.Object.Destroy(m_ItemCardsUI[0].gameObject);
			yield return new WaitForEndOfFrame();
			m_ItemCardsUI[0] = null;
			if (m_ItemCardsUI[1] == null)
			{
				ItemImage.sprite = DisabledSprite;
				m_Enabled = false;
			}
			else if (m_IsSecondSelected)
			{
				ItemImage.sprite = SelectedSecondSprite;
			}
			else
			{
				ItemImage.sprite = StandardSprite;
			}
		}
	}

	private void OnSelectionChanged(object o, EventArgs e)
	{
		ItemCardUI itemCardUI = o as ItemCardUI;
		if (itemCardUI == m_ItemCardsUI[0])
		{
			m_IsFirstSelected = itemCardUI.m_IsSelected;
		}
		else if (itemCardUI == m_ItemCardsUI[1])
		{
			m_IsSecondSelected = itemCardUI.m_IsSelected;
		}
		if (m_IsFirstSelected && m_IsSecondSelected)
		{
			m_IsFirstSelected = false;
			m_IsSecondSelected = false;
			m_IsBothSelected = true;
			ItemImage.sprite = SelectedBothSprite;
		}
		else if (m_IsFirstSelected && !m_IsSecondSelected)
		{
			ItemImage.sprite = SelectedFirstSprite;
		}
		else if (!m_IsFirstSelected && m_IsSecondSelected)
		{
			ItemImage.sprite = SelectedSecondSprite;
		}
		else
		{
			ItemImage.sprite = StandardSprite;
		}
	}

	public void OnPointerEnter()
	{
		if (m_Enabled)
		{
			if (m_ItemCardsUI[0] != null)
			{
				m_ItemCardsUI[0].gameObject.SetActive(value: true);
			}
			if (m_ItemCardsUI[1] != null)
			{
				m_ItemCardsUI[1].gameObject.SetActive(value: true);
			}
		}
	}

	public void OnPointerExit()
	{
		if (m_Enabled)
		{
			if (m_ItemCardsUI[0] != null)
			{
				m_ItemCardsUI[0].gameObject.SetActive(value: false);
			}
			if (m_ItemCardsUI[1] != null)
			{
				m_ItemCardsUI[1].gameObject.SetActive(value: false);
			}
		}
	}

	public void OnPointerDown()
	{
		if (!m_Enabled)
		{
			return;
		}
		if (m_IsFirstSelected)
		{
			if (m_ItemCardsUI[1] != null)
			{
				m_ItemCardsUI[0].ToggleSelect(select: false);
				m_Inventory.DeselectItem(m_ItemCardsUI[0].item);
				m_ItemCardsUI[1].ToggleSelect(select: true);
				m_Inventory.SelectItem(m_ItemCardsUI[1].item);
			}
			else
			{
				m_ItemCardsUI[0].ToggleSelect(select: false);
				m_Inventory.DeselectItem(m_ItemCardsUI[0].item);
			}
		}
		else if (m_IsSecondSelected)
		{
			if (m_ItemCardsUI[0] != null)
			{
				m_ItemCardsUI[0].ToggleSelect(select: true);
				m_Inventory.SelectItem(m_ItemCardsUI[0].item);
			}
			else
			{
				m_ItemCardsUI[1].ToggleSelect(select: false);
				m_Inventory.DeselectItem(m_ItemCardsUI[1].item);
			}
		}
		else if (m_IsBothSelected)
		{
			m_ItemCardsUI[0].ToggleSelect(select: false);
			m_Inventory.DeselectItem(m_ItemCardsUI[0].item);
			m_ItemCardsUI[1].ToggleSelect(select: false);
			m_Inventory.DeselectItem(m_ItemCardsUI[1].item);
		}
		else if (m_ItemCardsUI[0] != null)
		{
			m_ItemCardsUI[0].ToggleSelect(select: true);
			m_Inventory.SelectItem(m_ItemCardsUI[0].item);
		}
		else if (m_ItemCardsUI[1] != null)
		{
			m_ItemCardsUI[1].ToggleSelect(select: true);
			m_Inventory.SelectItem(m_ItemCardsUI[1].item);
		}
	}
}
