using System;
using FHGUI;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modals;
using FHGUI.Services;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainHeaderModule : FHMapViewModule
{
	[Header("Hero")]
	[SerializeField]
	private Image _heroIcon;

	[SerializeField]
	private FHButton _heroPortrait;

	[SerializeField]
	private UITextTooltipTarget _heroPortraitTooltip;

	[SerializeField]
	private TMP_Text _heroLevel;

	[SerializeField]
	private TMP_Text _heroData;

	[Header("Town")]
	[SerializeField]
	private TMP_Text _townLevel;

	[SerializeField]
	private FHButton _outpostButton;

	[Header("Resources")]
	[SerializeField]
	private ResourcesBarElement _characterResources;

	[SerializeField]
	private ResourcesBarElement _outpostResources;

	[Header("Town Stats")]
	[SerializeField]
	private ResourceElement _moraleStat;

	[SerializeField]
	private ResourceElement _inspirationStat;

	[SerializeField]
	private ResourceElement _defenseStat;

	[SerializeField]
	private ResourceElement _soldiersStat;

	[SerializeField]
	private ResourceElement _prosperityStat;

	[Header("Seasons")]
	[SerializeField]
	private TMP_Text _weekText;

	[SerializeField]
	private UITextTooltipTarget _weekTooltip;

	[SerializeField]
	private FHRadialElement _seasonRadial;

	[SerializeField]
	private Image _seasonRadialImage;

	[SerializeField]
	private Image _seasonIcon;

	[SerializeField]
	private Sprite _summerIcon;

	[SerializeField]
	private Sprite _winterIcon;

	[SerializeField]
	private Color _summerColor;

	[SerializeField]
	private Color _winterColor;

	[SerializeField]
	private FHToggle _calendarToggle;

	[SerializeField]
	private CalendarModal _calendar;

	[Header("Side buttons")]
	[SerializeField]
	private FHButton _settingsButton;

	[SerializeField]
	private FHButton _wikiButton;

	[SerializeField]
	private GameObject _newWikiEntryIndicator;

	[SerializeField]
	private FHButton _eventLogButton;

	[SerializeField]
	private FHButton _closeButton;

	private MainHeaderDTO _headerData;

	private HeaderViewService.HeaderParams _params;

	protected override bool PlaySoundOnEnable => false;

	public GameObject TownIconGO => _outpostButton?.gameObject;

	public ResourcesBarElement CharacterResources => _characterResources;

	public ResourcesBarElement OutpostResources => _outpostResources;

	private void Awake()
	{
		_heroPortrait.onClick.AddListener(() =>
		{
			_params?.OnPortraitClicked?.Invoke();
		});
		_heroPortrait.onRightClick.AddListener(() =>
		{
			_params?.OnPortraitClicked?.Invoke();
		});
		_outpostButton.onClick.AddListener(() =>
		{
			_params?.OnTownClicked?.Invoke();
		});
		_closeButton.onClick.AddListener(() =>
		{
			_params?.OnCloseClicked?.Invoke();
		});
		_settingsButton.onClick.AddListener(() =>
		{
			_params?.OnSettingsClicked?.Invoke();
		});
		_wikiButton.onClick.AddListener(() =>
		{
			_params?.OnWikiClicked?.Invoke();
		});
		_eventLogButton.onClick.AddListener(() =>
		{
			_params?.OnEventLogClicked?.Invoke();
		});
		_calendarToggle.onValueChanged.AddListener(ToggleCalendar);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_params = null;
	}

	public void Init(MainHeaderDTO headerData, HeaderViewService.HeaderFunctionality functionality = HeaderViewService.HeaderFunctionality.All, HeaderViewService.HeaderParams @params = null)
	{
		_headerData = headerData;
		_params = @params;
		bool flag = _headerData.SelectedHero != null;
		CharacterDataDTO characterDataDTO = _headerData.SelectedHero ?? CharacterDataDTO.GenerateDummyDto();
		_heroPortraitTooltip.enabled = flag;
		if (flag)
		{
			ECharacter character = CrossReference.ClassIdToECharacterModel(_headerData.SelectedHero.Class.ID.ID);
			_heroIcon.enabled = true;
			_heroIcon.sprite = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration;
			_heroLevel.enabled = true;
			_heroLevel.text = characterDataDTO.Level.ToString();
			_heroData.enabled = true;
			_heroData.text = string.Format("{0}, {1} {2}", characterDataDTO.Name, LocalizationManager.GetTranslation("GUI_LEVEL"), characterDataDTO.Level);
			_heroPortraitTooltip.SetText(characterDataDTO.Name);
		}
		else
		{
			_heroIcon.enabled = false;
			_heroLevel.enabled = false;
			_heroData.enabled = false;
		}
		_heroPortrait.interactable = _params?.OnPortraitClicked != null;
		_outpostButton.interactable = _params?.OnTownClicked != null;
		_townLevel.text = _headerData.ProsperityLevel.ToString();
		int gold = (int)(flag ? _headerData.SelectedHero.Gold : 0);
		OutpostResources resources = (flag ? _headerData.SelectedHero.Resources : default(OutpostResources));
		_outpostResources.Init(0, _headerData.OutpostResources);
		_characterResources.Init(gold, resources);
		_moraleStat.Init(_headerData.Morale);
		_inspirationStat.Init(_headerData.Inspiration);
		_defenseStat.Init(_headerData.TotalDefense);
		_soldiersStat.Init(_headerData.Soldiers);
		_prosperityStat.Init(_headerData.Prosperity);
		if ((bool)_weekTooltip)
		{
			string translation = LocalizationManager.GetTranslation((_headerData.Season == CalendarSeason.Summer) ? "GUI_SUMMER" : "GUI_Winter");
			translation += Environment.NewLine;
			translation += FHUILocFormatting.PopulateParams("GUI_WEEK", new LocalizationParam("{0}", (_headerData.Week != 0) ? ((_headerData.Week - 1) % 10 + 1) : 0u), new LocalizationParam("{1}", 10));
			_weekTooltip.SetText(translation);
		}
		_weekText.text = _headerData.Week.ToString();
		float progress = ((_headerData.Week == 0) ? 0f : ((float)((_headerData.Week - 1) % 10 + 1) / 10f));
		_seasonRadial.SetProgress(progress);
		_seasonRadialImage.color = ((_headerData.Season == CalendarSeason.Summer) ? _summerColor : _winterColor);
		_seasonIcon.sprite = ((_headerData.Season == CalendarSeason.Summer) ? _summerIcon : _winterIcon);
		_settingsButton.interactable = @params != null && !@params.IsStandalone && @params.OnSettingsClicked != null;
		_wikiButton.interactable = @params != null && !@params.IsStandalone && @params.OnWikiClicked != null;
		_eventLogButton.interactable = @params != null && !@params.IsStandalone && @params.OnEventLogClicked != null;
		_closeButton.gameObject.SetActive(@params?.OnCloseClicked != null);
		_newWikiEntryIndicator.SetActive(_headerData.HasNewCodexEntry);
		if (_wikiButton != null)
		{
			UIContextualTutorialModule.OnEvent("Codex", _wikiButton);
		}
		_calendar.gameObject.SetActive(value: false);
		_calendarToggle.SetIsOnWithoutNotify(value: false);
	}

	private void ToggleCalendar(bool show)
	{
		if (show)
		{
			_calendar.Init(_headerData.Calendar, OnCalendarClose);
		}
		else
		{
			_calendar.Close(sendEvents: false);
		}
	}

	private void OnCalendarClose()
	{
		_calendarToggle.SetIsOnWithoutNotify(value: false);
	}
}
