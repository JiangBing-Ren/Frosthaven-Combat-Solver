using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class FHButton : Button, IFHSelectable
{
	private TextMeshProUGUI _text;

	private UITextTooltipTarget _tooltip;

	private CanvasGroup _canvas;

	[SerializeField]
	private ButtonClickedEvent m_OnRightClick = new ButtonClickedEvent();

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

	public UITextTooltipTarget Tooltip
	{
		get
		{
			if (!_tooltip)
			{
				return _tooltip = GetComponentInChildren<UITextTooltipTarget>();
			}
			return _tooltip;
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

	public bool IsFaded { get; private set; }

	public bool IsActiveAndEnabled => isActiveAndEnabled;

	public ButtonClickedEvent onRightClick
	{
		get
		{
			return m_OnRightClick;
		}
		set
		{
			m_OnRightClick = value;
		}
	}

	public event UnityAction HighlightBegin;

	public event UnityAction HighlightEnd;

	public event UnityAction PointerEntered;

	public event UnityAction PointerExited;

	public event UnityAction PointerDown;

	public event UnityAction PointerUp;

	public event UnityAction Selected;

	public event UnityAction Deselected;

	public event UnityAction<PointerEventData> PointerEvent;

	public event UnityAction<IFHSelectable.SelectionState> OnSelectionStateChanged;

	public new bool IsHighlighted()
	{
		return base.IsHighlighted();
	}

	public bool IsSelected()
	{
		return currentSelectionState == SelectionState.Selected;
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

	public virtual void RemoveAllEventListeners()
	{
		onClick.RemoveAllListeners();
		HighlightBegin = null;
		HighlightEnd = null;
		PointerEntered = null;
		PointerExited = null;
		PointerDown = null;
		PointerUp = null;
		Selected = null;
		Deselected = null;
	}

	public void DoLeftClick()
	{
		OnPointerClick(new PointerEventData(EventSystem.current)
		{
			button = PointerEventData.InputButton.Left
		});
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForFHButton(this))
		{
			Log.GUIInfo("Button press for button " + gameObject.name + "  intercepted and prevented by InteractabilityManager");
			return;
		}
		if (IsActive() && IsInteractable())
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				m_OnRightClick.Invoke();
			}
		}
		PointerEvent?.Invoke(eventData);
		base.OnPointerClick(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		PointerEvent?.Invoke(eventData);
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
		PointerEvent?.Invoke(eventData);
		PointerExited?.Invoke();
		if (flag)
		{
			HighlightEnd?.Invoke();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		PointerEvent?.Invoke(eventData);
		PointerDown?.Invoke();
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		PointerEvent?.Invoke(eventData);
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
			InvokeOnSelectionStateChanged(state);
		}
	}

	protected void InvokeOnSelectionStateChanged(SelectionState state)
	{
		OnSelectionStateChanged?.Invoke(ConvertState(state));
	}

	protected IFHSelectable.SelectionState ConvertState(SelectionState state)
	{
		return state switch
		{
			SelectionState.Normal => IFHSelectable.SelectionState.Normal, 
			SelectionState.Highlighted => IFHSelectable.SelectionState.Highlighted, 
			SelectionState.Pressed => IFHSelectable.SelectionState.Pressed, 
			SelectionState.Disabled => IFHSelectable.SelectionState.Disabled, 
			SelectionState.Selected => IFHSelectable.SelectionState.Selected, 
			_ => throw new ArgumentOutOfRangeException("state", $"Not expected SelectionState value: {state}"), 
		};
	}
}
