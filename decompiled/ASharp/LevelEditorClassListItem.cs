using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorClassListItem : MonoBehaviour
{
	[HideInInspector]
	public int classIndex;

	public TextMeshProUGUI ClassText;

	public UnityAction<LevelEditorClassListItem> OnRemoveButtonPressedAction;

	public string ClassName => ClassText.text;

	public void Init(string itemClassName)
	{
		ClassText.text = itemClassName;
	}

	public void OnRemoveButtonPress()
	{
		OnRemoveButtonPressedAction?.Invoke(this);
	}
}
