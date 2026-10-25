using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class TestingItemTooltip : MonoBehaviour
{
	public TMP_Dropdown Items;

	public RectTransform TooltipAnchor;

	public RectTransform CardAnchor;

	public Button PrevButton;

	public Button NextButton;

	public UIOutpostItemElement ItemUI;

	public FHItemTooltip ItemTooltip;

	private DefRepository _defRepository;

	private ItemID[] _items;

	private GameObject _card;

	private void Start()
	{
		_defRepository = YMLDefRepository.Repo.DefRepository;
		_items = (from i in _defRepository.Items
			select i.Key into i
			orderby i.ID
			select i).ToArray();
		PrevButton.onClick.AddListener(() =>
		{
			Items.value -= 1;
		});
		NextButton.onClick.AddListener(() =>
		{
			Items.value += 1;
		});
		foreach (ItemCardYMLData item in ScenarioRuleClient.SRLYML.ItemCards.ToList())
		{
			if (PersistentData.CreateItemCard1(item, out var newItemCard, out var card))
			{
				PersistentData.CreateItemCard2(item, newItemCard, card);
			}
		}
		Items.options = _items.Select((ItemID i) => new TMP_Dropdown.OptionData
		{
			text = $"{_defRepository.Items[i].Name} ({_defRepository.Items[i].ID.ID})"
		}).ToList();
		Items.value = 0;
		Items.onValueChanged.AddListener(OnChooseItem);
		OnChooseItem(0);
	}

	private ItemDataDTO GetItem(int scenarioItemID)
	{
		return new ItemDataDTO(ScenarioRuleClient.SRLYML.ItemCards.First((ItemCardYMLData i) => i.ID == scenarioItemID));
	}

	private FHItemTooltip.ItemTooltipParams GetTooltipParams(ItemDataDTO item)
	{
		FHItemTooltip.ItemTooltipParams itemTooltipParams = new FHItemTooltip.ItemTooltipParams();
		itemTooltipParams.ItemData = item;
		itemTooltipParams.FlipItem = (item.FlipScenarioItemID.HasValue ? GetItem(item.FlipScenarioItemID.Value) : null);
		itemTooltipParams.Data = new FHItemTooltip.ItemTooltipData
		{
			Actions = new FHItemTooltip.ItemTooltipAction[1]
			{
				new FHItemTooltip.ItemTooltipAction
				{
					Name = "Select",
					IsInteractable = true,
					OnClick = () =>
					{
						Debug.Log(item.NameLocalized);
					}
				}
			}
		};
		return itemTooltipParams;
	}

	private void OnChooseItem(int arg0)
	{
		ItemDef itemDef = _defRepository.Items[_items[arg0]];
		ItemDataDTO itemDataDTO = new ItemDataDTO(itemDef);
		FHItemTooltip.ItemTooltipParams tooltipParams = GetTooltipParams(itemDataDTO);
		ItemUI.SetItem(itemDataDTO, new UIItemElementInteractionEvents
		{
			OnHover = (UIOutpostItemElement target, bool raised) =>
			{
				Debug.Log(raised);
			}
		}, tooltipParams);
		ItemTooltip.Show(tooltipParams);
		PrevButton.interactable = Items.value >= 0;
		NextButton.interactable = Items.value < Items.options.Count - 1;
		if (_card != null)
		{
			ObjectPool.RecycleCard(_card.GetComponent<ItemCardUI>().CardID, ObjectPool.ECardType.Item, _card);
		}
		_card = ObjectPool.SpawnCard(itemDef.ScenarioItemID, ObjectPool.ECardType.Item, CardAnchor, resetLocalScale: true, resetToMiddle: true, resetLocalRotation: true);
	}
}
