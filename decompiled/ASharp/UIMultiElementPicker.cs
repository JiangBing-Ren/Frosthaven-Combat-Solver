using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIMultiElementPicker : ElementPicker
{
	[SerializeField]
	private List<UIElementPicker> pickers;

	private Action<List<ElementInfusionBoardManager.EElement>> onSelectedAllElements;

	private Action<int, ElementInfusionBoardManager.EElement> onSelectedElement;

	private Dictionary<UIElementPicker, ElementInfusionBoardManager.EElement> pickedElements = new Dictionary<UIElementPicker, ElementInfusionBoardManager.EElement>();

	private int amount;

	public void Init(int amount, Action<List<ElementInfusionBoardManager.EElement>> onSelectedAllElements, Action<int, ElementInfusionBoardManager.EElement> onSelectedElement = null, List<ElementInfusionBoardManager.EElement> excludeElements = null, Action onOpenPicker = null, Action onClosePicker = null)
	{
		Init(onOpenPicker, onClosePicker);
		this.amount = amount;
		this.onSelectedAllElements = onSelectedAllElements;
		this.onSelectedElement = onSelectedElement;
		pickedElements.Clear();
		HelperTools.NormalizePool(ref pickers, pickers[0].gameObject, transform, amount);
		for (int i = 0; i < amount; i++)
		{
			UIElementPicker picker = pickers[i];
			_ = pickers[(i == 0) ? (amount - 1) : (i - 1)];
			_ = pickers[(i + 1) % amount];
			picker.Init((ElementInfusionBoardManager.EElement element) =>
			{
				OnSelect(picker, element);
			}, excludeElements);
		}
	}

	private void OnSelect(UIElementPicker picker, ElementInfusionBoardManager.EElement element)
	{
		if (pickedElements.ContainsKey(picker))
		{
			ElementInfusionBoardManager.EElement element2 = pickedElements[picker];
			for (int i = 0; i < amount; i++)
			{
				pickers[i].SetInteractable(element2, interactable: true);
			}
		}
		pickedElements[picker] = element;
		onSelectedElement?.Invoke(pickers.IndexOf(picker), element);
		if (pickedElements.Count == amount)
		{
			List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
			foreach (UIElementPicker picker2 in pickers)
			{
				if (!picker2.gameObject.activeSelf)
				{
					break;
				}
				list.Add(pickedElements[picker2]);
			}
			onSelectedAllElements(list);
			return;
		}
		for (int j = 0; j < amount; j++)
		{
			if (pickers[j] != picker)
			{
				pickers[j].SetInteractable(element, interactable: false);
			}
		}
	}

	public override void Show()
	{
		for (int i = 0; i < amount; i++)
		{
			pickers[i].RefreshAvailable();
		}
		base.Show();
	}

	public override void Hide()
	{
		pickedElements.Clear();
		base.Hide();
	}
}
