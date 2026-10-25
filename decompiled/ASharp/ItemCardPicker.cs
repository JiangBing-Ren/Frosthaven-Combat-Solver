using System;
using System.Collections;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ItemCardPicker : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private RectTransform cardContainer;

	[SerializeField]
	private ItemCardPickerSlot slotPrefab;

	[SerializeField]
	private ScrollRect scrollRect;

	private List<ItemCardPickerSlot> cardPool = new List<ItemCardPickerSlot>();

	private Dictionary<CItem, ItemCardPickerSlot> cardSlots = new Dictionary<CItem, ItemCardPickerSlot>();

	private int itemsToSelect;

	private List<CItem> itemsSelected;

	private Action<List<CItem>> onItemsSelected;

	private Action onItemsDeselected;

	private bool maxSelectIsOptional;

	public bool AreAllItemsSelected => itemsSelected.Count == itemsToSelect;

	private void Start()
	{
		window.onHidden.AddListener(OnHidden);
	}

	private void OnHidden()
	{
		ClearContent();
	}

	private void ClearContent()
	{
		foreach (KeyValuePair<CItem, ItemCardPickerSlot> cardSlot in cardSlots)
		{
			cardSlot.Value.Clear();
		}
		cardSlots.Clear();
	}

	public void Show(List<CItem> items, int itemsToSelect = 1, Action<List<CItem>> onAllItemsSelected = null, Action onItemsDeselected = null, bool maxSelectIsOptional = false)
	{
		this.maxSelectIsOptional = maxSelectIsOptional;
		onItemsSelected = onAllItemsSelected;
		this.onItemsDeselected = onItemsDeselected;
		this.itemsToSelect = Math.Min(items.Count, itemsToSelect);
		itemsSelected = new List<CItem>();
		ClearContent();
		window.Show();
		HelperTools.NormalizePool(ref cardPool, slotPrefab.gameObject, cardContainer, items.Count);
		for (int i = 0; i < items.Count; i++)
		{
			cardPool[i].Init(items[i], OnSelectSlot, OnDeselectSlot);
			cardSlots[items[i]] = cardPool[i];
		}
		StartCoroutine(ResetScrollPosition());
	}

	private void OnSelectSlot(CItem item)
	{
		if (itemsSelected.Contains(item))
		{
			return;
		}
		itemsSelected.Add(item);
		if (itemsSelected.Count <= itemsToSelect)
		{
			if (maxSelectIsOptional || itemsSelected.Count == itemsToSelect)
			{
				onItemsSelected?.Invoke(itemsSelected);
			}
		}
		else if (itemsSelected.Count > itemsToSelect)
		{
			cardSlots[itemsSelected[0]].Deselect();
		}
	}

	private void OnDeselectSlot(CItem item)
	{
		if (!itemsSelected.Contains(item))
		{
			return;
		}
		itemsSelected.Remove(item);
		if (itemsSelected.Count <= itemsToSelect)
		{
			if (maxSelectIsOptional || itemsSelected.Count == itemsToSelect)
			{
				onItemsSelected?.Invoke(itemsSelected);
			}
		}
		else
		{
			onItemsDeselected?.Invoke();
		}
	}

	public List<CItem> GetCurrentSelectedItems()
	{
		return itemsSelected;
	}

	public void Hide()
	{
		window.Hide();
	}

	private IEnumerator ResetScrollPosition()
	{
		yield return new WaitForEndOfFrame();
		if ((bool)scrollRect)
		{
			scrollRect.verticalNormalizedPosition = 1f;
		}
	}
}
