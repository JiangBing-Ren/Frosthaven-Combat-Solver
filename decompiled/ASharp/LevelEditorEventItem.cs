using System;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorEventItem : MonoBehaviour
{
	public Image Background;

	public TextMeshProUGUI EventText;

	[HideInInspector]
	public int ItemIndex;

	[HideInInspector]
	public CLevelEvent ItemEvent;

	private Color m_BackgroundOriginalColor;

	public Action<LevelEditorEventItem> ButtonClickedAction { get; private set; }

	public void Init(CLevelEvent itemEvent, int index, Action<LevelEditorEventItem> buttonClicked)
	{
		ItemIndex = index;
		ItemEvent = itemEvent;
		ButtonClickedAction = buttonClicked;
		Refresh();
	}

	public void Refresh()
	{
		EventText.text = $"{ItemEvent.EventType} {ItemIndex}";
	}

	public void SetSelectedState(bool selected)
	{
		Background.color = (selected ? new Color(0.85f, 0.745f, 0.541f) : m_BackgroundOriginalColor);
	}

	public void OnButtonClicked()
	{
		ButtonClickedAction?.Invoke(this);
	}

	private void Awake()
	{
		m_BackgroundOriginalColor = Background.color;
	}
}
