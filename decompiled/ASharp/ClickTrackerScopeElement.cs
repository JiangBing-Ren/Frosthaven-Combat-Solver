using System;
using System.Collections.Generic;
using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public sealed class ClickTrackerScopeElement : MonoBehaviour, IScopeElement
{
	public enum TrackingArea
	{
		WithinThisArea,
		OutsideThisArea,
		Anywhere
	}

	[Serializable]
	public class ClickButtonEvent : UnityEvent
	{
	}

	public List<RectTransform> area;

	public ClickButtonEvent onClick;

	public MouseButton clickType;

	public TrackingArea trackingArea;

	public bool useButtonDown = true;

	public string audioItemClick;

	public bool skipOutsideScreen = true;

	[SerializeField]
	private InputActionReference m_InputAction;

	private InputAction _inputAction;

	private RectTransform _screenArea;

	public InputActionReference InputAction => m_InputAction;

	private void Awake()
	{
		if (skipOutsideScreen)
		{
			_screenArea = transform.root as RectTransform;
		}
	}

	private void OnEnable()
	{
		if ((bool)InputAction)
		{
			InputFHManager.Controls.Enable(this, InputAction);
		}
	}

	private void OnDisable()
	{
		InputFHManager.Controls?.DisableAll(this);
	}

	private void Update()
	{
		if (useButtonDown ? InputFHManager.GetMouseDown(clickType) : InputFHManager.GetMouseUp(clickType))
		{
			if ((!(_screenArea != null) || RectTransformUtility.RectangleContainsScreenPoint(_screenArea, InputFHManager.CursorPosition, UIManager.Instance.UICamera)) && ((trackingArea == TrackingArea.WithinThisArea && InArea()) || (trackingArea == TrackingArea.OutsideThisArea && !InArea()) || trackingArea == TrackingArea.Anywhere))
			{
				ProcessClick();
			}
		}
		else if (_inputAction != null && _inputAction.WasPerformedThisFrame())
		{
			ProcessClick();
		}
	}

	private void ProcessClick()
	{
		if (onClick != null)
		{
			AudioControllerUtils.PlaySound(audioItemClick);
			onClick.Invoke();
		}
		else
		{
			Log.GUIError("ClickTrackerExtended: Trying to invoke but the event is null.");
		}
	}

	private bool InArea()
	{
		return area.Exists((RectTransform it) => IsVisible(it) && RectTransformUtility.RectangleContainsScreenPoint(it, InputFHManager.CursorPosition, UIManager.Instance.UICamera));
	}

	private bool IsVisible(RectTransform rect)
	{
		UIWindow component = rect.GetComponent<UIWindow>();
		if (component == null)
		{
			return rect.gameObject.activeInHierarchy;
		}
		return IsVisible(component);
	}

	private bool IsVisible(UIWindow rect)
	{
		if (rect.gameObject.activeInHierarchy)
		{
			return rect.IsOpen;
		}
		return false;
	}
}
