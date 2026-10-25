using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using UnityEngine;

public class OutpostQuickAccessBarModule : FHMapViewModule
{
	[Header("Side buttons")]
	[Header("Outpost")]
	[SerializeField]
	private FHToggle _buildButton;

	[Header("Build filters")]
	[SerializeField]
	private GameObject _activeFilter;

	[SerializeField]
	private GameObject _underConstructionFilter;

	[SerializeField]
	private GameObject _newFilter;

	[SerializeField]
	private GameObject _wreckedFilter;

	[SerializeField]
	private FHButton _worldMap;

	[SerializeField]
	private GameObject _scenarioNotification;

	[Header("World map")]
	[SerializeField]
	private FHButton _outpost;

	[SerializeField]
	private GameObject _sled;

	[SerializeField]
	private GameObject _climbingGear;

	[SerializeField]
	private GameObject _boat;

	private MainViewSubCategoryTab[] _tabs;

	private QuickAccessBarDTO _data;

	private Action _onBuildButtonClicked;

	private Action _onOutpostButtonClicked;

	private Action _onWorldMapButtonClicked;

	private Action _onHide;

	private BuildingFlagAction _onBuildingSelected;

	private readonly List<BuildingStateDTO> _buildings = new List<BuildingStateDTO>();

	protected override bool PlaySoundOnEnable => false;

	[field: SerializeField]
	public OutpostConstructBuildingModule BuildMenu { get; private set; }

	public FHToggle BuildButton => _buildButton;

	public FHButton WorldMapButton => _worldMap;

	private void Awake()
	{
		_tabs = GetComponentsInChildren<MainViewSubCategoryTab>(includeInactive: true);
		MainViewSubCategoryTab[] tabs = _tabs;
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].Clicked += OnMainTabClicked;
		}
		_buildButton.onValueChanged.AddListener(OnBuildingButtonClick);
		_outpost.onClick.AddListener(() =>
		{
			_onOutpostButtonClicked?.Invoke();
		});
		_worldMap.onClick.AddListener(() =>
		{
			_onWorldMapButtonClicked?.Invoke();
		});
	}

	protected override void OnHide()
	{
		base.OnHide();
		_data = null;
		_onBuildButtonClicked = null;
		_onOutpostButtonClicked = null;
		_onWorldMapButtonClicked = null;
		_onBuildingSelected = null;
		BuildMenu.gameObject.SetActive(value: false);
	}

	private void OnBuildingButtonClick(bool active)
	{
		if (BuildMenu.gameObject.activeSelf)
		{
			HideBuildMenu();
		}
		else
		{
			ShowBuildMenu();
		}
		_onBuildButtonClicked?.Invoke();
	}

	private void ShowBuildMenu(UIBuildingStateFilter filter = UIBuildingStateFilter.Active)
	{
		BuildMenu.gameObject.SetActive(value: true);
		_activeFilter.SetActive(value: true);
		_underConstructionFilter.SetActive(value: true);
		_newFilter.SetActive(value: true);
		_wreckedFilter.SetActive(value: true);
		BuildMenu.Init(_buildings, _onBuildingSelected, filter);
		_buildButton.SetIsOnWithoutNotify(value: true);
	}

	private void HideBuildMenu()
	{
		BuildMenu.gameObject.SetActive(value: false);
		_activeFilter.SetActive(value: false);
		_underConstructionFilter.SetActive(value: false);
		_newFilter.SetActive(value: false);
		_wreckedFilter.SetActive(value: false);
		_onHide?.Invoke();
		_buildButton.SetIsOnWithoutNotify(value: false);
	}

	public void Init(QuickAccessBarDTO data, IEnumerable<BuildingStateDTO> allBuildings, QuickAccessActionsDTO actionsDto, bool isLocked, MainViewSubCategories[] allowedInteractions, Action onHideAction)
	{
		_data = data;
		_buildings.Clear();
		_buildings.AddRange(allBuildings);
		_buildButton.SetIsOnWithoutNotify(value: false);
		_onBuildButtonClicked = actionsDto?.OnBuildButtonClicked;
		_onOutpostButtonClicked = actionsDto?.OnOutpostButtonClicked;
		_onWorldMapButtonClicked = actionsDto?.OnWorldMapButtonClicked;
		_onBuildingSelected = actionsDto?.OnBuildingSelected;
		_onHide = onHideAction;
		if (_tabs == null)
		{
			_tabs = GetComponentsInChildren<MainViewSubCategoryTab>(includeInactive: true);
		}
		MainViewSubCategoryTab[] tabs = _tabs;
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].gameObject.SetActive(value: false);
		}
		foreach (var availableTab in data.AvailableTabs)
		{
			MainViewSubCategories item = availableTab.Item1;
			QuickAccessTabDTO item2 = availableTab.Item2;
			MainViewSubCategoryTab tabByCategory = GetTabByCategory(item);
			tabByCategory.gameObject.SetActive(QuickAccessBarUtils.GetContext(item) == data.Context);
			tabByCategory.ShowNotificationBadge = item2.HasNew;
			tabByCategory.IsOn = item == data.SelectedCategory;
			if (tabByCategory.Tooltip != null)
			{
				tabByCategory.Tooltip.tooltipText = item2.TooltipText ?? tabByCategory.Tooltip.tooltipText;
			}
			tabByCategory.IsInteractable = item2.IsInteractable && (allowedInteractions?.Contains(item) ?? (!isLocked));
		}
		bool flag = data.Context == MainViewMainCategories.WorldMap;
		_boat.SetActive(flag && data.HasBoat);
		_climbingGear.SetActive(flag && data.HasClimbingGear);
		_sled.SetActive(flag && data.HasSled);
		_outpost.gameObject.SetActive(flag);
		bool active = data.Context == MainViewMainCategories.Outpost && data.SelectedCategory == MainViewSubCategories.None;
		_buildButton.gameObject.SetActive(active);
		_worldMap.gameObject.SetActive(active);
		_buildButton.interactable = !isLocked;
		_worldMap.interactable = !isLocked;
		_outpost.interactable = !isLocked;
		UIManager.RegisterLateActiveState(_scenarioNotification, data.HasActiveScenario);
		if (data.BuildingFilter != UIBuildingStateFilter.None)
		{
			ShowBuildMenu(data.BuildingFilter);
		}
		else
		{
			HideBuildMenu();
		}
	}

	private void OnMainTabClicked(MainViewSubCategories category)
	{
		foreach (var (mainViewSubCategories, quickAccessTabDTO) in _data.AvailableTabs)
		{
			if (mainViewSubCategories == category)
			{
				quickAccessTabDTO.OnClick?.Invoke();
				break;
			}
		}
	}

	private MainViewSubCategoryTab GetTabByCategory(MainViewSubCategories category)
	{
		return _tabs.FirstOrDefault((MainViewSubCategoryTab tab) => tab.Category == category);
	}
}
