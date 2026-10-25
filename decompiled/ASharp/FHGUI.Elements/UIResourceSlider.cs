using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIResourceSlider : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _playerResources;

	[SerializeField]
	private TMP_Text _townResources;

	[SerializeField]
	private FHSlider _resourceSlider;

	[SerializeField]
	private Image _sliderBackground;

	[SerializeField]
	private TMP_Text _sliderText;

	[SerializeField]
	private Image _resourceLeftImage;

	[SerializeField]
	private Image _resourceRightImage;

	[Header("Settings")]
	[SerializeField]
	private Color _regularTextColor = new Color(1f, 1f, 1f);

	[SerializeField]
	private Color _selectedTextColor = new Color(1f, 1f, 1f);

	[SerializeField]
	private Color _unavailableTextColor = new Color(1f, 0f, 0f);

	[SerializeField]
	private Color _sliderRegularColor = new Color(1f, 1f, 1f);

	[SerializeField]
	private Color _sliderSelectedColor = new Color(1f, 1f, 1f);

	[SerializeField]
	private float _regularTextSize = 30f;

	[SerializeField]
	private float _selectedTextSize = 40f;

	private int _startingPlayerResVal;

	private int _startingTownResVal;

	private int _currentResourcesVal;

	private int _playerResourcesVal;

	private int _townResourcesVal;

	private Action<UIResourceSlider> _onSelect;

	public int CurrentVal => _currentResourcesVal;

	private void Awake()
	{
		_resourceSlider.onValueChanged.AddListener(OnSliderValueChanged);
		FHSlider resourceSlider = _resourceSlider;
		resourceSlider.OnBeginDragEvent = (Action)Delegate.Combine(resourceSlider.OnBeginDragEvent, (Action)(() =>
		{
			SetState(selected: true);
		}));
		FHSlider resourceSlider2 = _resourceSlider;
		resourceSlider2.OnEndDragEvent = (Action)Delegate.Combine(resourceSlider2.OnEndDragEvent, (Action)(() =>
		{
			SetState(selected: false);
		}));
	}

	private void OnDisable()
	{
		_onSelect = null;
		ResetValues();
	}

	public void Init(int playerResource, int townResource, Action<UIResourceSlider> onSelect = null, bool forceRefresh = true)
	{
		_onSelect = onSelect;
		_startingPlayerResVal = playerResource;
		_startingTownResVal = townResource;
		_currentResourcesVal = ((!forceRefresh) ? Mathf.Clamp(_currentResourcesVal, 0, playerResource) : 0);
		SetValues(_currentResourcesVal, playerResource - _currentResourcesVal, townResource + _currentResourcesVal);
		_resourceSlider.SetValueWithoutNotify(_currentResourcesVal);
		_resourceSlider.maxValue = playerResource;
		_resourceSlider.minValue = 0f;
		_resourceSlider.interactable = playerResource > 0;
		_resourceSlider.wholeNumbers = true;
		_playerResources.color = ((playerResource > 0) ? _regularTextColor : _unavailableTextColor);
	}

	private void ResetValues()
	{
		_currentResourcesVal = 0;
		_playerResourcesVal = 0;
		_townResourcesVal = 0;
		_startingPlayerResVal = 0;
		_startingTownResVal = 0;
	}

	private void SetValues(int currentResource, int playerResource, int townResource)
	{
		_currentResourcesVal = currentResource;
		_playerResourcesVal = playerResource;
		_townResourcesVal = townResource;
		_sliderText.text = _currentResourcesVal.ToString();
		_playerResources.text = _playerResourcesVal.ToString();
		_townResources.text = _townResourcesVal.ToString();
	}

	private void OnSliderValueChanged(float val)
	{
		int num = (int)val;
		SetValues(num, _startingPlayerResVal - num, _startingTownResVal + num);
	}

	public void SetState(bool selected)
	{
		_sliderBackground.color = (selected ? _sliderSelectedColor : _sliderRegularColor);
		_playerResources.fontSize = (selected ? _selectedTextSize : _regularTextSize);
		_townResources.fontSize = (selected ? _selectedTextSize : _regularTextSize);
		_playerResources.color = (selected ? _selectedTextColor : _regularTextColor);
		_townResources.color = (selected ? _selectedTextColor : _regularTextColor);
		_resourceLeftImage.color = (selected ? _selectedTextColor : _regularTextColor);
		_resourceRightImage.color = (selected ? _selectedTextColor : _regularTextColor);
	}
}
