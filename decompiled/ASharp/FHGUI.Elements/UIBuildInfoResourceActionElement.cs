using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildInfoResourceActionElement : MonoBehaviour
{
	public struct ResourceBuildingActionData
	{
		public string NameLocKey;

		public string DescriptionLocKey;

		public BuildingFunctionalityAction Action;

		public ResourceElement.ResourceActionElementData LeftSideElement;

		public ResourceElement.ResourceActionElementData RightSideElement;

		public bool Locked;

		public uint LockedLevel;
	}

	[SerializeField]
	private TextLocalizedListener _name;

	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private ResourceElement _leftSide;

	[SerializeField]
	private ResourceElement _rightSide;

	[SerializeField]
	private UIBuildInfoActionElement _action;

	[SerializeField]
	private GameObject _lockedContainer;

	[SerializeField]
	private TMP_Text _lockedLevel;

	public void Init(ResourceBuildingActionData data)
	{
		_name.SetTextKey(data.NameLocKey);
		_description.enabled = data.DescriptionLocKey.IsNOTNullOrEmpty();
		_description.SetTextKey(data.DescriptionLocKey);
		_action.Init(data.Action);
		_leftSide.Init(data.LeftSideElement);
		_rightSide.Init(data.RightSideElement);
		_lockedContainer.SetActive(data.Locked);
		_lockedLevel.text = data.LockedLevel.ToString();
		if (data.Locked)
		{
			_action.Button.interactable = true;
		}
	}
}
