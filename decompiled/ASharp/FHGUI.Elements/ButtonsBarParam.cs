using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FHGUI.Elements;

[Serializable]
public struct ButtonsBarParam
{
	public string LabelKey;

	public Sprite Icon;

	public string HintKey;

	[NonSerialized]
	public InputAction Hotkey;

	public InputActionReference HotkeyReference;

	public Action Callback;

	public Action<object> CallbackWithData;

	public object UserData;

	public bool NonInteractable;

	public GameObject Prototype;

	public bool Interactable
	{
		get
		{
			return !NonInteractable;
		}
		set
		{
			NonInteractable = !value;
		}
	}

	public bool IsValid => !Equals(default(ButtonsBarParam));

	public ButtonsBarParam(string labelKey, InputAction hotkey, Action callback)
		: this(labelKey, "", hotkey, callback)
	{
	}

	public ButtonsBarParam(string labelKey, string hintKey, InputAction hotkey, Action callback)
	{
		LabelKey = labelKey;
		Icon = null;
		HintKey = hintKey;
		Hotkey = hotkey;
		HotkeyReference = null;
		Callback = callback;
		CallbackWithData = null;
		UserData = null;
		NonInteractable = false;
		Prototype = null;
	}

	public ButtonsBarParam(Sprite icon, InputAction hotkey, Action callback)
	{
		LabelKey = null;
		Icon = icon;
		HintKey = "";
		Hotkey = hotkey;
		HotkeyReference = null;
		Callback = callback;
		CallbackWithData = null;
		UserData = null;
		NonInteractable = false;
		Prototype = null;
	}

	public override string ToString()
	{
		return $"LabelKey: {LabelKey}, Prototype{Prototype}";
	}
}
