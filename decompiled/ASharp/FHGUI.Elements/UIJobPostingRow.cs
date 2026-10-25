using System;
using FHGUI.DTO.Outpost;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIJobPostingRow : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TMP_Text _title;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _completedCheckmark;

	[SerializeField]
	private FHButton _button;

	[Header("States")]
	[SerializeField]
	private Color _selectedTextColor;

	[SerializeField]
	private Color _normalTextColor;

	[SerializeField]
	private Color _completedTextColor;

	[SerializeField]
	private GameObject _highlight;

	private Action<UIJobPostingRow> _onSelect;

	public bool IsSelected { get; private set; }

	public bool IsHovered { get; private set; }

	public JobPostingDTO Data { get; private set; }

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
	}

	public void Init(JobPostingDTO jobPosting, Action<UIJobPostingRow> onSelect = null, bool isSelected = false)
	{
		_title.text = jobPosting.Name;
		Data = jobPosting;
		_onSelect = onSelect;
		IsSelected = isSelected;
		UpdateState();
	}

	private void UpdateState()
	{
		bool flag = IsSelected || IsHovered;
		_highlight.SetActive(flag);
		TMP_Text title = _title;
		Color color;
		if (flag)
		{
			color = _selectedTextColor;
		}
		else
		{
			color = (Data.IsCompleted ? _completedTextColor : _normalTextColor);
		}
		title.color = color;
		_icon.enabled = !Data.IsCompleted;
		_completedCheckmark.enabled = Data.IsCompleted;
	}

	private void OnDisable()
	{
		_onSelect = null;
		IsSelected = false;
		IsHovered = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsHovered = true;
		if (!IsSelected)
		{
			UpdateState();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsHovered = false;
		if (!IsSelected)
		{
			UpdateState();
		}
	}

	private void OnClick()
	{
		if (!IsSelected)
		{
			IsSelected = true;
			_onSelect?.Invoke(this);
			UpdateState();
		}
	}
}
