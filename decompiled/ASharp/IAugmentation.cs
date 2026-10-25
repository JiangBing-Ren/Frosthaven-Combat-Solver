using System.Collections.Generic;
using ScenarioRuleLibrary;

public interface IAugmentation
{
	string ID { get; }

	void ActiveAugment(List<ElementInfusionBoardManager.EElement> elements);

	void DisactiveAugment();

	bool CanBeDisactivated();

	string GetSelectAudioItem();

	CActionAugmentation GetAugmentation();

	IEnumerable<CActionAugmentation> GetAvailableAugmentations();
}
