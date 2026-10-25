using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class ResourceBuilding_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<ResourceBuildingData>, IBuildInfoFunctionality
{
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private UIBuildInfoResourceActionElement _buyAction;

	[SerializeField]
	private UIBuildInfoResourceActionElement _sellAction;

	[SerializeField]
	private UIBuildInfoResourceActionElement _sellAtHighPriceAction;

	private UIBuildingLockRequirement[] _locks;

	private void Awake()
	{
		if (_locks == null)
		{
			_locks = GetComponentsInChildren<UIBuildingLockRequirement>();
		}
	}

	public void Init(BuildingStateDTO building, ResourceBuildingData data)
	{
		if (_locks == null)
		{
			_locks = GetComponentsInChildren<UIBuildingLockRequirement>();
		}
		gameObject.SetActive(value: true);
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_buyAction.gameObject.SetActive(data.BuyAction.HasValue);
		_sellAction.gameObject.SetActive(data.SellAction.HasValue);
		_sellAtHighPriceAction.gameObject.SetActive(data.SellAtHighPriceAction.HasValue);
		if (data.BuyAction.HasValue)
		{
			_buyAction.Init(data.BuyAction.Value);
		}
		if (data.SellAction.HasValue)
		{
			_sellAction.Init(data.SellAction.Value);
		}
		if (data.SellAtHighPriceAction.HasValue)
		{
			_sellAtHighPriceAction.Init(data.SellAtHighPriceAction.Value);
		}
		UIBuildingLockRequirement[] locks = _locks;
		for (int i = 0; i < locks.Length; i++)
		{
			locks[i].ShowBasedOnLevel(building.Level);
		}
	}
}
