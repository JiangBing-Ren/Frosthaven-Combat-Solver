using System.Linq;
using System.Runtime.InteropServices;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Info_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<IBuildingInfoParams>, IBuildInfoFunctionality
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct InfoParams : IBuildingInfoParams
	{
	}

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private BuildingId[] _buildingIds;

	private UIBuildingLockRequirement[] _locks;

	private UIBuildingLockRequirement[] Locks
	{
		get
		{
			if (_locks == null)
			{
				_locks = GetComponentsInChildren<UIBuildingLockRequirement>();
			}
			return _locks;
		}
	}

	public bool IsForBuilding(BuildingId id)
	{
		return _buildingIds.Contains(id);
	}

	public void Init(BuildingStateDTO building, IBuildingInfoParams data)
	{
		gameObject.SetActive(value: true);
		if (_description != null)
		{
			_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		}
		UIBuildingLockRequirement[] locks = Locks;
		for (int i = 0; i < locks.Length; i++)
		{
			locks[i].ShowBasedOnLevel(building.Level);
		}
	}
}
