using System.Collections.Generic;
using ScenarioRuleLibrary;

public class AdjustInitativeActiveBonus : ActiveBonus
{
	private ISingleOptionHolder optionHolder;

	private List<IOption> options;

	public AdjustInitativeActiveBonus(CActiveBonus bonus, CActor actor, ISingleOptionHolder optionHolder, List<IOption> options)
		: base(bonus, actor)
	{
		this.optionHolder = optionHolder;
		this.options = options;
	}

	public override void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false)
	{
		ToggleActiveBonus(eElement, optionHolder.SelectedOption != options[0], fromClick: true);
	}
}
