using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.Contexts;
using DevLocker.GFrame.Input.UIInputDisplay;
using DevLocker.GFrame.Input.UIScope;
using FH.GameConsole;
using Libs.GameConsole;
using Libs.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InputFHManager : Singleton<InputFHManager>
{
	public InputBindingDisplayAsset[] BindingDisplayAssets;

	private InputComponentContext _inputContext;

	public FHControls PlayerControl { get; private set; }

	public static FHControls Controls
	{
		get
		{
			if (!Singleton<InputFHManager>.InstanceFast)
			{
				return null;
			}
			return Singleton<InputFHManager>.InstanceFast.PlayerControl;
		}
	}

	public InputComponentContext InputContext { get; private set; }

	public static Vector2 CursorPosition
	{
		get
		{
			if ((!Singleton<InputFHManager>.Instance || !Singleton<InputFHManager>.Instance.UsingGamepad) && Pointer.current != null)
			{
				return Pointer.current.position.ReadValue();
			}
			return new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f);
		}
	}

	public bool UsingGamepad => false;

	public static bool IsTextFieldFocused => PlayerContextUIRootObject.GlobalPlayerContext.IsTextFieldFocused();

	public InputDevice GetLastUsedInputDevice()
	{
		return PlayerContextUIRootObject.GlobalPlayerContext.InputContext.GetLastUsedInputDevice();
	}

	public InputControlScheme GetLastUsedInputControlScheme()
	{
		return PlayerContextUIRootObject.GlobalPlayerContext.InputContext.GetLastUsedInputControlScheme();
	}

	protected override void AwakeInit()
	{
		PlayerControl = new FHControls();
		PlayerInput componentInChildren = GetComponentInChildren<PlayerInput>();
		componentInChildren.actions = PlayerControl.asset;
		InputSystemUIInputModule componentInChildren2 = GetComponentInChildren<InputSystemUIInputModule>();
		componentInChildren2.actionsAsset = PlayerControl.asset;
		componentInChildren.uiInputModule = componentInChildren2;
		InputContext = new InputComponentContext(componentInChildren, new InputActionsMaskedStack(PlayerControl), IInputContext.InputBehaviours.Default, BindingDisplayAssets);
		PlayerControl.SetInputContext(InputContext);
		componentInChildren2.submit = null;
		PlayerControl.UI.Submit.performed += SubmitHotkeyOverride;
		PlayerContextUIRootObject.GlobalPlayerContext.SetupGlobal(componentInChildren2.GetComponent<EventSystem>(), InputContext);
		PlayerControl.Enable(this, PlayerControl.UI);
	}

	protected override void AwakenedDestroy()
	{
		PlayerControl.UI.Submit.performed -= SubmitHotkeyOverride;
	}

	private void LateUpdate()
	{
		if (UsingGamepad && IsTextFieldFocused && PlayerControl.UI.Cancel.WasPerformedThisFrame())
		{
			DeactivateInputField();
		}
	}

	private void DeactivateInputField()
	{
		GameObject selectedGameObject = PlayerContextUIRootObject.GlobalPlayerContext.SelectedGameObject;
		if (selectedGameObject != null)
		{
			if (selectedGameObject.TryGetComponent<InputField>(out var component) && component.isFocused)
			{
				component.DeactivateInputField();
			}
			if (selectedGameObject.TryGetComponent<TMP_InputField>(out var component2) && component2.isFocused)
			{
				component2.DeactivateInputField();
			}
		}
	}

	public static Key GetInputKey(string input)
	{
		if (Keyboard.current == null)
		{
			return Key.None;
		}
		foreach (KeyControl allKey in Keyboard.current.allKeys)
		{
			if (string.Compare(allKey.displayName, input, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return allKey.keyCode;
			}
		}
		return Key.None;
	}

	public static bool GetKey(Key key)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		int num = (int)(key - 1);
		if (num < 0 || num >= Keyboard.current.allKeys.Count)
		{
			Log.Error($"Invalid key: {key}");
		}
		return Keyboard.current[key].isPressed;
	}

	public static bool GetKeyDown(Key key)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		int num = (int)(key - 1);
		if (num < 0 || num >= Keyboard.current.allKeys.Count)
		{
			Log.Error($"Invalid key: {key}");
		}
		return Keyboard.current[key].wasPressedThisFrame;
	}

	public static bool GetKeyUp(Key key)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		int num = (int)(key - 1);
		if (num < 0 || num >= Keyboard.current.allKeys.Count)
		{
			Log.Error($"Invalid key: {key}");
		}
		return Keyboard.current[key].wasReleasedThisFrame;
	}

	public static bool GetMouse(MouseButton button)
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return button switch
		{
			MouseButton.Left => Mouse.current.leftButton.isPressed, 
			MouseButton.Middle => Mouse.current.middleButton.isPressed, 
			MouseButton.Right => Mouse.current.rightButton.isPressed, 
			MouseButton.Back => Mouse.current.backButton.isPressed, 
			MouseButton.Forward => Mouse.current.forwardButton.isPressed, 
			_ => throw new ArgumentOutOfRangeException("button", button, null), 
		};
	}

	public static bool GetMouseDown(MouseButton button)
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return button switch
		{
			MouseButton.Left => Mouse.current.leftButton.wasPressedThisFrame, 
			MouseButton.Middle => Mouse.current.middleButton.wasPressedThisFrame, 
			MouseButton.Right => Mouse.current.rightButton.wasPressedThisFrame, 
			MouseButton.Back => Mouse.current.backButton.wasPressedThisFrame, 
			MouseButton.Forward => Mouse.current.forwardButton.wasPressedThisFrame, 
			_ => throw new ArgumentOutOfRangeException("button", button, null), 
		};
	}

	public static bool GetMouseUp(MouseButton button)
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return button switch
		{
			MouseButton.Left => Mouse.current.leftButton.wasReleasedThisFrame, 
			MouseButton.Middle => Mouse.current.middleButton.wasReleasedThisFrame, 
			MouseButton.Right => Mouse.current.rightButton.wasReleasedThisFrame, 
			MouseButton.Back => Mouse.current.backButton.wasReleasedThisFrame, 
			MouseButton.Forward => Mouse.current.forwardButton.wasReleasedThisFrame, 
			_ => throw new ArgumentOutOfRangeException("button", button, null), 
		};
	}

	public static bool GetInputKeyDown(string input)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		foreach (KeyControl allKey in Keyboard.current.allKeys)
		{
			if (string.Compare(allKey.displayName, input, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return allKey.wasPressedThisFrame;
			}
		}
		return false;
	}

	public static bool AnyKey()
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current.anyKey.wasPressedThisFrame;
	}

	private void SubmitHotkeyOverride(InputAction.CallbackContext context)
	{
		EventSystem eventSystem = PlayerContextUIRootObject.GlobalPlayerContext.EventSystem;
		if (!(eventSystem.currentSelectedGameObject == null))
		{
			BaseEventData eventData = new BaseEventData(eventSystem);
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, eventData, ExecuteEvents.submitHandler);
		}
	}

	[ConsoleCommand(Command = "input_info", Description = "[includeDisabled=false] [displayHotkeys=DisplayInputActions] - Prints the current scopes info.")]
	public static void PrintInputInfo(IConsole console, bool includeDisabled = false, UIScopeDebugUtils.DisplayHotkeyType displayHotkeys = UIScopeDebugUtils.DisplayHotkeyType.DisplayInputActions)
	{
		PlayerContextUIRootObject globalPlayerContext = PlayerContextUIRootObject.GlobalPlayerContext;
		InputControlScheme lastUsedInputControlScheme = globalPlayerContext.InputContext.GetLastUsedInputControlScheme();
		UIScope uIScope = UIScope.FocusedScope(globalPlayerContext);
		bool flag = uIScope == GameConsoleInput.Instance.OwningScope;
		if (flag)
		{
			uIScope.FocusPreviousScope();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Selected Object: ");
		stringBuilder.AppendLine(globalPlayerContext.SelectedGameObject ? UnityUtil.GetFindPath(null, globalPlayerContext.SelectedGameObject.transform) : "null");
		stringBuilder.Append("SelectionController: ");
		SelectionController activeInstanceFor = SelectionController.GetActiveInstanceFor(globalPlayerContext);
		stringBuilder.AppendLine(activeInstanceFor ? UnityUtil.GetFindPath(null, activeInstanceFor.transform) : "null");
		stringBuilder.AppendLine("Scopes Info:");
		if (displayHotkeys != UIScopeDebugUtils.DisplayHotkeyType.DisplayInputUsers)
		{
			stringBuilder.AppendLine(UIScopeDebugUtils.GetScopesInfo(displayHotkeys, includeDisabled, lastUsedInputControlScheme));
		}
		else
		{
			stringBuilder.AppendLine(UIScopeDebugUtils.GetScopesInfo(UIScopeDebugUtils.DisplayHotkeyType.DisplayInputActions, includeDisabled, lastUsedInputControlScheme));
			stringBuilder.AppendLine("Action Users:");
			foreach (InputAction allAction in globalPlayerContext.InputContext.GetAllActions())
			{
				if (globalPlayerContext.InputContext.InputActionsMaskedStack.GetEnablingSourcesFor(allAction).Any())
				{
					string value = UIScopeDebugUtils.GetHotkeyNames(globalPlayerContext.InputContext, new InputAction[1] { allAction }, default, UIScopeDebugUtils.DisplayHotkeyType.DisplayInputUsers).FirstOrDefault();
					if (!string.IsNullOrEmpty(value))
					{
						stringBuilder.Append("- ").AppendLine(value);
					}
				}
			}
		}
		console.WriteLine(stringBuilder);
		Debug.Log(stringBuilder);
		if (flag)
		{
			uIScope.Focus();
		}
	}

	[ConsoleCommand(Command = "input_actions", Description = "[includeDisabled=false, includeBindings=false] - Print out all input actions and their state.")]
	public static void PrintInputActions(IConsole console, bool includeDisabled = false, bool includeBindings = false)
	{
		PlayerContextUIRootObject globalPlayerContext = PlayerContextUIRootObject.GlobalPlayerContext;
		UIScope uIScope = UIScope.FocusedScope(globalPlayerContext);
		bool flag = uIScope == GameConsoleInput.Instance.OwningScope;
		if (flag)
		{
			uIScope.FocusPreviousScope();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Active Scopes: " + string.Join(" ", from s in UIScope.GetActiveScopes(globalPlayerContext)
			select "\"" + s.name + "\""));
		stringBuilder.AppendLine("Input Actions State:");
		foreach (InputAction allAction in globalPlayerContext.InputContext.GetAllActions())
		{
			if (includeDisabled || allAction.enabled)
			{
				string arg = (includeBindings ? allAction.ToString() : (allAction.actionMap.name + "/" + allAction.name));
				stringBuilder.AppendLine($"  {arg} [{allAction.phase}, {allAction.type}]");
			}
		}
		console.WriteLine(stringBuilder);
		Debug.Log(stringBuilder);
		if (flag)
		{
			uIScope.Focus();
		}
	}

	[ConsoleCommand(Command = "input_focus_scope", Description = "(scopeName) - focus enabled scope by name.")]
	public static void FocusScope(IConsole console, string scopeName)
	{
		UIScope uIScope = UIScope.GetRegisteredScopes(PlayerContextUIRootObject.GlobalPlayerContext).First((UIScope s) => s.name == scopeName);
		if (UIScope.FocusedScope(PlayerContextUIRootObject.GlobalPlayerContext) == GameConsoleInput.Instance.OwningScope)
		{
			GameConsoleInput.Instance.OwningScope.LastFocusedScope = uIScope;
			GameConsoleWindow.Instance.ToggleVisibility();
		}
		else
		{
			uIScope.Focus();
		}
	}

	[ConsoleCommand(Command = "input_check_conflicts", Description = "Check for input actions conflicts and illegal actions.")]
	public static void CheckForConflicts(IConsole console)
	{
		IInputContext inputContext = PlayerContextUIRootObject.GlobalPlayerContext.InputContext;
		if (inputContext == null)
		{
			return;
		}
		InputActionsMaskedStack.InputActionConflictsReport conflictingActionRequests = inputContext.InputActionsMaskedStack.GetConflictingActionRequests(inputContext.GetUIActions());
		if (conflictingActionRequests.HasIssuesFound)
		{
			IEnumerable<string> first = conflictingActionRequests.Conflicts.Select((KeyValuePair<InputAction, IReadOnlyCollection<object>> pair) => "- " + pair.Key.name + " [" + string.Join(", ", pair.Value) + "]");
			IEnumerable<string> second = conflictingActionRequests.IllegalActions.Select((InputAction action) => "- " + action.name + " [ILLEGAL]");
			console.WriteWithStyle("[Input] Input actions in conflict found:\n" + string.Join('\n', first.Concat(second)), CommandOutputStyle.Error);
		}
	}
}
