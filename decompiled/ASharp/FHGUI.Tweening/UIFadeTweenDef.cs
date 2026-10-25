using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UIFadeTweenDef")]
public class UIFadeTweenDef : UITweenDef
{
	[Header("Fade")]
	[SerializeField]
	private float _fromValue;

	[SerializeField]
	private float _endValue = 1f;

	protected override Tween StartTween_Internal(RectTransform rt, float executionDuration)
	{
		if (rt.TryGetComponent<CanvasGroup>(out var component))
		{
			return component.DOFade(_endValue, executionDuration).From(_fromValue);
		}
		if (rt.TryGetComponent<Image>(out var component2))
		{
			return component2.DOFade(_endValue, executionDuration).From(_fromValue);
		}
		if (rt.TryGetComponent<TMP_Text>(out var component3))
		{
			return component3.DOFade(_endValue, executionDuration).From(_fromValue);
		}
		Log.GUIError("[" + name + "] No CanvasGroup/Image/TMP_Text component found on gameObject [" + rt.name + "]!", rt);
		return null;
	}

	public override void SnapToEndValue(RectTransform rt)
	{
		Image component2;
		TMP_Text component3;
		if (rt.TryGetComponent<CanvasGroup>(out var component))
		{
			component.alpha = _endValue;
		}
		else if (rt.TryGetComponent<Image>(out component2))
		{
			Color color = component2.color;
			color.a = _endValue;
			component2.color = color;
		}
		else if (rt.TryGetComponent<TMP_Text>(out component3))
		{
			Color color2 = component3.color;
			color2.a = _endValue;
			component3.color = color2;
		}
	}
}
