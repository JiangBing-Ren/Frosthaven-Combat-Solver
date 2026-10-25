using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUseOption : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI text;

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void SetOption(Sprite option)
	{
		text.gameObject.SetActive(value: false);
		if (option == null)
		{
			icon.gameObject.SetActive(value: false);
			return;
		}
		icon.sprite = option;
		icon.gameObject.SetActive(value: true);
	}

	public void SetOption(string option)
	{
		icon.gameObject.SetActive(value: false);
		if (option == null)
		{
			text.gameObject.SetActive(value: false);
			return;
		}
		text.text = option;
		text.gameObject.SetActive(value: true);
	}

	public void Clear()
	{
		text.gameObject.SetActive(value: false);
		icon.gameObject.SetActive(value: false);
	}
}
