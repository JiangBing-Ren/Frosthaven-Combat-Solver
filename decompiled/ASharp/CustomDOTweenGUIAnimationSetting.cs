using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class CustomDOTweenGUIAnimationSetting : DOTweenGUIAnimationSetting
{
	[SerializeField]
	private CustomDOTweenAnimator animation;

	public CustomDOTweenGUIAnimationSetting(float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(duration, delay, easing)
	{
	}

	protected override Tween BuildTweenAction()
	{
		return animation.BuildTweenAction(Duration);
	}

	public override void RestoreOriginalValue()
	{
		animation.RestorOriginalValue();
	}

	public override void SetFinalValue()
	{
		animation.SetFinalValue();
	}
}
