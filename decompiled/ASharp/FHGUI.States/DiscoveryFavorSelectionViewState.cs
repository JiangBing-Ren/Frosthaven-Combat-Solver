using System;
using System.Linq;
using FHGUI.Common.Elements;
using FHGUI.Modals;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class DiscoveryFavorSelectionViewState : FHMapViewState
{
	private DiscoveryFavorModal.Params _originalData;

	private OutpostResources _selected;

	public override StateCategory Category => StateCategory.Briefing;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private DiscoveryFavorModal DiscoveryFavorModal => View.Modules.BriefingModules.DiscoveryFavorModal;

	private BattleBriefingViewService _briefingViewService => View.GetService<BattleBriefingViewService>();

	private DefRepository _defRepository => View.GameController.DefRepository;

	public DiscoveryFavorSelectionViewState()
	{
		_originalData = GetModalDataFromGameState();
	}

	public override void OnShow()
	{
		UpdateModule();
	}

	private void UpdateModule()
	{
		int num = _originalData.PickerData.PickerElements.Count((FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO p) => p.Selected);
		int total = _selected.Total;
		int num2 = num - total;
		DiscoveryFavorModal.Init(new DiscoveryFavorModal.Params
		{
			CurrentPoints = _originalData.CurrentPoints + num2,
			MaxPoints = _originalData.MaxPoints,
			PickerData = new UIPicker.UIPickerData
			{
				targetPoint = _originalData.PickerData.targetPoint,
				TitleLocKey = _originalData.PickerData.TitleLocKey,
				PickerElements = _originalData.PickerData.PickerElements.Select((FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO slot, int i) => new FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO
				{
					SlotData = slot.SlotData,
					Selected = HasSelected(slot),
					Available = IsAvailable(slot),
					Icon = slot.Icon,
					Locked = slot.Locked,
					Quantity = slot.Quantity,
					Tooltip = slot.Tooltip
				}).ToArray()
			}
		}, OnClose, OnConfirm, OnSelect, ValidateSelect, CanConfirmState());
	}

	private bool ValidateSelect(FHGUI.Common.Elements.UIPickerSlot slot)
	{
		if (GetCurrentPoints() == 0 && !HasSelected(slot.Data))
		{
			return false;
		}
		return true;
	}

	private int GetCurrentPoints()
	{
		int num = _originalData.PickerData.PickerElements.Count((FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO p) => p.Selected);
		int total = _selected.Total;
		int num2 = num - total;
		return _originalData.CurrentPoints + num2;
	}

	private void OnSelect(FHGUI.Common.Elements.UIPickerSlot slot, bool selected)
	{
		if (selected)
		{
			if (GetCurrentPoints() == 0)
			{
				UpdateModule();
				return;
			}
			object slotData = slot.Data.SlotData;
			if (!(slotData is OutpostHerbType outpostHerbType))
			{
				if (slotData is OutpostMaterialType outpostMaterialType && !_selected.HasMaterial(outpostMaterialType))
				{
					_selected.SetMaterialAmount(outpostMaterialType, 1);
				}
			}
			else if (!_selected.HasHerb(outpostHerbType))
			{
				_selected.SetHerbAmount(outpostHerbType, 1);
			}
		}
		else
		{
			object slotData = slot.Data.SlotData;
			if (!(slotData is OutpostHerbType m))
			{
				if (slotData is OutpostMaterialType m2)
				{
					_selected.SetMaterialAmount(m2, 0);
				}
			}
			else
			{
				_selected.SetHerbAmount(m, 0);
			}
		}
		UpdateModule();
	}

	private void OnConfirm()
	{
		if (CanConfirmState())
		{
			_briefingViewService.StoreDiscoveryFavorState(_selected);
			PopThisState();
		}
	}

	private void OnClose()
	{
		if (HasChangedState())
		{
			string translation = LocalizationManager.GetTranslation("GUI_HANDMANAGEMENT_WARNING_UNSAVEDCHANGES");
			string translation2 = LocalizationManager.GetTranslation("GUI_CONFIRM");
			Singleton<UIConfirmationBoxManager>.InstanceFast.ShowGenericConfirmation(translation2, translation, PopThisState);
		}
		else
		{
			PopThisState();
		}
	}

	public override void OnHide()
	{
		_selected = new OutpostResources(0);
	}

	private DiscoveryFavorModal.Params GetModalDataFromGameState()
	{
		uint maxPotentialFavorPoints = _briefingViewService.GetMaxPotentialFavorPoints();
		int num = (int)(maxPotentialFavorPoints - _briefingViewService.GetFavorDifficulty());
		OutpostResources resourcesCanUseForDiscoveryFavor = View.GameController.ScenarioController.GetResourcesCanUseForDiscoveryFavor();
		_selected = new OutpostResources(0);
		foreach (ScenarioFavorToBeUsed item in View.GameController.State.ActiveScenario.FavorsToUse.Where((ScenarioFavorToBeUsed m) => m.type == ScenarioFavorType.Discovery))
		{
			if (Enum.TryParse<OutpostMaterialType>(item.param1, out var result))
			{
				_selected.AddMaterial(result);
			}
			if (Enum.TryParse<OutpostHerbType>(item.param1, out var result2))
			{
				_selected.AddHerb(result2);
			}
		}
		UIPicker.UIPickerData resourcePickerAvailableOnly = UIPicker.UIPickerData.GetResourcePickerAvailableOnly(resourcesCanUseForDiscoveryFavor, "GUI_RESOURCE_ALLOCATION_ADD_RESOURCE", skipQuantity: true);
		FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO[] pickerElements = resourcePickerAvailableOnly.PickerElements;
		foreach (FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO uIPickerSlotDTO in pickerElements)
		{
			object slotData = uIPickerSlotDTO.SlotData;
			if (num == 0)
			{
				uIPickerSlotDTO.Available = false;
			}
			uIPickerSlotDTO.Selected = (slotData is OutpostHerbType herb && _selected.HasHerb(herb)) || (slotData is OutpostMaterialType material && _selected.HasMaterial(material));
		}
		return new DiscoveryFavorModal.Params
		{
			CurrentPoints = num,
			MaxPoints = maxPotentialFavorPoints,
			PickerData = resourcePickerAvailableOnly
		};
	}

	private bool HasSelected(FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO slot)
	{
		object slotData = slot.SlotData;
		if (!(slotData is OutpostHerbType herb))
		{
			if (slotData is OutpostMaterialType material)
			{
				return _selected.HasMaterial(material);
			}
			return false;
		}
		return _selected.HasHerb(herb);
	}

	private bool IsAvailable(FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO slot)
	{
		if (GetCurrentPoints() <= 0)
		{
			return HasSelected(slot);
		}
		return true;
	}

	private bool HasChangedState()
	{
		for (int i = 0; i < _originalData.PickerData.PickerElements.Length; i++)
		{
			FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO uIPickerSlotDTO = _originalData.PickerData.PickerElements[i];
			bool flag = HasSelected(uIPickerSlotDTO);
			if ((uIPickerSlotDTO.Selected && !flag) || (!uIPickerSlotDTO.Selected & flag))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanConfirmState()
	{
		return HasChangedState();
	}
}
