using FHGUI.Modules;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class InitViewState : FHMapViewState
{
	public override StateCategory Category => StateCategory.Init;

	[Preserve]
	private UIEmptyModule Empty => View.Modules.EmptyModule;

	public override void OnShow()
	{
		View.DecideViewStateFromGameState();
	}
}
