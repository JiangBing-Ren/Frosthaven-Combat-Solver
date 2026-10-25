using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Card Skin", fileName = "Card Skin Config")]
public class AbilityCardUISkinConfig : ScriptableObject
{
	public AbilityCardUISkin Skin = new AbilityCardUISkin();

	[Header("Card styles")]
	public Color AddedEffectColor = Color.white;

	public Color ConsumeColor = Color.white;

	public Color DividerColor = Color.white;

	public Color SummonColor = Color.white;

	public Color initiativeColor => Skin.initiativeColor;

	public Sprite titleSprite => Skin.titleSprite;

	public Sprite buttonsHolderSprite => Skin.buttonsHolderSprite;

	public Sprite topActionRegularSprite => Skin.topActionRegularSprite;

	public Sprite topActionHighlightSprite => Skin.topActionHighlightSprite;

	public Sprite topActionSelectedSprite => Skin.topActionSelectedSprite;

	public Sprite topActionDisabledSprite => Skin.topActionDisabledSprite;

	public Sprite bottomActionRegularSprite => Skin.bottomActionRegularSprite;

	public Sprite bottomActionHighlightSprite => Skin.bottomActionHighlightSprite;

	public Sprite bottomActionSelectedSprite => Skin.bottomActionSelectedSprite;

	public Sprite bottomActionDisabledSprite => Skin.bottomActionDisabledSprite;

	public Sprite defaultTopActionRegularSprite => Skin.defaultTopActionRegularSprite;

	public Sprite defaultTopActionHighlightSprite => Skin.defaultTopActionHighlightSprite;

	public Sprite defaultTopActionDisabledSprite => Skin.defaultTopActionDisabledSprite;

	public Sprite defaultBottomActionRegularSprite => Skin.defaultBottomActionRegularSprite;

	public Sprite defaultBottomActionHighlightSprite => Skin.defaultBottomActionHighlightSprite;

	public Sprite defaultBottomActionDisabledSprite => Skin.defaultBottomActionDisabledSprite;

	public Sprite longRestActionRegularSprite => Skin.longRestActionRegularSprite;

	public Sprite longRestActionHighlightSprite => Skin.longRestActionHighlightSprite;

	public Sprite longRestActionSelectedSprite => Skin.longRestActionSelectedSprite;

	public Sprite longRestActionDisabledSprite => Skin.longRestActionDisabledSprite;

	public Sprite initiativeBackground => Skin.initiativeBackground;

	public Color previewRegularTextColor => Skin.previewRegularTextColor;

	public Color previewActiveTextColor => Skin.previewActiveTextColor;

	public Color previewSelectedTextColor => Skin.previewSelectedTextColor;

	public Color previewDiscardedTextColor => Skin.previewDiscardedTextColor;

	public Color previewLostTextColor => Skin.previewLostTextColor;

	public Color previewPermalostTextColor => Skin.previewPermalostTextColor;

	public float previewDiscardedTextOpacity => Skin.previewDiscardedTextOpacity;

	public float previewLostTextOpacity => Skin.previewLostTextOpacity;

	public Sprite regularPreviewBackground => Skin.regularPreviewBackground;

	public Sprite regularHighlightedPreviewBackground => Skin.regularHighlightedPreviewBackground;

	public Sprite selectedPreviewBackground => Skin.selectedPreviewBackground;

	public Sprite selectedHighlightedPreviewBackground => Skin.selectedHighlightedPreviewBackground;

	public Sprite activePreviewBackground => Skin.activePreviewBackground;

	public Sprite activeHighlightedPreviewBackground => Skin.activeHighlightedPreviewBackground;

	public Sprite discardedPreviewBackground => Skin.discardedPreviewBackground;

	public Sprite lostPreviewBackground => Skin.lostPreviewBackground;

	public Sprite permalostPreviewBackground => Skin.permalostPreviewBackground;

	public Sprite longRestPreviewBackground => Skin.longRestPreviewBackground;

	public Sprite longRestHighlightedPreviewBackground => Skin.longRestHighlightedPreviewBackground;

	public Sprite longRestSelectedPreviewBackground => Skin.longRestSelectedPreviewBackground;

	public Sprite longRestSelectedHighlightedPreviewBackground => Skin.longRestSelectedHighlightedPreviewBackground;

	public Sprite longRestDiscardedPreviewBackground => Skin.longRestDiscardedPreviewBackground;

	public Sprite longRestLostPreviewBackground => Skin.longRestLostPreviewBackground;
}
