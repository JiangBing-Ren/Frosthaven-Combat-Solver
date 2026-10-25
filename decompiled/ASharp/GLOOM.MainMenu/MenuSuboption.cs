using System;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MenuSuboption : IMenuSuboption
{
	private string nameLocKey;

	private MenuOptionIcon icon;

	private Action onSelected;

	private Action onDeselected;

	private Action<bool> onHovered;

	private bool interactable;

	private bool selected;

	private BasicEventHandler onUpdatedInteractable;

	private BasicEventHandler onToggledSelect;

	public string NameLocKey => nameLocKey;

	public Sprite Icon => icon.Icon;

	public Sprite IconHighlight => icon.IconHighlight;

	public bool IsInteractable
	{
		get
		{
			return interactable;
		}
		set
		{
			if (interactable != value)
			{
				interactable = value;
				onUpdatedInteractable?.Invoke();
			}
		}
	}

	public MenuSuboption(string nameLocKey, MenuOptionIcon icon, Action onSelected, Action onDeselected = null, bool interactable = true, Action<bool> onHovered = null)
	{
		this.nameLocKey = nameLocKey;
		this.icon = icon;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		this.interactable = interactable;
		this.onHovered = onHovered;
		Reset();
	}

	public void Reset()
	{
		onUpdatedInteractable = null;
		onToggledSelect = null;
	}

	public void RegisterOnUpdatedInteraction(Action<bool> callback)
	{
		onUpdatedInteractable = (BasicEventHandler)Delegate.Combine(onUpdatedInteractable, (BasicEventHandler)(() =>
		{
			callback(IsInteractable);
		}));
	}

	public void RegisterOnToggledSelect(Action<bool> callback)
	{
		onToggledSelect = (BasicEventHandler)Delegate.Combine(onToggledSelect, (BasicEventHandler)(() =>
		{
			callback(selected);
		}));
	}

	public void Deselect()
	{
		if (selected)
		{
			selected = false;
			onToggledSelect();
		}
	}

	public void Select()
	{
		if (!selected)
		{
			selected = true;
			onToggledSelect();
		}
	}

	public void OnSelected()
	{
		Singleton<PromotionDLCManager>.InstanceFast?.Hide();
		selected = true;
		onSelected?.Invoke();
	}

	public void OnDeselected()
	{
		Singleton<PromotionDLCManager>.InstanceFast?.Show();
		selected = false;
		onDeselected?.Invoke();
	}

	public void OnHovered(bool isHovered)
	{
		onHovered?.Invoke(isHovered);
	}
}
