using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorItemLoadoutItem : MonoBehaviour
{
	public TextMeshProUGUI DescText;

	public TextMeshProUGUI SlotText;

	public UnityAction<LevelEditorItemLoadoutItem> DeleteButtonPressedAction;

	public CItem ItemOnDisplay { get; private set; }

	public void SetupUI(CItem item)
	{
		ItemOnDisplay = item;
		DescText.text = LevelEditorUI.FormatFHName(ItemOnDisplay.Name);
		SlotText.text = ItemOnDisplay.YMLData.Slot.ToString();
	}

	public void DeletePressed()
	{
		DeleteButtonPressedAction?.Invoke(this);
	}
}
