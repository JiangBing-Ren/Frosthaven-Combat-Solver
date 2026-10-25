using System;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;

public class LevelEditorMessagePageItem : MonoBehaviour
{
	public TextMeshProUGUI ItemText;

	public string MessageName { get; private set; }

	public Action<LevelEditorMessagePageItem> ButtonClickedAction { get; private set; }

	public CLevelMessage Message { get; private set; }

	public int PageIndex { get; private set; }

	public CLevelMessagePage Page { get; private set; }

	public void Init(string messageName, int index, Action<LevelEditorMessagePageItem> buttonClicked)
	{
		MessageName = messageName;
		PageIndex = index;
		Message = SaveData.Instance.Global.CurrentEditorLevelData.LevelMessages.Single((CLevelMessage s) => s.MessageName == MessageName);
		if (Message.Pages != null && Message.Pages.Count > PageIndex)
		{
			Page = Message.Pages[PageIndex];
		}
		else
		{
			Log.Error("Unable to display Tutorial Message Page with index [" + PageIndex + "] for message [" + Message.MessageName + "]");
		}
		ItemText.text = messageName + " Page " + PageIndex;
		ButtonClickedAction = buttonClicked;
	}

	public void OnButtonClicked()
	{
		ButtonClickedAction?.Invoke(this);
	}
}
