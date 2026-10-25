using System.Linq;
using FHGUI.Common.Elements;
using FHGUI.DTO;
using FHRL.Buildings;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Tavern_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<Tavern_UIBuildInfoFunctionality.TavernStateParams>, IBuildInfoFunctionality
{
	public struct TavernStateParams : IBuildingInfoParams
	{
		public BuildingFunctionalityAction HoldFestival;

		public Tavern_BuildingTypeDef.FestivalSize CurrentFestival;

		public TavernFestivalData[] Festivals;
	}

	public struct TavernFestivalData
	{
		public enum State
		{
			None,
			Active,
			Locked
		}

		public int Gold;

		public int Morale;

		public State CurrentState;

		public Tavern_BuildingTypeDef.FestivalSize FestivalSize;

		public bool CanAfford;

		public bool CanHoldFestival;

		public uint LockedLevel;
	}

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private UIBuildInfoActionElement _holdFestivalButton;

	[SerializeField]
	private UIBuildingLockRequirement _lock;

	[Header("Festival Picker")]
	[SerializeField]
	private ResourceElement _morale;

	[SerializeField]
	private UIPickerButton _pickerButton;

	private TavernStateParams? _data;

	private BuildingStateDTO _building;

	private Tavern_BuildingTypeDef.FestivalSize _selectedFestival;

	private TavernFestivalData? SelectedFestivalData
	{
		get
		{
			if (_data.HasValue && _pickerButton.SelectedIndex != -1)
			{
				return _data.Value.Festivals[_pickerButton.SelectedIndex];
			}
			return null;
		}
	}

	private Tavern_BuildingTypeDef.FestivalSize SelectedFestival => SelectedFestivalData?.FestivalSize ?? Tavern_BuildingTypeDef.FestivalSize.None;

	private bool HasSelectedFestival => SelectedFestival != Tavern_BuildingTypeDef.FestivalSize.None;

	private void Awake()
	{
		_holdFestivalButton.Button.onClick.AddListener(HoldFestival);
	}

	private void OnDisable()
	{
		_data = null;
	}

	public void Init(BuildingStateDTO building, TavernStateParams data)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_building = building;
		_lock.Show(1u, building.Level);
		bool flag = data.CurrentFestival != Tavern_BuildingTypeDef.FestivalSize.None;
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_pickerButton.Button.interactable = building.IsActive && !building.IsWrecked && !building.IsRebuilding && !building.IsUnderConstruction && !flag;
		_holdFestivalButton.Button.interactable = !flag && HasSelectedFestival && (SelectedFestivalData?.CanAfford ?? false);
		if (data.CurrentFestival != Tavern_BuildingTypeDef.FestivalSize.None)
		{
			TavernFestivalData tavernFestivalData = data.Festivals.First((TavernFestivalData f) => f.FestivalSize == data.CurrentFestival);
			_morale.Init(tavernFestivalData.Morale);
		}
		else if (_pickerButton.SelectedIndex != -1)
		{
			ChooseFestival(data.Festivals[_pickerButton.SelectedIndex].FestivalSize);
		}
		else
		{
			_morale.Init(1);
		}
		SetFestivalPicker();
	}

	private void ChooseFestival(Tavern_BuildingTypeDef.FestivalSize size)
	{
		_selectedFestival = size;
		_morale.Init(_data.Value.Festivals.FirstOrDefault((TavernFestivalData f) => f.FestivalSize == size).Morale);
		_holdFestivalButton.Button.interactable = _data.Value.CurrentFestival == Tavern_BuildingTypeDef.FestivalSize.None;
	}

	private void SetFestivalPicker()
	{
		_pickerButton.Init(new UIPicker.UIPickerData
		{
			PickerElements = _data.Value.Festivals.Select((TavernFestivalData f) => new FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO
			{
				Icon = UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized,
				Quantity = f.Gold,
				Locked = (f.CurrentState == TavernFestivalData.State.Locked),
				Selected = (_selectedFestival == f.FestivalSize),
				Available = (f.CanAfford && f.CanHoldFestival),
				SlotData = f
			}).ToArray()
		}, OnChooseFestival, null, UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized);
		UIBuildingLockRequirement[] componentsInChildren = _pickerButton.Picker.GetComponentsInChildren<UIBuildingLockRequirement>();
		for (uint num = 0u; num < componentsInChildren.Length; num++)
		{
			componentsInChildren[num].Show(num + 1, _building.Level);
		}
	}

	private void OnChooseFestival(FHGUI.Common.Elements.UIPickerSlot slot, bool selected)
	{
		if (selected)
		{
			TavernFestivalData tavernFestivalData = ((slot.Data.SlotData is TavernFestivalData) ? ((TavernFestivalData)slot.Data.SlotData) : default(TavernFestivalData));
			if (tavernFestivalData.FestivalSize != Tavern_BuildingTypeDef.FestivalSize.None && _data.Value.CurrentFestival == Tavern_BuildingTypeDef.FestivalSize.None)
			{
				ChooseFestival(tavernFestivalData.FestivalSize);
				SetFestivalPicker();
			}
		}
	}

	private void HoldFestival()
	{
		if (_selectedFestival != Tavern_BuildingTypeDef.FestivalSize.None)
		{
			_data?.HoldFestival.OnClick?.Invoke(_selectedFestival);
		}
	}
}
