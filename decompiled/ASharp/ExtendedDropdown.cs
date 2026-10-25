using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExtendedDropdown : TMP_Dropdown
{
	public UnityEvent OnCreatedOptions;

	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;

	public UnityEvent OnClosed;

	private List<DropdownItem> items = new List<DropdownItem>();

	private ScrollRect scroll;

	protected override GameObject CreateBlocker(Canvas rootCanvas)
	{
		GameObject gameObject = base.CreateBlocker(rootCanvas);
		gameObject.GetComponent<Button>().onClick.AddListener(Deselect);
		UnityEvent onCreatedOptions = OnCreatedOptions;
		if (onCreatedOptions != null)
		{
			onCreatedOptions.Invoke();
			return gameObject;
		}
		return gameObject;
	}

	protected override void DestroyDropdownList(GameObject dropdownList)
	{
		scroll = null;
		base.DestroyDropdownList(dropdownList);
		OnClosed?.Invoke();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		OnSelected.Invoke();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		OnDeselected.Invoke();
	}

	protected override GameObject CreateDropdownList(GameObject template)
	{
		GameObject gameObject = base.CreateDropdownList(template);
		scroll = gameObject.GetComponent<ScrollRect>();
		return gameObject;
	}

	protected override DropdownItem CreateItem(DropdownItem itemTemplate)
	{
		DropdownItem item = base.CreateItem(itemTemplate);
		items.Add(item);
		((ExtendedToggle)item.toggle).onSelected.AddListener(() =>
		{
			scroll.ScrollVerticallyToTop(item.rectTransform);
		});
		return item;
	}

	protected override void DestroyItem(DropdownItem item)
	{
		items.Remove(item);
		base.DestroyItem(item);
	}

	private void Deselect()
	{
		if (EventSystem.current.currentSelectedGameObject == gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
