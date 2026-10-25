using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class OutpostVillageMapState : FHMapViewState
{
	private UIBuildingStateFilter _filter;

	private readonly PartyViewService _partyViewService;

	private readonly BuildingViewService _buildingViewService;

	private readonly OutpostNotificationViewService _notificationViewService;

	public override StateCategory Category => StateCategory.Outpost;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private OutpostVillageMapModule VillageMapModule => GetModule<OutpostVillageMapModule>();

	[Preserve]
	private BuildingInfoModule BuildingTooltips => GetModule<BuildingInfoModule>();

	[Preserve]
	private OutpostNotificationModule NotificationModule => GetModule<OutpostNotificationModule>();

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	public BuildingTypeID? SelectedBuilding { get; private set; }

	public OutpostVillageMapState(BuildingTypeID? selectedBuilding = null, UIBuildingStateFilter filter = UIBuildingStateFilter.None)
	{
		SelectedBuilding = selectedBuilding;
		_filter = filter;
		_partyViewService = View.GetService<PartyViewService>();
		_buildingViewService = View.GetService<BuildingViewService>();
		_notificationViewService = View.GetService<OutpostNotificationViewService>();
	}

	public override void OnShow()
	{
		UnsubscribeEvents();
		HeaderService.SetupHeader(HeaderModule);
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, MainViewSubCategories.None, _filter);
		_partyViewService.SetupPartyColumn(PartyModule);
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		NotificationModule.Init(_notificationViewService.GetAvailableNotifications(), new UIOutpostNotificationElement.OutpostNotificationParams
		{
			OnClickGroup = _notificationViewService.OnNotificationGroupClick,
			OnDismiss = OnDismiss,
			OnDismissGroup = OnDismissGroup,
			CanDismiss = _notificationViewService.CanDismiss,
			GetTooltip = _notificationViewService.BuildNotificationGroupTooltip
		}, _notificationViewService.GetStoryEventParams());
		SubscribeEvents();
		VillageMapModule.Init();
		if (SelectedBuilding.HasValue)
		{
			View.BuildingTooltipActionEventChannel.SelectBuilding(SelectedBuilding.Value);
		}
		else
		{
			View.BuildingTooltipActionEventChannel.DeselectBuilding();
		}
	}

	private void OnDismiss(OutpostNotificationDTO dto)
	{
		if (_notificationViewService.DismissNotification(dto))
		{
			View.RefreshView();
		}
	}

	private void OnDismissGroup(OutpostNotificationType dto)
	{
		if (_notificationViewService.DismissNotificationGroup(dto))
		{
			View.RefreshView();
		}
	}

	public override void OnHide()
	{
		SelectedBuilding = null;
		View.BuildingTooltipActionEventChannel.DeselectBuilding();
		View.GetService<BuildingViewService>()?.HideDonationModal();
		UnsubscribeEvents();
		View.OutpostSceneController.Reset();
	}

	private void OnBuildingSelected(BuildingTypeID id)
	{
		SelectedBuilding = id;
	}

	private void OnBuildingDeselected()
	{
		if (SelectedBuilding.HasValue)
		{
			SelectedBuilding = null;
		}
	}

	private void SubscribeEvents()
	{
		UnsubscribeEvents();
		View.BuildingTooltipActionEventChannel.OnSelectBuilding.AddListener(OnBuildingSelected);
		View.BuildingTooltipActionEventChannel.OnDeselectBuilding.AddListener(OnBuildingDeselected);
	}

	private void UnsubscribeEvents()
	{
		View.BuildingTooltipActionEventChannel.OnSelectBuilding.RemoveListener(OnBuildingSelected);
		View.BuildingTooltipActionEventChannel.OnDeselectBuilding.RemoveListener(OnBuildingDeselected);
	}

	private void SaveGame()
	{
		Singleton<FHMapChoreographer>.InstanceFast.AutoSave();
	}
}
