using System;
using FHGUI.DTO.Outpost;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UICalendarWeek : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TMP_Text _weekNum;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private GameObject _currentWeekIndicator;

	[SerializeField]
	private GameObject _hoverIndicator;

	[SerializeField]
	private CalendarEventsListContainer _eventContainer;

	[Header("States")]
	[SerializeField]
	private Color _winterTextColor;

	[SerializeField]
	private Sprite _winterBackground;

	[SerializeField]
	private Color _summerTextColor;

	[SerializeField]
	private Sprite _summerBackground;

	private CalendarWeekDTO? _data;

	private Action<uint, bool> _onHover;

	public void Init(CalendarWeekDTO week, bool isCurrentWeek, Action<uint, bool> onHover = null)
	{
		_data = week;
		_weekNum.text = week.WeekNum.ToString();
		_background.sprite = ((week.Season == CalendarSeason.Summer) ? _summerBackground : _winterBackground);
		_weekNum.color = ((week.Season == CalendarSeason.Summer) ? _summerTextColor : _winterTextColor);
		_currentWeekIndicator.gameObject.SetActive(isCurrentWeek);
		_eventContainer.Init(week.CampaignEvents);
		_onHover = onHover;
		OnHover(hover: false, propagateEvent: false);
	}

	private void OnDisable()
	{
		OnHover(hover: false, propagateEvent: false);
		_data = null;
		_onHover = null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHover(hover: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnHover(hover: false);
	}

	private void OnHover(bool hover, bool propagateEvent = true)
	{
		if (_hoverIndicator != null)
		{
			_hoverIndicator.gameObject.SetActive(hover);
		}
		if (propagateEvent && _data.HasValue)
		{
			_onHover?.Invoke(_data.Value.WeekNum, hover);
		}
	}
}
