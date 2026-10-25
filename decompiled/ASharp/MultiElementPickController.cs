using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;

public class MultiElementPickController : ElementPickController
{
	private Action<List<ElementInfusionBoardManager.EElement>> onSelected;

	private List<IElementHolder> elementsUI;

	private List<IElementHolder> anyElementUI;

	private Action<int, ElementInfusionBoardManager.EElement> onSelectedElement;

	private Action onCancelled;

	public MultiElementPickController(UIElementPicker picker)
		: base(picker)
	{
	}

	public MultiElementPickController SetOnSelectedAllElements(Action<List<ElementInfusionBoardManager.EElement>> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public MultiElementPickController SetOnSelectedElement(Action<int, ElementInfusionBoardManager.EElement> onSelectedElement)
	{
		this.onSelectedElement = onSelectedElement;
		return this;
	}

	public MultiElementPickController SetOnCancelled(Action onCancelled)
	{
		this.onCancelled = onCancelled;
		return this;
	}

	public void Setup(List<IElementHolder> elementUI)
	{
		elementsUI = elementUI;
		anyElementUI = elementUI.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
	}

	private void OnClosePicker()
	{
		if (!AreAllAnySelected())
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

	public void OnSelectedAll(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < anyElementUI.Count; i++)
		{
			anyElementUI[i].SetSelectedElement(elements[i]);
		}
		picker.Hide();
		if (Pick())
		{
			onSelected?.Invoke(elementsUI.Select((IElementHolder it) => it.SelectedElement.Value).ToList());
		}
	}

	public void Cancel()
	{
		bool flag = false;
		foreach (IElementHolder item in elementsUI)
		{
			flag |= item.SelectedElement.HasValue;
			item.SetSelectedElement(null);
		}
		if (flag)
		{
			onCancelled?.Invoke();
		}
	}

	public bool AreAllAnySelected()
	{
		return !anyElementUI.Exists((IElementHolder it) => !it.SelectedElement.HasValue);
	}

	public override bool Pick()
	{
		foreach (IElementHolder elementUI in elementsUI)
		{
			if (elementUI.RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				if (!elementUI.SelectedElement.HasValue)
				{
					List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
					if (excludedElements != null)
					{
						list.AddRange(excludedElements);
					}
					IEnumerable<ElementInfusionBoardManager.EElement> enumerable = from element in elementsUI
						where element.RequiredElement != ElementInfusionBoardManager.EElement.Any && element.RequiredElement != elementUI.RequiredElement
						select element.RequiredElement;
					if (enumerable.Any())
					{
						list.AddRange(enumerable);
					}
					picker.Init(OnSelectedAll, list, isInfusion: false, anyElementUI.Count, OnSelectedElement, onOpenPicker, OnClosePicker);
					picker.Show();
					return false;
				}
				continue;
			}
			if (ElementInfusionBoardManager.ElementColumn(elementUI.RequiredElement, Choreographer.s_Choreographer.CurrentPlayerActor) != ElementInfusionBoardManager.EColumn.Inert)
			{
				InfusionBoardUI instance = InfusionBoardUI.Instance;
				if ((object)instance == null || !instance.IsElementReserved(elementUI.RequiredElement))
				{
					continue;
				}
			}
			Log.GUIError("An active bonus with consume element is enabled when missing its infusion " + elementUI.RequiredElement);
			return false;
		}
		foreach (IElementHolder item in elementsUI)
		{
			if (item.RequiredElement != ElementInfusionBoardManager.EElement.Any)
			{
				item.SetSelectedElement(item.RequiredElement);
			}
		}
		return true;
	}
}
