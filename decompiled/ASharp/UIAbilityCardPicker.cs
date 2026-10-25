using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIAbilityCardPicker : Singleton<UIAbilityCardPicker>
{
	[SerializeField]
	private RectTransform choiceContainer;

	[SerializeField]
	private UIAbilityCardPickerSlot choicePrefab;

	[SerializeField]
	private List<UIAbilityCardPickerSlot> choicesPool;

	private Action<List<IAbilityCardOption>> onAllSelected;

	private Action onDeselected;

	private List<IAbilityCardOption> selectedOptions = new List<IAbilityCardOption>();

	private int optionsToSelect;

	private Dictionary<IAbilityCardOption, UIAbilityCardPickerSlot> assignedSlots = new Dictionary<IAbilityCardOption, UIAbilityCardPickerSlot>();

	private void ClearContent()
	{
		selectedOptions.Clear();
		foreach (UIAbilityCardPickerSlot value in assignedSlots.Values)
		{
			value.gameObject.SetActive(value: false);
		}
		assignedSlots.Clear();
	}

	public void Show<T>(CActor actor, List<T> options, int optionsToSelect = 2, Action<List<IAbilityCardOption>> onAllSelected = null, Action onDeselected = null) where T : IAbilityCardOption
	{
		this.onAllSelected = onAllSelected;
		this.onDeselected = onDeselected;
		this.optionsToSelect = optionsToSelect;
		ClearContent();
		HelperTools.NormalizePool(ref choicesPool, choicePrefab.gameObject, choiceContainer, options.Count);
		for (int i = 0; i < options.Count; i++)
		{
			IAbilityCardOption abilityCardOption = options[i];
			choicesPool[i].SetOption(actor, abilityCardOption, OnSelectSlot, OnDeselectSlot);
			assignedSlots[abilityCardOption] = choicesPool[i];
		}
		gameObject.SetActive(value: true);
	}

	private void OnSelectSlot(IAbilityCardOption option, bool networkActionIfOnline = true)
	{
		if (selectedOptions.Contains(option))
		{
			return;
		}
		if (selectedOptions.Count >= optionsToSelect)
		{
			assignedSlots[option].Deselect(networkActionIfOnline);
			return;
		}
		selectedOptions.Add(option);
		if (selectedOptions.Count != optionsToSelect)
		{
			return;
		}
		foreach (KeyValuePair<IAbilityCardOption, UIAbilityCardPickerSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.SetFocused(selectedOptions.Contains(assignedSlot.Key));
		}
		onAllSelected?.Invoke(selectedOptions);
	}

	private void OnDeselectSlot(IAbilityCardOption option, bool networkActionIfOnline = true)
	{
		if (!selectedOptions.Contains(option))
		{
			return;
		}
		if (selectedOptions.Count == optionsToSelect)
		{
			foreach (UIAbilityCardPickerSlot value in assignedSlots.Values)
			{
				value.SetFocused(focused: true);
			}
		}
		selectedOptions.Remove(option);
		onDeselected?.Invoke();
	}

	public void ClearSelection()
	{
		foreach (IAbilityCardOption item in selectedOptions.ToList())
		{
			assignedSlots[item].Deselect();
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		ClearContent();
	}

	private void OnDisable()
	{
		ClearContent();
	}

	public void ProxySelectAbilityCard(GameAction action, bool select)
	{
		UIAbilityCardPickerSlot uIAbilityCardPickerSlot = choicesPool.SingleOrDefault((UIAbilityCardPickerSlot x) => x.AbilityCardOption != null && x.AbilityCardOption.AbilityCard.ID == action.SupplementaryDataIDMax);
		if (uIAbilityCardPickerSlot != null)
		{
			if (select)
			{
				uIAbilityCardPickerSlot.Select(networkActionIfOnline: false);
			}
			else
			{
				uIAbilityCardPickerSlot.Deselect(networkActionIfOnline: false);
			}
			return;
		}
		throw new Exception("Error " + (select ? "selecting" : "deselecting") + " a card. Card option pool does not contain a card with ID: " + action.SupplementaryDataIDMax);
	}
}
