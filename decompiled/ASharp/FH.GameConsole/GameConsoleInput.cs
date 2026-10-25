using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevLocker.GFrame.Input.UIScope;
using Libs.GameConsole;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FH.GameConsole;

public class GameConsoleInput : MonoBehaviour
{
	private class EnableConsoleCodeInfo
	{
		public Key[] EnableConsoleCode;

		public int NextKeyPos;

		public bool OpenConsole;

		public bool OpenAnvilCheats;
	}

	[Flags]
	private enum InputModifiers
	{
		None = 0,
		Ctrl = 1,
		Alt = 2,
		Shift = 4
	}

	private class InputBoundCommand : IEquatable<InputBoundCommand>
	{
		public readonly string Input;

		public readonly InputModifiers Modifiers;

		public readonly string Command;

		private readonly Key _inputKey;

		public InputBoundCommand(string input, InputModifiers modifiers, string command)
		{
			Input = input;
			Modifiers = modifiers;
			Command = command;
			_inputKey = InputFHManager.GetInputKey(Input);
		}

		public bool IsActive()
		{
			if (InputFHManager.GetKeyDown(_inputKey))
			{
				InputModifiers inputModifiers = InputModifiers.None;
				if (InputFHManager.GetKey(Key.LeftCtrl) || InputFHManager.GetKey(Key.RightCtrl))
				{
					inputModifiers |= InputModifiers.Ctrl;
				}
				if (InputFHManager.GetKey(Key.LeftAlt) || InputFHManager.GetKey(Key.RightAlt))
				{
					inputModifiers |= InputModifiers.Alt;
				}
				if (InputFHManager.GetKey(Key.LeftShift) || InputFHManager.GetKey(Key.RightShift))
				{
					inputModifiers |= InputModifiers.Shift;
				}
				return Modifiers == inputModifiers;
			}
			return false;
		}

		public bool Equals(InputBoundCommand other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (string.Equals(Input, other.Input))
			{
				return Modifiers == other.Modifiers;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((InputBoundCommand)obj);
		}

		public override int GetHashCode()
		{
			return (((Input != null) ? Input.GetHashCode() : 0) * 397) ^ (int)Modifiers;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Modifiers != InputModifiers.None)
			{
				if ((Modifiers & InputModifiers.Ctrl) != 0)
				{
					stringBuilder.Append("ctrl ");
				}
				if ((Modifiers & InputModifiers.Alt) != 0)
				{
					stringBuilder.Append("alt ");
				}
				if ((Modifiers & InputModifiers.Shift) != 0)
				{
					stringBuilder.Append("shift ");
				}
				stringBuilder.Append("+ ");
			}
			stringBuilder.Append(Input).Append(" - ").Append(Command);
			return stringBuilder.ToString();
		}
	}

	private EnableConsoleCodeInfo[] EnableConsoleCodeInfos = new EnableConsoleCodeInfo[1]
	{
		new EnableConsoleCodeInfo
		{
			EnableConsoleCode = new Key[12]
			{
				Key.UpArrow,
				Key.DownArrow,
				Key.LeftArrow,
				Key.RightArrow,
				Key.S,
				Key.N,
				Key.A,
				Key.P,
				Key.S,
				Key.H,
				Key.O,
				Key.T
			},
			OpenConsole = true
		}
	};

	private static GameConsoleInput _instance;

	public UIScope OwningScope;

	private GameConsoleWindow _console;

	private bool _prevVisible;

	private bool _inputDisabled;

	private readonly List<InputBoundCommand> _inputBoundCommands = new List<InputBoundCommand>();

