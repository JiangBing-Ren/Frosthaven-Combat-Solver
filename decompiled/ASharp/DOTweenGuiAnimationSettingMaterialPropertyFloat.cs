using System;
using DG.Tweening;
using UnityEngine.UI;

[Serializable]
public class DOTweenGuiAnimationSettingMaterialPropertyFloat : DOTweenGuiAnimationSettingMaterial<float>
{
	protected override Tween BuildTweenAction()
	{
		if (!Target.material.HasProperty(property))
		{
			return null;
		}
		return DOTween.To(SetValue, Target.material.GetFloat(property), ToValue, Duration).SetTarget(Target.gameObject);
	}

	protected override void SetValue(float value)
	{
		switch (Animation)
		{
		case GUIAnimationMaterialType.Material:
			if (Target.material.HasProperty(property))
			{
				Target.material.SetFloat(property, value);
			}
			break;
		case GUIAnimationMaterialType.MaterialForRendering:
			if (Target.materialForRendering.HasProperty(property))
			{
				Target.materialForRendering.SetFloat(property, value);
			}
			break;
		default:
			Log.GUIError("Effect not supported :" + Animation);
			break;
		}
	}

	public DOTweenGuiAnimationSettingMaterialPropertyFloat(Graphic target, string property, float duration, GUIAnimationMaterialType type = GUIAnimationMaterialType.MaterialForRendering, float delay = 0f, Ease easing = Ease.Unset, bool applyToOriginalMaterial = false)
		: base(target, property, duration, type, delay, easing, applyToOriginalMaterial)
	{
	}
}
