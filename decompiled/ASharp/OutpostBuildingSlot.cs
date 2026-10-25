using System;
using System.Collections.Generic;
using FH.World;
using FHGUI;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.EventChannels;
using FHRL;
using Logging.AdventureMap.OutpostVillage;
using UnityEngine;
using UnityEngine.Serialization;

public class OutpostBuildingSlot : MonoBehaviour, IHoverable
{
	public struct BuildingSlotDTO
	{
		public bool Interactable;

		public BuildingStateDTO? Building;

		public bool SlotUnlocked;

		public bool Empty => !Building.HasValue;

		public BuildingSlotDTO(BuildingStateDTO? building)
		{
			Building = building;
			SlotUnlocked = true;
			Interactable = building.HasValue;
		}

		public BuildingSlotDTO(bool unlocked, bool interactable)
		{
			Building = null;
			SlotUnlocked = unlocked;
			Interactable = interactable;
		}
	}

	[SerializeField]
	private uint _buildingType;

	[SerializeField]
	private UIBuildingTooltipActionEventChannel _buildingEventChannel;

	[SerializeField]
	private OutpostBuildingSlotUI _worldUI;

	[Header("Levels")]
	[SerializeField]
	private OutpostBuildingSlotVisuals[] _levelVisuals;

	[Header("States")]
	[SerializeField]
	private GameObject _emptyPlot;

	[SerializeField]
	private OutpostBuildingSlotVisuals _emptyPlotVisuals;

	[SerializeField]
	private GameObject _emptyPlotHighlight;

	[FormerlySerializedAs("_constructablePlotHighlight")]
	[SerializeField]
	private GameObject _unlockedPlot;

	[SerializeField]
	private GameObject _underConstructionState;

	[SerializeField]
	private OutpostBuildingSlotVisuals _underConstructionVisuals;

	[SerializeField]
	private GameObject _upgradingState;

	[SerializeField]
	private OutpostBuildingSlotVisuals _upgardeVisuals;

	[SerializeField]
	private GameObject _donation;

	[SerializeField]
	private OutpostBuildingSlotVisuals _donationVisuals;

	private OutpostBuildingSlotVisuals _visuals;

	private List<Collider> _colliders;

	public BuildingTypeID BuildingID { get; private set; }

	public CalendarSeason Season { get; private set; }

	public BuildingStateDTO? BuildingData => BuildingSlot.Building;

	public BuildingSlotDTO BuildingSlot { get; private set; }

	public bool Interactable => BuildingSlot.Interactable;

	public bool Hovered { get; private set; }

	public bool Selected { get; private set; }

	private void Awake()
	{
		BuildingID = new BuildingTypeID(_buildingType);
		for (uint num = 1u; num <= _levelVisuals.Length; num++)
		{
			HideLevelVisuals(num);
		}
		_worldUI.Hide();
	}

	private void OnDisable()
	{
		Hovered = false;
		Selected = false;
	}

	private void ResetState(bool hideLevels = false)
	{
		if (hideLevels)
		{
			for (uint num = 1u; num <= _levelVisuals.Length; num++)
			{
				HideLevelVisuals(num);
			}
		}
		if (_emptyPlot != null)
		{
			_emptyPlot.SetActive(value: false);
		}
		if (_underConstructionState != null)
		{
			_underConstructionState.SetActive(value: false);
		}
		if (_donation != null)
		{
			_donation.SetActive(value: false);
		}
		if ((bool)_donationVisuals)
		{
			_donationVisuals.SetActive(active: false);
		}
		if (_upgradingState != null)
		{
			_upgradingState.SetActive(value: false);
		}
		if (_upgardeVisuals != null)
		{
			_upgardeVisuals.SetActive(active: false);
		}
		if (_emptyPlotHighlight != null)
		{
			_emptyPlotHighlight.SetActive(value: false);
		}
		if (_unlockedPlot != null)
		{
			_unlockedPlot.SetActive(value: false);
		}
		if (BuildingData.HasValue)
		{
			_worldUI?.Show(BuildingData.Value, OnBuildingIndicatorClick, OnBuildingIndicatorHover);
		}
	}

	private void OnBuildingIndicatorClick()
	{
		SelectBuilding();
	}

	private void OnBuildingIndicatorHover(bool hovered)
	{
		if (hovered)
		{
			OnCursorEnter();
		}
		else
		{
			OnCursorExit();
		}
	}

	private void ShowEmptySlot(BuildingSlotDTO slot, CalendarSeason season)
	{
		ResetState(hideLevels: true);
		BuildingSlot = slot;
		Season = season;
		if (_emptyPlot != null)
		{
			_emptyPlot.SetActive(value: true);
		}
		if (_emptyPlotVisuals != null)
		{
			_emptyPlotVisuals.Init(Season);
		}
		if (_unlockedPlot != null)
		{
			_unlockedPlot.SetActive(BuildingSlot.SlotUnlocked);
		}
		_worldUI?.Hide();
		UpdateColliders();
	}

