using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIScaleTweenDef")]
public class UIScaleTweenDef : UITweenDef
{
	[Header("Scale")]
	[SerializeField]
	private float _endValue = 1.2f;

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		return rt.DOScale(_endValue, executionDuration);
	}

	public override void SnapToEndValue(RectTransform rt)
	{
		rt.localScale = _endValue * Vector3.one;
	}
}
