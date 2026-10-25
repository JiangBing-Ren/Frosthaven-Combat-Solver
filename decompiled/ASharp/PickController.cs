using System;

public abstract class PickController
{
	protected Action onOpenPicker;

	protected Action onClosePicker;

	public PickController SetOnOpenPicker(Action callback)
	{
		onOpenPicker = callback;
		return this;
	}

	public PickController SetOnClosePicker(Action callback)
	{
		onClosePicker = callback;
		return this;
	}

	public abstract bool Pick();

	public abstract bool IsSelecting();

	public abstract void ClosePicker();

	public virtual void InitializePicker()
	{
	}
}
