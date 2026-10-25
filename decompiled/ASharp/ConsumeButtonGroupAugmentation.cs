using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;

public class ConsumeButtonGroupAugmentation : IAugmentation
{
	private ISingleOptionHolder optionHolder;

	private UIUsePreview previewEffect;

	private string groupId;

	private List<CActionAugmentation> augmentations;

	public AugmentationHolder SelectedAugment
	{
		get
		{
			return optionHolder.SelectedOption as AugmentationOption;
		}
		set
		{
			optionHolder.SelectedOption = (IOption)value;
		}
	}

	public string ID => groupId;

	public ConsumeButtonGroupAugmentation(string groupId, List<CActionAugmentation> augmentations, ISingleOptionHolder optionHolder, UIUsePreview previewEffect)
	{
		this.groupId = groupId;
		this.optionHolder = optionHolder;
		this.previewEffect = previewEffect;
		this.augmentations = augmentations?.ToList();
		previewEffect.SetDescription(LocalizationManager.GetTranslation($"UI_PREVIEW_EFFECT_AUGMENT_GROUP_{groupId}"));
	}

	public CActionAugmentation GetAugmentation()
	{
		if (SelectedAugment != null)
		{
			return GetAvailableAugmentations().FirstOrDefault((CActionAugmentation x) => x.Name == SelectedAugment.ID);
		}
		return null;
	}

	public IEnumerable<CActionAugmentation> GetAvailableAugmentations()
	{
		return augmentations;
	}

	public void ActiveAugment(List<ElementInfusionBoardManager.EElement> elements)
	{
		if (SelectedAugment?.Augmentation != null)
		{
			previewEffect.SetDescription(SelectedAugment.Augmentation);
		}
		SelectedAugment.ActiveAugment(elements);
	}

	public bool CanBeDisactivated()
	{
		if (SelectedAugment != null)
		{
			return SelectedAugment.CanBeDisactivated();
		}
		return false;
	}

	public string GetSelectAudioItem()
	{
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}

	public void DisactiveAugment()
	{
		previewEffect.SetDescription(LocalizationManager.GetTranslation($"UI_PREVIEW_EFFECT_AUGMENT_GROUP_{groupId}"));
		if (optionHolder.SelectedOption != null)
		{
			SelectedAugment.DisactiveAugment();
		}
	}
}
