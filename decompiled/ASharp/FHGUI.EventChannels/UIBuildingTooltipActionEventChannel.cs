using System;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using Libs.GameConsole;
using UnityEngine;
using UnityEngine.Events;

namespace FHGUI.EventChannels;

[CreateAssetMenu(menuName = "UI Events/Building Events Channel", fileName = "UIBuildingEventsChannel")]
public class UIBuildingTooltipActionEventChannel : ScriptableObject
{
	[NonSerialized]
	public UnityEvent<BuildingTypeID> OnSelectBuilding;

	[NonSerialized]
	public UnityEvent OnDeselectBuilding;

	private FHMapView _view;

	public BuildingTypeID? SelectedBuilding { get; private set; }

	[ConsoleCommand(Command = "show_build_screen")]
	public static void ShowBuildScreen_Console(IConsole console, uint id)
	{
		Singleton<FHMapChoreographer>.InstanceFast.View.BuildingTooltipActionEventChannel.ShowBuildingInfo(new BuildingTypeID(id));
	}

	private void OnEnable()
	{
		if (OnSelectBuilding == null)
		{
			OnSelectBuilding = new UnityEvent<BuildingTypeID>();
		}
		if (OnDeselectBuilding == null)
		{
			OnDeselectBuilding = new UnityEvent();
		}
	}

	private void OnDisable()
	{
		if (OnSelectBuilding != null)
		{
			OnSelectBuilding.RemoveAllListeners();
		}
		if (OnDeselectBuilding != null)
		{
			OnDeselectBuilding.RemoveAllListeners();
		}
	}

	public void SetReference(FHMapView view)
	{
		if (_view != null && _view != view)
		{
			Log.GUIWarning(name + " already has FHMapView referenced!", this);
		}
		_view = view;
	}

	public void SelectBuilding(BuildingTypeID id)
	{
		ShowBuildingInfo(id);
		SelectedBuilding = id;
		OnSelectBuilding?.Invoke(id);
	}

	public void DeselectBuilding()
	{
		if (SelectedBuilding.HasValue)
		{
			HideTooltip();
			SelectedBuilding = null;
			OnDeselectBuilding?.Invoke();
		}
	}

	public void ShowBuildingInfo(BuildingTypeID id)
	{
		BuildingViewService service = _view.GetService<BuildingViewService>();
		BuildingStateDTO buildingById = service.GetBuildingById(id);
		_view.Modules.OutpostMap.BuildingInfoModule.ShowInfo(new BuildingInfoModule.BuildingInfoParams
		{
			Building = buildingById,
			StateParams = service.GetBuildingStateParams(buildingById),
			InfoParams = service.GetBuildingInfoParams(buildingById),
			OnHide = OnTooltipHide
		});
	}

	private void OnTooltipHide()
	{
		DeselectBuilding();
	}

	public void HideTooltip()
	{
		if (_view.Modules.OutpostMap.BuildingInfoModule.Active)
		{
			_view.Modules.OutpostMap.BuildingInfoModule.Hide();
		}
	}
}
