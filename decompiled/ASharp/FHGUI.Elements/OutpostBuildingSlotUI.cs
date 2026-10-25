using System;
using FHGUI.DTO;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class OutpostBuildingSlotUI : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private TextLocalizedListener _name;

	[SerializeField]
	private TextLocalizedListener _hintName;

	[SerializeField]
	private TextLocalizedListener _status;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _iconBackground;

	[SerializeField]
	private TMP_Text _level;

	[SerializeField]
	private Image _LevelBackground;

	[SerializeField]
	private GameObject _statusContainer;

	[SerializeField]
	private Image _statusBackground;

	[SerializeField]
	private GameObject _unavailableUpgradeState;

	[SerializeField]
	private GameObject _hint;

	[Header("Sprites")]
	[SerializeField]
	private Sprite _regularSprite;

	[SerializeField]
	private Sprite _wreckedSprite;

	[SerializeField]
	private Sprite _donationSprite;

	[SerializeField]
	private Sprite _underConstructionSprite;

	[SerializeField]
	private Sprite _levelRegularSprite;

	[SerializeField]
	private Sprite _levelWreckedSprite;

	[SerializeField]
	private Sprite _levelDonationSprite;

	[SerializeField]
	private Sprite _levelUnderConstructionSprite;

	[SerializeField]
	private Sprite _statusWreckedSprite;

	[SerializeField]
	private Sprite _statusDonationSprite;

	[SerializeField]
	private Sprite _statusUnderConstructionSprite;

	private bool _selected;

	private bool _hovered;

	private bool _invalidateHover;

	private Action _onClick;

	private Action<bool> _onHover;

	private BuildingStateDTO? _buildingStateDto;

	private bool ShowDetails
	{
		get
		{
			if (_buildingStateDto.HasValue)
			{
				if (!_buildingStateDto.Value.IsWrecked && !_buildingStateDto.Value.HasStartedProject)
				{
					return _buildingStateDto.Value.IsUnderConstruction;
				}
				return true;
			}
			return false;
		}
	}

	public void Show(BuildingStateDTO buildingState, Action onClick = null, Action<bool> onHover = null)
	{
		gameObject.SetActive(value: true);
		_buildingStateDto = buildingState;
		_name.SetTextKey(buildingState.NameLocKey);
		_hintName.SetTextKey(buildingState.NameLocKey);
		_level.text = ((buildingState.Level != 0) ? buildingState.Level.ToString() : "-");
		_icon.sprite = buildingState.IconTintable;
		SetState(buildingState);
		_onClick = onClick;
		_onHover = onHover;
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void SetState(BuildingStateDTO buildingState)
	{
		ResetStates();
		_level.enabled = buildingState.HasMoreUpgrades;
		switch (buildingState.ConstructionState)
		{
		case BuildingConstructionType.None:
			if (buildingState.IsWrecked)
			{
				_statusBackground.sprite = _statusWreckedSprite;
				_iconBackground.sprite = _wreckedSprite;
				_LevelBackground.sprite = _levelWreckedSprite;
				_status.SetTextKey("GUI_WRECKED");
			}
			break;
		case BuildingConstructionType.NewBuilding:
		case BuildingConstructionType.Rebuilding:
		case BuildingConstructionType.Upgrading:
			if (buildingState.HasStartedProject)
			{
				_status.SetTextKey("GUI_BUILDING_STATUS_DONATING");
				_statusBackground.sprite = _statusDonationSprite;
				_iconBackground.sprite = _donationSprite;
				_LevelBackground.sprite = _levelDonationSprite;
			}
			else
			{
				_status.SetTextKey("GUI_BUILDING_STATUS_CONSTRUCTING");
				_statusBackground.sprite = _statusUnderConstructionSprite;
				_iconBackground.sprite = _underConstructionSprite;
				_LevelBackground.sprite = _levelUnderConstructionSprite;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("ConstructionState", buildingState.ConstructionState, null);
		}
		if (!buildingState.HasMoreUpgrades)
		{
			_unavailableUpgradeState.SetActive(value: true);
		}
		_statusContainer.gameObject.SetActive(ShowDetails);
	}

	private void Update()
	{
		if (_invalidateHover)
		{
			_hint.gameObject.SetActive(!ShowDetails && (_hovered || _selected));
			_invalidateHover = false;
		}
	}

	public void SetSelectedState(bool selected)
	{
		_selected = selected;
		_invalidateHover = true;
	}

	public void SetHoveredState(bool hover)
	{
		if (_hovered != hover)
		{
			_hovered = hover;
			_invalidateHover = true;
		}
	}

	private void ResetStates()
	{
		_iconBackground.sprite = _regularSprite;
		_LevelBackground.sprite = _levelRegularSprite;
		_unavailableUpgradeState.SetActive(value: false);
		_hint.gameObject.SetActive(value: false);
		_status.ClearText();
	}

	private void OnDisable()
	{
		_onClick = null;
		_onHover = null;
		_selected = false;
		_hovered = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!_selected)
		{
			_onClick?.Invoke();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SetHoveredState(hover: true);
		_onHover?.Invoke(obj: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetHoveredState(hover: false);
		_onHover?.Invoke(obj: false);
	}
}
