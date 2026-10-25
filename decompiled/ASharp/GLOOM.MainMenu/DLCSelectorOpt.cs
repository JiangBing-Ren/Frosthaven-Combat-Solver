using System;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public class DLCSelectorOpt : SelectorOptData<DLCRegistry.EDLCKey>
{
	public bool IsOwned { get; private set; }

	public DLCSelectorOpt(DLCRegistry.EDLCKey key, bool isOwned)
		: base(key, (Func<string>)(() => LocalizationManager.GetTranslation(key.ToString())))
	{
		IsOwned = isOwned;
	}
}
