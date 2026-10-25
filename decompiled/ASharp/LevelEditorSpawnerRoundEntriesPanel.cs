using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class LevelEditorSpawnerRoundEntriesPanel : MonoBehaviour
{
	public TextMeshProUGUI ListTitle;

	public GameObject ItemLoadoutItemPrefab;

	private List<LevelEditorSpawnerRoundEntriesItem> m_LoadoutItems;

	private SpawnerData m_spawnerData;

	private void Awake()
	{
		ItemLoadoutItemPrefab.SetActive(value: false);
	}

	public void DisplaySpawnRoundEntriesForSpawner(SpawnerData spawnerData)
	{
		ListTitle.text = "SpawnRoundEntriesForSpawner";
		m_spawnerData = spawnerData;
		FillList();
	}

	public void AddListItemForSpawnRoundEntry(SpawnRoundEntry itemToAdd)
	{
		GameObject gameObject = Object.Instantiate(ItemLoadoutItemPrefab, ItemLoadoutItemPrefab.transform.parent);
		gameObject.SetActive(value: true);
		LevelEditorSpawnerRoundEntriesItem component = gameObject.GetComponent<LevelEditorSpawnerRoundEntriesItem>();
		component.SetupUI(itemToAdd);
		component.DeleteButtonPressedAction = ItemDeleted;
		m_LoadoutItems.Add(component);
	}

	public void ItemDeleted(LevelEditorSpawnerRoundEntriesItem itemDeleted)
	{
		m_spawnerData.SpawnRoundEntryDictionary["Default"].Remove(itemDeleted.SpawnRoundEntry);
		GameObject obj = itemDeleted.gameObject;
		m_LoadoutItems.Remove(itemDeleted);
		Object.Destroy(obj);
	}

	private void ClearList()
	{
		if (m_LoadoutItems == null)
		{
			m_LoadoutItems = new List<LevelEditorSpawnerRoundEntriesItem>();
			return;
		}
		for (int i = 0; i < m_LoadoutItems.Count; i++)
		{
			Object.Destroy(m_LoadoutItems[i].gameObject);
		}
		m_LoadoutItems.Clear();
	}

	private void FillList()
	{
		ClearList();
		if (m_spawnerData.SpawnRoundEntryDictionary == null)
		{
			return;
		}
		foreach (SpawnRoundEntry item in m_spawnerData.SpawnRoundEntryDictionary["Default"])
		{
			AddListItemForSpawnRoundEntry(item);
		}
	}

	public void AddSpawnRoundEntry()
	{
		SpawnRoundEntry spawnRoundEntry = new SpawnRoundEntry(new List<string> { "Bandit Guard", "Bandit Guard", "Bandit Guard", "Bandit Guard" });
		m_spawnerData.SpawnRoundEntryDictionary["Default"].Add(spawnRoundEntry);
		AddListItemForSpawnRoundEntry(spawnRoundEntry);
	}
}
