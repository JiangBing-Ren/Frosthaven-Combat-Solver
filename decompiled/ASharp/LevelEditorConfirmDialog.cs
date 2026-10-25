using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorConfirmDialog : MonoBehaviour
{
	public CanvasGroup MainCanvasGroup;

	public TextMeshProUGUI ConfirmationLabel;

	private UnityAction CachedConfirmAction;

	private UnityAction CachedCancelAction;

	public void ShowDialogWithText(string confirmationText, UnityAction confirmAction, UnityAction cancelAction)
	{
		ConfirmationLabel.text = confirmationText;
		CachedConfirmAction = confirmAction;
		CachedCancelAction = cancelAction;
		MainCanvasGroup.alpha = 1f;
		MainCanvasGroup.interactable = true;
		MainCanvasGroup.blocksRaycasts = true;
	}

	public void ConfirmPressed()
	{
		MainCanvasGroup.alpha = 0f;
		MainCanvasGroup.interactable = false;
		MainCanvasGroup.blocksRaycasts = false;
		CachedConfirmAction?.Invoke();
		CachedConfirmAction = null;
	}

	public void CancelPressed()
	{
		MainCanvasGroup.alpha = 0f;
		MainCanvasGroup.interactable = false;
		MainCanvasGroup.blocksRaycasts = false;
		CachedCancelAction?.Invoke();
		CachedCancelAction = null;
	}
}
