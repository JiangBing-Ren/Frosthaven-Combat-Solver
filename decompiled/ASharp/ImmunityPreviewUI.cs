using UnityEngine;
using UnityEngine.UI;

public class ImmunityPreviewUI : MonoBehaviour
{
	[SerializeField]
	private ImmunityPopup descriptionPopup;

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

	public void SetImmunity(string immunity, Transform holder)
	{
		ToggleDisplayBonus(active: false);
		abilityIconImage.sprite = UIInfoTools.Instance.GetImmunityIcon(immunity);
		descriptionPopup.Init(immunity, holder);
	}
}
