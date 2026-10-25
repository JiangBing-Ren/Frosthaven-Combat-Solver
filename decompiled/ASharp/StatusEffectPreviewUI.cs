using UnityEngine;
using UnityEngine.UI;

public class StatusEffectPreviewUI : MonoBehaviour
{
	[SerializeField]
	private StatusEffectPopup descriptionPopup;

	[SerializeField]
	private Image abilityIconImage;

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

	public void SetEffect(string title, string description, Sprite image, Transform holder)
	{
		ToggleDisplayBonus(active: false);
		abilityIconImage.sprite = image;
		descriptionPopup.Init(title, description, image, holder);
	}
}
