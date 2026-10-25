using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Common;

public class UIGlossaryContainer : ListContainerBase<UIGlossaryText, object, UIGlossaryData>
{
	[SerializeField]
	private UIAutoSeparators _autoSeparators;

	protected override void InitElement(UIGlossaryText listElement, UIGlossaryData elementParams, object listParams)
	{
		listElement.Init(elementParams);
	}

	protected override void AfterInit_Impl(object listParams)
	{
		base.AfterInit_Impl(listParams);
		_autoSeparators.ForceRebuild();
	}
}
