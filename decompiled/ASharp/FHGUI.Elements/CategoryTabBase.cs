using System;
using UnityEngine;

namespace FHGUI.Elements;

public abstract class CategoryTabBase<CategoryType> : MonoBehaviour where CategoryType : Enum
{
	public delegate void CategoryEventHandler(CategoryType category);

	public CategoryType Category;

	[SerializeField]
	private FHTabButton _button;

	[SerializeField]
	private GameObject _notificationBadge;

	[SerializeField]
	private UITextTooltipTarget _tooltipTarget;

	public UITextTooltipTarget Tooltip => _tooltipTarget;

	public bool IsOn
	{
		get
		{
			return _button.IsFocused;
		}
		set
		{
			_button.IsFocused = value;
		}
	}

	public bool IsInteractable
	{
		get
		{
			return _button.IsInteractable();
		}
		set
		{
			_button.interactable = value;
		}
	}

	public bool ShowNotificationBadge
	{
		get
		{
			return _notificationBadge.activeSelf;
		}
		set
		{
			_notificationBadge.SetActive(value);
		}
	}

	public event CategoryEventHandler Clicked;

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
		if (_tooltipTarget != null)
		{
			_tooltipTarget.tooltipText = (_tooltipTarget.tooltipText.IsNullOrEmpty() ? Category.ToString().SplitCamelCase() : _tooltipTarget.tooltipText);
		}
	}

	private void OnClick()
	{
		Clicked?.Invoke(Category);
	}
}
