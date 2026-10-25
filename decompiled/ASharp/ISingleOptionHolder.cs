using System.Collections.Generic;

public abstract class ISingleOptionHolder : IOptionHolder
{
	public abstract IOption SelectedOption { get; set; }

	public List<IOption> SelectedOptions
	{
		get
		{
			if (SelectedOption != null)
			{
				return new List<IOption> { SelectedOption };
			}
			return new List<IOption>();
		}
		set
		{
			SelectedOption = ((value != null && value.Count > 0) ? value[0] : null);
		}
	}

	public virtual bool AlwaysShowPicker => false;

	public void ClearSelection()
	{
		SelectedOption = null;
	}

	public virtual void OnHoveredOption(IOption option, bool hovered)
	{
	}
}