	public static GameConsoleInput Instance
	{
		get
		{
			if (!(_instance != null))
			{
				return _instance = UnityEngine.Object.FindObjectOfType<GameConsoleInput>();
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	public void Awake()
	{
		Instance = this;
		_console = GetComponent<GameConsoleWindow>();
	}

	private void Update()
	{
		if (GameConsoleWindow.DisableConsoleAccess && InputFHManager.AnyKey())
		{
			EnableConsoleCodeInfo[] enableConsoleCodeInfos = EnableConsoleCodeInfos;
			foreach (EnableConsoleCodeInfo enableConsoleCodeInfo in enableConsoleCodeInfos)
			{
				if (!InputFHManager.GetKeyDown(enableConsoleCodeInfo.EnableConsoleCode[enableConsoleCodeInfo.NextKeyPos]))
				{
					enableConsoleCodeInfo.NextKeyPos = 0;
					continue;
				}
				enableConsoleCodeInfo.NextKeyPos++;
				if (enableConsoleCodeInfo.NextKeyPos == enableConsoleCodeInfo.EnableConsoleCode.Length)
				{
					enableConsoleCodeInfo.NextKeyPos = 0;
					GameConsoleWindow.DisableConsoleAccess = false;
					Main.s_DevMode = true;
					if (enableConsoleCodeInfo.OpenConsole)
					{
						Invoke("ShowConsole", 0.1f);
					}
					_ = enableConsoleCodeInfo.OpenAnvilCheats;
				}
			}
		}
		if ((bool)OwningScope)
		{
			OwningScope.enabled = !_inputDisabled && _console.enabled;
			if (OwningScope.enabled && !OwningScope.IsFocused)
			{
				_console.ToggleVisibility();
				OwningScope.enabled = false;
			}
		}
		if (_inputDisabled)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		bool flag = current?.currentSelectedGameObject != null && current?.currentSelectedGameObject.GetComponent<InputField>() != null && current.currentSelectedGameObject.GetComponent<InputField>().isActiveAndEnabled;
		if (!flag)
		{
			flag = current?.currentSelectedGameObject != null && current?.currentSelectedGameObject.GetComponent<TMP_InputField>() != null && current.currentSelectedGameObject.GetComponent<TMP_InputField>().isActiveAndEnabled;
		}
		bool flag2 = false;
		if (flag && GameConsoleWindow.Instance != null)
		{
			flag2 = current.currentSelectedGameObject == GameConsoleWindow.Instance.InputField.gameObject;
		}
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Gamepad current2 = Gamepad.current;
		if (current2 != null)
		{
			flag3 = (current2.leftStickButton.isPressed && current2.rightStickButton.wasPressedThisFrame) || (current2.leftStickButton.wasPressedThisFrame && current2.rightStickButton.isPressed);
			flag4 = current2.buttonSouth.wasPressedThisFrame || current2.rightTrigger.wasPressedThisFrame;
			flag5 = current2.buttonEast.wasPressedThisFrame;
		}
		if (((InputFHManager.GetKeyDown(Key.Backquote) || (!_console.Visible && InputFHManager.GetKeyDown(Key.Slash))) | flag3) && (!flag | flag2))
		{
			_console.ToggleVisibility();
		}
		if (GameConsoleWindow.DisableConsoleAccess)
		{
			return;
		}
		if (_console.Visible & flag2)
		{
			if ((InputFHManager.GetKeyDown(Key.Enter) || InputFHManager.GetKeyDown(Key.NumpadEnter)) | flag4)
			{
				if (_console.HasSelectedSuggestion)
				{
					_console.CompleteWithAutocomplete();
				}
				_console.OnSubmit();
			}
			if (InputFHManager.GetKeyDown(Key.Escape) | flag5)
			{
				_console.ToggleVisibility();
			}
			if (InputFHManager.GetKeyDown(Key.UpArrow))
			{
				if (IsControlPressed() && _console.CanSelectSuggestion)
				{
					_console.OnPreviousSuggestion();
				}
				else
				{
					_console.OnPrevCommand();
				}
			}
			if (InputFHManager.GetKeyDown(Key.DownArrow))
			{
				if (IsControlPressed() && _console.CanSelectSuggestion)
				{
					_console.OnNextSuggestion();
				}
				else
				{
					_console.OnNextCommand();
				}
			}
			if (InputFHManager.GetKeyDown(Key.Tab))
			{
				if (_console.CanSelectSuggestion && IsShiftPressed())
				{
					_console.OnPreviousSuggestion();
				}
				else if (_console.CanSelectSuggestion && IsControlPressed())
				{
					_console.OnNextSuggestion();
				}
				else
				{
					_console.OnComplete();
				}
			}
		}
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		LevelEditorUI levelEditorUI = s_Instance?.m_LevelEditorUIInstance;
		bool flag6 = levelEditorUI != null && s_Instance.IsEditing && levelEditorUI.PropPanel.isActiveAndEnabled;
		if (flag || flag6)
		{
			return;
		}
		foreach (InputBoundCommand inputBoundCommand in _inputBoundCommands)
		{
			if (!inputBoundCommand.IsActive())
			{
				continue;
			}
			foreach (InputAction allAction in Singleton<InputFHManager>.Instance.InputContext.GetAllActions())
			{
				allAction.Reset();
			}
			_console.ExecuteCommandLine(inputBoundCommand.Command);
		}
	}

	private void ShowConsole()
	{
		if (!_console.Visible)
		{
			_console.ToggleVisibility();
		}
	}

	private bool IsControlPressed()
	{
		if (!InputFHManager.GetKey(Key.LeftCtrl))
		{
			return InputFHManager.GetKey(Key.RightCtrl);
		}
		return true;
	}

	private bool IsShiftPressed()
	{
		if (!InputFHManager.GetKey(Key.LeftShift))
		{
			return InputFHManager.GetKey(Key.RightShift);
		}
		return true;
	}

	private static void ParseInputEvent(string inputEvent, out string input, out InputModifiers modifiers)
	{
		input = inputEvent;
		modifiers = InputModifiers.None;
		if (input.Contains("+"))
		{
			string[] array = input.Split(new char[1] { '+' }, 2);
			string[] array2 = array[0].Trim().ToLowerInvariant().Split(' ');
			foreach (string text in array2)
			{
				switch (text)
				{
				case "ctrl":
					modifiers |= InputModifiers.Ctrl;
					break;
				case "alt":
					modifiers |= InputModifiers.Alt;
					break;
				case "shift":
					modifiers |= InputModifiers.Shift;
					break;
				default:
					throw new InvalidOperationException("Invalid input modifier: " + text);
				}
			}
			input = array[1].Trim();
		}
		if (InputFHManager.GetInputKey(input) == Key.None)
		{
			throw new InvalidOperationException("Invalid input key: " + input);
		}
	}

	[ConsoleCommand(Command = "bind", Description = "Binds a console command to an input event")]
	public static void BindCommand(IConsole console, string inputEvent, string command)
	{
		string input;
		InputModifiers modifiers;
		try
		{
			ParseInputEvent(inputEvent, out input, out modifiers);
		}
		catch (Exception ex)
		{
			console.WriteLine("Invalid input event: " + ex.Message);
			return;
		}
		try
		{
			Instance._console.ValidateCommandLine(command);
		}
		catch (Exception ex2)
		{
			console.WriteLine("Invalid command: " + ex2.Message);
			return;
		}
		Instance._inputBoundCommands.Add(new InputBoundCommand(input, modifiers, command));
	}

	[ConsoleCommand(Command = "unbind", Description = "Unbinds a console command from an input event")]
	public static void UnbindCommand(IConsole console, string inputEvent)
	{
		ParseInputEvent(inputEvent, out var input, out var modifiers);
		if (Instance._inputBoundCommands.RemoveAll((InputBoundCommand c) => c.Input == input && c.Modifiers == modifiers) == 0)
		{
			console.WriteLine("Input event " + inputEvent + " is not bound");
		}
	}

	[ConsoleCommand(Command = "unbind_all", Description = "Unbinds all commands bound to input events")]
	public static void UnbindAllCommand(IConsole console)
	{
		Instance._inputBoundCommands.Clear();
	}

	[ConsoleCommand(Command = "list_binds", Description = "Lists all commands bound to input events")]
	public static void ListBindsCommand(IConsole console)
	{
		foreach (string item in from b in Instance._inputBoundCommands
			select b.ToString() into b
			orderby b
			select b)
		{
			console.WriteLine(item);
		}
	}

	public void EnableInput(bool enable)
	{
		_inputDisabled = !enable;
		if (!enable && _console.Visible)
		{
			_console.ToggleVisibility();
		}
	}
}
