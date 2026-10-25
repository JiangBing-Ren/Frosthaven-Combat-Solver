using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIGradientTweenDef")]
public class UIGradientTweenDef : UITweenDef
{
	[Header("Gradient")]
	[SerializeField]
	private Gradient _gradient;

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		if (rt.TryGetComponent<Image>(out var component))
		{
			return component.DOGradientColor(_gradient, executionDuration);
		}
		Log.GUIError("[" + name + "] No Image component found on gameObject [" + rt.name + "]!", rt);
		return null;
	}

	public override void SnapToEndValue(RectTransform rt)
	{
		if (rt.TryGetComponent<Image>(out var component))
		{
			component.color = _gradient.colorKeys.Last().color;
		}
	}
}
