using System;
using System.Collections.Generic;
using System.Linq;

public class SingleOptionPickController : PickController
{
	private Action<IOption> onSelected;

	protected ISingleOptionHolder elementUI;

	private UIOptionPicker picker;

	public List<IOption> Options { get; private set; }

	public SingleOptionPickController(UIOptionPicker picker)
	{
		this.picker = picker;
	}

	public SingleOptionPickController SetOnSelected(Action<IOption> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public void Setup(ISingleOptionHolder elementUI, List<IOption> options)
	{
		Options = options;
		this.elementUI = elementUI;
	}

	private void OnSelected(List<IOption> elements, bool isDeselection)
	{
		elementUI.SelectedOption = elements[0];
		picker.Hide();
		onSelected?.Invoke(elements[0]);
	}

	public override bool IsSelecting()
	{
		return picker.IsOpen;
	}

	public override void ClosePicker()
	{
		picker.Hide();
	}

	public void SelectOption(string optionId)
	{
		IOption selectedOption = Options.FirstOrDefault((IOption it) => optionId == it.ID);
		elementUI.SelectedOption = selectedOption;
	}

	public override bool Pick()
	{
		if (elementUI.SelectedOption == null || elementUI.AlwaysShowPicker)
		{
			if (IsSelecting())
			{
				ClosePicker();
				return true;
			}
			if (Options.Count == 1 && !elementUI.AlwaysShowPicker)
			{
				elementUI.SelectedOption = Options[0];
				return true;
			}
			picker.Init(Options, OnSelected, 1, null, onOpenPicker, onClosePicker);
			picker.Show();
			return false;
		}
		return true;
	}
}
