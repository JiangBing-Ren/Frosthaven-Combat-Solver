using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class DemoMarketingPopupViewState : FHMapViewState
{
	public override StateCategory Category => StateCategory.Outpost;

	[Preserve]
	private DemoMarketingPopupModule DemoMarketingPopup => View.Modules.DemoMarketingPopup;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		DemoMarketingPopupModule.Params obj = new DemoMarketingPopupModule.Params
		{
			OnClose = PopThisState,
			OnAddToWishList = () =>
			{
				Application.OpenURL("https://store.steampowered.com/app/2347080/Frosthaven/");
			}
		};
		DemoMarketingPopup.Init(obj);
	}
}
