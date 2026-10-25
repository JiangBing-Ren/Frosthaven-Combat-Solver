internal class InitiativeOptionController : ISingleOptionHolder
{
	private UIUseOption optionUI;

	private IOption selectedOption;

	private int defaultInitiative;

	public override IOption SelectedOption
	{
		get
		{
			return selectedOption;
		}
		set
		{
			selectedOption = value;
			optionUI.SetOption((selectedOption != null) ? selectedOption.GetSelectedText() : defaultInitiative.ToString());
		}
	}

	public InitiativeOptionController(UIUseOption optionUI, int defaultInitiative)
	{
		this.defaultInitiative = defaultInitiative;
		this.optionUI = optionUI;
		optionUI.Show();
		ClearSelection();
	}
}
