using FHGUI.Modules;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class StandaloneVideoViewState : FHMapViewState
{
	private string _url;

	private bool _fullscreen;

	private bool _closeOnComplete;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private StandaloneVideoModule StandaloneVideoModal => View.Modules.GenericModals.StandaloneVideoModal;

	public StandaloneVideoViewState(string url, bool fullscreen, bool closeOnComplete)
	{
		_url = url;
		_fullscreen = fullscreen;
		_closeOnComplete = closeOnComplete;
	}

	public override void OnShow()
	{
		StandaloneVideoModule.Params obj = new StandaloneVideoModule.Params
		{
			OnDone = PopThisState,
			Fullscreen = _fullscreen,
			URL = _url,
			ShowSkip = _fullscreen,
			AutoAdvance = _closeOnComplete
		};
		StandaloneVideoModal.Init(obj);
	}
}
