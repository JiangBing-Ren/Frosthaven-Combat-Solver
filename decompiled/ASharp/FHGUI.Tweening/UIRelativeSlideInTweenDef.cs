using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIRelativeSlideInTweenDef")]
public class UIRelativeSlideInTweenDef : UITweenDef
{
	[Header("Relative Slide")]
	[SerializeField]
	private Vector2 _relativeStartValue = new Vector2(0f, 0f);

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		return rt.DOAnchorPos(_relativeStartValue, executionDuration).From(isRelative: true);
	}

	public override void SnapToEndValue(RectTransform rt)
	{
	}
}
