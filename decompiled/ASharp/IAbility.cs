using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public interface IAbility
{
	string ID { get; }

	Sprite Icon { get; set; }

	List<CAbility> Abilities { get; }

	List<IInfuseElement> Infusions { get; }

	List<ElementInfusionBoardManager.EElement> Consumes { get; }

	List<ElementInfusionBoardManager.EElement> ExcludedConsumes { get; }

	List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

	CItem DescriptionItem { get; }

	string DescriptionTitle { get; }

	string DescriptionText { get; }

	string OverrideDescription { get; set; }

	Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI);

	string GetSelectAudioItem();
}
