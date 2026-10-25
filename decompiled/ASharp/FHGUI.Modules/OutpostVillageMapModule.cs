using FHGUI.EventChannels;
using FHGUI.Services;
using FHGUI.States;
using FHRL;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

namespace FHGUI.Modules;

public class OutpostVillageMapModule : FHMapViewModule
{
	[SerializeField]
	private OutpostMapScene _outpostMapScene;

	[SerializeField]
	private UIBuildingTooltipActionEventChannel _buildingTooltipActionEventChannel;

	[Header("DEMO")]
	[SerializeField]
	private FHButton _openDemoMarketingPromoButton;

	protected override bool PlaySoundOnEnable => false;

	public void SetSceneRootReference(OutpostMapScene outpostMapScene)
	{
		_outpostMapScene = outpostMapScene;
	}

	protected override void OnShow()
	{
		base.OnShow();
		_buildingTooltipActionEventChannel.OnSelectBuilding.AddListener(SetSelectedBuilding);
		_buildingTooltipActionEventChannel.OnDeselectBuilding.AddListener(DeselectBuilding);
		if ((bool)_openDemoMarketingPromoButton)
		{
			_openDemoMarketingPromoButton.onClick.AddListener(() =>
			{
				Singleton<FHMapChoreographer>.InstanceFast?.View.PushState(new DemoMarketingPopupViewState(), FHMapViewLayer.LayerType.Modal);
			});
			bool valueOrDefault = Singleton<FHMapChoreographer>.InstanceFast?.View.GetService<OutpostNotificationViewService>()?.IsDemoCompleted() == true;
			_openDemoMarketingPromoButton.gameObject.SetActive(valueOrDefault);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (_outpostMapScene != null)
		{
			_outpostMapScene.gameObject.SetActive(value: false);
		}
		if ((bool)_buildingTooltipActionEventChannel)
		{
			_buildingTooltipActionEventChannel.OnSelectBuilding.RemoveListener(SetSelectedBuilding);
			_buildingTooltipActionEventChannel.OnDeselectBuilding.RemoveListener(DeselectBuilding);
		}
		_openDemoMarketingPromoButton.RemoveAllEventListeners();
	}

	public void Init()
	{
		Singleton<FHMapChoreographer>.InstanceFast?.View.OutpostSceneController.OpenOutpostMap();
	}

	private void Update()
	{
		if (!EventSystem.current.IsPointerOverGameObject() && InputFHManager.GetMouse(MouseButton.Left) && (bool)_outpostMapScene.HoveredSlot)
		{
			_buildingTooltipActionEventChannel?.SelectBuilding(_outpostMapScene.HoveredSlot.BuildingID);
		}
	}

	public void SetSelectedBuilding(BuildingTypeID id)
	{
		_outpostMapScene.SetSelectedBuilding(id);
	}

	public void DeselectBuilding()
	{
		_outpostMapScene.DeselectBuilding();
	}
}
