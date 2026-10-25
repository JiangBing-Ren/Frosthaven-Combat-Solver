using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class OutpostConstructBuildingModule : MonoBehaviour
{
	[SerializeField]
	private UIConstructBuildingListElement _buildingPrefab;

	[SerializeField]
	private RectTransform _buildingsContainer;

	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private FHButton _previousArrow;

	[SerializeField]
	private FHButton _nextArrow;

	[Header("Filter")]
	[SerializeField]
	private RectTransform _filterContainer;

	[SerializeField]
	private FHToggle _allBuildingsFilter;

	[SerializeField]
	private FHToggle _activeBuildingsFilter;

	[SerializeField]
	private FHToggle _constructBuildingsFilter;

	[SerializeField]
	private FHToggle _upgradeBuildingsFilter;

	[SerializeField]
	private FHToggle _wreckedBuildingsFilter;

	private bool _active;

	private bool _init;

	private UIBuildingStateFilter _filter;

	private BuildingFlagAction _onSelect;

	private UISelectableChildElement[] _allBuildingsFilterSelectable;

	private List<BuildingStateDTO> _buildings = new List<BuildingStateDTO>();

	private List<BuildingStateDTO> _filtered = new List<BuildingStateDTO>();

	private List<UIConstructBuildingListElement> _buildingListElements = new List<UIConstructBuildingListElement>();

	public Action<UIBuildingStateFilter> OnFilterClicked;

	private bool _shouldFilter;

	public UIConstructBuildingListElement Selected { get; private set; }

	private void InitCache()
	{
		if (!_init)
		{
			_upgradeBuildingsFilter.onValueChanged.AddListener((bool v) =>
			{
				OnFilter(UIBuildingStateFilter.Upgradeable, v);
			});
			_constructBuildingsFilter.onValueChanged.AddListener((bool v) =>
			{
				OnFilter(UIBuildingStateFilter.Construct, v);
			});
			_wreckedBuildingsFilter.onValueChanged.AddListener((bool v) =>
			{
				OnFilter(UIBuildingStateFilter.Wrecked, v);
			});
			_activeBuildingsFilter.onValueChanged.AddListener((bool v) =>
			{
				OnFilter(UIBuildingStateFilter.Active, v);
			});
			_allBuildingsFilterSelectable = _filterContainer.GetComponentsInChildren<UISelectableChildElement>(includeInactive: true);
			_buildingListElements = _buildingsContainer.GetComponentsInChildren<UIConstructBuildingListElement>(includeInactive: true).ToList();
			_init = true;
		}
	}

	private void OnDisable()
	{
		_onSelect = null;
		Selected = null;
	}

	public void Toggle()
	{
		_active = !_active;
	}

	public void Init(List<BuildingStateDTO> buildings, BuildingFlagAction onSelect, UIBuildingStateFilter filter = UIBuildingStateFilter.All)
	{
		if (!_init)
		{
			InitCache();
		}
		_buildings.Clear();
		_buildings.AddRange(buildings);
		_onSelect = onSelect;
		SetFilter(filter);
		FilterData(buildings, filter);
	}

	private void FilterData(IList<BuildingStateDTO> buildings, UIBuildingStateFilter filter)
	{
		_filtered.Clear();
		_filtered.AddRange(filterBuildingData(_buildings, filter));
		HelperTools.NormalizePool(ref _buildingListElements, _buildingPrefab.gameObject, _buildingsContainer, _filtered.Count);
		setBuildingData(_filtered);
		_previousArrow.gameObject.SetActive(_filtered.Count > 6);
		_nextArrow.gameObject.SetActive(_filtered.Count > 6);
		IEnumerable<BuildingStateDTO> filterBuildingData(IList<BuildingStateDTO> allBuildings, UIBuildingStateFilter filter2)
		{
			foreach (BuildingStateDTO allBuilding in allBuildings)
			{
				if (FilterState(filter2, allBuilding))
				{
					yield return allBuilding;
				}
			}
		}
		void setBuildingData(List<BuildingStateDTO> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				_buildingListElements[i].Init(list[i], SelectBuilding);
			}
		}
	}

	private bool FilterState(UIBuildingStateFilter filter, BuildingStateDTO building)
	{
		if (filter == UIBuildingStateFilter.None)
		{
			return false;
		}
		if (filter.HasFlag(UIBuildingStateFilter.All))
		{
			return true;
		}
		if (filter.HasFlag(UIBuildingStateFilter.Active))
		{
			if (!building.IsWrecked)
			{
				if (!building.IsConstructed)
				{
					return building.ConstructionState == BuildingConstructionType.NewBuilding;
				}
				return true;
			}
			return false;
		}
		if (filter.HasFlag(UIBuildingStateFilter.Construct))
		{
			return !building.IsConstructed;
		}
		if (filter.HasFlag(UIBuildingStateFilter.Upgradeable))
		{
			if (building.IsConstructed && building.HasMoreUpgrades && !building.Def.CannotBuyUpgrades)
			{
				return building.HasUpgradeRequirements;
			}
			return false;
		}
		if (filter.HasFlag(UIBuildingStateFilter.Wrecked))
		{
			if (!building.IsWrecked)
			{
				return building.IsRebuilding;
			}
			return true;
		}
		return false;
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void OnFilter(UIBuildingStateFilter flag, bool value)
	{
		if (_init)
		{
			UIBuildingStateFilter filter = _filter;
			if (flag.HasFlag(UIBuildingStateFilter.All) || flag == UIBuildingStateFilter.None)
			{
				value = true;
				filter = UIBuildingStateFilter.All;
			}
			else
			{
				value = true;
				filter = (value ? flag : UIBuildingStateFilter.All);
			}
			if (flag == UIBuildingStateFilter.None)
			{
				filter = UIBuildingStateFilter.All;
			}
			SetFilter(filter);
			_shouldFilter = true;
			OnFilterClicked?.Invoke(filter);
		}
	}

	private void SetFilter(UIBuildingStateFilter filter)
	{
		if (_filter != filter)
		{
			_filter = filter;
			if (_filter == UIBuildingStateFilter.None)
			{
				_filter = UIBuildingStateFilter.Active;
			}
			if (_allBuildingsFilter != null)
			{
				_allBuildingsFilter.SetIsOnWithoutNotify(_filter.HasFlag(UIBuildingStateFilter.All));
			}
			_activeBuildingsFilter.SetIsOnWithoutNotify(_filter.HasFlag(UIBuildingStateFilter.Active));
			_constructBuildingsFilter.SetIsOnWithoutNotify(_filter.HasFlag(UIBuildingStateFilter.Construct));
			_upgradeBuildingsFilter.SetIsOnWithoutNotify(_filter.HasFlag(UIBuildingStateFilter.Upgradeable));
			_wreckedBuildingsFilter.SetIsOnWithoutNotify(_filter.HasFlag(UIBuildingStateFilter.Wrecked));
			UISelectableChildElement[] allBuildingsFilterSelectable = _allBuildingsFilterSelectable;
			for (int i = 0; i < allBuildingsFilterSelectable.Length; i++)
			{
				allBuildingsFilterSelectable[i].RefreshState();
			}
		}
	}

	private void Update()
	{
		if (_shouldFilter && !CanvasUpdateRegistry.IsRebuildingLayout())
		{
			FilterData(_buildings, _filter);
			_shouldFilter = false;
		}
	}

	private void SelectBuilding(UIConstructBuildingListElement element)
	{
		if (Selected != null)
		{
			Selected.SetSelectedState(selected: false);
			_onSelect?.Invoke(element.ID, isFlagTrue: false);
		}
		_onSelect?.Invoke(element.ID, isFlagTrue: true);
		Selected = element;
	}

	public void SetSelectedState(BuildingTypeID id, bool selected)
	{
		if (Selected != null && Selected.ID == id)
		{
			Selected.SetSelectedState(selected);
			Selected = (selected ? Selected : null);
			return;
		}
		UIConstructBuildingListElement uIConstructBuildingListElement = null;
		foreach (UIConstructBuildingListElement buildingListElement in _buildingListElements)
		{
			if (!buildingListElement.isActiveAndEnabled)
			{
				break;
			}
			if (buildingListElement.ID == id)
			{
				uIConstructBuildingListElement = buildingListElement;
				break;
			}
		}
		if (uIConstructBuildingListElement != null)
		{
			Selected?.SetSelectedState(selected: false);
			uIConstructBuildingListElement.SetSelectedState(selected);
			Selected = (selected ? uIConstructBuildingListElement : null);
		}
	}
}
