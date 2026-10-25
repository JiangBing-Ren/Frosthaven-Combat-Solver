using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;

public class SingleElementPickController : ElementPickController
{
	private Action<ElementInfusionBoardManager.EElement> onSelected;

	private IElementHolder elementUI;

	public SingleElementPickController(UIElementPicker picker)
		: base(picker)
	{
	}

	public SingleElementPickController SetOnSelectedElement(Action<ElementInfusionBoardManager.EElement> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public void Setup(IElementHolder elementUI)
	{
		this.elementUI = elementUI;
		if (elementUI.RequiredElement == ElementInfusionBoardManager.EElement.Any)
		{
			picker.Init(OnSelected, excludedElements, isInfusion: false, 1, null, onOpenPicker, onClosePicker);
		}
	}

	public void OnSelected(List<ElementInfusionBoardManager.EElement> elements)
	{
		elementUI.SetSelectedElement(elements[0]);
		picker.Hide();
		onSelected?.Invoke(elements[0]);
	}

	public override bool Pick()
	{
		if (elementUI.RequiredElement == ElementInfusionBoardManager.EElement.Any)
		{
			if (!elementUI.SelectedElement.HasValue)
			{
				List<ElementInfusionBoardManager.EElement> list = InfusionBoardUI.Instance?.GetAvailableElements();
				if (list.Count == 1)
				{
					elementUI.SetSelectedElement(list[0]);
					return true;
				}
				picker.Show();
				return false;
			}
			return true;
		}
		if (ElementInfusionBoardManager.ElementColumn(elementUI.RequiredElement, Choreographer.s_Choreographer.CurrentPlayerActor) != ElementInfusionBoardManager.EColumn.Inert)
		{
			InfusionBoardUI instance = InfusionBoardUI.Instance;
			if ((object)instance == null || !instance.IsElementReserved(elementUI.RequiredElement))
			{
				elementUI.SetSelectedElement(elementUI.RequiredElement);
				return true;
			}
		}
		Log.GUIError("An active bonus with consume element is enabled when missing its infusion " + elementUI.RequiredElement);
		return false;
	}
}
