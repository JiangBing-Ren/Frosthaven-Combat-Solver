using System;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class LevelEditorRoomListItem : MonoBehaviour
{
	public TextMeshProUGUI ItemText;

	public CMap MapItem { get; private set; }

	public Action<LevelEditorRoomListItem> ButtonClickedAction { get; private set; }

	public void Init(CMap map, Action<LevelEditorRoomListItem> buttonClicked)
	{
		ItemText.text = map.RoomName + " - " + map.MapType;
		MapItem = map;
		ButtonClickedAction = buttonClicked;
	}

	public void OnButtonClicked()
	{
		ButtonClickedAction?.Invoke(this);
	}
}
