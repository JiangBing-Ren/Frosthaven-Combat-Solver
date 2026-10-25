using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class DOTweenGuiAnimationSettingScale : DOTweenGUIAnimationSetting<Vector3, RectTransform>
{
	protected override Tween BuildTweenAction()
	{
		return Target.DOScale(ToValue, Duration);
	}

	protected override void SetValue(Vector3 value)
	{
		Target.localScale = value;
	}

	public DOTweenGuiAnimationSettingScale(RectTransform target, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target, duration, delay, easing)
	{
	}
}
