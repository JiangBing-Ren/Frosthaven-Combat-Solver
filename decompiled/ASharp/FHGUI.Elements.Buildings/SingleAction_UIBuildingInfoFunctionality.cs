using System;
using System.Linq;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class SingleAction_UIBuildingInfoFunctionality : MonoBehaviour, IBuildInfoFunctionality<SingleActionBuildingParams>, IBuildInfoFunctionality
{
	[SerializeField]
	private FHButton _actionButton;

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private TextLocalizedListener _actionHint;

	[SerializeField]
	private BuildingId[] _buildingIds;

	private UIBuildingLockRequirement[] _locks;

	private Action _action;

	public bool IsForBuilding(BuildingId id)
	{
		return _buildingIds.Contains(id);
	}

	private void Awake()
	{
		_locks = GetComponentsInChildren<UIBuildingLockRequirement>();
		_actionButton.onClick.AddListener(() =>
		{
			_action?.Invoke();
		});
	}

	public void Init(BuildingStateDTO building, SingleActionBuildingParams data)
	{
		if (_locks == null)
		{
			_locks = GetComponentsInChildren<UIBuildingLockRequirement>();
		}
		gameObject.SetActive(value: true);
		_action = data.Action;
		_actionButton.interactable = data.IsInteractable;
		_actionButton.gameObject.SetActive(building.IsConstructed);
		if (_actionHint != null)
		{
			_actionHint.gameObject.SetActive(!string.IsNullOrEmpty(data.ActionHintLocKey) && building.IsConstructed);
			UIHelperUtil.SetTextKey(_actionHint, data.ActionHintLocKey);
		}
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		UIHelperUtil.SetTextKey(_actionButton.Text, data.ActionLocKey);
		UIBuildingLockRequirement[] locks = _locks;
		for (int i = 0; i < locks.Length; i++)
		{
			locks[i].ShowBasedOnLevel(building.Level);
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
