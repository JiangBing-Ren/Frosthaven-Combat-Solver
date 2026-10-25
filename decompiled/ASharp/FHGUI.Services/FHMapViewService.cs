using FHRL;

namespace FHGUI.Services;

public abstract class FHMapViewService
{
	protected readonly FHMapView _view;

	public FHMapChoreographer Choreographer => _view.Choreographer;

	public DefRepository DefRepo => Choreographer.DefRepo;

	public CampaignController GameController => Choreographer.GameController;

	public MultiplayerGameController MultiplayerGameController => Choreographer.GameClient.GameController;

	public FHMapViewService(FHMapView view)
	{
		_view = view;
	}

	public virtual void OnDestroy()
	{
	}
}
