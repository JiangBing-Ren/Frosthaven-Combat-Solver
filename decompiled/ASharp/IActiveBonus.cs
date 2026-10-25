using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public interface IActiveBonus
{
	bool IsToggled { get; }

	bool IsToggleLocked { get; }

	void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false);

	List<ElementInfusionBoardManager.EElement> GetConsumes();

	ElementInfusionBoardManager.EElement? GetSelectedConsume();

	Sprite GetIcon();

	void UntoggleActiveBonus(bool fromClick = false);

	string GetSelectAudioItem();
}
