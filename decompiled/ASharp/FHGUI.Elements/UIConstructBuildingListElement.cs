using System;
using FHGUI.DTO;
using FHRL;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIConstructBuildingListElement : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener _name;

	[SerializeField]
	private TextLocalizedListener _level;

	[Header("Visuals")]
	[SerializeField]
	private Image _icon;

	[Header("States")]
	[SerializeField]
	private GameObject _availableBuild;

	[SerializeField]
	private GameObject _underConstructionState;

	[SerializeField]
	private GameObject _upgradeState;

	[SerializeField]
	private GameObject _importantState;

	[SerializeField]
	private GameObject _maxLevelState;

	[SerializeField]
	private GameObject _donationState;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	private Action<UIConstructBuildingListElement> _onSelect;

	public BuildingTypeID ID { get; private set; }

	public bool Selected { get; private set; }

	public FHButton Button { get; private set; }

	private void Awake()
	{
		Button = GetComponent<FHButton>();
	}

	public void Init(BuildingStateDTO buildingDto, Action<UIConstructBuildingListElement> onSelect = null)
	{
		_onSelect = onSelect;
		if (_level != null)
		{
			_level.SetParam("0", buildingDto.Level);
		}
		if (_name != null)
		{
			_name.SetTextKey(buildingDto.NameLocKey);
		}
		_icon.sprite = buildingDto.Icon;
		ID = buildingDto.Def.ID;
		SetState(buildingDto);
		if (_tooltip != null)
		{
			_tooltip.SetText(LocalizationManager.GetTranslation(buildingDto.NameLocKey));
		}
	}

	public void OnClick()
	{
		SetSelectedState(selected: true);
		_onSelect?.Invoke(this);
	}

	public void SetSelectedState(bool selected)
	{
		if (Selected != selected)
		{
			Selected = selected;
			Button.interactable = !selected;
		}
	}

	private void OnDisable()
	{
		_onSelect = null;
		SetSelectedState(selected: false);
	}

	private void SetState(BuildingStateDTO buildingDto)
	{
		bool active = false;
		bool flag = false;
		bool active2 = false;
		bool active3 = false;
		bool active4 = false;
		if (!buildingDto.IsWrecked)
		{
			if (buildingDto.IsUnderConstruction)
			{
				active2 = true;
			}
			else if (!buildingDto.IsConstructed)
			{
				active = true;
			}
			else if (buildingDto.HasStartedProject)
			{
				active4 = true;
			}
			else
			{
				flag = buildingDto.HasMoreUpgrades;
				active3 = !flag;
			}
		}
		setActive(_availableBuild, active);
		setActive(_upgradeState, flag);
		setActive(_underConstructionState, active2);
		setActive(_maxLevelState, active3);
		setActive(_donationState, active4);
		static void setActive(GameObject obj, bool active5)
		{
			if (obj != null)
			{
				obj.SetActive(active5);
			}
		}
	}
}
