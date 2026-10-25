using System.Runtime.InteropServices;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Walls_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<Walls_UIBuildInfoFunctionality.WallsData>, IBuildInfoFunctionality
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct WallsData : IBuildingInfoParams
	{
	}

	[SerializeField]
	private TextLocalizedListener _description;

	public void Init(BuildingStateDTO building, WallsData data)
	{
		gameObject.SetActive(value: true);
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
	}
}
