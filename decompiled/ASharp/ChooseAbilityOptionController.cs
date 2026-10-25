internal class ChooseAbilityOptionController : ISingleOptionHolder
{
	private UIUseOption optionUI;

	private IOption selectedOption;

	public override IOption SelectedOption
	{
		get
		{
			return selectedOption;
		}
		set
		{
			selectedOption = value;
			optionUI.SetOption((selectedOption != null) ? selectedOption.GetSelectedText() : null);
		}
	}

	public ChooseAbilityOptionController(UIUseOption optionUI)
	{
		this.optionUI = optionUI;
		optionUI.Show();
		ClearSelection();
	}
}
