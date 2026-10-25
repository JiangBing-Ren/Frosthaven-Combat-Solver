using FHGUI.Common;
using FHGUI.Elements;

namespace FHGUI;

public class MonsterInnateAbilitiesUI : ListContainerBase<UIConditionEffectElement, object, StatusEffectDTO>
{
	protected override void InitElement(UIConditionEffectElement listElement, StatusEffectDTO elementParams, object listParams)
	{
		listElement.Init(elementParams);
	}
}
