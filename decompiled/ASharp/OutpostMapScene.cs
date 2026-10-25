using System.Collections.Generic;
using System.Linq;
using FHGUI;
using FHGUI.DTO;
using FHRL;
using Logging.AdventureMap.OutpostVillage;
using UnityEngine;
using UnityEngine.EventSystems;

public class OutpostMapScene : MonoBehaviour, ISceneRoot
{
	public struct OutpostVillageSceneData
	{
		public IEnumerable<BuildingStateDTO> Buildings;

		public IEnumerable<BuildingTypeID> AvailableToConstruct;

		public CalendarSeason Season;

		public IReadOnlyList<CampaignStickerID> Stickers;
	}

	[SerializeField]
	private Transform _buildingSlots;

	[SerializeField]
	private FHMapCameraController _camera;

	[Header("Winter assets")]
	[SerializeField]
	private Ambience _winterAmbience;

	[SerializeField]
	private GameObject[] _winterAssets;

	[Header("Summer assets")]
	[SerializeField]
	private Ambience _summerAmbience;

	[SerializeField]
	private GameObject[] _summerAssets;

	private OutpostBuildingSlot[] _slots;

	private OutpostCampaignStickerVisuals[] _stickers;

	private RaycastHit _hit;

	private LayerMask _mask;

	private OutpostVillageSceneData? _data;

	public OutpostBuildingSlot HoveredSlot { get; private set; }

	public OutpostBuildingSlot SelectedSlot { get; private set; }

	public Transform Root => transform;

	public Camera Camera => _camera.Camera;

	private void Awake()
	{
		_mask = 1 << LayerMask.NameToLayer("Ground");
	}

	private void OnDisable()
	{
		HoveredSlot = null;
		SelectedSlot?.SetSelected(selected: false);
	}

	public void RefreshStickerVisuals(IReadOnlyList<CampaignStickerID> stickers)
	{
		if (_stickers != null)
		{
			OutpostCampaignStickerVisuals[] stickers2 = _stickers;
			for (int i = 0; i < stickers2.Length; i++)
			{
				stickers2[i].Init(stickers);
			}
		}
	}

	public void Init(IEnumerable<BuildingStateDTO> buildings, IEnumerable<BuildingTypeID> availableBuildingsToConstruct, IReadOnlyList<CampaignStickerID> stickers, CalendarSeason season)
	{
		gameObject.SetActive(value: true);
		InitCache();
		List<BuildingStateDTO> list = buildings.ToList();
		BuildingTypeID[] source = availableBuildingsToConstruct.ToArray();
		OutpostBuildingSlot[] slots = _slots;
		foreach (OutpostBuildingSlot slot in slots)
		{
			int num = list.FindIndex((BuildingStateDTO b) => b.Def.ID == slot.BuildingID);
			bool flag = num >= 0;
			BuildingStateDTO? building = (flag ? new BuildingStateDTO?(list[num]) : ((BuildingStateDTO?)null));
			if (flag)
			{
				list.RemoveAt(num);
			}
			if (building.HasValue)
			{
				slot.Init(new OutpostBuildingSlot.BuildingSlotDTO(building), season);
				continue;
			}
			bool flag2 = source.Any((BuildingTypeID id) => id == slot.BuildingID);
			slot.Init(new OutpostBuildingSlot.BuildingSlotDTO(flag2, flag2), season);
		}
		foreach (BuildingStateDTO item in list)
		{
			Debug.LogError($"Building slot for building {item.Def.ID} is missing in the village map");
		}
		RefreshStickerVisuals(stickers);
		bool flag3 = season == CalendarSeason.Summer;
		GameObject[] summerAssets = _summerAssets;
		for (int i = 0; i < summerAssets.Length; i++)
		{
			summerAssets[i].SetActive(flag3);
		}
		summerAssets = _winterAssets;
		for (int i = 0; i < summerAssets.Length; i++)
		{
			summerAssets[i].SetActive(!flag3);
		}
		if (_summerAmbience != null)
		{
			int num2 = (flag3 ? 1 : 0);
			_summerAmbience.gameObject.SetActive(flag3);
			_summerAmbience.ApplyBlend(num2, num2);
		}
		if (_winterAmbience != null)
		{
			int num3 = ((!flag3) ? 1 : 0);
			_winterAmbience.gameObject.SetActive(!flag3);
			_winterAmbience.ApplyBlend(num3, num3);
		}
	}

