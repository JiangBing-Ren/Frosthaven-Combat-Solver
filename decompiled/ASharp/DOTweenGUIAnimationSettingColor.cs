using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DOTweenGUIAnimationSettingColor : DOTweenGUIAnimationSetting<Color, GUIAnimationColorType, RectTransform>
{
	protected override Tween BuildTweenAction()
	{
		switch (Animation)
		{
		case GUIAnimationColorType.GRAPHIC:
			return Target.GetComponent<Graphic>().DOColor(ToValue, Duration);
		case GUIAnimationColorType.TEXT:
		{
			TextMeshProUGUI component = Target.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				return component.DOColor(ToValue, Duration);
			}
			Log.GUIErrorFormat("Error in DOTweenGUIAnimationSettingColor: Target ({0}) doesn't have TextMeshProUGUI component", Target.name);
			return null;
		}
		default:
			Log.GUIError("Effect not supported :" + Animation);
			return null;
		}
	}

	protected override void SetValue(Color value)
	{
		switch (Animation)
		{
		case GUIAnimationColorType.GRAPHIC:
		{
			Graphic component2 = Target.GetComponent<Graphic>();
			if (component2 == null)
			{
				Log.GUIErrorFormat("Error in DOTweenGUIAnimationSettingColor: Target ({0}) doesn't have Graphic component", Target.name);
			}
			else
			{
				component2.color = value;
			}
			break;
		}
		case GUIAnimationColorType.TEXT:
		{
			TextMeshProUGUI component = Target.GetComponent<TextMeshProUGUI>();
			if (component == null)
			{
				Log.GUIErrorFormat("Error in DOTweenGUIAnimationSettingColor: Target ({0}) doesn't have TextMeshProUGUI component", Target.name);
			}
			else
			{
				component.color = value;
			}
			break;
		}
		default:
			Log.GUIError("Effect not supported :" + Animation);
			break;
		}
	}

	public DOTweenGUIAnimationSettingColor(RectTransform target, GUIAnimationColorType type, float duration, float delay = 0f, Ease easing = Ease.Unset)
		: base(target, type, duration, delay, easing)
	{
	}
}
