using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIRotateTweenDef")]
public class UIRotateTweenDef : UITweenDef
{
	[Header("Rotate")]
	[SerializeField]
	private Vector3 _endValue = new Vector3(1f, 0f, 0f);

	[SerializeField]
	private RotateMode _rotateMode;

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		return rt.DOLocalRotate(_endValue, executionDuration, _rotateMode);
	}

	public override void SnapToEndValue(RectTransform rt)
	{
	}
}
