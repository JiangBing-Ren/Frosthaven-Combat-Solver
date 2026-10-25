using System;
using System.Linq;
using FHGUI.DTO;
using FHRL;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Garden_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<GardenStateParams>, IBuildInfoFunctionality
{
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private UIGardenSlot[] _slots;

	[SerializeField]
	private UIHerbPicker _herbPicker;

	[SerializeField]
	private GameObject _HintCannotPlant;

	private GardenStateParams _data;

	private BuildingStateDTO _building;

	private void Awake()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			UIGardenSlot obj = _slots[i];
			obj.OnSlotClick = (Action<UIGardenSlot>)Delegate.Combine(obj.OnSlotClick, new Action<UIGardenSlot>(OnSlotClick));
		}
		_herbPicker.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		_herbPicker.gameObject.SetActive(value: false);
	}

	private void OnSlotClick(UIGardenSlot slot)
	{
		int slotIndex = _slots.IndexOf(slot);
		if (_data.Plots.Length > slotIndex)
		{
			_herbPicker.Show(new UIHerbPicker.HerbPickerData
			{
				Locked = false,
				Resources = _data.Resources,
				Selected = _data.Plots[slotIndex].Herb,
				TitleLocKey = "GUI_herb_picker_title"
			}, (OutpostHerbType herb) =>
			{
				OnSelect(slotIndex, herb);
			});
		}
	}

	private void OnSelect(int slotIndex, OutpostHerbType herb)
	{
		GardenStateParams.GardenPlotData gardenPlotData = _data.Plots[slotIndex];
		_data.Plots[slotIndex].Herb = herb;
		int num;
		if (_data.PlantState == GardenState.PlantStateType.Plant)
		{
			BuildingStateDTO building = _building;
			num = ((!building.IsWrecked && !building.IsRebuilding && building.IsUnderConstruction) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		_slots[slotIndex].Init(new UIGardenSlot.UIGardenSlotData
		{
			Available = gardenPlotData.Unlocked,
			Herb = herb,
			Interactable = (flag && gardenPlotData.Unlocked),
			Locked = !gardenPlotData.Unlocked,
			LockLevel = gardenPlotData.UnlockLevel
		}, _building.Level);
		_data.Plant?.Invoke((from p in _data.Plots
			where p.Unlocked
			select p.Herb).ToArray());
	}

	public void Init(BuildingStateDTO building, GardenStateParams data)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_building = building;
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		int num;
		if (_data.PlantState == GardenState.PlantStateType.Plant)
		{
			BuildingStateDTO building2 = _building;
			num = ((!building2.IsWrecked && !building2.IsRebuilding && !building2.IsUnderConstruction) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		for (int i = 0; i < _slots.Length; i++)
		{
			GardenStateParams.GardenPlotData gardenPlotData = _data.Plots[i];
			_slots[i].Init(new UIGardenSlot.UIGardenSlotData
			{
				Available = gardenPlotData.Unlocked,
				Interactable = (flag && gardenPlotData.Unlocked),
				Herb = ((data.Plots.Length > i) ? gardenPlotData.Herb : OutpostHerbType.None),
				Locked = !gardenPlotData.Unlocked,
				LockLevel = gardenPlotData.UnlockLevel
			}, _building.Level);
		}
		if ((bool)_HintCannotPlant)
		{
			_HintCannotPlant.gameObject.SetActive(data.PlantState != GardenState.PlantStateType.Plant);
		}
	}
}
