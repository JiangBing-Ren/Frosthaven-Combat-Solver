using Assets.Script.GUI.Controller.Keyboard;
using TMPro;
using UnityEngine;

namespace Assets.Script.GUI.Controller;

public class ControllerInputKeyboard : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField m_KeyboardInputField;

	[SerializeField]
	private UIKeyboard keyboard;

	private void Awake()
	{
		keyboard.OnSelectedKeyCode.AddListener(ProcessKeyCode);
		keyboard.Hide();
	}

	private void ProcessKeyCode(KeyCode keyCode)
	{
		if (keyCode == KeyCode.Delete)
		{
			Undo();
			return;
		}
		char[] array = KeyCodeConverter.ConvertToValue(keyCode).ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			m_KeyboardInputField.ProcessEvent(new Event
			{
				character = array[i]
			});
		}
		m_KeyboardInputField.ForceLabelUpdate();
	}

	private void Undo()
	{
		string text = m_KeyboardInputField.text;
		if (text.Length != 0)
		{
			m_KeyboardInputField.text = text.Substring(0, text.Length - 1);
		}
	}
}
