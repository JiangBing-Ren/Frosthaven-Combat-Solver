using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ShowCreditsPopupViewState : FHMapViewState
{
	private bool HasBeenShown;

	public override StateCategory Category => StateCategory.Outpost;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private ShowCreditsPopupModule CreditsPopupModule => View.Modules.ShowCreditsPopup;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	public override void OnShow()
	{
		if (!HasBeenShown)
		{
			HasBeenShown = true;
			View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
			ShowCreditsPopupModule.Params obj = new ShowCreditsPopupModule.Params
			{
				OnClose = PopThisState
			};
			CreditsPopupModule.Init(obj);
		}
	}
}
