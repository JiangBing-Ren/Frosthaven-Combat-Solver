using DG.Tweening;
using UnityEngine;

public class UICampaignRewardRevealAnimator : CustomDOTweenAnimator
{
	[SerializeField]
	private RectTransform revealRect;

	[SerializeField]
	private float widthToExpand;

	public override void SetFinalValue()
	{
		revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (revealRect.parent as RectTransform).rect.width);
	}

	public override void RestorOriginalValue()
	{
		revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (revealRect.parent as RectTransform).rect.width - widthToExpand);
	}

	public override Tween BuildTweenAction(float duration)
	{
		return DOTween.To((float w) =>
		{
			revealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
		}, (revealRect.parent as RectTransform).rect.width - widthToExpand, (revealRect.parent as RectTransform).rect.width, duration).SetTarget(revealRect.gameObject);
	}
}
