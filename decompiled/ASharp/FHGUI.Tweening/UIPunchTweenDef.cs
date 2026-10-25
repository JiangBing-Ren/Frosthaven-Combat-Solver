using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIPunchTweenDef")]
public class UIPunchTweenDef : UITweenDef
{
	[Header("Punch")]
	[SerializeField]
	private Vector3 _punchValue = new Vector3(0.2f, 0.2f, 0.2f);

	[SerializeField]
	private int _vibrato = 10;

	[SerializeField]
	private float _elasticity = 1f;

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		return rt.DOPunchScale(_punchValue, executionDuration, _vibrato, _elasticity);
	}

	public override void SnapToEndValue(RectTransform rt)
	{
	}
}
