using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Buildings;

public class WorkshopUpgrade_UIBuildingFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<WorkshopUpgradeBuildingParams>, IBuildInfoFunctionality
{
	[Header("Texts")]
	[SerializeField]
	private TextLocalizedListener _description;

	[Header("Elements")]
	[SerializeField]
	private Image _upgradeIcon;

	[Header("Materials")]
	[SerializeField]
	private Material _grayscaleMaterial;

	public void Init(BuildingStateDTO building, WorkshopUpgradeBuildingParams data)
	{
		gameObject.SetActive(value: true);
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_upgradeIcon.material = (data.IsWorkshopWrecked ? _grayscaleMaterial : null);
		_upgradeIcon.sprite = data.Icon;
	}
}
