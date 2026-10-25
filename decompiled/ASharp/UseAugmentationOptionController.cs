using System;
using UnityEngine;

public class UseAugmentationOptionController : ISingleOptionHolder
{
	private UIUseAugmentationElement augmentUI;

	private IOption selectedOption;

	private Action<IOption> onUpdatedOption;

	public override IOption SelectedOption
	{
		get
		{
			return selectedOption;
		}
		set
		{
			selectedOption = value;
			augmentUI.SetSelected((selectedOption != null) ? selectedOption.GetSelectedText() : null);
			onUpdatedOption?.Invoke(selectedOption);
		}
	}

	public UseAugmentationOptionController(UIUseAugmentationElement augmentUI, Sprite characterIcon, bool showOption = false, Action<IOption> onUpdatedOption = null)
	{
		RegistedOnUpdatedOption(onUpdatedOption);
		this.augmentUI = augmentUI;
		augmentUI.Init(characterIcon, showOption);
		ClearSelection();
	}

	public void RegistedOnUpdatedOption(Action<IOption> onUpdatedOption)
	{
		this.onUpdatedOption = onUpdatedOption;
	}
}
