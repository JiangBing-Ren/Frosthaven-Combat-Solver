using System;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class Library_UIBuildingInfoFunctionality : MonoBehaviour, IBuildInfoFunctionality<Library_UIBuildingInfoFunctionality.LibraryStateParams>, IBuildInfoFunctionality
{
	public struct LibraryStateParams : IBuildingInfoParams
	{
		public Action Action;

		public bool IsInteractable;

		public int Discount;

		public bool Locked;
	}

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private FHButton _actionButton;

	[SerializeField]
	private UIBuildingLockRequirement _levelContainerLock;

	[SerializeField]
	private TextLocalizedListener _effectText;

	[SerializeField]
	private TMP_Text _discountText;

	private Action _action;

	private void Awake()
	{
		_actionButton.onClick.AddListener(() =>
		{
			_action?.Invoke();
		});
	}

	public void Init(BuildingStateDTO building, LibraryStateParams data)
	{
		gameObject.SetActive(value: true);
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_effectText.SetParam("0", data.Discount);
		_action = data.Action;
		_actionButton.interactable = data.IsInteractable;
		_discountText.text = $"-{data.Discount}<sprite name=Gold>";
		_levelContainerLock.Show(1u, building.Level);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
