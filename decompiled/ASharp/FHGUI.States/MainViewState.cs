using FHGUI.DTO;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class MainViewState : FHMapViewState
{
	private MainViewSubCategories _category;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	public MainViewState(MainViewSubCategories category)
	{
		_category = category;
	}

	public override void OnShow()
	{
		HeaderService.SetupHeader(HeaderModule);
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, _category);
	}
}
