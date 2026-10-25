using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class WorldViewState : FHMapViewState
{
	public override StateCategory Category => StateCategory.WorldMap;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Base;

	[Preserve]
	private WorldMapModule WorldMap => View.Modules.WorldMap.WorldMap;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		View.OutpostSceneController.OpenWorldMap();
	}

	public override void OnHide()
	{
		View.OutpostSceneController.Reset();
	}
}
