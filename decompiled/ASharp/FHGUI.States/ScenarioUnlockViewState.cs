using System;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ScenarioUnlockViewState : FHMapViewState
{
	public Action OnDone;

	private int[] _scenarios;

	private int _destinationId;

	private bool _fromMidPoint;

	private bool _popStateOnDone;

	public override StateCategory Category => StateCategory.WorldMap;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Base;

	[Preserve]
	private WorldMapModule WorldMap => View.Modules.WorldMap.WorldMap;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	private ScenarioViewService ScenarioService => View.GetService<ScenarioViewService>();

	public ScenarioUnlockViewState(int[] scenarios, bool popStateOnDone = true)
	{
		_scenarios = scenarios;
		_popStateOnDone = popStateOnDone;
	}

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		View.OutpostSceneController.OpenWorldMap();
		View.OutpostSceneController.WorldMapScene.ShowUnlockScenarios(_scenarios, OnFinish);
	}

	private void OnFinish()
	{
		OnDone?.Invoke();
		if (_popStateOnDone)
		{
			PopThisState();
		}
	}

	public override void OnHide()
	{
		View.OutpostSceneController.Reset();
	}
}