	public void Init(BuildingSlotDTO slot, CalendarSeason season)
	{
		ResetState(hideLevels: true);
		BuildingSlot = slot;
		Season = season;
		if (slot.Empty)
		{
			ShowEmptySlot(slot, season);
			return;
		}
		ShowLevelVisuals(BuildingData.Value.Level, season);
		SetBuildState(BuildingData.Value.ConstructionState);
		SetWreckedState(BuildingData.Value.IsWrecked);
		_worldUI?.Show(BuildingData.Value, OnBuildingIndicatorClick, OnBuildingIndicatorHover);
		UpdateColliders();
	}

	private void SetBuildState(BuildingConstructionType constructionType)
	{
		ResetState();
		switch (constructionType)
		{
		case BuildingConstructionType.NewBuilding:
		{
			bool hasStartedProject = BuildingData.Value.HasStartedProject;
			if (_underConstructionState != null)
			{
				_underConstructionState.SetActive(!hasStartedProject);
				foreach (Transform item in _underConstructionState.transform)
				{
					item.gameObject.SetActive(!hasStartedProject);
				}
			}
			if (!(_donation != null))
			{
				break;
			}
			_donation.SetActive(hasStartedProject);
			foreach (Transform item2 in _donation.transform)
			{
				item2.gameObject.SetActive(hasStartedProject);
			}
			break;
		}
		case BuildingConstructionType.Rebuilding:
		case BuildingConstructionType.Upgrading:
			if (!(_upgradingState != null))
			{
				break;
			}
			_upgradingState.SetActive(value: true);
			foreach (Transform item3 in _upgradingState.transform)
			{
				item3.gameObject.SetActive(value: true);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("constructionType", constructionType, null);
		case BuildingConstructionType.None:
			break;
		}
		FHMapShadowController.RequestShadowUpdate();
	}

	private void SetWreckedState(bool wrecked)
	{
		_visuals?.SetWreckedState(wrecked);
	}

	private void ShowLevelVisuals(uint level, CalendarSeason season)
	{
		if (_levelVisuals.Length < level)
		{
			Log.Warning($"Level visuals for level {level} for building {(BuildingId)_buildingType} are missing!");
		}
		else if (level != 0)
		{
			_visuals = _levelVisuals[level - 1];
			if (_visuals != null)
			{
				_visuals.Init(season);
			}
		}
		for (uint num = 1u; num <= _levelVisuals.Length; num++)
		{
			if (num != level)
			{
				HideLevelVisuals(num);
			}
		}
		if (_donationVisuals != null)
		{
			_donationVisuals.Init(Season);
		}
		if (_underConstructionVisuals != null)
		{
			_underConstructionVisuals.Init(Season);
		}
		if (_upgardeVisuals != null)
		{
			_upgardeVisuals.Init(Season);
		}
	}

	public void HideLevelVisuals(uint level)
	{
		OutpostBuildingSlotVisuals outpostBuildingSlotVisuals = _levelVisuals[level - 1];
		outpostBuildingSlotVisuals.SetActive(active: false);
		if (_visuals == outpostBuildingSlotVisuals)
		{
			_visuals = null;
		}
	}

	private void UpdateColliders()
	{
		if (_colliders == null)
		{
			_colliders = new List<Collider>();
			GetComponentsInChildren(_colliders);
		}
		_colliders?.ForEach((Collider c) =>
		{
			c.enabled = Interactable;
		});
	}

	public void OnCursorEnter()
	{
		Hovered = true;
		_visuals?.SetHoveredState(hovered: true);
		_donationVisuals?.SetHoveredState(hovered: true);
		_underConstructionVisuals?.SetHoveredState(hovered: true);
		_upgardeVisuals?.SetHoveredState(hovered: true);
		_worldUI?.SetHoveredState(hover: true);
		if (_emptyPlotHighlight != null)
		{
			_emptyPlotHighlight.SetActive(Interactable && (Hovered || Selected));
		}
	}

	public void OnCursorExit()
	{
		Hovered = false;
		_visuals?.SetHoveredState(hovered: false);
		_donationVisuals?.SetHoveredState(hovered: false);
		_underConstructionVisuals?.SetHoveredState(hovered: false);
		_upgardeVisuals?.SetHoveredState(hovered: false);
		_worldUI?.SetHoveredState(hover: false);
		if (_emptyPlotHighlight != null)
		{
			_emptyPlotHighlight.SetActive(Selected);
		}
	}

	private void SelectBuilding()
	{
		SetSelected(selected: true);
		_buildingEventChannel?.SelectBuilding(BuildingID);
	}

	public void SetSelected(bool selected)
	{
		if (Selected != selected)
		{
			Selected = selected;
			_visuals?.SetSelectedState(selected);
			_donationVisuals?.SetSelectedState(selected);
			_underConstructionVisuals?.SetSelectedState(selected);
			_upgardeVisuals?.SetSelectedState(selected);
			_worldUI.SetSelectedState(selected);
			if (_emptyPlotHighlight != null)
			{
				_emptyPlotHighlight?.gameObject.SetActive(selected);
			}
		}
	}
}
