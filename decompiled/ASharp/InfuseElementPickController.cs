using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;

public class InfuseElementPickController : ElementPickController
{
	private Action<List<ElementInfusionBoardManager.EElement>> onSelected;

	private List<IElementHolder> elementUI;

	private List<IElementHolder> anyElementUI;

	private Action<int, ElementInfusionBoardManager.EElement> onSelectedElement;

	private Func<bool> cancelCheck;

	public InfuseElementPickController(UIElementPicker picker)
		: base(picker)
	{
	}

	public InfuseElementPickController SetOnSelectedAllElements(Action<List<ElementInfusionBoardManager.EElement>> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public InfuseElementPickController SetOnSelectedElement(Action<int, ElementInfusionBoardManager.EElement> onSelectedElement)
	{
		this.onSelectedElement = onSelectedElement;
		return this;
	}

	public InfuseElementPickController SetOnCancelCondition(Func<bool> cancelCheck)
	{
		this.cancelCheck = cancelCheck;
		return this;
	}

	public void Setup(List<IElementHolder> elementUI)
	{
		this.elementUI = elementUI;
		anyElementUI = elementUI.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
	}

	private void OnClosePicker()
	{
		if (!AreAllAnySelected() || (cancelCheck != null && cancelCheck()))
		{
			Cancel();
		}
		onClosePicker?.Invoke();
	}

	private void OnSelectedElement(ElementInfusionBoardManager.EElement element, bool selected)
	{
		if (selected)
		{
			int num = anyElementUI.FindIndex((IElementHolder it) => !it.SelectedElement.HasValue);
			anyElementUI[num].SetSelectedElement(element);
			onSelectedElement?.Invoke(num, element);
		}
		else
		{
			int index = anyElementUI.FindIndex((IElementHolder it) => it.SelectedElement == element);
			anyElementUI[index].SetSelectedElement(null);
		}
	}

	private void OnSelectedAll(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < anyElementUI.Count; i++)
		{
			anyElementUI[i].SetSelectedElement(elements[i]);
		}
		picker.Hide();
		if (Pick())
		{
			onSelected?.Invoke(elementUI.Select((IElementHolder it) => it.SelectedElement.Value).ToList());
		}
	}

	private void Cancel()
	{
		foreach (IElementHolder item in elementUI)
		{
			item.SetSelectedElement(null);
		}
	}

	public override bool Pick()
	{
		foreach (IElementHolder item in elementUI)
		{
			if (item.RequiredElement == ElementInfusionBoardManager.EElement.Any && !item.SelectedElement.HasValue)
			{
				picker.Init(OnSelectedAll, excludedElements, isInfusion: true, anyElementUI.Count, OnSelectedElement, onOpenPicker, OnClosePicker, saveInfusionsState: false);
				picker.Show();
				return false;
			}
		}
		foreach (IElementHolder item2 in elementUI)
		{
			if (item2.RequiredElement != ElementInfusionBoardManager.EElement.Any)
			{
				item2.SetSelectedElement(item2.RequiredElement);
			}
		}
		return true;
	}

	public bool AreAllAnySelected()
	{
		if (anyElementUI == null)
		{
			return false;
		}
		return !anyElementUI.Exists((IElementHolder it) => !it.SelectedElement.HasValue);
	}
}
