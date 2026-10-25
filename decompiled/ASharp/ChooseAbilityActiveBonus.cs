using System.Collections.Generic;
using ScenarioRuleLibrary;

public class ChooseAbilityActiveBonus : ActiveBonus
{
	private ISingleOptionHolder optionHolder;

	private List<IOption> options;

	private UIUseActiveBonus slot;

	public ChooseAbilityActiveBonus(CChooseAbilityActiveBonus bonus, CActor actor, ISingleOptionHolder optionHolder, List<IOption> options, UIUseActiveBonus slot)
		: base(bonus, actor)
	{
		this.optionHolder = optionHolder;
		this.options = options;
		this.slot = slot;
	}

	public override void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false)
	{
		((CChooseAbilityActiveBonus)bonus).AbilityChosen(options.IndexOf(optionHolder.SelectedOption));
		base.ToggleActiveBonus(eElement, fromClick: true);
		slot.Hide();
	}

	public override void UntoggleActiveBonus(bool fromClick = false)
	{
		if (fromClick)
		{
			((CChooseAbilityActiveBonus)bonus).ResetChosenAbility();
		}
		base.UntoggleActiveBonus();
	}
}