	private void InitCache()
	{
		if (_slots == null || _slots.Length == 0)
		{
			_slots = _buildingSlots.GetComponentsInChildren<OutpostBuildingSlot>(includeInactive: true);
		}
		if (_stickers == null || _stickers.Length == 0)
		{
			_stickers = transform.GetComponentsInChildren<OutpostCampaignStickerVisuals>(includeInactive: true);
		}
		FHMapView fHMapView = Singleton<FHMapChoreographer>.InstanceFast?.View;
		if (fHMapView != null)
		{
			fHMapView.Modules.OutpostMap.OutpostOverviewModule.SetSceneRootReference(fHMapView.OutpostSceneController.OutpostMapScene);
		}
	}

	private void Update()
	{
		if (EventSystem.current.IsPointerOverGameObject())
		{
			HoveredSlot?.OnCursorExit();
			HoveredSlot = null;
		}
		else if (Physics.Raycast(_camera.Camera.ScreenPointToRay(Input.mousePosition), out _hit, 1000f, _mask))
		{
			if (HoveredSlot == null || HoveredSlot.gameObject != _hit.collider.gameObject)
			{
				HoveredSlot?.OnCursorExit();
				HoveredSlot = _hit.collider.gameObject.GetComponentInParent<OutpostBuildingSlot>();
				HoveredSlot?.OnCursorEnter();
			}
		}
		else
		{
			HoveredSlot?.OnCursorExit();
			HoveredSlot = null;
		}
	}

	public void SetSelectedBuilding(BuildingTypeID id)
	{
		OutpostBuildingSlot slotByBuildingId = GetSlotByBuildingId(id);
		if (slotByBuildingId == null)
		{
			Debug.LogError($"Slot for building {id} is missing!");
		}
		else if (!(SelectedSlot == slotByBuildingId))
		{
			DeselectBuilding(resetCamera: false);
			SelectedSlot = slotByBuildingId;
			slotByBuildingId.SetSelected(selected: true);
			_camera.FocusOnTarget(slotByBuildingId.transform.position, 1f);
		}
	}

	public void DeselectBuilding(bool resetCamera = true)
	{
		if (SelectedSlot != null)
		{
			SelectedSlot.SetSelected(selected: false);
			SelectedSlot = null;
			if (resetCamera)
			{
				_camera.ResetFocus();
			}
		}
	}

	private OutpostBuildingSlot GetSlotByBuildingId(BuildingTypeID id)
	{
		return _slots.FirstOrDefault((OutpostBuildingSlot s) => s.BuildingID == id);
	}

	public void Show(object data = null)
	{
		gameObject.SetActive(value: true);
		_data = (OutpostVillageSceneData)data;
		Init(_data.Value.Buildings, _data.Value.AvailableToConstruct, _data.Value.Stickers, _data.Value.Season);
	}

	public void Hide()
	{
		_data = null;
		gameObject.SetActive(value: false);
	}

	public bool ShouldRefresh(object data = null)
	{
		if (!_data.HasValue)
		{
			return true;
		}
		OutpostVillageSceneData outpostVillageSceneData = (OutpostVillageSceneData)data;
		if (_data.Value.Season == outpostVillageSceneData.Season && _data.Value.Buildings.SequenceEqual(outpostVillageSceneData.Buildings) && _data.Value.AvailableToConstruct.SequenceEqual(outpostVillageSceneData.AvailableToConstruct))
		{
			return !_data.Value.Stickers.SequenceEqual(outpostVillageSceneData.Stickers);
		}
		return true;
	}
}
