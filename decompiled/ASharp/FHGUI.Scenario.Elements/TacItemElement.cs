using System.Collections.Generic;
using System.Linq;
using FHGUI.Scenario.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Scenario.Elements;

public class TacItemElement : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Button _button;

	public void SetItem(TacItemDTO item)
	{
		_image.sprite = item.Image;
	}

	public static void ClearItems(Transform itemsContainer)
	{
		List<TacItemElement> pool = itemsContainer.GetComponentsInChildren<TacItemElement>(includeInactive: true).ToList();
		HelperTools.NormalizePool(ref pool, pool[0].gameObject, itemsContainer, 0);
		foreach (TacItemElement item in pool)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	public static void SetItems(Transform itemsContainer, List<TacItemDTO> items)
	{
		List<TacItemElement> list = itemsContainer.GetComponentsInChildren<TacItemElement>(includeInactive: true).ToList();
		GameObject defaultPrototype = list[0].gameObject;
		foreach (TacItemElement item2 in list)
		{
			item2.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < items.Count; i++)
		{
			TacItemDTO item = items[i];
			TacItemElement orCreateItem = GetOrCreateItem(itemsContainer, defaultPrototype, list, item);
			orCreateItem.transform.SetSiblingIndex(i);
			orCreateItem.gameObject.SetActive(value: true);
			list.Remove(orCreateItem);
		}
	}

	private static TacItemElement GetOrCreateItem(Transform container, GameObject defaultPrototype, List<TacItemElement> itemsElement, TacItemDTO item)
	{
		TacItemElement tacItemElement = itemsElement.FirstOrDefault();
		if ((object)tacItemElement == null)
		{
			tacItemElement = Object.Instantiate(defaultPrototype).GetComponent<TacItemElement>();
			tacItemElement.transform.SetParent(container, worldPositionStays: false);
		}
		tacItemElement.SetItem(item);
		return tacItemElement;
	}
}
