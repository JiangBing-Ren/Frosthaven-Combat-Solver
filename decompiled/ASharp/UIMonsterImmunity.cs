using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMonsterImmunity : MonoBehaviour
{
	[SerializeField]
	private TMP_Text immunityName;

	[SerializeField]
	private Image immunityIcon;

	public void Initialize(string name)
	{
		immunityName.text = CreateLayout.LocaliseText("$ImmunityTo$ ") + LocalizationManager.GetTranslation(name);
		immunityIcon.sprite = UIInfoTools.Instance.GetImmunityIcon(name);
	}
}
