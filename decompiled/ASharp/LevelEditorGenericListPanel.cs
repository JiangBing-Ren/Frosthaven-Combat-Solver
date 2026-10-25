using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorGenericListPanel : MonoBehaviour
{
	public TextMeshProUGUI ListTitle;

	public LayoutGroup ListItemParent;

	public GameObject ListItemPrefab;

	public TMP_Dropdown ListItemAdditionDropdown;

	private List<LevelEditorListItemInlineButtons> m_ListItems;

	private List<string> m_AddableItems;

	private Action<string> m_OnItemAddedAction;

	private Action<string> m_OnSecondaryItemAddedAction;

	private Action<string, int> m_OnItemRemovedAction;

	private Action<string, int> m_OnItemPressedAction;

	public void RefreshUIWithItems(List<string> items)
	{
		if (m_ListItems == null)
		{
			m_ListItems = new List<LevelEditorListItemInlineButtons>();
			m_AddableItems = new List<string>();
		}
		for (int i = 0; i < m_ListItems.Count; i++)
		{
			UnityEngine.Object.Destroy(m_ListItems[i].gameObject);
		}
		m_ListItems.Clear();
		for (int j = 0; j < items.Count; j++)
		{
			LevelEditorListItemInlineButtons component = UnityEngine.Object.Instantiate(ListItemPrefab, ListItemParent.transform).GetComponent<LevelEditorListItemInlineButtons>();
			component.SetupListItem(items[j], j, OnListItemRemovePressed, OnListItemPressed);
			m_ListItems.Add(component);
		}
	}

	public void SetupItemsAvailableToAdd(List<string> itemsThatCanBeAdded)
	{
		ListItemAdditionDropdown?.SetValueWithoutNotify(0);
		ListItemAdditionDropdown?.ClearOptions();
		m_AddableItems = itemsThatCanBeAdded.ToList();
		ListItemAdditionDropdown?.AddOptions(m_AddableItems);
	}

	public void SetupDelegateActions(Action<string> onItemAdded = null, Action<string, int> onItemRemoved = null, Action<string, int> onItemPressed = null, Action<string> onSecondaryItemAdded = null)
	{
		m_OnItemAddedAction = onItemAdded;
		m_OnSecondaryItemAddedAction = onSecondaryItemAdded;
		m_OnItemRemovedAction = onItemRemoved;
		m_OnItemPressedAction = onItemPressed;
	}

	public void OnAddButtonPressed()
	{
		if (ListItemAdditionDropdown?.options != null && ListItemAdditionDropdown.options.Count > 0)
		{
			string obj = ListItemAdditionDropdown?.options[ListItemAdditionDropdown.value].text ?? null;
			m_OnItemAddedAction?.Invoke(obj);
		}
		else
		{
			m_OnItemAddedAction?.Invoke(string.Empty);
		}
	}

	public void OnSecondaryAddButtonPressed()
	{
		if (ListItemAdditionDropdown?.options != null && ListItemAdditionDropdown.options.Count > 0)
		{
			string obj = ListItemAdditionDropdown?.options[ListItemAdditionDropdown.value].text ?? null;
			m_OnSecondaryItemAddedAction?.Invoke(obj);
		}
		else
		{
			m_OnSecondaryItemAddedAction?.Invoke(string.Empty);
		}
	}

	private void OnListItemRemovePressed(LevelEditorListItemInlineButtons listItemRemoved)
	{
		string text = listItemRemoved.DescriptionLabel.text;
		int itemIndex = listItemRemoved.ItemIndex;
		m_OnItemRemovedAction?.Invoke(text, itemIndex);
	}

	private void OnListItemPressed(LevelEditorListItemInlineButtons listItemPressed)
	{
		string text = listItemPressed.DescriptionLabel.text;
		int itemIndex = listItemPressed.ItemIndex;
		m_OnItemPressedAction?.Invoke(text, itemIndex);
	}
}
