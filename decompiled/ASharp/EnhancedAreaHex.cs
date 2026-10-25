using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class EnhancedAreaHex : EnhancementButtonBase
{
	[SerializeField]
	private Image hexImage;

	public void Init(CEnhancement enhancement, string nonEnhanced, string enhanced, string abilityNameLookupText, RectTransform parentContainer)
	{
		base.Init(enhancement, abilityNameLookupText, parentContainer);
		hexImage.sprite = CreateLayout.GetAtlasSprite(UIInfoTools.Instance.AreaEffectSpriteAtlas, "Dot");
	}

	public void Init(CEnhancement enhancement)
	{
		Init(enhancement, "AbilityCard/Dot", "AbilityCard/Enhanced", string.Empty, transform.parent as RectTransform);
		if (enhancement.Enhancement == EEnhancement.NoEnhancement)
		{
			RemoveEnhancement();
		}
		else
		{
			ApplyEnhancement();
		}
	}

	public void ApplyEnhancement()
	{
		EnhancementType = EEnhancement.Area;
		hexImage.sprite = CreateLayout.GetAtlasSprite(UIInfoTools.Instance.AreaEffectSpriteAtlas, "Enhanced");
	}

	public void RemoveEnhancement()
	{
		EnhancementType = EEnhancement.NoEnhancement;
		hexImage.sprite = CreateLayout.GetAtlasSprite(UIInfoTools.Instance.AreaEffectSpriteAtlas, "Dot");
	}
}
