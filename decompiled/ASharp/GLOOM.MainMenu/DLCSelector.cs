using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;

namespace GLOOM.MainMenu;

public class DLCSelector : SelectorWrapper<DLCRegistry.EDLCKey>
{
	public DLCSelector(ExtendedDropdown dropdown)
		: base((TMP_Dropdown)dropdown, (List<SelectorOptData<DLCRegistry.EDLCKey>>)null)
	{
		dropdown.OnCreatedOptions.AddListener(OnCreatedOptions);
	}

	private void OnCreatedOptions()
	{
		DLCDropdownItem[] componentsInChildren = dropdown.GetComponentsInChildren<DLCDropdownItem>();
		for (int i = 0; i < dropdown.options.Count; i++)
		{
			DLCSelectorOpt dLCSelectorOpt = (DLCSelectorOpt)dropdown.options[i];
			componentsInChildren[i].IsOwned = dLCSelectorOpt.IsOwned;
		}
	}
}
