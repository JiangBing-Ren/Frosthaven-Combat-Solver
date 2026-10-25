using System.Collections.Generic;
using System.Linq;
using FHRL;
using UnityEngine;

namespace FHGUI.DTO.Outpost;

public class OutpostBuildingDataSO : ScriptableObject
{
	public BuildingConfigUI[] BuildingConfigs;

	private Dictionary<int, uint> _buildingDataMapper;

	private bool _init;

	public BuildingConfigUI this[int id]
	{
		get
		{
			if (!_init)
			{
				Init();
			}
			if (!_buildingDataMapper.ContainsKey(id))
			{
				Debug.LogError($"Missing data for building {id}");
				return null;
			}
			return BuildingConfigs[_buildingDataMapper[id]];
		}
	}

	public BuildingConfigUI this[BuildingTypeID id] => this[(int)id.ID];

	public BuildingConfigUI this[BuildingId id] => this[(int)id];

	private void Init()
	{
		_buildingDataMapper = new Dictionary<int, uint>();
		for (uint num = 0u; num < BuildingConfigs.Length; num++)
		{
			_buildingDataMapper[(int)BuildingConfigs[num].BuildingId] = num;
		}
	}

	public Sprite GetBuildingIcon(BuildingTypeID buildingTypeID)
	{
		return this[buildingTypeID].Icon;
	}

	public Sprite GetBuildingIcon(BuildingId buildingId)
	{
		return this[buildingId].Icon;
	}

	public Sprite GetBuildingIconTimetable(BuildingTypeID buildingTypeID)
	{
		return this[buildingTypeID].TimetableIcon;
	}

	public Sprite GetBuildingIconTimetable(BuildingId buildingId)
	{
		return this[buildingId].TimetableIcon;
	}

	public string GetBuildingName(BuildingTypeID buildingTypeID)
	{
		return this[buildingTypeID].NameLocKey[0];
	}

	public string GetBuildingName(BuildingTypeID buildingTypeID, int level)
	{
		if (level < this[buildingTypeID].NameLocKey.Length)
		{
			return this[buildingTypeID].NameLocKey[level];
		}
		return this[buildingTypeID].NameLocKey.Last();
	}

	public string GetBuildingName(BuildingId buildingId)
	{
		return this[buildingId].NameLocKey[0];
	}

	public string GetBuildingName(BuildingId buildingId, int level)
	{
		if (level < this[buildingId].NameLocKey.Length)
		{
			return this[buildingId].NameLocKey[level];
		}
		return this[buildingId].NameLocKey.Last();
	}

	public string GetBuildingDescription(BuildingTypeID buildingTypeID)
	{
		return this[buildingTypeID].ActiveEffects.FirstOrDefault();
	}
}
