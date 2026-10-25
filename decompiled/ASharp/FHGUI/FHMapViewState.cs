using System;
using System.Linq;
using FHRL;

namespace FHGUI;

public abstract class FHMapViewState : FHViewStateBase
{
	public enum StateCategory
	{
		None,
		Init,
		Outpost,
		Characters,
		WorldMap,
		Roster,
		Shop,
		Briefing,
		Narration
	}

	public Func<bool> ValidationHandler;

	public Action OnValidFail;

	public virtual FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Base;

	public virtual StateCategory Category => StateCategory.None;

	public FHMapView View => Choreographer.View;

	public FHMapChoreographer Choreographer => Singleton<FHMapChoreographer>.InstanceFast;

	public CampaignController GameController => Choreographer.GameController;

	public MultiplayerGameController MultiplayerGameController => Choreographer.GameClient.GameController;

	public CampaignState GameState => GameController.State;

	public bool IsValid => ValidationHandler?.Invoke() ?? true;

	protected TModule GetModule<TModule>() where TModule : FHMapViewModule
	{
		TModule val = View.AllModules.FirstOrDefault((IUIViewModule m) => m.GetType() == typeof(TModule)) as TModule;
		if (val != null)
		{
			return val;
		}
		return View.AllModules.OfType<TModule>().First();
	}

	protected void PopThisState()
	{
		if (!View.TryPopState(this))
		{
			Log.GUIError($"Tried to pop [{GetType()}] state but it is not on top of the stack!");
		}
	}

	protected virtual void SavePayload()
	{
		Log.GUIError("[" + GetType().Name + "]: Saving empty payload! You must override the SavePayload method!");
	}

	protected virtual void LoadPayload()
	{
		Log.GUIError("[" + GetType().Name + "]: Loading empty payload! You must override the LoadPayload method!");
	}

	protected virtual void ClearPayload()
	{
		Log.GUIError("[" + GetType().Name + "]: Cannot clear payload! You must override the ClearPayload method!");
	}
}
