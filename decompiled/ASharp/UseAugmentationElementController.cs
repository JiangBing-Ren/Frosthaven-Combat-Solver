using ScenarioRuleLibrary;
using UnityEngine;

public class UseAugmentationElementController : IElementHolder
{
	private UIUseAugmentationElement augmentUI;

	public ElementInfusionBoardManager.EElement RequiredElement { get; private set; }

	public ElementInfusionBoardManager.EElement? SelectedElement { get; private set; }

	public UseAugmentationElementController(UIUseAugmentationElement augmentUI, ElementInfusionBoardManager.EElement elementTarget, Sprite overrideSprite)
	{
		this.augmentUI = augmentUI;
		RequiredElement = elementTarget;
		augmentUI.Init(overrideSprite ?? UIInfoTools.Instance.GetElementUseSprite(elementTarget), elementTarget == ElementInfusionBoardManager.EElement.Any);
		SetSelectedElement(null);
	}

	public void SetSelectedElement(ElementInfusionBoardManager.EElement? element)
	{
		SelectedElement = element;
		augmentUI.SetSelected(element.HasValue ? UIInfoTools.Instance.GetElementPickerSelectedSprite(element.Value) : null);
	}

	public void ClearSelection()
	{
		SetSelectedElement(null);
	}
}
