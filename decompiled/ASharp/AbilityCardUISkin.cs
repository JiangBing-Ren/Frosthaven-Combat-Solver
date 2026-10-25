using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[StateDump(NoMembers = true)]
public class AbilityCardUISkin
{
	public bool __HACK_FHCards;

	[Header("Full Card")]
	public Color initiativeColor = new Color32(243, 221, 171, byte.MaxValue);

	public Sprite titleSprite;

	public Sprite buttonsHolderSprite;

	[Header("Top Action Sprites")]
	public Sprite topActionRegularSprite;

	public Sprite topActionHighlightSprite;

	public Sprite topActionSelectedSprite;

	public Sprite topActionDisabledSprite;

	[Header("Bottom Action Sprites")]
	public Sprite bottomActionRegularSprite;

	public Sprite bottomActionHighlightSprite;

	public Sprite bottomActionSelectedSprite;

	public Sprite bottomActionDisabledSprite;

	[Header("Default Top Action Sprites")]
	public Sprite defaultTopActionRegularSprite;

	public Sprite defaultTopActionHighlightSprite;

	public Sprite defaultTopActionDisabledSprite;

	[Header("Default Action Sprites")]
	public Sprite defaultBottomActionRegularSprite;

	public Sprite defaultBottomActionHighlightSprite;

	public Sprite defaultBottomActionDisabledSprite;

	[Header("Long Rest Action Sprites")]
	public Sprite longRestActionRegularSprite;

	public Sprite longRestActionHighlightSprite;

	public Sprite longRestActionSelectedSprite;

	public Sprite longRestActionDisabledSprite;

	[Header("Preview")]
	public Sprite initiativeBackground;

	public Color previewRegularTextColor = new Color32(243, 221, 171, byte.MaxValue);

	public Color previewActiveTextColor = Color.white;

	public Color previewSelectedTextColor = Color.white;

	public Color previewDiscardedTextColor = new Color32(98, 98, 98, 128);

	public Color previewLostTextColor = new Color32(62, 44, 43, 128);

	public Color previewPermalostTextColor = new Color32(0, 0, 0, 41);

	[Range(0f, 1f)]
	public float previewDiscardedTextOpacity = 1f;

	[Range(0f, 1f)]
	public float previewLostTextOpacity = 0.75f;

	public Sprite regularPreviewBackground;

	public Sprite regularHighlightedPreviewBackground;

	public Sprite selectedPreviewBackground;

	public Sprite selectedHighlightedPreviewBackground;

	public Sprite activePreviewBackground;

	public Sprite activeHighlightedPreviewBackground;

	public Sprite discardedPreviewBackground;

	public Sprite lostPreviewBackground;

	public Sprite permalostPreviewBackground;

	[Space]
	public Sprite longRestPreviewBackground;

	public Sprite longRestHighlightedPreviewBackground;

	public Sprite longRestSelectedPreviewBackground;

	public Sprite longRestSelectedHighlightedPreviewBackground;

	public Sprite longRestDiscardedPreviewBackground;

	public Sprite longRestLostPreviewBackground;

	[HideInInspector]
	public string ID;

	public SpriteState GetActionSpriteState(bool isTopAction)
	{
		return new SpriteState
		{
			highlightedSprite = (isTopAction ? topActionHighlightSprite : bottomActionHighlightSprite),
			pressedSprite = (isTopAction ? topActionSelectedSprite : bottomActionSelectedSprite),
			disabledSprite = (isTopAction ? topActionDisabledSprite : bottomActionDisabledSprite),
			selectedSprite = (isTopAction ? topActionHighlightSprite : bottomActionHighlightSprite)
		};
	}

	public SpriteState GetLongRestActionSpriteState()
	{
		return new SpriteState
		{
			highlightedSprite = longRestActionHighlightSprite,
			pressedSprite = longRestActionSelectedSprite,
			disabledSprite = longRestActionDisabledSprite,
			selectedSprite = longRestActionHighlightSprite
		};
	}

	public SpriteState GetDefaultActionSpriteState(bool isTopAction)
	{
		return new SpriteState
		{
			highlightedSprite = (isTopAction ? defaultTopActionHighlightSprite : defaultBottomActionHighlightSprite),
			pressedSprite = (isTopAction ? defaultTopActionHighlightSprite : defaultBottomActionHighlightSprite),
			disabledSprite = (isTopAction ? defaultTopActionDisabledSprite : defaultBottomActionDisabledSprite),
			selectedSprite = (isTopAction ? defaultTopActionHighlightSprite : defaultBottomActionHighlightSprite)
		};
	}

	public Color GetPreviewTextColor(CardPileType type, bool isHighlighted)
	{
		Color result = type switch
		{
			CardPileType.Permalost => previewPermalostTextColor, 
			CardPileType.Lost => previewLostTextColor, 
			CardPileType.Round => previewSelectedTextColor, 
			CardPileType.Discarded => previewDiscardedTextColor, 
			CardPileType.Active => previewActiveTextColor, 
			_ => previewRegularTextColor, 
		};
		result = type switch
		{
			CardPileType.Permalost => new Color(result.r, result.g, result.b, isHighlighted ? 1f : previewLostTextOpacity), 
			CardPileType.Lost => new Color(result.r, result.g, result.b, isHighlighted ? 1f : previewLostTextOpacity), 
			CardPileType.Discarded => new Color(result.r, result.g, result.b, isHighlighted ? 1f : previewDiscardedTextOpacity), 
			_ => new Color(result.r, result.g, result.b, 1f), 
		};
		return result;
	}

	public Sprite GetPreviewBackground(CardPileType type, bool isHighlighted, bool isLongRest = false)
	{
		switch (type)
		{
		case CardPileType.Active:
			if (!isHighlighted)
			{
				return activePreviewBackground;
			}
			return activeHighlightedPreviewBackground;
		case CardPileType.Lost:
			if (!isLongRest)
			{
				return lostPreviewBackground;
			}
			return longRestLostPreviewBackground;
		case CardPileType.Discarded:
			if (!isLongRest)
			{
				return discardedPreviewBackground;
			}
			return longRestDiscardedPreviewBackground;
		case CardPileType.Round:
			if (isLongRest)
			{
				if (!isHighlighted)
				{
					return longRestSelectedPreviewBackground;
				}
				return longRestSelectedHighlightedPreviewBackground;
			}
			if (!isHighlighted)
			{
				return selectedPreviewBackground;
			}
			return selectedHighlightedPreviewBackground;
		case CardPileType.Permalost:
			if (!isLongRest)
			{
				return permalostPreviewBackground;
			}
			return longRestLostPreviewBackground;
		default:
			if (isLongRest)
			{
				if (!isHighlighted)
				{
					return longRestPreviewBackground;
				}
				return longRestHighlightedPreviewBackground;
			}
			if (!isHighlighted)
			{
				return regularPreviewBackground;
			}
			return regularHighlightedPreviewBackground;
		}
	}
}
