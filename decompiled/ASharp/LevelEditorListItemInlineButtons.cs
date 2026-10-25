using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditorListItemInlineButtons : MonoBehaviour
{
	public TextMeshProUGUI DescriptionLabel;

	public Button DeleteButton;

	public Button ItemButton;

	[HideInInspector]
	public int ItemIndex;

	public UnityAction<LevelEditorListItemInlineButtons> DeleteButtonPressedAction;

	public UnityAction<LevelEditorListItemInlineButtons> ItemButtonPressedAction;

	public void SetupListItem(string itemDesc, int index, UnityAction<LevelEditorListItemInlineButtons> deleteAction = null, UnityAction<LevelEditorListItemInlineButtons> itemPressedAction = null)
	{
		DescriptionLabel.text = itemDesc;
		DeleteButtonPressedAction = deleteAction;
		ItemButtonPressedAction = itemPressedAction;
		ItemIndex = index;
		if (deleteAction == null)
		{
			DeleteButton.gameObject.SetActive(value: false);
		}
		if (itemPressedAction == null)
		{
			ItemButton.enabled = false;
		}
	}

	public void ListItemPressed()
	{
		ItemButtonPressedAction?.Invoke(this);
	}

	public void DeletePressed()
	{
		DeleteButtonPressedAction?.Invoke(this);
	}
}
