using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DOTweenGUIAnimationConfig
{
	[Header("Scale")]
	[SerializeField]
	private List<DOTweenGuiAnimationSettingScale> scaleEffects;

	[Header("Move")]
	[SerializeField]
	private List<DOTweenGuiAnimationSettingMove> moveEffects;

	[Header("Fade")]
	[SerializeField]
	private List<DOTweenGuiAnimationSettingFade> fadeEffects;

	[Header("Color")]
	[SerializeField]
	private List<DOTweenGUIAnimationSettingColor> colorEffects;

	[Header("Material")]
	[SerializeField]
	private List<DOTweenGuiAnimationSettingMaterialPropertyFloat> materialEffects;

	[Header("Custom")]
	[SerializeField]
	private List<CustomDOTweenGUIAnimationSetting> customEffects;

	[NonSerialized]
	private List<DOTweenGUIAnimationSetting> allSettings;

	public IReadOnlyList<DOTweenGUIAnimationSetting> GetSettings()
	{
		return allSettings ?? (allSettings = ((IEnumerable<DOTweenGUIAnimationSetting>)scaleEffects).Concat((IEnumerable<DOTweenGUIAnimationSetting>)moveEffects).Concat(materialEffects).Concat(fadeEffects)
			.Concat(customEffects)
			.Concat(colorEffects)
			.ToList());
	}
}
