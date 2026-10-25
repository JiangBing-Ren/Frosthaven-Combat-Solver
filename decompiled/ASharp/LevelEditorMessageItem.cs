using System;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;

public class LevelEditorMessageItem : MonoBehaviour
{
	public TextMeshProUGUI ItemText;

	public string MessageName { get; private set; }

	public int MessageIndex { get; private set; }

	public Action<LevelEditorMessageItem> ButtonClickedAction { get; private set; }

	public Action<LevelEditorMessageItem, bool> ReorderClickedAction { get; private set; }

	public CLevelMessage Message => SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages[MessageIndex];

	public void Init(Action<LevelEditorMessageItem> buttonClicked, Action<LevelEditorMessageItem, bool> reorderClicked, int index)
	{
		MessageIndex = index;
		ItemText.text = Message.MessageName;
		ButtonClickedAction = buttonClicked;
		ReorderClickedAction = reorderClicked;
	}

	public void UpdateMessageName()
	{
		ItemText.text = Message.MessageName;
	}

	public void OnButtonClicked()
	{
		ButtonClickedAction?.Invoke(this);
	}

	public void OnMoveUpPressed()
	{
		ReorderClickedAction?.Invoke(this, arg2: true);
	}

	public void OnMoveDownPressed()
	{
		ReorderClickedAction?.Invoke(this, arg2: false);
	}
}
