using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugEditInventory : MonoBehaviour
{
	public TMP_Dropdown SlotDropdown;

	public TMP_Dropdown IndexDropdown;

	public TextMeshProUGUI ItemText;

	public ConvenientDropdown ItemsDropdown;

	public Button ItemButton;

	public TextMeshProUGUI StatusText;

	public TMP_Dropdown StatusDropdown;

	public Button StatusButton;

	public GameObject MainMenu;

	private const string EmptyItemSlot = "Empty";

	private const string EmptyItemStatus = "None";

	private const string UnusedStatus = "Unused";

	private CInventory currentInventory;

	private CItem.EItemSlot currentSlot;

	private int currentIndex;

	private CItem currentItem;

	private void Awake()
	{
		SlotDropdown.AddOptions((from x in CItem.ItemSlots
			where x != CItem.EItemSlot.None
			select x.ToString()).ToList());
		StatusDropdown.AddOptions((from x in CItem.UsageTypes
			where x != CItem.EUsageType.None
			select x.ToString()).ToList());
	}

	public void ShowEditInventory(CInventory inventory)
	{
		MainMenu.SetActive(value: false);
		gameObject.SetActive(value: true);
		currentInventory = inventory;
		SetCurrentSlot();
	}

	public void HideEditInventory()
	{
		currentInventory = null;
		gameObject.SetActive(value: false);
		MainMenu.SetActive(value: true);
	}

	public void SlotChanged()
	{
		SetCurrentSlot();
	}

	public void IndexChanged()
	{
		SetCurrentIndex();
		PopulateItems();
	}

	public void UpdateItem()
	{
		currentItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData x) => LevelEditorUI.FormatFHName(x.Name) == ItemsDropdown.options[ItemsDropdown.value].text && !x.StringID.EndsWith("2ID")).GetItem;
		currentInventory.RemoveItem(currentItem);
		currentInventory.AddItem(currentItem);
		PopulateItems();
	}

	public void UpdateStatus()
	{
		if (StatusDropdown.options[StatusDropdown.value].text == "Unused")
		{
			currentInventory.ReactivateItem(currentItem);
			StatusText.text = "Unused";
		}
		else
		{
			currentInventory.UseItem(currentItem);
			StatusText.text = currentItem.YMLData.Usage.ToString();
		}
	}

	private void UpdateIndex()
	{
		IndexDropdown.value = 0;
		IndexDropdown.ClearOptions();
		if (currentSlot == CItem.EItemSlot.SmallItem)
		{
			for (int i = 1; i <= currentInventory.SmallItemMax; i++)
			{
				IndexDropdown.AddOptions(new List<TMP_Dropdown.OptionData>
				{
					new TMP_Dropdown.OptionData(i.ToString())
				});
			}
		}
		else if (currentSlot == CItem.EItemSlot.OneHand)
		{
			IndexDropdown.AddOptions(new List<string> { "1", "2" });
		}
		else if (currentSlot != CItem.EItemSlot.None)
		{
			IndexDropdown.AddOptions(new List<TMP_Dropdown.OptionData>
			{
				new TMP_Dropdown.OptionData("1")
			});
		}
		SetCurrentIndex();
	}

	private void SetCurrentSlot()
	{
		currentSlot = CItem.ItemSlots.Single((CItem.EItemSlot x) => x.ToString() == SlotDropdown.options[SlotDropdown.value].text);
		UpdateIndex();
	}

	private void SetCurrentIndex()
	{
		if (int.TryParse(IndexDropdown.options[IndexDropdown.value].text, out var result))
		{
			currentIndex = result - 1;
		}
		PopulateItems();
	}

	private void PopulateItems()
	{
		List<CItem> equippedItems = currentInventory.GetItemsInSlot(currentSlot);
		ItemsDropdown.ClearOptions();
		ItemsDropdown.AddOptions((from n in (from x in ScenarioRuleClient.SRLYML.ItemCards
				where x.Slot == currentSlot
				where !equippedItems.Exists((CItem e) => e.ItemGuid == x.GetItem.ItemGuid)
				select LevelEditorUI.FormatFHName(x.Name)).Distinct()
			orderby n
			select n).ToList());
		currentItem = currentInventory.GetItem(currentSlot, currentIndex);
		SetItem();
	}

	private void SetItem()
	{
		StatusDropdown.ClearOptions();
		if (currentItem != null)
		{
			ItemText.text = LevelEditorUI.FormatFHName(currentItem.Name);
			if (currentItem.YMLData.Usage == CItem.EUsageType.Spent || currentItem.YMLData.Usage == CItem.EUsageType.Consumed)
			{
				StatusDropdown.enabled = true;
				StatusButton.enabled = true;
				StatusDropdown.AddOptions(new List<string>
				{
					"Unused",
					currentItem.YMLData.Usage.ToString()
				});
			}
			else
			{
				StatusDropdown.enabled = false;
				StatusButton.enabled = false;
			}
			if (currentInventory.IsItemUsed(currentItem))
			{
				StatusText.text = currentItem.YMLData.Usage.ToString();
			}
			else
			{
				StatusText.text = "Unused";
			}
		}
		else
		{
			ItemText.text = "Empty";
			StatusText.text = "None";
			StatusDropdown.enabled = false;
			StatusButton.enabled = false;
		}
	}
}
