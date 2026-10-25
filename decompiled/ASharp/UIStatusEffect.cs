using FHGUI.DTO;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatusEffect : MonoBehaviour
{
	[SerializeField]
	private TMP_Text effectName;

	[SerializeField]
	private TMP_Text effectDescription;

	[SerializeField]
	private Image effectIcon;

	[SerializeField]
	private EffectsUIDataSO effectsDataSo;

	public void Initialize(string effectLocKey, Color nameColor, bool showEffectName = true)
	{
		effectName.gameObject.SetActive(showEffectName);
		if (showEffectName)
		{
			effectName.text = LocalizationManager.GetTranslation(effectLocKey);
			effectName.color = nameColor;
		}
		effectDescription.gameObject.SetActive(value: false);
		effectIcon.sprite = effectsDataSo.GetIcon(effectLocKey);
	}

	public void Initialize(CCondition.ENegativeCondition condition)
	{
		Initialize(condition.ToString(), UIInfoTools.DefInstance.negativeStatusEffectColor, showEffectName: false);
	}

	public void Initialize(CCondition.EPositiveCondition condition)
	{
		Initialize(condition.ToString(), UIInfoTools.DefInstance.positiveStatusEffectColor, showEffectName: false);
	}

	public void Initialize(string customEffectName, Sprite customEffectSprite, Color customEffectColor, string customEffectDescription = "")
	{
		if (customEffectName == "")
		{
			effectName.gameObject.SetActive(value: false);
		}
		else
		{
			effectName.text = CreateLayout.LocaliseText(customEffectName);
		}
		effectIcon.sprite = customEffectSprite;
		effectName.color = customEffectColor;
		if (customEffectDescription == "")
		{
			effectDescription.gameObject.SetActive(value: false);
		}
		else
		{
			effectDescription.text = CreateLayout.LocaliseText(customEffectDescription);
		}
	}

	public void AppendSource(string sourceText)
	{
		TMP_Text tMP_Text = effectDescription;
		tMP_Text.text = tMP_Text.text + " (" + sourceText + ")";
	}
}
