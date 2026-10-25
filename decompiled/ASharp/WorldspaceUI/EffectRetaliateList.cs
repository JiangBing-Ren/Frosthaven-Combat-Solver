using System.Collections.Generic;
using FHGUI.Elements;

namespace WorldspaceUI;

public class EffectRetaliateList : ListContainerBase<EffectRetaliate, Dictionary<int, int>, KeyValuePair<int, int>>
{
	protected override void InitElement(EffectRetaliate listElement, KeyValuePair<int, int> rangeDamagePair, Dictionary<int, int> listParams)
	{
		listElement.Initialise();
		listElement.SetValues(rangeDamagePair.Value, rangeDamagePair.Key);
	}

	public void ActivateEffects(int distanceToAttacker)
	{
		foreach (EffectRetaliate element in Elements)
		{
			if (element.Range >= distanceToAttacker)
			{
				element.Activate();
			}
		}
	}

	public void WarnEffects(bool active, int distanceToAttacker)
	{
		foreach (EffectRetaliate element in Elements)
		{
			element.Warn(active && element.Range >= distanceToAttacker);
		}
	}

	public void CancelEffects()
	{
		foreach (EffectRetaliate element in Elements)
		{
			element.CancelEffectRetaliateTweens();
		}
	}
}
