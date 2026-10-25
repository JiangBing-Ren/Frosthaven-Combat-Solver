using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUsePreview : MonoBehaviour
{
	[SerializeField]
	private Image customImage;

	[SerializeField]
	private GameObject descriptionTextContainer;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI descriptionTextShadow;

	public void SetDescription(string text)
	{
		customImage.gameObject.SetActive(value: false);
		descriptionText.text = text;
		descriptionTextShadow.text = text.Replace("<sprite", "<sprite color=#000000");
		descriptionTextContainer.SetActive(value: true);
	}

	public void SetDescription(Sprite image)
	{
		customImage.sprite = image;
		customImage.gameObject.SetActive(value: true);
		descriptionTextContainer.SetActive(value: false);
	}

	public void SetDescription(List<CAbility> ability)
	{
		SetDescription(PreviewEffectGenerator.GenerateDescription(ability, isForPreview: true));
	}

	public void SetDescription(CAbility ability)
	{
		if (ability.PreviewEffectId.IsNOTNullOrEmpty())
		{
			SetDescription(UIInfoTools.Instance.GetPreviewEffectConfig(ability.PreviewEffectId));
		}
		else
		{
			SetDescription(PreviewEffectGenerator.GenerateDescription(ability, isForPreview: true));
		}
	}

	public void SetDescription(CActionAugmentation augmentation)
	{
		if (augmentation.PreviewEffectId.IsNOTNullOrEmpty())
		{
			SetDescription(UIInfoTools.Instance.GetPreviewEffectConfig(augmentation.PreviewEffectId));
		}
		else if (augmentation.PreviewEffectText.IsNOTNullOrEmpty())
		{
			SetDescription(augmentation.PreviewEffectText);
		}
		else
		{
			SetDescription(PreviewEffectGenerator.GenerateDescription(augmentation));
		}
	}

	private void SetDescription(PreviewEffectInfo previewEffect)
	{
		if (previewEffect?.previewEffectIcon != null)
		{
			SetDescription(previewEffect.previewEffectIcon);
		}
		else if (previewEffect != null && previewEffect.previewEffectText.IsNOTNullOrEmpty())
		{
			SetDescription(previewEffect.previewEffectText);
		}
		else
		{
			SetDescription(string.Empty);
		}
	}

	public void SetDescription(CItem item)
	{
		ItemConfigUI itemConfig = UIInfoTools.Instance.GetItemConfig(item.YMLData.Art);
		if (itemConfig != null && itemConfig.previewEffect.IsNotEmpty())
		{
			SetDescription(itemConfig.previewEffect);
		}
		else
		{
			SetDescription(PreviewEffectGenerator.GeneratePreview(item.YMLData.Data));
		}
	}
}
