using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.Elements;

public static class UIBuildingTooltipUtils
{
	public static void SetEffects(ref List<UIBuildingEffectRowElement> effects, GameObject prefab, RectTransform container, string activeEffects = null, string passiveEffects = null, string oneTimeEffects = null, string wreckedEffects = null)
	{
		bool flag = !string.IsNullOrEmpty(activeEffects);
		bool flag2 = !string.IsNullOrEmpty(passiveEffects);
		bool flag3 = !string.IsNullOrEmpty(oneTimeEffects);
		bool flag4 = !string.IsNullOrEmpty(wreckedEffects);
		int num = (flag ? 1 : 0);
		num += (flag2 ? 1 : 0);
		num += (flag3 ? 1 : 0);
		num += (flag4 ? 1 : 0);
		HelperTools.NormalizePool(ref effects, prefab, container, num);
		int num2 = 0;
		if (flag3)
		{
			effects[num2++].Init(oneTimeEffects);
		}
		if (flag)
		{
			effects[num2++].Init(activeEffects);
		}
		if (flag2)
		{
			effects[num2++].Init(passiveEffects);
		}
		if (flag4)
		{
			effects[num2++].Init(wreckedEffects);
		}
	}
}
