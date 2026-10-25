using System;
using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class ButtonsBarElement : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private HotkeyBaseScopeElement _hotkey;

	private UIScope _scope;

	private ButtonsBarParam _currentParam;

	private GameObject Prototype;

	public static ButtonsBarParam Common_Back(Action callback)
	{
		return new ButtonsBarParam("GUI_BACK", null, callback);
	}

	public static ButtonsBarParam Common_Select(Action callback, bool interactable)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_SELECT", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		return result;
	}

	public static ButtonsBarParam Common_Continue(Action callback, bool interactable)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_CONTINUE", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		return result;
	}

	public static ButtonsBarParam Common_Reset(Action callback, bool interactable)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_RESET", null, callback);
		result.NonInteractable = !interactable;
		return result;
	}

	public static void SetButtons(Transform buttonsContainer, params ButtonsBarParam[] buttonParams)
	{
		buttonParams = buttonParams.Where((ButtonsBarParam bp) => bp.IsValid).ToArray();
		List<ButtonsBarElement> list = buttonsContainer.GetComponentsInChildren<ButtonsBarElement>(includeInactive: true).ToList();
		GameObject gameObject = null;
		foreach (ButtonsBarElement item in list)
		{
			item.gameObject.SetActive(value: false);
			item.ClearParam();
			if (gameObject == null && item.Prototype == null)
			{
				gameObject = item.gameObject;
			}
		}
		for (int num = 0; num < buttonParams.Length; num++)
		{
			ButtonsBarParam param = buttonParams[num];
			ButtonsBarElement orCreateButton = GetOrCreateButton(buttonsContainer, gameObject, list, param);
			orCreateButton.transform.SetSiblingIndex(num);
			orCreateButton.gameObject.SetActive(value: true);
			list.Remove(orCreateButton);
		}
	}

	private static ButtonsBarElement GetOrCreateButton(Transform container, GameObject defaultPrototype, List<ButtonsBarElement> buttonsBarElements, ButtonsBarParam param)
	{
		ButtonsBarElement buttonsBarElement = buttonsBarElements.Find((ButtonsBarElement element) => element.Prototype == param.Prototype);
		if ((object)buttonsBarElement == null)
		{
			GameObject gameObject = (param.Prototype ? param.Prototype : defaultPrototype);
			buttonsBarElement = UnityEngine.Object.Instantiate(gameObject).GetComponent<ButtonsBarElement>();
			buttonsBarElement.Prototype = gameObject;
			buttonsBarElement.transform.SetParent(container, worldPositionStays: false);
		}
		buttonsBarElement.SetParam(param);
		return buttonsBarElement;
	}

	public static void ClearButtons(Transform buttonsContainer)
	{
		List<ButtonsBarElement> pool = buttonsContainer.GetComponentsInChildren<ButtonsBarElement>(includeInactive: true).ToList();
		if (pool.Count == 0)
		{
			return;
		}
		HelperTools.NormalizePool(ref pool, pool[0].gameObject, buttonsContainer, 0);
		foreach (ButtonsBarElement item in pool)
		{
			item.gameObject.SetActive(value: false);
			item.ClearParam();
		}
	}

	private void Awake()
	{
		_button.onClick.AddListener(OnClicked);
	}

	public bool TrySetParam(ButtonsBarParam? param)
	{
		if (param.HasValue)
		{
			SetParam(param.Value);
			return true;
		}
		ClearParam();
		return false;
	}

	public void SetParam(ButtonsBarParam param)
	{
		_currentParam = param;
		_button.interactable = !param.NonInteractable;
		if (!string.IsNullOrWhiteSpace(param.LabelKey))
		{
			if (param.LabelKey.StartsWith('@'))
			{
				_button.SetNonLocalizedText(param.LabelKey.Remove(0, 1));
			}
			else
			{
				_button.SetTextKey(param.LabelKey);
			}
		}
		if (_icon != null)
		{
			_icon.sprite = param.Icon;
		}
		if (_hotkey != null)
		{
			if (_scope == null)
			{
				_scope = GetComponentInParent<UIScope>(includeInactive: true);
			}
			if (param.Hotkey != null && param.HotkeyReference != null)
			{
				Debug.LogError($"Trying to set buttons bar parameter with Hotkey {param.Hotkey} and HotkeyReference {param.HotkeyReference} for {name}. This is strange.", this);
			}
			InputActionReference inputAction = param.HotkeyReference;
			if (param.Hotkey != null)
			{
				inputAction = InputActionReference.Create(param.Hotkey);
			}
			_hotkey.SetInputAction(inputAction);
			_hotkey.ApplyInputActionToChildren();
			if ((bool)_scope)
			{
				_scope.TryAppendOwnedScopeElements(_hotkey.GetComponentsInChildren<IScopeElement>());
			}
		}
	}

	public void ClearParam()
	{
		_currentParam = default;
	}

	private void OnClicked()
	{
		_currentParam.Callback?.Invoke();
		_currentParam.CallbackWithData?.Invoke(_currentParam.UserData);
	}
}
