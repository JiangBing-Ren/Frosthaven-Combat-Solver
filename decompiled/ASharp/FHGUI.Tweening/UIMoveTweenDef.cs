using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIMoveTweenDef")]
public class UIMoveTweenDef : UITweenDef
{
	[Header("Move")]
	[SerializeField]
	private Vector3 _endValue = new Vector3(1f, 0f, 0f);

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		return rt.DOLocalMove(_endValue, executionDuration);
	}

	public override void SnapToEndValue(RectTransform rt)
	{
		rt.localPosition = _endValue;
	}
}
