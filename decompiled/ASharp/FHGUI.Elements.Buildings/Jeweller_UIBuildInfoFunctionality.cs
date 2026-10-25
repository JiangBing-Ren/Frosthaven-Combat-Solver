using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Jeweller_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<Jeweller_UIBuildInfoFunctionality.JewellerStateParams>, IBuildInfoFunctionality
{
	public struct JewellerStateParams : IBuildingInfoParams
	{
		public UIJewellerRow.JewellerRowData? JewellerData;
	}

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private UIJewellerRow _jewellerRow;

	private JewellerStateParams? _data;

	private void OnDisable()
	{
		_data = null;
	}

	public void Init(BuildingStateDTO building, JewellerStateParams data)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		if (!data.JewellerData.HasValue)
		{
			_jewellerRow.gameObject.SetActive(value: false);
			return;
		}
		_jewellerRow.gameObject.SetActive(value: true);
		_jewellerRow.Init(data.JewellerData.Value, building.Level);
	}
}
