using System.Collections.Generic;
using System.Text.RegularExpressions;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIScenarioMechanic_024 : UIScenarioMechanicsEntry
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_EffectsContainer;

	[SerializeField]
	private GameObject m_EffectsPrototype;

	[Header("Settings")]
	[SerializeField]
	private Sprite m_SpriteAvailableEffect;

	[SerializeField]
	private Sprite m_SpriteSpendEffect;

	[SerializeField]
	private Color m_ColorAvailableEffect;

	[SerializeField]
	private Color m_ColorSpendEffect;

	[SerializeField]
	private Sprite[] m_EffectSprites;

	private List<RectTransform> m_EffectIcons = new List<RectTransform>();

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			for (int i = 0; i < m_EffectsContainer.transform.childCount; i++)
			{
				m_EffectIcons.Add(m_EffectsContainer.GetChild(i).transform as RectTransform);
			}
		}
	}

	public override void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		base.Init(newEntry, data, localizationParams, simpleTextLocKey);
		InitCache();
		if (data != null && data is List<(string, bool)> effects)
		{
			PopulateEffects(effects, localizationParams);
		}
	}

	private void PopulateEffects(List<(string, bool)> effects, LocalizationParam[] localizationParams)
	{
		if (m_EffectsContainer == null || m_EffectsPrototype == null)
		{
			return;
		}
		int num = effects?.Count ?? 0;
		HelperTools.NormalizePool(ref m_EffectIcons, m_EffectsPrototype.gameObject, m_EffectsContainer, num);
		for (int i = 0; i < num; i++)
		{
			(string, bool) tuple = effects[i];
			RectTransform rectTransform = m_EffectIcons[i];
			Image componentInChildren = rectTransform.GetComponentInChildren<Image>();
			if ((bool)componentInChildren)
			{
				componentInChildren.sprite = m_EffectSprites[i];
				componentInChildren.color = (tuple.Item2 ? m_ColorAvailableEffect : m_ColorSpendEffect);
			}
			TMP_Text componentInChildren2 = rectTransform.GetComponentInChildren<TMP_Text>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.text = LocalizationManager.GetTranslation("FH_24_Effect_" + tuple.Item1);
				componentInChildren2.color = (tuple.Item2 ? m_ColorAvailableEffect : m_ColorSpendEffect);
			}
			UITextTooltipTarget component = rectTransform.GetComponent<UITextTooltipTarget>();
			if (component != null)
			{
				string text = LocalizationManager.GetTranslation("FH_24_Effect_" + tuple.Item1);
				for (int j = 0; j < localizationParams.Length; j++)
				{
					LocalizationParam localizationParam = localizationParams[j];
					text = Regex.Replace(text, "{" + localizationParam.Name + "}", localizationParam.Value);
				}
				component.SetText(text);
			}
		}
	}
}
