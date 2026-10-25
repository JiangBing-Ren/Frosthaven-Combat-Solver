using System;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class LevelEditorDoorListItem : MonoBehaviour
{
	public TextMeshProUGUI ItemText;

	public CObjectDoor DoorItem { get; private set; }

	public Action<LevelEditorDoorListItem> ButtonClickedAction { get; private set; }

	public void Init(CObjectDoor door, Action<LevelEditorDoorListItem> buttonClicked)
	{
		ItemText.text = door.InstanceName + " - " + door.DoorType;
		DoorItem = door;
		ButtonClickedAction = buttonClicked;
	}

	public void OnButtonClicked()
	{
		ButtonClickedAction?.Invoke(this);
	}
}
