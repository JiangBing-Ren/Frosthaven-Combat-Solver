using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Barracks_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<BarracksStateParams>, IBuildInfoFunctionality
{
	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private TextLocalizedListener _staticEffect;

	[Header("Soldiers")]
	[SerializeField]
	private TMP_Text _soldierCapacity;

	[SerializeField]
	private RectTransform _soldierContainer;

	[SerializeField]
	private GameObject _soldierPrefab;

	[Header("Functionality")]
	[SerializeField]
	private UIBuildInfoActionElement _trainButton;

	[SerializeField]
	private ResourceElement _goldCost;

	[SerializeField]
	private ResourceElement _soldiers;

	[Header("Picker")]
	[SerializeField]
	private BuildingResourcePicker _picker;

	private BarracksStateParams _data;

	public void Init(BuildingStateDTO building, BarracksStateParams data)
	{
		gameObject.SetActive(value: true);
		_data = data;
		bool flag = building.IsConstructed && !building.IsWrecked && !building.IsRebuilding;
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_staticEffect.SetParam("0", data.CurrentDefense);
		_picker.Init(new BuildingResourcePicker.ResourcePickerData
		{
			CurrentResources = _data.CurrentResources,
			MaterialAmount = _data.MaterialCost,
			OnPickMaterialType = _data.OnPickMaterialType,
			PickedMaterialType = _data.PickedMaterialType
		});
		_picker.Interactable = flag && _data.CanTrainSoldier;
		SetSoldiers();
		_goldCost.Init(_data.GoldCost, _data.CurrentGold >= _data.GoldCost);
		_soldiers.Init(_data.SoldierPerBuy);
		_trainButton.Init(data.TrainAction);
		_picker.RefreshPicker();
	}

	private void SetSoldiers()
	{
		List<UIImageSpriteStateController> pool = _soldierContainer.GetComponentsInChildren<UIImageSpriteStateController>(includeInactive: true).ToList();
		HelperTools.NormalizePool(ref pool, _soldierPrefab, _soldierContainer, (int)_data.MaxSoldierCount);
		for (int i = 1; i <= _data.MaxSoldierCount; i++)
		{
			pool[i - 1].SetState((i > _data.SoldierCount) ? IFHSelectable.SelectionState.Disabled : IFHSelectable.SelectionState.Normal);
		}
		_soldierCapacity.text = FHUILocFormatting.PopulateParams("GUI_BARRACKS_SOLDIERCAPACITY", _data.SoldierCount, _data.MaxSoldierCount);
	}
}
