using System;
using FHGUI.DTO;
using FHRL;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Inn_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<Inn_UIBuildInfoFunctionality.InnStateParams>, IBuildInfoFunctionality
{
	public struct InnStateParams : IBuildingInfoParams
	{
		public uint CurrentGold;

		public uint GoldCost;

		public uint MaterialAmount;

		public OutpostMaterialType PickedMaterialType;

		public Action<OutpostMaterialType> OnPickMaterialType;

		public BuildingFunctionalityAction Functionality;

		public Predicate<OutpostMaterialType> CanBuy;
	}

	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private UIBuildInfoActionElement _functionality;

	[SerializeField]
	private FHButton _confirmButton;

	[SerializeField]
	private ResourceElement _goldCost;

	[SerializeField]
	private UIBuildingLockRequirement _lock;

	[Header("Picker")]
	[SerializeField]
	private BuildingResourcePicker _picker;

	private InnStateParams _data;

	public void Init(BuildingStateDTO building, InnStateParams data)
	{
		gameObject.SetActive(value: true);
		_data = data;
		int num;
		if (building.IsConstructed && !building.IsWrecked && !building.IsRebuilding)
		{
			BuildingFunctionalityAction functionality = _data.Functionality;
			num = ((functionality.IsInteractable && functionality.Charges > 0) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		_lock.Show(1u, building.Level);
		_picker.Init(new BuildingResourcePicker.ResourcePickerData
		{
			CurrentResources = new OutpostResources
			{
				Lumber = 1,
				Metal = 1,
				Hide = 1
			},
			MaterialAmount = _data.MaterialAmount,
			OnPickMaterialType = _data.OnPickMaterialType,
			PickedMaterialType = _data.PickedMaterialType
		});
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		if (_goldCost != null)
		{
			_goldCost.Init(_data.GoldCost, _data.CurrentGold >= _data.GoldCost);
		}
		_picker.Interactable = flag;
		_picker.RefreshPicker();
		_functionality.Init(_data.Functionality);
		_confirmButton.interactable = flag && _data.PickedMaterialType != OutpostMaterialType.None;
	}
}
