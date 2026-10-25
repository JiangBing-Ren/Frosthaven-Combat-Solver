using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Stables_UIBuildInfoFunctionality : MonoBehaviour, IBuildInfoFunctionality<Stables_UIBuildInfoFunctionality.StablesStateParams>, IBuildInfoFunctionality
{
	public struct StablesStateParams : IBuildingInfoParams
	{
		public List<PetDTO> Pets;

		public int PetCapacity;

		public int MaxPetCapacity;

		public Func<int, uint> GetSlotUnlockAtLevel;
	}

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private UIStablesPetSlot _slotPrefab;

	private List<UIStablesPetSlot> _slots;

	private bool _initialized;

	private BuildingStateDTO _building;

	public BuildingId BuildingId => BuildingId.Stables;

	private void Awake()
	{
		InitCache();
	}

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_slots = _container.GetComponentsInChildren<UIStablesPetSlot>().ToList();
		}
	}

	public void Init(BuildingStateDTO building, StablesStateParams data)
	{
		InitCache();
		gameObject.SetActive(value: true);
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		UIHelperUtil.UpdateUIList(data.MaxPetCapacity, _slots, _slotPrefab, _container, onShow);
		void onShow(int i, UIStablesPetSlot item)
		{
			UIStablesPetSlot.Params obj = new UIStablesPetSlot.Params
			{
				Pet = ((i < data.Pets.Count) ? data.Pets[i] : default(PetDTO)),
				Locked = (i >= data.PetCapacity),
				CanUsePet = (building.IsConstructed && !building.IsWrecked),
				RequiredStablesLevel = data.GetSlotUnlockAtLevel(i)
			};
			_slots[i].Init(obj, _building.Level);
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
