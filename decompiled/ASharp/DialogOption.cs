using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogOption
{
	public UnityAction onMouseClickAction;

	public UnityAction onMouseEnterAction;

	public UnityAction onMouseExitAction;

	public InputAction InputAction;

	public string text;

	public bool enabled = true;

	public bool visible = true;

	public FHDialogButtonVisuals.VisualParams? VisualParams;

	public CPlayerActor enabledForPlayer;

	public CLevelUIInteractionProfile.EIsolatedControlType? ControlType;

	private DialogOption()
	{
	}

	public DialogOption(string text, InputAction inputAction, UnityAction action = null, bool enabled = true, bool visible = true, CLevelUIInteractionProfile.EIsolatedControlType? ControlType = null, FHDialogButtonVisuals.VisualParams? visualParams = null, CPlayerActor enabledForPlayer = null)
	{
		onMouseClickAction = action;
		this.text = text;
		this.enabled = enabled;
		this.visible = visible;
		onMouseEnterAction = null;
		onMouseExitAction = null;
		InputAction = inputAction;
		this.ControlType = ControlType;
		VisualParams = visualParams;
		this.enabledForPlayer = enabledForPlayer;
	}

	public DialogOption(string text, InputAction inputAction, UnityAction onClick, UnityAction onEnter, UnityAction onExit, bool enabled = true, bool visible = true, CLevelUIInteractionProfile.EIsolatedControlType? ControlType = null, FHDialogButtonVisuals.VisualParams? visualParams = null, CPlayerActor enabledForPlayer = null)
	{
		onMouseClickAction = onClick;
		onMouseEnterAction = onEnter;
		onMouseExitAction = onExit;
		this.text = text;
		this.enabled = enabled;
		this.visible = visible;
		InputAction = inputAction;
		this.ControlType = ControlType;
		VisualParams = visualParams;
		this.enabledForPlayer = enabledForPlayer;
	}
}
