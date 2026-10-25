using System;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Elements.Buildings;
using FHRL;
using FHRL.Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class BuildingInfoModule : FHMapViewModule
{
	public struct BuildingInfoParams
	{
		public BuildingStateDTO Building;

		public IBuildingInfoParams InfoParams;

		public IBuildingInfoStateParams StateParams;

		public Action OnHide;
	}

	public class PersistentData
	{
		public OutpostMaterialType PickedSoldierTrainResource;

		public OutpostMaterialType PickedInnTradeResource;
	}

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private Image _buildingBanner;

	[SerializeField]
	private Sprite _defaultBanner;

	[SerializeField]
	private TextLocalizedListener _name;

	[SerializeField]
	private TMP_Text _level;

	[Header("Screens")]
	[SerializeField]
	private Info_UIBuildInfoFunctionality _info;

	[SerializeField]
	private SingleAction_UIBuildingInfoFunctionality _defaultSingleActionInfo;

	[Header("States")]
	[SerializeField]
	private UIBuildingInfoConstruct _constructInfo;

	[SerializeField]
	private UIBuildingInfoUpgrade _upgradeInfo;

	[SerializeField]
	private UIBuildingInfoRebuild _rebuildInfo;

	[Header("Header")]
	[SerializeField]
	private Sprite _defaultHeaderSprite;

	[SerializeField]
	private Sprite _inConstructionHeaderSprite;

	[SerializeField]
	private Sprite _wreckedHeaderSprite;

	[Header("Indicators")]
	[SerializeField]
	private GameObject _levelIndicator;

	[SerializeField]
	private GameObject _maxLevelIndicator;

	[SerializeField]
	private GameObject _wreckedIndicator;

	[SerializeField]
	private GameObject _wreckedBanner;

	[SerializeField]
	private GameObject _underConstructionIndicator;

	[SerializeField]
	private GameObject _newBuildingIndicator;

	private IBuildInfoFunctionality[] _buildingInfo;

	private Action _onHide;

	private bool _containerWasActive;

	public BuildingInfoParams data;

	public RectTransform Container => _container;

	public bool Active
	{
		get
		{
			if (!UIManager.TryGetLateActiveState(_container.gameObject, out var active))
			{
				return _container.gameObject.activeSelf;
			}
			return active;
		}
	}

	protected override bool PlaySoundOnEnable => false;

	private void Awake()
	{
		_buildingInfo = GetComponentsInChildren<IBuildInfoFunctionality>(includeInactive: true);
		IBuildInfoFunctionality[] buildingInfo = _buildingInfo;
		for (int i = 0; i < buildingInfo.Length; i++)
		{
			buildingInfo[i].Hide();
		}
		Hide();
	}

	protected override void OnShow()
	{
		base.OnShow();
		_containerWasActive = _container.gameObject.activeSelf;
	}

	protected override void OnHide()
	{
		base.OnHide();
		_onHide = null;
	}

	private void LateUpdate()
	{
		bool activeSelf = _container.gameObject.activeSelf;
		if (activeSelf != _containerWasActive)
		{
			_containerWasActive = activeSelf;
			if (activeSelf)
			{
				PlayShowSound();
			}
			else
			{
				PlayHideSound();
			}
		}
	}

	public void Refresh()
	{
		Hide();
	}

	public void CheckClickOutside()
	{
		if (EventSystem.current.isFocused && !EventSystem.current.IsPointerOverGameObject())
		{
			Hide();
		}
	}

	public void Hide()
	{
		UIManager.RegisterLateActiveState(_container.gameObject, active: false);
		_onHide?.Invoke();
	}

	public void ShowInfo(BuildingInfoParams data)
	{
		this.data = data;
		_onHide = data.OnHide;
		UIManager.RegisterLateActiveState(_container.gameObject, active: true);
		SetData();
		SetVisualState();
		bool flag = SetInfoSection(data.Building, data.InfoParams);
		if (!(SetStateSection(data.Building, data.StateParams) | flag))
		{
			Hide();
		}
		UIContextualTutorialModule.OnEvent("BuildingSelected", this, data);
	}

	private void SetData()
	{
		BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig((BuildingId)data.Building.Def.ID.ID);
		_name.SetTextKey(BuildInfoUtils.ValidateText(data.Building.NameLocKey));
		_level.text = data.Building.Level.ToString();
		_buildingBanner.sprite = buildingConfig?.Banner ?? _defaultBanner;
	}

	private void SetVisualState()
	{
		bool isWrecked = data.Building.IsWrecked;
		BuildingStateDTO building = data.Building;
		bool flag = building.HasMoreUpgrades && !building.IsWrecked && !building.IsUnderConstruction;
		bool isUnderConstruction = data.Building.IsUnderConstruction;
		_wreckedBanner.SetActive(isWrecked);
		_wreckedIndicator.SetActive(isWrecked);
		_maxLevelIndicator.SetActive(!flag);
		_levelIndicator.SetActive(data.Building.Level != 0 && !isUnderConstruction && !isWrecked);
		_underConstructionIndicator.SetActive(isUnderConstruction);
		_newBuildingIndicator.SetActive(data.Building.Level == 0);
	}

	private bool SetInfoSection(BuildingStateDTO building, IBuildingInfoParams buildingParams)
	{
		IBuildInfoFunctionality buildInfoFunctionality = null;
		bool flag = false;
		if (buildingParams != null)
		{
			buildInfoFunctionality = GetFunctionality(building.ID);
			flag = buildInfoFunctionality.Init(building, buildingParams);
		}
		IBuildInfoFunctionality[] buildingInfo = _buildingInfo;
		foreach (IBuildInfoFunctionality buildInfoFunctionality2 in buildingInfo)
		{
			if (!flag || buildInfoFunctionality2 != buildInfoFunctionality)
			{
				buildInfoFunctionality2.Hide();
			}
		}
		return flag;
	}

	private bool SetStateSection(BuildingStateDTO building, IBuildingInfoStateParams stateParams)
	{
		_constructInfo.gameObject.SetActive(value: false);
		_upgradeInfo.gameObject.SetActive(value: false);
		_rebuildInfo.gameObject.SetActive(value: false);
		if (stateParams == null)
		{
			return false;
		}
		try
		{
			if (!(stateParams is UIBuildingInfoConstruct.ConstructBuildingParams constructBuildingParams))
			{
				if (!(stateParams is UIBuildingInfoUpgrade.UpgradeBuildingParams upgradeBuildingParams))
				{
					if (stateParams is UIBuildingInfoRebuild.RebuildBuildingParams rebuildBuildingParams)
					{
						_rebuildInfo.Init(building, rebuildBuildingParams);
					}
				}
				else
				{
					_upgradeInfo.Init(building, upgradeBuildingParams);
				}
			}
			else
			{
				_constructInfo.Init(building, constructBuildingParams);
			}
		}
		catch (Exception)
		{
			Log.GUIError(string.Format("Invalid {0} given for building {1} in state {2} - given {3}", "IBuildingInfoStateParams", building.Def.Name, building.ConstructionState, stateParams.GetType()));
			return false;
		}
		BuildingTypeDef def = building.Def;
		if (def is Workshop_BuildingTypeDef || def is CraftingTable_BuildingTypeDef)
		{
			_constructInfo.gameObject.SetActive(value: false);
			_upgradeInfo.gameObject.SetActive(value: false);
		}
		return true;
	}

	private T GetFunctionality<T>() where T : IBuildInfoFunctionality
	{
		return (T)_buildingInfo.FirstOrDefault((IBuildInfoFunctionality info) => info is T);
	}

	private SingleAction_UIBuildingInfoFunctionality GetSingleButtonFunctionality(BuildingId id)
	{
		return (SingleAction_UIBuildingInfoFunctionality)_buildingInfo.FirstOrDefault((IBuildInfoFunctionality info) => info is SingleAction_UIBuildingInfoFunctionality singleAction_UIBuildingInfoFunctionality && singleAction_UIBuildingInfoFunctionality.IsForBuilding(id));
	}

	private Info_UIBuildInfoFunctionality GetInfoFunctionality(BuildingId id)
	{
		return _buildingInfo.OfType<Info_UIBuildInfoFunctionality>().FirstOrDefault((Info_UIBuildInfoFunctionality i) => i.IsForBuilding(id)) ?? _info;
	}

	private IBuildInfoFunctionality GetFunctionality(BuildingId id)
	{
		return id switch
		{
			BuildingId.MiningCamp => (IBuildInfoFunctionality)GetFunctionality<ResourceBuilding_UIBuildInfoFunctionality>(), 
			BuildingId.HuntingLodge => GetFunctionality<ResourceBuilding_UIBuildInfoFunctionality>(), 
			BuildingId.LoggingCamp => GetFunctionality<ResourceBuilding_UIBuildInfoFunctionality>(), 
			BuildingId.MetalDepot => GetFunctionality<ResourceBuilding_UIBuildInfoFunctionality>(), 
			BuildingId.LumberDepot => GetFunctionality<ResourceBuilding_UIBuildInfoFunctionality>(), 
			BuildingId.HideDepot => GetFunctionality<ResourceBuilding_UIBuildInfoFunctionality>(), 
			BuildingId.Inn => GetFunctionality<Inn_UIBuildInfoFunctionality>(), 
			BuildingId.Alchemist => GetSingleButtonFunctionality(BuildingId.Alchemist), 
			BuildingId.Craftsman => GetSingleButtonFunctionality(BuildingId.Craftsman), 
			BuildingId.Enhancer => GetSingleButtonFunctionality(BuildingId.Enhancer), 
			BuildingId.TradingPost => GetSingleButtonFunctionality(BuildingId.TradingPost), 
			BuildingId.TempleOfTheGreatOak => GetSingleButtonFunctionality(BuildingId.TempleOfTheGreatOak), 
			BuildingId.CraftingTable => _defaultSingleActionInfo, 
			BuildingId.Garden => GetFunctionality<Garden_UIBuildInfoFunctionality>(), 
			BuildingId.Barracks => GetFunctionality<Barracks_UIBuildInfoFunctionality>(), 
			BuildingId.Library => GetFunctionality<Library_UIBuildingInfoFunctionality>(), 
			BuildingId.Stables => GetFunctionality<Stables_UIBuildInfoFunctionality>(), 
			BuildingId.Tavern => GetFunctionality<Tavern_UIBuildInfoFunctionality>(), 
			BuildingId.Jeweler => GetFunctionality<Jeweller_UIBuildInfoFunctionality>(), 
			BuildingId.HallOfRevelry => GetFunctionality<HallOfRevelry_UIBuildInfoFunctionality>(), 
			BuildingId.Townhall => GetFunctionality<TownHall_UIBuildInfoFunctionality>(), 
			BuildingId.Workshop => GetFunctionality<Workshop_UIBuildInfoFunctionality>(), 
			BuildingId.Boat => GetFunctionality<WorkshopUpgrade_UIBuildingFunctionality>(), 
			BuildingId.Sled => GetFunctionality<WorkshopUpgrade_UIBuildingFunctionality>(), 
			BuildingId.ClimbingGear => GetFunctionality<WorkshopUpgrade_UIBuildingFunctionality>(), 
			BuildingId.Wall => GetFunctionality<Walls_UIBuildInfoFunctionality>(), 
			_ => GetInfoFunctionality(id), 
		};
	}
}
