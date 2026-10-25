using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using TMPro;
using UnityEngine;

namespace FHGUI.Modals;

public class CalendarModal : MonoBehaviour
{
	[SerializeField]
	private FHButton _nextYearButton;

	[SerializeField]
	private FHButton _previousYearButton;

	[SerializeField]
	private RectTransform _weekContainer;

	[SerializeField]
	private TMP_Text _yearNum;

	[SerializeField]
	private CalendarEventsDescListContainer _tooltip;

	private CalendarDTO _data;

	private Action _onClose;

	private uint _currentlyViewedYear;

	private UICalendarWeek[] _weeks;

	private bool _setup;

	private void OnDisable()
	{
		_data = null;
		_onClose = null;
	}

	private void Setup()
	{
		if (!_setup)
		{
			_nextYearButton.onClick.AddListener(ShowNextYear);
			_previousYearButton.onClick.AddListener(ShowPreviousYear);
			_weeks = _weekContainer.GetComponentsInChildren<UICalendarWeek>();
			_setup = true;
		}
	}

	public void Init(CalendarDTO data, Action onClose = null)
	{
		gameObject.SetActive(value: true);
		Setup();
		_data = data;
		_onClose = onClose;
		_currentlyViewedYear = data.CurrentYear;
		ShowYear(_currentlyViewedYear);
		ToggleTooltip(args: false);
	}

	public void Close(bool sendEvents = true)
	{
		if (sendEvents)
		{
			_onClose?.Invoke();
		}
		gameObject.SetActive(value: false);
	}

	private void InitWeeks(uint year)
	{
		CalendarYearDTO year2 = GetYear(year);
		for (int i = 0; i < _weeks.Length; i++)
		{
			CalendarWeekDTO week = year2[i];
			_weeks[i].Init(week, week.WeekNum == _data.CurrentWeek, OnWeekHover);
		}
	}

	private void RefreshState()
	{
		_previousYearButton.interactable = _currentlyViewedYear > 1;
		_nextYearButton.interactable = _data.Years.Any((KeyValuePair<uint, CalendarYearDTO> year) => year.Key > _currentlyViewedYear);
	}

	private void ShowNextYear()
	{
		ShowYear(_currentlyViewedYear + 1);
	}

	private void ShowPreviousYear()
	{
		if (_currentlyViewedYear > 1)
		{
			ShowYear(_currentlyViewedYear - 1);
		}
	}

	private void ShowYear(uint year)
	{
		_currentlyViewedYear = year;
		_yearNum.text = _currentlyViewedYear.ToString();
		_previousYearButton.interactable = _currentlyViewedYear > 1;
		InitWeeks(_currentlyViewedYear);
		RefreshState();
	}

	private CalendarYearDTO GetYear(uint year)
	{
		return _data.GetYearData(year);
	}

	private void OnWeekHover(uint absoluteWeek, bool hover)
	{
		if (!hover)
		{
			ToggleTooltip(args: false);
			return;
		}
		uint weekIndex = CalendarUtils.GetWeekIndex(absoluteWeek);
		CalendarWeekDTO calendarWeekDTO = _data.GetYearData(_currentlyViewedYear)[weekIndex];
		if (!calendarWeekDTO.CampaignEvents.Any())
		{
			ToggleTooltip(args: false);
			return;
		}
		_tooltip.Init(null, calendarWeekDTO.CampaignEvents);
		ToggleTooltip(args: true);
	}

	private void ToggleTooltip(bool args)
	{
		if (_tooltip != null)
		{
			_tooltip.gameObject.SetActive(args);
		}
	}
}
