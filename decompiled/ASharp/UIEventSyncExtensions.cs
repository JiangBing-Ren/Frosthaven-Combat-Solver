using TMPro;
using UnityEngine.UI;

public static class UIEventSyncExtensions
{
	private static Slider.SliderEvent emptySliderEvent = new Slider.SliderEvent();

	private static Toggle.ToggleEvent emptyToggleEvent = new Toggle.ToggleEvent();

	private static InputField.OnChangeEvent emptyInputFieldEvent = new InputField.OnChangeEvent();

	private static TMP_InputField.OnChangeEvent emptyTMPInputFieldEvent = new TMP_InputField.OnChangeEvent();

	private static Dropdown.DropdownEvent emptyDropdownEvent = new Dropdown.DropdownEvent();

	public static void SetValue(this Slider instance, float value)
	{
		Slider.SliderEvent onValueChanged = instance.onValueChanged;
		instance.onValueChanged = emptySliderEvent;
		instance.value = value;
		instance.onValueChanged = onValueChanged;
	}

	public static void SetValue(this Toggle instance, bool value)
	{
		Toggle.ToggleEvent onValueChanged = instance.onValueChanged;
		instance.onValueChanged = emptyToggleEvent;
		instance.isOn = value;
		instance.onValueChanged = onValueChanged;
	}

	public static void SetValue(this InputField instance, string value)
	{
		InputField.OnChangeEvent onValueChanged = instance.onValueChanged;
		instance.onValueChanged = emptyInputFieldEvent;
		instance.text = value;
		instance.onValueChanged = onValueChanged;
	}

	public static void SetValue(this TMP_InputField instance, string value)
	{
		TMP_InputField.OnChangeEvent onValueChanged = instance.onValueChanged;
		instance.onValueChanged = emptyTMPInputFieldEvent;
		instance.text = value;
		instance.onValueChanged = onValueChanged;
	}

	public static void SetValue(this Dropdown instance, int value)
	{
		Dropdown.DropdownEvent onValueChanged = instance.onValueChanged;
		instance.onValueChanged = emptyDropdownEvent;
		instance.value = value;
		instance.onValueChanged = onValueChanged;
	}
}
