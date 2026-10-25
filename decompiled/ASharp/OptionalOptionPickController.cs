using System;
using System.Collections.Generic;

public class OptionalOptionPickController : PickController
{
	private Action<List<IOption>, bool> onSelected;

	private Action<IOption, bool> onHovered;

	protected IOptionHolder elementUI;

	protected UIOptionPicker picker;

	public List<IOption> Options { get; private set; }

	public OptionalOptionPickController(UIOptionPicker picker)
	{
		this.picker = picker;
	}

	public void Setup(IOptionHolder elementUI, List<IOption> options)
	{
		Options = options;
		this.elementUI = elementUI;
	}

	public void SelectOptions(List<string> optionsID)
	{
		elementUI.SelectedOptions = Options.FindAll((IOption it) => optionsID.Contains(it.ID));
	}

	public override bool IsSelecting()
	{
		return picker.IsOpen;
	}

	public override void ClosePicker()
	{
		picker.Hide();
	}

	public OptionalOptionPickController SetOnSelected(Action<List<IOption>, bool> onSelected)
	{
		this.onSelected = onSelected;
		return this;
	}

	public OptionalOptionPickController SetOnHovered(Action<IOption, bool> onHovered)
	{
		this.onHovered = onHovered;
		return this;
	}

	private void OnSelected(List<IOption> option, bool isDeselection)
	{
		elementUI.SelectedOptions = option;
		onSelected?.Invoke(option, isDeselection);
	}

	public override bool Pick()
	{
		if (elementUI.SelectedOptions == null || elementUI.SelectedOptions.Count == 0)
		{
			if (Options.Count == 1)
			{
				elementUI.SelectedOptions = new List<IOption> { Options[0] };
				return true;
			}
			InitializePicker();
			return false;
		}
		return true;
	}

	public override void InitializePicker()
	{
		picker.Init(Options, OnSelected, -1, null, onOpenPicker, onClosePicker, onHovered);
		picker.Show();
	}
}
