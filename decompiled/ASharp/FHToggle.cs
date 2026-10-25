using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class FHToggle : Toggle, IFHSelectable
{
	private TextMeshProUGUI _text;

	private CanvasGroup _canvas;

	private UITextTooltipTarget _tooltip;

	private string _tooltipOnText;

	private string _tooltipOffText;

	private string _tooltipDisabledText;

	public TextMeshProUGUI Text
	{
		get
		{
			if (!_text)
			{
				return _text = GetComponentInChildren<TextMeshProUGUI>();
			}
			return _text;
		}
	}

	public CanvasGroup CanvasGroup
	{
		get
		{
			if (!_canvas)
			{
				return _canvas = GetComponent<CanvasGroup>();
			}
			return _canvas;
		}
	}

	public UITextTooltipTarget Tooltip
	{
		get
		{
			if (!_tooltip)
			{
				return _tooltip = GetComponent<UITextTooltipTarget>();
			}
			return _tooltip;
		}
	}

	public bool IsFaded { get; private set; }

	public bool IsActiveAndEnabled => isActiveAndEnabled;

	public event UnityAction HighlightBegin;

	public event UnityAction HighlightEnd;

	public event UnityAction PointerEntered;

	public event UnityAction PointerExited;

	public event UnityAction PointerDown;

	public event UnityAction PointerUp;

	public event UnityAction Selected;

	public event UnityAction Deselected;

	public event UnityAction<IFHSelectable.SelectionState> OnSelectionStateChanged;

	public new bool IsHighlighted()
	{
		return base.IsHighlighted();
	}

	public bool IsSelected()
	{
		if (!isOn)
		{
			return currentSelectionState == SelectionState.Selected;
		}
		return true;
	}

	protected override void Awake()
	{
		base.Awake();
		onValueChanged.AddListener(RefreshState);
	}

	private void RefreshState(bool arg0)
	{
		SelectionState selectionState = currentSelectionState;
		if (selectionState == SelectionState.Normal)
		{
			selectionState = (isOn ? SelectionState.Selected : SelectionState.Normal);
		}
		DoStateTransition(selectionState, instant: true);
		SetTooltipTextState();
	}

	protected override void OnDisable()
	{
		bool flag = currentSelectionState == SelectionState.Selected;
		bool flag2 = IsHighlighted();
		base.OnDisable();
		if (flag2)
		{
			OnPointerExit(new PointerEventData(EventSystem.current));
		}
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
			Deselected?.Invoke();
		}
		else if (flag)
		{
			Deselected?.Invoke();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveAllEventListeners();
		_text = null;
		_canvas = null;
	}

	public void SetAsFaded(bool faded, float fade = 0.3f)
	{
		if (CanvasGroup == null)
		{
			Debug.LogError(name + " doesn't have canvas group to be set as faded.", this);
			return;
		}
		IsFaded = faded;
		CanvasGroup.alpha = (faded ? fade : 1f);
	}

	public void SetTooltipText(string onText, string offText, string disabledText)
	{
		_tooltipOnText = onText;
		_tooltipOffText = offText;
		_tooltipDisabledText = disabledText;
		SetTooltipTextState();
	}

	private void SetTooltipTextState()
	{
		if (!(Tooltip == null))
		{
			UITextTooltipTarget tooltip = _tooltip;
			string text;
			if (interactable)
			{
				text = (isOn ? _tooltipOnText : _tooltipOffText);
			}
			else
			{
				text = _tooltipDisabledText;
			}
			tooltip.SetText(text);
		}
	}

	public void RemoveAllEventListeners()
	{
		onValueChanged.RemoveAllListeners();
		HighlightBegin = null;
		HighlightEnd = null;
		PointerEntered = null;
		PointerExited = null;
		PointerDown = null;
		PointerUp = null;
		Selected = null;
		Deselected = null;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && IsActive() && IsInteractable())
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
		base.OnPointerClick(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		PointerEntered?.Invoke();
		if (IsHighlighted())
		{
			HighlightBegin?.Invoke();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		bool flag = IsHighlighted();
		base.OnPointerExit(eventData);
		PointerExited?.Invoke();
		if (flag)
		{
			HighlightEnd?.Invoke();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		PointerDown?.Invoke();
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		PointerUp?.Invoke();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		Selected?.Invoke();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		Deselected?.Invoke();
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (gameObject.activeInHierarchy)
		{
			OnSelectionStateChanged?.Invoke(ConvertState(state));
		}
	}

	protected IFHSelectable.SelectionState ConvertState(SelectionState state)
	{
		return state switch
		{
			SelectionState.Normal => isOn ? IFHSelectable.SelectionState.Selected : IFHSelectable.SelectionState.Normal, 
			SelectionState.Highlighted => (!isOn) ? IFHSelectable.SelectionState.Highlighted : IFHSelectable.SelectionState.Selected, 
			SelectionState.Pressed => isOn ? IFHSelectable.SelectionState.Selected : IFHSelectable.SelectionState.Pressed, 
			SelectionState.Disabled => IFHSelectable.SelectionState.Disabled, 
			SelectionState.Selected => IFHSelectable.SelectionState.Selected, 
			_ => throw new ArgumentOutOfRangeException("state", $"Not expected SelectionState value: {state}"), 
		};
	}
}
