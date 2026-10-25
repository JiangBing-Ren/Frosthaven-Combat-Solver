using System;
using System.Collections.Generic;
using DG.Tweening;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UINewPerkInventorySlot : UIPerkInventorySlot
{
	[SerializeField]
	private float hoverBackgroundOpacity = 0.9f;

	[Header("Active")]
	[SerializeField]
	private float activeMaskBackgroundPosition;

	[SerializeField]
	private float activeHoverDuration;

	[SerializeField]
	private float activeBackgroundOpacity = 1f;

	[SerializeField]
	private Image pairHighlgiht;

	private Tween activeHoverAnimation;

	private bool isHovered;

	protected override void Awake()
	{
		base.Awake();
		background.material = new Material(background.material);
	}

	public override void Init(ICharacterService character, List<CharacterPerk> perks, Action<bool, UIPerkInventorySlot> onHoverCallback, Action<UIPerkInventorySlot> onSelectedCallback, bool isInteractable = true)
	{
		base.Init(character, perks, onHoverCallback, onSelectedCallback, isInteractable);
		isHovered = false;
		pairHighlgiht.enabled = (transform.GetSiblingIndex() + 1) % 2 == 0;
	}

	public override void OnHover(bool hovered)
	{
		isHovered = hovered;
		base.OnHover(hovered);
		RefreshActiveHover();
	}

	protected override void PreviewSelect(bool preview)
	{
		if (preview)
		{
			background.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait((character.PerkPoints > 0 || ActiveCounters > 0) ? character.CharacterModel : ECharacter.Elementalist, highlighted: true, character.Class.CustomCharacterConfig);
			background.color = ((character.PerkPoints > 0 || ActiveCounters > 0) ? UIInfoTools.Instance.White : Color.gray);
		}
		base.PreviewSelect(preview && character.PerkPoints > 0);
	}

	private void RefreshActiveHover()
	{
		if (ActiveCounters == TotalCounters)
		{
			background.SetAlpha(activeBackgroundOpacity);
			AnimateActiveHighlight(isHovered ? 0f : activeMaskBackgroundPosition, animate: true);
		}
		else
		{
			background.SetAlpha(isHovered ? hoverBackgroundOpacity : 0f);
			AnimateActiveHighlight(0f, animate: true);
		}
	}

	private void AnimateActiveHighlight(float to, bool animate = false)
	{
		CancelHoverActiveAnimation();
		Vector2 tiling = background.materialForRendering.GetTextureScale("_Mask");
		if (!animate)
		{
			background.materialForRendering.SetTextureScale("_Mask", new Vector2(to, tiling.y));
			return;
		}
		float duration = Math.Abs(to - tiling.x) / activeMaskBackgroundPosition * activeHoverDuration;
		activeHoverAnimation = DOTween.To((float val) =>
		{
			background.materialForRendering.SetTextureScale("_Mask", new Vector2(val, tiling.y));
		}, tiling.x, to, duration).SetTarget(background.gameObject).OnComplete(() =>
		{
			activeHoverAnimation = null;
		});
	}

	public override void RefreshCounters()
	{
		base.RefreshCounters();
		RefreshActiveHover();
	}

	private void CancelHoverActiveAnimation()
	{
		if (activeHoverAnimation != null)
		{
			activeHoverAnimation.Kill();
		}
		activeHoverAnimation = null;
	}

	protected virtual void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			CancelHoverActiveAnimation();
		}
	}
}
