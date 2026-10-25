using System;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;

public class LevelEditorLimitedInteractionItem : MonoBehaviour
{
	public TextMeshProUGUI ItemText;

	public int ControlIndex;

	public Action<LevelEditorLimitedInteractionItem> ButtonClickedAction { get; private set; }

	public CLevelUIInteractionProfile.CLevelUIInteractionSpecific Control { get; private set; }

	public void Init(CLevelUIInteractionProfile.CLevelUIInteractionSpecific control, Action<LevelEditorLimitedInteractionItem> buttonPressedAction, int index)
	{
		ItemText.text = control.ControlType.ToString();
		Control = control;
		ControlIndex = index;
		ButtonClickedAction = buttonPressedAction;
	}

	public void OnButtonPressed()
	{
		ButtonClickedAction?.Invoke(this);
	}
}
