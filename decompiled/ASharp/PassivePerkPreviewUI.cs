using System;
using UnityEngine;
using UnityEngine.UI;

public class PassivePerkPreviewUI : MonoBehaviour
{
	[SerializeField]
	private PassivePerkPopup descriptionPopup;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image durationImage;

	private Action onCancel;

	private Button button;

	public Selectable Selectable => button;

	private void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(() =>
		{
			onCancel?.Invoke();
		});
	}

	public void SetPassive(Sprite image, string nameKey, string descriptionKey, Transform holder, Action onCancel = null, bool localizeKeys = true, bool showDuration = true)
	{
		this.onCancel = onCancel;
		ToggleDisplayBonus(active: false);
		descriptionPopup.Init(image, nameKey, descriptionKey, holder, onCancel != null, localizeKeys, showDuration);
		iconImage.sprite = image;
		durationImage.enabled = showDuration;
	}

	public void ToggleDisplayBonus(bool active)
	{
		if (active)
		{
			descriptionPopup.Show();
		}
		else
		{
			descriptionPopup.Hide();
		}
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable && onCancel != null;
	}
}
