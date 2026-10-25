using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DOTweenGuiAnimationSettingFade : DOTweenGUIAnimationSetting<float, GUIAnimationFadeType, RectTransform>
{
	protected override Tween BuildTweenAction()
	{
		switch (Animation)
		{
		case GUIAnimationFadeType.CANVAS_GROUP:
		{
			CanvasGroup component2 = Target.GetComponent<CanvasGroup>();
			if (component2 != null)
			{
				return component2.DOFade(ToValue, Duration);
			}
			Log.GUIError("Error in DOTweenGuiAnimationSettingAlpha: Target (" + Target.name + ") doesn't have CanvasGroup component");
			return null;
		}
		case GUIAnimationFadeType.IMAGE:
		case GUIAnimationFadeType.GRAPHIC:
			return Target.GetComponent<Graphic>().DOFade(ToValue, Duration);
		case GUIAnimationFadeType.TEXT:
		{
			TextMeshProUGUI component = Target.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				return component.DOFade(ToValue, Duration).From(OriginalValue);
			}
			Log.GUIError("Error in DOTweenGuiAnimationSettingAlpha: Target (" + Target.name + ") doesn't have TextMeshProUGUI component");
			return null;
		}
		default:
			Log.GUIError("Effect not supported :" + Animation);
			return null;
		}
	}

	protected override void SetValue(float value)
	{
		switch (Animation)
		{
		case GUIAnimationFadeType.CANVAS_GROUP:
		{
			CanvasGroup component3 = Target.GetComponent<CanvasGroup>();
			if (component3 != null)
			{
				component3.alpha = value;
			}
			else
			{
				Log.GUIErrorFormat("Error in DOTweenGuiAnimationSettingAlpha: Target (" + Target.name + ") doesn't have CanvasGroup component");
			}
			break;
		}
		case GUIAnimationFadeType.IMAGE:
		case GUIAnimationFadeType.GRAPHIC:
		{
			Graphic component2 = Target.GetComponent<Graphic>();
			if (component2 == null)
			{
				Log.GUIErrorFormat("Error in DOTweenGuiAnimationSettingAlpha: Target (" + Target.name + ") doesn't have Graphic component");
				break;
			}
			Color color = component2.color;
			color.a = value;
			component2.color = color;
			break;
		}
		case GUIAnimationFadeType.TEXT:
		{
			TextMeshProUGUI component = Target.GetComponent<TextMeshProUGUI>();
			if (component == null)
			{
				Log.GUIErrorFormat("Error in DOTweenGuiAnimationSettingAlpha: Target (" + Target.name + ") doesn't have TextMeshProUGUI component");
			}
			else
			{
				component.SetAlpha(value);
			}
			break;
		}
		default:
			Log.GUIError("Effect not supported :" + Animation);
			break;
		}
	}

	public DOTweenGuiAnimationSettingFade(CanvasGroup target, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target.GetComponent<RectTransform>(), GUIAnimationFadeType.CANVAS_GROUP, duration, delay, easing)
	{
	}

	public DOTweenGuiAnimationSettingFade(Graphic target, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target.rectTransform, GUIAnimationFadeType.GRAPHIC, duration, delay, easing)
	{
	}
}
