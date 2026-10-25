using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public abstract class DOTweenGuiAnimationSettingMaterial<T> : DOTweenGUIAnimationSetting<T, GUIAnimationMaterialType, Graphic>
{
	[Header("Material properties")]
	public string property;

	public bool applyToOriginalMaterial;

	public override void Initialize()
	{
		base.Initialize();
		if (!applyToOriginalMaterial)
		{
			Target.material = new Material(Target.material);
		}
	}

	protected DOTweenGuiAnimationSettingMaterial(Graphic target, string property, float duration, GUIAnimationMaterialType type = GUIAnimationMaterialType.MaterialForRendering, float delay = 0f, Ease easing = Ease.Unset, bool applyToOriginalMaterial = false)
		: base(target, type, duration, delay, easing)
	{
		this.applyToOriginalMaterial = applyToOriginalMaterial;
		this.property = property;
	}
}
