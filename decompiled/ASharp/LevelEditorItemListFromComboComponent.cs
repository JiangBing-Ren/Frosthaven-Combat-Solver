using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorItemListFromComboComponent : MonoBehaviour
{
	public GameObject ItemPrefab;

	public TMP_Dropdown DropDown;

	public LayoutGroup ListContentGroup;

	private List<LevelEditorClassListItem> m_items = new List<LevelEditorClassListItem>();

	private Func<int, string> m_typeIndexToString;

	private Func<List<int>> m_getAllIndexes;

	public void Setup(Func<int, string> typeIndexToString, Func<List<int>> getAllIndexes, List<string> options)
	{
		m_typeIndexToString = typeIndexToString;
		m_getAllIndexes = getAllIndexes;
		DropDown.options.Clear();
		DropDown.AddOptions(options);
		DropDown.transform.parent.parent.Find("AddButton").GetComponent<Button>().onClick.AddListener(OnAddButtonClicked);
	}

	public void RefreshUi()
	{
		Clear();
		if (m_getAllIndexes == null)
		{
			return;
		}
		foreach (int item in m_getAllIndexes())
		{
			DropDown.value = item;
			OnAddButtonClicked();
		}
		DropDown.value = 0;
	}

	public IEnumerable<string> GetItems()
	{
		return m_items.Select((LevelEditorClassListItem i) => i.ClassName);
	}

	public void Clear()
	{
		for (int num = m_items.Count - 1; num >= 0; num--)
		{
			OnRemoveItem(m_items[num]);
		}
	}

	private void OnAddButtonClicked()
	{
		GameObject gameObject = null;
		gameObject = UnityEngine.Object.Instantiate(ItemPrefab, ListContentGroup.transform);
		if (gameObject != null)
		{
			LevelEditorClassListItem component = gameObject.GetComponent<LevelEditorClassListItem>();
			component.Init(m_typeIndexToString(DropDown.value));
			component.OnRemoveButtonPressedAction = OnRemoveItem;
			m_items.Add(component);
		}
	}

	private void OnRemoveItem(LevelEditorClassListItem playerClassItem)
	{
		m_items.Remove(playerClassItem);
		UnityEngine.Object.Destroy(playerClassItem.gameObject);
	}
}
