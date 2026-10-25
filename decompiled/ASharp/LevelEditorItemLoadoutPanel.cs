using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorItemLoadoutPanel : MonoBehaviour
{
	public TextMeshProUGUI ListTitle;

	public LayoutGroup ItemLoadoutParent;

	public GameObject ItemLoadoutItemPrefab;

	public TMP_Dropdown ItemLoadoutDropdown;

	private bool m_IsShowingForProp;

	private CPlayerActor m_PlayerShowingFor;

	private CInventory m_InventoryShowingFor;

	private CObjectChest m_ChestPropShowingFor;

	private List<LevelEditorItemLoadoutItem> m_LoadoutItems;

	public void DisplayItemLoadoutForPlayer(CPlayerActor playerActor)
	{
		ListTitle.text = "Item loadout for " + LocalizationManager.GetTranslation(playerActor.ActorLocKey()) + ":";
		m_PlayerShowingFor = playerActor;
		m_InventoryShowingFor = playerActor.Inventory;
		FillList();
	}

	public void DisplayItemLoadoutForObjectProp(CObjectProp objectProp)
	{
		m_IsShowingForProp = true;
		ListTitle.text = "Item loadout for Object prop " + objectProp.PrefabName + ":";
		m_ChestPropShowingFor = objectProp as CObjectChest;
		FillList();
	}

	public void AddListItemForItem(CItem itemToAdd)
	{
		LevelEditorItemLoadoutItem component = Object.Instantiate(ItemLoadoutItemPrefab, ItemLoadoutParent.transform).GetComponent<LevelEditorItemLoadoutItem>();
		component.SetupUI(itemToAdd);
		component.DeleteButtonPressedAction = ItemDeleted;
		m_LoadoutItems.Add(component);
	}

	public void ItemDeleted(LevelEditorItemLoadoutItem itemDeleted)
	{
		if (m_IsShowingForProp)
		{
			m_ChestPropShowingFor.StartingItems.Remove(itemDeleted.ItemOnDisplay);
		}
		else
		{
			m_InventoryShowingFor.RemoveItem(itemDeleted.ItemOnDisplay);
		}
		GameObject obj = itemDeleted.gameObject;
		m_LoadoutItems.Remove(itemDeleted);
		Object.Destroy(obj);
		RefreshAddDropDown();
	}

	private void ClearList()
	{
		if (m_LoadoutItems == null)
		{
			m_LoadoutItems = new List<LevelEditorItemLoadoutItem>();
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
		if (m_IsShowingForProp)
		{
			if (m_ChestPropShowingFor.StartingItems != null)
			{
				foreach (CItem startingItem in m_ChestPropShowingFor.StartingItems)
				{
					AddListItemForItem(startingItem);
				}
			}
		}
		else
		{
			foreach (CItem allItem in m_InventoryShowingFor.AllItems)
			{
				AddListItemForItem(allItem);
			}
		}
		RefreshAddDropDown();
	}

	private void RefreshAddDropDown()
	{
		ItemLoadoutDropdown.ClearOptions();
		ItemLoadoutDropdown.options.Add(new TMP_Dropdown.OptionData("<SELECT ITEM TO ADD>"));
		List<string> options = (from n in ScenarioRuleClient.SRLYML.ItemCards.Select((ItemCardYMLData s) => LevelEditorUI.FormatFHName(s.Name)).Distinct()
			orderby n
			select n).ToList();
		ItemLoadoutDropdown.AddOptions(options);
		ItemLoadoutDropdown.value = 0;
	}

	public void AddItemPressed()
	{
		if (ItemLoadoutDropdown.value <= 0)
		{
			return;
		}
		CItem cItem = ScenarioRuleClient.SRLYML.ItemCards.Select((ItemCardYMLData s) => s.GetItem).ToList().SingleOrDefault((CItem s) => LevelEditorUI.FormatFHName(s.Name) == ItemLoadoutDropdown.options[ItemLoadoutDropdown.value].text && !s.YMLData.StringID.EndsWith("2ID"));
		if (m_IsShowingForProp)
		{
			if (cItem != null)
			{
				if (m_ChestPropShowingFor.StartingItems == null)
				{
					m_ChestPropShowingFor.StartingItems = new List<CItem>();
				}
				m_ChestPropShowingFor.StartingItems.Add(cItem);
				AddListItemForItem(cItem);
			}
		}
		else if (cItem != null)
		{
			m_InventoryShowingFor.AddItem(cItem);
			AddListItemForItem(cItem);
			FillList();
		}
	}
}
